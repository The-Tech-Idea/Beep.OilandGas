using System;
using System.Collections.Generic;
using System.Linq;
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
    /// IAS 8 accounting policy changes with effective dating and audit trail.
    /// </summary>
    public class AccountingPolicyService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<AccountingPolicyService> _logger;
        private const string ConnectionName = "PPDM39";

        public AccountingPolicyService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<AccountingPolicyService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ACCOUNTING_METHOD> RecordPolicyChangeAsync(
            string methodType,
            DateTime effectiveDate,
            string description,
            string reason,
            string userId,
            string? fieldId = null,
            string? costCenterId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(methodType))
                throw new ArgumentNullException(nameof(methodType));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentNullException(nameof(reason));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var repo = await GetRepoAsync<ACCOUNTING_METHOD>("ACCOUNTING_METHOD", cn);

            var activePolicies = await GetActivePoliciesAsync(methodType, fieldId, costCenterId, cn);
            foreach (var policy in activePolicies)
            {
                policy.EXPIRY_DATE = effectiveDate.AddDays(-1);
                policy.ACTIVE_IND = _defaults.GetActiveIndicatorNo();
                policy.ROW_CHANGED_BY = userId;
                policy.ROW_CHANGED_DATE = DateTime.UtcNow;
                await repo.UpdateAsync(policy, userId);
            }

            var newPolicy = new ACCOUNTING_METHOD
            {
                ACCOUNTING_METHOD_ID = Guid.NewGuid().ToString(),
                METHOD_TYPE = methodType,
                FIELD_ID = fieldId,
                COST_CENTER_ID = costCenterId,
                EFFECTIVE_DATE = effectiveDate,
                EXPIRY_DATE = null,
                DESCRIPTION = description,
                REMARK = reason,
                SOURCE = "IAS8_POLICY_CHANGE",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            await repo.InsertAsync(newPolicy, userId);
            _logger?.LogInformation("Recorded accounting policy change {MethodType} effective {EffectiveDate}",
                methodType, effectiveDate);
            return newPolicy;
        }

        public async Task<ACCOUNTING_METHOD?> GetActivePolicyAsync(
            string methodType,
            string? fieldId = null,
            string? costCenterId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(methodType))
                throw new ArgumentNullException(nameof(methodType));

            var policies = await GetActivePoliciesAsync(methodType, fieldId, costCenterId, connectionName ?? ConnectionName);
            return policies.OrderByDescending(p => p.EFFECTIVE_DATE).FirstOrDefault();
        }

        private async Task<List<ACCOUNTING_METHOD>> GetActivePoliciesAsync(
            string methodType,
            string? fieldId,
            string? costCenterId,
            string cn)
        {
            var repo = await GetRepoAsync<ACCOUNTING_METHOD>("ACCOUNTING_METHOD", cn);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "METHOD_TYPE", Operator = "=", FilterValue = methodType },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            if (!string.IsNullOrWhiteSpace(fieldId))
                filters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });
            if (!string.IsNullOrWhiteSpace(costCenterId))
                filters.Add(new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId });

            var results = await repo.GetAsync(filters);
            return results?.Cast<ACCOUNTING_METHOD>().ToList() ?? new List<ACCOUNTING_METHOD>();
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), cn, tableName);
        }
    }
}
