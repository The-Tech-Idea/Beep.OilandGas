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
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs.LifeCycle;
using Beep.OilandGas.ProductionAccounting.Services;
using TheTechIdea.Beep.Editor;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Accounting
{
    /// <summary>
    /// Service for integrating Work Orders with ProductionAccounting
    /// Handles AFE creation, cost transaction creation, and GL posting
    /// </summary>
    public class WorkOrderAccountingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly GLIntegrationService? _glIntegrationService;
        private readonly string _connectionName;
        private readonly ILogger<WorkOrderAccountingService>? _logger;
        private const decimal AFE_THRESHOLD = 10000m; // Create AFE for work orders over this amount

        public WorkOrderAccountingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            GLIntegrationService? glIntegrationService = null,
            string connectionName = "PPDM39",
            ILogger<WorkOrderAccountingService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _glIntegrationService = glIntegrationService;
            _connectionName = connectionName ?? "PPDM39";
            _logger = logger;
        }

        #region AFE Management

        /// <summary>
        /// Creates or links an AFE for a work order
        /// </summary>
        public async Task<AFE> CreateOrLinkAFEAsync(WorkOrderResponse workOrder, string userId)
        {
            try
            {
                // Check if AFE already exists for this work order
                var existingAfe = await GetAFEForWorkOrderAsync(workOrder.WorkOrderId);
                if (existingAfe != null)
                {
                    _logger?.LogInformation("AFE {AfeId} already exists for work order {WorkOrderId}", existingAfe.AFE_ID, workOrder.WorkOrderId);
                    return existingAfe;
                }

                // Create AFE if estimated cost exceeds threshold or if explicitly requested
                if (workOrder.EstimatedCost.HasValue && workOrder.EstimatedCost.Value >= AFE_THRESHOLD)
                {
                    var afeRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(AFE), _connectionName, "AFE", null);

                    var afe = new AFE();
                    afe.AFE_ID = _defaults.GenerateId("AFE");
                    afe.AFE_NUMBER = $"AFE-{workOrder.WorkOrderNumber}";
                    afe.AFE_NAME = $"AFE for {workOrder.WorkOrderType} - {workOrder.WorkOrderNumber}";
                    afe.PROPERTY_ID = workOrder.PropertyId;
                    afe.FIELD_ID = workOrder.FieldId;
                    afe.ESTIMATED_COST = workOrder.EstimatedCost;
                    afe.ACTUAL_COST = 0;
                    afe.STATUS = "PENDING";
                    afe.DESCRIPTION = $"AFE for work order {workOrder.WorkOrderNumber}";
                    afe.ACTIVE_IND = "Y";

                    if (afe is IPPDMEntity afeEntity)
                        _commonColumnHandler.PrepareForInsert(afeEntity, userId);
                    var result = await afeRepo.InsertAsync(afe, userId);
                    var createdAfe = result as AFE ?? throw new InvalidOperationException("Failed to create AFE");

                    // Link AFE to work order (store in REMARK or create a link table entry)
                    await LinkAFEToWorkOrderAsync(createdAfe.AFE_ID, workOrder.WorkOrderId, userId);

                    _logger?.LogInformation("AFE {AfeId} created for work order {WorkOrderId}", createdAfe.AFE_ID, workOrder.WorkOrderId);
                    return createdAfe;
                }

                throw new InvalidOperationException($"Work order estimated cost {workOrder.EstimatedCost} is below AFE threshold {AFE_THRESHOLD}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating/linking AFE for work order {WorkOrderId}", workOrder.WorkOrderId);
                throw;
            }
        }

        /// <summary>
        /// Gets AFE for a work order (stored in WORK_ORDER.REMARK or via link table)
        /// </summary>
        private async Task<AFE?> GetAFEForWorkOrderAsync(string workOrderId)
        {
            try
            {
                var workOrderRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WORK_ORDER), _connectionName, "WORK_ORDER", null);

                var workOrder = await workOrderRepo.GetByIdAsync(workOrderId) as WORK_ORDER;
                if (workOrder == null || string.IsNullOrEmpty(workOrder.REMARK))
                {
                    return null;
                }

                // Try to extract AFE_ID from REMARK (format: "AFE_ID:xxx")
                var afeIdMatch = System.Text.RegularExpressions.Regex.Match(workOrder.REMARK, @"AFE_ID:([^\s]+)");
                if (!afeIdMatch.Success)
                {
                    return null;
                }

                var afeId = afeIdMatch.Groups[1].Value;
                var afeRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(AFE), _connectionName, "AFE", null);

                return await afeRepo.GetByIdAsync(afeId) as AFE;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error getting AFE for work order {WorkOrderId}", workOrderId);
                return null;
            }
        }

        /// <summary>
        /// Links AFE to work order by storing AFE_ID in WORK_ORDER.REMARK
        /// </summary>
        private async Task LinkAFEToWorkOrderAsync(string afeId, string workOrderId, string userId)
        {
            try
            {
                var workOrderRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WORK_ORDER), _connectionName, "WORK_ORDER", null);

                var workOrder = await workOrderRepo.GetByIdAsync(workOrderId) as WORK_ORDER;
                if (workOrder == null)
                {
                    throw new InvalidOperationException($"Work order {workOrderId} not found");
                }

                workOrder.REMARK = $"AFE_ID:{afeId}";
                if (workOrder is IPPDMEntity entity)
                    _commonColumnHandler.PrepareForUpdate(entity, userId);
                await workOrderRepo.UpdateAsync(workOrder, userId);

                _logger?.LogInformation("AFE {AfeId} linked to work order {WorkOrderId}", afeId, workOrderId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error linking AFE {AfeId} to work order {WorkOrderId}", afeId, workOrderId);
                throw;
            }
        }

        #endregion

        #region Cost Transaction Management

        /// <summary>
        /// Records a cost from work order execution and posts to ProductionAccounting
        /// </summary>
        public async Task<string> RecordWorkOrderCostAsync(
            WorkOrderCostRequest request,
            string? wellId = null,
            string? facilityId = null,
            string? fieldId = null,
            string? propertyId = null,
            string userId = "system")
        {
            try
            {
                // Get work order and AFE
                var workOrderRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WORK_ORDER), _connectionName, "WORK_ORDER", null);

                var workOrder = await workOrderRepo.GetByIdAsync(request.WorkOrderId) as WORK_ORDER;
                if (workOrder == null)
                {
                    throw new InvalidOperationException($"Work order {request.WorkOrderId} not found");
                }

                var afe = await GetAFEForWorkOrderAsync(request.WorkOrderId);

                // Create COST_TRANSACTION
                var costTransactionRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(COST_TRANSACTION), _connectionName, "COST_TRANSACTION", null);

                var costTransaction = new COST_TRANSACTION();
                costTransaction.COST_TRANSACTION_ID = _defaults.GenerateId("COST_TRANSACTION");
                costTransaction.WELL_ID = wellId;
                costTransaction.FIELD_ID = fieldId;
                costTransaction.PROPERTY_ID = propertyId;
                costTransaction.COST_TYPE = request.CostType;
                costTransaction.COST_CATEGORY = request.CostCategory;
                costTransaction.AMOUNT = request.Amount;
                costTransaction.TRANSACTION_DATE = request.TransactionDate ?? DateTime.UtcNow;
                costTransaction.IS_CAPITALIZED = request.IsCapitalized ? "Y" : "N";
                costTransaction.IS_EXPENSED = request.IsExpensed ? "Y" : "N";
                costTransaction.AFE_ID = afe?.AFE_ID;
                costTransaction.DESCRIPTION = request.Description ?? $"Cost for work order {workOrder.WORK_ORDER_NUMBER}";

                if (costTransaction is IPPDMEntity costEntity)
                    _commonColumnHandler.PrepareForInsert(costEntity, userId);
                await costTransactionRepo.InsertAsync(costTransaction, userId);

                // Update AFE actual cost
                if (afe != null)
                {
                    var afeRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(AFE), _connectionName, "AFE", null);

                    afe.ACTUAL_COST = (afe.ACTUAL_COST ?? 0) + request.Amount;
                    if (afe is IPPDMEntity afeEntity)
                        _commonColumnHandler.PrepareForUpdate(afeEntity, userId);
                    await afeRepo.UpdateAsync(afe, userId);
                }

                // Update PRODUCTION_COSTS if applicable
                if (request.CostType == "WORKOVER" || request.CostType == "MAINTENANCE" || request.CostType == "REPAIR")
                {
                    await UpdateProductionCostsAsync(wellId, fieldId, propertyId, request.CostType, request.Amount, request.TransactionDate ?? DateTime.UtcNow, userId);
                }

                // Post to GL if GLIntegrationService is available
                if (_glIntegrationService != null)
                {
                    try
                    {
                        var glAccountType = request.IsCapitalized ? "DevelopmentCost" : "OperatingExpense";
                        await _glIntegrationService.PostFinancialAccountingToGL(
                            costTransaction.COST_TRANSACTION_ID,
                            glAccountType,
                            request.Amount,
                            null,
                            false,
                            request.TransactionDate,
                            userId);
                    }
                    catch (Exception glEx)
                    {
                        _logger?.LogWarning(glEx, "Failed to post work order cost to GL: {CostTransactionId}", costTransaction.COST_TRANSACTION_ID);
                        // Don't throw - GL posting failure shouldn't fail the cost recording
                    }
                }

                _logger?.LogInformation("Cost {Amount} recorded for work order {WorkOrderId}, CostTransaction: {CostTransactionId}",
                    request.Amount, request.WorkOrderId, costTransaction.COST_TRANSACTION_ID);

                return costTransaction.COST_TRANSACTION_ID;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording cost for work order {WorkOrderId}", request.WorkOrderId);
                throw;
            }
        }

        /// <summary>
        /// Updates PRODUCTION_COSTS table with work order costs
        /// </summary>
        private async Task UpdateProductionCostsAsync(
            string? wellId,
            string? fieldId,
            string? propertyId,
            string costType,
            decimal amount,
            DateTime transactionDate,
            string userId)
        {
            try
            {
                var prodCostRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PRODUCTION_COSTS), _connectionName, "PRODUCTION_COSTS", null);

                // Find existing PRODUCTION_COSTS for this period
                var filters = new List<AppFilter>
                {
                    new AppFilter { FilterName = "COST_PERIOD", FilterValue = transactionDate.ToString("yyyy-MM-01"), FilterOperator = ">=" }
                };

                if (!string.IsNullOrEmpty(propertyId))
                {
                    filters.Add(new AppFilter { FilterName = "PROPERTY_ID", FilterValue = propertyId, FilterOperator = "=" });
                }

                var existingCosts = await prodCostRepo.GetEntityAsync(filters);
                var productionCost = existingCosts?.OfType<PRODUCTION_COSTS>().FirstOrDefault();

                if (productionCost == null)
                {
                    // Create new PRODUCTION_COSTS record
                    productionCost = new PRODUCTION_COSTS();
                    productionCost.PRODUCTION_COST_ID = _defaults.GenerateId("PRODUCTION_COSTS");
                    productionCost.PROPERTY_ID = propertyId;
                    productionCost.COST_PERIOD = transactionDate;
                    productionCost.OPERATING_COSTS = 0;
                    productionCost.WORKOVER_COSTS = 0;
                    productionCost.MAINTENANCE_COSTS = 0;
                    productionCost.TOTAL_PRODUCTION_COSTS = 0;
                    productionCost.ACTIVE_IND = "Y";

                    if (productionCost is IPPDMEntity prodCostEntity)
                        _commonColumnHandler.PrepareForInsert(prodCostEntity, userId);
                    await prodCostRepo.InsertAsync(productionCost, userId);
                }

                // Update appropriate cost field
                if (costType == "WORKOVER")
                {
                    productionCost.WORKOVER_COSTS = (productionCost.WORKOVER_COSTS ?? 0) + amount;
                }
                else if (costType == "MAINTENANCE")
                {
                    productionCost.MAINTENANCE_COSTS = (productionCost.MAINTENANCE_COSTS ?? 0) + amount;
                }
                else
                {
                    productionCost.OPERATING_COSTS = (productionCost.OPERATING_COSTS ?? 0) + amount;
                }

                productionCost.TOTAL_PRODUCTION_COSTS = (productionCost.OPERATING_COSTS ?? 0) +
                    (productionCost.WORKOVER_COSTS ?? 0) + (productionCost.MAINTENANCE_COSTS ?? 0);

                if (productionCost is IPPDMEntity prodCostEntity)
                    _commonColumnHandler.PrepareForUpdate(prodCostEntity, userId);
                await prodCostRepo.UpdateAsync(productionCost, userId);

                _logger?.LogInformation("PRODUCTION_COSTS updated for property {PropertyId}, cost type {CostType}, amount {Amount}",
                    propertyId, costType, amount);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error updating PRODUCTION_COSTS for work order cost");
                // Don't throw - PRODUCTION_COSTS update failure shouldn't fail the cost recording
            }
        }

        #endregion
    }
}

