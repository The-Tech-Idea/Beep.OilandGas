using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Controllers.Production
{
    /// <summary>
    /// API controller for Production & Reserves operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionController : ControllerBase
    {
        private readonly IPPDMProductionService _productionService;
        private readonly IFieldOrchestrator? _fieldOrchestrator;
        private readonly ILogger<ProductionController> _logger;

        public ProductionController(
            IPPDMProductionService productionService,
            ILogger<ProductionController> logger)
        {
            _productionService = productionService;
            _logger = logger;
        }

        // Constructor with FieldOrchestrator for field-scoped endpoints
        public ProductionController(
            IPPDMProductionService productionService,
            IFieldOrchestrator fieldOrchestrator,
            ILogger<ProductionController> logger) : this(productionService, logger)
        {
            _fieldOrchestrator = fieldOrchestrator;
        }

        /// <summary>
        /// Get fields
        /// </summary>
        [HttpGet("fields")]
        public async Task<ActionResult<List<FIELD>>> GetFields([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var fields = await _productionService.GetFieldsAsync(filters);
                return Ok(fields);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fields");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get pools
        /// </summary>
        [HttpGet("pools")]
        public async Task<ActionResult<List<POOL>>> GetPools([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var pools = await _productionService.GetPoolsAsync(filters);
                return Ok(pools);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pools");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get production data
        /// </summary>
        [HttpGet("production")]
        public async Task<ActionResult<List<PDEN_VOL_SUMMARY>>> GetProduction([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var production = await _productionService.GetProductionAsync(filters);
                return Ok(production);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production data");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get reserves data
        /// </summary>
        [HttpGet("reserves")]
        public async Task<ActionResult<List<RESERVE_ENTITY>>> GetReserves([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var reserves = await _productionService.GetReservesAsync(filters);
                return Ok(reserves);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reserves data");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get production reporting data
        /// </summary>
        [HttpGet("reporting")]
        public async Task<ActionResult<List<PDEN_VOL_SUMMARY>>> GetProductionReporting([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var reporting = await _productionService.GetProductionReportingAsync(filters);
                return Ok(reporting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production reporting data");
                return StatusCode(500, new { error = ex.Message });
            }
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



