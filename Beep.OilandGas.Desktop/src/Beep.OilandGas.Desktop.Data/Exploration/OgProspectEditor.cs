using System.Globalization;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;

/// <summary>One element of a prospect's geological risk (EXP-03 AC-5 carries its assessor and date).</summary>
public sealed record OgProspectRiskElement(
    string Element,
    decimal? Probability,
    string? Rationale,
    string? Assessor,
    DateTime? AssessedAt)
{
    /// <summary>True when the element carries a usable 0..1 probability.</summary>
    public bool IsRecorded => OgProspectChance.IsUsable(Probability);

    public string ProbabilityText => Probability is { } risk
        ? $"{(risk * 100m).ToString("0", CultureInfo.InvariantCulture)}%"
        : "-";
}

/// <summary>
/// The prospect's volume percentiles. Low is P90 (proved), Mid is P50 (best), High is P10 (possible) - the
/// convention the risk-assessment table's LOW/BEST/HIGH estimate columns carry.
/// </summary>
public sealed record OgProspectVolume(
    decimal? Low,
    decimal? Mid,
    decimal? High,
    string? Unit,
    string? Fluid)
{
    public bool HasUnit => !string.IsNullOrWhiteSpace(Unit);

    /// <summary>AC-3: the percentiles must not cross.</summary>
    public bool IsOrdered =>
        Low is not { } low || Mid is not { } mid || High is not { } high || (low <= mid && mid <= high);

    public string Describe() =>
        HasUnit
            ? $"P90 {Number(Low)} · P50 {Number(Mid)} · P10 {Number(High)} {Unit!.Trim()}"
            : $"P90 {Number(Low)} · P50 {Number(Mid)} · P10 {Number(High)} (unit not recorded)";

    private static string Number(decimal? value) =>
        value?.ToString("0.###", CultureInfo.InvariantCulture) ?? "-";
}

/// <summary>The identity tab.</summary>
public sealed record OgProspectIdentity(
    string Id,
    string Name,
    string? PlayId,
    string? PlayName,
    string? FieldId,
    string? OperatorId,
    string? Type,
    string Status,
    string? Description);

/// <summary>The trap tab (read-only here; the trap tables are captured elsewhere in the plan).</summary>
public sealed record OgProspectTrapDetail(
    string? TrapType,
    decimal? ClosureArea,
    string? ClosureAreaUnit,
    string? SealQuality,
    string? Confidence,
    string? Description);

/// <summary>The reservoir tab.</summary>
public sealed record OgProspectReservoirDetail(
    string? Name,
    string? StratUnitId,
    string? Lithology,
    decimal? Porosity,
    decimal? Permeability,
    decimal? NetPay,
    string? NetPayUnit,
    string? FluidType);

/// <summary>The source-rock tab.</summary>
public sealed record OgProspectSourceDetail(
    string? StratUnitId,
    decimal? Toc,
    string? MaturityStage,
    decimal? MigrationDistance,
    string? MigrationDistanceUnit,
    string? Description);

/// <summary>AC-6: the economic summary, read-only, with the route of the evaluation page that owns it.</summary>
public sealed record OgProspectEconomicSummary(
    string? Scenario,
    DateTime? EvaluatedAt,
    string? Evaluator,
    string? Description,
    decimal? Emv,
    decimal? Npv,
    string? Currency)
{
    public const string EvaluationRoute = "economics";

    public bool HasContent =>
        !string.IsNullOrWhiteSpace(Scenario) || Emv is not null || Npv is not null;
}

/// <summary>Everything the prospect editor shows.</summary>
public sealed record OgProspectDetail(
    OgProspectIdentity Identity,
    OgProspectTrapDetail Trap,
    OgProspectReservoirDetail Reservoir,
    OgProspectSourceDetail Source,
    OgProspectVolume Volume,
    IReadOnlyList<OgProspectRiskElement> Risk,
    decimal? ChancePercent,
    bool RiskComplete,
    OgProspectEconomicSummary Economics,
    IReadOnlyList<OgProspectStageRow> History,
    bool IsLocked)
{
    /// <summary>AC-1 / AC-2: the chance, or "incomplete".</summary>
    public string ChanceText => OgProspectChance.Text(ChancePercent, RiskComplete);

    /// <summary>The elements the chance is still waiting for.</summary>
    public IReadOnlyList<string> MissingElements =>
        Risk.Where(element => !element.IsRecorded).Select(element => element.Element).ToList();
}

