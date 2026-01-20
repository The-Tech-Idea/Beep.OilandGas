using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Calculations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Calculations
{
    /// <summary>
    /// API controller for production forecasting operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductionForecastingController : ControllerBase
    {
        private readonly IProductionForecastingService _service;
        private readonly ILogger<ProductionForecastingController> _logger;

        public ProductionForecastingController(IProductionForecastingService service, ILogger<ProductionForecastingController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<ProductionForecastResult>> GenerateForecast([FromBody] GenerateForecastRequest request)
        {
            try
            {
                var result = await _service.GenerateForecastAsync(
                    request.WellUWI,
                    request.FieldId,
                    request.ForecastMethod,
                    request.ForecastPeriod);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating production forecast");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("decline-curve")]
        public async Task<ActionResult<DeclineCurveAnalysis>> PerformDeclineCurveAnalysis([FromBody] DeclineCurveAnalysisRequest request)
        {
            try
            {
                var result = await _service.PerformDeclineCurveAnalysisAsync(
                    request.WellUWI,
                    request.StartDate,
                    request.EndDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing decline curve analysis for well {WellUWI}", request.WellUWI);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("forecast")]
        public async Task<ActionResult> SaveForecast([FromBody] ProductionForecastResult forecast, [FromQuery] string? userId = null)
        {
            try
            {
                await _service.SaveForecastAsync(forecast, userId ?? GetUserId());
                return Ok(new { message = "Production forecast saved successfully", forecastId = forecast.ForecastId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving production forecast");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }
}

