using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.Web.Services;

/// <summary>Page-lifetime protection; not a durable receipt or atomic provider transition.</summary>
public sealed class WorkOrderDetailState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly HashSet<(string Actor, Beep.OilandGas.Models.Data.AccessControl.CompanyScope Company, string Field, string Id)> uncertain=[];
    private bool disposed;
    public long Version { get; private set; }
    public Beep.OilandGas.ProductionOperations.Models.WorkOrderProcessReview? Processes { get; private set; }
    public WorkOrderDetailModel? Current { get; private set; }
    public WorkOrderDetailState(CompanyClientContext company){this.company=company;company.Changed+=Clear;}
    public void Clear(){++Version;Current=null;Processes=null;}
    public bool Blocked => Current is {} row && company.Selection is {} c && uncertain.Contains((c.UserId,c.Scope,row.FieldId,row.InstanceId));
    public async Task LoadAsync(string id,Func<Task<string?>> fieldLookup,Func<string,Task<WorkOrderDetailModel?>> read)
    {
        if(disposed)return;Clear();var generation=Version;var scope=company.Snapshot;if(scope.Selection is null || string.IsNullOrWhiteSpace(id))return;
        var field=await fieldLookup();if(disposed || generation!=Version || scope!=company.Snapshot)return;
        if(string.IsNullOrWhiteSpace(field))throw new InvalidOperationException("Confirm a FIELD before reading a work order.");
        var row=await read(id);if(disposed || generation!=Version || scope!=company.Snapshot)return;
        if(row is null || row.InstanceId!=id || row.FieldId!=field || (string.IsNullOrWhiteSpace(row.State) && !(row.State==string.Empty && row.Source=="WORK_ORDER" && !string.IsNullOrWhiteSpace(row.EvidenceUnavailableReason))))
            throw new InvalidOperationException("Recorded work-order detail could not be confirmed for this company, FIELD and identity.");
        RecordedWorkOrderComponentChecks.Require(row);
        Current=row;
    }
    public async Task LoadProcessesAsync(Func<Task<Beep.OilandGas.ProductionOperations.Models.WorkOrderProcessReview>> read)
    {
        Processes=null;var source=Current;var generation=Version;var scope=company.Snapshot;
        if(disposed || source is null || scope.Selection is null)return;
        var review=await read();if(disposed || generation!=Version || scope!=company.Snapshot || Current!=source)return;
        if(review is null || review.Company!=scope.Selection.Scope || review.Actor!=scope.Selection.UserId || review.FieldId!=source.FieldId || review.WorkOrderId!=source.InstanceId ||
           review.RetrievedAtUtc==default || review.RetrievedAtUtc.Offset!=TimeSpan.Zero || review.Links is null ||
           review.Links.Any(x=>x is null || string.IsNullOrWhiteSpace(x.InstanceId) || string.IsNullOrWhiteSpace(x.ProcessId) || string.IsNullOrWhiteSpace(x.ProcessName) || string.IsNullOrWhiteSpace(x.Status)) || review.Links.Select(x=>x.InstanceId).Distinct(StringComparer.Ordinal).Count()!=review.Links.Count)
            throw new InvalidOperationException("Work-order process evidence does not match the original company, FIELD and order.");
        Processes=review;
    }
    public Attempt Begin(string destination,string? notes,IReadOnlyList<string> allowed)
    {
        var snapshot=company.Snapshot;var row=Current;
        if(disposed || snapshot.Selection is null || row is null || string.IsNullOrWhiteSpace(row.State) || Blocked || string.IsNullOrWhiteSpace(destination) ||
           destination==row.State || !allowed.Contains(destination,StringComparer.Ordinal))
            throw new InvalidOperationException("Review the current order and permitted transition before submitting. An unconfirmed earlier outcome cannot be retried.");
        var request=new TransitionWorkOrderRequest{ExpectedFieldId=row.FieldId,ExpectedState=row.State,ToState=destination,Notes=notes};
        var attempt=new Attempt(Version,snapshot.Version,snapshot.Selection,row.InstanceId,row.WoSubType,request);
        uncertain.Add(Key(attempt));return attempt;
    }
    public bool IsCurrent(Attempt attempt,string id)=>!disposed && Version==attempt.Version && company.Snapshot==(attempt.CompanyVersion,attempt.Company) && id==attempt.Id;
    public void Confirm(Attempt attempt,WorkOrderSummary? response,WorkOrderDetailModel? readback)
    {
        Require(attempt,response);Require(attempt,readback);uncertain.Remove(Key(attempt));
    }
    private static void Require(Attempt a,WorkOrderSummary? row)
    {
        if(row is null || row.InstanceId!=a.Id || row.FieldId!=a.Request.ExpectedFieldId || row.WoSubType!=a.Subtype || row.State!=a.Request.ToState)
            throw new InvalidOperationException("The transition response and recorded reload must confirm the original order and requested state. Review its outcome before retrying.");
    }
    private static (string,Beep.OilandGas.Models.Data.AccessControl.CompanyScope,string,string) Key(Attempt a)=>(a.Company.UserId,a.Company.Scope,a.Request.ExpectedFieldId,a.Id);
    public void Dispose(){disposed=true;company.Changed-=Clear;Clear();}
    public sealed record Attempt(long Version,long CompanyVersion,CompanyClientSelection Company,string Id,string Subtype,TransitionWorkOrderRequest Request);
}
