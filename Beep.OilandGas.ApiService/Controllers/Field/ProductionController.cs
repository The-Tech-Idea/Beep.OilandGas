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
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Microsoft.Extensions.Logging;
using TheTechIdeaWeb.Diagnostics;

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
        private readonly WellServices _wells;
        private readonly ILogger<ProductionController> _logger;
        private readonly IFailureReporter _failures;

        public ProductionController(
            IFieldOrchestrator fieldOrchestrator,
            Beep.OilandGas.LifeCycle.Services.Production.PPDMProductionService productionService,
            IWorkOrderService workOrderService,
            WorkOrderAccountingService workOrderAccountingService,
            DecommissioningProcessService decommissioningProcessService,
            WellServices wells,
            ILogger<ProductionController> logger,
            IFailureReporter failures)
        {
            _failures = failures ?? throw new ArgumentNullException(nameof(failures));
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _productionService = productionService ?? throw new ArgumentNullException(nameof(productionService));
            _workOrderService = workOrderService ?? throw new ArgumentNullException(nameof(workOrderService));
            _workOrderAccountingService = workOrderAccountingService ?? throw new ArgumentNullException(nameof(workOrderAccountingService));
            _decommissioningProcessService = decommissioningProcessService ?? throw new ArgumentNullException(nameof(decommissioningProcessService));
            _wells = wells ?? throw new ArgumentNullException(nameof(wells));
            _logger = logger;
        }

        /// <summary>GET /api/field/current/production/wells/{wellId}/performance</summary>
        [HttpGet("wells/{wellId}/performance")]
        public async Task<ActionResult<WellPerformanceSummary>> GetWellPerformanceAsync(string wellId)
        {
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
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

        /// <summary>GET /api/field/current/production/wells/{wellId}/analysis</summary>
        [HttpGet("wells/{wellId}/analysis")]
        public async Task<ActionResult<WellPerformanceAnalysisResponse>> GetWellPerformanceAnalysisAsync(string wellId)
        {
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            var analysis = await _productionService.GetWellPerformanceAnalysisAsync(fieldId, wellId);
            return Ok(analysis);
        }

        /// <summary>POST /api/field/current/production/wells/{wellId}/analysis/deviation</summary>
        [HttpPost("wells/{wellId}/analysis/deviation")]
        public async Task<ActionResult<PerformanceDeviationResult>> LogPerformanceDeviationAsync(string wellId, [FromBody] PerformanceDeviationRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            var result = await _productionService.LogWellPerformanceDeviationAsync(fieldId, wellId, request, userId);
            return Ok(result);
        }

        /// <summary>GET /api/field/current/production/wells/{wellId}/tests</summary>
        [HttpGet("wells/{wellId}/tests")]
        public async Task<ActionResult<List<WellTestResponse>>> GetWellTestsAsync(string wellId)
        {
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            var tests = await _productionService.GetWellTestsForWellAsync(fieldId, wellId);
            return Ok(tests ?? new List<WellTestResponse>());
        }

        /// <summary>PATCH /api/field/current/production/allocation/{period}/wells/{wellId}</summary>
        [HttpPatch("allocation/{period}/wells/{wellId}")]
        public IActionResult PatchAllocationAsync(string period, string wellId, [FromBody] AllocationPatchRequest request)
        {
            if (string.IsNullOrWhiteSpace(period)) return BadRequest(new { error = "Period is required." });
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            // Not yet stored: nothing in the data-management layer writes a well's allocated volumes for a period (PPDM's
            // PDEN_VOL_SUMMARY), so this answers that it is not available. It answered 204 and logged "accepted", and every
            // adjustment made on the allocation workbench was thrown away while the page believed it saved (2026-10-10).
            return Problem(
                statusCode: StatusCodes.Status501NotImplemented,
                title: "Not available",
                detail: "Saving a well's allocation adjustment is not available yet.");
        }

        /// <summary>GET /api/field/current/production/intervention-candidates</summary>
        [HttpGet("intervention-candidates")]
        public async Task<ActionResult<List<InterventionCandidateDto>>> GetInterventionCandidatesAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            var activities = await _productionService.GetWellActivitiesForFieldAsync(fieldId);
            // A well's name is its WELL row's; the candidate had shown its UWI as its name.
            var wellNames = (await _wells.GetWellsByFieldAsync(fieldId))
                .Where(w => !string.IsNullOrWhiteSpace(w.UWI))
                .GroupBy(w => w.UWI, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(w => w.WELL_NAME?.Trim()).FirstOrDefault(name => !string.IsNullOrEmpty(name)),
                    StringComparer.OrdinalIgnoreCase);

            // Group by UWI and take the most recent activity per well as the candidate record
            var candidates = new List<InterventionCandidateDto>();
            foreach (var g in activities.Where(a => !string.IsNullOrWhiteSpace(a.UWI)).GroupBy(a => a.UWI, StringComparer.OrdinalIgnoreCase))
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
                candidates.Add(new InterventionCandidateDto
                {
                    WellId           = g.Key,
                    WellName         = wellNames.GetValueOrDefault(g.Key),
                    InterventionType = source.ACTIVITY_TYPE_ID ?? "WORKOVER",
                    Problem          = RemoveWorkflowMarkers(source.REMARK ?? string.Empty),
                    DeferredBopd     = await DeferredOilRateAsync(fieldId, g.Key),
                    // Not yet recorded: nothing stores when a well's problem began, what an intervention would cost or how
                    // urgent it is, so these stay empty (they were 0 days, $0 and "MEDIUM" for every candidate).
                    EstDaysSinceOnset = null,
                    Priority         = null,
                    EstCostUsd       = null,
                    Status           = MapInterventionStatus(latest.ACTIVITY_TYPE_ID),
                    WorkOrderId      = linkedWorkOrderId,
                    AfeId            = linkedAfeId,
                    AfeNumber        = linkedAfeNumber,
                    AbandonmentId    = linkedAbandonmentId,
                    ProcessInstanceId = linkedProcessInstanceId,
                });
            }

            return Ok(candidates);
        }

        /// <summary>POST /api/field/current/production/intervention-candidates/{uwi}/transition-to-decommissioning</summary>
        [HttpPost("intervention-candidates/{uwi}/transition-to-decommissioning")]
        public async Task<ActionResult<DecommissioningTriggerResult>> TransitionToDecommissioningAsync(string uwi, [FromBody] DecommissioningTriggerRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

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
                // The abandonment record above is already saved; whatever stops the workflow starting, the answer says the
                // record exists and the workflow did not start, with the reference its failure is filed under.
                // Cancellation is the request ending.
                catch (Exception workflowFailure) when (workflowFailure is not OperationCanceledException)
                {
                    var reference = _failures.ReportHandled(workflowFailure, $"starting the well abandonment workflow for {uwi}",
                        "the decommissioning record is saved without its workflow; the answer says so with this reference");
                    message = $"Decommissioning record created for {uwi}, but the lifecycle workflow could not be started (reference {reference}).";
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

        /// <summary>POST /api/field/current/production/intervention-candidates/{uwi}/decision</summary>
        [HttpPost("intervention-candidates/{uwi}/decision")]
        public async Task<ActionResult<InterventionDecisionResult>> PostInterventionDecisionAsync(string uwi, [FromBody] InterventionDecisionRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

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

        /// <summary>GET /api/field/current/production/dashboard/wells</summary>
        [HttpGet("dashboard/wells")]
        public async Task<ActionResult<List<ProductionWellStatusDto>>> GetDashboardWellsAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            var wells = await _productionService.GetProductionWellStatusAsync(fieldId);
            return Ok(wells ?? new List<ProductionWellStatusDto>());
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

        /// <summary>
        /// The oil rate a well is deferring, from its tests, by the rule its performance summary states
        /// (<see cref="GetWellPerformanceAsync"/>): its potential is its best tested oil rate, and the deferral is how far
        /// its latest tested oil rate is below that. Only tests that record an oil rate count; null when none does.
        /// </summary>
        private async Task<double?> DeferredOilRateAsync(string fieldId, string uwi)
        {
            var rated = (await _productionService.GetWellTestsForWellAsync(fieldId, uwi) ?? new List<WellTestResponse>())
                .Where(t => t.OilFlowAmount.HasValue)
                .ToList();
            if (rated.Count == 0)
            {
                return null;
            }

            var latest = rated.OrderByDescending(ResolveTestDate).First().OilFlowAmount!.Value;
            var potential = rated.Max(t => t.OilFlowAmount!.Value);
            return (double)Math.Max(0m, potential - latest);
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

    /// <summary>A well with intervention activity. A null figure is one nothing records.</summary>
    public class InterventionCandidateDto
    {
        public string WellId           { get; set; } = string.Empty;
        /// <summary>The well's name from its WELL row; null when the row records none.</summary>
        public string? WellName        { get; set; }
        public string InterventionType { get; set; } = string.Empty;
        public string Problem          { get; set; } = string.Empty;
        /// <summary>Best tested oil rate less the latest tested oil rate; null when no test records an oil rate.</summary>
        public double? DeferredBopd    { get; set; }
        public int?   EstDaysSinceOnset{ get; set; }
        public string? Priority        { get; set; }
        public string Status           { get; set; } = "REVIEW";
        public double? EstCostUsd      { get; set; }
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
