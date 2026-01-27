using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data;
using TheTechIdea.Beep.Editor;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.LifeCycle.Services.WorkOrder
{
    /// <summary>
    /// Comprehensive service for Work Order management including creation, tracking, and integration with AFEs
    /// </summary>
    public class WorkOrderManagementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<WorkOrderManagementService>? _logger;

        public WorkOrderManagementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<WorkOrderManagementService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? "PPDM39";
            _logger = logger;
        }

        #region Work Order Creation

        /// <summary>
        /// Creates a new work order linked to WELL_ID, FACILITY_ID, or PIPELINE_ID
        /// </summary>
        public async Task<WorkOrderResponse> CreateWorkOrderAsync(WorkOrderCreationRequest request, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WORK_ORDER");
                if (metadata == null)
                {
                    throw new InvalidOperationException("WORK_ORDER table metadata not found");
                }

                var entityType = typeof(WORK_ORDER);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "WORK_ORDER", null);

                // Create WORK_ORDER entity
                var workOrder = new WORK_ORDER();
                workOrder.WORK_ORDER_ID = _defaults.GenerateId("WORK_ORDER");
                workOrder.WORK_ORDER_NUMBER = request.WorkOrderNumber;
                workOrder.WORK_ORDER_TYPE = request.WorkOrderType;
                workOrder.INSTRUCTIONS = request.Instructions;
                workOrder.REQUEST_DATE = request.RequestDate ?? DateTime.UtcNow;
                workOrder.DUE_DATE = request.DueDate;
                workOrder.ACTIVE_IND = "Y";

                // Set common columns
                if (workOrder is IPPDMEntity entity)
                    _commonColumnHandler.PrepareForInsert(entity, userId);

                // Insert work order
                var result = await repo.InsertAsync(workOrder, userId);
                var createdWorkOrder = result as WORK_ORDER ?? throw new InvalidOperationException("Failed to create work order");

                // Link work order to entity using WORK_ORDER_XREF
                await LinkWorkOrderToEntityAsync(createdWorkOrder.WORK_ORDER_ID, request.EntityType, request.EntityId, userId);

                _logger?.LogInformation("Work order created: {WorkOrderId}, Number: {WorkOrderNumber}, Type: {WorkOrderType}",
                    createdWorkOrder.WORK_ORDER_ID, createdWorkOrder.WORK_ORDER_NUMBER, createdWorkOrder.WORK_ORDER_TYPE);

                return new WorkOrderResponse
                {
                    WorkOrderId = createdWorkOrder.WORK_ORDER_ID,
                    WorkOrderNumber = createdWorkOrder.WORK_ORDER_NUMBER ?? string.Empty,
                    WorkOrderType = createdWorkOrder.WORK_ORDER_TYPE ?? string.Empty,
                    EntityType = request.EntityType,
                    EntityId = request.EntityId,
                    FieldId = request.FieldId,
                    PropertyId = request.PropertyId,
                    Status = "REQUESTED",
                    RequestDate = createdWorkOrder.REQUEST_DATE,
                    DueDate = createdWorkOrder.DUE_DATE,
                    EstimatedCost = request.EstimatedCost,
                    Properties = request.AdditionalProperties
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating work order: {WorkOrderNumber}", request.WorkOrderNumber);
                throw;
            }
        }

        /// <summary>
        /// Links a work order to an entity (WELL, FACILITY, or PIPELINE) using WORK_ORDER_XREF
        /// </summary>
        private async Task LinkWorkOrderToEntityAsync(string workOrderId, string entityType, string entityId, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WORK_ORDER_XREF");

                if (metadata == null)
                {
                    throw new InvalidOperationException("WORK_ORDER_XREF table metadata not found");
                }

                var entityTypeXref = typeof(WORK_ORDER_XREF);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityTypeXref, _connectionName, "WORK_ORDER_XREF", null);

                var xref = new WORK_ORDER_XREF();
                xref.WORK_ORDER_XREF_ID = _defaults.GenerateId("WORK_ORDER_XREF");
                xref.WORK_ORDER_ID = workOrderId;
                xref.REFERENCE_ID = entityId;
                xref.WO_XREF_TYPE = entityType;
                xref.EFFECTIVE_DATE = DateTime.UtcNow;
                xref.ACTIVE_IND = "Y";

                if (xref is IPPDMEntity xrefEntity)
                    _commonColumnHandler.PrepareForInsert(xrefEntity, userId);
                await repo.InsertAsync(xref, userId);

                _logger?.LogInformation("Work order {WorkOrderId} linked to {EntityType} {EntityId}", workOrderId, entityType, entityId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error linking work order {WorkOrderId} to {EntityType} {EntityId}", workOrderId, entityType, entityId);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing work order
        /// </summary>
        public async Task<WorkOrderResponse> UpdateWorkOrderAsync(WorkOrderUpdateRequest request, string userId)
        {
            try
            {
                var entityType = typeof(WORK_ORDER);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "WORK_ORDER", null);

                var workOrder = await repo.GetByIdAsync(request.WorkOrderId) as WORK_ORDER;
                if (workOrder == null)
                {
                    throw new InvalidOperationException($"Work order {request.WorkOrderId} not found");
                }

                // Update fields
                if (!string.IsNullOrEmpty(request.Status))
                {
                    // Status is typically stored in a status field or derived from dates
                    if (request.Status == "COMPLETED" && request.CompleteDate.HasValue)
                    {
                        workOrder.COMPLETE_DATE = request.CompleteDate.Value;
                    }
                }

                if (!string.IsNullOrEmpty(request.Instructions))
                    workOrder.INSTRUCTIONS = request.Instructions;

                if (request.DueDate.HasValue)
                    workOrder.DUE_DATE = request.DueDate.Value;

                if (request.CompleteDate.HasValue)
                    workOrder.COMPLETE_DATE = request.CompleteDate.Value;

                if (workOrder is IPPDMEntity entity)
                    _commonColumnHandler.PrepareForUpdate(entity, userId);
                await repo.UpdateAsync(workOrder, userId);

                _logger?.LogInformation("Work order updated: {WorkOrderId}", workOrder.WORK_ORDER_ID);

                return await GetWorkOrderAsync(workOrder.WORK_ORDER_ID);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating work order: {WorkOrderId}", request.WorkOrderId);
                throw;
            }
        }

        /// <summary>
        /// Gets a work order by ID
        /// </summary>
        public async Task<WorkOrderResponse> GetWorkOrderAsync(string workOrderId)
        {
            try
            {
                var entityType = typeof(WORK_ORDER);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "WORK_ORDER", null);

                var workOrder = await repo.GetByIdAsync(workOrderId) as WORK_ORDER;
                if (workOrder == null)
                {
                    throw new InvalidOperationException($"Work order {workOrderId} not found");
                }

                // Get linked entity information
                var xrefRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WORK_ORDER_XREF), _connectionName, "WORK_ORDER_XREF", null);

                var filter = new AppFilter
                {
                    FieldName = "WORK_ORDER_ID",
                    FilterValue = workOrderId,
                    Operator = "="
                };

                var xrefs = await xrefRepo.GetAsync(new List<AppFilter> { filter });
                var xref = xrefs?.FirstOrDefault() as WORK_ORDER_XREF;

                // Get AFE actual cost and ID if AFE exists
                decimal? actualCost = null;
                string? afeId = null;
                if (!string.IsNullOrEmpty(workOrder.REMARK))
                {
                    var afeIdMatch = System.Text.RegularExpressions.Regex.Match(workOrder.REMARK, @"AFE_ID:([^\s]+)");
                    if (afeIdMatch.Success)
                    {
                        afeId = afeIdMatch.Groups[1].Value;
                        var afeRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                            typeof(AFE), _connectionName, "AFE", null);
                        var afe = await afeRepo.GetByIdAsync(afeId) as AFE;
                        if (afe != null)
                        {
                            actualCost = afe.ACTUAL_COST;
                        }
                    }
                }

                return new WorkOrderResponse
                {
                    WorkOrderId = workOrder.WORK_ORDER_ID,
                    WorkOrderNumber = workOrder.WORK_ORDER_NUMBER ?? string.Empty,
                    WorkOrderType = workOrder.WORK_ORDER_TYPE ?? string.Empty,
                    EntityType = xref?.WO_XREF_TYPE ?? string.Empty,
                    EntityId = xref?.REFERENCE_ID ?? string.Empty,
                    Status = workOrder.COMPLETE_DATE.HasValue ? "COMPLETED" : "ACTIVE",
                    AfeId = afeId,
                    RequestDate = workOrder.REQUEST_DATE,
                    DueDate = workOrder.DUE_DATE,
                    CompleteDate = workOrder.COMPLETE_DATE,
                    ActualCost = actualCost
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting work order: {WorkOrderId}", workOrderId);
                throw;
            }
        }

        /// <summary>
        /// Gets all work orders for a specific entity
        /// </summary>
        public async Task<List<WorkOrderResponse>> GetWorkOrdersByEntityAsync(string entityType, string entityId)
        {
            try
            {
                var xrefRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WORK_ORDER_XREF), _connectionName, "WORK_ORDER_XREF", null);

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "WO_XREF_TYPE", FilterValue = entityType, Operator = "=" },
                    new AppFilter { FieldName = "REFERENCE_ID", FilterValue = entityId, Operator = "=" },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                };

                var xrefs = await xrefRepo.GetAsync(filters);
                if (xrefs == null || !xrefs.Any())
                {
                    return new List<WorkOrderResponse>();
                }

                var workOrderIds = xrefs.OfType<WORK_ORDER_XREF>()
                    .Select(x => x.WORK_ORDER_ID)
                    .Distinct()
                    .ToList();

                var workOrderRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WORK_ORDER), _connectionName, "WORK_ORDER", null);

                var results = new List<WorkOrderResponse>();
                foreach (var workOrderId in workOrderIds)
                {
                    try
                    {
                        var workOrder = await GetWorkOrderAsync(workOrderId);
                        results.Add(workOrder);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Error getting work order {WorkOrderId} for entity {EntityType} {EntityId}",
                            workOrderId, entityType, entityId);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting work orders for {EntityType} {EntityId}", entityType, entityId);
                throw;
            }
        }

        #endregion
    }
}

