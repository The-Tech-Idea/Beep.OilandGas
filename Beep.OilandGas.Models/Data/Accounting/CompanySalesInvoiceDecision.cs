using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Accounting;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Independent invoice disposition; it changes no financial amounts and creates no posting.</summary>
public sealed record CompanySalesInvoiceDecisionCommand(string IntentId,CompanySalesInvoiceReviewSource Review,string Decision,
    string TaxDisposition,string TaxEvidence,string ChargesDisposition,string ChargesEvidence,string EntitlementDisposition,string EntitlementEvidence,string Reason)
{
    [JsonIgnore] public CompanyScope Company=>Review.Current.Company;
    [JsonIgnore] public string ActorUserId=>Review.Current.ActorUserId;
    [JsonIgnore] public string InvoiceId=>Review.InvoiceId;
    public void Require()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("decision-command",this);if(CompanySalesInvoiceValidation.Contains(key))return;
        BillingPolicyValues.Intent(IntentId);if(Review is null)throw new InvalidOperationException("The exact displayed independent invoice review is required.");
        Review.RequireIndependentCurrent();
        if(Review.Original.Invoice.STATUS!="DRAFT" || Review.Original.Invoice.REVIEW_WORKFLOW_JSON is not null || IntentId==InvoiceId || IntentId==Review.Original.Intent.SALES_INVOICE_INTENT_ID || Decision is not ("APPROVE" or "REJECT"))
            throw new InvalidOperationException("One explicit independent disposition of the original native draft is required.");
        foreach(var disposition in new[]{TaxDisposition,ChargesDisposition,EntitlementDisposition})
            if(disposition is not ("CONFIRMED" or "ISSUE"))throw new InvalidOperationException("Explicit tax, charges and entitlement dispositions are required.");
        foreach(var evidence in new[]{TaxEvidence,ChargesEvidence,EntitlementEvidence})BillingPolicyValues.Text(evidence,2000);
        BillingPolicyValues.Text(Reason,1000);
        if(Decision=="APPROVE" && new[]{TaxDisposition,ChargesDisposition,EntitlementDisposition}.Any(value=>value!="CONFIRMED"))throw new InvalidOperationException("Approval requires confirmed tax, charges and entitlement evidence.");
        if(JsonSerializer.Serialize(this).Length>SALES_INVOICE_REVIEW_INTENT.MaximumCommandLength)throw new InvalidOperationException("The original reviewer command exceeds its native journal bound.");CompanySalesInvoiceValidation.Remember(key);
    }
    public void RequireCurrent(CompanySalesInvoiceReviewSource current)
    {Require();current.Require(Company,ActorUserId,InvoiceId);if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(Review))throw new InvalidOperationException("The complete displayed invoice review changed before disposition.");}
}
public sealed record CompanySalesInvoiceDecisionProof(SALES_INVOICE_REVIEW_INTENT Intent,DateTime DecidedAtUtc,string RowRevision)
{
    public const int MaximumLength=67108864;
    private static string Revision(INVOICE row){var copy=CompanyInventorySourceReview.Copy(row);copy.REVIEW_WORKFLOW_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static string Create(SALES_INVOICE_REVIEW_INTENT intent,INVOICE row)
    {
        var company=new CompanyScope(intent.OWNER_TENANT_ID,intent.OWNER_COMPANY_BA_ID);var command=intent.Restore(company,intent.ROW_CREATED_BY);
        if(row.ROW_CHANGED_DATE is not {} at)throw new InvalidOperationException("A native UTC reviewer audit is required.");
        var json=JsonSerializer.Serialize(new CompanySalesInvoiceDecisionProof(CompanyInventorySourceReview.Copy(intent),at,Revision(row)));
        if(json.Length>MaximumLength)throw new InvalidOperationException("The complete invoice disposition proof exceeds its native bound.");return json;
    }
    public static CompanySalesInvoiceDecisionProof Require(CompanyScope company,INVOICE row)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();
        PurchaseOrderOwnership.RequireScope(company);
        if(string.IsNullOrWhiteSpace(row.REVIEW_WORKFLOW_JSON) || row.REVIEW_WORKFLOW_JSON.Length>MaximumLength)throw new InvalidOperationException("The complete native independent invoice disposition proof is required.");
        CompanySalesInvoiceDecisionProof proof;
        try{proof=JsonSerializer.Deserialize<CompanySalesInvoiceDecisionProof>(row.REVIEW_WORKFLOW_JSON,new JsonSerializerOptions{UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow})??throw new InvalidOperationException("Invoice disposition cannot be null.");}
        catch(JsonException){throw new InvalidOperationException("Invoice disposition proof is malformed.");}
        var key=CompanySalesInvoiceValidation.Key("decision-row",row,company);if(CompanySalesInvoiceValidation.Contains(key))return proof;
        if(proof.Intent is null)throw new InvalidOperationException("The original reviewer journal is required.");
        var command=proof.Intent.Restore(company,proof.Intent.ROW_CREATED_BY);var original=command.Review.Original.Invoice;
        var normalized=CompanyInventorySourceReview.Copy(row);normalized.REVIEW_WORKFLOW_JSON=null;normalized.STATUS=original.STATUS;normalized.ROW_CHANGED_BY=original.ROW_CHANGED_BY;normalized.ROW_CHANGED_DATE=original.ROW_CHANGED_DATE;
        if(row.STATUS!=(command.Decision=="APPROVE"?"APPROVED":"REJECTED") || row.ROW_CHANGED_BY!=command.ActorUserId || row.ROW_CHANGED_DATE!=proof.DecidedAtUtc ||
            proof.DecidedAtUtc==default || proof.DecidedAtUtc.Kind!=DateTimeKind.Utc || proof.DecidedAtUtc<proof.Intent.ROW_CREATED_DATE || proof.RowRevision!=Revision(row) ||
            JsonSerializer.Serialize(normalized)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The independent reviewer audit or original immutable invoice evidence is unconfirmed.");
        CompanySalesInvoiceValidation.Remember(key);return CompanyInventorySourceReview.Copy(proof);
    }
}
