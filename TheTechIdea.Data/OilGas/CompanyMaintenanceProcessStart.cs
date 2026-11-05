using System.Text.Json;

namespace TheTechIdea.Data.OilGas;

/// <summary>Original reviewed maintenance handoff, retained in the existing process payload.</summary>
public sealed record CompanyMaintenanceProcessStart(FieldSelectionReview Context,Guid IntentId,string ProcessId,
    string DefinitionRevision,ProductionInterventionMaintenanceLink Source)
{
    public const string PayloadKey="CompanyMaintenanceStart";
    public const string InstancePrefix="maintenance-process-";
    public void Require()
    {
        if(Context is null || Source is null || Context.Database is null || IntentId==Guid.Empty ||
            !LeaseSummaryValidator.Identifier(ProcessId) || !ProcessDefinitionRevision.IsValid(DefinitionRevision) ||
            !LeaseSummaryValidator.Identifier(Context.ActorUserId) || !LeaseSummaryValidator.Identifier(Context.FieldId) ||
            !LeaseSummaryValidator.Identifier(Context.Database.ConnectionName) || string.IsNullOrWhiteSpace(Context.Database.Fingerprint) ||
            string.IsNullOrWhiteSpace(Context.Revision))throw new InvalidOperationException("The original maintenance company/account/FIELD/database/definition review is required.");
        APInvoiceOwnership.RequireScope(Context.Company);
        Source.Require(Context.Company,Context.ActorUserId,Context.FieldId!,Source.WorkOrderId,Source.WellId,Source.DecisionId);
        if(!LeaseSummaryValidator.Identifier(Source.WorkOrderId) || !LeaseSummaryValidator.Identifier(Source.WellId) || JsonSerializer.Serialize(this).Length>8192)
            throw new InvalidOperationException("The recorded intervention handoff is incomplete or exceeds its bounded payload.");
    }
    public string InstanceId(){Require();return InstanceIdFor(Context.Company,IntentId);}
    public static string InstanceIdFor(CompanyScope company,Guid intentId)
    {
        APInvoiceOwnership.RequireScope(company);if(intentId==Guid.Empty)throw new ArgumentException("The original maintenance assignment reference is required.");
        return InstancePrefix+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,IntentId=intentId}));
    }
    public void RequireSource(ProductionInterventionMaintenanceLink current)
    {
        Require();current.Require(Context.Company,Context.ActorUserId,Context.FieldId!,Source.WorkOrderId,Source.WellId,Source.DecisionId);
        if(Source with{RetrievedAtUtc=current.RetrievedAtUtc}!=current)throw new InvalidOperationException("The original native approval/order/component receipt changed.");
    }
    public static CompanyMaintenanceProcessStart FromPayload(string json,string id,string field,string order)
    {
        using var document=JsonDocument.Parse(json);
        if(!document.RootElement.TryGetProperty("Data",out var data) || !data.TryGetProperty(PayloadKey,out var value))
            throw new InvalidOperationException("The recorded maintenance handoff has no original company ownership.");
        var intent=value.Deserialize<CompanyMaintenanceProcessStart>()??throw new InvalidOperationException("The recorded maintenance handoff is unavailable.");intent.Require();
        if(intent.InstanceId()!=id || intent.Context.FieldId!=field || intent.Source.WorkOrderId!=order)
            throw new InvalidOperationException("The maintenance process differs from its original company/FIELD/order identity.");
        return intent;
    }
    public static bool HasPayload(string? json)
    {
        if(string.IsNullOrEmpty(json))return false;
        using var document=JsonDocument.Parse(json);
        return document.RootElement.ValueKind==JsonValueKind.Object && document.RootElement.TryGetProperty("Data",out var data) && data.ValueKind==JsonValueKind.Object && data.TryGetProperty(PayloadKey,out _);
    }
}

public sealed record CompanyMaintenanceProcessStartReceipt(CompanyScope Company,string Actor,string FieldId,Guid IntentId,
    string InstanceId,string WorkOrderId,Guid DecisionId,string StepInstanceId,IReadOnlyList<string> TaskIds,string RecordedRevision)
{
    public void Require(CompanyMaintenanceProcessStart intent)
    {
        intent.Require();
        if(Company!=intent.Context.Company || Actor!=intent.Context.ActorUserId || FieldId!=intent.Context.FieldId || IntentId!=intent.IntentId ||
            InstanceId!=intent.InstanceId() || WorkOrderId!=intent.Source.WorkOrderId || DecisionId!=intent.Source.DecisionId ||
            !LeaseSummaryValidator.Identifier(StepInstanceId) || TaskIds is null || TaskIds.Count is <1 or >128 ||
            TaskIds.Any(id=>!LeaseSummaryValidator.Identifier(id)) || TaskIds.Distinct(StringComparer.Ordinal).Count()!=TaskIds.Count || !ProcessDefinitionRevision.IsValid(RecordedRevision))
            throw new InvalidOperationException("The maintenance assignment receipt differs from the retained original intent.");
    }
}

