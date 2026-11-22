using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WEL-03 against a real SQLite database: the list is only the selected well's bores, a bore cannot exist without
/// its well, a sidetrack records its parent, a depth keeps its unit, and a bore with surveys cannot be deleted.
/// </summary>
/// <remarks>Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>): it provisions a local database through BeepDM.</remarks>
public sealed class WellboreGatewayTests
{
    private const string ConnectionName = "wel03-wellbores";

    [IntegrationFact]
    public async Task Only_the_selected_wells_bores_are_listed_and_the_rules_hold()
    {        var root = Path.Combine(OgPaths.DataRoot, "wel03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWel03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "bores.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "bores.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgWellbores(() => source, reporter);

        // AC-1: only the selected well's bores.
        var a = await service.ListAsync("UWI-A");
        var b = await service.ListAsync("UWI-B");

        Assert.True(a.Success, a.Message);
        Assert.Equal(2, a.Value!.Count);
        Assert.All(a.Value, bore => Assert.Equal("UWI-A", bore.Uwi));
        Assert.Single(b.Value!);

        // AC-4: the depth keeps its unit; the second bore has none.
        var original = Assert.Single(a.Value, bore => bore.Id == "WB-1");
        Assert.True(original.HasDepthUnit);
        Assert.Equal("3000 m", original.DepthText);
        Assert.True(original.SurveyEnabled);

        var noUnit = Assert.Single(a.Value, bore => bore.Id == "WB-2");
        Assert.False(noUnit.HasDepthUnit);
        Assert.False(noUnit.SurveyEnabled);

        // AC-5: the sidetrack's parent is recorded.
        Assert.Equal("WB-1", noUnit.ParentId);
        Assert.Equal("SIDETRACK", noUnit.Kind);

        // AC-2: no bore without its well.
        var orphan = await service.CreateAsync(new OgWellboreRequest("UWI-MISSING", "WB-9"), "fahad");
        Assert.False(orphan.Success);
        Assert.Equal(OgWellbores.CodeParentWellMissing, orphan.Reference);

        // AC-5: a sidetrack needs a parent that exists on the same well.
        var noParent = await service.CreateAsync(
            new OgWellboreRequest("UWI-A", "WB-3", Kind: "SIDETRACK"), "fahad");
        Assert.Equal(OgWellbores.CodeParentBoreRequired, noParent.Reference);

        var wrongParent = await service.CreateAsync(
            new OgWellboreRequest("UWI-A", "WB-3", Kind: "SIDETRACK", ParentId: "WB-X"), "fahad");
        Assert.Equal(OgWellbores.CodeParentBoreUnknown, wrongParent.Reference);

        var sideTrack = await service.CreateAsync(
            new OgWellboreRequest("UWI-A", "WB-3", "WB-1 SIDETRACK", "WB-1", "SIDETRACK", "ACTIVE",
                TotalDepth: 3200m, DepthUnit: "m"),
            "fahad");

        Assert.True(sideTrack.Success, sideTrack.Message);
        var withSideTrack = await service.ListAsync("UWI-A");
        Assert.Equal(3, withSideTrack.Value!.Count);
        Assert.Equal("WB-1", Assert.Single(withSideTrack.Value, bore => bore.Id == "WB-3").ParentId);

        var duplicate = await service.CreateAsync(new OgWellboreRequest("UWI-A", "WB-1"), "fahad");
        Assert.Equal(OgWellbores.CodeDuplicate, duplicate.Reference);

        // AC-3: a bore with surveys is refused, and the count is in the sentence.
        var refused = await service.DeleteAsync("UWI-A", "WB-1", "fahad");

        Assert.False(refused.Success);
        Assert.Equal(OgWellbores.CodeHasSurveys, refused.Reference);
        Assert.Contains("1 survey", refused.Message!, StringComparison.OrdinalIgnoreCase);

        // A bore with a child is refused as well; the survey link is removed first.
        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM WELLBORE_SURVEY WHERE UWI = 'UWI-A' AND WELLBORE_ID = 'WB-1';";
            command.ExecuteNonQuery();
        }

        var hasChild = await service.DeleteAsync("UWI-A", "WB-1", "fahad");
        Assert.Equal(OgWellbores.CodeHasChildren, hasChild.Reference);

        // The last bore of a well cannot be deleted.
        var last = await service.DeleteAsync("UWI-B", "WB-B1", "fahad");
        Assert.Equal(OgWellbores.CodeLastBore, last.Reference);

        // A leaf with no surveys can be deleted.
        var deleted = await service.DeleteAsync("UWI-A", "WB-3", "fahad");
        Assert.True(deleted.Success, deleted.Message);
        Assert.Equal(2, (await service.ListAsync("UWI-A")).Value!.Count);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE WELL (UWI TEXT NOT NULL PRIMARY KEY, WELL_NAME TEXT NULL);
            CREATE TABLE WELLBORE (
                UWI TEXT NOT NULL,
                WELLBORE_ID TEXT NOT NULL,
                WELLBORE_NAME TEXT NULL,
                PARENT_WELLBORE_ID TEXT NULL,
                KIND TEXT NULL,
                STATUS TEXT NULL,
                SPUD_DATE TEXT NULL,
                TOTAL_DEPTH REAL NULL,
                TOTAL_DEPTH_OUOM TEXT NULL,
                SURFACE_LATITUDE REAL NULL,
                SURFACE_LONGITUDE REAL NULL,
                DEPTH_DATUM TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                PRIMARY KEY (UWI, WELLBORE_ID));
            CREATE TABLE WELLBORE_SURVEY (
                UWI TEXT NOT NULL,
                WELLBORE_ID TEXT NOT NULL,
                SURVEY_ID TEXT NOT NULL,
                SOURCE TEXT NOT NULL,
                PRIMARY KEY (UWI, WELLBORE_ID, SURVEY_ID, SOURCE));

            INSERT INTO WELL (UWI, WELL_NAME) VALUES ('UWI-A', 'ALPHA'), ('UWI-B', 'BRAVO');

            INSERT INTO WELLBORE (UWI, WELLBORE_ID, WELLBORE_NAME, PARENT_WELLBORE_ID, KIND, STATUS, SPUD_DATE, TOTAL_DEPTH, TOTAL_DEPTH_OUOM)
            VALUES
                ('UWI-A', 'WB-1', 'ALPHA ORIGINAL', NULL, 'ORIGINAL', 'ACTIVE', '2020-01-01', 3000, 'm'),
                ('UWI-A', 'WB-2', 'ALPHA SIDETRACK', 'WB-1', 'SIDETRACK', 'ACTIVE', '2021-01-01', 3100, NULL),
                ('UWI-B', 'WB-B1', 'BRAVO ORIGINAL', NULL, 'ORIGINAL', 'ACTIVE', '2022-01-01', 2000, 'm');

            INSERT INTO WELLBORE_SURVEY (UWI, WELLBORE_ID, SURVEY_ID, SOURCE)
            VALUES ('UWI-A', 'WB-1', 'SRV-1', 'office');
            """;
        command.ExecuteNonQuery();
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
