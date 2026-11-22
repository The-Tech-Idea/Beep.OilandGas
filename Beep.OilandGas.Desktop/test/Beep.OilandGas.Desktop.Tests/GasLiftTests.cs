using Beep.OilandGas.GasLift.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LFT-02: the catalog messages, the blocking rule, the reproducible spacing, the port sizes, the explicit units and the
/// evaluated set - the parts that do not need a database. Saving and reloading are proven in the gateway test.
/// </summary>
public sealed class GasLiftTests
{
    private const string PageFileName = "GasLiftPageView.cs";

    [Fact]
    public void Every_violated_limit_carries_the_message_from_the_owners_catalog()
    {
        // AC-1. A starved injection rate violates the minimum, and the sentence must be the catalog's own.
        var starved = OgGasLiftService.Build(Inputs() with { InjectionRate = 20m });

        var limit = Assert.Single(starved.Limits, item => item.Code == "MIN_INJECTION_MSCFD");

        Assert.True(limit.Blocking);
        Assert.Equal(GasLiftDesignLimitMessages.GetMessage("MIN_INJECTION_MSCFD"), limit.Message);
        Assert.Contains("50", limit.Message, StringComparison.Ordinal);
        Assert.Contains("blocking", limit.Describe(), StringComparison.Ordinal);

        // A shallow well with several valves breaks the screening spacing.
        var shallow = OgGasLiftService.Build(Inputs() with { WellDepthFt = 300m, Valves = 5 });

        var spacing = Assert.Single(shallow.Limits, item => item.Code == "MIN_VALVE_SPACING_FT");

        Assert.False(spacing.Blocking);
        Assert.Equal(GasLiftDesignLimitMessages.GetMessage("MIN_VALVE_SPACING_FT"), spacing.Message);
        Assert.Contains("flagged", spacing.Describe(), StringComparison.Ordinal);

        // A rate above the maximum is the other blocking case.
        var flooded = OgGasLiftService.Build(Inputs() with { InjectionRate = 12000m });

        var maximum = Assert.Single(flooded.Limits, item => item.Code == "MAX_INJECTION_MSCFD");

        Assert.True(maximum.Blocking);
        Assert.Equal(GasLiftDesignLimitMessages.GetMessage("MAX_INJECTION_MSCFD"), maximum.Message);
    }

    [Fact]
    public void A_blocking_limit_prevents_the_save_with_its_specific_message()
    {
        // AC-2.
        var tooMany = OgGasLiftService.Build(Inputs() with { Valves = 25 });

        var limit = Assert.Single(tooMany.Limits, item => item.Code == "MAX_VALVES");

        Assert.True(limit.Blocking);
        Assert.Equal(GasLiftDesignLimitMessages.GetMessage("MAX_VALVES"), limit.Message);
        Assert.True(tooMany.HasBlockingLimit);
        Assert.Equal(GasLiftDesignLimitMessages.GetMessage("MAX_VALVES"), tooMany.BlockingLimits[0].Message);

        // The engine can only place its own maximum, so the placed count is bounded while the limit still fires.
        Assert.Equal(GasLiftConstants.MaximumNumberOfValves, tooMany.ValveCount);

        var refused = Service().SaveAsync(tooMany, "sara").GetAwaiter().GetResult();

        Assert.False(refused.Success);
        Assert.Equal(GasLiftRules.CodeBlockingLimit, refused.Reference);
        Assert.Contains(GasLiftDesignLimitMessages.GetMessage("MAX_VALVES")!, refused.Message!, StringComparison.Ordinal);

        // A design whose only violation is a screening warning is saveable.
        var flagged = OgGasLiftService.Build(Inputs() with { WellDepthFt = 300m, Valves = 5 });

        Assert.False(flagged.HasBlockingLimit);
        Assert.NotEmpty(flagged.Limits);
    }

