using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Core.Wells;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WEL-01: the row filter, the shared well context and the saved-search file - the parts that do not need a
/// database. The same filter is proven against a real 5 000-row database in <see cref="WellSearchGatewayTests"/>.
/// </summary>
public sealed class WellSearchTests
{
    [Fact]
    public void A_free_text_search_matches_name_uwi_and_alternate_identity()
    {
        var wells = new[]
        {
            new WELL { UWI = "W-1", WELL_NAME = "ALPHA-1" },
            new WELL { UWI = "W-2", WELL_NAME = "BRAVO-2" },
            new WELL { UWI = "W-3", WELL_NAME = "CHARLIE-3" },
        };

        var xrefs = new[]
        {
            new WELL_XREF { UWI = "W-2", XREF_ID = "42-001-00002", XREF_TYPE = "API" },
        };

        Assert.Equal("W-1", Assert.Single(OgWellSearch.Filter(wells, xrefs, new OgWellQuery(Text: "ALPHA"))).UWI);
        Assert.Equal("W-2", Assert.Single(OgWellSearch.Filter(wells, xrefs, new OgWellQuery(Text: "W-2"))).UWI);
        Assert.Equal("W-2", Assert.Single(OgWellSearch.Filter(wells, xrefs, new OgWellQuery(Text: "001-00002"))).UWI);
    }

    [Fact]
    public void A_search_is_case_insensitive_and_does_not_treat_a_name_as_a_pattern()
    {
        var wells = new[]
        {
            new WELL { UWI = "W-1", WELL_NAME = "ALPHA-1" },
            new WELL { UWI = "W-2", WELL_NAME = "BRAVO-2" },
        };

        Assert.Equal("W-1", Assert.Single(OgWellSearch.Filter(wells, [], new OgWellQuery(Text: "alpha"))).UWI);
        Assert.Empty(OgWellSearch.Filter(wells, [], new OgWellQuery(Text: "ALPHA_1")));
        Assert.Empty(OgWellSearch.Filter(wells, [], new OgWellQuery(Text: "%")));
    }

    [Fact]
    public void Every_filter_that_is_set_must_match()
    {
        var wells = new[]
        {
            new WELL
            {
                UWI = "W-1",
                WELL_NAME = "ALPHA-1",
                ASSIGNED_FIELD = "NORTH",
                CURRENT_STATUS = "ACTIVE",
                WELL_LEVEL_TYPE = "OIL",
                SPUD_DATE = new DateTime(1995, 5, 1),
            },
            new WELL
            {
                UWI = "W-2",
                WELL_NAME = "ALPHA-2",
                ASSIGNED_FIELD = "NORTH",
                CURRENT_STATUS = "SUSPENDED",
                WELL_LEVEL_TYPE = "OIL",
                SPUD_DATE = new DateTime(1995, 5, 1),
            },
        };

        var query = new OgWellQuery(
            Field: "NORTH",
            Status: "ACTIVE",
            WellType: "OIL",
            SpudYearFrom: 1990,
            SpudYearTo: 2000);

        Assert.Equal("W-1", Assert.Single(OgWellSearch.Filter(wells, [], query)).UWI);
    }

    [Fact]
    public void No_filter_selects_every_row()
    {
        var wells = new[]
        {
            new WELL { UWI = "W-1", WELL_NAME = "ALPHA-1" },
            new WELL { UWI = "W-2", WELL_NAME = "BRAVO-2" },
        };

        Assert.Equal(2, OgWellSearch.Filter(wells, [], new OgWellQuery()).Count());
        Assert.False(new OgWellQuery().HasFilters);
    }

    [Fact]
    public void Selecting_a_well_sets_the_context_and_cancels_the_previous_load()
    {
        var context = new OgWellContext();
        var (token, version) = context.BeginLoad();

        Assert.False(token.IsCancellationRequested);
        Assert.True(context.IsCurrent(version));

        var changed = 0;
        context.Changed += (_, _) => changed++;

        context.Select(Well("W-1"));

        // AC-4
        Assert.True(token.IsCancellationRequested);
        Assert.False(context.IsCurrent(version));
        Assert.Equal("W-1", context.Current!.Id);
        Assert.Equal(1, changed);
    }

    [Fact]
    public void Clearing_the_context_also_cancels_an_in_flight_load()
    {
        var context = new OgWellContext();
        context.Select(Well("W-1"));
        var (token, version) = context.BeginLoad();

        context.Clear();

        Assert.True(token.IsCancellationRequested);
        Assert.False(context.IsCurrent(version));
        Assert.Null(context.Current);
    }

    [Fact]
    public void A_saved_search_survives_a_restart()
    {
        var path = TempSearchPath();
        var first = new OgSavedWellSearchStore(new Quiet(), path);

        first.Upsert(new OgSavedWellSearch("North active", Text: "ALPHA", Field: "NORTH", SpudYearFrom: 2010));

        // A new store instance is what a restart means: it reads the same file.
        var second = new OgSavedWellSearchStore(new Quiet(), path);
        var loaded = second.Load();

        Assert.True(loaded.Readable);
        var search = Assert.Single(loaded.Searches);
        Assert.Equal("North active", search.Name);
        Assert.Equal("ALPHA", search.Text);
        Assert.Equal("NORTH", search.Field);
        Assert.Equal(2010, search.SpudYearFrom);

        second.Upsert(new OgSavedWellSearch("north ACTIVE", Text: "BRAVO"));
        var replaced = new OgSavedWellSearchStore(new Quiet(), path).Load();

        Assert.Single(replaced.Searches);
        Assert.Equal("BRAVO", replaced.Searches[0].Text);

        second.Remove("North active");
        Assert.Empty(new OgSavedWellSearchStore(new Quiet(), path).Load().Searches);
    }

    [Fact]
    public void A_corrupt_saved_search_file_is_a_problem_not_a_crash()
    {
        var path = TempSearchPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "{ this is not json");

        var reporter = new Recording();
        var store = new OgSavedWellSearchStore(reporter, path);
        var loaded = store.Load();

        Assert.False(loaded.Readable);
        Assert.Empty(loaded.Searches);
        Assert.Single(reporter.Failures);
    }

    private static OgWellSummary Well(string id) =>
        new(id, "ALPHA-1", "42-001-00001", "NORTH", "ACTIVE", "OIL", new DateTime(2020, 5, 1), null);

    private static string TempSearchPath()
    {
        var directory = Path.Combine(OgPaths.DataRoot, "wellsearch-" + Guid.NewGuid().ToString("n")[..8]);
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "searches.json");
    }

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }

    private sealed class Recording : IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Failures.Add(operation);
            return FailureReferenceFormat.New();
        }
    }
}
