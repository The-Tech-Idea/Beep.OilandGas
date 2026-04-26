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
    private readonly IFieldOrchestrator _fieldOrchestrator;

    public HSEController(IFieldOrchestrator fieldOrchestrator)
    {
        _fieldOrchestrator = fieldOrchestrator;
    }

    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
    private IFieldHSEService Hse => _fieldOrchestrator.GetHSEService();

    // ── Incidents ──────────────────────────────────────────────────────────────

    [HttpGet("incidents")]
    public async Task<ActionResult<List<HSEIncidentRecord>>> GetIncidentsAsync(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        DateRangeFilter? range = (from.HasValue && to.HasValue)
            ? new DateRangeFilter(from.Value, to.Value)
            : null;

        if (string.IsNullOrWhiteSpace(_fieldOrchestrator.CurrentFieldId)) return BadRequest(new { error = "Current field is required." });
        return Ok(await Hse.GetIncidentsAsync(range));
    }

    [HttpGet("incidents/{incidentId}")]
    public async Task<ActionResult<HSEIncidentRecord>> GetIncidentAsync(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        var result = await Hse.GetIncidentAsync(incidentId);
        if (result is null) return NotFound(new { error = $"Incident {incidentId} not found." });
        return Ok(result);
    }

    [HttpPost("incidents")]
    public async Task<ActionResult<HSEIncidentRecord>> ReportAsync(
        [FromBody] ReportIncidentRequest request)
    {
        var result = await Hse.ReportIncidentAsync(request, UserId);
        return CreatedAtAction(nameof(GetIncidentAsync), new { incidentId = result.IncidentId }, result);
    }

    [HttpPost("incidents/{incidentId}/transition")]
    public async Task<ActionResult> TransitionAsync(
        string incidentId, [FromBody] TransitionIncidentRequest request)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        var ok = await Hse.TransitionAsync(incidentId, request.Trigger, request.Reason, UserId);
        return ok ? NoContent() : BadRequest(new { error = "Invalid transition." });
    }

    [HttpPut("incidents/{incidentId}/tier")]
    public async Task<ActionResult> UpdateTierAsync(
        string incidentId, [FromQuery] int tier)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        await Hse.UpdateTierAsync(incidentId, tier, UserId);
        return NoContent();
    }

    [HttpPut("incidents/{incidentId}/investigator")]
    public async Task<ActionResult> AssignInvestigatorAsync(
        string incidentId, [FromQuery] string baId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        if (string.IsNullOrWhiteSpace(baId)) return BadRequest(new { error = "Business associate ID is required." });
        await Hse.AssignInvestigatorAsync(incidentId, baId, UserId);
        return NoContent();
    }

    // ── RCA ────────────────────────────────────────────────────────────────────

    [HttpGet("incidents/{incidentId}/causes")]
    public async Task<ActionResult<List<CauseFinding>>> GetCausesAsync(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        return Ok(await Hse.GetCausesAsync(incidentId));
    }

    [HttpPost("incidents/{incidentId}/causes")]
    public async Task<ActionResult> AddCauseAsync(
        string incidentId, [FromBody] AddCauseRequest request)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        await Hse.AddCauseAsync(incidentId, request, UserId);
        return NoContent();
    }

    [HttpGet("incidents/{incidentId}/rca-complete")]
    public async Task<ActionResult<bool>> IsRCACompleteAsync(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        return Ok(await Hse.IsRcaCompleteAsync(incidentId));
    }

    // ── Barriers ───────────────────────────────────────────────────────────────

    [HttpGet("incidents/{incidentId}/barriers")]
    public async Task<ActionResult<List<BarrierRecord>>> GetBarriersAsync(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        return Ok(await Hse.GetBarriersAsync(incidentId));
    }

    [HttpPost("incidents/{incidentId}/barriers")]
    public async Task<ActionResult> AddBarrierAsync(
        string incidentId, [FromBody] AddBarrierRequest request)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        await Hse.AddBarrierAsync(incidentId, request, UserId);
        return NoContent();
    }

    [HttpPut("incidents/{incidentId}/barriers/{equipId}/status")]
    public async Task<ActionResult> SetBarrierStatusAsync(
        string incidentId, string equipId, [FromQuery] string status)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        if (string.IsNullOrWhiteSpace(equipId)) return BadRequest(new { error = "Equipment ID is required." });
        if (string.IsNullOrWhiteSpace(status)) return BadRequest(new { error = "Status is required." });
        await Hse.SetBarrierStatusAsync(incidentId, equipId, status, UserId);
        return NoContent();
    }

    [HttpGet("incidents/{incidentId}/barriers/summary")]
    public async Task<ActionResult<BarrierSummary>> GetBarrierSummaryAsync(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        return Ok(await Hse.GetBarrierSummaryAsync(incidentId));
    }

    // ── Corrective Actions ─────────────────────────────────────────────────────

    [HttpGet("incidents/{incidentId}/cas")]
    public async Task<ActionResult<List<CAStatus>>> GetCAsAsync(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        return Ok(await Hse.GetCorrectiveActionsAsync(incidentId));
    }

    [HttpPost("incidents/{incidentId}/ca-plan")]
    public async Task<ActionResult<string>> CreateCAPlanAsync(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        var id = await Hse.CreateCaPlanAsync(incidentId, UserId);
        return Ok(id);
    }

    [HttpPost("incidents/{incidentId}/cas")]
    public async Task<ActionResult<string>> AddCAAsync(
        string incidentId, [FromBody] AddCARequest request)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        var id = await Hse.AddCorrectiveActionAsync(incidentId, request, UserId);
        return Ok(id);
    }

    [HttpPost("incidents/{incidentId}/cas/{stepSeq}/complete")]
    public async Task<ActionResult> CompleteCAAsync(
        string incidentId, int stepSeq, [FromQuery] string notes = "")
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        await Hse.RecordCompletionAsync(incidentId, stepSeq, notes, UserId);
        return NoContent();
    }

    // ── HAZOP ──────────────────────────────────────────────────────────────────

    [HttpGet("hazop")]
    public async Task<ActionResult<List<HAZOPSummary>>> GetStudiesAsync()
    {
        var studies = await Hse.GetStudiesAsync();
        return Ok(studies);
    }

    [HttpPost("hazop")]
    public async Task<ActionResult<string>> CreateStudyAsync(
        [FromBody] CreateHAZOPStudyRequest request)
    {
        var id = await Hse.CreateStudyAsync(request, UserId);
        return CreatedAtAction(nameof(GetStudySummaryAsync), new { studyId = id }, id);
    }

    [HttpGet("hazop/{studyId}")]
    public async Task<ActionResult<HAZOPSummary>> GetStudySummaryAsync(string studyId)
    {
        if (string.IsNullOrWhiteSpace(studyId)) return BadRequest(new { error = "Study ID is required." });
        return Ok(await Hse.GetStudySummaryAsync(studyId));
    }

    [HttpGet("hazop/{studyId}/nodes")]
    public async Task<ActionResult<List<HAZOPNode>>> GetNodesAsync(string studyId)
    {
        if (string.IsNullOrWhiteSpace(studyId)) return BadRequest(new { error = "Study ID is required." });
        return Ok(await Hse.GetNodesAsync(studyId));
    }

    [HttpPost("hazop/{studyId}/nodes")]
    public async Task<ActionResult<string>> AddNodeAsync(
        string studyId, [FromBody] AddNodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(studyId)) return BadRequest(new { error = "Study ID is required." });
        var id = await Hse.AddNodeAsync(studyId, request, UserId);
        return Ok(id);
    }

    [HttpPost("hazop/{studyId}/nodes/{nodeSeq}/deviations")]
    public async Task<ActionResult<string>> AddDeviationAsync(
        string studyId, int nodeSeq, [FromBody] AddDeviationRequest request)
    {
        if (string.IsNullOrWhiteSpace(studyId)) return BadRequest(new { error = "Study ID is required." });
        var id = await Hse.AddDeviationAsync(studyId, nodeSeq, request, UserId);
        return Ok(id);
    }

    [HttpPut("hazop/{studyId}/nodes/{nodeSeq}/deviations/{condSeq}/status")]
    public async Task<ActionResult> UpdateDeviationStatusAsync(
        string studyId, int nodeSeq, int condSeq, [FromQuery] string status)
    {
        if (string.IsNullOrWhiteSpace(studyId)) return BadRequest(new { error = "Study ID is required." });
        if (string.IsNullOrWhiteSpace(status)) return BadRequest(new { error = "Status is required." });
        await Hse.UpdateDeviationStatusAsync(studyId, nodeSeq, condSeq, status, UserId);
        return NoContent();
    }

    // ── KPIs ───────────────────────────────────────────────────────────────────

    [HttpGet("kpi")]
    public async Task<ActionResult<HSEKPISet>> GetKPIsAsync(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var range = new DateRangeFilter(
            from ?? DateTime.UtcNow.AddYears(-1),
            to   ?? DateTime.UtcNow);

        return Ok(await Hse.GetKpisAsync(range));
    }

    [HttpGet("kpi/trend")]
    public async Task<ActionResult<List<TierRateTrend>>> GetTrendAsync(
        [FromQuery] int months = 12)
    {
        return Ok(await Hse.GetTrendAsync(months));
    }
}
