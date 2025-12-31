using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Operations
{
    /// <summary>
    /// API controller for prospect identification operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProspectIdentificationController : ControllerBase
    {
        private readonly IProspectIdentificationService _service;
        private readonly ILogger<ProspectIdentificationController> _logger;

        public ProspectIdentificationController(IProspectIdentificationService service, ILogger<ProspectIdentificationController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("evaluate/{prospectId}")]
        public async Task<ActionResult<ProspectEvaluationDto>> EvaluateProspect(string prospectId)
        {
            try
            {
                var result = await _service.EvaluateProspectAsync(prospectId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating prospect {ProspectId}", prospectId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<ProspectDto>>> GetProspects([FromQuery] Dictionary<string, string>? filters = null)
        {
            try
            {
                var result = await _service.GetProspectsAsync(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prospects");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<string>> CreateProspect([FromBody] ProspectDto prospect, [FromQuery] string? userId = null)
        {
            try
            {
                var prospectId = await _service.CreateProspectAsync(prospect, userId ?? GetUserId());
                return Ok(new { message = "Prospect created successfully", prospectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prospect");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("rank")]
        public async Task<ActionResult<List<ProspectRankingDto>>> RankProspects([FromBody] RankProspectsRequest request)
        {
            try
            {
                var result = await _service.RankProspectsAsync(request.ProspectIds, request.RankingCriteria);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ranking prospects");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }

    public class RankProspectsRequest
    {
        public List<string> ProspectIds { get; set; } = new();
        public Dictionary<string, decimal> RankingCriteria { get; set; } = new();
    }
}

