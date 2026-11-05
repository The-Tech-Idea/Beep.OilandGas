using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Web.Services;
public sealed partial class CompanySalesReceivableWorkspaceState
{
    private CompanySalesPaymentSource? paymentSource;
    private CompanySalesPaymentCommand? pendingPayment;
    private AR_PAYMENT? paymentResult;
    public bool HasPendingPayment=>pendingPayment is not null;
    private bool PaymentGrant=>Visible && company.Selection!.Permissions.Contains("Accounting.ReviewSalesPaymentSource",StringComparer.Ordinal);
    public bool CanReadPayments=>PaymentGrant;
    private bool PendingPaymentMatches=>PaymentGrant && pendingPayment is {} value && value.Source.Ledger.Company==company.Selection!.Scope && value.Source.Ledger.ActorUserId==company.Selection.UserId;
    public CompanySalesPaymentSource? PaymentSource=>PaymentGrant && paymentSource is not null?Copy(paymentSource):null;
    public CompanySalesPaymentCommand? PendingPayment=>PendingPaymentMatches?Copy(pendingPayment!):null;
    public AR_PAYMENT? PaymentResult=>PaymentGrant && paymentResult is not null?Copy(paymentResult):null;
    public bool CanPreparePayment=>PaymentGrant && !Busy && company.Selection!.Permissions.Contains("Accounting.PrepareSalesPayment",StringComparer.Ordinal) && pendingPaymentReview is null && pendingPayment is null && pending is null && pendingReview is null && pendingJournal is null && pendingJournalReview is null && pendingJournalPosting is null && paymentSource is not null;
    public async Task LoadPaymentSourceAsync(string receivable,string book,string currency,IAccountingServiceClient client)
    {
        Ready();if(!PaymentGrant)throw new UnauthorizedAccessException();var snapshot=company.Snapshot;Invalidate();admitted=true;var generation=version;Busy=true;Changed?.Invoke();
        try{var loaded=await client.GetCompanySalesPaymentSourceAsync(receivable,book,currency);loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,receivable,book,currency);if(!disposed && generation==version && snapshot==company.Snapshot)paymentSource=Copy(loaded);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task PreparePaymentAsync(string number,DateTime date,decimal amount,string method,string payer,string reference,string evidence,DateTime observed,string reason,IAccountingServiceClient client)
    {
        Ready();if(!CanPreparePayment || paymentSource is not {} displayed)throw new InvalidOperationException("Review the original posted payment source and reconcile all retained work first.");var command=new CompanySalesPaymentCommand(Guid.NewGuid().ToString("N"),Guid.NewGuid().ToString("N"),Copy(displayed),number,date,amount,method,payer,reference,evidence,observed,reason);command.Require();pendingPayment=Copy(command);await SubmitPaymentAsync(client);
    }
    public async Task SubmitPaymentAsync(IAccountingServiceClient client)
    {
        Ready();if(!PendingPaymentMatches || !company.Selection!.Permissions.Contains("Accounting.PrepareSalesPayment",StringComparer.Ordinal))throw new UnauthorizedAccessException();var original=Copy(pendingPayment!);var snapshot=company.Snapshot;Invalidate();admitted=true;var generation=version;Busy=true;Changed?.Invoke();bool Current()=>!disposed && generation==version && snapshot==company.Snapshot;
        try{var ack=await client.PrepareCompanySalesPaymentAsync(Copy(original));if(JsonSerializer.Serialize(CompanySalesPaymentProof.Require(ack).Command)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The original payment acknowledgement is unconfirmed.");if(!Current())return;var row=await client.GetCompanySalesPaymentDraftAsync(original.Id) ??throw new InvalidOperationException("The durable original payment proposal is unavailable.");if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(ack) || JsonSerializer.Serialize(CompanySalesPaymentProof.Require(row).Command)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The exact original payment recovery differs from its acknowledgement.");if(!Current())return;await ConfirmPaymentAsync(row,client,Current);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task RecoverPaymentAsync(string id,IAccountingServiceClient client)
    {
        Ready();if(!PaymentGrant || pendingPaymentReview is not null || pending is not null || pendingReview is not null || pendingJournal is not null || pendingJournalReview is not null || pendingJournalPosting is not null || pendingPayment is not null && (!PendingPaymentMatches || pendingPayment.Id!=id))throw new InvalidOperationException("Reconcile the retained original request in its own context.");var original=pendingPayment;var snapshot=company.Snapshot;Invalidate();admitted=true;var generation=version;Busy=true;Changed?.Invoke();bool Current()=>!disposed && generation==version && snapshot==company.Snapshot;
        try{var row=await client.GetCompanySalesPaymentDraftAsync(id) ??throw new InvalidOperationException("The original native payment request is unavailable.");var command=CompanySalesPaymentProof.Require(row).Command;if(command.Id!=id || command.Source.Ledger.Company!=snapshot.Selection!.Scope || command.Source.Ledger.ActorUserId!=snapshot.Selection.UserId || original is not null && JsonSerializer.Serialize(original)!=JsonSerializer.Serialize(command))throw new InvalidOperationException("The original payment producer/context/body is unconfirmed.");if(!Current())return;pendingPayment=Copy(command);await ConfirmPaymentAsync(row,client,Current);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    private async Task ConfirmPaymentAsync(AR_PAYMENT row,IAccountingServiceClient client,Func<bool> current)
    {
        var command=CompanySalesPaymentProof.Require(row).Command;var source=command.Source.Ledger;var loaded=await client.GetCompanySalesPaymentSourceAsync(source.ReceivableId,source.BookId,source.CurrencyCode);loaded.Require(source.Company,source.ActorUserId,source.ReceivableId,source.BookId,source.CurrencyCode);var expected=CompanySalesPaymentSource.Create(loaded.Ledger,loaded.CustomerId,loaded.Currency,loaded.CurrencyDecimals,loaded.OriginalAmount,[row]).Payments.Single();var currentFact=loaded.Payments.SingleOrDefault(value=>value.Id==command.Id);var confirmedFact=currentFact is {Review:not null}?currentFact with{Status="DRAFT",NativeRevision=currentFact.PreparationRevision!,PreparationRevision=null,Review=null}:currentFact;if(confirmedFact!=expected)throw new InvalidOperationException("The complete public payment source does not confirm this original proposal.");if(!current())return;paymentSource=Copy(loaded);paymentResult=Copy(row);pendingPayment=null;
    }
}
