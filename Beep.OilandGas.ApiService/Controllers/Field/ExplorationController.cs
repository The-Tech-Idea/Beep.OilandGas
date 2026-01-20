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
    /// API controller for Exploration phase business workflows, field-scoped
    /// 
    /// NOTE: For CRUD operations (Create, Read, Update, Delete), please use DataManagementController:
    /// - Get prospects: GET /api/datamanagement/PROSPECT?filters=[{"field":"FIELD_ID","operator":"equals","value":"{fieldId}"}]
    /// - Get prospect: GET /api/datamanagement/PROSPECT/{id}
    /// - Create prospect: POST /api/datamanagement/PROSPECT
    /// - Update prospect: PUT /api/datamanagement/PROSPECT/{id}
    /// - Delete prospect: DELETE /api/datamanagement/PROSPECT/{id}
    /// - Get seismic surveys: GET /api/datamanagement/SEIS_ACQTN_SURVEY
    /// - Create seismic survey: POST /api/datamanagement/SEIS_ACQTN_SURVEY
    /// - Get seismic lines: GET /api/datamanagement/SEIS_LINE
    /// - Get exploratory wells: GET /api/datamanagement/WELL with filters
    /// 
    /// This controller focuses on exploration workflow processes via ExplorationProcessService.
    /// </summary>
    [ApiController]
    [Route("api/field/current/exploration")]
    public class ExplorationController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly Beep.OilandGas.LifeCycle.Services.Exploration.Processes.ExplorationProcessService _explorationProcessService;
        private readonly ILogger<ExplorationController> _logger;

        public ExplorationController(
            IFieldOrchestrator fieldOrchestrator,
            Beep.OilandGas.LifeCycle.Services.Exploration.Processes.ExplorationProcessService explorationProcessService,
            ILogger<ExplorationController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _explorationProcessService = explorationProcessService ?? throw new ArgumentNullException(nameof(explorationProcessService));
            _logger = logger;
        }

        // ============================================
        // EXPLORATION WORKFLOW ENDPOINTS
        // ============================================

        /// <summary>
        /// Start Lead to Prospect workflow
        /// </summary>
        [HttpPost("workflows/lead-to-prospect")]
        public async Task<ActionResult<Beep.OilandGas.LifeCycle.Models.Processes.ProcessInstance>> StartLeadToProspectProcess(
            [FromBody] StartLeadToProspectRequest request)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(request.LeadId))
                {
                    return BadRequest(new { error = "LeadId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var instance = await _explorationProcessService.StartLeadToProspectProcessAsync(
                    request.LeadId, 
                    currentFieldId, 
                    request.UserId);
                
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Lead to Prospect process");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Evaluate lead
        /// </summary>
        [HttpPost("workflows/evaluate-lead")]
        public async Task<ActionResult<bool>> EvaluateLead([FromBody] EvaluateLeadRequest request)
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

                var result = await _explorationProcessService.EvaluateLeadAsync(
                    request.InstanceId, 
                    request.EvaluationData ?? new Dictionary<string, object>(), 
                    request.UserId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating lead");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Approve lead
        /// </summary>
        [HttpPost("workflows/approve-lead")]
        public async Task<ActionResult<bool>> ApproveLead([FromBody] ApproveLeadRequest request)
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

                var result = await _explorationProcessService.ApproveLeadAsync(request.InstanceId, request.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving lead");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Start Prospect to Discovery workflow
        /// </summary>
        [HttpPost("workflows/prospect-to-discovery")]
        public async Task<ActionResult<Beep.OilandGas.LifeCycle.Models.Processes.ProcessInstance>> StartProspectToDiscoveryProcess(
            [FromBody] StartProspectToDiscoveryRequest request)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(request.ProspectId))
                {
                    return BadRequest(new { error = "ProspectId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var instance = await _explorationProcessService.StartProspectToDiscoveryProcessAsync(
                    request.ProspectId, 
                    currentFieldId, 
                    request.UserId);
                
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Prospect to Discovery process");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Start Discovery to Development workflow
        /// </summary>
        [HttpPost("workflows/discovery-to-development")]
        public async Task<ActionResult<Beep.OilandGas.LifeCycle.Models.Processes.ProcessInstance>> StartDiscoveryToDevelopmentProcess(
            [FromBody] StartDiscoveryToDevelopmentRequest request)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(request.DiscoveryId))
                {
                    return BadRequest(new { error = "DiscoveryId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var instance = await _explorationProcessService.StartDiscoveryToDevelopmentProcessAsync(
                    request.DiscoveryId, 
                    currentFieldId, 
                    request.UserId);
                
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Discovery to Development process");
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
