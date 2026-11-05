using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Processes;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyAccountingProcessReads
{
    public async Task<CompanyAccountingStepReview?> ReviewAssignedStepEvidenceAsync(System.Security.Claims.ClaimsPrincipal user,UnifiedTask task,ProcessInstance process,CompanyScope company,string actor)
    {
        // Existing external workflows have no company command intent; their admission remains owned by the inbox.
        if(!process.ProcessData.Data.ContainsKey(CompanyAccountingProcessStart.PayloadKey))return null;
        var current=await context.CaptureProcessAsync(user);
        if(current.Access.Scope!=company || current.Actor!=actor)throw new UnauthorizedAccessException("The assigned accounting evidence owner changed.");
        var definition=await current.Process.GetProcessDefinitionAsync(process.ProcessId)??throw new InvalidOperationException("The assigned accounting definition is unavailable.");
        var templates=definition.Steps.Where(step=>step.StepId==process.CurrentStepId).ToArray();
        if(templates.Length!=1 || templates[0].StepConfiguration is null)throw new InvalidOperationException("The assigned accounting step configuration is ambiguous.");
        if(!CompanyAccountingSourceTypes.IsNativeDecision(process.EntityType) && !templates[0].StepConfiguration.ContainsKey(CompanyAccountingStepEvidence.ConfigurationKey))return null;
        var review=await ReviewStepAsync(user,process.InstanceId)??throw new InvalidOperationException("The assigned accounting step review is unavailable.");
        review.Require(company,actor,task.WorkflowName);
        if(review.StepInstanceId!=task.StepName || review.EntityId!=task.EntityId || review.EntityType!=task.EntityType || review.AssignedRole!=task.AssignedRole)
            throw new InvalidOperationException("The current accounting step differs from the recorded task assignment.");
        return review.AccountingEvidenceRevision is null?null:review;
    }
    private async Task<(string Eligibility,string? Revision,APInvoiceCorrectionStepEvidence? Correction)> ReadStepEvidenceAsync(System.Security.Claims.ClaimsPrincipal user,CompanyScope company,string actor,string type,string id,string sourceRevision,ProcessStepDefinition template,CompanyAccountingStepCommand? recordedCorrectionWork)
    {
        if(type=="SALES_CONTRACT")return await CommercialStepEvidence(user,company,actor,id,sourceRevision,template);
        if(type=="RUN_TICKET")return await DeliveryStepEvidence(user,company,actor,id,sourceRevision,template);
        if(type=="INVOICE")return await SalesInvoiceStepEvidence(user,company,actor,id,sourceRevision,template);
        if(type=="AR_PAYMENT")return await SalesPaymentStepEvidence(user,company,actor,id,sourceRevision,template);
        if(type=="JOURNAL_ENTRY")return await SalesJournalStepEvidence(user,company,actor,id,sourceRevision,template);
        if(type=="AR_INVOICE")return await SalesReceivableStepEvidence(user,company,actor,id,sourceRevision,template);
        if(template.StepConfiguration is null)throw new InvalidOperationException("The configured accounting step evidence is unavailable.");
        if(!template.StepConfiguration.TryGetValue(CompanyAccountingStepEvidence.ConfigurationKey,out var configured))return("ELIGIBLE",null,null);
        var action=configured switch{string text=>text,JsonElement {ValueKind:JsonValueKind.String} json=>json.GetString(),_=>null};
        var correction=action==CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork;
        if(type!="AP_INVOICE" || template.StepConfiguration.Count!=1 ||
            (correction?template.RequiresApproval || template.StepType!="ACTION":
            action is not (CompanyAccountingStepEvidence.InvoiceVarianceDisposition or CompanyAccountingStepEvidence.InvoiceVarianceCorrectionRequest or CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement) || !template.RequiresApproval))
            throw new InvalidOperationException("The configured accounting evidence action is unsupported or lacks independent approval.");
        var source=await invoices.GetSourceReviewAsync(company,id,actor)??throw new InvalidOperationException("The configured invoice evidence is unavailable.");source.Require(company,actor,id);
        if(source.ForCompanyWorkflow().Revision!=sourceRevision)throw new InvalidOperationException("The original variance handoff source changed.");
        _=APInvoiceVarianceProof.Require(company,source.Invoice);
        var evidenceRevision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{source.Revision,source.Invoice.VARIANCE_DECISION_JSON}));
        if(source.Invoice.VARIANCE_DECISION_JSON is null)return("RECORDED_DISPOSITION_REQUIRED",evidenceRevision,null);
        var proof=APInvoiceVarianceDecisionProof.Require(company,source.Invoice);
        if((correction || action==CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement || action==CompanyAccountingStepEvidence.InvoiceVarianceCorrectionRequest) && proof.Command.Disposition!="REQUEST_CORRECTION")
            return("REQUEST_CORRECTION_REQUIRED",evidenceRevision,null);
        if(correction || action==CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement)
        {
            var proposal=APInvoiceVarianceProof.Require(company,source.Invoice);
            if(correction && proposal.Command.ActorUserId!=actor)return("ORIGINAL_PREPARER_REQUIRED",evidenceRevision,null);
            if(source.Invoice.CORRECTION_HISTORY_JSON is null)return("CORRECTION_DATA_REQUIRED",evidenceRevision,null);
            var original=await production.CaptureAsync(user);
            var roles=original.Access.Roles.Order(StringComparer.Ordinal).ToArray();var permissions=original.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
            async Task<(RepositoryCompanyAccess Access,string Binding)> Guard()
            {
                var current=await production.CaptureAsync(user);
                if(current.Actor!=actor || current.Access.Scope!=company || current.Binding!=original.Binding ||
                    !current.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(roles) || !current.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(permissions) ||
                    !current.Access.Permissions.Contains(correction?"Accounting.CorrectInvoice":"Accounting.ReviewInvoiceVariance",StringComparer.Ordinal))
                    throw new UnauthorizedAccessException("Current correction work or independent acknowledgement authority changed.");
                return(current.Access,current.Binding);
            }
            var financial=await APInvoiceReadChecks.ReadFinancialReviewAsync(user,actor,id,source.Revision,Guard,invoices.GetSourceReviewAsync,orders.GetSourceReviewAsync,
                invoices.GetReceiptAllocationsForLineAsync,policies.GetCompanyMatchingPoliciesAsync,invoices.GetReceiptConsumptionAsync);
            await Guard();string? workIntent=null;
            if(!correction)
            {
                var prior=recordedCorrectionWork?.Review.CorrectionEvidence;
                var history=APInvoiceCorrectionHistory.Require(company,source.Invoice,source.Lines);var latest=history.Versions[^1].Command;
                if(prior is null || recordedCorrectionWork!.Review.Company!=company || recordedCorrectionWork.Review.EntityId!=id ||
                    prior.CorrectionVersion>latest.Version)
                    return("CORRECTION_WORK_EVIDENCE_REQUIRED",evidenceRevision,null);
                recordedCorrectionWork.Require();
                if(prior.Action==CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork && prior.CorrectionVersion<latest.Version || prior.Action==CompanyAccountingStepEvidence.InvoiceCorrectionReturn)
                {
                    var originalWork=prior.Action==CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork?recordedCorrectionWork.IntentId:prior.CorrectionWorkIntentId;
                    var returned=APInvoiceCorrectionStepEvidence.Create(financial,original.Binding,CompanyAccountingStepEvidence.InvoiceCorrectionReturn,originalWork);
                    return("CORRECTION_WORK_EVIDENCE_REQUIRED",returned.Revision,returned);
                }
                if(prior.CorrectionVersion!=latest.Version || prior.CorrectionIntentId!=latest.IntentId || prior.HistoryRevision!=ProcessDefinitionRevision.FromJson(source.Invoice.CORRECTION_HISTORY_JSON!) || prior.SourceRevision!=source.Revision)
                    return("CORRECTION_WORK_EVIDENCE_REQUIRED",evidenceRevision,null);
                workIntent=prior.Action==CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork?recordedCorrectionWork.IntentId:prior.CorrectionWorkIntentId;
            }
            var evidence=APInvoiceCorrectionStepEvidence.Create(financial,original.Binding,action!,workIntent);
            return("ELIGIBLE",evidence.Revision,evidence);
        }
        return(proof.Command.ActorUserId==actor?"ELIGIBLE":"RECORDED_CONTROLLER_REQUIRED",evidenceRevision,null);
    }
}
