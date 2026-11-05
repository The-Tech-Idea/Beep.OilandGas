using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.Web.Services;

/// <summary>Reuse authorized work-order discovery; financial reads retain current company/account/FIELD boundaries.</summary>
public sealed class WorkOrderFinancialWorkspaceState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly WorkOrderListState list;
    private int generation;
    private bool disposed;
    public WorkOrderFinancialWorkspaceState(CompanyClientContext company){this.company=company;list=new(company);company.Changed+=Clear;}
    public event Action? Changed;
    public bool Busy{get;private set;}
    public bool Confirmed=>list.Confirmed;
    public string? FieldId=>list.FieldId;
    public IReadOnlyList<WorkOrderSummary> Orders=>list.Rows.Where(row=>row.Source=="WORK_ORDER").Select(CompanyInventorySourceReview.Copy).ToArray();
    public CompanyWorkOrderFinancialSources? Current{get;private set;}
    public void Clear(){generation++;list.Clear();Current=null;Changed?.Invoke();}
    public void RetireCurrent(){generation++;Current=null;Changed?.Invoke();}
    private CompanyClientSelection RequireCurrent()
    {
        if(disposed || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal))
            throw new UnauthorizedAccessException("Current standard financial company/account visibility is required.");return selected;
    }
    public async Task LoadOrdersAsync(IWorkOrderServiceClient client,Func<Task<string?>> field)
    {
        if(Busy)throw new InvalidOperationException("A financial source read is already running.");RequireCurrent();Clear();var version=generation;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try
        {
            await list.LoadAsync(null,field,_=>client.GetWorkOrdersAsync());
            var closing=await field();if(disposed || version!=generation || snapshot!=company.Snapshot)return;
            if(list.FieldId!=closing){Clear();throw new InvalidOperationException("Current financial FIELD changed during work-order discovery.");}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task ReadAsync(IWorkOrderServiceClient client,string id)
    {
        if(Busy)throw new InvalidOperationException("A financial source read is already running.");var selected=RequireCurrent();
        if(!list.Confirmed || list.FieldId is not {} field || !list.Rows.Any(row=>row.Source=="WORK_ORDER" && row.InstanceId==id))throw new InvalidOperationException("Choose an original native order from the confirmed current FIELD population.");
        var query=new CompanyWorkOrderFinancialQuery(id,field);var version=generation;var snapshot=company.Snapshot;Current=null;Busy=true;Changed?.Invoke();
        try
        {
            var result=await client.ReadFinancialSourcesAsync(query);result.Require();
            if(result.Query!=query || result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original financial source company/account/query differs.");
            if(!disposed && version==generation && snapshot==company.Snapshot)Current=CompanyInventorySourceReview.Copy(result);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Clear;list.Dispose();generation++;Current=null;Changed=null;}
}
