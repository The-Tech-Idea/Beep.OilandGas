using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beep.OilandGas.DevelopmentPlanning.Models;

public sealed record DevelopmentInvestmentHandoffSelection([property:JsonRequired]string RoutingRevision,[property:JsonRequired]string ProcessId);

/// <summary>Frozen source/configuration/recipient proof for the canonical lifecycle owner, retained before transport.</summary>
public sealed record DevelopmentInvestmentHandoff([property:JsonRequired]DevelopmentInvestmentRoutingReview Review,
    [property:JsonRequired]DevelopmentReviewWorkflowChoice Workflow,[property:JsonRequired]Guid IntentId)
{
    public const string PayloadKey="DevelopmentInvestmentHandoff";
    public const string InstancePrefix="investment-handoff-";
    public const int MaximumBodyLength=4*1024*1024;
    public DevelopmentAssignedReviewScope Scope()=>Review.Source.Current.Source.Review.Scope;
    public DevelopmentInvestmentRoutingOption Option()=>Review.Options.Single(option=>option.ProcessId==Workflow.ProcessId);
    public void Require()
    {
        if(Review?.Source?.Current?.Source?.Review?.Scope is not {} scope || Workflow is null || IntentId==Guid.Empty)throw new InvalidOperationException("The original investment source, workflow and retained intent are required.");
        Review.Require(scope.Company,scope.ActorUserId,Review.Source.Current.Source.Review.InstanceId,Review.Source.AfeId);Workflow.Require();var option=Option();
        if(Review.ExistingHandoffId is not null || Review.Source.BlocksRouting() || !option.Ready || option.DefinitionRevision!=Workflow.DefinitionRevision || option.Name!=Workflow.ProcessName || option.Version!=Workflow.Version ||
            option.Stages.Count!=Workflow.Steps.Count || option.Stages.Where((stage,index)=>stage.StepId!=Workflow.Steps[index].StepId || stage.AssignedRole!=Workflow.Steps[index].AssignedRole ||
                !stage.Recipients.Select(recipient=>recipient.PersonaCode).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).SequenceEqual(Workflow.Steps[index].PersonaCodes.Order(StringComparer.Ordinal),StringComparer.Ordinal)).Any() ||
            JsonSerializer.Serialize(this).Length>MaximumBodyLength)
            throw new InvalidOperationException("The exact ready investment workflow and its independent current recipients must be confirmed.");
        foreach(var role in option.Stages.GroupBy(stage=>stage.AssignedRole,StringComparer.Ordinal))
            if(role.Select(stage=>JsonSerializer.Serialize(stage.Recipients)).Distinct(StringComparer.Ordinal).Count()!=1 || role.Select(stage=>stage.AuthorityRevision).Distinct(StringComparer.Ordinal).Count()!=1)
                throw new InvalidOperationException("Repeated investment roles must keep the same reviewed recipient authority.");
    }
    public string InstanceId(){Require();return InstancePrefix+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Scope().Company,IntentId}));}
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
    public void RequireSource(DevelopmentInvestmentReview current,bool originalActor=true)
    {
        Require();current.Require();var scope=current.Current.Source.Review.Scope;
        if((originalActor?scope:scope with{ActorUserId=Scope().ActorUserId})!=Scope() || current.Current.BudgetDatabase!=Review.Source.Current.BudgetDatabase || current.Afe()!=Review.Source.Afe() ||
            DevelopmentPlanRelease.ReviewEvidenceRevision(current.Current.Source.Review)!=DevelopmentPlanRelease.ReviewEvidenceRevision(Review.Source.Current.Source.Review) ||
            JsonSerializer.Serialize(current.Current.Source.Baseline)!=JsonSerializer.Serialize(Review.Source.Current.Source.Baseline))
            throw new InvalidOperationException("The original company/account/FIELD, approved FDP or immutable native AFE evidence changed.");
    }
    public static bool HasPayload(string? payload)
    {
        if(string.IsNullOrWhiteSpace(payload))return false;
        using var document=JsonDocument.Parse(payload);return document.RootElement.TryGetProperty("Data",out var data) && data.ValueKind==JsonValueKind.Object && data.TryGetProperty(PayloadKey,out _);
    }
    public static DevelopmentInvestmentHandoff FromPayload(string payload,string instance,string field,string afe)
    {
        if(payload is null || payload.Length>MaximumBodyLength+65536)throw new InvalidOperationException("The bounded original investment handoff payload is unavailable.");
        using var document=JsonDocument.Parse(payload);
        if(document.RootElement.GetProperty("InstanceId").GetString()!=instance || !document.RootElement.GetProperty("Data").TryGetProperty(PayloadKey,out var value))
            throw new InvalidOperationException("The original investment handoff ownership is unavailable.");
        var original=value.Deserialize<DevelopmentInvestmentHandoff>()??throw new InvalidOperationException("The original investment handoff is invalid.");original.Require();
        if(original.InstanceId()!=instance || original.Scope().FieldId!=field || original.Review.Source.AfeId!=afe)throw new InvalidOperationException("The investment handoff does not match its recorded source.");return original;
    }
    public static CompanyScope OwnerCompany(string payload)
    {
        if(payload is null || payload.Length>MaximumBodyLength+65536)throw new InvalidOperationException("The bounded investment assignment ownership is unavailable.");
        using var document=JsonDocument.Parse(payload);
        var owner=document.RootElement.GetProperty("Data").GetProperty(PayloadKey).GetProperty("Review").GetProperty("Source").GetProperty("Current").GetProperty("Source").GetProperty("Review").GetProperty("Scope").GetProperty("Company").Deserialize<CompanyScope>()
            ??throw new InvalidOperationException("The investment assignment company is unavailable.");APInvoiceOwnership.RequireScope(owner);return owner;
    }
}

