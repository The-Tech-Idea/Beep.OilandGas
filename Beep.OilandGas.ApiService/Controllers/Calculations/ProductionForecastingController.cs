using System.Security.Claims;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Calculations;
using GenerateForecastRequest = Beep.OilandGas.Models.Data.ProductionForecasting.GenerateForecastRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<ProductionForecastResult>> GenerateForecast([FromBody] GenerateForecastRequest? request)
        {
            if (request is null)
                return BadRequest(new { error = "Request body is required." });

            try
            {
                var result = await _service.GenerateForecastAsync(request);
                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating production forecast");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("decline-curve")]
        public async Task<ActionResult<DeclineCurveAnalysis>> PerformDeclineCurveAnalysis([FromBody] DeclineCurveAnalysisRequest? request)
        {
            if (request is null)
                return BadRequest(new { error = "Request body is required." });

            try
            {
                var result = await _service.PerformDeclineCurveAnalysisAsync(
                    request.WellUWI,
                    request.StartDate,
                    request.EndDate);
                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing decline curve analysis for well {WellUWI}", request.WellUWI);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("forecast")]
        public async Task<ActionResult> SaveForecast([FromBody] ProductionForecastResult? forecast, [FromQuery] string? userId = null)
        {
            if (forecast is null)
                return BadRequest(new { error = "Forecast body is required." });

            try
            {
                await _service.SaveForecastAsync(forecast, userId ?? GetUserId());
                return Ok(new { message = "Production forecast saved successfully", forecastId = forecast.ForecastId });
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving production forecast");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private string GetUserId() =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? "SYSTEM";
    }
}
