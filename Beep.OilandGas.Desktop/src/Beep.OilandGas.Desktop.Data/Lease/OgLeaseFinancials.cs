using System.Globalization;
using Beep.OilandGas.Models.Data.Lease;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Lease;

/// <summary>One burden on a lease (LSE-03): a named fraction deducted from gross revenue.</summary>
public sealed record OgLeaseBurden(string Name, decimal Fraction)
{
    public string Text => $"{Name} {(Fraction * 100m).ToString("0.####", CultureInfo.InvariantCulture)}%";
}

/// <summary>The terms of a lease, as a fraction of gross revenue.</summary>
public sealed record OgLeaseTerms(
    decimal? Bonus,
    string? BonusCurrency,
    decimal RoyaltyRate,
    decimal NetRevenueInterest,
    decimal OverridingRoyalty,
    IReadOnlyList<OgLeaseBurden> Burdens,
    DateTime? Effective,
    DateTime? Expiry,
    string? Reason = null)
{
    /// <summary>AC-2: the rate is stored as a fraction with an explicit 8-place precision.</summary>
    public const int RatePrecision = 8;

    public string RateFormat => "0." + new string('0', RatePrecision);

    public string RoyaltyText => RoyaltyRate.ToString(RateFormat, CultureInfo.InvariantCulture);

    public string RoyaltyPercentText => $"{(RoyaltyRate * 100m).ToString("0.####", CultureInfo.InvariantCulture)}%";

    public string NetRevenueText => $"{(NetRevenueInterest * 100m).ToString("0.####", CultureInfo.InvariantCulture)}%";

    public string OverridingText => $"{(OverridingRoyalty * 100m).ToString("0.####", CultureInfo.InvariantCulture)}%";

    public string BonusText => Bonus is { } bonus
        ? $"{bonus.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(BonusCurrency) ? "(currency not recorded)" : BonusCurrency!.Trim())
        : "no bonus recorded";

    public string BurdensText => Burdens.Count == 0
        ? "no burdens"
        : string.Join(" · ", Burdens.Select(burden => burden.Text));

    public string Describe() =>
        $"bonus {BonusText} · royalty {RoyaltyPercentText} ({RoyaltyText}) · NRI {NetRevenueText} · " +
        $"ORR {OverridingText} · {BurdensText}";
}

/// <summary>The outcome of the NRI arithmetic; a refusal names the operation and the value.</summary>
public sealed record OgLeaseNriResult(bool Success, decimal NetRevenueInterest, string? Code, string? Message)
{
    public static OgLeaseNriResult Ok(decimal nri) => new(true, nri, null, null);

    public static OgLeaseNriResult Refused(string code, string message) => new(false, 0m, code, message);
}

/// <summary>
/// LSE-03 AC-1/AC-2: the net revenue interest arithmetic, stated once. Every rate is a fraction with at most eight
/// decimal places, and the deductions may not exceed gross revenue unless the lease type explicitly permits it.
/// </summary>
public static class OgLeaseNri
{
    public const int RatePrecision = OgLeaseTerms.RatePrecision;
    public const decimal Tolerance = 0.00000001m;

    public const string CodePrecision = "lease-financial-precision";
    public const string CodeFraction = "lease-financial-fraction";
    public const string CodeUnrepresentable = "lease-financial-nri-unrepresentable";

    /// <summary>Lease types whose NRI is negotiated directly rather than derived by subtraction.</summary>
    public static IReadOnlyList<string> TypesPermittingUnboundedNri { get; } =
        ["NET_PROFIT", "NET_PROFIT_LEASE"];

    public static OgLeaseNriResult Compute(
        decimal royaltyRate,
        decimal overridingRoyalty,
        IReadOnlyList<OgLeaseBurden> burdens,
        decimal? suppliedNri,
        string? leaseType)
    {
        ArgumentNullException.ThrowIfNull(burdens);

        var checks = new[]
        {
            CheckFraction(royaltyRate, "Royalty rate"),
            CheckFraction(overridingRoyalty, "Overriding royalty"),
        };

        foreach (var check in checks)
        {
            if (check is not null)
            {
                return check;
            }
        }

        foreach (var burden in burdens)
        {
            if (CheckFraction(burden.Fraction, $"Burden '{burden.Name}'") is { } refused)
            {
                return refused;
            }
        }

        var total = royaltyRate + overridingRoyalty + burdens.Sum(burden => burden.Fraction);
        var derived = 1m - total;
        var permitted = !string.IsNullOrWhiteSpace(leaseType)
                        && TypesPermittingUnboundedNri.Contains(leaseType.Trim(), StringComparer.OrdinalIgnoreCase);

        if (total > 1m && !permitted)
        {
            return OgLeaseNriResult.Refused(
                CodeUnrepresentable,
                $"The deductions total {total.ToString("0.########", CultureInfo.InvariantCulture)} of gross " +
                $"revenue, which leaves a net revenue interest below zero. Reduce a royalty, overriding royalty " +
                $"or burden. (Lease type '{leaseType ?? "(none)"}' does not permit it.)");
        }

        if (suppliedNri is { } supplied)
        {
            if (CheckFraction(supplied, "Net revenue interest") is { } nriRefused)
            {
                return nriRefused;
            }

            if (!permitted && Math.Abs(supplied - derived) > Tolerance)
            {
                return OgLeaseNriResult.Refused(
                    CodeUnrepresentable,
                    $"The NRI {supplied.ToString("0.########", CultureInfo.InvariantCulture)} is not " +
                    $"1 - royalty - overriding royalty - burdens = " +
                    $"{derived.ToString("0.########", CultureInfo.InvariantCulture)}. Correct one of the values, " +
                    "or use a lease type that records the NRI directly.");
            }

            return OgLeaseNriResult.Ok(supplied);
        }

        return OgLeaseNriResult.Ok(derived);
    }

    private static OgLeaseNriResult? CheckFraction(decimal value, string what)
    {
        if (decimal.Round(value, RatePrecision) != value)
        {
            return OgLeaseNriResult.Refused(
                CodePrecision,
                $"{what} {value.ToString(CultureInfo.InvariantCulture)} has more than {RatePrecision} decimal " +
                "places; store the rate at the agreed precision instead.");
        }

        return value is < 0m or > 1m
            ? OgLeaseNriResult.Refused(
                CodeFraction,
                $"{what} {value.ToString(CultureInfo.InvariantCulture)} must be a fraction between 0 and 1.")
            : null;
    }
}

