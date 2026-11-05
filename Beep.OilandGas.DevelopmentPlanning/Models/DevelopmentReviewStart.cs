using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beep.OilandGas.DevelopmentPlanning.Models;

public sealed record DevelopmentReviewRecipientStep(
    [property:JsonRequired]string StepId,
    [property:JsonRequired]string AssignedRole,
    [property:JsonRequired]IReadOnlyList<RepositoryCompanyRoleRecipient> Recipients,
    [property:JsonRequired]string AuthorityRevision);

/// <summary>The frozen source, definition, recipients and targets accepted by the planner before transport.</summary>
public sealed record DevelopmentReviewStart(
    [property:JsonRequired]DevelopmentPlanSourceReview Source,
    [property:JsonRequired]AssetDatabaseScope LifecycleDatabase,
    [property:JsonRequired]DevelopmentReviewWorkflowChoice Workflow,
    [property:JsonRequired]IReadOnlyList<DevelopmentReviewRecipientStep> ReceivingSteps,
    [property:JsonRequired]Guid IntentId)
{
    public const string PayloadKey="DevelopmentReviewStart";
    public const string InstancePrefix="development-review-";
    public string InstanceId(){Require();return InstancePrefix+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Source.Context.Company,IntentId}));}
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
    public void Require()
    {
        if(Source is null || Source.Context is null || Source.Plan is null || Workflow is null || LifecycleDatabase is null || IntentId==Guid.Empty ||
            !LeaseSummaryValidator.Identifier(LifecycleDatabase.ConnectionName) || !LeaseSummaryValidator.Identifier(LifecycleDatabase.Fingerprint) ||
            ReceivingSteps is null || ReceivingSteps.Count!=Workflow.Steps?.Count)
            throw new InvalidOperationException("The original development source, workflow, recipients and lifecycle target are required.");
        Source.Require(Source.Context,Source.PlanId);Workflow.Require();
        foreach(var step in Workflow.Steps)
        {
            var matches=ReceivingSteps.Where(row=>row is not null && row.StepId==step.StepId).ToArray();
            if(matches.Length!=1 || matches[0].AssignedRole!=step.AssignedRole || !ProcessDefinitionRevision.IsValid(matches[0].AuthorityRevision) ||
                matches[0].Recipients is null || matches[0].Recipients.Count is <1 or >256 ||
                matches[0].Recipients.Any(row=>row is null || !LeaseSummaryValidator.Identifier(row.UserId) || !LeaseSummaryValidator.Identifier(row.PersonaCode) || row.PersonaCode.Length>64 ||
                    row.UserId==Source.Plan.ROW_CREATED_BY || row.UserId==Source.Context.ActorUserId) ||
                matches[0].Recipients.Select(row=>row.UserId).Distinct(StringComparer.Ordinal).Count()!=matches[0].Recipients.Count ||
                !matches[0].Recipients.Select(row=>row.PersonaCode).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)
                    .SequenceEqual(step.PersonaCodes.Order(StringComparer.Ordinal),StringComparer.Ordinal))
                throw new InvalidOperationException("Every configured stage needs independent current company recipients matching its reviewed personas.");
        }
        foreach(var role in ReceivingSteps.GroupBy(step=>step.AssignedRole,StringComparer.Ordinal))
            if(role.Select(step=>JsonSerializer.Serialize(step.Recipients)).Distinct(StringComparer.Ordinal).Count()!=1 || role.Select(step=>step.AuthorityRevision).Distinct(StringComparer.Ordinal).Count()!=1)
                throw new InvalidOperationException("Repeated workflow roles must retain the same source-qualified recipient authority.");
        if(JsonSerializer.Serialize(this).Length>262144)throw new InvalidOperationException("The reviewed development handoff exceeds its bounded payload.");
    }
    public void RequireSource(DevelopmentPlanSourceReview current)
    {
        Require();current.Require(current.Context,Source.PlanId);
        if(current.Plan is null || current.Context.Company!=Source.Context.Company || current.Context.FieldId!=Source.Context.FieldId ||
            current.Context.Database!=Source.Context.Database || current.PlanningDatabase!=Source.PlanningDatabase || current.Revision!=Source.Revision)
            throw new InvalidOperationException("The original recorded development source or its database binding changed.");
    }
    public static DevelopmentReviewStart FromPayload(string json,string instanceId,string fieldId,string planId)
    {
        using var document=JsonDocument.Parse(json);
        if(!document.RootElement.TryGetProperty("Data",out var data) || !data.TryGetProperty(PayloadKey,out var value))
            throw new InvalidOperationException("The recorded development process has no original reviewed ownership.");
        var intent=value.Deserialize<DevelopmentReviewStart>()??throw new InvalidOperationException("The development handoff payload is unavailable.");intent.Require();
        if(intent.InstanceId()!=instanceId || intent.Source.PlanId!=planId || intent.Source.Context.FieldId!=fieldId)
            throw new InvalidOperationException("The recorded development handoff ownership differs from its source.");
        return intent;
    }
}

