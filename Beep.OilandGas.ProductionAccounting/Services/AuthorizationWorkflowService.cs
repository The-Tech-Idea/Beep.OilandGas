using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionAccounting.Exceptions;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Authorization workflow service for accounting transactions (AFE approvals).
    /// </summary>
    public class AuthorizationWorkflowService : IAuthorizationWorkflowService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<AuthorizationWorkflowService> _logger;
        private const string ConnectionName = "PPDM39";

        public AuthorizationWorkflowService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<AuthorizationWorkflowService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<bool> ValidateAfeAuthorizationAsync(string afeId, string userId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(afeId))
                throw new ArgumentNullException(nameof(afeId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var afe = await GetAfeAsync(afeId, cn);
            if (afe == null)
                return false;

            if (!string.Equals(afe.ACTIVE_IND, _defaults.GetActiveIndicatorYes(), StringComparison.OrdinalIgnoreCase))
                return false;

            return string.Equals(afe.STATUS, "APPROVED", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<AFE> ApproveAfeAsync(string afeId, DateTime approvalDate, string userId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(afeId))
                throw new ArgumentNullException(nameof(afeId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var afe = await GetAfeAsync(afeId, cn);
            if (afe == null)
                throw new ProductionAccountingException($"AFE not found: {afeId}");

            afe.STATUS = "APPROVED";
            afe.APPROVAL_DATE = approvalDate;
            afe.ROW_CHANGED_BY = userId;
            afe.ROW_CHANGED_DATE = DateTime.UtcNow;

            var metadata = await _metadata.GetTableMetadataAsync("AFE");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(AFE);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "AFE");

            await repo.UpdateAsync(afe, userId);
            return afe;
        }

        public async Task<bool> IsCostAuthorizedAsync(ACCOUNTING_COST cost, string cn = "PPDM39")
        {
            if (cost == null)
                throw new ArgumentNullException(nameof(cost));

            if (string.IsNullOrWhiteSpace(cost.AFE_ID))
                return false;

            var afe = await GetAfeAsync(cost.AFE_ID, cn);
            if (afe == null)
                return false;

            if (!string.Equals(afe.ACTIVE_IND, _defaults.GetActiveIndicatorYes(), StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.Equals(afe.STATUS, "APPROVED", StringComparison.OrdinalIgnoreCase))
                return false;

            if (afe.ESTIMATED_COST.HasValue)
            {
                var projected = (afe.ACTUAL_COST ?? 0m) + cost.AMOUNT;
                if (projected > afe.ESTIMATED_COST.GetValueOrDefault(0m))
                    return false;
            }

            return true;
        }

        private async Task<AFE> GetAfeAsync(string afeId, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("AFE");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(AFE);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "AFE");

            var result = await repo.GetByIdAsync(afeId);
            return result as AFE;
        }
    }
}
