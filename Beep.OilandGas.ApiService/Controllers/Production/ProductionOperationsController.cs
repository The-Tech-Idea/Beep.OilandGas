using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
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
    /// Active endpoints map to implemented members in
    /// <c>Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService</c>.
    /// Staged/placeholder methods in the expanded local production-operations surface
    /// are intentionally not exposed here until implementation is completed.
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
            var userId = User.ActingUserId();
            if (request == null) return BadRequest(new { error = "Request body is required." });

            var created = await _service.CreateOperationAsync(request, userId);
            return Ok(created);
        }

        [HttpPost("/api/productionoperations/create")]
        public async Task<ActionResult<ProductionOperation>> CreateOperationCompatibility([FromBody] ProductionOperation request)
        {
            var userId = User.ActingUserId();
            if (request == null) return BadRequest(new { error = "Request body is required." });

            var created = await _managementService.CreateProductionOperationAsync(new CreateProductionOperationRequest
            {
                OperationDate = request.ScheduledDate == default ? null : request.ScheduledDate,
                OperationType = string.IsNullOrWhiteSpace(request.OperationType) ? null : request.OperationType,
                Status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status,
                AssignedTo = string.IsNullOrWhiteSpace(request.AssignedTo) ? null : request.AssignedTo,
                Remarks = string.IsNullOrWhiteSpace(request.Remarks) ? null : request.Remarks
            }, userId, HttpContext.RequestAborted);

            return Ok(MapToLegacyOperation(created, request));
        }

        [HttpGet("{operationId}")]
        public async Task<ActionResult<PRODUCTION_COSTS>> GetOperationStatus(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId)) return BadRequest(new { error = "Operation ID is required." });

            var operation = await _service.GetOperationStatusAsync(operationId);
            if (operation == null)
                return NotFound(new { error = $"Production operation {operationId} was not found." });

            return Ok(operation);
        }

        [HttpPut("{operationId}")]
        public async Task<ActionResult<PRODUCTION_COSTS>> UpdateOperation(string operationId, [FromBody] PRODUCTION_COSTS request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(operationId)) return BadRequest(new { error = "Operation ID is required." });
            if (request == null) return BadRequest(new { error = "Request body is required." });

            var updated = await _service.UpdateOperationAsync(operationId, request, userId);
            return Ok(updated);
        }

        [HttpGet("/api/production/data/{wellId}")]
        public async Task<ActionResult<PRODUCTION_ALLOCATION>> GetProductionDataCompatibility(string wellId)
        {
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });

            var records = await _service.GetProductionDataAsync(wellId, null, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);
            var latest = records
                .OrderByDescending(record => record.ProductionDate)
                .FirstOrDefault();

            if (latest == null)
                return NotFound(new { error = $"Production data for well {wellId} was not found." });

            return Ok(MapToAllocation(latest));
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

            var records = await _service.GetProductionDataAsync(wellId, null, startDate, endDate);
            var response = records
                .OrderByDescending(record => record.ProductionDate)
                .Select(record => MapToAllocation(record))
                .ToList();

            return Ok(response);
        }

        [HttpPost("/api/production/record")]
        public async Task<ActionResult<PRODUCTION_ALLOCATION>> RecordProductionCompatibility(
            [FromBody] PRODUCTION_ALLOCATION productionRecord)
        {
            var userId = User.ActingUserId();
            if (productionRecord == null) return BadRequest(new { error = "Request body is required." });

            var productionData = MapFromAllocation(productionRecord);
            await _service.RecordProductionDataAsync(productionData, userId);

            return Ok(MapToAllocation(productionData, productionRecord));
        }

        [HttpGet("data")]
        public async Task<ActionResult<List<ProductionData>>> GetProductionData(
            [FromQuery] string? wellUWI = null,
            [FromQuery] string? fieldId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;
            var result = await _service.GetProductionDataAsync(wellUWI, fieldId, start, end);
            return Ok(result);
        }

        [HttpPost("data")]
        public async Task<ActionResult> RecordProductionData([FromBody] ProductionData productionData)
        {
            var userId = User.ActingUserId();
            await _service.RecordProductionDataAsync(productionData, userId);
            return Ok(new { message = "Production data recorded successfully", productionId = productionData.ProductionId });
        }

        [HttpPost("optimize")]
        public async Task<ActionResult<List<ProductionOptimizationRecommendation>>> OptimizeProduction(
            [FromQuery] string wellUWI,
            [FromBody] Dictionary<string, object> optimizationGoals)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) return BadRequest(new { error = "Well UWI is required." });
            var result = await _service.OptimizeProductionAsync(wellUWI, optimizationGoals);
            return Ok(result);
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
                throw RefusalException.Invalid("WELL_ID or PDEN_ID is required.");

            var payload = ParsePayload(allocation.ALLOCATION_RESULTS_JSON);

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

        // The allocation results the caller sent. Unreadable, they were taken as absent and the total recorded as oil — a
        // record that is not what was sent; now they are refused. System.Text.Json has no question for "is this JSON".
        private static ProductionAllocationPayload? ParsePayload(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<ProductionAllocationPayload>(json);
            }
            catch (JsonException notAllocationResults)
            {
                throw new RefusalException(RefusalKind.Invalid,
                    "ALLOCATION_RESULTS_JSON is not valid allocation results JSON.", notAllocationResults);
            }
        }

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

