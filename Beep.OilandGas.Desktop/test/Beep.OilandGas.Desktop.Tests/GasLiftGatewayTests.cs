using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LFT-02 against a real SQLite database: the port sizes come from the reference set (AC-4), a design records its method,
/// its valves, its units and every evaluated limit (AC-5, AC-6), a blocking limit refuses the save with its catalog
/// message (AC-2) while a screening warning saves and is flagged, and a save whose acknowledgement was lost is retained
/// and retried. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class GasLiftGatewayTests
{
    private const string ConnectionName = "lft02-gaslift";

    [IntegrationFact]
    public async Task A_design_saves_with_its_method_valves_and_every_limit()
    {        var root = Path.Combine(OgPaths.DataRoot, "lft02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasLft02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "gaslift.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "gaslift.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgGasLiftService(() => source, reporter);

        // ---- AC-4: the port sizes come from the reference table --------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Designs);
        Assert.True(loaded.Value.PortSizesFromReference);
        Assert.Equal([0.25m, 0.375m, 0.5m, 0.75m], loaded.Value.PortSizes);
        Assert.Contains("R_GAS_LIFT_REFERENCE_CODE", loaded.Value.PortSizesText, StringComparison.Ordinal);
        Assert.False(loaded.Value.HistoryFailed);

        // ---- design and save -------------------------------------------------
        var designed = await service.DesignAsync(Inputs());

        Assert.True(designed.Success, designed.Message + " :: " + string.Join(" | ", reporter.Failures));

        var design = designed.Value!.Selected!;

        Assert.Equal(5, design.ValveCount);
        Assert.Equal(5, design.Valves.Count);
        Assert.Empty(design.Limits);

        var saved = await service.SaveAsync(design, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Equal(1, CountRows(database, "GAS_LIFT_DESIGN"));

        // AC-5 and AC-6: the method, the units, the valve list and every evaluated code.
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM GAS_LIFT_DESIGN WHERE SPACING_METHOD = 'UNIFORM_SPACING' " +
            "AND PORT_SIZE_IN = 0.5 AND INJECTION_UNIT = 'Mscf/d' AND PRODUCTION_UNIT = 'STB/d' " +
            "AND BLOCKING_IND = 'N' AND ACTOR = 'sara' AND LIMIT_CODES LIKE '%MIN_VALVE_SPACING_FT%' " +
            "AND VALVE_DEPTHS LIKE '%8000%';"));

        // ---- the reload keeps the valves -------------------------------------
        var reloaded = await service.LoadAsync("WELL-1");

        Assert.True(reloaded.Success, reloaded.Message);

        var again = reloaded.Value!.Selected!;

        Assert.Equal(5, again.ValveCount);
        Assert.Equal(design.Valves.Select(valve => valve.DepthFt), again.Valves.Select(valve => valve.DepthFt));
        Assert.Equal(design.EvaluatedCodes.Count, again.EvaluatedCodes.Count);
        Assert.Equal("sara", again.Actor);

        // ---- AC-2: a blocking limit refuses the save -------------------------
        var blocked = (await service.DesignAsync(Inputs() with { Valves = 25 })).Value!.Selected!;

        Assert.True(blocked.HasBlockingLimit);
        Assert.Contains(blocked.Limits, limit => limit.Code == "MAX_VALVES" && limit.Blocking);

        var refused = await service.SaveAsync(blocked, "sara");

        Assert.False(refused.Success);
        Assert.Equal(GasLiftRules.CodeBlockingLimit, refused.Reference);
        Assert.Contains("Maximum number of gas lift valves", refused.Message!, StringComparison.Ordinal);
        Assert.Equal(1, CountRows(database, "GAS_LIFT_DESIGN"));

        // ---- a screening warning saves and is flagged ------------------------
        var flagged = (await service.DesignAsync(Inputs() with { WellDepthFt = 300m })).Value!.Selected!;

        Assert.False(flagged.HasBlockingLimit);
        Assert.Contains(flagged.Limits, limit => limit.Code == "MIN_VALVE_SPACING_FT");

        var flaggedSaved = await service.SaveAsync(flagged, "sara");

        Assert.True(flaggedSaved.Success, flaggedSaved.Message);
        Assert.Equal(2, CountRows(database, "GAS_LIFT_DESIGN"));
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM GAS_LIFT_DESIGN WHERE LIMIT_MESSAGES LIKE '%Minimum valve spacing%';"));

        // ---- the state table's "lost acknowledgement" ------------------------
        Drop(database);

        var lost = await service.SaveAsync(design, "sara");

        Assert.False(lost.Success);
        Assert.NotNull(service.Pending);
        Assert.Equal("WELL-1", service.Pending!.Design.WellId);

        CreateTable(database);

        var retried = await service.RetrySaveAsync("sara");

        Assert.True(retried.Success, retried.Message);
        Assert.Null(service.Pending);
        Assert.Equal(1, CountRows(database, "GAS_LIFT_DESIGN"));

        // ---- an unavailable history is a state -------------------------------
        Drop(database);

        var unavailable = await service.LoadAsync();

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Designs);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE R_GAS_LIFT_REFERENCE_CODE (
                REFERENCE_SET TEXT NOT NULL,
                REFERENCE_CODE TEXT NOT NULL,
                LONG_NAME TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                PRIMARY KEY (REFERENCE_SET, REFERENCE_CODE));

            INSERT INTO R_GAS_LIFT_REFERENCE_CODE (REFERENCE_SET, REFERENCE_CODE, LONG_NAME) VALUES
                ('GAS_LIFT_PORT_SIZE_IN', '0.25', 'Port diameter 0.25 in'),
                ('GAS_LIFT_PORT_SIZE_IN', '0.375', 'Port diameter 0.375 in'),
                ('GAS_LIFT_PORT_SIZE_IN', '0.5', 'Port diameter 0.5 in'),
                ('GAS_LIFT_PORT_SIZE_IN', '0.75', 'Port diameter 0.75 in'),
                ('GAS_LIFT_DESIGN_LIMIT', 'MIN_INJECTION_MSCFD', 'Minimum injection rate (Mscf/d)'),
                ('GAS_LIFT_DESIGN_LIMIT', 'MAX_VALVES', 'Maximum number of valves');
            """;
        command.ExecuteNonQuery();

        CreateTable(database);
    }

    private static void CreateTable(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE GAS_LIFT_DESIGN (
                DESIGN_ID TEXT NOT NULL PRIMARY KEY,
                WELL_UWI TEXT NULL,
                DESIGN_DATE TEXT NULL,
                NUMBER_OF_VALVES INTEGER NULL,
                TOTAL_GAS_INJECTION_RATE REAL NULL,
                EXPECTED_PRODUCTION_RATE REAL NULL,
                GAS_INJECTION_PRESSURE REAL NOT NULL,
                SYSTEM_EFFICIENCY REAL NOT NULL,
                STATUS TEXT NULL,
                SPACING_METHOD TEXT NULL,
                PORT_SIZE_IN REAL NULL,
                INJECTION_UNIT TEXT NULL,
                PRODUCTION_UNIT TEXT NULL,
                VALVE_DEPTHS TEXT NULL,
                VALVE_PRESSURES TEXT NULL,
                LIMIT_CODES TEXT NULL,
                LIMIT_MESSAGES TEXT NULL,
                BLOCKING_IND TEXT NOT NULL,
                RECOMMENDED_INJECTION_RATE REAL NULL,
                INPUTS_JSON TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);
            """;
        command.ExecuteNonQuery();
    }

    private static OgGasLiftInputs Inputs() =>
        new(
            "WELL-1", 8000m, 150m, 2500m, 3000m, 500m, 500m, 35m, 0.2m, 100m, 200m, 0.65m,
            1200m, 500m, 5, 0.5m, GasLiftSpacingMethods.EqualDepth);

    private static long Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static long CountRows(string database, string table) =>
        Scalar(database, $"SELECT COUNT(*) FROM {table};");

    private static void Drop(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TABLE GAS_LIFT_DESIGN;";
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
