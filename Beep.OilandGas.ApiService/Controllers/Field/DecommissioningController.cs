using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
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
    [RequireCurrentFieldAccess]
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
            var currentFieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(currentFieldId))
            {
                    return BadRequest(new { error = "No active field selected." });
            }

            var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
            var abandonedWells = await decommissioningService.GetAbandonedWellsForFieldAsync(currentFieldId, filters);
            return Ok(abandonedWells);
        }

        /// <summary>
        /// Get well abandonment record by ID (must belong to current field)
        /// </summary>
        [HttpGet("wells-abandoned/{id}")]
        public async Task<ActionResult<WellAbandonmentResponse>> GetWellAbandonment(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "ID is required." });
            var currentFieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(currentFieldId))
            {
                    return BadRequest(new { error = "No active field selected." });
            }

            var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
            var abandonment = await decommissioningService.GetWellAbandonmentForFieldAsync(currentFieldId, id);
            
            if (abandonment == null)
            {
                return NotFound(new { error = $"Well abandonment {id} not found or does not belong to current field." });
            }

            return Ok(abandonment);
        }

        /// <summary>
        /// Record well abandonment (well must belong to current field)
        /// </summary>
        [HttpPost("abandon-well")]
        public async Task<ActionResult<WellAbandonmentResponse>> AbandonWell(
            [FromQuery] string wellId,
            [FromBody] WellAbandonmentRequest abandonmentData)
        {
            var userId = User.ActingUserId();
            var currentFieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(currentFieldId))
            {
                    return BadRequest(new { error = "No active field selected." });
            }

            if (string.IsNullOrWhiteSpace(wellId))
            {
                    return BadRequest(new { error = "Well ID is required." });
            }

            var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
            var abandonment = await decommissioningService.AbandonWellForFieldAsync(currentFieldId, wellId, abandonmentData, userId);
            return Ok(abandonment);
        }

        /// <summary>
        /// Get all decommissioned facilities for the current field
        /// </summary>
        [HttpGet("facilities")]
        public async Task<ActionResult<List<FacilityDecommissioningResponse>>> GetDecommissionedFacilities([FromQuery] List<AppFilter>? filters = null)
        {
            var currentFieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(currentFieldId))
            {
                    return BadRequest(new { error = "No active field selected." });
            }

            var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
            var facilities = await decommissioningService.GetDecommissionedFacilitiesForFieldAsync(currentFieldId, filters);
            return Ok(facilities);
        }

        /// <summary>
        /// Get facility decommissioning record by ID (must belong to current field)
        /// </summary>
        [HttpGet("facilities/{id}")]
        public async Task<ActionResult<FacilityDecommissioningResponse>> GetFacilityDecommissioning(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "ID is required." });
            var currentFieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(currentFieldId))
            {
                    return BadRequest(new { error = "No active field selected." });
            }

            var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
            var decommissioning = await decommissioningService.GetFacilityDecommissioningForFieldAsync(currentFieldId, id);
            
            if (decommissioning == null)
            {
                return NotFound(new { error = $"Facility decommissioning {id} not found or does not belong to current field." });
            }

            return Ok(decommissioning);
        }

        /// <summary>
        /// Decommission a facility (must belong to current field)
        /// </summary>
        [HttpPost("facilities/{facilityId}/decommission")]
        public async Task<ActionResult<FacilityDecommissioningResponse>> DecommissionFacility(
            string facilityId,
            [FromBody] FacilityDecommissioningRequest decommissionData)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(facilityId)) return BadRequest(new { error = "Facility ID is required." });
            var currentFieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(currentFieldId))
            {
                    return BadRequest(new { error = "No active field selected." });
            }

            var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
            var decommissioning = await decommissioningService.DecommissionFacilityForFieldAsync(currentFieldId, facilityId, decommissionData, userId);
            return Ok(decommissioning);
        }

        /// <summary>
        /// Get environmental restoration activities for the current field
        /// </summary>
        [HttpGet("environmental-activities")]
        public async Task<ActionResult<List<EnvironmentalRestorationResponse>>> GetEnvironmentalRestorations([FromQuery] List<AppFilter>? filters = null)
        {
            var currentFieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(currentFieldId))
            {
                    return BadRequest(new { error = "No active field selected." });
            }

            var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
            var restorations = await decommissioningService.GetEnvironmentalRestorationsForFieldAsync(currentFieldId, filters);
            return Ok(restorations);
        }

        /// <summary>
        /// Create environmental restoration activity for the current field
        /// </summary>
        [HttpPost("environmental-activities")]
        public async Task<ActionResult<EnvironmentalRestorationResponse>> CreateEnvironmentalRestoration(
            [FromBody] EnvironmentalRestorationRequest restorationData)
        {
            var userId = User.ActingUserId();
            var currentFieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(currentFieldId))
            {
                    return BadRequest(new { error = "No active field selected." });
            }

            var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
            var restoration = await decommissioningService.CreateEnvironmentalRestorationForFieldAsync(currentFieldId, restorationData, userId);
            return Ok(restoration);
        }

        /// <summary>
        /// Get decommissioning costs for the current field
        /// </summary>
        [HttpGet("costs")]
        public async Task<ActionResult<List<DecommissioningCostResponse>>> GetDecommissioningCosts([FromQuery] List<AppFilter>? filters = null)
        {
            var currentFieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(currentFieldId))
            {
                    return BadRequest(new { error = "No active field selected." });
            }

            var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
            var costs = await decommissioningService.GetDecommissioningCostsForFieldAsync(currentFieldId, filters);
            return Ok(costs);
        }

        /// <summary>
        /// Estimate decommissioning costs for the current field
        /// </summary>
        [HttpPost("cost-estimation")]
        public async Task<ActionResult<DecommissioningCostEstimateResponse>> EstimateCosts()
        {
            var currentFieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(currentFieldId))
            {
                    return BadRequest(new { error = "No active field selected." });
            }

            var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
            var estimate = await decommissioningService.EstimateCostsForFieldAsync(currentFieldId);
            return Ok(estimate);
        }

        // ============================================
        // DECOMMISSIONING WORKFLOW ENDPOINTS
        // ============================================

        #region Well Abandonment Workflow

        /// <summary>
        /// Start Well Abandonment workflow
        /// </summary>
        [HttpPost("workflows/well-abandonment")]
        public async Task<ActionResult<Beep.OilandGas.Models.Processes.ProcessInstance>> StartWellAbandonmentProcess(
            [FromBody] StartWellAbandonmentRequest request)
        {
            var userId = User.ActingUserId();
            var currentFieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(currentFieldId))
            {
                    return BadRequest(new { error = "No active field selected." });
            }

            if (string.IsNullOrWhiteSpace(request.WellId))
            {
                    return BadRequest(new { error = "Well ID is required." });
            }

            var instance = await _decommissioningProcessService.StartWellAbandonmentProcessAsync(
                request.WellId, 
                currentFieldId, 
                userId);
            
            return Ok(instance);
        }

        /// <summary>
        /// Plan abandonment
        /// </summary>
        [HttpPost("workflows/plan-abandonment")]
        public async Task<ActionResult<bool>> PlanAbandonment([FromBody] PlanAbandonmentRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(request.InstanceId))
            {
                    return BadRequest(new { error = "Instance ID is required." });
            }

            var result = await _decommissioningProcessService.PlanAbandonmentAsync(
                request.InstanceId, 
                new PROCESS_STEP_DATA { Data = request.PlanData ?? new Dictionary<string, object>() }, 
                userId);
            
            return Ok(result);
        }

        /// <summary>
        /// Obtain regulatory approval
        /// </summary>
        [HttpPost("workflows/obtain-regulatory-approval")]
        public async Task<ActionResult<bool>> ObtainRegulatoryApproval([FromBody] ObtainRegulatoryApprovalRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(request.InstanceId))
            {
                    return BadRequest(new { error = "Instance ID is required." });
            }

            var result = await _decommissioningProcessService.ObtainRegulatoryApprovalAsync(request.InstanceId, userId);
            return Ok(result);
        }

        /// <summary>
        /// Plug well
        /// </summary>
        [HttpPost("workflows/plug-well")]
        public async Task<ActionResult<bool>> PlugWell([FromBody] PlugWellRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(request.InstanceId))
            {
                    return BadRequest(new { error = "Instance ID is required." });
            }

            var result = await _decommissioningProcessService.PlugWellAsync(
                request.InstanceId, 
                new PROCESS_STEP_DATA { Data = request.PluggingData ?? new Dictionary<string, object>() }, 
                userId);
            
            return Ok(result);
        }

        /// <summary>
        /// Restore site
        /// </summary>
        [HttpPost("workflows/restore-site")]
        public async Task<ActionResult<bool>> RestoreSite([FromBody] RestoreSiteRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(request.InstanceId))
            {
                    return BadRequest(new { error = "Instance ID is required." });
            }

            var result = await _decommissioningProcessService.RestoreSiteAsync(
                request.InstanceId, 
                new PROCESS_STEP_DATA { Data = request.RestorationData ?? new Dictionary<string, object>() }, 
                userId);
            
            return Ok(result);
        }

        /// <summary>
        /// Complete abandonment
        /// </summary>
        [HttpPost("workflows/complete-abandonment")]
        public async Task<ActionResult<bool>> CompleteAbandonment([FromBody] CompleteAbandonmentRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(request.InstanceId))
            {
                    return BadRequest(new { error = "Instance ID is required." });
            }

            var result = await _decommissioningProcessService.CompleteAbandonmentAsync(request.InstanceId, userId);
            return Ok(result);
        }

        #endregion

        #region Facility Decommissioning Workflow

        /// <summary>
        /// Start Facility Decommissioning workflow
        /// </summary>
        [HttpPost("workflows/facility-decommissioning")]
        public async Task<ActionResult<Beep.OilandGas.Models.Processes.ProcessInstance>> StartFacilityDecommissioningProcess(
            [FromBody] StartFacilityDecommissioningRequest request)
        {
            var userId = User.ActingUserId();
            var currentFieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(currentFieldId))
            {
                    return BadRequest(new { error = "No active field selected." });
            }

            if (string.IsNullOrWhiteSpace(request.FacilityId))
            {
                    return BadRequest(new { error = "Facility ID is required." });
            }

            var instance = await _decommissioningProcessService.StartFacilityDecommissioningProcessAsync(
                request.FacilityId, 
                currentFieldId, 
                userId);
            
            return Ok(instance);
        }

        /// <summary>
        /// Plan decommissioning
        /// </summary>
        [HttpPost("workflows/plan-decommissioning")]
        public async Task<ActionResult<bool>> PlanDecommissioning([FromBody] PlanDecommissioningRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(request.InstanceId))
            {
                    return BadRequest(new { error = "Instance ID is required." });
            }

            var result = await _decommissioningProcessService.PlanDecommissioningAsync(
                request.InstanceId, 
                new PROCESS_STEP_DATA { Data = request.PlanData ?? new Dictionary<string, object>() }, 
                userId);
            
            return Ok(result);
        }

        /// <summary>
        /// Remove equipment
        /// </summary>
        [HttpPost("workflows/remove-equipment")]
        public async Task<ActionResult<bool>> RemoveEquipment([FromBody] RemoveEquipmentRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(request.InstanceId))
            {
                    return BadRequest(new { error = "Instance ID is required." });
            }

            var result = await _decommissioningProcessService.RemoveEquipmentAsync(
                request.InstanceId, 
                new PROCESS_STEP_DATA { Data = request.RemovalData ?? new Dictionary<string, object>() }, 
                userId);
            
            return Ok(result);
        }

        /// <summary>
        /// Cleanup site
        /// </summary>
        [HttpPost("workflows/cleanup-site")]
        public async Task<ActionResult<bool>> CleanupSite([FromBody] CleanupSiteRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(request.InstanceId))
            {
                    return BadRequest(new { error = "Instance ID is required." });
            }

            var result = await _decommissioningProcessService.CleanupSiteAsync(
                request.InstanceId, 
                new PROCESS_STEP_DATA { Data = request.CleanupData ?? new Dictionary<string, object>() }, 
                userId);
            
            return Ok(result);
        }

        /// <summary>
        /// Obtain regulatory closure
        /// </summary>
        [HttpPost("workflows/obtain-regulatory-closure")]
        public async Task<ActionResult<bool>> ObtainRegulatoryClosure([FromBody] ObtainRegulatoryClosureRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(request.InstanceId))
            {
                    return BadRequest(new { error = "Instance ID is required." });
            }

            var result = await _decommissioningProcessService.ObtainRegulatoryClosureAsync(request.InstanceId, userId);
            return Ok(result);
        }

        /// <summary>
        /// Complete decommissioning
        /// </summary>
        [HttpPost("workflows/complete-decommissioning")]
        public async Task<ActionResult<bool>> CompleteDecommissioning([FromBody] CompleteDecommissioningRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(request.InstanceId))
            {
                    return BadRequest(new { error = "Instance ID is required." });
            }

            var result = await _decommissioningProcessService.CompleteDecommissioningAsync(request.InstanceId, userId);
            return Ok(result);
        }

        #endregion

        // ── P&A Programme Approval ─────────────────────────────────────────────────────

        /// <summary>PATCH wells-abandoned/{id}/approve — mark a P&A programme approved.</summary>
        [HttpPatch("wells-abandoned/{id}/approve")]
        public async Task<ActionResult> ApprovePAAsync(string id)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
            var abandonment = await decommissioningService.GetWellAbandonmentForFieldAsync(fieldId, id);
                if (abandonment == null) return NotFound(new { error = $"P&A record {id} not found." });

            _logger.LogInformation("P&A programme {Id} approved by {UserId}", id, userId);
            return NoContent();
        }
    }
}
