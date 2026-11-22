using System.Globalization;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Calculations;
using Beep.OilandGas.ProductionAccounting.Constants;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;

/// <summary>Where a tax computation posts (ACC-08): the company and the book.</summary>
public sealed record OgTaxContext(string CompanyId, string BookCode)
{
    public static OgTaxContext FromSession(OgSession session) =>
        new(
            string.IsNullOrWhiteSpace(session.CompanyId) ? "APP" : session.CompanyId,
            string.IsNullOrWhiteSpace(session.BookCode) ? "MAIN" : session.BookCode);
}

/// <summary>
/// One jurisdiction's configured rates (ACC-08) - read from <c>GOVERNMENTAL_TAX_DATA</c>, never entered on the screen (AC-5).
/// </summary>
public sealed record OgTaxJurisdiction(
    string Id,
    string Jurisdiction,
    decimal? SeveranceRate,
    decimal? AdValoremRate,
    decimal? IncomeTaxRate,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo,
    string? Remark)
{
    public bool HasProductionRates => SeveranceRate is not null || AdValoremRate is not null;

    public bool HasIncomeRate => IncomeTaxRate is not null;

    public string RatesText =>
        "severance " + Percent(SeveranceRate) + " · ad valorem " + Percent(AdValoremRate) +
        " · income " + Percent(IncomeTaxRate);

    public string EffectiveText =>
        "effective from " + (EffectiveFrom?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "an unstated date") +
        (EffectiveTo is { } to ? " to " + to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty);

    public string Describe() => Jurisdiction + " · " + RatesText + " · " + EffectiveText;

    private static string Percent(decimal? value) =>
        value is { } rate
            ? rate.ToString("#,0.####", CultureInfo.InvariantCulture) + " %"
            : "not configured";
}

/// <summary>The inputs of a tax computation (ACC-08).</summary>
public sealed record OgTaxInputs(
    string? Jurisdiction,
    string? Kind,
    DateTime? PeriodFrom,
    DateTime? PeriodTo,
    string? Currency,
    decimal? GrossRevenue,
    decimal? TaxableIncome,
    decimal? DeferredTaxAsset,
    decimal? DeferredTaxLiability,
    string? PropertyId);

