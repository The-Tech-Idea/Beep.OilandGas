using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanySalesReceivableReviewSource(CompanySalesReceivableOutcome Original,CompanySalesInvoiceReviewSource CurrentInvoice,IReadOnlyList<CompanySalesReceivableContributor> Receivables,string Revision)
{
    [JsonIgnore]public CompanyScope Company=>CurrentInvoice.Current.Company;
    [JsonIgnore]public string ActorUserId=>CurrentInvoice.Current.ActorUserId;
    [JsonIgnore]public string ReceivableId=>Original.Receivable.AR_INVOICE_ID;
    [JsonIgnore]public string InvoiceId=>CurrentInvoice.InvoiceId;
    [JsonIgnore]public string FieldId=>CurrentInvoice.Current.FieldId;
    [JsonIgnore]public string BindingRevision=>CurrentInvoice.Current.BindingRevision;
    [JsonIgnore]public bool IsIndependent=>ActorUserId!=Original.Intent.ROW_CREATED_BY;
    [JsonIgnore]public bool HasCurrentOriginalSource {get{Require(Company,ActorUserId,ReceivableId);try{RequireOriginalSource();return true;}catch(InvalidOperationException){return false;}}}
    [JsonIgnore]public bool CanReview=>Original.Receivable.STATUS=="DRAFT" && IsIndependent && HasCurrentOriginalSource;
    public static CompanySalesReceivableReviewSource Create(CompanySalesReceivableOutcome original,CompanySalesInvoiceReviewSource invoice,IEnumerable<AR_INVOICE> receivables)
    {
        var result=new CompanySalesReceivableReviewSource(CompanyInventorySourceReview.Copy(original),CompanyInventorySourceReview.Copy(invoice),receivables.Select(row=>CompanySalesReceivableContributor.Create(invoice.Current.Company,row)).OrderBy(row=>row.ReceivableId,StringComparer.Ordinal).ToArray(),string.Empty);
        result=result with{Revision=Digest(result)};result.Require(result.Company,result.ActorUserId,result.ReceivableId);return result;
    }
    public void Require(CompanyScope company,string actor,string receivable)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("receivable-review",this,new{company,actor,receivable});if(CompanySalesInvoiceValidation.Contains(key))return;
        if(Original?.Intent is null || Original.Receivable is null || CurrentInvoice is null)throw new InvalidOperationException("Complete original receivable and current invoice evidence are required.");
        var command=Original.Intent.Restore(company,Original.Intent.ROW_CREATED_BY);Original.Require(command);CurrentInvoice.Require(company,actor,command.Source.InvoiceId);
        if(ReceivableId!=receivable || Original.Receivable.FIELD_ID!=FieldId || Receivables is not {Count:1} || JsonSerializer.Serialize(Receivables[0])!=JsonSerializer.Serialize(CompanySalesReceivableContributor.Create(company,Original.Receivable)) || Revision!=Digest(this))throw new InvalidOperationException("The complete current owned receivable review is unconfirmed.");
        CompanySalesInvoiceValidation.Remember(key);
    }
    public void RequireIndependentCurrent(){Require(Company,ActorUserId,ReceivableId);if(!IsIndependent)throw new InvalidOperationException("The receivable maker cannot independently review their draft.");RequireOriginalSource();}
    private void RequireOriginalSource()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("receivable-original-source",this);if(CompanySalesInvoiceValidation.Contains(key))return;
        var command=Original.Intent.Restore(Company,Original.Intent.ROW_CREATED_BY);
        command.RequireCurrent(CompanySalesReceivableSource.Create(CurrentInvoice.AsReader(command.Source.ActorUserId),[]));
        CompanySalesInvoiceValidation.Remember(key);
    }
    private static string Digest(CompanySalesReceivableReviewSource value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value with{Revision=string.Empty}));
}

/// <summary>Explicit independent financial/source disposition; no money, payment or ledger mutation.</summary>
public sealed record CompanySalesReceivableDecisionCommand(string IntentId,CompanySalesReceivableReviewSource Review,string Decision,string FinancialDisposition,string FinancialEvidence,string SourceDisposition,string SourceEvidence,string Reason)
{
    [JsonIgnore]public CompanyScope Company=>Review.Company;
    [JsonIgnore]public string ActorUserId=>Review.ActorUserId;
    [JsonIgnore]public string ReceivableId=>Review.ReceivableId;
    public void Require()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("receivable-decision",this);if(CompanySalesInvoiceValidation.Contains(key))return;
        BillingPolicyValues.Intent(IntentId);if(Review is null)throw new InvalidOperationException("The displayed independent receivable review is required.");Review.RequireIndependentCurrent();
        if(Review.Original.Receivable.STATUS!="DRAFT" || Review.Original.Receivable.REVIEW_WORKFLOW_JSON is not null || IntentId==ReceivableId || IntentId==Review.InvoiceId || IntentId==Review.Original.Intent.SALES_RECEIVABLE_INTENT_ID || Decision is not ("APPROVE" or "REJECT"))throw new InvalidOperationException("One explicit independent receivable draft disposition is required.");
        foreach(var disposition in new[]{FinancialDisposition,SourceDisposition})if(disposition is not ("CONFIRMED" or "ISSUE"))throw new InvalidOperationException("Explicit financial and source dispositions are required.");
        BillingPolicyValues.Text(FinancialEvidence,2000);BillingPolicyValues.Text(SourceEvidence,2000);BillingPolicyValues.Text(Reason,1000);
        if(Decision=="APPROVE" && (FinancialDisposition!="CONFIRMED" || SourceDisposition!="CONFIRMED"))throw new InvalidOperationException("Approval requires confirmed financial and source evidence.");
        if(JsonSerializer.Serialize(this).Length>SALES_RECEIVABLE_REVIEW_INTENT.MaximumCommandLength)throw new InvalidOperationException("The complete reviewer request exceeds its journal bound.");CompanySalesInvoiceValidation.Remember(key);
    }
    public void RequireCurrent(CompanySalesReceivableReviewSource current){Require();current.Require(Company,ActorUserId,ReceivableId);if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(Review))throw new InvalidOperationException("The complete displayed receivable review changed before disposition.");}
}

