using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;

namespace Beep.OilandGas.ApiService.Controllers.Production
{
    /// <summary>
    /// API controller for production operations.
    /// </summary>
    [ApiController]
    [Route("api/production/operations")]
    [Authorize]
    public class ProductionOperationsController : ControllerBase
    {
        private readonly Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService _service;
        private readonly IProductionManagementService _managementService;
        private readonly ILogger<ProductionOperationsController> _logger;

        public ProductionOperationsController(
            Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService service,
            IProductionManagementService managementService,
            ILogger<ProductionOperationsController> logger)
        {
            _service = service;
            _managementService = managementService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<ActionResult<PRODUCTION_COSTS>> CreateOperation([FromBody] PRODUCTION_COSTS request)
        {
            if (request == null) return BadRequest(new { error = "Request body is required." });

            try
            {
                var created = await _service.CreateOperationAsync(request, GetUserId());
                return Ok(created);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid production operation create request");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating production operation");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("/api/productionoperations/create")]
        public async Task<ActionResult<ProductionOperation>> CreateOperationCompatibility([FromBody] ProductionOperation request)
        {
            if (request == null) return BadRequest(new { error = "Request body is required." });

            try
            {
                var created = await _managementService.CreateProductionOperationAsync(new CreateProductionOperationRequest
                {
                    OperationDate = request.ScheduledDate == default ? null : request.ScheduledDate,
                    OperationType = string.IsNullOrWhiteSpace(request.OperationType) ? null : request.OperationType,
                    Status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status,
                    AssignedTo = string.IsNullOrWhiteSpace(request.AssignedTo) ? null : request.AssignedTo,
                    Remarks = string.IsNullOrWhiteSpace(request.Remarks) ? null : request.Remarks
                });

                return Ok(MapToLegacyOperation(created, request));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid legacy production operation create request");
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Legacy production operation creation failed");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating production operation through compatibility route");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpGet("{operationId}")]
        public async Task<ActionResult<PRODUCTION_COSTS>> GetOperationStatus(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId)) return BadRequest(new { error = "Operation ID is required." });

            try
            {
                var operation = await _service.GetOperationStatusAsync(operationId);
                if (operation == null)
                    return NotFound(new { error = $"Production operation {operationId} was not found." });

                return Ok(operation);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid production operation lookup request");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production operation {OperationId}", operationId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPut("{operationId}")]
        public async Task<ActionResult<PRODUCTION_COSTS>> UpdateOperation(string operationId, [FromBody] PRODUCTION_COSTS request)
        {
            if (string.IsNullOrWhiteSpace(operationId)) return BadRequest(new { error = "Operation ID is required." });
            if (request == null) return BadRequest(new { error = "Request body is required." });

            try
            {
                var updated = await _service.UpdateOperationAsync(operationId, request, GetUserId());
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid production operation update request for {OperationId}", operationId);
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Production operation update target not found for {OperationId}", operationId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating production operation {OperationId}", operationId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpGet("/api/production/data/{wellId}")]
        public async Task<ActionResult<PRODUCTION_ALLOCATION>> GetProductionDataCompatibility(string wellId)
        {
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });

            try
            {
                var records = await _service.GetProductionDataAsync(wellId, null, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);
                var latest = records
                    .OrderByDescending(record => record.ProductionDate)
                    .FirstOrDefault();

                if (latest == null)
                    return NotFound(new { error = $"Production data for well {wellId} was not found." });

                return Ok(MapToAllocation(latest));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid production data compatibility request for {WellId}", wellId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compatibility production data for {WellId}", wellId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("/api/production/history/{wellId}")]
        public async Task<ActionResult<List<PRODUCTION_ALLOCATION>>> GetProductionHistoryCompatibility(
            string wellId,
            [FromBody] ProductionHistoryRangeRequest? request)
        {
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });

            var startDate = request?.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = request?.EndDate ?? DateTime.UtcNow;
            if (startDate > endDate)
                return BadRequest(new { error = "StartDate must be on or before EndDate." });

