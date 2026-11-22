using System.Globalization;
using Beep.OilandGas.Models.Data.PumpPerformance;
using Beep.OilandGas.PumpPerformance.Calculations;
using Beep.OilandGas.PumpPerformance.Constants;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;

/// <summary>The pump types this screen checks a curve for (LFT-03).</summary>
public static class PumpTypes
{
    public const string Esp = "ESP";
    public const string Centrifugal = "CENTRIFUGAL";
    public const string PositiveDisplacement = "POSITIVE_DISPLACEMENT";
    public const string Jet = "JET";

    public static IReadOnlyList<string> All { get; } = [Esp, Centrifugal, PositiveDisplacement, Jet];

    public static bool IsSupported(string? type) =>
        !string.IsNullOrWhiteSpace(type)
        && All.Contains(type.Trim().ToUpperInvariant(), StringComparer.Ordinal);

    public static string NameOf(string type) => type switch
    {
        Esp => "Electrical submersible pump",
        Centrifugal => "Centrifugal pump",
        PositiveDisplacement => "Positive displacement pump",
        Jet => "Jet pump",
        _ => type,
    };
}

/// <summary>One point of the published H-Q curve, as the engineer enters it (LFT-03).</summary>
public sealed record OgPumpCurvePoint(decimal Rate, decimal Head, decimal? Power)
{
    public string Describe() =>
        $"{Rate.ToString("#,0.###", CultureInfo.InvariantCulture)} GPM · " +
        $"{Head.ToString("#,0.##", CultureInfo.InvariantCulture)} ft" +
        (Power is { } power
            ? $" · {power.ToString("#,0.##", CultureInfo.InvariantCulture)} hp"
            : string.Empty);
}

/// <summary>The inputs of one pump duty-point analysis (LFT-03). Every value is required.</summary>
public sealed record OgPumpInputs(
    string? WellId,
    string? PumpType,
    IReadOnlyList<OgPumpCurvePoint> Curve,
    int Stages,
    decimal? DutyRate,
    decimal? SpecificGravity,
    decimal? SpeedRpm,
    decimal? NewSpeedRpm,
    decimal? AtmosphericPressure,
    decimal? VaporPressure,
    decimal? SuctionHead,
    decimal? SuctionFriction);

/// <summary>One pump duty-point analysis (LFT-03), matching the plan's <c>OgPumpResult</c> plus the margin and affinity.</summary>
public sealed record OgPumpResult(
    string Id,
    string? WellId,
    string PumpType,
    string CurveSource,
    IReadOnlyList<OgPumpCurvePoint> Curve,
    int Stages,
    decimal Rate,
    decimal Head,
    decimal TotalHead,
    decimal Efficiency,
    decimal BrakeHorsepower,
    decimal PowerKilowatts,
    decimal NpshAvailable,
    decimal NpshRequired,
    decimal NpshMargin,
    decimal NpshMinimumMargin,
    bool NpshWarning,
    bool Extrapolated,
    string AffinityText,
    IReadOnlyList<string> Notes,
    string? Actor,
    DateTime? RecordedAt)
{
    public const string CurveFromEnteredPoints = "ENTERED";
    public const string RateUnit = "GPM";
    public const string HeadUnit = "ft";
    public const string PowerUnit = "hp";

    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    public decimal MinimumRate => Curve.Count == 0 ? 0m : Curve.Min(point => point.Rate);

    public decimal MaximumRate => Curve.Count == 0 ? 0m : Curve.Max(point => point.Rate);

    public string TypeText => PumpTypes.NameOf(PumpType);

    public string RangeText =>
        $"{MinimumRate.ToString("#,0.###", CultureInfo.InvariantCulture)} to " +
        $"{MaximumRate.ToString("#,0.###", CultureInfo.InvariantCulture)} {RateUnit}";

    public string RateText => Rate.ToString("#,0.###", CultureInfo.InvariantCulture) + " " + RateUnit;

    public string HeadText => Head.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + HeadUnit;

    public string TotalHeadText => TotalHead.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + HeadUnit;

    public string EfficiencyText =>
        (Efficiency * 100m).ToString("0.##", CultureInfo.InvariantCulture) + " %";

    public string PowerText =>
        BrakeHorsepower.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + PowerUnit +
        " (" + PowerKilowatts.ToString("#,0.##", CultureInfo.InvariantCulture) + " kW)";

    /// <summary>AC-2: both values and the module's minimum, so the warning explains itself.</summary>
    public string NpshText =>
        $"NPSHa {NpshAvailable.ToString("#,0.##", CultureInfo.InvariantCulture)} ft against NPSHr " +
        $"{NpshRequired.ToString("#,0.##", CultureInfo.InvariantCulture)} ft: margin " +
        $"{NpshMargin.ToString("#,0.##", CultureInfo.InvariantCulture)} ft " +
        (NpshWarning
            ? $"is below the {NpshMinimumMargin.ToString("#,0.##", CultureInfo.InvariantCulture)} ft minimum " +
              "(cavitation screening)"
            : $"meets the {NpshMinimumMargin.ToString("#,0.##", CultureInfo.InvariantCulture)} ft minimum");

    public string StageText => Stages == 1
        ? "one stage"
        : $"{Stages.ToString(CultureInfo.InvariantCulture)} stages · head per stage {HeadText}, total {TotalHeadText}";

    public string AffinitySummary => string.IsNullOrWhiteSpace(AffinityText)
        ? "no affinity change was applied"
        : AffinityText + (Extrapolated ? " · EXTRAPOLATED beyond the published curve" : string.Empty);

    public string NotesText => Notes.Count == 0 ? "no note" : string.Join(" · ", Notes);

    public string CurveText => Curve.Count == 0
        ? "no curve point"
        : string.Join(Environment.NewLine, Curve.Select(point => point.Describe()));

    public string SavedText => IsSaved
        ? $"saved by {Actor ?? "(unknown)"} on " +
          $"{RecordedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date"}"
        : "not saved";

    public string Describe() =>
        $"{WellId ?? "(no well)"} · {TypeText} · duty {RateText} at {HeadText} · {StageText} · " +
        $"efficiency {EfficiencyText} · power {PowerText} · {NpshText} · {SavedText}";
}

