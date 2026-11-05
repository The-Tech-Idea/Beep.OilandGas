using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.Models;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

public sealed record CompanySalesInvoiceFacts(string InvoiceNumber,DateTime InvoiceDate,DateTime DueDate,decimal? TaxAmount,decimal? ChargesAmount,
    string TaxReviewNotes,string ChargesReviewNotes,string EntitlementReviewNotes,string Reason)
{
    public void Require(CompanyBillingPolicyRules rules)
    {
        rules.Require();BillingPolicyValues.Text(InvoiceNumber,128);BillingPolicyValues.Text(TaxReviewNotes,2000);BillingPolicyValues.Text(ChargesReviewNotes,2000);BillingPolicyValues.Text(EntitlementReviewNotes,2000);BillingPolicyValues.Text(Reason,1000);
        if(InvoiceDate==default || InvoiceDate.Kind!=DateTimeKind.Unspecified || InvoiceDate.TimeOfDay!=TimeSpan.Zero || DueDate.Kind!=DateTimeKind.Unspecified || DueDate.TimeOfDay!=TimeSpan.Zero || DueDate<InvoiceDate || TaxAmount is null || ChargesAmount is null)
            throw new InvalidOperationException("Explicit stored invoice/due dates, tax and charges proposals are required.");rules.RequireExactAmount(TaxAmount.Value);rules.RequireExactAmount(ChargesAmount.Value);
    }
}
/// <summary>Bounded native contributors avoid embedding recursively growing invoice histories in later commands.</summary>
public sealed record CompanySalesInvoiceContributor(CompanyScope Company,string FieldId,string TicketId,string InvoiceId,string IntentId,string Status,
    string CurrencyCode,decimal ProposedTotal,string PreparedBy,string RowRevision)
{
    public void Require(CompanyScope company,string field,string ticket)
    {
        if(Company!=company || FieldId!=field || TicketId!=ticket || Status is not ("DRAFT" or "APPROVED" or "REJECTED") || ProposedTotal<=0)throw new InvalidOperationException("The owned native invoice contributor is unconfirmed.");
        BillingPolicyValues.Intent(InvoiceId);BillingPolicyValues.Intent(IntentId);BillingPolicyValues.Text(CurrencyCode,16);BillingPolicyValues.Text(PreparedBy,128);BillingPolicyValues.Revision(RowRevision);
    }
    public static CompanySalesInvoiceContributor Create(CompanyScope company,INVOICE row)
    {
        var proof=CompanySalesInvoiceDraftProof.Require(company,row);var draft=proof.Record.Command;
        return new(company,row.FIELD_ID!,draft.Review.Delivery.Source.TicketId,row.INVOICE_ID,draft.IntentId,row.STATUS,row.CURRENCY_CODE,row.TOTAL_AMOUNT,row.ROW_CREATED_BY,
            row.REVIEW_WORKFLOW_JSON is null?proof.RowRevision:ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row)));
    }
}
public sealed record CompanySalesInvoiceDraftReview(RevenueDeliveryAcceptanceReadReview Delivery,CompanyBillingPolicyReview Policies,
    IReadOnlyList<CompanySalesInvoiceContributor> Invoices,string BindingRevision,string Revision)
{
    public const int MaximumInvoices=32;
    [JsonIgnore] public CompanyScope Company=>Delivery.Source.Source.Source.Company;
    [JsonIgnore] public string ActorUserId=>Delivery.Source.Source.Source.ActorUserId;
    [JsonIgnore] public string FieldId=>Delivery.Source.Source.Source.FieldId;
    [JsonIgnore] public string TicketId=>Delivery.Source.TicketId;
    [JsonIgnore] public string SourceKey=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company,FieldId,TicketId}));
    public static CompanySalesInvoiceDraftReview Create(RevenueDeliveryAcceptanceReadReview delivery,CompanyBillingPolicyReview policies,IReadOnlyList<INVOICE> rows,string binding)
    {
        if(delivery is null || policies is null || rows is null || rows.Count>MaximumInvoices || rows.Any(row=>row is null))throw new InvalidOperationException("Complete bounded invoice source evidence is required.");
        delivery=CompanyInventorySourceReview.Copy(delivery);policies=CompanyInventorySourceReview.Copy(policies);
        var company=delivery.Source.Source.Source.Company;var contributors=rows.Select(row=>CompanySalesInvoiceContributor.Create(company,row)).OrderBy(row=>row.InvoiceId,StringComparer.Ordinal).ToArray();
        var result=new CompanySalesInvoiceDraftReview(delivery,policies,contributors,binding,string.Empty);result=result with{Revision=Digest(result)};result.Require();return result;
    }
    public void Require()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("draft-source",this);if(CompanySalesInvoiceValidation.Contains(key))return;
        if(Delivery is null || Policies is null || Invoices is null || Invoices.Count>MaximumInvoices || Invoices.Any(row=>row is null))throw new InvalidOperationException("The complete current invoice source review is required.");
        Delivery.Require();PurchaseOrderOwnership.RequireScope(Company);BillingPolicyValues.Text(ActorUserId,128);BillingPolicyValues.Text(FieldId,128);BillingPolicyValues.Text(TicketId,128);BillingPolicyValues.Revision(BindingRevision);
        Policies.Require(Company,ActorUserId);BillingPolicyValues.Text(Delivery.Source.Source.Contract.BUYER_BA_ID,128);
        if(Invoices.Select(row=>row.InvoiceId).Distinct(StringComparer.Ordinal).Count()!=Invoices.Count || Revision!=Digest(this))throw new InvalidOperationException("The complete unique invoice source history or revision changed.");foreach(var row in Invoices)row.Require(Company,FieldId,TicketId);CompanySalesInvoiceValidation.Remember(key);
    }
    public (CompanyBillingPolicyRules Rules,decimal RoundedPrice,decimal Subtotal,decimal Total) ReviewAmounts(CompanySalesInvoiceFacts facts)
    {
        Require();var basis=Delivery.ReviewBillingBasis();basis.RequireCompatible(Delivery);if(facts is null)throw new InvalidOperationException("Explicit invoice facts are required.");
        var policy=Policies.RequireApplicable(basis.CurrencyCode!,facts.InvoiceDate);var rules=CompanyBillingPolicyProof.Require(Company,policy).Record.Rules;facts.Require(rules);
        var rounded=rules.RoundPriceExtension(basis.UnroundedAmount!.Value);var subtotal=ExactSum(rounded,facts.ChargesAmount!.Value);return(rules,rounded,subtotal,ExactSum(subtotal,facts.TaxAmount!.Value));
    }
    private static string Digest(CompanySalesInvoiceDraftReview review)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(review with{Revision=string.Empty}));
    private static decimal ExactSum(decimal first,decimal second)
    {
        static (BigInteger Coefficient,int Scale) Parts(decimal value){var bits=decimal.GetBits(value);return((BigInteger)(uint)bits[0]+((BigInteger)(uint)bits[1]<<32)+((BigInteger)(uint)bits[2]<<64),(bits[3]>>16)&255);}
        var a=Parts(first);var b=Parts(second);var scale=Math.Max(a.Scale,b.Scale);var sum=a.Coefficient*BigInteger.Pow(10,scale-a.Scale)+b.Coefficient*BigInteger.Pow(10,scale-b.Scale);
        while(scale>0 && sum%10==0){sum/=10;--scale;}if(sum>((BigInteger.One<<96)-1) || scale>28)throw new InvalidOperationException("The exact invoice proposal total exceeds native decimal range or precision.");
        return new decimal(unchecked((int)(uint)(sum&uint.MaxValue)),unchecked((int)(uint)((sum>>32)&uint.MaxValue)),unchecked((int)(uint)((sum>>64)&uint.MaxValue)),false,(byte)scale);
    }
}
public sealed record CompanySalesInvoiceDraftCommand(string IntentId,string InvoiceId,CompanySalesInvoiceDraftReview Review,CompanySalesInvoiceFacts Facts)
{
    public void Require()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("draft-command",this);if(CompanySalesInvoiceValidation.Contains(key))return;
        BillingPolicyValues.Intent(IntentId);BillingPolicyValues.Intent(InvoiceId);if(IntentId==InvoiceId || Review is null)throw new InvalidOperationException("Distinct original invoice and command identities and full source review are required.");
        Review.Require();_=Review.ReviewAmounts(Facts);if(Review.Invoices.Count!=0)throw new InvalidOperationException("An owned invoice already contributes to this delivery. Reconcile its recorded workflow before preparing another draft.");
        if(JsonSerializer.Serialize(this).Length>SALES_INVOICE_INTENT.MaximumCommandLength)throw new InvalidOperationException("The complete invoice preparation exceeds its native journal bound.");CompanySalesInvoiceValidation.Remember(key);
    }
    public void RequireCurrent(CompanySalesInvoiceDraftReview current)
    {Require();current.Require();if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(Review))throw new InvalidOperationException("The complete original delivery, company billing policy, invoice history or binding changed.");}
    public void RequireAfterInsertion(CompanySalesInvoiceDraftReview current,INVOICE row)
    {
        if(!MatchesAfterInsertion(current,row))throw new InvalidOperationException("Invoice closure requires the unchanged original source/policies and exactly its proven native draft contribution.");
    }
    public bool MatchesAfterInsertion(CompanySalesInvoiceDraftReview current,INVOICE row)
    {
        Require();current.Require();var proof=CompanySalesInvoiceDraftProof.Require(Review.Company,row);
        return JsonSerializer.Serialize(proof.Record.Command)==JsonSerializer.Serialize(this) && current.BindingRevision==Review.BindingRevision &&
            JsonSerializer.Serialize(current.Delivery)==JsonSerializer.Serialize(Review.Delivery) && JsonSerializer.Serialize(current.Policies)==JsonSerializer.Serialize(Review.Policies) &&
            current.Invoices.Count==1 && JsonSerializer.Serialize(current.Invoices[0])==JsonSerializer.Serialize(CompanySalesInvoiceContributor.Create(Review.Company,row));
    }
}
public sealed record CompanySalesInvoiceDraftRecord(CompanySalesInvoiceDraftCommand Command,DateTime PreparedAtUtc)
{
    public void Require()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("draft-record",this);if(CompanySalesInvoiceValidation.Contains(key))return;
        if(Command is null)throw new InvalidOperationException("The original invoice preparation is required.");Command.Require();
        var review=Command.Review;var amounts=review.ReviewAmounts(Command.Facts);
        var policy=CompanyBillingPolicyProof.Require(review.Company,review.Policies.RequireApplicable(amounts.Rules.CurrencyCode,Command.Facts.InvoiceDate));
        if(PreparedAtUtc==default || PreparedAtUtc.Kind!=DateTimeKind.Utc || review.Delivery.Recorded?.DecidedAtUtc>PreparedAtUtc || policy.Record.ApprovedAtUtc>PreparedAtUtc)
            throw new InvalidOperationException("Ordered UTC invoice preparation after receiving and company policy approval is required.");CompanySalesInvoiceValidation.Remember(key);
    }
}
public sealed record CompanySalesInvoiceDraftProof(CompanySalesInvoiceDraftRecord Record,string RowRevision)
{
    public const int MaximumLength=4194304;
    public static INVOICE Prepare(CompanySalesInvoiceDraftCommand command,DateTime at)
    {
        command=CompanyInventorySourceReview.Copy(command);var record=new CompanySalesInvoiceDraftRecord(command,at);record.Require();
        var review=command.Review;var facts=command.Facts;var amounts=review.ReviewAmounts(facts);
        var row=new INVOICE{INVOICE_ID=command.InvoiceId,INVOICE_NUMBER=facts.InvoiceNumber,CUSTOMER_BA_ID=review.Delivery.Source.Source.Contract.BUYER_BA_ID,
            OWNER_TENANT_ID=review.Company.TenantId,OWNER_COMPANY_BA_ID=review.Company.BusinessAssociateId,FIELD_ID=review.FieldId,SOURCE_RUN_TICKET_ID=review.TicketId,BILLING_SOURCE_KEY=review.SourceKey,
            INVOICE_DATE=facts.InvoiceDate,DUE_DATE=facts.DueDate,SUBTOTAL=amounts.Subtotal,TAX_AMOUNT=facts.TaxAmount!.Value,TOTAL_AMOUNT=amounts.Total,PAID_AMOUNT=0m,BALANCE_DUE=amounts.Total,
            CURRENCY_CODE=amounts.Rules.CurrencyCode,STATUS="DRAFT",DESCRIPTION=facts.Reason,ACTIVE_IND="Y",PPDM_GUID=command.IntentId,ROW_CREATED_BY=review.ActorUserId,ROW_CHANGED_BY=review.ActorUserId,
            ROW_CREATED_DATE=at,ROW_CHANGED_DATE=at};
        row.BILLING_WORKFLOW_JSON=Create(record,row);_=Require(review.Company,row);return row;
    }
    public static string RowRevisionFor(INVOICE row){var copy=CompanyInventorySourceReview.Copy(row);copy.BILLING_WORKFLOW_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static string Create(CompanySalesInvoiceDraftRecord record,INVOICE row)
    {record.Require();var json=JsonSerializer.Serialize(new CompanySalesInvoiceDraftProof(record,RowRevisionFor(row)));if(json.Length>MaximumLength)throw new InvalidOperationException("The invoice draft proof exceeds its native bound.");return json;}
    public static CompanySalesInvoiceDraftProof Require(CompanyScope company,INVOICE row)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();
        if(row.REVIEW_WORKFLOW_JSON is not null)
        {
            var reviewed=CompanySalesInvoiceDecisionProof.Require(company,row);
            return Require(company,reviewed.Intent.Restore(company,reviewed.Intent.ROW_CREATED_BY).Review.Original.Invoice);
        }
        PurchaseOrderOwnership.RequireScope(company);if(string.IsNullOrWhiteSpace(row.BILLING_WORKFLOW_JSON) || row.BILLING_WORKFLOW_JSON.Length>MaximumLength)throw new InvalidOperationException("Complete native invoice preparation proof is required.");
        CompanySalesInvoiceDraftProof proof;try{proof=JsonSerializer.Deserialize<CompanySalesInvoiceDraftProof>(row.BILLING_WORKFLOW_JSON,new JsonSerializerOptions{UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow}) ??throw new InvalidOperationException("Invoice proof cannot be null.");}catch(JsonException){throw new InvalidOperationException("Invoice preparation proof is malformed.");}
        var key=CompanySalesInvoiceValidation.Key("draft-row",row,company);if(CompanySalesInvoiceValidation.Contains(key))return proof;
        if(proof.Record is null)throw new InvalidOperationException("The complete original invoice record is required.");var record=proof.Record;record.Require();var command=record.Command;var review=command.Review;var facts=command.Facts;var amounts=review.ReviewAmounts(facts);
        if(review.Company!=company || row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.FIELD_ID!=review.FieldId || row.SOURCE_RUN_TICKET_ID!=review.TicketId || row.BILLING_SOURCE_KEY!=review.SourceKey ||
            row.INVOICE_ID!=command.InvoiceId || row.INVOICE_NUMBER!=facts.InvoiceNumber || row.CUSTOMER_BA_ID!=review.Delivery.Source.Source.Contract.BUYER_BA_ID || row.INVOICE_DATE!=facts.InvoiceDate || row.INVOICE_DATE?.Kind!=DateTimeKind.Unspecified || row.DUE_DATE!=facts.DueDate || row.DUE_DATE?.Kind!=DateTimeKind.Unspecified ||
            row.SUBTOTAL!=amounts.Subtotal || row.TAX_AMOUNT!=facts.TaxAmount || row.TOTAL_AMOUNT!=amounts.Total || row.PAID_AMOUNT!=0 || row.BALANCE_DUE!=amounts.Total || row.CURRENCY_CODE!=amounts.Rules.CurrencyCode || row.STATUS!="DRAFT" || row.ACTIVE_IND!="Y" ||
            row.PPDM_GUID!=command.IntentId || row.ROW_CREATED_BY!=review.ActorUserId || row.ROW_CHANGED_BY!=review.ActorUserId || row.ROW_CREATED_DATE!=record.PreparedAtUtc || row.ROW_CREATED_DATE?.Kind!=DateTimeKind.Utc || row.ROW_CHANGED_DATE is not {} changed || changed.Kind!=DateTimeKind.Utc || changed<record.PreparedAtUtc ||
            row.DESCRIPTION!=facts.Reason || proof.RowRevision!=RowRevisionFor(row))throw new InvalidOperationException("Native invoice ownership, source, exact proposed amounts and preparation audit disagree.");CompanySalesInvoiceValidation.Remember(key);return proof;
    }
}
public sealed record CompanySalesInvoiceDraftOutcome(SALES_INVOICE_INTENT Intent,INVOICE Invoice,SALES_INVOICE_OUTCOME Receipt)
{
    public void Require(CompanySalesInvoiceDraftCommand command)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("draft-outcome",this,command);if(CompanySalesInvoiceValidation.Contains(key))return;
        command.Require();if(Intent is null || Invoice is null || Receipt is null)throw new InvalidOperationException("The original native command, invoice and append-only outcome receipt are required.");
        var retained=Intent.Restore(command.Review.Company,command.Review.ActorUserId);var invoice=CompanySalesInvoiceDraftProof.Require(command.Review.Company,Invoice);
        if(JsonSerializer.Serialize(retained)!=JsonSerializer.Serialize(command) || JsonSerializer.Serialize(invoice.Record.Command)!=JsonSerializer.Serialize(command) || Invoice.ROW_CREATED_DATE<Intent.ROW_CREATED_DATE ||
            JsonSerializer.Serialize(Receipt.Restore(Intent))!=JsonSerializer.Serialize(Invoice.REVIEW_WORKFLOW_JSON is null?Invoice:
                CompanySalesInvoiceDecisionProof.Require(command.Review.Company,Invoice).Intent.Restore(command.Review.Company,Invoice.ROW_CHANGED_BY).Review.Original.Invoice))
            throw new InvalidOperationException("The exact original journal and subsequent native invoice draft outcome are unconfirmed.");CompanySalesInvoiceValidation.Remember(key);
    }
}
public sealed record CompanySalesInvoiceWorkspaceReview(CompanySalesInvoiceDraftReview Source,SALES_INVOICE_INTENT? Retained,CompanySalesInvoiceDraftOutcome? Outcome)
{
    public void Require(CompanyScope company,string actor,string ticket)
    {
        if(Source is null)throw new InvalidOperationException("The complete original invoice workspace source is required.");Source.Require();
        if(Source.Company!=company || Source.ActorUserId!=actor || Source.TicketId!=ticket)throw new InvalidOperationException("The invoice workspace company/account/delivery changed.");
        if(Retained is null)
        {if(Outcome is not null || Source.Invoices.Any(row=>row.PreparedBy==actor))throw new InvalidOperationException("The original owned invoice command journal is unconfirmed.");return;}
        var command=Retained.Restore(company,actor);
        if(command.Review.FieldId!=Source.FieldId || command.Review.TicketId!=ticket)throw new InvalidOperationException("The retained command belongs to another original delivery.");
        if(Outcome is null){if(Source.Invoices.Count!=0)throw new InvalidOperationException("The retained invoice outcome or receipt is unconfirmed.");return;}Outcome.Require(command);
        if(JsonSerializer.Serialize(Outcome.Intent)!=JsonSerializer.Serialize(Retained) || Source.Invoices.Count!=1 ||
            JsonSerializer.Serialize(Source.Invoices[0])!=JsonSerializer.Serialize(CompanySalesInvoiceContributor.Create(company,Outcome.Invoice)))
            throw new InvalidOperationException("The workspace does not include the exact retained native invoice outcome.");
    }
}
