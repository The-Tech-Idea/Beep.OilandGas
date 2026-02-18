using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// IAS 37 provision support with ARO tracking, measurement updates, and accretion.
    /// </summary>
    public class ProvisionService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<ProvisionService> _logger;
        private const string ConnectionName = "PPDM39";

        public ProvisionService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<ProvisionService> logger,
            IAccountMappingService? accountMapping = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _basisPosting = basisPosting ?? throw new ArgumentNullException(nameof(basisPosting));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountMapping = accountMapping;
        }

        public async Task<ASSET_RETIREMENT_OBLIGATION> RecordAssetRetirementObligationAsync(
            decimal estimatedCost,
            DateTime? estimatedRetirementDate,
            decimal discountRate,
            string description,
            string userId,
            string? fieldId = null,
            string? wellId = null,
            string? facilityId = null,
            string? connectionName = null)
        {
            if (estimatedCost <= 0m)
                throw new InvalidOperationException("Estimated cost must be positive");
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var presentValue = CalculatePresentValue(estimatedCost, estimatedRetirementDate, discountRate);

            var aro = new ASSET_RETIREMENT_OBLIGATION
            {
                ARO_ID = Guid.NewGuid().ToString(),
                FIELD_ID = fieldId,
                WELL_ID = wellId,
                FACILITY_ID = facilityId,
                ESTIMATED_COST = estimatedCost,
                ESTIMATED_RETIREMENT_DATE = estimatedRetirementDate,
                DISCOUNT_RATE = discountRate,
                PRESENT_VALUE = presentValue,
                ACCRETION_EXPENSE = 0m,
                STATUS = "ACTIVE",
                DESCRIPTION = description,
                SOURCE = "IAS37",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ASSET_RETIREMENT_OBLIGATION>("ASSET_RETIREMENT_OBLIGATION", cn);
            await repo.InsertAsync(aro, userId);

            var debitAccount = GetAccountId(AccountMappingKeys.AssetRetirementCost, DefaultGlAccounts.AssetRetirementCost);
            var creditAccount = GetAccountId(AccountMappingKeys.AssetRetirementObligation, DefaultGlAccounts.AssetRetirementObligation);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                debitAccount,
                creditAccount,
                presentValue,
                $"ARO recognized: {description}",
                userId,
                cn);

            _logger?.LogInformation("Recorded ARO {AroId} with PV {PresentValue}", aro.ARO_ID, presentValue);
            return aro;
        }

        public async Task<ASSET_RETIREMENT_OBLIGATION> UpdateAssetRetirementEstimateAsync(
            string aroId,
            decimal updatedEstimatedCost,
            DateTime? updatedRetirementDate,
            decimal updatedDiscountRate,
            string updateReason,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(aroId))
                throw new ArgumentNullException(nameof(aroId));
            if (updatedEstimatedCost <= 0m)
                throw new InvalidOperationException("Updated cost must be positive");
            if (string.IsNullOrWhiteSpace(updateReason))
                throw new ArgumentNullException(nameof(updateReason));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var repo = await GetRepoAsync<ASSET_RETIREMENT_OBLIGATION>("ASSET_RETIREMENT_OBLIGATION", cn);
            var aro = await repo.GetByIdAsync(aroId) as ASSET_RETIREMENT_OBLIGATION;
            if (aro == null)
                throw new InvalidOperationException($"ARO not found: {aroId}");

            var oldPv = aro.PRESENT_VALUE is decimal pv ? pv : 0m;
            var newPv = CalculatePresentValue(updatedEstimatedCost, updatedRetirementDate, updatedDiscountRate);
            var delta = newPv - oldPv;

            aro.ESTIMATED_COST = updatedEstimatedCost;
            aro.ESTIMATED_RETIREMENT_DATE = updatedRetirementDate;
            aro.DISCOUNT_RATE = updatedDiscountRate;
            aro.PRESENT_VALUE = newPv;
            aro.REMARK = $"Estimate update: {updateReason}";
            aro.ROW_CHANGED_BY = userId;
            aro.ROW_CHANGED_DATE = DateTime.UtcNow;

            await repo.UpdateAsync(aro, userId);

            if (delta != 0m)
            {
                var debitAccount = GetAccountId(AccountMappingKeys.AssetRetirementCost, DefaultGlAccounts.AssetRetirementCost);
                var creditAccount = GetAccountId(AccountMappingKeys.AssetRetirementObligation, DefaultGlAccounts.AssetRetirementObligation);

                if (delta > 0m)
                {
                    await _basisPosting.PostBalancedEntryByAccountAsync(
                        debitAccount,
                        creditAccount,
                        delta,
                        $"ARO estimate increase: {updateReason}",
                        userId,
                        cn);
                }
                else
                {
                    await _basisPosting.PostBalancedEntryByAccountAsync(
                        creditAccount,
                        debitAccount,
                        Math.Abs(delta),
                        $"ARO estimate decrease: {updateReason}",
                        userId,
                        cn);
                }
            }

            return aro;
        }

        public async Task<ASSET_RETIREMENT_OBLIGATION> RecordAccretionAsync(
            string aroId,
            DateTime asOfDate,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(aroId))
                throw new ArgumentNullException(nameof(aroId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var repo = await GetRepoAsync<ASSET_RETIREMENT_OBLIGATION>("ASSET_RETIREMENT_OBLIGATION", cn);
            var aro = await repo.GetByIdAsync(aroId) as ASSET_RETIREMENT_OBLIGATION;
            if (aro == null)
                throw new InvalidOperationException($"ARO not found: {aroId}");
            // DISCOUNT_RATE is non-nullable decimal on the model; treat values <= 0 as not provided
            if (aro.DISCOUNT_RATE <= 0m)
                throw new InvalidOperationException("Discount rate is required for accretion");

            var baseDate = aro.ROW_CHANGED_DATE ?? aro.ROW_CREATED_DATE ?? DateTime.UtcNow;
            if (asOfDate <= baseDate)
                throw new InvalidOperationException("Accretion date must be after the last update date");

            var yearFraction = (decimal)(asOfDate - baseDate).TotalDays / 365m;
            var presentValue = aro.PRESENT_VALUE;
            var accretion = presentValue * aro.DISCOUNT_RATE * yearFraction;

            if (accretion <= 0m)
                throw new InvalidOperationException("Calculated accretion must be positive");

            aro.ACCRETION_EXPENSE = (aro.ACCRETION_EXPENSE is decimal ae ? ae : 0m) + accretion;
            aro.PRESENT_VALUE = presentValue + accretion;
            aro.ROW_CHANGED_BY = userId;
            aro.ROW_CHANGED_DATE = DateTime.UtcNow;

            await repo.UpdateAsync(aro, userId);

            var debitAccount = GetAccountId(AccountMappingKeys.AccretionExpense, DefaultGlAccounts.AccretionExpense);
            var creditAccount = GetAccountId(AccountMappingKeys.AssetRetirementObligation, DefaultGlAccounts.AssetRetirementObligation);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                debitAccount,
                creditAccount,
                accretion,
                $"ARO accretion {aro.ARO_ID}",
                userId,
                cn);

            return aro;
        }

        private decimal CalculatePresentValue(decimal estimatedCost, DateTime? retirementDate, decimal? discountRate)
        {
            if (!retirementDate.HasValue || !discountRate.HasValue || discountRate.Value <= 0m)
                return estimatedCost;

            var years = (decimal)(retirementDate.Value.Date - DateTime.UtcNow.Date).TotalDays / 365m;
            if (years <= 0m)
                return estimatedCost;

            var factor = (decimal)Math.Pow((double)(1m + discountRate.Value), (double)years);
            return Math.Round(estimatedCost / factor, 2);
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), cn, tableName);
        }

        private string GetAccountId(string key, string fallback)
        {
            return _accountMapping?.GetAccountId(key) ?? fallback;
        }
    }
}



