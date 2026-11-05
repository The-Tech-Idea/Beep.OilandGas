using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class CompanyLedgerAccountState
{
    private CompanyGLMappingTransitionSubmission? pendingMappingTransition;
    private CompanyGLMappingTransitionOutcome? mappingTransitionResult;
    public bool HasPendingMappingTransition=>pendingMappingTransition is not null;
    private bool TransitionContext=>!disposed && owner is not null && company.Selection is {} selected && selected.Roles.Count>0 && selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal);
    private bool PendingTransitionMatches=>TransitionContext && pendingMappingTransition is {} pending && pending.Command.Company==company.Selection!.Scope && pending.Command.ActorUserId==company.Selection.UserId;
    public CompanyGLMappingTransitionSubmission? PendingMappingTransition=>PendingTransitionMatches?CompanyInventorySourceReview.Copy(pendingMappingTransition!):null;
    public CompanyGLMappingTransitionOutcome? MappingTransitionResult=>TransitionContext && mappingTransitionResult is not null?CompanyInventorySourceReview.Copy(mappingTransitionResult):null;
    public async Task TransitionMappingAsync(string id,DateTime through,string? replacementAccountId,DateTime? replacementThrough,string reason,IAccountingServiceClient client)
    {
        ConfigurationReady();if(HasPendingLedgerRequest || mappingConfigurationReview is not {} displayed)throw new InvalidOperationException("Review the original mapping history and reconcile any retained ledger request first.");var history=displayed.History;displayed.Require(company.Selection!.Scope,company.Selection.UserId,history.Accounts.BookId,history.Accounts.CurrencyCode,history.AsOfDate);
        var original=history.Records.SingleOrDefault(row=>row.GL_ACCOUNT_MAPPING_ID==id) ??throw new InvalidOperationException("Select the original reviewed mapping version.");CompanyGLMappingConfigurationCommand? next=null;
        if(replacementAccountId is not null){var account=history.Accounts.Accounts.SingleOrDefault(row=>row.GL_ACCOUNT_ID==replacementAccountId) ??throw new InvalidOperationException("Choose the reviewed native replacement account.");next=new(Guid.NewGuid().ToString("N"),history.Accounts.Company,history.Accounts.ActorUserId,history.Accounts.BookId,history.Accounts.CurrencyCode,Guid.NewGuid().ToString("N"),original.MAPPING_KEY,account.GL_ACCOUNT_ID,account.ACCOUNT_NUMBER,account.ACCOUNT_TYPE,account.NORMAL_BALANCE,through.AddDays(1),replacementThrough,history.AsOfDate,reason,history.Revision,displayed.BindingRevision);}
        var command=new CompanyGLMappingTransitionCommand(Guid.NewGuid().ToString("N"),history.Accounts.Company,history.Accounts.ActorUserId,history.Accounts.BookId,history.Accounts.CurrencyCode,id,through,history.AsOfDate,reason,history.Revision,displayed.BindingRevision,next);command.Require(history);pendingMappingTransition=new(command,CompanyInventorySourceReview.Copy(history));await SubmitMappingTransitionAsync(client);
    }
    public async Task RecoverMappingTransitionAsync(string book,string currency,string id,IAccountingServiceClient client)
    {
        if(!TransitionContext || Busy || HasPendingFx || HasPendingBook || HasPendingConfiguration || HasPendingMappingConfiguration || HasPendingPeriod || HasPendingPeriodTransition || HasPendingMappingTransition && (!PendingTransitionMatches || pendingMappingTransition!.Command.MappingId!=id || pendingMappingTransition.Command.BookId!=book || pendingMappingTransition.Command.CurrencyCode!=currency))throw new InvalidOperationException("Return to the original company/account and reconcile any retained request.");var retained=pendingMappingTransition;var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        try{var result=await client.GetCompanyGLMappingTransitionAsync(book,currency,id) ??throw new InvalidOperationException("The durable original transition is unavailable.");result.Require(retained?.Command ?? result.Command);if(result.Command.Company!=snapshot.Selection!.Scope || result.Command.ActorUserId!=snapshot.Selection.UserId || result.Command.BookId!=book || result.Command.CurrencyCode!=currency || result.Command.MappingId!=id)throw new InvalidOperationException("The private transition belongs to another original context.");if(!disposed && generation==version && snapshot==company.Snapshot){mappingTransitionResult=CompanyInventorySourceReview.Copy(result);pendingMappingTransition=null;}}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task SubmitMappingTransitionAsync(IAccountingServiceClient client)
    {
        ConfigurationReady();if(!PendingTransitionMatches)throw new InvalidOperationException("Return to the original transition company/account to reconcile.");var pending=CompanyInventorySourceReview.Copy(pendingMappingTransition!);var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();bool Current()=>!disposed && generation==version && snapshot==company.Snapshot;
        try{var result=await client.TransitionCompanyGLMappingAsync(pending);result.Require(pending.Command);if(!Current())return;var recovery=await client.GetCompanyGLMappingTransitionAsync(pending.Command.BookId,pending.Command.CurrencyCode,pending.Command.MappingId);if(recovery is null || JsonSerializer.Serialize(recovery)!=JsonSerializer.Serialize(result))throw new InvalidOperationException("The original durable transition recovery is unconfirmed.");recovery.Require(pending.Command);if(!Current())return;
            var history=await client.GetCompanyGLMappingsAsync(pending.Command.BookId,pending.Command.CurrencyCode,pending.Command.ReviewDate);history.Require(pending.Command.Company,pending.Command.ActorUserId,pending.Command.BookId,pending.Command.CurrencyCode,pending.Command.ReviewDate);
            foreach(var native in new[]{result.Retired,result.Replacement}.Where(row=>row is not null)){var expected=CompanyInventorySourceReview.Copy(native!);expected.CONFIGURATION_COMMAND_JSON=null;expected.CONFIGURATION_TRANSITION_JSON=null;var row=history.Records.SingleOrDefault(row=>row.GL_ACCOUNT_MAPPING_ID==expected.GL_ACCOUNT_MAPPING_ID);if(row is null || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("Complete public history does not confirm the original transition.");}
            if(!Current())return;mappings=CompanyInventorySourceReview.Copy(history);mappingTransitionResult=CompanyInventorySourceReview.Copy(result);pendingMappingTransition=null;
        }finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
}
