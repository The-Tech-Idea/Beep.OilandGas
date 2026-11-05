using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;
public sealed record CompanyMatchingPolicyCommand(CompanyScope Company,string ActorUserId,string PolicyId,string Action,
    CompanyMatchingPolicyRules Rules,DateTime EffectiveAtUtc,DateTime? ExpiresAtUtc,string ExpectedHistoryRevision,string BindingRevision,string IntentId,string Reason)
{
    public void Require(CompanyMatchingPolicyReview review)
    {
        review.Require(Company,ActorUserId);RequireValues();
        if(review.Revision!=ExpectedHistoryRevision)throw new InvalidOperationException("The original policy history changed.");
        var row=review.Records.SingleOrDefault(item=>item.ACCOUNTING_METHOD_ID==PolicyId);
        if(Action=="DRAFT")
        {if(row is not null)throw new InvalidOperationException("The proposed policy identity is already recorded.");return;}
        if(row is null || row.MATCHING_POLICY_STATUS!="DRAFT" || row.ACTIVE_IND!="Y")throw new InvalidOperationException("An active recorded draft is required for independent approval.");
        var prepared=CompanyMatchingPolicyProof.Require(Company,row);
        if(prepared.Command.Action!="DRAFT" || prepared.Command.ActorUserId==ActorUserId ||
            JsonSerializer.Serialize(CompanyMatchingPolicyReview.Rules(row))!=JsonSerializer.Serialize(Rules) || row.EFFECTIVE_DATE!=EffectiveAtUtc || row.EXPIRY_DATE!=ExpiresAtUtc)
            throw new InvalidOperationException("Approval requires unchanged rules/dates and an account independent of the verified draft preparer.");
        if(review.Records.Any(item=>item.ACCOUNTING_METHOD_ID!=PolicyId && item.ACTIVE_IND=="Y" && item.MATCHING_POLICY_STATUS=="APPROVED" &&
            CompanyMatchingPolicyReview.Rules(item).CurrencyCode==Rules.CurrencyCode && item.EFFECTIVE_DATE<= (ExpiresAtUtc ?? DateTime.MaxValue) && (item.EXPIRY_DATE ?? DateTime.MaxValue)>=EffectiveAtUtc))
            throw new InvalidOperationException("Another recorded approved policy overlaps this currency/effective window. Resolve policy history independently before approval.");
    }
    public void RequireValues()
    {
        PurchaseOrderOwnership.RequireScope(Company);if(Rules is null)throw new InvalidOperationException("Explicit matching rules are required.");Rules.Require();
        static bool Text(string? value,int size)=>!string.IsNullOrWhiteSpace(value) && value.Length<=size && !value.Any(char.IsControl);
        if(!Text(ActorUserId,128) || !Text(PolicyId,128) || !Text(Reason,1000) || Action is not ("DRAFT" or "APPROVE") ||
            EffectiveAtUtc==default || EffectiveAtUtc.Kind!=DateTimeKind.Utc || ExpiresAtUtc is {} expiry && (expiry.Kind!=DateTimeKind.Utc || expiry<EffectiveAtUtc) ||
            !ProcessDefinitionRevision.IsValid(ExpectedHistoryRevision) || !ProcessDefinitionRevision.IsValid(BindingRevision) || !Guid.TryParseExact(IntentId,"N",out var intent) || intent==Guid.Empty)
            throw new InvalidOperationException("Explicit original policy identity, action, rules, UTC dates, history/binding, intent and reason are required.");
    }
}
public sealed record CompanyMatchingPolicyProof(CompanyMatchingPolicyCommand Command,ACCOUNTING_METHOD? Original,string RowRevision)
{
    public const int MaximumLength=24000;
    public static string Create(CompanyMatchingPolicyCommand command,ACCOUNTING_METHOD? original,ACCOUNTING_METHOD row)
    {
        var proof=new CompanyMatchingPolicyProof(command,original is null?null:PurchaseReceiptDecisionProof.Copy(original),Digest(row));
        var json=JsonSerializer.Serialize(proof);if(json.Length>MaximumLength)throw new InvalidOperationException("The policy command proof exceeds its storage bound.");
        return json;
    }
    public static CompanyMatchingPolicyProof Require(CompanyScope company,ACCOUNTING_METHOD row)
    {
        if(string.IsNullOrWhiteSpace(row.MATCHING_POLICY_COMMAND_JSON) || row.MATCHING_POLICY_COMMAND_JSON.Length>MaximumLength)throw new InvalidOperationException("Verified policy command proof is unavailable.");
        CompanyMatchingPolicyProof proof;
        try{proof=JsonSerializer.Deserialize<CompanyMatchingPolicyProof>(row.MATCHING_POLICY_COMMAND_JSON,new JsonSerializerOptions{UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow}) ?? throw new InvalidOperationException("Policy proof cannot be null.");}
        catch(JsonException){throw new InvalidOperationException("The recorded policy command proof is malformed.");}
        var command=proof.Command;if(command is null)throw new InvalidOperationException("The original policy command is required.");command.RequireValues();
        if(command.Company!=company || row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.ACCOUNTING_METHOD_ID!=command.PolicyId ||
            row.METHOD_TYPE!=CompanyMatchingPolicyReview.MethodType || row.ACTIVE_IND!="Y" || !string.IsNullOrEmpty(row.FIELD_ID) || !string.IsNullOrEmpty(row.COST_CENTER_ID) ||
            JsonSerializer.Serialize(CompanyMatchingPolicyReview.Rules(row))!=JsonSerializer.Serialize(command.Rules) || row.EFFECTIVE_DATE!=command.EffectiveAtUtc || row.EXPIRY_DATE!=command.ExpiresAtUtc ||
            proof.RowRevision!=Digest(row) || !ProcessDefinitionRevision.IsValid(proof.RowRevision) || row.ROW_CHANGED_BY!=command.ActorUserId || row.ROW_CHANGED_DATE is not {} changed || changed.Kind!=DateTimeKind.Utc || changed==default)
            throw new InvalidOperationException("The complete original policy command and recorded row do not agree.");
        if(command.Action=="DRAFT")
        {
            if(proof.Original is not null || row.MATCHING_POLICY_STATUS!="DRAFT" || row.ROW_CREATED_BY!=command.ActorUserId || row.ROW_CREATED_DATE is not {} created || created.Kind!=DateTimeKind.Utc || created==default || row.REMARK!=command.Reason)
                throw new InvalidOperationException("The original draft preparation is unconfirmed.");
        }
        else
        {
            if(proof.Original is null || row.MATCHING_POLICY_STATUS!="APPROVED")throw new InvalidOperationException("The original draft and final approval are required.");
            var prepared=Require(company,proof.Original);
            if(prepared.Command.Action!="DRAFT" || prepared.Command.ActorUserId==command.ActorUserId)throw new InvalidOperationException("The policy reviewer must be independent of its preparer.");
            var expected=PurchaseReceiptDecisionProof.Copy(proof.Original);expected.MATCHING_POLICY_STATUS="APPROVED";expected.ROW_CHANGED_BY=row.ROW_CHANGED_BY;expected.ROW_CHANGED_DATE=row.ROW_CHANGED_DATE;
            if(Digest(expected)!=Digest(row))throw new InvalidOperationException("Approval cannot change the prepared policy or its audit history.");
        }
        return proof;
    }
    private static string Digest(ACCOUNTING_METHOD row)
    {var copy=PurchaseReceiptDecisionProof.Copy(row);copy.MATCHING_POLICY_COMMAND_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
}
public sealed record CompanyMatchingPolicyCommandReview(CompanyMatchingPolicyReview History,string Action,string BindingRevision)
{
    public void Require(CompanyScope company,string actor,string action)
    {
        if(History is null || Action!=action || Action is not ("DRAFT" or "APPROVE") || !ProcessDefinitionRevision.IsValid(BindingRevision))throw new InvalidOperationException("Original policy action and production binding are required.");
        History.Require(company,actor);
    }
    public void Require(CompanyMatchingPolicyCommand command)
    {Require(command.Company,command.ActorUserId,command.Action);if(command.BindingRevision!=BindingRevision)throw new InvalidOperationException("The original policy binding changed.");command.Require(History);}
}
public sealed record CompanyMatchingPolicySubmission(CompanyMatchingPolicyCommand Command,CompanyMatchingPolicyCommandReview Review);
public sealed record CompanyMatchingPolicyResult(CompanyMatchingPolicyCommand Command,ACCOUNTING_METHOD Policy)
{
    public void Require(CompanyMatchingPolicySubmission submission)
    {
        submission.Review.Require(submission.Command);
        if(Command!=submission.Command || CompanyMatchingPolicyProof.Require(Command.Company,Policy).Command!=Command)throw new InvalidOperationException("The original matching-policy outcome is unconfirmed.");
    }
}
