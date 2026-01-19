using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Calculates and records production taxes (severance, ad valorem) for revenue transactions.
    /// </summary>
    public class ProductionTaxService : IProductionTaxService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<ProductionTaxService> _logger;
        private const string ConnectionName = "PPDM39";

        public ProductionTaxService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<ProductionTaxService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<TAX_TRANSACTION?> CalculateProductionTaxesAsync(
            REVENUE_TRANSACTION revenueTransaction,
            string userId,
            string cn = "PPDM39")
        {
            if (revenueTransaction == null)
                throw new ArgumentNullException(nameof(revenueTransaction));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var grossRevenue = revenueTransaction.GROSS_REVENUE ?? 0m;
            if (grossRevenue <= 0m)
                return null;

            var taxData = await GetTaxDataAsync(revenueTransaction.PROPERTY_ID, cn);
            if (taxData == null)
                return null;

            var severanceRate = NormalizeRate(taxData.SEVERANCE_TAX);
            var adValoremRate = NormalizeRate(taxData.AD_VALOREM_TAX);

            var severanceAmount = grossRevenue * severanceRate;
            var adValoremAmount = grossRevenue * adValoremRate;
            var totalTax = severanceAmount + adValoremAmount;

            if (totalTax <= 0m)
                return null;

            await InsertTaxTransactionAsync(revenueTransaction, "SEVERANCE", severanceAmount, userId, cn);
            await InsertTaxTransactionAsync(revenueTransaction, "AD_VALOREM", adValoremAmount, userId, cn);

            revenueTransaction.TAX_AMOUNT = totalTax;
            revenueTransaction.ROW_CHANGED_BY = userId;
            revenueTransaction.ROW_CHANGED_DATE = DateTime.UtcNow;

            var revenueRepo = await CreateRepoAsync<REVENUE_TRANSACTION>("REVENUE_TRANSACTION", cn);
            await revenueRepo.UpdateAsync(revenueTransaction, userId);

            _logger?.LogInformation(
                "Production taxes applied to revenue {RevenueId}: total {TaxAmount}",
                revenueTransaction.REVENUE_TRANSACTION_ID, totalTax);

            await RecordIdcAdjustmentAsync(revenueTransaction.PROPERTY_ID, revenueTransaction.TRANSACTION_DATE, userId, cn);
            await RecordDepletionAdjustmentsAsync(revenueTransaction.PROPERTY_ID, revenueTransaction.TRANSACTION_DATE, userId, cn);

            return new TAX_TRANSACTION
            {
                TAX_TRANSACTION_ID = Guid.NewGuid().ToString(),
                TAX_TYPE = "TOTAL_PRODUCTION_TAX",
                TAX_DATE = revenueTransaction.TRANSACTION_DATE ?? DateTime.UtcNow,
                TAX_AMOUNT = totalTax,
                TAX_JURISDICTION = ExtractJurisdiction(taxData.REMARK),
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };
        }

        private static decimal NormalizeRate(decimal? rate)
        {
            if (!rate.HasValue || rate.Value <= 0m)
                return 0m;
            if (rate.Value > 1m)
                return rate.Value / 100m;
            return rate.Value;
        }

        private async Task<GOVERNMENTAL_TAX_DATA?> GetTaxDataAsync(string propertyId, string cn)
        {
            var repo = await CreateRepoAsync<GOVERNMENTAL_TAX_DATA>("GOVERNMENTAL_TAX_DATA", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var all = results?.Cast<GOVERNMENTAL_TAX_DATA>().ToList()
                ?? new List<GOVERNMENTAL_TAX_DATA>();

            if (all.Count == 0)
                return null;

            if (!string.IsNullOrWhiteSpace(propertyId))
            {
                var match = all.FirstOrDefault(t => RemarkHasValue(t.REMARK, "PROPERTY_ID", propertyId));
                if (match != null)
                    return match;
            }

            return all.FirstOrDefault();
        }

        private async Task InsertTaxTransactionAsync(
            REVENUE_TRANSACTION revenueTransaction,
            string taxType,
            decimal amount,
            string userId,
            string cn)
        {
            if (amount <= 0m)
                return;

            var tax = new TAX_TRANSACTION
            {
                TAX_TRANSACTION_ID = Guid.NewGuid().ToString(),
                TAX_TYPE = taxType,
                TAX_DATE = revenueTransaction.TRANSACTION_DATE ?? DateTime.UtcNow,
                TAX_AMOUNT = amount,
                TAX_JURISDICTION = revenueTransaction.PROPERTY_ID,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await CreateRepoAsync<TAX_TRANSACTION>("TAX_TRANSACTION", cn);
            await repo.InsertAsync(tax, userId);
        }

        private async Task RecordIdcAdjustmentAsync(
            string propertyId,
            DateTime? periodDate,
            string userId,
            string cn)
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                return;

            var periodEnd = periodDate?.Date ?? DateTime.UtcNow.Date;
            var repo = await CreateRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var costs = results?.Cast<ACCOUNTING_COST>().ToList() ?? new List<ACCOUNTING_COST>();
            var idcCosts = costs.Where(c =>
                string.Equals(c.COST_CATEGORY, CostCategories.Drilling, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.COST_CATEGORY, CostCategories.Completion, StringComparison.OrdinalIgnoreCase)).ToList();

            if (idcCosts.Count == 0)
                return;

            var idcTotal = idcCosts.Sum(c => c.AMOUNT);
            if (idcTotal <= 0m)
                return;

            var adjustment = new TAX_ADJUSTMENT
            {
                TAX_ADJUSTMENT_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = propertyId,
                ADJUSTMENT_TYPE = "IDC_DEDUCTION",
                PERIOD_END_DATE = periodEnd,
                AMOUNT = idcTotal,
                NOTES = "IDC deduction based on drilling/completion costs",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var adjustRepo = await CreateRepoAsync<TAX_ADJUSTMENT>("TAX_ADJUSTMENT", cn);
            await adjustRepo.InsertAsync(adjustment, userId);
        }

        private async Task RecordDepletionAdjustmentsAsync(
            string propertyId,
            DateTime? periodDate,
            string userId,
            string cn)
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                return;

            var periodEnd = periodDate?.Date ?? DateTime.UtcNow.Date;
            var amortRepo = await CreateRepoAsync<AMORTIZATION_RECORD>("AMORTIZATION_RECORD", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "PERIOD_END_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var records = await amortRepo.GetAsync(filters);
            var amortizations = records?.Cast<AMORTIZATION_RECORD>().ToList() ?? new List<AMORTIZATION_RECORD>();
            var bookDepletion = amortizations.Sum(r => r.AMORTIZATION_AMOUNT ?? 0m);

            if (bookDepletion <= 0m)
                return;

            var taxRate = await GetTaxDepletionRateAsync(propertyId, cn);
            var taxDepletion = bookDepletion * taxRate;

            var adjustment = new TAX_ADJUSTMENT
            {
                TAX_ADJUSTMENT_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = propertyId,
                ADJUSTMENT_TYPE = "TAX_DEPLETION",
                PERIOD_END_DATE = periodEnd,
                AMOUNT = taxDepletion,
                NOTES = $"Tax depletion at rate {taxRate:0.####}",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var adjustRepo = await CreateRepoAsync<TAX_ADJUSTMENT>("TAX_ADJUSTMENT", cn);
            await adjustRepo.InsertAsync(adjustment, userId);

            var deferred = (taxDepletion - bookDepletion) * await GetDeferredTaxRateAsync(propertyId, cn);
            if (deferred != 0m)
            {
                var balance = new DEFERRED_TAX_BALANCE
                {
                    DEFERRED_TAX_BALANCE_ID = Guid.NewGuid().ToString(),
                    PROPERTY_ID = propertyId,
                    PERIOD_END_DATE = periodEnd,
                    DEFERRED_TAX_ASSET = deferred < 0m ? Math.Abs(deferred) : 0m,
                    DEFERRED_TAX_LIABILITY = deferred > 0m ? deferred : 0m,
                    NOTES = "Deferred tax from depletion timing",
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                var balanceRepo = await CreateRepoAsync<DEFERRED_TAX_BALANCE>("DEFERRED_TAX_BALANCE", cn);
                await balanceRepo.InsertAsync(balance, userId);
            }
        }

        private async Task<decimal> GetTaxDepletionRateAsync(string propertyId, string cn)
        {
            var data = await GetTaxDataAsync(propertyId, cn);
            if (data == null)
                return 1m;

            var rate = ExtractRate(data.REMARK, "TAX_DEPLETION_RATE");
            return rate > 0m ? rate : 1m;
        }

        private async Task<decimal> GetDeferredTaxRateAsync(string propertyId, string cn)
        {
            var data = await GetTaxDataAsync(propertyId, cn);
            if (data == null)
                return 0m;

            var rate = ExtractRate(data.REMARK, "DEFERRED_TAX_RATE");
            if (rate > 1m)
                rate /= 100m;
            return rate;
        }

        private static decimal ExtractRate(string remark, string key)
        {
            if (string.IsNullOrWhiteSpace(remark) || string.IsNullOrWhiteSpace(key))
                return 0m;

            var tokens = remark.Split(new[] { ';', ',', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var parts = token.Split('=');
                if (parts.Length != 2)
                    continue;
                if (!parts[0].Equals(key, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (decimal.TryParse(parts[1], out var value))
                    return value > 1m ? value / 100m : value;
            }

            return 0m;
        }

        private static string ExtractJurisdiction(string remark)
        {
            if (string.IsNullOrWhiteSpace(remark))
                return null;

            var tokens = remark.Split(new[] { ';', ',', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var parts = token.Split('=');
                if (parts.Length != 2)
                    continue;
                if (parts[0].Equals("JURISDICTION", StringComparison.OrdinalIgnoreCase))
                    return parts[1];
            }

            return null;
        }

        private static bool RemarkHasValue(string remark, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(remark) || string.IsNullOrWhiteSpace(key))
                return false;

            var tokens = remark.Split(new[] { ';', ',', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var parts = token.Split('=');
                if (parts.Length != 2)
                    continue;
                if (!parts[0].Equals(key, StringComparison.OrdinalIgnoreCase))
                    continue;
                return parts[1].Equals(value, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private async Task<PPDMGenericRepository> CreateRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(T);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, tableName);
        }
    }
}
