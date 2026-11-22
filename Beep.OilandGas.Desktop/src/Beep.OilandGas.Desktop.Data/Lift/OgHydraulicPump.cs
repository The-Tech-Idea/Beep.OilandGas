using System.Globalization;
using Beep.OilandGas.HydraulicPumps.Calculations;
using Beep.OilandGas.HydraulicPumps.Constants;
using Beep.OilandGas.HydraulicPumps.Validation;
using Beep.OilandGas.Models.Data.HydraulicPumps;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;

/// <summary>The two hydraulic pump kinds (LFT-06).</summary>
public static class HydraulicPumpKinds
{
    public const string Jet = "JET";
    public const string Piston = "PISTON";

    public static IReadOnlyList<string> All { get; } = [Jet, Piston];

    public static bool IsSupported(string? kind) =>
        !string.IsNullOrWhiteSpace(kind)
        && All.Contains(kind.Trim().ToUpperInvariant(), StringComparer.Ordinal);

    public static string NameOf(string kind) => kind.Trim().ToUpperInvariant() switch
    {
        Jet => "Hydraulic jet pump",
        Piston => "Hydraulic piston pump",
        _ => kind,
    };
}

/// <summary>One finding, as the page lists it (LFT-06 AC-3, AC-4).</summary>
public sealed record OgHydraulicFinding(string Code, string Message, string Severity)
{
    public bool Blocking => string.Equals(Severity, "Error", StringComparison.OrdinalIgnoreCase);

    public string Describe() => $"{Severity.ToLowerInvariant()} · {Code.ToLowerInvariant()}: {Message}";
}

/// <summary>The inputs of one hydraulic pump design (LFT-06). Every value the chosen kind needs is required.</summary>
public sealed record OgHydraulicInputs(
    string? WellId,
    string? Kind,
    decimal? WellDepthFt,
    decimal? TubingDiameterIn,
    decimal? CasingDiameterIn,
    decimal? WellheadPressure,
    decimal? BottomHolePressure,
    decimal? WellheadTemperature,
    decimal? BottomHoleTemperature,
    decimal? OilGravity,
    decimal? WaterCut,
    decimal? GasOilRatio,
    decimal? GasSpecificGravity,
    decimal? DesiredRate,
    decimal? PumpDepth,
    decimal? NozzleDiameter,
    decimal? ThroatDiameter,
    decimal? DiffuserDiameter,
    decimal? PistonDiameter,
    decimal? RodDiameter,
    decimal? StrokeLength,
    decimal? StrokesPerMinute,
    decimal? PowerFluidPressure,
    decimal? PowerFluidRate,
    decimal? PowerFluidSpecificGravity);

