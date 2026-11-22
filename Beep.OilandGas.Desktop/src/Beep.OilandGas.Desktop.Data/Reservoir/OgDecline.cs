using System.Globalization;
using Beep.OilandGas.DCA;
using Beep.OilandGas.DCA.AdvancedDeclineMethods;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionForecasting.Constants;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Production;
using TheTechIdea.OilGas.Production;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;

/// <summary>The four decline models RES-06 offers.</summary>
public static class DeclineModels
{
    public const string Exponential = "EXPONENTIAL";
    public const string Harmonic = "HARMONIC";
    public const string Hyperbolic = "HYPERBOLIC";
    public const string ModifiedHyperbolic = "MODIFIED_HYPERBOLIC";

    public static IReadOnlyList<string> All { get; } =
        [Exponential, Harmonic, Hyperbolic, ModifiedHyperbolic];

    public static string NameOf(string model) => model switch
    {
        Exponential => "Exponential (b = 0)",
        Harmonic => "Harmonic (b = 1)",
        Hyperbolic => "Hyperbolic",
        ModifiedHyperbolic => "Modified hyperbolic",
        _ => model,
    };
}

/// <summary>The fluid columns of the PPDM volume summary RES-06 reads.</summary>
public static class DeclineFluids
{
    public const string Oil = "OIL";
    public const string Gas = "GAS";
    public const string Water = "WATER";

    public static IReadOnlyList<string> All { get; } = [Oil, Gas, Water];
}

/// <summary>The inputs of one decline fit (RES-06).</summary>
public sealed record OgDeclineInputs(
    string? WellId,
    string? Fluid,
    DateTime? From,
    DateTime? To,
    string? Model,
    decimal? FixedB,
    decimal? EconomicLimit);

/// <summary>One history point, with the fit's value at it and why it was or was not used (RES-06 AC-3).</summary>
public sealed record OgDeclinePoint(
    DateTime Date,
    decimal Rate,
    decimal? Fitted,
    decimal? Residual,
    bool Used,
    string? Reason)
{
    public string DateText => Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public string RateText => Rate.ToString("#,0.###", CultureInfo.InvariantCulture);

    public string FittedText => Fitted is { } value ? value.ToString("#,0.###", CultureInfo.InvariantCulture) : "-";

    public string ResidualText => Residual is { } value
        ? value.ToString("#,0.###", CultureInfo.InvariantCulture)
        : "-";

    public string UseText => Used ? "used" : "excluded: " + (Reason ?? "no reason recorded");

    public string Describe() => $"{DateText} {RateText} · fitted {FittedText} · residual {ResidualText} · {UseText}";
}

/// <summary>The independent cross-check's answer (RES-06 AC-4).</summary>
public sealed record OgDeclineCrossCheck(
    string Source,
    bool Available,
    decimal? Qi,
    decimal? Di,
    decimal? B,
    decimal? RSquared,
    bool Agrees,
    string Text)
{
    public string QiText => Qi?.ToString("#,0.###", CultureInfo.InvariantCulture) ?? "not available";

    public string BText => B?.ToString("0.####", CultureInfo.InvariantCulture) ?? "not available";

    public string AgreeText => !Available ? "unavailable" : Agrees ? "agrees" : "DISAGREES";
}

