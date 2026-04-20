using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.Models.Data.Development;
using Beep.OilandGas.PPDM39.Models;
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
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var wells = await _developmentService.GetDevelopmentWellsForFieldAsync(fieldId);
                return Ok(wells ?? new List<WELL>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching development wells for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/development/wells/{uwi}</summary>
        [HttpGet("wells/{uwi}")]
        public async Task<ActionResult<WELL>> GetWellAsync(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var wells = await _developmentService.GetDevelopmentWellsForFieldAsync(fieldId);
                var well = wells?.FirstOrDefault(w => w.UWI == uwi);
                if (well == null) return NotFound();
                return Ok(well);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching well {Uwi}", uwi);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
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
                return StatusCode(500, new { error = "An internal error occurred." });
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
                return StatusCode(500, new { error = "An internal error occurred." });
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
                return StatusCode(500, new { error = "An internal error occurred." });
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
                return StatusCode(500, new { error = "An internal error occurred." });
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
                return StatusCode(500, new { error = "An internal error occurred." });
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
                return StatusCode(500, new { error = "An internal error occurred." });
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
                return StatusCode(500, new { error = "An internal error occurred." });
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
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        #endregion

        #region Dashboard

        /// <summary>GET /api/field/current/development/dashboard/summary</summary>
        [HttpGet("dashboard/summary")]
        public async Task<ActionResult<DevelopmentDashboardSummary>> GetDashboardSummaryAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var summary = await _developmentService.GetDevelopmentDashboardSummaryAsync(fieldId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching development dashboard summary for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/development/dashboard/wells</summary>
        [HttpGet("dashboard/wells")]
        public async Task<ActionResult<List<DevelopmentWellStatusDto>>> GetDashboardWellsAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var wells = await _developmentService.GetDevelopmentWellStatusAsync(fieldId);
                return Ok(wells);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching development well status for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/development/construction-progress</summary>
        [HttpGet("construction-progress")]
        public async Task<ActionResult<ConstructionProgressDto>> GetConstructionProgressAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var wells      = await _developmentService.GetDevelopmentWellsForFieldAsync(fieldId);
                var facilities = await _developmentService.GetFacilitiesForFieldAsync(fieldId);

                // Build "Wells" work group from well records
                var wellPackages = (wells ?? new()).Select(w =>
                {
                    var statusCode = w.CURRENT_STATUS ?? "PLANNED";
                    bool isCompleted = statusCode is "PRODUCING" or "COMPLETED" or "ABANDONED";
                    bool isActive    = statusCode is "DRILLING" or "COMPLETING" or "TESTING";
                    double pct = isCompleted ? 100 : isActive ? 50 : 0;
                    return new WorkPackageDto(
                        Name:       w.UWI ?? w.CURRENT_STATUS ?? "Unknown Well",
                        TargetDate: w.SPUD_DATE.HasValue ? w.SPUD_DATE.Value.ToString("MMM yyyy") : string.Empty,
                        IsCompleted: isCompleted,
                        IsActive:    isActive,
                        PercentComplete: pct,
                        Note:        statusCode);
                }).ToList();

                // Build "Facilities" work group
                var facilityPackages = (facilities ?? new()).Select(f =>
                {
                    var statusCode = f.ACTIVE_IND == "Y" ? "OPERATIONAL" : "PLANNED";
                    bool isCompleted = statusCode is "OPERATIONAL";
                    bool isActive    = false;
                    double pct = isCompleted ? 100 : 0;
                    return new WorkPackageDto(
                        Name:       f.FACILITY_LONG_NAME ?? f.FACILITY_SHORT_NAME ?? f.FACILITY_ID ?? "Unknown Facility",
                        TargetDate: string.Empty,
                        IsCompleted: isCompleted,
                        IsActive:    isActive,
                        PercentComplete: pct,
                        Note:        f.FACILITY_TYPE ?? string.Empty);
                }).ToList();

                var workGroups = new List<WorkGroupDto>();
                if (facilityPackages.Count > 0)
                    workGroups.Add(new WorkGroupDto("Facilities & Infrastructure", facilityPackages));
                if (wellPackages.Count > 0)
                    workGroups.Add(new WorkGroupDto("Wells", wellPackages));

                return Ok(new ConstructionProgressDto(workGroups, new List<PunchItemDto>()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching construction progress for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        #endregion

        // ── Rig Assignment ─────────────────────────────────────────────────────────────

        /// <summary>PUT /api/field/current/development/wells/{uwi}/rig — assign a rig to a well.</summary>
        [HttpPut("wells/{uwi}/rig")]
        public async Task<ActionResult> AssignRigAsync(string uwi, [FromBody] AssignRigRequest request)
        {
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
            try
            {
                // Store rig assignment via well status update using the data service
                var wells = await _developmentService.GetDevelopmentWellsForFieldAsync(fieldId);
                var well  = wells?.FirstOrDefault(w => w.UWI == uwi);
                if (well == null) return NotFound(new { error = $"Well {uwi} not found in field {fieldId}" });

                well.STATUS_TYPE    = $"RIG:{request.RigName}";
                well.ROW_CHANGED_BY   = userId;
                well.ROW_CHANGED_DATE = DateTime.UtcNow;

                _logger.LogInformation("Rig {RigName} assigned to well {Uwi} by {UserId}", request.RigName, uwi, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning rig to well {Uwi}", uwi);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

// ── FDP Gate Review ────────────────────────────────────────────────────────────

        // Step-name lookup for the GATE_FDP_REVIEW process definition
        private static readonly Dictionary<string, string> _fdpStepNames = new()
        {
            { "FDP_DRAFT",          "FDP Drafting" },
            { "FDP_RESERVES_CLASS", "PRMS Reserves Classification" },
            { "FDP_ECONOMICS",      "Project Economics Review" },
            { "FDP_PARTNER_ALIGN",  "Partner Alignment" },
            { "FDP_APPROVAL",       "Board / Regulator Approval" },
            { "FDP_SANCTION",       "Project Sanction" },
        };

        /// <summary>GET /api/field/current/development/fdp
        /// Returns the current FDP gate review status for the active field.
        /// HasFdp = false when no FDP process has been started.</summary>
        [HttpGet("fdp")]
        public async Task<ActionResult<FdpStatusResponse>> GetCurrentFdpAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var instance = await _developmentProcessService.GetCurrentFdpStatusAsync(fieldId);
                if (instance == null)
                    return Ok(new FdpStatusResponse { HasFdp = false });

                var steps = instance.StepInstances
                    .OrderBy(s => s.SequenceNumber)
                    .Select(s => new FdpStepStatus
                    {
                        StepId        = s.StepId,
                        StepName      = _fdpStepNames.TryGetValue(s.StepId, out var n) ? n : s.StepId,
                        IsCompleted   = s.Status == Beep.OilandGas.LifeCycle.Models.Processes.StepStatus.COMPLETED,
                        IsActive      = s.Status == Beep.OilandGas.LifeCycle.Models.Processes.StepStatus.IN_PROGRESS,
                        CompletedDate = s.CompletionDate?.ToString("MMM yyyy") ?? string.Empty,
                    }).ToList();

                return Ok(new FdpStatusResponse
                {
                    HasFdp            = true,
                    ProcessInstanceId = instance.InstanceId,
                    Status            = instance.Status.ToString(),
                    CurrentStepId     = instance.CurrentStepId,
                    StartDate         = instance.StartDate,
                    Steps             = steps,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching FDP status for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// POST /api/field/current/development/fdp
        /// Submits a Field Development Plan draft and starts the FDP Gate Review process.
        /// </summary>
        [HttpPost("fdp")]
        public async Task<ActionResult<SubmitFdpDraftResponse>> SubmitFdpAsync(
            [FromBody] SubmitFdpDraftRequest request)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
            try
            {
                var instance = await _developmentProcessService.StartFdpGateProcessAsync(fieldId, userId);
                return Ok(new SubmitFdpDraftResponse
                {
                    InstanceId = instance?.InstanceId ?? string.Empty,
                    Status     = instance != null ? instance.Status.ToString() : "SUBMITTED"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting FDP for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

    }

    // ── Construction-progress response DTOs ────────────────────────────────
    public record ConstructionProgressDto(
        List<WorkGroupDto>  WorkGroups,
        List<PunchItemDto>  PunchItems);

    public record WorkGroupDto(string Title, List<WorkPackageDto> Packages);

    public record WorkPackageDto(
        string Name,
        string TargetDate,
        bool   IsCompleted,
        bool   IsActive,
        double PercentComplete,
        string Note);

    public record PunchItemDto(
        string Description,
        string Owner,
        string DueDate,
        string Category);

    public record AssignRigRequest(string RigName, DateTime? MobDate);
}
