using System.Globalization;
using Beep.OilandGas.GasProperties.Calculations;
using Beep.OilandGas.GasProperties.Constants;
using Beep.OilandGas.GasProperties.Validation;
using Beep.OilandGas.Models.Data.Common;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;

/// <summary>
/// The unit conversions RES-01 needs, in one place.
/// </summary>
/// <remarks>
/// The engine works in psia and degrees Rankine and its constants must not change, so the screen converts. The application
/// has no unit catalog to call (DAT-04 is the PPDM table workbench, not a unit service), so the factors are stated here,
/// once, and the run records both the typed value and the converted one - which is what makes the result reproducible
/// even if this table is ever revised.
/// </remarks>
public static class GasUnits
{
    public const string Psia = "psia";
    public const string Kilopascal = "kPa";
    public const string Bar = "bar";
    public const string Megapascal = "MPa";

    public const string Rankine = "degR";
    public const string Fahrenheit = "degF";
    public const string Celsius = "degC";

    /// <summary>psia per unit.</summary>
    private static readonly IReadOnlyDictionary<string, decimal> PressureFactors =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            [Psia] = 1m,
            [Kilopascal] = 0.1450377377m,
            [Bar] = 14.50377377m,
            [Megapascal] = 145.0377377m,
        };

    public static IReadOnlyList<string> PressureUnits { get; } = [Psia, Kilopascal, Bar, Megapascal];

    public static IReadOnlyList<string> TemperatureUnits { get; } = [Rankine, Fahrenheit, Celsius];

    public static bool IsPressureUnit(string? unit) =>
        !string.IsNullOrWhiteSpace(unit) && PressureFactors.ContainsKey(unit.Trim());

    public static bool IsTemperatureUnit(string? unit) =>
        !string.IsNullOrWhiteSpace(unit) && TemperatureUnits.Contains(unit.Trim(), StringComparer.OrdinalIgnoreCase);

    /// <summary>The factor a pressure unit is converted to psia with, so the run can record how it was converted.</summary>
    public static decimal? PressureFactor(string? unit) =>
        IsPressureUnit(unit) ? PressureFactors[unit!.Trim()] : null;

    public static decimal ToPsia(decimal value, string unit) =>
        value * (PressureFactor(unit) ?? throw new ArgumentOutOfRangeException(nameof(unit), unit, "Unknown pressure unit."));

    /// <summary>Converts a temperature to degrees Rankine. Fahrenheit and Celsius are offsets, not factors only.</summary>
    public static decimal ToRankine(decimal value, string unit) => unit.Trim().ToUpperInvariant() switch
    {
        "DEGR" => value,
        "DEGF" => value + GasPropertiesConstants.FahrenheitToRankine,
        "DEGC" => value * 1.8m + GasPropertiesConstants.CelsiusToRankine,
        _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Unknown temperature unit."),
    };

    public static string Describe(decimal? value, string? unit) =>
        value is { } amount
            ? $"{amount.ToString("#,0.####", CultureInfo.InvariantCulture)} {unit ?? string.Empty}".Trim()
            : "(not recorded)";
}

/// <summary>The correlations the gas engine supports, named once for the service and the page.</summary>
/// <remarks>
/// The feature asks for correlation names to come from reference tables rather than literals. There is no
/// <c>R_GAS_CORRELATION</c> table in this repository, so the supported set is stated here - it is exactly the set
/// <c>GasPropertiesService.CalculateZFactor</c> switches on - and the page reads this list instead of hard-coding strings.
/// </remarks>
public static class GasCorrelations
{
    public const string StandingKatz = "Standing-Katz";
    public const string HallYarborough = "Hall-Yarborough";
    public const string BrillBeggs = "Brill-Beggs";

    public const string CarrKobayashiBurrows = "Carr-Kobayashi-Burrows";
    public const string LeeGonzalezEakin = "Lee-Gonzalez-Eakin";

    public static IReadOnlyList<string> Z { get; } = [StandingKatz, HallYarborough, BrillBeggs];

    public static IReadOnlyList<string> Viscosity { get; } = [CarrKobayashiBurrows, LeeGonzalezEakin];
}