/// <summary>One obligation or rental due on a lease (LSE-03 AC-5).</summary>
public sealed record OgLeaseObligation(
    string Id,
    string Type,
    string Description,
    DateTime? DueDate,
    decimal? MinimumSpend,
    string? Currency,
    bool Paid,
    bool Overdue,
    int? DaysOverdue)
{
    public const string Rental = "RENTAL";
    public const string MinimumSpendType = "MINIMUM_SPEND";
    public const string Other = "OTHER";

    public static IReadOnlyList<string> Types { get; } = [Rental, MinimumSpendType, Other];

    public string AmountText => MinimumSpend is { } amount
        ? $"{amount.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(Currency) ? "(currency not recorded)" : Currency!.Trim())
        : "no amount recorded";

    public string StatusText => Paid
        ? "paid"
        : Overdue
            ? $"overdue {DaysOverdue.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)} d"
            : "due";

    public string Describe() => $"{Type}: {Description} · {DueDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "no due date"} · {AmountText} · {StatusText}";
}

/// <summary>One payment against a lease (LSE-03).</summary>
public sealed record OgLeasePayment(
    string Id,
    string? ObligationId,
    DateTime? PaidAt,
    decimal Amount,
    string? Currency,
    string? Kind,
    string? Reference,
    string? Actor)
{
    public string AmountText =>
        $"{Amount.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
        (string.IsNullOrWhiteSpace(Currency) ? "(currency not recorded)" : Currency!.Trim());

    public string Describe() =>
        $"{PaidAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "unknown"} · {AmountText} · " +
        $"{Kind ?? "payment"}{(string.IsNullOrWhiteSpace(Reference) ? string.Empty : $" · {Reference}")}";
}

/// <summary>One lifecycle event (LSE-03).</summary>
public sealed record OgLeaseLifecycleEntry(
    string Event,
    DateTime? Effective,
    string? Rationale,
    string? Actor,
    DateTime? RecordedAt)
{
    public const string Renewed = "RENEWED";
    public const string Relinquished = "RELINQUISHED";
    public const string Expired = "EXPIRED";

    public string Describe() =>
        $"{Event} on {Effective?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "unknown"} by " +
        $"{Actor ?? "(unknown)"} ({Rationale ?? "no rationale"})";
}

