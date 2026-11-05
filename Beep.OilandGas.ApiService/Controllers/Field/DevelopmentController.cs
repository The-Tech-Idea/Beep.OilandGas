using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.Models.Data.Development;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    /// <summary>
    /// API controller for field-scoped development planning queries, selected CRUD, and workflow processes.
    /// </summary>
    [ApiController]
    [Route("api/field/current/development")]
    [RequireCurrentFieldAccess]
    public class DevelopmentController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly Beep.OilandGas.LifeCycle.Services.Development.Processes.DevelopmentProcessService _developmentProcessService;
        private readonly Beep.OilandGas.LifeCycle.Services.Development.PPDMDevelopmentService _developmentService;
        private readonly ILogger<DevelopmentController> _logger;

        public DevelopmentController(
            IFieldOrchestrator fieldOrchestrator,
            Beep.OilandGas.LifeCycle.Services.Development.Processes.DevelopmentProcessService developmentProcessService,
            Beep.OilandGas.LifeCycle.Services.Development.PPDMDevelopmentService developmentService,
            ILogger<DevelopmentController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _developmentProcessService = developmentProcessService ?? throw new ArgumentNullException(nameof(developmentProcessService));
            _developmentService = developmentService ?? throw new ArgumentNullException(nameof(developmentService));
            _logger = logger;
        }

        // ============================================
        // DATA QUERY ENDPOINTS
        // ============================================

        /// <summary>GET /api/field/current/development/wells</summary>
        [HttpGet("wells")]
        public async Task<ActionResult<List<WELL>>> GetWellsAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            var wells = await _developmentService.GetDevelopmentWellsForFieldAsync(fieldId);
            return Ok(wells ?? new List<WELL>());
        }

        /// <summary>GET /api/field/current/development/pools</summary>
        [HttpGet("pools")]
        public async Task<ActionResult<List<POOL>>> GetPoolsAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            var pools = await _developmentService.GetPoolsForFieldAsync(fieldId);
            return Ok(pools ?? new List<POOL>());
        }

        /// <summary>POST /api/field/current/development/pools</summary>
        [HttpPost("pools")]
        public async Task<ActionResult<POOL>> CreatePoolAsync([FromBody] PoolRequest request)
        {
            var userId = User.ActingUserId();
            if (request == null)
                return BadRequest(new { error = "Pool request is required." });
            if (string.IsNullOrWhiteSpace(request.PoolName))
                return BadRequest(new { error = "Pool name is required." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            var pool = await _developmentService.CreatePoolForFieldAsync(fieldId, request, userId);
            return Ok(pool);
        }

        /// <summary>PUT /api/field/current/development/pools/{poolId}</summary>
        [HttpPut("pools/{poolId}")]
        public async Task<ActionResult<POOL>> UpdatePoolAsync(string poolId, [FromBody] PoolRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(poolId))
                return BadRequest(new { error = "Pool ID is required." });
            if (request == null)
                return BadRequest(new { error = "Pool request is required." });
            if (string.IsNullOrWhiteSpace(request.PoolName))
                return BadRequest(new { error = "Pool name is required." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            var pool = await _developmentService.UpdatePoolForFieldAsync(fieldId, poolId, request, userId);
            return Ok(pool);
        }

        /// <summary>GET /api/field/current/development/wells/{uwi}</summary>
        [HttpGet("wells/{uwi}")]
        public async Task<ActionResult<WELL>> GetWellAsync(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            var wells = await _developmentService.GetDevelopmentWellsForFieldAsync(fieldId);
            var well = wells?.FirstOrDefault(w => w.UWI == uwi);
            if (well == null) return NotFound(new { error = $"Well {uwi} not found in field {fieldId}." });
            return Ok(well);
        }

        // ============================================
        // DEVELOPMENT WORKFLOW ENDPOINTS
        // ============================================

        #region Pool Definition Workflow

        /// <summary>
        /// Start Pool Definition workflow
        /// </summary>
        [HttpPost("workflows/pool-definition")]
        public async Task<ActionResult<Beep.OilandGas.Models.Processes.ProcessInstance>> StartPoolDefinitionProcess(
            [FromBody] StartPoolDefinitionRequest request)
        {
            var userId = User.ActingUserId();
            var currentFieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(currentFieldId))
            {
                    return BadRequest(new { error = "No active field selected." });
            }

            if (string.IsNullOrWhiteSpace(request.PoolId))
            {
                    return BadRequest(new { error = "Pool ID is required." });
            }

            var instance = await _developmentProcessService.StartPoolDefinitionProcessAsync(
                request.PoolId, 
                currentFieldId, 
                userId);
            
            return Ok(instance);
        }

        /// <summary>
        /// Delineate pool
        /// </summary>
        [HttpPost("workflows/delineate-pool")]
        public async Task<ActionResult<bool>> DelineatePool([FromBody] DelineatePoolRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(request.InstanceId))
            {
                    return BadRequest(new { error = "Instance ID is required." });
            }

            var result = await _developmentProcessService.DelineatePoolAsync(
                request.InstanceId, 
                new PROCESS_STEP_DATA { Data = request.DelineationData ?? new Dictionary<string, object>() }, 
                userId);
            
            return Ok(result);
        }

        /// <summary>
        /// Assign reserves to pool
        /// </summary>
        [HttpPost("workflows/assign-reserves")]
        public async Task<ActionResult<bool>> AssignReserves([FromBody] AssignReservesRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(request.InstanceId))
            {
                    return BadRequest(new { error = "Instance ID is required." });
            }

            var result = await _developmentProcessService.AssignReservesAsync(
                request.InstanceId, 
                new PROCESS_STEP_DATA { Data = request.ReserveData ?? new Dictionary<string, object>() }, 
                userId);
            
            return Ok(result);
        }

        /// <summary>
        /// Approve pool
        /// </summary>
        [HttpPost("workflows/approve-pool")]
        public async Task<ActionResult<bool>> ApprovePool([FromBody] ApprovePoolRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(request.InstanceId))
            {
                    return BadRequest(new { error = "Instance ID is required." });
            }

            var result = await _developmentProcessService.ApprovePoolAsync(request.InstanceId, userId);
            return Ok(result);
        }

        /// <summary>
        /// Activate pool
        /// </summary>
        [HttpPost("workflows/activate-pool")]
        public async Task<ActionResult<bool>> ActivatePool([FromBody] ActivatePoolRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(request.InstanceId))
            {
                    return BadRequest(new { error = "Instance ID is required." });
            }

            var result = await _developmentProcessService.ActivatePoolAsync(request.InstanceId, userId);
            return Ok(result);
        }

        #endregion

        #region Facility Development Workflow

        /// <summary>
        /// Start Facility Development workflow
        /// </summary>
        [HttpPost("workflows/facility-development")]
        public async Task<ActionResult<Beep.OilandGas.Models.Processes.ProcessInstance>> StartFacilityDevelopmentProcess(
            [FromBody] StartFacilityDevelopmentRequest request)
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

            var instance = await _developmentProcessService.StartFacilityDevelopmentProcessAsync(
                request.FacilityId, 
                currentFieldId, 
                userId);
            
            return Ok(instance);
        }

        #endregion

        #region Well Development Workflow

        /// <summary>
        /// Start Well Development workflow
        /// </summary>
        [HttpPost("workflows/well-development")]
        public async Task<ActionResult<Beep.OilandGas.Models.Processes.ProcessInstance>> StartWellDevelopmentProcess(
            [FromBody] StartWellDevelopmentRequest request)
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

            var instance = await _developmentProcessService.StartWellDevelopmentProcessAsync(
                request.WellId, 
                currentFieldId, 
                userId);
            
            return Ok(instance);
        }

        #endregion

        #region Pipeline Development Workflow

        /// <summary>
        /// Start Pipeline Development workflow
        /// </summary>
        [HttpPost("workflows/pipeline-development")]
        public async Task<ActionResult<Beep.OilandGas.Models.Processes.ProcessInstance>> StartPipelineDevelopmentProcess(
            [FromBody] StartPipelineDevelopmentRequest request)
        {
            var userId = User.ActingUserId();
            var currentFieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(currentFieldId))
            {
                    return BadRequest(new { error = "No active field selected." });
            }

            if (string.IsNullOrWhiteSpace(request.PipelineId))
            {
                    return BadRequest(new { error = "Pipeline ID is required." });
            }

            var instance = await _developmentProcessService.StartPipelineDevelopmentProcessAsync(
                request.PipelineId, 
                currentFieldId, 
                userId);
            
            return Ok(instance);
        }

        #endregion

        #region Dashboard

        /// <summary>GET /api/field/current/development/dashboard/summary</summary>
        [HttpGet("dashboard/summary")]
        public async Task<ActionResult<DevelopmentDashboardSummary>> GetDashboardSummaryAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            var summary = await _developmentService.GetDevelopmentDashboardSummaryAsync(fieldId);
            return Ok(summary);
        }

        /// <summary>GET /api/field/current/development/dashboard/wells</summary>
        [HttpGet("dashboard/wells")]
        public async Task<ActionResult<List<DevelopmentWellStatusDto>>> GetDashboardWellsAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            var wells = await _developmentService.GetDevelopmentWellStatusAsync(fieldId);
            return Ok(wells);
        }

        #endregion

        // ── Rig Assignment ─────────────────────────────────────────────────────────────

        /// <summary>PUT /api/field/current/development/wells/{uwi}/rig — assign a rig to a well.</summary>
        [HttpPut("wells/{uwi}/rig")]
        public async Task<ActionResult> AssignRigAsync(string uwi, [FromBody] AssignRigRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            var success = await _developmentService.AssignRigToWellAsync(fieldId, uwi, request.RigName, userId);
            if (!success)
                return NotFound(new { error = $"Well {uwi} not found in field {fieldId}." });

            _logger.LogInformation("Rig {RigName} assigned to well {Uwi} by {UserId}", request.RigName, uwi, userId);
            return NoContent();
        }


    }

    public record AssignRigRequest(string RigName, DateTime? MobDate);
}
