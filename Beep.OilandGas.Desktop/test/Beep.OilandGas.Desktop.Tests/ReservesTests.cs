using TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// RES-05: the totals rule, the validation, the revision rule, the disclosure gate and the separation of duties - the
/// parts that do not need a database. The reads, the characterization sources and the saved disclosure are proven in the
/// gateway test.
/// </summary>
public sealed class ReservesTests
{
    [Fact]
    public void Totals_exclude_unclassified_entries_and_say_how_many()
    {
        // AC-1.
        var entries = new[]
        {
            Entry("P-1", "PROVED", 100m, 80m),
            Entry("P-1", "PROVED", 50m, 40m, revision: 2),
            Entry("P-2", null, 900m, 900m),
        };

        var totals = ReserveRules.Totals(entries);

        var proved = Assert.Single(totals);

        Assert.Equal("PROVED", proved.Category);
        Assert.Equal(2, proved.Count);
        Assert.Equal(150m, proved.Gross);
        Assert.Equal(120m, proved.Net);
        Assert.Equal("bbl", proved.Unit);

        // The unclassified 900 bbl is nowhere in the totals.
        Assert.DoesNotContain(totals, total => total.Gross == 900m || total.Net == 900m);

        var workspace = Workspace(entries);

        Assert.Equal(1, workspace.UnclassifiedCount);
        Assert.Equal(900m, workspace.UnclassifiedGross);
        Assert.Contains("1 entry/entries are unclassified and excluded", workspace.UnclassifiedText, StringComparison.Ordinal);
        Assert.Contains("900 bbl", workspace.UnclassifiedText, StringComparison.Ordinal);
    }

    [Fact]
    public void Two_units_are_named_rather_than_added()
    {
        var entries = new[]
        {
            Entry("P-1", "PROVED", 100m, 80m),
            Entry("P-3", "PROVED", 2m, 2m, revision: 2, unit: "MMbbl"),
        };

        var total = Assert.Single(ReserveRules.Totals(entries));

        Assert.True(total.HasMixedUnits);
        Assert.Null(total.Gross);
        Assert.Equal("mixed units, so no total", total.GrossText);
        Assert.Equal(["bbl", "MMbbl"], total.MixedUnits);
    }

    [Fact]
    public void An_entry_needs_a_gross_net_unit_and_effective_date_but_not_a_category_or_classifier()
    {
        // AC-2: the entry carries a classifier and an effective date. A category is optional (AC-1 excludes it) and a
        // classifier is optional but flags the entry and blocks disclosure (AC-5) - so neither is refused here.
        Assert.Equal(ReserveRules.CodePoolRequired, ReserveRules.Validate(Values(pool: " "))!.Value.Code);
        Assert.Equal(ReserveRules.CodeCategoryInvalid, ReserveRules.Validate(Values(category: "MAYBE"))!.Value.Code);
        Assert.Equal(ReserveRules.CodeGrossInvalid, ReserveRules.Validate(Values(gross: -1m))!.Value.Code);
        Assert.Equal(ReserveRules.CodeNetInvalid, ReserveRules.Validate(Values(net: -1m))!.Value.Code);
        Assert.Equal(ReserveRules.CodeNetAboveGross, ReserveRules.Validate(Values(gross: 10m, net: 11m))!.Value.Code);
        Assert.Equal(ReserveRules.CodeUnitRequired, ReserveRules.Validate(Values(unit: " "))!.Value.Code);
        Assert.Equal(ReserveRules.CodeUnitInvalid, ReserveRules.Validate(Values(unit: "barrels"))!.Value.Code);

        // The effective date is what AC-2 requires outright.
        Assert.Equal(
            ReserveRules.CodeEffectiveDateRequired,
            ReserveRules.Validate(Values() with { EffectiveDate = null })!.Value.Code);

        // Unclassified and unclassified-by-a-person are allowed through validation; the screen flags them.
        Assert.Null(ReserveRules.Validate(Values(category: null, classifier: null)));
        Assert.Null(ReserveRules.Validate(Values()));
    }

