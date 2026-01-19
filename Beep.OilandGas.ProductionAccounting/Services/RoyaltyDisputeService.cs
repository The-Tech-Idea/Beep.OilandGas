using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Royalty;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Royalty dispute workflows.
    /// </summary>
    public class RoyaltyDisputeService : IRoyaltyDisputeService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<RoyaltyDisputeService> _logger;
        private const string ConnectionName = "PPDM39";

        public RoyaltyDisputeService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<RoyaltyDisputeService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<ROYALTY_DISPUTE> CreateDisputeAsync(ROYALTY_DISPUTE dispute, string userId, string cn = "PPDM39")
        {
            if (dispute == null)
                throw new ArgumentNullException(nameof(dispute));
            if (string.IsNullOrWhiteSpace(dispute.ROYALTY_STATEMENT_ID))
                throw new ProductionAccountingException("ROYALTY_STATEMENT_ID is required");

            dispute.ROYALTY_DISPUTE_ID ??= Guid.NewGuid().ToString();
            dispute.STATUS ??= "OPEN";
            dispute.DISPUTE_DATE ??= DateTime.UtcNow;
            dispute.ACTIVE_IND = _defaults.GetActiveIndicatorYes();
            dispute.PPDM_GUID ??= Guid.NewGuid().ToString();
            dispute.ROW_CREATED_BY = userId;
            dispute.ROW_CREATED_DATE = DateTime.UtcNow;

            var repo = await GetRepoAsync<ROYALTY_DISPUTE>("ROYALTY_DISPUTE", cn);
            await repo.InsertAsync(dispute, userId);
            return dispute;
        }

        public async Task<ROYALTY_DISPUTE> ResolveDisputeAsync(string disputeId, DateTime resolutionDate, string resolutionNotes, string userId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(disputeId))
                throw new ArgumentNullException(nameof(disputeId));

            var repo = await GetRepoAsync<ROYALTY_DISPUTE>("ROYALTY_DISPUTE", cn);
            var dispute = await repo.GetByIdAsync(disputeId) as ROYALTY_DISPUTE;
            if (dispute == null)
                throw new ProductionAccountingException($"Dispute not found: {disputeId}");

            dispute.STATUS = "RESOLVED";
            dispute.RESOLUTION_DATE = resolutionDate;
            dispute.RESOLUTION_NOTES = resolutionNotes;
            dispute.ROW_CREATED_BY ??= userId;
            dispute.ROW_CREATED_DATE ??= DateTime.UtcNow;

            await repo.UpdateAsync(dispute, userId);
            return dispute;
        }

        public async Task<List<ROYALTY_DISPUTE>> GetDisputesAsync(string royaltyOwnerBaId, string? status, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(royaltyOwnerBaId))
                throw new ArgumentNullException(nameof(royaltyOwnerBaId));

            var repo = await GetRepoAsync<ROYALTY_DISPUTE>("ROYALTY_DISPUTE", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ROYALTY_OWNER_BA_ID", Operator = "=", FilterValue = royaltyOwnerBaId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrWhiteSpace(status))
                filters.Add(new AppFilter { FieldName = "STATUS", Operator = "=", FilterValue = status });

            var results = await repo.GetAsync(filters);
            return results?.Cast<ROYALTY_DISPUTE>().ToList() ?? new List<ROYALTY_DISPUTE>();
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
