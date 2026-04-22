using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WellComparison;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using TheTechIdea.Beep.Report;
using WELL = Beep.OilandGas.PPDM39.Models.WELL;
using WELL_STATUS = Beep.OilandGas.PPDM39.Models.WELL_STATUS;

namespace Beep.OilandGas.ApiService.Controllers
{
    /// <summary>
    /// API controller for Well business operations
    /// 
    /// NOTE: For CRUD operations (Create, Read, Update, Delete), please use DataManagementController:
    /// - Get well by UWI: GET /api/datamanagement/WELL?filters=[{"field":"UWI","operator":"equals","value":"{uwi}"}]
    /// - Get well status: GET /api/datamanagement/WELL_STATUS?filters=[{"field":"WELL_ID","operator":"equals","value":"{wellId}"}]
    /// - Create well: POST /api/datamanagement/WELL
    /// - Update well: PUT /api/datamanagement/WELL/{id}
    /// - Delete well: DELETE /api/datamanagement/WELL/{id}
    /// 
    /// This controller focuses on business logic operations like well comparison.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WellController : ControllerBase
    {
        private readonly IWellComparisonService _wellComparisonService;
        private readonly WellServices _wellServices;
        private readonly ILogger<WellController> _logger;

        public WellController(
            IWellComparisonService wellComparisonService,
            WellServices wellServices,
            ILogger<WellController> logger)
        {
            _wellComparisonService = wellComparisonService ?? throw new ArgumentNullException(nameof(wellComparisonService));
            _wellServices = wellServices ?? throw new ArgumentNullException(nameof(wellServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ============================================
        // LEGACY WELL LOOKUP COMPATIBILITY ENDPOINTS
        // ============================================

        /// <summary>
        /// Legacy compatibility route used by well lookup pages.
        /// Returns the active well entity for the given UWI or null when none exists.
        /// </summary>
        [HttpGet("uwi/{uwi}")]
        public async Task<ActionResult<WELL?>> GetByUwi(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });

            try
            {
                var result = await _wellServices.GetByUwiAsync(uwi);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting well by UWI {UWI}", uwi);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Legacy compatibility route used by the well status lookup page.
        /// Returns current STATUS_TYPE -> WELL_STATUS values for the given UWI.
        /// </summary>
        [HttpGet("uwi/{uwi}/status")]
        public async Task<ActionResult<Dictionary<string, WELL_STATUS>>> GetCurrentStatusByUwi(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });

            try
            {
                var result = await _wellServices.GetCurrentWellStatusByUwiAsync(uwi);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current well status for UWI {UWI}", uwi);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        // ============================================
        // WELL COMPARISON ENDPOINTS
        // ============================================

        /// <summary>
        /// Compare multiple wells
        /// </summary>
        [HttpPost("compare")]
        public async Task<ActionResult<WellComparison>> CompareWells([FromBody] CompareWellsRequest request)
        {
            try
            {
                var comparison = await _wellComparisonService.CompareWellsAsync(
                    request.WellIdentifiers, 
                    request.FieldNames);
                return Ok(comparison);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing wells");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Compare wells from multiple data sources
        /// </summary>
        [HttpPost("compare-multi-source")]
        public async Task<ActionResult<WellComparison>> CompareWellsMultiSource([FromBody] CompareWellsMultiSourceRequest request)
        {
            try
            {
                var comparison = await _wellComparisonService.CompareWellsFromMultipleSourcesAsync(
                    request.WellComparisons, 
                    request.FieldNames);
                return Ok(comparison);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing wells from multiple sources");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Get available comparison fields
        /// </summary>
        [HttpGet("comparison-fields")]
        public async Task<ActionResult<List<ComparisonField>>> GetComparisonFields()
        {
            try
            {
                var fields = await _wellComparisonService.GetAvailableComparisonFieldsAsync();
                return Ok(fields);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comparison fields");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }
    }

}



