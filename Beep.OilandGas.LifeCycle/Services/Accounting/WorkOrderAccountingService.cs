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
using TheTechIdea.Beep.Report;

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
            if (string.IsNullOrWhiteSpace(workOrder.WorkOrderId))
                throw new ArgumentNullException(nameof(workOrder.WorkOrderId), "Work order ID is required.");

            var cn = connectionName ?? DefaultConnectionName;

            if (!string.IsNullOrWhiteSpace(workOrder.AfeId))
            {
                var existing = await GetAfeAsync(workOrder.AfeId, cn);
                if (existing != null)
                {
                    return existing;
                }
            }

            var linkedAfe = await GetAFEForWorkOrderAsync(workOrder.WorkOrderId, cn);
            if (linkedAfe != null)
            {
                return linkedAfe;
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
            await PersistAfeLinkAsync(workOrder.WorkOrderId, afe.AFE_ID, userId, cn);

            _logger?.LogInformation("Created AFE {AfeId} for work order {WorkOrderId}", afe.AFE_ID, workOrder.WorkOrderId);
            return afe;
        }

        public async Task<AFE> CreateOrLinkAFEAsync(
            string workOrderId,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(workOrderId))
                throw new ArgumentNullException(nameof(workOrderId));

            var cn = connectionName ?? DefaultConnectionName;
            var projectWorkOrder = await GetProjectWorkOrderAsync(workOrderId, cn);
            if (projectWorkOrder != null)
            {
                return await CreateOrLinkAFEAsync(MapProjectWorkOrderToResponse(projectWorkOrder), userId, cn);
            }

            var workOrder = await GetLegacyWorkOrderAsync(workOrderId, cn);
            if (workOrder == null)
                throw new InvalidOperationException($"Work order {workOrderId} was not found.");

            var workOrderResponse = MapLegacyWorkOrderToResponse(workOrder);
            return await CreateOrLinkAFEAsync(workOrderResponse, userId, cn);
        }

        public async Task<AFE?> GetAFEForWorkOrderAsync(string workOrderId, string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(workOrderId))
                return null;

            var cn = connectionName ?? DefaultConnectionName;
            var projectWorkOrder = await GetProjectWorkOrderAsync(workOrderId, cn);
            var afeId = ExtractAfeId(GetStringValue(projectWorkOrder, "REMARK"));
            if (string.IsNullOrWhiteSpace(afeId))
            {
                var workOrder = await GetLegacyWorkOrderAsync(workOrderId, cn);
                afeId = ExtractAfeId(workOrder?.REMARK);
            }

            if (string.IsNullOrWhiteSpace(afeId))
                return null;

            return await GetAfeAsync(afeId, cn);
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

            var linkedAfe = await GetAFEForWorkOrderAsync(request.WorkOrderId, cn);
            var afeId = linkedAfe?.AFE_ID;
            if (string.IsNullOrWhiteSpace(afeId))
            {
                var afe = await CreateOrLinkAFEAsync(request.WorkOrderId, userId, cn);
                afeId = afe.AFE_ID;
            }

            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                AFE_ID = afeId ?? string.Empty,
                WELL_ID = wellId ?? string.Empty,
                FIELD_ID = fieldId ?? string.Empty,
                PROPERTY_ID = propertyId ?? string.Empty,
                COST_TYPE = request.CostType,
                COST_CATEGORY = request.CostCategory,
                AMOUNT = request.Amount,
                COST_DATE = transactionDate ?? request.TransactionDate ?? DateTime.UtcNow,
                DESCRIPTION = description ?? request.Description ?? string.Empty,
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

        private async Task<WORK_ORDER?> GetLegacyWorkOrderAsync(string workOrderId, string connectionName)
        {
            if (string.IsNullOrWhiteSpace(workOrderId))
                return null;

            var repo = await CreateRepositoryAsync("WORK_ORDER", typeof(WORK_ORDER), connectionName);
            var entity = await repo.GetByIdAsync(workOrderId);
            return entity as WORK_ORDER;
        }

        private async Task<object?> GetProjectWorkOrderAsync(string workOrderId, string connectionName)
        {
            if (string.IsNullOrWhiteSpace(workOrderId))
                return null;

            var repo = await CreateRepositoryAsync("PROJECT", typeof(PROJECT), connectionName);
            var rows = await repo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "PROJECT_ID", Operator = "=", FilterValue = workOrderId },
                new AppFilter { FieldName = "PROJECT_TYPE", Operator = "=", FilterValue = "WORK_ORDER" },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            });

            return rows.FirstOrDefault();
        }

        private async Task PersistAfeLinkAsync(string workOrderId, string? afeId, string userId, string connectionName)
        {
            if (string.IsNullOrWhiteSpace(workOrderId) || string.IsNullOrWhiteSpace(afeId))
                return;

            var projectWorkOrder = await GetProjectWorkOrderAsync(workOrderId, connectionName);
            if (projectWorkOrder != null)
            {
                var currentRemark = GetStringValue(projectWorkOrder, "REMARK");
                var updatedRemark = UpsertAfeRemark(currentRemark, afeId);
                if (!string.Equals(updatedRemark, currentRemark, StringComparison.Ordinal))
                {
                    SetStringValue(projectWorkOrder, "REMARK", updatedRemark);
                    if (projectWorkOrder is IPPDMEntity projectEntity)
                    {
                        _commonColumnHandler.PrepareForUpdate(projectEntity, userId);
                    }

                    var projectRepo = await CreateRepositoryAsync("PROJECT", typeof(PROJECT), connectionName);
                    await projectRepo.UpdateAsync(projectWorkOrder, userId);
                }

                return;
            }

            var workOrder = await GetLegacyWorkOrderAsync(workOrderId, connectionName);
            if (workOrder == null)
                return;

            var legacyRemark = workOrder.REMARK;
            var updatedLegacyRemark = UpsertAfeRemark(legacyRemark, afeId);
            if (string.Equals(updatedLegacyRemark, legacyRemark, StringComparison.Ordinal))
                return;

            workOrder.REMARK = updatedLegacyRemark;
            if (workOrder is IPPDMEntity workOrderEntity)
            {
                _commonColumnHandler.PrepareForUpdate(workOrderEntity, userId);
            }

            var repo = await CreateRepositoryAsync("WORK_ORDER", typeof(WORK_ORDER), connectionName);
            await repo.UpdateAsync(workOrder, userId);
        }

        private static string? ExtractAfeId(string? remark)
        {
            if (string.IsNullOrWhiteSpace(remark))
                return null;

            var match = Regex.Match(remark, @"AFE_ID:([^\s]+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static string UpsertAfeRemark(string? remark, string afeId)
        {
            var marker = $"AFE_ID:{afeId}";
            if (string.IsNullOrWhiteSpace(remark))
                return marker;

            if (Regex.IsMatch(remark, @"AFE_ID:[^\s]+"))
                return Regex.Replace(remark, @"AFE_ID:[^\s]+", marker);

            return string.Concat(remark.TrimEnd(), " ", marker);
        }

        private static WorkOrderResponse MapProjectWorkOrderToResponse(object projectWorkOrder)
        {
            var projectId = GetStringValue(projectWorkOrder, "PROJECT_ID");
            var workOrderNumber = GetStringValue(projectWorkOrder, "PROJECT_NUM");
            if (string.IsNullOrWhiteSpace(workOrderNumber))
            {
                workOrderNumber = projectId;
            }

            var equipmentId = GetStringValue(projectWorkOrder, "EQUIPMENT_ID");

            return new WorkOrderResponse
            {
                WorkOrderId = projectId,
                WorkOrderNumber = workOrderNumber,
                WorkOrderType = GetStringValue(projectWorkOrder, "WO_SUBTYPE"),
                EntityType = "PROJECT",
                EntityId = equipmentId,
                FieldId = NullIfEmpty(GetStringValue(projectWorkOrder, "FIELD_ID")),
                PropertyId = NullIfEmpty(equipmentId),
                Status = NullIfEmpty(GetStringValue(projectWorkOrder, "PROJECT_STATUS")),
                RequestDate = GetDateValue(projectWorkOrder, "START_DATE"),
                DueDate = GetDateValue(projectWorkOrder, "PLAN_END_DATE"),
                CompleteDate = GetDateValue(projectWorkOrder, "ACTUAL_END_DATE"),
                EstimatedCost = null,
                ActualCost = null
            };
        }

        private static WorkOrderResponse MapLegacyWorkOrderToResponse(WORK_ORDER? workOrder)
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

        private static string GetStringValue(object? entity, string propertyName)
        {
            if (entity == null)
                return string.Empty;

            var value = entity.GetType().GetProperty(propertyName)?.GetValue(entity);
            return value?.ToString() ?? string.Empty;
        }

        private static DateTime? GetDateValue(object? entity, string propertyName)
        {
            if (entity == null)
                return null;

            var value = entity.GetType().GetProperty(propertyName)?.GetValue(entity);
            if (value is DateTime dateTime)
                return dateTime;

            if (value is string text && DateTime.TryParse(text, out var parsed))
                return parsed;

            return null;
        }

        private static void SetStringValue(object entity, string propertyName, string value)
        {
            entity.GetType().GetProperty(propertyName)?.SetValue(entity, value);
        }

        private static string? NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}
