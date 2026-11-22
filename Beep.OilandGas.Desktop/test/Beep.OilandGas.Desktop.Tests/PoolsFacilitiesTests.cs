using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Development;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEV-04: the unit rule, the shortfall arithmetic, the facility state, the allocation preview and the value rules -
/// the parts that do not need a database. The plan versioning, the saves and the field scope are proven in the
/// gateway test.
/// </summary>
public sealed class PoolsFacilitiesTests
{
    private static readonly DateTime AsOf = new(2026, 6, 1);

    [Fact]
    public void A_rate_is_compared_only_with_a_capacity_in_the_same_unit()
    {
        var schedules = new[]
        {
            Schedule("S-1", oil: 600m, gas: null),
            Schedule("S-2", oil: 700m, gas: null),
            Schedule("S-3", oil: null, gas: 25m),
            Schedule("S-4", oil: null, gas: null),
        };

        var facilities = new[]
        {
            Facility("F-1", "Central", capacity: 1000m, unit: "BOPD", state: OgFacilityPlan.Operating),
            Facility("F-2", "Gas plant", capacity: 40m, unit: "MMSCFD", state: OgFacilityPlan.InConstruction),
            Facility("F-3", "Water plant", capacity: 5000m, unit: "bbl/d", state: OgFacilityPlan.Operating),
            Facility("F-4", "Old battery", capacity: 200m, unit: "BOPD", state: OgFacilityPlan.Abandoned),
        };

        var check = OgPoolsFacilitiesService.CheckCapacity(schedules, facilities);

        Assert.Equal(2, check.Lines.Count);

        var oil = check.Lines.Single(line => line.Fluid == "Oil");

        Assert.Equal(1300m, oil.PlannedRate);
        Assert.Equal(2, oil.WellsWithRate);
        Assert.Equal(2, oil.WellsWithoutRate);
        Assert.Equal(1000m, oil.Capacity);
        Assert.Equal(-300m, oil.Headroom);
        Assert.Equal(300m, oil.Shortfall);

        // A facility whose unit is not BOPD is named, not converted, and the abandoned one is excluded entirely.
        Assert.Contains(oil.NotComparable, item => item.Contains("Gas plant", StringComparison.Ordinal));
        Assert.Contains(oil.NotComparable, item => item.Contains("bbl/d", StringComparison.Ordinal));
        Assert.DoesNotContain(oil.NotComparable, item => item.Contains("Old battery", StringComparison.Ordinal));

        var gas = check.Lines.Single(line => line.Fluid == "Gas");

        Assert.Equal(25m, gas.PlannedRate);
        Assert.Equal(40m, gas.Capacity);
        Assert.Equal(15m, gas.Headroom);
        Assert.Null(gas.Shortfall);
        Assert.Contains("headroom 15 MMSCFD", gas.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void A_shortfall_is_its_own_number_and_never_a_clamp()
    {
        var schedules = new[] { Schedule("S-1", oil: 1500m, gas: null) };
        var facilities = new[] { Facility("F-1", "Central", capacity: 1000m, unit: "BOPD", state: OgFacilityPlan.Operating) };

        var line = Assert.Single(OgPoolsFacilitiesService.CheckCapacity(schedules, facilities).Lines);

        Assert.Equal(500m, line.Shortfall); // 1500 planned - 1000 capacity, in BOPD
        Assert.Equal(-500m, line.Headroom);
        Assert.Contains("shortfall 500 BOPD", line.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void No_capacity_in_the_rate_unit_is_stated_rather_than_treated_as_zero()
    {
        var schedules = new[] { Schedule("S-1", oil: 500m, gas: null) };
        var facilities = new[] { Facility("F-1", "Gas plant", capacity: 40m, unit: "MMSCFD", state: OgFacilityPlan.Operating) };

        var line = Assert.Single(OgPoolsFacilitiesService.CheckCapacity(schedules, facilities).Lines);

        Assert.False(line.HasCapacity);
        Assert.Null(line.Headroom);
        Assert.Null(line.Shortfall);
        Assert.Contains("no capacity is recorded in BOPD", line.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void No_planned_rate_is_no_capacity_line_at_all()
    {
        var check = OgPoolsFacilitiesService.CheckCapacity(
            [Schedule("S-1", oil: null, gas: null)],
            [Facility("F-1", "Central", 1000m, "BOPD", OgFacilityPlan.Operating)]);

        Assert.Empty(check.Lines);
        Assert.Contains("nothing to compare", check.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void The_allocation_sums_only_pools_that_share_a_unit()
    {
        var facilities = new[]
        {
            Facility("F-1", "Central", 1000m, "BOPD", OgFacilityPlan.Operating),
            Facility("F-2", "Gas plant", 40m, "MMSCFD", OgFacilityPlan.InConstruction),
        };

        var pools = new[]
        {
            Pool("P-1", "Upper", ooip: 100m, unit: "MMbbl", factor: 0.35m, facility: "F-1"),
            Pool("P-2", "Lower", ooip: 50m, unit: "MMbbl", factor: 0.20m, facility: "F-1"),
            Pool("P-3", "Gas cap", ooip: 2m, unit: "Bcf", factor: 0.50m, facility: "F-2"),
            Pool("P-4", "Mixed", ooip: 30m, unit: "MMstb", factor: 0.10m, facility: "F-2"),
            Pool("P-5", "Nowhere", ooip: 10m, unit: "MMbbl", factor: 0.10m, facility: null),
        };

        var allocation = OgPoolsFacilitiesService.BuildAllocation(pools, facilities);

        var central = allocation.Facilities.Single(line => line.FacilityId == "F-1");

        Assert.Equal(45m, central.Total); // 35 + 10 MMbbl
        Assert.Equal("MMbbl", central.Unit);

        // F-2 holds a Bcf pool and an MMstb pool, so it has no total and names both units.
        var gas = allocation.Facilities.Single(line => line.FacilityId == "F-2");

        Assert.Null(gas.Total);
        Assert.Equal(new[] { "Bcf", "MMstb" }, gas.UnitMismatch);
        Assert.Contains("no total is produced", gas.Text, StringComparison.Ordinal);

        Assert.Equal("Nowhere", Assert.Single(allocation.UnroutedPools));
        Assert.Contains("routed to no facility", allocation.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void A_facility_state_comes_from_the_ppdm_row_and_the_planned_dates()
    {
        var operating = new FACILITY { FACILITY_ID = "F-1", ACTIVE_IND = "Y", ACTIVE_DATE = AsOf.AddYears(-1) };
        var abandoned = new FACILITY { FACILITY_ID = "F-2", ABANDONED_DATE = AsOf.AddDays(-10) };

        Assert.Equal(
            OgFacilityPlan.Operating,
            OgPoolsFacilitiesService.FacilityState(operating, AsOf.AddYears(-2), AsOf));

        Assert.Equal(
            OgFacilityPlan.Abandoned,
            OgPoolsFacilitiesService.FacilityState(abandoned, AsOf.AddYears(-2), AsOf));

        // Planned dates alone never report an operating facility: without a PPDM row there is no evidence it exists.
        Assert.Equal(
            OgFacilityPlan.InConstruction,
            OgPoolsFacilitiesService.FacilityState(null, AsOf.AddDays(-5), AsOf));

        Assert.Equal(
            OgFacilityPlan.Planned,
            OgPoolsFacilitiesService.FacilityState(null, AsOf.AddDays(30), AsOf));

        Assert.Equal(
            OgFacilityPlan.Planned,
            OgPoolsFacilitiesService.FacilityState(null, null, AsOf));
    }

    [Fact]
    public void A_planned_facility_is_excluded_from_the_operational_pages_and_an_operating_one_is_not()
    {
        var planned = Facility("F-1", "New plant", 500m, "BOPD", OgFacilityPlan.Planned);
        var operating = Facility("F-2", "Central", 1000m, "BOPD", OgFacilityPlan.Operating);

        Assert.True(planned.IsPlanned);
        Assert.True(planned.ExcludedFromOperationalPages);
        Assert.False(operating.IsPlanned);
        Assert.True(operating.IsOperating);
        Assert.False(operating.ExcludedFromOperationalPages);
    }

    [Fact]
    public void A_pool_shows_its_volume_unit_and_never_a_bare_number()
    {
        var withUnit = Pool("P-1", "Upper", 100m, "MMbbl", 0.35m, "F-1");
        var noUnit = Pool("P-2", "Lower", 100m, null, 0.35m, "F-1");

        Assert.Equal("100 MMbbl", withUnit.OoipText);
        Assert.Equal("35 MMbbl", withUnit.RecoverableText);

        Assert.False(noUnit.HasOoipUnit);
        Assert.Contains("(unit not recorded)", noUnit.OoipText, StringComparison.Ordinal);
        Assert.Contains("(unit not recorded)", noUnit.RecoverableText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_pool_value_is_refused_when_it_cannot_be_represented()
    {
        Assert.Equal(
            OgPoolsFacilitiesService.CodePoolNameRequired,
            OgPoolsFacilitiesService.ValidatePool(new OgPoolValues(" ", null, null, null, 10m, "MMbbl", 0.3m, 1, null))!.Value.Code);

        Assert.Equal(
            OgPoolsFacilitiesService.CodeVolumeUnitRequired,
            OgPoolsFacilitiesService.ValidatePool(new OgPoolValues("Upper", null, null, null, 10m, null, 0.3m, 1, null))!.Value.Code);

        Assert.Equal(
            OgPoolsFacilitiesService.CodeVolumeUnitRequired,
            OgPoolsFacilitiesService.ValidatePool(new OgPoolValues("Upper", null, null, null, -1m, "MMbbl", 0.3m, 1, null))!.Value.Code);

        Assert.Equal(
            OgPoolsFacilitiesService.CodeRecoveryInvalid,
            OgPoolsFacilitiesService.ValidatePool(new OgPoolValues("Upper", null, null, null, 10m, "MMbbl", 1.4m, 1, null))!.Value.Code);

        Assert.Equal(
            OgPoolsFacilitiesService.CodeWellsInvalid,
            OgPoolsFacilitiesService.ValidatePool(new OgPoolValues("Upper", null, null, null, 10m, "MMbbl", 0.3m, -1, null))!.Value.Code);

        Assert.Null(OgPoolsFacilitiesService.ValidatePool(
            new OgPoolValues("Upper", "P-1", "Sand A", "Water drive", 10m, "MMbbl", 0.3m, 4, null)));
    }

    [Fact]
    public void A_facility_value_is_refused_when_it_cannot_be_represented()
    {
        Assert.Equal(
            OgPoolsFacilitiesService.CodeFacilityNameRequired,
            OgPoolsFacilitiesService.ValidateFacility(new OgFacilityValues(null, " ", null, null, 10m, "BOPD", 1m, "USD", null, null))!.Value.Code);

        Assert.Equal(
            OgPoolsFacilitiesService.CodeCapacityUnitRequired,
            OgPoolsFacilitiesService.ValidateFacility(new OgFacilityValues(null, "Central", null, null, 10m, null, 1m, "USD", null, null))!.Value.Code);

        Assert.Equal(
            OgPoolsFacilitiesService.CodeCapacityUnitRequired,
            OgPoolsFacilitiesService.ValidateFacility(new OgFacilityValues(null, "Central", null, null, -1m, "BOPD", 1m, "USD", null, null))!.Value.Code);

        Assert.Equal(
            OgPoolsFacilitiesService.CodeCurrencyRequired,
            OgPoolsFacilitiesService.ValidateFacility(new OgFacilityValues(null, "Central", null, null, 10m, "BOPD", 1m, null, null, null))!.Value.Code);

        Assert.Equal(
            OgPoolsFacilitiesService.CodeDateOrder,
            OgPoolsFacilitiesService.ValidateFacility(
                new OgFacilityValues(null, "Central", null, null, 10m, "BOPD", 1m, "USD", AsOf, AsOf.AddDays(-1)))!.Value.Code);

        Assert.Null(OgPoolsFacilitiesService.ValidateFacility(
            new OgFacilityValues("F-1", "Central", "CPF", "CONSTRUCTION", 1000m, "BOPD", 80m, "USD", AsOf, AsOf.AddMonths(6))));
    }

    [Fact]
    public async Task No_connection_is_a_failure_not_an_empty_plan()
    {
        var service = new OgPoolsFacilitiesService(() => null, new Quiet());

        var loaded = await service.LoadAsync("F-1", "Alpha", null, AsOf);

        Assert.False(loaded.Success);
        Assert.Equal(OgPoolsFacilitiesService.CodeNoConnection, loaded.Reference);
        Assert.Null(loaded.Value);
    }

    [Fact]
    public async Task A_pool_save_needs_an_actor_and_a_valid_volume()
    {
        var service = new OgPoolsFacilitiesService(() => null, new Quiet());
        var values = new OgPoolValues("Upper", null, null, null, 10m, "MMbbl", 0.3m, 4, null);

        Assert.Equal(
            OgPoolsFacilitiesService.CodeActorRequired,
            (await service.SavePoolAsync("FDP-1", values, " ", AsOf)).Reference);

        Assert.Equal(
            OgPoolsFacilitiesService.CodeVolumeUnitRequired,
            (await service.SavePoolAsync(
                "FDP-1",
                new OgPoolValues("Upper", null, null, null, 10m, null, 0.3m, 4, null),
                "sara",
                AsOf)).Reference);

        Assert.Equal(
            OgPoolsFacilitiesService.CodeActorRequired,
            (await service.SaveFacilityAsync(
                "FDP-1",
                new OgFacilityValues(null, "Central", null, null, 10m, "BOPD", 1m, "USD", null, null),
                " ",
                AsOf)).Reference);
    }

    private static DEVELOPMENT_WELL_SCHEDULE Schedule(string id, decimal? oil, decimal? gas) =>
        new()
        {
            SCHEDULE_ID = id,
            FDP_ID = "FDP-1",
            FIELD_ID = "F-1",
            INITIAL_RATE_OIL_BOPD = oil,
            INITIAL_RATE_GAS_MMSCFD = gas,
        };

    private static OgFacilityPlan Facility(
        string id,
        string name,
        decimal? capacity,
        string? unit,
        string state) =>
        new(
            "INV-" + id,
            "FDP-1",
            3,
            id,
            name,
            "CPF",
            "CONSTRUCTION",
            capacity,
            unit,
            100m,
            "USD",
            AsOf.AddDays(-100),
            AsOf.AddDays(100),
            state,
            ExistsInPpdm: state == OgFacilityPlan.Operating);

    private static OgPoolPlan Pool(
        string id,
        string name,
        decimal? ooip,
        string? unit,
        decimal factor,
        string? facility) =>
        new(
            "PP-" + id,
            "FDP-1",
            "F-1",
            3,
            id,
            name,
            "Sand A",
            "Water drive",
            ooip,
            unit,
            factor,
            4,
            facility);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
