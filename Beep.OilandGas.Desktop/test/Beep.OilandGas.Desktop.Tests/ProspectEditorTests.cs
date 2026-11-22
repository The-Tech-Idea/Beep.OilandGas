using TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// EXP-03: the rules that hold without a database - a complete set of probabilities produces the chance, a missing
/// element reads "incomplete", the volume percentiles keep their unit and are checked for order, a submitted
/// prospect is locked, every risk save carries an assessor and a date, and the economic summary has no write path.
/// </summary>
public sealed class ProspectEditorTests
{
    [Fact]
    public void A_complete_set_of_probabilities_produces_the_chance()
    {
        var detail = Detail(
            new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase)
            {
                ["TRAP"] = 0.1m,
                ["RESERVOIR"] = 0.2m,
                ["SEAL"] = 0.1m,
                ["SOURCE"] = 0.1m,
                ["TIMING"] = 0.1m,
            });

        // AC-1: 0.9 * 0.8 * 0.9 * 0.9 * 0.9 = 52%.
        Assert.True(detail.RiskComplete);
        Assert.Equal(52m, detail.ChancePercent);
        Assert.Equal("52%", detail.ChanceText);
        Assert.Empty(detail.MissingElements);
    }

    [Fact]
    public void A_missing_element_shows_incomplete_and_never_a_number()
    {
        var detail = Detail(
            new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase)
            {
                ["TRAP"] = 0.1m,
                ["RESERVOIR"] = 0.2m,
                ["SEAL"] = null,
                ["SOURCE"] = 0.1m,
                ["TIMING"] = 0.1m,
            });

        // AC-2
        Assert.False(detail.RiskComplete);
        Assert.Null(detail.ChancePercent);
        Assert.Equal("incomplete", detail.ChanceText);
        Assert.Equal(["SEAL"], detail.MissingElements);
    }

    [Fact]
    public void Volume_percentiles_are_ordered_and_unit_annotated()
    {
        var ordered = new OgProspectVolume(10m, 20m, 30m, "MMBO", "OIL");
        var leaning = new OgProspectVolume(30m, 20m, 10m, "MMBO", "OIL");
        var noUnit = new OgProspectVolume(10m, 20m, 30m, null, "OIL");

        // AC-3
        Assert.True(ordered.IsOrdered);
        Assert.True(ordered.HasUnit);
        Assert.Contains("P90 10", ordered.Describe(), StringComparison.Ordinal);
        Assert.Contains("MMBO", ordered.Describe(), StringComparison.Ordinal);

        Assert.False(leaning.IsOrdered);

        Assert.False(noUnit.HasUnit);
        Assert.Contains("unit not recorded", noUnit.Describe(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("DRAFT", false)]
    [InlineData("ACTIVE", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("SUBMITTED", true)]
    [InlineData("submitted", true)]
    [InlineData("APPROVED", true)]
    [InlineData("CLOSED", true)]
    [InlineData("ARCHIVED", true)]
    public void A_submitted_or_closed_prospect_is_locked(string? status, bool locked)
    {
        // AC-4
        Assert.Equal(locked, OgProspectEditor.IsLocked(status));
    }

    [Fact]
    public async Task A_submitted_prospect_is_not_edited()
    {
        var editor = new OgProspectEditor(() => null!, new Quiet());
        var submitted = Identity("P-1", "Alpha", "SUBMITTED");

        var saved = await editor.SaveIdentityAsync(submitted, "Renamed", null, null, null, "fahad");
        var risk = await editor.SaveRiskAsync(
            "P-1",
            [new OgProspectRiskElement("TRAP", 0.2m, null, "fahad", DateTime.Today)],
            "fahad",
            DateTime.Today);

        Assert.Equal(OgProspectEditor.CodeLocked, saved.Reference);
        Assert.Contains("revise", saved.Message!, StringComparison.OrdinalIgnoreCase);

        // The risk save needs a connection before it can read the status, so with none it stops there; the
        // submitted-status refusal for risk is proven against a real database in the gateway test.
        Assert.Equal(OgProspectEditor.CodeNoConnection, risk.Reference);
    }

    [Fact]
    public async Task Every_risk_save_records_an_assessor_and_a_date()
    {
        var editor = new OgProspectEditor(() => null!, new Quiet());
        var element = new OgProspectRiskElement("TRAP", 0.2m, null, "fahad", DateTime.Today);

        // AC-5
        Assert.Equal(
            OgProspectEditor.CodeActorRequired,
            (await editor.SaveRiskAsync("P-1", [element], " ", DateTime.Today)).Reference);

        Assert.Equal(
            OgProspectEditor.CodeDateRequired,
            (await editor.SaveRiskAsync("P-1", [element], "fahad", default)).Reference);
    }

    [Fact]
    public async Task A_probability_outside_the_unit_interval_is_refused()
    {
        var editor = new OgProspectEditor(() => null!, new Quiet());

        var saved = await editor.SaveRiskAsync(
            "P-1",
            [new OgProspectRiskElement("TRAP", 1.4m, null, "fahad", DateTime.Today)],
            "fahad",
            DateTime.Today);

        Assert.Equal(OgProspectEditor.CodeProbability, saved.Reference);
        Assert.Contains("TRAP", saved.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Saving_nothing_is_refused()
    {
        var editor = new OgProspectEditor(() => null!, new Quiet());

        var saved = await editor.SaveRiskAsync(
            "P-1",
            [new OgProspectRiskElement("TRAP", null, null, "fahad", DateTime.Today)],
            "fahad",
            DateTime.Today);

        Assert.Equal(OgProspectEditor.CodeNothingToSave, saved.Reference);
    }

    [Fact]
    public async Task A_change_must_record_who_made_it_and_a_prospect_needs_a_name()
    {
        var editor = new OgProspectEditor(() => null!, new Quiet());
        var draft = Identity("P-1", "Alpha", "DRAFT");

        Assert.Equal(
            OgProspectEditor.CodeActorRequired,
            (await editor.SaveIdentityAsync(draft, "Alpha", null, null, null, " ")).Reference);

        Assert.Equal(
            OgProspectEditor.CodeNameRequired,
            (await editor.SaveIdentityAsync(draft, " ", null, null, null, "fahad")).Reference);
    }

    [Fact]
    public void The_economic_summary_is_read_only_and_names_its_route()
    {
        // AC-6: no editor command writes an economic summary; the summary is a read model with a route to ECO-01.
        var writes = typeof(IOgProspectEditor)
            .GetMethods()
            .Where(method => method.GetParameters()
                .Any(parameter => parameter.ParameterType == typeof(OgProspectEconomicSummary)))
            .ToList();

        Assert.Empty(writes);
        Assert.Equal("economics", OgProspectEconomicSummary.EvaluationRoute);

        var empty = new OgProspectEconomicSummary(null, null, null, null, null, null, null);
        var filled = empty with { Scenario = "BASE", Npv = 1_000_000m };

        Assert.False(empty.HasContent);
        Assert.True(filled.HasContent);
    }

    private static OgProspectIdentity Identity(string id, string name, string status) =>
        new(id, name, "PLAY-1", "Upper sand", "FIELD-1", "BA-1", "LEAD", status, null);

    private static OgProspectDetail Detail(IReadOnlyDictionary<string, decimal?> risks)
    {
        var complete = OgProspectChance.IsComplete(risks);

        var risk = OgProspectChance.RequiredElements
            .Select(element => new OgProspectRiskElement(
                element, risks[element], null, "fahad", new DateTime(2026, 3, 1)))
            .ToList();

        return new OgProspectDetail(
            Identity("P-1", "Alpha", "DRAFT"),
            new OgProspectTrapDetail(null, null, null, null, null, null),
            new OgProspectReservoirDetail(null, null, null, null, null, null, null, null),
            new OgProspectSourceDetail(null, null, null, null, null, null),
            new OgProspectVolume(null, null, null, null, null),
            risk,
            OgProspectChance.Of(risks),
            complete,
            new OgProspectEconomicSummary(null, null, null, null, null, null, null),
            [],
            IsLocked: false);
    }

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
