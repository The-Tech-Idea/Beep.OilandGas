using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.OilGas.Integrity;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ENG-03: the remaining-life basis (AC-4), the damage-class note (AC-2), the export's agreement with the rows (AC-5) and the
/// refusals - the parts that need no database and no caliper file. The joint measurement itself is proven in the gateway test.
/// </summary>
public sealed class MetalLossTests
{
    private const string PageFileName = "MetalLossPageView.cs";

    private static readonly DateTime Now = new(2026, 11, 8, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void No_earlier_inspection_is_a_basis_not_recorded()
    {
        // AC-4: nothing is projected from a single inspection, and the sentence says why.
        var life = MetalLossRules.RemainingLife(null, null, null, 0.2, 1000, Now, 0.5);

        Assert.False(life.HasBasis);
        Assert.Null(life.YearsRemaining);
        Assert.Contains("Basis not recorded", life.Basis, StringComparison.Ordinal);
        Assert.Contains("no earlier caliper result", life.Basis, StringComparison.Ordinal);
        Assert.Contains("Basis not recorded", life.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_penetration_at_a_different_depth_cannot_form_a_rate()
    {
        var life = MetalLossRules.RemainingLife(0.15, 900, Now.AddYears(-1), 0.2, 1000, Now, 0.5);

        Assert.False(life.HasBasis);
        Assert.Contains("moved from 900 to 1000", life.Basis, StringComparison.Ordinal);
        Assert.Contains("cannot be stated", life.Basis, StringComparison.Ordinal);
    }

    [Fact]
    public void A_recorded_basis_without_a_damage_limit_states_the_rate_and_no_projection()
    {
        // AC-4 and AC-2 together: the classes ship empty, so there is no limit to project to even with two dated inspections.
        var life = MetalLossRules.RemainingLife(0.15, 1000, Now.AddDays(-365.25), 0.2, 1000, Now, limit: null);

        Assert.False(life.HasBasis);
        Assert.Equal(0.05, life.RatePerYear!.Value, 6);
        Assert.Null(life.YearsRemaining);
        Assert.Contains("no damage limit is configured", life.Basis, StringComparison.Ordinal);
    }

    [Fact]
    public void A_recorded_basis_with_a_limit_projects_the_years_to_it()
    {
        var life = MetalLossRules.RemainingLife(0.15, 1000, Now.AddDays(-365.25), 0.2, 1000, Now, 0.5);

        Assert.True(life.HasBasis);
        Assert.Equal(0.05, life.RatePerYear!.Value, 6);
        Assert.Equal(6.0, life.YearsRemaining!.Value, 2);
        Assert.Contains("projected to the 50 % damage limit", life.Basis, StringComparison.Ordinal);
        Assert.Contains("year(s) to the damage limit", life.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_joint_already_past_the_limit_has_no_life_left_and_is_not_given_a_negative_one()
    {
        var life = MetalLossRules.RemainingLife(0.4, 1000, Now.AddDays(-365.25), 0.6, 1000, Now, 0.5);

        Assert.True(life.HasBasis);
        Assert.Equal(0d, life.YearsRemaining);
        Assert.Contains("at or beyond", life.Basis, StringComparison.Ordinal);
    }

    [Fact]
    public void No_loss_between_inspections_is_not_projected_from_a_zero_rate()
    {
        var life = MetalLossRules.RemainingLife(0.2, 1000, Now.AddDays(-365.25), 0.2, 1000, Now, 0.5);

        Assert.False(life.HasBasis);
        Assert.Null(life.YearsRemaining);
        Assert.Contains("rate of zero", life.Basis, StringComparison.Ordinal);
    }

    [Fact]
    public void The_damage_note_explains_an_empty_class_list_rather_than_inventing_bands()
    {
        // AC-2: the classes are company practice and ship empty.
        var empty = MetalLossRules.DamageClassNote([]);

        Assert.Contains("No damage class is configured", empty, StringComparison.Ordinal);
        Assert.Contains("invented", empty, StringComparison.Ordinal);
        Assert.Null(MetalLossRules.LimitOf([]));

        var configured = MetalLossRules.DamageClassNote(
        [
            new DamageClass { Name = "Moderate", PenetrationFrom = 0.2 },
            new DamageClass { Name = "Severe", PenetrationFrom = 0.5 },
        ]);

        Assert.Contains("2 damage class(es) configured", configured, StringComparison.Ordinal);
        Assert.Contains("Moderate from 20 %", configured, StringComparison.Ordinal);
        Assert.Contains("Severe from 50 %", configured, StringComparison.Ordinal);
        Assert.Equal(0.2, MetalLossRules.LimitOf(
        [
            new DamageClass { Name = "Moderate", PenetrationFrom = 0.2 },
            new DamageClass { Name = "Severe", PenetrationFrom = 0.5 },
        ]));
    }

    [Fact]
    public void The_export_is_the_grid_row_for_row()
    {
        // AC-5.
        var joints = new List<OgJointResult>
        {
            Joint(1, 1000, 1010, nominalWall: 0.5, minWall: 0.4, penetration: 0.2, damage: "Moderate", burst: 8000),
            Joint(2, 1010, 1020, nominalWall: 0.5, minWall: 0.45, penetration: 0.1, damage: null, burst: 9000),
        };

        var lines = MetalLossRules.Csv(joints)
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(
            "joint,top,bottom,unit,nominal_wall_in,min_wall_in,loss_percent,damage_class,grade,burst_psi,collapse_psi,tension_lbf,verdict",
            lines[0]);
        Assert.Equal(3, lines.Length);

        Assert.Contains("\"Moderate\"", lines[1], StringComparison.Ordinal);
        Assert.Contains("20", lines[1], StringComparison.Ordinal);
        Assert.Contains("8000", lines[1], StringComparison.Ordinal);
        Assert.Contains("\"passes\"", lines[1], StringComparison.Ordinal);

        // A joint with no damage class writes an empty quoted field, not a made-up name.
        Assert.Contains(",\"\",\"L80\"", lines[2], StringComparison.Ordinal);
    }

    [Fact]
    public void A_joint_states_its_loss_and_its_verdict()
    {
        var joint = Joint(1, 1000, 1010, 0.5, 0.4, 0.2, "Moderate", 8000);

        Assert.Equal(20d, joint.LossPercent!.Value, 6);
        Assert.Contains("20 %", joint.LossText, StringComparison.Ordinal);
        Assert.Contains("loss 20 %", joint.Describe(), StringComparison.Ordinal);
        Assert.Contains("Moderate", joint.Describe(), StringComparison.Ordinal);

        var unrated = joint with { Pass = null };

        Assert.Equal("not rated", unrated.VerdictText);
    }

    [Fact]
    public async Task The_analysis_is_refused_without_a_well_a_file_a_load_case_or_an_actor()
    {
        var offline = new OgMetalLossService(() => null, new Quiet());

        var noActor = await offline.AnalyseAsync("W-1", "caliper.las", 10m, 5000m, " ");

        Assert.False(noActor.Success);
        Assert.Equal(MetalLossRules.CodeActorRequired, noActor.Reference);

        var noWell = await offline.AnalyseAsync(" ", "caliper.las", 10m, 5000m, "sara");

        Assert.False(noWell.Success);
        Assert.Equal(MetalLossRules.CodeWellRequired, noWell.Reference);

        var noFile = await offline.AnalyseAsync("W-1", Path.Combine(Path.GetTempPath(), "not-a-caliper.las"), 10m, 5000m, "sara");

        Assert.False(noFile.Success);
        Assert.Equal(MetalLossRules.CodeNoLog, noFile.Reference);
        Assert.Contains("No multi-finger caliper file was found", noFile.Message!, StringComparison.Ordinal);

        // A file that exists is refused for the missing load case before it is ever read.
        var placeholder = Path.Combine(Path.GetTempPath(), "metal-loss-" + Guid.NewGuid().ToString("N")[..8] + ".las");
        await File.WriteAllTextAsync(placeholder, "~Version\n");

        try
        {
            var noLoad = await offline.AnalyseAsync("W-1", placeholder, 0m, 0m, "sara");

            Assert.False(noLoad.Success);
            Assert.Equal(MetalLossRules.CodeNoLoadCase, noLoad.Reference);
        }
        finally
        {
            File.Delete(placeholder);
        }
    }

    [Fact]
    public void The_page_shows_the_worst_joint_the_life_basis_and_the_export()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("workspace.WorstText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Life.Describe()", page, StringComparison.Ordinal);
        Assert.Contains("workspace.DamageClassNote", page, StringComparison.Ordinal);
        Assert.Contains("MetalLossRules.Csv(workspace.Joints)", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Rerate", page, StringComparison.Ordinal);

        // A failed run clears the joints rather than showing an empty grid.
        Assert.Contains("_jointGrid.ItemsSource = null", page, StringComparison.Ordinal);
    }

    private static OgJointResult Joint(
        int number,
        double top,
        double bottom,
        double nominalWall,
        double minWall,
        double penetration,
        string? damage,
        double burst) =>
        new(
            number, top, bottom, "ft", penetration, top + 5, 0.1877, minWall, nominalWall, 0.002, 6.2,
            damage, "PRODUCTION", "L80", minWall, burst, 3000, 400000, 5000, 520, 120000,
            burst + 1000, 3100, 420000, Pass: true, []);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
