using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Accounting.Models;

public sealed record CompanyGLAccountConfigurationCommand(string IntentId,CompanyScope Company,string ActorUserId,string BookId,string CurrencyCode,
    string AccountId,string AccountNumber,string AccountName,string AccountType,string NormalBalance,string? ParentAccountId,string? Description,string Reason,string SourceRevision,string BindingRevision)
{
    public void Require()
    {
        BillingPolicyValues.Intent(IntentId);BillingPolicyValues.Intent(AccountId);PurchaseOrderOwnership.RequireScope(Company);
        foreach(var (value,size) in new[]{(ActorUserId,128),(BookId,128),(CurrencyCode,16),(AccountNumber,128),(AccountName,256),(Reason,128)})BillingPolicyValues.Text(value,size);
        BillingPolicyValues.Revision(SourceRevision);BillingPolicyValues.Revision(BindingRevision);
        if(ParentAccountId is not null)BillingPolicyValues.Text(ParentAccountId,128);
        if(Description is not null && (Description.Length>1024 || Description.Any(char.IsControl)))throw new InvalidOperationException("The explicit account description is malformed.");
        if(AccountType is not ("ASSET" or "LIABILITY" or "EQUITY" or "REVENUE" or "EXPENSE") || NormalBalance is not ("DEBIT" or "CREDIT"))throw new InvalidOperationException("An explicit native account type and normal balance are required.");
    }
    public void Require(CompanyGLAccountReview review)
    {
        Require();review.Require(Company,ActorUserId,BookId,CurrencyCode);
        if(review.Revision!=SourceRevision || review.Accounts.Count>=CompanyGLAccountReview.MaximumAccounts || review.Accounts.Any(row=>row.GL_ACCOUNT_ID==AccountId || row.ACCOUNT_NUMBER==AccountNumber))
            throw new InvalidOperationException("The complete original account review changed, is full, or already reserves this account identity/number.");
        var visited=new HashSet<string>(StringComparer.Ordinal){AccountId};var parent=ParentAccountId;
        while(parent is not null)
        {
            if(!visited.Add(parent))throw new InvalidOperationException("The recorded parent account hierarchy is cyclic.");
            var row=review.Accounts.SingleOrDefault(row=>row.GL_ACCOUNT_ID==parent) ??throw new InvalidOperationException("A parent account must belong to the same current company book/currency review.");
            parent=row.PARENT_ACCOUNT_ID;
        }
    }
    public GL_ACCOUNT CreateRow()=>new(){GL_ACCOUNT_ID=AccountId,ACCOUNT_NUMBER=AccountNumber,ACCOUNT_NAME=AccountName,ACCOUNT_TYPE=AccountType,NORMAL_BALANCE=NormalBalance,
        PARENT_ACCOUNT_ID=ParentAccountId!,DESCRIPTION=Description!,OWNER_TENANT_ID=Company.TenantId,OWNER_COMPANY_BA_ID=Company.BusinessAssociateId,BOOK_ID=BookId,CURRENCY_CODE=CurrencyCode,
        ACTIVE_IND="Y",SOURCE=CompanyGLAccountConfigurationProof.Source,REMARK=Reason,PPDM_GUID=Guid.ParseExact(IntentId,"N").ToString()};
}
public sealed record CompanyGLAccountConfigurationSubmission(CompanyGLAccountConfigurationCommand Command,CompanyGLAccountReview? Review);
public sealed record CompanyGLAccountConfigurationReview(CompanyGLAccountReview Accounts,string BindingRevision)
{
    public void Require(CompanyScope company,string actor,string book,string currency)
    {if(Accounts is null)throw new InvalidOperationException("The complete original configuration review is unavailable.");Accounts.Require(company,actor,book,currency);BillingPolicyValues.Revision(BindingRevision);}
}
public sealed record CompanyGLAccountConfigurationReceipt(CompanyGLAccountConfigurationCommand Command,string RowRevision);

public static class CompanyGLAccountConfigurationProof
{
    public const string Source="COMPANY_GL_ACCOUNT_CONFIGURATION";
    public const int MaximumLength=32768;
    public static string Create(CompanyGLAccountConfigurationCommand command,GL_ACCOUNT row)
    {
        RequireFacts(command,row);var json=JsonSerializer.Serialize(new CompanyGLAccountConfigurationReceipt(command,Digest(row)));
        if(json.Length>MaximumLength)throw new InvalidOperationException("The original account configuration receipt exceeds its native capacity.");return json;
    }
    public static CompanyGLAccountConfigurationReceipt Require(GL_ACCOUNT row)
    {
        if(string.IsNullOrWhiteSpace(row.CONFIGURATION_COMMAND_JSON) || row.CONFIGURATION_COMMAND_JSON.Length>MaximumLength)throw new InvalidOperationException("The original account configuration receipt is unavailable.");
        var receipt=JsonSerializer.Deserialize<CompanyGLAccountConfigurationReceipt>(row.CONFIGURATION_COMMAND_JSON) ??throw new InvalidOperationException("The original account configuration receipt is malformed.");
        if(receipt.Command is null)throw new InvalidOperationException("The original account configuration command is unavailable.");
        RequireFacts(receipt.Command,row);if(receipt.RowRevision!=Digest(row))throw new InvalidOperationException("The complete native configured account changed.");return receipt;
    }
    private static void RequireFacts(CompanyGLAccountConfigurationCommand command,GL_ACCOUNT row)
    {
        command.Require();CompanyGLAccountReview.RequireAudit(row);var expected=command.CreateRow();expected.ROW_CREATED_BY=row.ROW_CREATED_BY;expected.ROW_CREATED_DATE=row.ROW_CREATED_DATE;
        expected.ROW_CHANGED_BY=row.ROW_CHANGED_BY;expected.ROW_CHANGED_DATE=row.ROW_CHANGED_DATE;
        var actual=CompanyInventorySourceReview.Copy(row);actual.CONFIGURATION_COMMAND_JSON=null;
        if(row.ROW_CREATED_BY!=command.ActorUserId || row.ROW_CHANGED_DATE is not null && row.ROW_CHANGED_BY!=command.ActorUserId || JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(actual))
            throw new InvalidOperationException("Only the exact original account facts, native creation audit and unavailable balances are authorized.");
    }
    private static string Digest(GL_ACCOUNT row){var copy=CompanyInventorySourceReview.Copy(row);copy.CONFIGURATION_COMMAND_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
}
