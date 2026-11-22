using System.Diagnostics;
using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core.Wells;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WEL-01 AC-1 and AC-2 against a real SQLite database: name, UWI and API find the expected well, 5 000 rows page
/// and filter in the database, and a missing table is a failure rather than an empty list.
/// </summary>
/// <remarks>Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>): it provisions a local database through BeepDM.</remarks>
public sealed class WellSearchGatewayTests
{
    private const string ConnectionName = "wel01-search";
    private const int TotalWells = 5_000;

    [IntegrationFact]
    public async Task Name_uwi_and_api_find_the_well_and_5000_rows_page_and_filter_in_the_database()
    {        var root = Path.Combine(OgPaths.DataRoot, "wel01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWel01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "wells.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "wells.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var search = new OgWellSearch(() => source, reporter);
        var timings = new List<string>();

        // AC-1: all three identities reach the same well.
        var nameClock = Stopwatch.StartNew();
        var byName = await search.SearchAsync(new OgWellQuery(Text: "ALPHA-4321"));
        nameClock.Stop();
        var byUwi = await search.SearchAsync(new OgWellQuery(Text: "UWI-0004321"));
        var byApi = await search.SearchAsync(new OgWellQuery(Text: "42-001-04321"));

        Assert.True(byName.Success, byName.Message);
        Assert.Equal("UWI-0004321", Assert.Single(byName.Value!.Items).Id);
        Assert.Equal("UWI-0004321", Assert.Single(byUwi.Value!.Items).Id);
        Assert.Equal("UWI-0004321", Assert.Single(byApi.Value!.Items).Id);
        Assert.Equal("42-001-04321", byApi.Value!.Items[0].ApiNumber);

        // AC-2: one bounded page, then a filtered page, both from the database.
        var clock = Stopwatch.StartNew();
        var firstPage = await search.SearchAsync(new OgWellQuery(Page: 0, PageSize: 100));
        clock.Stop();

        Assert.True(firstPage.Success, firstPage.Message);
        Assert.Equal(TotalWells, firstPage.Value!.Total);
        Assert.Equal(100, firstPage.Value.Items.Count);
        Assert.True(firstPage.Value.HasMore);
        Assert.True(clock.Elapsed < TimeSpan.FromSeconds(2), $"first page took {clock.ElapsedMilliseconds} ms");

        var filterClock = Stopwatch.StartNew();
        var filtered = await search.SearchAsync(new OgWellQuery(Field: "NORTH", PageSize: 100));
        filterClock.Stop();

        Assert.True(filtered.Success, filtered.Message);
        Assert.Equal(TotalWells / 2, filtered.Value!.Total);
        Assert.All(filtered.Value.Items, well => Assert.Equal("NORTH", well.Field));

        var yearFiltered = await search.SearchAsync(new OgWellQuery(SpudYearFrom: 2020, SpudYearTo: 2021, PageSize: 10));

        Assert.True(yearFiltered.Success, yearFiltered.Message);
        Assert.All(yearFiltered.Value!.Items, well => Assert.InRange(well.SpudDate!.Value.Year, 2020, 2021));

        var lastPage = await search.SearchAsync(new OgWellQuery(Page: 49, PageSize: 100));

        Assert.Equal(100, lastPage.Value!.Items.Count);
        Assert.False(lastPage.Value.HasMore);

        // AC-3: no match is an empty page (success), a failed read is a failure - the two are different.
        var noMatch = await search.SearchAsync(new OgWellQuery(Field: "NOWHERE", PageSize: 10));

        Assert.True(noMatch.Success, noMatch.Message);
        Assert.Empty(noMatch.Value!.Items);
        Assert.Equal(0, noMatch.Value.Total);

        // AC-3: a failed read is a failure with a reference, never an empty page.
        DropWellTables(database);
        var failed = await search.SearchAsync(new OgWellQuery());

        timings.Add($"name {nameClock.ElapsedMilliseconds} ms");
        timings.Add($"first page {clock.ElapsedMilliseconds} ms");
        timings.Add($"filter {filterClock.ElapsedMilliseconds} ms");
        File.WriteAllLines(Path.Combine(Path.GetTempPath(), "wel01-search.txt"), timings);

        Assert.False(failed.Success);
        Assert.NotNull(failed.Reference);
        Assert.Empty(failed.Value?.Items ?? []);

        var noConnection = await new OgWellSearch(() => null, reporter).SearchAsync(new OgWellQuery());
        Assert.False(noConnection.Success);
        Assert.Equal(OgWellSearch.CodeNoConnection, noConnection.Reference);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using (var create = connection.CreateCommand())
        {
            create.CommandText =
                """
                CREATE TABLE WELL (
                    UWI TEXT NOT NULL PRIMARY KEY,
                    WELL_NAME TEXT NULL,
                    ASSIGNED_FIELD TEXT NULL,
                    CURRENT_STATUS TEXT NULL,
                    WELL_LEVEL_TYPE TEXT NULL,
                    SPUD_DATE TEXT NULL,
                    CURRENT_STATUS_DATE TEXT NULL);
                CREATE TABLE WELL_XREF (
                    UWI TEXT NOT NULL,
                    XREF_ID TEXT NOT NULL,
                    XREF_TYPE TEXT NULL,
                    PRIMARY KEY (UWI, XREF_ID));
                """;
            create.ExecuteNonQuery();
        }

        using var transaction = connection.BeginTransaction();

        using (var insert = connection.CreateCommand())
        {
            insert.Transaction = transaction;
            insert.CommandText =
                """
                INSERT INTO WELL (UWI, WELL_NAME, ASSIGNED_FIELD, CURRENT_STATUS, WELL_LEVEL_TYPE, SPUD_DATE, CURRENT_STATUS_DATE)
                VALUES ($uwi, $name, $field, $status, $type, $spud, $activity);
                """;

            var uwi = insert.Parameters.Add("$uwi", SqliteType.Text);
            var name = insert.Parameters.Add("$name", SqliteType.Text);
            var field = insert.Parameters.Add("$field", SqliteType.Text);
            var status = insert.Parameters.Add("$status", SqliteType.Text);
            var type = insert.Parameters.Add("$type", SqliteType.Text);
            var spud = insert.Parameters.Add("$spud", SqliteType.Text);
            var activity = insert.Parameters.Add("$activity", SqliteType.Text);

            for (var index = 0; index < TotalWells; index++)
            {
                uwi.Value = $"UWI-{index:0000000}";
                name.Value = $"ALPHA-{index}";
                field.Value = index % 2 == 0 ? "NORTH" : "SOUTH";
                status.Value = index % 3 == 0 ? "ACTIVE" : "SUSPENDED";
                type.Value = "OIL";
                spud.Value = new DateTime(2000 + index % 30, 1 + index % 12, 1).ToString("yyyy-MM-dd");
                activity.Value = "2026-01-01";
                insert.ExecuteNonQuery();
            }
        }

        using (var xref = connection.CreateCommand())
        {
            xref.Transaction = transaction;
            xref.CommandText = "INSERT INTO WELL_XREF (UWI, XREF_ID, XREF_TYPE) VALUES ($uwi, $api, 'API');";

            var uwi = xref.Parameters.Add("$uwi", SqliteType.Text);
            var api = xref.Parameters.Add("$api", SqliteType.Text);

            for (var index = 0; index < TotalWells; index++)
            {
                uwi.Value = $"UWI-{index:0000000}";
                api.Value = $"42-001-{index:00000}";
                xref.ExecuteNonQuery();
            }
        }

        transaction.Commit();
    }

    private static void DropWellTables(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var drop = connection.CreateCommand();
        drop.CommandText = "DROP TABLE WELL; DROP TABLE WELL_XREF;";
        drop.ExecuteNonQuery();
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
            Failures.Add($"{operation} | {consequence} | {exception.Message}");
            return FailureReferenceFormat.New();
        }
    }
}
