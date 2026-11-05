using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.WorkOrder;

[ApiController]
[Route("api/field/current/workorder")]
[Authorize]
[RequireCurrentFieldAccess]
public class WorkOrderController : ControllerBase
{
    private readonly IAccessControlService _access;
    private readonly IProcessService _processes;
    private readonly CompanyModuleConnection _company;
    private readonly IWorkOrderService            _workOrders;
    private readonly ISchedulingService           _scheduling;
    private readonly IContractorManagementService _contractors;
    private readonly IInspectionService           _inspection;
    private readonly IFieldOrchestrator           _fieldOrchestrator;
    private readonly ILogger<WorkOrderController> _logger;

    public WorkOrderController(
        IWorkOrderService workOrders,
        ISchedulingService scheduling,
        IContractorManagementService contractors,
        IInspectionService inspection,
        IFieldOrchestrator fieldOrchestrator,
        ILogger<WorkOrderController> logger,
        IAccessControlService access,
        CompanyModuleConnection company,
        IProcessService processes)
    {
        _access = access;
        _company = company;
        _processes = processes;
        _workOrders        = workOrders;
        _scheduling        = scheduling;
        _contractors       = contractors;
        _inspection        = inspection;
        _fieldOrchestrator = fieldOrchestrator;
        _logger            = logger;
    }

    // ── WORK ORDER CRUD ───────────────────────────────────────────────────────

    /// <summary>List work orders for the current field. Filter by state and/or sub-type.</summary>
    [HttpGet]
    public async Task<ActionResult<List<WorkOrderSummary>>> GetListAsync(
        [FromQuery] string? state = null, [FromQuery] string? woSubType = null)
        => await ReadListAsync(field => _workOrders.GetByFieldAsync(field, state, woSubType));

    /// <summary>Return a bounded sample, not the complete FIELD work-order population.</summary>
    [HttpGet("sample")]
    public async Task<ActionResult<List<WorkOrderSummary>>> GetSampleAsync()
        => await ReadListAsync(field => _workOrders.GetByFieldSampleAsync(field, 20, HttpContext.RequestAborted));

