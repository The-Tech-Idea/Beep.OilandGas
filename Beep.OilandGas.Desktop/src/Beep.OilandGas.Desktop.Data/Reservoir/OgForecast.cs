using System.Globalization;
using Beep.OilandGas.DCA.AdvancedDeclineMethods;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.ProductionForecasting.Constants;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;

/// <summary>The lifecycle a saved forecast goes through (RES-07).</summary>
public static class ForecastStatuses
{
    public const string Draft = "Draft";
    public const string Approved = "Approved";
    public const string Archived = "Archived";

    public static IReadOnlyList<string> Fallback { get; } = [Draft, Approved, Archived];
}

/// <summary>The streams a forecast point can carry (AC-6).</summary>
public static class ForecastStreams
{
    public const string Oil = "OIL";
    public const string Gas = "GAS";
    public const string Water = "WATER";

    public static IReadOnlyList<string> All { get; } = [Oil, Gas, Water];
}

/// <summary>
/// The decline methods this screen can generate, mapped to the owner's <see cref="ForecastType"/> codes. The full method
/// list comes from the reference set; the ones not here have no source in this screen and are refused with that reason.
/// </summary>
public static class ForecastMethods
{
    public static IReadOnlyList<ForecastType> Supported { get; } =
        [ForecastType.Decline, ForecastType.Exponential, ForecastType.Harmonic, ForecastType.Hyperbolic];

    public static bool IsSupported(string? method) =>
        TryParse(method, out var parsed) && Supported.Contains(parsed);

    public static bool TryParse(string? method, out ForecastType parsed) =>
        Enum.TryParse(method?.Trim(), ignoreCase: true, out parsed)
        && Enum.IsDefined(typeof(ForecastType), parsed);

    /// <summary>The method a fit's b implies: exponential at 0, harmonic at 1, hyperbolic between.</summary>
    public static ForecastType ForB(decimal b) =>
        b == 0m ? ForecastType.Exponential : b == 1m ? ForecastType.Harmonic : ForecastType.Hyperbolic;
}

/// <summary>The inputs of one forecast generation (RES-07).</summary>
public sealed record OgForecastInputs(
    string? Name,
    string? WellId,
    string? FieldId,
    string? Method,
    string? SourceFitId,
    DateTime? StartDate,
    int Months,
    decimal? MaxRate,
    decimal? EconomicLimit,
    string? RiskRating,
    decimal? ManualQi = null,
    decimal? ManualDiPerDay = null,
    decimal? ManualB = null,
    string? ManualFluid = null,
    string? ManualUnit = null);

/// <summary>One monthly point of a forecast (RES-07), matching the plan's <c>OgForecastPoint</c> plus its volume.</summary>
public sealed record OgForecastPoint(
    DateTime Date,
    decimal Oil,
    decimal Gas,
    decimal Water,
    decimal Volume,
    decimal Cumulative,
    string? Unit)
{
    public string DateText => Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public string OilText => Number(Oil);

    public string GasText => Number(Gas);

    public string WaterText => Number(Water);

    public string VolumeText => Number(Volume) + (Unit is null ? string.Empty : " " + Unit);

    public string CumulativeText => Number(Cumulative) + (Unit is null ? string.Empty : " " + Unit);

    public string Describe() =>
        $"{DateText} · oil {OilText} · gas {GasText} · water {WaterText} · volume {VolumeText} · " +
        $"cumulative {CumulativeText}";

    private static string Number(decimal value) =>
        value.ToString("#,0.###", CultureInfo.InvariantCulture);
}

