using System.Globalization;
using Beep.OilandGas.EconomicAnalysis.Calculations;
using Beep.OilandGas.EconomicAnalysis.Constants;
using Beep.OilandGas.EconomicAnalysis.Data;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Economics;

/// <summary>One period of an evaluation (ECO-01).</summary>
public sealed record OgEconomicPeriod(
    int Period,
    int Year,
    decimal Volume,
    decimal Price,
    decimal Revenue,
    decimal Royalty,
    decimal Opex,
    decimal Capex,
    decimal Abandonment,
    decimal PreTax,
    decimal Tax,
    decimal Net,
    decimal Discounted,
    decimal Cumulative)
{
    public string Describe() =>
        $"{Year} · volume {Number(Volume)} · price {Number(Price)} · revenue {Money(Revenue)} · royalty " +
        $"{Money(Royalty)} · opex {Money(Opex)} · capex {Money(Capex)} · net {Money(Net)} · " +
        $"discounted {Money(Discounted)} · cumulative {Money(Cumulative)}";

    private static string Number(decimal value) =>
        value.ToString("#,0.###", CultureInfo.InvariantCulture);

    private static string Money(decimal value) => MoneyText.Of(value);
}

/// <summary>One sensitivity driver (ECO-01 AC-5, "show which inputs drive the answer").</summary>
public sealed record OgEconomicDriver(
    string Name,
    decimal LowValue,
    decimal BaseValue,
    decimal HighValue,
    decimal LowNpv,
    decimal BaseNpv,
    decimal HighNpv)
{
    /// <summary>How far the answer moves across the driver's range - what the ranking uses.</summary>
    public decimal Swing => HighNpv - LowNpv;

    public decimal SwingMagnitude => Math.Abs(Swing);

    public string Describe() =>
        $"{Name}: {Number(LowValue)} to {Number(HighValue)} moves NPV from {MoneyText.Of(LowNpv)} to " +
        $"{MoneyText.Of(HighNpv)} (base {MoneyText.Of(BaseNpv)}, swing {MoneyText.Of(Swing)})";

    private static string Number(decimal value) =>
        value.ToString("#,0.####", CultureInfo.InvariantCulture);
}

/// <summary>The inputs of one economic evaluation (ECO-01).</summary>
public sealed record OgEconomicInputs(
    string? EntityId,
    string? Scenario,
    string? ForecastVersion,
    string? Currency,
    decimal? DiscountRate,
    string? RegimeName,
    decimal? RoyaltyRate,
    decimal? TaxRate,
    int? PeriodCount,
    int? StartYear,
    decimal? AnnualVolume,
    string? VolumeUnit,
    decimal? UnitPrice,
    string? PriceUnit,
    decimal? OpexPerUnit,
    decimal? AnnualCapex,
    decimal? EscalationRate,
    decimal? AbandonmentCost);

/// <summary>
/// One economic evaluation (ECO-01), matching the plan's <c>OgEconomicResult</c> plus the periods written and the status.
/// </summary>
public sealed record OgEconomicResult(
    string Id,
    string? EntityId,
    string Scenario,
    string ForecastVersion,
    string Currency,
    decimal DiscountRate,
    string RegimeName,
    decimal RoyaltyRate,
    decimal TaxRate,
    decimal Npv,
    decimal? Irr,
    decimal? PaybackMonths,
    decimal PeakFunding,
    decimal UndiscountedTotal,
    IReadOnlyList<OgEconomicPeriod> Periods,
    IReadOnlyList<OgEconomicDriver> Drivers,
    string Status,
    int Version,
    string? Actor,
    DateTime? RecordedAt,
    string? ApprovedBy,
    DateTime? ApprovedAt)
{
    public const string StatusDraft = "DRAFT";
    public const string StatusApproved = "APPROVED";

    /// <summary>
    /// The inputs the evaluation was run with, set by <c>Build</c> and by the read path - a saved row is re-runnable from
    /// its own columns.
    /// </summary>
    public OgEconomicInputs Inputs { get; init; } = null!;

    /// <summary>The regime's own description, including that it is a screening model.</summary>
    public string RegimeText { get; init; } = string.Empty;

    /// <summary>The drivers as the saved row stores them, when the row came from the database.</summary>
    public string? StoredDriversText { get; init; }

    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    public bool IsApproved => string.Equals(Status, StatusApproved, StringComparison.OrdinalIgnoreCase);

    /// <summary>AC-2: an IRR that does not exist is "undefined", never 0.</summary>
    public bool IrrDefined => Irr is not null;

    public string NpvText => MoneyText.Of(Npv);

    public string IrrText => Irr is { } irr
        ? (irr * 100m).ToString("0.####", CultureInfo.InvariantCulture) + " %"
        : "undefined - the cash flow never changes sign, so no rate solves it";

    /// <summary>AC-2 in the stored form, so a saved row cannot read as a zero IRR.</summary>
    public string IrrStoredText => Irr is { } irr
        ? (irr * 100m).ToString("0.####", CultureInfo.InvariantCulture) + " %"
        : "undefined";

    public string PaybackText => PaybackMonths is { } months
        ? months.ToString("#,0.##", CultureInfo.InvariantCulture) + " months"
        : "never pays back within the periods evaluated";

    public string PeakFundingText => MoneyText.Of(PeakFunding);

    public string TotalText => MoneyText.Of(UndiscountedTotal);

    public string PeriodsText => Periods.Count == 0
        ? "no periods"
        : string.Join(Environment.NewLine, Periods.Select(period => period.Describe()));

    /// <summary>AC-5: the drivers, largest swing first - the same numbers the table shows, ordered for the tornado.</summary>
    public string DriversText => Drivers.Count == 0
        ? "no sensitivity was run"
        : string.Join(Environment.NewLine, Drivers.Select(driver => driver.Describe()));

    public string StatusText => IsApproved
        ? $"APPROVED by {ApprovedBy ?? "(unknown)"} on " +
          $"{ApprovedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date"}"
        : $"draft v{Version.ToString(CultureInfo.InvariantCulture)}, saved by {Actor ?? "(unknown)"}";

    /// <summary>SG-09-3: one evaluation, one scenario, one forecast version - said in one line.</summary>
    public string ScenarioText =>
        $"{Scenario} · forecast {ForecastVersion} · {Currency} · discount " +
        (DiscountRate * 100m).ToString("0.####", CultureInfo.InvariantCulture) + " %";

    public string Describe() =>
        $"{EntityId ?? "(no entity)"} · {ScenarioText} · NPV {NpvText} · IRR {IrrStoredText} · {StatusText}";
}

