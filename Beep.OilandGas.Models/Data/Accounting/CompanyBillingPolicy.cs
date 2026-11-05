using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Explicit independently reviewed sales-invoice draft rules; AP matching approvals do not grant these rules.</summary>
public sealed record CompanyBillingPolicyRules(string Version,string CurrencyCode,int CurrencyDecimals,string RoundingMode,
    DateTime EffectiveFrom,DateTime? EffectiveThrough,string TaxRule,string ChargesRule,string RightsRule)
{
    public void Require()
    {
        BillingPolicyValues.Text(Version,128);BillingPolicyValues.Text(CurrencyCode,16);
        if(CurrencyDecimals is <0 or >8 || RoundingMode is not ("TO_EVEN" or "AWAY_FROM_ZERO" or "EXACT_ONLY") ||
            EffectiveFrom==default || EffectiveFrom.Kind!=DateTimeKind.Unspecified || EffectiveFrom.TimeOfDay!=TimeSpan.Zero ||
            EffectiveThrough is {} end && (end.Kind!=DateTimeKind.Unspecified || end.TimeOfDay!=TimeSpan.Zero || end<EffectiveFrom) ||
            TaxRule!="EXPLICIT_INVOICE_TAX_REVIEW" || ChargesRule!="EXPLICIT_INVOICE_CHARGES_REVIEW" || RightsRule!="CONTRACT_ENTITLEMENT_REVIEW")
            throw new InvalidOperationException("Explicit currency precision/rounding, stored calendar dates and independent tax/charges/rights review rules are required.");
    }
    public decimal RoundPriceExtension(decimal value)
    {
        Require();if(value<=0)throw new InvalidOperationException("A positive source price extension is required.");
        var rounded=decimal.Round(value,CurrencyDecimals,RoundingMode=="AWAY_FROM_ZERO"?MidpointRounding.AwayFromZero:MidpointRounding.ToEven);
        if(rounded<=0 || RoundingMode=="EXACT_ONLY" && rounded!=value)throw new InvalidOperationException("The source price extension does not satisfy the explicit currency rounding policy.");return rounded;
    }
    public void RequireExactAmount(decimal value)
    {Require();if(value<0 || decimal.Round(value,CurrencyDecimals,MidpointRounding.ToEven)!=value)throw new InvalidOperationException("An explicit nonnegative amount in the reviewed currency precision is required.");}
}
public sealed record CompanyBillingPolicyRecord(string PrepareIntentId,CompanyScope Company,string PolicyId,string PreparedBy,DateTime PreparedAtUtc,
    CompanyBillingPolicyRules Rules,string PreparationReason,string? ApprovalIntentId,string? ApprovedBy,DateTime? ApprovedAtUtc,string? ApprovalReason)
{
    public void Require()
    {
        PurchaseOrderOwnership.RequireScope(Company);BillingPolicyValues.Intent(PrepareIntentId);BillingPolicyValues.Text(PolicyId,128);BillingPolicyValues.Text(PreparedBy,128);BillingPolicyValues.Text(PreparationReason,1000);
        if(Rules is null || PreparedAtUtc==default || PreparedAtUtc.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("The complete recorded billing policy preparation is required.");Rules.Require();
        if(ApprovedBy is null){if(ApprovalIntentId is not null || ApprovedAtUtc is not null || ApprovalReason is not null)throw new InvalidOperationException("Billing policy approval audit is incomplete.");return;}
        BillingPolicyValues.Intent(ApprovalIntentId!);BillingPolicyValues.Text(ApprovedBy,128);BillingPolicyValues.Text(ApprovalReason!,1000);
        if(ApprovedBy==PreparedBy || ApprovalIntentId==PrepareIntentId || ApprovedAtUtc is not {} at || at.Kind!=DateTimeKind.Utc || at<PreparedAtUtc)throw new InvalidOperationException("Independent billing policy approval and ordered original intent/audit are required.");
    }
}
public sealed record CompanyBillingPolicyProof(CompanyBillingPolicyRecord Record,CompanyBillingPolicyDraft Draft,
    CompanyBillingPolicyApproval? Approval,ACCOUNTING_METHOD? Original,string RowRevision)
{
    public const string MethodType="SALES_BILLING";
    public const int MaximumLength=24000;
    public static string RowRevisionFor(ACCOUNTING_METHOD row)
    {var copy=CompanyInventorySourceReview.Copy(row);copy.MATCHING_POLICY_COMMAND_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static string Create(CompanyBillingPolicyRecord record,CompanyBillingPolicyDraft draft,CompanyBillingPolicyApproval? approval,ACCOUNTING_METHOD? original,ACCOUNTING_METHOD row)
    {record.Require();var json=JsonSerializer.Serialize(new CompanyBillingPolicyProof(record,draft,approval,original,RowRevisionFor(row)));if(json.Length>MaximumLength)throw new InvalidOperationException("The complete billing policy proof exceeds its native bound.");return json;}
    public static CompanyBillingPolicyProof Require(CompanyScope company,ACCOUNTING_METHOD row)
    {
        PurchaseOrderOwnership.RequireScope(company);
        if(string.IsNullOrWhiteSpace(row.MATCHING_POLICY_COMMAND_JSON) || row.MATCHING_POLICY_COMMAND_JSON.Length>MaximumLength)throw new InvalidOperationException("The complete native billing policy proof is required.");
        CompanyBillingPolicyProof proof;
        try{proof=JsonSerializer.Deserialize<CompanyBillingPolicyProof>(row.MATCHING_POLICY_COMMAND_JSON,new JsonSerializerOptions{UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow}) ??throw new InvalidOperationException("Billing policy proof cannot be null.");}catch(JsonException){throw new InvalidOperationException("Billing policy proof is malformed.");}
        if(proof.Record is null || proof.Draft is null)throw new InvalidOperationException("The original billing policy record and command are required.");var record=proof.Record;record.Require();
        var draft=proof.Draft;draft.RequireValues();
        if(draft.Company!=company || draft.IntentId!=record.PrepareIntentId || draft.PolicyId!=record.PolicyId || draft.ActorUserId!=record.PreparedBy || draft.Rules!=record.Rules || draft.Reason!=record.PreparationReason)
            throw new InvalidOperationException("The original preparation command differs from the recorded billing policy.");
        if(record.ApprovedBy is null)
        {if(proof.Approval is not null || proof.Original is not null)throw new InvalidOperationException("An unapproved draft cannot contain approval evidence.");}
        else
        {
            var approval=proof.Approval ??throw new InvalidOperationException("The original independent approval command is required.");approval.RequireValues();
            var original=proof.Original ??throw new InvalidOperationException("The exact original billing policy draft is required.");
            // Approval may retain one draft, never another approval or a recursive history.
            if(original.MATCHING_POLICY_STATUS!="DRAFT")throw new InvalidOperationException("Only the original draft may be retained by approval.");
            var prepared=Require(company,original);
            if(prepared.Draft!=draft || prepared.Record!=(record with{ApprovalIntentId=null,ApprovedBy=null,ApprovedAtUtc=null,ApprovalReason=null}) ||
                approval.Company!=company || approval.ActorUserId!=record.ApprovedBy || approval.PolicyId!=record.PolicyId || approval.IntentId!=record.ApprovalIntentId || approval.Reason!=record.ApprovalReason || approval.OriginalRowRevision!=prepared.RowRevision)
                throw new InvalidOperationException("Independent approval differs from its exact original draft and command.");
            var expected=CompanyInventorySourceReview.Copy(original);expected.MATCHING_POLICY_STATUS="APPROVED";expected.ROW_CHANGED_BY=row.ROW_CHANGED_BY;expected.ROW_CHANGED_DATE=row.ROW_CHANGED_DATE;
            if(RowRevisionFor(expected)!=RowRevisionFor(row))throw new InvalidOperationException("Billing approval cannot change the prepared rules, metadata or audit history.");
        }
        if(record.Company!=company || row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.ACCOUNTING_METHOD_ID!=record.PolicyId ||
            row.METHOD_TYPE!=MethodType || row.ACTIVE_IND!="Y" || !string.IsNullOrEmpty(row.FIELD_ID) || !string.IsNullOrEmpty(row.COST_CENTER_ID) ||
            row.MATCHING_POLICY_STATUS!=(record.ApprovedBy is null?"DRAFT":"APPROVED") || row.MATCHING_POLICY_JSON!=JsonSerializer.Serialize(record.Rules) ||
            row.EFFECTIVE_DATE!=record.Rules.EffectiveFrom || row.EFFECTIVE_DATE?.Kind!=DateTimeKind.Unspecified || row.EXPIRY_DATE!=record.Rules.EffectiveThrough || row.EXPIRY_DATE is {} expiry && expiry.Kind!=DateTimeKind.Unspecified || row.PPDM_GUID!=record.PrepareIntentId ||
            row.ROW_CREATED_BY!=record.PreparedBy || row.ROW_CREATED_DATE!=record.PreparedAtUtc || row.ROW_CREATED_DATE?.Kind!=DateTimeKind.Utc || row.ROW_CHANGED_BY!=(record.ApprovedBy ??record.PreparedBy) ||
            row.ROW_CHANGED_DATE is not {} changed || changed.Kind!=DateTimeKind.Utc || changed<record.PreparedAtUtc || record.ApprovedBy is not null && changed!=record.ApprovedAtUtc || row.REMARK!=record.PreparationReason || row.DESCRIPTION!=record.Rules.Version || row.SOURCE!=MethodType || proof.RowRevision!=RowRevisionFor(row))
            throw new InvalidOperationException("The complete native company billing policy, original rules and independent audit disagree.");return proof;
    }
}
public sealed record CompanyBillingPolicyReview(CompanyScope Company,string ActorUserId,IReadOnlyList<ACCOUNTING_METHOD> Records,string Revision)
{
    public const int MaximumRecords=32;
    public static CompanyBillingPolicyReview Create(CompanyScope company,string actor,IReadOnlyList<ACCOUNTING_METHOD> rows)
    {if(rows is null || rows.Count>MaximumRecords || rows.Any(row=>row is null))throw new InvalidOperationException("The complete bounded billing policy history is required.");var records=rows.OrderBy(row=>row.ACCOUNTING_METHOD_ID,StringComparer.Ordinal).Select(CompanyInventorySourceReview.Copy).ToArray();var result=new CompanyBillingPolicyReview(company,actor,records,Digest(company,records));result.Require(company,actor);return result;}
    public void Require(CompanyScope company,string actor)
    {
        PurchaseOrderOwnership.RequireScope(company);BillingPolicyValues.Text(actor,128);
        if(Company!=company || ActorUserId!=actor || Records is null || Records.Count>MaximumRecords || Records.Any(row=>row is null) || Records.Select(row=>row.ACCOUNTING_METHOD_ID).Distinct(StringComparer.Ordinal).Count()!=Records.Count || Revision!=Digest(company,Records))throw new InvalidOperationException("The complete unique owned billing policy history is required.");
        foreach(var row in Records){_=CompanyBillingPolicyProof.Require(company,row);}
        var approved=Records.Where(row=>row.MATCHING_POLICY_STATUS=="APPROVED").Select(row=>CompanyBillingPolicyProof.Require(company,row).Record.Rules).ToArray();
        for(var i=0;i<approved.Length;++i)for(var j=i+1;j<approved.Length;++j)if(approved[i].CurrencyCode==approved[j].CurrencyCode && approved[i].EffectiveFrom<=(approved[j].EffectiveThrough ??DateTime.MaxValue) && approved[j].EffectiveFrom<=(approved[i].EffectiveThrough ??DateTime.MaxValue))throw new InvalidOperationException("Approved billing policy windows overlap for this currency.");
    }
    public ACCOUNTING_METHOD RequireApplicable(string currency,DateTime invoiceDate)
        => FindApplicable(currency,invoiceDate) ?? throw new InvalidOperationException("One independently approved applicable company sales-billing policy is required.");
    public ACCOUNTING_METHOD? FindApplicable(string currency,DateTime invoiceDate)
    {
        Require(Company,ActorUserId);if(invoiceDate==default || invoiceDate.Kind!=DateTimeKind.Unspecified || invoiceDate.TimeOfDay!=TimeSpan.Zero)throw new InvalidOperationException("An explicit stored invoice calendar date is required.");
        var rows=Records.Where(row=>{var record=CompanyBillingPolicyProof.Require(Company,row).Record;return record.ApprovedBy is not null && record.Rules.CurrencyCode==currency && record.Rules.EffectiveFrom<=invoiceDate && (record.Rules.EffectiveThrough is null || record.Rules.EffectiveThrough>=invoiceDate);}).ToArray();
        return rows.Length==1?CompanyInventorySourceReview.Copy(rows[0]):null;
    }
    private static string Digest(CompanyScope company,IReadOnlyList<ACCOUNTING_METHOD> rows)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{company,Records=rows.OrderBy(row=>row.ACCOUNTING_METHOD_ID,StringComparer.Ordinal)}));
}
public sealed record CompanyBillingPolicyDraft(string IntentId,CompanyScope Company,string ActorUserId,string PolicyId,CompanyBillingPolicyRules Rules,string Reason,string HistoryRevision,string BindingRevision)
{
    public void RequireValues()
    {PurchaseOrderOwnership.RequireScope(Company);BillingPolicyValues.Text(ActorUserId,128);BillingPolicyValues.Intent(IntentId);BillingPolicyValues.Text(PolicyId,128);BillingPolicyValues.Text(Reason,1000);BillingPolicyValues.Revision(HistoryRevision);BillingPolicyValues.Revision(BindingRevision);if(Rules is null)throw new InvalidOperationException("Explicit billing policy rules are required.");Rules.Require();}
    public void Require(CompanyBillingPolicyReview history)
    {RequireValues();history.Require(Company,ActorUserId);if(HistoryRevision!=history.Revision || history.Records.Any(row=>row.ACCOUNTING_METHOD_ID==PolicyId))throw new InvalidOperationException("The exact original policy history and unused billing policy identity are required.");}
}
public sealed record CompanyBillingPolicyApproval(string IntentId,CompanyScope Company,string ActorUserId,string PolicyId,string OriginalRowRevision,string Reason,string HistoryRevision,string BindingRevision)
{
    public void RequireValues()
    {PurchaseOrderOwnership.RequireScope(Company);BillingPolicyValues.Text(ActorUserId,128);BillingPolicyValues.Intent(IntentId);BillingPolicyValues.Text(PolicyId,128);BillingPolicyValues.Text(Reason,1000);BillingPolicyValues.Revision(HistoryRevision);BillingPolicyValues.Revision(OriginalRowRevision);BillingPolicyValues.Revision(BindingRevision);}
    public void Require(CompanyBillingPolicyReview history)
    {
        RequireValues();history.Require(Company,ActorUserId);
        if(HistoryRevision!=history.Revision)throw new InvalidOperationException("The reviewed billing policy history changed.");var row=history.Records.SingleOrDefault(row=>row.ACCOUNTING_METHOD_ID==PolicyId) ??throw new InvalidOperationException("The original billing policy draft is unavailable.");var proof=CompanyBillingPolicyProof.Require(Company,row);
        if(proof.Record.ApprovedBy is not null || proof.Record.PreparedBy==ActorUserId || proof.Record.PrepareIntentId==IntentId || OriginalRowRevision!=proof.RowRevision)throw new InvalidOperationException("An unchanged policy draft and independent reviewer are required.");
    }
}
public sealed record CompanyBillingPolicyCommandReview(CompanyBillingPolicyReview History,string Action,string BindingRevision)
{
    public void Require(CompanyScope company,string actor,string action)
    {if(History is null || Action!=action || action is not ("DRAFT" or "APPROVE"))throw new InvalidOperationException("The exact billing policy action and history are required.");BillingPolicyValues.Revision(BindingRevision);History.Require(company,actor);}
    public void Require(CompanyBillingPolicyDraft command)
    {Require(command.Company,command.ActorUserId,"DRAFT");if(command.BindingRevision!=BindingRevision)throw new InvalidOperationException("The original production binding changed.");command.Require(History);}
    public void Require(CompanyBillingPolicyApproval command)
    {Require(command.Company,command.ActorUserId,"APPROVE");if(command.BindingRevision!=BindingRevision)throw new InvalidOperationException("The original production binding changed.");command.Require(History);}
}
public sealed record CompanyBillingPolicyDraftSubmission(CompanyBillingPolicyDraft Command,CompanyBillingPolicyCommandReview Review);
public sealed record CompanyBillingPolicyApprovalSubmission(CompanyBillingPolicyApproval Command,CompanyBillingPolicyCommandReview Review);
public sealed record CompanyBillingPolicyResult(ACCOUNTING_METHOD Policy)
{
    public void Require(CompanyBillingPolicyDraftSubmission submission)
    {submission.Review.Require(submission.Command);if(Policy is null || CompanyBillingPolicyProof.Require(submission.Command.Company,Policy).Draft!=submission.Command)throw new InvalidOperationException("The exact original billing preparation outcome is unconfirmed.");}
    public void Require(CompanyBillingPolicyApprovalSubmission submission)
    {submission.Review.Require(submission.Command);if(Policy is null || CompanyBillingPolicyProof.Require(submission.Command.Company,Policy).Approval!=submission.Command)throw new InvalidOperationException("The exact original independent billing approval outcome is unconfirmed.");}
}
public static class BillingPolicyValues
{
    public static void Text(string value,int bound){if(string.IsNullOrWhiteSpace(value) || value.Length>bound || value.Any(char.IsControl))throw new InvalidOperationException("An explicit bounded billing value is required.");}
    public static void Intent(string value){if(!Guid.TryParseExact(value,"N",out var id) || id==Guid.Empty || value!=id.ToString("N"))throw new InvalidOperationException("An original billing intent identity is required.");}
    public static void Revision(string value){if(value is null || value.Length!=64 || value.Any(character=>!char.IsAsciiHexDigit(character)))throw new InvalidOperationException("The complete original billing revision is required.");}
}
