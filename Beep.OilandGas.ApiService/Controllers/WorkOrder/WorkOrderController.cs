using System.Security.Claims;
using Beep.OilandGas.LifeCycle.Services.Accounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WorkOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.WorkOrder;

[ApiController]
[Route("api/field/current/workorder")]
[Authorize]
public class WorkOrderController : ControllerBase
{
    private readonly IWorkOrderService            _workOrders;
    private readonly ISchedulingService           _scheduling;
    private readonly IContractorManagementService _contractors;
    private readonly ICostCaptureService          _costs;
    private readonly IInspectionService           _inspection;
    private readonly WorkOrderAccountingService   _accounting;
    private readonly IFieldOrchestrator           _fieldOrchestrator;
    private readonly ILogger<WorkOrderController> _logger;

    public WorkOrderController(
        IWorkOrderService workOrders,
        ISchedulingService scheduling,
        IContractorManagementService contractors,
        ICostCaptureService costs,
        IInspectionService inspection,
        WorkOrderAccountingService accounting,
        IFieldOrchestrator fieldOrchestrator,
        ILogger<WorkOrderController> logger)
    {
        _workOrders        = workOrders;
        _scheduling        = scheduling;
        _contractors       = contractors;
        _costs             = costs;
        _inspection        = inspection;
        _accounting        = accounting;
        _fieldOrchestrator = fieldOrchestrator;
        _logger            = logger;
    }

    // ── WORK ORDER CRUD ───────────────────────────────────────────────────────

