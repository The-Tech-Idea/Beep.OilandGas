using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class CompanySalesReceivableWorkspaceState
{
    private CompanySalesJournalPostingSource? journalPostingSource;
    private CompanySalesJournalPostingRecovery? journalPostingRecovery;
    private CompanySalesJournalPostingCommand? pendingJournalPosting;
    private bool PostingReadGrant=>Visible && company.Selection!.Permissions.Contains("Accounting.ReviewSalesJournalPosting",StringComparer.Ordinal) && company.Selection.Permissions.Contains("Accounting.ReviewSalesJournalSource",StringComparer.Ordinal);
    private bool PostingWriteGrant=>PostingReadGrant && company.Selection!.Permissions.Contains("Accounting.PostSalesJournal",StringComparer.Ordinal);
    private bool PendingPostingMatches=>PostingReadGrant && pendingJournalPosting is {} command && command.Source.ActorUserId==company.Selection!.UserId && command.Source.Current.Company==company.Selection.Scope;
    public bool HasPendingJournalPosting=>pendingJournalPosting is not null;
    public CompanySalesJournalPostingCommand? PendingJournalPosting=>PendingPostingMatches?Copy(pendingJournalPosting!):null;
    public CompanySalesJournalPostingSource? JournalPostingSource=>PostingReadGrant && journalPostingSource is not null?Copy(journalPostingSource):null;
    public CompanySalesJournalPostingRecovery? JournalPostingRecovery=>PostingReadGrant && journalPostingRecovery is not null?Copy(journalPostingRecovery):null;
    public bool CanPostJournal=>PostingWriteGrant && !Busy && journalPostingSource is not null && pendingPaymentReview is null && pendingPayment is null && pendingJournalPosting is null && pending is null && pendingReview is null && pendingJournal is null && pendingJournalReview is null;
    public bool CanRetryJournalPosting=>PostingWriteGrant && !Busy && PendingPostingMatches;
    public void InvalidateJournalPosting(){++version;journalPostingSource=null;journalPostingRecovery=null;Changed?.Invoke();}
    private void ReadyJournalPosting(string id,bool write=false)
    {
        Ready();if(!PostingReadGrant || write && !PostingWriteGrant)throw new UnauthorizedAccessException("Current posting-source review and separate posting authority are required.");
        if(pendingPaymentReview is not null || pendingPayment is not null || pending is not null || pendingReview is not null || pendingJournal is not null || pendingJournalReview is not null)throw new InvalidOperationException("Reconcile the other original receivable/journal request first.");
        if(pendingJournalPosting is not null && (!PendingPostingMatches || pendingJournalPosting.Source.Reviewed.Header.JOURNAL_ENTRY_ID!=id))throw new InvalidOperationException("Return to the original posting producer/company/account/journal to reconcile.");
    }
    private void AcceptJournalPosting(CompanySalesJournalPostingRecovery recovery)
    {
        recovery.Require();if(recovery.Company!=company.Selection!.Scope || recovery.ActorUserId!=company.Selection.UserId)throw new InvalidOperationException("The original posting recovery belongs to another company/account.");var command=recovery.Intent?.Restore();
        if(pendingJournalPosting is not null && (command is null || JsonSerializer.Serialize(command)!=JsonSerializer.Serialize(pendingJournalPosting)))throw new InvalidOperationException("Native recovery does not confirm the exact original posting request.");
        journalPostingRecovery=Copy(recovery);if(command is not null)pendingJournalPosting=recovery.Outcome is null?Copy(command):null;
    }
    public async Task LoadJournalPostingSourceAsync(string book,string currency,string id,string receivable,IAccountingServiceClient client)
    {
        ReadyJournalPosting(id);if(pendingJournalPosting is not null)throw new InvalidOperationException("Recover the retained posting request before loading another source.");var snapshot=company.Snapshot;var generation=++version;InvalidateReview();Busy=true;Changed?.Invoke();
        try{var source=Copy(await client.GetCompanySalesJournalPostingSourceAsync(book,currency,id));source.Require();if(source.ActorUserId!=snapshot.Selection!.UserId || source.Current.Company!=snapshot.Selection.Scope || source.Current.BookId!=book || source.Current.CurrencyCode!=currency || source.Reviewed.Header.JOURNAL_ENTRY_ID!=id || source.Current.ReceivableId!=receivable)throw new InvalidOperationException("The exact posting source belongs to another original context.");
            if(!disposed && generation==version && snapshot==company.Snapshot && PostingReadGrant)journalPostingSource=source;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task RecoverJournalPostingAsync(string book,string currency,string id,string receivable,IAccountingServiceClient client)
    {
        ReadyJournalPosting(id);var snapshot=company.Snapshot;var generation=++version;InvalidateReview();Busy=true;Changed?.Invoke();
        try{var recovery=Copy(await client.GetCompanySalesJournalPostingRecoveryAsync(book,currency,id));recovery.Require();if(recovery.Company!=snapshot.Selection!.Scope || recovery.ActorUserId!=snapshot.Selection.UserId || recovery.JournalId!=id || recovery.Intent is {} intent && (intent.BOOK_ID!=book || intent.CURRENCY_CODE!=currency || intent.Restore().Source.Current.ReceivableId!=receivable))throw new InvalidOperationException("The original posting recovery belongs to another receivable/company/account/book/currency/journal.");
            if(!disposed && generation==version && snapshot==company.Snapshot && PostingReadGrant)AcceptJournalPosting(recovery);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task PostJournalAsync(string reason,IAccountingServiceClient client)
    {
        if(!CanPostJournal || journalPostingSource is null)throw new InvalidOperationException("Review the current original independently approved journal and reconcile retained requests first.");var command=new CompanySalesJournalPostingCommand(Guid.NewGuid().ToString("N"),Copy(journalPostingSource),journalPostingSource.Reviewed.Lines.Select(_=>Guid.NewGuid().ToString("N")).ToArray(),reason);command.Require();pendingJournalPosting=Copy(command);await SubmitJournalPostingAsync(client);
    }
    public async Task SubmitJournalPostingAsync(IAccountingServiceClient client)
    {
        var command=PendingJournalPosting??throw new InvalidOperationException("Return to the original posting producer/company/account.");ReadyJournalPosting(command.Source.Reviewed.Header.JOURNAL_ENTRY_ID,true);var snapshot=company.Snapshot;var generation=++version;InvalidateReview();Busy=true;Changed?.Invoke();bool Current()=>!disposed && generation==version && snapshot==company.Snapshot && PostingWriteGrant;
        try{var result=await client.PostCompanySalesJournalAsync(Copy(command));result.Require(command);if(!Current())return;var recovery=await client.GetCompanySalesJournalPostingRecoveryAsync(command.Source.Current.BookId,command.Source.Current.CurrencyCode,command.Source.Reviewed.Header.JOURNAL_ENTRY_ID);recovery.Require();
            if(recovery.Outcome is null || JsonSerializer.Serialize(recovery.Outcome)!=JsonSerializer.Serialize(result))throw new InvalidOperationException("Fresh native recovery does not confirm the exact original posting acknowledgement.");if(Current())AcceptJournalPosting(recovery);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
}
