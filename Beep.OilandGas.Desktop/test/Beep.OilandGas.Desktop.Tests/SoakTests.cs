using System.Text.Json;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Headless;
using TheTechIdea.Beep.OilandGas.Desktop.Performance;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// REL-04 AC-5: the soak report's arithmetic and the <c>--soak</c> verb. The 30-minute run itself is recorded
/// in the evidence file; these tests pin the maths and the command so the long run means what it says.
/// </summary>
public sealed class SoakTests
{
    [Fact]
    public void The_report_computes_start_end_peak_growth_and_slope()
    {
        var samples = new List<OgSoakSample>
        {
            new(TimeSpan.Zero, 100, 60),
            new(TimeSpan.FromMinutes(1), 120, 65),
            new(TimeSpan.FromMinutes(2), 150, 70),
            new(TimeSpan.FromMinutes(3), 140, 66),
        };

        var report = OgSoakMath.Summarise(TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(1), 120, samples);

        Assert.Equal(100, report.StartMegabytes);
        Assert.Equal(140, report.EndMegabytes);
        Assert.Equal(150, report.PeakMegabytes);
        Assert.Equal(40, report.GrowthMegabytes);
        Assert.Equal(40d / 3d, report.GrowthPerMinute, 6);
        Assert.True(report.WithinBudget);
        Assert.Contains("within budget", report.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_peak_above_the_budget_fails_the_soak()
    {
        var samples = new List<OgSoakSample>
        {
            new(TimeSpan.Zero, 200, 100),
            new(TimeSpan.FromMinutes(30), 900, 400),
        };

        var report = OgSoakMath.Summarise(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(1), 500, samples);

        // AC-5 is about the budget, not about growth being flat.
        Assert.False(report.WithinBudget);
        Assert.Contains("OVER BUDGET", report.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_report_without_samples_proves_nothing_and_says_so()
    {
        var report = OgSoakMath.Summarise(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(1), 0, []);

        Assert.False(report.WithinBudget);
        Assert.Contains(report.Notes, note => note.Contains("proves nothing", StringComparison.Ordinal));
    }

    [Fact]
    public void The_report_serialises_with_its_samples_and_verdict()
    {
        var report = OgSoakMath.Summarise(
            TimeSpan.FromMinutes(30),
            TimeSpan.FromSeconds(60),
            900,
            [new(TimeSpan.Zero, 150, 90), new(TimeSpan.FromMinutes(30), 180, 95)]);

        using var document = JsonDocument.Parse(OgSoakMath.ToJson(report));
        var root = document.RootElement;

        Assert.Equal(30, root.GetProperty("durationMinutes").GetDouble(), 3);
        Assert.Equal(900, root.GetProperty("cycles").GetInt32());
        Assert.Equal(180, root.GetProperty("endMb").GetDouble(), 1);
        Assert.Equal(800, root.GetProperty("budgetMb").GetDouble(), 1);
        Assert.True(root.GetProperty("withinBudget").GetBoolean());
        Assert.Equal(2, root.GetProperty("samples").GetArrayLength());
    }

    [Fact]
    public void The_soak_verb_runs_a_short_soak_and_writes_a_parseable_report()
    {
        using var application = new OgBootstrapper().Create();
        var output = new StringWriter();

        var exit = OgHeadlessRunner.Run(
            application,
            new OgCommandLine(OgHeadlessVerb.Soak, "0.02"),
            output);

        var text = output.ToString();

        Assert.Equal(OgExitCode.Ok, exit);
        Assert.Contains("soak", text, StringComparison.Ordinal);
        Assert.Contains("within-budget:", text, StringComparison.Ordinal);
        Assert.Contains("report:", text, StringComparison.Ordinal);
        Assert.True(File.Exists(OgPaths.SoakReportPath));

        using var report = JsonDocument.Parse(File.ReadAllText(OgPaths.SoakReportPath));
        Assert.True(report.RootElement.GetProperty("cycles").GetInt32() > 0);
        Assert.Equal(800, report.RootElement.GetProperty("budgetMb").GetDouble(), 1);
    }

    [Fact]
    public void A_soak_duration_that_is_not_a_number_is_refused()
    {
        using var application = new OgBootstrapper().Create();
        var output = new StringWriter();

        var exit = OgHeadlessRunner.Run(
            application,
            new OgCommandLine(OgHeadlessVerb.Soak, "half an hour"),
            output);

        Assert.Equal(OgExitCode.Refused, exit);
        Assert.Contains("result: refused", output.ToString(), StringComparison.Ordinal);
    }
}
