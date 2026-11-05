using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>Public recorded financial outcome and current validity, fenced to the current circuit company/account/resource.</summary>
public sealed class WorkOrderBudgetDecisionStatusState(CompanyClientContext company):IDisposable
{
    private CompanyWorkOrderBudgetDecisionQuery? query;
    private bool subscribed,disposed;
    private int version;
    public event Action? Changed;
    public CompanyWorkOrderBudgetDecisionStatus? Current{get;private set;}
    public CompanyWorkOrderActualCostReview? ActualCosts{get;private set;}
    public bool Busy{get;private set;}
    public void Attach(CompanyWorkOrderBudgetDecisionQuery? next)
    {
        if(!subscribed){company.Changed+=Clear;subscribed=true;}next?.Require();if(query==next)return;Clear();query=next;
    }
    public void Clear(){version++;query=null;Current=null;ActualCosts=null;Changed?.Invoke();}
    public async Task LoadCostsAsync(IWorkOrderServiceClient client)
    {
        if(disposed || Busy || query is null || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal))
            throw new UnauthorizedAccessException("Current original financial company/account visibility is required.");
        var original=query;var snapshot=company.Snapshot;var generation=version;Busy=true;Current=null;ActualCosts=null;Changed?.Invoke();
        try
        {
            var result=await client.ReadActualCostsAsync(original);result.Require();
            if(result.Budget.Query!=original || result.Budget.Current.Context.Company!=selected.Scope || result.Budget.Current.Context.ActorUserId!=selected.UserId)
                throw new UnauthorizedAccessException("Original public cost company/account/query differs.");
            if(!disposed && generation==version && snapshot==company.Snapshot){ActualCosts=CompanyInventorySourceReview.Copy(result);Current=ActualCosts.Budget;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task LoadAsync(IWorkOrderServiceClient client)
    {
        if(disposed || Busy || query is null || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal))
            throw new UnauthorizedAccessException("Current original company/account and financial visibility are required.");
        var original=query;var snapshot=company.Snapshot;var generation=version;Busy=true;Current=null;ActualCosts=null;Changed?.Invoke();
        try
        {
            var result=await client.ReadBudgetDecisionStatusAsync(original);result.Require();
            if(result.Query!=original || result.Current.Context.Company!=selected.Scope || result.Current.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original current status account/company/query differs.");
            if(!disposed && generation==version && snapshot==company.Snapshot)Current=CompanyInventorySourceReview.Copy(result);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;version++;if(subscribed)company.Changed-=Clear;Changed=null;}
}
