using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.DataManagement.Services.Production;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Controllers.Production
{
    /// <summary>
    /// API controller for Production & Reserves operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionController : ControllerBase
    {
        private readonly IPPDMProductionService _productionService;
        private readonly ILogger<ProductionController> _logger;

        public ProductionController(
            IPPDMProductionService productionService,
            ILogger<ProductionController> logger)
        {
            _productionService = productionService;
            _logger = logger;
        }

        /// <summary>
        /// Get fields
        /// </summary>
        [HttpGet("fields")]
        public async Task<ActionResult<List<object>>> GetFields([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var fields = await _productionService.GetFieldsAsync(filters);
                return Ok(fields);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fields");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get pools
        /// </summary>
        [HttpGet("pools")]
        public async Task<ActionResult<List<object>>> GetPools([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var pools = await _productionService.GetPoolsAsync(filters);
                return Ok(pools);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pools");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get production data
        /// </summary>
        [HttpGet("production")]
        public async Task<ActionResult<List<object>>> GetProduction([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var production = await _productionService.GetProductionAsync(filters);
                return Ok(production);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production data");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get reserves data
        /// </summary>
        [HttpGet("reserves")]
        public async Task<ActionResult<List<object>>> GetReserves([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var reserves = await _productionService.GetReservesAsync(filters);
                return Ok(reserves);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reserves data");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get production reporting data
        /// </summary>
        [HttpGet("reporting")]
        public async Task<ActionResult<List<object>>> GetProductionReporting([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var reporting = await _productionService.GetProductionReportingAsync(filters);
                return Ok(reporting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production reporting data");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}