/// <summary>One forecast with its points and its basis (RES-07), matching the plan's <c>OgForecast</c>.</summary>
public sealed record OgForecast(
    string Id,
    string? Name,
    string? WellId,
    string? FieldId,
    string Method,
    string? SourceFitId,
    string Fluid,
    DateTime StartDate,
    int Months,
    string Status,
    string? RiskRating,
    int Version,
    string? VersionOf,
    bool Immutable,
    decimal? MaxRate,
    decimal? EconomicLimit,
    string? OilUnit,
    string? GasUnit,
    string? WaterUnit,
    IReadOnlyList<OgForecastPoint> Points,
    decimal InitialRate,
    decimal FinalRate,
    decimal Total,
    decimal AverageMonthly,
    string? RefusalCode,
    string? RefusalMessage,
    string? Actor,
    DateTime? RecordedAt,
    string? ApprovedBy,
    DateTime? ApprovedAt)
{
    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    public bool IsRefused => !string.IsNullOrWhiteSpace(RefusalCode);

    public bool IsApproved => string.Equals(Status, ForecastStatuses.Approved, StringComparison.OrdinalIgnoreCase);

    public string MethodText => Method;

    public string StatusText => Status.ToLowerInvariant();

    public string EntityText => WellId ?? FieldId ?? "(no entity)";

    public string VersionText => "v" + Version.ToString(CultureInfo.InvariantCulture);

    public string RateUnit => OilUnit ?? GasUnit ?? WaterUnit ?? string.Empty;

    public string InitialText => InitialRate.ToString("#,0.###", CultureInfo.InvariantCulture) + " " + RateUnit;

    public string FinalText => FinalRate.ToString("#,0.###", CultureInfo.InvariantCulture) + " " + RateUnit;

    public string TotalText => Total.ToString("#,0.###", CultureInfo.InvariantCulture) + " " + (OilUnit ?? GasUnit ?? WaterUnit ?? string.Empty);

    public string AverageText =>
        AverageMonthly.ToString("#,0.###", CultureInfo.InvariantCulture) + " " +
        (OilUnit ?? GasUnit ?? WaterUnit ?? string.Empty);

    public string UnitsText =>
        $"oil {(OilUnit ?? "(none)")}, gas {(GasUnit ?? "(none)")}, water {(WaterUnit ?? "(none)")}";

    public string ApprovalText => IsApproved
        ? $"approved by {ApprovedBy ?? "(unknown)"} on " +
          $"{ApprovedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date"}" +
          (Immutable ? " · immutable" : string.Empty)
        : "not approved";

    public string SourceText => string.IsNullOrWhiteSpace(SourceFitId)
        ? "manual parameters (no saved fit)"
        : "RES-06 fit " + SourceFitId;

    public string RefusalText => IsRefused
        ? $"{RefusalCode}: {RefusalMessage ?? "the forecast was refused"}"
        : "no refusal";

    public string Describe() => IsRefused
        ? $"REFUSED · {Name ?? "(no name)"} · {RefusalText}"
        : $"{Name ?? "(no name)"} · {EntityText} · {MethodText} · {VersionText} · {StatusText} · " +
          $"{Points.Count.ToString(CultureInfo.InvariantCulture)} month(s) · initial {InitialText} · " +
          $"final {FinalText} · total {TotalText} · {ApprovalText}";

    public string PointsText => Points.Count == 0
        ? "no point"
        : string.Join(Environment.NewLine, Points.Select(point => point.Describe()));

    /// <summary>The monthly and annual table, with the cumulative column, as the screen prints it.</summary>
    public string MonthlyTable
    {
        get
        {
            if (Points.Count == 0)
            {
                return "no forecast point";
            }

            var lines = new List<string>
            {
                $"month · rate · volume · cumulative · unit {RateUnit}",
            };

            lines.AddRange(Points.Select(point =>
                $"{point.DateText} · {point.VolumeText.Split(' ')[0]} volume · {point.CumulativeText}"));

            return string.Join(Environment.NewLine, lines);
        }
    }
}

/// <summary>A save whose acknowledgement was lost: the forecast is kept and the same insert is retried (RES-07).</summary>
public sealed record OgForecastPending(OgForecast Forecast, string Actor);

