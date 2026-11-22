using System.Globalization;
using Beep.OilandGas.Models.Data.Lease;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Lease;

/// <summary>The acquisition stages, in the order the pipeline moves through them (LSE-02).</summary>
public static class LeasePipelineStage
{
    public const string Opportunity = "OPPORTUNITY";
    public const string Screening = "SCREENING";
    public const string DueDiligence = "DUE_DILIGENCE";
    public const string Negotiation = "NEGOTIATION";
    public const string RightsReview = "RIGHTS_REVIEW";
    public const string Executed = "EXECUTED";
    public const string Declined = "DECLINED";

    /// <summary>The forward order; <see cref="Declined"/> is a side exit from any live stage.</summary>
    public static IReadOnlyList<string> Order { get; } =
        [Opportunity, Screening, DueDiligence, Negotiation, RightsReview, Executed];

    public static IReadOnlyList<string> All { get; } = [.. Order, Declined];

    public static bool IsKnown(string? stage) =>
        !string.IsNullOrWhiteSpace(stage)
        && All.Contains(stage.Trim(), StringComparer.OrdinalIgnoreCase);

    public static bool IsTerminal(string? stage) =>
        string.Equals(stage, Executed, StringComparison.OrdinalIgnoreCase)
        || string.Equals(stage, Declined, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Diligence gates the move past due diligence: from that stage onward an open item blocks the advance (AC-2).
    /// </summary>
    public static bool DiligenceGates(string? stage) =>
        string.Equals(stage, DueDiligence, StringComparison.OrdinalIgnoreCase)
        || string.Equals(stage, Negotiation, StringComparison.OrdinalIgnoreCase)
        || string.Equals(stage, RightsReview, StringComparison.OrdinalIgnoreCase);

    /// <summary>The one legal forward stage, or null when the stage is terminal.</summary>
    public static string? Next(string? stage)
    {
        var index = IndexOf(stage);

        return index < 0 || index >= Order.Count - 1 ? null : Order[index + 1];
    }

    /// <summary>The legal set a refusal lists: the forward stage and, from a live stage, decline.</summary>
    public static IReadOnlyList<string> LegalFrom(string? stage)
    {
        // An opportunity with no recorded stage can enter at the first stage, or be declined.
        if (string.IsNullOrWhiteSpace(stage) || !IsKnown(stage))
        {
            return [Opportunity, Declined];
        }

        var next = Next(stage);

        return next is null ? [] : [next, Declined];
    }

    private static int IndexOf(string? stage) =>
        string.IsNullOrWhiteSpace(stage)
            ? -1
            : Order.ToList().FindIndex(candidate =>
                string.Equals(candidate, stage.Trim(), StringComparison.OrdinalIgnoreCase));
}

/// <summary>One opportunity on the pipeline (LSE-02).</summary>
public sealed record OgLeaseOpportunity(
    string Id,
    string Name,
    string? Region,
    decimal? Acreage,
    string? AcreageUnit,
    decimal? AskingPrice,
    string? Currency,
    string? Stage,
    string? Owner,
    DateTime? StageEnteredAt,
    int? StageAgeDays)
{
    public const string UnclassifiedStage = "(unclassified)";

    public string StageLabel =>
        string.IsNullOrWhiteSpace(Stage) || !LeasePipelineStage.IsKnown(Stage)
            ? UnclassifiedStage
            : Stage!.Trim().ToUpperInvariant();

    /// <summary>AC-5: a money value is never shown without the currency it is in.</summary>
    public string AskingPriceText => AskingPrice is { } price
        ? $"{price.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(Currency) ? "(currency not recorded)" : Currency!.Trim())
        : "no asking price recorded";

    public string AcreageText => Acreage is { } acreage
        ? $"{acreage.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(AcreageUnit) ? "(unit not recorded)" : AcreageUnit!.Trim())
        : "no acreage recorded";

    public string AgeText => StageAgeDays is { } days
        ? $"{days.ToString(CultureInfo.InvariantCulture)} d"
        : "unknown";

    public string Describe() =>
        string.Join(
            " · ",
            new[]
            {
                Name,
                Region,
                StageLabel,
                AskingPriceText,
                $"owner {Owner ?? "(unknown)"}",
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
}

/// <summary>One due-diligence item (LSE-02 AC-2).</summary>
public sealed record OgDiligenceItem(
    string ItemId,
    string Category,
    string Question,
    string? Outcome,
    string? EvidenceRef,
    string? Reviewer,
    DateTime? ReviewedAt)
{
    public const string Open = "OPEN";
    public const string Pass = "PASS";
    public const string Fail = "FAIL";
    public const string Waived = "WAIVED";

    public static IReadOnlyList<string> Outcomes { get; } = [Open, Pass, Fail, Waived];

    /// <summary>An item with no outcome, or an explicit OPEN, still has to be answered.</summary>
    public bool IsOpen =>
        string.IsNullOrWhiteSpace(Outcome)
        || string.Equals(Outcome.Trim(), Open, StringComparison.OrdinalIgnoreCase);

    public string OutcomeText => IsOpen ? "open" : Outcome!.Trim().ToUpperInvariant();

    public string Describe() =>
        $"{Category}: {Question} [{OutcomeText}]" +
        (string.IsNullOrWhiteSpace(EvidenceRef) ? string.Empty : $" ref {EvidenceRef}");
}

/// <summary>One row of the rights matrix (LSE-02 AC-3).</summary>
public sealed record OgLeaseRight(
    string RightType,
    string? Owner,
    decimal InterestPercent,
    DateTime? Effective,
    DateTime? Expiry,
    bool Active)
{
    public const string Mineral = "MINERAL";
    public const string Surface = "SURFACE";
    public const string Working = "WORKING";

    public static IReadOnlyList<string> Types { get; } = [Mineral, Surface, Working];

    public string InterestText => $"{(InterestPercent * 100m).ToString("0.###", CultureInfo.InvariantCulture)}%";

    public string Describe() => $"{RightType}: {Owner ?? "(owner unknown)"} {InterestText}";
}

/// <summary>One negotiation round (LSE-02).</summary>
public sealed record OgNegotiationEntry(
    int Round,
    string? Counterparty,
    decimal Offer,
    string? Currency,
    string? Notes,
    string? Actor,
    DateTime? RecordedAt)
{
    public string OfferText =>
        $"{Offer.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
        (string.IsNullOrWhiteSpace(Currency) ? "(currency not recorded)" : Currency!.Trim());

    public string Describe() => $"Round {Round}: {Counterparty} offered {OfferText}";
}

/// <summary>One recorded stage transition.</summary>
public sealed record OgPipelineHistoryEntry(
    int Sequence,
    string Stage,
    string? Outcome,
    string? Rationale,
    string? Actor,
    DateTime? DecidedAt);

/// <summary>One card on the pipeline board.</summary>
public sealed record OgPipelineCard(
    OgLeaseOpportunity Opportunity,
    int OpenDiligenceCount,
    int RightsCount,
    int NegotiationRounds)
{
    public string Id => Opportunity.Id;

    public string StageLabel => Opportunity.StageLabel;

    public int StageOrder
    {
        get
        {
            var index = LeasePipelineStage.Order.ToList().FindIndex(stage =>
                string.Equals(stage, Opportunity.StageLabel, StringComparison.OrdinalIgnoreCase));

            return index < 0 ? int.MaxValue : index;
        }
    }

    public string DiligenceText => OpenDiligenceCount == 0
        ? "diligence complete"
        : $"{OpenDiligenceCount.ToString(CultureInfo.InvariantCulture)} open diligence item(s)";

    public string Describe() =>
        string.Join(
            " · ",
            new[]
            {
                Opportunity.Name,
                Opportunity.Region,
                Opportunity.AskingPriceText,
                StageLabel,
                DiligenceText,
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
}

/// <summary>One stage column of the board.</summary>
public sealed record OgPipelineColumn(
    string Stage,
    int Order,
    bool IsUnclassified,
    int Count,
    IReadOnlyList<OgPipelineCard> Cards)
{
    public string Header => $"{Stage} ({Count.ToString(CultureInfo.InvariantCulture)})";
}

/// <summary>The pipeline as the board shows it.</summary>
public sealed record OgPipelineSnapshot(
    string FieldName,
    IReadOnlyList<OgPipelineColumn> Columns,
    IReadOnlyList<OgPipelineCard> Cards,
    DateTime AsOf)
{
    public int CardCount => Cards.Count;

    public bool IsEmpty => Cards.Count == 0;

    public int OpenDiligenceCount => Cards.Sum(card => card.OpenDiligenceCount);

    public string Summary => string.Join(
        " · ",
        Columns.Select(column => column.Header));
}

/// <summary>Everything the detail pane shows for one opportunity.</summary>
public sealed record OgPipelineDetail(
    OgLeaseOpportunity Opportunity,
    IReadOnlyList<OgDiligenceItem> Diligence,
    IReadOnlyList<OgLeaseRight> Rights,
    IReadOnlyList<OgNegotiationEntry> Negotiations,
    IReadOnlyList<OgPipelineHistoryEntry> History)
{
    public IReadOnlyList<OgDiligenceItem> OpenDiligence =>
        Diligence.Where(item => item.IsOpen).ToList();

    public int OpenDiligenceCount => OpenDiligence.Count;

    public IReadOnlyList<OgLeaseRight> MineralRights =>
        Rights.Where(right => string.Equals(right.RightType, OgLeaseRight.Mineral, StringComparison.OrdinalIgnoreCase)).ToList();

    public IReadOnlyList<OgLeaseRight> SurfaceRights =>
        Rights.Where(right => string.Equals(right.RightType, OgLeaseRight.Surface, StringComparison.OrdinalIgnoreCase)).ToList();

    public IReadOnlyList<OgLeaseRight> WorkingInterest =>
        Rights.Where(right => string.Equals(right.RightType, OgLeaseRight.Working, StringComparison.OrdinalIgnoreCase)).ToList();

    public string RightsSummary =>
        $"mineral {MineralRights.Count.ToString(CultureInfo.InvariantCulture)} · " +
        $"surface {SurfaceRights.Count.ToString(CultureInfo.InvariantCulture)} · " +
        $"working {WorkingInterest.Count.ToString(CultureInfo.InvariantCulture)}";
}

/// <summary>One advance or decline, as the page submits it.</summary>
public sealed record OgPipelineDecision(
    string LeaseId,
    string? TargetStage,
    string Rationale,
    string Actor,
    DateTimeOffset? DecidedAt = null);

/// <summary>LSE-02: the acquisition pipeline, diligence, rights and negotiation.</summary>
public interface IOgLeasePipeline
{
    Task<OgResult<OgPipelineSnapshot>> LoadAsync(
        string? fieldId,
        string fieldName,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgPipelineDetail>> LoadDetailAsync(
        string leaseId,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>Records or closes a diligence item; an item with no outcome is open (AC-2).</summary>
    Task<OgResult<OgPipelineDetail>> RecordDiligenceAsync(
        string leaseId,
        string category,
        string question,
        string? outcome,
        string? evidenceRef,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Records a negotiation round; a counterparty and a currency are required.</summary>
    Task<OgResult<OgPipelineDetail>> RecordNegotiationAsync(
        string leaseId,
        string counterparty,
        int? round,
        decimal offerAmount,
        string currency,
        string? notes,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Advances one legal stage, recording actor, date and rationale; open diligence blocks it (AC-1, AC-2).</summary>
    Task<OgResult<OgPipelineDetail>> AdvanceAsync(
        OgPipelineDecision decision,
        CancellationToken cancellationToken = default);

    /// <summary>Declines a live opportunity; the history is kept (AC-4).</summary>
    Task<OgResult<OgPipelineDetail>> DeclineAsync(
        string leaseId,
        string rationale,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The decision that was not acknowledged, or null.</summary>
    OgPipelineDecision? Pending { get; }

    /// <summary>Re-sends the retained decision, unchanged.</summary>
    Task<OgResult<OgPipelineDetail>> RetryAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The pipeline over <c>LEASE_ACQUISITION</c> and the LeaseAcquisition-owned pipeline records
/// (<c>LEASE_PIPELINE_STAGE</c>, <c>LEASE_DILIGENCE_ITEM</c>, <c>LEASE_RIGHT</c>, <c>LEASE_NEGOTIATION</c>), read
/// and written through their entity models and a unit of work.
/// </summary>
public sealed class OgLeasePipelineService : IOgLeasePipeline
{
    public const string CodeNoConnection = "lease-pipeline-no-connection";
    public const string CodeReadFailed = "lease-pipeline-read-failed";
    public const string CodeWriteFailed = "lease-pipeline-write-failed";
    public const string CodeUnknown = "lease-pipeline-unknown";
    public const string CodeActorRequired = "lease-pipeline-actor-required";
    public const string CodeStageRequired = "lease-pipeline-stage-required";
    public const string CodeStageIllegal = "lease-pipeline-stage-illegal";
    public const string CodeStageSame = "lease-pipeline-stage-same";
    public const string CodeRationaleRequired = "lease-pipeline-rationale-required";
    public const string CodeDiligenceOpen = "lease-pipeline-diligence-open";
    public const string CodeCounterpartyRequired = "lease-pipeline-counterparty-required";
    public const string CodeCurrencyRequired = "lease-pipeline-currency-required";
    public const string CodeNothingPending = "lease-pipeline-nothing-pending";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgLeasePipelineService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgPipelineDecision? Pending { get; private set; }

    public async Task<OgResult<OgPipelineSnapshot>> LoadAsync(
        string? fieldId,
        string fieldName,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgPipelineSnapshot>.Failed(
                "No connection is open, so the acquisition pipeline cannot be read.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            var cards = model.Opportunities
                .Where(opportunity => FieldMatches(opportunity.FIELD_ID, fieldId, fieldName))
                .Select(opportunity => Card(opportunity, model, asOf.Date))
                .ToList();

            var columns = BuildColumns(cards);

            return OgResult<OgPipelineSnapshot>.Ok(new OgPipelineSnapshot(
                string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
                columns,
                cards,
                asOf.Date));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the acquisition pipeline",
                "the pipeline is not shown",
                FailureSeverity.Error);

            return OgResult<OgPipelineSnapshot>.Failed(
                "The acquisition pipeline could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgPipelineDetail>> LoadDetailAsync(
        string leaseId,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(leaseId);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPipelineDetail>.Failed(
                "No connection is open, so the opportunity cannot be read.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var detail = Detail(model, leaseId.Trim(), asOf.Date);

            return detail is null
                ? OgResult<OgPipelineDetail>.Failed(
                    $"Opportunity '{leaseId}' is not in this database.", CodeUnknown)
                : OgResult<OgPipelineDetail>.Ok(detail);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reading opportunity {leaseId}",
                "the opportunity is not shown",
                FailureSeverity.Error);

            return OgResult<OgPipelineDetail>.Failed(
                "The opportunity could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgPipelineDetail>> RecordDiligenceAsync(
        string leaseId,
        string category,
        string question,
        string? outcome,
        string? evidenceRef,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPipelineDetail>.Failed(
                "A diligence change must record who made it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            return OgResult<OgPipelineDetail>.Failed(
                "A diligence item needs its question.", "lease-pipeline-question-required");
        }

        var normalised = NormaliseOutcome(outcome);

        if (normalised is null)
        {
            return OgResult<OgPipelineDetail>.Failed(
                $"'{outcome}' is not a diligence outcome; use {string.Join(", ", OgDiligenceItem.Outcomes)}.",
                "lease-pipeline-outcome-invalid");
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPipelineDetail>.Failed(
                "No connection is open, so the diligence item cannot be recorded.", CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<LEASE_DILIGENCE_ITEM>(source);

            // The table's key is (LEASE_ID, ITEM_ID); ITEM_ID is the unique half, so the unit of work can find
            // the exact item to close rather than the first item of the lease.
            unit.PrimaryKey = "ITEM_ID";

            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("LEASE_ID", leaseId.Trim())], cancellationToken)
                .ConfigureAwait(false);

            var existing = rows.FirstOrDefault(row =>
                string.Equals(row.QUESTION?.Trim(), question.Trim(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(row.CATEGORY?.Trim() ?? string.Empty, category?.Trim() ?? string.Empty,
                    StringComparison.OrdinalIgnoreCase));

            var now = DateTime.UtcNow;
            var isOpen = string.Equals(normalised, OgDiligenceItem.Open, StringComparison.OrdinalIgnoreCase);

            if (existing is null)
            {
                unit.Add(new LEASE_DILIGENCE_ITEM
                {
                    LEASE_ID = leaseId.Trim(),
                    ITEM_ID = Guid.NewGuid().ToString("N"),
                    CATEGORY = Blank(category),
                    QUESTION = question.Trim(),
                    OUTCOME = normalised,
                    EVIDENCE_REF = Blank(evidenceRef),
                    REVIEWER = isOpen ? null : actor.Trim(),
                    REVIEWED_AT = isOpen ? null : now,
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }
            else
            {
                existing.CATEGORY = Blank(category);
                existing.OUTCOME = normalised;
                existing.EVIDENCE_REF = Blank(evidenceRef);
                existing.REVIEWER = isOpen ? null : actor.Trim();
                existing.REVIEWED_AT = isOpen ? null : now;
                existing.ROW_CHANGED_BY = actor.Trim();
                existing.ROW_CHANGED_DATE = now;

                var updated = unit.Update(existing);

                if (!OgUow.IsOk(updated))
                {
                    return OgResult<OgPipelineDetail>.Failed(
                        OgUow.Message(updated, "The diligence item was not recorded."), CodeWriteFailed);
                }
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgPipelineDetail>.Failed(
                    OgUow.Message(committed, "The diligence item was not recorded."), CodeWriteFailed);
            }

            return await DetailAfterWriteAsync(source, leaseId.Trim(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"recording diligence for {leaseId}",
                "the diligence item was not recorded",
                FailureSeverity.Error);

            return OgResult<OgPipelineDetail>.Failed(
                "The diligence item could not be recorded.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPipelineDetail>> RecordNegotiationAsync(
        string leaseId,
        string counterparty,
        int? round,
        decimal offerAmount,
        string currency,
        string? notes,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPipelineDetail>.Failed(
                "A negotiation round must record who made it.", CodeActorRequired);
        }

        // The state table refuses a negotiation without a counterparty.
        if (string.IsNullOrWhiteSpace(counterparty))
        {
            return OgResult<OgPipelineDetail>.Failed(
                "A negotiation round needs the counterparty it was held with.", CodeCounterpartyRequired);
        }

        // AC-5: a money value needs its currency, not just a number.
        if (string.IsNullOrWhiteSpace(currency))
        {
            return OgResult<OgPipelineDetail>.Failed(
                "An offer needs the currency it is in.", CodeCurrencyRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPipelineDetail>.Failed(
                "No connection is open, so the negotiation round cannot be recorded.", CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<LEASE_NEGOTIATION>(source);
            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("LEASE_ID", leaseId.Trim())], cancellationToken)
                .ConfigureAwait(false);

            var now = DateTime.UtcNow;
            var next = round is > 0 ? round.Value : (rows.Count == 0 ? 1 : rows.Max(row => row.ROUND_NO) + 1);

            unit.Add(new LEASE_NEGOTIATION
            {
                LEASE_ID = leaseId.Trim(),
                NEGOTIATION_ID = Guid.NewGuid().ToString("N"),
                ROUND_NO = next,
                COUNTERPARTY = counterparty.Trim(),
                OFFER_AMOUNT = offerAmount,
                CURRENCY = currency.Trim().ToUpperInvariant(),
                NOTES = Blank(notes),
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
                return OgResult<OgPipelineDetail>.Failed(
                    OgUow.Message(committed, "The negotiation round was not recorded."), CodeWriteFailed);
            }

            return await DetailAfterWriteAsync(source, leaseId.Trim(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"recording a negotiation round for {leaseId}",
                "the round was not recorded",
                FailureSeverity.Error);

            return OgResult<OgPipelineDetail>.Failed(
                "The negotiation round could not be recorded.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPipelineDetail>> AdvanceAsync(
        OgPipelineDecision decision,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(decision);

        if (string.IsNullOrWhiteSpace(decision.Actor))
        {
            return OgResult<OgPipelineDetail>.Failed(
                "A stage change must record who made it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(decision.TargetStage))
        {
            return OgResult<OgPipelineDetail>.Failed(
                "Choose the stage to move to.", CodeStageRequired);
        }

        if (string.IsNullOrWhiteSpace(decision.Rationale))
        {
            return OgResult<OgPipelineDetail>.Failed(
                "A stage change needs a rationale.", CodeRationaleRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPipelineDetail>.Failed(
                "No connection is open, so the opportunity cannot be moved.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var opportunity = model.Opportunities.FirstOrDefault(row =>
                string.Equals(row.LEASE_ACQUISITION_ID, decision.LeaseId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (opportunity is null)
            {
                return OgResult<OgPipelineDetail>.Failed(
                    $"Opportunity '{decision.LeaseId}' is not in this database.", CodeUnknown);
            }

            var current = CurrentStage(model, opportunity);
            var target = decision.TargetStage.Trim().ToUpperInvariant();

            if (!LeasePipelineStage.IsKnown(target))
            {
                return OgResult<OgPipelineDetail>.Failed(
                    $"'{decision.TargetStage}' is not a pipeline stage; use one of: " +
                    $"{string.Join(", ", LeasePipelineStage.All)}.",
                    CodeStageIllegal);
            }

            if (string.Equals(current, target, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgPipelineDetail>.Failed(
                    $"{opportunity.LEASE_NAME} is already {target}.", CodeStageSame);
            }

            var legal = LeasePipelineStage.LegalFrom(current);

            if (!legal.Contains(target, StringComparer.OrdinalIgnoreCase))
            {
                return OgResult<OgPipelineDetail>.Failed(
                    $"{opportunity.LEASE_NAME} is {current ?? OgLeaseOpportunity.UnclassifiedStage} and cannot move " +
                    $"to {target}. " +
                    (legal.Count == 0
                        ? "It is in a terminal stage."
                        : $"Legal next stage(s): {string.Join(", ", legal)}."),
                    CodeStageIllegal);
            }

            // AC-2: open diligence blocks the move out of due diligence and every stage after it.
            if (LeasePipelineStage.DiligenceGates(current))
            {
                var open = model.Diligence
                    .Where(item => string.Equals(item.LEASE_ID, opportunity.LEASE_ACQUISITION_ID, StringComparison.OrdinalIgnoreCase))
                    .Where(item => string.IsNullOrWhiteSpace(item.OUTCOME)
                                   || string.Equals(item.OUTCOME.Trim(), OgDiligenceItem.Open, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (open.Count > 0)
                {
                    return OgResult<OgPipelineDetail>.Failed(
                        $"{open.Count.ToString(CultureInfo.InvariantCulture)} diligence item(s) are still open: " +
                        $"{string.Join("; ", open.Take(5).Select(item => item.QUESTION))}. Close them before advancing.",
                        CodeDiligenceOpen);
                }
            }

            var decidedAt = decision.DecidedAt ?? DateTimeOffset.UtcNow;
            var sequence = model.Stages
                .Where(row => string.Equals(row.LEASE_ID, opportunity.LEASE_ACQUISITION_ID, StringComparison.OrdinalIgnoreCase))
                .Select(row => row.STAGE_SEQ_NO)
                .DefaultIfEmpty(0)
                .Max() + 1;

            var stageUnit = OgUow.For<LEASE_PIPELINE_STAGE>(source);

            stageUnit.Add(new LEASE_PIPELINE_STAGE
            {
                STAGE_ID = Guid.NewGuid().ToString("N"),
                LEASE_ID = opportunity.LEASE_ACQUISITION_ID!,
                STAGE_SEQ_NO = sequence,
                STAGE = target,
                OUTCOME = string.Equals(target, LeasePipelineStage.Declined, StringComparison.OrdinalIgnoreCase)
                    ? "DECLINED"
                    : "ADVANCED",
                RATIONALE = decision.Rationale.Trim(),
                ACTOR = decision.Actor.Trim(),
                DECIDED_AT = decidedAt.UtcDateTime,
                ROW_CREATED_BY = decision.Actor.Trim(),
                ROW_CREATED_DATE = decidedAt.UtcDateTime,
                ROW_CHANGED_BY = decision.Actor.Trim(),
                ROW_CHANGED_DATE = decidedAt.UtcDateTime,
            });

            var staged = await stageUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(staged))
            {
                // The original is retained unchanged so a retry sends the same stage, rationale and actor.
                Pending = decision;
                return OgResult<OgPipelineDetail>.Failed(
                    $"The stage change was not acknowledged: {OgUow.Message(staged, "the row was not written")}. " +
                    "Retry sends the same change.",
                    CodeWriteFailed);
            }

            var wellUnit = OgUow.For<LEASE_ACQUISITION>(source);
            var row = (await OgUow.ReadAsync(wellUnit,
                    [OgUow.Equal("LEASE_ACQUISITION_ID", opportunity.LEASE_ACQUISITION_ID)],
                    cancellationToken).ConfigureAwait(false))
                .FirstOrDefault();

            if (row is not null)
            {
                row.ACQUISITION_STATUS = target;
                row.ROW_CHANGED_BY = decision.Actor.Trim();
                row.ROW_CHANGED_DATE = decidedAt.UtcDateTime;

                var updated = wellUnit.Update(row);

                if (!OgUow.IsOk(updated))
                {
                    Pending = decision;
                    return OgResult<OgPipelineDetail>.Failed(
                        OgUow.Message(updated, "The opportunity's stage was not updated."), CodeWriteFailed);
                }

                var committed = await wellUnit.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(committed))
                {
                    Pending = decision;
                    return OgResult<OgPipelineDetail>.Failed(
                        OgUow.Message(committed, "The opportunity's stage was not updated."), CodeWriteFailed);
                }
            }

            Pending = null;
            return await DetailAfterWriteAsync(source, opportunity.LEASE_ACQUISITION_ID!, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = decision;
            var reference = _failures.ReportHandled(
                ex,
                $"moving opportunity {decision.LeaseId}",
                "the stage change was not applied; the original is retained for retry",
                FailureSeverity.Error);

            return OgResult<OgPipelineDetail>.Failed(
                "The opportunity could not be moved; retry sends the same change.", reference ?? CodeWriteFailed);
        }
    }

    public Task<OgResult<OgPipelineDetail>> DeclineAsync(
        string leaseId,
        string rationale,
        string actor,
        CancellationToken cancellationToken = default) =>
        // AC-4: a decline is a recorded stage transition like any other, so the history is kept.
        AdvanceAsync(
            new OgPipelineDecision(leaseId, LeasePipelineStage.Declined, rationale, actor),
            cancellationToken);

    public async Task<OgResult<OgPipelineDetail>> RetryAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgPipelineDetail>.Failed(
                "There is no stage change waiting to be retried.", CodeNothingPending);
        }

        return await AdvanceAsync(pending, cancellationToken).ConfigureAwait(false);
    }

    // ------------------------------------------------------------------ reads

    private static async Task<PipelineModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var opportunities = await OgUow.ReadAllAsync<LEASE_ACQUISITION>(source).ConfigureAwait(false);
        var stages = await OgUow.ReadAllAsync<LEASE_PIPELINE_STAGE>(source).ConfigureAwait(false);
        var diligence = await OgUow.ReadAllAsync<LEASE_DILIGENCE_ITEM>(source).ConfigureAwait(false);
        var rights = await OgUow.ReadAllAsync<LEASE_RIGHT>(source).ConfigureAwait(false);
        var negotiations = await OgUow.ReadAllAsync<LEASE_NEGOTIATION>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new PipelineModel(opportunities, stages, diligence, rights, negotiations);
    }

    private static OgPipelineCard Card(LEASE_ACQUISITION row, PipelineModel model, DateTime asOf)
    {
        var id = row.LEASE_ACQUISITION_ID ?? string.Empty;
        var stage = CurrentStage(model, row);
        var entered = model.Stages
            .Where(history => string.Equals(history.LEASE_ID, id, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(history => history.STAGE_SEQ_NO)
            .FirstOrDefault()?.DECIDED_AT ?? row.ACQUISITION_DATE;

        return new OgPipelineCard(
            new OgLeaseOpportunity(
                id,
                Blank(row.LEASE_NAME) ?? id,
                Blank(row.STATE_ID),
                row.GROSS_ACREAGE == 0 ? null : row.GROSS_ACREAGE,
                Blank(row.ACREAGE_OUOM),
                row.BONUS_AMOUNT == 0 ? null : row.BONUS_AMOUNT,
                Blank(row.BONUS_AMOUNT_OUOM),
                stage,
                Blank(row.OPERATOR),
                entered,
                entered is null ? null : Math.Max(0, (asOf.Date - entered.Value.Date).Days)),
            model.Diligence.Count(item =>
                string.Equals(item.LEASE_ID, id, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(item.OUTCOME)
                    || string.Equals(item.OUTCOME.Trim(), OgDiligenceItem.Open, StringComparison.OrdinalIgnoreCase))),
            model.Rights.Count(right => string.Equals(right.LEASE_ID, id, StringComparison.OrdinalIgnoreCase)),
            model.Negotiations.Count(round => string.Equals(round.LEASE_ID, id, StringComparison.OrdinalIgnoreCase)));
    }

    private static OgPipelineDetail? Detail(PipelineModel model, string leaseId, DateTime asOf)
    {
        var row = model.Opportunities.FirstOrDefault(opportunity =>
            string.Equals(opportunity.LEASE_ACQUISITION_ID, leaseId, StringComparison.OrdinalIgnoreCase));

        if (row is null)
        {
            return null;
        }

        var id = row.LEASE_ACQUISITION_ID ?? leaseId;

        var diligence = model.Diligence
            .Where(item => string.Equals(item.LEASE_ID, id, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.CATEGORY, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.QUESTION, StringComparer.OrdinalIgnoreCase)
            .Select(item => new OgDiligenceItem(
                item.ITEM_ID ?? string.Empty,
                Blank(item.CATEGORY) ?? "(uncategorised)",
                Blank(item.QUESTION) ?? string.Empty,
                Blank(item.OUTCOME),
                Blank(item.EVIDENCE_REF),
                Blank(item.REVIEWER),
                item.REVIEWED_AT))
            .ToList();

        var rights = model.Rights
            .Where(right => string.Equals(right.LEASE_ID, id, StringComparison.OrdinalIgnoreCase))
            .OrderBy(right => right.RIGHT_TYPE, StringComparer.OrdinalIgnoreCase)
            .ThenBy(right => right.OWNER, StringComparer.OrdinalIgnoreCase)
            .Select(right => new OgLeaseRight(
                Blank(right.RIGHT_TYPE) ?? "(untyped)",
                Blank(right.OWNER),
                right.INTEREST_PERCENT,
                right.EFFECTIVE_DATE,
                right.EXPIRY_DATE,
                !string.Equals(right.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var negotiations = model.Negotiations
            .Where(round => string.Equals(round.LEASE_ID, id, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(round => round.ROUND_NO)
            .Select(round => new OgNegotiationEntry(
                round.ROUND_NO,
                Blank(round.COUNTERPARTY),
                round.OFFER_AMOUNT,
                Blank(round.CURRENCY),
                Blank(round.NOTES),
                Blank(round.ACTOR),
                round.RECORDED_AT))
            .ToList();

        var history = model.Stages
            .Where(entry => string.Equals(entry.LEASE_ID, id, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(entry => entry.STAGE_SEQ_NO)
            .Select(entry => new OgPipelineHistoryEntry(
                entry.STAGE_SEQ_NO,
                Blank(entry.STAGE) ?? OgLeaseOpportunity.UnclassifiedStage,
                Blank(entry.OUTCOME),
                Blank(entry.RATIONALE),
                Blank(entry.ACTOR),
                entry.DECIDED_AT))
            .ToList();

        var card = Card(row, model, asOf);

        return new OgPipelineDetail(card.Opportunity, diligence, rights, negotiations, history);
    }

    private static string? CurrentStage(PipelineModel model, LEASE_ACQUISITION row)
    {
        var id = row.LEASE_ACQUISITION_ID ?? string.Empty;

        var recorded = model.Stages
            .Where(entry => string.Equals(entry.LEASE_ID, id, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(entry => entry.STAGE_SEQ_NO)
            .FirstOrDefault()?.STAGE;

        if (LeasePipelineStage.IsKnown(recorded))
        {
            return recorded!.Trim().ToUpperInvariant();
        }

        return LeasePipelineStage.IsKnown(row.ACQUISITION_STATUS)
            ? row.ACQUISITION_STATUS!.Trim().ToUpperInvariant()
            : null;
    }

    /// <summary>The board's columns: every forward stage, decline when used, then the unclassified column.</summary>
    public static IReadOnlyList<OgPipelineColumn> BuildColumns(IReadOnlyList<OgPipelineCard> cards)
    {
        var stages = new List<string>(LeasePipelineStage.Order);

        if (cards.Any(card => string.Equals(card.StageLabel, LeasePipelineStage.Declined, StringComparison.OrdinalIgnoreCase)))
        {
            stages.Add(LeasePipelineStage.Declined);
        }

        var columns = new List<OgPipelineColumn>(stages.Count + 1);

        for (var index = 0; index < stages.Count; index++)
        {
            var stage = stages[index];
            var inStage = cards
                .Where(card => string.Equals(card.StageLabel, stage, StringComparison.OrdinalIgnoreCase))
                .OrderBy(card => card.Opportunity.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(card => card.Id, StringComparer.Ordinal)
                .ToList();

            columns.Add(new OgPipelineColumn(stage, index, false, inStage.Count, inStage));
        }

        var unclassified = cards
            .Where(card => !LeasePipelineStage.IsKnown(card.Opportunity.Stage))
            .OrderBy(card => card.Opportunity.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(card => card.Id, StringComparer.Ordinal)
            .ToList();

        if (unclassified.Count > 0)
        {
            columns.Add(new OgPipelineColumn(
                OgLeaseOpportunity.UnclassifiedStage,
                int.MaxValue,
                true,
                unclassified.Count,
                unclassified));
        }

        return columns;
    }

    private async Task<OgResult<OgPipelineDetail>> DetailAfterWriteAsync(
        IDataSource source,
        string leaseId,
        CancellationToken cancellationToken)
    {
        var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
        var detail = Detail(model, leaseId, DateTime.Today);

        return detail is null
            ? OgResult<OgPipelineDetail>.Failed(
                $"Opportunity '{leaseId}' is not in this database.", CodeUnknown)
            : OgResult<OgPipelineDetail>.Ok(detail);
    }

    private static string? NormaliseOutcome(string? outcome)
    {
        if (string.IsNullOrWhiteSpace(outcome))
        {
            return OgDiligenceItem.Open;
        }

        var match = OgDiligenceItem.Outcomes.FirstOrDefault(candidate =>
            string.Equals(candidate, outcome.Trim(), StringComparison.OrdinalIgnoreCase));

        return match;
    }

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
         && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record PipelineModel(
        IReadOnlyList<LEASE_ACQUISITION> Opportunities,
        IReadOnlyList<LEASE_PIPELINE_STAGE> Stages,
        IReadOnlyList<LEASE_DILIGENCE_ITEM> Diligence,
        IReadOnlyList<LEASE_RIGHT> Rights,
        IReadOnlyList<LEASE_NEGOTIATION> Negotiations);
}
