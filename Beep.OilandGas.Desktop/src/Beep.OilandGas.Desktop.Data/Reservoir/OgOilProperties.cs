using System.Globalization;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.OilProperties.Calculations;
using Beep.OilandGas.OilProperties.Constants;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;

/// <summary>The correlations the oil engine implements, named once so the view carries no literal (RES-02 section 9).</summary>
/// <remarks>
/// The owning service passes the string <c>"STANDING_BEGGS"</c> around; there is no reference-code table for oil
/// correlations in this repository, so the names are stated here - exactly the relations
/// <c>OilPropertyCalculator</c> implements - and the page reads this type instead of hard-coding them.
/// </remarks>
public static class OilCorrelations
{
    public const string BubblePointAndGor = "Standing (bubble point, Rs, Bo)";
    public const string FormationVolumeFactor = "Standing (Bo)";
    public const string Viscosity = "Beggs-Robinson (dead and saturated)";

    /// <summary>The one combined name recorded on a result, in the same shape the Service uses.</summary>
    public const string Combined = "STANDING_BEGGS";
}

/// <summary>The temperature units the oil screen accepts; the calculator works in Fahrenheit.</summary>
public static class OilUnits
{
    public const string Fahrenheit = "degF";
    public const string Rankine = "degR";

    public static IReadOnlyList<string> TemperatureUnits { get; } = [Fahrenheit, Rankine];

    public static bool IsTemperatureUnit(string? unit) =>
        !string.IsNullOrWhiteSpace(unit)
        && TemperatureUnits.Contains(unit.Trim(), StringComparer.OrdinalIgnoreCase);

    /// <summary>Converts to the Fahrenheit the calculator's arguments are in.</summary>
    public static decimal ToFahrenheit(decimal value, string unit) => unit.Trim().ToUpperInvariant() switch
    {
        "DEGF" => value,
        "DEGR" => OilPropertyUnits.RankineToFahrenheit(value),
        _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Unknown temperature unit."),
    };

    public static string Describe(decimal? value, string? unit) =>
        value is { } amount
            ? $"{amount.ToString("#,0.####", CultureInfo.InvariantCulture)} {unit ?? string.Empty}".Trim()
            : "(not recorded)";
}

/// <summary>The inputs of one oil screening run, as the page enters them (RES-02).</summary>
public sealed record OgOilPropertyInputs(
    decimal? ApiGravity,
    decimal? GasSpecificGravity,
    bool UseDefaultGasGravity,
    decimal? Temperature,
    string? TemperatureUnit,
    decimal? Pressure,
    decimal? SolutionGor,
    string? Correlation);

