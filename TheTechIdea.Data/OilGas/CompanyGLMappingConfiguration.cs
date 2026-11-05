using System.Text.Json;
using Beep.OilandGas.Accounting.Models;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanyGLMappingConfigurationCommand(string IntentId,CompanyScope Company,string ActorUserId,string BookId,string CurrencyCode,string MappingId,string MappingKey,
    string AccountId,string AccountNumber,string AccountType,string NormalBalance,DateTime EffectiveFrom,DateTime? EffectiveThrough,DateTime ReviewDate,string Reason,string SourceRevision,string BindingRevision)
{
    public void Require()
    {
        BillingPolicyValues.Intent(IntentId);BillingPolicyValues.Intent(MappingId);PurchaseOrderOwnership.RequireScope(Company);
        foreach(var value in new[]{ActorUserId,BookId,MappingKey,AccountId,AccountNumber,Reason})BillingPolicyValues.Text(value,128);BillingPolicyValues.Text(CurrencyCode,16);
        CompanyGLMappingReview.RequireDate(EffectiveFrom);CompanyGLMappingReview.RequireDate(ReviewDate);if(EffectiveThrough is {} end){CompanyGLMappingReview.RequireDate(end);if(end<EffectiveFrom)throw new InvalidOperationException("The explicit mapping date range is invalid.");}
        BillingPolicyValues.Revision(SourceRevision);BillingPolicyValues.Revision(BindingRevision);
        if(AccountType is not ("ASSET" or "LIABILITY" or "EQUITY" or "REVENUE" or "EXPENSE") || NormalBalance is not ("DEBIT" or "CREDIT"))throw new InvalidOperationException("Explicit compatible mapping account facts are required.");
    }
    public void Require(CompanyGLMappingReview review)
    {
        Require();review.Require(Company,ActorUserId,BookId,CurrencyCode,ReviewDate);
        if(review.Revision!=SourceRevision || review.Records.Count>=CompanyGLMappingReview.MaximumMappings || review.Records.Any(row=>row.GL_ACCOUNT_MAPPING_ID==MappingId))throw new InvalidOperationException("The complete original mapping source changed, is full or already reserves this identity.");
        var account=review.Accounts.Accounts.SingleOrDefault(row=>row.GL_ACCOUNT_ID==AccountId);
        if(account is null || account.ACCOUNT_NUMBER!=AccountNumber || account.ACCOUNT_TYPE!=AccountType || account.NORMAL_BALANCE!=NormalBalance)throw new InvalidOperationException("The explicit mapping must refer to the reviewed native company/book/currency account.");
        if(review.Records.Any(row=>row.MAPPING_KEY==MappingKey && (row.EXPIRY_DATE is null || row.EXPIRY_DATE>=EffectiveFrom) && (EffectiveThrough is null || row.EFFECTIVE_DATE<=EffectiveThrough)))
            throw new InvalidOperationException("A mapping version overlaps the explicit effective range. Existing history cannot be overwritten or chosen by order.");
    }
    public GLAccountMapping CreateRow()=>new(){GL_ACCOUNT_MAPPING_ID=MappingId,MAPPING_KEY=MappingKey,GL_ACCOUNT_ID=AccountId,GL_ACCOUNT_NUMBER=AccountNumber,
        OWNER_TENANT_ID=Company.TenantId,OWNER_COMPANY_BA_ID=Company.BusinessAssociateId,BOOK_ID=BookId,CURRENCY_CODE=CurrencyCode,EFFECTIVE_DATE=EffectiveFrom,EXPIRY_DATE=EffectiveThrough,
        ACTIVE_IND="Y",SOURCE=CompanyGLMappingConfigurationProof.Source,REMARK=Reason,PPDM_GUID=Guid.ParseExact(IntentId,"N").ToString()};
}
public sealed record CompanyGLMappingConfigurationReview(CompanyGLMappingReview History,string BindingRevision)
{
    public void Require(CompanyScope company,string actor,string book,string currency,DateTime date)
    {if(History is null)throw new InvalidOperationException("The original complete mapping review is unavailable.");History.Require(company,actor,book,currency,date);BillingPolicyValues.Revision(BindingRevision);}
}
public sealed record CompanyGLMappingConfigurationSubmission(CompanyGLMappingConfigurationCommand Command,CompanyGLMappingReview? Review);
public sealed record CompanyGLMappingConfigurationReceipt(CompanyGLMappingConfigurationCommand Command,string RowRevision);
public static class CompanyGLMappingConfigurationProof
{
    public const string Source="COMPANY_GL_MAPPING_CONFIGURATION";
    public const int MaximumLength=32768;
    public static string Create(CompanyGLMappingConfigurationCommand command,GLAccountMapping row)
    {
        RequireFacts(command,row);var json=JsonSerializer.Serialize(new CompanyGLMappingConfigurationReceipt(command,Digest(row)));
        if(json.Length>MaximumLength)throw new InvalidOperationException("The original mapping receipt exceeds native capacity.");return json;
    }
    public static CompanyGLMappingConfigurationReceipt Require(GLAccountMapping row)
    {
        if(row.CONFIGURATION_TRANSITION_JSON is not null)row=CompanyGLMappingTransitionProof.Require(row).Original;
        if(string.IsNullOrWhiteSpace(row.CONFIGURATION_COMMAND_JSON) || row.CONFIGURATION_COMMAND_JSON.Length>MaximumLength)throw new InvalidOperationException("The original mapping configuration receipt is unavailable.");
        var receipt=JsonSerializer.Deserialize<CompanyGLMappingConfigurationReceipt>(row.CONFIGURATION_COMMAND_JSON) ??throw new InvalidOperationException("The original mapping receipt is malformed.");
        if(receipt.Command is null)throw new InvalidOperationException("The original mapping command is unavailable.");RequireFacts(receipt.Command,row);
        if(receipt.RowRevision!=Digest(row))throw new InvalidOperationException("The complete configured mapping changed.");return receipt;
    }
    private static void RequireFacts(CompanyGLMappingConfigurationCommand command,GLAccountMapping row)
    {
        command.Require();CompanyGLAccountReview.RequireAudit(row);var expected=command.CreateRow();expected.ROW_CREATED_BY=row.ROW_CREATED_BY;expected.ROW_CREATED_DATE=row.ROW_CREATED_DATE;expected.ROW_CHANGED_BY=row.ROW_CHANGED_BY;expected.ROW_CHANGED_DATE=row.ROW_CHANGED_DATE;
        var actual=CompanyInventorySourceReview.Copy(row);actual.CONFIGURATION_COMMAND_JSON=null;
        if(row.ROW_CREATED_BY!=command.ActorUserId || row.ROW_CHANGED_DATE is not null && row.ROW_CHANGED_BY!=command.ActorUserId || JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("Only the exact original mapping and native creation audit are authorized.");
    }
    private static string Digest(GLAccountMapping row){var copy=CompanyInventorySourceReview.Copy(row);copy.CONFIGURATION_COMMAND_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
}