/// <summary>The inputs of one gas-property run, as the page enters them (RES-01).</summary>
public sealed record OgGasPropertyInputs(
    decimal? Pressure,
    string? PressureUnit,
    decimal? Temperature,
    string? TemperatureUnit,
    decimal? SpecificGravity,
    string? ZCorrelation,
    string? ViscosityCorrelation);

/// <summary>One gas-property calculation, run or saved (RES-01).</summary>
public sealed record OgGasPropertyRun(
    string Id,
    string Status,
    decimal? Pressure,
    string? PressureUnit,
    decimal? Temperature,
    string? TemperatureUnit,
    decimal? PressurePsia,
    decimal? TemperatureRankine,
    decimal? SpecificGravity,
    string ZCorrelation,
    string ViscosityCorrelation,
    decimal? ZFactor,
    decimal? Viscosity,
    decimal? Density,
    decimal? FormationVolumeFactor,
    decimal? PseudoPressure,
    IReadOnlyList<string> Warnings,
    string? FailureCode,
    string? FailureMessage,
    string? Actor,
    DateTime? RecordedAt)
{
    public const string OkStatus = "OK";
    public const string FailedStatus = "FAILED";

    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    /// <summary>AC-3: a failed calculation is a state of the record, and no result is shown for it.</summary>
    public bool IsFailed => string.Equals(Status, FailedStatus, StringComparison.OrdinalIgnoreCase);

    public string StatusText => Status.ToLowerInvariant();

    /// <summary>AC-2: the correlation is displayed with the result, not only stored.</summary>
    public string CorrelationText => $"Z: {ZCorrelation} · viscosity: {ViscosityCorrelation}";

    public string PressureText => GasUnits.Describe(Pressure, PressureUnit);

    public string TemperatureText => GasUnits.Describe(Temperature, TemperatureUnit);

    /// <summary>The pair the engine actually used, so the conversion is visible beside the typed value.</summary>
    public string EngineText =>
        $"engine inputs: {GasUnits.Describe(PressurePsia, GasUnits.Psia)} · " +
        $"{GasUnits.Describe(TemperatureRankine, GasUnits.Rankine)} · specific gravity {SpecificGravity?.ToString("0.####", CultureInfo.InvariantCulture) ?? "(not recorded)"}";

    public string ZText => Result(ZFactor, "0.######");

    public string ViscosityText => Result(Viscosity, "0.######", "cp");

    public string DensityText => Result(Density, "0.######", "lb/ft3");

    public string FormationVolumeFactorText => Result(FormationVolumeFactor, "0.######", "ft3/scf");

    public string PseudoPressureText => Result(PseudoPressure, "#,0.####", "psi2/cp");

    public string WarningsText => Warnings.Count == 0 ? "no applicability warning" : string.Join(" · ", Warnings);

    public string FailureText => IsFailed
        ? $"{FailureCode ?? "gas-property-run-failed"}: {FailureMessage ?? "the calculation did not complete"}"
        : "no failure";

    public string SavedText => IsSaved
        ? $"saved by {Actor ?? "(unknown)"} on " +
          $"{RecordedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "unknown"}"
        : "not saved";

    public string Describe() => IsFailed
        ? $"FAILED · {FailureText} · {CorrelationText} · {PressureText} · {TemperatureText} · {SavedText}"
        : $"Z {ZText} · viscosity {ViscosityText} · density {DensityText} · Bg {FormationVolumeFactorText} · " +
          $"pseudo-pressure {PseudoPressureText} · {CorrelationText} · {PressureText} · {TemperatureText} · {SavedText}";

    private string Result(decimal? value, string format, string? unit = null) => IsFailed
        ? "failed"
        : value is { } amount
            ? amount.ToString(format, CultureInfo.InvariantCulture) + (unit is null ? string.Empty : " " + unit)
            : "(not computed)";
}

