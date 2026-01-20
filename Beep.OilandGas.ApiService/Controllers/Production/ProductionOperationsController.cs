using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Production
{
    /// <summary>
    /// API controller for production operations.
    /// </summary>
    [ApiController]
    [Route("api/production/operations")]
    [Authorize]
    public class ProductionOperationsController : ControllerBase
    {
        private readonly IProductionOperationsService _service;
        private readonly ILogger<ProductionOperationsController> _logger;

        public ProductionOperationsController(IProductionOperationsService service, ILogger<ProductionOperationsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("data")]
        public async Task<ActionResult<List<ProductionData>>> GetProductionData(
            [FromQuery] string? wellUWI = null,
            [FromQuery] string? fieldId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;
                var result = await _service.GetProductionDataAsync(wellUWI, fieldId, start, end);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production data");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("data")]
        public async Task<ActionResult> RecordProductionData([FromBody] ProductionData productionData, [FromQuery] string? userId = null)
        {
            try
            {
                await _service.RecordProductionDataAsync(productionData, userId ?? GetUserId());
                return Ok(new { message = "Production data recorded successfully", productionId = productionData.ProductionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording production data");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("optimize")]
        public async Task<ActionResult<List<ProductionOptimizationRecommendation>>> OptimizeProduction(
            [FromQuery] string wellUWI,
            [FromBody] Dictionary<string, object> optimizationGoals)
        {
            try
            {
                var result = await _service.OptimizeProductionAsync(wellUWI, optimizationGoals);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing production");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }
}

