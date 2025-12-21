using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Core.DTOs;
using TheTechIdea.Beep.Report;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.ApiService.Controllers
{
    /// <summary>
    /// API controller for Well operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WellController : ControllerBase
    {
        private readonly WellRepository _wellRepository;
        private readonly IWellComparisonService _wellComparisonService;
        private readonly ILogger<WellController> _logger;

        public WellController(
            WellRepository wellRepository, 
            IWellComparisonService wellComparisonService,
            ILogger<WellController> logger)
        {
            _wellRepository = wellRepository ?? throw new ArgumentNullException(nameof(wellRepository));
            _wellComparisonService = wellComparisonService ?? throw new ArgumentNullException(nameof(wellComparisonService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get well by UWI
        /// </summary>
        [HttpGet("uwi/{uwi}")]
        public async Task<ActionResult<WELL>> GetByUwi(string uwi)
        {
            try
            {
                var well = await _wellRepository.GetByUwiAsync(uwi);
                if (well == null)
                    return NotFound(new { message = $"Well with UWI '{uwi}' not found" });
                
                return Ok(well);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting well by UWI: {UWI}", uwi);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get well status by UWI
        /// </summary>
        [HttpGet("uwi/{uwi}/status")]
        public async Task<ActionResult<Dictionary<string, WELL_STATUS>>> GetWellStatus(string uwi)
        {
            try
            {
                var status = await _wellRepository.GetCurrentWellStatusByUwiAsync(uwi);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting well status for UWI: {UWI}", uwi);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new well
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<WELL>> CreateWell([FromBody] CreateWellRequest request)
        {
            try
            {
                var well = await _wellRepository.CreateWellAsync(
                    request.Well, 
                    request.UserId, 
                    request.InitializeDefaultStatuses);
                return CreatedAtAction(nameof(GetByUwi), new { uwi = well.UWI }, well);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating well");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update a well
        /// </summary>
        [HttpPut("uwi/{uwi}")]
        public async Task<ActionResult<WELL>> UpdateWell(string uwi, [FromBody] UpdateWellRequest request)
        {
            try
            {
                var well = await _wellRepository.UpdateWellAsync(request.Well, request.UserId);
                return Ok(well);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating well: {UWI}", uwi);
                return StatusCode(500, new { error = ex.Message });
            }
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

    public class CreateWellRequest
    {
        [Required]
        public WELL Well { get; set; } = null!;
        
        [Required]
        public string UserId { get; set; } = null!;
        
        public bool InitializeDefaultStatuses { get; set; } = true;
    }

    public class UpdateWellRequest
    {
        [Required]
        public WELL Well { get; set; } = null!;
        
        [Required]
        public string UserId { get; set; } = null!;
    }

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



