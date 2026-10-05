using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Pumps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Pumps
{
    /// <summary>
    /// API controller for sucker rod pumping operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SuckerRodPumpingController : ControllerBase
    {
        private readonly ISuckerRodPumpingService _service;
        private readonly ILogger<SuckerRodPumpingController> _logger;

        public SuckerRodPumpingController(ISuckerRodPumpingService service, ILogger<SuckerRodPumpingController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("design")]
        public async Task<ActionResult<SuckerRodPumpDesign>> DesignPumpSystem([FromBody] DesignPumpSystemRequest request)
        {
            var result = await _service.DesignPumpSystemAsync(request.WellUWI, request.WellProperties);
            return Ok(result);
        }

        [HttpPost("analyze-performance")]
        public async Task<ActionResult<SuckerRodPumpPerformance>> AnalyzePerformance([FromBody] AnalyzePerformanceRequest request)
        {
            var result = await _service.AnalyzePerformanceAsync(request.PumpId);
            return Ok(result);
        }

        [HttpPost("design/save")]
        public async Task<ActionResult> SavePumpDesign([FromBody] SuckerRodPumpDesign design)
        {
            var userId = User.ActingUserId();
            await _service.SavePumpDesignAsync(design, userId);
            return Ok(new { message = "Sucker rod pump design saved successfully", designId = design.DesignId });
        }
    }
}