/// <summary>
/// One hydraulic pump design (LFT-06), matching the plan's <c>OgHydraulicPumpDesign</c> plus the index and the findings.
/// </summary>
public sealed record OgHydraulicDesign(
    string Id,
    string? WellId,
    string Kind,
    OgHydraulicInputs Inputs,
    decimal ProductionRate,
    decimal TotalFlowRate,
    decimal? ProductionRatio,
    decimal PumpEfficiency,
    decimal SystemEfficiency,
    decimal? RawSystemEfficiency,
    decimal PowerFluidHorsepower,
    decimal HydraulicHorsepower,
    decimal IntakePressure,
    decimal DischargePressure,
    decimal? Displacement,
    decimal? AreaRatio,
    decimal CavitationAvailable,
    decimal CavitationRequired,
    decimal CavitationMargin,
    bool CavitationRisk,
    IReadOnlyList<OgHydraulicFinding> Findings,
    string? Actor,
    DateTime? RecordedAt)
{
    public const string RateUnit = "bbl/d";
    public const string PressureUnit = "psia";
    public const string PowerUnit = "hp";

    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    public string KindText => HydraulicPumpKinds.NameOf(Kind);

    public IReadOnlyList<OgHydraulicFinding> Blocking =>
        Findings.Where(finding => finding.Blocking).ToList();

    public bool HasBlockingFinding => Blocking.Count > 0;

    public bool HasWarning => Findings.Any(finding =>
        string.Equals(finding.Severity, "Warning", StringComparison.OrdinalIgnoreCase));

    /// <summary>AC-3: the engine clamped the system efficiency rather than reporting what it computed.</summary>
    public bool EfficiencyClamped =>
        RawSystemEfficiency is { } raw && raw != SystemEfficiency;

    public string ProductionText =>
        ProductionRate.ToString("#,0.###", CultureInfo.InvariantCulture) + " " + RateUnit;

    public string PowerFluidText =>
        Inputs.PowerFluidRate is { } rate
            ? rate.ToString("#,0.###", CultureInfo.InvariantCulture) + " " + RateUnit + " at " +
              (Inputs.PowerFluidPressure?.ToString("#,0.##", CultureInfo.InvariantCulture) ?? "0") + " " + PressureUnit
            : "no power fluid rate";

    public string EfficiencyText =>
        "pump " + (PumpEfficiency * 100m).ToString("0.##", CultureInfo.InvariantCulture) + " % · system " +
        (SystemEfficiency * 100m).ToString("0.##", CultureInfo.InvariantCulture) + " %";

    /// <summary>AC-3: when the raw ratio left the range, both numbers are said out loud.</summary>
    public string ClampText => RawSystemEfficiency is not { } raw
        ? "the system efficiency is the engine's own ratio"
        : EfficiencyClamped
            ? $"the engine computed {raw.ToString("#,0.####", CultureInfo.InvariantCulture)} and clamped it to " +
              $"{SystemEfficiency.ToString("#,0.####", CultureInfo.InvariantCulture)}; the clamped value is what the " +
              "pump delivers"
            : $"the ratio {raw.ToString("#,0.####", CultureInfo.InvariantCulture)} is inside 0 to 1 and was not clamped";

    /// <summary>AC-2: the index and its threshold, not just the word 'cavitation'.</summary>
    public string CavitationText =>
        $"available NPSH {CavitationAvailable.ToString("#,0.##", CultureInfo.InvariantCulture)} {PressureUnit} against " +
        $"the {CavitationRequired.ToString("#,0.##", CultureInfo.InvariantCulture)} {PressureUnit} required: margin " +
        $"{CavitationMargin.ToString("#,0.##", CultureInfo.InvariantCulture)} {PressureUnit} " +
        (CavitationRisk ? "(cavitation predicted)" : "(no cavitation)");

    public string JetText => Kind != HydraulicPumpKinds.Jet
        ? "not a jet pump"
        : $"nozzle {Text(Inputs.NozzleDiameter)} in · throat {Text(Inputs.ThroatDiameter)} in · area ratio " +
          $"{Text(AreaRatio)} · production ratio {Text(ProductionRatio)}";

    public string PistonText => Kind != HydraulicPumpKinds.Piston
        ? "not a piston pump"
        : $"piston {Text(Inputs.PistonDiameter)} in · rod {Text(Inputs.RodDiameter)} in · stroke " +
          $"{Text(Inputs.StrokeLength)} in at {Text(Inputs.StrokesPerMinute)} spm · displacement {Text(Displacement)} " +
          RateUnit;

    public string FindingsText => Findings.Count == 0
        ? "no finding: every rule passed"
        : string.Join(Environment.NewLine, Findings.Select(finding => finding.Describe()));

    public string WorstSeverity => Findings.Count == 0
        ? "Info"
        : Findings.Max(finding => finding.Severity switch
        {
            "Error" => 2,
            "Warning" => 1,
            _ => 0,
        }) switch
        {
            2 => "Error",
            1 => "Warning",
            _ => "Info",
        };

    public string SavedText => IsSaved
        ? $"saved by {Actor ?? "(unknown)"} on " +
          $"{RecordedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date"}"
        : "not saved";

    public string Describe() =>
        $"{WellId ?? "(no well)"} · {KindText} · {ProductionText} · {EfficiencyText} · " +
        $"{Findings.Count.ToString(CultureInfo.InvariantCulture)} finding(s) · {SavedText}";

    private static string Text(decimal? value) =>
        value?.ToString("#,0.###", CultureInfo.InvariantCulture) ?? "(none)";

    private static string Text(decimal value) =>
        value.ToString("#,0.####", CultureInfo.InvariantCulture);
}

/// <summary>A design whose save was not acknowledged: retained and retried (LFT-06's state table).</summary>
public sealed record OgHydraulicPending(OgHydraulicDesign Design, string Actor);

