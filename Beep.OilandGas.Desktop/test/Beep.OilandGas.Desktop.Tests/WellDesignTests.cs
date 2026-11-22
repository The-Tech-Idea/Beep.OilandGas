using TheTechIdea.Beep.OilandGas.Desktop.Data.Development;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEV-03: the completeness rule, the depth and cost validation, the casing input check and the preview gate - the
/// parts that do not need a database. The versioned design, the scoping and the recorded check are proven in the
/// gateway test.
/// </summary>
public sealed class WellDesignTests
{
    private static readonly DateTime AsOf = new(2026, 6, 1);

    [Fact]
    public void A_complete_design_can_be_authored_into_a_program_and_an_empty_one_names_every_gap()
    {
        var empty = Well();
        var missing = OgWellDesignService.MissingItems(empty);

        Assert.False(empty.CanAuthorProgram);
        Assert.Contains("Objective", missing);
        Assert.Contains("Target formation", missing);
        Assert.Contains("Target depth with its unit", missing);
        Assert.Contains("Trajectory intent", missing);
        Assert.Contains("Casing scheme", missing);
        Assert.Contains("Completion concept", missing);
        Assert.Contains("Estimated cost with its currency", missing);
        Assert.Contains("Spud window", missing);

        var complete = Well(
            objective: "Drain the upper sand",
            formation: "Upper sand",
            depth: 2400m,
            unit: "m",
            casing: [Casing(1, "SURFACE", 300m, "m")],
            completion: "Perforated liner",
            cost: 12.5m,
            currency: "USD",
            spudStart: AsOf,
            spudEnd: AsOf.AddDays(20),
            trajectory: "Vertical");

        Assert.Empty(OgWellDesignService.MissingItems(complete));
        Assert.True(complete.CanAuthorProgram);
        Assert.Equal("The design is complete.", complete.MissingText);
    }