public sealed record CompanySalesReceivableDecisionProof(SALES_RECEIVABLE_REVIEW_INTENT Intent,DateTime DecidedAtUtc,string RowRevision)
{
    public const int MaximumLength=268435456;
    private static string Revision(AR_INVOICE row){var copy=CompanyInventorySourceReview.Copy(row);copy.REVIEW_WORKFLOW_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static string Create(SALES_RECEIVABLE_REVIEW_INTENT intent,AR_INVOICE row)
    {
        _=intent.Restore(new(intent.OWNER_TENANT_ID,intent.OWNER_COMPANY_BA_ID),intent.ROW_CREATED_BY);
        if(row.ROW_CHANGED_DATE is not {} at)throw new InvalidOperationException("Native UTC reviewer audit is required.");
        var json=JsonSerializer.Serialize(new CompanySalesReceivableDecisionProof(CompanyInventorySourceReview.Copy(intent),at,Revision(row)));if(json.Length>MaximumLength)throw new InvalidOperationException("The complete native receivable review proof exceeds its bound.");return json;
    }
    public static CompanySalesReceivableDecisionProof Require(CompanyScope company,AR_INVOICE row)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();PurchaseOrderOwnership.RequireScope(company);
        if(string.IsNullOrWhiteSpace(row.REVIEW_WORKFLOW_JSON) || row.REVIEW_WORKFLOW_JSON.Length>MaximumLength)throw new InvalidOperationException("The complete native independent receivable disposition proof is required.");
        CompanySalesReceivableDecisionProof proof;
        try{proof=JsonSerializer.Deserialize<CompanySalesReceivableDecisionProof>(row.REVIEW_WORKFLOW_JSON,new JsonSerializerOptions{UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow})??throw new InvalidOperationException("Receivable disposition cannot be null.");}
        catch(JsonException){throw new InvalidOperationException("Receivable disposition proof is malformed.");}
        var key=CompanySalesInvoiceValidation.Key("receivable-decision-row",row,company);if(CompanySalesInvoiceValidation.Contains(key))return proof;
        if(proof.Intent is null)throw new InvalidOperationException("The original reviewer journal is required.");var command=proof.Intent.Restore(company,proof.Intent.ROW_CREATED_BY);var original=command.Review.Original.Receivable;
        var normalized=CompanyInventorySourceReview.Copy(row);normalized.REVIEW_WORKFLOW_JSON=null;normalized.STATUS=original.STATUS;normalized.ROW_CHANGED_BY=original.ROW_CHANGED_BY;normalized.ROW_CHANGED_DATE=original.ROW_CHANGED_DATE;
        if(row.STATUS!=(command.Decision=="APPROVE"?"APPROVED":"REJECTED") || row.ROW_CHANGED_BY!=command.ActorUserId || row.ROW_CHANGED_DATE?.Kind!=DateTimeKind.Utc || row.ROW_CHANGED_DATE!=proof.DecidedAtUtc || proof.DecidedAtUtc==default || proof.DecidedAtUtc.Kind!=DateTimeKind.Utc || proof.DecidedAtUtc<proof.Intent.ROW_CREATED_DATE || proof.RowRevision!=Revision(row) || JsonSerializer.Serialize(normalized)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Independent reviewer audit or original immutable receivable facts disagree.");
        CompanySalesInvoiceValidation.Remember(key);return CompanyInventorySourceReview.Copy(proof);
    }
    public static AR_INVOICE OriginalDraft(CompanyScope company,AR_INVOICE row)
    {
        if(row.REVIEW_WORKFLOW_JSON is null){_=CompanySalesReceivableProof.Require(company,row);return CompanyInventorySourceReview.Copy(row);}
        var proof=Require(company,row);return CompanyInventorySourceReview.Copy(proof.Intent.Restore(company,proof.Intent.ROW_CREATED_BY).Review.Original.Receivable);
    }
}