    [Fact]
    public void Spacing_is_reproducible_for_the_chosen_method()
    {
        // AC-3.
        var first = OgGasLiftService.Build(Inputs());
        var second = OgGasLiftService.Build(Inputs());

        Assert.Equal(first.Valves.Count, second.Valves.Count);

        for (var index = 0; index < first.Valves.Count; index++)
        {
            Assert.Equal(first.Valves[index].DepthFt, second.Valves[index].DepthFt);
            Assert.Equal(first.Valves[index].OpeningPressure, second.Valves[index].OpeningPressure);
            Assert.Equal(first.Valves[index].ClosingPressure, second.Valves[index].ClosingPressure);
        }

        // Equal depth places the valves on an even beat; the pressure-drop method does not.
        var even = first.Valves.Select(valve => valve.DepthFt).ToList();
        var gaps = even.Zip(even.Skip(1), (upper, lower) => lower - upper).Distinct().ToList();

        Assert.Single(gaps);

        var pressureDrop = OgGasLiftService.Build(Inputs() with { SpacingMethod = GasLiftSpacingMethods.EqualPressureDrop });

        Assert.Equal(5, pressureDrop.Valves.Count);
        Assert.NotEqual(first.Valves[0].DepthFt, pressureDrop.Valves[0].DepthFt);
        Assert.Equal(GasLiftSpacingMethods.EqualPressureDrop, pressureDrop.SpacingMethod);

        // The first valve unloads, the last operates, the ones between are intermediate.
        Assert.Equal("UNLOADING", first.Valves[0].Service);
        Assert.Equal("OPERATING", first.Valves[^1].Service);
        Assert.All(first.Valves.Skip(1).Take(first.Valves.Count - 2),
            valve => Assert.Equal("INTERMEDIATE", valve.Service));

        // A closing pressure is the standard ratio below the opening one.
        Assert.Equal(
            first.Valves[0].OpeningPressure * GasLiftConstants.StandardClosingPressureRatio,
            first.Valves[0].ClosingPressure);
    }

    [Fact]
    public async Task Port_sizes_come_from_the_reference_set_and_an_unknown_one_is_refused()
    {
        // AC-4.
        var standard = OgGasLiftService.Build(Inputs());

        Assert.Contains(standard.PortSizeIn!.Value, GasLiftConstants.StandardPortSizes);

        // Without a connection the offer is the module's standard sizes, and a size outside them is refused.
        var refused = await Service().DesignAsync(Inputs() with { PortSizeIn = 0.3m });

        Assert.False(refused.Success);
        Assert.Equal(GasLiftRules.CodePortUnknown, refused.Reference);
        Assert.Contains("0.5", refused.Message!, StringComparison.Ordinal);

        var accepted = await Service().DesignAsync(Inputs() with { PortSizeIn = 0.375m });

        Assert.True(accepted.Success, accepted.Message);
        Assert.Equal(0.375m, accepted.Value!.Selected!.PortSizeIn);
        Assert.Contains(0.375m, accepted.Value.PortSizes);
    }

    [Fact]
    public void The_units_are_explicit_and_the_evaluated_set_is_recorded()
    {
        // AC-5 and AC-6.
        var design = OgGasLiftService.Build(Inputs());

        Assert.Equal(GasLiftDesignLimits.EvaluatedCodes, design.EvaluatedCodes);
        Assert.Equal(6, design.EvaluatedCodes.Count);
        Assert.Contains("MIN_INJECTION_MSCFD", design.EvaluatedCodes);
        Assert.Contains("MIN_VALVE_SPACING_FT", design.EvaluatedCodes);

        Assert.Equal(OgGasLiftDesign.InjectionUnitName, design.InjectionUnit);
        Assert.Equal(OgGasLiftDesign.ProductionUnitName, design.ProductionUnit);
        Assert.Contains("Mscf/d", design.InjectionText, StringComparison.Ordinal);
        Assert.Contains("STB/d", design.ExpectedText, StringComparison.Ordinal);
        Assert.Contains("evaluated:", design.EvaluatedText, StringComparison.Ordinal);

        // AC-5 is also a guard on the view: it prints the units the service states rather than its own literals.
        var page = File.ReadAllText(SourceRoot.AllSources().Single(file => Path.GetFileName(file) == PageFileName));

        Assert.Contains("InjectionUnit", page, StringComparison.Ordinal);
        Assert.Contains("ProductionUnit", page, StringComparison.Ordinal);
    }

