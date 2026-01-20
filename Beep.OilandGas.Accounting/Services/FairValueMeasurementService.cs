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
    /// IFRS 13 fair value measurement and hierarchy disclosure support.
    /// </summary>
    public class FairValueMeasurementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<FairValueMeasurementService> _logger;
        private const string ConnectionName = "PPDM39";

        public FairValueMeasurementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<FairValueMeasurementService> logger,
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

        public async Task<ACCOUNTING_COST> RecordFairValueChangeAsync(
            string assetAccountId,
            DateTime valuationDate,
            decimal changeAmount,
            string userId,
            string? description = null,
            string? fairValueLevel = null,
            string? referenceId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(assetAccountId))
                throw new ArgumentNullException(nameof(assetAccountId));
            if (changeAmount == 0m)
                throw new InvalidOperationException("Fair value change cannot be zero");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = referenceId ?? assetAccountId,
                COST_TYPE = "FAIR_VALUE_MEASUREMENT",
                COST_CATEGORY = changeAmount > 0m ? "GAIN" : "LOSS",
                AMOUNT = Math.Abs(changeAmount),
                COST_DATE = valuationDate,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = description ?? $"Fair value change for {assetAccountId}",
                REMARK = string.IsNullOrWhiteSpace(fairValueLevel) ? null : $"LEVEL={fairValueLevel}",
                SOURCE = "IFRS13",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            if (changeAmount > 0m)
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    assetAccountId,
                    GetAccountId(AccountMappingKeys.FairValueGain, DefaultGlAccounts.FairValueGain),
                    changeAmount,
                    $"Fair value gain {assetAccountId}",
                    userId,
                    cn);
            }
            else
            {
                var amount = Math.Abs(changeAmount);
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    GetAccountId(AccountMappingKeys.FairValueLoss, DefaultGlAccounts.FairValueLoss),
                    assetAccountId,
                    amount,
                    $"Fair value loss {assetAccountId}",
                    userId,
                    cn);
            }

            _logger?.LogInformation("Recorded IFRS 13 fair value change {Account} amount {Amount}",
                assetAccountId, changeAmount);

            return cost;
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(T);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, tableName);
        }

        private string GetAccountId(string key, string fallback)
        {
            return _accountMapping?.GetAccountId(key) ?? fallback;
        }
    }
}



