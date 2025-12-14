using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.DrillingAndConstruction.Services;

namespace Beep.OilandGas.API.Controllers
{
    /// <summary>
    /// API controller for drilling operations.
    /// </summary>
    [ApiController]
    [Route("api/v1/drilling-operations")]
    [Produces("application/json")]
    public class DrillingOperationsController : ControllerBase
    {
        private readonly IDrillingOperationService _drillingService;
        private readonly ILogger<DrillingOperationsController> _logger;

        public DrillingOperationsController(
            IDrillingOperationService drillingService,
            ILogger<DrillingOperationsController> logger)
        {
            _drillingService = drillingService ?? throw new ArgumentNullException(nameof(drillingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all drilling operations.
        /// </summary>
        /// <param name="wellUWI">Optional well UWI to filter operations.</param>
        /// <returns>List of drilling operations.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<DrillingOperationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DrillingOperationDto>>> GetDrillingOperations([FromQuery] string? wellUWI = null)
        {
            try
            {
                var operations = await _drillingService.GetDrillingOperationsAsync(wellUWI);
                return Ok(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling operations");
                return StatusCode(500, new { error = "An error occurred while retrieving drilling operations." });
            }
        }

        /// <summary>
        /// Gets a drilling operation by ID.
        /// </summary>
        /// <param name="id">Operation ID.</param>
        /// <returns>Drilling operation details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DrillingOperationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DrillingOperationDto>> GetDrillingOperation(string id)
        {
            try
            {
                var operation = await _drillingService.GetDrillingOperationAsync(id);
                if (operation == null)
                    return NotFound(new { error = $"Drilling operation with ID {id} not found." });

                return Ok(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling operation {OperationId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the drilling operation." });
            }
        }

        /// <summary>
        /// Creates a new drilling operation.
        /// </summary>
        /// <param name="createDto">Drilling operation creation data.</param>
        /// <returns>Created drilling operation.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(DrillingOperationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DrillingOperationDto>> CreateDrillingOperation([FromBody] CreateDrillingOperationDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var operation = await _drillingService.CreateDrillingOperationAsync(createDto);
                return CreatedAtAction(nameof(GetDrillingOperation), new { id = operation.OperationId }, operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating drilling operation");
                return StatusCode(500, new { error = "An error occurred while creating the drilling operation." });
            }
        }

        /// <summary>
        /// Updates a drilling operation.
        /// </summary>
        /// <param name="id">Operation ID.</param>
        /// <param name="updateDto">Drilling operation update data.</param>
        /// <returns>Updated drilling operation.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DrillingOperationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DrillingOperationDto>> UpdateDrillingOperation(string id, [FromBody] UpdateDrillingOperationDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var operation = await _drillingService.UpdateDrillingOperationAsync(id, updateDto);
                return Ok(operation);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Drilling operation with ID {id} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating drilling operation {OperationId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the drilling operation." });
            }
        }

        /// <summary>
        /// Gets drilling reports for an operation.
        /// </summary>
        /// <param name="id">Operation ID.</param>
        /// <returns>List of drilling reports.</returns>
        [HttpGet("{id}/reports")]
        [ProducesResponseType(typeof(List<DrillingReportDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DrillingReportDto>>> GetDrillingReports(string id)
        {
            try
            {
                var reports = await _drillingService.GetDrillingReportsAsync(id);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling reports for operation {OperationId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving drilling reports." });
            }
        }

        /// <summary>
        /// Creates a drilling report.
        /// </summary>
        /// <param name="id">Operation ID.</param>
        /// <param name="createDto">Drilling report creation data.</param>
        /// <returns>Created drilling report.</returns>
        [HttpPost("{id}/reports")]
        [ProducesResponseType(typeof(DrillingReportDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DrillingReportDto>> CreateDrillingReport(string id, [FromBody] CreateDrillingReportDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var report = await _drillingService.CreateDrillingReportAsync(id, createDto);
                return CreatedAtAction(nameof(GetDrillingReports), new { id }, report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating drilling report for operation {OperationId}", id);
                return StatusCode(500, new { error = "An error occurred while creating the drilling report." });
            }
        }
    }
}

