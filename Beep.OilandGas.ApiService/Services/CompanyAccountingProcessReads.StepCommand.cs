using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Processes;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyAccountingProcessReads
{
    public async Task<CompanyAccountingStepReceipt> CompleteStepAsync(ClaimsPrincipal user,CompanyAccountingStepCommand submission)
    {
        using var invoiceValidation=CompanySalesInvoiceValidation.Enter();
        ArgumentNullException.ThrowIfNull(submission);submission.Require();
        var command=JsonSerializer.Deserialize<CompanyAccountingStepCommand>(JsonSerializer.Serialize(submission))!;var review=command.Review;var returning=command.Action=="RETURN_CORRECTION_WORK";var nativeRejection=CompanyAccountingSourceTypes.IsNativeDecision(review.EntityType) && command.Action==CompanyAccountingSourceTypes.RejectionRoute(review.EntityType);
        string? verifiedEvidenceAction=null;
        var original=await context.CaptureProcessAsync(user);
        var roles=original.Access.Roles?.Order(StringComparer.Ordinal).ToArray() ?? throw new UnauthorizedAccessException("Current company roles are required.");
        var permissions=original.Access.Permissions?.Order(StringComparer.Ordinal).ToArray() ?? throw new UnauthorizedAccessException("Current company permissions are required.");
        async Task Guard()
        {
            var current=await context.CaptureProcessAsync(user);
            if(user.Identity?.IsAuthenticated!=true || current.Access.Scope!=review.Company || current.Actor!=review.ActorUserId || current.Binding!=review.BindingRevision ||
                current.Access.Scope!=original.Access.Scope || current.Actor!=original.Actor || current.Binding!=original.Binding ||
                current.Access.Roles is null || !current.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(roles) ||
                current.Access.Permissions is null || !current.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(permissions) ||
                !current.Access.Roles.Any(user.IsInRole) ||
                !(current.Access.Roles.Contains(review.AssignedRole,StringComparer.Ordinal) && user.IsInRole(review.AssignedRole) ||
                  current.Access.Roles.Contains("Administrator",StringComparer.Ordinal) && user.IsInRole("Administrator")) ||
                !current.Access.Permissions.Contains(review.RequiredPermission,StringComparer.Ordinal) ||
                !current.Access.Permissions.Contains(CompanyAccountingSourceTypes.ReadPermission(review.EntityType),StringComparer.Ordinal))
                throw new UnauthorizedAccessException("The original company step command authority changed.");
        }
        await Guard();
        if(original.Process is not ICompanyAccountingProcessCommands commands)throw new InvalidOperationException("A bound native company command owner is required.");
        static bool Same(CompanyAccountingStepCommand? recorded,CompanyAccountingStepCommand requested)=>recorded is not null && JsonSerializer.Serialize(recorded)==JsonSerializer.Serialize(requested);
        async Task<ProcessInstance> SourceAndHistoricalAuthority(bool requireRecorded)
        {
            await Guard();
            var captured=await ReadWithNativeSourcesAsync(user,review.InstanceId,review.Company,review.ActorUserId);var instance=captured.Process ?? throw new InvalidOperationException("The owned command process is unavailable.");
            var receivable=review.EntityType=="AR_INVOICE"?captured.Receivable ?? throw new InvalidOperationException("The verified receivable command source is unavailable."):null;
            if(receivable is not null && !receivable.HasCurrentOriginalSource)throw new InvalidOperationException("The original receivable source changed before task completion.");
            var journal=review.EntityType=="JOURNAL_ENTRY"?captured.Journal ?? throw new InvalidOperationException("The verified journal command source is unavailable."):null;
            var payment=review.EntityType=="AR_PAYMENT"?captured.Payment ?? throw new InvalidOperationException("The verified payment process source is unavailable."):null;
            var source=payment is not null?(Revision:payment.SourceRevision,Preparer:payment.Payment.CreatedBy):journal is not null?(Revision:journal.SourceRevision,Preparer:journal.Journal.Header.ROW_CREATED_BY):receivable is null?await ReadStartSourceAsync(user,review.Company,review.ActorUserId,review.EntityType,review.EntityId,review.SourceRevision):
                (Revision:CompanyAccountingSourceTypes.ReceivablePreparationRevision(receivable.Original),Preparer:receivable.Original.Intent.ROW_CREATED_BY);
            if((receivable is not null || journal is not null || payment is not null) && source.Revision!=review.SourceRevision || source.Preparer!=review.SourceMakerUserId || instance.ProcessId!=review.ProcessId || instance.EntityType!=review.EntityType || instance.EntityId!=review.EntityId || instance.StartedBy!=review.WorkflowStarterUserId)
                throw new InvalidOperationException("The original command source or maker changed.");
            var definition=await original.Process.GetProcessDefinitionAsync(review.ProcessId) ?? throw new InvalidOperationException("The command definition is unavailable.");
            if(!definition.IsActive || definition.ProcessType!="FINANCIAL" || definition.EntityType!=review.EntityType || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition))!=review.DefinitionRevision)
                throw new InvalidOperationException("The original command definition changed.");
            var steps=instance.StepInstances.Where(step=>step.StepInstanceId==review.StepInstanceId && step.StepId==review.StepId).ToArray();
            var templates=definition.Steps.Where(step=>step.StepId==review.StepId).ToArray();
            if(steps.Length!=1 || templates.Length!=1 || steps[0].RequiredRole!=review.AssignedRole || steps[0].StepName!=review.StepName || steps[0].InstanceId!=review.InstanceId)
                throw new InvalidOperationException("The recorded original command step changed.");
            var step=steps[0];var template=templates[0];verifiedEvidenceAction=template.StepConfiguration.TryGetValue(CompanyAccountingStepEvidence.ConfigurationKey,out var configured)?configured switch{string text=>text,JsonElement {ValueKind:JsonValueKind.String} json=>json.GetString(),_=>null}:null;
            var evidence=payment is not null?await SalesPaymentStepEvidence(user,review.Company,review.ActorUserId,review.EntityId,review.SourceRevision,template,payment):journal is not null?await SalesJournalStepEvidence(user,review.Company,review.ActorUserId,review.EntityId,review.SourceRevision,template,journal):receivable is not null?await SalesReceivableStepEvidence(user,review.Company,review.ActorUserId,review.EntityId,review.SourceRevision,template,receivable):
                await ReadStepEvidenceAsync(user,review.Company,review.ActorUserId,review.EntityType,review.EntityId,review.SourceRevision,template,await commands.GetRecordedCompanyAccountingStepCommandAsync(review.InstanceId));
            if(evidence.Eligibility!=(nativeRejection?CompanyAccountingSourceTypes.RejectionEligibility(review.EntityType):returning?"CORRECTION_WORK_EVIDENCE_REQUIRED":"ELIGIBLE") || evidence.Revision!=review.AccountingEvidenceRevision || evidence.Correction!=review.CorrectionEvidence)
                throw new InvalidOperationException("The recorded independent controller disposition differs from the original step review.");
            if(ProcessStepRoleChecks.RequiresIndependentActor(step,definition,template)!=review.RequiresIndependentActor ||
                (step.ApprovalRequired || template.StepType=="APPROVAL"?"Workflow.Approve":"Workflow.Execute")!=review.RequiredPermission)
                throw new InvalidOperationException("The original step command policy changed.");
            if(requireRecorded)
            {
                if(!Same(await commands.GetRecordedCompanyAccountingStepCommandAsync(review.InstanceId),command) || (returning?step.Status!=StepStatus.BLOCKED:step.Status!=StepStatus.COMPLETED || step.CompletedBy!=review.ActorUserId || step.Notes!=command.Notes))
                    throw new InvalidOperationException("The original completed command cannot be reconciled.");
                // Only exact persisted command evidence permits checks against its historical step.
                instance=JsonSerializer.Deserialize<ProcessInstance>(JsonSerializer.Serialize(instance))!;instance.CurrentStepId=review.StepId;
                if(returning){step=System.Text.Json.JsonSerializer.Deserialize<ProcessStepInstance>(System.Text.Json.JsonSerializer.Serialize(step))!;step.Status=StepStatus.PENDING;}
            }
            var principal=new ClaimsPrincipal(new ClaimsIdentity(roles.Where(user.IsInRole).Select(role=>new Claim(ClaimTypes.Role,role)),"company-command"));
            if(!ProcessStepRoleChecks.CanAct(principal,review.ActorUserId,instance,step,definition,template))throw new UnauthorizedAccessException("The current role or independent actor refuses the original step.");
            await Guard();return instance;
        }
        var retained=await commands.GetRecordedCompanyAccountingStepCommandAsync(review.InstanceId);
        var retry=retained?.IntentId==command.IntentId;
        if(retry && !Same(retained,command))throw new InvalidOperationException("The retained step intent changed.");
        // For a new AR or journal command, the complete initial/closing current-step review below already
        // verifies the original maker, receipt, definition, policy and current source. Historical
        // reconstruction is needed for exact retry and after the durable workflow write.
        if(retry || review.EntityType is not ("AR_INVOICE" or "JOURNAL_ENTRY" or "AR_PAYMENT"))await SourceAndHistoricalAuthority(retry);
        if(!retry)
        {
            var current=await ReviewStepAsync(user,review.InstanceId) ?? throw new InvalidOperationException("The current step review is unavailable.");
            if(current!=review)throw new InvalidOperationException("The submission differs from the current company step review.");
        }
        // Guard performs current RBAC/configuration reads only inside LIFECYCLE; PRODUCTION remains outside it.
        var receipt=await commands.CompleteCompanyAccountingStepAsync(command,Guard);receipt.Require(command);
        var closing=await SourceAndHistoricalAuthority(true);
        var audit=closing.History.Where(row=>row.HistoryId==receipt.HistoryId).ToArray();
        if(audit.Length!=1 || audit[0].Action!=(nativeRejection?CompanyAccountingSourceTypes.RejectionHistory(review.EntityType):returning?"CORRECTION_RETURNED":CompanyAccountingSourceTypes.IsNativeDecision(review.EntityType) && !review.RequiresIndependentActor && !(review.EntityType=="JOURNAL_ENTRY" && verifiedEvidenceAction==CompanyAccountingSourceTypes.JournalPosting)?CompanyAccountingSourceTypes.RejectionReviewHistory(review.EntityType):"STEP_COMPLETED") || audit[0].PerformedBy!=review.ActorUserId || audit[0].StepInstanceId!=review.StepInstanceId || audit[0].Notes!=command.Notes)
            throw new InvalidOperationException("The recorded step audit could not be confirmed.");
        await Guard();return receipt;
    }
}