/// <summary>EXP-03: the prospect editor and its risk assessment.</summary>
public interface IOgProspectEditor
{
    Task<OgResult<OgProspectDetail>> LoadAsync(string prospectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the identity fields. A submitted prospect is not editable (AC-4), and the update is guarded on the
    /// status the editor read, so a prospect submitted by somebody else in the meantime is a conflict.
    /// </summary>
    Task<OgResult> SaveIdentityAsync(
        OgProspectIdentity original,
        string name,
        string? playId,
        string? operatorId,
        string? description,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records one or more risk elements. Each recorded probability must be 0..1 and every save records its
    /// assessor and date (AC-5). A partial set is allowed - the chance stays incomplete until all five are there
    /// (AC-2) - and the chance is recomputed from the probabilities only (AC-1).
    /// </summary>
    Task<OgResult> SaveRiskAsync(
        string prospectId,
        IReadOnlyList<OgProspectRiskElement> elements,
        string assessor,
        DateTime assessedAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// AC-4: a revision of a submitted prospect is a new prospect row (status DRAFT) with a copy of its assessment,
    /// never an edit of the submitted one.
    /// </summary>
    Task<OgResult<OgProspectDetail>> ReviseAsync(
        string prospectId,
        string newProspectId,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The editor over <c>PROSPECT</c>, <c>PLAY</c>, <c>PROSPECT_TRAP</c>, <c>PROSPECT_RESERVOIR</c>,
/// <c>PROSPECT_SOURCE_ROCK</c>, <c>PROSPECT_VOLUME_ESTIMATE</c>, <c>PROSPECT_RISK_ASSESSMENT</c>,
/// <c>PROSPECT_ECONOMIC</c>, <c>PROSPECT_HISTORY</c> and <c>PROSPECT_WORKFLOW_STAGE</c>, read and written through
/// their entity models and a unit of work.
/// </summary>
public sealed class OgProspectEditor : IOgProspectEditor
{
    public const string CodeNoConnection = "prospect-editor-no-connection";
    public const string CodeReadFailed = "prospect-editor-read-failed";
    public const string CodeWriteFailed = "prospect-editor-write-failed";
    public const string CodeUnknown = "prospect-editor-unknown";
    public const string CodeLocked = "prospect-editor-locked";
    public const string CodeConflict = "prospect-editor-conflict";
    public const string CodeActorRequired = "prospect-editor-actor-required";
    public const string CodeNameRequired = "prospect-editor-name-required";
    public const string CodeProbability = "prospect-editor-probability-invalid";
    public const string CodeNothingToSave = "prospect-editor-nothing-to-save";
    public const string CodeDateRequired = "prospect-editor-date-required";
    public const string CodeRevisionNeedsLocked = "prospect-editor-revision-needs-submitted";
    public const string CodeRevisionExists = "prospect-editor-revision-exists";

    /// <summary>Statuses a prospect cannot be edited in: it has left the working set.</summary>
    public static IReadOnlyList<string> LockedStatuses { get; } =
        ["SUBMITTED", "APPROVED", "CLOSED", "COMPLETED", "ARCHIVED"];

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgProspectEditor(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    /// <summary>AC-4: true when the status means the prospect is no longer editable.</summary>
    public static bool IsLocked(string? status) =>
        !string.IsNullOrWhiteSpace(status)
        && LockedStatuses.Contains(status.Trim(), StringComparer.OrdinalIgnoreCase);

    public async Task<OgResult<OgProspectDetail>> LoadAsync(
        string prospectId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prospectId);

        IDataSource? source;

        try
        {
            source = _source();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "opening the connection for the prospect editor",
                "the prospect is not shown",
                FailureSeverity.Error);

            return OgResult<OgProspectDetail>.Failed(
                "The prospect could not be read.", reference ?? CodeNoConnection);
        }

        if (source is null)
        {
            return OgResult<OgProspectDetail>.Failed(
                "No connection is open, so the prospect cannot be read.", CodeNoConnection);
        }

        try
        {
            var prospect = await ReadProspectAsync(source, prospectId.Trim(), cancellationToken).ConfigureAwait(false);

            if (prospect is null)
            {
                return OgResult<OgProspectDetail>.Failed(
                    $"Prospect '{prospectId}' is not in this database.", CodeUnknown);
            }

            var identity = await ReadIdentityAsync(source, prospect, cancellationToken).ConfigureAwait(false);
            var assessment = await ReadAssessmentAsync(source, identity.Id, cancellationToken).ConfigureAwait(false);

            var risks = assessment is null
                ? OgProspectChance.RequiredElements.ToDictionary(
                    element => element,
                    _ => (decimal?)null,
                    StringComparer.OrdinalIgnoreCase)
                : AssessmentRisks(assessment);

            var complete = OgProspectChance.IsComplete(risks);

            var risk = OgProspectChance.RequiredElements
                .Select(element => new OgProspectRiskElement(
                    element,
                    risks[element],
                    Blank(assessment?.RISK_FACTORS),
                    Blank(assessment?.ASSESSOR),
                    assessment?.ASSESSMENT_DATE))
                .ToList();

            var detail = new OgProspectDetail(
                identity,
                await ReadTrapAsync(source, identity.Id, cancellationToken).ConfigureAwait(false),
                await ReadReservoirAsync(source, identity.Id, cancellationToken).ConfigureAwait(false),
                await ReadSourceAsync(source, identity.Id, cancellationToken).ConfigureAwait(false),
                await ReadVolumeAsync(source, identity.Id, prospect, assessment, cancellationToken).ConfigureAwait(false),
                risk,
                OgProspectChance.Of(risks),
                complete,
                await ReadEconomicsAsync(source, identity.Id, assessment, prospect, cancellationToken).ConfigureAwait(false),
                await ReadHistoryAsync(source, identity.Id, cancellationToken).ConfigureAwait(false),
                IsLocked(identity.Status));

            return OgResult<OgProspectDetail>.Ok(detail);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reading prospect {prospectId}",
                "the prospect is not shown",
                FailureSeverity.Error);

            return OgResult<OgProspectDetail>.Failed(
                "The prospect could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult> SaveIdentityAsync(
        OgProspectIdentity original,
        string name,
        string? playId,
        string? operatorId,
        string? description,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(original);

        if (IsLocked(original.Status))
        {
            return OgResult.Failed(
                $"{original.Name} is {original.Status}. A submitted prospect is not edited; revise it to a new " +
                "version instead.",
                CodeLocked);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return OgResult.Failed("A prospect needs a name.", CodeNameRequired);
        }

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult.Failed("A change must record who made it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult.Failed(
                "No connection is open, so the prospect cannot be saved.", CodeNoConnection);
        }

        try
        {
            // The row is re-read first: a prospect submitted by somebody else is a conflict, not an overwrite.
            var unit = OgUow.For<PROSPECT>(source);
            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("PROSPECT_ID", original.Id)], cancellationToken)
                .ConfigureAwait(false);
            var current = rows.FirstOrDefault();

            if (current is null)
            {
                return OgResult.Failed(
                    $"Prospect '{original.Id}' is no longer in this database.", CodeUnknown);
            }

            var status = current.PROSPECT_STATUS ?? string.Empty;

            if (!string.Equals(status, original.Status, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult.Failed(
                    $"The prospect was {original.Status} when it was opened and is {status} now, so the " +
                    "change was not applied. Reload the editor.",
                    CodeConflict);
            }

            current.PROSPECT_NAME = name.Trim();
            current.PLAY_ID = Trimmed(playId);
            current.CURRENT_OPERATOR = Trimmed(operatorId);
            current.DESCRIPTION = Trimmed(description);
            current.ROW_CHANGED_BY = actor.Trim();
            current.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(current);

            if (!OgUow.IsOk(updated))
            {
                return OgResult.Failed(
                    $"The prospect was not saved: {OgUow.Message(updated, "the row was not updated")}",
                    CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult.Failed(
                    $"The prospect was not saved: {OgUow.Message(committed, "the row was not updated")}",
                    CodeWriteFailed);
            }

            await RecordHistoryAsync(
                source,
                original.Id,
                "IDENTITY",
                original.Name,
                name.Trim(),
                "edit",
                actor,
                cancellationToken).ConfigureAwait(false);

            return OgResult.Ok("Prospect saved.");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving prospect {original.Id}",
                "the change was not applied",
                FailureSeverity.Error);

            return OgResult.Failed("The prospect could not be saved.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult> SaveRiskAsync(
        string prospectId,
        IReadOnlyList<OgProspectRiskElement> elements,
        string assessor,
        DateTime assessedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prospectId);
        ArgumentNullException.ThrowIfNull(elements);

        if (string.IsNullOrWhiteSpace(assessor))
        {
            // AC-5
            return OgResult.Failed(
                "Every risk row records who assessed it, so an assessor is required.", CodeActorRequired);
        }

        if (assessedAt == default)
        {
            return OgResult.Failed(
                "Every risk row records when it was assessed, so a date is required.", CodeDateRequired);
        }

        var recorded = elements
            .Where(element => element.Probability is not null)
            .ToList();

        if (recorded.Count == 0)
        {
            return OgResult.Failed(
                "No probability was entered, so there is nothing to save.", CodeNothingToSave);
        }

        foreach (var element in recorded)
        {
            if (!OgProspectChance.IsUsable(element.Probability))
            {
                return OgResult.Failed(
                    $"{element.Element} is {element.ProbabilityText}; a probability must be between 0 and 1.",
                    CodeProbability);
            }
        }

        var source = _source();

        if (source is null)
        {
            return OgResult.Failed(
                "No connection is open, so the risk cannot be saved.", CodeNoConnection);
        }

        try
        {
            var prospect = await ReadProspectAsync(source, prospectId.Trim(), cancellationToken).ConfigureAwait(false);

            if (prospect is null)
            {
                return OgResult.Failed(
                    $"Prospect '{prospectId}' is not in this database.", CodeUnknown);
            }

            if (IsLocked(prospect.PROSPECT_STATUS))
            {
                return OgResult.Failed(
                    "This prospect is submitted. A submitted prospect is not edited; revise it to a new version.",
                    CodeLocked);
            }

            var existing = await ReadAssessmentAsync(source, prospectId.Trim(), cancellationToken).ConfigureAwait(false);

            var merged = existing is null
                ? OgProspectChance.RequiredElements.ToDictionary(
                    element => element,
                    _ => (decimal?)null,
                    StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, decimal?>(AssessmentRisks(existing), StringComparer.OrdinalIgnoreCase);

            foreach (var element in recorded)
            {
                if (merged.ContainsKey(element.Element))
                {
                    merged[element.Element] = element.Probability;
                }
            }

            var complete = OgProspectChance.IsComplete(merged);
            var success = complete
                ? OgProspectChance.RequiredElements.Aggregate(
                    1m,
                    (product, element) => product * (1m - merged[element]!.Value))
                : (decimal?)null;

            var unit = OgUow.For<PROSPECT_RISK_ASSESSMENT>(source);

            if (existing is not null)
            {
                existing.TRAP_RISK = merged[OgProspectChance.RequiredElements[0]];
                existing.RESERVOIR_RISK = merged[OgProspectChance.RequiredElements[1]];
                existing.SEAL_RISK = merged[OgProspectChance.RequiredElements[2]];
                existing.SOURCE_RISK = merged[OgProspectChance.RequiredElements[3]];
                existing.TIMING_RISK = merged[OgProspectChance.RequiredElements[4]];
                existing.OVERALL_GEOLOGICAL_RISK = success is null ? null : 1m - success;
                existing.POS_GEOLOGICAL = success;
                existing.ASSESSOR = assessor.Trim();
                existing.ASSESSMENT_DATE = assessedAt.Date;
                existing.ROW_CHANGED_BY = assessor.Trim();
                existing.ROW_CHANGED_DATE = DateTime.UtcNow;

                var update = unit.Update(existing);

                if (!OgUow.IsOk(update))
                {
                    return OgResult.Failed(
                        $"The risk was not saved: {OgUow.Message(update, "the row was not updated")}",
                        CodeWriteFailed);
                }
            }
            else
            {
                unit.Add(new PROSPECT_RISK_ASSESSMENT
                {
                    PROSPECT_ID = prospectId.Trim(),
                    ASSESSMENT_ID = Guid.NewGuid().ToString("N"),
                    RISK_MODEL_TYPE = "GEOLOGICAL",
                    TRAP_RISK = merged[OgProspectChance.RequiredElements[0]],
                    RESERVOIR_RISK = merged[OgProspectChance.RequiredElements[1]],
                    SEAL_RISK = merged[OgProspectChance.RequiredElements[2]],
                    SOURCE_RISK = merged[OgProspectChance.RequiredElements[3]],
                    TIMING_RISK = merged[OgProspectChance.RequiredElements[4]],
                    OVERALL_GEOLOGICAL_RISK = success is null ? null : 1m - success,
                    POS_GEOLOGICAL = success,
                    ASSESSOR = assessor.Trim(),
                    ASSESSMENT_DATE = assessedAt.Date,
                    ROW_CREATED_BY = assessor.Trim(),
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    ROW_CHANGED_BY = assessor.Trim(),
                    ROW_CHANGED_DATE = DateTime.UtcNow,
                });
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult.Failed(
                    $"The risk was not saved: {OgUow.Message(committed, "the row was not written")}",
                    CodeWriteFailed);
            }

            await RecordHistoryAsync(
                source,
                prospectId.Trim(),
                "RISK",
                existing is null ? "(none)" : "recorded",
                complete ? $"chance {OgProspectChance.Text(OgProspectChance.Of(merged), true)}" : "incomplete",
                "risk assessment",
                assessor,
                cancellationToken).ConfigureAwait(false);

            return OgResult.Ok(
                complete
                    ? $"Risk saved; the chance is {OgProspectChance.Text(OgProspectChance.Of(merged), true)}."
                    : "Risk saved. The chance stays incomplete until every element is recorded.");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving the risk assessment of {prospectId}",
                "the risk was not saved",
                FailureSeverity.Error);

            return OgResult.Failed("The risk could not be saved.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgProspectDetail>> ReviseAsync(
        string prospectId,
        string newProspectId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prospectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(newProspectId);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgProspectDetail>.Failed(
                "A revision must record who made it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgProspectDetail>.Failed(
                "No connection is open, so the prospect cannot be revised.", CodeNoConnection);
        }

        try
        {
            var identityRow = await ReadProspectAsync(source, prospectId.Trim(), cancellationToken).ConfigureAwait(false);

            if (identityRow is null)
            {
                return OgResult<OgProspectDetail>.Failed(
                    $"Prospect '{prospectId}' is not in this database.", CodeUnknown);
            }

            var identity = await ReadIdentityAsync(source, identityRow, cancellationToken).ConfigureAwait(false);

            // AC-4: a working prospect is edited; a revision is for one that has left the working set.
            if (!IsLocked(identity.Status))
            {
                return OgResult<OgProspectDetail>.Failed(
                    $"{identity.Name} is {identity.Status}, so it can still be edited. Revise only a submitted " +
                    "prospect.",
                    CodeRevisionNeedsLocked);
            }

            if (await ReadProspectAsync(source, newProspectId.Trim(), cancellationToken).ConfigureAwait(false) is not null)
            {
                return OgResult<OgProspectDetail>.Failed(
                    $"Prospect '{newProspectId}' already exists; a revision needs a new identifier.",
                    CodeRevisionExists);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<PROSPECT>(source);

            unit.Add(new PROSPECT
            {
                PROSPECT_ID = newProspectId.Trim(),
                PROSPECT_NAME = identityRow.PROSPECT_NAME,
                PROSPECT_SHORT_NAME = identityRow.PROSPECT_SHORT_NAME,
                PRIMARY_FIELD_ID = identityRow.PRIMARY_FIELD_ID,
                PLAY_ID = identityRow.PLAY_ID,
                PROSPECT_TYPE = identityRow.PROSPECT_TYPE,
                PROSPECT_STATUS = "DRAFT",
                RISK_LEVEL = identityRow.RISK_LEVEL,
                CURRENT_OPERATOR = identityRow.CURRENT_OPERATOR,
                DESCRIPTION = identityRow.DESCRIPTION,
                FIELD_ID = identityRow.FIELD_ID,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var copied = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(copied))
            {
                return OgResult<OgProspectDetail>.Failed(
                    $"The revision was not created: {OgUow.Message(copied, "the row was not inserted")}",
                    CodeWriteFailed);
            }

            var assessment = await ReadAssessmentAsync(source, identity.Id, cancellationToken).ConfigureAwait(false);

            if (assessment is not null)
            {
                // The assessment is copied as it was, so the probabilities keep their original author and date.
                var riskUnit = OgUow.For<PROSPECT_RISK_ASSESSMENT>(source);

                riskUnit.Add(Clone(assessment, newProspectId.Trim(), actor.Trim(), now));

                var riskCopy = await riskUnit.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(riskCopy))
                {
                    return OgResult<OgProspectDetail>.Failed(
                        "The prospect was copied but its risk assessment was not; the new draft has no risk row.",
                        CodeWriteFailed);
                }
            }

            await RecordHistoryAsync(
                source,
                newProspectId.Trim(),
                "REVISION",
                identity.Id,
                newProspectId.Trim(),
                $"revision of {identity.Name} ({identity.Status})",
                actor,
                cancellationToken).ConfigureAwait(false);

            var loaded = await LoadAsync(newProspectId.Trim(), cancellationToken).ConfigureAwait(false);

            return loaded.Success
                ? loaded
                : OgResult<OgProspectDetail>.Failed(
                    loaded.Message ?? "The revision was created but could not be read back.",
                    loaded.Reference ?? CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"revising prospect {prospectId}",
                "the revision was not created",
                FailureSeverity.Error);

            return OgResult<OgProspectDetail>.Failed(
                "The prospect could not be revised.", reference ?? CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ reads

    private static async Task<PROSPECT?> ReadProspectAsync(
        IDataSource source,
        string prospectId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<PROSPECT>(source);
        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("PROSPECT_ID", prospectId)], cancellationToken)
            .ConfigureAwait(false);

        return rows.FirstOrDefault();
    }

    private static async Task<OgProspectIdentity> ReadIdentityAsync(
        IDataSource source,
        PROSPECT prospect,
        CancellationToken cancellationToken)
    {
        var playUnit = OgUow.For<PLAY>(source);
        var plays = await OgUow.ReadAsync(playUnit, [OgUow.Equal("PLAY_ID", prospect.PLAY_ID ?? string.Empty)],
            cancellationToken).ConfigureAwait(false);
        var play = plays.FirstOrDefault();

        var id = prospect.PROSPECT_ID ?? string.Empty;

        return new OgProspectIdentity(
            id,
            Blank(prospect.PROSPECT_NAME) ?? id,
            Blank(prospect.PLAY_ID),
            Blank(play?.PLAY_NAME),
            Blank(prospect.FIELD_ID) ?? Blank(prospect.PRIMARY_FIELD_ID),
            Blank(prospect.CURRENT_OPERATOR),
            Blank(prospect.PROSPECT_TYPE),
            Blank(prospect.PROSPECT_STATUS) ?? string.Empty,
            Blank(prospect.DESCRIPTION));
    }

    private static async Task<PROSPECT_RISK_ASSESSMENT?> ReadAssessmentAsync(
        IDataSource source,
        string prospectId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<PROSPECT_RISK_ASSESSMENT>(source);
        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("PROSPECT_ID", prospectId)], cancellationToken)
            .ConfigureAwait(false);

        return rows
            .OrderByDescending(row => row.ASSESSMENT_ID, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static async Task<OgProspectTrapDetail> ReadTrapAsync(
        IDataSource source,
        string prospectId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<PROSPECT_TRAP>(source);
        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("PROSPECT_ID", prospectId)], cancellationToken)
            .ConfigureAwait(false);
        var row = rows.OrderBy(item => item.TRAP_ID, StringComparer.Ordinal).FirstOrDefault();

        return row is null
            ? new OgProspectTrapDetail(null, null, null, null, null, null)
            : new OgProspectTrapDetail(
                Blank(row.TRAP_TYPE),
                row.CLOSURE_AREA,
                Blank(row.CLOSURE_AREA_OUOM),
                Blank(row.SEAL_QUALITY),
                Blank(row.TRAP_CONFIDENCE),
                Blank(row.DESCRIPTION));
    }

    private static async Task<OgProspectReservoirDetail> ReadReservoirAsync(
        IDataSource source,
        string prospectId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<PROSPECT_RESERVOIR>(source);
        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("PROSPECT_ID", prospectId)], cancellationToken)
            .ConfigureAwait(false);
        var row = rows.OrderBy(item => item.RESERVOIR_ID, StringComparer.Ordinal).FirstOrDefault();

        return row is null
            ? new OgProspectReservoirDetail(null, null, null, null, null, null, null, null)
            : new OgProspectReservoirDetail(
                Blank(row.RESERVOIR_NAME),
                Blank(row.STRAT_UNIT_ID),
                Blank(row.LITHOLOGY),
                row.POROSITY,
                row.PERMEABILITY,
                row.NET_PAY,
                Blank(row.NET_PAY_OUOM),
                Blank(row.FLUID_TYPE));
    }

    private static async Task<OgProspectSourceDetail> ReadSourceAsync(
        IDataSource source,
        string prospectId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<PROSPECT_SOURCE_ROCK>(source);
        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("PROSPECT_ID", prospectId)], cancellationToken)
            .ConfigureAwait(false);
        var row = rows.OrderBy(item => item.SOURCE_ID, StringComparer.Ordinal).FirstOrDefault();

        return row is null
            ? new OgProspectSourceDetail(null, null, null, null, null, null)
            : new OgProspectSourceDetail(
                Blank(row.STRAT_UNIT_ID),
                row.TOC,
                Blank(row.MATURITY_STAGE),
                row.MIGRATION_DISTANCE,
                Blank(row.MIGRATION_DISTANCE_OUOM),
                Blank(row.DESCRIPTION));
    }

    /// <summary>
    /// AC-3: the percentiles keep the unit recorded on the prospect's volume estimate (or the prospect's own volume
    /// unit); nothing guesses a unit.
    /// </summary>
    private static async Task<OgProspectVolume> ReadVolumeAsync(
        IDataSource source,
        string prospectId,
        PROSPECT prospect,
        PROSPECT_RISK_ASSESSMENT? assessment,
        CancellationToken cancellationToken)
    {
        decimal? oilLow = null, oilMid = null, oilHigh = null;
        decimal? gasLow = null, gasMid = null, gasHigh = null;
        string? fluid = null;

        if (assessment is not null)
        {
            oilLow = assessment.LOW_ESTIMATE_OIL;
            oilMid = assessment.BEST_ESTIMATE_OIL;
            oilHigh = assessment.HIGH_ESTIMATE_OIL;
            gasLow = assessment.LOW_ESTIMATE_GAS;
            gasMid = assessment.BEST_ESTIMATE_GAS;
            gasHigh = assessment.HIGH_ESTIMATE_GAS;
            fluid = Blank(assessment.FLUID_TYPE);
        }

        var estimates = await OgUow.ReadAsync(
            OgUow.For<PROSPECT_VOLUME_ESTIMATE>(source),
            [OgUow.Equal("PROSPECT_ID", prospectId)],
            cancellationToken).ConfigureAwait(false);

        var estimate = estimates
            .OrderByDescending(row => row.ESTIMATE_DATE)
            .ThenBy(row => row.ESTIMATE_ID, StringComparer.Ordinal)
            .FirstOrDefault();

        fluid ??= Blank(estimate?.FLUID_TYPE);

        var useOil = oilLow is not null || oilMid is not null || oilHigh is not null;
        var unit = Blank(estimate?.VOLUME_OUOM) ?? Blank(prospect.ESTIMATED_VOLUME_OUOM);

        return new OgProspectVolume(
            useOil ? oilLow : gasLow,
            useOil ? oilMid : gasMid,
            useOil ? oilHigh : gasHigh,
            unit,
            useOil ? fluid ?? "OIL" : fluid ?? "GAS");
    }

    private static async Task<OgProspectEconomicSummary> ReadEconomicsAsync(
        IDataSource source,
        string prospectId,
        PROSPECT_RISK_ASSESSMENT? assessment,
        PROSPECT prospect,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAsync(
            OgUow.For<PROSPECT_ECONOMIC>(source),
            [OgUow.Equal("PROSPECT_ID", prospectId)],
            cancellationToken).ConfigureAwait(false);

        var economic = rows
            .OrderByDescending(row => row.EVALUATION_DATE)
            .FirstOrDefault();

        var currency = Blank(prospect.ESTIMATED_VALUE_CURRENCY);

        return economic is null
            ? new OgProspectEconomicSummary(null, null, null, null, assessment?.EMV, assessment?.NPV, currency)
            : new OgProspectEconomicSummary(
                Blank(economic.ECONOMIC_SCENARIO),
                economic.EVALUATION_DATE,
                Blank(economic.EVALUATOR),
                Blank(economic.DESCRIPTION),
                assessment?.EMV,
                assessment?.NPV,
                currency);
    }

    private static async Task<IReadOnlyList<OgProspectStageRow>> ReadHistoryAsync(
        IDataSource source,
        string prospectId,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAsync(
            OgUow.For<PROSPECT_WORKFLOW_STAGE>(source),
            [OgUow.Equal("PROSPECT_ID", prospectId)],
            cancellationToken).ConfigureAwait(false);

        return rows
            .OrderByDescending(row => row.STAGE_SEQ_NO)
            .ThenByDescending(row => row.STAGE_START_DATE)
            .Select(row => new OgProspectStageRow(
                Blank(row.WORKFLOW_STAGE) ?? OgProspectCard.UnclassifiedStage,
                (int)row.STAGE_SEQ_NO,
                row.STAGE_START_DATE,
                row.STAGE_END_DATE,
                Blank(row.STAGE_OUTCOME)))
            .ToList();
    }

    // ----------------------------------------------------------------- writes

    private async Task RecordHistoryAsync(
        IDataSource source,
        string prospectId,
        string field,
        string oldValue,
        string newValue,
        string reason,
        string actor,
        CancellationToken cancellationToken)
    {
        try
        {
            var unit = OgUow.For<PROSPECT_HISTORY>(source);
            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("PROSPECT_ID", prospectId)], cancellationToken)
                .ConfigureAwait(false);

            var sequence = rows.Count == 0 ? 1m : rows.Max(row => row.HISTORY_SEQ_NO) + 1m;
            var now = DateTime.UtcNow;

            unit.Add(new PROSPECT_HISTORY
            {
                PROSPECT_ID = prospectId,
                HISTORY_SEQ_NO = sequence,
                FIELD_NAME = field,
                OLD_VALUE = oldValue,
                NEW_VALUE = newValue,
                CHANGE_REASON = reason,
                CHANGED_BY = actor.Trim(),
                CHANGE_DATE = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                // The change itself was saved; the history row is the audit trail and its absence is reported.
                _failures.ReportHandled(
                    new InvalidOperationException(
                        $"The history row for {prospectId} ({field}) was not written: " +
                        OgUow.Message(committed, "the row was not inserted")),
                    $"recording the history of {prospectId}",
                    "the change stands but its history row is missing",
                    FailureSeverity.Degraded);
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            _failures.ReportHandled(
                ex,
                $"recording the history of {prospectId}",
                "the change stands but its history row is missing",
                FailureSeverity.Degraded);
        }
    }

    // ---------------------------------------------------------------- helpers

    private static IReadOnlyDictionary<string, decimal?> AssessmentRisks(PROSPECT_RISK_ASSESSMENT assessment) =>
        new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase)
        {
            [OgProspectChance.RequiredElements[0]] = assessment.TRAP_RISK,
            [OgProspectChance.RequiredElements[1]] = assessment.RESERVOIR_RISK,
            [OgProspectChance.RequiredElements[2]] = assessment.SEAL_RISK,
            [OgProspectChance.RequiredElements[3]] = assessment.SOURCE_RISK,
            [OgProspectChance.RequiredElements[4]] = assessment.TIMING_RISK,
        };

    private static PROSPECT_RISK_ASSESSMENT Clone(
        PROSPECT_RISK_ASSESSMENT source,
        string prospectId,
        string actor,
        DateTime now) =>
        new()
        {
            PROSPECT_ID = prospectId,
            ASSESSMENT_ID = source.ASSESSMENT_ID,
            RISK_MODEL_TYPE = source.RISK_MODEL_TYPE,
            TRAP_RISK = source.TRAP_RISK,
            RESERVOIR_RISK = source.RESERVOIR_RISK,
            SEAL_RISK = source.SEAL_RISK,
            SOURCE_RISK = source.SOURCE_RISK,
            TIMING_RISK = source.TIMING_RISK,
            OVERALL_GEOLOGICAL_RISK = source.OVERALL_GEOLOGICAL_RISK,
            POS_GEOLOGICAL = source.POS_GEOLOGICAL,
            FLUID_TYPE = source.FLUID_TYPE,
            ASSESSOR = source.ASSESSOR,
            ASSESSMENT_DATE = source.ASSESSMENT_DATE,
            ROW_CREATED_BY = actor,
            ROW_CREATED_DATE = now,
            ROW_CHANGED_BY = actor,
            ROW_CHANGED_DATE = now,
        };

    private static string? Trimmed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
