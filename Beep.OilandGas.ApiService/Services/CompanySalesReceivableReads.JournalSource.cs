using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Data.Security;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanySalesReceivableReads
{
    public Task<CompanySalesJournalSourceReview> ReadJournalSourceAsync(ClaimsPrincipal user,string receivable,string book,string currency,string calendar,DateTime date,CompanySalesJournalMappings selection,JournalEntryService journal,AccountMappingService mappings,PeriodClosingService periods,CancellationToken cancellationToken=default)=>ReadJournalSourceCore(user,receivable,book,currency,calendar,date,selection,journal,mappings,periods,cancellationToken);
    private async Task<CompanySalesJournalSourceReview> ReadJournalSourceCore(ClaimsPrincipal user,string receivable,string book,string currency,string calendar,DateTime date,CompanySalesJournalMappings selection,JournalEntryService journal,AccountMappingService mappings,PeriodClosingService periods,CancellationToken cancellationToken=default,string? evidenceActor=null,CompanySalesJournalSourceReview? originalSource=null)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();ReviewReceivableId(receivable);var original=await Authority(user);_=CompanyLedgerPeriodReview.Create(original.Company,original.Actor,book,currency,calendar,[]);CompanyGLMappingReview.RequireDate(date);string? field=null;
        async Task Guard()
        {
            cancellationToken.ThrowIfCancellationRequested();
            await original.Guard();var current=await context.CaptureAsync(user);if(!current.Access.Permissions.Contains(PermissionConstants.Accounting.ReviewSalesJournalSource,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current company sales journal source review authority is required.");
            if(field is not null && !(await assets.CheckAssetAccessAsync(original.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original sales journal FIELD READ authority is required.");
        }
        async Task Field(string value){if(field is not null && field!=value)throw new InvalidOperationException("The original sales journal FIELD changed.");field=value;await Guard();}
        await Guard();if(receivables is not ARService ar)throw new InvalidOperationException("The existing native company receivable owner is unavailable.");
        // The journal owner performs the complete initial/closing cross-owner capture. Each callback supplies
        // one native AR/invoice/physical/policy capture; native receipt readers and authority still close themselves.
        async Task<CompanySalesReceivableReviewSource> Current(){await Guard();var source=await ReadReviewCore(user,receivable,true,close:false,cancellationToken:cancellationToken);source.Require(original.Company,original.Actor,receivable);await Field(source.FieldId);if(source.BindingRevision!=original.Binding)throw new InvalidOperationException("The original sales journal source binding is unconfirmed.");await Guard();return source;}
        var result=await journal.GetCompanySalesJournalSourceAsync(original.Company,original.Actor,receivable,book,currency,calendar,date,selection,ar,mappings,periods,Current,Guard,Field,evidenceActor,originalSource);await Guard();result.Require(original.Company,evidenceActor??original.Actor,receivable,book,currency,calendar,date,selection);if(result.BindingRevision!=original.Binding)throw new InvalidOperationException("The original journal source binding changed.");return result;
    }
}
