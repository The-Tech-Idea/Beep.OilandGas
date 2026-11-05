using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Data.Security;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanySalesInvoiceReads
{
    public async Task<SALES_INVOICE_REVIEW_INTENT?> ReadReviewIntentAsync(ClaimsPrincipal user,string invoiceId)
    {
        var authority=await Authority(user,false);
        async Task Guard(){await authority.Guard();var current=await context.CaptureAsync(user);if(!current.Access.Permissions.Contains(PermissionConstants.Accounting.ReviewSalesInvoice,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current invoice review authority is required.");}
        async Task Field(string field){await Guard();if(!(await assets.CheckAssetAccessAsync(authority.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current invoice FIELD access is required.");}
        var intent=await invoices.GetCompanyInvoiceReviewIntentAsync(authority.Company,authority.Actor,invoiceId,Guard,Field);await Guard();return intent;
    }
    public async Task<CompanySalesInvoiceDraftOutcome> RecordDecisionAsync(ClaimsPrincipal user,CompanySalesInvoiceDecisionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);command=CompanyInventorySourceReview.Copy(command);command.Require();
        var authority=await Authority(user,false,command.Company,command.ActorUserId,command.Review.Current.BindingRevision,command.Review.Current.FieldId);
        async Task Guard()
        {
            await authority.Guard();var current=await context.CaptureAsync(user);
            if(!current.Access.Permissions.Contains(PermissionConstants.Accounting.ReviewSalesInvoice,StringComparer.Ordinal) || !current.Access.Permissions.Contains(PermissionConstants.Accounting.RecordSalesInvoiceDecision,StringComparer.Ordinal) ||
                !(await assets.CheckAssetAccessAsync(authority.Actor,command.Review.Current.FieldId,"FIELD","WRITE")).HasAccess)throw new UnauthorizedAccessException("Current independent invoice disposition and FIELD WRITE authority are required.");
        }
        async Task<CompanySalesInvoiceReviewSource> Current(){await Guard();var review=await ReadReviewAsync(user,command.InvoiceId);await Guard();return review;}
        await Guard();var result=await invoices.RecordCompanyInvoiceDecisionAsync(command,Guard,Current);await Guard();
        if(System.Text.Json.JsonSerializer.Serialize(CompanySalesInvoiceDecisionProof.Require(command.Company,result.Invoice).Intent.Restore(command.Company,command.ActorUserId))!=System.Text.Json.JsonSerializer.Serialize(command))throw new InvalidOperationException("The original recorded invoice disposition is unconfirmed.");return result;
    }
    public Task<CompanySalesInvoiceReviewSource> ReadReviewAsync(ClaimsPrincipal user,string invoiceId)=>ReadReviewCore(user,invoiceId,false);
    public Task<CompanySalesInvoiceReviewSource> ReadWorkflowSourceAsync(ClaimsPrincipal user,string invoiceId)=>ReadReviewCore(user,invoiceId,true);
    public Task<CompanySalesInvoiceReviewSource> ReadReceivableSourceAsync(ClaimsPrincipal user,string invoiceId)=>ReadReviewCore(user,invoiceId,false,PermissionConstants.Accounting.PrepareSalesReceivable);
    public Task<CompanySalesInvoiceReviewSource> ReadReceivableReviewSourceAsync(ClaimsPrincipal user,string invoiceId)=>ReadReviewCore(user,invoiceId,false,PermissionConstants.Accounting.ReviewSalesReceivable);
    internal async Task<CompanySalesInvoiceReviewSource> ReadReceivableMakerWorkflowSourceAsync(ClaimsPrincipal user,CompanySalesReceivableOutcome native)
    {
        // Called only after the AR owner recovered and FIELD-authorized this maker's native outcome.
        using var validation=CompanySalesInvoiceValidation.Enter();var authority=await Authority(user,false);
        var preparation=native.Intent.Restore(authority.Company,authority.Actor);native.Require(preparation);
        var result=await ReadReviewCore(user,preparation.Source.InvoiceId,false,PermissionConstants.Accounting.View);await authority.Guard();
        result.Require(authority.Company,authority.Actor,preparation.Source.InvoiceId);return result;
    }
    private async Task<CompanySalesInvoiceReviewSource> ReadReviewCore(ClaimsPrincipal user,string invoiceId,bool allowOriginalMaker,string? receivablePermission=null)
    {
        var authority=await Authority(user,false);string? field=null;string? maker=null;
        async Task Guard()
        {
            await authority.Guard();var current=await context.CaptureAsync(user);
            if(receivablePermission is not null ? !current.Access.Permissions.Contains(receivablePermission,StringComparer.Ordinal) :
                (!allowOriginalMaker || maker is not null && maker!=authority.Actor) && !current.Access.Permissions.Contains(PermissionConstants.Accounting.ReviewSalesInvoice,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current source purpose authority is required.");
            if(field is not null && !(await assets.CheckAssetAccessAsync(authority.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original invoice FIELD access is required.");
        }
        async Task Field(string discovered)
        {if(field is not null && field!=discovered)throw new InvalidOperationException("The original invoice review FIELD changed.");field=discovered;await Guard();}
        async Task<CompanySalesInvoiceReviewSource> Capture()
        {
            await Guard();var original=await invoices.GetCompanyInvoiceDraftForReviewAsync(authority.Company,invoiceId,Guard,Field);
            if(maker is not null && maker!=original.Intent.ROW_CREATED_BY)throw new InvalidOperationException("The original invoice maker changed.");maker=original.Intent.ROW_CREATED_BY;await Guard();
            var workspace=await ReadCore(user,original.Invoice.SOURCE_RUN_TICKET_ID!,false);await Guard();
            var result=CompanySalesInvoiceReviewSource.Create(original,workspace.Source);result.Require(authority.Company,authority.Actor,invoiceId);return result;
        }
        var first=await Capture();var closing=await Capture();await Guard();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete invoice and current review source changed.");return CompanyInventorySourceReview.Copy(closing);
    }
}
