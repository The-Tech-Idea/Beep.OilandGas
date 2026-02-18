using System;
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
    /// IAS 36 impairment tracking and posting.
    /// </summary>
    public class ImpairmentService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<ImpairmentService> _logger;
        private const string ConnectionName = "PPDM39";

        public ImpairmentService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<ImpairmentService> logger,
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

        public async Task<IMPAIRMENT_RECORD> RecordImpairmentAsync(
            string propertyId,
            string costCenterId,
            DateTime impairmentDate,
            decimal impairmentAmount,
            string impairmentType,
            string reason,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                throw new ArgumentNullException(nameof(propertyId));
            if (string.IsNullOrWhiteSpace(costCenterId))
                throw new ArgumentNullException(nameof(costCenterId));
            if (impairmentAmount <= 0m)
                throw new InvalidOperationException("Impairment amount must be positive");
            if (string.IsNullOrWhiteSpace(impairmentType))
                throw new ArgumentNullException(nameof(impairmentType));
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentNullException(nameof(reason));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var impairment = new IMPAIRMENT_RECORD
            {
                IMPAIRMENT_RECORD_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = propertyId,
                COST_CENTER_ID = costCenterId,
                IMPAIRMENT_DATE = impairmentDate,
                IMPAIRMENT_AMOUNT = impairmentAmount,
                IMPAIRMENT_TYPE = impairmentType,
                REASON = reason,
                SOURCE = "IAS36",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<IMPAIRMENT_RECORD>("IMPAIRMENT_RECORD", cn);
            await repo.InsertAsync(impairment, userId);

            var lossAccount = GetAccountId(AccountMappingKeys.ImpairmentLoss, DefaultGlAccounts.ImpairmentLoss);
            var allowanceAccount = GetAccountId(AccountMappingKeys.ImpairmentAllowance, DefaultGlAccounts.ImpairmentAllowance);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                lossAccount,
                allowanceAccount,
                impairmentAmount,
                $"Impairment {impairmentType} {propertyId}",
                userId,
                cn);

            _logger?.LogInformation("Impairment recorded for {PropertyId} amount {Amount}",
                propertyId, impairmentAmount);

            return impairment;
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



