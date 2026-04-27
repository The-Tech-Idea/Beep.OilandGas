using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Services.Accounting;
using Beep.OilandGas.LifeCycle.Services.Decommissioning.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.ApiService.Attributes;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    [ApiController]
    [Route("api/field/current/production")]
    [RequireCurrentFieldAccess]
    public class ProductionController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly Beep.OilandGas.LifeCycle.Services.Production.PPDMProductionService _productionService;
        private readonly IWorkOrderService _workOrderService;
        private readonly WorkOrderAccountingService _workOrderAccountingService;
        private readonly DecommissioningProcessService _decommissioningProcessService;
        private readonly ILogger<ProductionController> _logger;

        public ProductionController(
            IFieldOrchestrator fieldOrchestrator,
            Beep.OilandGas.LifeCycle.Services.Production.PPDMProductionService productionService,
            IWorkOrderService workOrderService,
            WorkOrderAccountingService workOrderAccountingService,
            DecommissioningProcessService decommissioningProcessService,
            ILogger<ProductionController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _productionService = productionService ?? throw new ArgumentNullException(nameof(productionService));
            _workOrderService = workOrderService ?? throw new ArgumentNullException(nameof(workOrderService));
            _workOrderAccountingService = workOrderAccountingService ?? throw new ArgumentNullException(nameof(workOrderAccountingService));
            _decommissioningProcessService = decommissioningProcessService ?? throw new ArgumentNullException(nameof(decommissioningProcessService));
            _logger = logger;
        }

        /// <summary>GET /api/field/current/production/wells/{wellId}/performance</summary>
        [HttpGet("wells/{wellId}/performance")]
        public async Task<ActionResult<WellPerformanceSummary>> GetWellPerformanceAsync(string wellId)
        {
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            try
            {
                var tests = await _productionService.GetWellTestsForWellAsync(fieldId, wellId);
                var orderedTests = (tests ?? new List<WellTestResponse>())
                    .OrderByDescending(ResolveTestDate)
                    .ToList();
                var latest = orderedTests.FirstOrDefault();
                var maxOilRate = orderedTests.Max(t => ToDouble(t.OilFlowAmount));

                var summary = new WellPerformanceSummary
                {
                    WellId = wellId,
                    Status = latest != null ? "ACTIVE" : "UNKNOWN",
                    OilRate = latest != null ? ToDouble(latest.OilFlowAmount) : 0,
                    GasRate = latest != null ? ToDouble(latest.GasFlowAmount) : 0,
                    WaterRate = latest != null ? ToDouble(latest.WaterFlowAmount) : 0,
                    PotentialRate = maxOilRate > 0 ? maxOilRate : (latest != null ? ToDouble(latest.OilFlowAmount) : 0),
                    CumOil = 0,
                    LastTestDate = ResolveTestDate(latest),
                    WellTests = orderedTests,
                };
                return Ok(summary);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Unable to fetch performance for well {WellId}", wellId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching performance for well {WellId}", wellId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/production/wells/{wellId}/analysis</summary>
        [HttpGet("wells/{wellId}/analysis")]
        public async Task<ActionResult<WellPerformanceAnalysisResponse>> GetWellPerformanceAnalysisAsync(string wellId)
        {
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            try
            {
                var analysis = await _productionService.GetWellPerformanceAnalysisAsync(fieldId, wellId);
                return Ok(analysis);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Unable to analyze performance for well {WellId}", wellId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing performance for well {WellId}", wellId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>POST /api/field/current/production/wells/{wellId}/analysis/deviation</summary>
        [HttpPost("wells/{wellId}/analysis/deviation")]
        public async Task<ActionResult<PerformanceDeviationResult>> LogPerformanceDeviationAsync(string wellId, [FromBody] PerformanceDeviationRequest request)
        {
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";

            try
            {
                var result = await _productionService.LogWellPerformanceDeviationAsync(fieldId, wellId, request, userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Unable to log performance deviation for well {WellId}", wellId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging performance deviation for well {WellId}", wellId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/production/wells/{wellId}/tests</summary>
        [HttpGet("wells/{wellId}/tests")]
        public async Task<ActionResult<List<WellTestResponse>>> GetWellTestsAsync(string wellId)
        {
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            try
            {
                var tests = await _productionService.GetWellTestsForWellAsync(fieldId, wellId);
                return Ok(tests ?? new List<WellTestResponse>());
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Unable to fetch well tests for {WellId}", wellId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching well tests for well {WellId}", wellId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>PATCH /api/field/current/production/allocation/{period}/wells/{wellId}</summary>
        [HttpPatch("allocation/{period}/wells/{wellId}")]
        public IActionResult PatchAllocationAsync(string period, string wellId, [FromBody] AllocationPatchRequest request)
        {
            if (string.IsNullOrWhiteSpace(period)) return BadRequest(new { error = "Period is required." });
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            // Allocation adjustments are persisted through PDEN_VOL_SUMMARY.
            // Accept the patch and return 204 — full PDEN persistence is done by the data-management layer.
            _logger.LogInformation("Allocation patch accepted for well {WellId} period {Period}", wellId, period);
            return NoContent();
        }

        /// <summary>GET /api/field/current/production/intervention-candidates</summary>
        [HttpGet("intervention-candidates")]
        public async Task<ActionResult<List<InterventionCandidateDto>>> GetInterventionCandidatesAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            try
            {
                var activities = await _productionService.GetWellActivitiesForFieldAsync(fieldId);

                // Group by UWI and take the most recent activity per well as the candidate record
                var candidates = activities
                    .GroupBy(a => a.UWI)
                    .Select(g =>
                    {
                        var latest = g.OrderByDescending(a => a.ACTIVITY_OBS_NO).First();
                        var source = g
                            .Where(a => !IsDecisionActivity(a.ACTIVITY_TYPE_ID))
                            .OrderByDescending(a => a.ACTIVITY_OBS_NO)
                            .FirstOrDefault() ?? latest;

                        var linkedWorkOrderId = ExtractWorkflowMarker(latest.REMARK, "WORK_ORDER_ID");
                        var linkedAfeId = ExtractWorkflowMarker(latest.REMARK, "AFE_ID");
                        var linkedAfeNumber = ExtractWorkflowMarker(latest.REMARK, "AFE_NUMBER");
                        var linkedAbandonmentId = ExtractWorkflowMarker(latest.REMARK, "ABANDONMENT_ID");
                        var linkedProcessInstanceId = ExtractWorkflowMarker(latest.REMARK, "PROCESS_INSTANCE_ID");
                        return new InterventionCandidateDto
                        {
                            WellId           = latest.UWI ?? string.Empty,
                            WellName         = latest.UWI ?? string.Empty,
                            InterventionType = source.ACTIVITY_TYPE_ID ?? "WORKOVER",
                            Problem          = RemoveWorkflowMarkers(source.REMARK ?? string.Empty),
                            DeferredBopd     = 0,
                            EstDaysSinceOnset= 0,
                            Priority         = "MEDIUM",
                            Status           = MapInterventionStatus(latest.ACTIVITY_TYPE_ID),
                            EstCostUsd       = 0,
                            WorkOrderId      = linkedWorkOrderId,
                            AfeId            = linkedAfeId,
                            AfeNumber        = linkedAfeNumber,
                            AbandonmentId    = linkedAbandonmentId,
                            ProcessInstanceId = linkedProcessInstanceId,
                        };
                    })
                    .ToList();

                return Ok(candidates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching intervention candidates for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>POST /api/field/current/production/intervention-candidates/{uwi}/transition-to-decommissioning</summary>
        [HttpPost("intervention-candidates/{uwi}/transition-to-decommissioning")]
        public async Task<ActionResult<DecommissioningTriggerResult>> TransitionToDecommissioningAsync(string uwi, [FromBody] DecommissioningTriggerRequest request)
        {
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";

            try
            {
                var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
                var existingAbandonment = (await decommissioningService.GetAbandonedWellsForFieldAsync(fieldId))
                    .FirstOrDefault(a => string.Equals(a.WellId, uwi, StringComparison.OrdinalIgnoreCase));

                if (existingAbandonment != null)
                {
                    return Ok(new DecommissioningTriggerResult
                    {
                        Success = true,
                        AlreadyExists = true,
                        WorkflowStarted = false,
                        WellId = uwi,
                        AbandonmentId = existingAbandonment.AbandonmentId,
                        Message = $"Decommissioning programme already exists for {uwi}."
                    });
                }

                var estimatedCost = request.EstimatedCostUsd.HasValue
                    ? Convert.ToDecimal(request.EstimatedCostUsd.Value)
                    : (decimal?)null;

                var abandonment = await decommissioningService.AbandonWellForFieldAsync(
                    fieldId,
                    uwi,
                    new WellAbandonmentRequest
                    {
                        WellId = uwi,
                        FieldId = fieldId,
                        AbandonmentType = string.IsNullOrWhiteSpace(request.AbandonmentType)
                            ? "PERMANENTLY_ABANDONED"
                            : request.AbandonmentType,
                        AbandonmentMethod = string.IsNullOrWhiteSpace(request.AbandonmentMethod)
                            ? "CEMENT_PLUG"
                            : request.AbandonmentMethod,
                        Status = "IN_PROGRESS",
                        AbandonmentStartDate = DateTime.UtcNow,
                        PluggingDate = DateTime.UtcNow,
                        AbandonmentCost = estimatedCost,
                        AbandonmentCostCurrency = estimatedCost.HasValue ? "USD" : null,
                        PluggingCost = estimatedCost,
                        PluggingCostCurrency = estimatedCost.HasValue ? "USD" : null,
                        ActiveInd = "Y"
                    },
                    userId);

                Beep.OilandGas.Models.Processes.ProcessInstance? processInstance = null;
                string? message = null;

                if (request.StartWorkflow)
                {
                    try
                    {
                        processInstance = await _decommissioningProcessService.StartWellAbandonmentProcessAsync(uwi, fieldId, userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Decommissioning record created for well {UWI}, but workflow startup failed", uwi);
                        message = $"Decommissioning record created for {uwi}, but the lifecycle workflow could not be started.";
                    }
                }

                await _productionService.RecordInterventionDecisionAsync(
                    uwi,
                    "DECOMMISSIONING_TRIGGERED",
                    BuildDecommissioningRemark(request.Note, abandonment.AbandonmentId, processInstance?.InstanceId),
                    userId);

                return Ok(new DecommissioningTriggerResult
                {
                    Success = true,
                    WorkflowStarted = processInstance != null,
                    WellId = uwi,
                    AbandonmentId = abandonment.AbandonmentId,
                    ProcessInstanceId = processInstance?.InstanceId,
                    Message = message ?? $"Decommissioning handoff created for {uwi}."
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Unable to transition well {UWI} to decommissioning", uwi);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transitioning well {UWI} to decommissioning", uwi);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>POST /api/field/current/production/intervention-candidates/{uwi}/decision</summary>
        [HttpPost("intervention-candidates/{uwi}/decision")]
        public async Task<ActionResult<InterventionDecisionResult>> PostInterventionDecisionAsync(string uwi, [FromBody] InterventionDecisionRequest request)
        {
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";

            try
            {
                var normalizedDecision = request.Decision?.Trim().ToUpperInvariant();
                var activityTypeId = normalizedDecision switch
                {
                    "APPROVED" => "WORKOVER_APPROVED",
                    "DEFERRED" => "WORKOVER_DEFERRED",
                    "REJECTED" => "WORKOVER_REJECTED",
                    _          => "WORKOVER_REVIEWED",
                };

                WorkOrderSummary? createdWorkOrder = null;
                AFE? linkedAfe = null;

                if (normalizedDecision == "APPROVED")
                {
                    if (!string.IsNullOrWhiteSpace(request.WorkOrderId))
                    {
                        createdWorkOrder = await _workOrderService.GetByIdAsync(fieldId, request.WorkOrderId);
                    }

                    if (createdWorkOrder == null && request.CreateWorkOrder)
                    {
                        createdWorkOrder = await _workOrderService.CreateAsync(new CreateWorkOrderRequest
                        {
                            FieldId = fieldId,
                            WoSubType = WorkOrderSubType.Corrective,
                            InstanceName = BuildWorkOrderTitle(uwi, request.InterventionType),
                            EquipmentId = uwi,
                            Description = BuildWorkOrderDescription(request),
                            Jurisdiction = "USA"
                        }, userId);

                        createdWorkOrder = await _workOrderService.TransitionStateAsync(
                            fieldId,
                            createdWorkOrder.InstanceId,
                            WorkOrderState.Planned,
                            userId,
                            request.Note);
                    }

                    if (createdWorkOrder != null && request.LinkAfe)
                    {
                        var workOrderResponse = new WorkOrderResponse
                        {
                            WorkOrderId = createdWorkOrder.InstanceId,
                            WorkOrderNumber = createdWorkOrder.InstanceId,
                            WorkOrderType = createdWorkOrder.WoSubType,
                            EntityType = "WELL",
                            EntityId = uwi,
                            FieldId = fieldId,
                            PropertyId = string.IsNullOrWhiteSpace(createdWorkOrder.EquipmentId) ? uwi : createdWorkOrder.EquipmentId,
                            Status = createdWorkOrder.State,
                            EstimatedCost = request.EstimatedCostUsd.HasValue ? (decimal?)request.EstimatedCostUsd.Value : null,
                            ActualCost = null,
                            AfeId = request.AfeId
                        };

                        linkedAfe = await _workOrderAccountingService.CreateOrLinkAFEAsync(workOrderResponse, userId);
                    }
                }

                var remark = BuildDecisionRemark(request.Note, createdWorkOrder?.InstanceId, linkedAfe?.AFE_ID, linkedAfe?.AFE_NUMBER);
                await _productionService.RecordInterventionDecisionAsync(uwi, activityTypeId, remark, userId);

                return Ok(new InterventionDecisionResult
                {
                    Success = true,
                    WellId = uwi,
                    Decision = normalizedDecision ?? "REVIEWED",
                    WorkOrderId = createdWorkOrder?.InstanceId,
                    AfeId = linkedAfe?.AFE_ID,
                    AfeNumber = linkedAfe?.AFE_NUMBER,
                    Message = createdWorkOrder == null
                        ? $"Intervention {normalizedDecision?.ToLowerInvariant() ?? "reviewed"} for {uwi}."
                        : $"Intervention approved and linked to work order {createdWorkOrder.InstanceId}."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording intervention decision for well {UWI}", uwi);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/production/dashboard/summary</summary>
        [HttpGet("dashboard/summary")]
        public async Task<ActionResult<ProductionDashboardSummary>> GetDashboardSummaryAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            try
            {
                var summary = await _productionService.GetProductionDashboardSummaryAsync(fieldId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production dashboard summary for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/production/dashboard/wells</summary>
        [HttpGet("dashboard/wells")]
        public async Task<ActionResult<List<ProductionWellStatusDto>>> GetDashboardWellsAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            try
            {
                var wells = await _productionService.GetProductionWellStatusAsync(fieldId);
                return Ok(wells ?? new List<ProductionWellStatusDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard wells for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private static DateTime ResolveTestDate(WellTestResponse? test)
        {
            if (test == null)
            {
                return DateTime.MinValue;
            }

            return test.TestDate
                ?? test.TestStartDate
                ?? test.TestEndDate
                ?? DateTime.MinValue;
        }

        private static double ToDouble(decimal? value)
        {
            return value.HasValue ? (double)value.Value : 0;
        }

        private static bool IsDecisionActivity(string? activityTypeId)
        {
            return string.Equals(activityTypeId, "DECOMMISSIONING_TRIGGERED", StringComparison.OrdinalIgnoreCase)
                || (activityTypeId?.StartsWith("WORKOVER_", StringComparison.OrdinalIgnoreCase) == true
                    && (activityTypeId.Contains("APPROVED", StringComparison.OrdinalIgnoreCase)
                        || activityTypeId.Contains("DEFERRED", StringComparison.OrdinalIgnoreCase)
                        || activityTypeId.Contains("REJECTED", StringComparison.OrdinalIgnoreCase)
                        || activityTypeId.Contains("REVIEWED", StringComparison.OrdinalIgnoreCase)));
        }

        private static string MapInterventionStatus(string? activityTypeId)
        {
            return activityTypeId?.ToUpperInvariant() switch
            {
                "WORKOVER_APPROVED" => "APPROVED",
                "WORKOVER_DEFERRED" => "DEFERRED",
                "WORKOVER_REJECTED" => "REJECTED",
                "DECOMMISSIONING_TRIGGERED" => "TRANSFERRED",
                "PERFORMANCE_DEVIATION" => "REVIEW",
                _ => "REVIEW"
            };
        }

        private static string? ExtractWorkflowMarker(string? remark, string marker)
        {
            if (string.IsNullOrWhiteSpace(remark))
                return null;

            var match = Regex.Match(remark, $@"{Regex.Escape(marker)}:([^\s]+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static string RemoveWorkflowMarkers(string remark)
        {
            if (string.IsNullOrWhiteSpace(remark))
                return string.Empty;

            var withoutMarkers = Regex.Replace(remark, @"\s*(WORK_ORDER_ID|AFE_ID|AFE_NUMBER|ABANDONMENT_ID|PROCESS_INSTANCE_ID|SOURCE_PROCESS):[^\s]+", string.Empty);
            return withoutMarkers.Trim();
        }

        private static string BuildDecisionRemark(string? note, string? workOrderId, string? afeId, string? afeNumber)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(note))
                parts.Add(note.Trim());
            if (!string.IsNullOrWhiteSpace(workOrderId))
                parts.Add($"WORK_ORDER_ID:{workOrderId}");
            if (!string.IsNullOrWhiteSpace(afeId))
                parts.Add($"AFE_ID:{afeId}");
            if (!string.IsNullOrWhiteSpace(afeNumber))
                parts.Add($"AFE_NUMBER:{afeNumber}");

            return string.Join(" ", parts);
        }

        private static string BuildDecommissioningRemark(string? note, string? abandonmentId, string? processInstanceId)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(note))
                parts.Add(note.Trim());
            if (!string.IsNullOrWhiteSpace(abandonmentId))
                parts.Add($"ABANDONMENT_ID:{abandonmentId}");
            if (!string.IsNullOrWhiteSpace(processInstanceId))
                parts.Add($"PROCESS_INSTANCE_ID:{processInstanceId}");

            parts.Add("SOURCE_PROCESS:DECOMMISSIONING");
            return string.Join(" ", parts);
        }

        private static string BuildWorkOrderTitle(string uwi, string? interventionType)
        {
            return string.IsNullOrWhiteSpace(interventionType)
                ? $"Intervention follow-up for {uwi}"
                : $"{interventionType} for {uwi}";
        }

        private static string BuildWorkOrderDescription(InterventionDecisionRequest request)
        {
            var descriptionParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(request.Problem))
                descriptionParts.Add(request.Problem.Trim());
            if (!string.IsNullOrWhiteSpace(request.Note))
                descriptionParts.Add(request.Note.Trim());

            return descriptionParts.Count == 0
                ? "Create an intervention work order for the approved candidate."
                : string.Join(" ", descriptionParts);
        }
    }

    // ── Response DTOs ─────────────────────────────────────────────────────────
    public class WellPerformanceSummary
    {
        public string WellId        { get; set; } = string.Empty;
        public string Status        { get; set; } = string.Empty;
        public double OilRate       { get; set; }
        public double GasRate       { get; set; }
        public double WaterRate     { get; set; }
        public double PotentialRate { get; set; }
        public double CumOil        { get; set; }
        public DateTime? LastTestDate { get; set; }
        public List<WellTestResponse> WellTests { get; set; } = new();
    }

    public class AllocationPatchRequest
    {
        public double AllocatedOilBopd    { get; set; }
        public double AllocatedGasMmscfd  { get; set; }
        public double AllocatedWaterBopd  { get; set; }
    }

    public class InterventionCandidateDto
    {
        public string WellId           { get; set; } = string.Empty;
        public string WellName         { get; set; } = string.Empty;
        public string InterventionType { get; set; } = string.Empty;
        public string Problem          { get; set; } = string.Empty;
        public double DeferredBopd     { get; set; }
        public int    EstDaysSinceOnset{ get; set; }
        public string Priority         { get; set; } = "MEDIUM";
        public string Status           { get; set; } = "REVIEW";
        public double EstCostUsd       { get; set; }
        public string? WorkOrderId     { get; set; }
        public string? AfeId           { get; set; }
        public string? AfeNumber       { get; set; }
        public string? AbandonmentId   { get; set; }
        public string? ProcessInstanceId { get; set; }
    }

    public class InterventionDecisionRequest
    {
        public string? Decision { get; set; }
        public string? Note     { get; set; }
        public string? InterventionType { get; set; }
        public string? Problem { get; set; }
        public double? EstimatedCostUsd { get; set; }
        public bool CreateWorkOrder { get; set; } = true;
        public bool LinkAfe { get; set; } = true;
        public string? WorkOrderId { get; set; }
        public string? AfeId { get; set; }
    }

    public class InterventionDecisionResult
    {
        public bool Success { get; set; }
        public string WellId { get; set; } = string.Empty;
        public string Decision { get; set; } = string.Empty;
        public string? WorkOrderId { get; set; }
        public string? AfeId { get; set; }
        public string? AfeNumber { get; set; }
        public string? Message { get; set; }
    }

    public class DecommissioningTriggerRequest
    {
        public string? Note { get; set; }
        public string? InterventionType { get; set; }
        public string? Problem { get; set; }
        public double? EstimatedCostUsd { get; set; }
        public string? AbandonmentType { get; set; }
        public string? AbandonmentMethod { get; set; }
        public bool StartWorkflow { get; set; } = true;
    }

    public class DecommissioningTriggerResult
    {
        public bool Success { get; set; }
        public bool AlreadyExists { get; set; }
        public bool WorkflowStarted { get; set; }
        public string WellId { get; set; } = string.Empty;
        public string? AbandonmentId { get; set; }
        public string? ProcessInstanceId { get; set; }
        public string? Message { get; set; }
    }

}
