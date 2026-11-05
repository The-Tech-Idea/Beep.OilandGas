using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed class MaintenanceAssetEvidenceState(CompanyClientContext company):IDisposable
{
    private CompanyWorkOrderCompletionQuery? query;
    private int version;
    private bool subscribed,disposed;
    public event Action? Changed;
    public bool Busy{get;private set;}
    public CompanyMaintenanceAssetEvidence? Current{get;private set;}
    public CompanyMaintenanceAssetReturnStatus? ReturnStatus{get;private set;}
    public void Attach(CompanyWorkOrderCompletionQuery? next){if(!subscribed){subscribed=true;company.Changed+=Clear;}next?.Require();if(next==query)return;Clear();query=next;}
    public void Clear(){version++;query=null;Current=null;ReturnStatus=null;Changed?.Invoke();}
    public async Task ReadReturnStatusAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(disposed || Busy || query is null || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Maintenance.ReturnToService",StringComparer.Ordinal))throw new UnauthorizedAccessException("Original report and current standard company return-read capability are required.");
        var original=query;var snapshot=company.Snapshot;var generation=version;Busy=true;Current=null;ReturnStatus=null;Changed?.Invoke();
        try
        {
            var result=await client.ReadMaintenanceAssetReturnStatusAsync(original);result.Require();
            if(result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId || result.Query!=original)throw new UnauthorizedAccessException("Original public return company/account/report differs.");
            if(!disposed && snapshot==company.Snapshot && generation==version)ReturnStatus=CompanyInventorySourceReview.Copy(result);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task ReadAsync(ICompanyMaintenanceHandoffClient client)
    {
            if(disposed || Busy || query is null || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Maintenance.ReturnToService",StringComparer.Ordinal))throw new UnauthorizedAccessException("Original closed report, current standard company roles and return evidence capability are required.");var original=query;var snapshot=company.Snapshot;var generation=version;Busy=true;Current=null;ReturnStatus=null;Changed?.Invoke();
        try{var result=await client.ReadMaintenanceAssetEvidenceAsync(original);result.Require();if(result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId || result.Report.Query!=original)throw new UnauthorizedAccessException("Original company/account/report asset evidence differs.");if(!disposed && snapshot==company.Snapshot && generation==version)Current=CompanyInventorySourceReview.Copy(result);}
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;version++;if(subscribed)company.Changed-=Clear;Changed=null;}
}
