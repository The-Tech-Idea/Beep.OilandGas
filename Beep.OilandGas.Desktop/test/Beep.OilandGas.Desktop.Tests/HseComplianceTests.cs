using Beep.OilandGas.Models.Data.Compliance;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// HSE-05: the state derivation, the explicit jurisdiction, the fulfilment gate, the calendar totals and the report-process
/// sentence - the parts that need no database. The register, its rows and the commands are proven in the gateway test.
/// </summary>
public sealed class HseComplianceTests
{
    private const string PageFileName = "CompliancePageView.cs";

    private static readonly DateTime Today = new(2026, 11, 8);

    [Fact]
    public void A_missing_due_date_is_unscheduled_and_never_overdue()
    {
        // AC-1 and AC-2.
        Assert.Equal(ComplianceRules.Unscheduled, ComplianceRules.StateOf(false, false, null, Today));

        // A recorded date that has passed is overdue; today or later is pending.
        Assert.Equal(ObligationStatus.Overdue,
            ComplianceRules.StateOf(false, false, Today.AddDays(-1), Today));
        Assert.Equal(ObligationStatus.Pending, ComplianceRules.StateOf(false, false, Today, Today));
        Assert.Equal(ObligationStatus.Pending,
            ComplianceRules.StateOf(false, false, Today.AddDays(10), Today));

        // Recorded facts win over the date: a fulfilled or waived obligation is not overdue.
        Assert.Equal(ObligationStatus.Submitted,
            ComplianceRules.StateOf(true, false, Today.AddDays(-1), Today));
        Assert.Equal(ObligationStatus.Waived,
            ComplianceRules.StateOf(false, true, Today.AddDays(-1), Today));
    }

