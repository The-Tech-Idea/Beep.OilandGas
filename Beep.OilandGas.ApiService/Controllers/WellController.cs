using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Beep.OilandGas.PPDM39.Models;
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
        private readonly ILogger<WellController> _logger;

        public WellController(WellRepository wellRepository, ILogger<WellController> logger)
        {
            _wellRepository = wellRepository;
            _logger = logger;
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
}


