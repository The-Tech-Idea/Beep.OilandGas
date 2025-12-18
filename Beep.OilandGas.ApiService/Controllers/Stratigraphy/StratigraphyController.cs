using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.DataManagement.Services.Stratigraphy;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.ApiService.Controllers.Stratigraphy
{
    /// <summary>
    /// API controller for Stratigraphy operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StratigraphyController : ControllerBase
    {
        private readonly IPPDMStratigraphyService _stratigraphyService;
        private readonly ILogger<StratigraphyController> _logger;

        public StratigraphyController(
            IPPDMStratigraphyService stratigraphyService,
            ILogger<StratigraphyController> logger)
        {
            _stratigraphyService = stratigraphyService;
            _logger = logger;
        }

        /// <summary>
        /// Get stratigraphic columns
        /// </summary>
        [HttpGet("columns")]
        public async Task<ActionResult<List<STRAT_COLUMN>>> GetStratigraphicColumns([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var columns = await _stratigraphyService.GetStratigraphicColumnsAsync(filters);
                return Ok(columns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stratigraphic columns");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get stratigraphic column by ID
        /// </summary>
        [HttpGet("columns/{columnId}")]
        public async Task<ActionResult<STRAT_COLUMN>> GetStratigraphicColumnById(string columnId)
        {
            try
            {
                var column = await _stratigraphyService.GetStratigraphicColumnByIdAsync(columnId);
                if (column == null)
                    return NotFound(new { message = $"Stratigraphic column '{columnId}' not found" });
                
                return Ok(column);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stratigraphic column: {ColumnId}", columnId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get stratigraphic units
        /// </summary>
        [HttpGet("units")]
        public async Task<ActionResult<List<STRAT_UNIT>>> GetStratigraphicUnits([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var units = await _stratigraphyService.GetStratigraphicUnitsAsync(filters);
                return Ok(units);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stratigraphic units");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get stratigraphic unit by ID
        /// </summary>
        [HttpGet("units/{unitId}")]
        public async Task<ActionResult<STRAT_UNIT>> GetStratigraphicUnitById(string unitId)
        {
            try
            {
                var unit = await _stratigraphyService.GetStratigraphicUnitByIdAsync(unitId);
                if (unit == null)
                    return NotFound(new { message = $"Stratigraphic unit '{unitId}' not found" });
                
                return Ok(unit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stratigraphic unit: {UnitId}", unitId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get stratigraphic hierarchy
        /// </summary>
        [HttpGet("hierarchy")]
        public async Task<ActionResult<List<STRAT_HIERARCHY>>> GetStratigraphicHierarchy([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var hierarchy = await _stratigraphyService.GetStratigraphicHierarchyAsync(filters);
                return Ok(hierarchy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stratigraphic hierarchy");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get well sections
        /// </summary>
        [HttpGet("well-sections")]
        public async Task<ActionResult<List<STRAT_WELL_SECTION>>> GetWellSections([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var sections = await _stratigraphyService.GetWellSectionsAsync(filters);
                return Ok(sections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting well sections");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get well sections by UWI
        /// </summary>
        [HttpGet("well-sections/well/{uwi}")]
        public async Task<ActionResult<List<STRAT_WELL_SECTION>>> GetWellSectionsByWell(string uwi)
        {
            try
            {
                var sections = await _stratigraphyService.GetWellSectionsByWellAsync(uwi);
                return Ok(sections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting well sections for UWI: {UWI}", uwi);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new stratigraphic column
        /// </summary>
        [HttpPost("columns")]
        public async Task<ActionResult<STRAT_COLUMN>> CreateStratigraphicColumn([FromBody] CreateStratColumnRequest request)
        {
            try
            {
                var column = await _stratigraphyService.CreateStratigraphicColumnAsync(request.Column, request.UserId);
                return CreatedAtAction(nameof(GetStratigraphicColumnById), new { columnId = column.STRAT_COLUMN_ID }, column);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stratigraphic column");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update a stratigraphic column
        /// </summary>
        [HttpPut("columns/{columnId}")]
        public async Task<ActionResult<STRAT_COLUMN>> UpdateStratigraphicColumn(string columnId, [FromBody] UpdateStratColumnRequest request)
        {
            try
            {
                var column = await _stratigraphyService.UpdateStratigraphicColumnAsync(request.Column, request.UserId);
                return Ok(column);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stratigraphic column: {ColumnId}", columnId);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Request DTOs
    public class CreateStratColumnRequest
    {
        [Required]
        public STRAT_COLUMN Column { get; set; } = null!;
        
        [Required]
        public string UserId { get; set; } = null!;
    }

    public class UpdateStratColumnRequest
    {
        [Required]
        public STRAT_COLUMN Column { get; set; } = null!;
        
        [Required]
        public string UserId { get; set; } = null!;
    }
}