/// <summary>API-owned lifecycle authority is rechecked inside its transaction; production reads are outside it.</summary>
public interface ICompanyMaintenanceProcessCommands
{
    Task<CompanyMaintenanceProcessStartReceipt> StartMaintenanceProcessAsync(CompanyMaintenanceProcessStart intent,
        Func<Task> requireLifecycleAuthority,Func<Task<ProductionInterventionMaintenanceLink>> readOriginalSource);
    Task<CompanyMaintenanceProcessStartReceipt> RecoverMaintenanceProcessAsync(CompanyMaintenanceProcessStart intent,
        Func<Task> requireLifecycleAuthority,Func<Task<ProductionInterventionMaintenanceLink>> readOriginalSource);
    Task<CompanyMaintenanceProcessStart> ReadOriginalMaintenanceIntentAsync(FieldSelectionReview current,CompanyMaintenanceRecoveryQuery query,Func<Task> requireLifecycleAuthority);
}

public sealed record CompanyMaintenanceSourceQuery(Guid DecisionId,string WellId,string WorkOrderId)
{
    public void Require()=>new CompanyMaintenanceProcessStartQuery("source",DecisionId,WellId,WorkOrderId).Require();
}
public sealed record CompanyMaintenanceStartCatalog(FieldSelectionReview Context,ProductionInterventionMaintenanceLink Source,IReadOnlyList<CompanyMaintenanceProcessStartReview> Choices)
{
    public void Require(CompanyScope company,string actor,CompanyMaintenanceSourceQuery query)
    {
        query.Require();
        FieldSelectionReviewValidator.RequireReview(Context,company,actor);
        if(Source is null || Context is null || Context.Company!=company || Context.ActorUserId!=actor || Choices is null || Choices.Count>64 || Choices.Any(choice=>choice is null) || Choices.Select(choice=>choice.ProcessId).Distinct(StringComparer.Ordinal).Count()!=Choices.Count)
            throw new InvalidOperationException("The maintenance workflow choices differ from the original company/account.");
        Source.Require(company,actor,Context.FieldId!,query.WorkOrderId,query.WellId,query.DecisionId);
        foreach(var choice in Choices)
        {
            if(choice.Context!=Context)throw new InvalidOperationException("Maintenance workflow choices have different authority reviews.");
            choice.CreateIntent(Guid.NewGuid()).RequireSource(Source);
        }
    }
}
public sealed record CompanyMaintenanceRecoveryQuery(Guid IntentId,Guid DecisionId,string FieldId,string WellId,string WorkOrderId)
{
    public void Require()
    {
        new CompanyMaintenanceSourceQuery(DecisionId,WellId,WorkOrderId).Require();
        if(IntentId==Guid.Empty || !LeaseSummaryValidator.Identifier(FieldId))throw new ArgumentException("The original assignment reference and FIELD are required.");
    }
}
public sealed record CompanyMaintenanceRecovery(CompanyMaintenanceProcessStart Intent,CompanyMaintenanceProcessStartReceipt Receipt)
{
    public void Require(CompanyScope company,string actor,CompanyMaintenanceRecoveryQuery query)
    {
        query.Require();if(Intent is null || Receipt is null)throw new InvalidOperationException("The original recovered assignment and receipt are unavailable.");Intent.Require();Receipt.Require(Intent);
        if(Intent.Context.Company!=company || Intent.Context.ActorUserId!=actor || Intent.IntentId!=query.IntentId || Intent.Context.FieldId!=query.FieldId || Intent.Source.DecisionId!=query.DecisionId || Intent.Source.WellId!=query.WellId || Intent.Source.WorkOrderId!=query.WorkOrderId)
            throw new InvalidOperationException("The recovered assignment differs from its original company/account/FIELD/source reference.");
    }
}

public sealed record CompanyMaintenanceProcessStartQuery(string ProcessId,Guid DecisionId,string WellId,string WorkOrderId)
{
    public void Require()
    {
        if(DecisionId==Guid.Empty || new[]{ProcessId,WellId,WorkOrderId}.Any(value=>!LeaseSummaryValidator.Identifier(value)))
            throw new ArgumentException("The configured workflow and original decision/order/well identities are required.");
    }
}

public sealed record CompanyMaintenanceProcessStartReview(FieldSelectionReview Context,string ProcessId,string DefinitionRevision,
    ProductionInterventionMaintenanceLink Source,string ProcessName,string ReceivingRole)
{
    public CompanyMaintenanceProcessStart CreateIntent(Guid id)
    {
        var value=new CompanyMaintenanceProcessStart(Context,id,ProcessId,DefinitionRevision,Source);value.Require();
        if(string.IsNullOrWhiteSpace(ProcessName) || !LeaseSummaryValidator.Identifier(ReceivingRole))throw new InvalidOperationException("The configured receiving workflow and role are unavailable.");
        return value;
    }
    public void RequireIntent(CompanyMaintenanceProcessStart intent)
    {
        intent.Require();
        if(CreateIntent(intent.IntentId) with{Source=intent.Source}!=intent)throw new InvalidOperationException("The original maintenance authority or definition review changed.");
        intent.RequireSource(Source);
    }
}
