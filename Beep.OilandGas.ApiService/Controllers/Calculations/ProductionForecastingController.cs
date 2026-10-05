using Beep.OilandGas.ApiService.Services;
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

            var result = await _service.GenerateForecastAsync(request);
            return Ok(result);
        }

        [HttpPost("decline-curve")]
        public async Task<ActionResult<DeclineCurveAnalysis>> PerformDeclineCurveAnalysis([FromBody] DeclineCurveAnalysisRequest? request)
        {
            if (request is null)
                return BadRequest(new { error = "Request body is required." });

            var result = await _service.PerformDeclineCurveAnalysisAsync(
                request.WellUWI,
                request.StartDate,
                request.EndDate);
            return Ok(result);
        }

        [HttpPost("forecast")]
        public async Task<ActionResult> SaveForecast([FromBody] ProductionForecastResult? forecast)
        {
            var userId = User.ActingUserId();
            if (forecast is null)
                return BadRequest(new { error = "Forecast body is required." });

            await _service.SaveForecastAsync(forecast, userId);
            return Ok(new { message = "Production forecast saved successfully", forecastId = forecast.ForecastId });
        }
    }
}
