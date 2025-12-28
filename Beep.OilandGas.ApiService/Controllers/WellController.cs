using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.DTOs;
using TheTechIdea.Beep.Report;

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
        private readonly ILogger<WellController> _logger;

        public WellController(
            IWellComparisonService wellComparisonService,
            ILogger<WellController> logger)
        {
            _wellComparisonService = wellComparisonService ?? throw new ArgumentNullException(nameof(wellComparisonService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ============================================
        // WELL COMPARISON ENDPOINTS
        // ============================================

        /// <summary>
        /// Compare multiple wells
        /// </summary>
        [HttpPost("compare")]
        public async Task<ActionResult<WellComparisonDTO>> CompareWells([FromBody] CompareWellsRequest request)
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
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Compare wells from multiple data sources
        /// </summary>
        [HttpPost("compare-multi-source")]
        public async Task<ActionResult<WellComparisonDTO>> CompareWellsMultiSource([FromBody] CompareWellsMultiSourceRequest request)
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
                return StatusCode(500, new { error = ex.Message });
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
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // ============================================
    // REQUEST DTOs
    // ============================================

    public class CompareWellsRequest
    {
        public List<string> WellIdentifiers { get; set; } = new List<string>();
        public List<string>? FieldNames { get; set; }
    }

    public class CompareWellsMultiSourceRequest
    {
        public List<WellSourceMapping> WellComparisons { get; set; } = new List<WellSourceMapping>();
        public List<string>? FieldNames { get; set; }
    }
}



