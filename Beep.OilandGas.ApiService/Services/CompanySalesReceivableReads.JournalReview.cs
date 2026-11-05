using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Data.Security;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanySalesReceivableReads
{
    public Task<CompanySalesJournalReviewSource> ReadJournalReviewAsync(ClaimsPrincipal user,string id,string book,string currency,JournalEntryService journal,AccountMappingService mappings,PeriodClosingService periods,CancellationToken cancellationToken=default)=>ReadJournalReviewCore(user,id,book,currency,journal,mappings,periods,cancellationToken,false);
    private async Task<CompanySalesJournalReviewSource> ReadJournalReviewCore(ClaimsPrincipal user,string id,string book,string currency,JournalEntryService journal,AccountMappingService mappings,PeriodClosingService periods,CancellationToken cancellationToken,bool afterDisposition,bool close=true)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var authority=await JournalReviewAuthority(user,false,cancellationToken);string? field=null;
        async Task Field(string value){if(field is not null && field!=value)throw new InvalidOperationException("The original journal review FIELD changed.");field=value;await authority.Guard();if(!(await assets.CheckAssetAccessAsync(authority.Actor,value,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original journal FIELD READ is required.");}
        async Task<CompanySalesJournalReviewSource> Capture()
        {
            await authority.Guard();var actual=await journal.GetCompanySalesJournalForReviewAsync(authority.Company,authority.Actor,book,currency,id,authority.Guard,Field) ??throw new InvalidOperationException("The original company journal is unavailable.");await authority.Guard();
            if(actual.Header.STATUS!="DRAFT" && !afterDisposition)throw new InvalidOperationException("The original journal already has a recorded independent disposition. Recover its original reviewer request.");
            var header=actual.Header.REVIEW_WORKFLOW_JSON is null?actual.Header:CompanySalesJournalReviewProof.OriginalDraftHeader(actual.Header);var original=new CompanySalesJournalPreparationOutcome(header,actual.Lines);var preparation=CompanySalesJournalPreparationProof.Require(header).Command;original.Require(preparation);
            if(preparation.Source.ActorUserId==authority.Actor)throw new InvalidOperationException("The journal maker cannot independently review their own draft.");
            var source=preparation.Source;if(source.BindingRevision!=authority.Binding)throw new InvalidOperationException("Return to the original journal source binding to review.");
            var current=await ReadJournalSourceCore(user,source.ReceivableId,book,currency,source.CalendarId,source.PostingDate,source.Selection,journal,mappings,periods,cancellationToken,source.ActorUserId,source);await authority.Guard();await Field(source.FieldId);return CompanySalesJournalReviewSource.Create(authority.Actor,original,current);
        }
        if(close)return await Stable(Capture,authority.Guard);var captured=await Capture();await authority.Guard();return captured;
    }
    private async Task<(CompanyScope Company,string Actor,string Binding,Func<Task> Guard)> JournalReviewAuthority(ClaimsPrincipal user,bool write,CancellationToken cancellationToken)
    {
        var original=await Authority(user);async Task Guard(){cancellationToken.ThrowIfCancellationRequested();await original.Guard();var current=await context.CaptureAsync(user);
            if(!current.Access.Permissions.Contains(PermissionConstants.Accounting.ReviewSalesJournal,StringComparer.Ordinal) || !current.Access.Permissions.Contains(PermissionConstants.Accounting.ReviewSalesJournalSource,StringComparer.Ordinal) || write && !current.Access.Permissions.Contains(PermissionConstants.Accounting.RecordSalesJournalDecision,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original company journal review/disposition authority is required.");}
        await Guard();return(original.Company,original.Actor,original.Binding,Guard);
    }
    public async Task<CompanySalesJournalPreparationOutcome> RecordJournalReviewAsync(ClaimsPrincipal user,CompanySalesJournalReviewCommand command,JournalEntryService journal,AccountMappingService mappings,PeriodClosingService periods,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(command);command=CompanyInventorySourceReview.Copy(command);command.Require();var original=await JournalReviewAuthority(user,true,cancellationToken);var source=command.Review.Current;var id=command.Review.Original.Header.JOURNAL_ENTRY_ID;
        async Task Guard(){await original.Guard();if(command.Review.ActorUserId!=original.Actor || source.Company!=original.Company || source.BindingRevision!=original.Binding || !(await assets.CheckAssetAccessAsync(original.Actor,source.FieldId,"FIELD","READ")).HasAccess || !(await assets.CheckAssetAccessAsync(original.Actor,source.FieldId,"FIELD","WRITE")).HasAccess)throw new UnauthorizedAccessException("Current actual reviewer, original company/source binding and FIELD READ/WRITE are required.");}
        // The disposition owner compares this full source before and after each owned transaction.
        async Task<CompanySalesJournalReviewSource> Current(){await Guard();var review=await ReadJournalReviewCore(user,id,source.BookId,source.CurrencyCode,journal,mappings,periods,cancellationToken,true,close:false);await Guard();return review;}
        await Guard();var result=await journal.RecordCompanySalesJournalReviewAsync(command,Guard,Current);await Guard();if(System.Text.Json.JsonSerializer.Serialize(CompanySalesJournalReviewProof.Require(result.Header).Intent.Restore())!=System.Text.Json.JsonSerializer.Serialize(command))throw new InvalidOperationException("The reviewed native outcome differs from the original independent reviewer request.");return result;
    }
    public async Task<CompanySalesJournalReviewRecovery> RecoverJournalReviewAsync(ClaimsPrincipal user,string id,string book,string currency,JournalEntryService journal,CancellationToken cancellationToken=default)
    {
        var original=await JournalReviewAuthority(user,false,cancellationToken);async Task Field(string value){await original.Guard();if(!(await assets.CheckAssetAccessAsync(original.Actor,value,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original journal FIELD READ is required.");}
        var result=await journal.GetCompanySalesJournalReviewRecoveryAsync(original.Company,original.Actor,book,currency,id,original.Guard,Field);await original.Guard();result.Require();if(result.Intent is not null && result.Intent.Restore().Review.Current.BindingRevision!=original.Binding)throw new InvalidOperationException("Return to the original reviewer binding to reconcile.");return result;
    }
}
