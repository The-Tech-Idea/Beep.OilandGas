using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Development;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEV-04 against a real SQLite database: the capacity check refuses mixed units and reports the shortfall as a
/// number (AC-1, AC-2); a planned facility is distinguishable from an operating one through the PPDM row (AC-3);
/// each pool and facility row carries the plan and plan version it belongs to (AC-4); and the saves refuse a volume
/// without a unit or a pool routed to a facility outside the plan. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class PoolsFacilitiesGatewayTests
{
    private const string ConnectionName = "dev04-pools";

    [IntegrationFact]
    public async Task The_plan_shows_capacity_shortfall_states_and_versioned_pool_and_facility_rows()
    {        var root = Path.Combine(OgPaths.DataRoot, "dev04-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDev04"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "pools.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "pools.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgPoolsFacilitiesService(() => source, reporter);
        var asOf = new DateTime(2026, 6, 1);

        var loaded = await service.LoadAsync("F-1", "Alpha field", null, asOf);

        Assert.True(loaded.Success, loaded.Message);
        var plan = loaded.Value!;

        // The newest plan version of the field is the one shown, and the other field's plan is absent.
        Assert.True(plan.HasPlan);
        Assert.Equal("FDP-1", plan.PlanId);
        Assert.Equal(3, plan.PlanVersion);
        Assert.Equal(5, plan.Pools.Count);
        Assert.Equal(4, plan.Facilities.Count);

        // AC-4: every pool and facility row carries the plan and the plan version.
        Assert.All(plan.Pools, pool => Assert.Equal(3, pool.PlanVersion));
        Assert.All(plan.Pools, pool => Assert.Equal("FDP-1", pool.PlanId));
        Assert.All(plan.Facilities, facility => Assert.Equal(3, facility.PlanVersion));

        // AC-3: operating, in construction, planned and abandoned are distinguishable.
        Assert.Equal(OgFacilityPlan.Operating, plan.Facilities.Single(f => f.FacilityId == "F-1").State);
        Assert.Equal(OgFacilityPlan.InConstruction, plan.Facilities.Single(f => f.FacilityId == "F-2").State);
        Assert.Equal(OgFacilityPlan.Abandoned, plan.Facilities.Single(f => f.FacilityId == "F-4").State);

        Assert.Equal(1, plan.OperatingFacilities);
        Assert.True(plan.Facilities.Single(f => f.FacilityId == "F-2").ExcludedFromOperationalPages);
        Assert.False(plan.Facilities.Single(f => f.FacilityId == "F-1").ExcludedFromOperationalPages);

        // AC-1/AC-2: oil is short by its own number; the bbl/d facility is named, not converted.
        var oil = plan.Capacity.Lines.Single(line => line.Fluid == "Oil");

        Assert.Equal(1300m, oil.PlannedRate);
        Assert.Equal(1000m, oil.Capacity);
        Assert.Equal(300m, oil.Shortfall);
        Assert.Contains(oil.NotComparable, item => item.Contains("bbl/d", StringComparison.Ordinal));
        Assert.True(plan.Capacity.HasShortfall);

        var gas = plan.Capacity.Lines.Single(line => line.Fluid == "Gas");

        Assert.Equal(25m, gas.PlannedRate);
        Assert.Equal(40m, gas.Capacity);
        Assert.Null(gas.Shortfall);

        // The allocation preview totals only the pools that share a unit, and names the unrouted one.
        var central = plan.Allocation.Facilities.Single(line => line.FacilityId == "F-1");

        Assert.Equal(45m, central.Total);
        Assert.Equal("MMbbl", central.Unit);

        var gasPlant = plan.Allocation.Facilities.Single(line => line.FacilityId == "F-2");

        Assert.Null(gasPlant.Total);
        Assert.Equal(new[] { "Bcf", "MMstb" }, gasPlant.UnitMismatch);
        Assert.Equal("Nowhere", Assert.Single(plan.Allocation.UnroutedPools));

        // A pool whose OOIP has no unit is refused, so the total is never built from a bare number.
        var noUnit = await service.SavePoolAsync(
            "FDP-1",
            new OgPoolValues("No unit", null, "Sand A", "Water drive", 10m, null, 0.3m, 2, "F-1"),
            "sara",
            asOf);

        Assert.False(noUnit.Success);
        Assert.Equal(OgPoolsFacilitiesService.CodeVolumeUnitRequired, noUnit.Reference);

        // A pool routed to a facility outside the plan is refused.
        var badRoute = await service.SavePoolAsync(
            "FDP-1",
            new OgPoolValues("Elsewhere", null, "Sand A", "Water drive", 10m, "MMbbl", 0.3m, 2, "F-OTHER"),
            "sara",
            asOf);

        Assert.False(badRoute.Success);
        Assert.Equal(OgPoolsFacilitiesService.CodeFacilityUnknown, badRoute.Reference);

        // A new pool with a unit is saved against the plan version.
        var savedPool = await service.SavePoolAsync(
            "FDP-1",
            new OgPoolValues("Deeper sand", "P-9", "Sand B", "Gas cap", 40m, "MMbbl", 0.25m, 2, "F-1"),
            "sara",
            asOf);

        Assert.True(savedPool.Success, savedPool.Message);
        var deeper = savedPool.Value!.Pools.Single(pool => pool.PoolId == "P-9");

        Assert.Equal(3, deeper.PlanVersion);
        Assert.Equal(10m, deeper.Recoverable);
        Assert.Equal(55m, savedPool.Value.Allocation.Facilities.Single(line => line.FacilityId == "F-1").Total);

        // A new facility needs its capacity unit and currency, then is saved as planned.
        var noUnitFacility = await service.SaveFacilityAsync(
            "FDP-1",
            new OgFacilityValues("F-9", "New plant", "CPF", "PLANNED", 500m, null, 60m, "USD", null, null),
            "sara",
            asOf);

        Assert.False(noUnitFacility.Success);
        Assert.Equal(OgPoolsFacilitiesService.CodeCapacityUnitRequired, noUnitFacility.Reference);

        var savedFacility = await service.SaveFacilityAsync(
            "FDP-1",
            new OgFacilityValues(
                "F-9",
                "New plant",
                "CPF",
                "PLANNED",
                500m,
                "BOPD",
                60m,
                "USD",
                new DateTime(2026, 9, 1),
                new DateTime(2027, 3, 1)),
            "sara",
            asOf);

        Assert.True(savedFacility.Success, savedFacility.Message);
        var newPlant = savedFacility.Value!.Facilities.Single(facility => facility.FacilityId == "F-9");

        Assert.Equal(3, newPlant.PlanVersion);
        Assert.Equal(OgFacilityPlan.Planned, newPlant.State);
        Assert.True(newPlant.ExcludedFromOperationalPages);

        // The added capacity removes the oil shortfall: 1000 + 500 against 1300 planned.
        var oilAfter = savedFacility.Value.Capacity.Lines.Single(line => line.Fluid == "Oil");

        Assert.Equal(1500m, oilAfter.Capacity);
        Assert.Null(oilAfter.Shortfall);
        Assert.Equal(200m, oilAfter.Headroom);

        // A failed plan read is a failure, not an empty plan.
        Drop(database, "FIELD_DEVELOPMENT_PLAN");
        var failed = await service.LoadAsync("F-1", "Alpha field", null, asOf);

        Assert.False(failed.Success);
        Assert.NotNull(failed.Reference);
        Assert.Null(failed.Value);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE FIELD_DEVELOPMENT_PLAN (
                FDP_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_ID TEXT NULL,
                FDP_NAME TEXT NULL,
                FDP_VERSION INTEGER NOT NULL,
                SUBMISSION_DATE TEXT NULL);

            CREATE TABLE DEVELOPMENT_POOL_PLAN (
                POOL_PLAN_ID TEXT NOT NULL PRIMARY KEY,
                FDP_ID TEXT NOT NULL,
                PLAN_VERSION INTEGER NOT NULL,
                FIELD_ID TEXT NOT NULL,
                POOL_ID TEXT NULL,
                POOL_NAME TEXT NOT NULL,
                RESERVOIR TEXT NULL,
                DRIVE_MECHANISM TEXT NULL,
                OOIP REAL NOT NULL,
                OOIP_OUOM TEXT NULL,
                RECOVERY_FACTOR REAL NOT NULL,
                PLANNED_WELLS INTEGER NOT NULL,
                FACILITY_ID TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE FACILITY_INVESTMENT (
                FACILITY_INV_ID TEXT NOT NULL PRIMARY KEY,
                FDP_ID TEXT NOT NULL,
                FIELD_ID TEXT NULL,
                FACILITY_ID TEXT NULL,
                FACILITY_NAME TEXT NULL,
                FACILITY_TYPE TEXT NULL,
                INVESTMENT_PHASE TEXT NULL,
                CAPACITY_PLANNED_BOPD REAL NULL,
                CAPACITY_OUOM TEXT NULL,
                CAPEX_PLANNED_MM REAL NULL,
                COST_CURRENCY TEXT NULL,
                PLANNED_START_DATE TEXT NULL,
                PLANNED_COMPLETION_DATE TEXT NULL,
                PLAN_VERSION INTEGER NOT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE DEVELOPMENT_WELL_SCHEDULE (
                SCHEDULE_ID TEXT NOT NULL PRIMARY KEY,
                FDP_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                PLANNED_WELL_ID TEXT NULL,
                WELL_NAME TEXT NULL,
                INITIAL_RATE_OIL_BOPD REAL NULL,
                INITIAL_RATE_GAS_MMSCFD REAL NULL,
                SCHEDULE_STATUS TEXT NULL);

            CREATE TABLE FACILITY (
                FACILITY_ID TEXT NOT NULL PRIMARY KEY,
                ACTIVE_IND TEXT NULL,
                ACTIVE_DATE TEXT NULL,
                INACTIVE_DATE TEXT NULL,
                ABANDONED_DATE TEXT NULL,
                FACILITY_TYPE TEXT NULL);

            INSERT INTO FIELD_DEVELOPMENT_PLAN (FDP_ID, FIELD_ID, FDP_NAME, FDP_VERSION, SUBMISSION_DATE) VALUES
                ('FDP-1', 'F-1', 'Alpha FDP', 3, '2026-02-01'),
                ('FDP-2', 'F-2', 'Other field FDP', 1, '2026-03-01');

            INSERT INTO DEVELOPMENT_WELL_SCHEDULE
                (SCHEDULE_ID, FDP_ID, FIELD_ID, PLANNED_WELL_ID, WELL_NAME, INITIAL_RATE_OIL_BOPD,
                 INITIAL_RATE_GAS_MMSCFD, SCHEDULE_STATUS) VALUES
                ('S-1', 'FDP-1', 'F-1', 'WELL-1', 'WELL-1', 600, NULL, 'PLANNED'),
                ('S-2', 'FDP-1', 'F-1', 'WELL-2', 'WELL-2', 700, NULL, 'PLANNED'),
                ('S-3', 'FDP-1', 'F-1', 'WELL-3', 'WELL-3', NULL, 25, 'PLANNED'),
                ('S-4', 'FDP-1', 'F-1', 'WELL-4', 'WELL-4', NULL, NULL, 'PLANNED'),
                ('S-9', 'FDP-2', 'F-2', 'WELL-9', 'WELL-9', 9000, NULL, 'PLANNED');

            INSERT INTO FACILITY_INVESTMENT
                (FACILITY_INV_ID, FDP_ID, FIELD_ID, FACILITY_ID, FACILITY_NAME, FACILITY_TYPE, INVESTMENT_PHASE,
                 CAPACITY_PLANNED_BOPD, CAPACITY_OUOM, CAPEX_PLANNED_MM, COST_CURRENCY, PLANNED_START_DATE,
                 PLANNED_COMPLETION_DATE, PLAN_VERSION) VALUES
                ('FI-1', 'FDP-1', 'F-1', 'F-1', 'Central', 'CPF', 'CONSTRUCTION', 1000, 'BOPD', 80, 'USD',
                 '2025-01-01', '2026-12-01', 3),
                ('FI-2', 'FDP-1', 'F-1', 'F-2', 'Gas plant', 'GAS', 'CONSTRUCTION', 40, 'MMSCFD', 30, 'USD',
                 '2026-01-01', '2026-12-01', 3),
                ('FI-3', 'FDP-1', 'F-1', 'F-3', 'Water plant', 'WATER', 'PLANNED', 5000, 'bbl/d', 5, 'USD',
                 '2026-07-01', '2027-01-01', 3),
                ('FI-4', 'FDP-1', 'F-1', 'F-4', 'Old battery', 'BATTERY', 'ABANDONED', 200, 'BOPD', 2, 'USD',
                 '2010-01-01', '2012-01-01', 3),
                ('FI-9', 'FDP-2', 'F-2', 'F-9', 'Other field plant', 'CPF', 'PLANNED', 9000, 'BOPD', 90, 'USD',
                 '2027-01-01', '2028-01-01', 1);

            INSERT INTO FACILITY (FACILITY_ID, ACTIVE_IND, ACTIVE_DATE, INACTIVE_DATE, ABANDONED_DATE, FACILITY_TYPE) VALUES
                ('F-1', 'Y', '2025-06-01', NULL, NULL, 'CPF'),
                ('F-4', NULL, NULL, NULL, '2020-01-01', 'BATTERY');

            INSERT INTO DEVELOPMENT_POOL_PLAN
                (POOL_PLAN_ID, FDP_ID, PLAN_VERSION, FIELD_ID, POOL_ID, POOL_NAME, RESERVOIR, DRIVE_MECHANISM,
                 OOIP, OOIP_OUOM, RECOVERY_FACTOR, PLANNED_WELLS, FACILITY_ID) VALUES
                ('PP-1', 'FDP-1', 3, 'F-1', 'P-1', 'Upper', 'Sand A', 'Water drive', 100, 'MMbbl', 0.35, 4, 'F-1'),
                ('PP-2', 'FDP-1', 3, 'F-1', 'P-2', 'Lower', 'Sand B', 'Solution gas', 50, 'MMbbl', 0.20, 3, 'F-1'),
                ('PP-3', 'FDP-1', 3, 'F-1', 'P-3', 'Gas cap', 'Sand A', 'Gas cap', 2, 'Bcf', 0.50, 1, 'F-2'),
                ('PP-4', 'FDP-1', 3, 'F-1', 'P-4', 'Mixed', 'Sand C', 'Water drive', 30, 'MMstb', 0.10, 1, 'F-2'),
                ('PP-5', 'FDP-1', 3, 'F-1', 'P-5', 'Nowhere', 'Sand D', 'Water drive', 10, 'MMbbl', 0.10, 1, NULL),
                ('PP-9', 'FDP-2', 1, 'F-2', 'P-9', 'Other field pool', 'Sand Z', 'Water drive', 500, 'MMbbl', 0.1, 9, 'F-9');
            """;
        command.ExecuteNonQuery();
    }

    private static void Drop(string database, string table)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"DROP TABLE {table};";
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
