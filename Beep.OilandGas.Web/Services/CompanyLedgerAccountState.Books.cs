using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class CompanyLedgerAccountState
{
    private CompanyLedgerBookReview? books;
    private CompanyLedgerBookConfigurationReview? bookConfigurationReview;
    private CompanyLedgerBookConfigurationSubmission? pendingBook;
    private COMPANY_LEDGER_BOOK? bookResult;
    public bool HasPendingBook=>pendingBook is not null;
    public CompanyLedgerBookReview? Books=>books is null?null:CompanyInventorySourceReview.Copy(books);
    public CompanyLedgerBookConfigurationReview? BookConfigurationReview=>bookConfigurationReview is null?null:CompanyInventorySourceReview.Copy(bookConfigurationReview);
    public COMPANY_LEDGER_BOOK? BookResult=>bookResult is null?null:CompanyInventorySourceReview.Copy(bookResult);
    private bool BookContext=>!disposed && owner is not null && company.Selection is {} selected && selected.Roles.Count>0 && selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal);
    private bool PendingBookMatches=>BookContext && pendingBook is {} pending && company.Selection!.Scope==pending.Command.Company && company.Selection.UserId==pending.Command.ActorUserId;
    public CompanyLedgerBookConfigurationSubmission? PendingBook=>PendingBookMatches?CompanyInventorySourceReview.Copy(pendingBook!):null;
    public async Task LoadBooksAsync(IAccountingServiceClient client)
    {
        if(!BookContext || Busy)throw new UnauthorizedAccessException();var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        try{var loaded=await client.GetCompanyLedgerBooksAsync();loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId);if(!disposed && generation==version && snapshot==company.Snapshot)books=CompanyInventorySourceReview.Copy(loaded);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task LoadBookConfigurationReviewAsync(IAccountingServiceClient client)
    {
        ConfigurationReady();if(HasPendingLedgerRequest)throw new InvalidOperationException("Reconcile the retained original ledger request first.");var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        try{var loaded=await client.GetCompanyLedgerBookConfigurationReviewAsync();loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId);if(!disposed && generation==version && snapshot==company.Snapshot){bookConfigurationReview=CompanyInventorySourceReview.Copy(loaded);books=CompanyInventorySourceReview.Copy(loaded.History);}}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task ConfigureBookAsync(string code,string name,string currency,int precision,string rounding,string calendar,string timezone,string reason,IAccountingServiceClient client)
    {
        ConfigurationReady();if(HasPendingLedgerRequest || bookConfigurationReview is not {} source)throw new InvalidOperationException("Review the original company book configuration first.");source.Require(company.Selection!.Scope,company.Selection.UserId);
        var command=new CompanyLedgerBookConfigurationCommand(Guid.NewGuid().ToString("N"),source.History.Company,source.History.ActorUserId,Guid.NewGuid().ToString("N"),code,name,currency,precision,rounding,calendar,timezone,reason,source.History.Revision,source.BindingRevision);command.Require(source.History);pendingBook=new(command,CompanyInventorySourceReview.Copy(source.History));await SubmitBookAsync(client);
    }
    public async Task SubmitBookAsync(IAccountingServiceClient client)
    {
        ConfigurationReady();if(!PendingBookMatches)throw new InvalidOperationException("Return to the original book producer company/account.");var pending=CompanyInventorySourceReview.Copy(pendingBook!);var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();bool Current()=>!disposed && generation==version && snapshot==company.Snapshot;
        try{var row=await client.ConfigureCompanyLedgerBookAsync(CompanyInventorySourceReview.Copy(pending));if(CompanyLedgerBookConfigurationProof.Require(row).Command!=pending.Command)throw new InvalidOperationException("The original book outcome is unconfirmed.");if(!Current())return;await ConfirmBookAsync(row,client,Current);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task RecoverBookAsync(string id,IAccountingServiceClient client)
    {
        if(!BookContext || Busy || HasPendingFx || HasPendingConfiguration || HasPendingMappingConfiguration || HasPendingMappingTransition || HasPendingPeriod || HasPendingPeriodTransition || HasPendingBook && (!PendingBookMatches || pendingBook!.Command.Id!=id))throw new InvalidOperationException("Recover the retained original ledger request in its own context.");var retained=pendingBook;var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();bool Current()=>!disposed && generation==version && snapshot==company.Snapshot;
        try{var row=await client.GetCompanyLedgerBookConfigurationAsync(id) ??throw new InvalidOperationException("The original native book configuration is unavailable.");var command=CompanyLedgerBookConfigurationProof.Require(row).Command;if(command.Id!=id || command.Company!=snapshot.Selection!.Scope || command.ActorUserId!=snapshot.Selection.UserId || retained is not null && retained.Command!=command)throw new InvalidOperationException("The exact original company book command is unconfirmed.");if(!Current())return;pendingBook=new(CompanyInventorySourceReview.Copy(command),null);await ConfirmBookAsync(row,client,Current);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    private async Task ConfirmBookAsync(COMPANY_LEDGER_BOOK row,IAccountingServiceClient client,Func<bool> current)
    {
        var command=CompanyLedgerBookConfigurationProof.Require(row).Command;var source=await client.GetCompanyLedgerBooksAsync();source.Require(command.Company,command.ActorUserId);var expected=CompanyLedgerBookReview.Create(command.Company,command.ActorUserId,[row]).Books.Single();if(source.Books.SingleOrDefault(book=>book.Id==command.Id)!=expected)throw new InvalidOperationException("The complete public book facts do not confirm the original native configuration.");if(!current())return;books=CompanyInventorySourceReview.Copy(source);bookResult=CompanyInventorySourceReview.Copy(row);pendingBook=null;
    }
}