/// <summary>
/// The outcome of one decline fit (RES-06), matching the plan's <c>OgDeclineFit</c> plus the history and diagnostics the
/// screen needs.
/// </summary>
public sealed record OgDeclineFit(
    string Id,
    string Outcome,
    string Model,
    string? RefusalCode,
    string? RefusalMessage,
    decimal? Qi,
    decimal? DiPerDay,
    decimal? B,
    bool Bounded,
    decimal? Eur,
    decimal? EconomicLimit,
    string? Unit,
    decimal? RSquared,
    int PointsUsed,
    int PointsExcluded,
    IReadOnlyList<OgDeclinePoint> Points,
    IReadOnlyList<string> Gaps,
    OgDeclineCrossCheck CrossCheck,
    string Fluid,
    string? WellId,
    string? PoolId,
    DateTime? HistoryFrom,
    DateTime? HistoryTo,
    string? Actor,
    DateTime? RecordedAt)
{
    public const string OkOutcome = "OK";
    public const string RefusedOutcome = "REFUSED";

    public bool IsRefused => string.Equals(Outcome, RefusedOutcome, StringComparison.OrdinalIgnoreCase);

    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    /// <summary>AC-5: a non-converged or refused fit draws no curve.</summary>
    public bool HasCurve => !IsRefused && Qi is not null && DiPerDay is not null && B is not null;

    public string OutcomeText => Outcome.ToLowerInvariant();

    public string ModelText => DeclineModels.NameOf(Model);

    public string QiText => Qi is { } qi
        ? qi.ToString("#,0.###", CultureInfo.InvariantCulture)
        : "not computed";

    public string DiText => DiPerDay is { } di
        ? (di * 100m).ToString("0.####", CultureInfo.InvariantCulture) + " %/day"
        : "not computed";

    public string BText => B is { } b ? b.ToString("0.####", CultureInfo.InvariantCulture) : "not computed";

    public string EurText => Eur is { } eur
        ? eur.ToString("#,0.###", CultureInfo.InvariantCulture) + " " + (Unit ?? string.Empty).Trim()
        : "no economic limit given, so no EUR";

    public string R2Text => RSquared is { } r2 ? r2.ToString("0.####", CultureInfo.InvariantCulture) : "not computed";

    public string PointsText =>
        $"{PointsUsed.ToString(CultureInfo.InvariantCulture)} used · " +
        $"{PointsExcluded.ToString(CultureInfo.InvariantCulture)} excluded";

    public string BoundsText => Bounded
        ? "b is inside the configured bounds"
        : "b is NOT inside the configured bounds";

    public string RefusalText => IsRefused
        ? $"{RefusalCode ?? DeclineRules.CodeFitRefused}: {RefusalMessage ?? "the fit was refused"}"
        : "no refusal";

    public string ExclusionsText => Points.Count == 0
        ? "no history point was read"
        : Points.Where(point => !point.Used).Select(point => point.Describe()).DefaultIfEmpty("no point was excluded")
            .Aggregate((left, right) => left + Environment.NewLine + right);

    public string GapsText => Gaps.Count == 0
        ? "no gap in the history"
        : string.Join(Environment.NewLine, Gaps);

    public string CurveText => HasCurve
        ? string.Join(
            Environment.NewLine,
            Points.Where(point => point.Used && point.Fitted is not null)
                .Select(point => $"{point.DateText}  {point.RateText}  {point.FittedText}"))
        : "no curve is drawn for a fit that did not succeed";

    public string Describe() => IsRefused
        ? $"REFUSED · {ModelText} · {RefusalText}"
        : $"{ModelText} · qi {QiText} · Di {DiText} · b {BText} · EUR {EurText} · R2 {R2Text} · {PointsText} · " +
          $"{CrossCheck.AgreeText}";
}