    [Fact]
    public void A_target_depth_without_a_unit_is_not_a_depth()
    {
        var noUnit = Well(depth: 2400m, unit: null);

        Assert.False(noUnit.HasDepth);
        Assert.Contains("Target depth with its unit", OgWellDesignService.MissingItems(noUnit));
        Assert.Contains("(unit not recorded)", noUnit.TargetText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_depth_without_a_unit_is_refused_and_so_is_a_negative_or_unpriced_value()
    {
        Assert.Equal(
            OgWellDesignService.CodeDepthUnitRequired,
            OgWellDesignService.Validate(Values(depth: 2400m, unit: null))!.Value.Code);

        Assert.Equal(
            OgWellDesignService.CodeDepthInvalid,
            OgWellDesignService.Validate(Values(depth: -1m, unit: "m"))!.Value.Code);

        Assert.Equal(
            OgWellDesignService.CodeCostInvalid,
            OgWellDesignService.Validate(Values(depth: 2400m, unit: "m", cost: -1m))!.Value.Code);

        Assert.Equal(
            OgWellDesignService.CodeCurrencyRequired,
            OgWellDesignService.Validate(Values(depth: 2400m, unit: "m", cost: 1m, currency: null))!.Value.Code);

        Assert.Equal(
            OgWellDesignService.CodeWindowOrder,
            OgWellDesignService.Validate(Values(
                depth: 2400m,
                unit: "m",
                spudStart: AsOf,
                spudEnd: AsOf.AddDays(-1)))!.Value.Code);

        Assert.Null(OgWellDesignService.Validate(Values(depth: 2400m, unit: "m")));
    }

    [Fact]
    public void The_casing_check_passes_consistent_inputs()
    {
        var design = Well(
            depth: 2400m,
            unit: "m",
            casing:
            [
                Casing(1, "SURFACE", 400m, "m"),
                Casing(2, "INTERMEDIATE", 1600m, "m"),
                Casing(3, "PRODUCTION", 2400m, "m"),
            ]);

        Assert.Empty(OgWellDesignService.CheckCasing(design, design.Casing));
    }

    [Fact]
    public void The_casing_check_names_each_inconsistent_input()
    {
        var design = Well(
            depth: 2400m,
            unit: "m",
            casing:
            [
                Casing(1, "SURFACE", 1600m, "m"),
                Casing(2, "PRODUCTION", 400m, "m"),
                Casing(3, "LINER", 2600m, "m"),
                Casing(4, "INTERMEDIATE", 900m, "ft") with { Grade = null, Od = 0m },
            ]);

        var findings = OgWellDesignService.CheckCasing(design, design.Casing);

        Assert.Contains(findings, finding => finding.Contains("not deeper", StringComparison.Ordinal));
        Assert.Contains(findings, finding => finding.Contains("below the", StringComparison.Ordinal));
        Assert.Contains(findings, finding => finding.Contains("measured in ft", StringComparison.Ordinal));
        Assert.Contains(findings, finding => finding.Contains("no steel grade", StringComparison.Ordinal));
        Assert.Contains(findings, finding => finding.Contains("no outside diameter", StringComparison.Ordinal));
    }

    [Fact]
    public void The_casing_check_refuses_an_empty_outline_and_a_design_without_a_unit()
    {
        var noCasing = Well(depth: 2400m, unit: "m");
        var noUnit = Well(casing: [Casing(1, "SURFACE", 400m, "m")]);

        Assert.Contains(
            OgWellDesignService.CheckCasing(noCasing, noCasing.Casing),
            finding => finding.Contains("No casing string", StringComparison.Ordinal));

        Assert.Contains(
            OgWellDesignService.CheckCasing(noUnit, noUnit.Casing),
            finding => finding.Contains("no unit", StringComparison.Ordinal));
    }

    [Fact]
    public void The_schematic_preview_is_disabled_with_the_reason_until_casing_exists()
    {
        var noCasing = Well(depth: 2400m, unit: "m");

        Assert.False(noCasing.CasingPreviewEnabled);
        Assert.Contains("no casing string", noCasing.CasingPreviewReason!, StringComparison.OrdinalIgnoreCase);

        var withCasing = Well(casing: [Casing(1, "SURFACE", 400m, "m")]);

        Assert.True(withCasing.CasingPreviewEnabled);
        Assert.Null(withCasing.CasingPreviewReason);
        Assert.Equal(1, withCasing.CasingCount);
    }

    [Fact]
    public async Task No_connection_is_a_failure_not_an_empty_design_list()
    {
        var service = new OgWellDesignService(() => null, new Quiet());

        var loaded = await service.LoadAsync("F-1", "Alpha", null);

        Assert.False(loaded.Success);
        Assert.Equal(OgWellDesignService.CodeNoConnection, loaded.Reference);
        Assert.Null(loaded.Value);
    }

    [Fact]
    public async Task A_casing_string_needs_a_name_a_depth_and_a_unit()
    {
        var service = new OgWellDesignService(() => null, new Quiet());

        Assert.Equal(
            OgWellDesignService.CodeActorRequired,
            (await service.AddCasingStringAsync("D-1", new OgCasingStringRequest("SURFACE", 20m, 100m, "K55", 400m, "m"), " ")).Reference);

        Assert.Equal(
            OgWellDesignService.CodeCasingInvalid,
            (await service.AddCasingStringAsync("D-1", new OgCasingStringRequest(" ", 20m, 100m, "K55", 400m, "m"), "sara")).Reference);

        Assert.Equal(
            OgWellDesignService.CodeCasingInvalid,
            (await service.AddCasingStringAsync("D-1", new OgCasingStringRequest("SURFACE", 20m, 100m, "K55", 0m, "m"), "sara")).Reference);

        Assert.Equal(
            OgWellDesignService.CodeDepthUnitRequired,
            (await service.AddCasingStringAsync("D-1", new OgCasingStringRequest("SURFACE", 20m, 100m, "K55", 400m, " "), "sara")).Reference);
    }

    [Fact]
    public async Task An_approval_needs_an_actor_and_a_revision_needs_a_reason()
    {
        var service = new OgWellDesignService(() => null, new Quiet());

        Assert.Equal(
            OgWellDesignService.CodeActorRequired,
            (await service.ApproveAsync("D-1", " ")).Reference);

        Assert.Equal(
            OgWellDesignService.CodeActorRequired,
            (await service.ReviseAsync("D-1", Values(depth: 2400m, unit: "m"), "why", " ")).Reference);

        Assert.Equal(
            "well-design-reason-required",
            (await service.ReviseAsync("D-1", Values(depth: 2400m, unit: "m"), " ", "sara")).Reference);
    }

    private static OgDevelopmentWell Well(
        string? objective = null,
        string? formation = null,
        decimal? depth = null,
        string? unit = null,
        IReadOnlyList<OgCasingString>? casing = null,
        string? completion = null,
        decimal? cost = null,
        string? currency = null,
        DateTime? spudStart = null,
        DateTime? spudEnd = null,
        string? trajectory = null) =>
        new(
            "D-1",
            "FDP-1",
            "F-1",
            "SCH-1",
            "WELL-1",
            "WELL-1",
            1,
            true,
            OgDevelopmentWell.Concept,
            objective,
            formation,
            depth,
            unit,
            trajectory,
            completion,
            null,
            cost,
            currency,
            spudStart,
            spudEnd,
            null,
            null,
            null,
            casing ?? [],
            null);

    private static OgCasingString Casing(int order, string name, decimal depth, string unit) =>
        new(order, name, 20m, 100m, "K55", depth, unit);

    private static OgWellDesignValues Values(
        decimal? depth,
        string? unit,
        decimal? cost = null,
        string? currency = null,
        DateTime? spudStart = null,
        DateTime? spudEnd = null) =>
        new("Drain the upper sand", "Upper sand", depth, unit, "Vertical", "Perforated liner", null,
            cost, currency, spudStart, spudEnd);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