/// <summary>A design whose save was not acknowledged: retained and retried (LFT-03's state table).</summary>
public sealed record OgPumpPending(OgPumpResult Result, string Actor);

/// <summary>The pump workspace (LFT-03).</summary>
public sealed record OgPumpWorkspace(
    IReadOnlyList<OgPumpResult> Designs,
    OgPumpResult? Selected,
    IReadOnlyList<string> PumpTypes,
    bool HistoryFailed,
    string? HistoryReason,
    string CurveSourceNote)
{
    public string DesignsText => Designs.Count == 0
        ? "No pump duty point is saved yet."
        : string.Join(Environment.NewLine, Designs.Select(design => design.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved duty points could not be read; new analyses are still allowed"
        : $"{Designs.Count.ToString(CultureInfo.InvariantCulture)} saved duty point(s)";
}

/// <summary>The pump rules, stated once so the page, the service and the tests agree.</summary>
public static class PumpRules
{
    public const string CodeNoConnection = "pump-no-connection";
    public const string CodeReadFailed = "pump-read-failed";
    public const string CodeWriteFailed = "pump-write-failed";
    public const string CodeActorRequired = "pump-actor-required";
    public const string CodeWellRequired = "pump-well-required";
    public const string CodeInputRequired = "pump-input-required";
    public const string CodeInputInvalid = "pump-input-invalid";
    public const string CodeTypeUnknown = "pump-type-unknown";
    public const string CodeCurveMissing = "pump-curve-missing";
    public const string CodeCurveInvalid = "pump-curve-invalid";
    public const string CodeDutyOutsideCurve = "pump-duty-outside-curve";
    public const string CodeNothingToSave = "pump-nothing-to-save";
    public const string CodeNothingPending = "pump-nothing-pending";

    /// <summary>The NPSH margin the module states, read from <see cref="PumpConstants"/> rather than written here (AC-2).</summary>
    public static decimal MinimumNpshMargin => (decimal)PumpConstants.DefaultNpshSafetyMarginFeet;

    /// <summary>The curve source this screen can offer: there is no pump-curve catalog in this repository.</summary>
    public const string CurveSource = OgPumpResult.CurveFromEnteredPoints;

    public static IReadOnlyList<string> Missing(OgPumpInputs inputs)
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

        Need(inputs.DutyRate, "duty rate");
        Need(inputs.SpecificGravity, "specific gravity");
        Need(inputs.SpeedRpm, "pump speed");
        Need(inputs.AtmosphericPressure, "atmospheric pressure");
        Need(inputs.VaporPressure, "vapour pressure");
        Need(inputs.SuctionHead, "suction head");
        Need(inputs.SuctionFriction, "suction friction loss");

        if (string.IsNullOrWhiteSpace(inputs.PumpType))
        {
            missing.Add("pump type");
        }

        if (inputs.Curve is null || inputs.Curve.Count < PumpConstants.MinDataPoints)
        {
            missing.Add($"at least {PumpConstants.MinDataPoints} published curve point(s)");
        }

        return missing;
    }

    public static (string Code, string Message)? Validate(OgPumpInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            return missing.Contains("well")
                ? (CodeWellRequired, "Choose the well the pump is for.")
                : (CodeInputRequired, $"The analysis needs {string.Join(", ", missing)}.");
        }

        if (!PumpTypes.IsSupported(inputs.PumpType))
        {
            return (CodeTypeUnknown,
                $"'{inputs.PumpType}' is not a pump type; use {string.Join(", ", PumpTypes.All)}.");
        }

        if (inputs.Stages < 1)
        {
            return (CodeInputInvalid, "A pump string has at least one stage.");
        }

        if (inputs.SpecificGravity is <= 0m)
        {
            return (CodeInputInvalid, "The specific gravity must be above zero.");
        }

        if (inputs.SpeedRpm is <= 0m)
        {
            return (CodeInputInvalid, "The pump speed must be above zero.");
        }

        if (inputs.NewSpeedRpm is <= 0m)
        {
            return (CodeInputInvalid, "The affinity speed must be above zero.");
        }

        // The curve has to be a curve: two or more points, strictly increasing in rate, with a positive head.
        var rates = inputs.Curve!.Select(point => point.Rate).ToList();

        if (rates.Any(rate => rate < 0m) || inputs.Curve.Any(point => point.Head < 0m))
        {
            return (CodeCurveInvalid, "A curve point cannot have a negative rate or head.");
        }

        for (var index = 1; index < rates.Count; index++)
        {
            if (rates[index] <= rates[index - 1])
            {
                return (CodeCurveInvalid,
                    "The published curve's rates must increase from point to point, or the head at the duty rate " +
                    "cannot be read from it.");
            }
        }

        // A pump is not selected at zero rate: there is no duty point there, and the engine's NPSH correlation divides
        // by the rate. The shut-off point may stay on the published curve.
        if (inputs.DutyRate <= 0m)
        {
            return (CodeInputInvalid, "The duty rate must be above zero.");
        }

        var minimum = rates.Min();
        var maximum = rates.Max();

        // AC-1 and SG-07-4: a duty point outside the published curve is refused, with the range it was checked against.
        return inputs.DutyRate < minimum || inputs.DutyRate > maximum
            ? (CodeDutyOutsideCurve,
                $"The duty rate {inputs.DutyRate!.Value.ToString("#,0.###", CultureInfo.InvariantCulture)} GPM is " +
                $"outside the published curve, which covers " +
                $"{minimum.ToString("#,0.###", CultureInfo.InvariantCulture)} to " +
                $"{maximum.ToString("#,0.###", CultureInfo.InvariantCulture)} GPM. The pump is not selected at a " +
                "rate its curve does not describe.")
            : null;
    }
}

