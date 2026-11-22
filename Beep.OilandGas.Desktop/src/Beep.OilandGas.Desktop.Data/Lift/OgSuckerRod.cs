using System.Globalization;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.SuckerRodPumping.Calculations;
using Beep.OilandGas.SuckerRodPumping.Constants;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;

/// <summary>The rod grades the design accepts, read from the owning constants (LFT-04 AC-2).</summary>
public static class RodGrades
{
    /// <summary>The grades a design may use - not AUTO, which is the optimizer's instruction rather than a grade.</summary>
    public static IReadOnlyList<string> All { get; } =
    [
        SuckerRodConstants.RodGradeC,
        SuckerRodConstants.RodGradeD,
        SuckerRodConstants.RodGradeHL,
        SuckerRodConstants.RodGradeHY,
    ];

    public static bool IsSupported(string? grade) =>
        !string.IsNullOrWhiteSpace(grade)
        && All.Contains(grade.Trim().ToUpperInvariant(), StringComparer.Ordinal);

    public static bool IsOptimizer(string? grade) =>
        string.Equals(grade?.Trim(), SuckerRodConstants.RodGradeAuto, StringComparison.OrdinalIgnoreCase);

    public static string NameOf(string grade) => grade.Trim().ToUpperInvariant() switch
    {
        SuckerRodConstants.RodGradeC => "Grade C (carbon steel)",
        SuckerRodConstants.RodGradeD => "Grade D (medium carbon)",
        SuckerRodConstants.RodGradeHL => "Grade HL (high-strength low-alloy)",
        SuckerRodConstants.RodGradeHY => "Grade HY (high-strength alloy)",
        SuckerRodConstants.RodGradeAuto => "AUTO (the optimizer chooses)",
        _ => grade,
    };
}

/// <summary>One section of the rod string, as the engineer enters it (LFT-04).</summary>
public sealed record OgRodSection(decimal DiameterIn, decimal LengthFt)
{
    public string Describe() =>
        $"{DiameterIn.ToString("0.###", CultureInfo.InvariantCulture)} in × " +
        $"{LengthFt.ToString("#,0.##", CultureInfo.InvariantCulture)} ft";
}

/// <summary>The inputs of one sucker rod design (LFT-04). Every value is required except the density, which must be accepted.</summary>
public sealed record OgSrpInputs(
    string? WellId,
    decimal? PumpDiameterIn,
    decimal? StrokeLengthIn,
    decimal? StrokesPerMinute,
    decimal? PumpDepthFt,
    decimal? FluidLevelFt,
    decimal? TubingDiameterIn,
    decimal? OilGravity,
    decimal? WaterCut,
    decimal? GasOilRatio,
    decimal? GasSpecificGravity,
    decimal? RodDensity,
    bool AcceptDefaultDensity,
    string? Grade,
    IReadOnlyList<OgRodSection> Sections);

/// <summary>One section's fatigue result, as the page lists it (LFT-04).</summary>
public sealed record OgRodSectionResult(
    int Order,
    decimal DiameterIn,
    decimal LengthFt,
    decimal MaxStress,
    decimal MinStress,
    decimal SafetyFactor,
    decimal GoodmanRatio)
{
    public string DiameterText => DiameterIn.ToString("0.###", CultureInfo.InvariantCulture) + " in";

    public string LengthText => LengthFt.ToString("#,0.##", CultureInfo.InvariantCulture) + " ft";

    public string StressText => MaxStress.ToString("#,0.##", CultureInfo.InvariantCulture) + " psi";

    public string SafetyText => SafetyFactor.ToString("0.###", CultureInfo.InvariantCulture);

    public string GoodmanText => GoodmanRatio.ToString("0.###", CultureInfo.InvariantCulture);

    public string Describe() =>
        $"section {Order.ToString(CultureInfo.InvariantCulture)} · {DiameterText} × {LengthText} · " +
        $"max stress {StressText} · safety factor {SafetyText} · Goodman {GoodmanText}";
}

/// <summary>One point of the pump card (LFT-04 AC-3).</summary>
public sealed record OgPumpCardPoint(decimal Position, decimal Load)
{
    public string Describe() =>
        $"{Position.ToString("#,0.###", CultureInfo.InvariantCulture)} in · " +
        $"{Load.ToString("#,0.##", CultureInfo.InvariantCulture)} lb";
}

