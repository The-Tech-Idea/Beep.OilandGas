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
/// WEL-06 against a real SQLite database: three wells compared side by side, a unit mismatch with no delta, a
/// missing value distinct from zero, an overlay refusal naming the well, and the export matching the matrix.
/// </summary>
/// <remarks>Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>): it provisions a local database through BeepDM.</remarks>
public sealed class WellCompareGatewayTests
{
    private const string ConnectionName = "wel06-compare";

    [IntegrationFact]
    public async Task Three_wells_compare_with_units_missing_values_and_an_overlay_refusal()
    {        var root = Path.Combine(OgPaths.DataRoot, "wel06-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWel06"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "compare.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "compare.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var comparison = new OgWellComparison(() => source, reporter);

        var listed = await comparison.ListWellsAsync();
        Assert.True(listed.Success, listed.Message);
        Assert.Equal(3, listed.Value!.Count);

        // One well alone is refused.
        var single = await comparison.CompareAsync(["UWI-A"]);
        Assert.False(single.Success);
        Assert.Equal(OgWellComparison.CodeNeedsTwo, single.Reference);

        var compared = await comparison.CompareAsync(["UWI-A", "UWI-B", "UWI-C"]);
        Assert.True(compared.Success, compared.Message);
        var matrix = compared.Value!;

        Assert.Equal(3, matrix.Wells.Count);

        // AC-1: TD in m vs ft -> unit mismatch, no delta.
        var td = Assert.Single(matrix.Rows, row => row.PropertyKey == "final-td");
        Assert.True(td.UnitMismatch);
        Assert.Empty(td.Deltas);
        Assert.Equal(string.Empty, td.DifferenceText);

        // AC-2: C has no TD (missing "-") but a real zero KB ("0"), and KB deltas are computed.
        Assert.True(td.Values[2].Missing);
        Assert.Equal("-", td.Values[2].Text);

        var kb = Assert.Single(matrix.Rows, row => row.PropertyKey == "kb");
        Assert.False(kb.UnitMismatch);
        Assert.False(kb.Values[2].Missing);
        Assert.Equal(0m, kb.Values[2].Value);
        Assert.Equal(2, kb.Deltas.Count);
        Assert.Contains("UWI-C", kb.Deltas[1], StringComparison.Ordinal);

        // The pick count is part of the matrix.
        var picks = Assert.Single(matrix.Rows, row => row.PropertyKey == "picks");
        Assert.Equal("2", picks.Values[0].Text);
        Assert.Equal("0", picks.Values[2].Text);

        // AC-4: C has no coordinates, so the overlay is refused and names it.
        Assert.False(matrix.CanOverlay);
        Assert.Contains("UWI-C", matrix.OverlayReason!, StringComparison.Ordinal);
        Assert.Contains("surface coordinates", matrix.OverlayReason!, StringComparison.Ordinal);

        // A and B do have geometry, so the overlay is offered for that pair.
        var pair = await comparison.CompareAsync(["UWI-A", "UWI-B"]);
        Assert.True(pair.Value!.CanOverlay);
        Assert.Null(pair.Value.OverlayReason);

        // AC-3: the export matches the matrix.
        var csv = matrix.ToCsv().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(matrix.Rows.Count + 1, csv.Length);
        Assert.StartsWith("Property,Category,Unit,UWI-A,UWI-B,UWI-C,Difference", csv[0], StringComparison.Ordinal);

        var tdLine = csv.First(line => line.StartsWith("Total depth", StringComparison.Ordinal));
        Assert.Contains("10000 ft", tdLine, StringComparison.Ordinal);
        Assert.Contains(",-,", tdLine, StringComparison.Ordinal);

        var kbLine = csv.First(line => line.StartsWith("Kelly bushing", StringComparison.Ordinal));
        Assert.Contains("+10 m", kbLine, StringComparison.Ordinal);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE WELL (
                UWI TEXT NOT NULL PRIMARY KEY,
                WELL_NAME TEXT NULL,
                ASSIGNED_FIELD TEXT NULL,
                OPERATOR TEXT NULL,
                WELL_GOVERNMENT_ID TEXT NULL,
                CURRENT_STATUS TEXT NULL,
                CURRENT_CLASS TEXT NULL,
                FINAL_TD REAL NULL,
                FINAL_TD_OUOM TEXT NULL,
                KB_ELEV REAL NULL,
                GROUND_ELEV REAL NULL,
                GROUND_ELEV_OUOM TEXT NULL,
                SURFACE_LATITUDE REAL NULL,
                SURFACE_LONGITUDE REAL NULL,
                SPUD_DATE TEXT NULL,
                COMPLETION_DATE TEXT NULL,
                ABANDONMENT_DATE TEXT NULL);

            CREATE TABLE STRAT_WELL_SECTION (
                UWI TEXT NOT NULL,
                STRAT_NAME_SET_ID TEXT NOT NULL,
                STRAT_UNIT_ID TEXT NOT NULL,
                INTERP_ID TEXT NOT NULL,
                PICK_DEPTH REAL NULL,
                ACTIVE_IND TEXT NULL,
                PRIMARY KEY (UWI, STRAT_NAME_SET_ID, STRAT_UNIT_ID, INTERP_ID));

            INSERT INTO WELL
                (UWI, WELL_NAME, ASSIGNED_FIELD, OPERATOR, WELL_GOVERNMENT_ID, CURRENT_STATUS, CURRENT_CLASS,
                 FINAL_TD, FINAL_TD_OUOM, KB_ELEV, GROUND_ELEV, GROUND_ELEV_OUOM,
                 SURFACE_LATITUDE, SURFACE_LONGITUDE, SPUD_DATE)
            VALUES
                ('UWI-A', 'ALPHA', 'NORTH', 'BA-1', 'GOV-A', 'ACTIVE', 'DEVELOPMENT', 3000, 'm', 110, 100, 'm', 26.1, 50.2, '2020-01-01'),
                ('UWI-B', 'BRAVO', 'NORTH', 'BA-1', 'GOV-B', 'ACTIVE', 'DEVELOPMENT', 10000, 'ft', 120, 105, 'm', 26.2, 50.3, '2021-01-01'),
                ('UWI-C', 'CHARLIE', 'SOUTH', 'BA-2', NULL, 'SUSPENDED', 'EXPLORATION', NULL, NULL, 0, 50, 'm', NULL, NULL, '2022-01-01');

            INSERT INTO STRAT_WELL_SECTION (UWI, STRAT_NAME_SET_ID, STRAT_UNIT_ID, INTERP_ID, PICK_DEPTH, ACTIVE_IND)
            VALUES
                ('UWI-A', 'N1', 'F1', 'I1', 1000, 'Y'),
                ('UWI-A', 'N1', 'F2', 'I2', 1200, 'Y'),
                ('UWI-B', 'N1', 'F1', 'I3', 1500, 'Y');
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
