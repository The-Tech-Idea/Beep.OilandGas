using Beep.OilandGas.EconomicAnalysis.Data;
using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Economics;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ECO-01 against a real SQLite database: the scenario list is read from R_ECONOMIC_SCENARIO (SG-09-3), an evaluation
/// saves with its forecast version, fiscal terms and cash-flow rows (AC-6), an approval is recorded and then immutable
/// (AC-6), a missing fiscal term writes nothing (AC-4, SG-09-2), a save whose acknowledgement was lost is retained and
/// retried, and an unavailable history is a state. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class EconomicsGatewayTests
{
    private const string ConnectionName = "eco01-economics";

    [IntegrationFact]
    public async Task An_evaluation_saves_with_its_forecast_fiscal_terms_and_cash_flow()
    {        var root = Path.Combine(OgPaths.DataRoot, "eco01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasEco01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "economics.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "economics.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgEconomicsService(() => source, reporter);

        // ---- SG-09-3: the scenarios come from the reference table ------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Results);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Equal(["BASE", "BEST", "WORST"], loaded.Value.Scenarios);
        Assert.Contains("R_ECONOMIC_SCENARIO", loaded.Value.ReferenceText, StringComparison.Ordinal);
        Assert.Contains("decimal", loaded.Value.ReferenceText, StringComparison.Ordinal);

        // ---- an unconfigured scenario is refused and writes nothing ----------------
        var unknown = await service.EvaluateAsync(Inputs() with { Scenario = "MAGIC" });

        Assert.False(unknown.Success);
        Assert.Equal(EconomicRules.CodeScenarioUnknown, unknown.Reference);
        Assert.Equal(0, CountRows(database, "ECONOMIC_ANALYSIS_RESULT"));

        // ---- AC-4 and SG-09-2: a missing fiscal term is refused by name ------------
        var noTax = await service.EvaluateAsync(Inputs() with { TaxRate = null });

        Assert.False(noTax.Success);
        Assert.Equal(EconomicRules.CodeFiscalTermMissing, noTax.Reference);
        Assert.Contains("tax rate", noTax.Message!, StringComparison.Ordinal);
        Assert.Equal(0, CountRows(database, "ECONOMIC_ANALYSIS_RESULT"));

        // ---- evaluate and save ----------------------------------------------------
        var evaluated = await service.EvaluateAsync(Inputs());

        Assert.True(evaluated.Success, evaluated.Message + " :: " + string.Join(" | ", reporter.Failures));

        var result = evaluated.Value!.Selected!;

        Assert.Equal("BASE", result.Scenario);
        Assert.Equal("FC-2026-01", result.ForecastVersion);
        Assert.Equal(11, result.Periods.Count);
        Assert.True(result.IrrDefined);

        var saved = await service.SaveAsync(result, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Equal(1, CountRows(database, "ECONOMIC_ANALYSIS_RESULT"));
        Assert.Equal(11, CountRows(database, "ECONOMIC_CASH_FLOW"));

        // AC-6: the forecast version, the fiscal terms, the answer and the actor are on the row.
        // One predicate per assertion, so a stored shape that differs says which column it is.
        foreach (var (predicate, label) in new[]
                 {
                     ("SCENARIO = 'BASE'", "scenario"),
                     ("FORECAST_VERSION = 'FC-2026-01'", "forecast version"),
                     ("ENTITY_ID = 'WELL-1'", "entity"),
                     ("CURRENCY = 'USD'", "currency"),
                     ("STATUS = 'DRAFT'", "status"),
                     ("FISCAL_REGIME = 'Concession'", "regime name"),
                     ("ROYALTY_RATE = 0.125", "royalty"),
                     ("TAX_RATE = 0.3", "tax"),
                     ("PERIOD_COUNT = 10", "period count"),
                     ("START_YEAR = 2026", "start year"),
                     ("ANNUAL_VOLUME = 1 AND VOLUME_OUOM = 'bbl'", "volume"),
                     ("UNIT_PRICE = 100 AND PRICE_OUOM = 'USD/bbl'", "price"),
                     ("ANNUAL_CAPEX = 300", "capex"),
                     ("IRR_DEFINED_IND = 'Y'", "irr defined"),
                     ("NPV > 0", "npv"),
                     ("PAYBACK_PERIOD > 0", "payback"),
                     ("PEAK_FUNDING = -300", "peak funding"),
                     ("DRIVERS_TEXT LIKE '%discount rate%'", "drivers"),
                     ("FISCAL_REGIME_TEXT LIKE '%screening%'", "regime sentence"),
                     ("ACTOR = 'sara'", "actor"),
                 })
        {
            Assert.True(
                Scalar(database, $"SELECT COUNT(*) FROM ECONOMIC_ANALYSIS_RESULT WHERE {predicate};") == 1,
                "the stored row does not match on " + label);
        }

        // The capital period is a real row: it carries the outflow at period 0.
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM ECONOMIC_CASH_FLOW WHERE PERIOD = 0 AND AMOUNT = -300;"));

        // ---- the reload keeps the forecast version and the periods -----------------
        var reloaded = await service.LoadAsync();

        Assert.True(reloaded.Success, reloaded.Message);

        var again = reloaded.Value!.Selected!;

        Assert.Equal("BASE", again.Scenario);
        Assert.Equal("FC-2026-01", again.ForecastVersion);
        Assert.Equal("Concession", again.RegimeName);
        Assert.Equal(0.125m, again.RoyaltyRate);
        Assert.Equal(0.30m, again.TaxRate);
        Assert.Equal(11, again.Periods.Count);
        Assert.True(again.IsSaved);
        Assert.False(again.IsApproved);
        Assert.InRange(Math.Abs(result.Npv - again.Npv), 0m, 0.01m);
        Assert.Equal("sara", again.Actor);

        // ---- AC-6: the approval is recorded, and then the row is immutable ---------
        var approved = await service.ApproveAsync(again.Id, "sara");

        Assert.True(approved.Success, approved.Message);
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM ECONOMIC_ANALYSIS_RESULT WHERE STATUS = 'APPROVED' " +
            "AND APPROVED_BY = 'sara' AND APPROVED_AT IS NOT NULL;"));

        var approvedResult = approved.Value!.Selected!;

        Assert.True(approvedResult.IsApproved);
        Assert.Equal("FC-2026-01", approvedResult.ForecastVersion);
        Assert.Contains("APPROVED by sara", approvedResult.StatusText, StringComparison.Ordinal);

        var second = await service.ApproveAsync(again.Id, "sara");

        Assert.False(second.Success);
        Assert.Equal(EconomicRules.CodeAlreadyApproved, second.Reference);

        var resaved = await service.SaveAsync(approvedResult, "sara");

        Assert.False(resaved.Success);
        Assert.Equal(EconomicRules.CodeApprovedImmutable, resaved.Reference);
        Assert.Equal(1, CountRows(database, "ECONOMIC_ANALYSIS_RESULT"));

        // ---- the state table's "lost acknowledgement" -----------------------------
        Drop(database, "ECONOMIC_CASH_FLOW");
        Drop(database, "ECONOMIC_ANALYSIS_RESULT");

        var lost = await service.SaveAsync(result, "sara");

        Assert.False(lost.Success);
        Assert.NotNull(service.Pending);
        Assert.Equal("WELL-1", service.Pending!.Result.EntityId);

        CreateResultTable(database);
        CreateCashFlowTable(database);

        var retried = await service.RetrySaveAsync("sara");

        Assert.True(retried.Success, retried.Message);
        Assert.Null(service.Pending);
        Assert.Equal(1, CountRows(database, "ECONOMIC_ANALYSIS_RESULT"));
        Assert.Equal(11, CountRows(database, "ECONOMIC_CASH_FLOW"));

        // ---- an unavailable history is a state -------------------------------------
        Drop(database, "ECONOMIC_ANALYSIS_RESULT");

        var unavailable = await service.LoadAsync();

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Results);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database)
    {
        CreateScenarioTable(database);
        CreateResultTable(database);
        CreateCashFlowTable(database);
    }

    private static void CreateScenarioTable(string database) =>
        Execute(database,
            """
            CREATE TABLE R_ECONOMIC_SCENARIO (
                ECONOMIC_SCENARIO TEXT NOT NULL PRIMARY KEY,
                ABBREVIATION TEXT NULL,
                ACTIVE_IND TEXT NULL,
                LONG_NAME TEXT NULL,
                SHORT_NAME TEXT NULL);

            INSERT INTO R_ECONOMIC_SCENARIO (ECONOMIC_SCENARIO, ACTIVE_IND, LONG_NAME) VALUES
                ('BASE', 'Y', 'Base Case'),
                ('BEST', 'Y', 'Best Case'),
                ('WORST', 'Y', 'Worst Case'),
                ('STRESS', 'N', 'Stress Case - inactive');
            """);

    private static void CreateResultTable(string database) =>
        Execute(database,
            """
            CREATE TABLE ECONOMIC_ANALYSIS_RESULT (
                ECONOMIC_ANALYSIS_RESULT_ID TEXT NOT NULL PRIMARY KEY,
                ANALYSIS_ID TEXT NULL,
                ANALYSIS_DATE TEXT NULL,
                NPV REAL NULL,
                IRR REAL NULL,
                PAYBACK_PERIOD REAL NULL,
                DISCOUNT_RATE REAL NULL,
                SCENARIO TEXT NULL,
                FORECAST_VERSION TEXT NULL,
                ENTITY_ID TEXT NULL,
                CURRENCY TEXT NULL,
                STATUS TEXT NOT NULL,
                VERSION INTEGER NOT NULL,
                FISCAL_REGIME TEXT NULL,
                ROYALTY_RATE REAL NULL,
                TAX_RATE REAL NULL,
                PERIOD_COUNT INTEGER NULL,
                START_YEAR INTEGER NULL,
                ANNUAL_VOLUME REAL NULL,
                VOLUME_OUOM TEXT NULL,
                UNIT_PRICE REAL NULL,
                PRICE_OUOM TEXT NULL,
                OPEX_PER_UNIT REAL NULL,
                ANNUAL_CAPEX REAL NULL,
                ESCALATION_RATE REAL NULL,
                ABANDONMENT_COST REAL NULL,
                IRR_DEFINED_IND TEXT NOT NULL,
                PEAK_FUNDING REAL NULL,
                UNDISCOUNTED_TOTAL REAL NULL,
                DRIVERS_TEXT TEXT NULL,
                FISCAL_REGIME_TEXT TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                APPROVED_BY TEXT NULL,
                APPROVED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);
            """);

    private static void CreateCashFlowTable(string database) =>
        Execute(database,
            """
            CREATE TABLE ECONOMIC_CASH_FLOW (
                ECONOMIC_CASH_FLOW_ID TEXT NOT NULL PRIMARY KEY,
                ECONOMIC_ANALYSIS_ID TEXT NULL,
                PERIOD INTEGER NULL,
                AMOUNT REAL NULL,
                DESCRIPTION TEXT NULL);
            """);

    private static void Drop(string database, string table) =>
        Execute(database, $"DROP TABLE {table};");

    private static void Execute(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    // ------------------------------------------------------------------ the inputs

    private static OgEconomicInputs Inputs() =>
        new(
            "WELL-1", "BASE", "FC-2026-01", "USD",
            DiscountRate: 0.10m,
            RegimeName: "Concession",
            RoyaltyRate: 0.125m,
            TaxRate: 0.30m,
            PeriodCount: 10,
            StartYear: 2026,
            AnnualVolume: 1m,
            VolumeUnit: "bbl",
            UnitPrice: 100m,
            PriceUnit: "USD/bbl",
            OpexPerUnit: 0m,
            AnnualCapex: 300m,
            EscalationRate: 0m,
            AbandonmentCost: 0m);

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
