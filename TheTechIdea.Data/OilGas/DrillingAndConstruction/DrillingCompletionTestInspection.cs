using System.Text.Json;
using System.Text.Json.Serialization;
namespace TheTechIdea.Data.OilGas;

/// <summary>An explicit existing PPDM test selection, never a guessed mapping from a reported reference.</summary>
public sealed record DrillingCompletionTestSelection(
    [property:JsonRequired] DrillingCompletionAssignedReview Review,
    [property:JsonRequired] int ReportedTestIndex,
    [property:JsonRequired] string Source,
    [property:JsonRequired] string TestType,
    [property:JsonRequired] string RunNumber,
    [property:JsonRequired] string TestNumber)
{
    public void Require()
    {
        if(Review is null)throw new InvalidOperationException("The exact assigned completion review is required.");
        var scope=Review.Source.History.Execution.Basis.Scope();Review.Require(scope.Company,scope.ActorUserId,Review.InstanceId);
        if(ReportedTestIndex<0 || ReportedTestIndex>=Review.Original.Source.ReportedCompletion!.Report.Tests.Count ||
            !new[]{Source,TestType,RunNumber,TestNumber}.All(value=>!string.IsNullOrWhiteSpace(value) && value==value.Trim() && value.Length<=128 && !value.Any(char.IsControl)))
            throw new InvalidOperationException("Choose one reported test and explicitly enter its native source, test type, run and test number.");
    }
    public string Revision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Review=Review.Revision,ReportedTestIndex,Source,TestType,RunNumber,TestNumber}));
}

/// <summary>Read-only native evidence. A PPDM TEST_DATE without a timezone remains a recorded date, not an inferred UTC instant.</summary>
public sealed record DrillingNativeWellTest(
    [property:JsonRequired] string Uwi,[property:JsonRequired] string Source,[property:JsonRequired] string TestType,
    [property:JsonRequired] string RunNumber,[property:JsonRequired] string TestNumber,
    [property:JsonRequired] string ReportReference,[property:JsonRequired] DateTime TestDate,
    [property:JsonRequired] string ResultCode,[property:JsonRequired] string? Remark,
    [property:JsonRequired] string CreatedBy,[property:JsonRequired] DateTime CreatedAt,
    [property:JsonRequired] string ChangedBy,[property:JsonRequired] DateTime ChangedAt,
    [property:JsonRequired] string NativeRevision)
{
    public void Require(DrillingCompletionTestSelection selection,DateTimeOffset checkedAt)
    {
        selection.Require();var well=selection.Review.Source.History.Execution.Basis.Source.Basis.Well;
        Require(well,selection.Source,selection.TestType,selection.RunNumber,selection.TestNumber,checkedAt);
    }
    public void Require(DrillingProgramWellFacts well,string source,string type,string run,string number,DateTimeOffset checkedAt)
    {
        if(well is null || Uwi!=well.Uwi || Source!=source || TestType!=type || RunNumber!=run || TestNumber!=number ||
            !DrillingDailyReportInput.Text(ReportReference,true) || ReportReference.Length>256 || !DrillingDailyReportInput.Text(ResultCode,true) || ResultCode.Length>128 ||
            Remark is not null && (!DrillingDailyReportInput.Text(Remark,false) || Remark.Length>16000) || TestDate==default || TestDate.Kind==DateTimeKind.Local ||
            !LeaseSummaryValidator.Identifier(CreatedBy) || !LeaseSummaryValidator.Identifier(ChangedBy) || CreatedAt==default || CreatedAt.Kind==DateTimeKind.Local ||
            ChangedAt.Kind==DateTimeKind.Local || ChangedAt<CreatedAt || ChangedAt>checkedAt.UtcDateTime ||
            !ProcessDefinitionRevision.IsValid(NativeRevision))throw new InvalidOperationException("The exact native test, result, reference and accountable audit are unconfirmed.");
    }
}

/// <summary>Current native evidence for a source-owner-admitted accepted claim. Reading history alone does not provide this readiness.</summary>
public sealed record DrillingAcceptedCompletionTests(
    [property:JsonRequired]DrillingCompletionReviewProgress Acceptance,[property:JsonRequired]IReadOnlyList<DrillingNativeWellTest> Tests,
    [property:JsonRequired]DrillingProgramWellFacts Well,[property:JsonRequired]string Revision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Acceptance=Acceptance.Revision,Tests,Well}));
    public void Require(CompanyScope company,string actor,string instance)
    {
        Acceptance.RequireFacts(company,actor,instance);var source=Acceptance.Source.History.Execution.Basis.Source.Basis.Well;
        if(!Acceptance.OriginalCompletionAccepted || !Acceptance.AcceptedClaimIsCurrent || Well!=source || Tests is null || Tests.Any(test=>test is null) ||
            CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || Revision!=ContentRevision())throw new InvalidOperationException("Current accepted claim, well and complete native test readiness must be confirmed through their source owners.");
        var expected=Acceptance.Stages.Last().AcceptedDecision!.Acceptance!.Tests.OrderBy(test=>test.ReportedTestIndex).ToArray();
        if(Tests.Count!=expected.Length)throw new InvalidOperationException("Every accepted physical test requires current native evidence.");
        for(int index=0;index<Tests.Count;index++)
        {
            var original=expected[index].Native;var current=Tests[index];current.Require(Well,original.Source,original.TestType,original.RunNumber,original.TestNumber,CheckedAtUtc);
            if(current!=original)throw new InvalidOperationException("An accepted native test no longer matches its original checked evidence.");
        }
    }
}

public sealed record DrillingCompletionTestInspection(
    [property:JsonRequired] DrillingCompletionTestSelection Selection,[property:JsonRequired] DrillingNativeWellTest Native,
    [property:JsonRequired] DrillingProgramWellFacts Well,[property:JsonRequired] string Revision,[property:JsonRequired] DateTimeOffset CheckedAtUtc)
{
    [JsonIgnore] public DrillingCompletionTestReference Reported=>Selection.Review.Original.Source.ReportedCompletion!.Report.Tests[Selection.ReportedTestIndex];
    [JsonIgnore] public bool ReferenceMatches=>Native.ReportReference==Reported.Reference;
    [JsonIgnore] public bool CalendarDateMatches=>Native.TestDate.Date==Reported.TestedAtUtc.UtcDateTime.Date;
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Selection=Selection.Revision(),Native,Well}));
    public void Require(DrillingCompletionTestSelection selection)
    {
        if(Selection is null || Native is null || Well is null || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || Selection.Revision()!=selection.Revision() ||
            Well!=Selection.Review.Source.History.Execution.Basis.Source.Basis.Well || Revision!=ContentRevision())throw new InvalidOperationException("The native test inspection differs from its exact original review, reported test, well or source.");
        Native.Require(Selection,CheckedAtUtc);
    }
}
