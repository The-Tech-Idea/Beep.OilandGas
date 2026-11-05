using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class CompanySalesReceivableWorkspaceState
{
    private CompanySalesPaymentReviewSource? paymentReviewSource;
    private CompanySalesPaymentReviewCommand? pendingPaymentReview;
    private CompanySalesPaymentReviewRecovery? paymentReviewRecovery;
    public bool HasPendingPaymentReview=>pendingPaymentReview is not null;
    private bool PaymentReviewerGrant=>PaymentGrant && company.Selection!.Permissions.Contains("Accounting.ReviewSalesPayment",StringComparer.Ordinal);
    private bool PendingPaymentReviewMatches=>PaymentGrant && pendingPaymentReview is {} value && value.Review.Current.Ledger.Company==company.Selection!.Scope && value.Review.Current.Ledger.ActorUserId==company.Selection.UserId;
    public bool CanReadPaymentReview=>PaymentReviewerGrant;
    public CompanySalesPaymentReviewSource? PaymentReviewSource=>PaymentReviewerGrant && paymentReviewSource is not null?Copy(paymentReviewSource):null;
    public CompanySalesPaymentReviewCommand? PendingPaymentReview=>PendingPaymentReviewMatches?Copy(pendingPaymentReview!):null;
    public CompanySalesPaymentReviewRecovery? PaymentReviewRecovery=>PaymentGrant && paymentReviewRecovery is not null?Copy(paymentReviewRecovery):null;
    private bool NoOtherPaymentReviewWork=>pendingPayment is null && pending is null && pendingReview is null && pendingJournal is null && pendingJournalReview is null && pendingJournalPosting is null;
    public bool CanRecordPaymentReview=>PaymentReviewerGrant && !Busy && company.Selection!.Permissions.Contains("Accounting.RecordSalesPaymentDecision",StringComparer.Ordinal) && NoOtherPaymentReviewWork && pendingPaymentReview is null && paymentReviewSource is not null;
    public bool CanRetryPaymentReview=>PendingPaymentReviewMatches && PaymentReviewerGrant && !Busy && company.Selection!.Permissions.Contains("Accounting.RecordSalesPaymentDecision",StringComparer.Ordinal);
    public async Task LoadPaymentReviewAsync(string id,IAccountingServiceClient client)
    {
        Ready();if(!PaymentReviewerGrant || !NoOtherPaymentReviewWork || pendingPaymentReview is not null)throw new InvalidOperationException("Recover the original retained financial work before a fresh independent review.");var snapshot=company.Snapshot;Invalidate();admitted=true;var generation=version;Busy=true;Changed?.Invoke();
        try{var loaded=await client.GetCompanySalesPaymentReviewAsync(id);loaded.Require();if(loaded.Original.AR_PAYMENT_ID!=id || loaded.Current.Ledger.Company!=snapshot.Selection!.Scope || loaded.Current.Ledger.ActorUserId!=snapshot.Selection.UserId)throw new InvalidOperationException("The original payment reviewer context is unconfirmed.");if(!disposed && generation==version && snapshot==company.Snapshot)paymentReviewSource=Copy(loaded);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task RecordPaymentReviewAsync(string decision,string financialDisposition,string financialEvidence,string receiptDisposition,string receiptEvidence,string reason,IAccountingServiceClient client)
    {
        Ready();if(!CanRecordPaymentReview)throw new InvalidOperationException("Review the original native payment draft and reconcile retained work first.");var command=new CompanySalesPaymentReviewCommand(Guid.NewGuid().ToString("N"),Copy(paymentReviewSource!),decision,financialDisposition,financialEvidence,receiptDisposition,receiptEvidence,reason);command.Require();pendingPaymentReview=Copy(command);await SubmitPaymentReviewAsync(client);
    }
    public async Task SubmitPaymentReviewAsync(IAccountingServiceClient client)
    {
        Ready();if(!CanRetryPaymentReview || !NoOtherPaymentReviewWork)throw new UnauthorizedAccessException();var original=Copy(pendingPaymentReview!);var snapshot=company.Snapshot;Invalidate();admitted=true;var generation=version;Busy=true;Changed?.Invoke();bool Current()=>!disposed && generation==version && snapshot==company.Snapshot;
        try{var row=await client.RecordCompanySalesPaymentReviewAsync(Copy(original));if(JsonSerializer.Serialize(CompanySalesPaymentReviewProof.Require(row).Intent.Restore())!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The exact original payment decision acknowledgement is unconfirmed.");if(!Current())return;var recovery=await client.GetCompanySalesPaymentReviewRecoveryAsync(original.Review.Original.AR_PAYMENT_ID);if(!Current())return;await ConfirmPaymentReview(recovery,original,client,Current);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task RecoverPaymentReviewAsync(string id,IAccountingServiceClient client)
    {
        Ready();if(!PaymentGrant || !NoOtherPaymentReviewWork || pendingPaymentReview is not null && (!PendingPaymentReviewMatches || pendingPaymentReview.Review.Original.AR_PAYMENT_ID!=id))throw new InvalidOperationException("Recover the retained original payment review in its own company/account.");var original=pendingPaymentReview;var snapshot=company.Snapshot;Invalidate();admitted=true;var generation=version;Busy=true;Changed?.Invoke();bool Current()=>!disposed && generation==version && snapshot==company.Snapshot;
        try{var loaded=await client.GetCompanySalesPaymentReviewRecoveryAsync(id);loaded.Require();if(loaded.Company!=snapshot.Selection!.Scope || loaded.ActorUserId!=snapshot.Selection.UserId || loaded.PaymentId!=id)throw new InvalidOperationException("The original native payment reviewer recovery is unconfirmed.");if(!Current())return;if(loaded.Intent is null){if(original is not null)throw new InvalidOperationException("No retained native decision confirms this original request. Keep it for explicit retry.");paymentReviewRecovery=Copy(loaded);return;}var command=loaded.Intent.Restore();if(original is not null && JsonSerializer.Serialize(command)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Another original decision owns this payment.");pendingPaymentReview=Copy(command);if(loaded.Outcome is null){paymentReviewRecovery=Copy(loaded);return;}await ConfirmPaymentReview(loaded,command,client,Current);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    private async Task ConfirmPaymentReview(CompanySalesPaymentReviewRecovery recovery,CompanySalesPaymentReviewCommand command,IAccountingServiceClient client,Func<bool> current)
    {
        recovery.Require();if(recovery.Intent is null || recovery.Outcome is null || JsonSerializer.Serialize(recovery.Intent.Restore())!=JsonSerializer.Serialize(command))throw new InvalidOperationException("The durable original payment disposition is unconfirmed.");var source=command.Review.Current.Ledger;var loaded=await client.GetCompanySalesPaymentSourceAsync(source.ReceivableId,source.BookId,source.CurrencyCode);loaded.Require(source.Company,source.ActorUserId,source.ReceivableId,source.BookId,source.CurrencyCode);var fact=loaded.Payments.SingleOrDefault(row=>row.Id==recovery.PaymentId);if(fact?.NativeRevision!=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(recovery.Outcome)) || fact.Review!=CompanySalesPaymentReviewProof.Require(recovery.Outcome).Summary())throw new InvalidOperationException("The complete public native payment source does not confirm this decision.");if(!current())return;paymentSource=Copy(loaded);paymentReviewRecovery=Copy(recovery);pendingPaymentReview=null;
    }
}