/// <summary>The decline workspace: one fit, the history it used, the saved fits and the bounds on offer (RES-06).</summary>
public sealed record OgDeclineWorkspace(
    OgDeclineFit? Fit,
    IReadOnlyList<OgDeclineFit> History,
    IReadOnlyList<string> Models,
    IReadOnlyList<string> Fluids,
    decimal MinimumB,
    decimal MaximumB,
    decimal SoftMaximumB,
    int MinimumPoints,
    int MinimumPointsForFit,
    string AuthoritativeText,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string BoundsDisplay =>
        $"b is held between {MinimumB.ToString("0.##", CultureInfo.InvariantCulture)} and " +
        $"{MaximumB.ToString("0.##", CultureInfo.InvariantCulture)}; a fit that wants more than " +
        $"{MaximumB.ToString("0.##", CultureInfo.InvariantCulture)} is refused rather than reported at the bound. " +
        $"Above {SoftMaximumB.ToString("0.##", CultureInfo.InvariantCulture)} the fit is flagged, because a b that high " +
        "is transient flow rather than depletion.";

    public string HistoryText => History.Count == 0
        ? "No decline fit is saved yet."
        : string.Join(Environment.NewLine, History.Select(fit => fit.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved fits could not be read; new fits are still allowed"
        : $"{History.Count.ToString(CultureInfo.InvariantCulture)} saved fit(s)";
}

/// <summary>The decline rules, stated once so the page, the service and the tests agree.</summary>
public static class DeclineRules
{
    public const string CodeNoConnection = "decline-no-connection";
    public const string CodeReadFailed = "decline-read-failed";
    public const string CodeWriteFailed = "decline-write-failed";
    public const string CodeActorRequired = "decline-actor-required";
    public const string CodeWellRequired = "decline-well-required";
    public const string CodeWellUnknown = "decline-well-unknown";
    public const string CodeFluidInvalid = "decline-fluid-invalid";
    public const string CodeModelUnknown = "decline-model-unknown";
    public const string CodeTooFewPoints = "decline-too-few-points";
    public const string CodeBOutsideBounds = "decline-b-outside-bounds";
    public const string CodeFitUnbounded = "decline-fit-unbounded";
    public const string CodeFitFailed = "decline-fit-failed";
    public const string CodeFitRefused = "decline-fit-refused";
    public const string CodeNothingToSave = "decline-nothing-to-save";
    public const string CodeEconomicLimitInvalid = "decline-economic-limit-invalid";

    /// <summary>
    /// AC-4: the tolerance the cross-check is judged at. b is compared absolutely (0.05 is a fifth of the whole
    /// 0-1 range) and qi relatively (5%), and both must hold for the fitters to be said to agree.
    /// </summary>
    public const decimal BAgreementTolerance = 0.05m;

    public const decimal QiAgreementTolerance = 0.05m;

    /// <summary>AC-6: the fit is arithmetic, so the same history and model must give the same answer.</summary>
    public const string DeterminismText =
        "the same history, model and bounds always give the same fit: the search is a fixed golden-section sequence " +
        "with no random or clock-dependent step";

    public static bool IsModel(string? model) =>
        !string.IsNullOrWhiteSpace(model)
        && DeclineModels.All.Contains(model.Trim().ToUpperInvariant(), StringComparer.Ordinal);

    public static bool IsFluid(string? fluid) =>
        !string.IsNullOrWhiteSpace(fluid)
        && DeclineFluids.All.Contains(fluid.Trim().ToUpperInvariant(), StringComparer.Ordinal);

    public static string? CanonicalModel(string? model) =>
        IsModel(model)
            ? DeclineModels.All.First(candidate =>
                string.Equals(candidate, model!.Trim(), StringComparison.OrdinalIgnoreCase))
            : null;

    /// <summary>The bounds a model is fitted inside, from the owner's own Arps constants.</summary>
    public static (decimal Minimum, decimal Maximum, decimal SoftMaximum) BoundsFor(string model) =>
        CanonicalModel(model) == DeclineModels.ModifiedHyperbolic
            ? (ForecastAlgorithmConstants.ArpsBdfMinB, ForecastAlgorithmConstants.ArpsBdfSoftMaxB,
                ForecastAlgorithmConstants.ArpsBdfSoftMaxB)
            : (ForecastAlgorithmConstants.ArpsBdfMinB, ForecastAlgorithmConstants.ArpsBdfMaxB,
                ForecastAlgorithmConstants.ArpsBdfSoftMaxB);

    public static (string Code, string Message)? Validate(OgDeclineInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (string.IsNullOrWhiteSpace(inputs.WellId))
        {
            return (CodeWellRequired, "Choose the well whose history is fitted.");
        }

        if (!IsFluid(inputs.Fluid))
        {
            return (CodeFluidInvalid,
                $"'{inputs.Fluid}' is not a fluid; use {string.Join(", ", DeclineFluids.All)}.");
        }

        if (!IsModel(inputs.Model))
        {
            return (CodeModelUnknown,
                $"'{inputs.Model}' is not a decline model; use {string.Join(", ", DeclineModels.All)}.");
        }

        // AC-2: a b the operator fixes must be inside the bounds, whatever the model.
        if (inputs.FixedB is { } fixedB
            && (fixedB < ForecastAlgorithmConstants.ArpsBdfMinB || fixedB > ForecastAlgorithmConstants.ArpsBdfMaxB))
        {
            return (CodeBOutsideBounds,
                $"b = {fixedB.ToString("0.####", CultureInfo.InvariantCulture)} is outside the configured bounds " +
                $"{ForecastAlgorithmConstants.ArpsBdfMinB.ToString("0.##", CultureInfo.InvariantCulture)} to " +
                $"{ForecastAlgorithmConstants.ArpsBdfMaxB.ToString("0.##", CultureInfo.InvariantCulture)}.");
        }

        // A fixed b is only meaningful for a hyperbolic model; exponential and harmonic fix it themselves.
        return inputs.EconomicLimit is <= 0m
            ? (CodeEconomicLimitInvalid, "The economic limit rate must be above zero.")
            : null;
    }

    /// <summary>The cross-check's verdict, stated once so the page and the tests read the same rule.</summary>
    public static bool Agrees(decimal qi, decimal b, decimal crossQi, decimal crossB) =>
        Math.Abs(b - crossB) <= BAgreementTolerance
        && (qi <= 0m ? false : Math.Abs(qi - crossQi) / qi <= QiAgreementTolerance);
}

/// <summary>RES-06: decline curve analysis.</summary>
public interface IOgDeclineCurve
{
    /// <summary>Fits the well's history. Nothing is written.</summary>
    Task<OgResult<OgDeclineWorkspace>> AnalyzeAsync(
        OgDeclineInputs inputs,
        CancellationToken cancellationToken = default);

    /// <summary>Keeps a successful fit (the command table's "save analysis").</summary>
    Task<OgResult<OgDeclineWorkspace>> SaveAsync(
        OgDeclineFit fit,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The saved fits and the bounds on offer; an unavailable history is a state.</summary>
    Task<OgResult<OgDeclineWorkspace>> LoadAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// RES-06 over the ProductionForecasting <c>DCAGenerator</c> (the authoritative fit) and the sibling tools'
/// <c>DeclineFitter</c> (the independent cross-check), recording a fit in the ProductionForecasting-owned
/// <c>DECLINE_FIT</c> row through a unit of work.
/// </summary>
/// <remarks>
/// The history is the PPDM <c>PDEN_VOL_SUMMARY</c> at the grain PRD-01 established: the highest amendment per entity and
/// day, production rows only, summed within one unit. Points that cannot enter a log-scale fit - a zero or negative rate -
/// are excluded with their reason and counted (AC-3). The fit's b is held inside the module's Arps bounds, and a fit the
/// data pushes to the upper bound is refused rather than reported at the bound (AC-2, SG-06-6).
/// </remarks>
public sealed class OgDeclineCurveService : IOgDeclineCurve
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgDeclineCurveService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgDeclineWorkspace>> AnalyzeAsync(
        OgDeclineInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (DeclineRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgDeclineWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgDeclineWorkspace>.Failed(
                "No connection is open, so the production history cannot be read.",
                DeclineRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var fit = Fit(model, inputs);
            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgDeclineWorkspace>.Ok(Workspace(fit, history, false, null));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"fitting a decline curve for well {inputs.WellId}",
                "the fit is not shown",
                FailureSeverity.Error);

            return OgResult<OgDeclineWorkspace>.Failed(
                "The decline fit could not be run.", reference ?? DeclineRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgDeclineWorkspace>> SaveAsync(
        OgDeclineFit fit,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fit);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgDeclineWorkspace>.Failed(
                "A saved fit must record who saved it.", DeclineRules.CodeActorRequired);
        }

        // A refused fit is a refusal, not a result: keeping it would let a later reader mistake it for an analysis.
        if (fit.IsRefused || !fit.HasCurve)
        {
            return OgResult<OgDeclineWorkspace>.Failed(
                "There is no successful fit to save; run a fit that succeeds first.",
                DeclineRules.CodeNothingToSave);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgDeclineWorkspace>.Failed(
                "No connection is open, so the fit cannot be saved.", DeclineRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var unit = OgUow.For<DECLINE_FIT>(source);
            unit.PrimaryKey = "FIT_ID";

            unit.Add(new DECLINE_FIT
            {
                FIT_ID = Guid.NewGuid().ToString("N"),
                WELL_ID = fit.WellId,
                POOL_ID = fit.PoolId,
                FLUID = fit.Fluid,
                MODEL = fit.Model,
                OUTCOME = OgDeclineFit.OkOutcome,
                REFUSAL_CODE = null,
                REFUSAL_MESSAGE = null,
                QI = fit.Qi,
                DI_PER_DAY = fit.DiPerDay,
                B = fit.B,
                BOUNDED_IND = fit.Bounded ? "Y" : "N",
                EUR = fit.Eur,
                ECONOMIC_LIMIT = fit.EconomicLimit,
                VOLUME_OUOM = fit.Unit,
                R_SQUARED = fit.RSquared,
                POINTS_USED = fit.PointsUsed,
                POINTS_EXCLUDED = fit.PointsExcluded,
                EXCLUSIONS_TEXT = fit.PointsExcluded == 0
                    ? null
                    : string.Join(
                        "\n",
                        fit.Points.Where(point => !point.Used).Select(point => point.Describe())),
                HISTORY_FROM = fit.HistoryFrom,
                HISTORY_TO = fit.HistoryTo,
                CROSS_CHECK_SOURCE = fit.CrossCheck.Source,
                CROSS_CHECK_QI = fit.CrossCheck.Qi,
                CROSS_CHECK_B = fit.CrossCheck.B,
                CROSS_CHECK_AGREES = fit.CrossCheck.Available ? (fit.CrossCheck.Agrees ? "Y" : "N") : null,
                CROSS_CHECK_TEXT = fit.CrossCheck.Text,
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
                return OgResult<OgDeclineWorkspace>.Failed(
                    OgUow.Message(committed, "The fit was not saved."), DeclineRules.CodeWriteFailed);
            }

            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            var saved = fit with { Actor = actor.Trim(), RecordedAt = now, Id = string.Empty };

            return OgResult<OgDeclineWorkspace>.Ok(Workspace(saved, history, false, null));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving a decline fit for well {fit.WellId}",
                "the fit was not saved",
                FailureSeverity.Error);

            return OgResult<OgDeclineWorkspace>.Failed(
                "The fit could not be saved.", reference ?? DeclineRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgDeclineWorkspace>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgDeclineWorkspace>.Failed(
                "No connection is open, so the saved fits cannot be read.", DeclineRules.CodeNoConnection);
        }

        try
        {
            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgDeclineWorkspace>.Ok(Workspace(null, history, false, null));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the saved decline fits",
                "the saved fits are unavailable; new fits are still allowed",
                FailureSeverity.Degraded);

            return OgResult<OgDeclineWorkspace>.Ok(Workspace(
                null, [], true, $"the saved fits could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the fit

    /// <summary>
    /// The fit itself: the exclusion rule, the too-few-points refusal, the bounded fit, the unbounded refusal, the EUR
    /// and the independent cross-check, in the order the state table lists them.
    /// </summary>
    public OgDeclineFit Fit(DeclineModel model, OgDeclineInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(inputs);

        var decline = CanonicalModel(inputs.Model);
        var fluid = inputs.Fluid!.Trim().ToUpperInvariant();
        var (minimumB, maximumB, _) = DeclineRules.BoundsFor(decline);

        var well = inputs.WellId!.Trim();

        var points = model.Readings
            .Where(reading => string.Equals(reading.WellId, well, StringComparison.OrdinalIgnoreCase))
            .Where(reading => reading.Fluid == fluid)
            .Where(reading => inputs.From is null || reading.Date >= inputs.From.Value.Date)
            .Where(reading => inputs.To is null || reading.Date <= inputs.To.Value.Date)
            .OrderBy(reading => reading.Date)
            .ToList();

        var unit = points
            .Select(point => point.Unit)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(value => value!, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key)
            .FirstOrDefault();

        var gaps = new List<string>();

        for (var index = 1; index < points.Count; index++)
        {
            var days = (points[index].Date - points[index - 1].Date).Days;

            if (days > 31)
            {
                gaps.Add(
                    $"{points[index - 1].Date:yyyy-MM-dd} to {points[index].Date:yyyy-MM-dd}: " +
                    $"{days.ToString(CultureInfo.InvariantCulture)} days with no recorded volume");
            }
        }

        var flagged = points
            .Select(point => point.Rate <= 0m
                ? new OgDeclinePoint(
                    point.Date, point.Rate, null, null, false,
                    "a rate of zero or less cannot be fitted on a log scale")
                : new OgDeclinePoint(point.Date, point.Rate, null, null, true, null))
            .ToList();

        var usable = flagged.Where(point => point.Used).ToList();
        var excluded = flagged.Count - usable.Count;

        DateTime? from = points.Count == 0 ? null : points[0].Date;
        DateTime? to = points.Count == 0 ? null : points[^1].Date;

        OgDeclineFit Refused(string code, string message) =>
            new(
                string.Empty,
                OgDeclineFit.RefusedOutcome,
                decline,
                code,
                message,
                null,
                null,
                null,
                false,
                null,
                inputs.EconomicLimit,
                unit,
                null,
                usable.Count,
                excluded,
                flagged,
                gaps,
                Unavailable(),
                fluid,
                inputs.WellId,
                model.PoolId,
                from,
                to,
                null,
                null);

        // The state table's "too few points": the minimum is named.
        if (usable.Count < ForecastAlgorithmConstants.MinHistoryPointsForFit)
        {
            return Refused(
                DeclineRules.CodeTooFewPoints,
                $"the fit needs at least {ForecastAlgorithmConstants.MinHistoryPointsForFit.ToString(CultureInfo.InvariantCulture)} " +
                $"usable points and this range has {usable.Count.ToString(CultureInfo.InvariantCulture)}" +
                (excluded == 0
                    ? string.Empty
                    : $" ({excluded.ToString(CultureInfo.InvariantCulture)} point(s) were excluded, with their reasons listed)"));
        }

        var rates = usable.Select(point => (double)point.Rate).ToList();
        var times = usable.Select(point => point.Date).ToList();

        var fixedB = inputs.FixedB;

        if (decline == DeclineModels.Exponential)
        {
            fixedB = 0m;
        }
        else if (decline == DeclineModels.Harmonic)
        {
            fixedB = 1m;
        }

        var fitted = fixedB is { } value
            ? DCAGenerator.TryFitDeclineAtB(rates, times, value, out var atB) ? atB : (DCAGenerator.ArpsFit?)null
            : DCAGenerator.TryFitDecline(rates, times, minimumB, maximumB, out var searched)
                ? searched
                : null;

        if (fitted is not { } fit)
        {
            return Refused(
                DeclineRules.CodeFitFailed,
                "the engine could not fit this history: no parameter set reproduces it. No curve is drawn.");
        }

        // AC-2 and SG-06-6: b outside the bounds is refused, and so is a b the data pushes to the upper bound.
        if (fit.PinnedAtMaximum && fixedB is null)
        {
            return Refused(
                DeclineRules.CodeFitUnbounded,
                $"the data wants a decline exponent above the configured maximum " +
                $"{maximumB.ToString("0.####", CultureInfo.InvariantCulture)} (the fit settles at the bound), so it is " +
                "refused rather than reported as a result. Choose the hyperbolic model with a fixed b, or a shorter " +
                "range, and state why.");
        }

        if ((decimal)fit.B < ForecastAlgorithmConstants.ArpsBdfMinB
            || (decimal)fit.B > ForecastAlgorithmConstants.ArpsBdfMaxB)
        {
            return Refused(
                DeclineRules.CodeBOutsideBounds,
                $"the fitted b {fit.B.ToString("0.####", CultureInfo.InvariantCulture)} is outside the configured bounds " +
                $"{ForecastAlgorithmConstants.ArpsBdfMinB.ToString("0.##", CultureInfo.InvariantCulture)} to " +
                $"{ForecastAlgorithmConstants.ArpsBdfMaxB.ToString("0.##", CultureInfo.InvariantCulture)}.");
        }

        var residualPoints = usable
            .Select((point, index) => point with
            {
                Fitted = (decimal)fit.Predicted[index],
                Residual = point.Rate - (decimal)fit.Predicted[index],
            })
            .ToList();

        var merged = flagged
            .Select(point => point.Used
                ? residualPoints[usable.FindIndex(used => used.Date == point.Date && used.Rate == point.Rate)]
                : point)
            .ToList();

        var eur = Eur(fit.Qi, fit.Di, fit.B, inputs.EconomicLimit);

        var crossCheck = CrossCheck(
            usable.Select(point => (point.Date, (double)point.Rate)).ToList(),
            fit.Qi,
            fit.B);

        return new OgDeclineFit(
            string.Empty,
            OgDeclineFit.OkOutcome,
            decline,
            null,
            null,
            (decimal)fit.Qi,
            (decimal)fit.Di,
            (decimal)fit.B,
            (decimal)fit.B >= ForecastAlgorithmConstants.ArpsBdfMinB
            && (decimal)fit.B <= ForecastAlgorithmConstants.ArpsBdfMaxB,
            eur is null ? null : (decimal)eur.Value,
            inputs.EconomicLimit,
            unit,
            (decimal)fit.RSquared,
            fit.PointsUsed,
            excluded,
            merged,
            gaps,
            crossCheck,
            fluid,
            inputs.WellId,
            model.PoolId,
            from,
            to,
            null,
            null);
    }

    /// <summary>
    /// The EUR from the owner's closed forms at the economic limit the operator states. No limit means no EUR: the
    /// exponential theoretical EUR at infinite time is not a reserve, so it is not offered as one.
    /// </summary>
    private static double? Eur(double qi, double di, double b, decimal? economicLimit)
    {
        if (economicLimit is not { } limit || limit <= 0m || (double)limit > qi || di <= 0.0)
        {
            return null;
        }

        var economic = (double)limit;

        return b switch
        {
            0.0 => ArpsDeclineMethods.ExponentialCumulativeProduction(qi, di, Math.Log(qi / economic) / di),
            1.0 => ArpsDeclineMethods.HarmonicReserves(qi, di, economic),
            _ => ArpsDeclineMethods.HyperbolicReserves(qi, di, b, economic),
        };
    }

    /// <summary>
    /// AC-4: the sibling tools' <c>DeclineFitter</c> is fitted to the same points and compared. It is guarded, because a
    /// cross-check that cannot run is a state of the answer, not a failure of the fit.
    /// </summary>
    private OgDeclineCrossCheck CrossCheck(
        IReadOnlyList<(DateTime Date, double Rate)> points,
        double qi,
        double b)
    {
        const string source = "TheTechIdea.OilGas.Production DeclineFitter (independent implementation)";

        try
        {
            var observations = points
                .Select(point => new RateObservation((point.Date - points[0].Date).TotalDays, point.Rate))
                .ToList();

            var fit = DeclineFitter.Fit(
                observations,
                (double)ForecastAlgorithmConstants.ArpsBdfMinB,
                (double)ForecastAlgorithmConstants.ArpsBdfMaxB);

            var agrees = DeclineRules.Agrees(
                (decimal)fit.Curve.Qi,
                (decimal)fit.Curve.B,
                (decimal)qi,
                (decimal)b);

            return new OgDeclineCrossCheck(
                source,
                true,
                (decimal)fit.Curve.Qi,
                (decimal)fit.Curve.Di,
                (decimal)fit.Curve.B,
                (decimal)fit.RSquared,
                agrees,
                agrees
                    ? $"the independent fitter agrees: qi {fit.Curve.Qi.ToString("#,0.###", CultureInfo.InvariantCulture)}, " +
                      $"b {fit.Curve.B.ToString("0.####", CultureInfo.InvariantCulture)} " +
                      $"(within b {DeclineRules.BAgreementTolerance.ToString("0.##", CultureInfo.InvariantCulture)} and " +
                      $"qi {DeclineRules.QiAgreementTolerance.ToString("0.##%", CultureInfo.InvariantCulture)})"
                    : $"the independent fitter disagrees: it reads qi " +
                      $"{fit.Curve.Qi.ToString("#,0.###", CultureInfo.InvariantCulture)} and b " +
                      $"{fit.Curve.B.ToString("0.####", CultureInfo.InvariantCulture)} against this fit's " +
                      $"{qi.ToString("#,0.###", CultureInfo.InvariantCulture)} and " +
                      $"{b.ToString("0.####", CultureInfo.InvariantCulture)}; both are shown and the disagreement stands");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            _failures.ReportHandled(
                ex,
                "running the independent decline-fit cross-check",
                "only this fit is shown, because the cross-check could not be run",
                FailureSeverity.Degraded);

            return Unavailable();
        }
    }

    private static OgDeclineCrossCheck Unavailable(string? reason = null) =>
        new(
            "TheTechIdea.OilGas.Production DeclineFitter (independent implementation)",
            false,
            null,
            null,
            null,
            null,
            false,
            "the cross-check could not be run" + (reason is null ? string.Empty : $" ({reason})") +
            ", so only this fit is shown");

    // ------------------------------------------------------------------ the reads

    private static async Task<DeclineModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var wells = await OgUow.ReadAllAsync<WELL>(source).ConfigureAwait(false);
        var entities = await OgUow.ReadAllAsync<PDEN_WELL>(source).ConfigureAwait(false);
        var volumes = await OgUow.ReadAllAsync<PDEN_VOL_SUMMARY>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new DeclineModel(wells, entities, volumes, BuildReadings(entities, volumes));
    }

    /// <summary>
    /// The daily readings, at PRD-01's grain: the highest amendment per entity, day and activity, production rows only,
    /// each fluid summed within the dominant unit it is recorded in.
    /// </summary>
    private static IReadOnlyList<DeclineReading> BuildReadings(
        IReadOnlyList<PDEN_WELL> entities,
        IReadOnlyList<PDEN_VOL_SUMMARY> volumes)
    {
        var entityToWell = entities
            .Where(row => !string.IsNullOrWhiteSpace(row.PDEN_ID) && !string.IsNullOrWhiteSpace(row.PRIMARY_UWI))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .GroupBy(row => row.PDEN_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().PRIMARY_UWI!, StringComparer.OrdinalIgnoreCase);

        var rows = volumes
            .Where(row => !string.IsNullOrWhiteSpace(row.PDEN_ID) && entityToWell.ContainsKey(row.PDEN_ID!))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => row.VOLUME_DATE is not null)
            .Where(row => OperationsCentreRules.IsProductionRow(row.ACTIVITY_TYPE))
            .GroupBy(row => (row.PDEN_ID!.Trim(), Day: row.VOLUME_DATE!.Value.Date,
                Activity: (row.ACTIVITY_TYPE ?? string.Empty).Trim().ToUpperInvariant()))
            .Select(group => group.OrderByDescending(row => row.AMENDMENT_SEQ_NO).First())
            .ToList();

        var readings = new List<DeclineReading>();

        foreach (var fluid in DeclineFluids.All)
        {
            var units = rows
                .Select(row => UnitOf(row, fluid))
                .Where(unit => !string.IsNullOrWhiteSpace(unit))
                .GroupBy(unit => unit!, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(group => group.Count())
                .Select(group => group.Key)
                .ToList();

            if (units.Count == 0)
            {
                continue;
            }

            var unit = units[0];

            var daily = rows
                .Where(row => string.Equals(UnitOf(row, fluid), unit, StringComparison.OrdinalIgnoreCase))
                .GroupBy(row => (Well: entityToWell[row.PDEN_ID!], Day: row.VOLUME_DATE!.Value.Date))
                .Select(group => new DeclineReading(
                    group.Key.Well,
                    group.Key.Day,
                    fluid,
                    group.Sum(row => AmountOf(row, fluid)),
                    unit));

            readings.AddRange(daily);
        }

        return readings;
    }

    private static string? UnitOf(PDEN_VOL_SUMMARY row, string fluid) => fluid switch
    {
        DeclineFluids.Oil => Blank(row.OIL_VOLUME_OUOM),
        DeclineFluids.Gas => Blank(row.GAS_VOLUME_OUOM),
        _ => Blank(row.WATER_VOLUME_OUOM),
    };

    private static decimal AmountOf(PDEN_VOL_SUMMARY row, string fluid) => fluid switch
    {
        DeclineFluids.Oil => row.OIL_VOLUME,
        DeclineFluids.Gas => row.GAS_VOLUME,
        _ => row.WATER_VOLUME,
    };

    private static async Task<IReadOnlyList<OgDeclineFit>> ReadHistoryAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAllAsync<DECLINE_FIT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.FIT_ID))
            .OrderByDescending(row => row.RECORDED_AT ?? DateTime.MinValue)
            .ThenBy(row => row.FIT_ID, StringComparer.Ordinal)
            .Select(ToFit)
            .Take(50)
            .ToList();
    }

    private static OgDeclineFit ToFit(DECLINE_FIT row) =>
        new(
            row.FIT_ID ?? string.Empty,
            string.IsNullOrWhiteSpace(row.OUTCOME) ? OgDeclineFit.OkOutcome : row.OUTCOME!,
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
            string.IsNullOrWhiteSpace(row.EXCLUSIONS_TEXT)
                ? []
                : row.EXCLUSIONS_TEXT!
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(line => new OgDeclinePoint(default, 0m, null, null, false, line))
                    .ToList(),
            [],
            new OgDeclineCrossCheck(
                string.IsNullOrWhiteSpace(row.CROSS_CHECK_SOURCE) ? "not recorded" : row.CROSS_CHECK_SOURCE!,
                row.CROSS_CHECK_QI is not null || row.CROSS_CHECK_B is not null,
                row.CROSS_CHECK_QI,
                null,
                row.CROSS_CHECK_B,
                null,
                string.Equals(row.CROSS_CHECK_AGREES, "Y", StringComparison.OrdinalIgnoreCase),
                row.CROSS_CHECK_TEXT ?? "no cross-check was recorded"),
            row.FLUID ?? string.Empty,
            row.WELL_ID,
            row.POOL_ID,
            row.HISTORY_FROM,
            row.HISTORY_TO,
            row.ACTOR,
            row.RECORDED_AT);

    private static OgDeclineWorkspace Workspace(
        OgDeclineFit? fit,
        IReadOnlyList<OgDeclineFit> history,
        bool historyFailed,
        string? historyReason) =>
        new(
            fit,
            history,
            DeclineModels.All,
            DeclineFluids.All,
            ForecastAlgorithmConstants.ArpsBdfMinB,
            ForecastAlgorithmConstants.ArpsBdfMaxB,
            ForecastAlgorithmConstants.ArpsBdfSoftMaxB,
            ForecastAlgorithmConstants.MinHistoryPointsForFit,
            ForecastAlgorithmConstants.MinHistoryPointsForFit,
            "the ProductionForecasting module's DCAGenerator is authoritative; the tools' DeclineFitter is the " +
            "independent cross-check, and a disagreement is shown rather than resolved",
            historyFailed,
            historyReason);

    private static string? CanonicalModel(string? model) => DeclineRules.CanonicalModel(model);

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>One recorded daily rate for one well and fluid.</summary>
    public sealed record DeclineReading(string WellId, DateTime Date, string Fluid, decimal Rate, string? Unit);

    /// <summary>Everything the fit reads, loaded once.</summary>
    public sealed record DeclineModel(
        IReadOnlyList<WELL> Wells,
        IReadOnlyList<PDEN_WELL> Entities,
        IReadOnlyList<PDEN_VOL_SUMMARY> Volumes,
        IReadOnlyList<DeclineReading> Readings)
    {
        /// <summary>The pool the well is recorded in, taken from its field when the well row has no pool of its own.</summary>
        public string? PoolId => Wells
            .FirstOrDefault(well => string.Equals(well.UWI, Readings.FirstOrDefault()?.WellId,
                StringComparison.OrdinalIgnoreCase))
            ?.ASSIGNED_FIELD;
    }
}
