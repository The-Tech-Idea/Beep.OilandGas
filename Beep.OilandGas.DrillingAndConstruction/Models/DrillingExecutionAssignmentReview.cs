using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beep.OilandGas.DrillingAndConstruction.Models;

public sealed record DrillingExecutionAssignmentReview(
    [property: JsonRequired] DrillingExecutionStatus Source,
    [property: JsonRequired] IReadOnlyList<DrillingExecutionAssignmentOption> Options,
    [property: JsonRequired] IReadOnlyList<string> SenderRoles,
    [property: JsonRequired] string? SenderPersonaCode,
    [property: JsonRequired] string? SenderAuthorityRevision,
    [property: JsonRequired] string? ExistingInstanceId,
    [property: JsonRequired] string Revision,
    [property: JsonRequired] DateTimeOffset CheckedAtUtc)
{
    [JsonIgnore] public bool CanAssign => SenderRoles.Count > 0 && ExistingInstanceId is null;
    public string ContentRevision() => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new {
        Source = Source.Revision, Options, SenderRoles, SenderPersonaCode, SenderAuthorityRevision, ExistingInstanceId }));
    public void Require(CompanyScope company, string actor, string reviewId)
    {
        Source.Require(); var scope = Source.Basis.Scope();
        if (scope.Company != company || scope.ActorUserId != actor || Source.Basis.ReviewInstanceId != reviewId || Source.Recorded is null ||
            Options is null || Options.Count > 128 || Options.Any(option => option is null) ||
            Options.Select(option => option.ProcessId).Distinct(StringComparer.Ordinal).Count() != Options.Count ||
            SenderRoles is null || SenderRoles.Count > 256 || SenderRoles.Any(role => !LeaseSummaryValidator.Identifier(role)) ||
            SenderRoles.Distinct(StringComparer.Ordinal).Count() != SenderRoles.Count ||
            (SenderRoles.Count > 0 ? !LeaseSummaryValidator.Identifier(SenderPersonaCode) || SenderPersonaCode!.Length > 64 ||
                !ProcessDefinitionRevision.IsValid(SenderAuthorityRevision) : SenderPersonaCode is not null || SenderAuthorityRevision is not null || Options.Count != 0) ||
            ExistingInstanceId is not null && (!LeaseSummaryValidator.Identifier(ExistingInstanceId) ||
                !ExistingInstanceId.StartsWith(DrillingExecutionAssignment.InstancePrefix, StringComparison.Ordinal) || Options.Count != 0) ||
            Revision != ContentRevision() || CheckedAtUtc == default || CheckedAtUtc.Offset != TimeSpan.Zero ||
            JsonSerializer.SerializeToUtf8Bytes(this).Length > DrillingExecutionAssignment.MaximumBodyLength)
            throw new InvalidOperationException("Execution assignment review must match the current original company, account, recorded execution and complete configured routing.");
        foreach (var option in Options) option.Require();
    }
}

public sealed record DrillingExecutionAssignedWork(
    [property: JsonRequired] DrillingReportingGeneration Responsibility,
    [property: JsonRequired] DrillingExecutionStatus Source,
    [property: JsonRequired] string TaskId,
    [property: JsonRequired] string StepInstanceId,
    [property: JsonRequired] string AssignedRole,
    [property: JsonRequired] string PersonaCode,
    [property: JsonRequired] string ProcessRevision,
    [property: JsonRequired] string TaskRevision,
    [property: JsonRequired] string NativeRevision,
    [property: JsonRequired] string Revision,
    [property: JsonRequired] DateTimeOffset CheckedAtUtc)
{
    [JsonIgnore] public string InstanceId => Responsibility.Root.InstanceId();
    public string ContentRevision() => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new {
        Responsibility = Responsibility.ContentRevision(), Source = Source.Revision, TaskId, StepInstanceId, AssignedRole, PersonaCode, ProcessRevision, TaskRevision, NativeRevision }));
    public void Require(CompanyScope company, string actor, string instance)
    {
        Responsibility.Require();Responsibility.Root.RequireSource(Source, false); var scope = Source.Basis.Scope(); var step = Responsibility.Workflow.Step;
        if (scope.Company != company || scope.ActorUserId != actor || InstanceId != instance ||
            !Responsibility.Workflow.Authority.Recipients.Any(row => row.UserId == actor && row.PersonaCode == PersonaCode) ||
            AssignedRole != step.AssignedRole || !step.PersonaCodes.Contains(PersonaCode, StringComparer.Ordinal) ||
            StepInstanceId != Responsibility.StepInstanceId ||
            TaskId != "task-" + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Process = instance, Step = StepInstanceId, Persona = PersonaCode })) ||
            !ProcessDefinitionRevision.IsValid(ProcessRevision) || !ProcessDefinitionRevision.IsValid(TaskRevision) ||
            !ProcessDefinitionRevision.IsValid(NativeRevision) || Revision != ContentRevision() || CheckedAtUtc == default || CheckedAtUtc.Offset != TimeSpan.Zero)
            throw new InvalidOperationException("The original execution responsibility, individual recipient and current source are unconfirmed.");
    }
}
