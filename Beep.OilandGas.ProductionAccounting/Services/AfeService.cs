using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// AFE Service - Manages Authorization for Expenditure budgets and actuals.
    /// </summary>
    public class AfeService : IAfeService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IAuthorizationWorkflowService _authorizationWorkflowService;
        private readonly ILogger<AfeService> _logger;
        private const string ConnectionName = "PPDM39";

        public AfeService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<AfeService> logger = null,
            IAuthorizationWorkflowService authorizationWorkflowService = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _authorizationWorkflowService = authorizationWorkflowService;
            _logger = logger;
        }

        public async Task<AFE> CreateAfeAsync(AFE afe, string userId, string cn = "PPDM39")
        {
            if (afe == null)
                throw new ArgumentNullException(nameof(afe));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrWhiteSpace(afe.AFE_NUMBER))
                throw new ProductionAccountingException("AFE_NUMBER is required");

            afe.AFE_ID ??= Guid.NewGuid().ToString();
            afe.STATUS ??= "DRAFT";
            afe.ACTIVE_IND = _defaults.GetActiveIndicatorYes();
            afe.PPDM_GUID ??= Guid.NewGuid().ToString();
            afe.ROW_CREATED_BY = userId;
            afe.ROW_CREATED_DATE = DateTime.UtcNow;

            var metadata = await _metadata.GetTableMetadataAsync("AFE");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(AFE);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "AFE");

            await repo.InsertAsync(afe, userId);
            return afe;
        }

        public async Task<AFE_LINE_ITEM> AddLineItemAsync(
            AFE_LINE_ITEM lineItem,
            string userId,
            string cn = "PPDM39")
        {
            if (lineItem == null)
                throw new ArgumentNullException(nameof(lineItem));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrWhiteSpace(lineItem.AFE_ID))
                throw new ProductionAccountingException("AFE_ID is required");

            lineItem.AFE_LINE_ITEM_ID ??= Guid.NewGuid().ToString();
            lineItem.ACTIVE_IND = _defaults.GetActiveIndicatorYes();
            lineItem.PPDM_GUID ??= Guid.NewGuid().ToString();
            lineItem.ROW_CREATED_BY = userId;
            lineItem.ROW_CREATED_DATE = DateTime.UtcNow;
            lineItem.VARIANCE = (lineItem.ACTUAL_AMOUNT ?? 0m) - (lineItem.BUDGET_AMOUNT ?? 0m);

            var metadata = await _metadata.GetTableMetadataAsync("AFE_LINE_ITEM");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(AFE_LINE_ITEM);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "AFE_LINE_ITEM");

            await repo.InsertAsync(lineItem, userId);
            await UpdateAfeActualsAsync(lineItem.AFE_ID, userId, cn);
            return lineItem;
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

        public async Task<ACCOUNTING_COST> RecordCostAsync(
            string afeId,
            ACCOUNTING_COST cost,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(afeId))
                throw new ArgumentNullException(nameof(afeId));
            if (cost == null)
                throw new ArgumentNullException(nameof(cost));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (cost.AMOUNT <= 0)
                throw new ProductionAccountingException("Cost amount must be positive");

            var afe = await GetAfeAsync(afeId, cn);
            if (afe == null)
                throw new ProductionAccountingException($"AFE not found: {afeId}");
            if (!string.Equals(afe.STATUS, "APPROVED", StringComparison.OrdinalIgnoreCase))
                throw new ProductionAccountingException("AFE must be approved before recording costs");

            cost.ACCOUNTING_COST_ID ??= Guid.NewGuid().ToString();
            cost.AFE_ID = afeId;
            cost.ACTIVE_IND = _defaults.GetActiveIndicatorYes();
            cost.PPDM_GUID ??= Guid.NewGuid().ToString();
            cost.ROW_CREATED_BY = userId;
            cost.ROW_CREATED_DATE = DateTime.UtcNow;

            if (_authorizationWorkflowService != null)
            {
                var isAuthorized = await _authorizationWorkflowService.IsCostAuthorizedAsync(cost, cn);
                if (!isAuthorized)
                    throw new ProductionAccountingException("Cost exceeds AFE authorization or AFE is not approved");
            }

            var metadata = await _metadata.GetTableMetadataAsync("ACCOUNTING_COST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ACCOUNTING_COST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ACCOUNTING_COST");

            await repo.InsertAsync(cost, userId);
            await UpdateAfeActualsAsync(afeId, userId, cn);
            return cost;
        }

        public async Task<List<AFE_LINE_ITEM>> GetLineItemsAsync(string afeId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(afeId))
                throw new ArgumentNullException(nameof(afeId));

            var metadata = await _metadata.GetTableMetadataAsync("AFE_LINE_ITEM");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(AFE_LINE_ITEM);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "AFE_LINE_ITEM");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AFE_ID", Operator = "=", FilterValue = afeId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<AFE_LINE_ITEM>().ToList() ?? new List<AFE_LINE_ITEM>();
        }

        public async Task<COST_VARIANCE_REPORT> GenerateBudgetVarianceReportAsync(
            string afeId,
            decimal varianceThreshold,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(afeId))
                throw new ArgumentNullException(nameof(afeId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var lineItems = await GetLineItemsAsync(afeId, cn);
            var budgetAmount = lineItems.Sum(li => li.BUDGET_AMOUNT ?? 0m);
            var actualAmount = await GetCostTotalAsync(afeId, cn);
            var varianceAmount = actualAmount - budgetAmount;
            var variancePercent = budgetAmount != 0m ? (varianceAmount / budgetAmount) * 100m : 0m;
            var status = Math.Abs(variancePercent) >= varianceThreshold ? "THRESHOLD_EXCEEDED" : "OK";

            var report = new COST_VARIANCE_REPORT
            {
                COST_VARIANCE_REPORT_ID = Guid.NewGuid().ToString(),
                AFE_ID = afeId,
                BUDGET_AMOUNT = budgetAmount,
                ACTUAL_AMOUNT = actualAmount,
                VARIANCE_AMOUNT = varianceAmount,
                VARIANCE_PERCENT = variancePercent,
                STATUS = status,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var metadata = await _metadata.GetTableMetadataAsync("COST_VARIANCE_REPORT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(COST_VARIANCE_REPORT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "COST_VARIANCE_REPORT");

            await repo.InsertAsync(report, userId);
            return report;
        }

        public async Task<List<COST_VARIANCE_REPORT>> GetVarianceReportsAsync(
            string? afeId,
            string? costCenterId,
            DateTime? startDate,
            DateTime? endDate,
            string cn = "PPDM39")
        {
            var metadata = await _metadata.GetTableMetadataAsync("COST_VARIANCE_REPORT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(COST_VARIANCE_REPORT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "COST_VARIANCE_REPORT");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrWhiteSpace(afeId))
                filters.Add(new AppFilter { FieldName = "AFE_ID", Operator = "=", FilterValue = afeId });
            if (!string.IsNullOrWhiteSpace(costCenterId))
                filters.Add(new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId });
            if (startDate.HasValue)
                filters.Add(new AppFilter { FieldName = "ROW_CREATED_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
            if (endDate.HasValue)
                filters.Add(new AppFilter { FieldName = "ROW_CREATED_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });

            var results = await repo.GetAsync(filters);
            return results?.Cast<COST_VARIANCE_REPORT>().ToList() ?? new List<COST_VARIANCE_REPORT>();
        }

        private async Task<AFE?> GetAfeAsync(string afeId, string cn)
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

        private async Task UpdateAfeActualsAsync(string afeId, string userId, string cn)
        {
            var afe = await GetAfeAsync(afeId, cn);
            if (afe == null)
                return;

            var lineItems = await GetLineItemsAsync(afeId, cn);
            var lineItemTotal = lineItems.Sum(li => li.ACTUAL_AMOUNT ?? 0m);

            var costTotal = await GetCostTotalAsync(afeId, cn);
            var actualTotal = Math.Max(lineItemTotal, costTotal);

            afe.ACTUAL_COST = actualTotal;
            afe.ROW_CHANGED_BY = userId;
            afe.ROW_CHANGED_DATE = DateTime.UtcNow;

            var metadata = await _metadata.GetTableMetadataAsync("AFE");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(AFE);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "AFE");

            await repo.UpdateAsync(afe, userId);
        }

        private async Task<decimal> GetCostTotalAsync(string afeId, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("ACCOUNTING_COST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ACCOUNTING_COST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ACCOUNTING_COST");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AFE_ID", Operator = "=", FilterValue = afeId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var costs = results?.Cast<ACCOUNTING_COST>().ToList() ?? new List<ACCOUNTING_COST>();
            return costs.Sum(c => c.AMOUNT);
        }
    }
}
