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
    private async Task<CompanySalesInvoiceReviewSource> ReadSalesInvoiceSourceAsync(ClaimsPrincipal user,CompanyScope company,string actor,string id,string? preparation=null)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();
        if(salesInvoices is null)throw new InvalidOperationException("The canonical company invoice source reader is required.");
        var source=await salesInvoices.ReadWorkflowSourceAsync(user,id);source.Require(company,actor,id);
        if(preparation is not null && CompanyAccountingSourceTypes.InvoicePreparationRevision(source.Original)!=preparation)throw new InvalidOperationException("The original invoice preparation receipt changed.");return source;
    }
    public async Task<CompanySalesInvoiceProcessSourceReview?> ReadSalesInvoiceProcessSourceAsync(ClaimsPrincipal user,string id)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var original=await context.CaptureProcessAsync(user);
        var process=await ReadAsync(user,id,original.Access.Scope,original.Actor);if(process is null)return null;
        if(process.EntityType!="INVOICE")throw new InvalidOperationException("An owned native invoice workflow is required.");
        var start=NativeDecisionStart(process,"INVOICE");var invoice=await ReadSalesInvoiceSourceAsync(user,original.Access.Scope,original.Actor,process.EntityId,start.SourceRevision);
        var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process));
        var result=new CompanySalesInvoiceProcessSourceReview(original.Access.Scope,original.Actor,id,process.EntityId,start.SourceRevision,revision,invoice);result.Require(original.Access.Scope,original.Actor,id);
        var closing=await ReadAsync(user,id,original.Access.Scope,original.Actor);var closingInvoice=await ReadSalesInvoiceSourceAsync(user,original.Access.Scope,original.Actor,process.EntityId,start.SourceRevision);var authority=await context.CaptureProcessAsync(user);
        if(closing is null || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(closing))!=revision || JsonSerializer.Serialize(invoice)!=JsonSerializer.Serialize(closingInvoice) ||
            authority.Actor!=original.Actor || authority.Access.Scope!=original.Access.Scope || authority.Binding!=original.Binding || !ReferenceEquals(authority.Process,original.Process) ||
            JsonSerializer.Serialize(authority.Access.Roles.Order(StringComparer.Ordinal))!=JsonSerializer.Serialize(original.Access.Roles.Order(StringComparer.Ordinal)) || JsonSerializer.Serialize(authority.Access.Permissions.Order(StringComparer.Ordinal))!=JsonSerializer.Serialize(original.Access.Permissions.Order(StringComparer.Ordinal)))
            throw new InvalidOperationException("The owned invoice process, source or current authority changed.");return CompanyInventorySourceReview.Copy(result);
    }
    private async Task<(string Eligibility,string? Revision,APInvoiceCorrectionStepEvidence? Correction)> SalesInvoiceStepEvidence(ClaimsPrincipal user,CompanyScope company,string actor,string id,string preparation,ProcessStepDefinition step)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var rejectionReview=NativeDecisionStep(step,"INVOICE",false);
        if(!NativeDecisionStep(step,"INVOICE",true) && !rejectionReview)throw new InvalidOperationException("An explicit native invoice approval or rejection-review step is required.");
        var source=await ReadSalesInvoiceSourceAsync(user,company,actor,id,preparation);var current=await production.CaptureAsync(user);var row=source.Original.Invoice;
        if(current.Access.Scope!=company || current.Actor!=actor || current.Binding!=source.Current.BindingRevision || current.Access.Roles?.Any(user.IsInRole)!=true || current.Access.Permissions?.Contains("Accounting.View",StringComparer.Ordinal)!=true ||
            row.ROW_CREATED_BY!=actor && !current.Access.Permissions.Contains("Accounting.ReviewSalesInvoice",StringComparer.Ordinal))throw new UnauthorizedAccessException("The original invoice step production authority changed.");
        var proof=row.REVIEW_WORKFLOW_JSON is null?null:CompanySalesInvoiceDecisionProof.Require(company,row);
        var eligibility=!source.HasCurrentOriginalSource?"SALES_INVOICE_SOURCE_CHANGED":rejectionReview?
            row.STATUS!="REJECTED"?"SALES_INVOICE_REJECTION_REQUIRED":row.ROW_CREATED_BY!=actor?"SALES_INVOICE_PREPARER_REQUIRED":current.Access.Permissions.Contains("Accounting.PrepareSalesInvoice",StringComparer.Ordinal)?"ELIGIBLE":"SALES_INVOICE_PREPARATION_PERMISSION_REQUIRED":
            proof is null?"SALES_INVOICE_APPROVAL_REQUIRED":proof.Intent.ROW_CREATED_BY!=actor?"SALES_INVOICE_REVIEWER_REQUIRED":!current.Access.Permissions.Contains("Accounting.RecordSalesInvoiceDecision",StringComparer.Ordinal)?"SALES_INVOICE_DECISION_PERMISSION_REQUIRED":
            !(await resources.CheckAssetAccessAsync(actor,source.Current.FieldId,"FIELD","WRITE")).HasAccess?"SALES_INVOICE_FIELD_WRITE_REQUIRED":row.STATUS=="REJECTED"?CompanyAccountingSourceTypes.RejectionEligibility("INVOICE"):"ELIGIBLE";
        var evidence=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Preparation=preparation,Invoice=row}));return(eligibility,evidence,null);
    }
}
