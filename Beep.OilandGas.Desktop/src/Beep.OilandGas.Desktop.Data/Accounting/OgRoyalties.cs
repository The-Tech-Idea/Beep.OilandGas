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

/// <summary>Where a royalty or a joint-interest bill posts (ACC-07): the company and the book.</summary>
public sealed record OgRoyaltyContext(string CompanyId, string BookCode)
{
    public static OgRoyaltyContext FromSession(OgSession session) =>
        new(
            string.IsNullOrWhiteSpace(session.CompanyId) ? "APP" : session.CompanyId,
            string.IsNullOrWhiteSpace(session.BookCode) ? "MAIN" : session.BookCode);
}

/// <summary>The deduction components of a royalty (ACC-07).</summary>
public sealed record OgRoyaltyDeductions(
    decimal Transportation,
    decimal AdValorem,
    decimal Severance,
    decimal Processing,
    decimal Other)
{
    public decimal Total => RoyaltyMath.Round(Transportation + AdValorem + Severance + Processing + Other);

    public string Describe() =>
        "transport " + Money(Transportation) + " · ad valorem " + Money(AdValorem) +
        " · severance " + Money(Severance) + " · processing " + Money(Processing) +
        " · other " + Money(Other) + " · total " + Money(Total);

    private static string Money(decimal value) => MoneyText.Of(value);
}

/// <summary>The inputs of a royalty calculation (ACC-07).</summary>
public sealed record OgRoyaltyInputs(
    string? LeaseId,
    string? OwnerId,
    string? AllocationDetailId,
    string? AllocationResultId,
    decimal? GrossVolume,
    string? VolumeUnit,
    decimal? PricePerUnit,
    decimal? RatePercent,
    decimal? DeclaredResidualPercent,
    string? Currency,
    OgRoyaltyDeductions Deductions);

