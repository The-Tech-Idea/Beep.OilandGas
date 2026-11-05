using System.Text.Json;
namespace TheTechIdea.Data.OilGas;
/// <summary>Public proven payment facts and stable original draft identity; private receipt bodies are excluded.</summary>
public sealed record CompanySalesPaymentWorkflowSource(CompanyScope Company,string ActorUserId,string BindingRevision,string PaymentId,string SourceRevision,CompanySalesPaymentSource Current,string Revision)
{
    public CompanySalesPaymentFacts Payment=>Current.Payments.Single(row=>row.Id==PaymentId);
    public static CompanySalesPaymentWorkflowSource Create(CompanySalesPaymentSource current,string id)
    {
        var row=current.Payments.SingleOrDefault(row=>row.Id==id)??throw new InvalidOperationException("The original native payment is absent from its complete source.");var value=new CompanySalesPaymentWorkflowSource(current.Ledger.Company,current.Ledger.ActorUserId,current.Ledger.BindingRevision,id,row.PreparationRevision??row.NativeRevision,CompanyInventorySourceReview.Copy(current),string.Empty);value=value with{Revision=Digest(value)};value.Require(value.Company,value.ActorUserId,id);return value;
    }
    public void Require(CompanyScope company,string actor,string id)
    {
        PurchaseOrderOwnership.RequireScope(company);BillingPolicyValues.Text(actor,128);BillingPolicyValues.Intent(id);BillingPolicyValues.Revision(BindingRevision);BillingPolicyValues.Revision(SourceRevision);ArgumentNullException.ThrowIfNull(Current);Current.Require(company,actor,Current.Ledger.ReceivableId,Current.Ledger.BookId,Current.Ledger.CurrencyCode);
        if(Company!=company || ActorUserId!=actor || PaymentId!=id || Current.Ledger.BindingRevision!=BindingRevision || Current.Payments.Count(row=>row.Id==id)!=1 || SourceRevision!=(Payment.PreparationRevision??Payment.NativeRevision) || Revision!=Digest(this))throw new InvalidOperationException("The exact original payment workflow source/context changed.");
    }
    private static string Digest(CompanySalesPaymentWorkflowSource value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value with{Revision=string.Empty}));
}
public sealed record CompanySalesPaymentProcessSourceReview(CompanyScope Company,string ActorUserId,string InstanceId,string PaymentId,string SourceRevision,string ProcessRevision,CompanySalesPaymentWorkflowSource Payment)
{
    public void Require(CompanyScope company,string actor,string instance)
    {
        PurchaseOrderOwnership.RequireScope(company);BillingPolicyValues.Text(actor,128);BillingPolicyValues.Text(instance,128);BillingPolicyValues.Intent(PaymentId);BillingPolicyValues.Revision(SourceRevision);BillingPolicyValues.Revision(ProcessRevision);if(Payment is null || Company!=company || ActorUserId!=actor || InstanceId!=instance)throw new InvalidOperationException("The complete owned payment process source is required.");Payment.Require(company,actor,PaymentId);if(SourceRevision!=Payment.SourceRevision)throw new InvalidOperationException("The payment process no longer matches its original immutable draft.");
    }
}