/// <summary>One sucker rod design (LFT-04), matching the plan's <c>OgSrpResult</c> plus the taper and the fatigue.</summary>
public sealed record OgSrpResult(
    string Id,
    string? WellId,
    decimal PumpDiameterIn,
    decimal StrokeLengthIn,
    decimal StrokesPerMinute,
    decimal PumpDepthFt,
    string Grade,
    IReadOnlyList<OgRodSection> Sections,
    decimal TotalLengthFt,
    decimal PeakLoad,
    decimal MinLoad,
    decimal FluidLoad,
    decimal MaxStress,
    decimal StressRange,
    decimal MinSafetyFactor,
    decimal RequiredSafetyFactor,
    decimal GoodmanRatio,
    int CriticalSection,
    string Risk,
    decimal ProductionRate,
    decimal MotorHorsepower,
    decimal CardPeak,
    decimal CardMinimum,
    IReadOnlyList<OgPumpCardPoint> CardPoints,
    IReadOnlyList<OgRodSectionResult> SectionResults,
    bool DensityAssumed,
    string? Actor,
    DateTime? RecordedAt)
{
    public const string LoadUnit = "lb";
    public const string StressUnit = "psi";
    public const string RateUnit = "STB/d";
    public const string PowerUnit = "hp";
    public const string DiameterUnit = "in";
    public const string LengthUnit = "ft";

    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    /// <summary>AC-1: the string is unsafe when its lowest safety factor is under the module's minimum.</summary>
    public bool IsUnsafe => MinSafetyFactor < RequiredSafetyFactor;

    public string GradeText => RodGrades.NameOf(Grade);

    public string UnitsText =>
        $"diameters in {DiameterUnit}, lengths in {LengthUnit}, loads in {LoadUnit}, stresses in {StressUnit}, " +
        $"rate in {RateUnit}, power in {PowerUnit}";

    public string PeakText => PeakLoad.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + LoadUnit;

    public string MinLoadText => MinLoad.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + LoadUnit;

    public string FluidLoadText => FluidLoad.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + LoadUnit;

    public string MaxStressText => MaxStress.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + StressUnit;

    public string SafetyText =>
        MinSafetyFactor.ToString("0.###", CultureInfo.InvariantCulture) + " against the module's " +
        RequiredSafetyFactor.ToString("0.###", CultureInfo.InvariantCulture) + " minimum";

    public string RateText => ProductionRate.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + RateUnit;

    public string PowerText => MotorHorsepower.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + PowerUnit;

    public string CriticalText =>
        $"section {CriticalSection.ToString(CultureInfo.InvariantCulture)}";
    public string TaperText => Sections.Count == 0
        ? "no section"
        : string.Join(Environment.NewLine, Sections.Select(section => section.Describe()));

    public string SectionsText => SectionResults.Count == 0
        ? "no section was analysed"
        : string.Join(Environment.NewLine, SectionResults.Select(section => section.Describe()));

    public string CardText => CardPoints.Count == 0
        ? "no pump card"
        : $"the card runs from {CardMinimum.ToString("#,0.##", CultureInfo.InvariantCulture)} {LoadUnit} to " +
          $"{CardPeak.ToString("#,0.##", CultureInfo.InvariantCulture)} {LoadUnit} over " +
          $"{CardPoints.Count.ToString(CultureInfo.InvariantCulture)} point(s)";

    public string DensityText => DensityAssumed
        ? $"the default steel density {SuckerRodConstants.SteelDensity.ToString("#,0.##", CultureInfo.InvariantCulture)} " +
          "lb/ft3 was accepted explicitly"
        : "the rod density was entered";

    public string SavedText => IsSaved
        ? $"saved by {Actor ?? "(unknown)"} on " +
          $"{RecordedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date"}"
        : "not saved";

    public string Describe() =>
        $"{WellId ?? "(no well)"} · {GradeText} · {Sections.Count.ToString(CultureInfo.InvariantCulture)} section(s) · " +
        $"peak {PeakText} · max stress {MaxStressText} · safety factor {SafetyText} · {RateText} · {SavedText}";
}

/// <summary>A design whose save was not acknowledged: retained and retried (LFT-04's state table).</summary>
public sealed record OgSrpPending(OgSrpResult Result, string Actor);

