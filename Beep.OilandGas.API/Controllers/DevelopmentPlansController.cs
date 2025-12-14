using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.DevelopmentPlanning.Services;

namespace Beep.OilandGas.API.Controllers
{
    /// <summary>
    /// API controller for development planning.
    /// </summary>
    [ApiController]
    [Route("api/v1/development-plans")]
    [Produces("application/json")]
    public class DevelopmentPlansController : ControllerBase
    {
        private readonly IDevelopmentPlanService _planService;
        private readonly ILogger<DevelopmentPlansController> _logger;

        public DevelopmentPlansController(
            IDevelopmentPlanService planService,
            ILogger<DevelopmentPlansController> logger)
        {
            _planService = planService ?? throw new ArgumentNullException(nameof(planService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all development plans.
        /// </summary>
        /// <param name="fieldId">Optional field ID to filter plans.</param>
        /// <returns>List of development plans.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<DevelopmentPlanDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DevelopmentPlanDto>>> GetDevelopmentPlans([FromQuery] string? fieldId = null)
        {
            try
            {
                var plans = await _planService.GetDevelopmentPlansAsync(fieldId);
                return Ok(plans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting development plans");
                return StatusCode(500, new { error = "An error occurred while retrieving development plans." });
            }
        }

        /// <summary>
        /// Gets a development plan by ID.
        /// </summary>
        /// <param name="id">Plan ID.</param>
        /// <returns>Development plan details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DevelopmentPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DevelopmentPlanDto>> GetDevelopmentPlan(string id)
        {
            try
            {
                var plan = await _planService.GetDevelopmentPlanAsync(id);
                if (plan == null)
                    return NotFound(new { error = $"Development plan with ID {id} not found." });

                return Ok(plan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting development plan {PlanId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the development plan." });
            }
        }

        /// <summary>
        /// Creates a new development plan.
        /// </summary>
        /// <param name="createDto">Development plan creation data.</param>
        /// <returns>Created development plan.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(DevelopmentPlanDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DevelopmentPlanDto>> CreateDevelopmentPlan([FromBody] CreateDevelopmentPlanDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var plan = await _planService.CreateDevelopmentPlanAsync(createDto);
                return CreatedAtAction(nameof(GetDevelopmentPlan), new { id = plan.PlanId }, plan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating development plan");
                return StatusCode(500, new { error = "An error occurred while creating the development plan." });
            }
        }

        /// <summary>
        /// Updates a development plan.
        /// </summary>
        /// <param name="id">Plan ID.</param>
        /// <param name="updateDto">Development plan update data.</param>
        /// <returns>Updated development plan.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DevelopmentPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DevelopmentPlanDto>> UpdateDevelopmentPlan(string id, [FromBody] UpdateDevelopmentPlanDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var plan = await _planService.UpdateDevelopmentPlanAsync(id, updateDto);
                return Ok(plan);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Development plan with ID {id} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating development plan {PlanId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the development plan." });
            }
        }

        /// <summary>
        /// Approves a development plan.
        /// </summary>
        /// <param name="id">Plan ID.</param>
        /// <param name="approvedBy">User who approved the plan.</param>
        /// <returns>Approved development plan.</returns>
        [HttpPost("{id}/approve")]
        [ProducesResponseType(typeof(DevelopmentPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DevelopmentPlanDto>> ApproveDevelopmentPlan(string id, [FromBody] string approvedBy)
        {
            try
            {
                var plan = await _planService.ApproveDevelopmentPlanAsync(id, approvedBy);
                return Ok(plan);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Development plan with ID {id} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving development plan {PlanId}", id);
                return StatusCode(500, new { error = "An error occurred while approving the development plan." });
            }
        }

        /// <summary>
        /// Gets well plans for a development plan.
        /// </summary>
        /// <param name="id">Plan ID.</param>
        /// <returns>List of well plans.</returns>
        [HttpGet("{id}/well-plans")]
        [ProducesResponseType(typeof(List<WellPlanDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<WellPlanDto>>> GetWellPlans(string id)
        {
            try
            {
                var wellPlans = await _planService.GetWellPlansAsync(id);
                return Ok(wellPlans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting well plans for plan {PlanId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving well plans." });
            }
        }

        /// <summary>
        /// Gets facility plans for a development plan.
        /// </summary>
        /// <param name="id">Plan ID.</param>
        /// <returns>List of facility plans.</returns>
        [HttpGet("{id}/facility-plans")]
        [ProducesResponseType(typeof(List<FacilityPlanDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<FacilityPlanDto>>> GetFacilityPlans(string id)
        {
            try
            {
                var facilityPlans = await _planService.GetFacilityPlansAsync(id);
                return Ok(facilityPlans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facility plans for plan {PlanId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving facility plans." });
            }
        }

        /// <summary>
        /// Gets permit applications for a development plan.
        /// </summary>
        /// <param name="id">Plan ID.</param>
        /// <returns>List of permit applications.</returns>
        [HttpGet("{id}/permits")]
        [ProducesResponseType(typeof(List<PermitApplicationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PermitApplicationDto>>> GetPermitApplications(string id)
        {
            try
            {
                var permits = await _planService.GetPermitApplicationsAsync(id);
                return Ok(permits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permit applications for plan {PlanId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving permit applications." });
            }
        }
    }
}

