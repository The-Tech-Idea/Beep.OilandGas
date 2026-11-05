using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Security.Claims;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Security;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanySalesReceivableReads
{
    private async Task<(CompanyScope Company,string Actor,string Binding,Func<Task> Guard)> PaymentReviewAuthority(ClaimsPrincipal user,bool fresh,bool write,CancellationToken token)
    {
        var value=await PaymentAuthority(user,false,token);async Task Guard(){await value.Guard();var current=await context.CaptureAsync(user);if(fresh && !current.Access.Permissions.Contains(PermissionConstants.Accounting.ReviewSalesPayment,StringComparer.Ordinal) || write && !current.Access.Permissions.Contains(PermissionConstants.Accounting.RecordSalesPaymentDecision,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current independent payment review and separate disposition authority are required.");}await Guard();return(value.Company,value.Actor,value.Binding,Guard);
    }
    public async Task<CompanySalesPaymentReviewSource> ReadPaymentReviewAsync(ClaimsPrincipal user,string id,JournalEntryService journal,CancellationToken token=default)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var authority=await PaymentReviewAuthority(user,true,false,token);if(receivables is not ARService ar)throw new InvalidOperationException("The native payment review owner is unavailable.");async Task Field(string field){await authority.Guard();if(!(await assets.CheckAssetAccessAsync(authority.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original payment FIELD READ is required.");}var value=await ar.GetCompanySalesPaymentReviewAsync(authority.Company,authority.Actor,authority.Binding,id,journal,authority.Guard,Field);value.Require();await authority.Guard();return value;
    }
    public async Task<AR_PAYMENT> RecordPaymentReviewAsync(ClaimsPrincipal user,CompanySalesPaymentReviewCommand command,JournalEntryService journal,CancellationToken token=default)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();command=CompanyInventorySourceReview.Copy(command);command.Require();var authority=await PaymentReviewAuthority(user,true,true,token);if(receivables is not ARService ar)throw new InvalidOperationException("The native payment review owner is unavailable.");var source=command.Review.Current.Ledger;async Task Guard(){await authority.Guard();if(source.Company!=authority.Company || source.ActorUserId!=authority.Actor || source.BindingRevision!=authority.Binding)throw new UnauthorizedAccessException("The original independent reviewer/company/binding is required.");}async Task Field(string field){await Guard();if(field!=source.FieldId || !(await assets.CheckAssetAccessAsync(authority.Actor,field,"FIELD","READ")).HasAccess || !(await assets.CheckAssetAccessAsync(authority.Actor,field,"FIELD","WRITE")).HasAccess)throw new UnauthorizedAccessException("Current original payment FIELD READ/WRITE is required.");}var row=await ar.RecordCompanySalesPaymentReviewAsync(command,journal,Guard,Field);if(System.Text.Json.JsonSerializer.Serialize(CompanySalesPaymentReviewProof.Require(row).Intent.Restore())!=System.Text.Json.JsonSerializer.Serialize(command))throw new InvalidOperationException("The exact original native payment review is unconfirmed.");await Guard();return row;
    }
    public async Task<CompanySalesPaymentReviewRecovery> RecoverPaymentReviewAsync(ClaimsPrincipal user,string id,CancellationToken token=default)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var authority=await PaymentReviewAuthority(user,false,false,token);if(receivables is not ARService ar)throw new InvalidOperationException("The native payment review owner is unavailable.");async Task Field(string field){await authority.Guard();if(!(await assets.CheckAssetAccessAsync(authority.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original payment FIELD READ is required.");}var value=await ar.GetCompanySalesPaymentReviewRecoveryAsync(authority.Company,authority.Actor,id,authority.Guard,Field);value.Require();if(value.Intent is not null && value.Intent.Restore().Review.Current.Ledger.BindingRevision!=authority.Binding)throw new InvalidOperationException("Return to the original payment review binding.");await authority.Guard();return value;
    }
}