public sealed record DevelopmentReviewSelection(
    [property:JsonRequired]string SourceRevision,
    [property:JsonRequired]string CatalogRevision,
    [property:JsonRequired]string ProcessId);

public sealed record DevelopmentReviewStartReceipt(
    [property:JsonRequired]string IntentRevision,
    [property:JsonRequired]string InstanceId,
    [property:JsonRequired]bool Recorded,
    [property:JsonRequired]string? StepInstanceId,
    [property:JsonRequired]IReadOnlyList<string> TaskIds,
    [property:JsonRequired]string RecordedRevision,
    [property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(DevelopmentReviewStart original)
    {
        original.Require();
        if(IntentRevision!=original.Revision() || InstanceId!=original.InstanceId() || TaskIds is null ||
            TaskIds.Distinct(StringComparer.Ordinal).Count()!=TaskIds.Count || TaskIds.Any(id=>!LeaseSummaryValidator.Identifier(id)) ||
            !ProcessDefinitionRevision.IsValid(RecordedRevision) || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero ||
            (Recorded? !LeaseSummaryValidator.Identifier(StepInstanceId) || TaskIds.Count is <1 or >128 : StepInstanceId is not null || TaskIds.Count!=0))
            throw new InvalidOperationException("The development review outcome does not confirm the retained original intent.");
    }
}

/// <summary>Internal workflow assignment lookup through the canonical app asset-grant owner; no grant or business write.</summary>
public interface IWorkflowRecipientAssetReader
{
    Task<IReadOnlyList<AppUserAssetAccess>> ReadExecutionRecipientFieldGrantsAsync(DevelopmentAssignedReviewScope current,IReadOnlyList<string> users);
    Task<IReadOnlyList<AppUserAssetAccess>> ReadProgramReviewRecipientFieldGrantsAsync(DevelopmentAssignedReviewScope current,IReadOnlyList<string> users);
    Task<IReadOnlyList<AppUserAssetAccess>> ReadCompletionReviewRecipientFieldGrantsAsync(DevelopmentAssignedReviewScope current,IReadOnlyList<string> users);
    Task<IReadOnlyList<AppUserAssetAccess>> ReadProgramRecipientFieldGrantsAsync(DevelopmentAssignedReviewScope current,IReadOnlyList<string> users);
    Task<IReadOnlyList<AppUserAssetAccess>> ReadInvestmentReviewRecipientFieldGrantsAsync(DevelopmentAssignedReviewScope current,IReadOnlyList<string> users);
    Task<IReadOnlyList<AppUserAssetAccess>> ReadInvestmentRecipientFieldGrantsAsync(DevelopmentAssignedReviewScope current,IReadOnlyList<string> users);
    Task<IReadOnlyList<AppUserAssetAccess>> ReadRecipientFieldGrantsAsync(FieldSelectionReview current,IReadOnlyList<string> users);
    Task<IReadOnlyList<AppUserAssetAccess>> ReadReviewRecipientFieldGrantsAsync(DevelopmentAssignedReviewScope current,IReadOnlyList<string> users);
}
