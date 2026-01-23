using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.LifeCycle.Services.Accounting
{
    /// <summary>
    /// Service for creating AFEs and recording work order costs.
    /// </summary>
    public class WorkOrderAccountingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<WorkOrderAccountingService>? _logger;
        private const string DefaultConnectionName = "PPDM39";

        public WorkOrderAccountingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<WorkOrderAccountingService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<AFE> CreateOrLinkAFEAsync(
            WorkOrderResponse workOrder,
            string userId,
            string? connectionName = null)
        {
            if (workOrder == null)
                throw new ArgumentNullException(nameof(workOrder));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? DefaultConnectionName;

            if (!string.IsNullOrWhiteSpace(workOrder.AfeId))
            {
                var existing = await GetAfeAsync(workOrder.AfeId, cn);
                if (existing != null)
                {
                    return existing;
                }
            }

            var afe = new AFE
            {
                AFE_ID = Guid.NewGuid().ToString(),
                AFE_NUMBER = string.IsNullOrWhiteSpace(workOrder.WorkOrderNumber)
                    ? $"AFE-{DateTime.UtcNow:yyyyMMddHHmmss}"
                    : $"AFE-{workOrder.WorkOrderNumber}",
                AFE_NAME = string.IsNullOrWhiteSpace(workOrder.WorkOrderNumber)
                    ? "Work Order AFE"
                    : $"Work Order {workOrder.WorkOrderNumber}",
                FIELD_ID = workOrder.FieldId,
                PROPERTY_ID = workOrder.PropertyId,
                ESTIMATED_COST = workOrder.EstimatedCost,
                ACTUAL_COST = workOrder.ActualCost,
                STATUS = "DRAFT",
                DESCRIPTION = workOrder.WorkOrderType
            };

            if (afe is IPPDMEntity afeEntity)
            {
                _commonColumnHandler.PrepareForInsert(afeEntity, userId);
            }

            var repo = await CreateRepositoryAsync("AFE", typeof(AFE), cn);
            await repo.InsertAsync(afe, userId);

            _logger?.LogInformation("Created AFE {AfeId} for work order {WorkOrderId}", afe.AFE_ID, workOrder.WorkOrderId);
            return afe;
        }

        public async Task<string> RecordWorkOrderCostAsync(
            WorkOrderCostRequest request,
            string? wellId,
            string? facilityId,
            string? fieldId,
            string? propertyId,
            string userId,
            DateTime? transactionDate = null,
            string? description = null,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? DefaultConnectionName;

            var workOrder = await GetWorkOrderAsync(request.WorkOrderId, cn);
            var afeId = ExtractAfeId(workOrder?.REMARK);
            if (string.IsNullOrWhiteSpace(afeId))
            {
                var workOrderResponse = MapWorkOrderToResponse(workOrder);
                var afe = await CreateOrLinkAFEAsync(workOrderResponse, userId, cn);
                afeId = afe.AFE_ID;
            }

            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                AFE_ID = afeId ?? string.Empty,
                WELL_ID = wellId,
                FIELD_ID = fieldId,
                PROPERTY_ID = propertyId,
                COST_TYPE = request.CostType,
                COST_CATEGORY = request.CostCategory,
                AMOUNT = request.Amount,
                COST_DATE = transactionDate ?? request.TransactionDate ?? DateTime.UtcNow,
                DESCRIPTION = description ?? request.Description,
                IS_CAPITALIZED = request.IsCapitalized ? "Y" : "N",
                IS_EXPENSED = request.IsExpensed ? "Y" : "N"
            };

            if (cost is IPPDMEntity costEntity)
            {
                _commonColumnHandler.PrepareForInsert(costEntity, userId);
            }

            var repo = await CreateRepositoryAsync("ACCOUNTING_COST", typeof(ACCOUNTING_COST), cn);
            await repo.InsertAsync(cost, userId);

            _logger?.LogInformation("Recorded work order cost {CostId} for work order {WorkOrderId}", cost.ACCOUNTING_COST_ID, request.WorkOrderId);
            return cost.ACCOUNTING_COST_ID ?? string.Empty;
        }

        private async Task<PPDMGenericRepository> CreateRepositoryAsync(string tableName, Type fallbackType, string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            var entityType = fallbackType;
            if (metadata != null && !string.IsNullOrEmpty(metadata.EntityTypeName))
            {
                var resolved = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (resolved != null)
                {
                    entityType = resolved;
                }
            }

            return new PPDMGenericRepository(
                _editor,
                _commonColumnHandler,
                _defaults,
                _metadata,
                entityType,
                connectionName,
                tableName,
                null);
        }

        private async Task<AFE?> GetAfeAsync(string afeId, string connectionName)
        {
            var repo = await CreateRepositoryAsync("AFE", typeof(AFE), connectionName);
            var entity = await repo.GetByIdAsync(afeId);
            return entity as AFE;
        }

        private async Task<WORK_ORDER?> GetWorkOrderAsync(string workOrderId, string connectionName)
        {
            if (string.IsNullOrWhiteSpace(workOrderId))
                return null;

            var repo = await CreateRepositoryAsync("WORK_ORDER", typeof(WORK_ORDER), connectionName);
            var entity = await repo.GetByIdAsync(workOrderId);
            return entity as WORK_ORDER;
        }

        private static string? ExtractAfeId(string? remark)
        {
            if (string.IsNullOrWhiteSpace(remark))
                return null;

            var match = Regex.Match(remark, @"AFE_ID:([^\s]+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static WorkOrderResponse MapWorkOrderToResponse(WORK_ORDER? workOrder)
        {
            if (workOrder == null)
            {
                return new WorkOrderResponse();
            }

            return new WorkOrderResponse
            {
                WorkOrderId = workOrder.WORK_ORDER_ID ?? string.Empty,
                WorkOrderNumber = workOrder.WORK_ORDER_NUMBER ?? string.Empty,
                WorkOrderType = workOrder.WORK_ORDER_TYPE ?? string.Empty,
                EntityType = string.Empty,
                EntityId = string.Empty,
                FieldId = null,
                PropertyId = null,
                Status = workOrder.COMPLETE_DATE.HasValue ? "COMPLETED" : "ACTIVE",
                RequestDate = workOrder.REQUEST_DATE,
                DueDate = workOrder.DUE_DATE,
                CompleteDate = workOrder.COMPLETE_DATE,
                EstimatedCost = null,
                ActualCost = null
            };
        }
    }
}
