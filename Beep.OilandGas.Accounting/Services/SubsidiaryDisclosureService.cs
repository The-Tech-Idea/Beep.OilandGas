using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// IFRS 19 subsidiary disclosure capture for entities without public accountability.
    /// </summary>
    public class SubsidiaryDisclosureService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<SubsidiaryDisclosureService> _logger;
        private const string ConnectionName = "PPDM39";

        public SubsidiaryDisclosureService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<SubsidiaryDisclosureService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ACCOUNTING_COST> RecordDisclosureAsync(
            string subsidiaryName,
            string disclosureTopic,
            DateTime periodEnd,
            string disclosureNotes,
            string userId,
            string? subsidiaryId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(subsidiaryName))
                throw new ArgumentNullException(nameof(subsidiaryName));
            if (string.IsNullOrWhiteSpace(disclosureTopic))
                throw new ArgumentNullException(nameof(disclosureTopic));
            if (string.IsNullOrWhiteSpace(disclosureNotes))
                throw new ArgumentNullException(nameof(disclosureNotes));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var remark = $"SUBSIDIARY={subsidiaryName};TOPIC={disclosureTopic}";

            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = subsidiaryId ?? "IFRS19_DISCLOSURE",
                COST_TYPE = "IFRS19_DISCLOSURE",
                COST_CATEGORY = disclosureTopic,
                AMOUNT = 0m,
                COST_DATE = periodEnd,
                IS_CAPITALIZED = _defaults.GetActiveIndicatorNo(),
                IS_EXPENSED = _defaults.GetActiveIndicatorNo(),
                DESCRIPTION = disclosureNotes,
                REMARK = remark,
                SOURCE = "IFRS19",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            _logger?.LogInformation("Recorded IFRS 19 disclosure {Subsidiary} {Topic}",
                subsidiaryName, disclosureTopic);

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
    }
}