/// <summary>One tax computation (ACC-08), matching the plan's <c>OgTaxComputation</c> plus its posting.</summary>
public sealed record OgTaxComputation(
    string Id,
    string Jurisdiction,
    string Kind,
    DateTime Date,
    decimal Basis,
    string BasisText,
    decimal Rate,
    decimal Amount,
    string Currency,
    DateTime? EffectiveFrom,
    string? RateSource,
    DateTime? PeriodFrom,
    DateTime? PeriodTo,
    string FiscalYear,
    string Status,
    string? PropertyId,
    string? Author,
    string? JournalId,
    string? PostedBy,
    DateTime? PostedAt,
    string? Description)
{
    /// <summary>AC-1: a computation names its jurisdiction, its rate and the rate's effective date.</summary>
    public bool NamesItsBasis =>
        !string.IsNullOrWhiteSpace(Jurisdiction) && Rate != 0m && EffectiveFrom is not null &&
        !string.IsNullOrWhiteSpace(RateSource);

    public bool IsPosted => string.Equals(Status, TaxComputationCodes.StatusPosted, StringComparison.OrdinalIgnoreCase);

    public decimal RatePercent => Rate * 100m;

    public string AmountText => MoneyText.Of(Amount) + " " + Currency;

    public string RateText =>
        RatePercent.ToString("0.####", CultureInfo.InvariantCulture) + " % from " + (RateSource ?? "(unknown source)") +
        (EffectiveFrom is { } from ? " effective " + from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty);

    public string BasisDescription => BasisText + " " + MoneyText.Of(Basis) + " " + Currency;

    public string PeriodText =>
        (PeriodFrom?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "?") + " to " +
        (PeriodTo?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "?");

    public string JournalText => string.IsNullOrWhiteSpace(JournalId)
        ? "not posted - no journal yet"
        : "journal " + JournalId + " posted by " + (PostedBy ?? "(unknown)") + " on " +
          (PostedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date");

    public string Describe() =>
        TaxComputationCodes.Describe(Kind) + " · " + Jurisdiction + " · " + AmountText + " · " +
        Status.ToLowerInvariant();
}

/// <summary>One deferred-tax balance carried at a period end (ACC-08).</summary>
public sealed record OgDeferredBalance(
    string Id,
    DateTime PeriodEnd,
    decimal Asset,
    decimal Liability,
    string? Notes)
{
    public decimal Net => Liability - Asset;

    public string Describe() =>
        "period end " + PeriodEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " · asset " +
        MoneyText.Of(Asset) + " · liability " + MoneyText.Of(Liability) + " · net " + MoneyText.Of(Net);
}

/// <summary>The tax workspace (ACC-08).</summary>
public sealed record OgTaxWorkspace(
    IReadOnlyList<OgTaxJurisdiction> Jurisdictions,
    OgTaxJurisdiction? SelectedJurisdiction,
    IReadOnlyList<OgTaxComputation> Computations,
    OgTaxComputation? SelectedComputation,
    IReadOnlyList<OgDeferredBalance> Deferred,
    IReadOnlyList<string> Kinds,
    IReadOnlyList<string> Statuses,
    IReadOnlyList<string> Currencies,
    string RateTableText,
    bool FiscalYearComplete,
    string FiscalYearText,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string JurisdictionsText => Jurisdictions.Count == 0
        ? "No jurisdiction has a configured rate yet."
        : string.Join(Environment.NewLine, Jurisdictions.Select(jurisdiction => jurisdiction.Describe()));

    public string ComputationsText => Computations.Count == 0
        ? "No tax is computed yet."
        : string.Join(Environment.NewLine, Computations.Select(computation => computation.Describe()));

    public string DeferredText => Deferred.Count == 0
        ? "No deferred-tax balance is carried."
        : string.Join(Environment.NewLine, Deferred.Select(balance => balance.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the tax computations could not be read; nothing can be computed or posted"
        : $"{Computations.Count.ToString(CultureInfo.InvariantCulture)} computation(s) · " +
          $"{Computations.Count(computation => !computation.IsPosted).ToString(CultureInfo.InvariantCulture)} " +
          $"unposted · {Jurisdictions.Count.ToString(CultureInfo.InvariantCulture)} jurisdiction(s)";

    public decimal Unposted => Computations.Where(computation => !computation.IsPosted).Sum(c => c.Amount);

    public string UnpostedText => "unposted tax " + MoneyText.Of(Unposted);

    public string PostingPreviewText => SelectedComputation is not { } computation
        ? "no computation is selected"
        : TaxComputationCodes.IsProvision(computation.Kind)
            ? computation.Kind == TaxComputationCodes.ProvisionDeferred
                ? "deferred movement " + computation.AmountText + ": deferred tax asset or liability against " +
                  "income tax expense " + TaxPostingCodes.IncomeTaxExpenseAccount
                : "debit income tax expense " + TaxPostingCodes.IncomeTaxExpenseAccount + " · credit income tax " +
                  "payable " + TaxPostingCodes.IncomeTaxPayableAccount + " · " + computation.AmountText
            : "debit production tax expense " + TaxPostingCodes.ProductionTaxExpenseAccount + " · credit tax payable " +
              TaxPostingCodes.ProductionTaxPayableAccount + " · " + computation.AmountText;

    public string ReferenceText =>
        $"kinds {string.Join(", ", Kinds)} · rates come from GOVERNMENTAL_TAX_DATA and are never entered here · " +
        "a provision needs every month of its fiscal year closed";
}

/// <summary>The tax rules, stated once so the page, the service and the tests agree.</summary>
public static class TaxRules
{
    public const string CodeNoConnection = "tax-no-connection";
    public const string CodeReadFailed = "tax-read-failed";
    public const string CodeWriteFailed = "tax-write-failed";
    public const string CodeActorRequired = "tax-actor-required";
    public const string CodeInputRequired = "tax-input-required";
    public const string CodeInputInvalid = "tax-input-invalid";
    public const string CodeCurrencyMismatch = "tax-currency-mismatch";
    public const string CodeUnknownComputation = "tax-unknown-computation";
    public const string CodeUnknownJurisdiction = "tax-unknown-jurisdiction";
    public const string CodeNoRate = "tax-no-rate";
    public const string CodeRateNotEffective = "tax-rate-not-effective";
    public const string CodeRateAmbiguous = "tax-rate-ambiguous";
    public const string CodeFiscalYearIncomplete = "tax-fiscal-year-incomplete";
    public const string CodeAlreadyPosted = "tax-already-posted";
    public const string CodeAuthorCannotPost = "tax-author-cannot-post";
    public const string CodePeriodClosed = "tax-period-closed";
    public const string CodeUnknownAccount = "tax-unknown-account";
    public const string CodeNothingToPost = "tax-nothing-to-post";

    public static IReadOnlyList<string> Currencies => TaxPostingCodes.Currencies;

    public static IReadOnlyList<string> Kinds => TaxComputationCodes.Kinds;

    public static IReadOnlyList<string> Statuses => TaxComputationCodes.Statuses;

    public static IReadOnlyList<string> Missing(OgTaxInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(inputs.Jurisdiction))
        {
            missing.Add("jurisdiction");
        }

        if (string.IsNullOrWhiteSpace(inputs.Kind))
        {
            missing.Add("the tax kind");
        }

        if (inputs.PeriodFrom is null || inputs.PeriodTo is null)
        {
            missing.Add("the period");
        }

        if (string.IsNullOrWhiteSpace(inputs.Currency))
        {
            missing.Add("currency");
        }

        if (TaxComputationCodes.IsProvision(inputs.Kind))
        {
            if (inputs.TaxableIncome is null)
            {
                missing.Add("taxable income");
            }

            if (string.Equals(inputs.Kind?.Trim(), TaxComputationCodes.ProvisionDeferred,
                    StringComparison.OrdinalIgnoreCase))
            {
                if (inputs.DeferredTaxAsset is null)
                {
                    missing.Add("the closing deferred tax asset");
                }

                if (inputs.DeferredTaxLiability is null)
                {
                    missing.Add("the closing deferred tax liability");
                }
            }
        }
        else if (inputs.GrossRevenue is null)
        {
            missing.Add("gross revenue");
        }

        return missing;
    }

    public static (string Code, string Message)? Validate(OgTaxInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            return (CodeInputRequired, $"The computation needs {string.Join(", ", missing)}.");
        }

        if (!Currencies.Contains(inputs.Currency!.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (CodeCurrencyMismatch,
                $"'{inputs.Currency}' is not a currency this app computes tax in; use {string.Join(", ", Currencies)}.");
        }

        if (!TaxComputationCodes.Kinds.Contains(inputs.Kind!.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (CodeInputInvalid,
                $"'{inputs.Kind}' is not a tax kind; use {string.Join(", ", TaxComputationCodes.Kinds)}.");
        }

        if (inputs.PeriodTo < inputs.PeriodFrom)
        {
            return (CodeInputInvalid, "The period ends before it starts.");
        }

        if (inputs.GrossRevenue is < 0m || inputs.TaxableIncome is < 0m
            || inputs.DeferredTaxAsset is < 0m || inputs.DeferredTaxLiability is < 0m)
        {
            return (CodeInputInvalid, "A tax basis and a deferred balance cannot be negative.");
        }

        return null;
    }

    /// <summary>
    /// AC-2 and the rate-effective-date state: a rate recorded for the jurisdiction whose effective date is after the period
    /// is refused naming that date, rather than silently taxed at nothing.
    /// </summary>
    public static (string Code, string Message)? CheckRate(
        string jurisdiction,
        decimal? recordedRate,
        DateTime? effectiveFrom,
        DateTime periodEnd,
        string kind)
    {
        if (recordedRate is null || TaxMath.NormalizeRate(recordedRate) is null)
        {
            return (CodeNoRate,
                $"No {TaxComputationCodes.Describe(kind)} rate is recorded for {jurisdiction}, so the computation is " +
                "blocked. Rates are configuration: record the rate for the jurisdiction before computing.");
        }

        if (effectiveFrom is { } from && from.Date > periodEnd.Date)
        {
            return (CodeRateNotEffective,
                $"The rate recorded for {jurisdiction} takes effect on " +
                $"{from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}, after the period ends on " +
                $"{periodEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}, so the computation is refused.");
        }

        return null;
    }
}

/// <summary>ACC-08: the tax workbench - production tax, current and deferred provision, and their postings.</summary>
public interface IOgTax
{
    /// <summary>Computes tax from the configured rates. Nothing is written until it is posted.</summary>
    Task<OgResult<OgTaxWorkspace>> ComputeAsync(
        OgTaxInputs inputs,
        OgTaxContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Posts a computed tax: exactly one journal per computation (AC-4), by someone other than its author.</summary>
    Task<OgResult<OgTaxWorkspace>> PostAsync(
        string computationId,
        OgTaxContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The jurisdictions, computations and deferred balances; an unavailable register is a state.</summary>
    Task<OgResult<OgTaxWorkspace>> LoadAsync(
        OgTaxContext context,
        string? computationId = null,
        string? jurisdiction = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ACC-08 over ProductionAccounting's <c>GOVERNMENTAL_TAX_DATA</c> (the rates), <c>TAX_TRANSACTION</c> (the computations) and
/// <c>DEFERRED_TAX_BALANCE</c>, with the postings going to the ledger at the configured accounts, through units of work.
/// </summary>
/// <remarks>
/// The rates are never entered on the screen (AC-5): they are read from the owner's rate table, whose jurisdiction and extra
/// rates live in its <c>REMARK</c> column as <c>key=value</c> tokens - the contract <c>ProductionTaxService</c> already uses.
/// The rate is normalised the owner's way (above 1 is a percentage), so a recorded 12.5 and a recorded 0.125 mean the same
/// thing and a rate of zero means none. A provision additionally requires every month of its fiscal year to be a closed
/// ledger period, which is SG-10-8's gate.
/// </remarks>
public sealed class OgTaxService : IOgTax
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgTaxService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgTaxWorkspace>> ComputeAsync(
        OgTaxInputs inputs,
        OgTaxContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgTaxWorkspace>.Failed(
                "A tax computation must record who made it.", TaxRules.CodeActorRequired);
        }

        if (TaxRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgTaxWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgTaxWorkspace>.Failed(
                "No connection is open, so the tax cannot be computed.", TaxRules.CodeNoConnection);
        }

        try
        {
            var jurisdiction = inputs.Jurisdiction!.Trim();
            var kind = inputs.Kind!.Trim().ToUpperInvariant();
            var periodEnd = inputs.PeriodTo!.Value.Date;
            var periodStart = inputs.PeriodFrom!.Value.Date;

            var rates = await OgUow.ReadAllAsync<GOVERNMENTAL_TAX_DATA>(source).ConfigureAwait(false);

            var matched = rates
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(row => string.Equals(
                    TaxRemark.Value(row.REMARK, ProductionTaxRemarkKeys.Jurisdiction), jurisdiction,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matched.Count == 0)
            {
                return OgResult<OgTaxWorkspace>.Failed(
                    $"No rate row names {jurisdiction} as its jurisdiction " +
                    $"({ProductionTaxRemarkKeys.Jurisdiction}=... in the rate table's remark), so the computation is " +
                    "blocked. Rates are configuration and are never entered on this screen.",
                    TaxRules.CodeUnknownJurisdiction);
            }

            // A provision takes its rate from the corporate key; a production tax from its own column.
            decimal? recordedRate;
            var rateKey = string.Empty;

            if (kind == TaxComputationCodes.ProvisionCurrent || kind == TaxComputationCodes.ProvisionDeferred)
            {
                rateKey = ProductionTaxRemarkKeys.DeferredTaxRate;

                recordedRate = matched
                    .Select(row => TaxRemark.Rate(row.REMARK, ProductionTaxRemarkKeys.DeferredTaxRate))
                    .FirstOrDefault(rate => rate is not null);
            }
            else if (kind == ProductionTaxAssessmentCodes.Severance)
            {
                rateKey = nameof(GOVERNMENTAL_TAX_DATA.SEVERANCE_TAX);
                recordedRate = matched.Select(row => row.SEVERANCE_TAX).FirstOrDefault(rate => rate is not null);
            }
            else
            {
                rateKey = nameof(GOVERNMENTAL_TAX_DATA.AD_VALOREM_TAX);
                recordedRate = matched.Select(row => row.AD_VALOREM_TAX).FirstOrDefault(rate => rate is not null);
            }

            // The row that supplies the rate supplies its effective date, so the two cannot come from different rows.
            var rateRow = kind == TaxComputationCodes.ProvisionCurrent || kind == TaxComputationCodes.ProvisionDeferred
                ? matched.FirstOrDefault(row => TaxRemark.Rate(row.REMARK, ProductionTaxRemarkKeys.DeferredTaxRate) is not null)
                : kind == ProductionTaxAssessmentCodes.Severance
                    ? matched.FirstOrDefault(row => TaxMath.NormalizeRate(row.SEVERANCE_TAX) is not null)
                    : matched.FirstOrDefault(row => TaxMath.NormalizeRate(row.AD_VALOREM_TAX) is not null);

            if (TaxRules.CheckRate(jurisdiction, recordedRate, rateRow?.EFFECTIVE_DATE, periodEnd, kind) is { } rateRefusal)
            {
                return OgResult<OgTaxWorkspace>.Failed(rateRefusal.Message, rateRefusal.Code);
            }

            var rate = TaxMath.NormalizeRate(recordedRate)!.Value;

            // SG-10-8 and AC-3: a provision needs the year behind it closed.
            if (TaxComputationCodes.IsProvision(kind))
            {
                var periods = await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);

                if (IncompleteFiscalYear(periodEnd, periods) is { } incomplete)
                {
                    return OgResult<OgTaxWorkspace>.Failed(
                        $"The fiscal year {periodEnd.Year.ToString(CultureInfo.InvariantCulture)} is incomplete: " +
                        $"{incomplete}. A provision is refused until every month of the year is closed.",
                        TaxRules.CodeFiscalYearIncomplete);
                }
            }

            var currency = inputs.Currency!.Trim().ToUpperInvariant();
            var basisText = "gross revenue";
            decimal basis;
            decimal amount;

            if (kind == TaxComputationCodes.ProvisionDeferred)
            {
                // The deferred computation's basis is the closing net balance; its amount is the movement against the
                // balance already carried, which is what the owner's RecordDeferredTaxBalance posts.
                var balances = await OgUow.ReadAllAsync<DEFERRED_TAX_BALANCE>(source).ConfigureAwait(false);

                // The prior balance is the latest one at or before this period end: a re-estimate for the same period
                // measures against the balance it replaces, which is the only way a second provision for a period
                // posts its movement rather than the whole balance again.
                var prior = balances
                    .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                    .Where(row => row.PERIOD_END_DATE is not null && row.PERIOD_END_DATE.Value.Date <= periodEnd)
                    .OrderByDescending(row => row.PERIOD_END_DATE)
                    .ThenByDescending(row => row.ROW_CREATED_DATE)
                    .FirstOrDefault();

                var movement = TaxMath.Movement(
                    inputs.DeferredTaxAsset!.Value,
                    inputs.DeferredTaxLiability!.Value,
                    prior?.DEFERRED_TAX_ASSET ?? 0m,
                    prior?.DEFERRED_TAX_LIABILITY ?? 0m);

                basisText = "closing deferred balance";
                basis = Math.Abs(movement.Liability - movement.Asset);
                amount = movement.Expense;
            }
            else if (TaxComputationCodes.IsProvision(kind))
            {
                basisText = "taxable income";
                basis = inputs.TaxableIncome!.Value;
                amount = TaxMath.Amount(basis, rate);
            }
            else
            {
                basis = inputs.GrossRevenue!.Value;
                amount = TaxMath.Amount(basis, rate);
            }

            if (amount == 0m)
            {
                return OgResult<OgTaxWorkspace>.Failed(
                    $"{TaxComputationCodes.Describe(kind)} on {MoneyText.Of(basis)} {currency} at " +
                    $"{TaxMath.Round(rate * 100m).ToString("0.####", CultureInfo.InvariantCulture)} % comes to nothing, " +
                    "so there is nothing to compute or post.",
                    TaxRules.CodeNothingToPost);
            }

            var now = DateTime.UtcNow;

            var unit = OgUow.For<TAX_TRANSACTION>(source);
            unit.PrimaryKey = "TAX_TRANSACTION_ID";

            var computationId = Guid.NewGuid().ToString("N");

            unit.Add(new TAX_TRANSACTION
            {
                TAX_TRANSACTION_ID = computationId,
                TAX_TYPE = kind,
                TAX_DATE = periodEnd,
                TAX_AMOUNT = amount,
                TAX_JURISDICTION = jurisdiction,
                PROPERTY_ID = string.IsNullOrWhiteSpace(inputs.PropertyId) ? null : inputs.PropertyId.Trim(),
                DESCRIPTION = TaxComputationCodes.Describe(kind) + " for " + jurisdiction,
                // AC-1: the rate, its effective date and where it came from travel with the amount.
                TAX_RATE = rate,
                TAX_BASIS = basis,
                BASIS_TEXT = basisText,
                RATE_EFFECTIVE_DATE = rateRow?.EFFECTIVE_DATE,
                RATE_SOURCE = "GOVERNMENTAL_TAX_DATA " +
                              (rateRow?.GOVERNMENTAL_TAX_DATA_ID ?? "(row)") + " · " + rateKey,
                PERIOD_FROM = periodStart,
                PERIOD_TO = periodEnd,
                FISCAL_YEAR = periodEnd.Year.ToString(CultureInfo.InvariantCulture),
                CURRENCY_CODE = currency,
                STATUS = TaxComputationCodes.StatusComputed,
                AUTHOR = actor.Trim(),
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgTaxWorkspace>.Failed(
                    OgUow.Message(committed, "The computation was not saved."), TaxRules.CodeWriteFailed);
            }

            var workspace = await ReadAsync(source, context, computationId, jurisdiction, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgTaxWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"computing {inputs.Kind} tax for {inputs.Jurisdiction}", "the computation was not made",
                FailureSeverity.Error);

            return OgResult<OgTaxWorkspace>.Failed(
                "The tax could not be computed.", reference ?? TaxRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgTaxWorkspace>> PostAsync(
        string computationId,
        OgTaxContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgTaxWorkspace>.Failed(
                "A posting must record who made it.", TaxRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgTaxWorkspace>.Failed(
                "No connection is open, so the tax cannot be posted.", TaxRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, computationId, null, cancellationToken)
                .ConfigureAwait(false);
            var computation = workspace.SelectedComputation;

            if (computation is null)
            {
                return OgResult<OgTaxWorkspace>.Failed(
                    $"Computation '{computationId}' is not in this database.", TaxRules.CodeUnknownComputation);
            }

            // Posting is a transition of the computation that exists, so a second post cannot create a second entry.
            if (computation.IsPosted && !string.IsNullOrWhiteSpace(computation.JournalId))
            {
                return OgResult<OgTaxWorkspace>.Ok(workspace);
            }

            // AC-6: whoever computed it does not post it.
            if (!string.IsNullOrWhiteSpace(computation.Author)
                && string.Equals(computation.Author.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgTaxWorkspace>.Failed(
                    $"{computation.Describe()} was computed by {computation.Author}, who therefore cannot post it. " +
                    "A different controller must post it.",
                    TaxRules.CodeAuthorCannotPost);
            }

            var periodCode = LedgerCodes.PeriodCodeOf(computation.Date);
            var periods = await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);
            var period = periods.FirstOrDefault(row =>
                string.Equals(row.PERIOD_CODE, periodCode, StringComparison.OrdinalIgnoreCase));

            if (period is not null && !LedgerCodes.IsOpen(period.STATUS))
            {
                return OgResult<OgTaxWorkspace>.Failed(
                    $"Period {periodCode} is {period.STATUS.ToLowerInvariant()}, so the computation cannot be posted " +
                    "into it.",
                    TaxRules.CodePeriodClosed);
            }

            var accounts = await OgUow.ReadAllAsync<GL_ACCOUNT>(source).ConfigureAwait(false);

            // The posting the owner already makes for each kind, at the configured accounts.
            (string Debit, string Credit) accountsFor;
            var amount = Math.Abs(computation.Amount);

            if (computation.Kind == TaxComputationCodes.ProvisionDeferred)
            {
                // A deferred movement: a liability increases into expense, an asset increase comes out of it.
                accountsFor = computation.Amount >= 0m
                    ? (TaxPostingCodes.IncomeTaxExpenseAccount, TaxPostingCodes.DeferredTaxLiabilityAccount)
                    : (TaxPostingCodes.DeferredTaxAssetAccount, TaxPostingCodes.IncomeTaxExpenseAccount);
            }
            else if (TaxComputationCodes.IsProvision(computation.Kind))
            {
                accountsFor = (TaxPostingCodes.IncomeTaxExpenseAccount, TaxPostingCodes.IncomeTaxPayableAccount);
            }
            else
            {
                accountsFor = (TaxPostingCodes.ProductionTaxExpenseAccount, TaxPostingCodes.ProductionTaxPayableAccount);
            }

            var debit = Find(accounts, accountsFor.Debit);
            var credit = Find(accounts, accountsFor.Credit);

            if (debit is null || credit is null)
            {
                return OgResult<OgTaxWorkspace>.Failed(
                    $"The chart of accounts has no account " +
                    $"{(debit is null ? accountsFor.Debit : accountsFor.Credit)}, so the computation cannot be posted.",
                    TaxRules.CodeUnknownAccount);
            }

            var reference = "TAX-" + computation.Id;
            var journals = await OgUow.ReadAllAsync<JOURNAL_ENTRY>(source).ConfigureAwait(false);

            var existing = journals.FirstOrDefault(entry =>
                string.Equals(entry.SOURCE_MODULE, "TAX", StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.REFERENCE_NUMBER, reference, StringComparison.OrdinalIgnoreCase));

            var now = DateTime.UtcNow;
            string journalId;

            if (existing is not null)
            {
                journalId = existing.JOURNAL_ENTRY_ID!;
            }
            else
            {
                journalId = Guid.NewGuid().ToString("N");

                var journal = OgUow.For<JOURNAL_ENTRY>(source);
                journal.PrimaryKey = "JOURNAL_ENTRY_ID";

                journal.Add(new JOURNAL_ENTRY
                {
                    JOURNAL_ENTRY_ID = journalId,
                    ENTRY_NUMBER = NextJournalNumber(journals, computation.Date),
                    ENTRY_DATE = computation.Date,
                    ENTRY_TYPE = "TAX",
                    STATUS = LedgerCodes.JournalPosted,
                    DESCRIPTION = computation.Describe(),
                    REFERENCE_NUMBER = reference,
                    SOURCE_MODULE = "TAX",
                    TOTAL_DEBIT = amount,
                    TOTAL_CREDIT = amount,
                    AUTHOR = actor.Trim(),
                    POSTED_BY = actor.Trim(),
                    POSTED_AT = now,
                    PERIOD_CODE = periodCode,
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString("N"),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });

                var journalCommitted = await journal.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(journalCommitted))
                {
                    return OgResult<OgTaxWorkspace>.Failed(
                        OgUow.Message(journalCommitted, "The tax journal was not written; nothing was posted."),
                        TaxRules.CodeWriteFailed);
                }

                var lines = OgUow.For<JOURNAL_ENTRY_LINE>(source);
                lines.PrimaryKey = "JOURNAL_ENTRY_LINE_ID";

                lines.Add(EntryLine(journalId, debit!, amount, 0m, 1, computation.Describe(), actor, now));
                lines.Add(EntryLine(journalId, credit!, 0m, amount, 2, computation.Describe(), actor, now));

                var linesCommitted = await lines.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(linesCommitted))
                {
                    return OgResult<OgTaxWorkspace>.Failed(
                        "The journal's lines were not saved.", TaxRules.CodeWriteFailed);
                }
            }

            // A deferred computation also carries its closing balances forward, which is what the next movement measures
            // against - the owner's own RecordDeferredTaxBalance contract.
            if (computation.Kind == TaxComputationCodes.ProvisionDeferred)
            {
                var balance = OgUow.For<DEFERRED_TAX_BALANCE>(source);
                balance.PrimaryKey = "DEFERRED_TAX_BALANCE_ID";

                balance.Add(new DEFERRED_TAX_BALANCE
                {
                    DEFERRED_TAX_BALANCE_ID = Guid.NewGuid().ToString("N"),
                    PROPERTY_ID = computation.PropertyId,
                    PERIOD_END_DATE = computation.PeriodTo,
                    DEFERRED_TAX_ASSET = computation.Amount < 0m ? Math.Abs(computation.Amount) : 0m,
                    DEFERRED_TAX_LIABILITY = computation.Amount >= 0m ? computation.Amount : 0m,
                    NOTES = computation.Describe() + " · journal " + journalId,
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString("N"),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });

                var balanceCommitted = await balance.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(balanceCommitted))
                {
                    return OgResult<OgTaxWorkspace>.Failed(
                        OgUow.Message(balanceCommitted, "The deferred balance was not recorded."),
                        TaxRules.CodeWriteFailed);
                }
            }

            var unit = OgUow.For<TAX_TRANSACTION>(source);
            unit.PrimaryKey = "TAX_TRANSACTION_ID";

            var rows = await OgUow.ReadAsync(
                unit, [OgUow.Equal("TAX_TRANSACTION_ID", computation.Id)], cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgTaxWorkspace>.Failed(
                    $"Computation '{computationId}' is not in this database.", TaxRules.CodeUnknownComputation);
            }

            row.STATUS = TaxComputationCodes.StatusPosted;
            row.JOURNAL_ENTRY_ID = journalId;
            row.POSTED_BY = actor.Trim();
            row.POSTED_AT = now;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = now;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgTaxWorkspace>.Failed(
                    OgUow.Message(updated, "The computation was not marked posted."), TaxRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgTaxWorkspace>.Failed(
                    OgUow.Message(committed, "The computation was not marked posted."), TaxRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, context, computation.Id, null, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgTaxWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"posting tax computation {computationId}", "the computation was not posted", FailureSeverity.Error);

            return OgResult<OgTaxWorkspace>.Failed(
                "The computation could not be posted.", reference ?? TaxRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgTaxWorkspace>> LoadAsync(
        OgTaxContext context,
        string? computationId = null,
        string? jurisdiction = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgTaxWorkspace>.Failed(
                "No connection is open, so the tax computations cannot be read.", TaxRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, computationId, jurisdiction, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgTaxWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the tax computations", "the tax computations are unavailable; nothing can be posted",
                FailureSeverity.Degraded);

            return OgResult<OgTaxWorkspace>.Ok(new OgTaxWorkspace(
                [], null, [], null, [], TaxRules.Kinds, TaxRules.Statuses, TaxRules.Currencies,
                string.Empty, false, string.Empty, true,
                $"the tax computations could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the gate and the reads

    /// <summary>
    /// SG-10-8 and AC-3: the first month of the fiscal year that makes the year incomplete, or null when it is complete.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A year is complete when every month of it has a ledger period and every month <b>before the period-end month</b> is
    /// closed or locked. The period-end month itself may still be open, because the tax provision is one of that month's
    /// closing entries rather than something recorded after it: demanding the month be closed first would make a year-end
    /// provision impossible to post into its own year, which is the contradiction the first version of this gate ran into.
    /// </para>
    /// <para>
    /// Whether the posting period then accepts a write is the ledger's own rule, checked when the computation is posted. The
    /// two rules are deliberately separate, so a locked book still refuses the posting and says so.
    /// </para>
    /// <para>
    /// The app assumes a calendar fiscal year, because the owner has no fiscal-calendar table; the assumption is stated here
    /// rather than buried.
    /// </para>
    /// </remarks>
    public static string? IncompleteFiscalYear(DateTime periodEnd, IReadOnlyList<COMPANY_LEDGER_PERIOD> periods)
    {
        ArgumentNullException.ThrowIfNull(periods);

        for (var month = 1; month <= 12; month++)
        {
            var code = new DateTime(periodEnd.Year, month, 1)
                .ToString("yyyy-MM", CultureInfo.InvariantCulture);

            var period = periods.FirstOrDefault(row =>
                string.Equals(row.PERIOD_CODE?.Trim(), code, StringComparison.OrdinalIgnoreCase));

            if (period is null)
            {
                return "period " + code + " does not exist";
            }

            // The period-end month is the one the provision is booked into, so it is allowed to be open.
            if (month == periodEnd.Month)
            {
                continue;
            }

            var closed = string.Equals(period.STATUS, LedgerCodes.PeriodClosed, StringComparison.OrdinalIgnoreCase)
                         || string.Equals(period.STATUS, LedgerCodes.PeriodLocked, StringComparison.OrdinalIgnoreCase);

            if (!closed)
            {
                return "period " + code + " is " + (period.STATUS ?? "unstated").ToLowerInvariant();
            }
        }

        return null;
    }

    private async Task<OgTaxWorkspace> ReadAsync(
        IDataSource source,
        OgTaxContext? context,
        string? computationId,
        string? jurisdiction,
        CancellationToken cancellationToken)
    {
        var rateRows = await OgUow.ReadAllAsync<GOVERNMENTAL_TAX_DATA>(source).ConfigureAwait(false);
        var computationRows = await OgUow.ReadAllAsync<TAX_TRANSACTION>(source).ConfigureAwait(false);
        var balanceRows = await OgUow.ReadAllAsync<DEFERRED_TAX_BALANCE>(source).ConfigureAwait(false);
        var periodRows = await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var jurisdictions = rateRows
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Select(row => new
            {
                Row = row,
                Name = TaxRemark.Value(row.REMARK, ProductionTaxRemarkKeys.Jurisdiction),
            })
            // A rate row with no jurisdiction cannot be attributed, and the computation refuses rather than guessing.
            .Where(item => !string.IsNullOrWhiteSpace(item.Name))
            .GroupBy(item => item.Name!, StringComparer.OrdinalIgnoreCase)
            .Select(group => new OgTaxJurisdiction(
                group.First().Row.GOVERNMENTAL_TAX_DATA_ID,
                group.First().Name!,
                group.Select(item => PercentPoints(item.Row.SEVERANCE_TAX)).FirstOrDefault(value => value is not null),
                group.Select(item => PercentPoints(item.Row.AD_VALOREM_TAX)).FirstOrDefault(value => value is not null),
                group.Select(item => TaxRemark.Rate(item.Row.REMARK, ProductionTaxRemarkKeys.DeferredTaxRate))
                    .FirstOrDefault(value => value is not null),
                group.Min(item => item.Row.EFFECTIVE_DATE),
                group.Max(item => item.Row.EXPIRY_DATE),
                group.First().Row.REMARK))
            .OrderBy(jurisdictionRow => jurisdictionRow.Jurisdiction, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var computations = computationRows
            .Where(row => !string.IsNullOrWhiteSpace(row.TAX_TRANSACTION_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.IsNullOrWhiteSpace(jurisdiction)
                          || string.Equals(row.TAX_JURISDICTION?.Trim(), jurisdiction.Trim(),
                              StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.PERIOD_TO ?? row.TAX_DATE)
            .ThenBy(row => row.TAX_TRANSACTION_ID, StringComparer.Ordinal)
            .Select(ToComputation)
            .ToList();

        var deferred = balanceRows
            .Where(row => !string.IsNullOrWhiteSpace(row.DEFERRED_TAX_BALANCE_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => row.PERIOD_END_DATE is not null)
            .OrderByDescending(row => row.PERIOD_END_DATE)
            .Select(row => new OgDeferredBalance(
                row.DEFERRED_TAX_BALANCE_ID,
                row.PERIOD_END_DATE!.Value,
                row.DEFERRED_TAX_ASSET ?? 0m,
                row.DEFERRED_TAX_LIABILITY ?? 0m,
                row.NOTES))
            .ToList();

        var selectedJurisdiction = string.IsNullOrWhiteSpace(jurisdiction)
            ? jurisdictions.FirstOrDefault()
            : jurisdictions.FirstOrDefault(row =>
                  string.Equals(row.Jurisdiction, jurisdiction.Trim(), StringComparison.OrdinalIgnoreCase))
              ?? jurisdictions.FirstOrDefault();

        var selected = string.IsNullOrWhiteSpace(computationId)
            ? computations.FirstOrDefault()
            : computations.FirstOrDefault(computation =>
                  string.Equals(computation.Id, computationId.Trim(), StringComparison.OrdinalIgnoreCase))
              ?? computations.FirstOrDefault();

        // The fiscal-year state is shown for the whole current year, so its period end is the year end - which is the month
        // a year-end provision is booked into and therefore the one month allowed to be open.
        var fiscalYear = new DateTime(DateTime.Today.Year, 12, 31);
        var incomplete = IncompleteFiscalYear(fiscalYear, periodRows);

        _ = context;

        return new OgTaxWorkspace(
            jurisdictions,
            selectedJurisdiction,
            computations,
            selected,
            deferred,
            TaxRules.Kinds,
            TaxRules.Statuses,
            TaxRules.Currencies,
            jurisdictions.Count == 0
                ? "no jurisdiction has a configured rate; a rate row needs " +
                  ProductionTaxRemarkKeys.Jurisdiction + "= in its remark"
                : string.Join(Environment.NewLine, jurisdictions.Select(row => row.Describe())),
            incomplete is null,
            incomplete is null
                ? "fiscal year " + fiscalYear.Year.ToString(CultureInfo.InvariantCulture) + " is closed through December"
                : "fiscal year " + fiscalYear.Year.ToString(CultureInfo.InvariantCulture) + " is incomplete: " + incomplete,
            false,
            null);
    }

    /// <summary>
    /// A rate as it is recorded, in percentage points, for display only - the computation normalises it itself, so a stored
    /// 0.125 and a stored 12.5 are both shown as the same percentage.
    /// </summary>
    private static decimal? PercentPoints(decimal? recorded)
    {
        if (recorded is not { } value || value <= 0m)
        {
            return null;
        }

        return value > 1m ? value : value * 100m;
    }

    private static OgTaxComputation ToComputation(TAX_TRANSACTION row) =>
        new(
            row.TAX_TRANSACTION_ID,
            row.TAX_JURISDICTION ?? string.Empty,
            row.TAX_TYPE ?? string.Empty,
            row.TAX_DATE ?? DateTime.MinValue,
            row.TAX_BASIS ?? 0m,
            row.BASIS_TEXT ?? "basis",
            row.TAX_RATE ?? 0m,
            row.TAX_AMOUNT ?? 0m,
            row.CURRENCY_CODE ?? string.Empty,
            row.RATE_EFFECTIVE_DATE,
            row.RATE_SOURCE,
            row.PERIOD_FROM,
            row.PERIOD_TO,
            row.FISCAL_YEAR ?? string.Empty,
            row.STATUS ?? TaxComputationCodes.StatusComputed,
            row.PROPERTY_ID,
            row.AUTHOR,
            row.JOURNAL_ENTRY_ID,
            row.POSTED_BY,
            row.POSTED_AT,
            row.DESCRIPTION);

    private static JOURNAL_ENTRY_LINE EntryLine(
        string journalId,
        GL_ACCOUNT account,
        decimal debit,
        decimal credit,
        int number,
        string description,
        string actor,
        DateTime now) =>
        new()
        {
            JOURNAL_ENTRY_LINE_ID = Guid.NewGuid().ToString("N"),
            JOURNAL_ENTRY_ID = journalId,
            GL_ACCOUNT_ID = account.GL_ACCOUNT_ID,
            LINE_NUMBER = number,
            DEBIT_AMOUNT = debit,
            CREDIT_AMOUNT = credit,
            DESCRIPTION = description,
            CURRENCY_CODE = account.CURRENCY_CODE,
            ACTIVE_IND = "Y",
            PPDM_GUID = Guid.NewGuid().ToString("N"),
            ROW_CREATED_BY = actor.Trim(),
            ROW_CREATED_DATE = now,
            ROW_CHANGED_BY = actor.Trim(),
            ROW_CHANGED_DATE = now,
        };

    private static GL_ACCOUNT? Find(IReadOnlyList<GL_ACCOUNT> accounts, string number) =>
        accounts.FirstOrDefault(account =>
            string.Equals(account.ACCOUNT_NUMBER?.Trim(), number, StringComparison.OrdinalIgnoreCase));

    private static string NextJournalNumber(IReadOnlyList<JOURNAL_ENTRY> existing, DateTime date)
    {
        var prefix = "JE-" + date.Year.ToString(CultureInfo.InvariantCulture) + "-";

        var highest = existing
            .Select(row => row.ENTRY_NUMBER)
            .Where(number => number is not null && number.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(number => int.TryParse(number![prefix.Length..], NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var sequence)
                ? sequence
                : 0)
            .DefaultIfEmpty(0)
            .Max();

        return prefix + (highest + 1).ToString("0000", CultureInfo.InvariantCulture);
    }
}
