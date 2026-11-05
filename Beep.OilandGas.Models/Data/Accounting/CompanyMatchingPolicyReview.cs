using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Explicit recorded matching configuration. Recording rules does not independently approve them.</summary>
public sealed record CompanyMatchingPolicyRules(string PolicyVersion,string CurrencyCode,decimal? QuantityTolerancePercent,decimal? PriceTolerancePercent,
    string UnitRule,string CurrencyRule,string TaxTreatment,string RoundingRule,string NonPurchaseInvoiceRule)
{
    public void Require()
    {
        static bool Text(string? value,int bound)=>!string.IsNullOrWhiteSpace(value) && value.Length<=bound && !value.Any(char.IsControl);
        if(!Text(PolicyVersion,128) || !Text(CurrencyCode,32) || QuantityTolerancePercent is not (>=0 and <=100) || PriceTolerancePercent is not (>=0 and <=100) ||
            UnitRule!="EXACT_RECORDED_UNITS" || CurrencyRule!="EXACT_RECORDED_CURRENCY" || TaxTreatment!="SEPARATE_REVIEW_REQUIRED" ||
            RoundingRule!="EXACT_SOURCE_AMOUNTS" || NonPurchaseInvoiceRule!="SEPARATE_REVIEW_REQUIRED")
            throw new InvalidOperationException("Version, currency, explicit bounded tolerances and supported unit/currency/tax/rounding/non-PO rules are required. No defaults or conversion policy are inferred.");
    }
}
/// <summary>All company-owned matching policy records, including history; no latest-row or approval inference.</summary>
public sealed record CompanyMatchingPolicyReview(CompanyScope Company,string ActorUserId,IReadOnlyList<ACCOUNTING_METHOD> Records,string Revision,DateTimeOffset RetrievedAtUtc)
{
    public const string MethodType="AP_MATCHING";
    public static CompanyMatchingPolicyReview Create(CompanyScope company,string actor,IReadOnlyList<ACCOUNTING_METHOD> rows)
    {
        var frozen=PurchaseReceiptDecisionProof.Copy(rows.OrderBy(row=>row.ACCOUNTING_METHOD_ID,StringComparer.Ordinal).ToArray());
        var review=new CompanyMatchingPolicyReview(company,actor,Array.AsReadOnly(frozen),Digest(company,frozen),DateTimeOffset.UtcNow);review.Require(company,actor);return review;
    }
    public static CompanyMatchingPolicyRules Rules(ACCOUNTING_METHOD row)
    {
        if(string.IsNullOrWhiteSpace(row.MATCHING_POLICY_JSON) || row.MATCHING_POLICY_JSON.Length>4000)throw new InvalidOperationException("The recorded matching rules are unavailable or exceed their bound.");
        CompanyMatchingPolicyRules? rules;
        try{rules=JsonSerializer.Deserialize<CompanyMatchingPolicyRules>(row.MATCHING_POLICY_JSON,new JsonSerializerOptions{UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow});}
        catch(JsonException){throw new InvalidOperationException("The recorded matching rules are malformed.");}
        if(rules is null)throw new InvalidOperationException("Matching rules cannot be null.");rules.Require();return rules;
    }
    public void Require(CompanyScope company,string actor)
    {
        PurchaseOrderOwnership.RequireScope(company);
        if(Company!=company || string.IsNullOrWhiteSpace(actor) || ActorUserId!=actor || actor.Length>128 || actor.Any(char.IsControl) || Records is null ||
            Records.Any(row=>row is null) || Records.Select(row=>row.ACCOUNTING_METHOD_ID).Distinct(StringComparer.Ordinal).Count()!=Records.Count)
            throw new InvalidOperationException("Original company/account and distinct policy identities are required.");
        foreach(var row in Records)
        {
            if(row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.METHOD_TYPE!=MethodType ||
                string.IsNullOrWhiteSpace(row.ACCOUNTING_METHOD_ID) || row.ACCOUNTING_METHOD_ID.Length>128 || row.ACCOUNTING_METHOD_ID.Any(char.IsControl) ||
                !string.IsNullOrEmpty(row.FIELD_ID) || !string.IsNullOrEmpty(row.COST_CENTER_ID) || row.ACTIVE_IND is not ("Y" or "N") ||
                row.MATCHING_POLICY_STATUS is not ("DRAFT" or "APPROVED" or "RETIRED") || row.EFFECTIVE_DATE is not {} effective || effective==default || effective.Kind!=DateTimeKind.Utc ||
                row.EXPIRY_DATE is {} expiry && (expiry.Kind!=DateTimeKind.Utc || expiry<effective))
                throw new InvalidOperationException("The matching record has unconfirmed company ownership, scope, status or effective dates.");
            _=Rules(row);
        }
        if(!ProcessDefinitionRevision.IsValid(Revision) || Revision!=Digest(company,Records) || RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The matching policy content or provenance changed.");
    }
    private static string Digest(CompanyScope company,IReadOnlyList<ACCOUNTING_METHOD> rows)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{company,Records=rows.OrderBy(row=>row.ACCOUNTING_METHOD_ID,StringComparer.Ordinal)}));
}
