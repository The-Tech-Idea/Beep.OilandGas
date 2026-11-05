using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Accounting.Models;
namespace Beep.OilandGas.Web.Services;
public sealed partial class CompanyLedgerAccountState
{
    private CompanyGLMappingConfigurationReview? mappingConfigurationReview;
    private CompanyGLMappingConfigurationSubmission? pendingMappingConfiguration;
    private GLAccountMapping? mappingConfigurationResult;
    public bool HasPendingMappingConfiguration=>pendingMappingConfiguration is not null;
    public bool HasPendingLedgerRequest=>HasPendingFx || HasPendingBook || HasPendingConfiguration || HasPendingMappingConfiguration || HasPendingMappingTransition || HasPendingPeriod || HasPendingPeriodTransition;
    private bool PendingMappingMatches()=>!disposed && pendingMappingConfiguration is {} pending && company.Selection is {} selected && selected.Scope==pending.Command.Company && selected.UserId==pending.Command.ActorUserId &&
        selected.Roles.Count>0 && selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal) && selected.Permissions.Contains("Accounting.ConfigureLedger",StringComparer.Ordinal);
    public CompanyGLMappingConfigurationReview? MappingConfigurationReview=>mappingConfigurationReview is null?null:CompanyInventorySourceReview.Copy(mappingConfigurationReview);
    public CompanyGLMappingConfigurationSubmission? PendingMappingConfiguration=>PendingMappingMatches()?CompanyInventorySourceReview.Copy(pendingMappingConfiguration):null;
    public GLAccountMapping? MappingConfigurationResult=>mappingConfigurationResult is null?null:CompanyInventorySourceReview.Copy(mappingConfigurationResult);
    public async Task LoadMappingConfigurationReviewAsync(string book,string currency,DateTime date,IAccountingServiceClient client)
    {
        ConfigurationReady();if(HasPendingLedgerRequest)throw new InvalidOperationException("Reconcile the original ledger request before another configuration.");var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        try{var loaded=await client.GetCompanyGLMappingConfigurationReviewAsync(book,currency,date);loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,book,currency,date);if(!disposed && generation==version && snapshot==company.Snapshot)mappingConfigurationReview=CompanyInventorySourceReview.Copy(loaded);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task ConfigureMappingAsync(string key,string accountId,DateTime from,DateTime? through,string reason,IAccountingServiceClient client)
    {
        ConfigurationReady();if(HasPendingLedgerRequest || mappingConfigurationReview is not {} displayed)throw new InvalidOperationException("Review the complete original mapping configuration first.");var history=displayed.History;
        displayed.Require(company.Selection!.Scope,company.Selection.UserId,history.Accounts.BookId,history.Accounts.CurrencyCode,history.AsOfDate);
        var account=history.Accounts.Accounts.SingleOrDefault(account=>account.GL_ACCOUNT_ID==accountId) ??throw new InvalidOperationException("Choose the reviewed native account identity.");
        var command=new CompanyGLMappingConfigurationCommand(Guid.NewGuid().ToString("N"),history.Accounts.Company,history.Accounts.ActorUserId,history.Accounts.BookId,history.Accounts.CurrencyCode,Guid.NewGuid().ToString("N"),key,account.GL_ACCOUNT_ID,account.ACCOUNT_NUMBER,account.ACCOUNT_TYPE,account.NORMAL_BALANCE,from,through,history.AsOfDate,reason,history.Revision,displayed.BindingRevision);
        command.Require(history);pendingMappingConfiguration=new(command,CompanyInventorySourceReview.Copy(history));await SubmitMappingConfigurationAsync(client);
    }
    public async Task RecoverMappingConfigurationAsync(string book,string currency,string id,IAccountingServiceClient client)
    {
        ConfigurationReady();if(HasPendingLedgerRequest)throw new InvalidOperationException("Reconcile the original retained request first.");var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        try{var command=await client.GetCompanyGLMappingConfigurationRequestAsync(book,currency,id) ??throw new InvalidOperationException("The original committed mapping request is unavailable.");command.Require();
            if(command.Company!=snapshot.Selection!.Scope || command.ActorUserId!=snapshot.Selection.UserId || command.BookId!=book || command.CurrencyCode!=currency || command.MappingId!=id)throw new InvalidOperationException("The original mapping request is unconfirmed.");if(!disposed && generation==version && snapshot==company.Snapshot)pendingMappingConfiguration=new(CompanyInventorySourceReview.Copy(command),null);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task SubmitMappingConfigurationAsync(IAccountingServiceClient client)
    {
        ConfigurationReady();if(!PendingMappingMatches())throw new InvalidOperationException("Return to the original mapping company/account to reconcile.");var pending=CompanyInventorySourceReview.Copy(pendingMappingConfiguration!);var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        bool Current()=>!disposed && generation==version && snapshot==company.Snapshot;
        try
        {
            var result=await client.ConfigureCompanyGLMappingAsync(CompanyInventorySourceReview.Copy(pending));if(CompanyGLMappingConfigurationProof.Require(result).Command!=pending.Command)throw new InvalidOperationException("The original mapping outcome is unconfirmed.");if(!Current())return;
            var publicReview=await client.GetCompanyGLMappingsAsync(pending.Command.BookId,pending.Command.CurrencyCode,pending.Command.ReviewDate);publicReview.Require(pending.Command.Company,pending.Command.ActorUserId,pending.Command.BookId,pending.Command.CurrencyCode,pending.Command.ReviewDate);
            var actual=publicReview.Records.SingleOrDefault(row=>row.GL_ACCOUNT_MAPPING_ID==pending.Command.MappingId);var expected=CompanyInventorySourceReview.Copy(result);expected.CONFIGURATION_COMMAND_JSON=null;
            var account=publicReview.Accounts.Accounts.SingleOrDefault(row=>row.GL_ACCOUNT_ID==pending.Command.AccountId);
            if(actual is null || JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(expected) || account is null || account.ACCOUNT_NUMBER!=pending.Command.AccountNumber || account.ACCOUNT_TYPE!=pending.Command.AccountType || account.NORMAL_BALANCE!=pending.Command.NormalBalance)throw new InvalidOperationException("The complete public account/mapping review does not confirm the exact original outcome.");
            if(!Current())return;mappings=CompanyInventorySourceReview.Copy(publicReview);mappingConfigurationResult=CompanyInventorySourceReview.Copy(result);pendingMappingConfiguration=null;
        }
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
}
