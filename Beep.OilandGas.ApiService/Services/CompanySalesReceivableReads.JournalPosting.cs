using System.Security.Claims;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanySalesReceivableReads
{
    private async Task<(CompanyScope Company,string Actor,string Binding,Func<Task> Guard)> JournalPostingAuthority(ClaimsPrincipal user,bool write,CancellationToken cancellationToken)
    {
        var original=await Authority(user);async Task Guard(){cancellationToken.ThrowIfCancellationRequested();await original.Guard();var current=await context.CaptureAsync(user);
            if(!current.Access.Permissions.Contains(PermissionConstants.Accounting.ReviewSalesJournalPosting,StringComparer.Ordinal) || !current.Access.Permissions.Contains(PermissionConstants.Accounting.ReviewSalesJournalSource,StringComparer.Ordinal) || write && !current.Access.Permissions.Contains(PermissionConstants.Accounting.PostSalesJournal,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current independent posting-source review and separate journal posting authority are required.");}
        await Guard();return(original.Company,original.Actor,original.Binding,Guard);
    }
    public Task<CompanySalesJournalPostingSource> ReadJournalPostingSourceAsync(ClaimsPrincipal user,string id,string book,string currency,JournalEntryService journal,AccountMappingService mappings,PeriodClosingService periods,CancellationToken cancellationToken=default)=>ReadJournalPostingSourceCore(user,id,book,currency,journal,mappings,periods,cancellationToken,false,true);
    private async Task<CompanySalesJournalPostingSource> ReadJournalPostingSourceCore(ClaimsPrincipal user,string id,string book,string currency,JournalEntryService journal,AccountMappingService mappings,PeriodClosingService periods,CancellationToken cancellationToken,bool afterPosting,bool close)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var original=await JournalPostingAuthority(user,false,cancellationToken);string? field=null;CompanySalesJournalPreparation? preparation=null;
        async Task Field(string value){if(field is not null && field!=value)throw new InvalidOperationException("The original posting FIELD changed.");field=value;await original.Guard();if(!(await assets.CheckAssetAccessAsync(original.Actor,value,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original posting FIELD READ is required.");}
        if(receivables is not ARService ar)throw new InvalidOperationException("The actual native AR posting owner is unavailable.");
        async Task<CompanySalesJournalSourceReview> Current()
        {
            await original.Guard();var native=await journal.GetCompanySalesJournalForReviewAsync(original.Company,original.Actor,book,currency,id,original.Guard,Field)??throw new InvalidOperationException("The original posting journal is unavailable.");var command=CompanySalesJournalPreparationProof.Require(native.Header).Command;
            if(preparation is not null && System.Text.Json.JsonSerializer.Serialize(preparation)!=System.Text.Json.JsonSerializer.Serialize(command))throw new InvalidOperationException("The exact original journal preparation changed during posting-source read.");preparation=command;var source=command.Source;
            if(source.BindingRevision!=original.Binding)throw new InvalidOperationException("Return to the original journal posting source binding.");await Field(source.FieldId);
            var current=await ReadJournalSourceCore(user,source.ReceivableId,book,currency,source.CalendarId,source.PostingDate,source.Selection,journal,mappings,periods,cancellationToken,source.ActorUserId,source);await original.Guard();return current;
        }
        var result=await journal.GetCompanySalesJournalPostingSourceAsync(original.Company,original.Actor,book,currency,id,ar,Current,original.Guard,Field,afterPosting,close);result.Require();await original.Guard();await Field(result.Current.FieldId);
        if(result.ActorUserId!=original.Actor || result.Current.Company!=original.Company || result.Current.BindingRevision!=original.Binding)throw new InvalidOperationException("The original actual poster/company/binding is unconfirmed.");return result;
    }
    public async Task<CompanySalesJournalPostingOutcome> PostJournalAsync(ClaimsPrincipal user,CompanySalesJournalPostingCommand command,JournalEntryService journal,AccountMappingService mappings,PeriodClosingService periods,CancellationToken cancellationToken=default)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();ArgumentNullException.ThrowIfNull(command);command=CompanyInventorySourceReview.Copy(command);command.Require();var original=await JournalPostingAuthority(user,true,cancellationToken);var source=command.Source.Current;if(receivables is not ARService ar)throw new InvalidOperationException("The native AR posting owner is unavailable.");
        async Task Guard(){await original.Guard();if(command.Source.ActorUserId!=original.Actor || source.Company!=original.Company || source.BindingRevision!=original.Binding)throw new UnauthorizedAccessException("The original actual posting producer/company/binding is required.");}
        async Task Field(string value,string access){await Guard();if(value!=source.FieldId || !(await assets.CheckAssetAccessAsync(original.Actor,value,"FIELD",access)).HasAccess)throw new UnauthorizedAccessException("Current original posting FIELD "+access+" is required.");}
        // The immutable command supplies selectors; the native posting reader still confirms actual journal/AR
        // artifacts and the mutation owner compares the whole live capture before and after each transaction.
        async Task<CompanySalesJournalSourceReview> Financial(){await Guard();var current=await ReadJournalSourceCore(user,source.ReceivableId,source.BookId,source.CurrencyCode,source.CalendarId,source.PostingDate,source.Selection,journal,mappings,periods,cancellationToken,source.ActorUserId,source);await Guard();return current;}
        async Task<CompanySalesJournalPostingSource> Current(){await Guard();var current=await journal.GetCompanySalesJournalPostingSourceAsync(original.Company,original.Actor,source.BookId,source.CurrencyCode,command.Source.Reviewed.Header.JOURNAL_ENTRY_ID,ar,Financial,Guard,value=>Field(value,"READ"),true,false);await Guard();return current;}
        await Guard();await Field(source.FieldId,"READ");await Field(source.FieldId,"WRITE");
        var result=await journal.PostCompanySalesJournalAsync(command,ar,Current,Guard,value=>Field(value,"READ"),value=>Field(value,"WRITE"));result.Require(command);await Field(source.FieldId,"READ");await Field(source.FieldId,"WRITE");await Guard();return result;
    }
    public async Task<CompanySalesJournalPostingRecovery> RecoverJournalPostingAsync(ClaimsPrincipal user,string id,string book,string currency,JournalEntryService journal,CancellationToken cancellationToken=default)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var original=await JournalPostingAuthority(user,false,cancellationToken);
        async Task Field(string value){await original.Guard();if(!(await assets.CheckAssetAccessAsync(original.Actor,value,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original posting FIELD READ is required.");}
        var result=await journal.GetCompanySalesJournalPostingRecoveryAsync(original.Company,original.Actor,book,currency,id,original.Guard,Field);result.Require();if(result.Intent is not null && result.Intent.Restore().Source.Current.BindingRevision!=original.Binding)throw new InvalidOperationException("Return to the original posting binding to reconcile.");await original.Guard();return result;
    }
}
