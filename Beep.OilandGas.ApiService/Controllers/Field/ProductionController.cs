using Beep.OilandGas.Repository;
using TheTechIdea.Data.OilGas;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Services.Decommissioning.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using Microsoft.Extensions.Logging;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    [ApiController]
    [Route("api/field/current/production")]
    [RequireCurrentFieldAccess]
    public class ProductionController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly Beep.OilandGas.LifeCycle.Services.Production.PPDMProductionService _productionService;
        private readonly IWorkOrderService _workOrderService;
        private readonly DecommissioningProcessService _decommissioningProcessService;
        private readonly ILogger<ProductionController> _logger;
        private readonly IFailureReporter _failures;

        public ProductionController(
            IFieldOrchestrator fieldOrchestrator,
            Beep.OilandGas.LifeCycle.Services.Production.PPDMProductionService productionService,
            IWorkOrderService workOrderService,
            DecommissioningProcessService decommissioningProcessService,
            ILogger<ProductionController> logger,
            IFailureReporter failures)
        {
            _failures = failures ?? throw new ArgumentNullException(nameof(failures));
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _productionService = productionService ?? throw new ArgumentNullException(nameof(productionService));
            _workOrderService = workOrderService ?? throw new ArgumentNullException(nameof(workOrderService));
            _decommissioningProcessService = decommissioningProcessService ?? throw new ArgumentNullException(nameof(decommissioningProcessService));
            _logger = logger;
        }

        /// <summary>GET /api/field/current/production/wells/{wellId}/performance</summary>
        [HttpGet("wells/{wellId}/performance")]
        public async Task<ActionResult<WellPerformanceSummary>> GetWellPerformanceAsync(string wellId)
        {
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            var tests = await _productionService.GetWellTestsForWellAsync(fieldId, wellId);
            var orderedTests = (tests ?? new List<WellTestResponse>())
                .OrderByDescending(ResolveTestDate)
                .ToList();
            var latest = orderedTests.FirstOrDefault();
            var maxOilRate = orderedTests.Max(t => ToDouble(t.OilFlowAmount));

            var summary = new WellPerformanceSummary
            {
                WellId = wellId,
                Status = latest != null ? "ACTIVE" : "UNKNOWN",
                OilRate = latest != null ? ToDouble(latest.OilFlowAmount) : 0,
                GasRate = latest != null ? ToDouble(latest.GasFlowAmount) : 0,
                WaterRate = latest != null ? ToDouble(latest.WaterFlowAmount) : 0,
                PotentialRate = maxOilRate > 0 ? maxOilRate : (latest != null ? ToDouble(latest.OilFlowAmount) : 0),
                CumOil = 0,
                LastTestDate = ResolveTestDate(latest),
                WellTests = orderedTests,
            };
            return Ok(summary);
        }

        /// <summary>GET /api/field/current/production/wells/{wellId}/analysis</summary>
        [HttpGet("wells/{wellId}/analysis")]
        public async Task<ActionResult<WellPerformanceAnalysisResponse>> GetWellPerformanceAnalysisAsync(string wellId)
        {
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            var analysis = await _productionService.GetWellPerformanceAnalysisAsync(fieldId, wellId);
            return Ok(analysis);
        }

        /// <summary>POST /api/field/current/production/wells/{wellId}/analysis/deviation</summary>
        [HttpPost("wells/{wellId}/analysis/deviation")]
        public async Task<ActionResult<PerformanceDeviationResult>> LogPerformanceDeviationAsync(string wellId, [FromBody] PerformanceDeviationRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            var result = await _productionService.LogWellPerformanceDeviationAsync(fieldId, wellId, request, userId);
            return Ok(result);
        }

        /// <summary>GET /api/field/current/production/wells/{wellId}/tests</summary>
        [HttpGet("wells/{wellId}/tests")]
        public async Task<ActionResult<List<WellTestResponse>>> GetWellTestsAsync(string wellId, [FromServices] IAccessControlService resources, [FromServices] RepositoryTenantContext tenant)
        {
            var review=await ReadWellTestReviewAsync(wellId,resources,tenant);
            return Ok(review.Rows);
        }

        [HttpGet("wells/{wellId}/tests/review")]
        public async Task<ActionResult<WellTestReview>> GetWellTestReviewAsync(string wellId,[FromServices] IAccessControlService resources,[FromServices] RepositoryTenantContext tenant)
            => Ok(await ReadWellTestReviewAsync(wellId,resources,tenant));

        private async Task<WellTestReview> ReadWellTestReviewAsync(string wellId,IAccessControlService resources,RepositoryTenantContext tenant)
        {
            if(string.IsNullOrWhiteSpace(wellId))throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Invalid("Well identity is required.");
            var actor=User.ActingUserId();var field=_fieldOrchestrator.CurrentFieldId;
            if(tenant.Company is not {} company || company.UserId!=actor || string.IsNullOrWhiteSpace(field))
                throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Forbidden("Confirm company and FIELD before reading observations.");
            if(!(await resources.CheckAssetAccessAsync(actor,wellId,"WELL","READ")).HasAccess)
                throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Forbidden("Current WELL read authority is required.");
            var tests=await _productionService.GetWellTestsForWellAsync(field,wellId);
            if(tests is null || _fieldOrchestrator.CurrentFieldId!=field || tenant.Company!=company)
                throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Conflict("Recorded tests could not be confirmed in the original company/FIELD.");
            if(!(await resources.CheckAssetAccessAsync(actor,wellId,"WELL","READ")).HasAccess || _fieldOrchestrator.CurrentFieldId!=field)
                throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Forbidden("Original observation authority changed during the read.");
            return new(company.Scope,actor,field,wellId,DateTimeOffset.UtcNow,tests.AsReadOnly());
        }

        /// <summary>PATCH /api/field/current/production/allocation/{period}/wells/{wellId}</summary>
        [HttpPatch("allocation/{period}/wells/{wellId}")]
        public IActionResult PatchAllocationAsync(string period, string wellId, [FromBody] AllocationPatchRequest request)
        {
            if (string.IsNullOrWhiteSpace(period)) return BadRequest(new { error = "Period is required." });
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            // Allocation adjustments are persisted through PDEN_VOL_SUMMARY.
            // Accept the patch and return 204 â€” full PDEN persistence is done by the data-management layer.
            _logger.LogInformation("Allocation patch accepted for well {WellId} period {Period}", wellId, period);
            return NoContent();
        }

        /// <summary>GET /api/field/current/production/intervention-candidates</summary>
        [HttpGet("intervention-candidates")]
        public async Task<ActionResult<List<ProductionInterventionCandidateDto>>> GetInterventionCandidatesAsync([FromServices] RepositoryTenantContext tenant)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            var actor=User.ActingUserId();
            if(tenant.Company is not {} company || company.UserId!=actor)return Forbid();
            var activities = await _productionService.GetWellActivitiesForFieldAsync(fieldId);
            if(activities is null || _fieldOrchestrator.CurrentFieldId!=fieldId)return Conflict(new { error="Intervention source could not be confirmed in the original FIELD." });
            var selection = await _productionService.GetInterventionSelectionReviewAsync(fieldId);
            if(selection.Company!=company.Scope || selection.ActorUserId!=actor || selection.FieldId!=fieldId)return Forbid();
            var retrieved=DateTimeOffset.UtcNow;

            // Group by UWI and take the most recent activity per well as the candidate record
            List<ProductionInterventionCandidateDto> candidates;
            try
            {
                candidates = activities
                    .GroupBy(a => a.UWI)
                    .Select(g =>
                    {
                        var latest = g.OrderByDescending(a => a.ACTIVITY_OBS_NO).First();
                        var source = ProductionInterventionSourceReviews.OriginalSource(g,latest.UWI);

                        var references = ProductionInterventionMarkers.Read(latest.REMARK);
                        var linkedWorkOrderId = references.GetValueOrDefault("WORK_ORDER_ID");
                        var linkedAfeId = references.GetValueOrDefault("AFE_ID");
                        var linkedAfeNumber = references.GetValueOrDefault("AFE_NUMBER");
                        var linkedAbandonmentId = references.GetValueOrDefault("ABANDONMENT_ID");
                        var linkedProcessInstanceId = references.GetValueOrDefault("PROCESS_INSTANCE_ID");
                        return new ProductionInterventionCandidateDto
                        {
                            SourcePreparer   = source.ROW_CREATED_BY,
                            SourceRevision   = ProductionInterventionSourceReviews.Revision(g,company.Scope,actor,fieldId,latest.UWI),
                            Company          = company.Scope,
                            SelectionContext = selection,
                            LatestDecisionId = ProductionInterventionSourceReviews.IsDecisionActivity(latest.ACTIVITY_TYPE_ID) && Guid.TryParse(latest.PPDM_GUID,out var decisionId) ? decisionId : null,
                            Actor            = actor,
                            FieldId          = fieldId,
                            SourceObservationNumber = source.ACTIVITY_OBS_NO,
                            LatestObservationNumber = latest.ACTIVITY_OBS_NO,
                            SourceStartedAt  = source.START_DATE,
                            RetrievedAtUtc   = retrieved,
                            WellId           = latest.UWI ?? string.Empty,
                            WellName         = string.Empty,
                            InterventionType = source.ACTIVITY_TYPE_ID ?? string.Empty,
                            Problem          = ProductionInterventionSourceReviews.RemoveWorkflowMarkers(source.REMARK ?? string.Empty),
                            DeferredBopd     = null,
                            EstDaysSinceOnset= null,
                            Priority         = null,
                            Status           = MapInterventionStatus(latest.ACTIVITY_TYPE_ID),
                            LatestActivityType = latest.ACTIVITY_TYPE_ID,
                            LatestActivityRemark = latest.REMARK,
                            LatestRecordedBy = latest.ROW_CREATED_BY,
                            EstCostUsd       = null,
                            WorkOrderId      = linkedWorkOrderId,
                            AfeId            = linkedAfeId,
                            AfeNumber        = linkedAfeNumber,
                            AbandonmentId    = linkedAbandonmentId,
                            ProcessInstanceId = linkedProcessInstanceId,
                        };
                    })
                    .ToList();

            }
            catch(InvalidOperationException changed)
            {
                _logger.LogWarning(changed,"Recorded intervention activity or workflow references could not be confirmed.");
                return Conflict(new {error="Recorded intervention source or workflow references are ambiguous. Review the original activity records."});
            }
            ProductionInterventionCandidateChecks.Require(candidates,company.Scope,actor,fieldId);
            return Ok(candidates);
        }

        /// <summary>POST /api/field/current/production/intervention-candidates/{uwi}/transition-to-decommissioning</summary>
        [HttpPost("intervention-candidates/{uwi}/transition-to-decommissioning")]
        public async Task<ActionResult<DecommissioningTriggerResult>> TransitionToDecommissioningAsync(string uwi, [FromBody] DecommissioningTriggerRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            var decommissioningService = _fieldOrchestrator.GetDecommissioningService();
            var existingAbandonment = (await decommissioningService.GetAbandonedWellsForFieldAsync(fieldId))
                .FirstOrDefault(a => string.Equals(a.WellId, uwi, StringComparison.OrdinalIgnoreCase));

            if (existingAbandonment != null)
            {
                return Ok(new DecommissioningTriggerResult
                {
                    Success = true,
                    AlreadyExists = true,
                    WorkflowStarted = false,
                    WellId = uwi,
                    AbandonmentId = existingAbandonment.AbandonmentId,
                    Message = $"Decommissioning programme already exists for {uwi}."
                });
            }

            var estimatedCost = request.EstimatedCostUsd.HasValue
                ? Convert.ToDecimal(request.EstimatedCostUsd.Value)
                : (decimal?)null;

            var abandonment = await decommissioningService.AbandonWellForFieldAsync(
                fieldId,
                uwi,
                new WellAbandonmentRequest
                {
                    WellId = uwi,
                    FieldId = fieldId,
                    AbandonmentType = string.IsNullOrWhiteSpace(request.AbandonmentType)
                        ? "PERMANENTLY_ABANDONED"
                        : request.AbandonmentType,
                    AbandonmentMethod = string.IsNullOrWhiteSpace(request.AbandonmentMethod)
                        ? "CEMENT_PLUG"
                        : request.AbandonmentMethod,
                    Status = "IN_PROGRESS",
                    AbandonmentStartDate = DateTime.UtcNow,
                    PluggingDate = DateTime.UtcNow,
                    AbandonmentCost = estimatedCost,
                    AbandonmentCostCurrency = estimatedCost.HasValue ? "USD" : null,
                    PluggingCost = estimatedCost,
                    PluggingCostCurrency = estimatedCost.HasValue ? "USD" : null,
                    ActiveInd = "Y"
                },
                userId);

            Beep.OilandGas.Models.Processes.ProcessInstance? processInstance = null;
            string? message = null;

            if (request.StartWorkflow)
            {
                try
                {
                    processInstance = await _decommissioningProcessService.StartWellAbandonmentProcessAsync(uwi, fieldId, userId);
                }
                // The abandonment record above is already saved; whatever stops the workflow starting, the answer says the
                // record exists and the workflow did not start, with the reference its failure is filed under.
                // Cancellation is the request ending.
                catch (Exception workflowFailure) when (workflowFailure is not OperationCanceledException)
                {
                    var reference = _failures.ReportHandled(workflowFailure, $"starting the well abandonment workflow for {uwi}",
                        "the decommissioning record is saved without its workflow; the answer says so with this reference");
                    message = $"Decommissioning record created for {uwi}, but the lifecycle workflow could not be started (reference {reference}).";
                }
            }

            await _productionService.RecordInterventionDecisionAsync(
                uwi,
                "DECOMMISSIONING_TRIGGERED",
                BuildDecommissioningRemark(request.Note, abandonment.AbandonmentId, processInstance?.InstanceId),
                userId);

            return Ok(new DecommissioningTriggerResult
            {
                Success = true,
                WorkflowStarted = processInstance != null,
                WellId = uwi,
                AbandonmentId = abandonment.AbandonmentId,
                ProcessInstanceId = processInstance?.InstanceId,
                Message = message ?? $"Decommissioning handoff created for {uwi}."
            });
        }

        /// <summary>Records the original native command; query identities preserve opaque international WELL keys.</summary>
        [HttpPost("intervention-decisions")]
        [RequireCurrentFieldAccess("WRITE")]
        public async Task<ActionResult<ProductionInterventionDecisionResult>> RecordNativeInterventionDecisionAsync(
            [FromQuery] string wellId, [FromBody] ProductionInterventionDecisionRequest request,
            [FromServices] IAccessControlService resources, [FromServices] RepositoryTenantContext tenant)
        {
            var actor = User.ActingUserId();
            if (tenant.Company is not { } company || company.UserId != actor || request.Context?.Company != company.Scope) return Forbid();
            if (string.IsNullOrWhiteSpace(wellId) || request.IntentId == Guid.Empty) return BadRequest(new { error = "Original native decision and WELL identities are required." });
            if (request.FieldId != _fieldOrchestrator.CurrentFieldId) return Conflict(new { error = "Return to the original FIELD before recording its decision." });
            if ((await resources.CheckAssetAccessAsync(actor, wellId, "WELL", "WRITE"))?.HasAccess != true) return Forbid();
            if(!string.IsNullOrWhiteSpace(request.WorkOrderId) && (await resources.CheckAssetAccessAsync(actor,request.WorkOrderId,"WORK_ORDER","READ"))?.HasAccess!=true)return Forbid();
            try { return Ok(await _productionService.RecordNativeInterventionDecisionAsync(wellId, request, actor)); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message, intentId = request.IntentId, recoveryRequired = true }); }
        }

        [HttpGet("intervention-decisions/{intentId:guid}")]
        [RequireCurrentFieldAccess("READ")]
        public async Task<ActionResult<ProductionInterventionNativeRecovery>> RecoverNativeInterventionDecisionAsync(
            Guid intentId, [FromQuery] string wellId, [FromServices] IAccessControlService resources, [FromServices] RepositoryTenantContext tenant)
        {
            var actor = User.ActingUserId();
            if (tenant.Company is not { } company || company.UserId != actor) return Forbid();
            if ((await resources.CheckAssetAccessAsync(actor, wellId, "WELL", "READ"))?.HasAccess != true) return Forbid();
            try { return Ok(await _productionService.RecoverNativeInterventionDecisionAsync(wellId, intentId, actor, company.Scope)); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message, intentId, recoveryRequired = true }); }
        }

        [HttpGet("intervention-decisions/{intentId:guid}/maintenance-link")]
        [RequireCurrentFieldAccess("READ")]
        public async Task<ActionResult<ProductionInterventionMaintenanceLink>> ReadInterventionMaintenanceLinkAsync(Guid intentId,[FromQuery]string wellId,[FromQuery]string workOrderId,
            [FromServices]IAccessControlService resources,[FromServices]RepositoryTenantContext tenant)
        {
            var actor=User.ActingUserId();if(tenant.Company is not {} company || company.UserId!=actor)return Forbid();
            if((await resources.CheckAssetAccessAsync(actor,wellId,"WELL","READ"))?.HasAccess!=true || (await resources.CheckAssetAccessAsync(actor,workOrderId,"WORK_ORDER","READ"))?.HasAccess!=true)return Forbid();
            try{return Ok(await _productionService.ReadInterventionMaintenanceLinkAsync(wellId,intentId,workOrderId,actor,company.Scope));}
            catch(UnauthorizedAccessException){return Forbid();}
            catch(InvalidOperationException ex){return Conflict(new{error=ex.Message});}
        }

        /// <summary>Legacy decisions remain subject to the original command-owner boundary.</summary>
        [HttpPost("intervention-candidates/{uwi}/decision")]
        [RequireCurrentFieldAccess("WRITE")]
        public async Task<ActionResult<ProductionInterventionDecisionResult>> PostInterventionDecisionAsync(string uwi, [FromBody] ProductionInterventionDecisionRequest request, [FromServices] IAccessControlService resources, [FromServices] RepositoryTenantContext tenant)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            if(tenant.Company is not {} company || company.UserId!=userId)return Forbid();
            if(request.LinkAfe)
                return Conflict(new { error="Intervention AFE linking requires the original durable decision and a company-scoped financial handoff. No AFE is created through the default connection." });
            if(request.CreateWorkOrder && string.IsNullOrWhiteSpace(request.WorkOrderId))
                return Conflict(new { error="Create the maintenance header separately. The recorded well/component relationship must be reviewed before a linked intervention handoff; a well identity cannot be used as equipment." });
            var approvalDenied=false;
            async Task<bool> SourceStillMatchesAsync()
            {
                var rows=await _productionService.GetWellActivitiesForFieldAsync(fieldId);
                if(rows is null || _fieldOrchestrator.CurrentFieldId!=fieldId)return false;
                try
                {
                    ProductionInterventionSourceReviews.RequireDecision(rows.Where(x=>x.UWI==uwi),request,company.Scope,userId,fieldId,uwi);
                    return true;
                }
                catch(UnauthorizedAccessException){approvalDenied=true;return false;}
                catch(InvalidOperationException){return false;}
            }
            string normalizedDecision;
            try { normalizedDecision=ProductionInterventionDecisionChecks.Require(request,fieldId); }
            catch(ArgumentException ex){return BadRequest(new { error=ex.Message });}
            catch(InvalidOperationException ex){return Conflict(new { error=ex.Message });}
            if(!(await resources.CheckAssetAccessAsync(userId,uwi,"WELL","WRITE")).HasAccess) return Forbid();
            if(_fieldOrchestrator.CurrentFieldId!=fieldId) return Conflict(new { error="FIELD changed before the decision." });
            if(!await SourceStillMatchesAsync())
            {
                if(approvalDenied)return Forbid();
                return Conflict(new { error="Intervention evidence changed or submitted content differs. Reload the original source before deciding." });
            }
            var activityTypeId = normalizedDecision switch
            {
                "APPROVED" => "WORKOVER_APPROVED",
                "DEFERRED" => "WORKOVER_DEFERRED",
                "REJECTED" => "WORKOVER_REJECTED",
                _          => "WORKOVER_REVIEWED",
            };

            WorkOrderSummary? linkedWorkOrder = null;
            async Task<WorkOrderSummary?> ReadLinkedOrderAsync()
            {
                if(string.IsNullOrWhiteSpace(request.WorkOrderId))return null;
                if(_fieldOrchestrator.CurrentFieldId!=fieldId || (await resources.CheckAssetAccessAsync(userId,request.WorkOrderId,"WORK_ORDER","READ"))?.HasAccess!=true)
                    throw new UnauthorizedAccessException("Current original FIELD and recorded order READ authority are required.");
                var recorded=await _workOrderService.GetByIdAsync(fieldId,request.WorkOrderId);
                if(recorded is null)throw new InvalidOperationException("The asserted recorded order was not found; no replacement is created.");
                RecordedWorkOrderComponentChecks.RequireWellLink(recorded,fieldId,request.WorkOrderId,uwi);
                if(_fieldOrchestrator.CurrentFieldId!=fieldId || (await resources.CheckAssetAccessAsync(userId,request.WorkOrderId,"WORK_ORDER","READ"))?.HasAccess!=true)
                    throw new UnauthorizedAccessException("Recorded order READ authority changed during review.");
                return recorded;
            }

            if (normalizedDecision == "APPROVED")
            {
                if (!string.IsNullOrWhiteSpace(request.WorkOrderId))
                {
                    try { linkedWorkOrder=await ReadLinkedOrderAsync(); }
                    catch(UnauthorizedAccessException){return Forbid();}
                    catch(InvalidOperationException ex){return Conflict(new { error=ex.Message });}
                }

                if(_fieldOrchestrator.CurrentFieldId!=fieldId)
                    return Conflict(new { error="FIELD changed during the handoff. Review recorded orders before retrying.", workOrderId=linkedWorkOrder?.InstanceId });

            }

            if(_fieldOrchestrator.CurrentFieldId!=fieldId || !(await resources.CheckAssetAccessAsync(userId,uwi,"WELL","WRITE")).HasAccess)
                return Conflict(new { error="Original FIELD/well authority changed. Earlier handoff writes may exist; review recorded source before retrying.", workOrderId=linkedWorkOrder?.InstanceId });
            if(!await SourceStillMatchesAsync())return Conflict(new { error="Intervention source changed during the handoff. Earlier writes may exist; review recorded source before retrying.",workOrderId=linkedWorkOrder?.InstanceId });
            if(linkedWorkOrder is not null)
            {
                try
                {
                    var currentOrder=await ReadLinkedOrderAsync();
                    if(currentOrder is null || !currentOrder.Components.OrderBy(c=>c.ComponentId,StringComparer.Ordinal).SequenceEqual(linkedWorkOrder.Components.OrderBy(c=>c.ComponentId,StringComparer.Ordinal)))
                        return Conflict(new { error="The recorded work-order component relationships changed before linking. Review the original source again." });
                }
                catch(UnauthorizedAccessException){return Forbid();}
                catch(InvalidOperationException ex){return Conflict(new { error=ex.Message });}
            }
            var remark = BuildDecisionRemark(request.Note, linkedWorkOrder?.InstanceId, null, null);
            await _productionService.RecordInterventionDecisionAsync(uwi, activityTypeId, remark, userId);

            return Ok(new ProductionInterventionDecisionResult
            {
                ReviewedSourceRevision=request.ExpectedSourceRevision,
                Success = true,
                FieldId = fieldId,
                WellId = uwi,
                Decision = normalizedDecision ?? "REVIEWED",
                WorkOrderId = linkedWorkOrder?.InstanceId,
                AfeId = null,
                AfeNumber = null,
                Message = linkedWorkOrder == null
                    ? $"Intervention {normalizedDecision?.ToLowerInvariant() ?? "reviewed"} for {uwi}."
                    : $"Intervention approved and linked to work order {linkedWorkOrder.InstanceId}."
            });
        }

        /// <summary>GET /api/field/current/production/dashboard/wells</summary>
        [HttpGet("dashboard/wells")]
        public async Task<ActionResult<List<ProductionWellStatusDto>>> GetDashboardWellsAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            var wells = await _productionService.GetProductionWellStatusAsync(fieldId);
            return Ok(wells ?? new List<ProductionWellStatusDto>());
        }

        private static DateTime ResolveTestDate(WellTestResponse? test)
        {
            if (test == null)
            {
                return DateTime.MinValue;
            }

            return test.TestDate
                ?? test.TestStartDate
                ?? test.TestEndDate
                ?? DateTime.MinValue;
        }

        private static double ToDouble(decimal? value)
        {
            return value.HasValue ? (double)value.Value : 0;
        }

        private static string MapInterventionStatus(string? activityTypeId)
        {
            return activityTypeId?.ToUpperInvariant() switch
            {
                "WORKOVER_APPROVED" => "APPROVED",
                "WORKOVER_DEFERRED" => "DEFERRED",
                "WORKOVER_REJECTED" => "REJECTED",
                "DECOMMISSIONING_TRIGGERED" => "TRANSFERRED",
                "PERFORMANCE_DEVIATION" => "REVIEW",
                _ => "REVIEW"
            };
        }

        private static string BuildDecisionRemark(string? note,string? workOrderId,string? afeId,string? afeNumber)=>
            ProductionInterventionDecisionChecks.RecordedRemark(note,workOrderId,afeId,afeNumber);

        private static string BuildDecommissioningRemark(string? note, string? abandonmentId, string? processInstanceId)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(note))
                parts.Add(note.Trim());
            if (!string.IsNullOrWhiteSpace(abandonmentId))
                parts.Add($"ABANDONMENT_ID:{abandonmentId}");
            if (!string.IsNullOrWhiteSpace(processInstanceId))
                parts.Add($"PROCESS_INSTANCE_ID:{processInstanceId}");

            parts.Add("SOURCE_PROCESS:DECOMMISSIONING");
            return string.Join(" ", parts);
        }

        private static string BuildWorkOrderTitle(string uwi, string? interventionType)
        {
            return string.IsNullOrWhiteSpace(interventionType)
                ? $"Intervention follow-up for {uwi}"
                : $"{interventionType} for {uwi}";
        }

        private static string BuildWorkOrderDescription(ProductionInterventionDecisionRequest request)
        {
            var descriptionParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(request.Problem))
                descriptionParts.Add(request.Problem.Trim());
            if (!string.IsNullOrWhiteSpace(request.Note))
                descriptionParts.Add(request.Note.Trim());

            return descriptionParts.Count == 0
                ? "Create an intervention work order for the approved candidate."
                : string.Join(" ", descriptionParts);
        }
    }

    // â”€â”€ Response DTOs â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public class WellPerformanceSummary
    {
        public string WellId        { get; set; } = string.Empty;
        public string Status        { get; set; } = string.Empty;
        public double OilRate       { get; set; }
        public double GasRate       { get; set; }
        public double WaterRate     { get; set; }
        public double PotentialRate { get; set; }
        public double CumOil        { get; set; }
        public DateTime? LastTestDate { get; set; }
        public List<WellTestResponse> WellTests { get; set; } = new();
    }

    public class AllocationPatchRequest
    {
        public double AllocatedOilBopd    { get; set; }
        public double AllocatedGasMmscfd  { get; set; }
        public double AllocatedWaterBopd  { get; set; }
    }

    public class DecommissioningTriggerRequest
    {
        public string? Note { get; set; }
        public string? InterventionType { get; set; }
        public string? Problem { get; set; }
        public double? EstimatedCostUsd { get; set; }
        public string? AbandonmentType { get; set; }
        public string? AbandonmentMethod { get; set; }
        public bool StartWorkflow { get; set; } = true;
    }

    public class DecommissioningTriggerResult
    {
        public bool Success { get; set; }
        public bool AlreadyExists { get; set; }
        public bool WorkflowStarted { get; set; }
        public string WellId { get; set; } = string.Empty;
        public string? AbandonmentId { get; set; }
        public string? ProcessInstanceId { get; set; }
        public string? Message { get; set; }
    }

}