            try
            {
                var records = await _service.GetProductionDataAsync(wellId, null, startDate, endDate);
                var response = records
                    .OrderByDescending(record => record.ProductionDate)
                    .Select(record => MapToAllocation(record))
                    .ToList();

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid production history compatibility request for {WellId}", wellId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compatibility production history for {WellId}", wellId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("/api/production/record")]
        public async Task<ActionResult<PRODUCTION_ALLOCATION>> RecordProductionCompatibility(
            [FromBody] PRODUCTION_ALLOCATION productionRecord,
            [FromQuery] string? userId = null)
        {
            if (productionRecord == null) return BadRequest(new { error = "Request body is required." });

            try
            {
                var productionData = MapFromAllocation(productionRecord);
                await _service.RecordProductionDataAsync(productionData, userId ?? GetUserId());

                return Ok(MapToAllocation(productionData, productionRecord));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid compatibility production record request for well {WellId}", productionRecord.WELL_ID ?? productionRecord.PDEN_ID);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording compatibility production data for well {WellId}", productionRecord.WELL_ID ?? productionRecord.PDEN_ID);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpGet("data")]
        public async Task<ActionResult<List<ProductionData>>> GetProductionData(
            [FromQuery] string? wellUWI = null,
            [FromQuery] string? fieldId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;
                var result = await _service.GetProductionDataAsync(wellUWI, fieldId, start, end);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production data");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("data")]
        public async Task<ActionResult> RecordProductionData([FromBody] ProductionData productionData, [FromQuery] string? userId = null)
        {
            try
            {
                await _service.RecordProductionDataAsync(productionData, userId ?? GetUserId());
                return Ok(new { message = "Production data recorded successfully", productionId = productionData.ProductionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording production data");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("optimize")]
        public async Task<ActionResult<List<ProductionOptimizationRecommendation>>> OptimizeProduction(
            [FromQuery] string wellUWI,
            [FromBody] Dictionary<string, object> optimizationGoals)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) return BadRequest(new { error = "Well UWI is required." });
            try
            {
                var result = await _service.OptimizeProductionAsync(wellUWI, optimizationGoals);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid optimize request for {WellUWI}", wellUWI);
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Unable to optimize production for {WellUWI}", wellUWI);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing production");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private static PRODUCTION_ALLOCATION MapToAllocation(ProductionData data, PRODUCTION_ALLOCATION? seed = null)
        {
            var payload = new ProductionAllocationPayload
            {
                OilVolume = data.OilVolume,
                GasVolume = data.GasVolume,
                WaterVolume = data.WaterVolume,
                Status = data.Status
            };

            return new PRODUCTION_ALLOCATION
            {
                PRODUCTION_ALLOCATION_ID = seed?.PRODUCTION_ALLOCATION_ID ?? data.ProductionId,
                PDEN_ID = data.WellUWI,
                FIELD_ID = data.FieldId ?? seed?.FIELD_ID,
                WELL_ID = data.WellUWI,
                POOL_ID = seed?.POOL_ID,
                ALLOCATION_DATE = data.ProductionDate,
                TOTAL_PRODUCTION = data.OilVolume + data.GasVolume + data.WaterVolume,
                ALLOCATION_METHOD = seed?.ALLOCATION_METHOD ?? "PDEN_VOL_SUMMARY_COMPAT",
                ALLOCATION_RESULTS_JSON = JsonSerializer.Serialize(payload),
                DESCRIPTION = seed?.DESCRIPTION ?? data.Status
            };
        }


        private static ProductionOperation MapToLegacyOperation(PDEN operation, ProductionOperation? seed = null)
        {
            return new ProductionOperation
            {
                OperationId = operation.PDEN_ID ?? seed?.OperationId ?? string.Empty,
                OperationType = string.IsNullOrWhiteSpace(operation.PDEN_SUBTYPE) ? seed?.OperationType ?? "PRODUCTION" : operation.PDEN_SUBTYPE,
                ScheduledDate = operation.CURRENT_STATUS_DATE ?? seed?.ScheduledDate ?? DateTime.UtcNow,
                Status = string.IsNullOrWhiteSpace(operation.PDEN_STATUS) ? seed?.Status ?? "Planned" : operation.PDEN_STATUS,
                AssignedTo = string.IsNullOrWhiteSpace(operation.CURRENT_OPERATOR) ? seed?.AssignedTo ?? string.Empty : operation.CURRENT_OPERATOR,
                Remarks = string.IsNullOrWhiteSpace(operation.REMARK) ? seed?.Remarks ?? string.Empty : operation.REMARK
            };
        }
        private static ProductionData MapFromAllocation(PRODUCTION_ALLOCATION allocation)
        {
            if (allocation == null)
                throw new ArgumentNullException(nameof(allocation));

            var wellId = allocation.WELL_ID ?? allocation.PDEN_ID;
            if (string.IsNullOrWhiteSpace(wellId))
                throw new ArgumentException("WELL_ID or PDEN_ID is required.", nameof(allocation));

            var payload = TryParsePayload(allocation.ALLOCATION_RESULTS_JSON);

            return new ProductionData
            {
                ProductionId = allocation.PRODUCTION_ALLOCATION_ID,
                WellUWI = wellId,
                FieldId = allocation.FIELD_ID,
                ProductionDate = allocation.ALLOCATION_DATE ?? DateTime.UtcNow,
                OilVolume = payload?.OilVolume ?? allocation.TOTAL_PRODUCTION,
                GasVolume = payload?.GasVolume ?? 0m,
                WaterVolume = payload?.WaterVolume ?? 0m,
                Status = payload?.Status ?? allocation.DESCRIPTION
            };
        }

        private static ProductionAllocationPayload? TryParsePayload(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<ProductionAllocationPayload>(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";

        private sealed class ProductionAllocationPayload
        {
            public decimal OilVolume { get; set; }
            public decimal GasVolume { get; set; }
            public decimal WaterVolume { get; set; }
            public string? Status { get; set; }
        }

        public sealed class ProductionHistoryRangeRequest
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
    }
}

