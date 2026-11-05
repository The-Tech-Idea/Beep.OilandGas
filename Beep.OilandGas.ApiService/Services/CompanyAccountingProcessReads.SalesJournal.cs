using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Processes;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyAccountingProcessReads
{
    public Task<(ProcessInstance Process,CompanyAccountingStepReview Review)> ReadAssignedJournalTaskAsync(ClaimsPrincipal user,UnifiedTask task,CompanyScope company,string actor)=>ReadAssignedNativeFinancialTaskAsync(user,task,company,actor,"JOURNAL_ENTRY");
    public Task<(ProcessInstance Process,CompanyAccountingStepReview Review)> ReadAssignedPaymentTaskAsync(ClaimsPrincipal user,UnifiedTask task,CompanyScope company,string actor)=>ReadAssignedNativeFinancialTaskAsync(user,task,company,actor,"AR_PAYMENT");
    private async Task<(ProcessInstance Process,CompanyAccountingStepReview Review)> ReadAssignedNativeFinancialTaskAsync(ClaimsPrincipal user,UnifiedTask task,CompanyScope company,string actor,string type)
    {
        if(task.ScopeKind!="COMPANY_ACCOUNTING" || task.EntityType!=type)throw new InvalidOperationException("An owned journal assignment is required.");
        ProcessInstance? process=null;var review=await ReviewStepCoreAsync(user,task.WorkflowName,value=>process=value) ?? throw new InvalidOperationException("The assigned journal step is unavailable.");
        review.Require(company,actor,task.WorkflowName);
        if(process is null || process.EntityType!=type || process.EntityId!=task.EntityId || review.EntityId!=task.EntityId || review.StepInstanceId!=task.StepName || review.AssignedRole!=task.AssignedRole || review.ProcessRevision!=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process)) || !WorkflowTaskProcessAdmission.Matches(task,process,actor))throw new InvalidOperationException("The assigned journal process and complete step evidence disagree.");
        return(process,review);
    }
    public async Task<CompanySalesJournalProcessSourceReview?> ReadSalesJournalProcessSourceAsync(ClaimsPrincipal user,string id)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var original=await context.CaptureProcessAsync(user);CompanySalesJournalWorkflowSource? journal=null;
        var process=await ReadCoreAsync(user,id,original.Access.Scope,original.Actor,null,source=>journal=source);if(process is null)return null;
        if(process.EntityType!="JOURNAL_ENTRY" || journal is null)throw new InvalidOperationException("An owned native journal workflow is required.");
        var start=NativeDecisionStart(process,"JOURNAL_ENTRY");var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process));
        var result=new CompanySalesJournalProcessSourceReview(original.Access.Scope,original.Actor,id,process.EntityId,start.SourceRevision,revision,journal);result.Require(original.Access.Scope,original.Actor,id);
        // Each native source observation finishes before any separately owned lifecycle operation.
        CompanySalesJournalWorkflowSource? closingJournal=null;var closing=await ReadCoreAsync(user,id,original.Access.Scope,original.Actor,null,source=>closingJournal=source);var authority=await context.CaptureProcessAsync(user);
        if(closing is null || closingJournal is null || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(closing))!=revision || closingJournal.Revision!=journal.Revision || authority.Actor!=original.Actor || authority.Access.Scope!=original.Access.Scope || authority.Binding!=original.Binding || !ReferenceEquals(authority.Process,original.Process) ||
            !authority.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(original.Access.Roles.Order(StringComparer.Ordinal)) || !authority.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(original.Access.Permissions.Order(StringComparer.Ordinal)))throw new InvalidOperationException("The owned journal process, native source or current authority changed.");return CompanyInventorySourceReview.Copy(result);
    }
    private async Task<CompanySalesJournalWorkflowSource> ReadSalesJournalSourceAsync(ClaimsPrincipal user,CompanyScope company,string actor,string id,string? preparation=null)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();if(journals is null)throw new InvalidOperationException("The actual company journal workflow source owner is required.");var original=await production.CaptureAsync(user);var roles=original.Access.Roles.Order(StringComparer.Ordinal).ToArray();var grants=original.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
        async Task Guard(){var current=await production.CaptureAsync(user);if(current.Access.Scope!=company || current.Actor!=actor || current.Binding!=original.Binding || !current.Access.Roles.Any(user.IsInRole) || !current.Access.Permissions.Contains("Accounting.View",StringComparer.Ordinal) || !roles.SequenceEqual(current.Access.Roles.Order(StringComparer.Ordinal)) || !grants.SequenceEqual(current.Access.Permissions.Order(StringComparer.Ordinal)))throw new UnauthorizedAccessException("The original journal workflow production authority changed.");}
        async Task Field(string value){await Guard();if(!(await resources.CheckAssetAccessAsync(actor,value,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original journal FIELD READ is required.");}
        await Guard();var source=await journals.GetCompanySalesJournalWorkflowSourceAsync(company,actor,original.Binding,id,Guard,Field);source.Require(company,actor,id);
        if(source.Journal.Header.ROW_CREATED_BY!=actor && !original.Access.Permissions.Contains("Accounting.ReviewSalesJournalSource",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current journal source permission is required to read another maker's private workflow evidence.");
        if(preparation is not null && source.SourceRevision!=preparation)throw new InvalidOperationException("The original completed native journal preparation changed.");await Guard();return source;
    }
    private async Task<(string Eligibility,string? Revision,APInvoiceCorrectionStepEvidence? Correction)> SalesJournalStepEvidence(ClaimsPrincipal user,CompanyScope company,string actor,string id,string preparation,ProcessStepDefinition step,CompanySalesJournalWorkflowSource? captured=null)
    {
        var source=captured ?? await ReadSalesJournalSourceAsync(user,company,actor,id,preparation);source.Require(company,actor,id);if(source.SourceRevision!=preparation)throw new InvalidOperationException("The captured original journal preparation changed.");var current=await production.CaptureAsync(user);if(current.Access.Scope!=company || current.Actor!=actor || current.Binding!=source.BindingRevision || !current.Access.Roles.Any(user.IsInRole) || !current.Access.Permissions.Contains("Accounting.View",StringComparer.Ordinal) || source.Journal.Header.ROW_CREATED_BY!=actor && !current.Access.Permissions.Contains("Accounting.ReviewSalesJournalSource",StringComparer.Ordinal))throw new UnauthorizedAccessException("The original journal step production authority changed.");var row=source.Journal.Header;var posting=JournalPostingStep(step);var rejection=NativeDecisionStep(step,"JOURNAL_ENTRY",false);
        if(!posting && !rejection && !NativeDecisionStep(step,"JOURNAL_ENTRY",true))throw new InvalidOperationException("An explicit native journal review, posting or rejection-review step is required.");
        var proof=row.REVIEW_WORKFLOW_JSON is null?null:CompanySalesJournalReviewProof.Require(row);string eligibility;
        if(posting)eligibility=row.STATUS!="POSTED"?"SALES_JOURNAL_POSTING_REQUIRED":row.ROW_CHANGED_BY!=actor?"SALES_JOURNAL_POSTER_REQUIRED":!current.Access.Permissions.Contains("Accounting.PostSalesJournal",StringComparer.Ordinal)?"SALES_JOURNAL_POSTING_PERMISSION_REQUIRED":"ELIGIBLE";
        else if(rejection)eligibility=row.STATUS!="REJECTED"?"SALES_JOURNAL_REJECTION_REQUIRED":row.ROW_CREATED_BY!=actor?"SALES_JOURNAL_PREPARER_REQUIRED":!current.Access.Permissions.Contains("Accounting.PrepareSalesJournal",StringComparer.Ordinal)?"SALES_JOURNAL_PREPARATION_PERMISSION_REQUIRED":"ELIGIBLE";
        else eligibility=proof is null?"SALES_JOURNAL_APPROVAL_REQUIRED":proof.Intent.ROW_CREATED_BY!=actor?"SALES_JOURNAL_REVIEWER_REQUIRED":!current.Access.Permissions.Contains("Accounting.RecordSalesJournalDecision",StringComparer.Ordinal)?"SALES_JOURNAL_DECISION_PERMISSION_REQUIRED":row.STATUS=="REJECTED"?CompanyAccountingSourceTypes.RejectionEligibility("JOURNAL_ENTRY"):"ELIGIBLE";
        if(eligibility is "ELIGIBLE" or "SALES_JOURNAL_REJECTION_RECORDED" && !(await resources.CheckAssetAccessAsync(actor,row.FIELD_ID!,"FIELD","WRITE")).HasAccess)eligibility="SALES_JOURNAL_FIELD_WRITE_REQUIRED";
        return(eligibility,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Preparation=preparation,source.Journal})),null);
    }
    private static bool JournalPostingStep(ProcessStepDefinition step)
    {
        if(step.StepConfiguration is not {Count:1} || step.RequiresApproval || step.StepType!="ACTION" || !step.StepConfiguration.TryGetValue(CompanyAccountingStepEvidence.ConfigurationKey,out var value))return false;return value switch{string text=>text==CompanyAccountingSourceTypes.JournalPosting,JsonElement {ValueKind:JsonValueKind.String} json=>json.GetString()==CompanyAccountingSourceTypes.JournalPosting,_=>false};
    }
}