/// <summary>The workspace: the saved history and the choices on offer (RES-01).</summary>
public sealed record OgGasPropertyWorkspace(
    IReadOnlyList<OgGasPropertyRun> History,
    IReadOnlyList<string> ZCorrelations,
    IReadOnlyList<string> ViscosityCorrelations,
    IReadOnlyList<string> PressureUnits,
    IReadOnlyList<string> TemperatureUnits,
    bool HistoryFailed,
    string? HistoryReason)
{
    /// <summary>AC-6: a run made with the same inputs twice is the same result, so this is the determinism the page states.</summary>
    public string DeterminismText =>
        "the same inputs always produce the same result: the engine is arithmetic with no random or time-dependent term";

    public string HistoryText => History.Count == 0
        ? "No gas-property result is saved yet."
        : string.Join(Environment.NewLine, History.Select(run => run.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved history could not be read; new runs are still allowed"
        : $"{History.Count.ToString(CultureInfo.InvariantCulture)} saved run(s)";

    public string CorrelationsText => string.Join(", ", ZCorrelations);
}

/// <summary>The gas-property rules, stated once so the page, the service and the tests agree.</summary>
public static class GasPropertyRules
{
    public const string CodeNoConnection = "gas-property-no-connection";
    public const string CodeReadFailed = "gas-property-read-failed";
    public const string CodeWriteFailed = "gas-property-write-failed";
    public const string CodeActorRequired = "gas-property-actor-required";
    public const string CodePressureRequired = "gas-property-pressure-required";
    public const string CodeTemperatureRequired = "gas-property-temperature-required";
    public const string CodeGravityRequired = "gas-property-gravity-required";
    public const string CodeCorrelationRequired = "gas-property-correlation-required";
    public const string CodeCorrelationUnknown = "gas-property-correlation-unknown";
    public const string CodePressureUnitInvalid = "gas-property-pressure-unit-invalid";
    public const string CodeTemperatureUnitInvalid = "gas-property-temperature-unit-invalid";
    public const string CodeCalculationRefused = "gas-property-calculation-refused";
    public const string CodeNotRun = "gas-property-nothing-to-save";

    /// <summary>AC-1 and the state table: what is still missing, named, so the run button can be disabled with a reason.</summary>
    public static IReadOnlyList<string> Missing(OgGasPropertyInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = new List<string>();

        if (inputs.Pressure is null)
        {
            missing.Add("pressure");
        }

        if (string.IsNullOrWhiteSpace(inputs.PressureUnit))
        {
            missing.Add("pressure unit");
        }

        if (inputs.Temperature is null)
        {
            missing.Add("temperature");
        }

        if (string.IsNullOrWhiteSpace(inputs.TemperatureUnit))
        {
            missing.Add("temperature unit");
        }

        if (inputs.SpecificGravity is null)
        {
            missing.Add("specific gravity");
        }

        if (string.IsNullOrWhiteSpace(inputs.ZCorrelation))
        {
            missing.Add("Z correlation");
        }

        if (string.IsNullOrWhiteSpace(inputs.ViscosityCorrelation))
        {
            missing.Add("viscosity correlation");
        }

        return missing;
    }

    /// <summary>
    /// AC-4 and the state table's unit mismatch: a unit outside the catalog is refused and the compatible units are named.
    /// </summary>
    public static (string Code, string Message)? Validate(OgGasPropertyInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            var code = missing.Contains("pressure")
                ? CodePressureRequired
                : missing.Contains("temperature")
                    ? CodeTemperatureRequired
                    : missing.Contains("specific gravity")
                        ? CodeGravityRequired
                        : CodeCorrelationRequired;

            return (code, $"The run needs {string.Join(", ", missing)}.");
        }

        if (!GasUnits.IsPressureUnit(inputs.PressureUnit))
        {
            return (CodePressureUnitInvalid,
                $"'{inputs.PressureUnit}' is not a pressure unit; use {string.Join(", ", GasUnits.PressureUnits)}.");
        }

        if (!GasUnits.IsTemperatureUnit(inputs.TemperatureUnit))
        {
            return (CodeTemperatureUnitInvalid,
                $"'{inputs.TemperatureUnit}' is not a temperature unit; use {string.Join(", ", GasUnits.TemperatureUnits)}.");
        }

        if (!GasCorrelations.Z.Contains(inputs.ZCorrelation!.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return (CodeCorrelationUnknown,
                $"'{inputs.ZCorrelation}' is not a Z correlation; use {string.Join(", ", GasCorrelations.Z)}.");
        }

        return GasCorrelations.Viscosity.Contains(inputs.ViscosityCorrelation!.Trim(), StringComparer.OrdinalIgnoreCase)
            ? null
            : (CodeCorrelationUnknown,
                $"'{inputs.ViscosityCorrelation}' is not a viscosity correlation; use " +
                $"{string.Join(", ", GasCorrelations.Viscosity)}.");
    }
}

/// <summary>RES-01: gas properties.</summary>
public interface IOgGasProperties
{
    /// <summary>Runs the calculation. Nothing is written: saving is a separate command (the command table).</summary>
    Task<OgResult<OgGasPropertyRun>> RunAsync(OgGasPropertyInputs inputs, CancellationToken cancellationToken = default);

    /// <summary>Persists a run, with its inputs, its correlations and the actor (AC-5).</summary>
    Task<OgResult<OgGasPropertyWorkspace>> SaveAsync(
        OgGasPropertyRun run,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The saved history and the choices on offer. An unavailable history is a state, not a failure of the page.</summary>
    Task<OgResult<OgGasPropertyWorkspace>> LoadAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// RES-01 over the GasProperties-owned calculators and the <c>GAS_PROPERTY_RUN</c> row, through a unit of work.
/// </summary>
/// <remarks>
/// The calculation itself is the owning project's: <c>ZFactorCalculator</c>, <c>GasViscosityCalculator</c>,
/// <c>PseudoPressureCalculator</c> and the applicability validator are called directly, so this screen adds no second
/// implementation of a correlation. What it adds is the unit conversion, the failure state, the record of what was run and
/// the history.
/// </remarks>
public sealed class OgGasPropertiesService : IOgGasProperties
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgGasPropertiesService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public Task<OgResult<OgGasPropertyRun>> RunAsync(
        OgGasPropertyInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        cancellationToken.ThrowIfCancellationRequested();

        if (GasPropertyRules.Validate(inputs) is { } refusal)
        {
            return Task.FromResult(OgResult<OgGasPropertyRun>.Failed(refusal.Message, refusal.Code));
        }

        var pressurePsia = GasUnits.ToPsia(inputs.Pressure!.Value, inputs.PressureUnit!);
        var temperatureR = GasUnits.ToRankine(inputs.Temperature!.Value, inputs.TemperatureUnit!);
        var gravity = inputs.SpecificGravity!.Value;
        var zName = Canonical(GasCorrelations.Z, inputs.ZCorrelation!);
        var viscosityName = Canonical(GasCorrelations.Viscosity, inputs.ViscosityCorrelation!);

        Func<decimal, decimal, decimal, decimal> z =
            zName == GasCorrelations.HallYarborough
                ? ZFactorCalculator.CalculateHallYarborough
                : zName == GasCorrelations.BrillBeggs
                    ? (p, t, g) => ZFactorCalculator.CalculateBrillBeggs(p, t, g)
                    : ZFactorCalculator.CalculateStandingKatz;

        Func<decimal, decimal, decimal, decimal, decimal> viscosity =
            viscosityName == GasCorrelations.LeeGonzalezEakin
                ? GasViscosityCalculator.CalculateLeeGonzalezEakin
                : GasViscosityCalculator.CalculateCarrKobayashiBurrows;

        OgGasPropertyRun Build(
            string status,
            decimal? zFactor,
            decimal? viscosityValue,
            decimal? density,
            decimal? fvf,
            decimal? pseudoPressure,
            IReadOnlyList<string> warnings,
            string? failureCode,
            string? failureMessage) =>
            new(
                string.Empty,
                status,
                inputs.Pressure,
                inputs.PressureUnit!.Trim(),
                inputs.Temperature,
                inputs.TemperatureUnit!.Trim(),
                pressurePsia,
                temperatureR,
                gravity,
                zName,
                viscosityName,
                zFactor,
                viscosityValue,
                density,
                fvf,
                pseudoPressure,
                warnings,
                failureCode,
                failureMessage,
                null,
                null);

        // AC-3: a refused calculation is a run in the FAILED state carrying its reason, never a number.
        OgGasPropertyRun Refused(string reason) =>
            Build(
                OgGasPropertyRun.FailedStatus,
                null,
                null,
                null,
                null,
                null,
                NoWarnings,
                GasPropertyRules.CodeCalculationRefused,
                reason);

        try
        {
            // The engine's own validator decides whether the inputs are inside its range (AC-3, the state table).
            if (!InRange(pressurePsia, GasPropertiesConstants.MinimumPressure, GasPropertiesConstants.MaximumPressure,
                    out var pressureRefusal))
            {
                return Task.FromResult(OgResult<OgGasPropertyRun>.Ok(Refused(pressureRefusal)));
            }

            if (!InRange(temperatureR, GasPropertiesConstants.MinimumTemperature,
                    GasPropertiesConstants.MaximumTemperature, out var temperatureRefusal))
            {
                return Task.FromResult(OgResult<OgGasPropertyRun>.Ok(Refused(temperatureRefusal)));
            }

            if (!InRange(gravity, GasPropertiesConstants.MinimumSpecificGravity,
                    GasPropertiesConstants.MaximumSpecificGravity, out var gravityRefusal))
            {
                return Task.FromResult(OgResult<OgGasPropertyRun>.Ok(Refused(gravityRefusal)));
            }

            // "Correlation not applicable - refused with the reason": the owner ships the predicate for this one.
            if (zName == GasCorrelations.BrillBeggs
                && !ZFactorCalculator.IsBrillBeggsDefined(temperatureR, gravity))
            {
                var reason =
                    $"Brill-Beggs is not defined at this temperature and specific gravity: its pseudo-reduced " +
                    $"temperature must exceed {ZFactorCalculator.BrillBeggsMinimumPseudoReducedTemperature}; choose " +
                    $"{GasCorrelations.StandingKatz} or {GasCorrelations.HallYarborough}.";

                return Task.FromResult(OgResult<OgGasPropertyRun>.Ok(Refused(reason)));
            }

            // Standing-Katz is solved by iteration, so its own outcome decides: a point the equation cannot converge
            // is a failed run, not the clamped number the older wrapper returned (AC-3).
            decimal zFactor;

            if (zName == GasCorrelations.StandingKatz)
            {
                if (!ZFactorCalculator.TryCalculateStandingKatz(pressurePsia, temperatureR, gravity, out zFactor))
                {
                    var lowestT = ZFactorCalculator.DAKMinimumPseudoReducedTemperature;
                    var highestT = ZFactorCalculator.DAKMaximumPseudoReducedTemperature;
                    var highestP = ZFactorCalculator.DAKMaximumPseudoReducedPressure;

                    var notConverged =
                        $"{GasCorrelations.StandingKatz} (Dranchuk-Abou-Kassem) did not converge at this point, or " +
                        "the point is outside its range (pseudo-reduced temperature above " +
                        lowestT.ToString("0.##", CultureInfo.InvariantCulture) + " up to " +
                        highestT.ToString("0.##", CultureInfo.InvariantCulture) + ", pseudo-reduced pressure up to " +
                        highestP.ToString("0", CultureInfo.InvariantCulture) + "); the run is failed rather than " +
                        "recorded as a number.";

                    return Task.FromResult(OgResult<OgGasPropertyRun>.Ok(Refused(notConverged)));
                }
            }
            else
            {
                zFactor = z(pressurePsia, temperatureR, gravity);
            }

            if (!InRange(zFactor, GasPropertiesConstants.MinimumZFactor, GasPropertiesConstants.MaximumZFactor,
                    out var zRefusal))
            {
                var shownZ = zFactor.ToString("0.####", CultureInfo.InvariantCulture);
                var lowestZ = GasPropertiesConstants.MinimumZFactor.ToString("0.##", CultureInfo.InvariantCulture);
                var highestZ = GasPropertiesConstants.MaximumZFactor.ToString("0.##", CultureInfo.InvariantCulture);

                var reason =
                    $"the {zName} correlation returned Z = {shownZ}, outside the engine\'s {lowestZ} to {highestZ} " +
                    "range; the run is failed rather than recorded as a number.";

                return Task.FromResult(OgResult<OgGasPropertyRun>.Ok(Refused(reason)));
            }

            var viscosityValue = viscosity(pressurePsia, temperatureR, gravity, zFactor);

            var molecularWeight = gravity * GasPropertiesConstants.AirMolecularWeight;

            var density = (pressurePsia * molecularWeight) /
                          (zFactor * GasPropertiesConstants.UniversalGasConstant * temperatureR);

            var fvf = FormationVolumeFactorConstant * zFactor * temperatureR / pressurePsia;

            var pseudoPressure = PseudoPressureCalculator.CalculatePseudoPressure(
                pressurePsia, temperatureR, gravity, z, viscosity);

            var warnings = GasPropertiesValidator.GetApplicabilityWarnings(
                pressurePsia, temperatureR, gravity, composition: null);

            return Task.FromResult(OgResult<OgGasPropertyRun>.Ok(Build(
                OgGasPropertyRun.OkStatus,
                zFactor,
                viscosityValue,
                density,
                fvf,
                pseudoPressure,
                warnings,
                null,
                null)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"calculating gas properties at {GasUnits.Describe(pressurePsia, GasUnits.Psia)}",
                "the run is shown as failed and no result is reported",
                FailureSeverity.Degraded);

            // AC-3: an engine failure is a failed run, never a number. The exception's own text goes to the failure
            // store under the reference; the person reads the reference and a sentence.
            return Task.FromResult(OgResult<OgGasPropertyRun>.Ok(Build(
                OgGasPropertyRun.FailedStatus,
                null,
                null,
                null,
                null,
                null,
                NoWarnings,
                GasPropertyRules.CodeCalculationRefused,
                $"the engine did not complete the calculation (reference {reference})")));
        }
    }

    public async Task<OgResult<OgGasPropertyWorkspace>> SaveAsync(
        OgGasPropertyRun run,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(run);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgGasPropertyWorkspace>.Failed(
                "A saved result must record who saved it.", GasPropertyRules.CodeActorRequired);
        }

        if (run.PressurePsia is null || run.TemperatureRankine is null)
        {
            return OgResult<OgGasPropertyWorkspace>.Failed(
                "There is no calculation to save; run it first.", GasPropertyRules.CodeNotRun);
        }

        if (run.IsSaved)
        {
            // Saving an already-saved run again would be a duplicate of the same record.
            return await LoadAsync(cancellationToken).ConfigureAwait(false);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgGasPropertyWorkspace>.Failed(
                "No connection is open, so the result cannot be saved.", GasPropertyRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var unit = OgUow.For<GAS_PROPERTY_RUN>(source);
            unit.PrimaryKey = "RUN_ID";

            unit.Add(new GAS_PROPERTY_RUN
            {
                RUN_ID = Guid.NewGuid().ToString("N"),
                STATUS = run.IsFailed ? OgGasPropertyRun.FailedStatus : OgGasPropertyRun.OkStatus,
                PRESSURE_INPUT = run.Pressure,
                PRESSURE_UNIT = run.PressureUnit,
                PRESSURE_PSIA = run.PressurePsia,
                TEMPERATURE_INPUT = run.Temperature,
                TEMPERATURE_UNIT = run.TemperatureUnit,
                TEMPERATURE_R = run.TemperatureRankine,
                SPECIFIC_GRAVITY = run.SpecificGravity,
                Z_CORRELATION = run.ZCorrelation,
                VISCOSITY_CORRELATION = run.ViscosityCorrelation,
                Z_FACTOR = run.ZFactor,
                VISCOSITY_CP = run.Viscosity,
                DENSITY = run.Density,
                FORMATION_VOLUME_FACTOR = run.FormationVolumeFactor,
                PSEUDO_PRESSURE = run.PseudoPressure,
                APPLICABILITY_TEXT = run.Warnings.Count == 0
                    ? null
                    : string.Join("\n", run.Warnings),
                FAILURE_CODE = run.FailureCode,
                FAILURE_MESSAGE = run.FailureMessage,
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
                return OgResult<OgGasPropertyWorkspace>.Failed(
                    OgUow.Message(committed, "The result was not saved."), GasPropertyRules.CodeWriteFailed);
            }

            return await LoadAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "saving a gas-property result",
                "the result was not saved",
                FailureSeverity.Error);

            return OgResult<OgGasPropertyWorkspace>.Failed(
                "The result could not be saved.", reference ?? GasPropertyRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgGasPropertyWorkspace>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgGasPropertyWorkspace>.Failed(
                "No connection is open, so the gas-property history cannot be read.",
                GasPropertyRules.CodeNoConnection);
        }

        try
        {
            var rows = await OgUow.ReadAllAsync<GAS_PROPERTY_RUN>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var history = rows
                .Where(row => !string.IsNullOrWhiteSpace(row.RUN_ID))
                .OrderByDescending(row => row.RECORDED_AT ?? DateTime.MinValue)
                .ThenBy(row => row.RUN_ID, StringComparer.Ordinal)
                .Select(ToRun)
                .Take(50)
                .ToList();

            return OgResult<OgGasPropertyWorkspace>.Ok(new OgGasPropertyWorkspace(
                history,
                GasCorrelations.Z,
                GasCorrelations.Viscosity,
                GasUnits.PressureUnits,
                GasUnits.TemperatureUnits,
                false,
                null));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the gas-property history",
                "the saved history is unavailable; new runs are still allowed",
                FailureSeverity.Degraded);

            // The state table's "history unavailable": a state, not a dead page.
            return OgResult<OgGasPropertyWorkspace>.Ok(new OgGasPropertyWorkspace(
                [],
                GasCorrelations.Z,
                GasCorrelations.Viscosity,
                GasUnits.PressureUnits,
                GasUnits.TemperatureUnits,
                true,
                $"the saved history could not be read (reference {reference})"));
        }
    }

    /// <summary>
    /// Mirrors the coefficient in <c>GasPropertiesService.CalculateFormationVolumeFactor</c> (Bg = 0.02827 Z T / P). It is
    /// not exposed as a constant by the owner, so it is named here rather than left as a literal in a formula.
    /// </summary>
    private const decimal FormationVolumeFactorConstant = 0.02827m;

    private static readonly IReadOnlyList<string> NoWarnings = [];

    /// <summary>
    /// The engine's own bounds, checked here so the refusal is a sentence built from its constants rather than an
    /// exception's text leaving a catch.
    /// </summary>
    private static bool InRange(decimal value, decimal minimum, decimal maximum, out string refusal)
    {
        if (value < minimum || value > maximum)
        {
            var shown = value.ToString("0.####", CultureInfo.InvariantCulture);
            var lowest = minimum.ToString("0.##", CultureInfo.InvariantCulture);
            var highest = maximum.ToString("0.##", CultureInfo.InvariantCulture);

            refusal = $"{shown} is outside the engine\'s {lowest} to {highest} range for this calculation.";
            return false;
        }

        refusal = string.Empty;
        return true;
    }

    private static string Canonical(IReadOnlyList<string> supported, string value) =>
        supported.First(candidate => string.Equals(candidate, value.Trim(), StringComparison.OrdinalIgnoreCase));

    private static OgGasPropertyRun ToRun(GAS_PROPERTY_RUN row) =>
        new(
            row.RUN_ID ?? string.Empty,
            string.IsNullOrWhiteSpace(row.STATUS) ? OgGasPropertyRun.OkStatus : row.STATUS!,
            row.PRESSURE_INPUT,
            row.PRESSURE_UNIT,
            row.TEMPERATURE_INPUT,
            row.TEMPERATURE_UNIT,
            row.PRESSURE_PSIA,
            row.TEMPERATURE_R,
            row.SPECIFIC_GRAVITY,
            row.Z_CORRELATION ?? string.Empty,
            row.VISCOSITY_CORRELATION ?? string.Empty,
            row.Z_FACTOR,
            row.VISCOSITY_CP,
            row.DENSITY,
            row.FORMATION_VOLUME_FACTOR,
            row.PSEUDO_PRESSURE,
            string.IsNullOrWhiteSpace(row.APPLICABILITY_TEXT)
                ? []
                : row.APPLICABILITY_TEXT!
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            row.FAILURE_CODE,
            row.FAILURE_MESSAGE,
            row.ACTOR,
            row.RECORDED_AT);
}
