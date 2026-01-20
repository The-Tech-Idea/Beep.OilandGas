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
    /// IFRS 3 business combinations (simplified goodwill recognition).
    /// </summary>
    public class BusinessCombinationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<BusinessCombinationService> _logger;
        private const string ConnectionName = "PPDM39";

        public BusinessCombinationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<BusinessCombinationService> logger,
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

        public async Task<ACCOUNTING_COST> RecordBusinessCombinationAsync(
            string acquisitionName,
            DateTime acquisitionDate,
            decimal considerationPaid,
            decimal netAssetsFairValue,
            string userId,
            string? netAssetsAccountId = null,
            string? considerationAccountId = null,
            string? acquisitionId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(acquisitionName))
                throw new ArgumentNullException(nameof(acquisitionName));
            if (considerationPaid <= 0m)
                throw new InvalidOperationException("Consideration must be positive");
            if (netAssetsFairValue <= 0m)
                throw new InvalidOperationException("Net assets fair value must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var goodwill = considerationPaid - netAssetsFairValue;
            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = acquisitionId ?? "BUSINESS_COMBINATION",
                COST_TYPE = "BUSINESS_COMBINATION",
                COST_CATEGORY = goodwill >= 0m ? "GOODWILL" : "BARGAIN_PURCHASE",
                AMOUNT = Math.Abs(goodwill),
                COST_DATE = acquisitionDate,
                IS_CAPITALIZED = goodwill >= 0m ? _defaults.GetActiveIndicatorYes() : _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = goodwill < 0m ? _defaults.GetActiveIndicatorYes() : _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = $"Business combination: {acquisitionName}",
                SOURCE = "IFRS3",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            var assetAccount = string.IsNullOrWhiteSpace(netAssetsAccountId)
                ? GetAccountId(AccountMappingKeys.FixedAssets, DefaultGlAccounts.FixedAssets)
                : netAssetsAccountId;

            var considerationAccount = string.IsNullOrWhiteSpace(considerationAccountId)
                ? GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash)
                : considerationAccountId;

            var lines = new List<JOURNAL_ENTRY_LINE>
            {
                new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = assetAccount,
                    DEBIT_AMOUNT = netAssetsFairValue,
                    CREDIT_AMOUNT = 0m,
                    DESCRIPTION = $"Net assets acquired {acquisitionName}"
                },
                new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = considerationAccount,
                    DEBIT_AMOUNT = 0m,
                    CREDIT_AMOUNT = considerationPaid,
                    DESCRIPTION = $"Consideration paid {acquisitionName}"
                }
            };

            if (goodwill > 0m)
            {
                lines.Add(new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = GetAccountId(AccountMappingKeys.Goodwill, DefaultGlAccounts.Goodwill),
                    DEBIT_AMOUNT = goodwill,
                    CREDIT_AMOUNT = 0m,
                    DESCRIPTION = $"Goodwill {acquisitionName}"
                });
            }
            else if (goodwill < 0m)
            {
                lines.Add(new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = GetAccountId(AccountMappingKeys.BusinessCombinationGain, DefaultGlAccounts.BusinessCombinationGain),
                    DEBIT_AMOUNT = 0m,
                    CREDIT_AMOUNT = Math.Abs(goodwill),
                    DESCRIPTION = $"Bargain purchase gain {acquisitionName}"
                });
            }

            await _basisPosting.PostEntryAsync(
                acquisitionDate,
                $"Business combination {acquisitionName}",
                lines,
                userId,
                referenceNumber: null,
                sourceModule: "BUSINESS_COMBINATION");

            _logger?.LogInformation("Recorded IFRS 3 business combination {Name} goodwill {Goodwill}",
                acquisitionName, goodwill);

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