    [Fact]
    public void A_new_obligation_needs_an_explicit_jurisdiction()
    {
        // AC-3.
        Assert.Null(ComplianceRules.Validate(Inputs()));

        var noJurisdiction = ComplianceRules.Validate(Inputs() with { Jurisdiction = "  " });

        Assert.Equal(ComplianceRules.CodeJurisdictionRequired, noJurisdiction!.Value.Code);
        Assert.Contains("None is assumed", noJurisdiction.Value.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("USA", noJurisdiction.Value.Message, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(ComplianceRules.CodeInputRequired,
            ComplianceRules.Validate(Inputs() with { Kind = null })!.Value.Code);
        Assert.Equal(ComplianceRules.CodeInputRequired,
            ComplianceRules.Validate(Inputs() with { Description = " " })!.Value.Code);
        Assert.Equal(ComplianceRules.CodeNoComponents,
            ComplianceRules.Validate(Inputs() with { Components = [] })!.Value.Code);
    }

    [Fact]
    public void An_obligation_without_components_cannot_be_fulfilled()
    {
        // AC-4.
        var empty = Obligation(components: []);
        var noComponents = ComplianceRules.CanFulfil(empty);

        Assert.Equal(ComplianceRules.CodeNoComponents, noComponents!.Value.Code);
        Assert.Contains("cannot be marked fulfilled", noComponents.Value.Message, StringComparison.Ordinal);

        // A required component outstanding is named.
        var partial = Obligation(components:
        [
            new OgObligationComponent(1m, 1m, "Signed form", true, Today.AddDays(-2), "filed"),
            new OgObligationComponent(1m, 2m, "Fee paid", true, null, null),
        ]);

        var outstanding = ComplianceRules.CanFulfil(partial);

        Assert.Equal(ComplianceRules.CodeComponentsOutstanding, outstanding!.Value.Code);
        Assert.Contains("Fee paid", outstanding.Value.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("Signed form", outstanding.Value.Message, StringComparison.Ordinal);

        // An optional component may stay unfulfilled.
        var optionalOutstanding = Obligation(components:
        [
            new OgObligationComponent(1m, 1m, "Signed form", true, Today, "filed"),
            new OgObligationComponent(1m, 2m, "Extra copy", false, null, null),
        ]);

        Assert.Null(ComplianceRules.CanFulfil(optionalOutstanding));
    }

    [Fact]
    public void The_calendar_and_the_list_count_the_same_rows()
    {
        // AC-6, over a month with a past and a future date.
        var month = new DateTime(2026, 11, 1);
        var obligations = new List<OgObligation>
        {
            Obligation("OBL-1", ComplianceRules.StateOf(false, false, new DateTime(2026, 11, 3), Today),
                new DateTime(2026, 11, 3)),
            Obligation("OBL-2", ComplianceRules.StateOf(false, false, new DateTime(2026, 11, 20), Today),
                new DateTime(2026, 11, 20)),
            Obligation("OBL-3", ComplianceRules.StateOf(true, false, new DateTime(2026, 11, 25), Today),
                new DateTime(2026, 11, 25)),
            Obligation("OBL-4", ComplianceRules.StateOf(false, false, null, Today), null),
        };

        var calendar = ComplianceRules.CalendarOf(obligations, month, Today);

        Assert.Equal(30, calendar.Count);
        Assert.Equal(2, calendar.Sum(day => day.Due));
        Assert.Equal(1, calendar.Sum(day => day.Overdue));
        Assert.Equal(1, calendar.Single(day => day.Date.Day == 3).Overdue);
        Assert.Equal(1, calendar.Single(day => day.Date.Day == 20).Due);
        Assert.Equal(0, calendar.Single(day => day.Date.Day == 20).Overdue);

        // The fulfilled one is not on the calendar; the unscheduled one is on no day at all.
        Assert.Equal(0, calendar.Single(day => day.Date.Day == 25).Due);
    }

    [Fact]
    public async Task The_report_process_says_no_regulator_delivery_is_established()
    {
        // AC-5: the sentence is a constant and the command states it rather than pretending to file.
        Assert.Contains("No regulator delivery is established",
            OgComplianceWorkspace.ReportProcessText, StringComparison.Ordinal);
        Assert.Contains("nothing is filed", OgComplianceWorkspace.ReportProcessText, StringComparison.Ordinal);

        var compliance = new OgCompliance(() => null, new Quiet());
        var outcome = await compliance.RequestReportAsync("OBL-1", 1m, "sara");

        Assert.False(outcome.Success);
        Assert.Null(outcome.Value);
        Assert.Equal(ComplianceRules.CodeReportNotWired, outcome.Reference);
        Assert.Contains("No regulator delivery is established", outcome.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public void The_obligation_says_its_recorded_due_date_or_unscheduled()
    {
        var due = new OgObligation("OBL-1", 1m, "ONRR_PRODUCTION_REPORT", "Monthly report", "TX",
            new DateTime(2026, 11, 3), ObligationStatus.Pending, []);
        var undated = due with { DueDate = null, State = ComplianceRules.Unscheduled };
        var noJurisdiction = due with { Jurisdiction = string.Empty };

        Assert.Equal("2026-11-03", due.DueText);
        Assert.Equal("unscheduled", undated.DueText);
        Assert.True(undated.IsUnscheduled);
        Assert.False(undated.IsOverdue);
        Assert.Equal("not specified", noJurisdiction.JurisdictionText);
        Assert.Equal("TX", due.JurisdictionText);
    }

    [Fact]
    public void The_page_shows_the_report_sentence_and_never_defaults_a_country()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-5: the constant is printed.
        Assert.Contains("ReportProcessText", page, StringComparison.Ordinal);

        // AC-6: the calendar and the list are both bound from the workspace.
        Assert.Contains("workspace.Calendar", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Obligations", page, StringComparison.Ordinal);

        // AC-1 and AC-3: the due text and the jurisdiction text come from the record.
        Assert.Contains("DueText", page, StringComparison.Ordinal);
        Assert.Contains("JurisdictionText", page, StringComparison.Ordinal);

        Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex("\\b(USA|United States|Canada|UK)\\b"), page);
    }

    private static OgObligationInputs Inputs() =>
        new(
            ObligationType.OnrrProductionReport,
            "TX",
            "F-1",
            new DateTime(2026, 12, 1),
            "Monthly production report",
            ["Signed form", "Fee paid"]);

    private static OgObligation Obligation(
        string id = "OBL-1",
        string state = ObligationStatus.Pending,
        DateTime? dueDate = null,
        IReadOnlyList<OgObligationComponent>? components = null) =>
        new(id, 1m, ObligationType.OnrrProductionReport, "Monthly report", "TX",
            dueDate ?? new DateTime(2026, 12, 1), state, components ?? []);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