/// <summary>The hydraulic pump workspace (LFT-06).</summary>
public sealed record OgHydraulicWorkspace(
    IReadOnlyList<OgHydraulicDesign> Designs,
    OgHydraulicDesign? Selected,
    IReadOnlyList<string> Kinds,
    IReadOnlyList<decimal> StandardNozzleDiameters,
    IReadOnlyList<decimal> StandardThroatDiameters,
    IReadOnlyList<decimal> StandardPistonDiameters,
    IReadOnlyList<decimal> StandardStrokeLengths,
    IReadOnlyList<decimal> StandardStrokesPerMinute,
    decimal RequiredNpshPsi,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string DesignsText => Designs.Count == 0
        ? "No hydraulic pump design is saved yet."
        : string.Join(Environment.NewLine, Designs.Select(design => design.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved designs could not be read; new designs are still allowed"
        : $"{Designs.Count.ToString(CultureInfo.InvariantCulture)} saved design(s)";

    public string ReferenceText =>
        $"nozzles {string.Join(", ", StandardNozzleDiameters.Select(Text))} in · throats " +
        $"{string.Join(", ", StandardThroatDiameters.Select(Text))} in · pistons " +
        $"{string.Join(", ", StandardPistonDiameters.Select(Text))} in · required NPSH " +
        $"{RequiredNpshPsi.ToString("#,0.##", CultureInfo.InvariantCulture)} psi";

    private static string Text(decimal value) =>
        value.ToString("0.###", CultureInfo.InvariantCulture);
}

/// <summary>The hydraulic pump rules, stated once so the page, the service and the tests agree.</summary>
public static class HydraulicRules
{
    public const string CodeNoConnection = "hydraulic-no-connection";
    public const string CodeReadFailed = "hydraulic-read-failed";
    public const string CodeWriteFailed = "hydraulic-write-failed";
    public const string CodeActorRequired = "hydraulic-actor-required";
    public const string CodeWellRequired = "hydraulic-well-required";
    public const string CodeInputRequired = "hydraulic-input-required";
    public const string CodeKindUnknown = "hydraulic-kind-unknown";
    public const string CodeNozzleNotStandard = "hydraulic-nozzle-not-standard";
    public const string CodeThroatNotStandard = "hydraulic-throat-not-standard";
    public const string CodePistonNotStandard = "hydraulic-piston-not-standard";
    public const string CodeStrokeNotStandard = "hydraulic-stroke-not-standard";
    public const string CodeInvalidCombination = "hydraulic-invalid-combination";
    public const string CodeBlockingFinding = "hydraulic-blocking-finding";
    public const string CodeNothingToSave = "hydraulic-nothing-to-save";
    public const string CodeNothingPending = "hydraulic-nothing-pending";

    /// <summary>The NPSH the module assumes, from the owner rather than written here (AC-2).</summary>
    public static decimal RequiredNpshPsi => HydraulicPumpFindings.DefaultRequiredNpshPsi;

    public static IReadOnlyList<string> Missing(OgHydraulicInputs inputs)
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
        Need(inputs.TubingDiameterIn, "tubing diameter");
        Need(inputs.CasingDiameterIn, "casing diameter");
        Need(inputs.WellheadPressure, "wellhead pressure");
        Need(inputs.BottomHolePressure, "bottom hole pressure");
        Need(inputs.WellheadTemperature, "wellhead temperature");
        Need(inputs.BottomHoleTemperature, "bottom hole temperature");
        Need(inputs.OilGravity, "oil gravity");
        Need(inputs.WaterCut, "water cut");
        Need(inputs.GasOilRatio, "gas-oil ratio");
        Need(inputs.GasSpecificGravity, "gas specific gravity");
        Need(inputs.DesiredRate, "desired production rate");
        Need(inputs.PumpDepth, "pump depth");

        // The state table: "missing power fluid pressure - refused with the reason".
        Need(inputs.PowerFluidPressure, "power fluid pressure");
        Need(inputs.PowerFluidRate, "power fluid rate");
        Need(inputs.PowerFluidSpecificGravity, "power fluid specific gravity");

        if (HydraulicPumpKinds.IsSupported(inputs.Kind) && IsJet(inputs.Kind))
        {
            Need(inputs.NozzleDiameter, "nozzle diameter");
            Need(inputs.ThroatDiameter, "throat diameter");
        }

        if (HydraulicPumpKinds.IsSupported(inputs.Kind) && !IsJet(inputs.Kind))
        {
            Need(inputs.PistonDiameter, "piston diameter");
            Need(inputs.RodDiameter, "rod diameter");
            Need(inputs.StrokeLength, "stroke length");
            Need(inputs.StrokesPerMinute, "strokes per minute");
        }

        if (string.IsNullOrWhiteSpace(inputs.Kind))
        {
            missing.Add("pump kind");
        }

        return missing;
    }

    public static (string Code, string Message)? Validate(OgHydraulicInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            return missing.Contains("well")
                ? (CodeWellRequired, "Choose the well the design is for.")
                : missing.Contains("power fluid pressure")
                    ? (CodeInputRequired,
                        "The design needs the power fluid pressure: without it there is no power fluid horsepower to " +
                        "compare the hydraulic horsepower against.")
                    : (CodeInputRequired, $"The design needs {string.Join(", ", missing)}.");
        }

        if (!HydraulicPumpKinds.IsSupported(inputs.Kind))
        {
            return (CodeKindUnknown,
                $"'{inputs.Kind}' is not a hydraulic pump kind; use {string.Join(", ", HydraulicPumpKinds.All)}.");
        }

        if (inputs.WaterCut is < 0m or > 1m)
        {
            return (CodeInputRequired, "The water cut is a fraction between 0 and 1.");
        }

        return null;
    }

    /// <summary>Whether the kind is the jet pump (null-safe).</summary>
    public static bool IsJet(string? kind) =>
        string.Equals(kind?.Trim(), HydraulicPumpKinds.Jet, StringComparison.OrdinalIgnoreCase);
}

/// <summary>LFT-06: hydraulic pumps.</summary>
public interface IOgHydraulicPump
{
    /// <summary>Designs and checks the pump. Nothing is written.</summary>
    Task<OgResult<OgHydraulicWorkspace>> DesignAsync(
        OgHydraulicInputs inputs,
        CancellationToken cancellationToken = default);