/// <summary>One black-oil screening result, run or saved (RES-02).</summary>
public sealed record OgOilPropertyResult(
    string Id,
    string Status,
    decimal? ApiGravity,
    decimal? GasSpecificGravity,
    bool GasGravityAssumed,
    decimal? Temperature,
    string? TemperatureUnit,
    decimal? TemperatureF,
    decimal? Pressure,
    decimal? SolutionGor,
    decimal? BubblePoint,
    decimal? FormationVolumeFactor,
    decimal? DeadOilViscosity,
    decimal? SaturatedViscosity,
    decimal? Density,
    string Correlation,
    bool OutsideRange,
    IReadOnlyList<string> RangeNotes,
    string? FailureCode,
    string? FailureMessage,
    string? Actor,
    DateTime? RecordedAt)
{
    public const string OkStatus = "OK";
    public const string FailedStatus = "FAILED";

    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    public bool IsFailed => string.Equals(Status, FailedStatus, StringComparison.OrdinalIgnoreCase);

    public string StatusText => Status.ToLowerInvariant();

    /// <summary>AC-2: a result outside the correlation's range is a result, marked - not a refusal.</summary>
    public string RangeText => OutsideRange
        ? "outside the correlation's validity range: " + string.Join("; ", RangeNotes)
        : "inside the correlation's validity range";

    public string GasGravityText => GasGravityAssumed
        ? $"{GasSpecificGravity?.ToString("0.####", CultureInfo.InvariantCulture)} (the default, accepted explicitly)"
        : GasSpecificGravity?.ToString("0.####", CultureInfo.InvariantCulture) ?? "(not recorded)";

    public string BubblePointText => Number(BubblePoint, "#,0.##", "psia");
    public string SolutionGorText => Number(SolutionGor, "0.##", "scf/STB");
    public string FormationVolumeFactorText => Number(FormationVolumeFactor, "0.######", "rb/STB");
    public string DeadOilViscosityText => Number(DeadOilViscosity, "0.####", "cp");
    public string SaturatedViscosityText => Number(SaturatedViscosity, "0.####", "cp");
    public string DensityText => Number(Density, "0.####", "lb/ft3");
    public string PressureText => Number(Pressure, "#,0.##", "psia");
    public string TemperatureText => OilUnits.Describe(Temperature, TemperatureUnit);

    public string FailureText => IsFailed
        ? $"{FailureCode ?? OilPropertyRules.CodeScreeningRefused}: {FailureMessage ?? "the screening did not complete"}"
        : "no failure";

    public string SavedText => IsSaved
        ? $"saved by {Actor ?? "(unknown)"} on " +
          $"{RecordedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "unknown"}"
        : "not saved";

    public string Describe() => IsFailed
        ? $"FAILED · {FailureText} · {SavedText}"
        : $"Pb {BubblePointText} · Rs {SolutionGorText} · Bo {FormationVolumeFactorText} · " +
          $"dead viscosity {DeadOilViscosityText} · saturated viscosity {SaturatedViscosityText} · " +
          $"density {DensityText} · {Correlation} · {TemperatureText} · {PressureText} · " +
          $"gas gravity {GasGravityText} · {RangeText} · {SavedText}";

    private string Number(decimal? value, string format, string unit) => IsFailed
        ? "not computed"
        : value is { } amount
            ? amount.ToString(format, CultureInfo.InvariantCulture) + " " + unit
            : "not computed";
}

