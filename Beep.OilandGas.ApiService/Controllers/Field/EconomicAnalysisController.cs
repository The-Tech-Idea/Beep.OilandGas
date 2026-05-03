using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Controllers.Common;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Field;

[ApiController]
[Authorize]
[Route("api/field/current/economic-analysis")]
[RequireCurrentFieldAccess]
public class EconomicAnalysisController : ControllerBase
{
    private readonly IEconomicAnalysisService _service;
    private readonly ILogger<EconomicAnalysisController> _logger;

    public EconomicAnalysisController(IEconomicAnalysisService service, ILogger<EconomicAnalysisController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost("npv")]
    public ActionResult<double> CalculateNPV([FromBody] CalculateNPVRequest request)
    {
        if (request == null)
            return BadRequest(new { error = "Request payload is required." });
        try
        {
            var result = _service.CalculateNPV(EconomicAnalysisControllerHelpers.ToCashFlows(request.CashFlows), request.DISCOUNT_RATE);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid field-scoped request for NPV calculation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating field-scoped NPV");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    [HttpPost("irr")]
    public ActionResult<double> CalculateIRR([FromBody] CalculateIRRRequest request)
    {
        if (request == null)
            return BadRequest(new { error = "Request payload is required." });
        try
        {
            var result = _service.CalculateIRR(EconomicAnalysisControllerHelpers.ToCashFlows(request.CashFlows), request.InitialGuess);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid field-scoped request for IRR calculation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating field-scoped IRR");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    [HttpPost("analyze")]
    public ActionResult<EconomicResult> Analyze([FromBody] AnalyzeRequest request)
    {
        if (request == null)
            return BadRequest(new { error = "Request payload is required." });
        try
        {
            var result = _service.Analyze(
                EconomicAnalysisControllerHelpers.ToCashFlows(request.CashFlows),
                request.DISCOUNT_RATE,
                request.FinanceRate,
                request.ReinvestRate);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid field-scoped request for economic analysis");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running field-scoped economic analysis");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    [HttpPost("result")]
    public async Task<ActionResult> SaveResult([FromBody] SaveAnalysisResultRequest request, [FromQuery] string? userId = null)
    {
        if (!EconomicAnalysisControllerHelpers.TryValidateSaveRequest(request, out var validationError))
            return BadRequest(new { error = validationError });
        try
        {
            await _service.SaveAnalysisResultAsync(request.AnalysisId, request.Result!, userId ?? ResolveUserId());
            return Ok(new { message = "Economic analysis result saved successfully", analysisId = request.AnalysisId });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid field-scoped request when saving economic analysis result");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving field-scoped economic analysis result");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    [HttpGet("result/{analysisId}")]
    public async Task<ActionResult<EconomicResult>> GetResult(string analysisId)
    {
        if (string.IsNullOrWhiteSpace(analysisId))
            return BadRequest(new { error = "Analysis ID is required." });

        try
        {
            var result = await _service.GetAnalysisResultAsync(analysisId);
            if (result == null)
                return NotFound(new { error = $"Analysis {analysisId} not found." });
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid field-scoped request when retrieving economic analysis result");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting field-scoped economic analysis result {AnalysisId}", analysisId);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    private string ResolveUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? "SYSTEM";

}
