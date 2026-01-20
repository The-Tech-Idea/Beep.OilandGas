using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.LifeCycle;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    /// <summary>
    /// API controller for Development phase business workflows, field-scoped
    /// 
    /// NOTE: For CRUD operations (Create, Read, Update, Delete), please use DataManagementController:
    /// - Get pools: GET /api/datamanagement/POOL
    /// - Get pool: GET /api/datamanagement/POOL/{id}
    /// - Create pool: POST /api/datamanagement/POOL
    /// - Update pool: PUT /api/datamanagement/POOL/{id}
    /// - Get development wells: GET /api/datamanagement/WELL with filters
    /// - Create development well: POST /api/datamanagement/WELL
    /// - Get wellbores: GET /api/datamanagement/WELL with filters
    /// - Get facilities: GET /api/datamanagement/FACILITY
    /// - Create facility: POST /api/datamanagement/FACILITY
    /// - Get pipelines: GET /api/datamanagement/PIPELINE
    /// - Create pipeline: POST /api/datamanagement/PIPELINE
    /// 
    /// This controller focuses on development workflow processes via DevelopmentProcessService.
    /// </summary>
    [ApiController]
    [Route("api/field/current/development")]
    public class DevelopmentController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly Beep.OilandGas.LifeCycle.Services.Development.Processes.DevelopmentProcessService _developmentProcessService;
        private readonly ILogger<DevelopmentController> _logger;

        public DevelopmentController(
            IFieldOrchestrator fieldOrchestrator,
            Beep.OilandGas.LifeCycle.Services.Development.Processes.DevelopmentProcessService developmentProcessService,
            ILogger<DevelopmentController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _developmentProcessService = developmentProcessService ?? throw new ArgumentNullException(nameof(developmentProcessService));
            _logger = logger;
        }

        // ============================================
        // DEVELOPMENT WORKFLOW ENDPOINTS
        // ============================================

        #region Pool Definition Workflow

        /// <summary>
        /// Start Pool Definition workflow
        /// </summary>
        [HttpPost("workflows/pool-definition")]
        public async Task<ActionResult<Beep.OilandGas.LifeCycle.Models.Processes.ProcessInstance>> StartPoolDefinitionProcess(
            [FromBody] StartPoolDefinitionRequest request)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(request.PoolId))
                {
                    return BadRequest(new { error = "PoolId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var instance = await _developmentProcessService.StartPoolDefinitionProcessAsync(
                    request.PoolId, 
                    currentFieldId, 
                    request.UserId);
                
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Pool Definition process");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delineate pool
        /// </summary>
        [HttpPost("workflows/delineate-pool")]
        public async Task<ActionResult<bool>> DelineatePool([FromBody] DelineatePoolRequest request)
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

                var result = await _developmentProcessService.DelineatePoolAsync(
                    request.InstanceId, 
                    request.DelineationData ?? new Dictionary<string, object>(), 
                    request.UserId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delineating pool");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Assign reserves to pool
        /// </summary>
        [HttpPost("workflows/assign-reserves")]
        public async Task<ActionResult<bool>> AssignReserves([FromBody] AssignReservesRequest request)
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

                var result = await _developmentProcessService.AssignReservesAsync(
                    request.InstanceId, 
                    request.ReserveData ?? new Dictionary<string, object>(), 
                    request.UserId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning reserves");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Approve pool
        /// </summary>
        [HttpPost("workflows/approve-pool")]
        public async Task<ActionResult<bool>> ApprovePool([FromBody] ApprovePoolRequest request)
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

                var result = await _developmentProcessService.ApprovePoolAsync(request.InstanceId, request.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving pool");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Activate pool
        /// </summary>
        [HttpPost("workflows/activate-pool")]
        public async Task<ActionResult<bool>> ActivatePool([FromBody] ActivatePoolRequest request)
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

                var result = await _developmentProcessService.ActivatePoolAsync(request.InstanceId, request.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating pool");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Facility Development Workflow

        /// <summary>
        /// Start Facility Development workflow
        /// </summary>
        [HttpPost("workflows/facility-development")]
        public async Task<ActionResult<Beep.OilandGas.LifeCycle.Models.Processes.ProcessInstance>> StartFacilityDevelopmentProcess(
            [FromBody] StartFacilityDevelopmentRequest request)
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

                var instance = await _developmentProcessService.StartFacilityDevelopmentProcessAsync(
                    request.FacilityId, 
                    currentFieldId, 
                    request.UserId);
                
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Facility Development process");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Well Development Workflow

        /// <summary>
        /// Start Well Development workflow
        /// </summary>
        [HttpPost("workflows/well-development")]
        public async Task<ActionResult<Beep.OilandGas.LifeCycle.Models.Processes.ProcessInstance>> StartWellDevelopmentProcess(
            [FromBody] StartWellDevelopmentRequest request)
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

                var instance = await _developmentProcessService.StartWellDevelopmentProcessAsync(
                    request.WellId, 
                    currentFieldId, 
                    request.UserId);
                
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Well Development process");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Pipeline Development Workflow

        /// <summary>
        /// Start Pipeline Development workflow
        /// </summary>
        [HttpPost("workflows/pipeline-development")]
        public async Task<ActionResult<Beep.OilandGas.LifeCycle.Models.Processes.ProcessInstance>> StartPipelineDevelopmentProcess(
            [FromBody] StartPipelineDevelopmentRequest request)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(request.PipelineId))
                {
                    return BadRequest(new { error = "PipelineId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var instance = await _developmentProcessService.StartPipelineDevelopmentProcessAsync(
                    request.PipelineId, 
                    currentFieldId, 
                    request.UserId);
                
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Pipeline Development process");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

    }
}