    /// <summary>List work orders for the current field. Filter by state and/or sub-type.</summary>
    [HttpGet]
    public async Task<ActionResult<List<WorkOrderSummary>>> GetListAsync(
        [FromQuery] string? state = null, [FromQuery] string? woSubType = null)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "No active field selected." });
        var result = await _workOrders.GetByFieldAsync(fieldId, state, woSubType);
        return Ok(result);
    }

    /// <summary>Get full detail for a single work order.</summary>
    [HttpGet("{instanceId}")]
    public async Task<ActionResult<WorkOrderDetailModel>> GetDetailAsync(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "No active field selected." });
        var detail = await _workOrders.GetByIdAsync(fieldId, instanceId);
        if (detail is null) return NotFound(new { error = $"Work order {instanceId} not found." });
        return Ok(detail);
    }

    /// <summary>Create a new work order.</summary>
    [HttpPost]
    public async Task<ActionResult<WorkOrderSummary>> CreateAsync(
        [FromBody] CreateWorkOrderRequest request)
    {
        var userId  = UserId();
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "No active field selected." });

        request.FieldId = fieldId;
        var result = await _workOrders.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetDetailAsync), new { instanceId = result.InstanceId }, result);
    }

    /// <summary>Transition a work order state (e.g. DRAFT → PLANNED).</summary>
    [HttpPost("{instanceId}/transition")]
    public async Task<ActionResult<WorkOrderSummary>> TransitionAsync(
        string instanceId, [FromBody] TransitionWorkOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var userId = UserId();
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "No active field selected." });

        try
        {
            var result = await _workOrders.TransitionStateAsync(
                fieldId, instanceId, request.ToState, userId, request.Notes);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = "An internal error occurred." });
        }
    }

    /// <summary>Returns valid next states for a work order.</summary>
    [HttpGet("{instanceId}/transitions")]
    public async Task<ActionResult<List<string>>> GetTransitionsAsync(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "No active field selected." });
        var detail = await _workOrders.GetByIdAsync(fieldId, instanceId);
        if (detail is null) return NotFound(new { error = $"Work order {instanceId} not found." });
        var transitions = _workOrders.GetAvailableTransitions(detail.State, detail.WoSubType);
        return Ok(transitions);
    }

    [HttpDelete("{instanceId}")]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    public async Task<IActionResult> DeleteAsync(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "No active field selected." });
        await _workOrders.DeleteAsync(fieldId, instanceId, UserId());
        return NoContent();
    }

    // ── SCHEDULING ────────────────────────────────────────────────────────────

    [HttpPost("{instanceId}/schedule")]
    public async Task<ActionResult<ScheduleResult>> ScheduleAsync(
        string instanceId, [FromBody] ScheduleWorkOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var result = await _scheduling.ScheduleWorkOrderAsync(
            instanceId,
            request.EquipmentId,
            request.ProposedStart,
            TimeSpan.FromHours(request.DurationHours),
            UserId());
        return Ok(result);
    }

    [HttpPost("{instanceId}/reschedule")]
    public async Task<ActionResult<ScheduleResult>> RescheduleAsync(
        string instanceId, [FromBody] DateTime newStart)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var result = await _scheduling.RescheduleAsync(instanceId, newStart, UserId());
        return Ok(result);
    }

    /// <summary>Calendar view for the current field (all planned WOs in range).</summary>
    [HttpGet("calendar")]
    public async Task<ActionResult<List<CalendarSlot>>> GetCalendarAsync(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to   = null)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "No active field selected." });
        var start  = from ?? DateTime.UtcNow.AddDays(-7);
        var end    = to   ?? DateTime.UtcNow.AddDays(30);
        var result = await _scheduling.GetFieldCalendarAsync(fieldId, start, end);
        return Ok(result);
    }

    [HttpGet("{instanceId}/conflicts")]
    public async Task<ActionResult<List<ScheduleConflict>>> GetConflictsAsync(
        string instanceId,
        [FromQuery] string equipmentId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
            if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var conflicts = await _scheduling.GetConflictsAsync(equipmentId, from, to, instanceId);
        return Ok(conflicts);
    }

    // ── CONTRACTORS ───────────────────────────────────────────────────────────

    [HttpGet("{instanceId}/contractors")]
    public async Task<ActionResult<List<ContractorAssignment>>> GetContractorsAsync(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var result = await _contractors.GetAssignmentsAsync(instanceId);
        return Ok(result);
    }

    [HttpPost("{instanceId}/contractors")]
    public async Task<ActionResult<ContractorAssignment>> AssignContractorAsync(
        string instanceId, [FromBody] AssignContractorRequest request)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var result = await _contractors.AssignContractorAsync(
            instanceId, request.StepId, request.BaId, request.RoleCode, UserId());
        return Ok(result);
    }

    [HttpDelete("{instanceId}/contractors/{baId}")]
    public async Task<IActionResult> RemoveContractorAsync(string instanceId, string baId,
        [FromQuery] string stepId)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
            if (string.IsNullOrWhiteSpace(baId)) return BadRequest(new { error = "Business associate ID is required." });
        await _contractors.RemoveContractorAsync(instanceId, stepId, baId, UserId());
        return NoContent();
    }

    [HttpGet("contractor/{baId}/validate")]
    public async Task<ActionResult<ContractorQualificationResult>> ValidateContractorAsync(
        string baId,
        [FromQuery] string woType,
        [FromQuery] string jurisdiction = "USA")
    {
            if (string.IsNullOrWhiteSpace(baId)) return BadRequest(new { error = "Business associate ID is required." });
        var result = await _contractors.ValidateContractorAsync(baId, woType, jurisdiction);
        return Ok(result);
    }

    // ── COST CAPTURE ──────────────────────────────────────────────────────────

    [HttpGet("{instanceId}/costs")]
    public async Task<ActionResult<List<CostVarianceLine>>> GetCostsAsync(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var result = await _costs.GetVarianceSummaryAsync(instanceId);
        return Ok(result);
    }

    [HttpGet("{instanceId}/afe")]
    public async Task<ActionResult<object>> GetLinkedAfeAsync(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });

        var afe = await _accounting.GetAFEForWorkOrderAsync(instanceId);
        if (afe == null)
            return NotFound(new { error = $"No AFE linked to work order {instanceId}." });

        return Ok(new
        {
            AfeId = afe.AFE_ID,
            AfeNumber = afe.AFE_NUMBER,
            AfeName = afe.AFE_NAME,
            EstimatedCost = afe.ESTIMATED_COST,
            ActualCost = afe.ACTUAL_COST,
            Status = afe.STATUS,
            Description = afe.DESCRIPTION,
            WorkOrderId = instanceId
        });
    }

    [HttpPost("{instanceId}/afe")]
    public async Task<ActionResult<object>> CreateOrLinkAfeAsync(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });

        try
        {
            var afe = await _accounting.CreateOrLinkAFEAsync(instanceId, UserId());
            return Ok(new
            {
                AfeId = afe.AFE_ID,
                AfeNumber = afe.AFE_NUMBER,
                AfeName = afe.AFE_NAME,
                EstimatedCost = afe.ESTIMATED_COST,
                ActualCost = afe.ACTUAL_COST,
                Status = afe.STATUS,
                Description = afe.DESCRIPTION,
                WorkOrderId = instanceId
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot create or link AFE for work order {WorkOrderId}", instanceId);
            return BadRequest(new { error = "Unable to create or link an AFE for this work order." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating or linking AFE for work order {WorkOrderId}", instanceId);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    [HttpPost("{instanceId}/costs/afe")]
    public async Task<IActionResult> UpsertAFEAsync(
        string instanceId, [FromBody] UpsertAFERequest request)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        await _costs.UpsertAFEAsync(instanceId, request.BudgetAmount, UserId());
        return NoContent();
    }

    [HttpPost("{instanceId}/costs/line")]
    public async Task<IActionResult> AddCostLineAsync(
        string instanceId, [FromBody] AddCostLineRequest request)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        await _costs.AddCostLineAsync(
            instanceId, request.CompCode, request.BudgetAmt, request.Description, UserId());
        return NoContent();
    }

    // ── INSPECTION ────────────────────────────────────────────────────────────

    [HttpGet("{instanceId}/checklist")]
    public async Task<ActionResult<List<InspectionCondition>>> GetChecklistAsync(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var result = await _inspection.GetChecklistAsync(instanceId);
        return Ok(result);
    }

    [HttpPost("{instanceId}/checklist/{condSeq}")]
    public async Task<IActionResult> RecordResultAsync(
        string instanceId, int condSeq,
        [FromBody] RecordInspectionResultRequest request)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        await _inspection.RecordResultAsync(instanceId, condSeq, request.Result, request.Notes, UserId());
        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string UserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
}
