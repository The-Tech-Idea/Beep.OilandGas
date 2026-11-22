using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.GasLift.Calculations;
using Beep.OilandGas.GasLift.Data;
using Beep.OilandGas.GasLift.Constants;
using Beep.OilandGas.Models.Data.GasLift;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;

/// <summary>The spacing methods the engine implements (LFT-02).</summary>
public static class GasLiftSpacingMethods
{
    public const string EqualDepth = "UNIFORM_SPACING";
    public const string EqualPressureDrop = "EQUAL_PRESSURE_DROP";

    /// <summary>Listed by the reference set but not implemented by the engine.</summary>
    public const string GradientBased = "GRADIENT_BASED";

    public static IReadOnlyList<string> All { get; } = [EqualDepth, EqualPressureDrop, GradientBased];

    public static bool IsSupported(string? method) =>
        !string.IsNullOrWhiteSpace(method)
        && (string.Equals(method.Trim(), EqualDepth, StringComparison.OrdinalIgnoreCase)
            || string.Equals(method.Trim(), EqualPressureDrop, StringComparison.OrdinalIgnoreCase));

    public static string NameOf(string method) => method switch
    {
        EqualDepth => "Equal depth spacing",
        EqualPressureDrop => "Equal pressure-drop spacing",
        GradientBased => "Gradient-based spacing (not implemented)",
        _ => method,
    };
}

/// <summary>The inputs of one gas lift design (LFT-02). Every value is required: nothing is defaulted.</summary>
public sealed record OgGasLiftInputs(
    string? WellId,
    decimal? WellDepthFt,
    decimal? WellheadPressure,
    decimal? BottomHolePressure,
    decimal? ReservoirPressure,
    decimal? GasOilRatio,
    decimal? DesiredProductionRate,
    decimal? OilGravity,
    decimal? WaterCut,
    decimal? WellheadTemperature,
    decimal? BottomHoleTemperature,
    decimal? GasSpecificGravity,
    decimal? InjectionPressure,
    decimal? InjectionRate,
    int Valves,
    decimal? PortSizeIn,
    string? SpacingMethod);

/// <summary>One placed valve (LFT-02).</summary>
public sealed record OgGasLiftValve(
    int Index,
    decimal DepthFt,
    decimal OpeningPressure,
    decimal ClosingPressure,
    string Service)
{
    public string DepthText => DepthFt.ToString("#,0.##", CultureInfo.InvariantCulture);

    public string OpeningText => OpeningPressure.ToString("#,0.##", CultureInfo.InvariantCulture);

    public string ClosingText => ClosingPressure.ToString("#,0.##", CultureInfo.InvariantCulture);

    public string Describe() =>
        $"valve {Index.ToString(CultureInfo.InvariantCulture)} · {ServiceText} · {DepthText} ft · " +
        $"opens {OpeningText} psia · closes {ClosingText} psia";

    public string ServiceText => Service.ToLowerInvariant();
}

/// <summary>One violated limit, with the owning project's message (LFT-02 AC-1).</summary>
public sealed record OgGasLiftLimit(string Code, string Message, bool Blocking)
{
    public string SeverityText => Blocking ? "blocking" : "flagged";

    public string Describe() => $"{Code.ToLowerInvariant()} ({SeverityText}): {Message}";
}

