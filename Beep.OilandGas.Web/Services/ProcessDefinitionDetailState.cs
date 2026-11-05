using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.LifeCycle.Models;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.Process;
namespace Beep.OilandGas.Web.Services;
public sealed class ProcessDefinitionDetailState:IDisposable
{
    private readonly CompanyClientContext company;
    private readonly HashSet<(string Actor,CompanyScope Company,string Field,string Definition,string Entity)> pending=[];
    private bool disposed;
    private int sourceVersion;
    public ProcessStartSourceReview? SourceReview {get;private set;}
    public void ClearSource(){++sourceVersion;SourceReview=null;}
    public int Version {get;private set;}
    public string? FieldId {get;private set;}
    public ProcessDefinition? Current {get;private set;}
    public string? Revision {get;private set;}
    public ProcessDefinitionDetailState(CompanyClientContext company){this.company=company;company.Changed+=Clear;}
    public void Clear(){++Version;ClearSource();FieldId=null;Current=null;Revision=null;}
    public async Task LoadAsync(string id,Func<Task<string?>> lookup,Func<string,Task<ProcessDefinition?>> read)
    {
        if(disposed)return;Clear();var version=Version;var scope=company.Snapshot;
        if(scope.Selection is null || string.IsNullOrWhiteSpace(id))return;
        var field=await lookup();if(disposed || version!=Version || scope!=company.Snapshot || string.IsNullOrWhiteSpace(field))return;
        var source=await read(id);if(disposed || version!=Version || scope!=company.Snapshot)return;
        RequireDefinition(source,id);var json=JsonSerializer.Serialize(source);Current=JsonSerializer.Deserialize<ProcessDefinition>(json)!;Revision=ProcessDefinitionRevision.FromJson(json);FieldId=field;
    }
    public static void RequireDefinition(ProcessDefinition? source,string id)
    {
        if(source is null || source.ProcessId!=id || string.IsNullOrWhiteSpace(source.ProcessName) || string.IsNullOrWhiteSpace(source.ProcessType) || string.IsNullOrWhiteSpace(source.EntityType) || source.Steps is null ||
            source.Steps.Any(row=>row is null || string.IsNullOrWhiteSpace(row.StepId) || string.IsNullOrWhiteSpace(row.StepName) || row.SequenceNumber<=0 || row.RequiredRoles is null || row.RequiredRoles.Any(string.IsNullOrWhiteSpace)) ||
            source.Steps.Select(row=>row.StepId).Distinct(StringComparer.Ordinal).Count()!=source.Steps.Count || source.Steps.Select(row=>row.SequenceNumber).Distinct().Count()!=source.Steps.Count)
            throw new InvalidOperationException("The configured definition and recorded steps could not be confirmed.");
    }
    public bool IsBlocked(string entity)=>company.Selection is {} selected && FieldId is {} field && Current is {} definition && pending.Contains((selected.UserId,selected.Scope,field,definition.ProcessId,entity));
    public ProcessInstanceRequest CreateStartRequest(string entity)
    {
        var scope=company.Snapshot;
        if(disposed || scope.Selection is null || Current is not {IsActive:true} definition || definition.Steps.Count==0 || FieldId is null || Revision is null || string.IsNullOrWhiteSpace(entity))
            throw new InvalidOperationException("Confirm the active original definition and explicit source entity. An unconfirmed earlier start cannot be repeated.");
        return new ProcessInstanceRequest{ProcessId=definition.ProcessId,EntityId=entity,EntityType=definition.EntityType,ExpectedFieldId=FieldId,ExpectedDefinitionRevision=Revision};
    }
    public async Task ReviewSourceAsync(string entity,Func<ProcessInstanceRequest,Task<ProcessStartSourceReview>> read)
    {
        ClearSource();var request=CreateStartRequest(entity);var version=Version;var source=sourceVersion;var scope=company.Snapshot;
        var review=await read(request);
        if(disposed || version!=Version || source!=sourceVersion || scope!=company.Snapshot)return;
        if(review is null)throw new InvalidOperationException("The recorded start source was empty.");
        review.Require(scope.Selection!.Scope,scope.Selection.UserId,request);SourceReview=review;
    }
    public Attempt Begin(string entity)
    {
        var request=CreateStartRequest(entity);var scope=company.Snapshot;
        if(SourceReview is null || IsBlocked(entity))throw new InvalidOperationException("Review the recorded source before starting. An unresolved start cannot be repeated.");
        SourceReview.Require(scope.Selection!.Scope,scope.Selection.UserId,request);request.ExpectedSourceRevision=SourceReview.Revision;
        var definition=Current!;
        pending.Add((scope.Selection.UserId,scope.Selection.Scope,FieldId!,definition.ProcessId,entity));return new(Version,scope.Version,scope.Selection,request);
    }
    public bool IsCurrent(Attempt attempt)=>!disposed && Version==attempt.Version && company.Snapshot==(attempt.CompanyVersion,attempt.Company) && FieldId==attempt.Request.ExpectedFieldId && Current?.ProcessId==attempt.Request.ProcessId && SourceReview?.Revision==attempt.Request.ExpectedSourceRevision;
    public void Confirm(Attempt attempt,ProcessInstance? response,ProcessInstance? readback)
    {
        if(!IsCurrent(attempt))throw new InvalidOperationException("The original start context is no longer current.");
        foreach(var row in new[]{response,readback})
            if(row is null || string.IsNullOrWhiteSpace(row.InstanceId) || row.ProcessId!=attempt.Request.ProcessId || row.FieldId!=attempt.Request.ExpectedFieldId || row.EntityId!=attempt.Request.EntityId || row.EntityType!=attempt.Request.EntityType || row.StartedBy!=attempt.Company.UserId)
                throw new InvalidOperationException("The returned and recorded process do not confirm the original start request.");
        if(response!.InstanceId!=readback!.InstanceId)throw new InvalidOperationException("Recorded instance identity differs from the start response.");
        pending.Remove((attempt.Company.UserId,attempt.Company.Scope,attempt.Request.ExpectedFieldId,attempt.Request.ProcessId,attempt.Request.EntityId));
    }
    public void Dispose(){disposed=true;company.Changed-=Clear;Clear();}
    public sealed record Attempt(int Version,long CompanyVersion,CompanyClientSelection Company,ProcessInstanceRequest Request);
}