    /// <summary>Saves a design whose findings are not blocking.</summary>
    Task<OgResult<OgHydraulicWorkspace>> SaveAsync(
        OgHydraulicDesign design,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The saved designs and the standard sizes; an unavailable history is a state.</summary>
    Task<OgResult<OgHydraulicWorkspace>> LoadAsync(
        string? wellId = null,
        string? designId = null,
        CancellationToken cancellationToken = default);

    /// <summary>The design retained when its save was not acknowledged.</summary>
    OgHydraulicPending? Pending { get; }

    /// <summary>Retries the same insert.</summary>
    Task<OgResult<OgHydraulicWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// LFT-06 over the HydraulicPumps module's jet and piston calculators, its findings catalog and the
/// <c>HYDRAULIC_PUMP_DESIGN</c> row, through a unit of work.
/// </summary>
/// <remarks>
/// Every number is the owner's: <c>HydraulicJetPumpCalculator</c> or <c>HydraulicPistonPumpCalculator</c> for the result,
/// <c>HydraulicPumpCalculator.TryCalculateJetPumpPerformance</c> for the unclamped jet efficiency, and
/// <c>HydraulicPumpFindings</c> for every severity and sentence. The screen adds the standard-size check, the refusal of a
/// blocking finding and the record.
/// </remarks>
public sealed class OgHydraulicPumpService : IOgHydraulicPump
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgHydraulicPumpService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgHydraulicPending? Pending { get; private set; }

    public async Task<OgResult<OgHydraulicWorkspace>> DesignAsync(
        OgHydraulicInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (HydraulicRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgHydraulicWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        // AC-5: the nozzle and throat (or the piston and stroke) come from the standard sets.
        if (HydraulicRules.IsJet(inputs.Kind))
        {
            if (!HydraulicPumpConstants.StandardNozzleDiameters.Contains(inputs.NozzleDiameter!.Value))
            {
                return OgResult<OgHydraulicWorkspace>.Failed(
                    $"The nozzle {Number(inputs.NozzleDiameter.Value)} in is not a standard size " +
                    $"({Sizes(HydraulicPumpConstants.StandardNozzleDiameters)}).",
                    HydraulicRules.CodeNozzleNotStandard);
            }

            if (!HydraulicPumpConstants.StandardThroatDiameters.Contains(inputs.ThroatDiameter!.Value))
            {
                return OgResult<OgHydraulicWorkspace>.Failed(
                    $"The throat {Number(inputs.ThroatDiameter.Value)} in is not a standard size " +
                    $"({Sizes(HydraulicPumpConstants.StandardThroatDiameters)}).",
                    HydraulicRules.CodeThroatNotStandard);
            }
        }
        else
        {
            if (!HydraulicPumpConstants.StandardPistonDiameters.Contains(inputs.PistonDiameter!.Value))
            {
                return OgResult<OgHydraulicWorkspace>.Failed(
                    $"The piston {Number(inputs.PistonDiameter.Value)} in is not a standard size " +
                    $"({Sizes(HydraulicPumpConstants.StandardPistonDiameters)}).",
                    HydraulicRules.CodePistonNotStandard);
            }

            if (!HydraulicPumpConstants.StandardStrokeLengths.Contains(inputs.StrokeLength!.Value))
            {
                return OgResult<OgHydraulicWorkspace>.Failed(
                    $"The stroke {Number(inputs.StrokeLength.Value)} in is not a standard length " +
                    $"({Sizes(HydraulicPumpConstants.StandardStrokeLengths)}).",
                    HydraulicRules.CodeStrokeNotStandard);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var design = Build(inputs);

            // The state table's "invalid combination of nozzle/throat is refused by the validator": an error finding
            // means the engine's numbers came from an input it would not accept, so they are not presented as a design.
            if (design.HasBlockingFinding)
            {
                return OgResult<OgHydraulicWorkspace>.Failed(
                    "The design was refused: " +
                    string.Join(" · ", design.Blocking.Select(finding =>
                        finding.Severity.ToLowerInvariant() + " " + finding.Code.ToLowerInvariant() + ": " +
                        finding.Message)),
                    HydraulicRules.CodeInvalidCombination);
            }

            var source = _source();
            var history = source is null
                ? []
                : await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgHydraulicWorkspace>.Ok(Workspace(history, design));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"designing a hydraulic pump for well {inputs.WellId}",
                "the design is not shown",
                FailureSeverity.Error);

            return OgResult<OgHydraulicWorkspace>.Failed(
                "The hydraulic pump design could not be run.", reference ?? HydraulicRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgHydraulicWorkspace>> SaveAsync(
        OgHydraulicDesign design,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(design);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgHydraulicWorkspace>.Failed(
                "A saved design must record who saved it.", HydraulicRules.CodeActorRequired);
        }

        // An error finding is a refused input: it cannot be stored as a design.
        if (design.HasBlockingFinding)
        {
            return OgResult<OgHydraulicWorkspace>.Failed(
                "The design cannot be saved: " +
                string.Join(" ", design.Blocking.Select(finding => finding.Message)),
                HydraulicRules.CodeBlockingFinding);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgHydraulicWorkspace>.Failed(
                "No connection is open, so the design cannot be saved.", HydraulicRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var unit = OgUow.For<HYDRAULIC_PUMP_DESIGN>(source);
            unit.PrimaryKey = "DESIGN_ID";

            unit.Add(new HYDRAULIC_PUMP_DESIGN
            {
                DESIGN_ID = Guid.NewGuid().ToString("N"),
                WELL_UWI = design.WellId,
                DESIGN_DATE = now,
                KIND = design.Kind,
                WELL_DEPTH_FT = design.Inputs.WellDepthFt!.Value,
                TUBING_DIAMETER_IN = design.Inputs.TubingDiameterIn!.Value,
                CASING_DIAMETER_IN = design.Inputs.CasingDiameterIn!.Value,
                WELLHEAD_PRESSURE_PSIA = design.Inputs.WellheadPressure!.Value,
                BOTTOM_HOLE_PRESSURE_PSIA = design.Inputs.BottomHolePressure!.Value,
                WELLHEAD_TEMPERATURE_R = design.Inputs.WellheadTemperature!.Value,
                BOTTOM_HOLE_TEMPERATURE_R = design.Inputs.BottomHoleTemperature!.Value,
                OIL_GRAVITY_API = design.Inputs.OilGravity!.Value,
                WATER_CUT = design.Inputs.WaterCut!.Value,
                GAS_OIL_RATIO_SCF_STB = design.Inputs.GasOilRatio!.Value,
                GAS_SPECIFIC_GRAVITY = design.Inputs.GasSpecificGravity!.Value,
                DESIRED_PRODUCTION_RATE_BBLD = design.Inputs.DesiredRate!.Value,
                PUMP_DEPTH_FT = design.Inputs.PumpDepth!.Value,
                NOZZLE_DIAMETER_IN = design.Inputs.NozzleDiameter,
                THROAT_DIAMETER_IN = design.Inputs.ThroatDiameter,
                DIFFUSER_DIAMETER_IN = design.Inputs.DiffuserDiameter,
                AREA_RATIO = design.AreaRatio,
                PISTON_DIAMETER_IN = design.Inputs.PistonDiameter,
                ROD_DIAMETER_IN = design.Inputs.RodDiameter,
                STROKE_LENGTH_IN = design.Inputs.StrokeLength,
                STROKES_PER_MINUTE = design.Inputs.StrokesPerMinute,
                POWER_FLUID_PRESSURE_PSIA = design.Inputs.PowerFluidPressure!.Value,
                POWER_FLUID_RATE_BBLD = design.Inputs.PowerFluidRate!.Value,
                POWER_FLUID_SPECIFIC_GRAVITY = design.Inputs.PowerFluidSpecificGravity!.Value,
                PRODUCTION_RATE_BBLD = design.ProductionRate,
                TOTAL_FLOW_RATE_BBLD = design.TotalFlowRate,
                PRODUCTION_RATIO = design.ProductionRatio,
                PUMP_EFFICIENCY = design.PumpEfficiency,
                SYSTEM_EFFICIENCY = design.SystemEfficiency,
                RAW_SYSTEM_EFFICIENCY = design.RawSystemEfficiency,
                POWER_FLUID_HORSEPOWER = design.PowerFluidHorsepower,
                HYDRAULIC_HORSEPOWER = design.HydraulicHorsepower,
                PUMP_INTAKE_PRESSURE_PSIA = design.IntakePressure,
                PUMP_DISCHARGE_PRESSURE_PSIA = design.DischargePressure,
                PUMP_DISPLACEMENT_BBLD = design.Displacement,
                CAVITATION_AVAILABLE_PSI = design.CavitationAvailable,
                CAVITATION_REQUIRED_PSI = design.CavitationRequired,
                CAVITATION_MARGIN_PSI = design.CavitationMargin,
                CAVITATION_RISK_IND = design.CavitationRisk ? "Y" : "N",
                FINDINGS_TEXT = design.Findings.Count == 0
                    ? null
                    : string.Join("\n", design.Findings.Select(finding => finding.Describe())),
                WORST_SEVERITY = design.WorstSeverity.ToUpperInvariant(),
                RATE_OUOM = OgHydraulicDesign.RateUnit,
                PRESSURE_OUOM = OgHydraulicDesign.PressureUnit,
                POWER_OUOM = OgHydraulicDesign.PowerUnit,
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
                Pending = new OgHydraulicPending(design, actor.Trim());

                return OgResult<OgHydraulicWorkspace>.Failed(
                    OgUow.Message(committed, "The design was not saved; the original is retained and can be retried."),
                    HydraulicRules.CodeWriteFailed);
            }

            Pending = null;

            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);
            var saved = design with { Id = string.Empty, Actor = actor.Trim(), RecordedAt = now };

            return OgResult<OgHydraulicWorkspace>.Ok(Workspace(history, saved));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = new OgHydraulicPending(design, actor.Trim());

            var reference = _failures.ReportHandled(
                ex,
                $"saving a hydraulic pump design for well {design.WellId}",
                "the design was not acknowledged; the original is retained and the save can be retried",
                FailureSeverity.Error);

            return OgResult<OgHydraulicWorkspace>.Failed(
                "The design was not acknowledged; the original is retained and the save can be retried.",
                reference ?? HydraulicRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgHydraulicWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgHydraulicWorkspace>.Failed(
                "There is no design waiting to be retried.", HydraulicRules.CodeNothingPending);
        }

        var retried = await SaveAsync(pending.Design, actor, cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    public async Task<OgResult<OgHydraulicWorkspace>> LoadAsync(
        string? wellId = null,
        string? designId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgHydraulicWorkspace>.Failed(
                "No connection is open, so the saved designs cannot be read.", HydraulicRules.CodeNoConnection);
        }

        try
        {
            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            var selected = string.IsNullOrWhiteSpace(designId)
                ? history.FirstOrDefault(design => string.IsNullOrWhiteSpace(wellId)
                    || string.Equals(design.WellId, wellId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? history.FirstOrDefault()
                : history.FirstOrDefault(design =>
                      string.Equals(design.Id, designId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? history.FirstOrDefault();

            return OgResult<OgHydraulicWorkspace>.Ok(Workspace(history, selected));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the saved hydraulic pump designs",
                "the saved designs are unavailable; new designs are still allowed",
                FailureSeverity.Degraded);

            return OgResult<OgHydraulicWorkspace>.Ok(new OgHydraulicWorkspace(
                [],
                null,
                HydraulicPumpKinds.All,
                HydraulicPumpConstants.StandardNozzleDiameters,
                HydraulicPumpConstants.StandardThroatDiameters,
                HydraulicPumpConstants.StandardPistonDiameters,
                HydraulicPumpConstants.StandardStrokeLengths,
                HydraulicPumpConstants.StandardSPM,
                HydraulicRules.RequiredNpshPsi,
                true,
                $"the saved designs could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the design

    /// <summary>
    /// Runs the owner's calculator for the chosen kind and its findings catalog. A blocking finding is returned as a
    /// design the caller can see; only the save refuses it, so the engineer reads the reason.
    /// </summary>
    public static OgHydraulicDesign Build(OgHydraulicInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var kind = inputs.Kind!.Trim().ToUpperInvariant();

        var well = new HYDRAULIC_PUMP_WELL_PROPERTIES
        {
            HYDRAULIC_PUMP_WELL_PROPERTIES_ID = Guid.NewGuid().ToString("N"),
            WELL_UWI = inputs.WellId!.Trim(),
            WELL_DEPTH = inputs.WellDepthFt!.Value,
            TUBING_DIAMETER = inputs.TubingDiameterIn!.Value,
            CASING_DIAMETER = inputs.CasingDiameterIn!.Value,
            WELLHEAD_PRESSURE = inputs.WellheadPressure!.Value,
            BOTTOM_HOLE_PRESSURE = inputs.BottomHolePressure!.Value,
            WELLHEAD_TEMPERATURE = inputs.WellheadTemperature!.Value,
            BOTTOM_HOLE_TEMPERATURE = inputs.BottomHoleTemperature!.Value,
            OIL_GRAVITY = inputs.OilGravity!.Value,
            WATER_CUT = inputs.WaterCut!.Value,
            GAS_OIL_RATIO = inputs.GasOilRatio!.Value,
            GAS_SPECIFIC_GRAVITY = inputs.GasSpecificGravity!.Value,
            DESIRED_PRODUCTION_RATE = inputs.DesiredRate!.Value,
            PUMP_DEPTH = inputs.PumpDepth!.Value,
        };

        if (HydraulicRules.IsJet(kind))
        {
            var jet = new HYDRAULIC_JET_PUMP_PROPERTIES
            {
                HYDRAULIC_JET_PUMP_PROPERTIES_ID = Guid.NewGuid().ToString("N"),
                NOZZLE_DIAMETER = inputs.NozzleDiameter!.Value,
                THROAT_DIAMETER = inputs.ThroatDiameter!.Value,
                DIFFUSER_DIAMETER = inputs.DiffuserDiameter ?? 0m,
                POWER_FLUID_PRESSURE = inputs.PowerFluidPressure!.Value,
                POWER_FLUID_RATE = inputs.PowerFluidRate!.Value,
                POWER_FLUID_SPECIFIC_GRAVITY = inputs.PowerFluidSpecificGravity!.Value,
                PUMP_DEPTH = inputs.PumpDepth.Value,
            };

            // The owner validates and computes; a refused combination throws its own exception type.
            var result = HydraulicJetPumpCalculator.CalculatePerformance(well, jet);

            // The unclamped ratio, from the same expression the calculator clamps (AC-3).
            decimal? raw = result.POWER_FLUID_HORSEPOWER > 0m
                ? result.HYDRAULIC_HORSEPOWER / result.POWER_FLUID_HORSEPOWER
                : null;

            // AC-1: the expected jet efficiency is the module's own M x N, before its clamp.
            var efficiencies = HydraulicPumpCalculator.TryCalculateJetPumpPerformance(
                result.PRODUCTION_RATE,
                jet.POWER_FLUID_RATE,
                result.PUMP_INTAKE_PRESSURE,
                result.PUMP_DISCHARGE_PRESSURE,
                jet.POWER_FLUID_PRESSURE,
                out var rawPumpEfficiency,
                out var clampedPumpEfficiency,
                out var areaRatio);

            var findings = Findings(HydraulicPumpFindings.EvaluateJet(well, jet, result));
            var cavitation = HydraulicPumpFindings.Cavitation(
                result.PUMP_INTAKE_PRESSURE,
                HydraulicPumpFindings.VapourPressure(well));

            if (efficiencies && rawPumpEfficiency != clampedPumpEfficiency)
            {
                findings = findings
                    .Append(new OgHydraulicFinding(
                        "JET_EFFICIENCY_CLAMPED",
                        $"The jet efficiency M x N is {Number(rawPumpEfficiency)} and the engine clamps it to " +
                        $"{Number(clampedPumpEfficiency)}; the clamped value is shown.",
                        "Warning"))
                    .ToList();
            }

            return new OgHydraulicDesign(
                string.Empty,
                inputs.WellId.Trim(),
                kind,
                inputs,
                result.PRODUCTION_RATE,
                result.TOTAL_FLOW_RATE,
                result.PRODUCTION_RATIO,
                result.PUMP_EFFICIENCY,
                result.SYSTEM_EFFICIENCY,
                raw,
                result.POWER_FLUID_HORSEPOWER,
                result.HYDRAULIC_HORSEPOWER,
                result.PUMP_INTAKE_PRESSURE,
                result.PUMP_DISCHARGE_PRESSURE,
                null,
                efficiencies ? areaRatio : null,
                cavitation.AvailablePsi,
                cavitation.RequiredPsi,
                cavitation.MarginPsi,
                cavitation.Risk,
                findings,
                null,
                null);
        }

        var piston = new HYDRAULIC_PISTON_PUMP_PROPERTIES
        {
            HYDRAULIC_PISTON_PUMP_PROPERTIES_ID = Guid.NewGuid().ToString("N"),
            PISTON_DIAMETER = inputs.PistonDiameter!.Value,
            ROD_DIAMETER = inputs.RodDiameter!.Value,
            STROKE_LENGTH = inputs.StrokeLength!.Value,
            STROKES_PER_MINUTE = inputs.StrokesPerMinute!.Value,
            POWER_FLUID_PRESSURE = inputs.PowerFluidPressure!.Value,
            POWER_FLUID_RATE = inputs.PowerFluidRate!.Value,
            POWER_FLUID_SPECIFIC_GRAVITY = inputs.PowerFluidSpecificGravity!.Value,
        };

        var pistonResult = HydraulicPistonPumpCalculator.CalculatePerformance(well, piston);

        decimal? rawSystem = pistonResult.POWER_FLUID_HORSEPOWER > 0m
            ? pistonResult.HYDRAULIC_HORSEPOWER / pistonResult.POWER_FLUID_HORSEPOWER
            : null;

        var pistonFindings = Findings(HydraulicPumpFindings.EvaluatePiston(well, piston, pistonResult));
        var pistonCavitation = HydraulicPumpFindings.Cavitation(
            pistonResult.PUMP_INTAKE_PRESSURE,
            HydraulicPumpFindings.VapourPressure(well));

        return new OgHydraulicDesign(
            string.Empty,
            inputs.WellId.Trim(),
            kind,
            inputs,
            pistonResult.PRODUCTION_RATE,
            pistonResult.PRODUCTION_RATE + pistonResult.POWER_FLUID_CONSUMPTION,
            null,
            pistonResult.VOLUMETRIC_EFFICIENCY,
            pistonResult.SYSTEM_EFFICIENCY,
            rawSystem,
            pistonResult.POWER_FLUID_HORSEPOWER,
            pistonResult.HYDRAULIC_HORSEPOWER,
            pistonResult.PUMP_INTAKE_PRESSURE,
            pistonResult.PUMP_DISCHARGE_PRESSURE,
            pistonResult.PUMP_DISPLACEMENT,
            null,
            pistonCavitation.AvailablePsi,
            pistonCavitation.RequiredPsi,
            pistonCavitation.MarginPsi,
            pistonCavitation.Risk,
            pistonFindings,
            null,
            null);
    }

    private static IReadOnlyList<OgHydraulicFinding> Findings(IEnumerable<HydraulicPumpFinding> findings) =>
        findings
            .Select(finding => new OgHydraulicFinding(
                finding.Code,
                finding.Message,
                finding.Severity.ToString()))
            .ToList();

    // ------------------------------------------------------------------ the reads

    private static async Task<IReadOnlyList<OgHydraulicDesign>> ReadHistoryAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAllAsync<HYDRAULIC_PUMP_DESIGN>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.DESIGN_ID))
            .OrderByDescending(row => row.RECORDED_AT ?? row.DESIGN_DATE ?? DateTime.MinValue)
            .ThenBy(row => row.DESIGN_ID, StringComparer.Ordinal)
            .Select(ToDesign)
            .Take(50)
            .ToList();
    }

    private static OgHydraulicDesign ToDesign(HYDRAULIC_PUMP_DESIGN row)
    {
        var inputs = new OgHydraulicInputs(
            row.WELL_UWI,
            row.KIND,
            row.WELL_DEPTH_FT,
            row.TUBING_DIAMETER_IN,
            row.CASING_DIAMETER_IN,
            row.WELLHEAD_PRESSURE_PSIA,
            row.BOTTOM_HOLE_PRESSURE_PSIA,
            row.WELLHEAD_TEMPERATURE_R,
            row.BOTTOM_HOLE_TEMPERATURE_R,
            row.OIL_GRAVITY_API,
            row.WATER_CUT,
            row.GAS_OIL_RATIO_SCF_STB,
            row.GAS_SPECIFIC_GRAVITY,
            row.DESIRED_PRODUCTION_RATE_BBLD,
            row.PUMP_DEPTH_FT,
            row.NOZZLE_DIAMETER_IN,
            row.THROAT_DIAMETER_IN,
            row.DIFFUSER_DIAMETER_IN,
            row.PISTON_DIAMETER_IN,
            row.ROD_DIAMETER_IN,
            row.STROKE_LENGTH_IN,
            row.STROKES_PER_MINUTE,
            row.POWER_FLUID_PRESSURE_PSIA,
            row.POWER_FLUID_RATE_BBLD,
            row.POWER_FLUID_SPECIFIC_GRAVITY);

        var findings = string.IsNullOrWhiteSpace(row.FINDINGS_TEXT)
            ? []
            : row.FINDINGS_TEXT!
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(line =>
                {
                    var separator = line.IndexOf('·');
                    var head = separator > 0 ? line[..separator].Trim() : line;
                    var rest = separator > 0 ? line[(separator + 1)..].Trim() : line;
                    var severity = head.Split(' ')[0];
                    var colon = rest.IndexOf(':');
                    var code = colon > 0 ? rest[..colon].Trim().ToUpperInvariant() : string.Empty;
                    var message = colon > 0 ? rest[(colon + 1)..].Trim() : rest;

                    return new OgHydraulicFinding(code, message, Title(severity));
                })
                .ToList();

        return new OgHydraulicDesign(
            row.DESIGN_ID ?? string.Empty,
            row.WELL_UWI,
            row.KIND ?? HydraulicPumpKinds.Jet,
            inputs,
            row.PRODUCTION_RATE_BBLD,
            row.TOTAL_FLOW_RATE_BBLD,
            row.PRODUCTION_RATIO,
            row.PUMP_EFFICIENCY,
            row.SYSTEM_EFFICIENCY,
            row.RAW_SYSTEM_EFFICIENCY,
            row.POWER_FLUID_HORSEPOWER,
            row.HYDRAULIC_HORSEPOWER,
            row.PUMP_INTAKE_PRESSURE_PSIA,
            row.PUMP_DISCHARGE_PRESSURE_PSIA,
            row.PUMP_DISPLACEMENT_BBLD,
            row.AREA_RATIO,
            row.CAVITATION_AVAILABLE_PSI,
            row.CAVITATION_REQUIRED_PSI,
            row.CAVITATION_MARGIN_PSI,
            string.Equals(row.CAVITATION_RISK_IND, "Y", StringComparison.OrdinalIgnoreCase),
            findings,
            row.ACTOR,
            row.RECORDED_AT ?? row.DESIGN_DATE);
    }

    private static string Title(string severity) => severity.ToLowerInvariant() switch
    {
        "error" => "Error",
        "warning" => "Warning",
        _ => "Info",
    };

    private static OgHydraulicWorkspace Workspace(
        IReadOnlyList<OgHydraulicDesign> history,
        OgHydraulicDesign? selected) =>
        new(
            history,
            selected,
            HydraulicPumpKinds.All,
            HydraulicPumpConstants.StandardNozzleDiameters,
            HydraulicPumpConstants.StandardThroatDiameters,
            HydraulicPumpConstants.StandardPistonDiameters,
            HydraulicPumpConstants.StandardStrokeLengths,
            HydraulicPumpConstants.StandardSPM,
            HydraulicRules.RequiredNpshPsi,
            false,
            null);

    private static string Number(decimal value) =>
        value.ToString("#,0.####", CultureInfo.InvariantCulture);

    private static string Sizes(IEnumerable<decimal> sizes) =>
        string.Join(", ", sizes.Select(Number));
}