/// <summary>
/// One gas lift design (LFT-02), matching the plan's <c>OgGasLiftDesign</c> plus the potential recommendation and the
/// evaluated limits.
/// </summary>
public sealed record OgGasLiftDesign(
    string Id,
    string? WellId,
    string SpacingMethod,
    decimal? PortSizeIn,
    decimal InjectionRate,
    decimal InjectionPressure,
    int ValveCount,
    decimal ExpectedProductionRate,
    decimal SystemEfficiency,
    decimal? RecommendedInjectionRate,
    decimal? MaximumProductionRate,
    decimal? OptimalGasLiquidRatio,
    IReadOnlyList<OgGasLiftValve> Valves,
    IReadOnlyList<OgGasLiftLimit> Limits,
    IReadOnlyList<string> EvaluatedCodes,
    string InjectionUnit,
    string ProductionUnit,
    string? Actor,
    DateTime? RecordedAt)
{
    public const string InjectionUnitName = "Mscf/d";
    public const string ProductionUnitName = "STB/d";

    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    /// <summary>AC-2: a blocking limit prevents the save.</summary>
    public bool HasBlockingLimit => Limits.Any(limit => limit.Blocking);

    public IReadOnlyList<OgGasLiftLimit> BlockingLimits => Limits.Where(limit => limit.Blocking).ToList();

    public string MethodText => GasLiftSpacingMethods.NameOf(SpacingMethod);

    public string InjectionText =>
        InjectionRate.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + InjectionUnit;

    public string InjectionPressureText =>
        InjectionPressure.ToString("#,0.##", CultureInfo.InvariantCulture) + " psia";

    public string ExpectedText =>
        ExpectedProductionRate.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + ProductionUnit;

    public string EfficiencyText =>
        (SystemEfficiency * 100m).ToString("0.#", CultureInfo.InvariantCulture) + "%";

    public string PortText => PortSizeIn is { } port
        ? port.ToString("0.###", CultureInfo.InvariantCulture) + " in"
        : "no port size recorded";

    public string RecommendationText => RecommendedInjectionRate is not { } rate
        ? "the potential analysis made no recommendation"
        : $"the potential analysis recommends {rate.ToString("#,0.##", CultureInfo.InvariantCulture)} Mscf/d, " +
          $"where the well would make about " +
          $"{MaximumProductionRate?.ToString("#,0.##", CultureInfo.InvariantCulture) ?? "(unreported)"} {ProductionUnit}";

    public string LimitsText => Limits.Count == 0
        ? "no design limit is violated"
        : string.Join(Environment.NewLine, Limits.Select(limit => limit.Describe()));

    public string ValvesText => Valves.Count == 0
        ? "no valve is placed"
        : string.Join(Environment.NewLine, Valves.Select(valve => valve.Describe()));

    public string EvaluatedText => EvaluatedCodes.Count == 0
        ? "no limit was evaluated"
        : "evaluated: " + string.Join(", ", EvaluatedCodes.Select(code => code.ToLowerInvariant()));

    public string SavedText => IsSaved
        ? $"saved by {Actor ?? "(unknown)"} on " +
          $"{RecordedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date"}"
        : "not saved";

    public string Describe() =>
        $"{WellId ?? "(no well)"} · {MethodText} · {ValveCount.ToString(CultureInfo.InvariantCulture)} valve(s) · " +
        $"injection {InjectionText} at {InjectionPressureText} · expected {ExpectedText} · " +
        $"{Limits.Count.ToString(CultureInfo.InvariantCulture)} limit(s) violated · {SavedText}";
}

/// <summary>
/// A design whose save was not acknowledged: the design is retained and the same insert is retried (LFT-02's state table).
/// </summary>
public sealed record OgGasLiftPending(OgGasLiftDesign Design, string Actor);

