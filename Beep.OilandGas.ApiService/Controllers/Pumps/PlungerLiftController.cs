using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Pumps;
using Beep.OilandGas.Models.Data.PlungerLift;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Pumps
{
    /// <summary>
    /// API controller for plunger lift operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlungerLiftController : ControllerBase
    {
        private readonly IPlungerLiftService _service;
        private readonly ILogger<PlungerLiftController> _logger;

        public PlungerLiftController(IPlungerLiftService service, ILogger<PlungerLiftController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("design")]
        public async Task<ActionResult<PlungerLiftDesign>> DesignPlungerLiftSystem([FromBody] DesignPlungerLiftSystemRequest request)
        {
            var result = await _service.DesignPlungerLiftSystemAsync(request.WellUWI, request.WellProperties);
            return Ok(result);
        }

        [HttpPost("analyze-performance")]
        public async Task<ActionResult<PlungerLiftPerformance>> AnalyzePerformance([FromBody] AnalyzePerformanceRequest request)
        {
            var result = await _service.AnalyzePerformanceAsync(request.WellUWI);
            return Ok(result);
        }

        [HttpPost("design/save")]
        public async Task<ActionResult> SavePlungerLiftDesign([FromBody] PlungerLiftDesign design)
        {
            var userId = User.ActingUserId();
            await _service.SavePlungerLiftDesignAsync(design, userId);
            return Ok(new { message = "Plunger lift design saved successfully", designId = design.DesignId });
        }
    }
}