    [Fact]
    public void The_potential_analysis_recommends_an_injection_rate()
    {
        var design = OgGasLiftService.Build(Inputs());

        Assert.NotNull(design.RecommendedInjectionRate);
        Assert.InRange(
            design.RecommendedInjectionRate!.Value,
            GasLiftConstants.MinimumGasInjectionRate,
            GasLiftConstants.MaximumGasInjectionRate);

        Assert.NotNull(design.MaximumProductionRate);
        Assert.Contains("recommends", design.RecommendationText, StringComparison.Ordinal);

        // The engine's expected rate and efficiency are carried as computed.
        Assert.True(design.ExpectedProductionRate > 0m);
        Assert.InRange(design.SystemEfficiency, 0m, 1m);
    }

    [Fact]
    public void The_inputs_are_validated_before_the_engine_is_called()
    {
        Assert.Equal(GasLiftRules.CodeWellRequired, GasLiftRules.Validate(Inputs() with { WellId = " " })!.Value.Code);
        Assert.Equal(GasLiftRules.CodeInputRequired, GasLiftRules.Validate(Inputs() with { WellDepthFt = null })!.Value.Code);
        Assert.Equal(
            GasLiftRules.CodeSpacingUnsupported,
            GasLiftRules.Validate(Inputs() with { SpacingMethod = GasLiftSpacingMethods.GradientBased })!.Value.Code);
        Assert.Equal(
            GasLiftRules.CodeInputInvalid,
            GasLiftRules.Validate(Inputs() with { InjectionPressure = 100m })!.Value.Code);
        Assert.Equal(
            GasLiftRules.CodeInputInvalid,
            GasLiftRules.Validate(Inputs() with { WaterCut = 1.5m })!.Value.Code);
        Assert.Equal(
            GasLiftRules.CodeInputInvalid,
            GasLiftRules.Validate(Inputs() with { Valves = 0 })!.Value.Code);

        Assert.Null(GasLiftRules.Validate(Inputs()));
    }

    [Fact]
    public async Task Commands_need_a_connection_and_an_actor_and_keep_a_lost_save()
    {
        var service = new OgGasLiftService(() => null, new Quiet());
        var design = OgGasLiftService.Build(Inputs());

        Assert.Equal(GasLiftRules.CodeNoConnection, (await service.LoadAsync()).Reference);
        Assert.Equal(GasLiftRules.CodeActorRequired, (await service.SaveAsync(design, " ")).Reference);
        Assert.Equal(GasLiftRules.CodeNoConnection, (await service.SaveAsync(design, "sara")).Reference);
        Assert.Equal(GasLiftRules.CodeNothingPending, (await service.RetrySaveAsync("sara")).Reference);
        Assert.Null(service.Pending);
    }

    // ------------------------------------------------------------------ helpers

    private static OgGasLiftService Service() => new(() => null, new Quiet());

    /// <summary>An 8,000 ft well producing 500 STB/d, lifted with 500 Mscf/d at 1,200 psia.</summary>
    private static OgGasLiftInputs Inputs() =>
        new(
            "WELL-1",
            WellDepthFt: 8000m,
            WellheadPressure: 150m,
            BottomHolePressure: 2500m,
            ReservoirPressure: 3000m,
            GasOilRatio: 500m,
            DesiredProductionRate: 500m,
            OilGravity: 35m,
            WaterCut: 0.2m,
            WellheadTemperature: 100m,
            BottomHoleTemperature: 200m,
            GasSpecificGravity: 0.65m,
            InjectionPressure: 1200m,
            InjectionRate: 500m,
            Valves: 5,
            PortSizeIn: 0.5m,
            SpacingMethod: GasLiftSpacingMethods.EqualDepth);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