/// <summary>The gas lift workspace (LFT-02).</summary>
public sealed record OgGasLiftWorkspace(
    IReadOnlyList<OgGasLiftDesign> Designs,
    OgGasLiftDesign? Selected,
    IReadOnlyList<string> SpacingMethods,
    IReadOnlyList<decimal> PortSizes,
    bool PortSizesFromReference,
    bool HistoryFailed,
    string? HistoryReason,
    string NoInjectionReason)
{
    public string DesignsText => Designs.Count == 0
        ? "No gas lift design is saved yet."
        : string.Join(Environment.NewLine, Designs.Select(design => design.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved designs could not be read; new designs are still allowed"
        : $"{Designs.Count.ToString(CultureInfo.InvariantCulture)} saved design(s)";

    public string PortSizesText => PortSizes.Count == 0
        ? "no port size is on offer, so no design can be saved"
        : "port sizes " + string.Join(", ", PortSizes.Select(size => size.ToString("0.###", CultureInfo.InvariantCulture))) +
          " in · " + (PortSizesFromReference
              ? "from R_GAS_LIFT_REFERENCE_CODE"
              : "from GasLiftConstants, because the reference table has no port-size row");
}

/// <summary>The gas lift rules, stated once so the page, the service and the tests agree.</summary>
public static class GasLiftRules
{
    public const string CodeNoConnection = "gaslift-no-connection";
    public const string CodeReadFailed = "gaslift-read-failed";
    public const string CodeWriteFailed = "gaslift-write-failed";
    public const string CodeActorRequired = "gaslift-actor-required";
    public const string CodeWellRequired = "gaslift-well-required";
    public const string CodeInputRequired = "gaslift-input-required";
    public const string CodeInputInvalid = "gaslift-input-invalid";
    public const string CodeInjectionUnavailable = "gaslift-injection-unavailable";
    public const string CodeSpacingUnsupported = "gaslift-spacing-unsupported";
    public const string CodePortUnknown = "gaslift-port-size-unknown";
    public const string CodeBlockingLimit = "gaslift-blocking-limit";
    public const string CodeNothingToSave = "gaslift-nothing-to-save";
    public const string CodeNothingPending = "gaslift-nothing-pending";

    public static IReadOnlyList<string> Missing(OgGasLiftInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = new List<string>();

        void Need(decimal? value, string name)
        {
            if (value is null)
            {
                missing.Add(name);
            }
        }

        if (string.IsNullOrWhiteSpace(inputs.WellId))
        {
            missing.Add("well");
        }

        Need(inputs.WellDepthFt, "well depth");
        Need(inputs.WellheadPressure, "wellhead pressure");
        Need(inputs.BottomHolePressure, "bottom hole pressure");
        Need(inputs.ReservoirPressure, "reservoir pressure");
        Need(inputs.GasOilRatio, "gas-oil ratio");
        Need(inputs.DesiredProductionRate, "desired production rate");
        Need(inputs.OilGravity, "oil gravity");
        Need(inputs.WaterCut, "water cut");
        Need(inputs.WellheadTemperature, "wellhead temperature");
        Need(inputs.BottomHoleTemperature, "bottom hole temperature");
        Need(inputs.GasSpecificGravity, "gas specific gravity");
        Need(inputs.InjectionPressure, "gas injection pressure");
        Need(inputs.InjectionRate, "gas injection rate");
        Need(inputs.PortSizeIn, "valve port size");

        if (string.IsNullOrWhiteSpace(inputs.SpacingMethod))
        {
            missing.Add("spacing method");
        }

        return missing;
    }

    public static (string Code, string Message)? Validate(OgGasLiftInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            return missing.Contains("well")
                ? (CodeWellRequired, "Choose the well the design is for.")
                : (CodeInputRequired, $"The design needs {string.Join(", ", missing)}.");
        }

        if (!GasLiftSpacingMethods.IsSupported(inputs.SpacingMethod))
        {
            return (CodeSpacingUnsupported,
                $"{inputs.SpacingMethod} has no implementation in the gas lift engine, which places valves by equal " +
                $"depth or by equal pressure drop; use {GasLiftSpacingMethods.EqualDepth} or " +
                $"{GasLiftSpacingMethods.EqualPressureDrop}.");
        }

        if (inputs.Valves <= 0)
        {
            return (CodeInputInvalid, "A design places at least one valve.");
        }

        if (inputs.InjectionPressure <= inputs.WellheadPressure)
        {
            return (CodeInputInvalid,
                "The gas injection pressure must be above the wellhead pressure, or the gas cannot enter the tubing.");
        }

        if (inputs.BottomHolePressure <= inputs.WellheadPressure)
        {
            return (CodeInputInvalid, "The bottom hole pressure must be above the wellhead pressure.");
        }

        if (inputs.WaterCut is < 0m or > 1m)
        {
            return (CodeInputInvalid, "The water cut is a fraction between 0 and 1.");
        }

        foreach (var (value, name) in new (decimal?, string)[]
                 {
                     (inputs.WellDepthFt, "well depth"),
                     (inputs.WellheadPressure, "wellhead pressure"),
                     (inputs.ReservoirPressure, "reservoir pressure"),
                     (inputs.DesiredProductionRate, "desired production rate"),
                     (inputs.GasSpecificGravity, "gas specific gravity"),
                 })
        {
            if (value is <= 0m)
            {
                return (CodeInputInvalid, $"The {name} must be above zero.");
            }
        }

        return null;
    }
}

/// <summary>LFT-02: gas lift design.</summary>
public interface IOgGasLift
{
    /// <summary>Designs the installation. Nothing is written.</summary>
    Task<OgResult<OgGasLiftWorkspace>> DesignAsync(
        OgGasLiftInputs inputs,
        CancellationToken cancellationToken = default);

    /// <summary>Saves a design; a blocking limit refuses the save (AC-2).</summary>
    Task<OgResult<OgGasLiftWorkspace>> SaveAsync(
        OgGasLiftDesign design,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The saved designs, the port sizes on offer and the spacing methods; an unavailable history is a state.</summary>
    Task<OgResult<OgGasLiftWorkspace>> LoadAsync(
        string? wellId = null,
        string? designId = null,
        CancellationToken cancellationToken = default);

    /// <summary>The design retained when its save was not acknowledged.</summary>
    OgGasLiftPending? Pending { get; }

    /// <summary>Retries the same insert.</summary>
    Task<OgResult<OgGasLiftWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// LFT-02 over the GasLift module's potential, valve-design and spacing calculators, its design-limit catalog and the
/// <c>GAS_LIFT_DESIGN</c> row, through a unit of work.
/// </summary>
/// <remarks>
/// Every number is the owner's: <c>AnalyzeGasLiftPotential</c> for the recommendation, <c>DesignValvesUS</c> for the
/// expected rate and efficiency, the spacing calculator for the valve list and <c>GasLiftDesignLimits</c> for the
/// guardrails. The screen adds the explicit inputs, the units and the refusal when a limit blocks the save.
/// </remarks>
public sealed class OgGasLiftService : IOgGasLift
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgGasLiftService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgGasLiftPending? Pending { get; private set; }

    public async Task<OgResult<OgGasLiftWorkspace>> DesignAsync(
        OgGasLiftInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (GasLiftRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgGasLiftWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var source = _source();

        var portSizes = source is null
            ? GasLiftConstants.StandardPortSizes.ToList()
            : await ReadPortSizesAsync(source, cancellationToken).ConfigureAwait(false);

        if (!portSizes.Contains(inputs.PortSizeIn!.Value))
        {
            return OgResult<OgGasLiftWorkspace>.Failed(
                $"The port size {inputs.PortSizeIn.Value.ToString("0.###", CultureInfo.InvariantCulture)} in is not one " +
                $"of the reference sizes ({string.Join(", ", portSizes.Select(size => size.ToString("0.###", CultureInfo.InvariantCulture)))}).",
                GasLiftRules.CodePortUnknown);
        }

        try
        {
            var design = Build(inputs);

            var history = source is null
                ? []
                : await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            var fromReference = source is not null && await PortSizesFromReferenceAsync(source, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgGasLiftWorkspace>.Ok(
                Workspace(history, design, portSizes, fromReference));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"designing a gas lift installation for well {inputs.WellId}",
                "the design is not shown",
                FailureSeverity.Error);

            return OgResult<OgGasLiftWorkspace>.Failed(
                "The gas lift design could not be run.", reference ?? GasLiftRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgGasLiftWorkspace>> SaveAsync(
        OgGasLiftDesign design,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(design);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgGasLiftWorkspace>.Failed(
                "A saved design must record who saved it.", GasLiftRules.CodeActorRequired);
        }

        // AC-2: a blocking limit refuses the save with its own catalog message.
        if (design.BlockingLimits.Count > 0)
        {
            return OgResult<OgGasLiftWorkspace>.Failed(
                "The design cannot be saved: " +
                string.Join(" ", design.BlockingLimits.Select(limit => limit.Message)),
                GasLiftRules.CodeBlockingLimit);
        }

        if (design.Valves.Count == 0)
        {
            return OgResult<OgGasLiftWorkspace>.Failed(
                "There is no valve to save; design the installation first.", GasLiftRules.CodeNothingToSave);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgGasLiftWorkspace>.Failed(
                "No connection is open, so the design cannot be saved.", GasLiftRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var unit = OgUow.For<GAS_LIFT_DESIGN>(source);
            unit.PrimaryKey = "DESIGN_ID";

            unit.Add(new GAS_LIFT_DESIGN
            {
                DESIGN_ID = Guid.NewGuid().ToString("N"),
                WELL_UWI = design.WellId,
                DESIGN_DATE = now,
                NUMBER_OF_VALVES = design.ValveCount,
                TOTAL_GAS_INJECTION_RATE = design.InjectionRate,
                EXPECTED_PRODUCTION_RATE = design.ExpectedProductionRate,
                GAS_INJECTION_PRESSURE = design.InjectionPressure,
                SYSTEM_EFFICIENCY = design.SystemEfficiency,
                STATUS = design.HasBlockingLimit ? "BLOCKED" : "DESIGNED",
                SPACING_METHOD = design.SpacingMethod,
                PORT_SIZE_IN = design.PortSizeIn,
                INJECTION_UNIT = design.InjectionUnit,
                PRODUCTION_UNIT = design.ProductionUnit,
                VALVE_DEPTHS = string.Join("\n", design.Valves.Select(valve =>
                    valve.DepthFt.ToString("0.##", CultureInfo.InvariantCulture))),
                VALVE_PRESSURES = string.Join("\n", design.Valves.Select(valve =>
                    valve.OpeningText + " / " + valve.ClosingText)),
                LIMIT_CODES = string.Join(",", design.EvaluatedCodes),
                LIMIT_MESSAGES = design.Limits.Count == 0
                    ? null
                    : string.Join("\n", design.Limits.Select(limit => limit.Describe())),
                BLOCKING_IND = design.HasBlockingLimit ? "Y" : "N",
                RECOMMENDED_INJECTION_RATE = design.RecommendedInjectionRate,
                INPUTS_JSON = ToJson(design),
                ACTOR = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                Pending = new OgGasLiftPending(design, actor.Trim());

                return OgResult<OgGasLiftWorkspace>.Failed(
                    OgUow.Message(committed, "The design was not saved; the original is retained and can be retried."),
                    GasLiftRules.CodeWriteFailed);
            }

            Pending = null;

            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);
            var portSizes = await ReadPortSizesAsync(source, cancellationToken).ConfigureAwait(false);
            var fromReference = await PortSizesFromReferenceAsync(source, cancellationToken).ConfigureAwait(false);
            var saved = design with { Id = string.Empty, Actor = actor.Trim(), RecordedAt = now };

            return OgResult<OgGasLiftWorkspace>.Ok(Workspace(history, saved, portSizes, fromReference));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = new OgGasLiftPending(design, actor.Trim());

            var reference = _failures.ReportHandled(
                ex,
                $"saving a gas lift design for well {design.WellId}",
                "the design was not acknowledged; the original is retained and the save can be retried",
                FailureSeverity.Error);

            return OgResult<OgGasLiftWorkspace>.Failed(
                "The design was not acknowledged; the original is retained and the save can be retried.",
                reference ?? GasLiftRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgGasLiftWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgGasLiftWorkspace>.Failed(
                "There is no design waiting to be retried.", GasLiftRules.CodeNothingPending);
        }

        var retried = await SaveAsync(pending.Design, actor, cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    public async Task<OgResult<OgGasLiftWorkspace>> LoadAsync(
        string? wellId = null,
        string? designId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgGasLiftWorkspace>.Failed(
                "No connection is open, so the saved designs cannot be read.", GasLiftRules.CodeNoConnection);
        }

        try
        {
            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);
            var portSizes = await ReadPortSizesAsync(source, cancellationToken).ConfigureAwait(false);
            var fromReference = await PortSizesFromReferenceAsync(source, cancellationToken).ConfigureAwait(false);

            var selected = string.IsNullOrWhiteSpace(designId)
                ? history.FirstOrDefault(design => string.IsNullOrWhiteSpace(wellId)
                    || string.Equals(design.WellId, wellId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? history.FirstOrDefault()
                : history.FirstOrDefault(design =>
                      string.Equals(design.Id, designId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? history.FirstOrDefault();

            return OgResult<OgGasLiftWorkspace>.Ok(Workspace(history, selected, portSizes, fromReference));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the saved gas lift designs",
                "the saved designs are unavailable; new designs are still allowed",
                FailureSeverity.Degraded);

            return OgResult<OgGasLiftWorkspace>.Ok(new OgGasLiftWorkspace(
                [],
                null,
                GasLiftSpacingMethods.All,
                GasLiftConstants.StandardPortSizes.ToList(),
                false,
                true,
                $"the saved designs could not be read (reference {reference})",
                string.Empty));
        }
    }

    // ------------------------------------------------------------------ the design

    /// <summary>
    /// Runs the owner's three calculators and the limit evaluation. A blocking limit does not stop the design - the
    /// engineer sees the numbers and the reason - it stops the save (AC-2).
    /// </summary>
    public static OgGasLiftDesign Build(OgGasLiftInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var well = new GAS_LIFT_WELL_PROPERTIES
        {
            GAS_LIFT_WELL_PROPERTIES_ID = Guid.NewGuid().ToString("N"),
            WELL_UWI = inputs.WellId!.Trim(),
            WELL_DEPTH = inputs.WellDepthFt!.Value,
            WELLHEAD_PRESSURE = inputs.WellheadPressure!.Value,
            BOTTOM_HOLE_PRESSURE = inputs.BottomHolePressure!.Value,
            RESERVOIR_PRESSURE = inputs.ReservoirPressure!.Value,
            GAS_OIL_RATIO = inputs.GasOilRatio!.Value,
            DESIRED_PRODUCTION_RATE = inputs.DesiredProductionRate!.Value,
            OIL_GRAVITY = inputs.OilGravity!.Value,
            WATER_CUT = inputs.WaterCut!.Value,
            WELLHEAD_TEMPERATURE = inputs.WellheadTemperature!.Value,
            BOTTOM_HOLE_TEMPERATURE = inputs.BottomHoleTemperature!.Value,
            GAS_SPECIFIC_GRAVITY = inputs.GasSpecificGravity!.Value,
            CURRENT_PRODUCTION_RATE = inputs.DesiredProductionRate.Value,
            TUBING_DIAMETER = 0,
        };

        var injectionPressure = inputs.InjectionPressure!.Value;
        var injectionRate = inputs.InjectionRate!.Value;

        // The engine places at most its own maximum, so the count it is given is bounded; the limits are evaluated
        // against the count the engineer asked for, so a request outside the range is reported (and blocks the save)
        // instead of being quietly reduced to one the module accepts.
        var requested = inputs.Valves;
        var valves = Math.Clamp(inputs.Valves, GasLiftConstants.MinimumNumberOfValves,
            GasLiftConstants.MaximumNumberOfValves);

        // The potential analysis recommends an injection rate over the module's own range.
        var potential = GasLiftPotentialCalculator.AnalyzeGasLiftPotential(
            well,
            GasLiftConstants.MinimumGasInjectionRate,
            GasLiftConstants.MaximumGasInjectionRate,
            numberOfPoints: 20);

        var design = GasLiftValveDesignCalculator.DesignValvesUS(well, injectionPressure, valves);

        var spacing = string.Equals(inputs.SpacingMethod!.Trim(), GasLiftSpacingMethods.EqualPressureDrop,
            StringComparison.OrdinalIgnoreCase)
            ? GasLiftValveSpacingCalculator.CalculateEqualPressureDropSpacing(well, injectionPressure, valves)
            : GasLiftValveSpacingCalculator.CalculateEqualDepthSpacing(well, injectionPressure, valves);

        var placed = new List<OgGasLiftValve>();

        for (var index = 0; index < spacing.ValveDepths.Count; index++)
        {
            var opening = index < spacing.OpeningPressures.Count ? spacing.OpeningPressures[index] : 0m;

            placed.Add(new OgGasLiftValve(
                index + 1,
                spacing.ValveDepths[index],
                opening,
                opening * GasLiftConstants.StandardClosingPressureRatio,
                index == 0
                    ? "UNLOADING"
                    : index == spacing.ValveDepths.Count - 1
                        ? "OPERATING"
                        : "INTERMEDIATE"));
        }

        var limits = GasLiftDesignLimits.Evaluate(injectionRate, requested, spacing.ValveDepths)
            .Select(limit => new OgGasLiftLimit(limit.Code, limit.Message, limit.Blocking))
            .ToList();

        return new OgGasLiftDesign(
            string.Empty,
            inputs.WellId!.Trim(),
            inputs.SpacingMethod!.Trim().ToUpperInvariant(),
            inputs.PortSizeIn,
            injectionRate,
            injectionPressure,
            spacing.ValveDepths.Count,
            design.EXPECTED_PRODUCTION_RATE,
            design.SYSTEM_EFFICIENCY,
            potential.OPTIMAL_GAS_INJECTION_RATE > 0m ? potential.OPTIMAL_GAS_INJECTION_RATE : null,
            potential.MAXIMUM_PRODUCTION_RATE > 0m ? potential.MAXIMUM_PRODUCTION_RATE : null,
            potential.OPTIMAL_GAS_LIQUID_RATIO > 0m ? potential.OPTIMAL_GAS_LIQUID_RATIO : null,
            placed,
            limits,
            GasLiftDesignLimits.EvaluatedCodes,
            OgGasLiftDesign.InjectionUnitName,
            OgGasLiftDesign.ProductionUnitName,
            null,
            null);
    }

    // ------------------------------------------------------------------ the reads

    private static async Task<IReadOnlyList<decimal>> ReadPortSizesAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var reference = await OgUow.ReadAllAsync<R_GAS_LIFT_REFERENCE_CODE>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var sizes = reference
            .Where(row => string.Equals(row.REFERENCE_SET, GasLiftReferenceSets.PortSizeIn,
                StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.IsNullOrWhiteSpace(row.REFERENCE_CODE))
            .Select(row => decimal.TryParse(row.REFERENCE_CODE, NumberStyles.Any, CultureInfo.InvariantCulture,
                out var size)
                ? (decimal?)size
                : null)
            .Where(size => size is not null)
            .Select(size => size!.Value)
            .Distinct()
            .OrderBy(size => size)
            .ToList();

        return sizes.Count == 0 ? GasLiftConstants.StandardPortSizes.ToList() : sizes;
    }

    private static async Task<bool> PortSizesFromReferenceAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var reference = await OgUow.ReadAllAsync<R_GAS_LIFT_REFERENCE_CODE>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return reference.Any(row => string.Equals(row.REFERENCE_SET, GasLiftReferenceSets.PortSizeIn,
            StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<IReadOnlyList<OgGasLiftDesign>> ReadHistoryAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAllAsync<GAS_LIFT_DESIGN>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.DESIGN_ID))
            .OrderByDescending(row => row.RECORDED_AT ?? row.DESIGN_DATE ?? DateTime.MinValue)
            .ThenBy(row => row.DESIGN_ID, StringComparer.Ordinal)
            .Select(ToDesign)
            .Take(50)
            .ToList();
    }

    private static OgGasLiftDesign ToDesign(GAS_LIFT_DESIGN row)
    {
        var depths = Split(row.VALVE_DEPTHS);
        var pressures = Lines(row.VALVE_PRESSURES);

        var valves = depths
            .Select((depth, index) =>
            {
                var pair = index < pressures.Count ? pressures[index] : string.Empty;
                var parts = pair.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                decimal Read(string? text) => decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture,
                    out var value)
                    ? value
                    : 0m;

                return new OgGasLiftValve(
                    index + 1,
                    depth,
                    Read(parts.ElementAtOrDefault(0)),
                    Read(parts.ElementAtOrDefault(1)),
                    index == 0
                        ? "UNLOADING"
                        : index == depths.Count - 1
                            ? "OPERATING"
                            : "INTERMEDIATE");
            })
            .ToList();

        var limits = string.IsNullOrWhiteSpace(row.LIMIT_MESSAGES)
            ? []
            : row.LIMIT_MESSAGES!
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(line =>
                {
                    var colon = line.IndexOf(':');
                    var head = colon > 0 ? line[..colon] : line;
                    var message = colon > 0 ? line[(colon + 1)..].Trim() : line;
                    var blocking = head.Contains("(blocking", StringComparison.OrdinalIgnoreCase);
                    var code = head.Split(' ')[0].ToUpperInvariant();

                    return new OgGasLiftLimit(code, message, blocking);
                })
                .ToList();

        return new OgGasLiftDesign(
            row.DESIGN_ID ?? string.Empty,
            row.WELL_UWI,
            row.SPACING_METHOD ?? GasLiftSpacingMethods.EqualDepth,
            row.PORT_SIZE_IN,
            row.TOTAL_GAS_INJECTION_RATE ?? 0m,
            row.GAS_INJECTION_PRESSURE,
            row.NUMBER_OF_VALVES ?? 0,
            row.EXPECTED_PRODUCTION_RATE ?? 0m,
            row.SYSTEM_EFFICIENCY,
            row.RECOMMENDED_INJECTION_RATE,
            null,
            null,
            valves,
            limits,
            string.IsNullOrWhiteSpace(row.LIMIT_CODES)
                ? []
                : row.LIMIT_CODES!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            row.INJECTION_UNIT ?? OgGasLiftDesign.InjectionUnitName,
            row.PRODUCTION_UNIT ?? OgGasLiftDesign.ProductionUnitName,
            row.ACTOR,
            row.RECORDED_AT ?? row.DESIGN_DATE);
    }

    private static IReadOnlyList<decimal> Split(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? []
            : text!
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(line => decimal.TryParse(line, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
                    ? value
                    : 0m)
                .ToList();

    private static IReadOnlyList<string> Lines(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? []
            : text!.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static OgGasLiftWorkspace Workspace(
        IReadOnlyList<OgGasLiftDesign> history,
        OgGasLiftDesign? selected,
        IReadOnlyList<decimal> portSizes,
        bool fromReference) =>
        new(
            history,
            selected,
            GasLiftSpacingMethods.All,
            portSizes,
            fromReference,
            false,
            null,
            portSizes.Count == 0
                ? "No valve port size is on offer, so a design cannot be saved. The reference set " +
                  "GAS_LIFT_PORT_SIZE_IN is empty and so are the module's standard sizes."
                : string.Empty);

    private static string ToJson(OgGasLiftDesign design) =>
        JsonSerializer.Serialize(new
        {
            design.WellId,
            design.SpacingMethod,
            design.PortSizeIn,
            design.InjectionRate,
            design.InjectionPressure,
            design.ValveCount,
            design.ExpectedProductionRate,
            design.SystemEfficiency,
            design.RecommendedInjectionRate,
            Limits = design.Limits.Select(limit => limit.Code).ToList(),
        });
}
