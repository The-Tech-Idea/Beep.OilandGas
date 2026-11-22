using System.Globalization;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;

/// <summary>One stage a prospect has been through (a <c>PROSPECT_WORKFLOW_STAGE</c> row).</summary>
public sealed record OgProspectStageRow(
    string Stage,
    int Order,
    DateTime? StartedAt,
    DateTime? EndedAt,
    string? Outcome);

/// <summary>
/// EXP-02: one card on the maturation board.
/// </summary>
/// <param name="Stage">The current stage code, or null when no stage row records one (AC-2).</param>
/// <param name="StageOrder">The recorded sequence number of the current stage; the column order comes from it.</param>
/// <param name="ChancePercent">
/// The chance of success derived from the recorded overall geological risk (1 - risk), or null. It is null when the
/// assessment is incomplete, so nothing invents a number for a prospect whose risk is not recorded.
/// </param>
/// <param name="RiskComplete">True only when all five risk elements are recorded (AC-3).</param>
public sealed record OgProspectCard(
    string Id,
    string Name,
    string? Play,
    string? Basin,
    string? Owner,
    string? Stage,
    int StageOrder,
    DateTime? StageEnteredAt,
    int? StageAgeDays,
    decimal? ChancePercent,
    bool RiskComplete,
    IReadOnlyList<OgProspectStageRow> History)
{
    /// <summary>The column a prospect with no recorded stage is shown in (AC-2).</summary>
    public const string UnclassifiedStage = "(unclassified)";

    public string StageLabel => string.IsNullOrWhiteSpace(Stage) ? UnclassifiedStage : Stage!;

    /// <summary>AC-3: "incomplete" is a state, not a zero chance.</summary>
    public string ChanceText => !RiskComplete
        ? "incomplete"
        : ChancePercent is { } chance
            ? $"{chance.ToString("0", CultureInfo.InvariantCulture)}%"
            : "no chance recorded";

    public string AgeText => StageAgeDays is { } days
        ? $"{days.ToString(CultureInfo.InvariantCulture)} d"
        : "unknown";

    public string Describe() =>
        string.Join(
            " · ",
            new[]
            {
                Name,
                Play,
                StageLabel,
                $"chance {ChanceText}",
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
}

/// <summary>One stage column: its cards, its count and what its risk figures say.</summary>
public sealed record OgProspectColumn(
    string Stage,
    int Order,
    bool IsUnclassified,
    int Count,
    decimal? AverageChancePercent,
    int IncompleteRiskCount,
    IReadOnlyList<OgProspectCard> Cards)
{
    public string Header => $"{Stage} ({Count.ToString(CultureInfo.InvariantCulture)})";

    public string RiskText => AverageChancePercent is { } chance
        ? $"avg chance {chance.ToString("0", CultureInfo.InvariantCulture)}%" +
          (IncompleteRiskCount == 0
              ? string.Empty
              : $" · {IncompleteRiskCount.ToString(CultureInfo.InvariantCulture)} incomplete")
        : IncompleteRiskCount == 0
            ? "no comparable chance"
            : $"{IncompleteRiskCount.ToString(CultureInfo.InvariantCulture)} incomplete";
}

/// <summary>The board's filters. Every one that is set must match.</summary>
public sealed record OgProspectFilter(
    string? Play = null,
    string? Owner = null,
    string? Basin = null,
    decimal? MinimumChancePercent = null,
    int? MaximumStageAgeDays = null)
{
    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(Play)
        || !string.IsNullOrWhiteSpace(Owner)
        || !string.IsNullOrWhiteSpace(Basin)
        || MinimumChancePercent is not null
        || MaximumStageAgeDays is not null;

    public string Describe() =>
        string.Join(
            " · ",
            new[]
            {
                string.IsNullOrWhiteSpace(Play) ? null : $"play {Play!.Trim()}",
                string.IsNullOrWhiteSpace(Owner) ? null : $"owner {Owner!.Trim()}",
                string.IsNullOrWhiteSpace(Basin) ? null : $"basin {Basin!.Trim()}",
                MinimumChancePercent is { } chance
                    ? $"chance >= {chance.ToString("0", CultureInfo.InvariantCulture)}%"
                    : null,
                MaximumStageAgeDays is { } age
                    ? $"stage age <= {age.ToString(CultureInfo.InvariantCulture)} d"
                    : null,
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
}

/// <summary>The board: its columns in recorded order, the cards shown, and what the filters could offer.</summary>
public sealed record OgProspectBoardSnapshot(
    string FieldName,
    IReadOnlyList<OgProspectColumn> Columns,
    IReadOnlyList<OgProspectCard> Cards,
    IReadOnlyList<string> Plays,
    IReadOnlyList<string> Owners,
    IReadOnlyList<string> Basins,
    DateTime AsOf)
{
    public int CardCount => Cards.Count;

    public int ColumnCount => Columns.Count;

    public bool IsEmpty => Cards.Count == 0;

    /// <summary>Cards whose risk is not complete, across the shown cards (AC-3).</summary>
    public int IncompleteRiskCount => Cards.Count(card => !card.RiskComplete);
}

/// <summary>
/// EXP-02: the prospect portfolio as a stage board, plus the single advance command.
/// </summary>
public interface IOgProspectBoard
{
    Task<OgResult<OgProspectBoardSnapshot>> LoadAsync(
        string? fieldId,
        string fieldName,
        OgProspectFilter? filter,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The menu command. STG-14 wires the process engine; until then this refuses with a stated reason and writes
    /// nothing (AC-4).
    /// </summary>
    Task<OgResult> AdvanceAsync(
        OgProspectCard card,
        string? targetStage,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The drag-and-drop entry point. It is deliberately the same command, so a drop can never take a path the menu
    /// does not (AC-5).
    /// </summary>
    Task<OgResult> MoveAsync(
        OgProspectCard card,
        string? targetStage,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The board over <c>PROSPECT</c>, <c>PLAY</c>, <c>PROSPECT_RISK_ASSESSMENT</c> and
/// <c>PROSPECT_WORKFLOW_STAGE</c>, read through their entity models.
/// </summary>
/// <remarks>
/// The current stage is the row with the highest recorded <c>STAGE_SEQ_NO</c> (then the latest start), and the
/// columns are ordered by that recorded sequence rather than by the code's spelling, because the sequence is the
/// reference order the process wrote (AC-1). A prospect with no stage row is shown in an explicit unclassified
/// column rather than dropped (AC-2). A card's chance is derived from the recorded overall geological risk only
/// when the five elements are complete; otherwise it reads "incomplete" and no number is produced (AC-3).
/// </remarks>
public sealed class OgProspectBoard : IOgProspectBoard
{
    public const string CodeNoConnection = "prospect-board-no-connection";
    public const string CodeReadFailed = "prospect-board-read-failed";

    /// <summary>AC-4: the stage change needs the process engine (WFL-04, STG-14).</summary>
    public const string CodeAdvanceNotWired = "prospect-advance-engine-not-wired";

    public const string CodeTargetRequired = "prospect-advance-target-required";
    public const string CodeActorRequired = "prospect-advance-actor-required";
    public const string CodeUnchanged = "prospect-advance-unchanged";

    /// <summary>The five elements that make a geological risk assessment complete (AC-3).</summary>
    public static IReadOnlyList<string> RiskElements => OgProspectChance.RequiredElements;

    private readonly Func<IDataSource?> _source;
    private readonly TheTechIdeaWeb.Diagnostics.IFailureReporter _failures;

    public OgProspectBoard(Func<IDataSource?> source, TheTechIdeaWeb.Diagnostics.IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgProspectBoardSnapshot>> LoadAsync(
        string? fieldId,
        string fieldName,
        OgProspectFilter? filter,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        IDataSource? source;

        try
        {
            source = _source();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "opening the connection for the prospect board",
                "the board is not read",
                TheTechIdeaWeb.Diagnostics.FailureSeverity.Error);

            return OgResult<OgProspectBoardSnapshot>.Failed(
                "The prospect board could not be read.", reference ?? CodeNoConnection);
        }

        if (source is null)
        {
            return OgResult<OgProspectBoardSnapshot>.Failed(
                "No connection is open, so the prospect board cannot be read.", CodeNoConnection);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var prospects = await OgUow.ReadAllAsync<PROSPECT>(source).ConfigureAwait(false);
            var plays = await OgUow.ReadAllAsync<PLAY>(source).ConfigureAwait(false);
            var stages = await OgUow.ReadAllAsync<PROSPECT_WORKFLOW_STAGE>(source).ConfigureAwait(false);
            var assessments = await OgUow.ReadAllAsync<PROSPECT_RISK_ASSESSMENT>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var cards = ReadCards(prospects, plays, stages, assessments, fieldId, fieldName, asOf.Date);
            var shown = ApplyFilter(cards, filter).ToList();
            var columns = BuildColumns(cards, shown);

            var snapshot = new OgProspectBoardSnapshot(
                string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
                columns,
                shown,
                Options(cards.Select(card => card.Play)),
                Options(cards.Select(card => card.Owner)),
                Options(cards.Select(card => card.Basin)),
                asOf.Date);

            return OgResult<OgProspectBoardSnapshot>.Ok(snapshot);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the prospect board",
                "the board keeps what it showed and reports the failure",
                TheTechIdeaWeb.Diagnostics.FailureSeverity.Error);

            return OgResult<OgProspectBoardSnapshot>.Failed(
                "The prospect board could not be read; the board keeps what it showed.",
                reference ?? CodeReadFailed);
        }
    }

    public Task<OgResult> AdvanceAsync(
        OgProspectCard card,
        string? targetStage,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(card);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return Task.FromResult(OgResult.Failed(
                "A stage change must record who asked for it.", CodeActorRequired));
        }

        if (string.IsNullOrWhiteSpace(targetStage))
        {
            return Task.FromResult(OgResult.Failed(
                "Choose the stage to advance to.", CodeTargetRequired));
        }

        if (string.Equals(card.StageLabel, targetStage.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(OgResult.Failed(
                $"{card.Name} is already in {card.StageLabel}.", CodeUnchanged));
        }

        // AC-4: no write. The process engine owns the gate, the approval and the history row; a stage written here
        // would be a change with no process behind it.
        return Task.FromResult(OgResult.Failed(
            $"Advancing {card.Name} from {card.StageLabel} to {targetStage.Trim()} is refused: the process engine " +
            "that runs the maturation gate is not wired in this build (WFL-04, STG-14). Nothing was written, so the " +
            "prospect stays where it is.",
            CodeAdvanceNotWired));
    }

    public Task<OgResult> MoveAsync(
        OgProspectCard card,
        string? targetStage,
        string actor,
        CancellationToken cancellationToken = default) =>
        // AC-5: a drop is the menu command, not a second path.
        AdvanceAsync(card, targetStage, actor, cancellationToken);

    /// <summary>
    /// The columns: every current stage in recorded sequence order, with the unclassified column last. Columns are
    /// built from all cards so a filter hides cards but never hides a stage (AC-1, AC-2).
    /// </summary>
    public static IReadOnlyList<OgProspectColumn> BuildColumns(
        IReadOnlyList<OgProspectCard> all,
        IReadOnlyList<OgProspectCard> shown)
    {
        ArgumentNullException.ThrowIfNull(all);
        ArgumentNullException.ThrowIfNull(shown);

        var order = all
            .GroupBy(card => card.StageLabel, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Min(card => card.StageOrder),
                StringComparer.OrdinalIgnoreCase);

        var columns = new List<OgProspectColumn>(order.Count);

        foreach (var stage in order.Keys
                     .OrderBy(key => order[key])
                     .ThenBy(key => key, StringComparer.OrdinalIgnoreCase))
        {
            var cards = shown
                .Where(card => string.Equals(card.StageLabel, stage, StringComparison.OrdinalIgnoreCase))
                .OrderBy(card => card.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(card => card.Id, StringComparer.Ordinal)
                .ToList();

            var chances = cards
                .Where(card => card.RiskComplete && card.ChancePercent is not null)
                .Select(card => card.ChancePercent!.Value)
                .ToList();

            columns.Add(new OgProspectColumn(
                stage,
                order[stage],
                IsUnclassified: string.Equals(stage, OgProspectCard.UnclassifiedStage, StringComparison.Ordinal),
                cards.Count,
                chances.Count == 0 ? null : Math.Round(chances.Average(), 0),
                cards.Count(card => !card.RiskComplete),
                cards));
        }

        return columns;
    }

    /// <summary>The five element risks, keyed by the shared element names.</summary>
    public static IReadOnlyDictionary<string, decimal?> Elements(OgRisk risk)
    {
        ArgumentNullException.ThrowIfNull(risk);

        return new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase)
        {
            [OgProspectChance.RequiredElements[0]] = risk.Trap,
            [OgProspectChance.RequiredElements[1]] = risk.Reservoir,
            [OgProspectChance.RequiredElements[2]] = risk.Seal,
            [OgProspectChance.RequiredElements[3]] = risk.Source,
            [OgProspectChance.RequiredElements[4]] = risk.Timing,
        };
    }

    private static List<OgProspectCard> ReadCards(
        IReadOnlyList<PROSPECT> prospects,
        IReadOnlyList<PLAY> plays,
        IReadOnlyList<PROSPECT_WORKFLOW_STAGE> stages,
        IReadOnlyList<PROSPECT_RISK_ASSESSMENT> assessments,
        string? fieldId,
        string? fieldName,
        DateTime asOf)
    {
        var playById = plays
            .Where(play => !string.IsNullOrWhiteSpace(play.PLAY_ID))
            .GroupBy(play => play.PLAY_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var newest = assessments
            .Where(row => !string.IsNullOrWhiteSpace(row.PROSPECT_ID))
            .GroupBy(row => row.PROSPECT_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(row => row.ASSESSMENT_ID, StringComparer.Ordinal)
                    .First(),
                StringComparer.OrdinalIgnoreCase);

        var history = stages
            .Where(row => !string.IsNullOrWhiteSpace(row.PROSPECT_ID))
            .GroupBy(row => row.PROSPECT_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(row => row.STAGE_SEQ_NO)
                    .ThenByDescending(row => row.STAGE_START_DATE)
                    .Select(row => new OgProspectStageRow(
                        Blank(row.WORKFLOW_STAGE) ?? OgProspectCard.UnclassifiedStage,
                        (int)row.STAGE_SEQ_NO,
                        row.STAGE_START_DATE,
                        row.STAGE_END_DATE,
                        Blank(row.STAGE_OUTCOME)))
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);

        var cards = new List<OgProspectCard>();

        foreach (var prospect in prospects
                     .Where(prospect => FieldMatches(prospect.FIELD_ID, fieldId, fieldName)
                                        || FieldMatches(prospect.PRIMARY_FIELD_ID, fieldId, fieldName))
                     .OrderBy(prospect => prospect.PROSPECT_NAME, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(prospect => prospect.PROSPECT_ID, StringComparer.Ordinal))
        {
            var id = Blank(prospect.PROSPECT_ID);

            if (id is null)
            {
                continue;
            }

            playById.TryGetValue(prospect.PLAY_ID ?? string.Empty, out var play);
            newest.TryGetValue(id, out var assessment);
            history.TryGetValue(id, out var rows);

            var current = rows?.FirstOrDefault();
            var entered = current?.StartedAt?.Date;
            int? age = entered is null ? null : Math.Max(0, (asOf - entered.Value).Days);

            var risks = assessment is null
                ? null
                : new OgRisk(
                    assessment.TRAP_RISK,
                    assessment.RESERVOIR_RISK,
                    assessment.SEAL_RISK,
                    assessment.SOURCE_RISK,
                    assessment.TIMING_RISK,
                    assessment.OVERALL_GEOLOGICAL_RISK);

            var elements = risks is null ? null : Elements(risks);
            var complete = elements is not null && OgProspectChance.IsComplete(elements);

            cards.Add(new OgProspectCard(
                id,
                Blank(prospect.PROSPECT_NAME) ?? id,
                Blank(play?.PLAY_NAME) ?? Blank(prospect.PLAY_ID),
                Blank(play?.BASIN_ID),
                Blank(prospect.CURRENT_OPERATOR),
                current?.Stage,
                current?.Order ?? int.MaxValue,
                current?.StartedAt,
                age,
                elements is null ? null : OgProspectChance.Of(elements),
                complete,
                rows ?? []));
        }

        return cards;
    }

    /// <summary>True only when all five elements carry a usable probability (AC-3).</summary>
    public static bool IsComplete(OgRisk risk) => OgProspectChance.IsComplete(Elements(risk));

    private static IEnumerable<OgProspectCard> ApplyFilter(
        IEnumerable<OgProspectCard> cards,
        OgProspectFilter? filter)
    {
        if (filter is null || !filter.HasFilters)
        {
            return cards;
        }

        var filtered = cards;

        if (!string.IsNullOrWhiteSpace(filter.Play))
        {
            filtered = filtered.Where(card => Matches(card.Play, filter.Play));
        }

        if (!string.IsNullOrWhiteSpace(filter.Owner))
        {
            filtered = filtered.Where(card => Matches(card.Owner, filter.Owner));
        }

        if (!string.IsNullOrWhiteSpace(filter.Basin))
        {
            filtered = filtered.Where(card => Matches(card.Basin, filter.Basin));
        }

        if (filter.MinimumChancePercent is { } minimum)
        {
            // A prospect with no recorded chance cannot be shown to meet a minimum, so it is not shown under one.
            filtered = filtered.Where(card =>
                card.RiskComplete && card.ChancePercent is { } chance && chance >= minimum);
        }

        if (filter.MaximumStageAgeDays is { } maximumAge)
        {
            filtered = filtered.Where(card =>
                card.StageAgeDays is { } age && age <= maximumAge);
        }

        return filtered;
    }

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
         && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));

    private static bool Matches(string? value, string? expected) =>
        string.Equals(value ?? string.Empty, expected!.Trim(), StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<string> Options(IEnumerable<string?> values) =>
        values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}

/// <summary>The five risk elements and the recorded overall risk for one prospect.</summary>
public sealed record OgRisk(
    decimal? Trap,
    decimal? Reservoir,
    decimal? Seal,
    decimal? Source,
    decimal? Timing,
    decimal? OverallGeologicalRisk);
