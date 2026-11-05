using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Text.Json;
namespace Beep.OilandGas.Web.Services;

public sealed partial class CompanySalesReceivableWorkspaceState
{
    private CompanySalesReceivableReviewSource? reviewSource;
    private CompanySalesReceivableReviewRecovery? reviewRecovery;
    private CompanySalesReceivableDecisionCommand? pendingReview;
    private bool reviewSourceCurrent,reviewSourceEligible;
    private void InvalidateReview(){reviewSource=null;reviewRecovery=null;journalSource=null;journalPreparation=null;journalReview=null;journalReviewRecovery=null;journalPostingSource=null;journalPostingRecovery=null;postedLedger=null;reviewSourceCurrent=reviewSourceEligible=false;}
    private bool ReviewPendingMatches=>Visible && pendingReview is not null && pendingReview.Company==company.Selection!.Scope && pendingReview.ActorUserId==company.Selection.UserId;
    public bool HasPendingReview=>pendingReview is not null;
    public CompanySalesReceivableReviewSource? ReviewSource=>Visible && reviewSource is not null?Copy(reviewSource):null;
    public CompanySalesReceivableReviewRecovery? ReviewRecovery=>Visible && reviewRecovery is not null?Copy(reviewRecovery):null;
    public CompanySalesReceivableDecisionCommand? ReviewPending=>ReviewPendingMatches?Copy(pendingReview!):null;
    private bool ReviewGrant=>Visible && company.Selection!.Permissions.Contains("Accounting.ReviewSalesReceivable",StringComparer.Ordinal);
    private bool DecisionGrant=>ReviewGrant && company.Selection!.Permissions.Contains("Accounting.RecordSalesReceivableDecision",StringComparer.Ordinal);
    public bool ReviewSourceHasCurrentOriginalSource=>Visible && reviewSource is not null && reviewSourceCurrent;
    public bool CanRecordReview=>!Busy && DecisionGrant && pending is null && pendingReview is null && pendingJournal is null && pendingJournalReview is null && pendingPaymentReview is null && pendingPayment is null && pendingJournalPosting is null && reviewSource is not null && reviewSourceEligible;
    public bool CanRetryReview=>!Busy && DecisionGrant && ReviewPendingMatches;
    private void ReadyReview(string receivable,bool source=false,bool write=false)
    {
        Ready();if(!Guid.TryParseExact(receivable,"N",out var id) || id==Guid.Empty)throw new ArgumentException("The original receivable identity is required.");
        if(source && !ReviewGrant || write && !DecisionGrant)throw new UnauthorizedAccessException("Current receivable review and disposition authority are required.");
        if(pendingReview is not null && (!ReviewPendingMatches || pendingReview.ReceivableId!=receivable))throw new InvalidOperationException("Return to the original company, reviewer account and receivable to reconcile the retained decision.");
        if(write && pending is not null)throw new InvalidOperationException("Reconcile the original preparation request before recording a reviewer decision.");
        if(write && (pendingJournal is not null || pendingJournalReview is not null || pendingJournalPosting is not null))throw new InvalidOperationException("Reconcile the original journal preparation request before another reviewer decision.");
    }
    private void AcceptReview(CompanySalesReceivableReviewRecovery loaded)
    {
        if(pendingPaymentReview is not null || pendingPayment is not null || pending is not null || pendingJournal is not null || pendingJournalReview is not null || pendingJournalPosting is not null)throw new InvalidOperationException("Reconcile the original retained receivable or journal request first.");
        loaded.Require(company.Selection!.Scope,company.Selection.UserId,loaded.ReceivableId);
        var original=loaded.Retained?.Restore(loaded.Company,loaded.ActorUserId);
        if(pendingReview is not null && original is not null && JsonSerializer.Serialize(original)!=JsonSerializer.Serialize(pendingReview))throw new InvalidOperationException("Native recovery does not confirm the exact retained reviewer decision.");
        reviewRecovery=Copy(loaded);
        if(original is not null){if(loaded.Outcome is not null)pendingReview=null;else pendingReview=Copy(original);}
    }
    public async Task LoadReviewAsync(string receivable,IAccountingServiceClient client,string? preparationRevision=null,string? assignedReviewRevision=null)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();
        ReadyReview(receivable,true);var snapshot=company.Snapshot;var generation=++version;InvalidateReview();Busy=true;Changed?.Invoke();
        bool Current()=>!disposed && generation==version && snapshot==company.Snapshot && Visible;
        try
        {
            var loaded=Copy(await client.GetCompanySalesReceivableReviewAsync(receivable));loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,receivable);if(!Current())return;
            if(preparationRevision is not null && CompanyAccountingSourceTypes.ReceivablePreparationRevision(loaded.Original)!=preparationRevision || assignedReviewRevision is not null && loaded.Revision!=assignedReviewRevision)throw new InvalidOperationException("The assigned receivable preparation or displayed review changed. Reload the original process before reviewing.");
            var recovered=Copy(await client.GetCompanySalesReceivableReviewRecoveryAsync(receivable));recovered.Require(snapshot.Selection.Scope,snapshot.Selection.UserId,receivable);if(!Current())return;
            if(recovered.Outcome is not null && JsonSerializer.Serialize(recovered.Outcome)!=JsonSerializer.Serialize(loaded.Original))throw new InvalidOperationException("The recorded native disposition changed while loading review evidence.");
            // These facts describe the verified, privately owned loaded snapshot. Authority remains
            // live through Visible/DecisionGrant, and commands still validate complete source evidence.
            var sourceCurrent=loaded.HasCurrentOriginalSource;
            if(!Current())return;
            AcceptReview(recovered);reviewSource=loaded;reviewSourceCurrent=sourceCurrent;
            reviewSourceEligible=sourceCurrent && loaded.IsIndependent && loaded.Original.Receivable.STATUS=="DRAFT";
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverReviewAsync(string receivable,IAccountingServiceClient client)
    {
        ReadyReview(receivable);var snapshot=company.Snapshot;var generation=++version;InvalidateReview();Busy=true;Changed?.Invoke();
        bool Current()=>!disposed && generation==version && snapshot==company.Snapshot && Visible;
        try
        {
            var recovered=Copy(await client.GetCompanySalesReceivableReviewRecoveryAsync(receivable));recovered.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,receivable);if(!Current())return;
            CompanySalesReceivableStatusReview? headers=null;
            if(recovered.Outcome is not null){headers=await ReviewedHeaders(recovered.Outcome,client,snapshot.Selection.Scope,snapshot.Selection.UserId);if(!Current())return;}
            AcceptReview(recovered);if(headers is not null)status=headers;
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    private async Task<CompanySalesReceivableStatusReview> ReviewedHeaders(CompanySalesReceivableOutcome outcome,IAccountingServiceClient client,CompanyScope scope,string actor)
    {
        var loaded=Copy(await client.GetCompanySalesReceivableStatusAsync(outcome.Receivable.SOURCE_INVOICE_ID!));loaded.Require(scope,actor,outcome.Receivable.SOURCE_INVOICE_ID!);
        var row=loaded.Receivables.SingleOrDefault(value=>value.Id==outcome.Receivable.AR_INVOICE_ID);
        if(row is null || row.NativeRevision!=CompanySalesReceivableContributor.Create(loaded.Company,outcome.Receivable).Revision)throw new InvalidOperationException("The recorded headers do not confirm the exact native receivable disposition.");return loaded;
    }
    public async Task RecordReviewAsync(string decision,string financialDisposition,string financialEvidence,string sourceDisposition,string sourceEvidence,string reason,IAccountingServiceClient client)
    {
        if(!CanRecordReview || reviewSource is null)throw new InvalidOperationException("Review the current original receivable with independent disposition authority first.");
        var command=new CompanySalesReceivableDecisionCommand(Guid.NewGuid().ToString("N"),Copy(reviewSource),decision,financialDisposition,financialEvidence,sourceDisposition,sourceEvidence,reason);command.Require();pendingReview=command;await SubmitReviewAsync(client);
    }
    public async Task SubmitReviewAsync(IAccountingServiceClient client)
    {
        if(ReviewPending is not {} command)throw new InvalidOperationException("Return to the original reviewer company/account to reconcile.");ReadyReview(command.ReceivableId,write:true);
        var snapshot=company.Snapshot;var generation=++version;InvalidateReview();Busy=true;Changed?.Invoke();
        bool Current()=>!disposed && generation==version && snapshot==company.Snapshot && Visible;
        try
        {
            var result=Copy(await client.RecordCompanySalesReceivableDecisionAsync(Copy(command)));AccountingServiceClient.RequireReceivableDecision(result,command);if(!Current())return;
            var recovered=Copy(await client.GetCompanySalesReceivableReviewRecoveryAsync(command.ReceivableId));recovered.Require(command.Company,command.ActorUserId,command.ReceivableId);if(!Current())return;
            if(recovered.Outcome is null || JsonSerializer.Serialize(recovered.Outcome)!=JsonSerializer.Serialize(result))throw new InvalidOperationException("The fresh native recovery does not confirm the original reviewer disposition.");
            var headers=await ReviewedHeaders(result,client,snapshot.Selection!.Scope,snapshot.Selection.UserId);if(!Current())return;AcceptReview(recovered);status=headers;
        }
        finally{Busy=false;Changed?.Invoke();}
    }
}