    [Fact]
    public void A_revision_supersedes_the_previous_one_and_history_is_kept()
    {
        // AC-3: the current revision is the newest per pool and category, and the older rows stay.
        var entries = new[]
        {
            Entry("P-1", "PROVED", 100m, 80m),
            Entry("P-1", "PROVED", 120m, 96m, revision: 2),
            Entry("P-1", "PROBABLE", 40m, 32m),
            Entry("P-1", null, 10m, 10m),
        };

        var current = ReserveRules.Current(entries);

        Assert.Equal(3, current.Count);
        Assert.Equal(120m, current.Single(entry => entry.Category == "PROVED").Gross);
        Assert.Contains(current, entry => entry.IsUnclassified);

        // Every row is still in the workspace, history included: the revision did not overwrite anything.
        Assert.Equal(4, Workspace(entries).Entries.Count);
        Assert.Equal(1, Workspace(entries).CurrentRevisionCount);
    }

    [Fact]
    public void The_page_and_the_service_never_update_a_reserves_entry()
    {
        // AC-3 as a guard: the service only adds. A future edit that called Update on RESERVE_ENTRY would fail this.
        var service = File.ReadAllText(
            SourceRoot.AllSources().Single(file => Path.GetFileName(file) == "OgReserves.cs"));

        // No update anywhere in the service, and the revision and supersede columns are set on insert.
        Assert.DoesNotContain(".Update(", service, StringComparison.Ordinal);
        Assert.DoesNotContain(".Delete(", service, StringComparison.Ordinal);
        Assert.Contains("REVISION = (previous?.REVISION ?? 0) + 1", service, StringComparison.Ordinal);
        Assert.Contains("SUPERSEDES_ENTRY_ID = previous?.ENTRY_ID", service, StringComparison.Ordinal);
    }

    [Fact]
    public void Disclosure_is_blocked_while_an_entry_has_no_classifier()
    {
        // AC-5.
        var withClassifier = new[]
        {
            Entry("P-1", "PROVED", 100m, 80m),
            Entry("P-1", "PROBABLE", 40m, 32m),
        };

        Assert.Equal(string.Empty, ReserveRules.BlockedReason(withClassifier));
        Assert.False(Workspace(withClassifier).DisclosureBlocked);
        Assert.Contains("disclosure available", Workspace(withClassifier).DisclosureStateText, StringComparison.Ordinal);

        var missing = new[]
        {
            Entry("P-1", "PROVED", 100m, 80m, classifier: null),
        };

        var reason = ReserveRules.BlockedReason(missing);

        Assert.Contains("1 entry/entries carry no classifier", reason, StringComparison.Ordinal);
        Assert.Contains("P-1", reason, StringComparison.Ordinal);

        var blocked = Workspace(missing);

        Assert.True(blocked.DisclosureBlocked);
        Assert.Contains("disclosure blocked", blocked.DisclosureStateText, StringComparison.Ordinal);
        Assert.Single(blocked.MissingClassifier);
    }

