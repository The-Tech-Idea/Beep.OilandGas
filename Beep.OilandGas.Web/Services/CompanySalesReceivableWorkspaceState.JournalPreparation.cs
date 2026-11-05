using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class CompanySalesReceivableWorkspaceState
{
    private CompanySalesJournalPreparation? pendingJournal;
    private CompanySalesJournalPreparationOutcome? journalPreparation;
    public bool HasPendingJournal=>pendingJournal is not null;
    private bool JournalGrant=>Visible && company.Selection!.Permissions.Contains("Accounting.PrepareSalesJournal",StringComparer.Ordinal) && company.Selection.Permissions.Contains("Accounting.ReviewSalesJournalSource",StringComparer.Ordinal);
    private bool PendingJournalMatches=>JournalGrant && pendingJournal is {} value && value.Source.Company==company.Selection!.Scope && value.Source.ActorUserId==company.Selection.UserId;
    public CompanySalesJournalPreparation? PendingJournal=>PendingJournalMatches?Copy(pendingJournal!):null;
    public CompanySalesJournalPreparationOutcome? JournalPreparation=>JournalGrant && journalPreparation is not null?Copy(journalPreparation):null;
    public bool CanPrepareJournal=>JournalGrant && !Busy && pendingJournal is null && pendingJournalReview is null && pendingPaymentReview is null && pendingPayment is null && pendingJournalPosting is null && pending is null && pendingReview is null && journalSource is not null;
    public bool CanRetryJournal=>PendingJournalMatches && !Busy;
    public async Task PrepareJournalAsync(string number,string reason,IAccountingServiceClient client)
    {
        Ready();if(!CanPrepareJournal || journalSource is not {} displayed)throw new InvalidOperationException("Review the complete original journal source and reconcile every retained request first.");
        var command=new CompanySalesJournalPreparation(Guid.NewGuid().ToString("N"),Guid.NewGuid().ToString("N"),Copy(displayed),number,reason,displayed.Lines.Select(_=>Guid.NewGuid().ToString("N")).ToArray());command.Require();pendingJournal=Copy(command);await SubmitJournalPreparationAsync(client);
    }
    public async Task SubmitJournalPreparationAsync(IAccountingServiceClient client)
    {
        if(pendingPaymentReview is not null || pendingPayment is not null || pending is not null || pendingReview is not null || pendingJournalReview is not null || pendingJournalPosting is not null)throw new InvalidOperationException("Reconcile the original retained receivable or reviewer request first.");
        Ready();if(!PendingJournalMatches)throw new InvalidOperationException("Return to the original journal company/account and prepare authority to reconcile.");
        var command=Copy(pendingJournal!);var snapshot=company.Snapshot;var generation=++version;InvalidateReview();Busy=true;Changed?.Invoke();
        bool Current()=>!disposed && generation==version && snapshot==company.Snapshot && JournalGrant;
        try
        {
            var outcome=await client.PrepareCompanySalesJournalAsync(Copy(command));outcome.Require(command);if(!Current())return;
            var recovered=await client.GetCompanySalesJournalPreparationAsync(command.Source.BookId,command.Source.CurrencyCode,command.JournalId) ??throw new InvalidOperationException("The complete committed journal preparation is unconfirmed. Retain its original request.");recovered.Require(command);
            if(System.Text.Json.JsonSerializer.Serialize(recovered)!=System.Text.Json.JsonSerializer.Serialize(outcome) || recovered.Header.STATUS is not ("DRAFT" or "APPROVED" or "REJECTED" or "POSTED"))throw new InvalidOperationException("The complete recovered native journal does not confirm the original preparation acknowledgement.");
            if(!Current())return;journalPreparation=Copy(recovered);pendingJournal=null;
        }
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task RecoverJournalPreparationAsync(string book,string currency,string id,string receivable,IAccountingServiceClient client)
    {
        Ready();if(!JournalGrant || pendingJournal is not null || pending is not null || pendingReview is not null || pendingJournalReview is not null || pendingJournalPosting is not null)throw new InvalidOperationException("Reconcile the retained original request before recovering another journal.");
        var snapshot=company.Snapshot;var generation=++version;InvalidateReview();Busy=true;Changed?.Invoke();
        try
        {
            var recovered=await client.GetCompanySalesJournalPreparationAsync(book,currency,id) ??throw new InvalidOperationException("The original retained journal request is unavailable; no new request is authorized.");var command=CompanySalesJournalPreparationProof.Require(recovered.Header).Command;recovered.Require(command);
            if(command.Source.Company!=snapshot.Selection!.Scope || command.Source.ActorUserId!=snapshot.Selection.UserId || command.Source.BookId!=book || command.Source.CurrencyCode!=currency || command.JournalId!=id || command.Source.ReceivableId!=receivable)throw new InvalidOperationException("The original journal recovery belongs to another company/account/receivable/book/currency.");
            if(disposed || generation!=version || snapshot!=company.Snapshot || !JournalGrant)return;pendingJournal=Copy(command);journalPreparation=Copy(recovered);
        }
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
}
