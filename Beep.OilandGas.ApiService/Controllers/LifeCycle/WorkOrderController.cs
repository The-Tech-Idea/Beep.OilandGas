using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Services.Accounting;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.Editor;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.LifeCycle
{
    /// <summary>
    /// API controller for Work Order Accounting operations.
    /// Handles AFE creation/linking and work order cost recording with GL integration.
    /// </summary>
    [ApiController]
    [Route("api/lifecycle/workorders")]
    public class WorkOrderController : ControllerBase
    {
        private readonly WorkOrderAccountingService _workOrderAccountingService;
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<WorkOrderController> _logger;
        private const string ConnectionName = "PPDM39";

        public WorkOrderController(
            WorkOrderAccountingService workOrderAccountingService,
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<WorkOrderController> logger)
        {
            _workOrderAccountingService = workOrderAccountingService ?? throw new ArgumentNullException(nameof(workOrderAccountingService));
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        /// <summary>
        /// Create or link an AFE for a work order.
        /// </summary>
        [HttpPost("{workOrderId}/afe")]
        public async Task<ActionResult<object>> CreateOrLinkAFE(
            string workOrderId,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(workOrderId))
                    return BadRequest(new { error = "WorkOrderId is required" });

                // Get work order to convert to WorkOrderResponse
                var connName = connectionName ?? ConnectionName;
                var workOrderRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WORK_ORDER), connName, "WORK_ORDER", null);

                var workOrder = await workOrderRepo.GetByIdAsync(workOrderId) as WORK_ORDER;
                if (workOrder == null)
                    return NotFound(new { error = $"Work order {workOrderId} not found" });

                // Convert WORK_ORDER to WorkOrderResponse
                var workOrderResponse = new WorkOrderResponse
                {
                    WorkOrderId = workOrder.WORK_ORDER_ID ?? string.Empty,
                    WorkOrderNumber = workOrder.WORK_ORDER_NUMBER ?? string.Empty,
                    WorkOrderType = workOrder.WORK_ORDER_TYPE ?? string.Empty,
                    EntityType = workOrder.ENTITY_TYPE ?? string.Empty,
                    EntityId = workOrder.ENTITY_ID ?? string.Empty,
                    FieldId = workOrder.FIELD_ID,
                    PropertyId = workOrder.PROPERTY_ID,
                    Status = workOrder.STATUS,
                    RequestDate = workOrder.REQUEST_DATE,
                    DueDate = workOrder.DUE_DATE,
                    CompleteDate = workOrder.COMPLETE_DATE,
                    EstimatedCost = workOrder.ESTIMATED_COST,
                    ActualCost = workOrder.ACTUAL_COST
                };

                var afe = await _workOrderAccountingService.CreateOrLinkAFEAsync(
                    workOrderResponse,
                    userId ?? "system");

                return Ok(new
                {
                    AfeId = afe.AFE_ID,
                    AfeNumber = afe.AFE_NUMBER,
                    AfeName = afe.AFE_NAME,
                    EstimatedCost = afe.ESTIMATED_COST,
                    ActualCost = afe.ACTUAL_COST,
                    Status = afe.STATUS,
                    WorkOrderId = workOrderId
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot create AFE for work order {WorkOrderId}", workOrderId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/linking AFE for work order {WorkOrderId}", workOrderId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Record a cost for a work order and post to GL.
        /// </summary>
        [HttpPost("{workOrderId}/costs")]
        public async Task<ActionResult<object>> RecordWorkOrderCost(
            string workOrderId,
            [FromBody] WorkOrderCostRequest request,
            [FromQuery] string? wellId = null,
            [FromQuery] string? facilityId = null,
            [FromQuery] string? fieldId = null,
            [FromQuery] string? propertyId = null,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (string.IsNullOrEmpty(workOrderId))
                    return BadRequest(new { error = "WorkOrderId is required" });

                // Ensure request has the work order ID
                request.WorkOrderId = workOrderId;

                var costTransactionId = await _workOrderAccountingService.RecordWorkOrderCostAsync(
                    request,
                    wellId,
                    facilityId,
                    fieldId,
                    propertyId,
                    userId ?? "system");

                return Ok(new
                {
                    CostTransactionId = costTransactionId,
                    WorkOrderId = workOrderId,
                    Amount = request.Amount,
                    CostType = request.CostType,
                    CostCategory = request.CostCategory,
                    IsCapitalized = request.IsCapitalized,
                    Message = "Work order cost recorded successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot record cost for work order {WorkOrderId}", workOrderId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cost for work order {WorkOrderId}", workOrderId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get AFE for a work order.
        /// </summary>
        [HttpGet("{workOrderId}/afe")]
        public async Task<ActionResult<object>> GetAFEForWorkOrder(
            string workOrderId,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(workOrderId))
                    return BadRequest(new { error = "WorkOrderId is required" });

                var connName = connectionName ?? ConnectionName;
                var workOrderRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WORK_ORDER), connName, "WORK_ORDER", null);

                var workOrder = await workOrderRepo.GetByIdAsync(workOrderId) as WORK_ORDER;
                if (workOrder == null)
                    return NotFound(new { error = $"Work order {workOrderId} not found" });

                // Try to extract AFE_ID from REMARK (format: "AFE_ID:xxx")
                if (string.IsNullOrEmpty(workOrder.REMARK))
                    return NotFound(new { error = $"No AFE linked to work order {workOrderId}" });

                var afeIdMatch = System.Text.RegularExpressions.Regex.Match(workOrder.REMARK, @"AFE_ID:([^\s]+)");
                if (!afeIdMatch.Success)
                    return NotFound(new { error = $"No AFE linked to work order {workOrderId}" });

                var afeId = afeIdMatch.Groups[1].Value;
                var afeRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(AFE), connName, "AFE", null);

                var afe = await afeRepo.GetByIdAsync(afeId) as AFE;
                if (afe == null)
                    return NotFound(new { error = $"AFE {afeId} not found" });

                return Ok(new
                {
                    AfeId = afe.AFE_ID,
                    AfeNumber = afe.AFE_NUMBER,
                    AfeName = afe.AFE_NAME,
                    EstimatedCost = afe.ESTIMATED_COST,
                    ActualCost = afe.ACTUAL_COST,
                    Status = afe.STATUS,
                    Description = afe.DESCRIPTION,
                    WorkOrderId = workOrderId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AFE for work order {WorkOrderId}", workOrderId);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

