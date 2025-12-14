using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.ProspectIdentification.Services;

namespace Beep.OilandGas.API.Controllers
{
    /// <summary>
    /// API controller for prospect identification and evaluation.
    /// </summary>
    [ApiController]
    [Route("api/v1/prospects")]
    [Produces("application/json")]
    public class ProspectsController : ControllerBase
    {
        private readonly IProspectEvaluationService _prospectEvaluationService;
        private readonly ISeismicAnalysisService _seismicAnalysisService;
        private readonly ILogger<ProspectsController> _logger;

        public ProspectsController(
            IProspectEvaluationService prospectEvaluationService,
            ISeismicAnalysisService seismicAnalysisService,
            ILogger<ProspectsController> logger)
        {
            _prospectEvaluationService = prospectEvaluationService ?? throw new ArgumentNullException(nameof(prospectEvaluationService));
            _seismicAnalysisService = seismicAnalysisService ?? throw new ArgumentNullException(nameof(seismicAnalysisService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all prospects.
        /// </summary>
        /// <param name="fieldId">Optional field ID to filter prospects.</param>
        /// <returns>List of prospects.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProspectDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProspectDto>>> GetProspects([FromQuery] string? fieldId = null)
        {
            try
            {
                var prospects = await _prospectEvaluationService.GetProspectsAsync(fieldId);
                return Ok(prospects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prospects");
                return StatusCode(500, new { error = "An error occurred while retrieving prospects." });
            }
        }

        /// <summary>
        /// Gets a prospect by ID.
        /// </summary>
        /// <param name="id">Prospect ID.</param>
        /// <returns>Prospect details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProspectDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProspectDto>> GetProspect(string id)
        {
            try
            {
                var prospect = await _prospectEvaluationService.GetProspectAsync(id);
                if (prospect == null)
                    return NotFound(new { error = $"Prospect with ID {id} not found." });

                return Ok(prospect);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prospect {ProspectId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the prospect." });
            }
        }

        /// <summary>
        /// Creates a new prospect.
        /// </summary>
        /// <param name="createDto">Prospect creation data.</param>
        /// <returns>Created prospect.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProspectDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProspectDto>> CreateProspect([FromBody] CreateProspectDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var prospect = await _prospectEvaluationService.CreateProspectAsync(createDto);
                return CreatedAtAction(nameof(GetProspect), new { id = prospect.ProspectId }, prospect);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prospect");
                return StatusCode(500, new { error = "An error occurred while creating the prospect." });
            }
        }

        /// <summary>
        /// Updates a prospect.
        /// </summary>
        /// <param name="id">Prospect ID.</param>
        /// <param name="updateDto">Prospect update data.</param>
        /// <returns>Updated prospect.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProspectDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProspectDto>> UpdateProspect(string id, [FromBody] UpdateProspectDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var prospect = await _prospectEvaluationService.UpdateProspectAsync(id, updateDto);
                return Ok(prospect);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Prospect with ID {id} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prospect {ProspectId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the prospect." });
            }
        }

        /// <summary>
        /// Deletes a prospect.
        /// </summary>
        /// <param name="id">Prospect ID.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProspect(string id)
        {
            try
            {
                await _prospectEvaluationService.DeleteProspectAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Prospect with ID {id} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prospect {ProspectId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the prospect." });
            }
        }

        /// <summary>
        /// Evaluates a prospect.
        /// </summary>
        /// <param name="id">Prospect ID.</param>
        /// <returns>Evaluation results.</returns>
        [HttpPost("{id}/evaluate")]
        [ProducesResponseType(typeof(ProspectEvaluationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProspectEvaluationDto>> EvaluateProspect(string id)
        {
            try
            {
                var evaluation = await _prospectEvaluationService.EvaluateProspectAsync(id);
                return Ok(evaluation);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Prospect with ID {id} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating prospect {ProspectId}", id);
                return StatusCode(500, new { error = "An error occurred while evaluating the prospect." });
            }
        }

        /// <summary>
        /// Gets seismic surveys for a prospect.
        /// </summary>
        /// <param name="id">Prospect ID.</param>
        /// <returns>List of seismic surveys.</returns>
        [HttpGet("{id}/seismic-data")]
        [ProducesResponseType(typeof(List<SeismicSurveyDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SeismicSurveyDto>>> GetSeismicData(string id)
        {
            try
            {
                var surveys = await _seismicAnalysisService.GetSeismicSurveysAsync(id);
                return Ok(surveys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seismic data for prospect {ProspectId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving seismic data." });
            }
        }

        /// <summary>
        /// Analyzes seismic data for a survey.
        /// </summary>
        /// <param name="id">Prospect ID.</param>
        /// <param name="surveyId">Seismic survey ID.</param>
        /// <returns>Analysis results.</returns>
        [HttpPost("{id}/seismic-analysis")]
        [ProducesResponseType(typeof(SeismicAnalysisResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SeismicAnalysisResult>> AnalyzeSeismicData(string id, [FromQuery] string surveyId)
        {
            try
            {
                var result = await _seismicAnalysisService.AnalyzeSeismicDataAsync(surveyId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Seismic survey with ID {surveyId} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing seismic data for survey {SurveyId}", surveyId);
                return StatusCode(500, new { error = "An error occurred while analyzing seismic data." });
            }
        }

        /// <summary>
        /// Gets risk assessment for a prospect.
        /// </summary>
        /// <param name="id">Prospect ID.</param>
        /// <returns>Risk assessment.</returns>
        [HttpGet("{id}/risk-assessment")]
        [ProducesResponseType(typeof(ProspectEvaluationDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProspectEvaluationDto>> GetRiskAssessment(string id)
        {
            try
            {
                var evaluation = await _prospectEvaluationService.EvaluateProspectAsync(id);
                return Ok(evaluation);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Prospect with ID {id} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting risk assessment for prospect {ProspectId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving risk assessment." });
            }
        }
    }
}

