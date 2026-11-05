using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
namespace Beep.OilandGas.Web.Services;
public sealed partial class CompanySalesReceivableWorkspaceState
{
    private CompanySalesPostedLedgerReview? postedLedger;
    public bool CanReadPostedLedger=>Visible && company.Selection!.Permissions.Contains("Accounting.ReviewPostedSalesLedger",StringComparer.Ordinal);
    public CompanySalesPostedLedgerReview? PostedLedger=>CanReadPostedLedger && postedLedger is not null?Copy(postedLedger):null;
    public void InvalidatePostedLedger(){++version;postedLedger=null;Changed?.Invoke();}
    public async Task LoadPostedLedgerAsync(string receivable,string book,string currency,IAccountingServiceClient client)
    {
        Ready();if(!CanReadPostedLedger)throw new UnauthorizedAccessException("Current confirmed sales ledger read authority is required.");var snapshot=company.Snapshot;var generation=++version;InvalidateReview();Busy=true;Changed?.Invoke();
        try{var loaded=Copy(await client.GetCompanySalesPostedLedgerAsync(receivable,book,currency));loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,book,currency,receivable);if(!disposed && generation==version && snapshot==company.Snapshot && CanReadPostedLedger)postedLedger=loaded;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
}
