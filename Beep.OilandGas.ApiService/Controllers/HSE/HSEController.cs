using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.ApiService.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Beep.OilandGas.ApiService.Controllers.HSE;

[ApiController]
[Route("api/field/current/hse")]
[Authorize]
[RequireCurrentFieldAccess]
public class HSEController : ControllerBase
{
    private readonly IHSEService               _hse;
    private readonly IRCAService               _rca;
    private readonly IBarrierManagementService _barriers;
    private readonly ICorrectiveActionService  _ca;
    private readonly IHAZOPService             _hazop;
    private readonly IHSEKPIService            _kpi;
    private readonly IFieldOrchestrator        _fieldOrchestrator;

    public HSEController(
        IHSEService               hse,
        IRCAService               rca,
        IBarrierManagementService barriers,
        ICorrectiveActionService  ca,
        IHAZOPService             hazop,
        IHSEKPIService            kpi,
        IFieldOrchestrator        fieldOrchestrator)
    {
        _hse               = hse;
        _rca               = rca;
        _barriers          = barriers;
        _ca                = ca;
        _hazop             = hazop;
        _kpi               = kpi;
        _fieldOrchestrator = fieldOrchestrator;
    }

    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

    // ── Incidents ──────────────────────────────────────────────────────────────

    [HttpGet("incidents")]
    public async Task<ActionResult<List<HSEIncidentRecord>>> GetIncidentsAsync(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        DateRangeFilter? range = (from.HasValue && to.HasValue)
            ? new DateRangeFilter(from.Value, to.Value)
            : null;

        return Ok(await _hse.GetFieldIncidentsAsync(fieldId, range));
    }

    [HttpGet("incidents/{incidentId}")]
    public async Task<ActionResult<HSEIncidentRecord>> GetIncidentAsync(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        var result = await _hse.GetByIdAsync(incidentId);
        if (result is null) return NotFound(new { error = $"Incident {incidentId} not found." });
        return Ok(result);
    }

    [HttpPost("incidents")]
    public async Task<ActionResult<HSEIncidentRecord>> ReportAsync(
        [FromBody] ReportIncidentRequest request)
    {
        var result = await _hse.ReportIncidentAsync(request, UserId);
        return CreatedAtAction(nameof(GetIncidentAsync), new { incidentId = result.IncidentId }, result);
    }

