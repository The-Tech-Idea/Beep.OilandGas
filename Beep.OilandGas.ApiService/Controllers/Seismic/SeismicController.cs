using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.DataManagement.Services.Seismic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Controllers.Seismic
{
    /// <summary>
    /// API controller for Seismic operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SeismicController : ControllerBase
    {
        private readonly IPPDMSeismicService _seismicService;
        private readonly ILogger<SeismicController> _logger;

        public SeismicController(
            IPPDMSeismicService seismicService,
            ILogger<SeismicController> logger)
        {
            _seismicService = seismicService;
            _logger = logger;
        }

        /// <summary>
        /// Get seismic surveys
        /// </summary>
        [HttpGet("surveys")]
        public async Task<ActionResult<List<object>>> GetSeismicSurveys([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var surveys = await _seismicService.GetSeismicSurveysAsync(filters);
                return Ok(surveys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seismic surveys");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get seismic acquisition data
        /// </summary>
        [HttpGet("acquisition")]
        public async Task<ActionResult<List<object>>> GetSeismicAcquisition([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var acquisition = await _seismicService.GetSeismicAcquisitionAsync(filters);
                return Ok(acquisition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seismic acquisition data");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get seismic processing data
        /// </summary>
        [HttpGet("processing")]
        public async Task<ActionResult<List<object>>> GetSeismicProcessing([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var processing = await _seismicService.GetSeismicProcessingAsync(filters);
                return Ok(processing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seismic processing data");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}


