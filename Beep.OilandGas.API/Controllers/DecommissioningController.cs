using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Decommissioning.Services;

namespace Beep.OilandGas.API.Controllers
{
    /// <summary>
    /// API controller for decommissioning operations.
    /// </summary>
    [ApiController]
    [Route("api/v1/decommissioning")]
    [Produces("application/json")]
    public class DecommissioningController : ControllerBase
    {
        private readonly IWellPluggingService _pluggingService;
        private readonly ILogger<DecommissioningController> _logger;

        public DecommissioningController(
            IWellPluggingService pluggingService,
            ILogger<DecommissioningController> logger)
        {
            _pluggingService = pluggingService ?? throw new ArgumentNullException(nameof(pluggingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all well plugging operations.
        /// </summary>
        /// <param name="wellUWI">Optional well UWI to filter operations.</param>
        /// <returns>List of well plugging operations.</returns>
        [HttpGet("well-plugging")]
        [ProducesResponseType(typeof(List<WellPluggingDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<WellPluggingDto>>> GetWellPluggingOperations([FromQuery] string? wellUWI = null)
        {
            try
            {
                var operations = await _pluggingService.GetWellPluggingOperationsAsync(wellUWI);
                return Ok(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting well plugging operations");
                return StatusCode(500, new { error = "An error occurred while retrieving well plugging operations." });
            }
        }

        /// <summary>
        /// Gets a well plugging operation by ID.
        /// </summary>
        /// <param name="id">Plugging ID.</param>
        /// <returns>Well plugging operation details.</returns>
        [HttpGet("well-plugging/{id}")]
        [ProducesResponseType(typeof(WellPluggingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WellPluggingDto>> GetWellPluggingOperation(string id)
        {
            try
            {
                var operation = await _pluggingService.GetWellPluggingOperationAsync(id);
                if (operation == null)
                    return NotFound(new { error = $"Well plugging operation with ID {id} not found." });

                return Ok(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting well plugging operation {PluggingId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the well plugging operation." });
            }
        }

        /// <summary>
        /// Creates a new well plugging operation.
        /// </summary>
        /// <param name="createDto">Well plugging creation data.</param>
        /// <returns>Created well plugging operation.</returns>
        [HttpPost("well-plugging")]
        [ProducesResponseType(typeof(WellPluggingDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WellPluggingDto>> CreateWellPlugging([FromBody] CreateWellPluggingDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var operation = await _pluggingService.CreateWellPluggingOperationAsync(createDto);
                return CreatedAtAction(nameof(GetWellPluggingOperation), new { id = operation.PluggingId }, operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating well plugging operation");
                return StatusCode(500, new { error = "An error occurred while creating the well plugging operation." });
            }
        }

        /// <summary>
        /// Verifies well plugging.
        /// </summary>
        /// <param name="id">Plugging ID.</param>
        /// <param name="request">Verification request.</param>
        /// <returns>Verified well plugging operation.</returns>
        [HttpPost("well-plugging/{id}/verify")]
        [ProducesResponseType(typeof(WellPluggingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WellPluggingDto>> VerifyWellPlugging(string id, [FromBody] VerifyPluggingRequest request)
        {
            try
            {
                var operation = await _pluggingService.VerifyWellPluggingAsync(id, request.VerifiedBy, request.Passed);
                return Ok(operation);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Well plugging operation with ID {id} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying well plugging {PluggingId}", id);
                return StatusCode(500, new { error = "An error occurred while verifying the well plugging." });
            }
        }

        /// <summary>
        /// Gets facility decommissioning operations.
        /// </summary>
        /// <param name="facilityId">Optional facility ID to filter operations.</param>
        /// <returns>List of facility decommissioning operations.</returns>
        [HttpGet("facility-decommissioning")]
        [ProducesResponseType(typeof(List<FacilityDecommissioningDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<FacilityDecommissioningDto>>> GetFacilityDecommissioningOperations([FromQuery] string? facilityId = null)
        {
            try
            {
                var operations = await _pluggingService.GetFacilityDecommissioningOperationsAsync(facilityId);
                return Ok(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facility decommissioning operations");
                return StatusCode(500, new { error = "An error occurred while retrieving facility decommissioning operations." });
            }
        }

        /// <summary>
        /// Gets site restoration operations.
        /// </summary>
        /// <param name="siteId">Optional site ID to filter operations.</param>
        /// <returns>List of site restoration operations.</returns>
        [HttpGet("site-restoration")]
        [ProducesResponseType(typeof(List<SiteRestorationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SiteRestorationDto>>> GetSiteRestorationOperations([FromQuery] string? siteId = null)
        {
            try
            {
                var operations = await _pluggingService.GetSiteRestorationOperationsAsync(siteId);
                return Ok(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting site restoration operations");
                return StatusCode(500, new { error = "An error occurred while retrieving site restoration operations." });
            }
        }

        /// <summary>
        /// Gets abandonment operations.
        /// </summary>
        /// <param name="wellUWI">Optional well UWI to filter operations.</param>
        /// <returns>List of abandonment operations.</returns>
        [HttpGet("abandonment")]
        [ProducesResponseType(typeof(List<AbandonmentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AbandonmentDto>>> GetAbandonmentOperations([FromQuery] string? wellUWI = null)
        {
            try
            {
                var operations = await _pluggingService.GetAbandonmentOperationsAsync(wellUWI);
                return Ok(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting abandonment operations");
                return StatusCode(500, new { error = "An error occurred while retrieving abandonment operations." });
            }
        }
    }

    /// <summary>
    /// Request model for verifying well plugging.
    /// </summary>
    public class VerifyPluggingRequest
    {
        public string VerifiedBy { get; set; } = string.Empty;
        public bool Passed { get; set; }
    }
}

