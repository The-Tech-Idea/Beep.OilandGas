using System.Text.Json;
using System.Text.Json.Serialization;
namespace TheTechIdea.Data.OilGas;

/// <summary>Independent review evidence; read-only admission never records approval or posting.</summary>
public sealed record CompanySalesInvoiceReviewSource(CompanySalesInvoiceDraftOutcome Original,CompanySalesInvoiceDraftReview Current,string Revision)
{
    [JsonIgnore] public string InvoiceId=>Original.Invoice.INVOICE_ID;
    [JsonIgnore] public bool IsIndependent=>Current.ActorUserId!=Original.Intent.ROW_CREATED_BY;
    [JsonIgnore] public bool HasCurrentOriginalSource
    {
        get{Require(Current.Company,Current.ActorUserId,InvoiceId);try{RequireOriginalSource();return true;}catch(InvalidOperationException){return false;}}
    }
    [JsonIgnore] public bool CanReview=>Original.Invoice.STATUS=="DRAFT" && IsIndependent && HasCurrentOriginalSource;
    public static CompanySalesInvoiceReviewSource Create(CompanySalesInvoiceDraftOutcome original,CompanySalesInvoiceDraftReview current)
    {
        original=CompanyInventorySourceReview.Copy(original);current=CompanyInventorySourceReview.Copy(current);
        var result=new CompanySalesInvoiceReviewSource(original,current,string.Empty);result=result with{Revision=Digest(result)};result.Require(current.Company,current.ActorUserId,original.Invoice.INVOICE_ID);return result;
    }
    public void Require(CompanyScope company,string actor,string invoice)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("review-source",this,new{company,actor,invoice});if(CompanySalesInvoiceValidation.Contains(key))return;
        if(Original is null || Original.Intent is null || Original.Invoice is null || Current is null)throw new InvalidOperationException("Complete invoice command, native outcome and current review evidence are required.");
        var command=Original.Intent.Restore(company,Original.Intent.ROW_CREATED_BY);Original.Require(command);Current.Require();
        if(Current.Company!=company || Current.ActorUserId!=actor || InvoiceId!=invoice || Current.FieldId!=command.Review.FieldId || Current.TicketId!=command.Review.TicketId ||
            Current.Invoices.Count!=1 || JsonSerializer.Serialize(Current.Invoices[0])!=JsonSerializer.Serialize(CompanySalesInvoiceContributor.Create(company,Original.Invoice)) || Revision!=Digest(this))
            throw new InvalidOperationException("The original owned invoice differs from its complete current review source.");CompanySalesInvoiceValidation.Remember(key);
    }
    public void RequireIndependentCurrent()
    {using var validation=CompanySalesInvoiceValidation.Enter();Require(Current.Company,Current.ActorUserId,InvoiceId);if(!IsIndependent)throw new InvalidOperationException("The invoice maker cannot independently review their own proposal.");RequireOriginalSource();}
    private void RequireOriginalSource()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("invoice-original-source",this);if(CompanySalesInvoiceValidation.Contains(key))return;
        // Compare physical evidence as the original reader without changing stored maker proofs or the current reviewer's authority.
        var command=Original.Intent.Restore(Current.Company,Original.Intent.ROW_CREATED_BY);var delivery=Current.Delivery;var field=delivery.Source.Source.Source;
        if(delivery.IsHistorical)throw new InvalidOperationException("Historical receiving cannot establish a current invoice review source.");
        var normalizedField=FieldRevenueSourceReview.Create(field.Company,command.Review.ActorUserId,field.FieldId,field.From,field.Through,field.BindingRevision,field.Rows);
        var contract=RevenueSalesContractReview.Create(normalizedField,delivery.Source.Source.TransactionId,delivery.Source.Source.Contract);
        var physical=RevenueDeliverySourceReview.Create(contract,delivery.Source.Allocation,delivery.Source.Ticket);
        var currentPolicy=Current.Policies.RequireApplicable(Original.Invoice.CURRENCY_CODE,command.Facts.InvoiceDate);
        var originalPolicy=command.Review.Policies.RequireApplicable(Original.Invoice.CURRENCY_CODE,command.Facts.InvoiceDate);
        if(JsonSerializer.Serialize(currentPolicy)!=JsonSerializer.Serialize(originalPolicy))throw new InvalidOperationException("The invoice's applicable approved policy changed.");
        // Later non-overlapping policies remain visible in Current; they do not replace this invoice's unchanged applicable policy.
        var normalized=CompanySalesInvoiceDraftReview.Create(new(physical,delivery.Recorded),command.Review.Policies,[Original.Invoice],Current.BindingRevision);
        command.RequireAfterInsertion(normalized,Original.Invoice);
        CompanySalesInvoiceValidation.Remember(key);
    }
    private static string Digest(CompanySalesInvoiceReviewSource source)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(source with{Revision=string.Empty}));
    /// <summary>Rebuild read-contract revisions for source comparison only; this never grants reader authority.</summary>
    public CompanySalesInvoiceReviewSource AsReader(string actor)
    {
        Require(Current.Company,Current.ActorUserId,InvoiceId);BillingPolicyValues.Text(actor,128);
        var delivery=Current.Delivery;var field=delivery.Source.Source.Source;
        var normalizedField=FieldRevenueSourceReview.Create(field.Company,actor,field.FieldId,field.From,field.Through,field.BindingRevision,field.Rows);
        var contract=RevenueSalesContractReview.Create(normalizedField,delivery.Source.Source.TransactionId,delivery.Source.Source.Contract);
        var physical=RevenueDeliverySourceReview.Create(contract,delivery.Source.Allocation,delivery.Source.Ticket);
        return Create(Original,CompanySalesInvoiceDraftReview.Create(new(physical,delivery.Recorded),CompanyBillingPolicyReview.Create(Current.Company,actor,Current.Policies.Records),[Original.Invoice],Current.BindingRevision));
    }
}
