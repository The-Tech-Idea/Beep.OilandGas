using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Internal control workflows and segregation-of-duties rules.
    /// </summary>
    public class InternalControlService : IInternalControlService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<InternalControlService> _logger;
        private const string ConnectionName = "PPDM39";

        public InternalControlService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<InternalControlService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<APPROVAL_WORKFLOW> RequestApprovalAsync(string entityName, string entityId, decimal amount, string requestedBy, string? comment, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentNullException(nameof(entityName));
            if (string.IsNullOrWhiteSpace(entityId))
                throw new ArgumentNullException(nameof(entityId));
            if (string.IsNullOrWhiteSpace(requestedBy))
                throw new ArgumentNullException(nameof(requestedBy));

            var approval = new APPROVAL_WORKFLOW
            {
                APPROVAL_WORKFLOW_ID = Guid.NewGuid().ToString(),
                ENTITY_NAME = entityName,
                ENTITY_ID = entityId,
                AMOUNT = amount,
                STATUS = "PENDING",
                REQUESTED_BY = requestedBy,
                REQUESTED_DATE = DateTime.UtcNow,
                COMMENTS = comment,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = requestedBy,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<APPROVAL_WORKFLOW>("APPROVAL_WORKFLOW", cn);
            await repo.InsertAsync(approval, requestedBy);
            return approval;
        }

        public async Task<APPROVAL_WORKFLOW> ApproveAsync(string approvalId, string approverId, string? comment, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(approvalId))
                throw new ArgumentNullException(nameof(approvalId));
            if (string.IsNullOrWhiteSpace(approverId))
                throw new ArgumentNullException(nameof(approverId));

            var repo = await GetRepoAsync<APPROVAL_WORKFLOW>("APPROVAL_WORKFLOW", cn);
            var approval = await repo.GetByIdAsync(approvalId) as APPROVAL_WORKFLOW;
            if (approval == null)
                throw new ProductionAccountingException($"Approval not found: {approvalId}");

            if (!await ValidateSegregationOfDutiesAsync(approval.ENTITY_NAME, approval.REQUESTED_BY, approverId, approval.AMOUNT ?? 0m, cn))
                throw new ProductionAccountingException("Segregation-of-duties violation: approver must differ from requestor");

            approval.STATUS = "APPROVED";
            approval.APPROVED_BY = approverId;
            approval.APPROVAL_DATE = DateTime.UtcNow;
            approval.COMMENTS = comment ?? approval.COMMENTS;
            approval.ROW_CHANGED_BY = approverId;
            approval.ROW_CHANGED_DATE = DateTime.UtcNow;

            await repo.UpdateAsync(approval, approverId);
            return approval;
        }

        public async Task<List<APPROVAL_WORKFLOW>> GetApprovalsForEntityAsync(string entityName, string entityId, string cn = "PPDM39")
        {
            var repo = await GetRepoAsync<APPROVAL_WORKFLOW>("APPROVAL_WORKFLOW", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ENTITY_NAME", Operator = "=", FilterValue = entityName },
                new AppFilter { FieldName = "ENTITY_ID", Operator = "=", FilterValue = entityId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<APPROVAL_WORKFLOW>().OrderByDescending(a => a.REQUESTED_DATE).ToList()
                ?? new List<APPROVAL_WORKFLOW>();
        }

        public async Task<bool> ValidateSegregationOfDutiesAsync(string entityName, string requestedBy, string approverId, decimal amount, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentNullException(nameof(entityName));
            if (string.IsNullOrWhiteSpace(requestedBy))
                throw new ArgumentNullException(nameof(requestedBy));
            if (string.IsNullOrWhiteSpace(approverId))
                throw new ArgumentNullException(nameof(approverId));

            if (string.Equals(requestedBy, approverId, StringComparison.OrdinalIgnoreCase))
                return false;

            var rules = await GetRulesAsync(entityName, cn);
            foreach (var rule in rules)
            {
                if (rule.THRESHOLD_AMOUNT.HasValue && amount < rule.THRESHOLD_AMOUNT.Value)
                    continue;

                if (string.Equals(rule.RULE_TYPE, "SEGREGATION_OF_DUTIES", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(rule.REQUIRE_SECOND_APPROVER, "Y", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return true;
        }

        private async Task<List<INTERNAL_CONTROL_RULE>> GetRulesAsync(string entityName, string cn)
        {
            var repo = await GetRepoAsync<INTERNAL_CONTROL_RULE>("INTERNAL_CONTROL_RULE", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ENTITY_NAME", Operator = "=", FilterValue = entityName },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<INTERNAL_CONTROL_RULE>().ToList() ?? new List<INTERNAL_CONTROL_RULE>();
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