    [HttpPost("incidents/{incidentId}/transition")]
    public async Task<ActionResult> TransitionAsync(
        string incidentId, [FromBody] TransitionIncidentRequest request)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        var ok = await _hse.TransitionAsync(incidentId, request.Trigger, request.Reason, UserId);
        return ok ? NoContent() : BadRequest(new { error = "Invalid transition." });
    }

    [HttpPut("incidents/{incidentId}/tier")]
    public async Task<ActionResult> UpdateTierAsync(
        string incidentId, [FromQuery] int tier)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        await _hse.UpdateTierAsync(incidentId, tier, UserId);
        return NoContent();
    }

    [HttpPut("incidents/{incidentId}/investigator")]
    public async Task<ActionResult> AssignInvestigatorAsync(
        string incidentId, [FromQuery] string baId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        if (string.IsNullOrWhiteSpace(baId)) return BadRequest(new { error = "Business associate ID is required." });
        await _hse.AssignInvestigatorAsync(incidentId, baId, UserId);
        return NoContent();
    }

    // ── RCA ────────────────────────────────────────────────────────────────────

    [HttpGet("incidents/{incidentId}/causes")]
    public async Task<ActionResult<List<CauseFinding>>> GetCausesAsync(string incidentId)
        {
            if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
            return Ok(await _rca.GetCauseChainAsync(incidentId));
        }

    [HttpPost("incidents/{incidentId}/causes")]
    public async Task<ActionResult> AddCauseAsync(
        string incidentId, [FromBody] AddCauseRequest request)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        await _rca.AddCauseAsync(incidentId, request, UserId);
        return NoContent();
    }

    [HttpGet("incidents/{incidentId}/rca-complete")]
    public async Task<ActionResult<bool>> IsRCACompleteAsync(string incidentId)
        {
            if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
            return Ok(await _rca.IsRCACompleteAsync(incidentId));
        }

    // ── Barriers ───────────────────────────────────────────────────────────────

    [HttpGet("incidents/{incidentId}/barriers")]
    public async Task<ActionResult<List<BarrierRecord>>> GetBarriersAsync(string incidentId)
        {
            if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
            return Ok(await _barriers.GetBarriersForIncidentAsync(incidentId));
        }

    [HttpPost("incidents/{incidentId}/barriers")]
    public async Task<ActionResult> AddBarrierAsync(
        string incidentId, [FromBody] AddBarrierRequest request)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        await _barriers.AddBarrierAsync(incidentId, request, UserId);
        return NoContent();
    }

    [HttpPut("incidents/{incidentId}/barriers/{equipId}/status")]
    public async Task<ActionResult> SetBarrierStatusAsync(
        string incidentId, string equipId, [FromQuery] string status)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
            if (string.IsNullOrWhiteSpace(equipId)) return BadRequest(new { error = "Equipment ID is required." });
        if (string.IsNullOrWhiteSpace(status)) return BadRequest(new { error = "Status is required." });
        await _barriers.SetBarrierStatusAsync(incidentId, equipId, status, UserId);
        return NoContent();
    }

    [HttpGet("incidents/{incidentId}/barriers/summary")]
    public async Task<ActionResult<BarrierSummary>> GetBarrierSummaryAsync(string incidentId)
        {
            if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
            return Ok(await _barriers.GetBarrierSummaryAsync(incidentId));
        }

    // ── Corrective Actions ─────────────────────────────────────────────────────

    [HttpGet("incidents/{incidentId}/cas")]
    public async Task<ActionResult<List<CAStatus>>> GetCAsAsync(string incidentId)
        {
            if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
            return Ok(await _ca.GetCAStatusAsync(incidentId));
        }

    [HttpPost("incidents/{incidentId}/ca-plan")]
    public async Task<ActionResult<string>> CreateCAPlanAsync(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        var id = await _ca.CreateCAPlanAsync(incidentId, UserId);
        return Ok(id);
    }

    [HttpPost("incidents/{incidentId}/cas")]
    public async Task<ActionResult<string>> AddCAAsync(
        string incidentId, [FromBody] AddCARequest request)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        var id = await _ca.AddCorrectiveActionAsync(incidentId, request, UserId);
        return Ok(id);
    }

    [HttpPost("incidents/{incidentId}/cas/{stepSeq}/complete")]
    public async Task<ActionResult> CompleteCAAsync(
        string incidentId, int stepSeq, [FromQuery] string notes = "")
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        await _ca.RecordCompletionAsync(incidentId, stepSeq, notes, UserId);
        return NoContent();
    }

    // ── HAZOP ──────────────────────────────────────────────────────────────────

    [HttpGet("hazop")]
    public async Task<ActionResult<List<HAZOPSummary>>> GetStudiesAsync()
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        var studies = await _hazop.GetStudiesAsync(fieldId);
        return Ok(studies);
    }

    [HttpPost("hazop")]
    public async Task<ActionResult<string>> CreateStudyAsync(
        [FromBody] CreateHAZOPStudyRequest request)
    {
        var id = await _hazop.CreateStudyAsync(request, UserId);
        return CreatedAtAction(nameof(GetStudySummaryAsync), new { studyId = id }, id);
    }

    [HttpGet("hazop/{studyId}")]
    public async Task<ActionResult<HAZOPSummary>> GetStudySummaryAsync(string studyId)
        {
            if (string.IsNullOrWhiteSpace(studyId)) return BadRequest(new { error = "Study ID is required." });
            return Ok(await _hazop.GetSummaryAsync(studyId));
        }

    [HttpGet("hazop/{studyId}/nodes")]
    public async Task<ActionResult<List<HAZOPNode>>> GetNodesAsync(string studyId)
        {
            if (string.IsNullOrWhiteSpace(studyId)) return BadRequest(new { error = "Study ID is required." });
            return Ok(await _hazop.GetNodesAsync(studyId));
        }

    [HttpPost("hazop/{studyId}/nodes")]
    public async Task<ActionResult<string>> AddNodeAsync(
        string studyId, [FromBody] AddNodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(studyId)) return BadRequest(new { error = "Study ID is required." });
        var id = await _hazop.AddNodeAsync(studyId, request, UserId);
        return Ok(id);
    }

    [HttpPost("hazop/{studyId}/nodes/{nodeSeq}/deviations")]
    public async Task<ActionResult<string>> AddDeviationAsync(
        string studyId, int nodeSeq, [FromBody] AddDeviationRequest request)
    {
        if (string.IsNullOrWhiteSpace(studyId)) return BadRequest(new { error = "Study ID is required." });
        var id = await _hazop.AddDeviationAsync(studyId, nodeSeq, request, UserId);
        return Ok(id);
    }

    [HttpPut("hazop/{studyId}/nodes/{nodeSeq}/deviations/{condSeq}/status")]
    public async Task<ActionResult> UpdateDeviationStatusAsync(
        string studyId, int nodeSeq, int condSeq, [FromQuery] string status)
    {
        if (string.IsNullOrWhiteSpace(studyId)) return BadRequest(new { error = "Study ID is required." });
        if (string.IsNullOrWhiteSpace(status)) return BadRequest(new { error = "Status is required." });
        await _hazop.UpdateDeviationStatusAsync(studyId, nodeSeq, condSeq, status, UserId);
        return NoContent();
    }

    // ── KPIs ───────────────────────────────────────────────────────────────────

    [HttpGet("kpi")]
    public async Task<ActionResult<HSEKPISet>> GetKPIsAsync(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        var range = new DateRangeFilter(
            from ?? DateTime.UtcNow.AddYears(-1),
            to   ?? DateTime.UtcNow);

        return Ok(await _kpi.GetKPIsAsync(fieldId, range));
    }

    [HttpGet("kpi/trend")]
    public async Task<ActionResult<List<TierRateTrend>>> GetTrendAsync(
        [FromQuery] int months = 12)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        return Ok(await _kpi.GetTierRateTrendAsync(fieldId, months));
    }
}
