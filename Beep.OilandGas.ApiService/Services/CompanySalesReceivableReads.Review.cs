using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Data.Security;
namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanySalesReceivableReads
{
    private static void ReviewReceivableId(string receivable){if(!Guid.TryParseExact(receivable,"N",out var id) || id==Guid.Empty)throw new ArgumentException("The original receivable identity is required.");}
    public Task<CompanySalesReceivableReviewSource> ReadReviewAsync(ClaimsPrincipal user,string receivable)=>ReadReviewCore(user,receivable,false);
    public Task<CompanySalesReceivableReviewSource> ReadWorkflowSourceAsync(ClaimsPrincipal user,string receivable)=>ReadReviewCore(user,receivable,true);
    // The company process reader already performs a complete initial and closing capture.
    internal Task<CompanySalesReceivableReviewSource> CaptureWorkflowSourceAsync(ClaimsPrincipal user,string receivable)=>ReadReviewCore(user,receivable,true,false);
    private async Task<CompanySalesReceivableReviewSource> ReadReviewCore(ClaimsPrincipal user,string receivable,bool allowOriginalMaker,bool close=true,CancellationToken cancellationToken=default)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();ReviewReceivableId(receivable);var authority=await Authority(user);string? field=null;string? maker=null;
        async Task Guard()
        {
            cancellationToken.ThrowIfCancellationRequested();
            await authority.Guard();var current=await context.CaptureAsync(user);
            if((!allowOriginalMaker || maker is not null && maker!=authority.Actor) && !current.Access.Permissions.Contains(PermissionConstants.Accounting.ReviewSalesReceivable,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current receivable review authority is required.");
            if(field is not null && !(await assets.CheckAssetAccessAsync(authority.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original receivable FIELD READ authority is required.");
        }
        async Task Field(string value){if(field is not null && field!=value)throw new InvalidOperationException("The original receivable FIELD changed.");field=value;await Guard();}
        async Task<CompanySalesReceivableReviewSource> Capture()
        {
            await Guard();var original=await receivables.GetCompanySalesReceivableForReviewAsync(authority.Company,receivable,Guard,Field);
            if(maker is not null && maker!=original.Intent.ROW_CREATED_BY)throw new InvalidOperationException("The original receivable maker changed.");maker=original.Intent.ROW_CREATED_BY;await Guard();
            var invoice=allowOriginalMaker && maker==authority.Actor?await invoiceReads.ReadReceivableMakerWorkflowSourceAsync(user,original):await invoiceReads.ReadReceivableReviewSourceAsync(user,original.Receivable.SOURCE_INVOICE_ID!);await Guard();
            var rows=await receivables.GetCompanySalesReceivablesAsync(authority.Company,field!,invoice.InvoiceId,Guard);
            var review=CompanySalesReceivableReviewSource.Create(original,invoice,rows);review.Require(authority.Company,authority.Actor,receivable);
            if(review.BindingRevision!=authority.Binding)throw new InvalidOperationException("The original receivable review binding is unconfirmed.");await Guard();return review;
        }
        if(close)return await Stable(Capture,Guard);
        var captured=await Capture();await Guard();return CompanyInventorySourceReview.Copy(captured);
    }
    public async Task<CompanySalesReceivableReviewRecovery> ReadReviewRecoveryAsync(ClaimsPrincipal user,string receivable)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();ReviewReceivableId(receivable);var authority=await Authority(user);string? field=null;
        async Task Guard(){await authority.Guard();if(field is not null && !(await assets.CheckAssetAccessAsync(authority.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original receivable FIELD READ authority is required.");}
        async Task Field(string value){if(field is not null && field!=value)throw new InvalidOperationException("The original receivable FIELD changed.");field=value;await Guard();}
        async Task<CompanySalesReceivableReviewRecovery> Capture()
        {
            await Guard();var retained=await receivables.GetCompanySalesReceivableReviewIntentAsync(authority.Company,authority.Actor,receivable,Guard,Field);
            CompanySalesReceivableOutcome? outcome=null;
            if(retained is not null)
            {
                var native=await receivables.GetCompanySalesReceivableForReviewAsync(authority.Company,receivable,Guard,Field);
                if(native.Receivable.REVIEW_WORKFLOW_JSON is not null)outcome=native;
            }
            var result=new CompanySalesReceivableReviewRecovery(authority.Company,authority.Actor,receivable,retained,outcome);result.Require(authority.Company,authority.Actor,receivable);await Guard();return result;
        }
        return await Stable(Capture,Guard);
    }
    public async Task<CompanySalesReceivableOutcome> RecordDecisionAsync(ClaimsPrincipal user,CompanySalesReceivableDecisionCommand command)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();ArgumentNullException.ThrowIfNull(command);command=CompanyInventorySourceReview.Copy(command);command.Require();var authority=await Authority(user);
        async Task Guard()
        {
            await authority.Guard();var current=await context.CaptureAsync(user);
            if(command.Company!=authority.Company || command.ActorUserId!=authority.Actor || command.Review.BindingRevision!=authority.Binding ||
                !current.Access.Permissions.Contains(PermissionConstants.Accounting.ReviewSalesReceivable,StringComparer.Ordinal) || !current.Access.Permissions.Contains(PermissionConstants.Accounting.RecordSalesReceivableDecision,StringComparer.Ordinal) ||
                !(await assets.CheckAssetAccessAsync(authority.Actor,command.Review.FieldId,"FIELD","READ")).HasAccess || !(await assets.CheckAssetAccessAsync(authority.Actor,command.Review.FieldId,"FIELD","WRITE")).HasAccess)
                throw new UnauthorizedAccessException("Current original company/account, independent receivable review/disposition and FIELD READ/WRITE authority are required.");
        }
        async Task<CompanySalesReceivableReviewSource> Current(){await Guard();var review=await ReadReviewAsync(user,command.ReceivableId);await Guard();return review;}
        await Guard();var result=await receivables.RecordCompanySalesReceivableDecisionAsync(command,Guard,Current);await Guard();
        result.Require(command.Review.Original.Intent.Restore(command.Company,command.Review.Original.Intent.ROW_CREATED_BY));
        if(JsonSerializer.Serialize(CompanySalesReceivableDecisionProof.Require(command.Company,result.Receivable).Intent.Restore(command.Company,command.ActorUserId))!=JsonSerializer.Serialize(command))throw new InvalidOperationException("The native receivable disposition differs from the original reviewer command.");return result;
    }
}