/// <summary>One royalty calculation (ACC-07), matching the plan's <c>OgRoyalty</c> plus its dispute and posting.</summary>
public sealed record OgRoyalty(
    string Id,
    string LeaseId,
    string? OwnerId,
    DateTime Date,
    decimal Volume,
    string VolumeUnit,
    decimal PricePerUnit,
    decimal GrossValue,
    OgRoyaltyDeductions Deductions,
    decimal NetRevenue,
    decimal RatePercent,
    decimal Amount,
    string Status,
    string? Currency,
    bool Disputed,
    string? DisputeReference,
    string? DisputeReason,
    string? DisputedBy,
    DateTime? DisputedAt,
    string? DisputeResolvedBy,
    DateTime? DisputeResolvedAt,
    string? Author,
    string? JournalId,
    string? PaymentJournalId,
    DateTime? PaidAt,
    string? PaidBy,
    decimal? PaidAmount,
    string? AllocationDetailId,
    string? AllocationResultId)
{
    /// <summary>AC-1: the amount is the net revenue times the rate, exactly, in decimal.</summary>
    public bool NetsAsStated =>
        Amount == RoyaltyMath.Round(NetRevenue * (RatePercent / 100m));

    public bool IsPaid => string.Equals(Status, RoyaltyStatus.Paid, StringComparison.OrdinalIgnoreCase);

    public bool IsDisputed => Disputed;

    /// <summary>AC-3: a disputed calculation cannot be paid, and the sentence carries the dispute reference.</summary>
    public string PayableText => IsPaid
        ? "paid by " + (PaidBy ?? "(unknown)") + " on " +
          (PaidAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date") +
          (string.IsNullOrWhiteSpace(PaymentJournalId) ? string.Empty : " · journal " + PaymentJournalId)
        : IsDisputed
            ? "BLOCKED by dispute " + (DisputeReference ?? "(no reference)") + " - " + (DisputeReason ?? "no reason")
            : "payable";

    public bool CanPay => !IsPaid && !IsDisputed;

    public string AmountText => MoneyText.Of(Amount) + " " + (Currency ?? string.Empty);

    /// <summary>AC-6: the volume, its unit and the allocation behind it, said on the calculation itself.</summary>
    public string TraceText =>
        "volume " + Volume.ToString("#,0.####", CultureInfo.InvariantCulture) + " " + VolumeUnit +
        " at " + MoneyText.Of(PricePerUnit) + " · gross " + MoneyText.Of(GrossValue) +
        (string.IsNullOrWhiteSpace(AllocationDetailId) ? " · no allocation detail" : " · allocation " + AllocationDetailId) +
        (string.IsNullOrWhiteSpace(AllocationResultId) ? string.Empty : " of result " + AllocationResultId);

    public string Describe() =>
        $"{LeaseId} · {OwnerId ?? "(no owner)"} · {AmountText} · {Status.ToLowerInvariant()}" +
        (IsDisputed ? " · disputed" : string.Empty);
}

/// <summary>One line of a joint-interest bill (ACC-07).</summary>
public sealed record OgJibLine(string Kind, string Description, string? Category, decimal Amount)
{
    public string Describe() =>
        Kind + " · " + Description + " · " + MoneyText.Of(Amount) +
        (string.IsNullOrWhiteSpace(Category) ? string.Empty : " · " + Category);
}

/// <summary>One joint-interest statement (ACC-07), matching the plan's <c>OgJibStatement</c>.</summary>
public sealed record OgJib(
    string Id,
    string PartnerId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal Charges,
    decimal Credits,
    decimal Net,
    string Currency,
    string State,
    string? Operator,
    string? Author,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    string? PostedBy,
    DateTime? PostedAt,
    string? JournalId,
    string? SourceVolumeReference,
    string? ReturnReason,
    IReadOnlyList<OgJibLine> Lines)
{
    /// <summary>AC-4: the net is the charges less the credits, computed from the lines that were written.</summary>
    public bool NetsAsStated => Net == JibMath.Net(
        Lines.Where(line => line.Kind == "CHARGE").Select(line => line.Amount),
        Lines.Where(line => line.Kind == "CREDIT").Select(line => line.Amount));

    public bool IsPosted => string.Equals(State, JibStatus.Posted, StringComparison.OrdinalIgnoreCase);

    public bool IsDraft => string.Equals(State, JibStatus.Draft, StringComparison.OrdinalIgnoreCase);

    public bool IsApproved => string.Equals(State, JibStatus.Approved, StringComparison.OrdinalIgnoreCase);

    public bool HasJournal => !string.IsNullOrWhiteSpace(JournalId);

    public string NetText => MoneyText.Of(Net) + " " + Currency;

    public string TotalsText =>
        "charges " + MoneyText.Of(Charges) + " · credits " + MoneyText.Of(Credits) + " · net " + NetText;

    /// <summary>AC-6: the production the bill came from.</summary>
    public string TraceText => string.IsNullOrWhiteSpace(SourceVolumeReference)
        ? "NOT traceable - no source volume reference"
        : "traceable to " + SourceVolumeReference;

    public string JournalText => HasJournal
        ? "journal " + JournalId + " posted on " +
          (PostedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date")
        : "not posted - no journal yet";

    public string Describe() =>
        $"{PartnerId} · {PeriodStart:yyyy-MM-dd} to {PeriodEnd:yyyy-MM-dd} · {NetText} · {State.ToLowerInvariant()}";
}

/// <summary>One allocated share as the allocation tab shows it (ACC-07 AC-5).</summary>
public sealed record OgAllocationRow(
    string TargetId,
    string TargetType,
    decimal Percentage,
    decimal Amount,
    decimal Basis)
{
    public string Describe() =>
        TargetType + " " + TargetId + " · " + Percentage.ToString("#,0.####", CultureInfo.InvariantCulture) +
        " % · " + MoneyText.Of(Amount);
}

/// <summary>The allocation preview: the pool, the shares, and whether they add back to the pool (AC-5).</summary>
public sealed record OgAllocationPreview(
    decimal Pool,
    string Currency,
    IReadOnlyList<OgAllocationRow> Shares)
{
    public decimal Allocated => Shares.Sum(share => share.Amount);

    public decimal Difference => Pool - Allocated;

    /// <summary>AC-5's precondition: the shares must add back to the pool exactly.</summary>
    public bool AddsBack => Difference == 0m;

    public string TotalsText => "pool " + MoneyText.Of(Pool) + " " + Currency +
                               " · allocated " + MoneyText.Of(Allocated) +
                               (AddsBack ? " · adds back exactly" : " · SHORT by " + MoneyText.Of(Difference));

    public string RowsText => Shares.Count == 0
        ? "no target"
        : string.Join(Environment.NewLine, Shares.Select(share => share.Describe()));
}

/// <summary>The royalties and JIB workspace (ACC-07).</summary>
public sealed record OgRoyaltyWorkspace(
    IReadOnlyList<OgRoyalty> Royalties,
    OgRoyalty? SelectedRoyalty,
    IReadOnlyList<OgJib> Jibs,
    OgJib? SelectedJib,
    IReadOnlyList<OgAllocationRow> CommittedAllocation,
    string? CommittedAllocationId,
    DateTime? CommittedAllocationDate,
    IReadOnlyList<string> RoyaltyStates,
    IReadOnlyList<string> JibStates,
    IReadOnlyList<string> AllocationMethods,
    IReadOnlyList<string> TargetTypes,
    IReadOnlyList<string> Currencies,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string RoyaltiesText => Royalties.Count == 0
        ? "No royalty is calculated."
        : string.Join(Environment.NewLine, Royalties.Select(royalty => royalty.Describe()));

    public string JibsText => Jibs.Count == 0
        ? "No joint-interest statement exists."
        : string.Join(Environment.NewLine, Jibs.Select(jib => jib.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the royalties and bills could not be read; nothing can be calculated or paid"
        : $"{Royalties.Count.ToString(CultureInfo.InvariantCulture)} royalty calculation(s) · " +
          $"{Royalties.Count(royalty => royalty.CanPay).ToString(CultureInfo.InvariantCulture)} payable · " +
          $"{Royalties.Count(royalty => royalty.IsDisputed).ToString(CultureInfo.InvariantCulture)} disputed · " +
          $"{Jibs.Count.ToString(CultureInfo.InvariantCulture)} JIB statement(s)";

    public decimal Payable => Royalties.Where(royalty => royalty.CanPay).Sum(royalty => royalty.Amount);

    public string PayableText => "payable royalties " + MoneyText.Of(Payable);

    public string CommittedAllocationText => CommittedAllocation.Count == 0
        ? "nothing is allocated for this pool yet"
        : string.Join(Environment.NewLine, CommittedAllocation.Select(share => share.Describe())) +
          Environment.NewLine + "total " + MoneyText.Of(CommittedAllocation.Sum(share => share.Amount));

    public string ReferenceText =>
        $"royalty states {string.Join(", ", RoyaltyStates)} · JIB states {string.Join(", ", JibStates)} · " +
        $"allocation methods from the owner's AllocationMethod · postings to the configured chart";
}

/// <summary>The royalty and JIB rules, stated once so the page, the service and the tests agree.</summary>
public static class RoyaltyRules
{
    public const string CodeNoConnection = "royalty-no-connection";
    public const string CodeReadFailed = "royalty-read-failed";
    public const string CodeWriteFailed = "royalty-write-failed";
    public const string CodeActorRequired = "royalty-actor-required";
    public const string CodeInputRequired = "royalty-input-required";
    public const string CodeInputInvalid = "royalty-input-invalid";
    public const string CodeCurrencyMismatch = "royalty-currency-mismatch";
    public const string CodeUnknownRoyalty = "royalty-unknown";
    public const string CodeUnknownJib = "royalty-unknown-jib";
    public const string CodeOwnershipMissing = "royalty-ownership-missing";
    public const string CodeOwnershipNotSumming = "royalty-ownership-not-summing";
    public const string CodeOwnershipNegative = "royalty-ownership-negative";
    public const string CodeDisputed = "royalty-disputed";
    public const string CodeAlreadyPaid = "royalty-already-paid";
    public const string CodeAuthorCannotAct = "royalty-author-cannot-act";
    public const string CodeNotApproved = "royalty-jib-not-approved";
    public const string CodeAlreadyPosted = "royalty-jib-already-posted";
    public const string CodeReturnReasonRequired = "royalty-jib-return-reason-required";
    public const string CodeSourceVolumeRequired = "royalty-jib-source-volume-required";
    public const string CodePeriodClosed = "royalty-period-closed";
    public const string CodeUnknownAccount = "royalty-unknown-account";
    public const string CodeAllocationEmpty = "royalty-allocation-empty";
    public const string CodeDisputeReasonRequired = "royalty-dispute-reason-required";
    public const string CodeDisputeAlreadyOpen = "royalty-dispute-already-open";

    public static IReadOnlyList<string> Currencies { get; } = ["USD", "EUR", "GBP", "SAR", "AED"];

    public static IReadOnlyList<string> RoyaltyStates { get; } =
        [RoyaltyStatus.Calculated, RoyaltyStatus.Accrued, RoyaltyStatus.Paid, RoyaltyStatus.Disputed];

    public static IReadOnlyList<string> JibStates => JibStatus.States;

    public static IReadOnlyList<string> AllocationMethods => RoyaltyJibCodes.AllocationMethods;

    public static IReadOnlyList<string> TargetTypes => RoyaltyJibCodes.TargetTypes;

    public static IReadOnlyList<string> VolumeUnits { get; } = ["bbl", "Mscf", "boe"];

    public static IReadOnlyList<string> Missing(OgRoyaltyInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(inputs.LeaseId))
        {
            missing.Add("lease");
        }

        if (inputs.GrossVolume is null or <= 0m)
        {
            missing.Add("a gross volume above zero");
        }

        if (string.IsNullOrWhiteSpace(inputs.VolumeUnit))
        {
            missing.Add("volume unit");
        }

        if (inputs.PricePerUnit is null or < 0m)
        {
            missing.Add("a price per unit");
        }

        if (inputs.RatePercent is null)
        {
            missing.Add("royalty rate");
        }

        if (string.IsNullOrWhiteSpace(inputs.Currency))
        {
            missing.Add("currency");
        }

        return missing;
    }

    public static (string Code, string Message)? Validate(OgRoyaltyInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            return (CodeInputRequired, $"The royalty needs {string.Join(", ", missing)}.");
        }

        if (!Currencies.Contains(inputs.Currency!.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (CodeCurrencyMismatch,
                $"'{inputs.Currency}' is not a currency this app calculates royalties in; use " +
                $"{string.Join(", ", Currencies)}.");
        }

        if (inputs.RatePercent is < 0m or > 100m)
        {
            return (CodeInputInvalid,
                "The royalty rate is in percentage points between 0 and 100; 12.5 % is entered as 12.5.");
        }

        var gross = RoyaltyMath.GrossValue(inputs.GrossVolume!.Value, inputs.PricePerUnit!.Value);

        return inputs.Deductions.Total > gross
            ? (CodeInputInvalid,
                $"The deductions {MoneyText.Of(inputs.Deductions.Total)} exceed the gross value " +
                $"{MoneyText.Of(gross)}, so there is no net revenue to take a royalty on.")
            : null;
    }

    /// <summary>AC-4's precondition: a statement needs at least one charge or credit to be a statement.</summary>
    public static (string Code, string Message)? ValidateJib(
        string? partnerId,
        string? sourceVolumeReference,
        IReadOnlyList<OgJibLine> lines,
        string? currency)
    {
        ArgumentNullException.ThrowIfNull(lines);

        if (string.IsNullOrWhiteSpace(partnerId))
        {
            return (CodeInputRequired, "A joint-interest statement needs the partner it bills.");
        }

        // AC-6: a bill with no source volume cannot be followed back to the production that produced it.
        if (string.IsNullOrWhiteSpace(sourceVolumeReference))
        {
            return (CodeSourceVolumeRequired,
                "A joint-interest statement must name the source volume it bills against - an allocation detail, a run " +
                "ticket or the period volume - or the charge cannot be proved.");
        }

        if (lines.Count == 0)
        {
            return (CodeInputRequired, "A joint-interest statement needs at least one charge or credit.");
        }

        if (lines.Any(line => line.Amount < 0m))
        {
            return (CodeInputInvalid,
                "A charge or a credit is a positive amount; put a reduction on the credit side rather than a negative " +
                "charge.");
        }

        return string.IsNullOrWhiteSpace(currency) || !Currencies.Contains(currency.Trim().ToUpperInvariant(),
            StringComparer.Ordinal)
            ? (CodeCurrencyMismatch,
                $"'{currency}' is not a currency this app bills in; use {string.Join(", ", Currencies)}.")
            : null;
    }
}

/// <summary>ACC-07: royalties, joint-interest billing, cost allocation and the postings behind them.</summary>
public interface IOgRoyalties
{
    /// <summary>Calculates a royalty: ownership gate, exact arithmetic, accrual journal (AC-1, AC-2, AC-6).</summary>
    Task<OgResult<OgRoyaltyWorkspace>> CalculateAsync(
        OgRoyaltyInputs inputs,
        OgRoyaltyContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Pays an accrued royalty, refusing a disputed one with its reference (AC-3).</summary>
    Task<OgResult<OgRoyaltyWorkspace>> PayAsync(
        string royaltyId,
        OgRoyaltyContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Raises a dispute with a reference, which blocks payment until it is resolved.</summary>
    Task<OgResult<OgRoyaltyWorkspace>> DisputeAsync(
        string royaltyId,
        string reason,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Resolves the dispute, so the royalty may be paid.</summary>
    Task<OgResult<OgRoyaltyWorkspace>> ResolveDisputeAsync(
        string royaltyId,
        string note,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Raises a joint-interest statement from charges and credits (AC-4, AC-6).</summary>
    Task<OgResult<OgRoyaltyWorkspace>> RaiseJibAsync(
        string partnerId,
        DateTime periodStart,
        DateTime periodEnd,
        string? sourceVolumeReference,
        string? currency,
        IReadOnlyList<OgJibLine> lines,
        OgRoyaltyContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Approves or returns a statement; the author may not approve it.</summary>
    Task<OgResult<OgRoyaltyWorkspace>> ReviewJibAsync(
        string jibId,
        bool approve,
        string reason,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Posts the net to the configured accounts, traceable to its source volume (SG-10-7).</summary>
    Task<OgResult<OgRoyaltyWorkspace>> PostJibAsync(
        string jibId,
        OgRoyaltyContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The allocation preview - the same arithmetic the commit uses (AC-5). Nothing is written.</summary>
    OgResult<OgAllocationPreview> PreviewAllocation(
        decimal pool,
        string currency,
        IReadOnlyList<CostAllocationMath.AllocationTarget> targets,
        string method);

    /// <summary>Commits the allocation, writing the same shares the preview showed (AC-5).</summary>
    Task<OgResult<OgRoyaltyWorkspace>> CommitAllocationAsync(
        decimal pool,
        string currency,
        IReadOnlyList<CostAllocationMath.AllocationTarget> targets,
        string method,
        string reference,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The calculations, the statements and the committed allocation; an unavailable register is a state.</summary>
    Task<OgResult<OgRoyaltyWorkspace>> LoadAsync(
        OgRoyaltyContext context,
        string? royaltyId = null,
        string? jibId = null,
        string? allocationReference = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ACC-07 over ProductionAccounting's royalty, joint-interest and cost-allocation tables and the ledger, through units of
/// work.
/// </summary>
/// <remarks>
/// The arithmetic is the owner's, taken from <c>RoyaltyService</c>'s documented relation (net revenue x rate, the rate in
/// percentage points) and stated once in <see cref="RoyaltyMath"/> so the calculation, the payment and the test all use the
/// same code. Every posting resolves its accounts from the configured chart and refuses naming the account when it is
/// missing, and every journal reference is derived from the row that produced it, so a retry adopts its entry. A royalty
/// carries the volume, its unit and the allocation it came from (AC-6); a joint-interest statement carries the source volume
/// reference it bills against.
/// </remarks>
public sealed class OgRoyaltiesService : IOgRoyalties
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgRoyaltiesService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    // ------------------------------------------------------------------ royalty

    public async Task<OgResult<OgRoyaltyWorkspace>> CalculateAsync(
        OgRoyaltyInputs inputs,
        OgRoyaltyContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "A royalty calculation must record who made it.", RoyaltyRules.CodeActorRequired);
        }

        if (RoyaltyRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "No connection is open, so the royalty cannot be calculated.", RoyaltyRules.CodeNoConnection);
        }

        try
        {
            var leaseId = inputs.LeaseId!.Trim();
            var interests = await OgUow.ReadAllAsync<OWNERSHIP_INTEREST>(source).ConfigureAwait(false);
            var fractions = interests
                .Where(row => string.Equals(row.PROPERTY_OR_LEASE_ID?.Trim(), leaseId,
                    StringComparison.OrdinalIgnoreCase))
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(row => row.EFFECTIVE_START_DATE is null || row.EFFECTIVE_START_DATE <= DateTime.UtcNow.Date)
                .Where(row => row.EFFECTIVE_END_DATE is null || row.EFFECTIVE_END_DATE >= DateTime.UtcNow.Date)
                .Select(row => (OwnerId: row.OWNER_ID ?? string.Empty, FractionPercent: row.NET_REVENUE_INTEREST ?? 0m))
                .ToList();

            // AC-2: the fractions must exist and add to 100 % (or 100 % with the declared residual).
            if (OwnershipFractions.Validate(leaseId, fractions, inputs.DeclaredResidualPercent ?? 0m) is { } gate)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(gate.Message, gate.Code);
            }

            var gross = RoyaltyMath.GrossValue(inputs.GrossVolume!.Value, inputs.PricePerUnit!.Value);
            var deductions = inputs.Deductions;
            var answer = RoyaltyMath.Calculate(gross, deductions.Total, inputs.RatePercent!.Value);

            var periodCode = LedgerCodes.PeriodCodeOf(DateTime.UtcNow);
            var periods = await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);
            var period = periods.FirstOrDefault(row =>
                string.Equals(row.PERIOD_CODE, periodCode, StringComparison.OrdinalIgnoreCase));

            if (period is not null && !LedgerCodes.IsOpen(period.STATUS))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"Period {periodCode} is {period.STATUS.ToLowerInvariant()}, so a royalty cannot be accrued into it.",
                    RoyaltyRules.CodePeriodClosed);
            }

            // The deductions are their own record, so the total on the calculation is never an unexplained number.
            var deductionId = Guid.NewGuid().ToString("N");

            var deductionRow = OgUow.For<ROYALTY_DEDUCTIONS>(source);
            deductionRow.PrimaryKey = "ROYALTY_DEDUCTIONS_ID";

            deductionRow.Add(new ROYALTY_DEDUCTIONS
            {
                ROYALTY_DEDUCTIONS_ID = deductionId,
                TRANSPORTATION_COSTS = deductions.Transportation,
                PRODUCTION_TAXES = deductions.AdValorem + deductions.Severance,
                PROCESSING_COSTS = deductions.Processing,
                MARKETING_COSTS = 0m,
                OTHER_DEDUCTIONS = deductions.Other,
                TOTAL_DEDUCTIONS = deductions.Total,
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = DateTime.UtcNow,
            });

            var deductionCommitted = await deductionRow.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(deductionCommitted))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    OgUow.Message(deductionCommitted, "The royalty deductions were not saved."),
                    RoyaltyRules.CodeWriteFailed);
            }

            var now = DateTime.UtcNow;
            var royaltyId = Guid.NewGuid().ToString("N");

            // SG-10-7: the accrual goes to the configured accounts - royalty expense against the accrued liability.
            var journalId = await PostJournalAsync(
                source,
                "ROYALTY-" + royaltyId,
                now.Date,
                RoyaltyJibCodes.RoyaltyExpenseAccount,
                RoyaltyJibCodes.AccruedRoyaltyAccount,
                answer.Amount,
                "royalty accrued for " + leaseId,
                actor,
                cancellationToken).ConfigureAwait(false);

            if (journalId is null)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"The royalty's accrual journal was not written: the chart of accounts must carry " +
                    $"{RoyaltyJibCodes.RoyaltyExpenseAccount} and {RoyaltyJibCodes.AccruedRoyaltyAccount}. " +
                    "Nothing was accrued.",
                    RoyaltyRules.CodeUnknownAccount);
            }

            var unit = OgUow.For<ROYALTY_CALCULATION>(source);
            unit.PrimaryKey = "ROYALTY_CALCULATION_ID";

            unit.Add(new ROYALTY_CALCULATION
            {
                ROYALTY_CALCULATION_ID = royaltyId,
                CALCULATION_DATE = now.Date,
                PROPERTY_OR_LEASE_ID = leaseId,
                ROYALTY_OWNER_ID = string.IsNullOrWhiteSpace(inputs.OwnerId) ? null : inputs.OwnerId.Trim(),
                ROYALTY_INTEREST = inputs.RatePercent.Value,
                GROSS_REVENUE = gross,
                ROYALTY_DEDUCTIONS_ID = deductionId,
                NET_REVENUE = answer.NetRevenue,
                ROYALTY_AMOUNT = answer.Amount,
                ROYALTY_STATUS = RoyaltyStatus.Accrued,
                PAYMENT_STATUS = "UNPAID",
                ROYALTY_TYPE = "MINERAL",
                ROYALTY_BASIS = "NET_REVENUE",
                FLUID_TYPE = inputs.VolumeUnit!.Trim().ToUpperInvariant(),
                PRODUCTION_PERIOD_START = new DateTime(now.Year, now.Month, 1),
                // AC-6 and SG-10-7: the volume and the allocation behind it travel with the calculation.
                GROSS_VOLUME = inputs.GrossVolume.Value,
                GROSS_VOLUME_OUOM = inputs.VolumeUnit.Trim(),
                PRICE_PER_UNIT = inputs.PricePerUnit.Value,
                TRANSPORTATION_COST = deductions.Transportation,
                AD_VALOREM_TAX = deductions.AdValorem,
                SEVERANCE_TAX = deductions.Severance,
                ALLOCATION_DETAIL_ID = string.IsNullOrWhiteSpace(inputs.AllocationDetailId)
                    ? null
                    : inputs.AllocationDetailId.Trim(),
                ALLOCATION_RESULT_ID = string.IsNullOrWhiteSpace(inputs.AllocationResultId)
                    ? null
                    : inputs.AllocationResultId.Trim(),
                JOURNAL_ENTRY_ID = journalId,
                AUTHOR = actor.Trim(),
                CURRENCY_CODE = inputs.Currency!.Trim().ToUpperInvariant(),
                DISPUTED_IND = "N",
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
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    OgUow.Message(committed, "The royalty calculation was not saved."),
                    RoyaltyRules.CodeWriteFailed);
            }

            var workspace = await ReadAsync(source, context, royaltyId, null, null, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgRoyaltyWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"calculating a royalty for {inputs.LeaseId}", "the royalty was not calculated",
                FailureSeverity.Error);

            return OgResult<OgRoyaltyWorkspace>.Failed(
                "The royalty could not be calculated.", reference ?? RoyaltyRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgRoyaltyWorkspace>> PayAsync(
        string royaltyId,
        OgRoyaltyContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "A payment must record who made it.", RoyaltyRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "No connection is open, so the royalty cannot be paid.", RoyaltyRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, royaltyId, null, null, cancellationToken)
                .ConfigureAwait(false);
            var royalty = workspace.SelectedRoyalty;

            if (royalty is null)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"Royalty '{royaltyId}' is not in this database.", RoyaltyRules.CodeUnknownRoyalty);
            }

            if (royalty.IsPaid)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"{royalty.Describe()} is already paid; a royalty is paid once.", RoyaltyRules.CodeAlreadyPaid);
            }

            // AC-3: a disputed royalty is blocked, and the refusal carries the dispute reference.
            if (royalty.IsDisputed)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"{royalty.LeaseId} cannot be paid: dispute {royalty.DisputeReference ?? "(no reference)"} is open - " +
                    (royalty.DisputeReason ?? "no reason recorded") + ". Resolve the dispute first.",
                    RoyaltyRules.CodeDisputed);
            }

            // The pay command's segregation of duties: whoever calculated it does not pay it.
            if (!string.IsNullOrWhiteSpace(royalty.Author)
                && string.Equals(royalty.Author.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"{royalty.LeaseId} was calculated by {royalty.Author}, who therefore cannot pay it. A different " +
                    "controller must release the payment.",
                    RoyaltyRules.CodeAuthorCannotAct);
            }

            var now = DateTime.UtcNow;

            // SG-10-7: the payment clears the accrued liability into cash, at the configured accounts.
            var journalId = await PostJournalAsync(
                source,
                "ROYALTY-PAY-" + royalty.Id,
                now.Date,
                RoyaltyJibCodes.AccruedRoyaltyAccount,
                RoyaltyJibCodes.CashAccount,
                royalty.Amount,
                "royalty paid for " + royalty.LeaseId,
                actor,
                cancellationToken).ConfigureAwait(false);

            if (journalId is null)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"The payment journal was not written: the chart of accounts must carry " +
                    $"{RoyaltyJibCodes.AccruedRoyaltyAccount} and {RoyaltyJibCodes.CashAccount}. Nothing was paid.",
                    RoyaltyRules.CodeUnknownAccount);
            }

            var unit = OgUow.For<ROYALTY_CALCULATION>(source);
            unit.PrimaryKey = "ROYALTY_CALCULATION_ID";

            var rows = await OgUow.ReadAsync(
                unit, [OgUow.Equal("ROYALTY_CALCULATION_ID", royalty.Id)], cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"Royalty '{royaltyId}' is not in this database.", RoyaltyRules.CodeUnknownRoyalty);
            }

            row.ROYALTY_STATUS = RoyaltyStatus.Paid;
            row.PAYMENT_STATUS = "PAID";
            // The payment is the journal plus this state: ROYALTY_PAYMENT declares STATUS and Status, so it cannot be mapped.
            row.PAID_AT = now;
            row.PAID_BY = actor.Trim();
            row.PAID_AMOUNT = royalty.Amount;
            row.PAYMENT_JOURNAL_ENTRY_ID = journalId;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = now;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    OgUow.Message(updated, "The royalty was not marked paid."), RoyaltyRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    OgUow.Message(committed, "The royalty was not marked paid."), RoyaltyRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, context, royalty.Id, null, null, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgRoyaltyWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"paying royalty {royaltyId}", "the royalty was not paid", FailureSeverity.Error);

            return OgResult<OgRoyaltyWorkspace>.Failed(
                "The royalty could not be paid.", reference ?? RoyaltyRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgRoyaltyWorkspace>> DisputeAsync(
        string royaltyId,
        string reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "A dispute must record who raised it.", RoyaltyRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "A dispute needs its reason, or nobody can see what is being contested.",
                RoyaltyRules.CodeDisputeReasonRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "No connection is open, so the dispute cannot be raised.", RoyaltyRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, null, royaltyId, null, null, cancellationToken)
                .ConfigureAwait(false);
            var royalty = workspace.SelectedRoyalty;

            if (royalty is null)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"Royalty '{royaltyId}' is not in this database.", RoyaltyRules.CodeUnknownRoyalty);
            }

            if (royalty.IsDisputed)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"{royalty.LeaseId} is already disputed under {royalty.DisputeReference ?? "(no reference)"}.",
                    RoyaltyRules.CodeDisputeAlreadyOpen);
            }

            if (royalty.IsPaid)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"{royalty.LeaseId} is already paid; a dispute is raised before payment.", RoyaltyRules.CodeAlreadyPaid);
            }

            var unit = OgUow.For<ROYALTY_CALCULATION>(source);
            unit.PrimaryKey = "ROYALTY_CALCULATION_ID";

            var rows = await OgUow.ReadAsync(
                unit, [OgUow.Equal("ROYALTY_CALCULATION_ID", royalty.Id)], cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"Royalty '{royaltyId}' is not in this database.", RoyaltyRules.CodeUnknownRoyalty);
            }

            var existing = await OgUow.ReadAllAsync<ROYALTY_CALCULATION>(source).ConfigureAwait(false);
            var reference = NextDisputeReference(existing);

            row.DISPUTED_IND = "Y";
            row.DISPUTE_REFERENCE = reference;
            row.DISPUTE_REASON = reason.Trim();
            row.DISPUTED_BY = actor.Trim();
            row.DISPUTED_AT = DateTime.UtcNow;
            row.ROYALTY_STATUS = RoyaltyStatus.Disputed;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    OgUow.Message(updated, "The dispute was not recorded."), RoyaltyRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    OgUow.Message(committed, "The dispute was not recorded."), RoyaltyRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, null, royalty.Id, null, null, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgRoyaltyWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"raising a dispute on {royaltyId}", "the dispute was not recorded", FailureSeverity.Error);

            return OgResult<OgRoyaltyWorkspace>.Failed(
                "The dispute could not be recorded.", reference ?? RoyaltyRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgRoyaltyWorkspace>> ResolveDisputeAsync(
        string royaltyId,
        string note,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "Resolving a dispute must record who did it.", RoyaltyRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "No connection is open, so the dispute cannot be resolved.", RoyaltyRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, null, royaltyId, null, null, cancellationToken)
                .ConfigureAwait(false);
            var royalty = workspace.SelectedRoyalty;

            if (royalty is null)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"Royalty '{royaltyId}' is not in this database.", RoyaltyRules.CodeUnknownRoyalty);
            }

            if (!royalty.IsDisputed)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"{royalty.LeaseId} is not in dispute, so there is nothing to resolve.",
                    RoyaltyRules.CodeInputInvalid);
            }

            var unit = OgUow.For<ROYALTY_CALCULATION>(source);
            unit.PrimaryKey = "ROYALTY_CALCULATION_ID";

            var rows = await OgUow.ReadAsync(
                unit, [OgUow.Equal("ROYALTY_CALCULATION_ID", royalty.Id)], cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"Royalty '{royaltyId}' is not in this database.", RoyaltyRules.CodeUnknownRoyalty);
            }

            row.DISPUTED_IND = "N";
            row.DISPUTE_RESOLVED_BY = actor.Trim();
            row.DISPUTE_RESOLVED_AT = DateTime.UtcNow;
            row.DISPUTE_REASON = string.IsNullOrWhiteSpace(note)
                ? row.DISPUTE_REASON
                : row.DISPUTE_REASON + " | resolved: " + note.Trim();
            row.ROYALTY_STATUS = RoyaltyStatus.Accrued;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    OgUow.Message(updated, "The dispute was not resolved."), RoyaltyRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    OgUow.Message(committed, "The dispute was not resolved."), RoyaltyRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, null, royalty.Id, null, null, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgRoyaltyWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"resolving a dispute on {royaltyId}", "the dispute was not resolved", FailureSeverity.Error);

            return OgResult<OgRoyaltyWorkspace>.Failed(
                "The dispute could not be resolved.", reference ?? RoyaltyRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ JIB

    public async Task<OgResult<OgRoyaltyWorkspace>> RaiseJibAsync(
        string partnerId,
        DateTime periodStart,
        DateTime periodEnd,
        string? sourceVolumeReference,
        string? currency,
        IReadOnlyList<OgJibLine> lines,
        OgRoyaltyContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "A joint-interest statement must record who raised it.", RoyaltyRules.CodeActorRequired);
        }

        if (RoyaltyRules.ValidateJib(partnerId, sourceVolumeReference, lines, currency) is { } refusal)
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "No connection is open, so the statement cannot be raised.", RoyaltyRules.CodeNoConnection);
        }

        try
        {
            var charges = lines.Where(line => line.Kind == "CHARGE").Select(line => line.Amount).ToList();
            var credits = lines.Where(line => line.Kind == "CREDIT").Select(line => line.Amount).ToList();

            var totalCharges = JibMath.Total(charges);
            var totalCredits = JibMath.Total(credits);
            // AC-4: the net is computed from the lines that are about to be written, not stored beside them.
            var net = JibMath.Net(charges, credits);

            var now = DateTime.UtcNow;
            var statementId = Guid.NewGuid().ToString("N");

            var unit = OgUow.For<JOINT_INTEREST_STATEMENT>(source);
            unit.PrimaryKey = "JOINT_INTEREST_STATEMENT_ID";

            unit.Add(new JOINT_INTEREST_STATEMENT
            {
                JOINT_INTEREST_STATEMENT_ID = statementId,
                REPORT_TYPE = "JOINT_INTEREST_BILLING",
                REPORT_PERIOD_START = periodStart.Date,
                REPORT_PERIOD_END = periodEnd.Date,
                GENERATION_DATE = now.Date,
                GENERATED_BY = actor.Trim(),
                OPERATOR = context.CompanyId,
                TOTAL_CHARGES = totalCharges,
                TOTAL_CREDITS = totalCredits,
                NET_AMOUNT = net,
                PARTICIPANT_ID = partnerId.Trim(),
                STATUS = JibStatus.Draft,
                AUTHOR = actor.Trim(),
                CURRENCY_CODE = currency!.Trim().ToUpperInvariant(),
                // AC-6: the statement names the production it bills against.
                SOURCE_VOLUME_REFERENCE = sourceVolumeReference!.Trim(),
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
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    OgUow.Message(committed, "The statement was not saved."), RoyaltyRules.CodeWriteFailed);
            }

            if (!await WriteJibLinesAsync(source, statementId, lines, actor, cancellationToken).ConfigureAwait(false))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    "The statement's lines were not saved, so nothing was raised.", RoyaltyRules.CodeWriteFailed);
            }

            var workspace = await ReadAsync(source, context, null, statementId, null, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgRoyaltyWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"raising a joint-interest statement for {partnerId}", "the statement was not raised",
                FailureSeverity.Error);

            return OgResult<OgRoyaltyWorkspace>.Failed(
                "The statement could not be raised.", reference ?? RoyaltyRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgRoyaltyWorkspace>> ReviewJibAsync(
        string jibId,
        bool approve,
        string reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "A review must record who made it.", RoyaltyRules.CodeActorRequired);
        }

        if (!approve && string.IsNullOrWhiteSpace(reason))
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "Returning a statement needs its reason.", RoyaltyRules.CodeReturnReasonRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "No connection is open, so the statement cannot be reviewed.", RoyaltyRules.CodeNoConnection);
        }

        try
        {
            var jib = (await ReadAsync(source, null, null, jibId, null, cancellationToken).ConfigureAwait(false))
                .SelectedJib;

            if (jib is null)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"Joint-interest statement '{jibId}' is not in this database.", RoyaltyRules.CodeUnknownJib);
            }

            if (!jib.IsDraft)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"{jib.Describe()} is {jib.State.ToLowerInvariant()}; only a draft is reviewed.",
                    RoyaltyRules.CodeNotApproved);
            }

            // The post command's segregation of duties starts here: whoever raised it does not approve it.
            if (approve && !string.IsNullOrWhiteSpace(jib.Author)
                && string.Equals(jib.Author.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"{jib.Describe()} was raised by {jib.Author}, who therefore cannot approve it. A different " +
                    "controller must approve the bill.",
                    RoyaltyRules.CodeAuthorCannotAct);
            }

            var unit = OgUow.For<JOINT_INTEREST_STATEMENT>(source);
            unit.PrimaryKey = "JOINT_INTEREST_STATEMENT_ID";

            var rows = await OgUow.ReadAsync(
                unit, [OgUow.Equal("JOINT_INTEREST_STATEMENT_ID", jib.Id)], cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"Joint-interest statement '{jibId}' is not in this database.", RoyaltyRules.CodeUnknownJib);
            }

            row.STATUS = approve ? JibStatus.Approved : JibStatus.Returned;
            row.APPROVED_BY = approve ? actor.Trim() : null;
            row.APPROVED_AT = approve ? DateTime.UtcNow : null;
            row.RETURN_REASON = approve ? null : reason.Trim();
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    OgUow.Message(updated, "The review was not recorded."), RoyaltyRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    OgUow.Message(committed, "The review was not recorded."), RoyaltyRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, null, null, jib.Id, null, cancellationToken).ConfigureAwait(false);

            return OgResult<OgRoyaltyWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"reviewing joint-interest statement {jibId}", "the review was not recorded",
                FailureSeverity.Error);

            return OgResult<OgRoyaltyWorkspace>.Failed(
                "The statement could not be reviewed.", reference ?? RoyaltyRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgRoyaltyWorkspace>> PostJibAsync(
        string jibId,
        OgRoyaltyContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "A posting must record who made it.", RoyaltyRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "No connection is open, so the statement cannot be posted.", RoyaltyRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, null, jibId, null, cancellationToken)
                .ConfigureAwait(false);
            var jib = workspace.SelectedJib;

            if (jib is null)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"Joint-interest statement '{jibId}' is not in this database.", RoyaltyRules.CodeUnknownJib);
            }

            // Posting is a transition of the statement that exists, so a second post cannot create a second entry.
            if (jib.IsPosted && jib.HasJournal)
            {
                return OgResult<OgRoyaltyWorkspace>.Ok(workspace);
            }

            if (!jib.IsApproved && !jib.IsPosted)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"{jib.Describe()} is {jib.State.ToLowerInvariant()}; a statement is posted after it is approved.",
                    RoyaltyRules.CodeNotApproved);
            }

            // A statement that nets to nothing has nothing to post, and saying so is better than writing a zero entry.
            if (jib.Net == 0m)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"{jib.Describe()} nets to zero, so there is nothing to post.",
                    RoyaltyRules.CodeInputInvalid);
            }

            var periodCode = LedgerCodes.PeriodCodeOf(DateTime.UtcNow);
            var periods = await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);
            var period = periods.FirstOrDefault(row =>
                string.Equals(row.PERIOD_CODE, periodCode, StringComparison.OrdinalIgnoreCase));

            if (period is not null && !LedgerCodes.IsOpen(period.STATUS))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"Period {periodCode} is {period.STATUS.ToLowerInvariant()}, so the statement cannot be posted " +
                    "into it.",
                    RoyaltyRules.CodePeriodClosed);
            }

            // SG-10-7: the bill posts to the configured accounts - the partner receivable against the cost it recovers.
            var journalId = await PostJournalAsync(
                source,
                "JIB-" + jib.Id,
                DateTime.UtcNow.Date,
                RoyaltyJibCodes.JibReceivableAccount,
                RoyaltyJibCodes.JibCostAccount,
                Math.Abs(jib.Net),
                "joint-interest bill " + jib.PartnerId + " against " + (jib.SourceVolumeReference ?? "no source"),
                actor,
                cancellationToken).ConfigureAwait(false);

            if (journalId is null)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"The bill's journal was not written: the chart of accounts must carry " +
                    $"{RoyaltyJibCodes.JibReceivableAccount} and {RoyaltyJibCodes.JibCostAccount}. Nothing was posted.",
                    RoyaltyRules.CodeUnknownAccount);
            }

            var unit = OgUow.For<JOINT_INTEREST_STATEMENT>(source);
            unit.PrimaryKey = "JOINT_INTEREST_STATEMENT_ID";

            var rows = await OgUow.ReadAsync(
                unit, [OgUow.Equal("JOINT_INTEREST_STATEMENT_ID", jib.Id)], cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    $"Joint-interest statement '{jibId}' is not in this database.", RoyaltyRules.CodeUnknownJib);
            }

            row.STATUS = JibStatus.Posted;
            row.POSTED_BY = actor.Trim();
            row.POSTED_AT = DateTime.UtcNow;
            row.JOURNAL_ENTRY_ID = journalId;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    OgUow.Message(updated, "The statement was not marked posted."), RoyaltyRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    OgUow.Message(committed, "The statement was not marked posted."), RoyaltyRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, context, null, jib.Id, null, cancellationToken).ConfigureAwait(false);

            return OgResult<OgRoyaltyWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"posting joint-interest statement {jibId}", "the statement was not posted",
                FailureSeverity.Error);

            return OgResult<OgRoyaltyWorkspace>.Failed(
                "The statement could not be posted.", reference ?? RoyaltyRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ cost allocation

    public OgResult<OgAllocationPreview> PreviewAllocation(
        decimal pool,
        string currency,
        IReadOnlyList<CostAllocationMath.AllocationTarget> targets,
        string method)
    {
        if (targets is null || targets.Count == 0)
        {
            return OgResult<OgAllocationPreview>.Failed(
                "An allocation needs at least one target.", RoyaltyRules.CodeAllocationEmpty);
        }

        if (string.IsNullOrWhiteSpace(currency)
            || !RoyaltyRules.Currencies.Contains(currency.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return OgResult<OgAllocationPreview>.Failed(
                $"'{currency}' is not a currency this app allocates in.", RoyaltyRules.CodeCurrencyMismatch);
        }

        if (string.IsNullOrWhiteSpace(method)
            || !RoyaltyJibCodes.AllocationMethods.Contains(method.Trim(), StringComparer.Ordinal))
        {
            return OgResult<OgAllocationPreview>.Failed(
                $"'{method}' is not an allocation method; use {string.Join(", ", RoyaltyJibCodes.AllocationMethods)}.",
                RoyaltyRules.CodeInputInvalid);
        }

        // The calculator's guards, asked here so the answer is a sentence and not an exception.
        if (pool < 0m)
        {
            return OgResult<OgAllocationPreview>.Failed(
                "A cost pool cannot be negative.", RoyaltyRules.CodeInputInvalid);
        }

        if (targets.Any(target => target.Weight < 0m))
        {
            return OgResult<OgAllocationPreview>.Failed(
                "An allocation basis cannot be negative.", RoyaltyRules.CodeInputInvalid);
        }

        if (targets.Sum(target => target.Weight) <= 0m)
        {
            return OgResult<OgAllocationPreview>.Failed(
                "Every allocation basis is zero, so there is nothing to allocate by.", RoyaltyRules.CodeInputInvalid);
        }

        if (targets.Select(target => target.TargetId).Distinct(StringComparer.Ordinal).Count() != targets.Count)
        {
            return OgResult<OgAllocationPreview>.Failed(
                "Each allocation target may appear once; two shares for one property cannot be told apart.",
                RoyaltyRules.CodeInputInvalid);
        }

        // AC-5: this is the same call the commit makes, so the preview cannot differ from what is written.
        var shares = CostAllocationMath.Allocate(pool, targets);

        return OgResult<OgAllocationPreview>.Ok(new OgAllocationPreview(
            pool,
            currency.Trim().ToUpperInvariant(),
            shares.Select(share => new OgAllocationRow(
                share.TargetId, share.TargetType, share.Percentage, share.Amount, share.Basis)).ToList()));
    }

    public async Task<OgResult<OgRoyaltyWorkspace>> CommitAllocationAsync(
        decimal pool,
        string currency,
        IReadOnlyList<CostAllocationMath.AllocationTarget> targets,
        string method,
        string reference,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "A committed allocation must record who made it.", RoyaltyRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(reference))
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "A committed allocation needs the reference of the pool it splits.",
                RoyaltyRules.CodeInputRequired);
        }

        var preview = PreviewAllocation(pool, currency, targets, method);

        if (!preview.Success || preview.Value is null)
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                preview.Message ?? "The allocation cannot be computed.", preview.Reference ?? RoyaltyRules.CodeInputInvalid);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "No connection is open, so the allocation cannot be committed.", RoyaltyRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var unit = OgUow.For<COST_ALLOCATION>(source);
            unit.PrimaryKey = "COST_ALLOCATION_ID";

            foreach (var share in preview.Value.Shares)
            {
                unit.Add(new COST_ALLOCATION
                {
                    COST_ALLOCATION_ID = Guid.NewGuid().ToString("N"),
                    COST_TRANSACTION_ID = reference.Trim(),
                    PROPERTY_ID = string.Equals(share.TargetType, "PROPERTY", StringComparison.OrdinalIgnoreCase)
                        ? share.TargetId
                        : null,
                    WELL_ID = string.Equals(share.TargetType, "WELL", StringComparison.OrdinalIgnoreCase)
                        ? share.TargetId
                        : null,
                    COST_CENTER_ID = string.Equals(share.TargetType, "COST_CENTRE", StringComparison.OrdinalIgnoreCase)
                        ? share.TargetId
                        : null,
                    ALLOCATED_AMOUNT = share.Amount,
                    ALLOCATION_PERCENTAGE = share.Percentage,
                    ALLOCATION_METHOD = method.Trim(),
                    DESCRIPTION = string.Equals(share.TargetType, "COST_CENTRE", StringComparison.OrdinalIgnoreCase)
                        ? null
                        : share.TargetType + " " + share.TargetId,
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString("N"),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgRoyaltyWorkspace>.Failed(
                    OgUow.Message(committed, "The allocation was not committed."), RoyaltyRules.CodeWriteFailed);
            }

            var workspace = await ReadAsync(source, null, null, null, reference, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgRoyaltyWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var failure = _failures.ReportHandled(
                ex, $"committing a cost allocation for {reference}", "the allocation was not committed",
                FailureSeverity.Error);

            return OgResult<OgRoyaltyWorkspace>.Failed(
                "The allocation could not be committed.", failure ?? RoyaltyRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgRoyaltyWorkspace>> LoadAsync(
        OgRoyaltyContext context,
        string? royaltyId = null,
        string? jibId = null,
        string? allocationReference = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgRoyaltyWorkspace>.Failed(
                "No connection is open, so the royalties cannot be read.", RoyaltyRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, royaltyId, jibId, allocationReference, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgRoyaltyWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the royalties and bills", "the royalties are unavailable; nothing can be paid",
                FailureSeverity.Degraded);

            return OgResult<OgRoyaltyWorkspace>.Ok(new OgRoyaltyWorkspace(
                [], null, [], null, [], null, null,
                RoyaltyRules.RoyaltyStates, RoyaltyRules.JibStates, RoyaltyRules.AllocationMethods,
                RoyaltyRules.TargetTypes, RoyaltyRules.Currencies, true,
                $"the royalties could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the reads

    private async Task<OgRoyaltyWorkspace> ReadAsync(
        IDataSource source,
        OgRoyaltyContext? context,
        string? royaltyId,
        string? jibId,
        string? allocationReference,
        CancellationToken cancellationToken)
    {
        var calculationRows = await OgUow.ReadAllAsync<ROYALTY_CALCULATION>(source).ConfigureAwait(false);
        var deductionRows = await OgUow.ReadAllAsync<ROYALTY_DEDUCTIONS>(source).ConfigureAwait(false);
        var statementRows = await OgUow.ReadAllAsync<JOINT_INTEREST_STATEMENT>(source).ConfigureAwait(false);
        var chargeRows = await OgUow.ReadAllAsync<JIB_CHARGE>(source).ConfigureAwait(false);
        var creditRows = await OgUow.ReadAllAsync<JIB_CREDIT>(source).ConfigureAwait(false);
        var allocationRows = await OgUow.ReadAllAsync<COST_ALLOCATION>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var deductionsById = deductionRows
            .Where(row => !string.IsNullOrWhiteSpace(row.ROYALTY_DEDUCTIONS_ID))
            .ToDictionary(row => row.ROYALTY_DEDUCTIONS_ID, row => row, StringComparer.OrdinalIgnoreCase);

        var royalties = calculationRows
            .Where(row => !string.IsNullOrWhiteSpace(row.ROYALTY_CALCULATION_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.CALCULATION_DATE)
            .ThenBy(row => row.ROYALTY_CALCULATION_ID, StringComparer.Ordinal)
            .Select(row => ToRoyalty(
                row,
                row.ROYALTY_DEDUCTIONS_ID is { } id && deductionsById.TryGetValue(id, out var deduction)
                    ? deduction
                    : null))
            .ToList();

        var byStatement = chargeRows
            .Where(row => !string.IsNullOrWhiteSpace(row.JOINT_INTEREST_STATEMENT_ID))
            .GroupBy(row => row.JOINT_INTEREST_STATEMENT_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        var creditsByStatement = creditRows
            .Where(row => !string.IsNullOrWhiteSpace(row.JOINT_INTEREST_STATEMENT_ID))
            .GroupBy(row => row.JOINT_INTEREST_STATEMENT_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        var jibs = statementRows
            .Where(row => !string.IsNullOrWhiteSpace(row.JOINT_INTEREST_STATEMENT_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.REPORT_PERIOD_END)
            .ThenBy(row => row.JOINT_INTEREST_STATEMENT_ID, StringComparer.Ordinal)
            .Select(row => ToJib(
                row,
                byStatement.TryGetValue(row.JOINT_INTEREST_STATEMENT_ID, out var charges) ? charges : [],
                creditsByStatement.TryGetValue(row.JOINT_INTEREST_STATEMENT_ID, out var credits) ? credits : []))
            .ToList();

        var committed = allocationRows
            .Where(row => !string.IsNullOrWhiteSpace(row.COST_ALLOCATION_ID))
            .Where(row => string.IsNullOrWhiteSpace(allocationReference)
                          || string.Equals(row.COST_TRANSACTION_ID?.Trim(), allocationReference.Trim(),
                              StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Select(row => new OgAllocationRow(
                row.PROPERTY_ID ?? row.WELL_ID ?? row.COST_CENTER_ID ?? string.Empty,
                row.PROPERTY_ID is not null ? "PROPERTY" : row.WELL_ID is not null ? "WELL" : "COST_CENTRE",
                row.ALLOCATION_PERCENTAGE ?? 0m,
                row.ALLOCATED_AMOUNT ?? 0m,
                row.ALLOCATION_PERCENTAGE ?? 0m))
            .OrderBy(row => row.TargetId, StringComparer.Ordinal)
            .ToList();

        var selectedRoyalty = string.IsNullOrWhiteSpace(royaltyId)
            ? royalties.FirstOrDefault()
            : royalties.FirstOrDefault(royalty =>
                  string.Equals(royalty.Id, royaltyId.Trim(), StringComparison.OrdinalIgnoreCase))
              ?? royalties.FirstOrDefault();

        var selectedJib = string.IsNullOrWhiteSpace(jibId)
            ? jibs.FirstOrDefault()
            : jibs.FirstOrDefault(jib => string.Equals(jib.Id, jibId.Trim(), StringComparison.OrdinalIgnoreCase))
              ?? jibs.FirstOrDefault();

        _ = context;

        return new OgRoyaltyWorkspace(
            royalties,
            selectedRoyalty,
            jibs,
            selectedJib,
            committed,
            string.IsNullOrWhiteSpace(allocationReference) ? null : allocationReference.Trim(),
            committed.Count == 0 ? null : DateTime.UtcNow.Date,
            RoyaltyRules.RoyaltyStates,
            RoyaltyRules.JibStates,
            RoyaltyRules.AllocationMethods,
            RoyaltyRules.TargetTypes,
            RoyaltyRules.Currencies,
            false,
            null);
    }

    private static OgRoyalty ToRoyalty(ROYALTY_CALCULATION row, ROYALTY_DEDUCTIONS? deductions)
    {
        var parts = new OgRoyaltyDeductions(
            deductions?.TRANSPORTATION_COSTS ?? row.TRANSPORTATION_COST ?? 0m,
            deductions?.PRODUCTION_TAXES ?? (row.AD_VALOREM_TAX ?? 0m) + (row.SEVERANCE_TAX ?? 0m),
            0m,
            deductions?.PROCESSING_COSTS ?? 0m,
            deductions?.OTHER_DEDUCTIONS ?? 0m);

        // The recorded production tax is split back into its two components only for display; the total is what was stored.
        var adValorem = row.AD_VALOREM_TAX ?? 0m;
        var severance = row.SEVERANCE_TAX ?? 0m;

        parts = parts with { AdValorem = adValorem, Severance = severance };

        return new OgRoyalty(
            row.ROYALTY_CALCULATION_ID,
            row.PROPERTY_OR_LEASE_ID ?? string.Empty,
            row.ROYALTY_OWNER_ID,
            row.CALCULATION_DATE ?? DateTime.MinValue,
            row.GROSS_VOLUME ?? 0m,
            row.GROSS_VOLUME_OUOM ?? string.Empty,
            row.PRICE_PER_UNIT ?? 0m,
            row.GROSS_REVENUE ?? 0m,
            parts,
            row.NET_REVENUE ?? 0m,
            row.ROYALTY_INTEREST ?? 0m,
            row.ROYALTY_AMOUNT ?? 0m,
            row.ROYALTY_STATUS ?? RoyaltyStatus.Calculated,
            row.CURRENCY_CODE,
            string.Equals(row.DISPUTED_IND, "Y", StringComparison.OrdinalIgnoreCase),
            row.DISPUTE_REFERENCE,
            row.DISPUTE_REASON,
            row.DISPUTED_BY,
            row.DISPUTED_AT,
            row.DISPUTE_RESOLVED_BY,
            row.DISPUTE_RESOLVED_AT,
            row.AUTHOR,
            row.JOURNAL_ENTRY_ID,
            row.PAYMENT_JOURNAL_ENTRY_ID,
            row.PAID_AT,
            row.PAID_BY,
            row.PAID_AMOUNT,
            row.ALLOCATION_DETAIL_ID,
            row.ALLOCATION_RESULT_ID);
    }

    private static OgJib ToJib(
        JOINT_INTEREST_STATEMENT row,
        IReadOnlyList<JIB_CHARGE> charges,
        IReadOnlyList<JIB_CREDIT> credits)
    {
        var lines = charges
            .Select(charge => new OgJibLine("CHARGE", charge.DESCRIPTION ?? string.Empty, charge.CATEGORY, charge.AMOUNT))
            .Concat(credits.Select(credit => new OgJibLine("CREDIT", credit.DESCRIPTION ?? string.Empty,
                credit.CATEGORY, credit.AMOUNT)))
            .ToList();

        return new OgJib(
            row.JOINT_INTEREST_STATEMENT_ID,
            row.PARTICIPANT_ID ?? string.Empty,
            row.REPORT_PERIOD_START ?? DateTime.MinValue,
            row.REPORT_PERIOD_END ?? DateTime.MinValue,
            row.TOTAL_CHARGES,
            row.TOTAL_CREDITS,
            row.NET_AMOUNT,
            row.CURRENCY_CODE ?? string.Empty,
            row.STATUS ?? JibStatus.Draft,
            row.OPERATOR,
            row.AUTHOR,
            row.APPROVED_BY,
            row.APPROVED_AT,
            row.POSTED_BY,
            row.POSTED_AT,
            row.JOURNAL_ENTRY_ID,
            row.SOURCE_VOLUME_REFERENCE,
            row.RETURN_REASON,
            lines);
    }

    // ------------------------------------------------------------------ the writes

    private static async Task<bool> WriteJibLinesAsync(
        IDataSource source,
        string statementId,
        IReadOnlyList<OgJibLine> lines,
        string actor,
        CancellationToken cancellationToken)
    {
        var charges = OgUow.For<JIB_CHARGE>(source);
        charges.PrimaryKey = "JIB_CHARGE_ID";

        var credits = OgUow.For<JIB_CREDIT>(source);
        credits.PrimaryKey = "JIB_CREDIT_ID";

        var now = DateTime.UtcNow;

        foreach (var line in lines)
        {
            if (line.Kind == "CHARGE")
            {
                charges.Add(new JIB_CHARGE
                {
                    JIB_CHARGE_ID = Guid.NewGuid().ToString("N"),
                    JOINT_INTEREST_STATEMENT_ID = statementId,
                    DESCRIPTION = line.Description,
                    AMOUNT = line.Amount,
                    CATEGORY = line.Category,
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString("N"),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }
            else
            {
                credits.Add(new JIB_CREDIT
                {
                    JIB_CREDIT_ID = Guid.NewGuid().ToString("N"),
                    JOINT_INTEREST_STATEMENT_ID = statementId,
                    DESCRIPTION = line.Description,
                    AMOUNT = line.Amount,
                    CATEGORY = line.Category,
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString("N"),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        var chargesCommitted = await charges.Commit().ConfigureAwait(false);
        var creditsCommitted = await credits.Commit().ConfigureAwait(false);

        return OgUow.IsOk(chargesCommitted) && OgUow.IsOk(creditsCommitted);
    }

    /// <summary>
    /// Posts one balanced entry at the configured accounts and returns its id, or null when the chart does not carry them -
    /// the caller then refuses rather than moving money with no ledger effect.
    /// </summary>
    private static async Task<string?> PostJournalAsync(
        IDataSource source,
        string reference,
        DateTime date,
        string debitNumber,
        string creditNumber,
        decimal amount,
        string description,
        string actor,
        CancellationToken cancellationToken)
    {
        var accounts = await OgUow.ReadAllAsync<GL_ACCOUNT>(source).ConfigureAwait(false);

        var debit = accounts.FirstOrDefault(account =>
            string.Equals(account.ACCOUNT_NUMBER?.Trim(), debitNumber, StringComparison.OrdinalIgnoreCase));
        var credit = accounts.FirstOrDefault(account =>
            string.Equals(account.ACCOUNT_NUMBER?.Trim(), creditNumber, StringComparison.OrdinalIgnoreCase));

        if (debit is null || credit is null)
        {
            return null;
        }

        var journals = await OgUow.ReadAllAsync<JOURNAL_ENTRY>(source).ConfigureAwait(false);

        var existing = journals.FirstOrDefault(entry =>
            string.Equals(entry.SOURCE_MODULE, "ROYALTY_JIB", StringComparison.OrdinalIgnoreCase)
            && string.Equals(entry.REFERENCE_NUMBER, reference, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            return existing.JOURNAL_ENTRY_ID;
        }

        var now = DateTime.UtcNow;
        var journalId = Guid.NewGuid().ToString("N");

        var journal = OgUow.For<JOURNAL_ENTRY>(source);
        journal.PrimaryKey = "JOURNAL_ENTRY_ID";

        journal.Add(new JOURNAL_ENTRY
        {
            JOURNAL_ENTRY_ID = journalId,
            ENTRY_NUMBER = NextJournalNumber(journals, date),
            ENTRY_DATE = date,
            ENTRY_TYPE = "ROYALTY_JIB",
            STATUS = LedgerCodes.JournalPosted,
            DESCRIPTION = description,
            REFERENCE_NUMBER = reference,
            SOURCE_MODULE = "ROYALTY_JIB",
            TOTAL_DEBIT = amount,
            TOTAL_CREDIT = amount,
            AUTHOR = actor.Trim(),
            POSTED_BY = actor.Trim(),
            POSTED_AT = now,
            PERIOD_CODE = LedgerCodes.PeriodCodeOf(date),
            ACTIVE_IND = "Y",
            PPDM_GUID = Guid.NewGuid().ToString("N"),
            ROW_CREATED_BY = actor.Trim(),
            ROW_CREATED_DATE = now,
            ROW_CHANGED_BY = actor.Trim(),
            ROW_CHANGED_DATE = now,
        });

        var committed = await journal.Commit().ConfigureAwait(false);

        if (!OgUow.IsOk(committed))
        {
            return null;
        }

        var lines = OgUow.For<JOURNAL_ENTRY_LINE>(source);
        lines.PrimaryKey = "JOURNAL_ENTRY_LINE_ID";

        lines.Add(EntryLine(journalId, debit!, amount, 0m, 1, description, actor, now));
        lines.Add(EntryLine(journalId, credit!, 0m, amount, 2, description, actor, now));

        cancellationToken.ThrowIfCancellationRequested();

        var linesCommitted = await lines.Commit().ConfigureAwait(false);

        return OgUow.IsOk(linesCommitted) ? journalId : null;
    }

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

    private static string NextDisputeReference(IReadOnlyList<ROYALTY_CALCULATION> existing)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = "DIS-" + year.ToString(CultureInfo.InvariantCulture) + "-";

        var highest = existing
            .Select(row => row.DISPUTE_REFERENCE)
            .Where(reference => reference is not null && reference.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(reference => int.TryParse(reference![prefix.Length..], NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var sequence)
                ? sequence
                : 0)
            .DefaultIfEmpty(0)
            .Max();

        return prefix + (highest + 1).ToString("0000", CultureInfo.InvariantCulture);
    }

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
