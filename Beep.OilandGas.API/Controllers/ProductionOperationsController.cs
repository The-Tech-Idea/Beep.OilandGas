using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.ProductionOperations.Services;

namespace Beep.OilandGas.API.Controllers
{
    /// <summary>
    /// API controller for production operations.
    /// </summary>
    [ApiController]
    [Route("api/v1/production-operations")]
    [Produces("application/json")]
    public class ProductionOperationsController : ControllerBase
    {
        private readonly IProductionManagementService _productionService;
        private readonly ILogger<ProductionOperationsController> _logger;

        public ProductionOperationsController(
            IProductionManagementService productionService,
            ILogger<ProductionOperationsController> logger)
        {
            _productionService = productionService ?? throw new ArgumentNullException(nameof(productionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all production operations.
        /// </summary>
        /// <param name="wellUWI">Optional well UWI to filter operations.</param>
        /// <param name="startDate">Optional start date filter.</param>
        /// <param name="endDate">Optional end date filter.</param>
        /// <returns>List of production operations.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProductionOperationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductionOperationDto>>> GetProductionOperations(
            [FromQuery] string? wellUWI = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var operations = await _productionService.GetProductionOperationsAsync(wellUWI, startDate, endDate);
                return Ok(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production operations");
                return StatusCode(500, new { error = "An error occurred while retrieving production operations." });
            }
        }

        /// <summary>
        /// Gets a production operation by ID.
        /// </summary>
        /// <param name="id">Operation ID.</param>
        /// <returns>Production operation details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductionOperationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductionOperationDto>> GetProductionOperation(string id)
        {
            try
            {
                var operation = await _productionService.GetProductionOperationAsync(id);
                if (operation == null)
                    return NotFound(new { error = $"Production operation with ID {id} not found." });

                return Ok(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production operation {OperationId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the production operation." });
            }
        }

        /// <summary>
        /// Creates a new production operation.
        /// </summary>
        /// <param name="createDto">Production operation creation data.</param>
        /// <returns>Created production operation.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProductionOperationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductionOperationDto>> CreateProductionOperation([FromBody] CreateProductionOperationDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var operation = await _productionService.CreateProductionOperationAsync(createDto);
                return CreatedAtAction(nameof(GetProductionOperation), new { id = operation.OperationId }, operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating production operation");
                return StatusCode(500, new { error = "An error occurred while creating the production operation." });
            }
        }

        /// <summary>
        /// Gets production reports.
        /// </summary>
        /// <param name="wellUWI">Optional well UWI to filter reports.</param>
        /// <param name="startDate">Optional start date filter.</param>
        /// <param name="endDate">Optional end date filter.</param>
        /// <returns>List of production reports.</returns>
        [HttpGet("reports")]
        [ProducesResponseType(typeof(List<ProductionReportDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductionReportDto>>> GetProductionReports(
            [FromQuery] string? wellUWI = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var reports = await _productionService.GetProductionReportsAsync(wellUWI, startDate, endDate);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production reports");
                return StatusCode(500, new { error = "An error occurred while retrieving production reports." });
            }
        }

        /// <summary>
        /// Gets well operations.
        /// </summary>
        /// <param name="wellUWI">Well UWI.</param>
        /// <returns>List of well operations.</returns>
        [HttpGet("wells/{wellUWI}/operations")]
        [ProducesResponseType(typeof(List<WellOperationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<WellOperationDto>>> GetWellOperations(string wellUWI)
        {
            try
            {
                var operations = await _productionService.GetWellOperationsAsync(wellUWI);
                return Ok(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting well operations for well {WellUWI}", wellUWI);
                return StatusCode(500, new { error = "An error occurred while retrieving well operations." });
            }
        }

        /// <summary>
        /// Gets facility operations.
        /// </summary>
        /// <param name="facilityId">Facility ID.</param>
        /// <returns>List of facility operations.</returns>
        [HttpGet("facilities/{facilityId}/operations")]
        [ProducesResponseType(typeof(List<FacilityOperationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<FacilityOperationDto>>> GetFacilityOperations(string facilityId)
        {
            try
            {
                var operations = await _productionService.GetFacilityOperationsAsync(facilityId);
                return Ok(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facility operations for facility {FacilityId}", facilityId);
                return StatusCode(500, new { error = "An error occurred while retrieving facility operations." });
            }
        }
    }
}