/// <summary>The workspace: the saved history and the screening banner every state carries (RES-02 AC-4).</summary>
public sealed record OgOilPropertyWorkspace(
    IReadOnlyList<OgOilPropertyResult> History,
    IReadOnlyList<string> TemperatureUnits,
    string ScreeningBanner,
    string CorrelationsText,
    decimal DefaultGasSpecificGravity,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string HistoryText => History.Count == 0
        ? "No oil screening result is saved yet."
        : string.Join(Environment.NewLine, History.Select(run => run.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved history could not be read; new runs are still allowed"
        : $"{History.Count.ToString(CultureInfo.InvariantCulture)} saved run(s)";

    public string DefaultGasGravityText =>
        $"the default gas gravity is {DefaultGasSpecificGravity.ToString("0.####", CultureInfo.InvariantCulture)}, " +
        "and it is applied only when the defaults are accepted explicitly";
}

/// <summary>The oil screening rules, stated once so the page, the service and the tests agree.</summary>
public static class OilPropertyRules
{
    /// <summary>AC-4: the sentence that must be on the page in every state, including a failure.</summary>
    public const string ScreeningBanner =
        "Black-oil screening correlations, not a PVT report: these are fitted relations with a stated range, so a " +
        "result here supports a screening decision and does not replace a laboratory study or an equation-of-state model.";

    public const string CodeNoConnection = "oil-property-no-connection";
    public const string CodeReadFailed = "oil-property-read-failed";
    public const string CodeWriteFailed = "oil-property-write-failed";
    public const string CodeActorRequired = "oil-property-actor-required";
    public const string CodeApiRequired = "oil-property-api-required";
    public const string CodeApiInvalid = "oil-property-api-invalid";
    public const string CodeGasGravityRequired = "oil-property-gas-gravity-required";
    public const string CodeGasGravityInvalid = "oil-property-gas-gravity-invalid";
    public const string CodeTemperatureRequired = "oil-property-temperature-required";
    public const string CodeTemperatureUnitInvalid = "oil-property-temperature-unit-invalid";
    public const string CodePressureRequired = "oil-property-pressure-required";
    public const string CodeGorRequired = "oil-property-gor-required";
    public const string CodeCorrelationUnknown = "oil-property-correlation-unknown";
    public const string CodeBelowBubblePoint = "oil-property-pressure-below-bubble-point";
    public const string CodeScreeningRefused = "oil-property-screening-refused";
    public const string CodeNotRun = "oil-property-nothing-to-save";

    /// <summary>The state table's "missing input": what the run still needs, named.</summary>
    public static IReadOnlyList<string> Missing(OgOilPropertyInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = new List<string>();

        if (inputs.ApiGravity is null)
        {
            missing.Add("API gravity");
        }

        // AC-3: a gas gravity is never supplied silently - either it is given, or the operator accepts the default.
        if (inputs.GasSpecificGravity is null && !inputs.UseDefaultGasGravity)
        {
            missing.Add("gas specific gravity (or an explicit acceptance of the default)");
        }

        if (inputs.Temperature is null)
        {
            missing.Add("temperature");
        }

        if (string.IsNullOrWhiteSpace(inputs.TemperatureUnit))
        {
            missing.Add("temperature unit");
        }

        if (inputs.Pressure is null)
        {
            missing.Add("pressure");
        }

        if (inputs.SolutionGor is null)
        {
            missing.Add("solution gas-oil ratio");
        }

        return missing;
    }

    /// <summary>The pre-run validation; the bubble-point consistency check needs the run, so it lives in the service.</summary>
    public static (string Code, string Message)? Validate(OgOilPropertyInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            var code = missing.Contains("API gravity")
                ? CodeApiRequired
                : missing.Contains("temperature") || missing.Contains("temperature unit")
                    ? CodeTemperatureRequired
                    : missing.Contains("pressure")
                        ? CodePressureRequired
                        : missing.Contains("solution gas-oil ratio")
                            ? CodeGorRequired
                            : CodeGasGravityRequired;

            return (code, $"The screening needs {string.Join(", ", missing)}.");
        }

        if (inputs.ApiGravity is < OilPropertyConstants.MinimumApiGravity
            or > OilPropertyConstants.MaximumApiGravity)
        {
            return (CodeApiInvalid,
                $"API gravity {inputs.ApiGravity} is outside " +
                $"{OilPropertyConstants.MinimumApiGravity} to {OilPropertyConstants.MaximumApiGravity}.");
        }

        if (inputs.UseDefaultGasGravity && inputs.GasSpecificGravity is null)
        {
            // Accepted: the run proceeds with the constant and records that it did.
        }
        else if (inputs.GasSpecificGravity is <= 0m)
        {
            return (CodeGasGravityInvalid, "Gas specific gravity must be greater than zero.");
        }

        if (!OilUnits.IsTemperatureUnit(inputs.TemperatureUnit))
        {
            return (CodeTemperatureUnitInvalid,
                $"'{inputs.TemperatureUnit}' is not a temperature unit; use {string.Join(", ", OilUnits.TemperatureUnits)}.");
        }

        if (inputs.Pressure is <= 0m)
        {
            return (CodePressureRequired, "Pressure must be greater than zero.");
        }

        if (inputs.SolutionGor is < OilPropertyConstants.MinimumSolutionGOR)
        {
            return (CodeGorRequired, "The solution gas-oil ratio cannot be negative.");
        }

        return string.IsNullOrWhiteSpace(inputs.Correlation)
               || !string.Equals(inputs.Correlation.Trim(), OilCorrelations.Combined, StringComparison.OrdinalIgnoreCase)
            ? (CodeCorrelationUnknown,
                $"'{inputs.Correlation}' is not an oil correlation set; this screen implements " +
                $"{OilCorrelations.Combined} ({OilCorrelations.BubblePointAndGor} with {OilCorrelations.Viscosity}).")
            : null;
    }

    /// <summary>
    /// AC-2: the ranges the owner's constants state. A result outside one is still a result, and it is marked.
    /// </summary>
    public static IReadOnlyList<string> RangeNotes(decimal bubblePoint, decimal bo, decimal dead, decimal saturated)
    {
        var notes = new List<string>();

        if (bubblePoint > OilPropertyConstants.MaximumBubblePointPressure)
        {
            notes.Add(
                $"the bubble point {bubblePoint.ToString("0.##", CultureInfo.InvariantCulture)} psia is above the " +
                "correlation's recorded maximum");
        }

        if (bo < OilPropertyConstants.MinimumFormationVolumeFactor
            || bo > OilPropertyConstants.MaximumFormationVolumeFactor)
        {
            notes.Add(
                $"Bo {bo.ToString("0.######", CultureInfo.InvariantCulture)} is outside " +
                $"{OilPropertyConstants.MinimumFormationVolumeFactor.ToString("0.##", CultureInfo.InvariantCulture)} to " +
                $"{OilPropertyConstants.MaximumFormationVolumeFactor.ToString("0.##", CultureInfo.InvariantCulture)}");
        }

        if (saturated < OilPropertyConstants.MinimumViscosity
            || saturated > OilPropertyConstants.MaximumViscosity)
        {
            notes.Add(
                $"the saturated viscosity {saturated.ToString("0.####", CultureInfo.InvariantCulture)} cp is outside " +
                "the engine's accepted viscosity range");
        }

        if (dead < OilPropertyConstants.MinimumViscosity || dead > OilPropertyConstants.MaximumViscosity)
        {
            notes.Add(
                $"the dead-oil viscosity {dead.ToString("0.####", CultureInfo.InvariantCulture)} cp is outside the " +
                "engine's accepted viscosity range");
        }

        return notes;
    }
}

/// <summary>RES-02: oil properties (black-oil screening).</summary>
public interface IOgOilProperties
{
    /// <summary>Runs the screening. Nothing is written: saving is a separate command.</summary>
    Task<OgResult<OgOilPropertyResult>> RunAsync(OgOilPropertyInputs inputs, CancellationToken cancellationToken = default);

    /// <summary>Persists a result with its inputs and its correlation (AC-5).</summary>
    Task<OgResult<OgOilPropertyWorkspace>> SaveAsync(
        OgOilPropertyResult result,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The saved history and the choices on offer; an unavailable history is a state.</summary>
    Task<OgResult<OgOilPropertyWorkspace>> LoadAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// RES-02 over the OilProperties-owned calculators and the <c>OIL_PROPERTY_RESULT</c> row, through a unit of work.
/// </summary>
/// <remarks>
/// The correlations are the owning project's: <c>OilPropertyCalculator</c> is called directly, so this screen adds no
/// second implementation. What it adds is the gas-gravity rule (a default is never applied silently), the
/// bubble-point consistency check the state table asks for, the out-of-range marking and the saved record.
/// </remarks>
public sealed class OgOilPropertiesService : IOgOilProperties
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgOilPropertiesService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public Task<OgResult<OgOilPropertyResult>> RunAsync(
        OgOilPropertyInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        cancellationToken.ThrowIfCancellationRequested();

        if (OilPropertyRules.Validate(inputs) is { } refusal)
        {
            return Task.FromResult(OgResult<OgOilPropertyResult>.Failed(refusal.Message, refusal.Code));
        }

        var api = inputs.ApiGravity!.Value;
        var gravity = inputs.GasSpecificGravity ?? OilPropertyConstants.DefaultGasSpecificGravity;
        var assumed = inputs.GasSpecificGravity is null && inputs.UseDefaultGasGravity;
        var temperatureF = OilUnits.ToFahrenheit(inputs.Temperature!.Value, inputs.TemperatureUnit!);
        var pressure = inputs.Pressure!.Value;
        var gor = inputs.SolutionGor!.Value;

        try
        {
            var bubblePoint = OilPropertyCalculator.CalculateBubblePointPressure_Standing(gor, gravity, api, temperatureF);

            // The state table's inconsistent inputs: below the bubble point free gas exists, so the saturated
            // correlations do not describe this oil and the run is refused with both numbers named.
            if (pressure < bubblePoint)
            {
                var message =
                    $"the pressure {pressure.ToString("#,0.##", CultureInfo.InvariantCulture)} psia is below the " +
                    $"bubble point {bubblePoint.ToString("#,0.##", CultureInfo.InvariantCulture)} psia computed from " +
                    $"Rs {gor.ToString("#,0.##", CultureInfo.InvariantCulture)} scf/STB, so the saturated Rs and Bo " +
                    "correlations do not apply: raise the pressure above the bubble point or lower the solution GOR.";

                return Task.FromResult(OgResult<OgOilPropertyResult>.Failed(
                    message, OilPropertyRules.CodeBelowBubblePoint));
            }

            var gorAtPressure = OilPropertyCalculator.CalculateSolutionGOR_Standing(pressure, gravity, api, temperatureF);
            var bo = OilPropertyCalculator.CalculateOilFVF_Standing(gor, gravity, api, temperatureF);
            var dead = OilPropertyCalculator.CalculateDeadOilViscosity_BeggsRobinson(api, temperatureF);
            var saturated = OilPropertyCalculator.CalculateSaturatedViscosity_BeggsRobinson(dead, gor);

            // Black-oil density: rho = 62.4 * Go / Bo, using the owner's water density rather than a literal.
            var specificGravity = OilPropertyCalculator.OilSpecificGravityFromApi(api);
            var density = OilPropertyConstants.WaterDensity * specificGravity / bo;

            var notes = OilPropertyRules.RangeNotes(bubblePoint, bo, dead, saturated);

            return Task.FromResult(OgResult<OgOilPropertyResult>.Ok(new OgOilPropertyResult(
                string.Empty,
                OgOilPropertyResult.OkStatus,
                api,
                gravity,
                assumed,
                inputs.Temperature,
                inputs.TemperatureUnit!.Trim(),
                temperatureF,
                pressure,
                gorAtPressure,
                bubblePoint,
                bo,
                dead,
                saturated,
                density,
                OilCorrelations.Combined,
                notes.Count > 0,
                notes,
                null,
                null,
                null,
                null)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"screening oil properties at {api.ToString("0.##", CultureInfo.InvariantCulture)} API",
                "the screening is shown as failed and no result is reported",
                FailureSeverity.Degraded);

            return Task.FromResult(OgResult<OgOilPropertyResult>.Failed(
                $"the correlations did not complete the screening (reference {reference})",
                OilPropertyRules.CodeScreeningRefused));
        }
    }

    public async Task<OgResult<OgOilPropertyWorkspace>> SaveAsync(
        OgOilPropertyResult result,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgOilPropertyWorkspace>.Failed(
                "A saved result must record who saved it.", OilPropertyRules.CodeActorRequired);
        }

        if (result.IsFailed || result.BubblePoint is null)
        {
            return OgResult<OgOilPropertyWorkspace>.Failed(
                "There is no completed screening to save; run it first.", OilPropertyRules.CodeNotRun);
        }

        if (result.IsSaved)
        {
            return await LoadAsync(cancellationToken).ConfigureAwait(false);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgOilPropertyWorkspace>.Failed(
                "No connection is open, so the result cannot be saved.", OilPropertyRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var unit = OgUow.For<OIL_PROPERTY_RESULT>(source);
            unit.PrimaryKey = "CALCULATION_ID";

            unit.Add(new OIL_PROPERTY_RESULT
            {
                CALCULATION_ID = Guid.NewGuid().ToString("N"),
                OIL_COMPOSITION_ID = null,
                PRESSURE = result.Pressure,
                TEMPERATURE = result.TemperatureF,
                FORMATION_VOLUME_FACTOR = result.FormationVolumeFactor,
                DENSITY = result.Density,
                VISCOSITY = result.SaturatedViscosity,
                COMPRESSIBILITY = null,
                CALCULATION_DATE = now,
                CORRELATION_METHOD = result.Correlation,
                API_GRAVITY = result.ApiGravity,
                GAS_SPECIFIC_GRAVITY = result.GasSpecificGravity,
                GAS_GRAVITY_ASSUMED_IND = result.GasGravityAssumed ? "Y" : "N",
                SOLUTION_GOR = result.SolutionGor,
                BUBBLE_POINT_PRESSURE = result.BubblePoint,
                DEAD_OIL_VISCOSITY = result.DeadOilViscosity,
                OUTSIDE_RANGE_IND = result.OutsideRange ? "Y" : "N",
                RANGE_NOTES = result.RangeNotes.Count == 0 ? null : string.Join("\n", result.RangeNotes),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgOilPropertyWorkspace>.Failed(
                    OgUow.Message(committed, "The result was not saved."), OilPropertyRules.CodeWriteFailed);
            }

            return await LoadAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "saving an oil screening result",
                "the result was not saved",
                FailureSeverity.Error);

            return OgResult<OgOilPropertyWorkspace>.Failed(
                "The result could not be saved.", reference ?? OilPropertyRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgOilPropertyWorkspace>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgOilPropertyWorkspace>.Failed(
                "No connection is open, so the oil screening history cannot be read.",
                OilPropertyRules.CodeNoConnection);
        }

        try
        {
            var rows = await OgUow.ReadAllAsync<OIL_PROPERTY_RESULT>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var history = rows
                .Where(row => !string.IsNullOrWhiteSpace(row.CALCULATION_ID))
                .OrderByDescending(row => row.CALCULATION_DATE ?? DateTime.MinValue)
                .ThenBy(row => row.CALCULATION_ID, StringComparer.Ordinal)
                .Select(ToResult)
                .Take(50)
                .ToList();

            return OgResult<OgOilPropertyWorkspace>.Ok(Workspace(history, false, null));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the oil screening history",
                "the saved history is unavailable; new runs are still allowed",
                FailureSeverity.Degraded);

            return OgResult<OgOilPropertyWorkspace>.Ok(Workspace(
                [],
                true,
                $"the saved history could not be read (reference {reference})"));
        }
    }

    private static OgOilPropertyWorkspace Workspace(
        IReadOnlyList<OgOilPropertyResult> history,
        bool failed,
        string? reason) =>
        new(
            history,
            OilUnits.TemperatureUnits,
            OilPropertyRules.ScreeningBanner,
            OilCorrelations.Combined,
            OilPropertyConstants.DefaultGasSpecificGravity,
            failed,
            reason);

    private static OgOilPropertyResult ToResult(OIL_PROPERTY_RESULT row)
    {
        var temperatureF = row.TEMPERATURE;

        return new OgOilPropertyResult(
            row.CALCULATION_ID ?? string.Empty,
            OgOilPropertyResult.OkStatus,
            row.API_GRAVITY,
            row.GAS_SPECIFIC_GRAVITY,
            string.Equals(row.GAS_GRAVITY_ASSUMED_IND, "Y", StringComparison.OrdinalIgnoreCase),
            temperatureF,
            OilUnits.Fahrenheit,
            temperatureF,
            row.PRESSURE,
            row.SOLUTION_GOR,
            row.BUBBLE_POINT_PRESSURE,
            row.FORMATION_VOLUME_FACTOR,
            row.DEAD_OIL_VISCOSITY,
            row.VISCOSITY,
            row.DENSITY,
            row.CORRELATION_METHOD ?? OilCorrelations.Combined,
            string.Equals(row.OUTSIDE_RANGE_IND, "Y", StringComparison.OrdinalIgnoreCase),
            string.IsNullOrWhiteSpace(row.RANGE_NOTES)
                ? []
                : row.RANGE_NOTES!
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            null,
            null,
            row.ROW_CREATED_BY,
            row.CALCULATION_DATE);
    }
}
