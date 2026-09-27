using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Compliance;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using RoyaltySummary = Beep.OilandGas.Models.Data.Compliance.RoyaltySummary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Compliance;

[ApiController]
[Route("api/field/current/compliance")]
[Authorize]
[RequireCurrentFieldAccess]
public class ComplianceController : ControllerBase
{
    private readonly IComplianceService         _compliance;
    private readonly IRoyaltyCalculationService _royalty;
    private readonly IGHGReportingService       _ghg;
    private readonly IFieldOrchestrator         _fieldOrchestrator;

    public ComplianceController(
        IComplianceService         compliance,
        IRoyaltyCalculationService royalty,
        IGHGReportingService       ghg,
        IFieldOrchestrator         fieldOrchestrator)
    {
        _compliance        = compliance;
        _royalty           = royalty;
        _ghg               = ghg;
        _fieldOrchestrator = fieldOrchestrator;
    }

    // ── Obligations ───────────────────────────────────────────────────────────

    [HttpGet("obligations")]
    public async Task<ActionResult<List<ObligationSummary>>> GetAllAsync([FromQuery] int? year)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        var y       = year ?? DateTime.UtcNow.Year;
        return Ok(await _compliance.GetAllObligationsAsync(fieldId, y));
    }

    [HttpGet("obligations/upcoming")]
    public async Task<ActionResult<List<ObligationSummary>>> GetUpcomingAsync(
        [FromQuery] int daysAhead = 30)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        return Ok(await _compliance.GetUpcomingObligationsAsync(fieldId, daysAhead));
    }

    [HttpGet("obligations/overdue")]
    public async Task<ActionResult<List<ObligationSummary>>> GetOverdueAsync()
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        return Ok(await _compliance.GetOverdueObligationsAsync(fieldId));
    }

    [HttpGet("obligations/{obligationId}")]
    public async Task<ActionResult<ObligationDetailModel>> GetByIdAsync(string obligationId)
    {
        if (string.IsNullOrWhiteSpace(obligationId)) return BadRequest(new { error = "Obligation ID is required." });
        var result = await _compliance.GetByIdAsync(obligationId);
        if (result is null) return NotFound(new { error = $"Obligation {obligationId} not found." });
        return Ok(result);
    }

    [HttpPost("obligations")]
    public async Task<ActionResult<string>> CreateAsync(
        [FromBody] CreateObligationRequest request)
    {
        var userId = User.ActingUserId();
        var id = await _compliance.CreateObligationAsync(request, userId);
        return CreatedAtAction(nameof(GetByIdAsync), new { obligationId = id }, id);
    }

    [HttpPost("obligations/{obligationId}/submit")]
    public async Task<ActionResult> SubmitAsync(
        string obligationId, [FromQuery] DateTime? submitDate)
    {
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(obligationId)) return BadRequest(new { error = "Obligation ID is required." });
        await _compliance.MarkSubmittedAsync(obligationId, submitDate ?? DateTime.UtcNow, userId);
        return NoContent();
    }

    [HttpPost("obligations/{obligationId}/waive")]
    public async Task<ActionResult> WaiveAsync(
        string obligationId, [FromQuery] string reason)
    {
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(obligationId)) return BadRequest(new { error = "Obligation ID is required." });
        await _compliance.WaiveObligationAsync(obligationId, reason, userId);
        return NoContent();
    }

    [HttpPost("obligations/{obligationId}/payment")]
    public async Task<ActionResult> RecordPaymentAsync(
        string obligationId, [FromBody] RecordPaymentRequest request)
    {
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(obligationId)) return BadRequest(new { error = "Obligation ID is required." });
        await _compliance.RecordPaymentAsync(obligationId, request, userId);
        return NoContent();
    }

    [HttpGet("score")]
    public async Task<ActionResult<ComplianceScoreCard>> GetScoreAsync([FromQuery] int? year)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        var y       = year ?? DateTime.UtcNow.Year;
        return Ok(await _compliance.GetComplianceScoreAsync(fieldId, y));
    }

    // ── Royalty ───────────────────────────────────────────────────────────────

    [HttpPost("royalty/usa")]
    public async Task<ActionResult<RoyaltySummary>> CalculateUSARoyaltyAsync(
        [FromQuery] int year, [FromQuery] int month)
    {
        var userId = User.ActingUserId();
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        return Ok(await _royalty.CalculateUSARoyaltyAsync(fieldId, year, month, userId));
    }

    [HttpPost("royalty/canada")]
    public async Task<ActionResult<RoyaltySummary>> CalculateCanadaRoyaltyAsync(
        [FromQuery] int year, [FromQuery] int quarter)
    {
        var userId = User.ActingUserId();
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        return Ok(await _royalty.CalculateAlbertaCrownRoyaltyAsync(fieldId, year, quarter, userId));
    }

    [HttpGet("royalty/variance")]
    public async Task<ActionResult<List<RoyaltyVariance>>> GetVarianceAsync([FromQuery] int? year)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        var y       = year ?? DateTime.UtcNow.Year;
        return Ok(await _royalty.GetVarianceHistoryAsync(fieldId, y));
    }

    // ── GHG ───────────────────────────────────────────────────────────────────

    [HttpPost("ghg/report")]
    public async Task<ActionResult<GHGEmissionReport>> GenerateGHGReportAsync(
        [FromQuery] int year, [FromQuery] string jurisdiction = "USA")
    {
        var userId = User.ActingUserId();
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        return Ok(await _ghg.GenerateAnnualReportAsync(fieldId, year, jurisdiction, userId));
    }

    [HttpGet("ghg/total")]
    public async Task<ActionResult<double>> GetTotalEmissionsAsync(
        [FromQuery] int? year, [FromQuery] string? jurisdiction)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
        var y       = year ?? DateTime.UtcNow.Year;
        return Ok(await _ghg.GetTotalEmissionsAsync(fieldId, y, jurisdiction));
    }
}
