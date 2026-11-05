using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

public sealed partial class CompanyLedgerAccountState
{
    private CompanyGLAccountConfigurationReview? configurationReview;
    private CompanyGLAccountConfigurationSubmission? pendingConfiguration;
    private GL_ACCOUNT? configurationResult;
    private object? owner;
    public bool HasPendingConfiguration=>pendingConfiguration is not null;
    public CompanyGLAccountConfigurationReview? ConfigurationReview=>configurationReview is null?null:CompanyInventorySourceReview.Copy(configurationReview);
    public CompanyGLAccountConfigurationSubmission? PendingConfiguration=>PendingMatches()?CompanyInventorySourceReview.Copy(pendingConfiguration):null;
    public GL_ACCOUNT? ConfigurationResult=>configurationResult is null?null:CompanyInventorySourceReview.Copy(configurationResult);
    public void AttachOwner(object presentation){ArgumentNullException.ThrowIfNull(presentation);if(ReferenceEquals(owner,presentation))return;owner=presentation;Invalidate();}
    public void DetachOwner(object presentation){if(!ReferenceEquals(owner,presentation))return;owner=null;Invalidate();}
    private bool PendingMatches()=>!disposed && pendingConfiguration is {} pending && company.Selection is {} selected && selected.Scope==pending.Command.Company && selected.UserId==pending.Command.ActorUserId &&
        selected.Roles.Count>0 && selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal) && selected.Permissions.Contains("Accounting.ConfigureLedger",StringComparer.Ordinal);
    private void ConfigurationReady()
    {
        if(disposed || Busy || owner is null || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal) || !selected.Permissions.Contains("Accounting.ConfigureLedger",StringComparer.Ordinal))
            throw new UnauthorizedAccessException("Current company ledger management authority and the original presentation are required.");
    }
    public async Task LoadConfigurationReviewAsync(string book,string currency,IAccountingServiceClient client)
    {
        ConfigurationReady();if(HasPendingLedgerRequest)throw new InvalidOperationException("Reconcile the retained original configuration before another account.");
        var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        try{var loaded=await client.GetCompanyGLAccountConfigurationReviewAsync(book,currency);loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,book,currency);
            if(!disposed && generation==version && snapshot==company.Snapshot)configurationReview=CompanyInventorySourceReview.Copy(loaded);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task ConfigureAsync(string number,string name,string type,string normal,string? parent,string? description,string reason,IAccountingServiceClient client)
    {
        ConfigurationReady();if(HasPendingLedgerRequest || configurationReview is not {} displayed)throw new InvalidOperationException("Review the complete original company account configuration first.");
        var accounts=displayed.Accounts;displayed.Require(company.Selection!.Scope,company.Selection.UserId,accounts.BookId,accounts.CurrencyCode);
        var command=new CompanyGLAccountConfigurationCommand(Guid.NewGuid().ToString("N"),accounts.Company,accounts.ActorUserId,accounts.BookId,accounts.CurrencyCode,Guid.NewGuid().ToString("N"),number,name,type,normal,parent,description,reason,accounts.Revision,displayed.BindingRevision);
        command.Require(accounts);pendingConfiguration=new(command,CompanyInventorySourceReview.Copy(accounts));await SubmitConfigurationAsync(client);
    }
    public async Task RecoverConfigurationAsync(string book,string currency,string id,IAccountingServiceClient client)
    {
        ConfigurationReady();if(HasPendingLedgerRequest)throw new InvalidOperationException("Reconcile the original retained configuration first.");
        var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        try{var command=await client.GetCompanyGLAccountConfigurationRequestAsync(book,currency,id) ??throw new InvalidOperationException("The original committed request is unavailable; no new configuration is authorized.");command.Require();
            if(command.Company!=snapshot.Selection!.Scope || command.ActorUserId!=snapshot.Selection.UserId || command.BookId!=book || command.CurrencyCode!=currency || command.AccountId!=id)throw new InvalidOperationException("The original request is unconfirmed.");
            if(!disposed && generation==version && snapshot==company.Snapshot)pendingConfiguration=new(CompanyInventorySourceReview.Copy(command),null);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task SubmitConfigurationAsync(IAccountingServiceClient client)
    {
        ConfigurationReady();if(!PendingMatches())throw new InvalidOperationException("Return to the original company/account to reconcile this request.");
        var pending=CompanyInventorySourceReview.Copy(pendingConfiguration!);var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        bool Current()=>!disposed && generation==version && snapshot==company.Snapshot;
        try
        {
            var result=await client.ConfigureCompanyGLAccountAsync(CompanyInventorySourceReview.Copy(pending));if(CompanyGLAccountConfigurationProof.Require(result).Command!=pending.Command)throw new InvalidOperationException("The original configuration outcome is unconfirmed.");
            if(!Current())return;var publicReview=await client.GetCompanyGLAccountsAsync(pending.Command.BookId,pending.Command.CurrencyCode);publicReview.Require(pending.Command.Company,pending.Command.ActorUserId,pending.Command.BookId,pending.Command.CurrencyCode);
            var actual=publicReview.Accounts.SingleOrDefault(account=>account.GL_ACCOUNT_ID==pending.Command.AccountId);var expected=CompanyInventorySourceReview.Copy(result);expected.CONFIGURATION_COMMAND_JSON=null;
            if(actual is null || JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The complete public account review does not confirm the original configuration outcome.");
            if(!Current())return;review=CompanyInventorySourceReview.Copy(publicReview);configurationResult=CompanyInventorySourceReview.Copy(result);pendingConfiguration=null;
        }
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
}
