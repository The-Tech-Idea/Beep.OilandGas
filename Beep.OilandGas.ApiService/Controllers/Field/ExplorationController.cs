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
    /// API controller for Exploration phase operations, field-scoped
    /// </summary>
    [ApiController]
    [Route("api/field/current/exploration")]
    public class ExplorationController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly ILogger<ExplorationController> _logger;

        public ExplorationController(
            IFieldOrchestrator fieldOrchestrator,
            ILogger<ExplorationController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _logger = logger;
        }

        /// <summary>
        /// Get all prospects for the current field
        /// </summary>
        [HttpGet("prospects")]
        public async Task<ActionResult<List<PROSPECT>>> GetProspects([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var explorationService = _fieldOrchestrator.GetExplorationService();
                var prospects = await explorationService.GetProspectsForFieldAsync(currentFieldId, filters);
                
                return Ok(prospects);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prospects for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get a prospect by ID (must belong to current field)
        /// </summary>
        [HttpGet("prospects/{id}")]
        public async Task<ActionResult<PROSPECT>> GetProspect(string id)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var explorationService = _fieldOrchestrator.GetExplorationService();
                var prospect = await explorationService.GetProspectForFieldAsync(currentFieldId, id);
                
                if (prospect == null)
                {
                    return NotFound(new { error = $"Prospect {id} not found or does not belong to current field" });
                }

                return Ok(prospect);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prospect {ProspectId} for current field", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new prospect for the current field
        /// </summary>
        [HttpPost("prospects")]
        public async Task<ActionResult<PROSPECT>> CreateProspect([FromBody] ProspectRequest request, [FromQuery] string userId)
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

                var explorationService = _fieldOrchestrator.GetExplorationService();
                var prospect = await explorationService.CreateProspectForFieldAsync(currentFieldId, request, userId);
                
                // Get prospect ID from the returned PROSPECT object
                var prospectIdProperty = prospect.GetType().GetProperty("PROSPECT_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                var prospectId = prospectIdProperty?.GetValue(prospect)?.ToString() ?? string.Empty;
                
                return CreatedAtAction(nameof(GetProspect), new { id = prospectId }, prospect);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prospect for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update a prospect (must belong to current field)
        /// </summary>
        [HttpPut("prospects/{id}")]
        public async Task<ActionResult<PROSPECT>> UpdateProspect(string id, [FromBody] ProspectRequest request, [FromQuery] string userId)
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

                var explorationService = _fieldOrchestrator.GetExplorationService();
                var prospect = await explorationService.UpdateProspectForFieldAsync(currentFieldId, id, request, userId);
                
                return Ok(prospect);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prospect {ProspectId} for current field", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Soft delete a prospect (must belong to current field)
        /// </summary>
        [HttpDelete("prospects/{id}")]
        public async Task<ActionResult> DeleteProspect(string id, [FromQuery] string userId)
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

                var explorationService = _fieldOrchestrator.GetExplorationService();
                var success = await explorationService.DeleteProspectForFieldAsync(currentFieldId, id, userId);
                
                if (!success)
                {
                    return NotFound(new { error = $"Prospect {id} not found or could not be deleted" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prospect {ProspectId} for current field", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all seismic surveys for the current field
        /// </summary>
        [HttpGet("seismic-surveys")]
        public async Task<ActionResult<List<SEIS_ACQTN_SURVEY>>> GetSeismicSurveys([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var explorationService = _fieldOrchestrator.GetExplorationService();
                var surveys = await explorationService.GetSeismicSurveysForFieldAsync(currentFieldId, filters);
                
                return Ok(surveys);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seismic surveys for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new seismic survey for the current field
        /// </summary>
        [HttpPost("seismic-surveys")]
        public async Task<ActionResult<SEIS_ACQTN_SURVEY>> CreateSeismicSurvey([FromBody] SeismicSurveyRequest request, [FromQuery] string userId)
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

                var explorationService = _fieldOrchestrator.GetExplorationService();
                var survey = await explorationService.CreateSeismicSurveyForFieldAsync(currentFieldId, request, userId);
                
                return Ok(survey);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating seismic survey for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get seismic lines for a seismic survey
        /// </summary>
        [HttpGet("seismic-surveys/{surveyId}/lines")]
        public async Task<ActionResult<List<SEIS_LINE>>> GetSeismicLines(string surveyId, [FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var explorationService = _fieldOrchestrator.GetExplorationService();
                var lines = await explorationService.GetSeismicLinesForSurveyAsync(surveyId, filters);
                
                return Ok(lines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seismic lines for survey {SurveyId}", surveyId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all exploratory wells for the current field
        /// </summary>
        [HttpGet("exploratory-wells")]
        public async Task<ActionResult<List<WELL>>> GetExploratoryWells([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var explorationService = _fieldOrchestrator.GetExplorationService();
                var wells = await explorationService.GetExploratoryWellsForFieldAsync(currentFieldId, filters);
                
                return Ok(wells);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exploratory wells for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