/// <summary>A result whose save was not acknowledged: retained and retried (ECO-01's state table).</summary>
public sealed record OgEconomicPending(OgEconomicResult Result, string Actor);

/// <summary>The economics workspace (ECO-01).</summary>
public sealed record OgEconomicsWorkspace(
    IReadOnlyList<OgEconomicResult> Results,
    OgEconomicResult? Selected,
    IReadOnlyList<string> Scenarios,
    IReadOnlyList<string> Currencies,
    IReadOnlyList<string> VolumeUnits,
    FiscalRegime Regime,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string ResultsText => Results.Count == 0
        ? "No economic evaluation is saved yet."
        : string.Join(Environment.NewLine, Results.Select(result => result.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved evaluations could not be read; new evaluations are still allowed"
        : $"{Results.Count.ToString(CultureInfo.InvariantCulture)} saved evaluation(s)";

    /// <summary>
    /// SG-09-3: the comparison across scenarios. Each line states the one forecast version that scenario used, and a
    /// scenario carrying two different versions is flagged rather than silently averaged.
    /// </summary>
    public string ComparisonText
    {
        get
        {
            if (Results.Count == 0)
            {
                return "no scenario to compare yet";
            }

            var lines = Results
                .OrderBy(result => result.Scenario, StringComparer.OrdinalIgnoreCase)
                .Select(result =>
                    $"{result.Scenario}: NPV {result.NpvText} · IRR {result.IrrStoredText} · " +
                    $"forecast {result.ForecastVersion} · {result.StatusText}")
                .ToList();

            var conflicts = Results
                .GroupBy(result => result.Scenario, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Select(result => result.ForecastVersion)
                    .Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (conflicts.Count > 0)
            {
                lines.Add("one forecast per scenario is not satisfied for: " + string.Join(", ", conflicts) +
                          " - each line above states the version it used, and they differ");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }

    public string RegimeText => Regime.Describe();

    /// <summary>AC-4: the run is blocked until the regime is complete, and the panel says which term is missing.</summary>
    public string RegimeBlockedText => Regime.IsComplete
        ? string.Empty
        : Regime.BlockedReason;

    public string ReferenceText =>
        $"scenarios from R_ECONOMIC_SCENARIO · all money is decimal · fiscal terms: " +
        $"{string.Join(", ", FiscalRegime.RequiredTerms)}";
}

/// <summary>The economics rules, stated once so the page, the service and the tests agree.</summary>
public static class EconomicRules
{
    public const string CodeNoConnection = "economics-no-connection";
    public const string CodeReadFailed = "economics-read-failed";
    public const string CodeWriteFailed = "economics-write-failed";
    public const string CodeActorRequired = "economics-actor-required";
    public const string CodeEntityRequired = "economics-entity-required";
    public const string CodeInputRequired = "economics-input-required";
    public const string CodeInputInvalid = "economics-input-invalid";
    public const string CodeScenarioUnknown = "economics-scenario-unknown";
    public const string CodeScenarioTableEmpty = "economics-scenario-table-empty";
    public const string CodeFiscalTermMissing = "economics-fiscal-term-missing";
    public const string CodeFiscalTermOutOfRange = "economics-fiscal-term-out-of-range";
    public const string CodeCurrencyMismatch = "economics-currency-mismatch";
    public const string CodeApprovedImmutable = "economics-approved-immutable";
    public const string CodeNothingToSave = "economics-nothing-to-save";
    public const string CodeNothingPending = "economics-nothing-pending";
    public const string CodeAlreadyApproved = "economics-already-approved";

    /// <summary>The currencies an evaluation may state. Mixed currency is refused (the state table).</summary>
    public static IReadOnlyList<string> Currencies { get; } = ["USD", "EUR", "GBP", "SAR", "AED"];

    /// <summary>The volume units a forecast can be in.</summary>
    public static IReadOnlyList<string> VolumeUnits { get; } = ["bbl", "Mscf", "boe"];

    /// <summary>The one-way sensitivity applied to every driver (10 %).</summary>
    public const decimal SensitivityPercent = 0.10m;

    /// <summary>The fewest periods an evaluation can have.</summary>
    public const int MinimumPeriods = 1;

    /// <summary>The most periods - the engine's own limit is 1,000.</summary>
    public const int MaximumPeriods = 100;

    public static IReadOnlyList<string> Missing(OgEconomicInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = new List<string>();

        void Need(object? value, string name)
        {
            if (value is null || (value is string text && string.IsNullOrWhiteSpace(text)))
            {
                missing.Add(name);
            }
        }

        Need(inputs.EntityId, "well or entity");
        Need(inputs.Scenario, "scenario");
        Need(inputs.ForecastVersion, "forecast version");
        Need(inputs.Currency, "currency");
        Need(inputs.RegimeName, "fiscal regime name");
        Need(inputs.PeriodCount, "period count");
        Need(inputs.StartYear, "start year");
        Need(inputs.AnnualVolume, "annual volume");
        Need(inputs.VolumeUnit, "volume unit");
        Need(inputs.UnitPrice, "unit price");
        Need(inputs.PriceUnit, "price unit");
        Need(inputs.OpexPerUnit, "opex per unit");
        Need(inputs.AnnualCapex, "annual capex");
        Need(inputs.AbandonmentCost, "abandonment cost");
        Need(inputs.DiscountRate, "discount rate");

        return missing;
    }

    public static (string Code, string Message)? Validate(OgEconomicInputs inputs, FiscalRegime regime)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            return missing.Contains("well or entity")
                ? (CodeEntityRequired, "Choose the well, pool or field the evaluation is for.")
                : (CodeInputRequired, $"The evaluation needs {string.Join(", ", missing)}.");
        }

        var currency = inputs.Currency!.Trim().ToUpperInvariant();

        if (!Currencies.Contains(currency, StringComparer.Ordinal))
        {
            return (CodeCurrencyMismatch,
                $"'{inputs.Currency}' is not a currency this app evaluates in; use {string.Join(", ", Currencies)}.");
        }

        // AC-4 and SG-09-2: the fiscal terms are configuration, and a missing one blocks the run by name.
        if (FiscalRegimeRules.Check(regime) is { } fiscal)
        {
            return fiscal;
        }

        if (inputs.DiscountRate is < 0m or > 1m)
        {
            return (CodeInputInvalid,
                "The discount rate is a fraction between 0 and 1; 10 % is entered as 0.10.");
        }

        if (inputs.EscalationRate is < -1m or > 1m)
        {
            return (CodeInputInvalid, "The escalation rate is a fraction between -1 and 1.");
        }

        if (inputs.PeriodCount is < MinimumPeriods or > MaximumPeriods)
        {
            return (CodeInputInvalid,
                $"The evaluation runs between {MinimumPeriods.ToString(CultureInfo.InvariantCulture)} and " +
                $"{MaximumPeriods.ToString(CultureInfo.InvariantCulture)} periods.");
        }

        if (inputs.AnnualVolume <= 0m && inputs.AnnualVolume is not null)
        {
            return (CodeInputInvalid, "The annual volume must be above zero, or nothing is produced.");
        }

        return null;
    }
}

/// <summary>Money as the screen prints it, in one place so a period row and a total cannot differ.</summary>
internal static class MoneyText
{
    public static string Of(decimal value) =>
        value.ToString("#,0.00", CultureInfo.InvariantCulture);
}

/// <summary>ECO-01: economic evaluation.</summary>
public interface IOgEconomics
{
    /// <summary>Runs the evaluation. Nothing is written.</summary>
    Task<OgResult<OgEconomicsWorkspace>> EvaluateAsync(
        OgEconomicInputs inputs,
        CancellationToken cancellationToken = default);

    /// <summary>Saves a draft with its periods (AC-6).</summary>
    Task<OgResult<OgEconomicsWorkspace>> SaveAsync(
        OgEconomicResult result,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Approves a saved draft; an approved evaluation is never updated again (AC-6).</summary>
    Task<OgResult<OgEconomicsWorkspace>> ApproveAsync(
        string resultId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The saved evaluations; an unavailable history is a state.</summary>
    Task<OgResult<OgEconomicsWorkspace>> LoadAsync(
        string? entityId = null,
        string? resultId = null,
        CancellationToken cancellationToken = default);

    /// <summary>The result retained when its save was not acknowledged.</summary>
    OgEconomicPending? Pending { get; }

    /// <summary>Retries the same insert.</summary>
    Task<OgResult<OgEconomicsWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ECO-01 over the EconomicAnalysis module's decimal money arithmetic, its fiscal regime and its
/// <c>ECONOMIC_ANALYSIS_RESULT</c> and <c>ECONOMIC_CASH_FLOW</c> rows, through units of work.
/// </summary>
/// <remarks>
/// The cash flow is built from the stated volume, price deck escalation, opex, capex and abandonment against the fiscal
/// terms, and every money value is <c>decimal</c> end to end (AC-3). The sensitivity calls the same
/// <see cref="BuildPeriods"/> and the same <see cref="EconomicMoneyCalculator.Npv"/> as the base run, only with one input
/// scaled, so the table and the tornado cannot disagree with the answer they explain (AC-5).
/// </remarks>
public sealed class OgEconomicsService : IOgEconomics
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgEconomicsService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgEconomicPending? Pending { get; private set; }

    public async Task<OgResult<OgEconomicsWorkspace>> EvaluateAsync(
        OgEconomicInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var regime = RegimeOf(inputs, out var regimeName);

        if (EconomicRules.Validate(inputs, regime) is { } refusal)
        {
            return OgResult<OgEconomicsWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var scenario = inputs.Scenario!.Trim().ToUpperInvariant();

            var source = _source();
            var scenarios = source is null
                ? new List<string>()
                : await ReadScenariosAsync(source).ConfigureAwait(false);

            // The scenario list is configuration, read rather than assumed (SG-09-3).
            if (scenarios.Count == 0)
            {
                return OgResult<OgEconomicsWorkspace>.Failed(
                    "No scenario is configured: R_ECONOMIC_SCENARIO has no active row, so an evaluation cannot say " +
                    "which scenario it is. Seed the reference table first.",
                    EconomicRules.CodeScenarioTableEmpty);
            }

            if (!scenarios.Contains(scenario, StringComparer.OrdinalIgnoreCase))
            {
                return OgResult<OgEconomicsWorkspace>.Failed(
                    $"'{inputs.Scenario}' is not a scenario in R_ECONOMIC_SCENARIO; the configured scenarios are " +
                    $"{string.Join(", ", scenarios)}.",
                    EconomicRules.CodeScenarioUnknown);
            }

            var result = Build(inputs, regime, scenarios.First(candidate =>
                string.Equals(candidate, scenario, StringComparison.OrdinalIgnoreCase)));

            var history = source is null
                ? []
                : await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgEconomicsWorkspace>.Ok(Workspace(history, result, scenarios, regime));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"evaluating economics for {inputs.EntityId}",
                "the evaluation is not shown",
                FailureSeverity.Error);

            return OgResult<OgEconomicsWorkspace>.Failed(
                "The economic evaluation could not be run.", reference ?? EconomicRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgEconomicsWorkspace>> SaveAsync(
        OgEconomicResult result,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgEconomicsWorkspace>.Failed(
                "A saved evaluation must record who saved it.", EconomicRules.CodeActorRequired);
        }

        if (!result.IrrDefined && result.Periods.Count == 0)
        {
            return OgResult<OgEconomicsWorkspace>.Failed(
                "There is no evaluation to save; run one first.", EconomicRules.CodeNothingToSave);
        }

        if (result.IsApproved)
        {
            return OgResult<OgEconomicsWorkspace>.Failed(
                "An approved evaluation is immutable: run a new evaluation and approve that instead.",
                EconomicRules.CodeApprovedImmutable);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgEconomicsWorkspace>.Failed(
                "No connection is open, so the evaluation cannot be saved.", EconomicRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var id = Guid.NewGuid().ToString("N");

            var unit = OgUow.For<ECONOMIC_ANALYSIS_RESULT>(source);
            unit.PrimaryKey = "ECONOMIC_ANALYSIS_RESULT_ID";

            unit.Add(new ECONOMIC_ANALYSIS_RESULT
            {
                ECONOMIC_ANALYSIS_RESULT_ID = id,
                ANALYSIS_ID = id,
                ANALYSIS_DATE = now,
                NPV = result.Npv,
                IRR = result.Irr,
                PAYBACK_PERIOD = result.PaybackMonths,
                DISCOUNT_RATE = result.DiscountRate,
                SCENARIO = result.Scenario,
                FORECAST_VERSION = result.ForecastVersion,
                ENTITY_ID = result.EntityId,
                CURRENCY = result.Currency,
                STATUS = EconomicStatus(result),
                VERSION = result.Version,
                FISCAL_REGIME = result.RegimeName,
                ROYALTY_RATE = result.RoyaltyRate,
                TAX_RATE = result.TaxRate,
                PERIOD_COUNT = result.Inputs.PeriodCount,
                START_YEAR = result.Inputs.StartYear,
                ANNUAL_VOLUME = result.Inputs.AnnualVolume,
                VOLUME_OUOM = result.Inputs.VolumeUnit,
                UNIT_PRICE = result.Inputs.UnitPrice,
                PRICE_OUOM = result.Inputs.PriceUnit,
                OPEX_PER_UNIT = result.Inputs.OpexPerUnit,
                ANNUAL_CAPEX = result.Inputs.AnnualCapex,
                ESCALATION_RATE = result.Inputs.EscalationRate,
                ABANDONMENT_COST = result.Inputs.AbandonmentCost,
                IRR_DEFINED_IND = result.IrrDefined ? "Y" : "N",
                PEAK_FUNDING = result.PeakFunding,
                UNDISCOUNTED_TOTAL = result.UndiscountedTotal,
                DRIVERS_TEXT = result.Drivers.Count == 0
                    ? null
                    : string.Join("\n", result.Drivers.Select(driver => driver.Describe())),
                FISCAL_REGIME_TEXT = result.RegimeText,
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
                Pending = new OgEconomicPending(result, actor.Trim());

                return OgResult<OgEconomicsWorkspace>.Failed(
                    OgUow.Message(committed, "The evaluation was not saved; the original is retained and can be retried."),
                    EconomicRules.CodeWriteFailed);
            }

            // The periods are the row's own evidence; a failure here removes the half-written result rather than leaving
            // an evaluation whose cash flow is missing.
            if (!await WritePeriodsAsync(source, id, result, cancellationToken).ConfigureAwait(false))
            {
                await RemoveResultAsync(source, id, cancellationToken).ConfigureAwait(false);
                Pending = new OgEconomicPending(result, actor.Trim());

                return OgResult<OgEconomicsWorkspace>.Failed(
                    "The evaluation's cash flow was not saved, so the result was withdrawn; the original is retained " +
                    "and can be retried.",
                    EconomicRules.CodeWriteFailed);
            }

            Pending = null;

            var scenarios = await ReadScenariosAsync(source).ConfigureAwait(false);
            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);
            var saved = result with
            {
                Id = id,
                Status = EconomicStatus(result),
                Actor = actor.Trim(),
                RecordedAt = now,
            };

            return OgResult<OgEconomicsWorkspace>.Ok(Workspace(history, saved, scenarios, RegimeOf(result)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = new OgEconomicPending(result, actor.Trim());

            var reference = _failures.ReportHandled(
                ex,
                $"saving an economic evaluation for {result.EntityId}",
                "the evaluation was not acknowledged; the original is retained and the save can be retried",
                FailureSeverity.Error);

            return OgResult<OgEconomicsWorkspace>.Failed(
                "The evaluation was not acknowledged; the original is retained and the save can be retried.",
                reference ?? EconomicRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgEconomicsWorkspace>> ApproveAsync(
        string resultId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgEconomicsWorkspace>.Failed(
                "An approval must record who made it.", EconomicRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgEconomicsWorkspace>.Failed(
                "No connection is open, so the evaluation cannot be approved.", EconomicRules.CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<ECONOMIC_ANALYSIS_RESULT>(source);
            unit.PrimaryKey = "ECONOMIC_ANALYSIS_RESULT_ID";

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal("ECONOMIC_ANALYSIS_RESULT_ID", resultId?.Trim() ?? string.Empty)],
                cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgEconomicsWorkspace>.Failed(
                    $"Evaluation '{resultId}' is not in this database.", EconomicRules.CodeNothingToSave);
            }

            if (string.Equals(row.STATUS, OgEconomicResult.StatusApproved, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgEconomicsWorkspace>.Failed(
                    "That evaluation is already approved, and an approved evaluation is immutable.",
                    EconomicRules.CodeAlreadyApproved);
            }

            row.STATUS = OgEconomicResult.StatusApproved;
            row.APPROVED_BY = actor.Trim();
            row.APPROVED_AT = DateTime.UtcNow;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgEconomicsWorkspace>.Failed(
                    OgUow.Message(updated, "The evaluation was not approved."), EconomicRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgEconomicsWorkspace>.Failed(
                    OgUow.Message(committed, "The evaluation was not approved."), EconomicRules.CodeWriteFailed);
            }

            var scenarios = await ReadScenariosAsync(source).ConfigureAwait(false);
            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgEconomicsWorkspace>.Ok(Workspace(
                history,
                history.FirstOrDefault(result => result.Id == row.ECONOMIC_ANALYSIS_RESULT_ID),
                scenarios,
                RegimeOf(history)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"approving economic evaluation {resultId}",
                "the evaluation was not approved",
                FailureSeverity.Error);

            return OgResult<OgEconomicsWorkspace>.Failed(
                "The evaluation could not be approved.", reference ?? EconomicRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgEconomicsWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgEconomicsWorkspace>.Failed(
                "There is no evaluation waiting to be retried.", EconomicRules.CodeNothingPending);
        }

        var retried = await SaveAsync(pending.Result, actor, cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    public async Task<OgResult<OgEconomicsWorkspace>> LoadAsync(
        string? entityId = null,
        string? resultId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgEconomicsWorkspace>.Failed(
                "No connection is open, so the saved evaluations cannot be read.", EconomicRules.CodeNoConnection);
        }

        try
        {
            var scenarios = await ReadScenariosAsync(source).ConfigureAwait(false);
            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            var selected = string.IsNullOrWhiteSpace(resultId)
                ? history.FirstOrDefault(result => string.IsNullOrWhiteSpace(entityId)
                    || string.Equals(result.EntityId, entityId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? history.FirstOrDefault()
                : history.FirstOrDefault(result =>
                      string.Equals(result.Id, resultId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? history.FirstOrDefault();

            return OgResult<OgEconomicsWorkspace>.Ok(Workspace(history, selected, scenarios, RegimeOf(history)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the saved economic evaluations",
                "the saved evaluations are unavailable; new evaluations are still allowed",
                FailureSeverity.Degraded);

            return OgResult<OgEconomicsWorkspace>.Ok(Workspace(
                [], null, [], new FiscalRegime(null, null, null), true,
                $"the saved evaluations could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the evaluation

    /// <summary>
    /// The periods and their money, in decimal, from the stated inputs and the regime (AC-1, AC-3).
    /// </summary>
    /// <remarks>
    /// Price and opex escalate together by <c>(1 + escalation)^t</c>, the capex lands in the first period and the
    /// abandonment in the last. Tax is charged on a positive pre-tax flow only; there is no loss carry-forward, which the
    /// regime's own description states.
    /// </remarks>
    public static IReadOnlyList<OgEconomicPeriod> BuildPeriods(OgEconomicInputs inputs, FiscalRegime regime)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(regime);

        var count = inputs.PeriodCount!.Value;
        var startYear = inputs.StartYear!.Value;
        var rate = inputs.DiscountRate!.Value;
        var escalation = inputs.EscalationRate ?? 0m;
        var volume = inputs.AnnualVolume!.Value;
        var price = inputs.UnitPrice!.Value;
        var opexPerUnit = inputs.OpexPerUnit!.Value;
        var capex = inputs.AnnualCapex!.Value;
        var abandonment = inputs.AbandonmentCost!.Value;
        var royaltyRate = regime.RoyaltyRate!.Value;
        var taxRate = regime.TaxRate!.Value;

        var periods = new List<OgEconomicPeriod>(count + 1);
        var cumulative = 0m;

        // Period 0 is the capital period: the capex is spent before any production, so it is the only flow at t = 0 and
        // the first revenue year is discounted by one period. That is the convention the closed-form annuity test uses.
        var opening = -capex;
        cumulative += opening;

        periods.Add(new OgEconomicPeriod(
            0,
            startYear,
            0m,
            price,
            0m,
            0m,
            0m,
            capex,
            0m,
            opening,
            0m,
            opening,
            opening,
            cumulative));

        var factor = 1m;

        for (var year = 1; year <= count; year++)
        {
            var escalatedPrice = price * factor;
            var escalatedOpex = opexPerUnit * factor;

            var revenue = volume * escalatedPrice;
            var royalty = revenue * royaltyRate;
            var periodOpex = volume * escalatedOpex;
            var periodAbandonment = year == count ? abandonment : 0m;
            var preTax = revenue - royalty - periodOpex - periodAbandonment;
            var tax = preTax > 0m ? preTax * taxRate : 0m;
            var net = preTax - tax;

            cumulative += net;

            periods.Add(new OgEconomicPeriod(
                year,
                startYear + year - 1,
                volume,
                escalatedPrice,
                revenue,
                royalty,
                periodOpex,
                0m,
                periodAbandonment,
                preTax,
                tax,
                net,
                net * EconomicMoneyCalculator.DiscountFactor(rate, year),
                cumulative));

            factor *= 1m + escalation;
        }

        return periods;
    }

    /// <summary>Builds the evaluation, its answer and its sensitivity (AC-1, AC-2, AC-5).</summary>
    public static OgEconomicResult Build(
        OgEconomicInputs inputs,
        FiscalRegime regime,
        string scenario)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(regime);

        var periods = BuildPeriods(inputs, regime);
        var money = periods
            .Select(period => new EconomicMoneyCalculator.MoneyPeriod(period.Period, period.Net))
            .ToList();

        var rate = inputs.DiscountRate!.Value;
        var npv = EconomicMoneyCalculator.Npv(money, rate);
        var irr = EconomicMoneyCalculator.TryIrr(money, out var found) ? found : (decimal?)null;
        var payback = EconomicMoneyCalculator.Payback(money);
        var drivers = Sensitivity(inputs, regime, rate, npv);

        return new OgEconomicResult(
            string.Empty,
            inputs.EntityId!.Trim(),
            scenario,
            inputs.ForecastVersion!.Trim(),
            inputs.Currency!.Trim().ToUpperInvariant(),
            rate,
            string.IsNullOrWhiteSpace(inputs.RegimeName) ? "(unnamed regime)" : inputs.RegimeName!.Trim(),
            regime.RoyaltyRate!.Value,
            regime.TaxRate!.Value,
            npv,
            irr,
            payback is { } months ? months * 12m : null,
            EconomicMoneyCalculator.PeakFunding(money),
            EconomicMoneyCalculator.Total(money),
            periods,
            drivers,
            OgEconomicResult.StatusDraft,
            1,
            null,
            null,
            null,
            null)
        {
            Inputs = inputs,
            RegimeText = regime.Describe(),
        };
    }

    /// <summary>
    /// AC-5: one-way sensitivity that re-uses <see cref="BuildPeriods"/> and the same NPV as the base run.
    /// </summary>
    private static IReadOnlyList<OgEconomicDriver> Sensitivity(
        OgEconomicInputs inputs,
        FiscalRegime regime,
        decimal rate,
        decimal baseNpv)
    {
        var drivers = new List<OgEconomicDriver>();

        void Add(
            string name,
            decimal baseValue,
            decimal lowValue,
            decimal highValue,
            OgEconomicInputs lowInputs,
            OgEconomicInputs highInputs)
        {
            var lowNpv = NpvOf(lowInputs, regime);
            var highNpv = NpvOf(highInputs, regime);

            drivers.Add(new OgEconomicDriver(name, lowValue, baseValue, highValue, lowNpv, baseNpv, highNpv));
        }

        var swing = EconomicRules.SensitivityPercent;

        Add("unit price",
            inputs.UnitPrice!.Value,
            inputs.UnitPrice.Value * (1m - swing),
            inputs.UnitPrice.Value * (1m + swing),
            inputs with { UnitPrice = inputs.UnitPrice.Value * (1m - swing) },
            inputs with { UnitPrice = inputs.UnitPrice.Value * (1m + swing) });

        Add("opex per unit",
            inputs.OpexPerUnit!.Value,
            inputs.OpexPerUnit.Value * (1m - swing),
            inputs.OpexPerUnit.Value * (1m + swing),
            inputs with { OpexPerUnit = inputs.OpexPerUnit.Value * (1m - swing) },
            inputs with { OpexPerUnit = inputs.OpexPerUnit.Value * (1m + swing) });

        Add("annual capex",
            inputs.AnnualCapex!.Value,
            inputs.AnnualCapex.Value * (1m - swing),
            inputs.AnnualCapex.Value * (1m + swing),
            inputs with { AnnualCapex = inputs.AnnualCapex.Value * (1m - swing) },
            inputs with { AnnualCapex = inputs.AnnualCapex.Value * (1m + swing) });

        Add("annual volume",
            inputs.AnnualVolume!.Value,
            inputs.AnnualVolume.Value * (1m - swing),
            inputs.AnnualVolume.Value * (1m + swing),
            inputs with { AnnualVolume = inputs.AnnualVolume.Value * (1m - swing) },
            inputs with { AnnualVolume = inputs.AnnualVolume.Value * (1m + swing) });

        // The discount rate is swung by the same percentage of itself, so 10 % moves to 9 % and 11 %.
        Add("discount rate",
            rate,
            rate * (1m - swing),
            rate * (1m + swing),
            inputs with { DiscountRate = rate * (1m - swing) },
            inputs with { DiscountRate = rate * (1m + swing) });

        return drivers
            .OrderByDescending(driver => driver.SwingMagnitude)
            .ThenBy(driver => driver.Name, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// The NPV of one sensitivity case. It discounts at the case's <b>own</b> rate, so swinging the discount rate actually
    /// moves the answer instead of re-applying the base rate three times.
    /// </summary>
    private static decimal NpvOf(OgEconomicInputs inputs, FiscalRegime regime)
    {
        var periods = BuildPeriods(inputs, regime);

        return EconomicMoneyCalculator.Npv(
            periods.Select(period => new EconomicMoneyCalculator.MoneyPeriod(period.Period, period.Net)).ToList(),
            inputs.DiscountRate!.Value);
    }

    /// <summary>The regime an evaluation was run under, rebuilt from its own saved row.</summary>
    private static FiscalRegime RegimeOf(OgEconomicInputs inputs) =>
        new(inputs.RegimeName, inputs.RoyaltyRate, inputs.TaxRate);

    private static FiscalRegime RegimeOf(IReadOnlyList<OgEconomicResult> results) =>
        results.Count == 0
            ? new FiscalRegime(null, null, null)
            : new FiscalRegime(results[0].RegimeName, results[0].RoyaltyRate, results[0].TaxRate);

    private static FiscalRegime RegimeOf(OgEconomicResult result) =>
        new(result.RegimeName, result.RoyaltyRate, result.TaxRate);

    /// <summary>Gets the regime out of the inputs, reporting which name it will be stored under.</summary>
    private static FiscalRegime RegimeOf(OgEconomicInputs inputs, out string name)
    {
        name = inputs.RegimeName?.Trim() ?? string.Empty;

        return new FiscalRegime(inputs.RegimeName, inputs.RoyaltyRate, inputs.TaxRate);
    }

    private static string EconomicStatus(OgEconomicResult result) => result.IsApproved
        ? OgEconomicResult.StatusApproved
        : OgEconomicResult.StatusDraft;

    // ------------------------------------------------------------------ the reads and writes

    private static async Task<bool> WritePeriodsAsync(
        IDataSource source,
        string resultId,
        OgEconomicResult result,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<ECONOMIC_CASH_FLOW>(source);
        unit.PrimaryKey = "ECONOMIC_CASH_FLOW_ID";

        foreach (var period in result.Periods)
        {
            unit.Add(new ECONOMIC_CASH_FLOW
            {
                ECONOMIC_CASH_FLOW_ID = Guid.NewGuid().ToString("N"),
                ECONOMIC_ANALYSIS_ID = resultId,
                PERIOD = period.Period,
                AMOUNT = period.Net,
                DESCRIPTION = $"{period.Year}|discounted={period.Discounted.ToString(CultureInfo.InvariantCulture)}|" +
                              $"cumulative={period.Cumulative.ToString(CultureInfo.InvariantCulture)}",
            });
        }

        cancellationToken.ThrowIfCancellationRequested();

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed);
    }

    private static async Task RemoveResultAsync(
        IDataSource source,
        string resultId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<ECONOMIC_ANALYSIS_RESULT>(source);
        unit.PrimaryKey = "ECONOMIC_ANALYSIS_RESULT_ID";

        var rows = await OgUow.ReadAsync(
            unit,
            [OgUow.Equal("ECONOMIC_ANALYSIS_RESULT_ID", resultId)],
            cancellationToken).ConfigureAwait(false);

        if (rows.FirstOrDefault() is { } row)
        {
            unit.Delete(row);
            await unit.Commit().ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<string>> ReadScenariosAsync(IDataSource source)
    {
        var rows = await OgUow.ReadAllAsync<R_ECONOMIC_SCENARIO>(source).ConfigureAwait(false);

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.ECONOMIC_SCENARIO))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Select(row => row.ECONOMIC_SCENARIO!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static async Task<IReadOnlyList<OgEconomicResult>> ReadHistoryAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAllAsync<ECONOMIC_ANALYSIS_RESULT>(source).ConfigureAwait(false);
        var flows = await OgUow.ReadAllAsync<ECONOMIC_CASH_FLOW>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var byResult = flows
            .Where(flow => !string.IsNullOrWhiteSpace(flow.ECONOMIC_ANALYSIS_ID))
            .GroupBy(flow => flow.ECONOMIC_ANALYSIS_ID!, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(flow => flow.PERIOD).ToList(),
                StringComparer.Ordinal);

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.ECONOMIC_ANALYSIS_RESULT_ID))
            .Where(row => !string.IsNullOrWhiteSpace(row.SCENARIO))
            .OrderByDescending(row => row.RECORDED_AT ?? row.ANALYSIS_DATE ?? DateTime.MinValue)
            .ThenBy(row => row.ECONOMIC_ANALYSIS_RESULT_ID, StringComparer.Ordinal)
            .Select(row => ToResult(
                row,
                byResult.TryGetValue(row.ECONOMIC_ANALYSIS_RESULT_ID, out var periods) ? periods : []))
            .Take(50)
            .ToList();
    }

    private static OgEconomicResult ToResult(
        ECONOMIC_ANALYSIS_RESULT row,
        IReadOnlyList<ECONOMIC_CASH_FLOW> flows)
    {
        var periods = flows
            .Select(flow =>
            {
                var parts = (flow.DESCRIPTION ?? string.Empty).Split('|', StringSplitOptions.TrimEntries);
                var year = parts.Length > 0 && int.TryParse(parts[0], NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var parsed)
                    ? parsed
                    : (row.START_YEAR ?? 0) + (flow.PERIOD ?? 0);

                var net = flow.AMOUNT ?? 0m;

                return new OgEconomicPeriod(
                    flow.PERIOD ?? 0,
                    year,
                    row.ANNUAL_VOLUME ?? 0m,
                    row.UNIT_PRICE ?? 0m,
                    0m,
                    0m,
                    0m,
                    0m,
                    0m,
                    0m,
                    0m,
                    net,
                    0m,
                    0m);
            })
            .OrderBy(period => period.Period)
            .ToList();

        return new OgEconomicResult(
            row.ECONOMIC_ANALYSIS_RESULT_ID ?? string.Empty,
            row.ENTITY_ID,
            row.SCENARIO ?? string.Empty,
            row.FORECAST_VERSION ?? string.Empty,
            row.CURRENCY ?? string.Empty,
            row.DISCOUNT_RATE ?? 0m,
            row.FISCAL_REGIME ?? string.Empty,
            row.ROYALTY_RATE ?? 0m,
            row.TAX_RATE ?? 0m,
            row.NPV ?? 0m,
            string.Equals(row.IRR_DEFINED_IND, "Y", StringComparison.OrdinalIgnoreCase) ? row.IRR : null,
            row.PAYBACK_PERIOD,
            row.PEAK_FUNDING ?? 0m,
            row.UNDISCOUNTED_TOTAL ?? 0m,
            periods,
            [],
            string.Equals(row.STATUS, OgEconomicResult.StatusApproved, StringComparison.OrdinalIgnoreCase)
                ? OgEconomicResult.StatusApproved
                : OgEconomicResult.StatusDraft,
            row.VERSION,
            row.ACTOR,
            row.RECORDED_AT ?? row.ANALYSIS_DATE,
            row.APPROVED_BY,
            row.APPROVED_AT)
        {
            Inputs = new OgEconomicInputs(
                row.ENTITY_ID,
                row.SCENARIO,
                row.FORECAST_VERSION,
                row.CURRENCY,
                row.DISCOUNT_RATE,
                row.FISCAL_REGIME,
                row.ROYALTY_RATE,
                row.TAX_RATE,
                row.PERIOD_COUNT,
                row.START_YEAR,
                row.ANNUAL_VOLUME,
                row.VOLUME_OUOM,
                row.UNIT_PRICE,
                row.PRICE_OUOM,
                row.OPEX_PER_UNIT,
                row.ANNUAL_CAPEX,
                row.ESCALATION_RATE,
                row.ABANDONMENT_COST),
            RegimeText = row.FISCAL_REGIME_TEXT ?? string.Empty,
            StoredDriversText = row.DRIVERS_TEXT,
        };
    }

    private static OgEconomicsWorkspace Workspace(
        IReadOnlyList<OgEconomicResult> history,
        OgEconomicResult? selected,
        IReadOnlyList<string> scenarios,
        FiscalRegime regime,
        bool historyFailed = false,
        string? historyReason = null) =>
        new(
            history,
            selected,
            scenarios,
            EconomicRules.Currencies,
            EconomicRules.VolumeUnits,
            regime,
            historyFailed,
            historyReason);
}
