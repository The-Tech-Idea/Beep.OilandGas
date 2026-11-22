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
/// WEL-05 against a real SQLite database: three levels of units in a stable order, picks scoped to the selected
/// well, a pick's unit taken from the reference set, overlap flagging, and a rename that changes display only.
/// </summary>
/// <remarks>Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>): it provisions a local database through BeepDM.</remarks>
public sealed class StratigraphyGatewayTests
{
    private const string ConnectionName = "wel05-stratigraphy";

    [IntegrationFact]
    public async Task Units_are_three_levels_and_picks_are_scoped_with_a_reference_unit()
    {        var root = Path.Combine(OgPaths.DataRoot, "wel05-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWel05"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "strat.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "strat.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var stratigraphy = new OgStratigraphy(() => source, reporter);

        // AC-1: three levels, ordered by age code.
        var units = await stratigraphy.LoadUnitsAsync();
        Assert.True(units.Success, units.Message);
        Assert.Equal(5, units.Value!.Count);

        var hierarchy = OgStratigraphy.BuildHierarchy(units.Value);
        var group = Assert.Single(hierarchy);
        Assert.Equal("G1", group.Unit.Id);
        Assert.Equal(["F1", "F2"], group.Children.Select(node => node.Unit.Id));
        Assert.Equal(["M1", "M2"], Assert.Single(group.Children, node => node.Unit.Id == "F1").Children.Select(node => node.Unit.Id));

        // AC-6 + AC-3: only the selected well's picks, with the duplicate depth flagged.
        var picksA = await stratigraphy.LoadPicksAsync("UWI-A");
        var picksB = await stratigraphy.LoadPicksAsync("UWI-B");

        Assert.True(picksA.Success, picksA.Message);
        Assert.Equal(3, picksA.Value!.Count);
        Assert.All(picksA.Value, pick => Assert.Equal("UWI-A", pick.WellId));
        Assert.Single(picksB.Value!);

        Assert.Equal(2, picksA.Value.Count(pick => pick.Overlapping));
        var f1 = Assert.Single(picksA.Value, pick => pick.UnitId == "F1");
        Assert.Equal("Formation One", f1.UnitName);
        Assert.Equal(1000m, f1.TopMd);

        // AC-2: the unit must exist in the reference set.
        var freeText = await stratigraphy.AddPickAsync("UWI-A", "N1", "FREE TEXT", 1500m, "m", "fahad");

        Assert.False(freeText.Success);
        Assert.Equal(OgStratigraphy.CodeUnitUnknown, freeText.Reference);
        Assert.Contains("F1", freeText.Message!, StringComparison.Ordinal);

        // AC-2: a real unit adds a pick for this well only.
        var added = await stratigraphy.AddPickAsync("UWI-A", "N1", "M1", 1500m, "m", "fahad");

        Assert.True(added.Success, added.Message);
        Assert.Equal(4, (await stratigraphy.LoadPicksAsync("UWI-A")).Value!.Count);
        Assert.Single((await stratigraphy.LoadPicksAsync("UWI-B")).Value!);

        // AC-4: renaming changes the unit row, not the picks.
        var beforeRename = (await stratigraphy.LoadPicksAsync("UWI-A")).Value!
            .Where(pick => pick.UnitId == "F1")
            .ToList();

        var renamed = await stratigraphy.RenameUnitAsync("N1", "F1", "Formation One (renamed)", "fahad");

        Assert.True(renamed.Success, renamed.Message);
        Assert.Contains("2 pick(s)", renamed.Message!, StringComparison.OrdinalIgnoreCase);

        var afterRename = (await stratigraphy.LoadPicksAsync("UWI-A")).Value!
            .Where(pick => pick.UnitId == "F1")
            .ToList();

        Assert.Equal(beforeRename.Count, afterRename.Count);
        Assert.All(afterRename, pick => Assert.Equal("Formation One (renamed)", pick.UnitName));
        Assert.All(afterRename, pick => Assert.Equal("F1", pick.UnitId));

        // The columns list is read from STRAT_COLUMN.
        var columns = await stratigraphy.LoadColumnsAsync();
        Assert.True(columns.Success);
        Assert.Single(columns.Value!);
        Assert.Equal("Regional column", columns.Value![0].Name);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE STRAT_UNIT (
                STRAT_NAME_SET_ID TEXT NOT NULL,
                STRAT_UNIT_ID TEXT NOT NULL,
                LONG_NAME TEXT NULL,
                SHORT_NAME TEXT NULL,
                GROUP_CODE TEXT NULL,
                STRAT_UNIT_TYPE TEXT NULL,
                STRAT_TYPE TEXT NULL,
                ORDINAL_AGE_CODE REAL NULL,
                ACTIVE_IND TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                PRIMARY KEY (STRAT_NAME_SET_ID, STRAT_UNIT_ID));

            CREATE TABLE STRAT_COLUMN (
                STRAT_COLUMN_ID TEXT NOT NULL,
                SOURCE TEXT NOT NULL,
                STRAT_COLUMN_NAME TEXT NULL,
                STRAT_COLUMN_TYPE TEXT NULL,
                ACTIVE_IND TEXT NULL,
                PRIMARY KEY (STRAT_COLUMN_ID, SOURCE));

            CREATE TABLE STRAT_WELL_SECTION (
                UWI TEXT NOT NULL,
                STRAT_NAME_SET_ID TEXT NOT NULL,
                STRAT_UNIT_ID TEXT NOT NULL,
                INTERP_ID TEXT NOT NULL,
                PICK_DEPTH REAL NULL,
                PICK_DEPTH_OUOM TEXT NULL,
                ORDINAL_SEQ_NO REAL NULL,
                ACTIVE_IND TEXT NULL,
                PREFERRED_PICK_IND TEXT NULL,
                SOURCE TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                PRIMARY KEY (UWI, STRAT_NAME_SET_ID, STRAT_UNIT_ID, INTERP_ID));

            INSERT INTO STRAT_UNIT (STRAT_NAME_SET_ID, STRAT_UNIT_ID, LONG_NAME, SHORT_NAME, GROUP_CODE, STRAT_UNIT_TYPE, ORDINAL_AGE_CODE, ACTIVE_IND)
            VALUES
                ('N1', 'G1', 'Group One', 'G1', NULL, 'GROUP', 10, 'Y'),
                ('N1', 'F1', 'Formation One', 'F1', 'G1', 'FORMATION', 20, 'Y'),
                ('N1', 'F2', 'Formation Two', 'F2', 'G1', 'FORMATION', 30, 'Y'),
                ('N1', 'M1', 'Member One', 'M1', 'F1', 'MEMBER', 21, 'Y'),
                ('N1', 'M2', 'Member Two', 'M2', 'F1', 'MEMBER', 22, 'Y');

            INSERT INTO STRAT_COLUMN (STRAT_COLUMN_ID, SOURCE, STRAT_COLUMN_NAME, STRAT_COLUMN_TYPE, ACTIVE_IND)
            VALUES ('C1', 'office', 'Regional column', 'REGIONAL', 'Y');

            INSERT INTO STRAT_WELL_SECTION (UWI, STRAT_NAME_SET_ID, STRAT_UNIT_ID, INTERP_ID, PICK_DEPTH, PICK_DEPTH_OUOM, ORDINAL_SEQ_NO, ACTIVE_IND)
            VALUES
                ('UWI-A', 'N1', 'F1', 'I1', 1000, 'm', 1, 'Y'),
                ('UWI-A', 'N1', 'F2', 'I2', 1000, 'm', 2, 'Y'),
                ('UWI-A', 'N1', 'M1', 'I3', 1200, 'm', 3, 'Y'),
                ('UWI-B', 'N1', 'F1', 'I4', 500, 'm', 1, 'Y');
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