/// <summary>The sucker rod workspace (LFT-04).</summary>
public sealed record OgSrpWorkspace(
    IReadOnlyList<OgSrpResult> Designs,
    OgSrpResult? Selected,
    IReadOnlyList<string> Grades,
    IReadOnlyList<decimal> StandardRodDiameters,
    IReadOnlyList<decimal> StandardPumpDiameters,
    IReadOnlyList<decimal> StandardStrokeLengths,
    IReadOnlyList<decimal> StandardStrokesPerMinute,
    decimal RequiredSafetyFactor,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string DesignsText => Designs.Count == 0
        ? "No sucker rod design is saved yet."
        : string.Join(Environment.NewLine, Designs.Select(design => design.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved designs could not be read; new designs are still allowed"
        : $"{Designs.Count.ToString(CultureInfo.InvariantCulture)} saved design(s)";

    public string ReferenceText =>
        $"grades {string.Join(", ", Grades)} · rod diameters " +
        $"{string.Join(", ", StandardRodDiameters.Select(size => size.ToString("0.###", CultureInfo.InvariantCulture)))} in · " +
        $"required safety factor {RequiredSafetyFactor.ToString("0.##", CultureInfo.InvariantCulture)}";
}

/// <summary>The sucker rod rules, stated once so the page, the service and the tests agree.</summary>
public static class SrpRules
{
    public const string CodeNoConnection = "srp-no-connection";
    public const string CodeReadFailed = "srp-read-failed";
    public const string CodeWriteFailed = "srp-write-failed";
    public const string CodeActorRequired = "srp-actor-required";
    public const string CodeWellRequired = "srp-well-required";
    public const string CodeInputRequired = "srp-input-required";
    public const string CodeInputInvalid = "srp-input-invalid";
    public const string CodeGradeUnknown = "srp-grade-unknown";
    public const string CodeDensityNotAccepted = "srp-density-not-accepted";
    public const string CodeStringIncomplete = "srp-string-incomplete";
    public const string CodeUnsafeString = "srp-unsafe-string";
    public const string CodeNothingToSave = "srp-nothing-to-save";
    public const string CodeNothingPending = "srp-nothing-pending";

    /// <summary>The minimum safety factor, from <c>SuckerRodConstants</c> rather than written here (AC-1).</summary>
    public static decimal RequiredSafetyFactor => SuckerRodConstants.MinimumSafetyFactor;

    public static IReadOnlyList<string> Missing(OgSrpInputs inputs)
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

        Need(inputs.PumpDiameterIn, "pump diameter");
        Need(inputs.StrokeLengthIn, "stroke length");
        Need(inputs.StrokesPerMinute, "strokes per minute");
        Need(inputs.PumpDepthFt, "pump depth");
        Need(inputs.FluidLevelFt, "fluid level");
        Need(inputs.TubingDiameterIn, "tubing diameter");
        Need(inputs.OilGravity, "oil gravity");
        Need(inputs.WaterCut, "water cut");
        Need(inputs.GasOilRatio, "gas-oil ratio");
        Need(inputs.GasSpecificGravity, "gas specific gravity");

        if (string.IsNullOrWhiteSpace(inputs.Grade))
        {
            missing.Add("rod grade");
        }

        if (inputs.Sections is null || inputs.Sections.Count == 0)
        {
            missing.Add("at least one rod section");
        }

        // The state table: a missing density is a default that must be accepted explicitly.
        if (inputs.RodDensity is null && !inputs.AcceptDefaultDensity)
        {
            missing.Add($"the rod density, or the explicit acceptance of the default " +
                        $"{SuckerRodConstants.SteelDensity.ToString("#,0.##", CultureInfo.InvariantCulture)} lb/ft3");
        }

        return missing;
    }

    public static (string Code, string Message)? Validate(OgSrpInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            return missing.Contains("well")
                ? (CodeWellRequired, "Choose the well the design is for.")
                : missing.Any(item => item.StartsWith("the rod density", StringComparison.Ordinal))
                    ? (CodeDensityNotAccepted,
                        "The rod density is missing; enter it, or accept the module's default steel density explicitly.")
                    : (CodeInputRequired, $"The design needs {string.Join(", ", missing)}.");
        }

        if (!RodGrades.IsSupported(inputs.Grade))
        {
            return (CodeGradeUnknown,
                $"'{inputs.Grade}' is not a rod grade; the module's grades are {string.Join(", ", RodGrades.All)}.");
        }

        if (inputs.StrokesPerMinute is <= 0m)
        {
            return (CodeInputInvalid, "The strokes per minute must be above zero.");
        }

        if (inputs.StrokeLengthIn is <= 0m)
        {
            return (CodeInputInvalid, "The stroke length must be above zero.");
        }

        if (inputs.PumpDepthFt is <= 0m)
        {
            return (CodeInputInvalid, "The pump depth must be above zero.");
        }

        if (inputs.WaterCut is < 0m or > 1m)
        {
            return (CodeInputInvalid, "The water cut is a fraction between 0 and 1.");
        }

        foreach (var section in inputs.Sections!)
        {
            if (section.DiameterIn <= 0m || section.LengthFt <= 0m)
            {
                return (CodeStringIncomplete,
                    "Every rod section needs a diameter and a length above zero.");
            }
        }

        return null;
    }
}

/// <summary>LFT-04: sucker rod pumping.</summary>
public interface IOgSuckerRod
{
    /// <summary>Designs and checks the string. Nothing is written.</summary>
    Task<OgResult<OgSrpWorkspace>> DesignAsync(
        OgSrpInputs inputs,
        CancellationToken cancellationToken = default);

    /// <summary>Saves a design whose string is safe (AC-1 refuses an unsafe one).</summary>
    Task<OgResult<OgSrpWorkspace>> SaveAsync(
        OgSrpResult result,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The saved designs; an unavailable history is a state.</summary>
    Task<OgResult<OgSrpWorkspace>> LoadAsync(
        string? wellId = null,
        string? designId = null,
        CancellationToken cancellationToken = default);

    /// <summary>The result retained when its save was not acknowledged.</summary>
    OgSrpPending? Pending { get; }

    /// <summary>Retries the same insert.</summary>
    Task<OgResult<OgSrpWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// LFT-04 over the SuckerRodPumping module's load, fatigue, flow-rate, power and card calculators and the
/// <c>SRP_DESIGN</c> row, through a unit of work.
/// </summary>
/// <remarks>
/// The arithmetic is the owner's: <c>SuckerRodLoadCalculator</c> for the loads and the card,
/// <c>FatigueAnalysis</c> for the stress and the safety factor, <c>SuckerRodFlowRatePowerCalculator</c> for the rate and
/// the power. What the screen adds is the refusal of an unsafe string with the limiting section named (AC-1 and SG-07-5),
/// the explicit acceptance of the default density, the units and the record.
/// </remarks>
public sealed class OgSuckerRodService : IOgSuckerRod
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgSuckerRodService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgSrpPending? Pending { get; private set; }

    public async Task<OgResult<OgSrpWorkspace>> DesignAsync(
        OgSrpInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (SrpRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgSrpWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var result = Build(inputs);

            // AC-1 and SG-07-5: an unsafe string is refused, and the limiting section is named.
            if (result.IsUnsafe)
            {
                var critical = result.SectionResults.FirstOrDefault(section =>
                    section.Order == result.CriticalSection);

                return OgResult<OgSrpWorkspace>.Failed(
                    $"The string is unsafe: its lowest safety factor is " +
                    $"{result.MinSafetyFactor.ToString("0.###", CultureInfo.InvariantCulture)}, below the module's " +
                    $"{result.RequiredSafetyFactor.ToString("0.###", CultureInfo.InvariantCulture)} minimum, at " +
                    $"section {result.CriticalSection.ToString(CultureInfo.InvariantCulture)}" +
                    (critical is null
                        ? string.Empty
                        : $" ({critical.DiameterText}, max stress {critical.StressText}, Goodman " +
                          $"{critical.GoodmanText})") +
                    ". Use a larger diameter or a stronger grade.",
                    SrpRules.CodeUnsafeString);
            }

            var source = _source();
            var history = source is null
                ? []
                : await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgSrpWorkspace>.Ok(Workspace(history, result));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"designing a sucker rod string for well {inputs.WellId}",
                "the design is not shown",
                FailureSeverity.Error);

            return OgResult<OgSrpWorkspace>.Failed(
                "The sucker rod design could not be run.", reference ?? SrpRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgSrpWorkspace>> SaveAsync(
        OgSrpResult result,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgSrpWorkspace>.Failed(
                "A saved design must record who saved it.", SrpRules.CodeActorRequired);
        }

        // AC-1 again at the door: an unsafe string cannot be stored even if a caller constructed one.
        if (result.IsUnsafe)
        {
            return OgResult<OgSrpWorkspace>.Failed(
                $"The string is unsafe: its safety factor is " +
                $"{result.MinSafetyFactor.ToString("0.###", CultureInfo.InvariantCulture)}, below the module's " +
                $"{result.RequiredSafetyFactor.ToString("0.###", CultureInfo.InvariantCulture)} minimum, at " +
                $"section {result.CriticalSection.ToString(CultureInfo.InvariantCulture)}.",
                SrpRules.CodeUnsafeString);
        }

        if (result.Sections.Count == 0)
        {
            return OgResult<OgSrpWorkspace>.Failed(
                "There is no string to save; design it first.", SrpRules.CodeNothingToSave);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgSrpWorkspace>.Failed(
                "No connection is open, so the design cannot be saved.", SrpRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var unit = OgUow.For<SRP_DESIGN>(source);
            unit.PrimaryKey = "DESIGN_ID";

            unit.Add(new SRP_DESIGN
            {
                DESIGN_ID = Guid.NewGuid().ToString("N"),
                WELL_UWI = result.WellId,
                DESIGN_DATE = now,
                PUMP_DIAMETER_IN = result.PumpDiameterIn,
                STROKE_LENGTH_IN = result.StrokeLengthIn,
                STROKES_PER_MINUTE = result.StrokesPerMinute,
                PUMP_DEPTH_FT = result.PumpDepthFt,
                GRADE = result.Grade,
                TAPER = string.Join("\n", result.Sections.Select(section =>
                    section.DiameterIn.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                    section.LengthFt.ToString("0.##", CultureInfo.InvariantCulture))),
                SECTION_COUNT = result.Sections.Count,
                TOTAL_LENGTH_FT = result.TotalLengthFt,
                PEAK_LOAD_LB = result.PeakLoad,
                MIN_LOAD_LB = result.MinLoad,
                FLUID_LOAD_LB = result.FluidLoad,
                MAX_STRESS_PSI = result.MaxStress,
                STRESS_RANGE_PSI = result.StressRange,
                MIN_SAFETY_FACTOR = result.MinSafetyFactor,
                REQUIRED_SAFETY_FACTOR = result.RequiredSafetyFactor,
                GOODMAN_RATIO = result.GoodmanRatio,
                CRITICAL_SECTION = result.CriticalSection,
                RISK = result.Risk,
                PRODUCTION_RATE_BPD = result.ProductionRate,
                MOTOR_HORSEPOWER = result.MotorHorsepower,
                CARD_PEAK_LB = result.CardPeak,
                CARD_MIN_LB = result.CardMinimum,
                CARD_POINTS = result.CardPoints.Count,
                UNITS_TEXT = result.UnitsText,
                DENSITY_ASSUMED_IND = result.DensityAssumed ? "Y" : "N",
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
                Pending = new OgSrpPending(result, actor.Trim());

                return OgResult<OgSrpWorkspace>.Failed(
                    OgUow.Message(committed, "The design was not saved; the original is retained and can be retried."),
                    SrpRules.CodeWriteFailed);
            }

            Pending = null;

            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);
            var saved = result with { Id = string.Empty, Actor = actor.Trim(), RecordedAt = now };

            return OgResult<OgSrpWorkspace>.Ok(Workspace(history, saved));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = new OgSrpPending(result, actor.Trim());

            var reference = _failures.ReportHandled(
                ex,
                $"saving a sucker rod design for well {result.WellId}",
                "the design was not acknowledged; the original is retained and the save can be retried",
                FailureSeverity.Error);

            return OgResult<OgSrpWorkspace>.Failed(
                "The design was not acknowledged; the original is retained and the save can be retried.",
                reference ?? SrpRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgSrpWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgSrpWorkspace>.Failed(
                "There is no design waiting to be retried.", SrpRules.CodeNothingPending);
        }

        var retried = await SaveAsync(pending.Result, actor, cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    public async Task<OgResult<OgSrpWorkspace>> LoadAsync(
        string? wellId = null,
        string? designId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgSrpWorkspace>.Failed(
                "No connection is open, so the saved designs cannot be read.", SrpRules.CodeNoConnection);
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

            return OgResult<OgSrpWorkspace>.Ok(Workspace(history, selected));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the saved sucker rod designs",
                "the saved designs are unavailable; new designs are still allowed",
                FailureSeverity.Degraded);

            return OgResult<OgSrpWorkspace>.Ok(new OgSrpWorkspace(
                [],
                null,
                RodGrades.All,
                SuckerRodConstants.StandardRodDiameters,
                SuckerRodConstants.StandardPumpDiameters,
                SuckerRodConstants.StandardStrokeLengths,
                SuckerRodConstants.StandardSPM,
                SrpRules.RequiredSafetyFactor,
                true,
                $"the saved designs could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the design

    /// <summary>
    /// Builds the string and runs the owner's three calculators. The result is returned even when the string is unsafe; the
    /// caller refuses it, so the engineer can see which section and by how much.
    /// </summary>
    public static OgSrpResult Build(OgSrpInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var density = inputs.RodDensity ?? SuckerRodConstants.SteelDensity;
        var assumed = inputs.RodDensity is null && inputs.AcceptDefaultDensity;
        var topDiameter = inputs.Sections[0].DiameterIn;

        var system = new SUCKER_ROD_SYSTEM_PROPERTIES
        {
            SUCKER_ROD_SYSTEM_PROPERTIES_ID = Guid.NewGuid().ToString("N"),
            WELL_DEPTH = inputs.PumpDepthFt!.Value,
            PUMP_SETTING_DEPTH = inputs.PumpDepthFt.Value,
            TUBING_DIAMETER = inputs.TubingDiameterIn!.Value,
            ROD_DIAMETER = topDiameter,
            PUMP_DIAMETER = inputs.PumpDiameterIn!.Value,
            STROKE_LENGTH = inputs.StrokeLengthIn!.Value,
            STROKES_PER_MINUTE = inputs.StrokesPerMinute!.Value,
            WELLHEAD_PRESSURE = 100m,
            BOTTOM_HOLE_PRESSURE = 1000m,
            OIL_GRAVITY = inputs.OilGravity!.Value,
            WATER_CUT = inputs.WaterCut!.Value,
            GAS_OIL_RATIO = inputs.GasOilRatio!.Value,
            GAS_SPECIFIC_GRAVITY = inputs.GasSpecificGravity!.Value,
            ROD_DENSITY = density,
            PUMP_EFFICIENCY = SuckerRodConstants.StandardPumpEfficiency,
            FLUID_LEVEL = inputs.FluidLevelFt!.Value,
            FLUID_DENSITY = 62.4m,
        };

        var stringId = Guid.NewGuid().ToString("N");

        var rodString = new SUCKER_ROD_STRING
        {
            SUCKER_ROD_STRING_ID = stringId,
            TOTAL_LENGTH = inputs.Sections.Sum(section => section.LengthFt),
            SECTIONS = inputs.Sections
                .Select((section, index) => new ROD_SECTION
                {
                    ROD_SECTION_ID = Guid.NewGuid().ToString("N"),
                    SUCKER_ROD_STRING_ID = stringId,
                    DIAMETER = section.DiameterIn,
                    LENGTH = section.LengthFt,
                    DENSITY = density,
                    SECTION_ORDER = index + 1,
                })
                .ToList(),
        };

        var loads = SuckerRodLoadCalculator.CalculateLoads(system, rodString);
        var card = SuckerRodLoadCalculator.GeneratePumpCard(system, rodString);
        var flow = SuckerRodFlowRatePowerCalculator.CalculateFlowRateAndPower(system, loads);
        var fatigue = FatigueAnalysis.AnalyzeString(loads, rodString, inputs.Grade!, system);

        var sectionResults = fatigue.Sections
            .Select(section => new OgRodSectionResult(
                section.SectionOrder,
                (decimal)section.Diameter,
                inputs.Sections.ElementAtOrDefault(Math.Max(0, section.SectionOrder - 1)).LengthFt,
                (decimal)section.MaxStress,
                (decimal)section.MinStress,
                (decimal)section.SafetyFactor,
                (decimal)section.GoodmanRatio))
            .ToList();

        var critical = sectionResults.FirstOrDefault(section => section.Order == fatigue.CriticalSectionOrder)
                       ?? sectionResults.OrderBy(section => section.SafetyFactor).FirstOrDefault();

        var cardPeak = (decimal)card.PeakLoad;
        var cardMinimum = (decimal)card.MinimumLoad;
        var criticalOrder = critical?.Order ?? fatigue.CriticalSectionOrder;

        return new OgSrpResult(
            string.Empty,
            inputs.WellId!.Trim(),
            inputs.PumpDiameterIn.Value,
            inputs.StrokeLengthIn.Value,
            inputs.StrokesPerMinute.Value,
            inputs.PumpDepthFt.Value,
            inputs.Grade!.Trim().ToUpperInvariant(),
            inputs.Sections,
            rodString.TOTAL_LENGTH,
            loads.PEAK_LOAD,
            loads.MINIMUM_LOAD,
            loads.FLUID_LOAD,
            loads.MAXIMUM_STRESS,
            loads.STRESS_RANGE,
            (decimal)fatigue.MinSafetyFactor,
            SrpRules.RequiredSafetyFactor,
            (decimal)fatigue.MaxGoodmanRatio,
            criticalOrder,
            fatigue.OverallRisk,
            flow.PRODUCTION_RATE,
            flow.MOTOR_HORSEPOWER,
            cardPeak,
            cardMinimum,
            card.Points?
                .Select(point => new OgPumpCardPoint((decimal)point.Position, (decimal)point.Load))
                .ToList() ?? [],
            sectionResults,
            assumed,
            null,
            null);
    }

    // ------------------------------------------------------------------ the reads

    private static async Task<IReadOnlyList<OgSrpResult>> ReadHistoryAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAllAsync<SRP_DESIGN>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.DESIGN_ID))
            .OrderByDescending(row => row.RECORDED_AT ?? row.DESIGN_DATE ?? DateTime.MinValue)
            .ThenBy(row => row.DESIGN_ID, StringComparer.Ordinal)
            .Select(ToResult)
            .Take(50)
            .ToList();
    }

    private static OgSrpResult ToResult(SRP_DESIGN row)
    {
        var sections = string.IsNullOrWhiteSpace(row.TAPER)
            ? []
            : row.TAPER!
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(line => line.Split(',', StringSplitOptions.TrimEntries))
                .Where(parts => parts.Length >= 2)
                .Select(parts => new OgRodSection(
                    decimal.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m,
                    decimal.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var l) ? l : 0m))
                .ToList();

        return new OgSrpResult(
            row.DESIGN_ID ?? string.Empty,
            row.WELL_UWI,
            row.PUMP_DIAMETER_IN,
            row.STROKE_LENGTH_IN,
            row.STROKES_PER_MINUTE,
            row.PUMP_DEPTH_FT,
            row.GRADE ?? SuckerRodConstants.RodGradeDefault,
            sections,
            row.TOTAL_LENGTH_FT,
            row.PEAK_LOAD_LB,
            row.MIN_LOAD_LB,
            row.FLUID_LOAD_LB,
            row.MAX_STRESS_PSI,
            row.STRESS_RANGE_PSI,
            row.MIN_SAFETY_FACTOR,
            row.REQUIRED_SAFETY_FACTOR,
            row.GOODMAN_RATIO,
            row.CRITICAL_SECTION,
            row.RISK ?? string.Empty,
            row.PRODUCTION_RATE_BPD,
            row.MOTOR_HORSEPOWER,
            row.CARD_PEAK_LB,
            row.CARD_MIN_LB,
            [],
            [],
            string.Equals(row.DENSITY_ASSUMED_IND, "Y", StringComparison.OrdinalIgnoreCase),
            row.ACTOR,
            row.RECORDED_AT ?? row.DESIGN_DATE);
    }

    private static OgSrpWorkspace Workspace(IReadOnlyList<OgSrpResult> history, OgSrpResult? selected) =>
        new(
            history,
            selected,
            RodGrades.All,
            SuckerRodConstants.StandardRodDiameters,
            SuckerRodConstants.StandardPumpDiameters,
            SuckerRodConstants.StandardStrokeLengths,
            SuckerRodConstants.StandardSPM,
            SrpRules.RequiredSafetyFactor,
            false,
            null);
}
