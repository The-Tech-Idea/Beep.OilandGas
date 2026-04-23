using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Models;
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
        private readonly IPPDMProductionService _productionService;
        private readonly ILogger<ProductionController> _logger;

        public ProductionController(
            IFieldOrchestrator fieldOrchestrator,
            IPPDMProductionService productionService,
            ILogger<ProductionController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator;
            _productionService = productionService ?? throw new ArgumentNullException(nameof(productionService));
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
                    return BadRequest(new { error = "No active field selected." });
                }

                var productionService = _fieldOrchestrator.GetProductionService();
                var forecasts = await productionService.GetProductionForecastsForFieldAsync(currentFieldId, filters);
                return Ok(forecasts);
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { error = "An internal error occurred." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production forecasts for current field");
                return StatusCode(500, new { error = "An internal error occurred." });
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
                        return BadRequest(new { error = "No active field selected." });
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                        return BadRequest(new { error = "User ID is required." });
                }

                var productionService = _fieldOrchestrator.GetProductionService();
                var forecast = await productionService.CreateProductionForecastForFieldAsync(currentFieldId, forecastData, userId);
                return Ok(forecast);
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { error = "An internal error occurred." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating production forecast for current field");
                return StatusCode(500, new { error = "An internal error occurred." });
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
                    return BadRequest(new { error = "No active field selected." });
                }

                var productionService = _fieldOrchestrator.GetProductionService();
                var production = await productionService.GetProductionForFieldAsync(currentFieldId, filters);
                return Ok(production);
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { error = "An internal error occurred." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production for current field");
                return StatusCode(500, new { error = "An internal error occurred." });
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
                    return BadRequest(new { error = "No active field selected." });
                }

                var productionService = _fieldOrchestrator.GetProductionService();
                var production = await productionService.GetProductionByPoolForFieldAsync(currentFieldId, poolId);
                return Ok(production);
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { error = "An internal error occurred." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production by pool for current field");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Get well tests for a well in the current field
        /// </summary>
        [HttpGet("field/wells/{wellId}/tests")]
        public async Task<ActionResult<List<WellTestResponse>>> GetWellTests(string wellId, [FromQuery] List<AppFilter>? filters = null)
        {
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
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
                    return BadRequest(new { error = "No active field selected." });
                }

                var productionService = _fieldOrchestrator.GetProductionService();
                var wellTests = await productionService.GetWellTestsForWellAsync(currentFieldId, wellId, filters);
                return Ok(wellTests);
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { error = "An internal error occurred." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting well tests for well {WellId} in current field", wellId);
                return StatusCode(500, new { error = "An internal error occurred." });
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
                    return BadRequest(new { error = "No active field selected." });
                }

                var productionService = _fieldOrchestrator.GetProductionService();
                var facilityProduction = await productionService.GetFacilityProductionForFieldAsync(currentFieldId, filters);
                return Ok(facilityProduction);
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { error = "An internal error occurred." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facility production for current field");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }
        // ============================================
        // PPDM Reference Browse Endpoints
        // ============================================

        /// <summary>GET /api/production/fields — browse all FIELD records</summary>
        [HttpGet("fields")]
        public async Task<ActionResult<List<FIELD>>> GetFieldsAsync([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var fields = await _productionService.GetFieldsAsync(filters);
                return Ok(fields);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching fields");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/production/pools — browse all POOL records</summary>
        [HttpGet("pools")]
        public async Task<ActionResult<List<POOL>>> GetPoolsAsync([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var pools = await _productionService.GetPoolsAsync(filters);
                return Ok(pools);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pools");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/production/reserves — browse all RESERVE_ENTITY records</summary>
        [HttpGet("reserves")]
        public async Task<ActionResult<List<RESERVE_ENTITY>>> GetReservesAsync([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var reserves = await _productionService.GetReservesAsync(filters);
                return Ok(reserves);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reserves");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/production/reporting — browse PDEN_VOL_SUMMARY for reporting</summary>
        [HttpGet("reporting")]
        public async Task<ActionResult<List<PDEN_VOL_SUMMARY>>> GetProductionReportingAsync([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var reporting = await _productionService.GetProductionReportingAsync(filters);
                return Ok(reporting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production reporting");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }
    }
}

