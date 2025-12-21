using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    /// <summary>
    /// API controller for Development phase operations, field-scoped
    /// </summary>
    [ApiController]
    [Route("api/field/current/development")]
    public class DevelopmentController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly ILogger<DevelopmentController> _logger;

        public DevelopmentController(
            IFieldOrchestrator fieldOrchestrator,
            ILogger<DevelopmentController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _logger = logger;
        }

        /// <summary>
        /// Get all pools for the current field
        /// </summary>
        [HttpGet("pools")]
        public async Task<ActionResult<List<POOL>>> GetPools([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var developmentService = _fieldOrchestrator.GetDevelopmentService();
                var pools = await developmentService.GetPoolsForFieldAsync(currentFieldId, filters);
                
                return Ok(pools);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pools for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get a pool by ID (must belong to current field)
        /// </summary>
        [HttpGet("pools/{id}")]
        public async Task<ActionResult<POOL>> GetPool(string id)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var developmentService = _fieldOrchestrator.GetDevelopmentService();
                var pool = await developmentService.GetPoolForFieldAsync(currentFieldId, id);
                
                if (pool == null)
                {
                    return NotFound(new { error = $"Pool {id} not found or does not belong to current field" });
                }

                return Ok(pool);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pool {PoolId} for current field", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new pool for the current field
        /// </summary>
        [HttpPost("pools")]
        public async Task<ActionResult<POOL>> CreatePool([FromBody] PoolRequest request, [FromQuery] string userId)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new { error = "userId is required" });
                }

                var developmentService = _fieldOrchestrator.GetDevelopmentService();
                var pool = await developmentService.CreatePoolForFieldAsync(currentFieldId, request, userId);
                
                // Get pool ID from the returned POOL object using reflection
                var poolIdProperty = typeof(POOL).GetProperty("POOL_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                var poolId = poolIdProperty?.GetValue(pool)?.ToString() ?? string.Empty;
                
                return CreatedAtAction(nameof(GetPool), new { id = poolId }, pool);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pool for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update a pool (must belong to current field)
        /// </summary>
        [HttpPut("pools/{id}")]
        public async Task<ActionResult<POOL>> UpdatePool(string id, [FromBody] PoolRequest request, [FromQuery] string userId)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new { error = "userId is required" });
                }

                var developmentService = _fieldOrchestrator.GetDevelopmentService();
                var pool = await developmentService.UpdatePoolForFieldAsync(currentFieldId, id, request, userId);
                
                return Ok(pool);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pool {PoolId} for current field", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all development wells for the current field
        /// </summary>
        [HttpGet("wells")]
        public async Task<ActionResult<List<WELL>>> GetDevelopmentWells([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var developmentService = _fieldOrchestrator.GetDevelopmentService();
                var wells = await developmentService.GetDevelopmentWellsForFieldAsync(currentFieldId, filters);
                
                return Ok(wells);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting development wells for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new development well for the current field
        /// </summary>
        [HttpPost("wells")]
        public async Task<ActionResult<WELL>> CreateDevelopmentWell([FromBody] DevelopmentWellRequest request, [FromQuery] string userId)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new { error = "userId is required" });
                }

                var developmentService = _fieldOrchestrator.GetDevelopmentService();
                var well = await developmentService.CreateDevelopmentWellForFieldAsync(currentFieldId, request, userId);
                
                return Ok(well);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating development well for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get wellbores for a well (must belong to current field)
        /// Wellbores are WELL table records with specific well_level_type, linked via WELL_XREF
        /// </summary>
        [HttpGet("wells/{wellId}/wellbores")]
        public async Task<ActionResult<List<WELL>>> GetWellbores(string wellId, [FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var developmentService = _fieldOrchestrator.GetDevelopmentService();
                var wellbores = await developmentService.GetWellboresForWellAsync(currentFieldId, wellId, filters);
                
                return Ok(wellbores);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wellbores for well {WellId} in current field", wellId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all facilities for the current field
        /// </summary>
        [HttpGet("facilities")]
        public async Task<ActionResult<List<FACILITY>>> GetFacilities([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var developmentService = _fieldOrchestrator.GetDevelopmentService();
                var facilities = await developmentService.GetFacilitiesForFieldAsync(currentFieldId, filters);
                
                return Ok(facilities);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facilities for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new facility for the current field
        /// </summary>
        [HttpPost("facilities")]
        public async Task<ActionResult<FACILITY>> CreateFacility([FromBody] FacilityRequest request, [FromQuery] string userId)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new { error = "userId is required" });
                }

                var developmentService = _fieldOrchestrator.GetDevelopmentService();
                var facility = await developmentService.CreateFacilityForFieldAsync(currentFieldId, request, userId);
                
                return Ok(facility);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating facility for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all pipelines for the current field
        /// </summary>
        [HttpGet("pipelines")]
        public async Task<ActionResult<List<PIPELINE>>> GetPipelines([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var developmentService = _fieldOrchestrator.GetDevelopmentService();
                var pipelines = await developmentService.GetPipelinesForFieldAsync(currentFieldId, filters);
                
                return Ok(pipelines);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pipelines for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new pipeline for the current field
        /// </summary>
        [HttpPost("pipelines")]
        public async Task<ActionResult<PIPELINE>> CreatePipeline([FromBody] PipelineRequest request, [FromQuery] string userId)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new { error = "userId is required" });
                }

                var developmentService = _fieldOrchestrator.GetDevelopmentService();
                var pipeline = await developmentService.CreatePipelineForFieldAsync(currentFieldId, request, userId);
                
                return Ok(pipeline);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pipeline for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
