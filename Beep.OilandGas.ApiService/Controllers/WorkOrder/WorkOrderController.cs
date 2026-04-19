using System.Security.Claims;
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
    private readonly IFieldOrchestrator           _fieldOrchestrator;
    private readonly ILogger<WorkOrderController> _logger;

    public WorkOrderController(
        IWorkOrderService workOrders,
        ISchedulingService scheduling,
        IContractorManagementService contractors,
        ICostCaptureService costs,
        IInspectionService inspection,
        IFieldOrchestrator fieldOrchestrator,
        ILogger<WorkOrderController> logger)
    {
        _workOrders        = workOrders;
        _scheduling        = scheduling;
        _contractors       = contractors;
        _costs             = costs;
        _inspection        = inspection;
        _fieldOrchestrator = fieldOrchestrator;
        _logger            = logger;
    }

    // ── WORK ORDER CRUD ───────────────────────────────────────────────────────

    /// <summary>List work orders for the current field. Filter by state and/or sub-type.</summary>
    [HttpGet]
    public async Task<ActionResult<List<WorkOrderSummary>>> GetListAsync(
        [FromQuery] string? state = null, [FromQuery] string? woSubType = null)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest("No active field selected.");
        var result = await _workOrders.GetByFieldAsync(fieldId, state, woSubType);
        return Ok(result);
    }

    /// <summary>Get full detail for a single work order.</summary>
    [HttpGet("{instanceId}")]
    public async Task<ActionResult<WorkOrderDetailModel>> GetDetailAsync(string instanceId)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest("No active field selected.");
        var detail = await _workOrders.GetByIdAsync(fieldId, instanceId);
        if (detail is null) return NotFound();
        return Ok(detail);
    }

    /// <summary>Create a new work order.</summary>
    [HttpPost]
    public async Task<ActionResult<WorkOrderSummary>> CreateAsync(
        [FromBody] CreateWorkOrderRequest request)
    {
        var userId  = UserId();
        var fieldId = _fieldOrchestrator.CurrentFieldId;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest("No active field selected.");

        request.FieldId = fieldId;
        var result = await _workOrders.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetDetailAsync), new { instanceId = result.InstanceId }, result);
    }

    /// <summary>Transition a work order state (e.g. DRAFT → PLANNED).</summary>
    [HttpPost("{instanceId}/transition")]
    public async Task<ActionResult<WorkOrderSummary>> TransitionAsync(
        string instanceId, [FromBody] TransitionWorkOrderRequest request)
    {
        var userId  = UserId();
        var fieldId = _fieldOrchestrator.CurrentFieldId;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest("No active field selected.");

        try
        {
            var result = await _workOrders.TransitionStateAsync(
                fieldId, instanceId, request.ToState, userId, request.Notes);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    /// <summary>Returns valid next states for a work order.</summary>
    [HttpGet("{instanceId}/transitions")]
    public async Task<ActionResult<List<string>>> GetTransitionsAsync(string instanceId)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest("No active field selected.");
        var detail = await _workOrders.GetByIdAsync(fieldId, instanceId);
        if (detail is null) return NotFound();
        var transitions = _workOrders.GetAvailableTransitions(detail.State, detail.WoSubType);
        return Ok(transitions);
    }

    [HttpDelete("{instanceId}")]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    public async Task<IActionResult> DeleteAsync(string instanceId)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest("No active field selected.");
        await _workOrders.DeleteAsync(fieldId, instanceId, UserId());
        return NoContent();
    }

    // ── SCHEDULING ────────────────────────────────────────────────────────────

    [HttpPost("{instanceId}/schedule")]
    public async Task<ActionResult<ScheduleResult>> ScheduleAsync(
        string instanceId, [FromBody] ScheduleWorkOrderRequest request)
    {
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
        var result = await _scheduling.RescheduleAsync(instanceId, newStart, UserId());
        return Ok(result);
    }

    /// <summary>Calendar view for the current field (all planned WOs in range).</summary>
    [HttpGet("calendar")]
    public async Task<ActionResult<List<CalendarSlot>>> GetCalendarAsync(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to   = null)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest("No active field selected.");
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
        var conflicts = await _scheduling.GetConflictsAsync(equipmentId, from, to, instanceId);
        return Ok(conflicts);
    }

    // ── CONTRACTORS ───────────────────────────────────────────────────────────

    [HttpGet("{instanceId}/contractors")]
    public async Task<ActionResult<List<ContractorAssignment>>> GetContractorsAsync(string instanceId)
    {
        var result = await _contractors.GetAssignmentsAsync(instanceId);
        return Ok(result);
    }

    [HttpPost("{instanceId}/contractors")]
    public async Task<ActionResult<ContractorAssignment>> AssignContractorAsync(
        string instanceId, [FromBody] AssignContractorRequest request)
    {
        var result = await _contractors.AssignContractorAsync(
            instanceId, request.StepId, request.BaId, request.RoleCode, UserId());
        return Ok(result);
    }

    [HttpDelete("{instanceId}/contractors/{baId}")]
    public async Task<IActionResult> RemoveContractorAsync(string instanceId, string baId,
        [FromQuery] string stepId)
    {
        await _contractors.RemoveContractorAsync(instanceId, stepId, baId, UserId());
        return NoContent();
    }

    [HttpGet("contractor/{baId}/validate")]
    public async Task<ActionResult<ContractorQualificationResult>> ValidateContractorAsync(
        string baId,
        [FromQuery] string woType,
        [FromQuery] string jurisdiction = "USA")
    {
        var result = await _contractors.ValidateContractorAsync(baId, woType, jurisdiction);
        return Ok(result);
    }

    // ── COST CAPTURE ──────────────────────────────────────────────────────────

    [HttpGet("{instanceId}/costs")]
    public async Task<ActionResult<List<CostVarianceLine>>> GetCostsAsync(string instanceId)
    {
        var result = await _costs.GetVarianceSummaryAsync(instanceId);
        return Ok(result);
    }

    [HttpPost("{instanceId}/costs/afe")]
    public async Task<IActionResult> UpsertAFEAsync(
        string instanceId, [FromBody] UpsertAFERequest request)
    {
        await _costs.UpsertAFEAsync(instanceId, request.BudgetAmount, UserId());
        return NoContent();
    }

    [HttpPost("{instanceId}/costs/line")]
    public async Task<IActionResult> AddCostLineAsync(
        string instanceId, [FromBody] AddCostLineRequest request)
    {
        await _costs.AddCostLineAsync(
            instanceId, request.CompCode, request.BudgetAmt, request.Description, UserId());
        return NoContent();
    }

    // ── INSPECTION ────────────────────────────────────────────────────────────

    [HttpGet("{instanceId}/checklist")]
    public async Task<ActionResult<List<InspectionCondition>>> GetChecklistAsync(string instanceId)
    {
        var result = await _inspection.GetChecklistAsync(instanceId);
        return Ok(result);
    }

    [HttpPost("{instanceId}/checklist/{condSeq}")]
    public async Task<IActionResult> RecordResultAsync(
        string instanceId, int condSeq,
        [FromBody] RecordInspectionResultRequest request)
    {
        await _inspection.RecordResultAsync(instanceId, condSeq, request.Result, request.Notes, UserId());
        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string UserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
}