public sealed record DevelopmentInvestmentHandoffStatus([property:JsonRequired]DevelopmentInvestmentReview Source,[property:JsonRequired]string? HandoffId,
    [property:JsonRequired]string? WorkflowName,[property:JsonRequired]string? Sender,[property:JsonRequired]DateTimeOffset? StartedAtUtc,
    [property:JsonRequired]DevelopmentInvestmentProgress? Progress,[property:JsonRequired]string Revision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Source=Source.Current.Revision,Source.AfeId,Source.Recorded,HandoffId,WorkflowName,Sender,StartedAtUtc,Progress=Progress?.Revision}));
    public void Require(CompanyScope company,string actor,string process,string afe)
    {
        if(Source is null)throw new InvalidOperationException("The exact current investment source is required.");Source.Require();var scope=Source.Current.Source.Review.Scope;
        if(scope.Company!=company || scope.ActorUserId!=actor || Source.Current.Source.Review.InstanceId!=process || Source.AfeId!=afe || Revision!=ContentRevision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero ||
            (HandoffId is null? WorkflowName is not null || Sender is not null || StartedAtUtc is not null || Progress is not null:
                !LeaseSummaryValidator.Identifier(HandoffId) || !HandoffId.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal) || string.IsNullOrWhiteSpace(WorkflowName) || WorkflowName.Length>256 ||
                !LeaseSummaryValidator.Identifier(Sender) || StartedAtUtc is null || StartedAtUtc==default(DateTimeOffset) || StartedAtUtc.Value.Offset!=TimeSpan.Zero || Progress is null))
            throw new InvalidOperationException("The recorded handoff status or confirmed absence does not match the exact company/account/AFE source.");
        if(Progress is not null)Progress.Require(Source,HandoffId!,Sender!,StartedAtUtc!.Value);
    }
}

public sealed record DevelopmentInvestmentHandoffReceipt([property:JsonRequired]string IntentRevision,[property:JsonRequired]string InstanceId,
    [property:JsonRequired]bool Recorded,[property:JsonRequired]string? StepInstanceId,[property:JsonRequired]IReadOnlyList<string> TaskIds,
    [property:JsonRequired]string RecordedRevision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(DevelopmentInvestmentHandoff original)
    {
        original.Require();if(IntentRevision!=original.Revision() || InstanceId!=original.InstanceId() || TaskIds is null || TaskIds.Count>128 ||
            TaskIds.Any(id=>!LeaseSummaryValidator.Identifier(id)) || TaskIds.Distinct(StringComparer.Ordinal).Count()!=TaskIds.Count || !ProcessDefinitionRevision.IsValid(RecordedRevision) ||
            CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || (Recorded? !LeaseSummaryValidator.Identifier(StepInstanceId) || TaskIds.Count<1:StepInstanceId is not null || TaskIds.Count!=0))
            throw new InvalidOperationException("The investment handoff receipt does not confirm its exact retained original.");
    }
}