/// <summary>LFT-03: ESP and pump performance.</summary>
public interface IOgPump
{
    /// <summary>Checks the duty point against the published curve. Nothing is written.</summary>
    Task<OgResult<OgPumpWorkspace>> AnalyseAsync(
        OgPumpInputs inputs,
        CancellationToken cancellationToken = default);

    /// <summary>Saves the analysis with its curve source and stage count (AC-6).</summary>
    Task<OgResult<OgPumpWorkspace>> SaveAsync(
        OgPumpResult result,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The saved duty points; an unavailable history is a state.</summary>
    Task<OgResult<OgPumpWorkspace>> LoadAsync(
        string? wellId = null,
        string? designId = null,
        CancellationToken cancellationToken = default);

    /// <summary>The result retained when its save was not acknowledged.</summary>
    OgPumpPending? Pending { get; }

    /// <summary>Retries the same insert.</summary>
    Task<OgResult<OgPumpWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// LFT-03 over the PumpPerformance module's H-Q, efficiency, power, NPSH and affinity calculations and the
/// <c>PUMP_DESIGN</c> row, through a unit of work.
/// </summary>
/// <remarks>
/// The arithmetic is the owner's: <c>HeadQuantityCalculations</c> for the curve and the interpolation,
/// <c>PowerCalculations</c> for the brake horsepower, <c>EfficiencyCalculations</c> for the overall efficiency,
/// <c>NPSHCalculations</c> for the margin and <c>AffinityLaws</c> for the speed change. What the screen adds is the refusal
/// outside the curve, the module's NPSH minimum, the extrapolation label and the record.
/// </remarks>
public sealed class OgPumpService : IOgPump
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgPumpService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgPumpPending? Pending { get; private set; }

    public async Task<OgResult<OgPumpWorkspace>> AnalyseAsync(
        OgPumpInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (PumpRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgPumpWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var result = Build(inputs);

            var source = _source();
            var history = source is null
                ? []
                : await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgPumpWorkspace>.Ok(Workspace(history, result));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"checking a pump duty point for well {inputs.WellId}",
                "the analysis is not shown",
                FailureSeverity.Error);

            return OgResult<OgPumpWorkspace>.Failed(
                "The pump analysis could not be run.", reference ?? PumpRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgPumpWorkspace>> SaveAsync(
        OgPumpResult result,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPumpWorkspace>.Failed(
                "A saved design must record who saved it.", PumpRules.CodeActorRequired);
        }

        if (result.Curve.Count == 0 || result.Stages < 1)
        {
            return OgResult<OgPumpWorkspace>.Failed(
                "There is no duty point to save; run the analysis first.", PumpRules.CodeNothingToSave);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPumpWorkspace>.Failed(
                "No connection is open, so the design cannot be saved.", PumpRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var unit = OgUow.For<PUMP_DESIGN>(source);
            unit.PrimaryKey = "DESIGN_ID";

            unit.Add(new PUMP_DESIGN
            {
                DESIGN_ID = Guid.NewGuid().ToString("N"),
                WELL_UWI = result.WellId,
                DESIGN_DATE = now,
                PUMP_TYPE = result.PumpType,
                CURVE_SOURCE = result.CurveSource,
                CURVE_POINTS = string.Join("\n", result.Curve.Select(point =>
                    point.Rate.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                    point.Head.ToString("0.##", CultureInfo.InvariantCulture) + "," +
                    (point.Power?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty))),
                STAGES = result.Stages,
                DUTY_RATE = result.Rate,
                DUTY_HEAD = result.Head,
                TOTAL_HEAD = result.TotalHead,
                EFFICIENCY = result.Efficiency,
                BRAKE_HORSEPOWER = result.BrakeHorsepower,
                POWER_KW = result.PowerKilowatts,
                NPSH_AVAILABLE = result.NpshAvailable,
                NPSH_REQUIRED = result.NpshRequired,
                NPSH_MARGIN = result.NpshMargin,
                NPSH_MINIMUM_MARGIN = result.NpshMinimumMargin,
                NPSH_WARNING_IND = result.NpshWarning ? "Y" : "N",
                OPERATING_POINT_VALID = "Y",
                RATE_OUOM = OgPumpResult.RateUnit,
                HEAD_OUOM = OgPumpResult.HeadUnit,
                POWER_OUOM = OgPumpResult.PowerUnit,
                AFFINITY_TEXT = result.AffinityText,
                EXTRAPOLATED_IND = result.Extrapolated ? "Y" : "N",
                NOTES = result.Notes.Count == 0 ? null : string.Join("\n", result.Notes),
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
                Pending = new OgPumpPending(result, actor.Trim());

                return OgResult<OgPumpWorkspace>.Failed(
                    OgUow.Message(committed, "The design was not saved; the original is retained and can be retried."),
                    PumpRules.CodeWriteFailed);
            }

            Pending = null;

            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);
            var saved = result with { Id = string.Empty, Actor = actor.Trim(), RecordedAt = now };

            return OgResult<OgPumpWorkspace>.Ok(Workspace(history, saved));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = new OgPumpPending(result, actor.Trim());

            var reference = _failures.ReportHandled(
                ex,
                $"saving a pump design for well {result.WellId}",
                "the design was not acknowledged; the original is retained and the save can be retried",
                FailureSeverity.Error);

            return OgResult<OgPumpWorkspace>.Failed(
                "The design was not acknowledged; the original is retained and the save can be retried.",
                reference ?? PumpRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPumpWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgPumpWorkspace>.Failed(
                "There is no design waiting to be retried.", PumpRules.CodeNothingPending);
        }

        var retried = await SaveAsync(pending.Result, actor, cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    public async Task<OgResult<OgPumpWorkspace>> LoadAsync(
        string? wellId = null,
        string? designId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgPumpWorkspace>.Failed(
                "No connection is open, so the saved duty points cannot be read.", PumpRules.CodeNoConnection);
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

            return OgResult<OgPumpWorkspace>.Ok(Workspace(history, selected));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the saved pump duty points",
                "the saved duty points are unavailable; new analyses are still allowed",
                FailureSeverity.Degraded);

            return OgResult<OgPumpWorkspace>.Ok(new OgPumpWorkspace(
                [],
                null,
                PumpTypes.All,
                true,
                $"the saved duty points could not be read (reference {reference})",
                CurveSourceNote()));
        }
    }

    // ------------------------------------------------------------------ the analysis

    /// <summary>
    /// The analysis itself: the curve from the owner, the duty point read from it, the power and efficiency from the
    /// owner's formulas, the NPSH margin against the module's minimum, and the affinity result with its extrapolation
    /// label.
    /// </summary>
    public static OgPumpResult Build(OgPumpInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var curve = HeadQuantityCalculations.GenerateHQCurve(
            inputs.Curve.Select(point => (double)point.Rate).ToArray(),
            inputs.Curve.Select(point => (double)point.Head).ToArray(),
            inputs.Curve.Select(point => (double)(point.Power ?? 0m)).ToArray(),
            (double)inputs.SpecificGravity!.Value);

        var dutyRate = (double)inputs.DutyRate!.Value;

        var head = HeadQuantityCalculations.InterpolateHead(curve, dutyRate);
        var specificGravity = (double)inputs.SpecificGravity.Value;

        // The published efficiency at the duty point when the curve states one, otherwise the owner's overall efficiency
        // from the brake horsepower it computes.
        var stated = InterpolateEfficiency(curve, dutyRate);
        var brakeHorsepower = PowerCalculations.CalculateBrakeHorsepower(dutyRate, head, specificGravity);
        var efficiency = stated > 0.0
            ? stated
            : EfficiencyCalculations.CalculateOverallEfficiency(dutyRate, head, brakeHorsepower, specificGravity);

        var notes = new List<string>();

        if (stated > 0.0)
        {
            notes.Add("efficiency is the published value at the duty rate");
        }
        else
        {
            notes.Add("efficiency is the module's overall efficiency, because the curve states none at the duty rate");
        }

        var npshAvailable = NPSHCalculations.CalculateNPSHAvailable(
            (double)inputs.AtmosphericPressure!.Value,
            0.0,
            (double)inputs.VaporPressure!.Value,
            (double)inputs.SuctionHead!.Value,
            (double)inputs.SuctionFriction!.Value);

        var npshRequired = NPSHCalculations.CalculateNPSHRequired(dutyRate, (double)inputs.SpeedRpm!.Value);
        var margin = NPSHCalculations.CalculateNPSHMargin(npshAvailable, npshRequired);
        var minimum = PumpConstants.DefaultNpshSafetyMarginFeet;

        var npshWarning = margin < minimum;

        // AC-3: a speed change is labelled, and marked extrapolated when it leaves the published rate range.
        var affinity = string.Empty;
        var extrapolated = false;

        if (inputs.NewSpeedRpm is { } newSpeed && Math.Abs(newSpeed - inputs.SpeedRpm.Value) > 0.001m)
        {
            var newRate = AffinityLaws.CalculateFlowRateAtNewSpeed(
                dutyRate, (double)inputs.SpeedRpm.Value, (double)newSpeed);
            var newHead = AffinityLaws.CalculateHeadAtNewSpeed(
                head, (double)inputs.SpeedRpm.Value, (double)newSpeed);
            var newPower = AffinityLaws.CalculatePowerAtNewSpeed(
                brakeHorsepower, (double)inputs.SpeedRpm.Value, (double)newSpeed);

            var minimumRate = (double)inputs.Curve.Min(point => point.Rate);
            var maximumRate = (double)inputs.Curve.Max(point => point.Rate);

            extrapolated = newRate < minimumRate || newRate > maximumRate;

            affinity =
                $"at {newSpeed.ToString("#,0.##", CultureInfo.InvariantCulture)} rpm the duty point moves to " +
                $"{newRate.ToString("#,0.###", CultureInfo.InvariantCulture)} GPM at " +
                $"{newHead.ToString("#,0.##", CultureInfo.InvariantCulture)} ft and " +
                $"{newPower.ToString("#,0.##", CultureInfo.InvariantCulture)} hp" +
                (extrapolated
                    ? $" - outside the published {minimumRate.ToString("#,0.###", CultureInfo.InvariantCulture)} to " +
                      $"{maximumRate.ToString("#,0.###", CultureInfo.InvariantCulture)} GPM, so it is extrapolated"
                    : string.Empty);

            notes.Add(extrapolated
                ? "the affinity result lies outside the published curve and is labelled extrapolated"
                : "the affinity result lies inside the published curve");
        }

        var stages = inputs.Stages;

        return new OgPumpResult(
            string.Empty,
            inputs.WellId!.Trim(),
            inputs.PumpType!.Trim().ToUpperInvariant(),
            PumpRules.CurveSource,
            inputs.Curve,
            stages,
            (decimal)dutyRate,
            (decimal)head,
            (decimal)head * stages,
            (decimal)efficiency,
            (decimal)brakeHorsepower,
            (decimal)PowerCalculations.HorsepowerToKilowatts(brakeHorsepower),
            (decimal)npshAvailable,
            (decimal)npshRequired,
            (decimal)margin,
            (decimal)minimum,
            npshWarning,
            extrapolated,
            affinity,
            notes,
            null,
            null);
    }

    /// <summary>The published efficiency at a rate, or zero when the curve states none.</summary>
    private static double InterpolateEfficiency(IReadOnlyList<HeadQuantityPoint> curve, double rate)
    {
        for (var index = 1; index < curve.Count; index++)
        {
            if (rate <= curve[index].FlowRate)
            {
                var span = curve[index].FlowRate - curve[index - 1].FlowRate;

                return span <= 0.0
                    ? curve[index].EFFICIENCY
                    : curve[index - 1].EFFICIENCY +
                      (rate - curve[index - 1].FlowRate) / span *
                      (curve[index].EFFICIENCY - curve[index - 1].EFFICIENCY);
            }
        }

        return curve.Count == 0 ? 0.0 : curve[^1].EFFICIENCY;
    }

    // ------------------------------------------------------------------ the reads

    private static async Task<IReadOnlyList<OgPumpResult>> ReadHistoryAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAllAsync<PUMP_DESIGN>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.DESIGN_ID))
            .OrderByDescending(row => row.RECORDED_AT ?? row.DESIGN_DATE ?? DateTime.MinValue)
            .ThenBy(row => row.DESIGN_ID, StringComparer.Ordinal)
            .Select(ToResult)
            .Take(50)
            .ToList();
    }

    private static OgPumpResult ToResult(PUMP_DESIGN row)
    {
        var curve = string.IsNullOrWhiteSpace(row.CURVE_POINTS)
            ? []
            : row.CURVE_POINTS!
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(line => line.Split(',', StringSplitOptions.TrimEntries))
                .Where(parts => parts.Length >= 2)
                .Select(parts => new OgPumpCurvePoint(
                    decimal.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var rate) ? rate : 0m,
                    decimal.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var head) ? head : 0m,
                    parts.Length > 2 && decimal.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var power)
                        ? power
                        : null))
                .ToList();

        var notes = string.IsNullOrWhiteSpace(row.NOTES)
            ? []
            : row.NOTES!.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new OgPumpResult(
            row.DESIGN_ID ?? string.Empty,
            row.WELL_UWI,
            row.PUMP_TYPE ?? PumpTypes.Centrifugal,
            row.CURVE_SOURCE ?? PumpRules.CurveSource,
            curve,
            row.STAGES,
            row.DUTY_RATE,
            row.DUTY_HEAD,
            row.TOTAL_HEAD,
            row.EFFICIENCY,
            row.BRAKE_HORSEPOWER,
            row.POWER_KW,
            row.NPSH_AVAILABLE,
            row.NPSH_REQUIRED,
            row.NPSH_MARGIN,
            row.NPSH_MINIMUM_MARGIN,
            string.Equals(row.NPSH_WARNING_IND, "Y", StringComparison.OrdinalIgnoreCase),
            string.Equals(row.EXTRAPOLATED_IND, "Y", StringComparison.OrdinalIgnoreCase),
            row.AFFINITY_TEXT ?? string.Empty,
            notes,
            row.ACTOR,
            row.RECORDED_AT ?? row.DESIGN_DATE);
    }

    private static OgPumpWorkspace Workspace(IReadOnlyList<OgPumpResult> history, OgPumpResult? selected) =>
        new(history, selected, PumpTypes.All, false, null, CurveSourceNote());

    private static string CurveSourceNote() =>
        "The curve is the published H-Q curve the engineer enters: this repository has no pump-curve catalog, so " +
        "the source recorded on every design is ENTERED.";
}
