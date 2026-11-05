using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Text.Json;
using Beep.OilandGas.Accounting.Models;
namespace Beep.OilandGas.Accounting.Models;

/// <summary>Explicit inclusive retirement date and optional adjacent replacement, bound to a complete reviewed history.</summary>
public sealed record CompanyGLMappingTransitionCommand(string IntentId,CompanyScope Company,string ActorUserId,string BookId,string CurrencyCode,string MappingId,
    DateTime EffectiveThrough,DateTime ReviewDate,string Reason,string SourceRevision,string BindingRevision,CompanyGLMappingConfigurationCommand? Replacement=null)
{
    public void Require()
    {
        BillingPolicyValues.Intent(IntentId);BillingPolicyValues.Intent(MappingId);PurchaseOrderOwnership.RequireScope(Company);
        foreach(var value in new[]{ActorUserId,BookId,Reason})BillingPolicyValues.Text(value,128);BillingPolicyValues.Text(CurrencyCode,16);BillingPolicyValues.Revision(SourceRevision);BillingPolicyValues.Revision(BindingRevision);
        CompanyGLMappingReview.RequireDate(EffectiveThrough);CompanyGLMappingReview.RequireDate(ReviewDate);
        if(EffectiveThrough<ReviewDate || EffectiveThrough==DateTime.MaxValue.Date)throw new InvalidOperationException("Retirement requires an explicit last date at or after the reviewed company date.");
        if(Replacement is {} next){next.Require();if(next.Company!=Company || next.ActorUserId!=ActorUserId || next.BookId!=BookId || next.CurrencyCode!=CurrencyCode || next.BindingRevision!=BindingRevision || next.ReviewDate!=ReviewDate || next.SourceRevision!=SourceRevision || next.EffectiveFrom!=EffectiveThrough.AddDays(1) || next.IntentId==IntentId || next.MappingId==MappingId)throw new InvalidOperationException("Replacement must begin immediately after retirement in the same original reviewed context.");}
    }
    public GLAccountMapping Require(CompanyGLMappingReview review)
    {
        Require();review.Require(Company,ActorUserId,BookId,CurrencyCode,ReviewDate);
        var original=review.Records.SingleOrDefault(row=>row.GL_ACCOUNT_MAPPING_ID==MappingId) ??throw new InvalidOperationException("Select the original recorded mapping version.");
        if(review.Revision!=SourceRevision)throw new InvalidOperationException("The complete reviewed mapping source revision changed.");
        if(!CompanyGLMappingReview.Effective(original,ReviewDate) || original.EFFECTIVE_DATE>EffectiveThrough || original.EXPIRY_DATE is {} end && EffectiveThrough>=end)throw new InvalidOperationException("The reviewed mapping cannot be shortened to this explicit effective date.");
        if(Replacement is {} next)
        {
            if(next.MappingKey!=original.MAPPING_KEY)throw new InvalidOperationException("A replacement must preserve the original logical mapping key.");
            var shortened=CompanyInventorySourceReview.Copy(original);shortened.EXPIRY_DATE=EffectiveThrough;
            var adjusted=CompanyGLMappingReview.Create(review.Accounts,ReviewDate,review.Records.Select(row=>row.GL_ACCOUNT_MAPPING_ID==MappingId?shortened:row));
            (next with{SourceRevision=adjusted.Revision}).Require(adjusted);
            var oldAccount=review.Accounts.Accounts.Single(row=>row.GL_ACCOUNT_ID==original.GL_ACCOUNT_ID);
            if(next.AccountType!=oldAccount.ACCOUNT_TYPE || next.NormalBalance!=oldAccount.NORMAL_BALANCE)throw new InvalidOperationException("Replacement must preserve the reviewed posting purpose and normal balance.");
        }
        return CompanyInventorySourceReview.Copy(original);
    }
}
public sealed record CompanyGLMappingTransitionSubmission(CompanyGLMappingTransitionCommand Command,CompanyGLMappingReview? Review);
public sealed record CompanyGLMappingTransitionProof(CompanyGLMappingTransitionCommand Command,GLAccountMapping Original,DateTime ChangedAtUtc,string RowRevision)
{
    public const int MaximumLength=196608;
    public static CompanyGLMappingTransitionProof Create(CompanyGLMappingTransitionCommand command,GLAccountMapping original,DateTime changed)
    {command.Require();if(original.CONFIGURATION_TRANSITION_JSON is not null)throw new InvalidOperationException("An existing transition cannot be rewritten.");_=CompanyGLMappingConfigurationProof.Require(original);var proof=new CompanyGLMappingTransitionProof(command,CompanyInventorySourceReview.Copy(original),changed,string.Empty);var row=proof.Expected();return proof with{RowRevision=Digest(row)};}
    public GLAccountMapping Expected()
    {
        Command.Require();if(Original is null || Original.CONFIGURATION_TRANSITION_JSON is not null || Original.GL_ACCOUNT_MAPPING_ID!=Command.MappingId || Original.OWNER_TENANT_ID!=Command.Company.TenantId || Original.OWNER_COMPANY_BA_ID!=Command.Company.BusinessAssociateId || Original.BOOK_ID!=Command.BookId || Original.CURRENCY_CODE!=Command.CurrencyCode || !CompanyGLMappingReview.Effective(Original,Command.ReviewDate) || Original.EFFECTIVE_DATE>Command.EffectiveThrough || Original.EXPIRY_DATE is {} end && Command.EffectiveThrough>=end || ChangedAtUtc.Kind!=DateTimeKind.Utc || ChangedAtUtc<Original.ROW_CREATED_DATE || ChangedAtUtc<Original.ROW_CHANGED_DATE || Command.Replacement is {} next && next.MappingKey!=Original.MAPPING_KEY)
            throw new InvalidOperationException("The exact original mapping and ordered native transition audit are required.");
        _=CompanyGLMappingConfigurationProof.Require(Original);var row=CompanyInventorySourceReview.Copy(Original);row.EXPIRY_DATE=Command.EffectiveThrough;row.ROW_CHANGED_BY=Command.ActorUserId;row.ROW_CHANGED_DATE=ChangedAtUtc;return row;
    }
    public string Serialize(){var row=Expected();if(RowRevision!=Digest(row))throw new InvalidOperationException("The mapping transition digest changed.");var json=JsonSerializer.Serialize(this);if(json.Length>MaximumLength)throw new InvalidOperationException("The original transition exceeds native capacity.");return json;}
    public static CompanyGLMappingTransitionProof Require(GLAccountMapping row)
    {
        if(string.IsNullOrWhiteSpace(row.CONFIGURATION_TRANSITION_JSON) || row.CONFIGURATION_TRANSITION_JSON.Length>MaximumLength)throw new InvalidOperationException("The complete native mapping transition is unavailable.");
        var proof=JsonSerializer.Deserialize<CompanyGLMappingTransitionProof>(row.CONFIGURATION_TRANSITION_JSON) ??throw new InvalidOperationException("The native transition is malformed.");var expected=proof.Expected();var actual=CompanyInventorySourceReview.Copy(row);actual.CONFIGURATION_TRANSITION_JSON=null;
        if(proof.RowRevision!=Digest(actual) || JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(actual))throw new InvalidOperationException("Only the exact original expiry and native transition audit may change.");return proof;
    }
    private static string Digest(GLAccountMapping row){var copy=CompanyInventorySourceReview.Copy(row);copy.CONFIGURATION_TRANSITION_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
}
public sealed record CompanyGLMappingTransitionOutcome(CompanyGLMappingTransitionCommand Command,GLAccountMapping Retired,GLAccountMapping? Replacement)
{
    public void Require(CompanyGLMappingTransitionCommand command)
    {
        command.Require();if(Command!=command || Retired is null || CompanyGLMappingTransitionProof.Require(Retired).Command!=command)throw new InvalidOperationException("The original mapping retirement is unconfirmed.");
        if(command.Replacement is null?Replacement is not null:Replacement is null || CompanyGLMappingConfigurationProof.Require(Replacement).Command!=command.Replacement)throw new InvalidOperationException("The exact original adjacent replacement is unconfirmed.");
    }
}