/// <summary>One lease in the financial register.</summary>
public sealed record OgLeaseFinancialSummary(
    string Id,
    string Name,
    string? Region,
    DateTime? Effective,
    DateTime? Expiry,
    int? DaysToExpiry,
    decimal RoyaltyRate,
    decimal NetRevenueInterest,
    decimal OverridingRoyalty,
    decimal? Bonus,
    string? BonusCurrency,
    int OverdueCount,
    int? WorstOverdueDays,
    string Lifecycle)
{
    public const string Expired = "EXPIRED";
    public const string Expiring = "EXPIRING";
    public const string Active = "ACTIVE";
    public const string Unclassified = "UNCLASSIFIED";

    public string ExpiryState => Expiry is null
        ? Unclassified
        : DaysToExpiry is < 0
            ? Expired
            : DaysToExpiry is <= 90
                ? Expiring
                : Active;

    public string ExpiryText => Expiry?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "no expiry recorded";

    public string RoyaltyText => $"{(RoyaltyRate * 100m).ToString("0.####", CultureInfo.InvariantCulture)}%";

    public string NetRevenueText => $"{(NetRevenueInterest * 100m).ToString("0.####", CultureInfo.InvariantCulture)}%";

    public string OverdueText => OverdueCount == 0
        ? "no overdue obligation"
        : $"{OverdueCount.ToString(CultureInfo.InvariantCulture)} overdue" +
          (WorstOverdueDays is { } days ? $" (worst {days.ToString(CultureInfo.InvariantCulture)} d)" : string.Empty);

    public string Describe() =>
        string.Join(
            " · ",
            new[]
            {
                Name,
                Region,
                $"expires {ExpiryText}",
                $"royalty {RoyaltyText}",
                $"NRI {NetRevenueText}",
                OverdueText,
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
}

/// <summary>One 30/60/90-day expiry bucket.</summary>
public sealed record OgLeaseExpiryBucket(
    string Label,
    int FromDays,
    int ToDays,
    IReadOnlyList<OgLeaseFinancialSummary> Leases)
{
    public int Count => Leases.Count;

    public string Header => $"{Label} ({Count.ToString(CultureInfo.InvariantCulture)})";
}

/// <summary>The financial register and its expiry calendar.</summary>
public sealed record OgLeaseFinancialSnapshot(
    string FieldName,
    IReadOnlyList<OgLeaseFinancialSummary> Leases,
    IReadOnlyList<OgLeaseExpiryBucket> Expiry,
    DateTime AsOf)
{
    public int Count => Leases.Count;

    public bool IsEmpty => Count == 0;

    public int OverdueObligations => Leases.Sum(lease => lease.OverdueCount);

    public string Summary =>
        $"{Count.ToString(CultureInfo.InvariantCulture)} lease(s) · " +
        string.Join(" · ", Expiry.Select(bucket => bucket.Header));
}

/// <summary>Everything the financial detail shows for one lease.</summary>
public sealed record OgLeaseFinancialDetail(
    OgLeaseFinancialSummary Summary,
    OgLeaseTerms Terms,
    IReadOnlyList<OgLeaseTerms> TermsHistory,
    IReadOnlyList<OgLeaseObligation> Obligations,
    IReadOnlyList<OgLeasePayment> Payments,
    IReadOnlyList<OgLeaseLifecycleEntry> Lifecycle)
{
    public IReadOnlyList<OgLeaseObligation> OverdueObligations =>
        Obligations.Where(obligation => obligation.Overdue).ToList();

    public int OverdueCount => OverdueObligations.Count;

    public string LifecycleState => Lifecycle.Count == 0 ? "ACTIVE" : Lifecycle[0].Event;
}

/// <summary>One task the inbox (WFL-05) will read for an expiring lease (AC-3).</summary>
public sealed record OgLeaseExpiryTask(
    string LeaseId,
    string Title,
    DateTime Due,
    string Bucket,
    string Route)
{
    public const string LeaseRoute = "leasefinancials";
}

/// <summary>One renewal, as the page submits it.</summary>
public sealed record OgLeaseRenewal(
    string LeaseId,
    DateTime NewExpiry,
    string Rationale,
    string Actor,
    DateTimeOffset? DecidedAt = null);

/// <summary>LSE-03: the financial terms, obligations, payments and lifecycle of a lease.</summary>
public interface IOgLeaseFinancials
{
    Task<OgResult<OgLeaseFinancialSnapshot>> LoadAsync(
        string? fieldId,
        string fieldName,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgLeaseFinancialDetail>> LoadDetailAsync(
        string leaseId,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>Appends new terms, retaining the previous term (AC-4).</summary>
    Task<OgResult<OgLeaseFinancialDetail>> SaveTermsAsync(
        string leaseId,
        OgLeaseTerms terms,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgLeaseFinancialDetail>> RecordObligationAsync(
        string leaseId,
        string type,
        string description,
        DateTime dueDate,
        decimal minimumSpend,
        string currency,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgLeaseFinancialDetail>> RecordPaymentAsync(
        string leaseId,
        decimal amount,
        string currency,
        string kind,
        string? reference,
        string? obligationId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>AC-4: a renewal appends a new term and retains the old one.</summary>
    Task<OgResult<OgLeaseFinancialDetail>> RenewAsync(
        string leaseId,
        DateTime newExpiry,
        string rationale,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgLeaseFinancialDetail>> RelinquishAsync(
        string leaseId,
        DateTime effective,
        string rationale,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgLeaseFinancialDetail>> ExpireAsync(
        string leaseId,
        DateTime effective,
        string rationale,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>AC-3: the expiring set the task inbox (WFL-05) reads; the inbox itself is not built.</summary>
    Task<OgResult<IReadOnlyList<OgLeaseExpiryTask>>> ExpiringTasksAsync(
        DateTime asOf,
        int days = 90,
        CancellationToken cancellationToken = default);

    /// <summary>The renewal that was not acknowledged, or null.</summary>
    OgLeaseRenewal? Pending { get; }

    Task<OgResult<OgLeaseFinancialDetail>> RetryAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The financial side of a lease over <c>LEASE_ACQUISITION</c> and the LeaseAcquisition-owned records
/// <c>LEASE_TERM</c>, <c>LEASE_OBLIGATION</c>, <c>LEASE_PAYMENT</c> and <c>LEASE_LIFECYCLE</c>, read and written
/// through their entity models and a unit of work.
/// </summary>
public sealed class OgLeaseFinancialService : IOgLeaseFinancials
{
    public const string CodeNoConnection = "lease-financial-no-connection";
    public const string CodeReadFailed = "lease-financial-read-failed";
    public const string CodeWriteFailed = "lease-financial-write-failed";
    public const string CodeUnknown = "lease-financial-unknown";
    public const string CodeActorRequired = "lease-financial-actor-required";
    public const string CodeRationaleRequired = "lease-financial-rationale-required";
    public const string CodeCurrencyRequired = "lease-financial-currency-required";
    public const string CodeExpiryRequired = "lease-financial-expiry-required";
    public const string CodeExpiryOrder = "lease-financial-expiry-order";
    public const string CodeAmountInvalid = "lease-financial-amount-invalid";
    public const string CodeNothingPending = "lease-financial-nothing-pending";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgLeaseFinancialService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgLeaseRenewal? Pending { get; private set; }

    public async Task<OgResult<OgLeaseFinancialSnapshot>> LoadAsync(
        string? fieldId,
        string fieldName,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgLeaseFinancialSnapshot>.Failed(
                "No connection is open, so lease financials cannot be read.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var summaries = model.Opportunities
                .Where(opportunity => FieldMatches(opportunity.FIELD_ID, fieldId, fieldName))
                .Select(opportunity => Summary(opportunity, model, asOf.Date))
                .ToList();

            var buckets = BuildExpiry(summaries, asOf.Date);

            return OgResult<OgLeaseFinancialSnapshot>.Ok(new OgLeaseFinancialSnapshot(
                string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
                summaries,
                buckets,
                asOf.Date));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading lease financials",
                "the financial register is not shown",
                FailureSeverity.Error);

            return OgResult<OgLeaseFinancialSnapshot>.Failed(
                "Lease financials could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgLeaseFinancialDetail>> LoadDetailAsync(
        string leaseId,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(leaseId);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "No connection is open, so the lease cannot be read.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var detail = Detail(model, leaseId.Trim(), asOf.Date);

            return detail is null
                ? OgResult<OgLeaseFinancialDetail>.Failed(
                    $"Lease '{leaseId}' is not in this database.", CodeUnknown)
                : OgResult<OgLeaseFinancialDetail>.Ok(detail);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reading the financials of lease {leaseId}",
                "the lease financials are not shown",
                FailureSeverity.Error);

            return OgResult<OgLeaseFinancialDetail>.Failed(
                "The lease financials could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgLeaseFinancialDetail>> SaveTermsAsync(
        string leaseId,
        OgLeaseTerms terms,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(terms);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "A term change must record who made it.", CodeActorRequired);
        }

        if (terms.Bonus is < 0m)
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "A bonus cannot be negative.", CodeAmountInvalid);
        }

        if (terms.Bonus is > 0m && string.IsNullOrWhiteSpace(terms.BonusCurrency))
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "A bonus needs the currency it was paid in.", CodeCurrencyRequired);
        }

        if (terms.Effective is { } effective && terms.Expiry is { } expiry && expiry <= effective)
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "The expiry must be after the effective date.", CodeExpiryOrder);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "No connection is open, so the terms cannot be saved.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var opportunity = model.Opportunities.FirstOrDefault(row =>
                string.Equals(row.LEASE_ACQUISITION_ID, leaseId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (opportunity is null)
            {
                return OgResult<OgLeaseFinancialDetail>.Failed(
                    $"Lease '{leaseId}' is not in this database.", CodeUnknown);
            }

            // AC-1/AC-2: the NRI is checked against the deductions and the lease type's own rule.
            var nri = OgLeaseNri.Compute(
                terms.RoyaltyRate,
                terms.OverridingRoyalty,
                terms.Burdens,
                terms.NetRevenueInterest,
                opportunity.LEASE_TYPE);

            if (!nri.Success)
            {
                return OgResult<OgLeaseFinancialDetail>.Failed(
                    nri.Message ?? "The terms are not representable.", nri.Code);
            }

            var now = DateTime.UtcNow;
            var term = new LEASE_TERM
            {
                TERM_ID = Guid.NewGuid().ToString("N"),
                LEASE_ID = opportunity.LEASE_ACQUISITION_ID!,
                TERM_TYPE = ActiveTerm(model, opportunity.LEASE_ACQUISITION_ID!) is null ? "ORIGINAL" : "AMENDED",
                EFFECTIVE_DATE = terms.Effective ?? opportunity.ACQUISITION_DATE,
                EXPIRY_DATE = terms.Expiry ?? opportunity.LEASE_EXPIRATION,
                BONUS_AMOUNT = terms.Bonus ?? 0m,
                BONUS_CURRENCY = string.IsNullOrWhiteSpace(terms.BonusCurrency) ? null : terms.BonusCurrency.Trim().ToUpperInvariant(),
                ROYALTY_RATE = terms.RoyaltyRate,
                NET_REVENUE_INTEREST = nri.NetRevenueInterest,
                OVERRIDING_ROYALTY = terms.OverridingRoyalty,
                BURDENS_TEXT = FormatBurdens(terms.Burdens),
                RATIONALE = terms.Reason,
                ACTOR = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            };

            if (!await AppendTermAsync(source, opportunity, term, model, actor.Trim(), now, cancellationToken)
                    .ConfigureAwait(false))
            {
                return OgResult<OgLeaseFinancialDetail>.Failed(
                    "The terms were not saved.", CodeWriteFailed);
            }

            return await DetailAfterWriteAsync(source, opportunity.LEASE_ACQUISITION_ID!, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving the terms of lease {leaseId}",
                "the terms were not saved",
                FailureSeverity.Error);

            return OgResult<OgLeaseFinancialDetail>.Failed(
                "The terms could not be saved.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgLeaseFinancialDetail>> RecordObligationAsync(
        string leaseId,
        string type,
        string description,
        DateTime dueDate,
        decimal minimumSpend,
        string currency,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "An obligation must record who entered it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(description) || dueDate == default)
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "An obligation needs a description and a due date.", "lease-financial-obligation-required");
        }

        if (minimumSpend < 0m)
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "A minimum spend cannot be negative.", CodeAmountInvalid);
        }

        if (minimumSpend > 0m && string.IsNullOrWhiteSpace(currency))
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "A minimum spend needs its currency.", CodeCurrencyRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "No connection is open, so the obligation cannot be recorded.", CodeNoConnection);
        }

        try
        {
            var leases = await OgUow.ReadAsync(
                OgUow.For<LEASE_ACQUISITION>(source),
                [OgUow.Equal("LEASE_ACQUISITION_ID", leaseId.Trim())],
                cancellationToken).ConfigureAwait(false);

            if (leases.Count == 0)
            {
                return OgResult<OgLeaseFinancialDetail>.Failed(
                    $"Lease '{leaseId}' is not in this database.", CodeUnknown);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<LEASE_OBLIGATION>(source);

            unit.Add(new LEASE_OBLIGATION
            {
                OBLIGATION_ID = Guid.NewGuid().ToString("N"),
                LEASE_ID = leaseId.Trim(),
                OBLIGATION_TYPE = string.IsNullOrWhiteSpace(type) ? OgLeaseObligation.Other : type.Trim().ToUpperInvariant(),
                DESCRIPTION = description.Trim(),
                DUE_DATE = dueDate.Date,
                MINIMUM_SPEND = minimumSpend,
                CURRENCY = string.IsNullOrWhiteSpace(currency) ? null : currency.Trim().ToUpperInvariant(),
                ACTOR = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? await DetailAfterWriteAsync(source, leaseId.Trim(), cancellationToken).ConfigureAwait(false)
                : OgResult<OgLeaseFinancialDetail>.Failed(
                    OgUow.Message(committed, "The obligation was not recorded."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"recording an obligation for {leaseId}",
                "the obligation was not recorded",
                FailureSeverity.Error);

            return OgResult<OgLeaseFinancialDetail>.Failed(
                "The obligation could not be recorded.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgLeaseFinancialDetail>> RecordPaymentAsync(
        string leaseId,
        decimal amount,
        string currency,
        string kind,
        string? reference,
        string? obligationId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "A payment must record who entered it.", CodeActorRequired);
        }

        if (amount <= 0m)
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "A payment must be greater than zero.", CodeAmountInvalid);
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "A payment needs the currency it was made in.", CodeCurrencyRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "No connection is open, so the payment cannot be recorded.", CodeNoConnection);
        }

        try
        {
            var leases = await OgUow.ReadAsync(
                OgUow.For<LEASE_ACQUISITION>(source),
                [OgUow.Equal("LEASE_ACQUISITION_ID", leaseId.Trim())],
                cancellationToken).ConfigureAwait(false);

            if (leases.Count == 0)
            {
                return OgResult<OgLeaseFinancialDetail>.Failed(
                    $"Lease '{leaseId}' is not in this database.", CodeUnknown);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<LEASE_PAYMENT>(source);

            unit.Add(new LEASE_PAYMENT
            {
                PAYMENT_ID = Guid.NewGuid().ToString("N"),
                LEASE_ID = leaseId.Trim(),
                OBLIGATION_ID = string.IsNullOrWhiteSpace(obligationId) ? null : obligationId.Trim(),
                PAID_AT = now,
                AMOUNT = amount,
                CURRENCY = currency.Trim().ToUpperInvariant(),
                KIND = string.IsNullOrWhiteSpace(kind) ? "OTHER" : kind.Trim().ToUpperInvariant(),
                REFERENCE = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim(),
                ACTOR = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? await DetailAfterWriteAsync(source, leaseId.Trim(), cancellationToken).ConfigureAwait(false)
                : OgResult<OgLeaseFinancialDetail>.Failed(
                    OgUow.Message(committed, "The payment was not recorded."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var failureReference = _failures.ReportHandled(
                ex,
                $"recording a payment for {leaseId}",
                "the payment was not recorded",
                FailureSeverity.Error);

            return OgResult<OgLeaseFinancialDetail>.Failed(
                "The payment could not be recorded.", failureReference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgLeaseFinancialDetail>> RenewAsync(
        string leaseId,
        DateTime newExpiry,
        string rationale,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var decision = new OgLeaseRenewal(leaseId, newExpiry, rationale, actor);
        var renewed = await RenewCoreAsync(decision, cancellationToken).ConfigureAwait(false);

        if (renewed.Success)
        {
            Pending = null;
        }
        else if (renewed.Reference is not CodeActorRequired and not CodeRationaleRequired
                 and not CodeExpiryRequired and not CodeExpiryOrder and not CodeUnknown)
        {
            Pending = decision;
        }

        return renewed;
    }

    public async Task<OgResult<OgLeaseFinancialDetail>> RelinquishAsync(
        string leaseId,
        DateTime effective,
        string rationale,
        string actor,
        CancellationToken cancellationToken = default) =>
        await RecordLifecycleAsync(
            leaseId,
            OgLeaseLifecycleEntry.Relinquished,
            effective,
            rationale,
            actor,
            cancellationToken).ConfigureAwait(false);

    public async Task<OgResult<OgLeaseFinancialDetail>> ExpireAsync(
        string leaseId,
        DateTime effective,
        string rationale,
        string actor,
        CancellationToken cancellationToken = default) =>
        await RecordLifecycleAsync(
            leaseId,
            OgLeaseLifecycleEntry.Expired,
            effective,
            rationale,
            actor,
            cancellationToken).ConfigureAwait(false);

    public async Task<OgResult<IReadOnlyList<OgLeaseExpiryTask>>> ExpiringTasksAsync(
        DateTime asOf,
        int days = 90,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await LoadAsync(null, string.Empty, asOf, cancellationToken).ConfigureAwait(false);

        if (!snapshot.Success || snapshot.Value is null)
        {
            return OgResult<IReadOnlyList<OgLeaseExpiryTask>>.Failed(
                snapshot.Message ?? "The expiring leases could not be read.",
                snapshot.Reference ?? CodeReadFailed);
        }

        var tasks = snapshot.Value.Leases
            .Where(lease => lease.Expiry is { } expiry
                            && expiry.Date >= asOf.Date
                            && (expiry.Date - asOf.Date).TotalDays <= days)
            .OrderBy(lease => lease.Expiry)
            .Select(lease => new OgLeaseExpiryTask(
                lease.Id,
                $"{lease.Name} expires {lease.ExpiryText}",
                lease.Expiry!.Value,
                BucketLabel((int)(lease.Expiry.Value.Date - asOf.Date).TotalDays),
                OgLeaseExpiryTask.LeaseRoute))
            .ToList();

        return OgResult<IReadOnlyList<OgLeaseExpiryTask>>.Ok(tasks);
    }

    public async Task<OgResult<OgLeaseFinancialDetail>> RetryAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "There is no renewal waiting to be retried.", CodeNothingPending);
        }

        var retried = await RenewCoreAsync(pending, cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    // ------------------------------------------------------------------ reads

    private static async Task<FinancialModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var opportunities = await OgUow.ReadAllAsync<LEASE_ACQUISITION>(source).ConfigureAwait(false);
        var terms = await OgUow.ReadAllAsync<LEASE_TERM>(source).ConfigureAwait(false);
        var obligations = await OgUow.ReadAllAsync<LEASE_OBLIGATION>(source).ConfigureAwait(false);
        var payments = await OgUow.ReadAllAsync<LEASE_PAYMENT>(source).ConfigureAwait(false);
        var lifecycle = await OgUow.ReadAllAsync<LEASE_LIFECYCLE>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new FinancialModel(opportunities, terms, obligations, payments, lifecycle);
    }

    private static OgLeaseFinancialSummary Summary(
        LEASE_ACQUISITION opportunity,
        FinancialModel model,
        DateTime asOf)
    {
        var id = opportunity.LEASE_ACQUISITION_ID ?? string.Empty;
        var term = ActiveTerm(model, id);
        var expiry = term?.EXPIRY_DATE ?? opportunity.LEASE_EXPIRATION;
        var overdue = OverdueObligations(model, id, asOf).ToList();
        var lifecycle = model.Lifecycle
            .Where(entry => string.Equals(entry.LEASE_ID, id, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(entry => entry.RECORDED_AT)
            .FirstOrDefault()?.EVENT;

        return new OgLeaseFinancialSummary(
            id,
            Blank(opportunity.LEASE_NAME) ?? id,
            Blank(opportunity.STATE_ID),
            term?.EFFECTIVE_DATE ?? opportunity.ACQUISITION_DATE,
            expiry,
            expiry is null ? null : (int)Math.Floor((expiry.Value.Date - asOf.Date).TotalDays),
            term?.ROYALTY_RATE ?? opportunity.ROYALTY_RATE,
            term?.NET_REVENUE_INTEREST ?? opportunity.NET_REVENUE_INTEREST,
            term?.OVERRIDING_ROYALTY ?? 0m,
            term is null || term.BONUS_AMOUNT == 0m ? (opportunity.BONUS_AMOUNT == 0m ? null : opportunity.BONUS_AMOUNT) : term.BONUS_AMOUNT,
            term?.BONUS_CURRENCY ?? Blank(opportunity.BONUS_AMOUNT_OUOM),
            overdue.Count,
            overdue.Count == 0 ? null : overdue.Max(item => item.DaysOverdue),
            Blank(lifecycle) ?? "ACTIVE");
    }

    private static OgLeaseFinancialDetail? Detail(FinancialModel model, string leaseId, DateTime asOf)
    {
        var opportunity = model.Opportunities.FirstOrDefault(row =>
            string.Equals(row.LEASE_ACQUISITION_ID, leaseId, StringComparison.OrdinalIgnoreCase));

        if (opportunity is null)
        {
            return null;
        }

        var id = opportunity.LEASE_ACQUISITION_ID ?? leaseId;
        var summary = Summary(opportunity, model, asOf);
        var terms = ActiveTerm(model, id);
        var current = terms is null ? TermsFromOpportunity(opportunity) : TermsFromTerm(terms);
        var history = model.Terms
            .Where(term => string.Equals(term.LEASE_ID, id, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(term => term.RECORDED_AT)
            .Select(TermsFromTerm)
            .ToList();

        var paid = model.Payments
            .Where(payment => string.Equals(payment.LEASE_ID, id, StringComparison.OrdinalIgnoreCase)
                              && !string.IsNullOrWhiteSpace(payment.OBLIGATION_ID))
            .Select(payment => payment.OBLIGATION_ID!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var obligations = model.Obligations
            .Where(obligation => string.Equals(obligation.LEASE_ID, id, StringComparison.OrdinalIgnoreCase))
            .OrderBy(obligation => obligation.DUE_DATE)
            .Select(obligation =>
            {
                var isPaid = paid.Contains(obligation.OBLIGATION_ID ?? string.Empty);
                var overdue = !isPaid
                              && obligation.DUE_DATE is { } due
                              && due.Date < asOf.Date;

                return new OgLeaseObligation(
                    obligation.OBLIGATION_ID ?? string.Empty,
                    Blank(obligation.OBLIGATION_TYPE) ?? OgLeaseObligation.Other,
                    Blank(obligation.DESCRIPTION) ?? string.Empty,
                    obligation.DUE_DATE,
                    obligation.MINIMUM_SPEND == 0m ? null : obligation.MINIMUM_SPEND,
                    Blank(obligation.CURRENCY),
                    isPaid,
                    overdue,
                    overdue && obligation.DUE_DATE is { } dueDate
                        ? (int)(asOf.Date - dueDate.Date).TotalDays
                        : null);
            })
            .ToList();

        var payments = model.Payments
            .Where(payment => string.Equals(payment.LEASE_ID, id, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(payment => payment.PAID_AT)
            .Select(payment => new OgLeasePayment(
                payment.PAYMENT_ID ?? string.Empty,
                Blank(payment.OBLIGATION_ID),
                payment.PAID_AT,
                payment.AMOUNT,
                Blank(payment.CURRENCY),
                Blank(payment.KIND),
                Blank(payment.REFERENCE),
                Blank(payment.ACTOR)))
            .ToList();

        var lifecycle = model.Lifecycle
            .Where(entry => string.Equals(entry.LEASE_ID, id, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(entry => entry.RECORDED_AT)
            .Select(entry => new OgLeaseLifecycleEntry(
                Blank(entry.EVENT) ?? "EVENT",
                entry.EFFECTIVE_DATE,
                Blank(entry.RATIONALE),
                Blank(entry.ACTOR),
                entry.RECORDED_AT))
            .ToList();

        return new OgLeaseFinancialDetail(summary, current, history, obligations, payments, lifecycle);
    }

    private static OgLeaseTerms TermsFromOpportunity(LEASE_ACQUISITION opportunity) =>
        new(
            opportunity.BONUS_AMOUNT == 0m ? null : opportunity.BONUS_AMOUNT,
            Blank(opportunity.BONUS_AMOUNT_OUOM),
            opportunity.ROYALTY_RATE,
            opportunity.NET_REVENUE_INTEREST,
            0m,
            [],
            opportunity.ACQUISITION_DATE,
            opportunity.LEASE_EXPIRATION,
            "current acquisition row");

    private static OgLeaseTerms TermsFromTerm(LEASE_TERM term) =>
        new(
            term.BONUS_AMOUNT == 0m ? null : term.BONUS_AMOUNT,
            Blank(term.BONUS_CURRENCY),
            term.ROYALTY_RATE,
            term.NET_REVENUE_INTEREST,
            term.OVERRIDING_ROYALTY,
            ParseBurdens(term.BURDENS_TEXT),
            term.EFFECTIVE_DATE,
            term.EXPIRY_DATE,
            Blank(term.RATIONALE));

    private static LEASE_TERM? ActiveTerm(FinancialModel model, string leaseId) =>
        model.Terms
            .Where(term => string.Equals(term.LEASE_ID, leaseId, StringComparison.OrdinalIgnoreCase)
                           && !string.Equals(term.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(term => term.RECORDED_AT)
            .ThenByDescending(term => term.EFFECTIVE_DATE)
            .FirstOrDefault();

    private static IEnumerable<OgLeaseObligation> OverdueObligations(
        FinancialModel model,
        string leaseId,
        DateTime asOf)
    {
        var paid = model.Payments
            .Where(payment => string.Equals(payment.LEASE_ID, leaseId, StringComparison.OrdinalIgnoreCase)
                              && !string.IsNullOrWhiteSpace(payment.OBLIGATION_ID))
            .Select(payment => payment.OBLIGATION_ID!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return model.Obligations
            .Where(obligation => string.Equals(obligation.LEASE_ID, leaseId, StringComparison.OrdinalIgnoreCase))
            .Where(obligation => !paid.Contains(obligation.OBLIGATION_ID ?? string.Empty))
            .Where(obligation => obligation.DUE_DATE is { } due && due.Date < asOf.Date)
            .Select(obligation => new OgLeaseObligation(
                obligation.OBLIGATION_ID ?? string.Empty,
                Blank(obligation.OBLIGATION_TYPE) ?? OgLeaseObligation.Other,
                Blank(obligation.DESCRIPTION) ?? string.Empty,
                obligation.DUE_DATE,
                obligation.MINIMUM_SPEND == 0m ? null : obligation.MINIMUM_SPEND,
                Blank(obligation.CURRENCY),
                false,
                true,
                (int)(asOf.Date - obligation.DUE_DATE!.Value.Date).TotalDays));
    }

    /// <summary>The 30/60/90-day buckets. Public so the rule can be tested without a database.</summary>
    public static IReadOnlyList<OgLeaseExpiryBucket> BuildExpiry(
        IReadOnlyList<OgLeaseFinancialSummary> leases,
        DateTime asOf)
    {
        var live = leases
            .Where(lease => lease.Expiry is { } expiry && expiry.Date >= asOf.Date)
            .ToList();

        return
        [
            new OgLeaseExpiryBucket("0-30 days", 0, 30,
                live.Where(lease => (lease.Expiry!.Value.Date - asOf.Date).TotalDays <= 30)
                    .OrderBy(lease => lease.Expiry).ToList()),
            new OgLeaseExpiryBucket("31-60 days", 31, 60,
                live.Where(lease => (lease.Expiry!.Value.Date - asOf.Date).TotalDays is > 30 and <= 60)
                    .OrderBy(lease => lease.Expiry).ToList()),
            new OgLeaseExpiryBucket("61-90 days", 61, 90,
                live.Where(lease => (lease.Expiry!.Value.Date - asOf.Date).TotalDays is > 60 and <= 90)
                    .OrderBy(lease => lease.Expiry).ToList()),
        ];
    }

    // ----------------------------------------------------------------- writes

    private async Task<OgResult<OgLeaseFinancialDetail>> RenewCoreAsync(
        OgLeaseRenewal renewal,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(renewal.Actor))
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "A renewal must record who made it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(renewal.Rationale))
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "A renewal needs a rationale.", CodeRationaleRequired);
        }

        if (renewal.NewExpiry == default)
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "A renewal needs its new expiry date.", CodeExpiryRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "No connection is open, so the lease cannot be renewed.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var opportunity = model.Opportunities.FirstOrDefault(row =>
                string.Equals(row.LEASE_ACQUISITION_ID, renewal.LeaseId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (opportunity is null)
            {
                return OgResult<OgLeaseFinancialDetail>.Failed(
                    $"Lease '{renewal.LeaseId}' is not in this database.", CodeUnknown);
            }

            var active = ActiveTerm(model, opportunity.LEASE_ACQUISITION_ID!);
            var currentExpiry = active?.EXPIRY_DATE ?? opportunity.LEASE_EXPIRATION;

            if (currentExpiry is { } old && renewal.NewExpiry.Date <= old.Date)
            {
                return OgResult<OgLeaseFinancialDetail>.Failed(
                    $"The new expiry {renewal.NewExpiry:yyyy-MM-dd} must be after the current expiry " +
                    $"{old:yyyy-MM-dd}.",
                    CodeExpiryOrder);
            }

            var now = DateTime.UtcNow;
            var terms = active is null ? TermsFromOpportunity(opportunity) : TermsFromTerm(active);

            var term = new LEASE_TERM
            {
                TERM_ID = Guid.NewGuid().ToString("N"),
                LEASE_ID = opportunity.LEASE_ACQUISITION_ID!,
                TERM_TYPE = "RENEWAL",
                EFFECTIVE_DATE = currentExpiry ?? opportunity.ACQUISITION_DATE,
                EXPIRY_DATE = renewal.NewExpiry.Date,
                BONUS_AMOUNT = terms.Bonus ?? 0m,
                BONUS_CURRENCY = terms.BonusCurrency,
                ROYALTY_RATE = terms.RoyaltyRate,
                NET_REVENUE_INTEREST = terms.NetRevenueInterest,
                OVERRIDING_ROYALTY = terms.OverridingRoyalty,
                BURDENS_TEXT = FormatBurdens(terms.Burdens),
                RATIONALE = renewal.Rationale.Trim(),
                ACTOR = renewal.Actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = renewal.Actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = renewal.Actor.Trim(),
                ROW_CHANGED_DATE = now,
            };

            if (!await AppendTermAsync(source, opportunity, term, model, renewal.Actor.Trim(), now, cancellationToken)
                    .ConfigureAwait(false))
            {
                return OgResult<OgLeaseFinancialDetail>.Failed(
                    "The renewal was not acknowledged; retry sends the same renewal.", CodeWriteFailed);
            }

            // The term is already committed; a missing lifecycle event is reported as degraded, not as a failed
            // renewal that a retry would append a second term for.
            await AppendLifecycleAsync(
                source,
                opportunity.LEASE_ACQUISITION_ID!,
                OgLeaseLifecycleEntry.Renewed,
                renewal.NewExpiry.Date,
                renewal.Rationale,
                renewal.Actor,
                now,
                cancellationToken).ConfigureAwait(false);

            return await DetailAfterWriteAsync(source, opportunity.LEASE_ACQUISITION_ID!, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"renewing lease {renewal.LeaseId}",
                "the renewal was not applied; the original is retained for retry",
                FailureSeverity.Error);

            return OgResult<OgLeaseFinancialDetail>.Failed(
                "The lease could not be renewed; retry sends the same renewal.", reference ?? CodeWriteFailed);
        }
    }

    private async Task<OgResult<OgLeaseFinancialDetail>> RecordLifecycleAsync(
        string leaseId,
        string @event,
        DateTime effective,
        string rationale,
        string actor,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "A lifecycle change must record who made it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(rationale))
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "A lifecycle change needs a rationale.", CodeRationaleRequired);
        }

        if (effective == default)
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "A lifecycle change needs its effective date.", CodeExpiryRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgLeaseFinancialDetail>.Failed(
                "No connection is open, so the lease cannot be changed.", CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;

            var recorded = await AppendLifecycleAsync(source, leaseId.Trim(), @event, effective.Date, rationale,
                actor, now, cancellationToken).ConfigureAwait(false);

            return recorded
                ? await DetailAfterWriteAsync(source, leaseId.Trim(), cancellationToken).ConfigureAwait(false)
                : OgResult<OgLeaseFinancialDetail>.Failed(
                    "The lifecycle change was not recorded; the register is unchanged.", CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"recording the { @event.ToLowerInvariant() } of lease {leaseId}",
                "the lifecycle change was not applied",
                FailureSeverity.Error);

            return OgResult<OgLeaseFinancialDetail>.Failed(
                "The lifecycle change could not be applied.", reference ?? CodeWriteFailed);
        }
    }

    /// <summary>
    /// Appends the term, deactivates the previous active terms (never deletes them) and updates the acquisition
    /// row's current financial columns. One unit of work for the term table, one for the row.
    /// </summary>
    private async Task<bool> AppendTermAsync(
        IDataSource source,
        LEASE_ACQUISITION opportunity,
        LEASE_TERM term,
        FinancialModel model,
        string actor,
        DateTime now,
        CancellationToken cancellationToken)
    {
        // The new term is the critical record: write it first, so a failure leaves the previous term untouched and
        // a retry cannot duplicate a half-applied renewal.
        var termsUnit = OgUow.For<LEASE_TERM>(source);
        termsUnit.PrimaryKey = "TERM_ID";
        termsUnit.Add(term);

        var committedTerms = await termsUnit.Commit().ConfigureAwait(false);

        if (!OgUow.IsOk(committedTerms))
        {
            return false;
        }

        // The old term stays in the table; only its active marker changes, so the history is retained (AC-4).
        var previous = model.Terms
            .Where(existing => string.Equals(existing.LEASE_ID, opportunity.LEASE_ACQUISITION_ID, StringComparison.OrdinalIgnoreCase)
                               && !string.Equals(existing.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (previous.Count > 0)
        {
            var deactivateUnit = OgUow.For<LEASE_TERM>(source);
            deactivateUnit.PrimaryKey = "TERM_ID";

            var loaded = await OgUow.ReadAsync(deactivateUnit,
                [OgUow.Equal("LEASE_ID", opportunity.LEASE_ACQUISITION_ID)],
                cancellationToken).ConfigureAwait(false);

            foreach (var row in loaded.Where(existing =>
                         !string.Equals(existing.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(existing.TERM_ID, term.TERM_ID, StringComparison.OrdinalIgnoreCase)))
            {
                row.ACTIVE_IND = "N";
                row.ROW_CHANGED_BY = actor;
                row.ROW_CHANGED_DATE = now;
                deactivateUnit.Update(row);
            }

            var deactivated = await deactivateUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(deactivated))
            {
                _failures.ReportHandled(
                    new InvalidOperationException(
                        $"The previous term of {opportunity.LEASE_ACQUISITION_ID} was not deactivated: " +
                        OgUow.Message(deactivated, "the row was not updated")),
                    $"retaining the previous term of {opportunity.LEASE_ACQUISITION_ID}",
                    "the new term is recorded but the previous one still carries the active marker",
                    FailureSeverity.Degraded);
            }
        }

        var rowUnit = OgUow.For<LEASE_ACQUISITION>(source);
        var rows = await OgUow.ReadAsync(rowUnit,
            [OgUow.Equal("LEASE_ACQUISITION_ID", opportunity.LEASE_ACQUISITION_ID)],
            cancellationToken).ConfigureAwait(false);
        var current = rows.FirstOrDefault();

        if (current is null)
        {
            return true;
        }

        current.BONUS_AMOUNT = term.BONUS_AMOUNT;
        current.BONUS_AMOUNT_OUOM = term.BONUS_CURRENCY;
        current.ROYALTY_RATE = term.ROYALTY_RATE;
        current.NET_REVENUE_INTEREST = term.NET_REVENUE_INTEREST;
        current.LEASE_EXPIRATION = term.EXPIRY_DATE;
        current.ROW_CHANGED_BY = actor;
        current.ROW_CHANGED_DATE = now;

        var updatedRow = rowUnit.Update(current);

        if (!OgUow.IsOk(updatedRow))
        {
            _failures.ReportHandled(
                new InvalidOperationException(
                    $"The acquisition row of {opportunity.LEASE_ACQUISITION_ID} was not updated: " +
                    OgUow.Message(updatedRow, "the row was not updated")),
                $"applying the term to {opportunity.LEASE_ACQUISITION_ID}",
                "the term is recorded but the acquisition row still shows the previous values",
                FailureSeverity.Degraded);

            return true;
        }

        var committedRow = await rowUnit.Commit().ConfigureAwait(false);

        if (!OgUow.IsOk(committedRow))
        {
            _failures.ReportHandled(
                new InvalidOperationException(
                    $"The acquisition row of {opportunity.LEASE_ACQUISITION_ID} was not committed: " +
                    OgUow.Message(committedRow, "the row was not updated")),
                $"applying the term to {opportunity.LEASE_ACQUISITION_ID}",
                "the term is recorded but the acquisition row still shows the previous values",
                FailureSeverity.Degraded);
        }

        return true;
    }

    private async Task<bool> AppendLifecycleAsync(
        IDataSource source,
        string leaseId,
        string @event,
        DateTime effective,
        string rationale,
        string actor,
        DateTime now,
        CancellationToken cancellationToken)
    {
        try
        {
            var unit = OgUow.For<LEASE_LIFECYCLE>(source);

            unit.Add(new LEASE_LIFECYCLE
            {
                EVENT_ID = Guid.NewGuid().ToString("N"),
                LEASE_ID = leaseId,
                EVENT = @event,
                EFFECTIVE_DATE = effective,
                RATIONALE = rationale.Trim(),
                ACTOR = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (OgUow.IsOk(committed))
            {
                return true;
            }

            _failures.ReportHandled(
                new InvalidOperationException(
                    $"The lifecycle event for {leaseId} was not written: " +
                    OgUow.Message(committed, "the row was not inserted")),
                $"recording the lifecycle event of {leaseId}",
                "the financial state stands but its lifecycle event is missing",
                FailureSeverity.Degraded);

            return false;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            _failures.ReportHandled(
                ex,
                $"recording the lifecycle event of {leaseId}",
                "the financial state stands but its lifecycle event is missing",
                FailureSeverity.Degraded);

            return false;
        }
    }

    private async Task<OgResult<OgLeaseFinancialDetail>> DetailAfterWriteAsync(
        IDataSource source,
        string leaseId,
        CancellationToken cancellationToken)
    {
        var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
        var detail = Detail(model, leaseId, DateTime.Today);

        return detail is null
            ? OgResult<OgLeaseFinancialDetail>.Failed(
                $"Lease '{leaseId}' is not in this database.", CodeUnknown)
            : OgResult<OgLeaseFinancialDetail>.Ok(detail);
    }

    private static IReadOnlyList<OgLeaseBurden> ParseBurdens(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var burdens = new List<OgLeaseBurden>();

        foreach (var part in text.Split([';', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = part.IndexOf('=');

            if (separator <= 0)
            {
                continue;
            }

            var name = part[..separator].Trim();
            var value = part[(separator + 1)..].Trim();

            if (name.Length > 0
                && decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var fraction))
            {
                burdens.Add(new OgLeaseBurden(name, fraction));
            }
        }

        return burdens;
    }

    private static string? FormatBurdens(IReadOnlyList<OgLeaseBurden> burdens) =>
        burdens.Count == 0
            ? null
            : string.Join("; ", burdens.Select(burden =>
                $"{burden.Name}={burden.Fraction.ToString("0.########", CultureInfo.InvariantCulture)}"));

    private static string BucketLabel(int days) =>
        days <= 30 ? "0-30 days" : days <= 60 ? "31-60 days" : "61-90 days";

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
         && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record FinancialModel(
        IReadOnlyList<LEASE_ACQUISITION> Opportunities,
        IReadOnlyList<LEASE_TERM> Terms,
        IReadOnlyList<LEASE_OBLIGATION> Obligations,
        IReadOnlyList<LEASE_PAYMENT> Payments,
        IReadOnlyList<LEASE_LIFECYCLE> Lifecycle);
}