    private async Task<ActionResult<List<WorkOrderSummary>>> ReadListAsync(Func<string, Task<List<WorkOrderSummary>>> read)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "No active field selected." });
        var actor = User.ActingUserId(); _company.RequireActor(actor);
        var database = await _company.ResolveAsync(token: HttpContext.RequestAborted);
        var scope = _company.Company.Scope;
        async Task Guard()
        {
            if (_fieldOrchestrator.CurrentFieldId != fieldId || _company.Company.Scope != scope ||
                await _company.ResolveAsync(token: HttpContext.RequestAborted) != database)
                throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Conflict("The selected FIELD or current access changed. Reload the worklist.");
            _company.RequireActor(actor);
            var fieldAccess = await _access.CheckAssetAccessAsync(actor, fieldId, "FIELD", "READ");
            if (fieldAccess?.HasAccess != true || _fieldOrchestrator.CurrentFieldId != fieldId || _company.Company.Scope != scope)
                throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Conflict("The selected FIELD or current access changed. Reload the worklist.");
        }
        async Task<bool> CanRead(string id)
        {
            var access = await _access.CheckAssetAccessAsync(actor, id, "WORK_ORDER", "READ");
            return access?.HasAccess ?? throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Conflict("Work-order access could not be confirmed.");
        }
        var result = await CompanyWorkOrderListReview.ReadAsync(fieldId, () => read(fieldId), CanRead, Guard, HttpContext.RequestAborted);
        return Ok(result);
    }

    /// <summary>Get full detail for a single work order.</summary>
    [HttpGet("{instanceId}")]
    public async Task<ActionResult<WorkOrderDetailModel>> GetDetailAsync(string instanceId)
    {
        await RequireOrderSourceAsync(instanceId, "READ",requireState:false);
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "No active field selected." });
        var detail = await _workOrders.GetByIdAsync(fieldId, instanceId);
        if (detail is null) return NotFound(new { error = $"Work order {instanceId} not found." });
        await RequireOrderSourceAsync(instanceId, "READ",requireState:false);
        return Ok(detail);
    }

    /// <summary>Create a new work order.</summary>
    [HttpPost]
    [RequireCurrentFieldAccess("WRITE")]
    public async Task<ActionResult<WorkOrderSummary>> CreateAsync(
        [FromBody] CreateWorkOrderRequest request)
    {
        var userId = User.ActingUserId();
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "No active field selected." });

        if (string.IsNullOrWhiteSpace(request.FieldId) || request.FieldId != fieldId)
            return Conflict(new { error = "The submitted FIELD differs from the current selection. Review the order in its original FIELD." });
        if (string.IsNullOrWhiteSpace(request.WorkOrderNumber) || string.IsNullOrWhiteSpace(request.InstanceName) || string.IsNullOrWhiteSpace(request.Jurisdiction))
            return BadRequest(new { error = "Recorded order number, title and jurisdiction are required." });
        var result = await _workOrders.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetDetailAsync), new { instanceId = result.InstanceId }, result);
    }

    /// <summary>Transition a work order state (e.g. DRAFT → PLANNED).</summary>
    [HttpPost("{instanceId}/transition")]
    [RequireCurrentFieldAccess("WRITE")]
    public async Task<ActionResult<WorkOrderSummary>> TransitionAsync(
        string instanceId, [FromBody] TransitionWorkOrderRequest request)
    {
        await RequireOrderSourceAsync(instanceId, "WRITE");
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "No active field selected." });

        if (request.ExpectedFieldId != fieldId || string.IsNullOrWhiteSpace(request.ExpectedState))
            return Conflict(new { error = "Review the work order in its original FIELD and state before transitioning." });
        var original = await _workOrders.GetByIdAsync(fieldId, instanceId);
        if (original is null) return NotFound();
        if (original.InstanceId != instanceId || original.FieldId != fieldId || original.State != request.ExpectedState)
            return Conflict(new { error = "The reviewed work-order state changed. Reload before continuing." });
        if (!_workOrders.GetAvailableTransitions(original.State, original.WoSubType).Contains(request.ToState, StringComparer.Ordinal))
            return Conflict(new { error = "The requested transition is not available from the reviewed state." });
        var result = await _workOrders.TransitionStateAsync(
            fieldId, instanceId, request.ToState, userId, request.Notes);
        return Ok(result);
    }

    /// <summary>Returns valid next states for a work order.</summary>
    [HttpGet("{instanceId}/transitions")]
    public async Task<ActionResult<List<string>>> GetTransitionsAsync(string instanceId)
    {
        await RequireOrderSourceAsync(instanceId, "READ",requireState:false);
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "No active field selected." });
        var detail = await _workOrders.GetByIdAsync(fieldId, instanceId);
        if (detail is null) return NotFound(new { error = $"Work order {instanceId} not found." });
        var transitions = _workOrders.GetAvailableTransitions(detail.State, detail.WoSubType);
        await RequireOrderSourceAsync(instanceId, "READ",requireState:false);
        return Ok(transitions);
    }

    [HttpDelete("{instanceId}")]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    public async Task<IActionResult> DeleteAsync(string instanceId)
    {
        await RequireOrderSourceAsync(instanceId, "WRITE");
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "No active field selected." });
        await _workOrders.DeleteAsync(fieldId, instanceId, userId);
        return NoContent();
    }

    // ── SCHEDULING ────────────────────────────────────────────────────────────

    [HttpPost("{instanceId}/schedule")]
    public async Task<ActionResult<ScheduleResult>> ScheduleAsync(
        string instanceId, [FromBody] ScheduleWorkOrderRequest request)
    {
        await RequireOrderSourceAsync(instanceId, "WRITE");
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var result = await _scheduling.ScheduleWorkOrderAsync(
            instanceId,
            request.EquipmentId,
            request.ProposedStart,
            TimeSpan.FromHours(request.DurationHours),
            userId);
        return Ok(result);
    }

    [HttpPost("{instanceId}/reschedule")]
    public async Task<ActionResult<ScheduleResult>> RescheduleAsync(
        string instanceId, [FromBody] DateTime newStart)
    {
        await RequireOrderSourceAsync(instanceId, "WRITE");
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var result = await _scheduling.RescheduleAsync(instanceId, newStart, userId);
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
        await RequireOrderSourceAsync(instanceId, "READ");
            if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var conflicts = await _scheduling.GetConflictsAsync(equipmentId, from, to, instanceId);
        await RequireOrderSourceAsync(instanceId, "READ");
        return Ok(conflicts);
    }

    // ── CONTRACTORS ───────────────────────────────────────────────────────────

    [HttpGet("{instanceId}/contractors")]
    public async Task<ActionResult<List<ContractorAssignment>>> GetContractorsAsync(string instanceId)
    {
        await RequireOrderSourceAsync(instanceId, "READ");
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var result = await _contractors.GetAssignmentsAsync(instanceId);
        await RequireOrderSourceAsync(instanceId, "READ");
        return Ok(result);
    }

    [HttpPost("{instanceId}/contractors")]
    public async Task<ActionResult<ContractorAssignment>> AssignContractorAsync(
        string instanceId, [FromBody] AssignContractorRequest request)
    {
        await RequireOrderSourceAsync(instanceId, "WRITE");
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
        var result = await _contractors.AssignContractorAsync(
            instanceId, request.StepId, request.BaId, request.RoleCode, userId);
        return Ok(result);
    }

    [HttpDelete("{instanceId}/contractors/{baId}")]
    public async Task<IActionResult> RemoveContractorAsync(string instanceId, string baId,
        [FromQuery] string stepId)
    {
        await RequireOrderSourceAsync(instanceId, "WRITE");
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(instanceId)) return BadRequest(new { error = "Instance ID is required." });
            if (string.IsNullOrWhiteSpace(baId)) return BadRequest(new { error = "Business associate ID is required." });
        await _contractors.RemoveContractorAsync(instanceId, stepId, baId, userId);
        return NoContent();
    }

    [HttpGet("contractor/{baId}/validate")]
    public async Task<ActionResult<ContractorQualificationResult>> ValidateContractorAsync(
        string baId,
        [FromQuery] string woType,
        [FromQuery] string jurisdiction)
    {
            if (string.IsNullOrWhiteSpace(baId)) return BadRequest(new { error = "Business associate ID is required." });
        if (string.IsNullOrWhiteSpace(jurisdiction)) return BadRequest(new { error = "An explicit jurisdiction is required." });
        var result = await _contractors.ValidateContractorAsync(baId, woType, jurisdiction);
        return Ok(result);
    }

    // ── COST CAPTURE ──────────────────────────────────────────────────────────

    // ── INSPECTION ────────────────────────────────────────────────────────────

    [HttpGet("{instanceId}/checklist")]
    public async Task<ActionResult<List<InspectionCondition>>> GetChecklistAsync(string instanceId)
    {
        var source=await RequireOrderSourceAsync(instanceId,"READ",requireState:false);
        var result=await _inspection.GetChecklistAsync(instanceId,source.FieldId);
        await RequireOrderSourceAsync(instanceId,"READ",requireState:false);return Ok(result);
    }
    [HttpPost("{instanceId}/checklist")]
    public async Task<ActionResult<InspectionCondition>> AddInspectionRequirementAsync(string instanceId,[FromBody] AddWorkOrderInspectionRequirementRequest request)
    {
        var source=await RequireOrderSourceAsync(instanceId,"WRITE",requireState:false);
        if(request.FieldId!=source.FieldId)return Conflict(new { error="Inspection requirement FIELD differs from the original order." });
        var result=await _inspection.AddRequirementAsync(instanceId,request,User.ActingUserId());return Ok(result);
    }
    [HttpPost("{instanceId}/checklist/{condSeq}")]
    public async Task<ActionResult<InspectionCondition>> RecordResultAsync(string instanceId,int condSeq,[FromBody] RecordInspectionResultRequest request)
    {
        var source=await RequireOrderSourceAsync(instanceId,"WRITE",requireState:false);
        if(request.FieldId!=source.FieldId)return Conflict(new { error="Inspection result FIELD differs from the original order." });
        var result=await _inspection.RecordResultAsync(instanceId,condSeq,request,User.ActingUserId());return Ok(result);
    }
    [HttpGet("{instanceId}/processes")]
    public async Task<ActionResult<Beep.OilandGas.ProductionOperations.Models.WorkOrderProcessReview>> GetProcessesAsync(string instanceId)
    {
        var source=await RequireOrderSourceAsync(instanceId,"READ",requireState:false);
        var rows=await _processes.GetProcessInstancesForEntityAsync(instanceId,"WORK_ORDER");
        if(rows is null)throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Conflict("Recorded work-order process evidence is unavailable.");
        var reads=new CompanyProcessReadService(_access);
        foreach(var row in rows.Where(x=>x is not null && x.FieldId==source.FieldId))
        {
            var confirmed=await reads.ReadAsync(_processes,User.ActingUserId(),source.FieldId,row.InstanceId);
            if(confirmed is null || confirmed.EntityId!=instanceId || confirmed.EntityType!="WORK_ORDER" || confirmed.ProcessId!=row.ProcessId)
                throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Conflict("The recorded work-order process relationship changed.");
        }
        var definitions=new List<Beep.OilandGas.Models.Processes.ProcessDefinition>();
        foreach(var processId in rows.Where(x=>x is not null && x.FieldId==source.FieldId).Select(x=>x.ProcessId).Distinct(StringComparer.Ordinal))
        {
            var definition=await _processes.GetProcessDefinitionAsync(processId);
            if(definition is null)throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Conflict("A recorded work-order process definition is unavailable.");
            definitions.Add(definition);
        }
        var company=await _company.RefreshCompanyAsync();
        var review=WorkOrderProcessReviews.Project(company.Scope,company.UserId,source.FieldId,instanceId,rows,definitions);
        foreach(var link in review.Links)
        {
            var confirmed=await reads.ReadAsync(_processes,User.ActingUserId(),source.FieldId,link.InstanceId);
            if(confirmed is null || confirmed.EntityId!=instanceId || confirmed.EntityType!="WORK_ORDER" || confirmed.ProcessId!=link.ProcessId ||
                confirmed.CurrentState!=link.State || confirmed.CurrentStepId!=link.StepId || confirmed.Status.ToString()!=link.Status)
                throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Conflict("The reviewed process changed before delivery. Reload its recorded evidence.");
        }
        await RequireOrderSourceAsync(instanceId,"READ",requireState:false);
        return Ok(review);
    }

    private async Task<WorkOrderDetailModel> RequireOrderSourceAsync(string id,string permission,bool requireState=true)
    {
        var actor=User.ActingUserId();
        var field=_fieldOrchestrator.CurrentFieldId ?? string.Empty;
        return await WorkOrderSourceChecks.RequireAsync(field,id,
            async()=>_fieldOrchestrator.CurrentFieldId==field &&
                (await _access.CheckAssetAccessAsync(actor,field,"FIELD",permission))?.HasAccess==true &&
                (await _access.CheckAssetAccessAsync(actor,id,"WORK_ORDER",permission))?.HasAccess==true,
            ()=>_workOrders.GetByIdAsync(field,id),requireState:requireState);
    }
}
