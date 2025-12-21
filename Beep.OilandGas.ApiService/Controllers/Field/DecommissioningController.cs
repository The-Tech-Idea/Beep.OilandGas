using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    /// <summary>
    /// API controller for Decommissioning phase operations, field-scoped
    /// </summary>
    [ApiController]
    [Route("api/field/current/decommissioning")]
    public class DecommissioningController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly ILogger<DecommissioningController> _logger;

        public DecommissioningController(
            IFieldOrchestrator fieldOrchestrator,
            ILogger<DecommissioningController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _logger = logger;
        }

        /// <summary>
        /// Get all abandoned wells for the current field
        /// </summary>
        [HttpGet("wells-abandoned")]
        public async Task<ActionResult<List<WellAbandonmentResponse>>> GetAbandonedWells([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
                var abandonedWells = await decommissioningService.GetAbandonedWellsForFieldAsync(currentFieldId, filters);
                return Ok(abandonedWells);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting abandoned wells for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get well abandonment record by ID (must belong to current field)
        /// </summary>
        [HttpGet("wells-abandoned/{id}")]
        public async Task<ActionResult<WellAbandonmentResponse>> GetWellAbandonment(string id)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
                var abandonment = await decommissioningService.GetWellAbandonmentForFieldAsync(currentFieldId, id);
                
                if (abandonment == null)
                {
                    return NotFound(new { error = $"Well abandonment {id} not found or does not belong to current field" });
                }

                return Ok(abandonment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting well abandonment {AbandonmentId} for current field", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Record well abandonment (well must belong to current field)
        /// </summary>
        [HttpPost("abandon-well")]
        public async Task<ActionResult<WellAbandonmentResponse>> AbandonWell(
            [FromQuery] string wellId,
            [FromBody] WellAbandonmentRequest abandonmentData,
            [FromQuery] string userId)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(wellId))
                {
                    return BadRequest(new { error = "wellId is required" });
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new { error = "userId is required" });
                }

                var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
                var abandonment = await decommissioningService.AbandonWellForFieldAsync(currentFieldId, wellId, abandonmentData, userId);
                return Ok(abandonment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error abandoning well {WellId} for current field", wellId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all decommissioned facilities for the current field
        /// </summary>
        [HttpGet("facilities")]
        public async Task<ActionResult<List<FACILITY>>> GetDecommissionedFacilities([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
                var facilities = await decommissioningService.GetDecommissionedFacilitiesForFieldAsync(currentFieldId, filters);
                return Ok(facilities.Cast<FACILITY>().ToList());
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting decommissioned facilities for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get facility decommissioning record by ID (must belong to current field)
        /// </summary>
        [HttpGet("facilities/{id}")]
        public async Task<ActionResult<FacilityDecommissioningResponse>> GetFacilityDecommissioning(string id)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
                var decommissioning = await decommissioningService.GetFacilityDecommissioningForFieldAsync(currentFieldId, id);
                
                if (decommissioning == null)
                {
                    return NotFound(new { error = $"Facility decommissioning {id} not found or does not belong to current field" });
                }

                return Ok(decommissioning);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facility decommissioning {DecommissioningId} for current field", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Decommission a facility (must belong to current field)
        /// </summary>
        [HttpPost("facilities/{facilityId}/decommission")]
        public async Task<ActionResult<object>> DecommissionFacility(
            string facilityId,
            [FromBody] Dictionary<string, object> decommissionData,
            [FromQuery] string userId)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new { error = "userId is required" });
                }

                var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
                var decommissioning = await decommissioningService.DecommissionFacilityForFieldAsync(currentFieldId, facilityId, decommissionData, userId);
                return Ok(decommissioning);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decommissioning facility {FacilityId} for current field", facilityId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get environmental restoration activities for the current field
        /// </summary>
        [HttpGet("environmental-activities")]
        public async Task<ActionResult<List<EnvironmentalRestorationResponse>>> GetEnvironmentalRestorations([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
                var restorations = await decommissioningService.GetEnvironmentalRestorationsForFieldAsync(currentFieldId, filters);
                return Ok(restorations);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting environmental restorations for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create environmental restoration activity for the current field
        /// </summary>
        [HttpPost("environmental-activities")]
        public async Task<ActionResult<EnvironmentalRestorationResponse>> CreateEnvironmentalRestoration(
            [FromBody] EnvironmentalRestorationRequest restorationData,
            [FromQuery] string userId)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new { error = "userId is required" });
                }

                var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
                var restoration = await decommissioningService.CreateEnvironmentalRestorationForFieldAsync(currentFieldId, restorationData, userId);
                return Ok(restoration);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating environmental restoration for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get decommissioning costs for the current field
        /// </summary>
        [HttpGet("costs")]
        public async Task<ActionResult<List<FIN_COST_SUMMARY>>> GetDecommissioningCosts([FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
                var costs = await decommissioningService.GetDecommissioningCostsForFieldAsync(currentFieldId, filters);
                return Ok(costs.Cast<FIN_COST_SUMMARY>().ToList());
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting decommissioning costs for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Estimate decommissioning costs for the current field
        /// </summary>
        [HttpPost("cost-estimation")]
        public async Task<ActionResult<DecommissioningCostEstimateResponse>> EstimateCosts()
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
                var estimate = await decommissioningService.EstimateCostsForFieldAsync(currentFieldId);
                return Ok(estimate);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estimating decommissioning costs for current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
