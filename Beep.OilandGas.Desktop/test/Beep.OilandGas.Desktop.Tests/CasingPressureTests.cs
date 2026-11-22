using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.Beep.OilandGas.Desktop.Views;
using TheTechIdea.OilGas.Integrity;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ENG-04: the class and severity rules (AC-1, AC-3), the action that follows them, the configured-threshold guard (AC-6) and the
/// reading parser - the parts that need no database. The MAWOP/MAASP screening and the recorded tests are proven in the gateway
/// test.
/// </summary>
public sealed class CasingPressureTests
{
    private const string PageFileName = "CasingPressurePageView.cs";

    [Fact]
    public void The_class_names_the_api_state_the_diagnostic_found()
    {
        // AC-1: the class is the pressure state; the rule is the sentence the owner's diagnostic produced.
        Assert.Equal(CasingPressureRules.BelowThreshold, CasingPressureRules.ClassOf(Finding(aboveThreshold: false)));

        Assert.Equal(CasingPressureRules.Sustained,
            CasingPressureRules.ClassOf(Finding(aboveThreshold: true, sustained: true)));

        Assert.Equal(CasingPressureRules.NotSustained,
            CasingPressureRules.ClassOf(Finding(aboveThreshold: true, sustained: false)));
    }

    [Fact]
    public void A_pressure_above_the_maasp_is_critical_whatever_the_class_says()
    {
        // AC-3: never a warning, even when the bleed-down looked clean or the pressure is below the diagnostic threshold.
        var clean = Finding(aboveThreshold: true, sustained: false, initial: 3000);

        var (severity, exceedance) = CasingPressureRules.SeverityOf(clean, maasp: 2000);

        Assert.Equal(CasingPressureRules.Critical, severity);
        Assert.Equal(1000d, exceedance!.Value, 6);

        // Even a pressure below the diagnostic threshold is critical when it is above the MAASP.
        Assert.Equal(CasingPressureRules.Critical,
            CasingPressureRules.SeverityOf(Finding(aboveThreshold: false, initial: 3000), 2000).Severity);

        // The highest of the initial and build-up pressures decides, not the initial alone.
        var rebuilt = Finding(aboveThreshold: true, sustained: true, initial: 500, buildUp: 2400);

        Assert.Equal(CasingPressureRules.Critical, CasingPressureRules.SeverityOf(rebuilt, 2000).Severity);
        Assert.Equal(400d, CasingPressureRules.SeverityOf(rebuilt, 2000).Exceedance!.Value, 6);

        // Below the MAASP, a sustained pressure is a warning and a clean test is information.
        Assert.Equal(CasingPressureRules.Warning, CasingPressureRules.SeverityOf(rebuilt, 3000).Severity);
        Assert.Equal(CasingPressureRules.Info, CasingPressureRules.SeverityOf(clean, 3000).Severity);

        // With no MAASP there is nothing to exceed, so the class decides.
        Assert.Equal(CasingPressureRules.Info, CasingPressureRules.SeverityOf(clean, null).Severity);
        Assert.Null(CasingPressureRules.SeverityOf(clean, null).Exceedance);
    }

    [Fact]
    public void The_action_follows_the_severity_and_then_the_class()
    {
        var critical = CasingPressureRules.ActionOf(CasingPressureRules.Critical, CasingPressureRules.Sustained, 2000);

        Assert.Contains("Reduce the annular pressure below the MAASP", critical, StringComparison.Ordinal);
        Assert.Contains("2000", critical, StringComparison.Ordinal);
        Assert.Contains("re-test", critical, StringComparison.Ordinal);

        var sustained = CasingPressureRules.ActionOf(CasingPressureRules.Warning, CasingPressureRules.Sustained, 3000);

        Assert.Contains("risk assessment", sustained, StringComparison.Ordinal);
        Assert.Contains("not an automatic shutdown", sustained, StringComparison.Ordinal);

        Assert.Contains("did not rebuild", CasingPressureRules.ActionOf(
            CasingPressureRules.Info, CasingPressureRules.NotSustained, null), StringComparison.Ordinal);

        Assert.Contains("Below the diagnostic threshold", CasingPressureRules.ActionOf(
            CasingPressureRules.Info, CasingPressureRules.BelowThreshold, null), StringComparison.Ordinal);
    }

    [Fact]
    public void The_thresholds_are_the_configured_ones_and_their_absence_is_stated()
    {
        // AC-6: the settings ship with the API RP 90 values, and nothing here carries its own copy.
        var settings = new CasingPressureSettings();

        Assert.Equal(100d, settings.DiagnosticThreshold.In("psi"), 6);
        Assert.Equal(24d, settings.BleedHours);
        Assert.Null(CasingPressureRules.NotConfigured(settings));

        var noThreshold = new CasingPressureSettings
        {
            DiagnosticThreshold = new TheTechIdea.OilGas.Wellbore.Data.Measure(0, "psi"),
        };

        Assert.Contains("diagnostic threshold is not configured",
            CasingPressureRules.NotConfigured(noThreshold)!, StringComparison.Ordinal);

        var noBleed = new CasingPressureSettings { BleedHours = 0 };

        Assert.Contains("bleed-down allowance is not configured",
            CasingPressureRules.NotConfigured(noBleed)!, StringComparison.Ordinal);
    }

