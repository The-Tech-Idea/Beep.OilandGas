using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;
/// <summary>Retains circuit-lifetime uncertain decisions; durable API receipts remain required.</summary>
public sealed class ProductionInterventionDecisionState(CompanyClientContext company)
{
    private readonly Dictionary<(string Actor,CompanyScope Company,string Field,string Well),Attempt> pending=[];
    private readonly Dictionary<(string Actor,CompanyScope Company,string Field,string Well),ProductionInterventionDecisionResult> acknowledgements=[];
    private readonly HashSet<(string Actor,CompanyScope Company,string Field,string Well)> reading=[];
    private static ProductionInterventionDecisionResult Copy(ProductionInterventionDecisionResult value)=>System.Text.Json.JsonSerializer.Deserialize<ProductionInterventionDecisionResult>(System.Text.Json.JsonSerializer.Serialize(value))!;
    private static Attempt Copy(Attempt value)=>value with{Request=System.Text.Json.JsonSerializer.Deserialize<ProductionInterventionDecisionRequest>(System.Text.Json.JsonSerializer.Serialize(value.Request))!};
    public bool IsBlocked(string field,string well)=>company.Selection is {} c && pending.ContainsKey((c.UserId,c.Scope,field,well));
    public Attempt? Original(string field,string well)=>company.Selection is {} c && pending.TryGetValue((c.UserId,c.Scope,field,well),out var retained)?Copy(retained):null;
    public bool HasAcknowledgement(string field,string well)=>company.Selection is {} c && acknowledgements.ContainsKey((c.UserId,c.Scope,field,well));
    public void RetainAcknowledgement(Attempt attempt,ProductionInterventionDecisionResult result)
    {
        RequireRetained(attempt);ProductionInterventionDecisionChecks.RequireAcknowledgement(attempt.Request,result,attempt.Well);
        var key=(attempt.Company.UserId,attempt.Company.Scope,attempt.Request.FieldId,attempt.Well);
        if(acknowledgements.TryGetValue(key,out var previous) && System.Text.Json.JsonSerializer.Serialize(previous)!=System.Text.Json.JsonSerializer.Serialize(result))throw new InvalidOperationException("The original decision acknowledgement differs.");
        acknowledgements[key]=Copy(result);
    }
    public async Task<ProductionInterventionDecisionResult> ReviewAcknowledgedOutcomeAsync(string field,string well,Func<Task<List<ProductionInterventionCandidateDto>>> load)
    {
        var snapshot=company.Snapshot;if(snapshot.Selection is not {} selected)throw new InvalidOperationException("Return to the original decision company/account.");var key=(selected.UserId,selected.Scope,field,well);
        if(!pending.TryGetValue(key,out var retained) || !acknowledgements.TryGetValue(key,out var acknowledgement) || !reading.Add(key))throw new InvalidOperationException("An original retained acknowledgement is required; wait for any current review.");
        try
        {
            var records=await load();if(snapshot!=company.Snapshot)throw new OperationCanceledException("The original review context changed.");ProductionInterventionCandidateChecks.Require(records,selected.Scope,selected.UserId,field);
            ProductionInterventionDecisionChecks.RequireRecordedOutcome(retained.Request,acknowledgement,records.SingleOrDefault(row=>row.WellId==well),selected.Scope,selected.UserId,well);
            if(snapshot!=company.Snapshot || !pending.TryGetValue(key,out var current) || !ReferenceEquals(retained,current))throw new OperationCanceledException("The retained decision changed during read-back.");
            pending.Remove(key);acknowledgements.Remove(key);return Copy(acknowledgement);
        }
        finally{reading.Remove(key);}
    }
    public Attempt Begin(string well,ProductionInterventionDecisionRequest source)
    {
        var snapshot=company.Snapshot;
        if(snapshot.Selection is null || string.IsNullOrWhiteSpace(well)) throw new InvalidOperationException("Confirm company and well before deciding.");
        var decision=ProductionInterventionDecisionChecks.Require(source,source.FieldId);
        var key=(snapshot.Selection.UserId,snapshot.Selection.Scope,source.FieldId,well);
        if(pending.ContainsKey(key)) throw new InvalidOperationException("An earlier decision outcome is unconfirmed. Review recorded source before repeating it.");
        if(source.IntentId!=Guid.Empty){FieldSelectionReviewValidator.RequireReview(source.Context,snapshot.Selection.Scope,snapshot.Selection.UserId);if(source.Context!.FieldId!=source.FieldId)throw new InvalidOperationException("The original native FIELD review is required.");}
        var attempt=new Attempt(snapshot.Version,snapshot.Selection,well,new(){IntentId=source.IntentId,Context=source.Context,ExpectedSourceRevision=source.ExpectedSourceRevision,FieldId=source.FieldId,Jurisdiction=source.Jurisdiction,Decision=decision,Note=source.Note,
            InterventionType=source.InterventionType,Problem=source.Problem,EstimatedCostUsd=source.EstimatedCostUsd,
            WorkOrderNumber=source.WorkOrderNumber,CreateWorkOrder=source.CreateWorkOrder,LinkAfe=source.LinkAfe,WorkOrderId=source.WorkOrderId,AfeId=source.AfeId});
        pending.Add(key,Copy(attempt));return Copy(attempt);
    }
    public bool IsCurrent(Attempt a,string field)=>company.Snapshot==(a.Version,a.Company) && field==a.Request.FieldId;
    public async Task<ProductionInterventionDecisionResult?> RecoverNativeOutcomeAsync(string field,string well,Guid intentId,Func<Task<ProductionInterventionNativeRecovery>> load)
    {
        var snapshot=company.Snapshot;
        if(snapshot.Selection is not {} selected || intentId==Guid.Empty)throw new InvalidOperationException("Return to the original native decision identity/company/account.");
        var key=(selected.UserId,selected.Scope,field,well);
        if(!reading.Add(key))throw new InvalidOperationException("Wait for the original decision review to finish.");
        try
        {
            var recovery=await load();
            if(snapshot!=company.Snapshot)throw new OperationCanceledException("The original recovery context changed.");
            recovery.Require();
            if(recovery.Company!=selected.Scope || recovery.Actor!=selected.UserId || recovery.IntentId!=intentId || recovery.Intent is null)
                throw new InvalidOperationException("The original native reservation has not been confirmed. Keep its identity and review again; no decision is submitted.");
            var command=recovery.Intent.Restore();
            if(command.Review.Well.UWI!=well || command.Review.Context.FieldId!=field)throw new InvalidOperationException("The private original decision belongs to another FIELD/WELL.");
            if(pending.TryGetValue(key,out var original) && System.Text.Json.JsonSerializer.Serialize(original.Request)!=System.Text.Json.JsonSerializer.Serialize(command.Decision))
                throw new InvalidOperationException("The recorded native command differs from the original retained request.");
            if(recovery.Outcome is null){pending[key]=Copy(new Attempt(snapshot.Version,selected,well,command.Decision));return null;}
            var result=recovery.Outcome.Restore(recovery.Intent);
            ProductionInterventionDecisionChecks.RequireAcknowledgement(command.Decision,result,well);
            if(acknowledgements.TryGetValue(key,out var previous) && System.Text.Json.JsonSerializer.Serialize(previous)!=System.Text.Json.JsonSerializer.Serialize(result))
                throw new InvalidOperationException("The native result differs from the original acknowledgement.");
            if(snapshot!=company.Snapshot)throw new OperationCanceledException("The original native recovery changed.");
            pending.Remove(key);acknowledgements.Remove(key);return Copy(result);
        }
        finally{reading.Remove(key);}
    }
    public void Confirm(Attempt a,ProductionInterventionDecisionResult? result,ProductionInterventionCandidateDto? readback)
    {
        RequireRetained(a);
        ProductionInterventionDecisionChecks.RequireRecordedOutcome(a.Request,result,readback,a.Company.Scope,a.Company.UserId,a.Well);
        var key=(a.Company.UserId,a.Company.Scope,a.Request.FieldId,a.Well);pending.Remove(key);acknowledgements.Remove(key);
    }
    private void RequireRetained(Attempt a)
    {
        var key=(a.Company.UserId,a.Company.Scope,a.Request.FieldId,a.Well);
        if(!IsCurrent(a,a.Request.FieldId) || !pending.TryGetValue(key,out var retained) || System.Text.Json.JsonSerializer.Serialize(a)!=System.Text.Json.JsonSerializer.Serialize(retained))
            throw new InvalidOperationException("The exact retained original decision and current account/company are required.");
    }
    public sealed record Attempt(long Version,CompanyClientSelection Company,string Well,ProductionInterventionDecisionRequest Request);
}
