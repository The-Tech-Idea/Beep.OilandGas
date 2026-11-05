using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

/// <summary>Current responsibility and an explicitly reviewed replacement; this records no transfer.</summary>
public sealed record DrillingReportingReassignmentReview(
    [property: JsonRequired] DrillingExecutionAssignment Assignment,
    [property: JsonRequired] DrillingExecutionStatus Execution,
    [property: JsonRequired] int Generation,
    [property: JsonRequired] DrillingExecutionAssignmentOption FromWorkflow,
    [property: JsonRequired] string FromStepInstanceId,
    [property: JsonRequired] string ResponsibilityRevision,
    [property: JsonRequired] IReadOnlyList<string> TaskIds,
    [property: JsonRequired] string ProcessRevision,
    [property: JsonRequired] string NativeRevision,
    [property: JsonRequired] DrillingExecutionAssignmentOption Destination,
    [property: JsonRequired] string ManagerRole,
    [property: JsonRequired] string ManagerPersona,
    [property: JsonRequired] string AuthorityRevision,
    [property: JsonRequired] string Revision,
    [property: JsonRequired] DateTimeOffset CheckedAtUtc)
{
    public DevelopmentAssignedReviewScope Scope() => Execution.Basis.Scope();

    public string ContentRevision() => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new
    {
        Assignment = Assignment.Revision(), Execution = Execution.Revision, Generation, FromWorkflow,
        FromStepInstanceId, ResponsibilityRevision, TaskIds, ProcessRevision, NativeRevision, Destination,
        ManagerRole, ManagerPersona, AuthorityRevision
    }));

    public static bool SameDestination(DrillingExecutionAssignmentOption left, DrillingExecutionAssignmentOption right) =>
        left.ProcessId == right.ProcessId && left.Version == right.Version && left.DefinitionRevision == right.DefinitionRevision &&
        left.Step.StepId == right.Step.StepId && left.Step.StepName == right.Step.StepName && left.Step.Sequence == right.Step.Sequence &&
        left.Step.AssignedRole == right.Step.AssignedRole && left.Step.SlaHours == right.Step.SlaHours &&
        left.Step.PersonaCodes.Order(StringComparer.Ordinal).SequenceEqual(right.Step.PersonaCodes.Order(StringComparer.Ordinal)) &&
        left.Authority.Recipients.OrderBy(row => row.UserId, StringComparer.Ordinal).ThenBy(row => row.PersonaCode, StringComparer.Ordinal)
            .SequenceEqual(right.Authority.Recipients.OrderBy(row => row.UserId, StringComparer.Ordinal).ThenBy(row => row.PersonaCode, StringComparer.Ordinal));

    public void Require()
    {
        if (Assignment is null || Execution is null || FromWorkflow is null || Destination is null)
            throw new InvalidOperationException("The original reporting responsibility, current execution and proposed destination are required.");
        Assignment.RequireSource(Execution); FromWorkflow.Require(); Destination.Require();
        if (Generation is < 1 or > 256 || !FromWorkflow.Ready || !Destination.Ready ||
            !LeaseSummaryValidator.Identifier(FromStepInstanceId) || !ProcessDefinitionRevision.IsValid(ResponsibilityRevision) ||
            !ProcessDefinitionRevision.IsValid(ProcessRevision) || !ProcessDefinitionRevision.IsValid(NativeRevision) ||
            TaskIds is null || !TaskIds.Order(StringComparer.Ordinal).SequenceEqual(FromWorkflow.Step.PersonaCodes.Select(persona =>
                DrillingExecutionAssignmentReceipt.TaskId(Assignment.InstanceId(), FromStepInstanceId, persona)).Order(StringComparer.Ordinal)) ||
            ManagerRole != Assignment.SenderRole || !LeaseSummaryValidator.Identifier(ManagerPersona) || ManagerPersona.Length > 64 ||
            !ProcessDefinitionRevision.IsValid(AuthorityRevision) || SameDestination(FromWorkflow, Destination) ||
            (Generation == 1 && (JsonSerializer.Serialize(FromWorkflow) != JsonSerializer.Serialize(Assignment.Workflow) ||
                ResponsibilityRevision != Assignment.Revision() || FromStepInstanceId != DrillingExecutionAssignmentReceipt.StepId(Assignment.InstanceId(), FromWorkflow.Step.StepId))) ||
            CheckedAtUtc == default || CheckedAtUtc.Offset != TimeSpan.Zero || CheckedAtUtc < Execution.CheckedAtUtc || Revision != ContentRevision())
            throw new InvalidOperationException("Reassignment requires the complete current generation, former receiving set, changed configured destination and current management authority.");
    }
}

/// <summary>A retained transfer intent; never a claim that physical drilling work is complete.</summary>
public sealed record DrillingReportingReassignment(
    [property: JsonRequired] DrillingReportingReassignmentReview Review,
    [property: JsonRequired] Guid IntentId,
    [property: JsonRequired] DateTimeOffset ReassignedAtUtc,
    [property: JsonRequired] string Reason,
    [property: JsonRequired] bool Confirmed)
{
    public const string PayloadKey = "DrillingReportingReassignments";
    public DevelopmentAssignedReviewScope Scope() => Review.Scope();
    public void Require()
    {
        if (Review is null) throw new InvalidOperationException("The exact reviewed reporting transfer is required.");
        Review.Require();
        if (IntentId == Guid.Empty || !Confirmed || ReassignedAtUtc == default || ReassignedAtUtc.Offset != TimeSpan.Zero ||
            ReassignedAtUtc < Review.CheckedAtUtc || ReassignedAtUtc > DateTimeOffset.UtcNow || !DrillingDailyReportInput.Text(Reason, true) ||
            JsonSerializer.SerializeToUtf8Bytes(this).Length > DrillingExecutionAssignment.MaximumBodyLength)
            throw new InvalidOperationException("Retain the reviewed responsibility, explicit actual transfer UTC, reason and confirmation.");
    }
    public string Revision() { Require(); return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this)); }
    public string HistoryId() => "reporting-transfer-" + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Instance = Review.Assignment.InstanceId(), IntentId }));
    public string StepInstanceId() => "reporting-transfer-step-" + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Instance = Review.Assignment.InstanceId(), IntentId, Step = Review.Destination.Step.StepId }));
    public IReadOnlyList<string> TaskIds() => Review.Destination.Step.PersonaCodes.Select(persona =>
        DrillingExecutionAssignmentReceipt.TaskId(Review.Assignment.InstanceId(), StepInstanceId(), persona)).Order(StringComparer.Ordinal).ToArray();
}
