using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.DTOs.LifeCycle;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    /// <summary>
    /// API controller for Decommissioning phase business operations, field-scoped
    /// 
    /// This controller focuses on decommissioning business logic and workflow processes.
    /// Business logic endpoints (aggregations, cost estimation) are kept here.
    /// Workflow endpoints use DecommissioningProcessService for process orchestration.
    /// </summary>
    [ApiController]
    [Route("api/field/current/decommissioning")]
    public class DecommissioningController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly Beep.OilandGas.LifeCycle.Services.Decommissioning.Processes.DecommissioningProcessService _decommissioningProcessService;
        private readonly ILogger<DecommissioningController> _logger;

        public DecommissioningController(
            IFieldOrchestrator fieldOrchestrator,
            Beep.OilandGas.LifeCycle.Services.Decommissioning.Processes.DecommissioningProcessService decommissioningProcessService,
            ILogger<DecommissioningController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _decommissioningProcessService = decommissioningProcessService ?? throw new ArgumentNullException(nameof(decommissioningProcessService));
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
        public async Task<ActionResult<FacilityDecommissioningResponse>> DecommissionFacility(
            string facilityId,
            [FromBody] FacilityDecommissioningRequest decommissionData,
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

        // ============================================
        // DECOMMISSIONING WORKFLOW ENDPOINTS
        // ============================================

        #region Well Abandonment Workflow

        /// <summary>
        /// Start Well Abandonment workflow
        /// </summary>
        [HttpPost("workflows/well-abandonment")]
        public async Task<ActionResult<Beep.OilandGas.LifeCycle.Models.Processes.ProcessInstance>> StartWellAbandonmentProcess(
            [FromBody] StartWellAbandonmentRequest request)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(request.WellId))
                {
                    return BadRequest(new { error = "WellId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var instance = await _decommissioningProcessService.StartWellAbandonmentProcessAsync(
                    request.WellId, 
                    currentFieldId, 
                    request.UserId);
                
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Well Abandonment process");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Plan abandonment
        /// </summary>
        [HttpPost("workflows/plan-abandonment")]
        public async Task<ActionResult<bool>> PlanAbandonment([FromBody] PlanAbandonmentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                {
                    return BadRequest(new { error = "InstanceId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var result = await _decommissioningProcessService.PlanAbandonmentAsync(
                    request.InstanceId, 
                    request.PlanData ?? new Dictionary<string, object>(), 
                    request.UserId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error planning abandonment");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtain regulatory approval
        /// </summary>
        [HttpPost("workflows/obtain-regulatory-approval")]
        public async Task<ActionResult<bool>> ObtainRegulatoryApproval([FromBody] ObtainRegulatoryApprovalRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                {
                    return BadRequest(new { error = "InstanceId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var result = await _decommissioningProcessService.ObtainRegulatoryApprovalAsync(request.InstanceId, request.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining regulatory approval");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Plug well
        /// </summary>
        [HttpPost("workflows/plug-well")]
        public async Task<ActionResult<bool>> PlugWell([FromBody] PlugWellRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                {
                    return BadRequest(new { error = "InstanceId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var result = await _decommissioningProcessService.PlugWellAsync(
                    request.InstanceId, 
                    request.PluggingData ?? new Dictionary<string, object>(), 
                    request.UserId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error plugging well");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Restore site
        /// </summary>
        [HttpPost("workflows/restore-site")]
        public async Task<ActionResult<bool>> RestoreSite([FromBody] RestoreSiteRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                {
                    return BadRequest(new { error = "InstanceId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var result = await _decommissioningProcessService.RestoreSiteAsync(
                    request.InstanceId, 
                    request.RestorationData ?? new Dictionary<string, object>(), 
                    request.UserId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring site");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Complete abandonment
        /// </summary>
        [HttpPost("workflows/complete-abandonment")]
        public async Task<ActionResult<bool>> CompleteAbandonment([FromBody] CompleteAbandonmentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                {
                    return BadRequest(new { error = "InstanceId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var result = await _decommissioningProcessService.CompleteAbandonmentAsync(request.InstanceId, request.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing abandonment");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Facility Decommissioning Workflow

        /// <summary>
        /// Start Facility Decommissioning workflow
        /// </summary>
        [HttpPost("workflows/facility-decommissioning")]
        public async Task<ActionResult<Beep.OilandGas.LifeCycle.Models.Processes.ProcessInstance>> StartFacilityDecommissioningProcess(
            [FromBody] StartFacilityDecommissioningRequest request)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(request.FacilityId))
                {
                    return BadRequest(new { error = "FacilityId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var instance = await _decommissioningProcessService.StartFacilityDecommissioningProcessAsync(
                    request.FacilityId, 
                    currentFieldId, 
                    request.UserId);
                
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Facility Decommissioning process");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Plan decommissioning
        /// </summary>
        [HttpPost("workflows/plan-decommissioning")]
        public async Task<ActionResult<bool>> PlanDecommissioning([FromBody] PlanDecommissioningRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                {
                    return BadRequest(new { error = "InstanceId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var result = await _decommissioningProcessService.PlanDecommissioningAsync(
                    request.InstanceId, 
                    request.PlanData ?? new Dictionary<string, object>(), 
                    request.UserId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error planning decommissioning");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Remove equipment
        /// </summary>
        [HttpPost("workflows/remove-equipment")]
        public async Task<ActionResult<bool>> RemoveEquipment([FromBody] RemoveEquipmentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                {
                    return BadRequest(new { error = "InstanceId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var result = await _decommissioningProcessService.RemoveEquipmentAsync(
                    request.InstanceId, 
                    request.RemovalData ?? new Dictionary<string, object>(), 
                    request.UserId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing equipment");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cleanup site
        /// </summary>
        [HttpPost("workflows/cleanup-site")]
        public async Task<ActionResult<bool>> CleanupSite([FromBody] CleanupSiteRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                {
                    return BadRequest(new { error = "InstanceId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var result = await _decommissioningProcessService.CleanupSiteAsync(
                    request.InstanceId, 
                    request.CleanupData ?? new Dictionary<string, object>(), 
                    request.UserId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up site");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtain regulatory closure
        /// </summary>
        [HttpPost("workflows/obtain-regulatory-closure")]
        public async Task<ActionResult<bool>> ObtainRegulatoryClosure([FromBody] ObtainRegulatoryClosureRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                {
                    return BadRequest(new { error = "InstanceId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var result = await _decommissioningProcessService.ObtainRegulatoryClosureAsync(request.InstanceId, request.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining regulatory closure");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Complete decommissioning
        /// </summary>
        [HttpPost("workflows/complete-decommissioning")]
        public async Task<ActionResult<bool>> CompleteDecommissioning([FromBody] CompleteDecommissioningRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                {
                    return BadRequest(new { error = "InstanceId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var result = await _decommissioningProcessService.CompleteDecommissioningAsync(request.InstanceId, request.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing decommissioning");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion
    }
}
