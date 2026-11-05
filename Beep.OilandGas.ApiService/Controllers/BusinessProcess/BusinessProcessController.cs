using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Data.WorkOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using TheTechIdeaWeb.Diagnostics;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.Repository;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess
{
    /// <summary>
    /// Business process lifecycle endpoints — definitions, instances, transitions, and history.
    /// All operations are scoped to the current active field via IFieldOrchestrator.
    /// </summary>
    [ApiController]
    [Route("api/field/current/process")]
    [Authorize]
    [RequireCurrentFieldAccess]
    public class BusinessProcessController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly IProcessService _processService;
        private readonly ILogger<BusinessProcessController> _logger;
        private readonly CompanyProcessReadService _reads;
        private readonly IJournalEntryService _journals;
        private readonly IWorkOrderService _workOrders;
        private readonly IInspectionService _inspections;
        private readonly IFailureReporter _failures;

        public BusinessProcessController(
            IFieldOrchestrator fieldOrchestrator,
            IProcessService processService,
            ILogger<BusinessProcessController> logger, IAccessControlService resources, IFailureReporter failures, IJournalEntryService journals, IWorkOrderService workOrders, IInspectionService inspections)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _logger = logger;
            _reads = new CompanyProcessReadService(resources);
            _workOrders = workOrders ?? throw new ArgumentNullException(nameof(workOrders));
            _inspections = inspections ?? throw new ArgumentNullException(nameof(inspections));
            _journals = journals ?? throw new ArgumentNullException(nameof(journals));
            _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        }

        // ─── Process Definitions ────────────────────────────────────────────────

        /// <summary>List all process definitions available for the current field.</summary>
        [HttpGet("definitions")]
        [ProducesResponseType(typeof(List<ProcessDefinition>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<List<ProcessDefinition>>> GetDefinitionsAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            var service=await GetOperationServiceAsync();
            try
            {
                var definitions=await ProcessDefinitionCatalog.ReadAsync(service.GetProcessDefinitionsByTypeAsync);
                if(_fieldOrchestrator.CurrentFieldId!=fieldId)return Conflict(new {error="FIELD changed during catalog review. Reload the original company context."});
                return Ok(definitions);
            }
            catch(InvalidOperationException changed)
            {
                _logger.LogWarning(changed,"The configured process definition catalog could not be confirmed.");
                return Conflict(new {error="The configured process catalog is unavailable or ambiguous. Reload before selecting a definition."});
            }
        }

        /// <summary>Get a single process definition by ID.</summary>
        [HttpGet("definitions/{processId}")]
        [ProducesResponseType(typeof(ProcessDefinition), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProcessDefinition>> GetDefinitionAsync(string processId)
        {
            if (string.IsNullOrWhiteSpace(processId))
                    return BadRequest(new { error = "Process ID is required." });

            var def = await _processService.GetProcessDefinitionAsync(processId);
            if (def == null)
                return NotFound(new { error = $"Process definition '{processId}' not found." });
            return Ok(def);
        }

        /// <summary>List process definitions filtered by category/type.</summary>
        [HttpGet("definitions/category/{categoryId}")]
        [ProducesResponseType(typeof(List<ProcessDefinition>), 200)]
        public async Task<ActionResult<List<ProcessDefinition>>> GetDefinitionsByCategoryAsync(string categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
                    return BadRequest(new { error = "Category ID is required." });

            var defs = await _processService.GetProcessDefinitionsByTypeAsync(categoryId.ToUpperInvariant());
            return Ok(defs ?? new List<ProcessDefinition>());
        }

        /// <summary>List process definitions filtered by jurisdiction tag (USA, CANADA, INTERNATIONAL).</summary>
        [HttpGet("definitions/jurisdiction/{tag}")]
        [ProducesResponseType(typeof(List<ProcessDefinition>), 200)]
        public async Task<ActionResult<List<ProcessDefinition>>> GetDefinitionsByJurisdictionAsync(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                    return BadRequest(new { error = "Jurisdiction tag is required." });

            // Fetch all then filter by configuration/metadata tag
            var allDefinitions = new List<ProcessDefinition>();
            var categories = new[]
            {
                "WORK_ORDER", "GATE_REVIEW", "HSE", "COMPLIANCE",
                "WELL_LIFECYCLE", "FACILITY_LIFECYCLE", "RESERVOIR", "PIPELINE"
            };
            foreach (var cat in categories)
            {
                var defs = await _processService.GetProcessDefinitionsByTypeAsync(cat);
                if (defs != null) allDefinitions.AddRange(defs);
            }
            var filtered = allDefinitions.FindAll(d =>
                d.Configuration != null &&
                d.Configuration.TryGetValue("JurisdictionTag", out var jTag) &&
                string.Equals(jTag?.ToString(), tag, StringComparison.OrdinalIgnoreCase));
            return Ok(filtered);
        }

        // ─── Process Instances ───────────────────────────────────────────────────

        /// <summary>Start a new process instance for an entity in the current field.</summary>
        [HttpPost("instances")]
        [ProducesResponseType(typeof(ProcessInstance), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ProcessInstance>> StartInstanceAsync(
            [FromBody] ProcessInstanceRequest request, [FromServices] IAccessControlService resources, [FromServices] RepositoryTenantContext tenant)
        {
            if (request == null)
                 return BadRequest(new { error = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.ProcessId))
                 return BadRequest(new { error = "ProcessId is required." });
            if (string.IsNullOrWhiteSpace(request.EntityId))
                 return BadRequest(new { error = "EntityId is required." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                 return BadRequest(new { error = "No active field selected." });

            var userId = User.ActingUserId();

            if(request.ExpectedFieldId!=fieldId || string.IsNullOrWhiteSpace(request.EntityType) || !ProcessDefinitionRevision.IsValid(request.ExpectedDefinitionRevision))
                return BadRequest(new {error="The original FIELD, definition revision and source entity type are required."});
            var service=await GetOperationServiceAsync();
            var definition=await service.GetProcessDefinitionAsync(request.ProcessId);
            if(definition is null)return NotFound();
            try { ProcessDefinitionStartReview.Require(request,fieldId,definition); }
            catch (InvalidOperationException changed) { _failures.ReportHandled(changed,"reviewing a process start","configured definition is not confirmed",FailureSeverity.Degraded); return Conflict(new {error="The configured definition changed. Reload before starting."}); }
            if((await resources.CheckAssetAccessAsync(userId,fieldId,"FIELD","WRITE"))?.HasAccess!=true ||
                (await resources.CheckAssetAccessAsync(userId,request.EntityId,request.EntityType,"WRITE"))?.HasAccess!=true)return Forbid();
            if (!ProcessDefinitionRevision.IsValid(request.ExpectedSourceRevision))
                return BadRequest(new {error="The recorded source review is required before starting."});
            try
            {
                var reviewed=await ReadStartSourceAsync(service,request,tenant,resources);
                if(reviewed.Revision!=request.ExpectedSourceRevision)return Conflict(new {error="The recorded source changed. Review it again before starting."});
            }
            catch(UnauthorizedAccessException denied){_failures.ReportHandled(denied,"reviewing a process start source","current company source access is refused",FailureSeverity.Degraded);return Forbid();}
            catch(InvalidOperationException changed){_failures.ReportHandled(changed,"reviewing a process start source","recorded source is not confirmed",FailureSeverity.Degraded);return Conflict(new {error="The recorded source could not be confirmed for starting. Review it again."});}
            var closing=await service.GetProcessDefinitionAsync(request.ProcessId);
            try { ProcessDefinitionStartReview.Require(request,_fieldOrchestrator.CurrentFieldId ?? string.Empty,closing); }
            catch (InvalidOperationException changed) { _failures.ReportHandled(changed,"confirming a process start","original FIELD or definition is not confirmed",FailureSeverity.Degraded); return Conflict(new {error="Original FIELD or definition changed during start review."}); }
            if((await resources.CheckAssetAccessAsync(userId,fieldId,"FIELD","WRITE"))?.HasAccess!=true ||
                (await resources.CheckAssetAccessAsync(userId,request.EntityId,request.EntityType,"WRITE"))?.HasAccess!=true)return Forbid();
            if (_fieldOrchestrator.CurrentFieldId != fieldId)
                return Conflict(new {error="Original FIELD changed during source access review."});
            var instance=await service.StartProcessAsync(request.ProcessId,request.EntityId,request.EntityType,fieldId,userId);
            return CreatedAtAction(nameof(GetInstanceAsync), new { instanceId = instance.InstanceId }, instance);
        }

        [HttpPost("instances/start-review")]
        public async Task<ActionResult<ProcessStartSourceReview>> ReviewStartSourceAsync(
            [FromBody] ProcessInstanceRequest request,[FromServices] RepositoryTenantContext tenant,[FromServices] IAccessControlService resources)
        {
            if(request is null || string.IsNullOrWhiteSpace(request.ProcessId) || string.IsNullOrWhiteSpace(request.EntityId) ||
                string.IsNullOrWhiteSpace(request.EntityType) || string.IsNullOrWhiteSpace(request.ExpectedFieldId) ||
                request.ExpectedFieldId!=_fieldOrchestrator.CurrentFieldId || !ProcessDefinitionRevision.IsValid(request.ExpectedDefinitionRevision))
                return BadRequest(new {error="The original FIELD, definition revision and explicit source are required."});
            try { return Ok(await ReadStartSourceAsync(await GetOperationServiceAsync(),request,tenant,resources)); }
            catch(UnauthorizedAccessException denied){_failures.ReportHandled(denied,"reviewing a process start source","current company source access is refused",FailureSeverity.Degraded);return Forbid();}
            catch(InvalidOperationException changed){_failures.ReportHandled(changed,"reviewing a process start source","recorded source is not confirmed",FailureSeverity.Degraded);return Conflict(new {error="The recorded start source is unavailable or changed. Review the original definition and source."});}
        }
        private async Task<ProcessStartSourceReview> ReadStartSourceAsync(IProcessService service,ProcessInstanceRequest request,
            RepositoryTenantContext tenant,IAccessControlService resources)
        {
            var actor=User.ActingUserId();
            if(tenant.Company is not {} company || company.UserId!=actor)throw new UnauthorizedAccessException("A current company account is required.");
            async Task<ProcessStartSourceReview> CaptureAsync()
            {
                if(_fieldOrchestrator.CurrentFieldId!=request.ExpectedFieldId || tenant.Company!=company)
                    throw new InvalidOperationException("Original company or FIELD changed before source review.");
                ProcessDefinitionStartReview.Require(request,_fieldOrchestrator.CurrentFieldId ?? string.Empty,
                    await service.GetProcessDefinitionAsync(request.ProcessId));
                if((await resources.CheckAssetAccessAsync(actor,request.ExpectedFieldId,"FIELD","READ"))?.HasAccess!=true ||
                    (await resources.CheckAssetAccessAsync(actor,request.EntityId,request.EntityType!,"READ"))?.HasAccess!=true)
                    throw new UnauthorizedAccessException("Original FIELD and source READ access is required.");
                var review=request.EntityType switch
                {
                    "JOURNAL_ENTRY"=>ProcessStartSources.Journal(company.Scope,actor,request,
                        await _journals.GetEntryByIdAsync(request.EntityId),await _journals.GetEntryLineItemsAsync(request.EntityId)),
                    "WORK_ORDER"=>ProcessStartSources.WorkOrder(company.Scope,actor,request,
                        await _workOrders.GetByIdAsync(request.ExpectedFieldId,request.EntityId)),
                    _=>throw new InvalidOperationException("This definition source has no verified initiation reader.")
                };
                if(_fieldOrchestrator.CurrentFieldId!=request.ExpectedFieldId || tenant.Company!=company)
                    throw new InvalidOperationException("Original company or FIELD changed during source review.");
                return review;
            }
            var initial=await CaptureAsync();var closing=await CaptureAsync();
            if(initial.Revision!=closing.Revision)throw new InvalidOperationException("The recorded source changed during initiation review.");
            return closing;
        }

        /// <summary>List all active process instances for the current field.</summary>
        [HttpGet("instances")]
        [ProducesResponseType(typeof(List<ProcessInstanceSummary>), 200)]
        public async Task<ActionResult<List<ProcessInstanceSummary>>> GetInstancesAsync([FromServices] RepositoryTenantContext? tenant=null)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            var summaries = new List<ProcessInstanceSummary>();
            var processService = await GetOperationServiceAsync();
                var instances = await processService.GetProcessInstancesForFieldAsync(fieldId);
                if (instances != null)
                {
                    foreach (var inst in instances.Where(IsCurrentField))
                    {
                        if(IsRecordedCompletionReview(inst) || IsRecordedInvestmentProcess(inst))continue;
                        if(IsRecordedMaintenanceProcess(inst) && CompanyMaintenanceProcessStart.FromPayload(JsonSerializer.Serialize(inst.ProcessData),inst.InstanceId,inst.FieldId,inst.EntityId).Context.Company!=CurrentMaintenanceCompany(tenant))continue;
                        summaries.Add(new ProcessInstanceSummary
                        {
                            InstanceId = inst.InstanceId,
                            ProcessId = inst.ProcessId,
                            EntityId = inst.EntityId,
                            EntityType = inst.EntityType,
                            CurrentStepId = inst.CurrentStepId,
                            Status = inst.Status.ToString(),
                            StartedAt = inst.StartDate
                        });
                    }
                }
            return Ok(summaries);
        }

        /// <summary>Get a specific process instance with its current step state.</summary>
        [HttpGet("instances/{instanceId}")]
        [ProducesResponseType(typeof(ProcessInstance), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProcessInstance>> GetInstanceAsync(string instanceId,[FromServices] RepositoryTenantContext? tenant=null)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });

            try
            {
                var service = await GetOperationServiceAsync();
                var instance = await _reads.ReadAsync(service, User.ActingUserId(), _fieldOrchestrator.CurrentFieldId ?? "", instanceId,CurrentMaintenanceCompany(tenant));
                return instance is null ? NotFound() : Ok(instance);
            }
            catch (UnauthorizedAccessException denied) { _failures.ReportHandled(denied, "reading a process source", "source READ access is refused", FailureSeverity.Degraded); return Forbid(); }
            catch (InvalidOperationException changed) { _failures.ReportHandled(changed, "reading a process source", "source data is not confirmed; reload is required", FailureSeverity.Degraded); return Conflict(new { error = "The process source changed. Reload before continuing." }); }
        }

        [HttpGet("instances/{instanceId}/transitions")]
        public async Task<ActionResult<ProcessTransitionReview>> ReviewTransitions(string instanceId, [FromServices] RepositoryTenantContext tenant)
        {
            try
            {
                var service=await GetOperationServiceAsync();
                var review=await ReadTransitionReviewAsync(service,instanceId,tenant);
                return review is null ? NotFound() : Ok(review);
            }
            catch(UnauthorizedAccessException denied){_failures.ReportHandled(denied,"reviewing process actions","source action access is refused",FailureSeverity.Degraded);return Forbid();}
            catch(InvalidOperationException changed){_failures.ReportHandled(changed,"reviewing process actions","source action review is not confirmed",FailureSeverity.Degraded);return Conflict(new { error="Source or workflow changed. Reload source details." });}
        }
        private async Task<ProcessTransitionReview?> ReadTransitionReviewAsync(IProcessService service,string id,RepositoryTenantContext tenant)
        {
            var actor=User.ActingUserId();
            if(tenant.Company is not { } company || company.UserId!=actor) throw new UnauthorizedAccessException("A current company account is required.");
            async Task<ProcessTransitionReview?> CaptureAsync()
            {
                var source=await _reads.ReadAsync(service,actor,_fieldOrchestrator.CurrentFieldId ?? "",id,CurrentMaintenanceCompany(tenant));
                if(source is null)return null;
                var history=await _reads.HistoryAsync(service,actor,source.FieldId,id,CurrentMaintenanceCompany(tenant)) ?? throw new InvalidOperationException("Source history is unavailable.");
                var definition=await service.GetProcessDefinitionAsync(source.ProcessId) ?? throw new InvalidOperationException("Source definition is unavailable.");
                JOURNAL_ENTRY? journal=null;
                List<JOURNAL_ENTRY_LINE>? journalLines=null;
                if(source.EntityType=="JOURNAL_ENTRY")
                {
                    // The existing journal service resolves the request's PRODUCTION company binding.
                    journal=await _journals.GetEntryByIdAsync(source.EntityId) ?? throw new InvalidOperationException("The recorded journal source is unavailable.");
                    journalLines=await _journals.GetEntryLineItemsAsync(source.EntityId);
                }
                WorkOrderDetailModel? order=null;
                List<InspectionCondition>? inspections=null;
                if(source.EntityType=="WORK_ORDER")
                {
                    order=await _workOrders.GetByIdAsync(source.FieldId,source.EntityId) ?? throw new InvalidOperationException("The recorded maintenance header is unavailable.");
                    inspections=await _inspections.GetChecklistAsync(source.EntityId,source.FieldId);
                }
                return ProcessTransitionReviews.Create(company.Scope,actor,User,source,definition,history,journal,journalLines,order,inspections);
            }
            var initial=await CaptureAsync();if(initial is null)return null;
            var closing=await CaptureAsync();
            if(closing?.Revision!=initial.Revision)throw new InvalidOperationException("Source or workflow changed during action review.");
            return initial;
        }

        /// <summary>Execute a state transition on a process instance.</summary>
        [HttpPost("instances/{instanceId}/transitions")]
        [ProducesResponseType(typeof(ProcessTransitionResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<ActionResult<ProcessTransitionResult>> ExecuteTransitionAsync(
            string instanceId,
            [FromBody] ProcessTransitionRequest request, [FromServices] RepositoryTenantContext tenant)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });
                if (request == null)
                    return BadRequest(new { error = "Request body is required." });
                if (string.IsNullOrWhiteSpace(request.Trigger))
                    return BadRequest(new { error = "Transition trigger is required." });

            var userId = User.ActingUserId();

            var processService = await GetOperationServiceAsync();
            var instance = await processService.GetProcessInstanceAsync(instanceId);
            if (instance is null) return NotFound();
            if (!await CanActOnStepAsync(processService, instance, instance.CurrentStepId, userId)) return Forbid();
            if (string.IsNullOrWhiteSpace(request.FromStateId) || string.IsNullOrWhiteSpace(request.ToStateId)) return BadRequest(new { error = "Reviewed source and destination states are required." });
            if (string.IsNullOrWhiteSpace(request.ExpectedRevision)) return BadRequest(new { error = "The original source action review revision is required." });
            if (instance.CurrentState != request.FromStateId) return Conflict(new { error = "The process state changed. Reload before submitting a transition." });
            var definition = await processService.GetProcessDefinitionAsync(instance.ProcessId);
            if (definition is null || !ProcessStepRoleChecks.CanTransition(User, userId, instance, definition, request.FromStateId, request.ToStateId, request.Trigger)) return Forbid();
            var actualFromState = instance.CurrentState;

            ProcessTransitionReview? review;
            try { review=await ReadTransitionReviewAsync(processService,instanceId,tenant); }
            catch(UnauthorizedAccessException denied){_failures.ReportHandled(denied,"submitting a process action","source action access is refused",FailureSeverity.Degraded);return Forbid();}
            catch(InvalidOperationException changed){_failures.ReportHandled(changed,"submitting a process action","source review is not confirmed; reload required",FailureSeverity.Degraded);return Conflict(new { error="Source or workflow changed. Reload source details." });}
            if(review is null)return NotFound();
            if(string.IsNullOrWhiteSpace(request.ExpectedRevision) || request.ExpectedRevision!=review.Revision)
                return Conflict(new { error = "The reviewed source or workflow changed. Reload before submitting a transition." });
            if(!review.Choices.Any(choice=>choice.FromStateId==request.FromStateId && choice.ToStateId==request.ToStateId && choice.Trigger==request.Trigger))return Forbid();
            var canTransition = await processService.CanTransitionAsync(instanceId, request.ToStateId);
            if (!canTransition)
                return UnprocessableEntity(new { error = $"Transition to '{request.ToStateId}' is not allowed from current state." });

            var success = await processService.TransitionStateAsync(instanceId, request.ToStateId, userId);
            var result = new ProcessTransitionResult
            {
                Success = success,
                InstanceId = instanceId,
                TransitionName = request.Trigger,
                FromState = actualFromState,
                NewStepId = request.ToStateId,
                Message = success ? "Transition completed successfully." : "Transition failed.",
                TransitionedAt = DateTime.UtcNow
            };
            return Ok(result);
        }

        /// <summary>Get the full audit history for a process instance.</summary>
        [HttpGet("instances/{instanceId}/history")]
        [ProducesResponseType(typeof(List<ProcessHistoryEntry>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<ProcessHistoryEntry>>> GetHistoryAsync(string instanceId,[FromServices] RepositoryTenantContext? tenant=null)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });

            try
            {
                var service = await GetOperationServiceAsync();
                var history = await _reads.HistoryAsync(service, User.ActingUserId(), _fieldOrchestrator.CurrentFieldId ?? "", instanceId,CurrentMaintenanceCompany(tenant));
                return history is null ? NotFound() : Ok(history);
            }
            catch (UnauthorizedAccessException denied) { _failures.ReportHandled(denied, "reading a process source", "source READ access is refused", FailureSeverity.Degraded); return Forbid(); }
            catch (InvalidOperationException changed) { _failures.ReportHandled(changed, "reading process source history", "source history is not confirmed; reload is required", FailureSeverity.Degraded); return Conflict(new { error = "The process source or history changed. Reload before continuing." }); }
        }

        /// <summary>Update step data or attach documents to a step.</summary>
        [HttpPatch("instances/{instanceId}/steps/{stepId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateStepAsync(
            string instanceId,
            string stepId,
            [FromBody] Beep.OilandGas.Models.Data.Process.PROCESS_STEP_DATA stepData)
        {
                if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });
                if (string.IsNullOrWhiteSpace(stepId))
                    return BadRequest(new { error = "Step ID is required." });

            var userId = User.ActingUserId();

            var processService = await GetOperationServiceAsync();
            var instance = await processService.GetProcessInstanceAsync(instanceId);
            if (instance is null) return NotFound();
            if (!await CanActOnStepAsync(processService, instance, stepId, userId)) return Forbid();
            var success = await processService.ExecuteStepAsync(instanceId, stepId, stepData, userId);
            if (!success)
                return BadRequest(new { error = "Step update failed. Verify the instance and step are in a valid state." });
            return NoContent();
        }

        /// <summary>Close or cancel a process instance.</summary>
        [HttpPost("instances/{instanceId}/close")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CloseInstanceAsync(
            string instanceId,
            [FromBody] ProcessCloseRequest request)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });

            var userId = User.ActingUserId();
            var reason = request?.Reason ?? "Closed by user.";

            var processService = await GetOperationServiceAsync();
            var instance = await processService.GetProcessInstanceAsync(instanceId);
            if (instance is null) return NotFound();
            if (!IsCurrentField(instance) || (instance.StartedBy != userId && !User.IsInRole("Administrator"))) return Forbid();
            if(IsRecordedCompletionReview(instance) || IsRecordedInvestmentProcess(instance))return Conflict(new{error="Investment review assignments require their recorded AFE handoff owner."});
            if(IsRecordedMaintenanceProcess(instance))return Conflict(new{error="Recorded intervention maintenance requires its original reviewed cancellation owner."});
            var success = await processService.CancelProcessAsync(instanceId, reason, userId);
            if (!success)
                return BadRequest(new { error = "Unable to close instance. It may already be closed or completed." });
            return NoContent();
        }

        private Task<IProcessService> GetOperationServiceAsync() => _processService is BoundProcessService bound
            ? bound.CreateBoundServiceAsync() : Task.FromResult(_processService);

        private bool IsCurrentField(ProcessInstance instance) =>
            !string.IsNullOrWhiteSpace(_fieldOrchestrator.CurrentFieldId) &&
            string.Equals(instance.FieldId, _fieldOrchestrator.CurrentFieldId, StringComparison.Ordinal);

        private CompanyScope? CurrentMaintenanceCompany(RepositoryTenantContext? tenant)=>tenant?.Company is {} company && company.UserId==User.ActingUserId()?company.Scope:null;
        private static bool IsRecordedMaintenanceProcess(ProcessInstance instance)=>instance.InstanceId.StartsWith(CompanyMaintenanceProcessStart.InstancePrefix,StringComparison.Ordinal) || CompanyMaintenanceProcessStart.HasPayload(JsonSerializer.Serialize(instance.ProcessData));
        private static bool IsRecordedCompletionReview(ProcessInstance instance)=>instance.EntityType=="DRILLING_DAILY_REPORT" || instance.InstanceId.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal) || instance.ProcessData?.Data?.ContainsKey(DrillingCompletionReviewStart.PayloadKey)==true;
        private static bool IsRecordedInvestmentProcess(ProcessInstance instance)=>instance.EntityType=="AFE" || instance.InstanceId.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal) || instance.ProcessData?.Data?.ContainsKey(DevelopmentInvestmentHandoff.PayloadKey)==true;

        private async Task<bool> CanActOnStepAsync(IProcessService service, ProcessInstance instance, string stepId, string userId)
        {
            if (!IsCurrentField(instance)) return false;
            if(IsRecordedCompletionReview(instance) || IsRecordedInvestmentProcess(instance))return false;
            if(IsRecordedMaintenanceProcess(instance))return false;
            var step = instance.StepInstances?.SingleOrDefault(x => x.StepId == stepId);
            if (step is null || instance.CurrentStepId != stepId) return false;
            var definition = await service.GetProcessDefinitionAsync(instance.ProcessId);
            var definitionStep = definition?.Steps?.SingleOrDefault(x => x.StepId == stepId);
            if(definition is null || definitionStep is null || !ProcessStepRoleChecks.CanAct(User,userId,instance,step,definition,definitionStep))return false;
            if(instance.EntityType=="JOURNAL_ENTRY" && ProcessStepRoleChecks.RequiresIndependentActor(step,definition,definitionStep))
            {
                // Recheck source resource authority before looking up the preparer in the bound accounting owner.
                ProcessInstance? confirmed;
                try { confirmed=await _reads.ReadAsync(service,userId,instance.FieldId,instance.InstanceId); }
                catch(UnauthorizedAccessException denied){_failures.ReportHandled(denied,"reviewing a journal step","journal source READ access is refused",FailureSeverity.Degraded);return false;}
                catch(InvalidOperationException changed){_failures.ReportHandled(changed,"reviewing a journal step","journal workflow source is not confirmed",FailureSeverity.Degraded);return false;}
                if(confirmed is null || confirmed.EntityId!=instance.EntityId || confirmed.EntityType!=instance.EntityType)return false;
                var journal=await _journals.GetEntryByIdAsync(instance.EntityId);
                if(journal is null || journal.JOURNAL_ENTRY_ID!=instance.EntityId || string.IsNullOrWhiteSpace(journal.ROW_CREATED_BY) || journal.ROW_CREATED_BY==userId)return false;
            }
            return true;
        }

        /// <summary>List all available seed process templates.</summary>
        [HttpGet("templates")]
        [ProducesResponseType(typeof(List<ProcessDefinition>), 200)]
        public async Task<ActionResult<List<ProcessDefinition>>> GetTemplatesAsync()
        {
            var templates = await _processService.GetProcessDefinitionsByTypeAsync("TEMPLATE");
            return Ok(templates ?? new List<ProcessDefinition>());
        }
    }

    /// <summary>Minimal close/cancel request body.</summary>
    public class ProcessCloseRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
