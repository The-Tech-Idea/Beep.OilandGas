using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Processes;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyAccountingProcessReads
{
    public Task<CompanyAccountingStepReview?> ReviewStepAsync(ClaimsPrincipal user,string id)=>ReviewStepCoreAsync(user,id,null);
    private async Task<CompanyAccountingStepReview?> ReviewStepCoreAsync(ClaimsPrincipal user,string id,Action<ProcessInstance>? captureProcess)
    {
        using var invoiceValidation=CompanySalesInvoiceValidation.Enter();
        if(string.IsNullOrWhiteSpace(id) || id.Length>128 || id.Any(char.IsControl))throw new ArgumentException("A recorded process identity is required.");
        var original=await context.CaptureProcessAsync(user);
        var originalRoles=original.Access.Roles?.Order(StringComparer.Ordinal).ToArray() ?? throw new UnauthorizedAccessException("Current company roles are required.");
        var originalPermissions=original.Access.Permissions?.Order(StringComparer.Ordinal).ToArray() ?? throw new UnauthorizedAccessException("Current company permissions are required.");
        void Guard(RepositoryCompanyAccess access,string actor,string binding)
        {
            if(access.Scope!=original.Access.Scope || actor!=original.Actor || binding!=original.Binding || access.Roles is null || access.Permissions is null ||
                !access.Roles.Order(StringComparer.Ordinal).SequenceEqual(originalRoles) ||
                !access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(originalPermissions))
                throw new UnauthorizedAccessException("The original company step authority changed.");
        }
        async Task<CompanyAccountingStepReview?> Capture()
        {
            var captured=await ReadWithNativeSourcesAsync(user,id,original.Access.Scope,original.Actor);var instance=captured.Process;if(instance is null)return null;
            var current=await context.CaptureProcessAsync(user);Guard(current.Access,current.Actor,current.Binding);
            var definition=await original.Process.GetProcessDefinitionAsync(instance.ProcessId) ?? throw new InvalidOperationException("The recorded step definition is unavailable.");
            var definitionRevision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition));
            if(!definition.IsActive || definition.ProcessType!="FINANCIAL" || definition.EntityType!=instance.EntityType || definition.ProcessId!=instance.ProcessId ||
                definition.Steps is null || instance.StepInstances is null || instance.Status!=ProcessStatus.IN_PROGRESS ||
                !instance.ProcessData.Data.TryGetValue(CompanyAccountingProcessStart.PayloadKey,out var payload))
                throw new InvalidOperationException("An active recorded company workflow is required.");
            CompanyAccountingProcessStart start;
            try{start=JsonSerializer.Deserialize<CompanyAccountingProcessStart>(JsonSerializer.Serialize(payload)) ?? throw new InvalidOperationException("The original handoff intent is unavailable.");start.Require();}
            catch(Exception invalid) when(invalid is JsonException or ArgumentException){throw new InvalidOperationException("The original handoff intent is invalid.",invalid);}
            if(start.Company!=original.Access.Scope || start.InstanceId()!=id || start.ActorUserId!=instance.StartedBy || start.ProcessId!=instance.ProcessId ||
                start.EntityType!=instance.EntityType || start.EntityId!=instance.EntityId || start.BindingRevision!=original.Binding || start.DefinitionRevision!=definitionRevision)
                throw new InvalidOperationException("The original company handoff or definition changed.");
            // The boundary's verified native snapshot supplies maker/preparation and financial evidence together.
            // Closing Capture rereads the full process and source; authorization and datasource reads stay uncached.
            var receivable=instance.EntityType=="AR_INVOICE"?captured.Receivable ?? throw new InvalidOperationException("The verified receivable process source is unavailable."):null;
            var journal=instance.EntityType=="JOURNAL_ENTRY"?captured.Journal ?? throw new InvalidOperationException("The verified journal process source is unavailable."):null;
            var payment=instance.EntityType=="AR_PAYMENT"?captured.Payment ?? throw new InvalidOperationException("The verified payment process source is unavailable."):null;
            var source=payment is not null?(Revision:payment.SourceRevision,Preparer:payment.Payment.CreatedBy):journal is not null?(Revision:journal.SourceRevision,Preparer:journal.Journal.Header.ROW_CREATED_BY):receivable is null?await ReadStartSourceAsync(user,original.Access.Scope,original.Actor,instance.EntityType,instance.EntityId,start.SourceRevision):
                (Revision:CompanyAccountingSourceTypes.ReceivablePreparationRevision(receivable.Original),Preparer:receivable.Original.Intent.ROW_CREATED_BY);
            if(source.Revision!=start.SourceRevision || source.Preparer!=start.SourcePreparerUserId)throw new InvalidOperationException("The original source maker changed.");
            var steps=instance.StepInstances.Where(step=>step.StepId==instance.CurrentStepId).ToArray();
            var templates=definition.Steps.Where(step=>step.StepId==instance.CurrentStepId).ToArray();
            if(steps.Length!=1 || templates.Length!=1 || steps[0].InstanceId!=id || steps[0].Status is not (StepStatus.PENDING or StepStatus.IN_PROGRESS) ||
                templates[0].RequiredRoles is null || templates[0].RequiredRoles.Count!=1 || steps[0].RequiredRole!=templates[0].RequiredRoles[0] ||
                steps[0].ApprovalRequired!=templates[0].RequiresApproval || steps[0].StepName!=templates[0].StepName)
                throw new InvalidOperationException("The current assigned step is ambiguous or changed.");
            var step=steps[0];var template=templates[0];var permission=step.ApprovalRequired || template.StepType=="APPROVAL"?"Workflow.Approve":"Workflow.Execute";
            var independent=ProcessStepRoleChecks.RequiresIndependentActor(step,definition,template);
            var permitted=current.Access.Permissions.Contains(permission,StringComparer.Ordinal);
            var authoritative=new ClaimsPrincipal(new ClaimsIdentity(current.Access.Roles.Where(user.IsInRole).Select(role=>new Claim(ClaimTypes.Role,role)),"company-review"));
            var canAct=ProcessStepRoleChecks.CanAct(authoritative,current.Actor,instance,step,definition,template);
            var evidence=payment is not null?await SalesPaymentStepEvidence(user,original.Access.Scope,original.Actor,instance.EntityId,source.Revision,template,payment):journal is not null?await SalesJournalStepEvidence(user,original.Access.Scope,original.Actor,instance.EntityId,source.Revision,template,journal):receivable is not null?await SalesReceivableStepEvidence(user,original.Access.Scope,original.Actor,instance.EntityId,source.Revision,template,receivable):
                await ReadStepEvidenceAsync(user,original.Access.Scope,original.Actor,instance.EntityType,instance.EntityId,source.Revision,template,original.Process is ICompanyAccountingProcessCommands commandOwner?await commandOwner.GetRecordedCompanyAccountingStepCommandAsync(id):null);
            var eligibility=!permitted?"WORKFLOW_PERMISSION_REQUIRED":!canAct?"ROLE_OR_INDEPENDENCE_REQUIRED":evidence.Eligibility;
            if(CompanyAccountingSourceTypes.IsNativeDecision(instance.EntityType) && eligibility==CompanyAccountingSourceTypes.RejectionEligibility(instance.EntityType))
            {
                var ordered=definition.Steps.OrderBy(row=>row.SequenceNumber).ToArray();var position=Array.FindIndex(ordered,row=>row.StepId==template.StepId);
                if(position!=0 || (instance.EntityType=="JOURNAL_ENTRY"?ordered.Length!=3 || !JournalPostingStep(ordered[1]) || !NativeDecisionStep(ordered[2],instance.EntityType,false):ordered.Length!=2 || !NativeDecisionStep(ordered[1],instance.EntityType,false)))eligibility=CompanyAccountingSourceTypes.Prefix(instance.EntityType)+"_RETURN_CONFIGURATION_REQUIRED";
            }
            if(eligibility=="ELIGIBLE" && evidence.Correction?.Action==CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork)
            {
                var ordered=definition.Steps.OrderBy(row=>row.SequenceNumber).ToArray();var position=Array.FindIndex(ordered,row=>row.StepId==template.StepId);
                var next=position>=0 && position+1<ordered.Length?ordered[position+1]:null;
                var action=next?.StepConfiguration?.TryGetValue(CompanyAccountingStepEvidence.ConfigurationKey,out var configured)==true
                    ?configured switch{string text=>text,JsonElement {ValueKind:JsonValueKind.String} json=>json.GetString(),_=>null}:null;
                if(action!=CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement || next?.RequiresApproval!=true || next.StepType!="APPROVAL")eligibility="CORRECTION_ACKNOWLEDGEMENT_REQUIRED";
            }
            var journalAction=instance.EntityType=="JOURNAL_ENTRY"?JournalPostingStep(template)?CompanyAccountingSourceTypes.JournalPosting:NativeDecisionStep(template,"JOURNAL_ENTRY",true)?CompanyAccountingSourceTypes.Approval("JOURNAL_ENTRY"):CompanyAccountingSourceTypes.RejectionReview("JOURNAL_ENTRY"):null;
            var revision=ProcessDefinitionRevision.FromJson(journalAction is null?JsonSerializer.Serialize(new{instance,definition,source,original.Binding,original.Actor,eligibility,permission,AccountingEvidenceRevision=evidence.Revision,CorrectionEvidence=evidence.Correction}):JsonSerializer.Serialize(new{instance,definition,source,original.Binding,original.Actor,eligibility,permission,AccountingEvidenceRevision=evidence.Revision,CorrectionEvidence=evidence.Correction,AccountingEvidenceAction=journalAction}));
            var result=new CompanyAccountingStepReview(original.Access.Scope,original.Actor,id,instance.ProcessId,instance.EntityType,instance.EntityId,
                step.StepInstanceId,step.StepId,step.StepName,step.RequiredRole!,source.Preparer,instance.StartedBy,independent,permission,eligibility=="ELIGIBLE",eligibility,
                source.Revision,definitionRevision,original.Binding,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(instance)),revision,evidence.Revision,evidence.Correction,journalAction);
            result.Require(original.Access.Scope,original.Actor,id);captureProcess?.Invoke(CompanyInventorySourceReview.Copy(instance));return result;
        }
        var initial=await Capture();if(initial is null)return null;var closing=await Capture();
        if(closing!=initial)throw new InvalidOperationException("The current process, source or step authority changed during review.");
        var final=await context.CaptureProcessAsync(user);Guard(final.Access,final.Actor,final.Binding);
        return initial;
    }
}
