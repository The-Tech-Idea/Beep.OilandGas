using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

public sealed record DrillingCompletionReviewStage(
    [property: JsonRequired] DevelopmentReviewRoleStep Definition,
    [property: JsonRequired] DevelopmentReviewRecipientStep Authority);

public sealed record DrillingCompletionReviewOption(
    [property: JsonRequired] string ProcessId,
    [property: JsonRequired] string ProcessName,
    [property: JsonRequired] string Version,
    [property: JsonRequired] string DefinitionRevision,
    [property: JsonRequired] IReadOnlyList<DrillingCompletionReviewStage> Stages)
{
    [JsonIgnore] public bool Ready => Stages.Count > 0 && Stages.All(stage => stage.Authority.Recipients.Count > 0);
    public void Require(DrillingDailyReportHistory source)
    {
        var claimant = source.ReportedCompletion?.RecorderUserId;
        var actor = source.Execution.Basis.Scope().ActorUserId;
        if (claimant is null || !LeaseSummaryValidator.Identifier(ProcessId) ||
            string.IsNullOrWhiteSpace(ProcessName) || ProcessName.Length > 256 || ProcessName.Any(char.IsControl) ||
            !LeaseSummaryValidator.Identifier(Version) || !ProcessDefinitionRevision.IsValid(DefinitionRevision) ||
            Stages is null || Stages.Count is < 1 or > 64 || Stages.Any(stage => stage?.Definition is null || stage.Authority is null) ||
            Stages.Select(stage => stage.Definition.StepId).Distinct(StringComparer.Ordinal).Count() != Stages.Count ||
            Stages.Select(stage => stage.Definition.Sequence).Distinct().Count() != Stages.Count ||
            !Stages.Select(stage => stage.Definition.Sequence).SequenceEqual(Stages.Select(stage => stage.Definition.Sequence).Order()))
            throw new InvalidOperationException("Completion review requires complete configured ordered approval stages against the current reported claim.");
        foreach (var stage in Stages)
        {
            var step = stage.Definition; var authority = stage.Authority; var recipients = authority.Recipients;
            if (!LeaseSummaryValidator.Identifier(step.StepId) || string.IsNullOrWhiteSpace(step.StepName) || step.StepName.Length > 256 || step.StepName.Any(char.IsControl) ||
                step.Sequence < 0 || step.SlaHours < 0 || !LeaseSummaryValidator.Identifier(step.AssignedRole) ||
                step.PersonaCodes is null || step.PersonaCodes.Count > 128 || step.PersonaCodes.Any(code => !LeaseSummaryValidator.Identifier(code) || code.Length > 64) ||
                step.PersonaCodes.Distinct(StringComparer.Ordinal).Count() != step.PersonaCodes.Count ||
                authority.StepId != step.StepId || authority.AssignedRole != step.AssignedRole || !ProcessDefinitionRevision.IsValid(authority.AuthorityRevision) ||
                recipients is null || recipients.Count > 256 || recipients.Any(row => row is null || !LeaseSummaryValidator.Identifier(row.UserId) ||
                    !LeaseSummaryValidator.Identifier(row.PersonaCode) || row.PersonaCode.Length > 64 || row.UserId == actor || row.UserId == claimant) ||
                recipients.Select(row => row.UserId).Distinct(StringComparer.Ordinal).Count() != recipients.Count ||
                !step.PersonaCodes.Order(StringComparer.Ordinal).SequenceEqual(recipients.Select(row => row.PersonaCode).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)))
                throw new InvalidOperationException("Completion review stages must retain independent current company role/workspace/FIELD recipient authority.");
        }
        foreach (var role in Stages.GroupBy(stage => stage.Definition.AssignedRole, StringComparer.Ordinal))
            if (role.Select(stage => JsonSerializer.Serialize(stage.Authority with { StepId = "" })).Distinct(StringComparer.Ordinal).Count() != 1)
                throw new InvalidOperationException("Repeated completion-review roles must retain the same current source-qualified authority.");
    }
}

/// <summary>Routing inspection only; no submitted review, accepted test or responsibility closure.</summary>
public sealed record DrillingCompletionReviewReadiness(
    [property: JsonRequired] DrillingDailyReportHistory Source,
    [property: JsonRequired] IReadOnlyList<DrillingCompletionReviewOption> Options,
    [property: JsonRequired] IReadOnlyList<string> SenderRoles,
    [property: JsonRequired] string? SenderPersonaCode,
    [property: JsonRequired] string? SenderAuthorityRevision,
    [property: JsonRequired] string Revision,
    [property: JsonRequired] DateTimeOffset CheckedAtUtc)
{
    public const int MaximumBodyLength = 8 * 1024 * 1024;
    [JsonRequired] public string? ExistingReviewInstanceId { get; init; }
    public string ContentRevision() => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new {
        Source = Source.Revision, Options, SenderRoles, SenderPersonaCode, SenderAuthorityRevision, ExistingReviewInstanceId }));
    public void Require(CompanyScope company, string actor, string reviewId)
    {
        Source.Require(); var scope = Source.Execution.Basis.Scope();
        if (scope.Company != company || scope.ActorUserId != actor || Source.Execution.Basis.ReviewInstanceId != reviewId || Source.ReportedCompletion is null ||
            Options is null || Options.Count > 128 || Options.Any(option => option is null) ||
            Options.Select(option => option.ProcessId).Distinct(StringComparer.Ordinal).Count() != Options.Count ||
            Options.Sum(option => option.Stages?.Count ?? 0) > 256 ||
            SenderRoles is null || SenderRoles.Count > 256 || SenderRoles.Any(role => !LeaseSummaryValidator.Identifier(role)) ||
            SenderRoles.Distinct(StringComparer.Ordinal).Count() != SenderRoles.Count ||
            (SenderRoles.Count > 0 ? !LeaseSummaryValidator.Identifier(SenderPersonaCode) || SenderPersonaCode!.Length > 64 ||
                !ProcessDefinitionRevision.IsValid(SenderAuthorityRevision) : SenderPersonaCode is not null || SenderAuthorityRevision is not null || Options.Count != 0) ||
            ExistingReviewInstanceId is not null && (!LeaseSummaryValidator.Identifier(ExistingReviewInstanceId) || !ExistingReviewInstanceId.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal) || Options.Count!=0) ||
            CheckedAtUtc == default || CheckedAtUtc.Offset != TimeSpan.Zero || CheckedAtUtc < Source.CheckedAtUtc || Revision != ContentRevision())
            throw new InvalidOperationException("Completion routing must match the original current company, actor, report history and configured sender authority.");
        foreach (var option in Options) option.Require(Source);
        if (JsonSerializer.SerializeToUtf8Bytes(this).Length > MaximumBodyLength)
            throw new InvalidOperationException("The complete completion-review routing exceeds its bounded payload.");
    }
}
