using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.Web.Services;
public sealed partial class CompanySalesReceivableWorkspaceState
{
    private CompanySalesJournalReviewSource? journalReview;
    private CompanySalesJournalReviewRecovery? journalReviewRecovery;
    private CompanySalesJournalReviewCommand? pendingJournalReview;
    private bool JournalReviewGrant=>Visible && company.Selection!.Permissions.Contains("Accounting.ReviewSalesJournal",StringComparer.Ordinal) && company.Selection.Permissions.Contains("Accounting.ReviewSalesJournalSource",StringComparer.Ordinal);
    private bool JournalDecisionGrant=>JournalReviewGrant && company.Selection!.Permissions.Contains("Accounting.RecordSalesJournalDecision",StringComparer.Ordinal);
    private bool PendingJournalReviewMatches=>JournalReviewGrant && pendingJournalReview is {} command && command.Review.Current.Company==company.Selection!.Scope && command.Review.ActorUserId==company.Selection.UserId;
    public bool HasPendingJournalReview=>pendingJournalReview is not null;
    public CompanySalesJournalReviewSource? JournalReview=>JournalReviewGrant && journalReview is not null?Copy(journalReview):null;
    public CompanySalesJournalReviewRecovery? JournalReviewRecovery=>JournalReviewGrant && journalReviewRecovery is not null?Copy(journalReviewRecovery):null;
    public CompanySalesJournalReviewCommand? PendingJournalReview=>PendingJournalReviewMatches?Copy(pendingJournalReview!):null;
    public bool CanRecordJournalReview=>JournalDecisionGrant && !Busy && journalReview is not null && pendingJournalReview is null && pendingPaymentReview is null && pendingPayment is null && pendingJournalPosting is null && pendingJournal is null && pendingReview is null && pending is null;
    public bool CanRetryJournalReview=>JournalDecisionGrant && !Busy && PendingJournalReviewMatches;
    public void InvalidateJournalReview(){++version;journalReview=null;journalReviewRecovery=null;Changed?.Invoke();}
    private void ReadyJournalReview(string id,bool write=false)
    {
        Ready();if(!JournalReviewGrant || write && !JournalDecisionGrant)throw new UnauthorizedAccessException("Current journal review and disposition authority are required.");
        if(pendingPaymentReview is not null || pendingPayment is not null || pending is not null || pendingReview is not null || pendingJournal is not null || pendingJournalPosting is not null)throw new InvalidOperationException("Reconcile the other original receivable or journal request first.");
        if(pendingJournalReview is not null && (!PendingJournalReviewMatches || pendingJournalReview.Review.Original.Header.JOURNAL_ENTRY_ID!=id))throw new InvalidOperationException("Return to the original reviewer company/account/journal to reconcile.");
    }
    private void AcceptJournalReview(CompanySalesJournalReviewRecovery recovery)
    {
        recovery.Require();if(recovery.Company!=company.Selection!.Scope || recovery.ActorUserId!=company.Selection.UserId)throw new InvalidOperationException("Original reviewer recovery belongs to another company/account.");
        var command=recovery.Intent?.Restore();
        if(pendingJournalReview is not null && (command is null || JsonSerializer.Serialize(command)!=JsonSerializer.Serialize(pendingJournalReview)))throw new InvalidOperationException("Native recovery does not confirm the exact original journal review request.");
        journalReviewRecovery=Copy(recovery);if(command is not null)pendingJournalReview=recovery.Outcome is null?Copy(command):null;
    }
    public async Task LoadJournalReviewAsync(string book,string currency,string id,string receivable,IAccountingServiceClient client)
    {
        ReadyJournalReview(id);if(pendingJournalReview is not null || pendingJournalPosting is not null)throw new InvalidOperationException("Recover the retained original review before loading a new source.");
        var snapshot=company.Snapshot;var generation=++version;InvalidateReview();Busy=true;Changed?.Invoke();
        try{var loaded=Copy(await client.GetCompanySalesJournalReviewAsync(book,currency,id));loaded.Require();if(loaded.ActorUserId!=snapshot.Selection!.UserId || loaded.Current.Company!=snapshot.Selection.Scope || loaded.Current.BookId!=book || loaded.Current.CurrencyCode!=currency || loaded.Original.Header.JOURNAL_ENTRY_ID!=id || loaded.Current.ReceivableId!=receivable)throw new InvalidOperationException("The journal belongs to another original receivable context.");
            if(!disposed && generation==version && snapshot==company.Snapshot && JournalReviewGrant)journalReview=loaded;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task RecoverJournalReviewAsync(string book,string currency,string id,string receivable,IAccountingServiceClient client)
    {
        ReadyJournalReview(id);var snapshot=company.Snapshot;var generation=++version;InvalidateReview();Busy=true;Changed?.Invoke();
        try{var recovered=Copy(await client.GetCompanySalesJournalReviewRecoveryAsync(book,currency,id));recovered.Require();
            if(recovered.Company!=snapshot.Selection!.Scope || recovered.ActorUserId!=snapshot.Selection.UserId || recovered.JournalId!=id || recovered.Intent is {} intent && (intent.BOOK_ID!=book || intent.CURRENCY_CODE!=currency || intent.Restore().Review.Current.ReceivableId!=receivable))throw new InvalidOperationException("The original journal review recovery belongs to another context.");
            if(!disposed && generation==version && snapshot==company.Snapshot && JournalReviewGrant)AcceptJournalReview(recovered);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task RecordJournalReviewAsync(string decision,string financial,string financialEvidence,string source,string sourceEvidence,string reason,IAccountingServiceClient client)
    {
        if(!CanRecordJournalReview || journalReview is null)throw new InvalidOperationException("Review the complete original journal and reconcile retained requests first.");
        var command=new CompanySalesJournalReviewCommand(Guid.NewGuid().ToString("N"),Copy(journalReview),decision,financial,financialEvidence,source,sourceEvidence,reason);command.Require();pendingJournalReview=Copy(command);await SubmitJournalReviewAsync(client);
    }
    public async Task SubmitJournalReviewAsync(IAccountingServiceClient client)
    {
        var command=PendingJournalReview??throw new InvalidOperationException("Return to the original reviewer company/account.");ReadyJournalReview(command.Review.Original.Header.JOURNAL_ENTRY_ID!,true);
        var snapshot=company.Snapshot;var generation=++version;InvalidateReview();Busy=true;Changed?.Invoke();bool Current()=>!disposed && generation==version && snapshot==company.Snapshot && JournalDecisionGrant;
        try{var result=await client.RecordCompanySalesJournalDecisionAsync(Copy(command));AccountingServiceClient.RequireJournalDecision(result,command);if(!Current())return;
            var recovered=await client.GetCompanySalesJournalReviewRecoveryAsync(command.Review.Current.BookId,command.Review.Current.CurrencyCode,command.Review.Original.Header.JOURNAL_ENTRY_ID!);recovered.Require();
            if(recovered.Outcome is null || JsonSerializer.Serialize(recovered.Outcome)!=JsonSerializer.Serialize(result))throw new InvalidOperationException("Fresh native recovery does not confirm the exact original journal review acknowledgement.");
            if(Current())AcceptJournalReview(recovered);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
}
