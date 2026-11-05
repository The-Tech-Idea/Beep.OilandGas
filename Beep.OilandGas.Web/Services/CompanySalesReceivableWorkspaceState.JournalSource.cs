using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class CompanySalesReceivableWorkspaceState
{
    private CompanySalesJournalSourceReview? journalSource;
    public CompanySalesJournalSourceReview? JournalSource=>Visible && company.Selection!.Permissions.Contains("Accounting.ReviewSalesJournalSource",StringComparer.Ordinal) && journalSource is not null?Copy(journalSource):null;
    public void InvalidateJournalSource(){++version;journalSource=null;journalPreparation=null;Changed?.Invoke();}
    public async Task LoadJournalSourceAsync(string receivable,string book,string currency,string calendar,DateTime date,CompanySalesJournalMappings selection,IAccountingServiceClient client)
    {
        Ready();if(!company.Selection!.Permissions.Contains("Accounting.ReviewSalesJournalSource",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current sales journal source review authority is required.");
        var snapshot=company.Snapshot;var generation=++version;InvalidateReview();journalSource=null;Busy=true;Changed?.Invoke();
        try{var source=await client.GetCompanySalesJournalSourceAsync(receivable,book,currency,calendar,date,selection);source.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,receivable,book,currency,calendar,date,selection);
            if(!disposed && generation==version && snapshot==company.Snapshot && Visible)journalSource=Copy(source);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
}
