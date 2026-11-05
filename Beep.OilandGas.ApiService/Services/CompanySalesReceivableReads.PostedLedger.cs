using System.Security.Claims;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanySalesReceivableReads
{
    public async Task<CompanySalesPostedLedgerReview> ReadPostedLedgerAsync(ClaimsPrincipal user,string receivable,string book,string currency,JournalEntryService journal,CancellationToken cancellationToken=default)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();ReviewReceivableId(receivable);var original=await Authority(user);string? field=null;
        async Task Guard(){cancellationToken.ThrowIfCancellationRequested();await original.Guard();var current=await context.CaptureAsync(user);if(!current.Access.Permissions.Contains(PermissionConstants.Accounting.ReviewPostedSalesLedger,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current confirmed sales ledger read authority is required.");if(field is not null && !(await assets.CheckAssetAccessAsync(original.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original posted ledger FIELD READ is required.");}
        async Task Field(string value){if(field is not null && field!=value)throw new InvalidOperationException("The original ledger FIELD changed.");field=value;await Guard();}
        await Guard();if(receivables is not ARService ar)throw new InvalidOperationException("The actual native AR ledger owner is unavailable.");var result=await journal.GetCompanySalesPostedLedgerAsync(original.Company,original.Actor,original.Binding,book,currency,receivable,ar,Guard,Field);result.Require(original.Company,original.Actor,book,currency,receivable);await Guard();if(result.BindingRevision!=original.Binding)throw new InvalidOperationException("The original public posted ledger binding changed.");return result;
    }
}