    [Fact]
    public void Readings_are_parsed_line_by_line_and_a_bad_line_names_itself()
    {
        var readings = CasingPressurePageView.Readings(
            "2026-11-01 08:00, 1200, psi\n\n2026-11-01 08:06, 0, psi\n", out var problem);

        Assert.Null(problem);
        Assert.Equal(2, readings.Count);
        Assert.Equal(1200m, readings[0].Pressure);
        Assert.Equal("psi", readings[0].Unit);
        Assert.Equal(new DateTime(2026, 11, 1, 8, 6, 0), readings[1].At);

        // A unit that is not written is psi, which is what the owner's pressures are read in.
        Assert.Equal("psi", CasingPressurePageView.Readings("2026-11-01 08:00, 300", out _)[0].Unit);

        var bad = CasingPressurePageView.Readings("2026-11-01 08:00, lots, psi", out var badProblem);

        Assert.Empty(bad);
        Assert.Contains("'2026-11-01 08:00, lots, psi'", badProblem!, StringComparison.Ordinal);

        Assert.Empty(CasingPressurePageView.Readings(" ", out var emptyProblem));
        Assert.Contains("at least one pressure reading", emptyProblem!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_test_is_refused_without_an_actor_a_well_an_annulus_or_a_reading()
    {
        var offline = new OgCasingPressureService(() => null, new Quiet());
        var readings = new List<OgPressureReading> { new(1200m, "psi", DateTime.UtcNow) };

        var noActor = await offline.RecordTestAsync("W-1", "A", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, 20m, "degC", "", readings, " ");

        Assert.False(noActor.Success);
        Assert.Equal(CasingPressureRules.CodeActorRequired, noActor.Reference);

        var noWell = await offline.RecordTestAsync(" ", "A", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, 20m, "degC", "", readings, "sara");

        Assert.False(noWell.Success);
        Assert.Equal(CasingPressureRules.CodeWellRequired, noWell.Reference);

        var noAnnulus = await offline.RecordTestAsync("W-1", " ", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, 20m, "degC", "", readings, "sara");

        Assert.False(noAnnulus.Success);
        Assert.Equal(CasingPressureRules.CodeAnnulusRequired, noAnnulus.Reference);

        var noReadings = await offline.RecordTestAsync("W-1", "A", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, 20m, "degC", "", [], "sara");

        Assert.False(noReadings.Success);
        Assert.Equal(CasingPressureRules.CodeNoReadings, noReadings.Reference);

        var noWellId = await offline.LoadAsync(" ", null);

        Assert.False(noWellId.Success);
        Assert.Equal(CasingPressureRules.CodeWellRequired, noWellId.Reference);
    }

    [Fact]
    public void The_limits_text_carries_the_governing_term_and_every_limit()
    {
        // AC-2: the number is never shown without its basis.
        var annulus = new OgAnnulusView(
            "A", "PRODUCTION", "SURFACE", "GAS", 9, "ppg", 1.0, "psi/ft", 5000, "psi",
            2061, "30% of the MIYP of SURFACE (outermost casing)",
            ["50% of the MIYP of SURFACE = 3435 psi", "30% of the MIYP of SURFACE (outermost casing) = 2061 psi"],
            [],
            3000, "Inner string collapse",
            ["Outer casing burst = 5496 psi (6870 psi x 0.8)", "Inner string collapse = 3000 psi (3000 psi x 1)"],
            [],
            1200, "psi", DateTime.UtcNow, "within the MAWOP");

        var text = annulus.LimitsText;

        Assert.Contains("MAWOP 2061", text, StringComparison.Ordinal);
        Assert.Contains("governed by 30% of the MIYP", text, StringComparison.Ordinal);
        Assert.Contains("MAASP 3000", text, StringComparison.Ordinal);
        Assert.Contains("MAWOP term: 50% of the MIYP of SURFACE = 3435 psi", text, StringComparison.Ordinal);
        Assert.Contains("MAASP limit: Outer casing burst = 5496 psi (6870 psi x 0.8)", text, StringComparison.Ordinal);
        Assert.True(annulus.CanClassify);

        // A MAWOP that could not be computed is stated with the missing input, and the recorder is disabled.
        var broken = annulus with { Mawop = null, MawopNotes = ["the annulus's inner or outer string is not in the model"] };

        Assert.False(broken.CanClassify);
        Assert.Contains("MAWOP not computed: the annulus's inner or outer string", broken.LimitsText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_shows_the_limits_the_classification_and_gates_the_recorder()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("workspace.Selected?.LimitsText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.ClassificationText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.History", page, StringComparison.Ordinal);

        // The feature's state: a MAWOP that cannot be computed disables the recorder.
        Assert.Contains("CanClassify: true", page, StringComparison.Ordinal);

        // A failed read clears the history rather than showing an empty one.
        Assert.Contains("_historyGrid.ItemsSource = null", page, StringComparison.Ordinal);
    }

    private static CasingPressureFinding Finding(
        bool aboveThreshold,
        bool sustained = false,
        double initial = 1200,
        double buildUp = 0) =>
        new()
        {
            InitialPressure = initial,
            MinimumBleedPressure = 0,
            HoursToZero = 0.1,
            MaximumBuildUp = buildUp,
            AboveThreshold = aboveThreshold,
            BledToZero = true,
            Sustained = sustained,
            Summary = "from the owner's diagnostic",
        };

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