    [Fact]
    public void A_characterization_fact_always_names_its_source()
    {
        // AC-4: the record says where each fact came from, including "nothing records this".
        var fact = new OgCharacterizationFact("Drive mechanism", "water drive", "DEVELOPMENT_POOL_PLAN.DRIVE_MECHANISM");

        Assert.Contains("(source: DEVELOPMENT_POOL_PLAN.DRIVE_MECHANISM)", fact.Describe(), StringComparison.Ordinal);

        var absent = new OgCharacterizationFact(
            "Reservoir pressure", "not recorded", "no table in this repository records reservoir pressure");

        Assert.Contains("not recorded", absent.Describe(), StringComparison.Ordinal);
        Assert.Contains("(source: no table", absent.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_preview_states_the_exclusions_and_the_entry_count()
    {
        var workspace = Workspace(
        [
            Entry("P-1", "PROVED", 100m, 80m),
            Entry("P-2", null, 900m, 900m),
        ]);

        Assert.Contains("Reserves disclosure preview", workspace.Preview, StringComparison.Ordinal);
        Assert.Contains("proved: 1 entry/entries", workspace.Preview, StringComparison.Ordinal);
        Assert.Contains("excluded (unclassified): 1 entry/entries", workspace.Preview, StringComparison.Ordinal);
        Assert.Contains("entries considered: 2", workspace.Preview, StringComparison.Ordinal);
    }

    [Fact]
    public async Task No_connection_is_a_failure_for_every_command()
    {
        var service = new OgReservesService(() => null, new Quiet());

        Assert.Equal(ReserveRules.CodeNoConnection, (await service.LoadAsync("F-1", "Alpha")).Reference);
        Assert.Equal(
            ReserveRules.CodeNoConnection,
            (await service.AddRevisionAsync(Values(), "sara")).Reference);
        Assert.Equal(
            ReserveRules.CodeNoConnection,
            (await service.ApproveDisclosureAsync("F-1", DateTime.Today, "sara")).Reference);
    }

    [Fact]
    public async Task A_revision_and_an_approval_both_need_an_actor()
    {
        var service = new OgReservesService(() => null, new Quiet());

        Assert.Equal(
            ReserveRules.CodeActorRequired,
            (await service.AddRevisionAsync(Values(), " ")).Reference);

        Assert.Equal(
            ReserveRules.CodeActorRequired,
            (await service.ApproveDisclosureAsync("F-1", DateTime.Today, " ")).Reference);
    }

    private static OgReserveEntryValues Values(
        string? pool = "P-1",
        string? category = "PROVED",
        decimal? gross = 100m,
        decimal? net = 80m,
        string? unit = "bbl",
        string? classifier = "sara") =>
        new(pool, category, gross, net, unit, new DateTime(2026, 10, 1), classifier, "annual revision");

    private static OgReserveEntry Entry(
        string pool,
        string? category,
        decimal gross,
        decimal net,
        int revision = 1,
        string? unit = "bbl",
        string? classifier = "sara") =>
        new(
            "E-" + pool + "-" + (category ?? "NONE") + "-" + revision.ToString(),
            pool,
            "Pool " + pool,
            "F-1",
            category,
            gross,
            net,
            unit,
            new DateTime(2026, 10, 1),
            revision,
            classifier,
            revision > 1 ? "E-" + pool + "-" + (category ?? "NONE") + "-" + (revision - 1).ToString() : null,
            "annual revision",
            "sara",
            new DateTime(2026, 10, 2));

    private static OgReserveWorkspace Workspace(IReadOnlyList<OgReserveEntry> entries)
    {
        var current = ReserveRules.Current(entries);
        var totals = ReserveRules.Totals(current);
        var unclassified = current.Where(entry => entry.IsUnclassified).ToList();
        var missing = current.Where(entry => entry.IsUnclassifiedByPerson).ToList();
        var reason = ReserveRules.BlockedReason(current);

        return new OgReserveWorkspace(
            "Alpha field",
            "F-1",
            new DateTime(2026, 10, 1),
            entries,
            totals,
            unclassified.Count,
            unclassified.Count == 0 ? null : unclassified.Sum(entry => entry.Gross ?? 0m),
            unclassified.Count == 0 ? null : unclassified.Sum(entry => entry.Net ?? 0m),
            unclassified.Select(entry => entry.Unit).FirstOrDefault(value => value is not null),
            missing,
            [],
            [],
            [],
            PreviewOf(current, totals, unclassified),
            reason.Length > 0,
            reason,
            string.Join(", ", ReserveUnits.Units),
            false,
            null);
    }

    private static string PreviewOf(
        IReadOnlyList<OgReserveEntry> current,
        IReadOnlyList<OgReserveTotal> totals,
        IReadOnlyList<OgReserveEntry> unclassified) =>
        string.Join(
            Environment.NewLine,
            new[]
            {
                "Reserves disclosure preview · field F-1 · 2026-10",
            }
                .Concat(totals.Select(total => "  " + total.Describe()))
                .Concat(new[]
                {
                    "  excluded (unclassified): " +
                    unclassified.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) + " entry/entries",
                    "  entries considered: " + current.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                }));

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
