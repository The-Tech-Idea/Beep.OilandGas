using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;
public sealed class MaintenanceRecordedWorkState:IDisposable
{
    private readonly CompanyClientContext company;
    private int version;
    private bool disposed;
    public event Action? Changed;
    public bool Busy{get;private set;}
    public CompanyMaintenanceRecordedWorkPage? Current{get;private set;}
    public MaintenanceRecordedWorkState(CompanyClientContext company){this.company=company;company.Changed+=Clear;}
    public void Clear(){version++;Current=null;Changed?.Invoke();}
    public async Task ReadAsync(ICompanyMaintenanceHandoffClient client,CompanyMaintenanceRecordedWorkWindow window,int page,int size)
    {
        window.Require();CompanyMaintenanceRecordedWorkPage.RequirePage(page,size);
        if(disposed || Busy || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Maintenance.ReturnToService",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current standard company role and recorded-work read capability are required.");
        Clear();var generation=version;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try{var result=await client.ReadMaintenanceRecordedWorkPageAsync(window,page,size);result.Require(selected.Scope,selected.UserId,window,page,size);if(!disposed && generation==version && snapshot==company.Snapshot)Current=CompanyInventorySourceReview.Copy(result);}
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;version++;company.Changed-=Clear;Changed=null;Current=null;}
}
