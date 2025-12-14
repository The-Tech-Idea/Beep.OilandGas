using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.EnhancedRecovery.Services;

namespace Beep.OilandGas.API.Controllers
{
    /// <summary>
    /// API controller for enhanced recovery operations.
    /// </summary>
    [ApiController]
    [Route("api/v1/enhanced-recovery")]
    [Produces("application/json")]
    public class EnhancedRecoveryController : ControllerBase
    {
        private readonly IEnhancedRecoveryService _eorService;
        private readonly ILogger<EnhancedRecoveryController> _logger;

        public EnhancedRecoveryController(
            IEnhancedRecoveryService eorService,
            ILogger<EnhancedRecoveryController> logger)
        {
            _eorService = eorService ?? throw new ArgumentNullException(nameof(eorService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all enhanced recovery operations.
        /// </summary>
        /// <param name="fieldId">Optional field ID to filter operations.</param>
        /// <returns>List of enhanced recovery operations.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<EnhancedRecoveryOperationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<EnhancedRecoveryOperationDto>>> GetEnhancedRecoveryOperations([FromQuery] string? fieldId = null)
        {
            try
            {
                var operations = await _eorService.GetEnhancedRecoveryOperationsAsync(fieldId);
                return Ok(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enhanced recovery operations");
                return StatusCode(500, new { error = "An error occurred while retrieving enhanced recovery operations." });
            }
        }

        /// <summary>
        /// Gets an enhanced recovery operation by ID.
        /// </summary>
        /// <param name="id">Operation ID.</param>
        /// <returns>Enhanced recovery operation details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EnhancedRecoveryOperationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EnhancedRecoveryOperationDto>> GetEnhancedRecoveryOperation(string id)
        {
            try
            {
                var operation = await _eorService.GetEnhancedRecoveryOperationAsync(id);
                if (operation == null)
                    return NotFound(new { error = $"Enhanced recovery operation with ID {id} not found." });

                return Ok(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enhanced recovery operation {OperationId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the enhanced recovery operation." });
            }
        }

        /// <summary>
        /// Creates a new enhanced recovery operation.
        /// </summary>
        /// <param name="createDto">Enhanced recovery operation creation data.</param>
        /// <returns>Created enhanced recovery operation.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(EnhancedRecoveryOperationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<EnhancedRecoveryOperationDto>> CreateEnhancedRecoveryOperation([FromBody] CreateEnhancedRecoveryOperationDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var operation = await _eorService.CreateEnhancedRecoveryOperationAsync(createDto);
                return CreatedAtAction(nameof(GetEnhancedRecoveryOperation), new { id = operation.OperationId }, operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating enhanced recovery operation");
                return StatusCode(500, new { error = "An error occurred while creating the enhanced recovery operation." });
            }
        }

        /// <summary>
        /// Gets injection operations.
        /// </summary>
        /// <param name="wellUWI">Optional well UWI to filter operations.</param>
        /// <returns>List of injection operations.</returns>
        [HttpGet("injection")]
        [ProducesResponseType(typeof(List<InjectionOperationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<InjectionOperationDto>>> GetInjectionOperations([FromQuery] string? wellUWI = null)
        {
            try
            {
                var operations = await _eorService.GetInjectionOperationsAsync(wellUWI);
                return Ok(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting injection operations");
                return StatusCode(500, new { error = "An error occurred while retrieving injection operations." });
            }
        }

        /// <summary>
        /// Gets water flooding operations.
        /// </summary>
        /// <param name="fieldId">Optional field ID to filter operations.</param>
        /// <returns>List of water flooding operations.</returns>
        [HttpGet("water-flooding")]
        [ProducesResponseType(typeof(List<WaterFloodingDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<WaterFloodingDto>>> GetWaterFloodingOperations([FromQuery] string? fieldId = null)
        {
            try
            {
                var operations = await _eorService.GetWaterFloodingOperationsAsync(fieldId);
                return Ok(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting water flooding operations");
                return StatusCode(500, new { error = "An error occurred while retrieving water flooding operations." });
            }
        }

        /// <summary>
        /// Gets gas injection operations.
        /// </summary>
        /// <param name="fieldId">Optional field ID to filter operations.</param>
        /// <returns>List of gas injection operations.</returns>
        [HttpGet("gas-injection")]
        [ProducesResponseType(typeof(List<GasInjectionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GasInjectionDto>>> GetGasInjectionOperations([FromQuery] string? fieldId = null)
        {
            try
            {
                var operations = await _eorService.GetGasInjectionOperationsAsync(fieldId);
                return Ok(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting gas injection operations");
                return StatusCode(500, new { error = "An error occurred while retrieving gas injection operations." });
            }
        }
    }
}

