using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Controllers.Production
{
    /// <summary>
    /// API controller for Production business operations
    /// 
    /// NOTE: For CRUD operations (Create, Read, Update, Delete), please use DataManagementController:
    /// - Get fields: GET /api/datamanagement/FIELD
    /// - Get pools: GET /api/datamanagement/POOL
    /// - Get production: GET /api/datamanagement/PDEN_VOL_SUMMARY
    /// - Get reserves: GET /api/datamanagement/RESERVE_ENTITY
    /// - Get production reporting: GET /api/datamanagement/PDEN_VOL_SUMMARY with filters
    /// 
    /// This controller focuses on field-scoped business logic operations via FieldOrchestrator.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionController : ControllerBase
    {
        private readonly IFieldOrchestrator? _fieldOrchestrator;
        private readonly ILogger<ProductionController> _logger;

        public ProductionController(
            IFieldOrchestrator fieldOrchestrator,
            ILogger<ProductionController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator;
            _logger = logger;
        }

        // ============================================
        // Field-Scoped Production Endpoints
        // ============================================

        /// <summary>
        /// Get production forecasts for the current field
        /// </summary>
        [HttpGet("field/forecasts")]
        public async Task<ActionResult<List<ProductionForecastResponse>>> GetProductionForecasts([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                if (_fieldOrchestrator == null)
                {
                    _logger.LogWarning("Field orchestrator not available for field-scoped endpoint");
                    return BadRequest(new { error = "Field orchestrator not available. Use field-scoped endpoints." });
                }

                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var productionService = _fieldOrchestrator.GetProductionService();
                var forecasts = await productionService.GetProductionForecastsForFieldAsync(currentFieldId, filters);
                return Ok(forecasts);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production forecasts for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create production forecast for the current field
        /// </summary>
        [HttpPost("field/forecasts")]
        public async Task<ActionResult<ProductionForecastResponse>> CreateProductionForecast(
            [FromBody] ProductionForecastRequest forecastData,
            [FromQuery] string userId)
        {
            try
            {
                if (_fieldOrchestrator == null)
                {
                    _logger.LogWarning("Field orchestrator not available for field-scoped endpoint");
                    return BadRequest(new { error = "Field orchestrator not available. Use field-scoped endpoints." });
                }

                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new { error = "userId is required" });
                }

                var productionService = _fieldOrchestrator.GetProductionService();
                var forecast = await productionService.CreateProductionForecastForFieldAsync(currentFieldId, forecastData, userId);
                return Ok(forecast);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating production forecast for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get production data for the current field
        /// </summary>
        [HttpGet("field/production")]
        public async Task<ActionResult<List<ProductionResponse>>> GetProductionForField([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                if (_fieldOrchestrator == null)
                {
                    _logger.LogWarning("Field orchestrator not available for field-scoped endpoint");
                    return BadRequest(new { error = "Field orchestrator not available. Use field-scoped endpoints." });
                }

                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var productionService = _fieldOrchestrator.GetProductionService();
                var production = await productionService.GetProductionForFieldAsync(currentFieldId, filters);
                return Ok(production);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get production by pool for the current field
        /// </summary>
        [HttpGet("field/production/by-pool")]
        public async Task<ActionResult<List<ProductionResponse>>> GetProductionByPool([FromQuery] string? poolId = null)
        {
            try
            {
                if (_fieldOrchestrator == null)
                {
                    _logger.LogWarning("Field orchestrator not available for field-scoped endpoint");
                    return BadRequest(new { error = "Field orchestrator not available. Use field-scoped endpoints." });
                }

                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var productionService = _fieldOrchestrator.GetProductionService();
                var production = await productionService.GetProductionByPoolForFieldAsync(currentFieldId, poolId);
                return Ok(production);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production by pool for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get well tests for a well in the current field
        /// </summary>
        [HttpGet("field/wells/{wellId}/tests")]
        public async Task<ActionResult<List<WellTestResponse>>> GetWellTests(string wellId, [FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                if (_fieldOrchestrator == null)
                {
                    _logger.LogWarning("Field orchestrator not available for field-scoped endpoint");
                    return BadRequest(new { error = "Field orchestrator not available. Use field-scoped endpoints." });
                }

                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var productionService = _fieldOrchestrator.GetProductionService();
                var wellTests = await productionService.GetWellTestsForWellAsync(currentFieldId, wellId, filters);
                return Ok(wellTests);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting well tests for well {WellId} in current field", wellId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get facility production for the current field
        /// </summary>
        [HttpGet("field/facilities/production")]
        public async Task<ActionResult<List<ProductionResponse>>> GetFacilityProduction([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                if (_fieldOrchestrator == null)
                {
                    _logger.LogWarning("Field orchestrator not available for field-scoped endpoint");
                    return BadRequest(new { error = "Field orchestrator not available. Use field-scoped endpoints." });
                }

                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var productionService = _fieldOrchestrator.GetProductionService();
                var facilityProduction = await productionService.GetFacilityProductionForFieldAsync(currentFieldId, filters);
                return Ok(facilityProduction);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facility production for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}



