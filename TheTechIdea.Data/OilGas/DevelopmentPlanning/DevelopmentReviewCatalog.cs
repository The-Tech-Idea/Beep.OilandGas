using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

public sealed record DevelopmentReviewRoleStep(
    [property: JsonRequired] string StepId,
    [property: JsonRequired] string StepName,
    [property: JsonRequired] int Sequence,
    [property: JsonRequired] string AssignedRole,
    [property: JsonRequired] IReadOnlyList<string> PersonaCodes,
    [property: JsonRequired] int? SlaHours);

/// <summary>A projection of an existing configured development review definition; not a replacement process model.</summary>
public sealed record DevelopmentReviewWorkflowChoice(
    [property: JsonRequired] string ProcessId,
    [property: JsonRequired] string ProcessName,
    [property: JsonRequired] string Version,
    [property: JsonRequired] string DefinitionRevision,
    [property: JsonRequired] IReadOnlyList<DevelopmentReviewRoleStep> Steps)
{
    public void Require()
    {
        if(!LeaseSummaryValidator.Identifier(ProcessId) || string.IsNullOrWhiteSpace(ProcessName) || ProcessName.Length>256 || ProcessName.Any(char.IsControl) ||
            !LeaseSummaryValidator.Identifier(Version) || !ProcessDefinitionRevision.IsValid(DefinitionRevision) || Steps is null || Steps.Count is <1 or >64 || Steps.Any(step=>step is null) ||
            Steps.Select(step=>step.StepId).Distinct(StringComparer.Ordinal).Count()!=Steps.Count || Steps.Select(step=>step.Sequence).Distinct().Count()!=Steps.Count ||
            !Steps.Select(step=>step.Sequence).SequenceEqual(Steps.Select(step=>step.Sequence).Order()))
            throw new InvalidOperationException("The recorded development review workflow is incomplete or ambiguous.");
        foreach(var step in Steps)
            if(!LeaseSummaryValidator.Identifier(step.StepId) || string.IsNullOrWhiteSpace(step.StepName) || step.StepName.Length>256 || step.StepName.Any(char.IsControl) ||
                step.Sequence<0 || !LeaseSummaryValidator.Identifier(step.AssignedRole) || step.SlaHours<0 || step.PersonaCodes is null || step.PersonaCodes.Count is <1 or >128 ||
                step.PersonaCodes.Any(code=>!LeaseSummaryValidator.Identifier(code) || code.Length>64) || step.PersonaCodes.Distinct(StringComparer.Ordinal).Count()!=step.PersonaCodes.Count)
                throw new InvalidOperationException("Each development review step needs its recorded role and active persona destinations.");
    }
}

public sealed record DevelopmentReviewCatalog(
    [property: JsonRequired] DevelopmentPlanSourceReview Source,
    [property: JsonRequired] AssetDatabaseScope LifecycleDatabase,
    [property: JsonRequired] IReadOnlyList<DevelopmentReviewWorkflowChoice> Choices,
    [property: JsonRequired] string Revision,
    [property: JsonRequired] DateTimeOffset CheckedAtUtc)
{
    public static string RevisionFor(DevelopmentPlanSourceReview source,AssetDatabaseScope lifecycle,IReadOnlyList<DevelopmentReviewWorkflowChoice> choices)=>
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{SourceRevision=source.Revision,LifecycleDatabase=lifecycle,Choices=choices}));
    public void Require(FieldSelectionReview context,string id,string sourceRevision)
    {
        Source.Require(context,id);
        if(Source.Plan is null || Source.Revision!=sourceRevision || LifecycleDatabase is null || !LeaseSummaryValidator.Identifier(LifecycleDatabase.ConnectionName) ||
            !LeaseSummaryValidator.Identifier(LifecycleDatabase.Fingerprint) || Choices is null || Choices.Count>128 || Choices.Any(choice=>choice is null) ||
            Choices.Select(choice=>choice.ProcessId).Distinct(StringComparer.Ordinal).Count()!=Choices.Count || Revision!=RevisionFor(Source,LifecycleDatabase,Choices) ||
            CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("Review workflow choices must match the exact recorded company FIELD draft.");
        foreach(var choice in Choices)choice.Require();
    }
}
