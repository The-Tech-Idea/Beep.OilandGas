using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.Web.Services;
public sealed class WorkOrderListState : IDisposable
{
    private readonly CompanyClientContext company;private int generation;private bool disposed;
    public int Version=>generation;
    public string? FieldId {get;private set;}
    public bool Confirmed {get;private set;}
    public IReadOnlyList<WorkOrderSummary> Rows {get;private set;}=[];
    public WorkOrderListState(CompanyClientContext company){this.company=company;company.Changed+=Clear;}
    public void Clear(){++generation;Rows=[];FieldId=null;Confirmed=false;}
    public async Task LoadAsync(string? subtype,Func<Task<string?>> fieldLookup,Func<string?,Task<List<WorkOrderSummary>>> read)
    {
        if(disposed)return;Clear();var version=generation;var scope=company.Snapshot;if(scope.Selection is null)return;
        var field=await fieldLookup();if(disposed || version!=generation || scope!=company.Snapshot || string.IsNullOrWhiteSpace(field))return;
        var rows=await read(subtype);if(disposed || version!=generation || scope!=company.Snapshot)return;
        string[] states=[WorkOrderState.Draft,WorkOrderState.Scoped,WorkOrderState.Planned,WorkOrderState.InProgress,WorkOrderState.UnderReview,WorkOrderState.Completed,WorkOrderState.Cancelled];
        if(rows is null || rows.Any(row=>row is null || string.IsNullOrWhiteSpace(row.InstanceId) || row.FieldId!=field ||
            !(states.Contains(row.State,StringComparer.OrdinalIgnoreCase) || row.State==string.Empty && row.Source=="WORK_ORDER" && !string.IsNullOrWhiteSpace(row.EvidenceUnavailableReason)) || !string.IsNullOrWhiteSpace(subtype) && row.WoSubType!=subtype) ||
            rows.Select(row=>row.InstanceId).Distinct(StringComparer.Ordinal).Count()!=rows.Count)
            throw new InvalidOperationException("Work orders cannot be confirmed for the selected company, FIELD and filter.");
        foreach(var row in rows)RecordedWorkOrderComponentChecks.Require(row);
        Rows=rows.AsReadOnly();FieldId=field;Confirmed=true;
    }
    public bool Matches(WorkOrderSummary row,string? query)
    {
        if(!Confirmed || !Rows.Contains(row))return false;
        if(string.IsNullOrWhiteSpace(query))return true;
        query=query.Trim();
        bool Contains(string? value)=>value?.Contains(query,StringComparison.OrdinalIgnoreCase)==true;
        return Contains(row.InstanceId) || Contains(row.InstanceName) || Contains(row.WoSubType) || row.Components.Any(component=>Contains(component.ComponentId) || Contains(component.FieldId) || Contains(component.WellId) || Contains(component.EquipmentId));
    }
    public void Dispose(){disposed=true;company.Changed-=Clear;Clear();}
}
