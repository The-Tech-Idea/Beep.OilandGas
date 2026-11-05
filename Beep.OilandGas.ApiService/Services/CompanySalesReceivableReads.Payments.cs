using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Security.Claims;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Data.Security;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanySalesReceivableReads
{
    public async Task<CompanySalesPaymentWorkflowSource> CapturePaymentWorkflowSourceAsync(ClaimsPrincipal user,string id,JournalEntryService journal,CancellationToken token=default)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var authority=await PaymentAuthority(user,false,token);if(receivables is not ARService ar)throw new InvalidOperationException("The native payment workflow owner is unavailable.");async Task Field(string field){await authority.Guard();if(!(await assets.CheckAssetAccessAsync(authority.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original payment FIELD READ is required.");}var value=await ar.CaptureCompanySalesPaymentWorkflowSourceAsync(authority.Company,authority.Actor,authority.Binding,id,journal,authority.Guard,Field);value.Require(authority.Company,authority.Actor,id);await authority.Guard();return value;
    }
    private async Task<(CompanyScope Company,string Actor,string Binding,Func<Task> Guard)> PaymentAuthority(ClaimsPrincipal user,bool write,CancellationToken cancellationToken)
    {
        var authority=await Authority(user);async Task Guard(){cancellationToken.ThrowIfCancellationRequested();await authority.Guard();var current=await context.CaptureAsync(user);if(!current.Access.Permissions.Contains(PermissionConstants.Accounting.ReviewSalesPaymentSource,StringComparer.Ordinal) || write && !current.Access.Permissions.Contains(PermissionConstants.Accounting.PrepareSalesPayment,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current payment-source authority and separate native proposal permission are required.");}await Guard();return(authority.Company,authority.Actor,authority.Binding,Guard);
    }
    public async Task<CompanySalesPaymentSource> ReadPaymentSourceAsync(ClaimsPrincipal user,string receivable,string book,string currency,JournalEntryService journal,CancellationToken cancellationToken=default)
    {
        var authority=await PaymentAuthority(user,false,cancellationToken);if(receivables is not ARService ar)throw new InvalidOperationException("The native AR payment owner is unavailable.");async Task Field(string id){await authority.Guard();if(!(await assets.CheckAssetAccessAsync(authority.Actor,id,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original payment FIELD READ is required.");}var result=await ar.GetCompanySalesPaymentSourceAsync(authority.Company,authority.Actor,authority.Binding,book,currency,receivable,journal,authority.Guard,Field);result.Require(authority.Company,authority.Actor,receivable,book,currency);await authority.Guard();return result;
    }
    public async Task<AR_PAYMENT> PreparePaymentAsync(ClaimsPrincipal user,CompanySalesPaymentCommand command,JournalEntryService journal,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(command);command=CompanyInventorySourceReview.Copy(command);command.Require();var authority=await PaymentAuthority(user,true,cancellationToken);var source=command.Source.Ledger;if(receivables is not ARService ar)throw new InvalidOperationException("The native AR payment owner is unavailable.");
        async Task Guard(){await authority.Guard();if(source.Company!=authority.Company || source.ActorUserId!=authority.Actor || source.BindingRevision!=authority.Binding)throw new UnauthorizedAccessException("The exact original payment company/producer/binding is required.");}
        async Task Field(string id){await Guard();if(id!=source.FieldId || !(await assets.CheckAssetAccessAsync(authority.Actor,id,"FIELD","READ")).HasAccess || !(await assets.CheckAssetAccessAsync(authority.Actor,id,"FIELD","WRITE")).HasAccess)throw new UnauthorizedAccessException("Current original payment FIELD READ/WRITE is required.");}
        await Guard();var row=await ar.PrepareCompanySalesPaymentAsync(command,journal,Guard,Field);if(System.Text.Json.JsonSerializer.Serialize(CompanySalesPaymentProof.Require(row).Command)!=System.Text.Json.JsonSerializer.Serialize(command))throw new InvalidOperationException("The original native payment proposal is unconfirmed.");await Guard();return row;
    }
    public async Task<AR_PAYMENT?> RecoverPaymentAsync(ClaimsPrincipal user,string id,CancellationToken cancellationToken=default)
    {
        var authority=await PaymentAuthority(user,false,cancellationToken);if(receivables is not ARService ar)throw new InvalidOperationException("The native payment owner is unavailable.");async Task Field(string field){await authority.Guard();if(!(await assets.CheckAssetAccessAsync(authority.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original payment FIELD READ is required.");}var row=await ar.GetCompanySalesPaymentDraftAsync(authority.Company,authority.Actor,id,authority.Guard,Field);await authority.Guard();if(row is not null && CompanySalesPaymentProof.Require(row).Command.Source.Ledger.BindingRevision!=authority.Binding)throw new InvalidOperationException("Return to the original payment source binding.");return row;
    }
}
