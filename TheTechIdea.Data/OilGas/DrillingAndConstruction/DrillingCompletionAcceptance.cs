using System.Text.Json;
using System.Text.Json.Serialization;
namespace TheTechIdea.Data.OilGas;

/// <summary>An independent human signoff against an exact checked native test and referenced criteria, not a computed physical pass.</summary>
public sealed record DrillingCompletionAcceptedTest(
    [property:JsonRequired]int ReportedTestIndex,[property:JsonRequired]DrillingNativeWellTest Native,
    [property:JsonRequired]string InspectionRevision,[property:JsonRequired]DateTimeOffset InspectedAtUtc,
    [property:JsonRequired]string CriteriaReference,[property:JsonRequired]string UtcCertificateReference,
    [property:JsonRequired]string DateDiscrepancyResolution,[property:JsonRequired]string Reason,
    [property:JsonRequired]bool ConfirmedPassingCriteria,[property:JsonRequired]bool ConfirmedReportedUtc)
{
    public DrillingCompletionTestSelection Selection(DrillingCompletionAssignedReview review)=>new(review,ReportedTestIndex,Native.Source,Native.TestType,Native.RunNumber,Native.TestNumber);
    public DrillingCompletionTestInspection Inspection(DrillingCompletionAssignedReview review)=>new(Selection(review),Native,review.Source.History.Execution.Basis.Source.Basis.Well,InspectionRevision,InspectedAtUtc);
    public void Require(DrillingCompletionAssignedReview review)
    {
        if(Native is null)throw new InvalidOperationException("The exact native test evidence is required for acceptance.");
        var checkedTest=Inspection(review);checkedTest.Require(checkedTest.Selection);var actor=review.Source.History.Execution.Basis.Scope().ActorUserId;
        if(InspectedAtUtc> DateTimeOffset.UtcNow || !checkedTest.ReferenceMatches || checkedTest.Reported.ReportedResult!="REPORTED_PASS" ||
            Native.CreatedBy==actor || Native.ChangedBy==actor || !DrillingDailyReportInput.Text(CriteriaReference,true) ||
            !DrillingDailyReportInput.Text(UtcCertificateReference,true) || !DrillingDailyReportInput.Text(Reason,true) ||
            !DrillingDailyReportInput.Text(DateDiscrepancyResolution,!checkedTest.CalendarDateMatches) || !ConfirmedPassingCriteria || !ConfirmedReportedUtc)
            throw new InvalidOperationException("Independently verify each exact native certificate/reference, reported passing result, acceptance criteria and reported UTC. Explain any native calendar-date discrepancy; do not accept your own native test edits.");
    }
}

public sealed record DrillingCompletionAcceptance(
    [property:JsonRequired]string ReviewRevision,[property:JsonRequired]DateTimeOffset AcceptedAtUtc,
    [property:JsonRequired]IReadOnlyList<DrillingCompletionAcceptedTest> Tests,[property:JsonRequired]bool ConfirmedCompletion)
{
    public void Require(DrillingCompletionAssignedReview review)
    {
        var report=review.Original.Source.ReportedCompletion!;
        if(ReviewRevision!=review.Revision || !review.IsCurrentClaim || review.StageNumber!=review.Original.Workflow.Stages.Count ||
            !ConfirmedCompletion || AcceptedAtUtc==default || AcceptedAtUtc.Offset!=TimeSpan.Zero || AcceptedAtUtc>DateTimeOffset.UtcNow ||
            AcceptedAtUtc<review.CheckedAtUtc || AcceptedAtUtc<report.RecordedAtUtc || !report.Report.OutstandingItemsReviewed || report.Report.OutstandingItems.Count!=0 ||
            Tests is null || Tests.Count!=report.Report.Tests.Count || Tests.Any(test=>test is null) ||
            !Tests.Select(test=>test.ReportedTestIndex).Order().SequenceEqual(Enumerable.Range(0,report.Report.Tests.Count)))
            throw new InvalidOperationException("Physical completion acceptance requires the exact current final review stage, every reported test, no unresolved work and explicit actual acceptance UTC/confirmation.");
        foreach(var test in Tests){test.Require(review);if(AcceptedAtUtc<test.InspectedAtUtc)throw new InvalidOperationException("Acceptance UTC must follow every inspected test.");}
        if(Tests.Select(test=>JsonSerializer.Serialize(new{test.Native.Uwi,test.Native.Source,test.Native.TestType,test.Native.RunNumber,test.Native.TestNumber})).Distinct(StringComparer.Ordinal).Count()!=Tests.Count)
            throw new InvalidOperationException("One native physical test cannot establish acceptance of multiple distinct reported tests.");
    }
}
