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
    private async Task<CompanySalesReceivableReviewSource> ReadSalesReceivableSourceAsync(ClaimsPrincipal user,CompanyScope company,string actor,string id,string? preparation=null)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();if(salesReceivables is null)throw new InvalidOperationException("The canonical company receivable source reader is required.");
        var source=await salesReceivables.CaptureWorkflowSourceAsync(user,id);source.Require(company,actor,id);
        if(preparation is not null && CompanyAccountingSourceTypes.ReceivablePreparationRevision(source.Original)!=preparation)throw new InvalidOperationException("The original receivable draft receipt changed.");return source;
    }
    public async Task<CompanySalesReceivableProcessSourceReview?> ReadSalesReceivableProcessSourceAsync(ClaimsPrincipal user,string id)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var original=await context.CaptureProcessAsync(user);var captured=await ReadWithNativeSourcesAsync(user,id,original.Access.Scope,original.Actor);var process=captured.Process;if(process is null)return null;
        if(process.EntityType!="AR_INVOICE")throw new InvalidOperationException("An owned native receivable workflow is required.");
        var start=NativeDecisionStart(process,"AR_INVOICE");var source=captured.Receivable ?? throw new InvalidOperationException("The verified receivable process source is unavailable.");var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process));
        var result=new CompanySalesReceivableProcessSourceReview(original.Access.Scope,original.Actor,id,process.EntityId,start.SourceRevision,revision,source);result.Require(original.Access.Scope,original.Actor,id);
        var closing=await ReadWithNativeSourcesAsync(user,id,original.Access.Scope,original.Actor);var authority=await context.CaptureProcessAsync(user);
        if(closing.Process is null || closing.Receivable is null || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(closing.Process))!=revision || source.Revision!=closing.Receivable.Revision || authority.Actor!=original.Actor || authority.Access.Scope!=original.Access.Scope || authority.Binding!=original.Binding || !ReferenceEquals(authority.Process,original.Process) ||
            JsonSerializer.Serialize(authority.Access.Roles.Order(StringComparer.Ordinal))!=JsonSerializer.Serialize(original.Access.Roles.Order(StringComparer.Ordinal)) || JsonSerializer.Serialize(authority.Access.Permissions.Order(StringComparer.Ordinal))!=JsonSerializer.Serialize(original.Access.Permissions.Order(StringComparer.Ordinal)))throw new InvalidOperationException("The owned receivable process, source or current authority changed.");return CompanyInventorySourceReview.Copy(result);
    }
    private async Task<(string Eligibility,string? Revision,APInvoiceCorrectionStepEvidence? Correction)> SalesReceivableStepEvidence(ClaimsPrincipal user,CompanyScope company,string actor,string id,string preparation,ProcessStepDefinition step,CompanySalesReceivableReviewSource? captured=null)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var rejectionReview=NativeDecisionStep(step,"AR_INVOICE",false);
        if(!NativeDecisionStep(step,"AR_INVOICE",true) && !rejectionReview)throw new InvalidOperationException("An explicit native receivable approval or rejection-review step is required.");
        var source=captured ?? await ReadSalesReceivableSourceAsync(user,company,actor,id,preparation);source.Require(company,actor,id);
        if(CompanyAccountingSourceTypes.ReceivablePreparationRevision(source.Original)!=preparation)throw new InvalidOperationException("The captured receivable preparation changed.");
        var current=await production.CaptureAsync(user);var row=source.Original.Receivable;
        if(current.Access.Scope!=company || current.Actor!=actor || current.Binding!=source.BindingRevision || current.Access.Roles?.Any(user.IsInRole)!=true || current.Access.Permissions?.Contains("Accounting.View",StringComparer.Ordinal)!=true ||
            row.ROW_CREATED_BY!=actor && !current.Access.Permissions.Contains("Accounting.ReviewSalesReceivable",StringComparer.Ordinal))throw new UnauthorizedAccessException("The original receivable step production authority changed.");
        var proof=row.REVIEW_WORKFLOW_JSON is null?null:CompanySalesReceivableDecisionProof.Require(company,row);
        var eligibility=!source.HasCurrentOriginalSource?"SALES_RECEIVABLE_SOURCE_CHANGED":rejectionReview?
            row.STATUS!="REJECTED"?"SALES_RECEIVABLE_REJECTION_REQUIRED":row.ROW_CREATED_BY!=actor?"SALES_RECEIVABLE_PREPARER_REQUIRED":current.Access.Permissions.Contains("Accounting.PrepareSalesReceivable",StringComparer.Ordinal)?"ELIGIBLE":"SALES_RECEIVABLE_PREPARATION_PERMISSION_REQUIRED":
            proof is null?"SALES_RECEIVABLE_APPROVAL_REQUIRED":proof.Intent.ROW_CREATED_BY!=actor?"SALES_RECEIVABLE_REVIEWER_REQUIRED":!current.Access.Permissions.Contains("Accounting.RecordSalesReceivableDecision",StringComparer.Ordinal)?"SALES_RECEIVABLE_DECISION_PERMISSION_REQUIRED":
            !(await resources.CheckAssetAccessAsync(actor,source.FieldId,"FIELD","WRITE")).HasAccess?"SALES_RECEIVABLE_FIELD_WRITE_REQUIRED":row.STATUS=="REJECTED"?CompanyAccountingSourceTypes.RejectionEligibility("AR_INVOICE"):"ELIGIBLE";
        return(eligibility,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Preparation=preparation,Receivable=row})),null);
    }
}
