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
using Beep.OilandGas.ProductionAccounting.Exceptions;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Implements take-or-pay contract adjustments using SALES_CONTRACT and CONTRACT_PERFORMANCE_OBLIGATION.
    /// </summary>
    public class TakeOrPayService : ITakeOrPayService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<TakeOrPayService> _logger;
        private const string ConnectionName = "PPDM39";

        public TakeOrPayService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<TakeOrPayService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<REVENUE_TRANSACTION?> ApplyTakeOrPayAsync(
            RUN_TICKET RUN_TICKET,
            ALLOCATION_RESULT ALLOCATION_RESULT,
            decimal deliveredVolume,
            string userId,
            string cn = "PPDM39")
        {
            if (RUN_TICKET == null)
                throw new ArgumentNullException(nameof(RUN_TICKET));
            if (ALLOCATION_RESULT == null)
                throw new ArgumentNullException(nameof(ALLOCATION_RESULT));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var contract = await FindApplicableContractAsync(RUN_TICKET, cn);
            if (contract == null)
                return null;

            var obligation = await GetTakeOrPayObligationAsync(contract.SALES_CONTRACT_ID, cn);
            if (obligation == null)
                return null;

            var contractDate = RUN_TICKET.TICKET_DATE_TIME ?? DateTime.UtcNow;
            var schedule = await GetScheduleAsync(contract.SALES_CONTRACT_ID, contractDate, cn);
            var minVolume = schedule?.MIN_VOLUME
                ?? ParseDecimal(obligation.OBLIGATION_DESCRIPTION, "MIN_VOLUME")
                ?? ParseDecimal(obligation.REMARK, "MIN_VOLUME")
                ?? ParseDecimal(obligation.OBLIGATION_DESCRIPTION, "MIN_QTY")
                ?? ParseDecimal(obligation.REMARK, "MIN_QTY")
                ?? 0m;

            if (minVolume <= 0m)
                return null;

            var price = schedule?.PRICE ?? contract.BASE_PRICE ?? 0m;
            var priceOverride = ParseDecimal(obligation.OBLIGATION_DESCRIPTION, "PRICE")
                ?? ParseDecimal(obligation.REMARK, "PRICE")
                ?? 0m;
            if (priceOverride > 0m)
                price = priceOverride;

            if (price <= 0m)
                return null;

            var deficiency = minVolume - deliveredVolume;
            if (deficiency <= 0m)
            {
                await UpdateObligationStatusAsync(obligation, "SATISFIED", userId, cn);
                await ApplyMakeupBalanceAsync(contract.SALES_CONTRACT_ID, deliveredVolume - minVolume, contractDate, userId, cn);
                return null;
            }

            var adjustmentRevenue = deficiency * price;
            var isGas = string.Equals(contract.COMMODITY_TYPE, "GAS", StringComparison.OrdinalIgnoreCase);

            var revenueTransaction = new REVENUE_TRANSACTION
            {
                REVENUE_TRANSACTION_ID = Guid.NewGuid().ToString(),
                ALLOCATION_RESULT_ID = ALLOCATION_RESULT.ALLOCATION_RESULT_ID,
                CONTRACT_ID = contract.SALES_CONTRACT_ID,
                PROPERTY_ID = RUN_TICKET.LEASE_ID,
                WELL_ID = RUN_TICKET.WELL_ID,
                AFE_ID = RUN_TICKET.AFE_ID,
                TRANSACTION_DATE = contractDate,
                REVENUE_TYPE = "TAKE_OR_PAY",
                GROSS_REVENUE = adjustmentRevenue,
                NET_REVENUE = adjustmentRevenue,
                OIL_VOLUME = isGas ? 0m : deficiency,
                GAS_VOLUME = isGas ? deficiency : 0m,
                OIL_PRICE = isGas ? null : price,
                GAS_PRICE = isGas ? price : null,
                CURRENCY_CODE = contract.CURRENCY_CODE ?? "USD",
                DESCRIPTION = $"Take-or-pay deficiency for contract {contract.CONTRACT_NUMBER}",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await CreateRepoAsync<REVENUE_TRANSACTION>("REVENUE_TRANSACTION", cn);
            await repo.InsertAsync(revenueTransaction, userId);

            await UpdateObligationStatusAsync(obligation, "PARTIALLY_SATISFIED", userId, cn);
            await ApplyMakeupBalanceAsync(contract.SALES_CONTRACT_ID, -deficiency, contractDate, userId, cn);

            _logger?.LogWarning(
                "Take-or-pay adjustment created for contract {ContractId}: deficiency {Deficiency} at price {Price}",
                contract.SALES_CONTRACT_ID, deficiency, price);

            return revenueTransaction;
        }

        private async Task<SALES_CONTRACT?> FindApplicableContractAsync(RUN_TICKET RUN_TICKET, string cn)
        {
            var repo = await CreateRepoAsync<SALES_CONTRACT>("SALES_CONTRACT", cn);
            var ticketDate = RUN_TICKET.TICKET_DATE_TIME ?? DateTime.UtcNow;
            var buyerId = RUN_TICKET.PURCHASER;
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrWhiteSpace(buyerId))
                filters.Add(new AppFilter { FieldName = "BUYER_BA_ID", Operator = "=", FilterValue = buyerId });

            var results = await repo.GetAsync(filters);
            var contracts = results?.Cast<SALES_CONTRACT>().ToList() ?? new List<SALES_CONTRACT>();

            return contracts
                .Where(c =>
                    (!c.EFFECTIVE_DATE.HasValue || c.EFFECTIVE_DATE.Value.Date <= ticketDate.Date) &&
                    (!c.EXPIRY_DATE.HasValue || c.EXPIRY_DATE.Value.Date >= ticketDate.Date))
                .OrderByDescending(c => c.EFFECTIVE_DATE ?? DateTime.MinValue)
                .FirstOrDefault();
        }

        private async Task<CONTRACT_PERFORMANCE_OBLIGATION?> GetTakeOrPayObligationAsync(string salesContractId, string cn)
        {
            var repo = await CreateRepoAsync<CONTRACT_PERFORMANCE_OBLIGATION>("CONTRACT_PERFORMANCE_OBLIGATION", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "SALES_CONTRACT_ID", Operator = "=", FilterValue = salesContractId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var obligations = results?.Cast<CONTRACT_PERFORMANCE_OBLIGATION>().ToList()
                ?? new List<CONTRACT_PERFORMANCE_OBLIGATION>();

            return obligations.FirstOrDefault(o =>
                string.Equals(o.OBLIGATION_TYPE, "TAKE_OR_PAY", StringComparison.OrdinalIgnoreCase) ||
                (o.OBLIGATION_DESCRIPTION?.IndexOf("TAKE_OR_PAY", StringComparison.OrdinalIgnoreCase) >= 0) ||
                (o.REMARK?.IndexOf("TAKE_OR_PAY", StringComparison.OrdinalIgnoreCase) >= 0));
        }

        private async Task<TAKE_OR_PAY_SCHEDULE?> GetScheduleAsync(string salesContractId, DateTime contractDate, string cn)
        {
            try
            {
                var repo = await CreateRepoAsync<TAKE_OR_PAY_SCHEDULE>("TAKE_OR_PAY_SCHEDULE", cn);
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "SALES_CONTRACT_ID", Operator = "=", FilterValue = salesContractId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var schedules = results?.Cast<TAKE_OR_PAY_SCHEDULE>().ToList() ?? new List<TAKE_OR_PAY_SCHEDULE>();
                return schedules.FirstOrDefault(s =>
                    (!s.PERIOD_START.HasValue || s.PERIOD_START.Value.Date <= contractDate.Date) &&
                    (!s.PERIOD_END.HasValue || s.PERIOD_END.Value.Date >= contractDate.Date));
            }
            catch
            {
                return null;
            }
        }

        private async Task ApplyMakeupBalanceAsync(
            string salesContractId,
            decimal volumeDelta,
            DateTime updateDate,
            string userId,
            string cn)
        {
            if (string.IsNullOrWhiteSpace(salesContractId))
                return;

            try
            {
                var repo = await CreateRepoAsync<TAKE_OR_PAY_BALANCE>("TAKE_OR_PAY_BALANCE", cn);
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "SALES_CONTRACT_ID", Operator = "=", FilterValue = salesContractId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var balance = results?.Cast<TAKE_OR_PAY_BALANCE>().FirstOrDefault();
                if (balance == null)
                {
                    balance = new TAKE_OR_PAY_BALANCE
                    {
                        TAKE_OR_PAY_BALANCE_ID = Guid.NewGuid().ToString(),
                        SALES_CONTRACT_ID = salesContractId,
                        BALANCE_VOLUME = volumeDelta,
                        LAST_UPDATED_DATE = updateDate,
                        ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                        PPDM_GUID = Guid.NewGuid().ToString(),
                        ROW_CREATED_BY = userId,
                        ROW_CREATED_DATE = DateTime.UtcNow
                    };

                    await repo.InsertAsync(balance, userId);
                    return;
                }

                var newBalance = (balance.BALANCE_VOLUME ?? 0m) + volumeDelta;
                if (newBalance < 0m)
                    newBalance = 0m;

                balance.BALANCE_VOLUME = newBalance;
                balance.LAST_UPDATED_DATE = updateDate;
                balance.ROW_CHANGED_BY = userId;
                balance.ROW_CHANGED_DATE = DateTime.UtcNow;

                await repo.UpdateAsync(balance, userId);
            }
            catch
            {
                // Optional table not available; skip
            }
        }

        private async Task UpdateObligationStatusAsync(CONTRACT_PERFORMANCE_OBLIGATION obligation, string status, string userId, string cn)
        {
            if (obligation == null || string.IsNullOrWhiteSpace(obligation.CONTRACT_PERFORMANCE_OBLIGATION_ID))
                return;

            obligation.STATUS = status;
            obligation.ROW_CHANGED_BY = userId;
            obligation.ROW_CHANGED_DATE = DateTime.UtcNow;

            var repo = await CreateRepoAsync<CONTRACT_PERFORMANCE_OBLIGATION>("CONTRACT_PERFORMANCE_OBLIGATION", cn);
            await repo.UpdateAsync(obligation, userId);
        }

        private static decimal? ParseDecimal(string input, string key)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(key))
                return null;

            var tokens = input.Split(new[] { ';', ',', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var parts = token.Split('=');
                if (parts.Length != 2)
                    continue;
                if (!parts[0].Equals(key, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (decimal.TryParse(parts[1], out var value))
                    return value;
            }

            return null;
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
