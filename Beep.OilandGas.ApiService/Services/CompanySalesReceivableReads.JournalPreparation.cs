using System.Security.Claims;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanySalesReceivableReads
{
    public async Task<CompanySalesJournalPreparationOutcome> PrepareJournalAsync(ClaimsPrincipal user,CompanySalesJournalPreparation command,JournalEntryService journal,AccountMappingService mappings,PeriodClosingService periods,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(command);command=CompanyInventorySourceReview.Copy(command);command.Require();var original=await Authority(user);
        async Task Guard()
        {
            cancellationToken.ThrowIfCancellationRequested();await original.Guard();var current=await context.CaptureAsync(user);
            if(command.Source.Company!=original.Company || command.Source.ActorUserId!=original.Actor || command.Source.BindingRevision!=original.Binding ||
                !current.Access.Permissions.Contains(PermissionConstants.Accounting.PrepareSalesJournal,StringComparer.Ordinal) || !current.Access.Permissions.Contains(PermissionConstants.Accounting.ReviewSalesJournalSource,StringComparer.Ordinal) ||
                !(await assets.CheckAssetAccessAsync(original.Actor,command.Source.FieldId,"FIELD","READ")).HasAccess || !(await assets.CheckAssetAccessAsync(original.Actor,command.Source.FieldId,"FIELD","WRITE")).HasAccess)
                throw new UnauthorizedAccessException("Current original company/account/source binding, journal prepare and FIELD READ/WRITE authority are required.");
        }
        async Task<CompanySalesJournalSourceReview> Current(){await Guard();var value=command.Source;var source=await ReadJournalSourceCore(user,value.ReceivableId,value.BookId,value.CurrencyCode,value.CalendarId,value.PostingDate,value.Selection,journal,mappings,periods,cancellationToken,originalSource:value);await Guard();return source;}
        await Guard();var outcome=await journal.PrepareCompanySalesJournalAsync(command,Guard,Current);await Guard();outcome.Require(command);return outcome;
    }
    public async Task<CompanySalesJournalPreparationOutcome?> RecoverJournalPreparationAsync(ClaimsPrincipal user,string id,string book,string currency,JournalEntryService journal,CancellationToken cancellationToken=default)
    {
        var original=await Authority(user);string? field=null;
        async Task Guard()
        {
            cancellationToken.ThrowIfCancellationRequested();await original.Guard();var current=await context.CaptureAsync(user);
            if(!current.Access.Permissions.Contains(PermissionConstants.Accounting.PrepareSalesJournal,StringComparer.Ordinal) || !current.Access.Permissions.Contains(PermissionConstants.Accounting.ReviewSalesJournalSource,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original journal preparation recovery authority is required.");
            if(field is not null && !(await assets.CheckAssetAccessAsync(original.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original journal FIELD READ is required.");
        }
        async Task Field(string value){if(field is not null && field!=value)throw new InvalidOperationException("The original journal recovery FIELD changed.");field=value;await Guard();}
        await Guard();var result=await journal.GetCompanySalesJournalPreparationAsync(original.Company,original.Actor,book,currency,id,Guard,Field);await Guard();if(result is null)return null;
        var command=CompanySalesJournalPreparationProof.Require(result.Header).Command;result.Require(command);if(command.Source.BindingRevision!=original.Binding)throw new InvalidOperationException("Return to the original journal binding to reconcile the preparation request.");return result;
    }
}