/// <summary>The forecasting workspace (RES-07).</summary>
public sealed record OgForecastWorkspace(
    IReadOnlyList<OgForecast> Forecasts,
    OgForecast? Selected,
    IReadOnlyList<Reservoir.OgDeclineFit> Fits,
    IReadOnlyList<string> Methods,
    IReadOnlyList<string> Statuses,
    IReadOnlyList<string> RiskRatings,
    bool ReferenceFromTable,
    bool HistoryFailed,
    string? HistoryReason)
{
    public bool HasFits => Fits.Count > 0;

    /// <summary>The state table's "no source": the generator is disabled with this reason.</summary>
    public string NoSourceReason => HasFits
        ? string.Empty
        : "No decline fit is saved, so there is nothing to generate from. Fit the well on the decline curve screen first, " +
          "or type the parameters by hand.";

    public string ForecastsText => Forecasts.Count == 0
        ? "No forecast is saved yet."
        : string.Join(Environment.NewLine, Forecasts.Select(forecast => forecast.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved forecasts could not be read; new forecasts can still be generated"
        : $"{Forecasts.Count.ToString(CultureInfo.InvariantCulture)} forecast version(s)";

    public string ReferenceText => ReferenceFromTable
        ? "from R_PRODUCTION_FORECASTING_REFERENCE_CODE"
        : "the reference table has no rows, so the method list is the ForecastType set and the statuses are the " +
          "lifecycle's own";

    /// <summary>AC-3: every version of a name is listed, and none replaced another.</summary>
    public string VersionsText(string? name)
    {
        var versions = Forecasts
            .Where(forecast => string.Equals(forecast.Name, name, StringComparison.OrdinalIgnoreCase))
            .OrderBy(forecast => forecast.Version)
            .ToList();

        return versions.Count == 0
            ? $"no forecast named {name ?? "(none)"}"
            : string.Join(", ", versions.Select(forecast => $"{forecast.VersionText} ({forecast.StatusText})"));
    }
}

/// <summary>The forecast rules, stated once so the page, the service and the tests agree.</summary>
public static class ForecastRules
{
    public const string CodeNoConnection = "forecast-no-connection";
    public const string CodeReadFailed = "forecast-read-failed";
    public const string CodeWriteFailed = "forecast-write-failed";
    public const string CodeActorRequired = "forecast-actor-required";
    public const string CodeNameRequired = "forecast-name-required";
    public const string CodeStartRequired = "forecast-start-required";
    public const string CodeMonthsInvalid = "forecast-months-invalid";
    public const string CodeMethodUnknown = "forecast-method-unknown";
    public const string CodeMethodUnsupported = "forecast-method-unsupported";
    public const string CodeNoSource = "forecast-no-source";
    public const string CodeFitUnknown = "forecast-fit-unknown";
    public const string CodeFitRefused = "forecast-source-fit-refused";
    public const string CodeManualIncomplete = "forecast-manual-incomplete";
    public const string CodeConstraintInvalid = "forecast-constraint-invalid";
    public const string CodeConstraintExceeded = "forecast-constraint-exceeded";
    public const string CodeProfileNegative = "forecast-profile-negative";
    public const string CodeProfileEmpty = "forecast-profile-empty";
    public const string CodeNothingToSave = "forecast-nothing-to-save";
    public const string CodeForecastUnknown = "forecast-unknown";
    public const string CodeAlreadyApproved = "forecast-already-approved";
    public const string CodeApproverIsCreator = "forecast-approver-is-creator";
    public const string CodeNothingPending = "forecast-nothing-pending";

    /// <summary>The horizon bounds: a month at the least, fifty years at the most.</summary>
    public const int MinimumMonths = 1;

    public const int MaximumMonths = 600;

    public static (string Code, string Message)? Validate(OgForecastInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (string.IsNullOrWhiteSpace(inputs.Name))
        {
            return (CodeNameRequired, "A forecast needs a name, so a plan or an economics run can point at it.");
        }

        if (inputs.StartDate is null)
        {
            return (CodeStartRequired, "A forecast needs the date its first month starts.");
        }

        if (inputs.Months < MinimumMonths || inputs.Months > MaximumMonths)
        {
            return (CodeMonthsInvalid,
                $"The horizon must be between {MinimumMonths.ToString(CultureInfo.InvariantCulture)} and " +
                $"{MaximumMonths.ToString(CultureInfo.InvariantCulture)} months.");
        }

        if (string.IsNullOrWhiteSpace(inputs.Method))
        {
            return (CodeMethodUnknown, "Choose the forecast method.");
        }

        if (!ForecastMethods.TryParse(inputs.Method, out var method))
        {
            return (CodeMethodUnknown,
                $"'{inputs.Method}' is not a forecast method code. The codes are the ForecastType names.");
        }

        if (!ForecastMethods.Supported.Contains(method))
        {
            return (CodeMethodUnsupported,
                $"{method} has no source in this screen: it generates an Arps decline profile from a RES-06 fit or from " +
                $"typed parameters. The methods it can generate are {string.Join(", ", ForecastMethods.Supported)}.");
        }

        if (inputs.MaxRate is <= 0m)
        {
            return (CodeConstraintInvalid, "A maximum rate must be above zero.");
        }

        if (inputs.EconomicLimit is <= 0m)
        {
            return (CodeConstraintInvalid, "An economic limit rate must be above zero.");
        }

        if (inputs.MaxRate is { } ceiling && inputs.EconomicLimit is { } floor && ceiling < floor)
        {
            return (CodeConstraintInvalid,
                $"The maximum rate {ceiling.ToString("#,0.###", CultureInfo.InvariantCulture)} is below the economic " +
                $"limit {floor.ToString("#,0.###", CultureInfo.InvariantCulture)}, so no profile can satisfy both.");
        }

        // The source is either a saved RES-06 fit or the three parameters typed by hand.
        if (string.IsNullOrWhiteSpace(inputs.SourceFitId))
        {
            if (inputs.ManualQi is not > 0m)
            {
                return (CodeNoSource,
                    "Choose a saved decline fit or type the initial rate, decline rate and b; without a source there is " +
                    "nothing to generate from.");
            }

            if (inputs.ManualDiPerDay is not > 0m)
            {
                return (CodeManualIncomplete, "A typed source needs a decline rate above zero per day.");
            }

            if (inputs.ManualB is < 0m or > 1m)
            {
                return (CodeManualIncomplete,
                    "A typed source needs b between 0 and 1; outside that the profile is not an Arps decline.");
            }
        }

        return null;
    }
}

/// <summary>RES-07: production forecasting with scenarios.</summary>
public interface IOgForecast
{
    /// <summary>Generates a profile. Nothing is written.</summary>
    Task<OgResult<OgForecastWorkspace>> GenerateAsync(
        OgForecastInputs inputs,
        CancellationToken cancellationToken = default);

    /// <summary>Saves a generated profile as a new version (AC-3).</summary>
    Task<OgResult<OgForecastWorkspace>> SaveAsync(
        OgForecast forecast,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Approves a saved draft; the approver must not be its creator (SoD).</summary>
    Task<OgResult<OgForecastWorkspace>> ApproveAsync(
        string forecastId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The saved forecasts, the fits on offer and the codes; an unavailable history is a state.</summary>
    Task<OgResult<OgForecastWorkspace>> LoadAsync(
        string? fieldId = null,
        string? fieldName = null,
        string? forecastId = null,
        CancellationToken cancellationToken = default);

    /// <summary>The forecast retained when its save was not acknowledged.</summary>
    OgForecastPending? Pending { get; }

    /// <summary>Retries the same insert.</summary>
    Task<OgResult<OgForecastWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// RES-07 over the ProductionForecasting <c>PRODUCTION_FORECAST</c> and <c>PRODUCTION_FORECAST_POINT</c> rows, the RES-06
/// <c>DECLINE_FIT</c> it consumes, and the module's reference sets, through a unit of work.
/// </summary>
/// <remarks>
/// The profile is the owner's Arps arithmetic: the initial rate, decline rate and b come from the RES-06 fit (or from
/// typed parameters), each month's rate is the owner's decline relation at the month start and its volume is the
/// difference of the owner's closed-form cumulative between the month's two ends. A ceiling is a constraint that is
/// refused when violated, never clamped (AC-5); an economic limit ends the profile rather than being a violation. A save
/// always inserts a new version, so no version can overwrite another (AC-3).
/// </remarks>
public sealed class OgForecastService : IOgForecast
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgForecastService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgForecastPending? Pending { get; private set; }

    public async Task<OgResult<OgForecastWorkspace>> GenerateAsync(
        OgForecastInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (ForecastRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgForecastWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgForecastWorkspace>.Failed(
                "No connection is open, so the source fit cannot be read.",
                ForecastRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            if (Build(inputs, model) is not { } built)
            {
                return OgResult<OgForecastWorkspace>.Failed(
                    "The forecast could not be built from that source.",
                    ForecastRules.CodeNoSource);
            }

            return OgResult<OgForecastWorkspace>.Ok(Workspace(model, built, built.Id));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"generating a forecast for {inputs.Name}",
                "the forecast is not shown",
                FailureSeverity.Error);

            return OgResult<OgForecastWorkspace>.Failed(
                "The forecast could not be generated.", reference ?? ForecastRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgForecastWorkspace>> SaveAsync(
        OgForecast forecast,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(forecast);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgForecastWorkspace>.Failed(
                "A saved forecast must record who saved it.", ForecastRules.CodeActorRequired);
        }

        if (forecast.IsRefused || forecast.Points.Count == 0)
        {
            return OgResult<OgForecastWorkspace>.Failed(
                "There is no profile to save; generate one first.", ForecastRules.CodeNothingToSave);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgForecastWorkspace>.Failed(
                "No connection is open, so the forecast cannot be saved.", ForecastRules.CodeNoConnection);
        }

        try
        {
            var saved = await InsertAsync(source, forecast, actor, cancellationToken).ConfigureAwait(false);

            if (!saved.Success || saved.Value is null)
            {
                Pending = new OgForecastPending(forecast, actor.Trim());

                return saved;
            }

            Pending = null;

            return saved;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            // The state table's "save acknowledgement lost": the forecast is kept and the same insert is retried.
            Pending = new OgForecastPending(forecast, actor.Trim());

            var reference = _failures.ReportHandled(
                ex,
                $"saving forecast {forecast.Name}",
                "the forecast was not acknowledged; the original is retained and the save can be retried",
                FailureSeverity.Error);

            return OgResult<OgForecastWorkspace>.Failed(
                "The forecast was not acknowledged; the original is retained and the save can be retried.",
                reference ?? ForecastRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgForecastWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgForecastWorkspace>.Failed(
                "There is no forecast waiting to be retried.", ForecastRules.CodeNothingPending);
        }

        var retried = await SaveAsync(pending.Forecast, actor, cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    public async Task<OgResult<OgForecastWorkspace>> ApproveAsync(
        string forecastId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgForecastWorkspace>.Failed(
                "An approval must record who approved it.", ForecastRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgForecastWorkspace>.Failed(
                "No connection is open, so the forecast cannot be approved.", ForecastRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            var row = model.Forecasts.FirstOrDefault(candidate =>
                string.Equals(candidate.PRODUCTION_FORECAST_ID, forecastId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (row is null)
            {
                return OgResult<OgForecastWorkspace>.Failed(
                    $"Forecast '{forecastId}' is not in this database.", ForecastRules.CodeForecastUnknown);
            }

            if (string.Equals(row.STATUS, ForecastStatuses.Approved, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgForecastWorkspace>.Failed(
                    "This forecast is already approved; an approved forecast is immutable.",
                    ForecastRules.CodeAlreadyApproved);
            }

            // Separation of duties: the person who generated the forecast does not approve it.
            if (string.Equals(row.ACTOR, actor.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgForecastWorkspace>.Failed(
                    $"{actor.Trim()} generated this forecast, so it cannot also approve it; a second person is needed.",
                    ForecastRules.CodeApproverIsCreator);
            }

            var unit = OgUow.For<PRODUCTION_FORECAST>(source);
            unit.PrimaryKey = "PRODUCTION_FORECAST_ID";

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal("PRODUCTION_FORECAST_ID", row.PRODUCTION_FORECAST_ID!)],
                cancellationToken).ConfigureAwait(false);

            var stored = rows.FirstOrDefault();

            if (stored is null)
            {
                return OgResult<OgForecastWorkspace>.Failed(
                    $"Forecast '{forecastId}' is not in this database.", ForecastRules.CodeForecastUnknown);
            }

            var now = DateTime.UtcNow;

            stored.STATUS = ForecastStatuses.Approved;
            stored.IMMUTABLE_IND = "Y";
            stored.APPROVED_BY = actor.Trim();
            stored.APPROVED_AT = now;
            stored.ROW_CHANGED_BY = actor.Trim();
            stored.ROW_CHANGED_DATE = now;

            var updated = unit.Update(stored);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgForecastWorkspace>.Failed(
                    OgUow.Message(updated, "The approval was not recorded."), ForecastRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgForecastWorkspace>.Failed(
                    OgUow.Message(committed, "The approval was not recorded."), ForecastRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgForecastWorkspace>.Ok(Workspace(after, null, row.PRODUCTION_FORECAST_ID));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"approving forecast {forecastId}",
                "the approval was not recorded",
                FailureSeverity.Error);

            return OgResult<OgForecastWorkspace>.Failed(
                "The approval was not recorded.", reference ?? ForecastRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgForecastWorkspace>> LoadAsync(
        string? fieldId = null,
        string? fieldName = null,
        string? forecastId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgForecastWorkspace>.Failed(
                "No connection is open, so the forecasts cannot be read.", ForecastRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgForecastWorkspace>.Ok(Workspace(model, null, forecastId));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the production forecasts",
                "the saved forecasts are unavailable; new forecasts can still be generated",
                FailureSeverity.Degraded);

            return OgResult<OgForecastWorkspace>.Ok(new OgForecastWorkspace(
                [],
                null,
                [],
                Fallback<ForecastType>(),
                ForecastStatuses.Fallback,
                [ProductionForecastingReferenceCodes.RiskUnknown],
                false,
                true,
                $"the saved forecasts could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ generation

    /// <summary>
    /// Builds the monthly profile from the source, applying the ceiling as a refusal and the economic limit as the end of
    /// the profile. Every refusal comes back as a forecast carrying its code and no points, so the caller can show it.
    /// </summary>
    public static OgForecast? Build(OgForecastInputs inputs, ForecastModel model)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(model);

        ForecastMethods.TryParse(inputs.Method, out var method);

        var start = inputs.StartDate!.Value.Date;

        OgForecast Refused(string code, string message) =>
            new(
                string.Empty,
                inputs.Name?.Trim(),
                inputs.WellId,
                inputs.FieldId,
                method.ToString(),
                inputs.SourceFitId,
                inputs.ManualFluid ?? string.Empty,
                start,
                inputs.Months,
                ForecastStatuses.Draft,
                inputs.RiskRating,
                0,
                null,
                false,
                inputs.MaxRate,
                inputs.EconomicLimit,
                null,
                null,
                null,
                [],
                0m,
                0m,
                0m,
                0m,
                code,
                message,
                null,
                null,
                null,
                null);

        decimal qi;
        decimal di;
        decimal b;
        string fluid;
        string? unit;
        string? sourceFitId = inputs.SourceFitId;

        if (!string.IsNullOrWhiteSpace(inputs.SourceFitId))
        {
            var fit = model.Fits.FirstOrDefault(candidate =>
                string.Equals(candidate.FIT_ID, inputs.SourceFitId!.Trim(), StringComparison.OrdinalIgnoreCase));

            if (fit is null)
            {
                return Refused(ForecastRules.CodeFitUnknown,
                    $"The decline fit '{inputs.SourceFitId}' is not saved; generate from a saved fit or type the parameters.");
            }

            if (string.Equals(fit.OUTCOME, "REFUSED", StringComparison.OrdinalIgnoreCase) ||
                fit.QI is null || fit.DI_PER_DAY is null || fit.B is null)
            {
                return Refused(ForecastRules.CodeFitRefused,
                    $"The decline fit '{inputs.SourceFitId}' was refused, so it has no parameters to forecast from.");
            }

            qi = fit.QI.Value;
            di = fit.DI_PER_DAY.Value;
            b = fit.B.Value;
            fluid = fit.FLUID ?? string.Empty;
            unit = fit.VOLUME_OUOM;
            sourceFitId = fit.FIT_ID;
        }
        else
        {
            qi = inputs.ManualQi!.Value;
            di = inputs.ManualDiPerDay!.Value;
            b = inputs.ManualB!.Value;
            fluid = (inputs.ManualFluid ?? ForecastStreams.Oil).Trim().ToUpperInvariant();
            unit = inputs.ManualUnit;
        }

        // The method the caller picked has to match the source's decline exponent, or the forecast would be labelled
        // with a model it was not generated with.
        var implied = ForecastMethods.ForB(b);

        if (method != ForecastType.Decline && method != implied)
        {
            return Refused(ForecastRules.CodeMethodUnsupported,
                $"The source's b is {b.ToString("0.####", CultureInfo.InvariantCulture)}, which is a {implied} decline; " +
                $"forecasting it as {method} would mislabel the profile. Choose {implied} or Decline.");
        }

        if (inputs.EconomicLimit is { } limit && limit >= qi)
        {
            return Refused(ForecastRules.CodeConstraintInvalid,
                $"The economic limit {limit.ToString("#,0.###", CultureInfo.InvariantCulture)} is at or above the " +
                $"initial rate {qi.ToString("#,0.###", CultureInfo.InvariantCulture)}, so the profile would be empty.");
        }

        var points = new List<OgForecastPoint>();
        var cumulative = 0m;

        for (var month = 0; month < inputs.Months; month++)
        {
            var from = start.AddMonths(month);
            var to = start.AddMonths(month + 1);

            var t1 = (from - start).TotalDays;
            var t2 = (to - start).TotalDays;

            var rate = Rate(qi, di, b, t1);
            var volume = Cumulative(qi, di, b, t2) - Cumulative(qi, di, b, t1);

            if (rate < 0m || volume < 0m)
            {
                return Refused(ForecastRules.CodeProfileNegative,
                    $"The profile goes negative in month {month + 1} " +
                    $"({from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}), which means the source parameters do " +
                    "not describe a declining well.");
            }

            // AC-5: a ceiling is a constraint. It is refused, never clamped.
            if (inputs.MaxRate is { } ceiling && rate > ceiling)
            {
                return Refused(ForecastRules.CodeConstraintExceeded,
                    $"The profile reaches {rate.ToString("#,0.###", CultureInfo.InvariantCulture)} in month " +
                    $"{month + 1} ({from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}), above the maximum " +
                    $"{ceiling.ToString("#,0.###", CultureInfo.InvariantCulture)}. The forecast is refused rather than " +
                    "clamped, because a clamped profile is not this well.");
            }

            cumulative += volume;

            points.Add(new OgForecastPoint(
                from,
                fluid == ForecastStreams.Oil ? rate : 0m,
                fluid == ForecastStreams.Gas ? rate : 0m,
                fluid == ForecastStreams.Water ? rate : 0m,
                volume,
                cumulative,
                unit));

            // The economic limit is where the profile ends, not a violation.
            if (inputs.EconomicLimit is { } floor && rate <= floor)
            {
                break;
            }
        }

        if (points.Count == 0)
        {
            return Refused(ForecastRules.CodeProfileEmpty, "The constraints leave no month in the profile.");
        }

        var initial = points[0].Volume;
        var final = points[^1].Oil + points[^1].Gas + points[^1].Water;

        return new OgForecast(
            string.Empty,
            inputs.Name!.Trim(),
            inputs.WellId,
            inputs.FieldId,
            method.ToString(),
            sourceFitId,
            fluid,
            start,
            points.Count,
            ForecastStatuses.Draft,
            string.IsNullOrWhiteSpace(inputs.RiskRating)
                ? ProductionForecastingReferenceCodes.RiskUnknown
                : inputs.RiskRating!.Trim(),
            0,
            null,
            false,
            inputs.MaxRate,
            inputs.EconomicLimit,
            fluid == ForecastStreams.Oil ? unit : null,
            fluid == ForecastStreams.Gas ? unit : null,
            fluid == ForecastStreams.Water ? unit : null,
            points,
            Rate(qi, di, b, 0d),
            final,
            cumulative,
            cumulative / points.Count,
            null,
            null,
            null,
            null,
            null,
            null);
    }

    private static decimal Rate(decimal qi, decimal di, decimal b, double days) =>
        b == 0m
            ? (decimal)ArpsDeclineMethods.ExponentialDecline((double)qi, (double)di, days)
            : b == 1m
                ? (decimal)ArpsDeclineMethods.HarmonicDecline((double)qi, (double)di, days)
                : (decimal)ArpsDeclineMethods.HyperbolicDecline((double)qi, (double)di, days, (double)b);

    private static decimal Cumulative(decimal qi, decimal di, decimal b, double days) =>
        b == 0m
            ? (decimal)ArpsDeclineMethods.ExponentialCumulativeProduction((double)qi, (double)di, days)
            : b == 1m
                ? (decimal)ArpsDeclineMethods.HarmonicCumulativeProduction((double)qi, (double)di, days)
                : (decimal)ArpsDeclineMethods.HyperbolicCumulativeProduction((double)qi, (double)di, days, (double)b);

    // ------------------------------------------------------------------ the write

    private async Task<OgResult<OgForecastWorkspace>> InsertAsync(
        IDataSource source,
        OgForecast forecast,
        string actor,
        CancellationToken cancellationToken)
    {
        var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

        // AC-3: a save is a new version of the same name and entity, never an overwrite.
        var existing = model.Forecasts
            .Where(row => string.Equals(row.FORECAST_NAME, forecast.Name, StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.WELL_UWI, forecast.WellId, StringComparison.OrdinalIgnoreCase)
                          || string.Equals(row.FIELD_ID, forecast.FieldId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.VERSION)
            .ToList();

        var version = (existing.FirstOrDefault()?.VERSION ?? 0) + 1;
        var versionOf = existing.FirstOrDefault()?.PRODUCTION_FORECAST_ID;

        var now = DateTime.UtcNow;
        var forecastId = Guid.NewGuid().ToString("N");

        var unit = OgUow.For<PRODUCTION_FORECAST>(source);
        unit.PrimaryKey = "PRODUCTION_FORECAST_ID";

        ForecastMethods.TryParse(forecast.Method, out var method);

        unit.Add(new PRODUCTION_FORECAST
        {
            PRODUCTION_FORECAST_ID = forecastId,
            FORECAST_ID = forecastId,
            FORECAST_NAME = forecast.Name,
            WELL_UWI = forecast.WellId,
            FIELD_ID = forecast.FieldId,
            FORECAST_TYPE = method,
            FORECAST_START_DATE = forecast.StartDate.Date,
            FORECAST_DURATION = forecast.Months,
            INITIAL_PRODUCTION_RATE = forecast.InitialRate,
            FINAL_PRODUCTION_RATE = forecast.FinalRate,
            TOTAL_CUMULATIVE_PRODUCTION = forecast.Total,
            TOTAL_FORCAST_PRODUCTION = forecast.Total,
            AVERAGE_MONTHLY_PRODUCTION = forecast.AverageMonthly,
            SOURCE_FIT_ID = forecast.SourceFitId,
            STATUS = ForecastStatuses.Draft,
            RISK_RATING = forecast.RiskRating,
            VERSION = version,
            VERSION_OF = versionOf,
            IMMUTABLE_IND = "N",
            MAX_RATE = forecast.MaxRate,
            ECONOMIC_LIMIT = forecast.EconomicLimit,
            OIL_VOLUME_OUOM = forecast.OilUnit,
            GAS_VOLUME_OUOM = forecast.GasUnit,
            WATER_VOLUME_OUOM = forecast.WaterUnit,
            ACTOR = actor.Trim(),
            RECORDED_AT = now,
            ROW_CREATED_BY = actor.Trim(),
            ROW_CREATED_DATE = now,
            ROW_CHANGED_BY = actor.Trim(),
            ROW_CHANGED_DATE = now,
        });

        var pointUnit = OgUow.For<PRODUCTION_FORECAST_POINT>(source);
        pointUnit.PrimaryKey = "FORECAST_POINT_ID";

        foreach (var point in forecast.Points)
        {
            pointUnit.Add(new PRODUCTION_FORECAST_POINT
            {
                FORECAST_POINT_ID = Guid.NewGuid().ToString("N"),
                FORECAST_ID = forecastId,
                FORECAST_DATE = point.Date.Date,
                OIL_RATE = point.Oil,
                GAS_RATE = point.Gas,
                WATER_RATE = point.Water,
                MONTHLY_VOLUME = point.Volume,
                CUMULATIVE_VOLUME = point.Cumulative,
                VOLUME_OUOM = point.Unit,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });
        }

        var committedPoints = await pointUnit.Commit().ConfigureAwait(false);

        if (!OgUow.IsOk(committedPoints))
        {
            return OgResult<OgForecastWorkspace>.Failed(
                OgUow.Message(committedPoints, "The points were not saved, so no forecast was recorded."),
                ForecastRules.CodeWriteFailed);
        }

        var committed = await unit.Commit().ConfigureAwait(false);

        if (!OgUow.IsOk(committed))
        {
            return OgResult<OgForecastWorkspace>.Failed(
                OgUow.Message(committed, "The forecast was not saved."), ForecastRules.CodeWriteFailed);
        }

        var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

        return OgResult<OgForecastWorkspace>.Ok(Workspace(after, null, forecastId));
    }

    // ------------------------------------------------------------------ the reads

    private static async Task<ForecastModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var forecasts = await OgUow.ReadAllAsync<PRODUCTION_FORECAST>(source).ConfigureAwait(false);
        var points = await OgUow.ReadAllAsync<PRODUCTION_FORECAST_POINT>(source).ConfigureAwait(false);
        var fits = await OgUow.ReadAllAsync<DECLINE_FIT>(source).ConfigureAwait(false);
        var reference = await OgUow.ReadAllAsync<R_PRODUCTION_FORECASTING_REFERENCE_CODE>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new ForecastModel(forecasts, points, fits, reference);
    }

    private static OgForecastWorkspace Workspace(ForecastModel model, OgForecast? generated, string? selectedId)
    {
        var methods = model.Reference
            .Where(row => string.Equals(row.REFERENCE_SET, ProductionForecastingReferenceSets.ForecastMethod,
                StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.IsNullOrWhiteSpace(row.REFERENCE_CODE))
            .Select(row => row.REFERENCE_CODE.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var statuses = model.Reference
            .Where(row => string.Equals(row.REFERENCE_SET, ProductionForecastingReferenceSets.ForecastRunStatus,
                StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.IsNullOrWhiteSpace(row.REFERENCE_CODE))
            .Select(row => row.REFERENCE_CODE.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var risks = model.Reference
            .Where(row => string.Equals(row.REFERENCE_SET, ProductionForecastingReferenceSets.ForecastRiskRating,
                StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.IsNullOrWhiteSpace(row.REFERENCE_CODE))
            .Select(row => row.REFERENCE_CODE.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var fromTable = methods.Count > 0;

        var forecasts = model.Forecasts
            .Where(row => !string.IsNullOrWhiteSpace(row.PRODUCTION_FORECAST_ID))
            .OrderByDescending(row => row.RECORDED_AT ?? DateTime.MinValue)
            .ThenBy(row => row.FORECAST_NAME, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(row => row.VERSION)
            .Select(row => ToForecast(row, model.Points))
            .Take(100)
            .ToList();

        var selected = generated
            ?? (string.IsNullOrWhiteSpace(selectedId)
                ? forecasts.FirstOrDefault()
                : forecasts.FirstOrDefault(forecast =>
                      string.Equals(forecast.Id, selectedId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? forecasts.FirstOrDefault());

        var fits = model.Fits
            .Where(row => !string.IsNullOrWhiteSpace(row.FIT_ID))
            .Where(row => !string.Equals(row.OUTCOME, "REFUSED", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.RECORDED_AT ?? DateTime.MinValue)
            .Select(ToFit)
            .Take(50)
            .ToList();

        return new OgForecastWorkspace(
            forecasts,
            selected,
            fits,
            fromTable ? methods : Fallback<ForecastType>(),
            statuses.Count == 0 ? ForecastStatuses.Fallback : statuses,
            risks.Count == 0 ? [ProductionForecastingReferenceCodes.RiskUnknown] : risks,
            fromTable,
            false,
            null);
    }

    private static IReadOnlyList<string> Fallback<T>() where T : struct, Enum =>
        Enum.GetNames(typeof(T)).ToList();

    private static OgForecast ToForecast(PRODUCTION_FORECAST row, IReadOnlyList<PRODUCTION_FORECAST_POINT> points)
    {
        var own = points
            .Where(point => string.Equals(point.FORECAST_ID, row.PRODUCTION_FORECAST_ID,
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(point => point.FORECAST_DATE)
            .Select(point => new OgForecastPoint(
                point.FORECAST_DATE ?? DateTime.MinValue,
                point.OIL_RATE ?? 0m,
                point.GAS_RATE ?? 0m,
                point.WATER_RATE ?? 0m,
                point.MONTHLY_VOLUME,
                point.CUMULATIVE_VOLUME,
                string.IsNullOrWhiteSpace(point.VOLUME_OUOM) ? null : point.VOLUME_OUOM!.Trim()))
            .ToList();

        var oilUnit = Blank(row.OIL_VOLUME_OUOM);
        var gasUnit = Blank(row.GAS_VOLUME_OUOM);
        var waterUnit = Blank(row.WATER_VOLUME_OUOM);

        return new OgForecast(
            row.PRODUCTION_FORECAST_ID ?? string.Empty,
            row.FORECAST_NAME,
            Blank(row.WELL_UWI),
            Blank(row.FIELD_ID),
            row.FORECAST_TYPE.ToString(),
            Blank(row.SOURCE_FIT_ID),
            oilUnit is not null ? ForecastStreams.Oil : gasUnit is not null ? ForecastStreams.Gas :
                waterUnit is not null ? ForecastStreams.Water : string.Empty,
            row.FORECAST_START_DATE,
            (int)row.FORECAST_DURATION,
            string.IsNullOrWhiteSpace(row.STATUS) ? ForecastStatuses.Draft : row.STATUS!,
            Blank(row.RISK_RATING),
            row.VERSION,
            Blank(row.VERSION_OF),
            string.Equals(row.IMMUTABLE_IND, "Y", StringComparison.OrdinalIgnoreCase),
            row.MAX_RATE,
            row.ECONOMIC_LIMIT,
            oilUnit,
            gasUnit,
            waterUnit,
            own,
            row.INITIAL_PRODUCTION_RATE,
            row.FINAL_PRODUCTION_RATE,
            row.TOTAL_CUMULATIVE_PRODUCTION,
            row.AVERAGE_MONTHLY_PRODUCTION,
            null,
            null,
            Blank(row.ACTOR),
            row.RECORDED_AT,
            Blank(row.APPROVED_BY),
            row.APPROVED_AT);
    }

    private static OgDeclineFit ToFit(DECLINE_FIT row) =>
        new(
            row.FIT_ID ?? string.Empty,
            string.IsNullOrWhiteSpace(row.OUTCOME) ? "OK" : row.OUTCOME!,
            string.IsNullOrWhiteSpace(row.MODEL) ? DeclineModels.Hyperbolic : row.MODEL!,
            row.REFUSAL_CODE,
            row.REFUSAL_MESSAGE,
            row.QI,
            row.DI_PER_DAY,
            row.B,
            !string.Equals(row.BOUNDED_IND, "N", StringComparison.OrdinalIgnoreCase),
            row.EUR,
            row.ECONOMIC_LIMIT,
            row.VOLUME_OUOM,
            row.R_SQUARED,
            row.POINTS_USED,
            row.POINTS_EXCLUDED,
            [],
            [],
            new OgDeclineCrossCheck("not read here", false, null, null, null, null, false, "the forecast does not re-run the cross-check"),
            row.FLUID ?? string.Empty,
            row.WELL_ID,
            row.POOL_ID,
            row.HISTORY_FROM,
            row.HISTORY_TO,
            row.ACTOR,
            row.RECORDED_AT);

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>Everything the forecast reads, loaded once.</summary>
    public sealed record ForecastModel(
        IReadOnlyList<PRODUCTION_FORECAST> Forecasts,
        IReadOnlyList<PRODUCTION_FORECAST_POINT> Points,
        IReadOnlyList<DECLINE_FIT> Fits,
        IReadOnlyList<R_PRODUCTION_FORECASTING_REFERENCE_CODE> Reference);
}
