using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;

namespace Beep.OilandGas.DevelopmentPlanning.Models;

/// <summary>Task-bound read scope; opening assigned work does not create or change an active FIELD preference.</summary>
public sealed record DevelopmentAssignedReviewScope(
    [property:JsonRequired]CompanyScope Company,
    [property:JsonRequired]string ActorUserId,
    [property:JsonRequired]string FieldId,
    [property:JsonRequired]AssetDatabaseScope CoreDatabase,
    [property:JsonRequired]AssetDatabaseScope PlanningDatabase,
    [property:JsonRequired]AssetDatabaseScope LifecycleDatabase);

public sealed record DevelopmentAssignedReview(
    [property:JsonRequired]DevelopmentAssignedReviewScope Scope,
    [property:JsonRequired]FIELD_DEVELOPMENT_PLAN Plan,
    [property:JsonRequired]string SourceRevision,
    [property:JsonRequired]string InstanceId,
    [property:JsonRequired]string WorkflowName,
    [property:JsonRequired]string WorkflowVersion,
    [property:JsonRequired]string WorkflowStarterUserId,
    [property:JsonRequired]string StepId,
    [property:JsonRequired]string StepInstanceId,
    [property:JsonRequired]string StepName,
    [property:JsonRequired]string AssignedRole,
    [property:JsonRequired]string PersonaCode,
    [property:JsonRequired]string TaskId,
    [property:JsonRequired]string DefinitionRevision,
    [property:JsonRequired]string ProcessRevision,
    [property:JsonRequired]string TaskRevision,
    [property:JsonRequired]string Revision,
    [property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this with{Revision="",CheckedAtUtc=default}));
    public void Require(CompanyScope company,string actor,string instanceId)
    {
        APInvoiceOwnership.RequireScope(company);
        if(Scope is null || Scope.Company!=company || Scope.ActorUserId!=actor || InstanceId!=instanceId || Plan is null ||
            new[]{actor,Scope.FieldId,InstanceId,WorkflowStarterUserId,StepId,StepInstanceId,AssignedRole,PersonaCode,TaskId,WorkflowVersion}.Any(value=>!LeaseSummaryValidator.Identifier(value)) ||
            PersonaCode.Length>64 || new[]{Scope.CoreDatabase,Scope.PlanningDatabase,Scope.LifecycleDatabase}.Any(database=>database is null ||
                !LeaseSummaryValidator.Identifier(database.ConnectionName) || !LeaseSummaryValidator.Identifier(database.Fingerprint)) ||
            new[]{WorkflowName,StepName}.Any(name=>string.IsNullOrWhiteSpace(name) || name.Length>256 || name.Any(char.IsControl)) ||
            actor==WorkflowStarterUserId || actor==Plan.ROW_CREATED_BY ||
            new[]{SourceRevision,DefinitionRevision,ProcessRevision,TaskRevision,Revision}.Any(value=>!ProcessDefinitionRevision.IsValid(value)) ||
            CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The assigned development review source, scope or independent actor is unavailable.");
        DevelopmentPlanDraftReview.RequirePopulation([Plan],company,Scope.FieldId,Plan.FDP_ID);
        if(SourceRevision!=DevelopmentPlanSourceReview.RevisionFor(company,Scope.CoreDatabase,Scope.FieldId,Scope.PlanningDatabase,Plan.FDP_ID,Plan) || Revision!=ContentRevision())
            throw new InvalidOperationException("The assigned development review changed. Reload its recorded task and source.");
    }
}
