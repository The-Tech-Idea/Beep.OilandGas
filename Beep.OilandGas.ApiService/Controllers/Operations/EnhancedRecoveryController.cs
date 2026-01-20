using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Operations
{
    /// <summary>
    /// API controller for enhanced recovery operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnhancedRecoveryController : ControllerBase
    {
        private readonly IEnhancedRecoveryService _service;
        private readonly ILogger<EnhancedRecoveryController> _logger;

        public EnhancedRecoveryController(IEnhancedRecoveryService service, ILogger<EnhancedRecoveryController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("analyze-eor")]
        public async Task<ActionResult<EnhancedRecoveryOperation>> AnalyzeEOR([FromBody] AnalyzeEORRequest request)
        {
            try
            {
                var result = await _service.AnalyzeEORPotentialAsync(request.FieldId, request.EorMethod);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing EOR potential for field {FieldId}", request.FieldId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("recovery-factor")]
        public async Task<ActionResult<EnhancedRecoveryOperation>> CalculateRecoveryFactor([FromBody] CalculateRecoveryFactorRequest request)
        {
            try
            {
                var result = await _service.CalculateRecoveryFactorAsync(request.ProjectId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating recovery factor for project {ProjectId}", request.ProjectId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("injection")]
        public async Task<ActionResult<InjectionOperation>> ManageInjection([FromBody] ManageInjectionRequest request)
        {
            try
            {
                var result = await _service.ManageInjectionAsync(request.InjectionWellId, request.InjectionRate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error managing injection for well {InjectionWellId}", request.InjectionWellId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }
}

