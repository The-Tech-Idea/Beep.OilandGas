using System.Globalization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Production;

/// <summary>One piece of evidence the diagnosis rests on (PRD-06 AC-1).</summary>
public sealed record OgInterventionEvidence(string Kind, string Statement, string Direction)
{
    /// <summary>The evidence points at a shortfall.</summary>
    public const string Negative = "NEGATIVE";

    /// <summary>The evidence points the other way: the well is performing.</summary>
    public const string Positive = "POSITIVE";

    /// <summary>The evidence explains a gap without taking a side (a material variance).</summary>
    public const string Neutral = "NEUTRAL";

    public bool IsNegative => string.Equals(Direction, Negative, StringComparison.OrdinalIgnoreCase);

    public bool IsPositive => string.Equals(Direction, Positive, StringComparison.OrdinalIgnoreCase);

    public string KindText => Kind.ToLowerInvariant();

    public string Describe() => $"{KindText}: {Statement}";
}

/// <summary>One well of the field, with its diagnosis (PRD-06).</summary>
public sealed record OgInterventionWell(
    string WellId,
    string? Name,
    string Status,
    DateTime? StatusSince,
    decimal? Rate,
    decimal? Average,
    string? Unit,
    decimal? Change,
    int HistoryDays,
    IReadOnlyList<OgInterventionEvidence> Evidence,
    IReadOnlyList<string> Missing,
    IReadOnlyList<string> Conflicts,
    decimal Confidence,
    IReadOnlyList<string> Flags,
    int DecisionCount)
{
    public string NameText => Name ?? WellId;

    public string StatusText => string.IsNullOrWhiteSpace(Status) ? "no status recorded" : Status.ToLowerInvariant();

    /// <summary>AC-1: with no evidence at all the diagnosis is "insufficient data", and it names what is missing.</summary>
    public bool IsInsufficient => Evidence.Count == 0;

    /// <summary>The state table's "conflicting evidence": both shown, nothing resolved automatically.</summary>
    public bool HasConflicts => Conflicts.Count > 0;

    public bool IsUnderperforming => Flags.Count > 0;

    public string RateText => OgProductionDay.Format(Rate, Unit);

    public string AverageText => HistoryDays == 0
        ? "no history"
        : OgProductionDay.Format(Average, Unit) +
          $" ({HistoryDays.ToString(CultureInfo.InvariantCulture)} day(s))";

    public string ChangeText => Change is { } change
        ? (change > 0m ? "+" : string.Empty) + OgProductionDay.Format(change, Unit)
        : "not computable";

    /// <summary>
    /// The stated confidence rule: a quarter for each of the four checks that produced evidence, capped at a half while a
    /// conflict is unresolved. Nothing here is scored by a model.
    /// </summary>
    public string ConfidenceText =>
        (Confidence * 100m).ToString("0", CultureInfo.InvariantCulture) + "%" +
        (HasConflicts ? " (capped: the evidence conflicts)" : string.Empty);

    public string DiagnosisText => IsInsufficient
        ? "insufficient data: " + string.Join("; ", Missing)
        : HasConflicts
            ? "conflicting evidence: " + string.Join(" · ", Conflicts)
            : Flags.Count == 0
                ? "no underperformance is evidenced"
                : string.Join(" · ", Flags);

    public string EvidenceText => Evidence.Count == 0
        ? "no evidence is recorded"
        : string.Join(" · ", Evidence.Select(item => item.Describe()));

    public string MissingText => Missing.Count == 0
        ? "every check produced evidence"
        : string.Join(" · ", Missing);

    public string FlagsText => Flags.Count == 0 ? "no flag" : string.Join(" · ", Flags);

    public string Describe() =>
        $"{NameText} · {StatusText} · rate {RateText} · average {AverageText} · change {ChangeText} · " +
        $"diagnosis {DiagnosisText} · confidence {ConfidenceText}";
}

/// <summary>One candidate intervention of a decision (PRD-06), matching the plan's <c>OgInterventionOption</c>.</summary>
public sealed record OgInterventionOption(
    string Id,
    string Method,
    string? MethodName,
    decimal? EstimatedGain,
    string? GainUnit,
    decimal? EstimatedCost,
    string? Currency,
    string? Risk,
    string Basis,
    string? BasisText,
    bool IsChosen,
    bool LiftRelated)
{
    public bool HasBasis => !string.IsNullOrWhiteSpace(Basis);

    public string MethodText => string.IsNullOrWhiteSpace(MethodName) ? Method : $"{Method} ({MethodName!.Trim()})";

    public string GainText => EstimatedGain is null
        ? "no gain estimated"
        : OgProductionDay.Format(EstimatedGain, GainUnit);

    public string CostText => EstimatedCost is { } cost
        ? cost.ToString("#,0.##", CultureInfo.InvariantCulture) + " " +
          (string.IsNullOrWhiteSpace(Currency) ? "(currency not recorded)" : Currency!.Trim())
        : "no cost estimated";

    public string RiskText => string.IsNullOrWhiteSpace(Risk) ? "risk not stated" : Risk!.Trim().ToLowerInvariant();

    /// <summary>AC-2: the basis is stated as a code with the evidence named beside it.</summary>
    public string BasisDescription => string.IsNullOrWhiteSpace(BasisText)
        ? Basis.ToLowerInvariant()
        : $"{Basis.ToLowerInvariant()}: {BasisText!.Trim()}";

    public string ChosenText => IsChosen ? "chosen" : "considered";

    public string Describe() =>
        $"{MethodText} · gain {GainText} · cost {CostText} · risk {RiskText} · basis {BasisDescription} · {ChosenText}";
}

/// <summary>One intervention decision with its options (PRD-06), matching the plan's <c>OgInterventionDecision</c>.</summary>
public sealed record OgInterventionDecision(
    string Id,
    string WellId,
    string? WellName,
    string State,
    string? Rationale,
    string? Owner,
    DateTime? TargetDate,
    string? WorkOrderRef,
    DateTime? WorkOrderAt,
    string? WorkOrderBy,
    IReadOnlyList<OgInterventionOption> Options,
    DateTime? RecordedAt)
{
    public const string DecidedState = "DECIDED";
    public const string ActedState = "ACTED";

    public bool HasWorkOrder => !string.IsNullOrWhiteSpace(WorkOrderRef);

    public bool IsActed => string.Equals(State, ActedState, StringComparison.OrdinalIgnoreCase);

    public string StateText => State.ToLowerInvariant();

    public OgInterventionOption? Chosen => Options.FirstOrDefault(option => option.IsChosen);

    public string NameText => WellName ?? WellId;

    public string ChosenText => Chosen is { } chosen ? chosen.MethodText : "(no option chosen)";

    public string TargetText => TargetDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "no target date";

    /// <summary>AC-5: the work order carries the decision it was raised from.</summary>
    public string WorkOrderLink => HasWorkOrder
        ? $"decision {Id} -> work order {WorkOrderRef}"
        : "no work order";

    public string OptionsText => Options.Count == 0
        ? "no option is recorded"
        : string.Join(" · ", Options.Select(option => option.Describe()));

    public string Describe() =>
        $"{NameText} · {StateText} · {ChosenText} · owner {Owner ?? "(none)"} · target {TargetText} · " +
        $"rationale {(string.IsNullOrWhiteSpace(Rationale) ? "(none)" : Rationale!.Trim())} · {WorkOrderLink}";
}

/// <summary>An option, as the page enters it.</summary>
public sealed record OgInterventionOptionValues(
    string? Method,
    decimal? EstimatedGain,
    string? GainUnit,
    decimal? EstimatedCost,
    string? Currency,
    string? Risk,
    string? Basis,
    string? BasisText);

/// <summary>A decision, as the page records it.</summary>
public sealed record OgInterventionDecisionValues(
    string? FieldId,
    string? WellId,
    IReadOnlyList<OgInterventionOptionValues> Options,
    string? ChosenMethod,
    string? Rationale,
    string? Owner,
    DateTime? TargetDate);

/// <summary>The work order the page asked for but which was not acknowledged (the state table's "creation lost").</summary>
public sealed record OgInterventionPendingWorkOrder(string DecisionId, string? Reference, string Actor);

/// <summary>The intervention workspace of one field (PRD-06).</summary>
public sealed record OgInterventionWorkspace(
    string FieldName,
    string? FieldId,
    DateTime AsOf,
    IReadOnlyList<OgInterventionWell> Wells,
    OgInterventionWell? Selected,
    IReadOnlyList<OgInterventionDecision> Decisions,
    IReadOnlyList<string> Methods,
    bool MethodReferenceMissing,
    IReadOnlyList<string> Risks)
{
    public int UnderperformingCount => Wells.Count(well => well.IsUnderperforming);

    public int InsufficientCount => Wells.Count(well => well.IsInsufficient);

    public int ConflictCount => Wells.Count(well => well.HasConflicts);

    public int OpenDecisionCount => Decisions.Count(decision => !decision.HasWorkOrder);

    public string MethodsText => MethodReferenceMissing
        ? "No intervention method is recorded, so no option can be saved. Add the operating company's methods to " +
          "R_INTERVENTION_METHOD."
        : string.Join(", ", Methods);

    public string Summary =>
        $"{Wells.Count.ToString(CultureInfo.InvariantCulture)} well(s) · " +
        $"{UnderperformingCount.ToString(CultureInfo.InvariantCulture)} flagged · " +
        $"{InsufficientCount.ToString(CultureInfo.InvariantCulture)} with insufficient data · " +
        $"{ConflictCount.ToString(CultureInfo.InvariantCulture)} with conflicting evidence · " +
        $"{OpenDecisionCount.ToString(CultureInfo.InvariantCulture)} decision(s) without a work order";

    public string DecisionsText => Decisions.Count == 0
        ? "No intervention decision is recorded for this field."
        : string.Join(Environment.NewLine, Decisions.Select(decision => decision.Describe()));

    public string WellListText => Wells.Count == 0
        ? "no well is recorded for this field"
        : string.Join(" · ", Wells.Select(well => $"{well.NameText} {well.FlagsText}"));
}

/// <summary>The intervention rules, stated once so the page, the service and the tests agree.</summary>
public static class InterventionRules
{
    public const string BasisTest = "TEST";
    public const string BasisAnalogue = "ANALOGUE";
    public const string BasisModel = "MODEL";

    public const string RiskLow = "LOW";
    public const string RiskMedium = "MEDIUM";
    public const string RiskHigh = "HIGH";

    /// <summary>The history a rate change needs before it is evidence at all.</summary>
    public const int RequiredHistoryDays = 7;

    public const string CodeNoConnection = "intervention-no-connection";
    public const string CodeReadFailed = "intervention-read-failed";
    public const string CodeWriteFailed = "intervention-write-failed";
    public const string CodeActorRequired = "intervention-actor-required";
    public const string CodeWellRequired = "intervention-well-required";
    public const string CodeWellUnknown = "intervention-well-unknown";
    public const string CodeNoOptions = "intervention-no-options";
    public const string CodeMethodRequired = "intervention-method-required";
    public const string CodeMethodReferenceMissing = "intervention-method-reference-missing";
    public const string CodeMethodUnknown = "intervention-method-unknown";
    public const string CodeMethodDuplicate = "intervention-method-duplicate";
    public const string CodeBasisRequired = "intervention-basis-required";
    public const string CodeBasisInvalid = "intervention-basis-invalid";
    public const string CodeGainUnitRequired = "intervention-gain-unit-required";
    public const string CodeGainInvalid = "intervention-gain-invalid";
    public const string CodeCostInvalid = "intervention-cost-invalid";
    public const string CodeRiskInvalid = "intervention-risk-invalid";
    public const string CodeChosenRequired = "intervention-chosen-required";
    public const string CodeChosenUnknown = "intervention-chosen-unknown";
    public const string CodeRationaleRequired = "intervention-rationale-required";
    public const string CodeOwnerRequired = "intervention-owner-required";
    public const string CodeTargetRequired = "intervention-target-required";
    public const string CodeDecisionUnknown = "intervention-decision-unknown";
    public const string CodeWorkOrderRequired = "intervention-work-order-required";
    public const string CodeWorkOrderExists = "intervention-work-order-exists";
    public const string CodeNothingPending = "intervention-nothing-pending";

    public static IReadOnlyList<string> Bases { get; } = [BasisTest, BasisAnalogue, BasisModel];

    public static IReadOnlyList<string> Risks { get; } = [RiskLow, RiskMedium, RiskHigh];

    /// <summary>
    /// AC-1 and AC-2 and AC-3, in one place: the well, at least one option, each option's method in the reference set and
    /// its basis stated, a gain with its unit, the chosen option, and the decision's rationale and owner.
    /// </summary>
    public static (string Code, string Message)? Validate(
        OgInterventionDecisionValues values,
        IReadOnlyCollection<string> methods)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(methods);

        if (string.IsNullOrWhiteSpace(values.WellId))
        {
            return (CodeWellRequired, "Choose the well the decision is about.");
        }

        var options = values.Options ?? [];

        if (options.Count == 0)
        {
            return (CodeNoOptions, "A decision records the options that were considered, not only the answer.");
        }

        if (methods.Count == 0)
        {
            return (CodeMethodReferenceMissing,
                "No intervention method is recorded in the reference set, so no option can be saved.");
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var option in options)
        {
            if (string.IsNullOrWhiteSpace(option.Method))
            {
                return (CodeMethodRequired, "Every option names its intervention method.");
            }

            var method = option.Method.Trim();

            if (!methods.Contains(method, StringComparer.OrdinalIgnoreCase))
            {
                return (CodeMethodUnknown,
                    $"'{method}' is not in the intervention method set; use {string.Join(", ", methods)}.");
            }

            if (!seen.Add(method))
            {
                return (CodeMethodDuplicate,
                    $"'{method}' is listed twice; a decision chooses between different methods.");
            }

            if (string.IsNullOrWhiteSpace(option.Basis))
            {
                return (CodeBasisRequired,
                    $"The option '{method}' must state its basis: {string.Join(", ", Bases)}.");
            }

            if (!Bases.Contains(option.Basis.Trim().ToUpperInvariant(), StringComparer.Ordinal))
            {
                return (CodeBasisInvalid,
                    $"'{option.Basis}' is not a basis; use {string.Join(", ", Bases)}.");
            }

            // AC-3: a gain carries its unit.
            if (option.EstimatedGain is not null && string.IsNullOrWhiteSpace(option.GainUnit))
            {
                return (CodeGainUnitRequired,
                    $"The estimated gain of '{method}' needs the unit it is measured in (for example bbl/d or Mcf/d).");
            }

            if (option.EstimatedGain is < 0m)
            {
                return (CodeGainInvalid, $"The estimated gain of '{method}' cannot be negative.");
            }

            if (option.EstimatedCost is < 0m)
            {
                return (CodeCostInvalid, $"The estimated cost of '{method}' cannot be negative.");
            }

            if (!string.IsNullOrWhiteSpace(option.Risk)
                && !Risks.Contains(option.Risk.Trim().ToUpperInvariant(), StringComparer.Ordinal))
            {
                return (CodeRiskInvalid,
                    $"'{option.Risk}' is not a risk level; use {string.Join(", ", Risks)}.");
            }
        }

        if (string.IsNullOrWhiteSpace(values.ChosenMethod))
        {
            return (CodeChosenRequired, "A decision chooses one of its options.");
        }

        if (!seen.Contains(values.ChosenMethod.Trim()))
        {
            return (CodeChosenUnknown,
                $"'{values.ChosenMethod}' is not one of the options recorded on this decision.");
        }

        if (string.IsNullOrWhiteSpace(values.Rationale))
        {
            return (CodeRationaleRequired, "A decision records why it was made.");
        }

        if (string.IsNullOrWhiteSpace(values.Owner))
        {
            return (CodeOwnerRequired, "A decision records who owns it.");
        }

        return values.TargetDate is null
            ? (CodeTargetRequired, "A decision records the date it is targeted for.")
            : null;
    }

    /// <summary>The diagnosis's confidence: a quarter per check that produced evidence, capped at a half on a conflict.</summary>
    public static decimal Confidence(int evidenceCount, bool hasConflicts)
    {
        var value = Math.Min(1m, evidenceCount / 4m);

        return hasConflicts ? Math.Min(value, 0.5m) : value;
    }

    /// <summary>A method whose reference row says it changes the lift arrangement gets the lift sentence.</summary>
    public static string LiftSentence(string method) =>
        $"{method} changes the artificial-lift arrangement, so a lift-method recommendation belongs to LFT-01 " +
        "(not built here); this screen records the decision, it does not compute a lift design.";
}

/// <summary>PRD-06: intervention decisions and the engineer workspace.</summary>
public interface IOgInterventions
{
    Task<OgResult<OgInterventionWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? wellId = null,
        DateTime? asOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>Records the diagnosis's decision with its options, rationale, owner and target.</summary>
    Task<OgResult<OgInterventionWorkspace>> RecordDecisionAsync(
        OgInterventionDecisionValues values,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Raises the work order from a decision, once (AC-5).</summary>
    Task<OgResult<OgInterventionWorkspace>> CreateWorkOrderAsync(
        string decisionId,
        string? workOrderRef,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The work order retained when its creation was not acknowledged.</summary>
    OgInterventionPendingWorkOrder? Pending { get; }

    Task<OgResult<OgInterventionWorkspace>> RetryWorkOrderAsync(
        string? workOrderRef,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The intervention workspace over the PPDM <c>WELL</c>, <c>WELL_STATUS</c>, <c>PDEN_WELL</c> and
/// <c>PDEN_VOL_SUMMARY</c> rows, this stage's <c>PRODUCTION_DEFERMENT</c> (PRD-03) and <c>RECONCILIATION_VARIANCE</c>
/// (PRD-05) evidence, and the ProductionOperations <c>INTERVENTION_DECISION</c>, <c>INTERVENTION_OPTION</c> and
/// <c>R_INTERVENTION_METHOD</c> rows, read and written through their entity models and a unit of work.
/// </summary>
/// <remarks>
/// The diagnosis is assembled from the records the earlier features already wrote - the measured rate against the well's
/// own history (PRD-01), the open deferment (PRD-03), the material reconciliation variance (PRD-05) and the PPDM status
/// (WEL-04) - so nothing here is a second opinion about the same volume. The work order itself belongs to WRK-01, which is
/// not built: this screen records the reference the decision was raised under and refuses a second one.
/// </remarks>
public sealed class OgInterventionService : IOgInterventions
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgInterventionService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgInterventionPendingWorkOrder? Pending { get; private set; }

    public async Task<OgResult<OgInterventionWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? wellId = null,
        DateTime? asOf = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgInterventionWorkspace>.Failed(
                "No connection is open, so the intervention workspace cannot be read.",
                InterventionRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgInterventionWorkspace>.Ok(
                Build(model, fieldId, fieldName, wellId, (asOf ?? DateTime.Today).Date));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the intervention workspace",
                "the diagnosis and the decisions are not shown",
                FailureSeverity.Error);

            return OgResult<OgInterventionWorkspace>.Failed(
                "The intervention workspace could not be read.", reference ?? InterventionRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgInterventionWorkspace>> RecordDecisionAsync(
        OgInterventionDecisionValues values,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgInterventionWorkspace>.Failed(
                "A decision must record who made it.", InterventionRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgInterventionWorkspace>.Failed(
                "No connection is open, so the decision cannot be saved.", InterventionRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var methods = Methods(model);

            if (InterventionRules.Validate(values, methods) is { } refusal)
            {
                return OgResult<OgInterventionWorkspace>.Failed(refusal.Message, refusal.Code);
            }

            var well = model.Wells.FirstOrDefault(row =>
                string.Equals(row.UWI, values.WellId!.Trim(), StringComparison.OrdinalIgnoreCase));

            if (well is null)
            {
                return OgResult<OgInterventionWorkspace>.Failed(
                    $"Well '{values.WellId}' is not in this database.", InterventionRules.CodeWellUnknown);
            }

            var now = DateTime.UtcNow;
            var decisionId = Guid.NewGuid().ToString("N");
            var chosen = values.ChosenMethod!.Trim().ToUpperInvariant();

            var decisionUnit = OgUow.For<INTERVENTION_DECISION>(source);
            decisionUnit.PrimaryKey = "DECISION_ID";

            decisionUnit.Add(new INTERVENTION_DECISION
            {
                DECISION_ID = decisionId,
                FIELD_ID = Blank(values.FieldId) ?? Blank(well.ASSIGNED_FIELD),
                WELL_ID = well.UWI!,
                CHOSEN_OPTION_ID = chosen,
                RATIONALE = values.Rationale!.Trim(),
                OWNER = values.Owner!.Trim(),
                TARGET_DATE = values.TargetDate!.Value.Date,
                STATE = OgInterventionDecision.DecidedState,
                ACTOR = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var optionUnit = OgUow.For<INTERVENTION_OPTION>(source);
            optionUnit.PrimaryKey = "OPTION_ID";

            foreach (var option in values.Options!)
            {
                var method = option.Method!.Trim().ToUpperInvariant();

                optionUnit.Add(new INTERVENTION_OPTION
                {
                    OPTION_ID = Guid.NewGuid().ToString("N"),
                    DECISION_ID = decisionId,
                    WELL_ID = well.UWI!,
                    METHOD = method,
                    ESTIMATED_GAIN = option.EstimatedGain,
                    GAIN_OUOM = Blank(option.GainUnit)?.Trim(),
                    ESTIMATED_COST = option.EstimatedCost,
                    CURRENCY = Blank(option.Currency)?.Trim(),
                    RISK = Blank(option.Risk)?.Trim().ToUpperInvariant(),
                    BASIS = option.Basis!.Trim().ToUpperInvariant(),
                    BASIS_TEXT = Blank(option.BasisText),
                    CHOSEN_IND = string.Equals(method, chosen, StringComparison.OrdinalIgnoreCase) ? "Y" : "N",
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }

            var committedOptions = await optionUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committedOptions))
            {
                return OgResult<OgInterventionWorkspace>.Failed(
                    OgUow.Message(committedOptions, "The options were not saved, so no decision was recorded."),
                    InterventionRules.CodeWriteFailed);
            }

            var committedDecision = await decisionUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committedDecision))
            {
                return OgResult<OgInterventionWorkspace>.Failed(
                    OgUow.Message(committedDecision, "The decision was not saved."),
                    InterventionRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgInterventionWorkspace>.Ok(
                Build(after, values.FieldId, string.Empty, well.UWI, DateTime.Today));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"recording an intervention decision for well {values.WellId}",
                "the decision was not saved",
                FailureSeverity.Error);

            return OgResult<OgInterventionWorkspace>.Failed(
                "The decision could not be saved.", reference ?? InterventionRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgInterventionWorkspace>> CreateWorkOrderAsync(
        string decisionId,
        string? workOrderRef,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgInterventionWorkspace>.Failed(
                "Creating a work order must record who did it.", InterventionRules.CodeActorRequired);
        }

        var raised = await RaiseWorkOrderAsync(decisionId, workOrderRef, actor, cancellationToken)
            .ConfigureAwait(false);

        if (raised.Success)
        {
            Pending = null;
        }

        return raised;
    }

    public async Task<OgResult<OgInterventionWorkspace>> RetryWorkOrderAsync(
        string? workOrderRef,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgInterventionWorkspace>.Failed(
                "There is no work order waiting to be retried.", InterventionRules.CodeNothingPending);
        }

        var retried = await RaiseWorkOrderAsync(
            pending.DecisionId,
            workOrderRef ?? pending.Reference,
            actor,
            cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    private async Task<OgResult<OgInterventionWorkspace>> RaiseWorkOrderAsync(
        string decisionId,
        string? workOrderRef,
        string actor,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(decisionId))
        {
            return OgResult<OgInterventionWorkspace>.Failed(
                "Choose the decision the work order is raised from.", InterventionRules.CodeDecisionUnknown);
        }

        if (string.IsNullOrWhiteSpace(workOrderRef))
        {
            return OgResult<OgInterventionWorkspace>.Failed(
                "A work order records its reference (WRK-01 owns the work order itself).",
                InterventionRules.CodeWorkOrderRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgInterventionWorkspace>.Failed(
                "No connection is open, so the work order cannot be created.", InterventionRules.CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var existing = model.Decisions.FirstOrDefault(row =>
                string.Equals(row.DECISION_ID, decisionId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                return OgResult<OgInterventionWorkspace>.Failed(
                    $"Decision '{decisionId}' is not in this database.", InterventionRules.CodeDecisionUnknown);
            }

            // AC-5: a decision carries at most one work order.
            if (!string.IsNullOrWhiteSpace(existing.WORK_ORDER_REF))
            {
                return OgResult<OgInterventionWorkspace>.Failed(
                    $"This decision already raised work order '{existing.WORK_ORDER_REF}'; a decision carries one.",
                    InterventionRules.CodeWorkOrderExists);
            }

            var unit = OgUow.For<INTERVENTION_DECISION>(source);
            unit.PrimaryKey = "DECISION_ID";

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal("DECISION_ID", existing.DECISION_ID)],
                cancellationToken).ConfigureAwait(false);
            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgInterventionWorkspace>.Failed(
                    $"Decision '{decisionId}' is not in this database.", InterventionRules.CodeDecisionUnknown);
            }

            var now = DateTime.UtcNow;

            row.WORK_ORDER_REF = workOrderRef.Trim();
            row.WORK_ORDER_AT = now;
            row.WORK_ORDER_BY = actor.Trim();
            row.STATE = OgInterventionDecision.ActedState;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = now;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                Pending = new OgInterventionPendingWorkOrder(existing.DECISION_ID!, workOrderRef.Trim(), actor.Trim());

                return OgResult<OgInterventionWorkspace>.Failed(
                    OgUow.Message(updated, "The work order was not acknowledged; retry sends the same reference."),
                    InterventionRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                Pending = new OgInterventionPendingWorkOrder(existing.DECISION_ID!, workOrderRef.Trim(), actor.Trim());

                return OgResult<OgInterventionWorkspace>.Failed(
                    OgUow.Message(committed, "The work order was not acknowledged; retry sends the same reference."),
                    InterventionRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgInterventionWorkspace>.Ok(
                Build(after, existing.FIELD_ID, string.Empty, existing.WELL_ID, DateTime.Today));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = new OgInterventionPendingWorkOrder(decisionId.Trim(), workOrderRef?.Trim(), actor.Trim());

            var reference = _failures.ReportHandled(
                ex,
                $"creating a work order from decision {decisionId}",
                "the work order was not recorded; retry sends the same reference",
                FailureSeverity.Error);

            return OgResult<OgInterventionWorkspace>.Failed(
                "The work order could not be created; retry sends the same reference.",
                reference ?? InterventionRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ reads

    private static async Task<InterventionModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var wells = await OgUow.ReadAllAsync<WELL>(source).ConfigureAwait(false);
        var statuses = await OgUow.ReadAllAsync<WELL_STATUS>(source).ConfigureAwait(false);
        var pdenWells = await OgUow.ReadAllAsync<PDEN_WELL>(source).ConfigureAwait(false);
        var volumes = await OgUow.ReadAllAsync<PDEN_VOL_SUMMARY>(source).ConfigureAwait(false);
        var deferments = await OgUow.ReadAllAsync<PRODUCTION_DEFERMENT>(source).ConfigureAwait(false);
        var variances = await OgUow.ReadAllAsync<RECONCILIATION_VARIANCE>(source).ConfigureAwait(false);
        var decisions = await OgUow.ReadAllAsync<INTERVENTION_DECISION>(source).ConfigureAwait(false);
        var options = await OgUow.ReadAllAsync<INTERVENTION_OPTION>(source).ConfigureAwait(false);
        var methods = await OgUow.ReadAllAsync<R_INTERVENTION_METHOD>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new InterventionModel(
            wells, statuses, pdenWells, volumes, deferments, variances, decisions, options, methods);
    }

    /// <summary>The active method codes from the reference set; a code outside it is refused.</summary>
    private static IReadOnlyList<string> Methods(InterventionModel model) =>
        model.Methods
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.IsNullOrWhiteSpace(row.METHOD_CODE))
            .Select(row => row.METHOD_CODE!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static OgInterventionWorkspace Build(
        InterventionModel model,
        string? fieldId,
        string fieldName,
        string? wellId,
        DateTime asOf)
    {
        var wells = model.Wells
            .Where(row => !string.IsNullOrWhiteSpace(row.UWI))
            .Where(row => FieldMatches(row.ASSIGNED_FIELD, fieldId, fieldName))
            .GroupBy(row => row.UWI!, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(row => row.WELL_NAME ?? row.UWI, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var wellIds = wells.Select(row => row.UWI!).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var entityToWell = model.PdenWells
            .Where(row => !string.IsNullOrWhiteSpace(row.PDEN_ID) && !string.IsNullOrWhiteSpace(row.PRIMARY_UWI))
            .Where(row => wellIds.Contains(row.PRIMARY_UWI!))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .GroupBy(row => row.PDEN_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().PRIMARY_UWI!, StringComparer.OrdinalIgnoreCase);

        var month = ReconciliationRules.MonthOf(asOf);
        var monthEnd = ReconciliationRules.MonthEnd(month);

        var rates = RateHistory(model, entityToWell, asOf);

        var latestStatus = model.Statuses
            .Where(row => !string.IsNullOrWhiteSpace(row.UWI))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .GroupBy(row => row.UWI!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(row => row.STATUS_DATE ?? row.START_TIME ?? row.EFFECTIVE_DATE ?? DateTime.MinValue)
                    .First(),
                StringComparer.OrdinalIgnoreCase);

        var openDeferments = model.Deferments
            .Where(row => !string.IsNullOrWhiteSpace(row.WELL_ID) && row.END_AT is null)
            .GroupBy(row => row.WELL_ID!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(row => row.START_AT).First(),
                StringComparer.OrdinalIgnoreCase);

        var materialVariances = model.Variances
            .Where(row => row.PERIOD_MONTH.Date == month)
            .Where(row => string.Equals(row.MATERIAL_IND, "Y", StringComparison.OrdinalIgnoreCase))
            .GroupBy(row => row.DIMENSION!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        var decisionCounts = model.Decisions
            .Where(row => !string.IsNullOrWhiteSpace(row.WELL_ID))
            .GroupBy(row => row.WELL_ID!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        var cards = wells
            .Select(well => Diagnose(
                well,
                latestStatus.GetValueOrDefault(well.UWI!),
                rates.GetValueOrDefault(well.UWI!),
                openDeferments.GetValueOrDefault(well.UWI!),
                materialVariances.GetValueOrDefault(well.UWI!),
                decisionCounts.GetValueOrDefault(well.UWI!),
                month))
            .ToList();

        var selected = string.IsNullOrWhiteSpace(wellId)
            ? cards.FirstOrDefault()
            : cards.FirstOrDefault(well =>
                  string.Equals(well.WellId, wellId.Trim(), StringComparison.OrdinalIgnoreCase))
              ?? cards.FirstOrDefault();

        var decisions = model.Decisions
            .Where(row => string.IsNullOrWhiteSpace(fieldId)
                          || string.Equals(row.FIELD_ID, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(row => wellIds.Contains(row.WELL_ID ?? string.Empty))
            .OrderByDescending(row => row.RECORDED_AT)
            .ThenBy(row => row.WELL_ID, StringComparer.OrdinalIgnoreCase)
            .Select(row => ToDecision(row, model, wells))
            .ToList();

        var methods = Methods(model);

        return new OgInterventionWorkspace(
            string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
            Blank(fieldId),
            asOf,
            cards,
            selected,
            decisions,
            methods,
            methods.Count == 0,
            InterventionRules.Risks);
    }

    private static OgInterventionWell Diagnose(
        WELL well,
        WELL_STATUS? status,
        WellRate? rate,
        PRODUCTION_DEFERMENT? deferment,
        IReadOnlyList<RECONCILIATION_VARIANCE>? variances,
        int decisionCount,
        DateTime month)
    {
        var evidence = new List<OgInterventionEvidence>();
        var missing = new List<string>();
        var flags = new List<string>();

        // ---- the measured rate against the well's own history (PRD-01) ----
        if (rate is { Today: { } today, HistoryDays: >= InterventionRules.RequiredHistoryDays, Average: { } average })
        {
            var change = today - average;

            evidence.Add(new OgInterventionEvidence(
                "RATE",
                $"measured {OgProductionDay.Format(today, rate.Unit)} against an average of " +
                $"{OgProductionDay.Format(average, rate.Unit)} over {rate.HistoryDays.ToString(CultureInfo.InvariantCulture)} day(s)",
                change < 0m
                    ? OgInterventionEvidence.Negative
                    : change > 0m
                        ? OgInterventionEvidence.Positive
                        : OgInterventionEvidence.Neutral));

            if (change < 0m)
            {
                flags.Add(
                    $"the rate fell {OgProductionDay.Format(-change, rate.Unit)} below its own average");
            }
        }
        else
        {
            missing.Add(rate is null
                ? "no measured volume for the well"
                : $"fewer than {InterventionRules.RequiredHistoryDays.ToString(CultureInfo.InvariantCulture)} day(s) of rate history");
        }

        // ---- an open deferment (PRD-03) ----
        if (deferment is not null)
        {
            evidence.Add(new OgInterventionEvidence(
                "DEFERMENT",
                $"open deferment {deferment.REASON_CODE} since " +
                $"{deferment.START_AT.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}" +
                (deferment.DEFERRED_VOLUME is { } lost
                    ? $" (deferred {OgProductionDay.Format(lost, deferment.VOLUME_OUOM)})"
                    : string.Empty),
                OgInterventionEvidence.Negative));

            flags.Add($"a deferment is open for reason {deferment.REASON_CODE}");
        }
        else
        {
            missing.Add("no open deferment");
        }

        // ---- a material reconciliation variance this month (PRD-05) ----
        if (variances is { Count: > 0 })
        {
            foreach (var variance in variances.OrderBy(row => row.STAGE, StringComparer.OrdinalIgnoreCase))
            {
                evidence.Add(new OgInterventionEvidence(
                    "VARIANCE",
                    $"a material reconciliation variance at {variance.STAGE!.Trim().ToLowerInvariant()} of " +
                    $"{OgProductionDay.Format(variance.AMOUNT, variance.VOLUME_OUOM)} in " +
                    $"{month.ToString("yyyy-MM", CultureInfo.InvariantCulture)}" +
                    (string.IsNullOrWhiteSpace(variance.REASON_CODE)
                        ? " (no reason assigned)"
                        : $" (reason {variance.REASON_CODE!.Trim()})"),
                    OgInterventionEvidence.Neutral));
            }

            flags.Add(
                $"{variances.Count.ToString(CultureInfo.InvariantCulture)} material variance(s) in " +
                month.ToString("yyyy-MM", CultureInfo.InvariantCulture));
        }
        else
        {
            missing.Add("no material variance this month");
        }

        // ---- the recorded status (WEL-04) ----
        var statusText = Blank(status?.STATUS) ?? Blank(well.CURRENT_STATUS);

        if (statusText is not null)
        {
            var producing = string.Equals(statusText, "PRODUCING", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(statusText, "INJECTING", StringComparison.OrdinalIgnoreCase);

            evidence.Add(new OgInterventionEvidence(
                "STATUS",
                $"the recorded status is {statusText.ToLowerInvariant()}" +
                (status?.STATUS_DATE is { } since
                    ? $" since {since.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}"
                    : string.Empty),
                producing ? OgInterventionEvidence.Positive : OgInterventionEvidence.Negative));

            if (!producing)
            {
                flags.Add($"the well is {statusText.ToLowerInvariant()}");
            }
        }
        else
        {
            missing.Add("no status recorded");
        }

        // ---- conflicting evidence: recorded as down, measured above its own average ----
        var conflicts = new List<string>();

        var recordedDown = deferment is not null
                           || (statusText is not null
                               && !string.Equals(statusText, "PRODUCING", StringComparison.OrdinalIgnoreCase)
                               && !string.Equals(statusText, "INJECTING", StringComparison.OrdinalIgnoreCase));

        var measuredAbove = rate is { Today: { } measured, Average: { } mean }
                            && measured > mean;

        if (recordedDown && measuredAbove)
        {
            conflicts.Add(
                "the well is recorded as down but measured above its own average; both are shown and neither is " +
                "resolved automatically");
        }

        return new OgInterventionWell(
            well.UWI!,
            Blank(well.WELL_NAME),
            statusText ?? string.Empty,
            status?.STATUS_DATE ?? status?.START_TIME,
            rate?.Today,
            rate?.Average,
            rate?.Unit,
            rate is { Today: { } lastRate, Average: { } meanRate } ? lastRate - meanRate : null,
            rate?.HistoryDays ?? 0,
            evidence,
            missing,
            conflicts,
            InterventionRules.Confidence(evidence.Count, conflicts.Count > 0),
            flags,
            decisionCount);
    }

    private static OgInterventionDecision ToDecision(
        INTERVENTION_DECISION row,
        InterventionModel model,
        IReadOnlyList<WELL> wells)
    {
        var well = wells.FirstOrDefault(candidate =>
            string.Equals(candidate.UWI, row.WELL_ID, StringComparison.OrdinalIgnoreCase));

        var methodNames = model.Methods
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate.METHOD_CODE))
            .GroupBy(candidate => candidate.METHOD_CODE!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => Blank(group.First().METHOD_NAME),
                StringComparer.OrdinalIgnoreCase);

        var options = model.Options
            .Where(option => string.Equals(option.DECISION_ID, row.DECISION_ID, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(option => string.Equals(option.CHOSEN_IND, "Y", StringComparison.OrdinalIgnoreCase))
            .ThenBy(option => option.METHOD, StringComparer.OrdinalIgnoreCase)
            .Select(option =>
            {
                var method = (option.METHOD ?? string.Empty).Trim();

                methodNames.TryGetValue(method, out var name);

                var liftRelated = model.Methods.Any(candidate =>
                    string.Equals(candidate.METHOD_CODE?.Trim(), method, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(candidate.LIFT_RELATED_IND, "Y", StringComparison.OrdinalIgnoreCase));

                return new OgInterventionOption(
                    option.OPTION_ID ?? string.Empty,
                    method,
                    name,
                    option.ESTIMATED_GAIN,
                    Blank(option.GAIN_OUOM),
                    option.ESTIMATED_COST,
                    Blank(option.CURRENCY),
                    Blank(option.RISK),
                    Blank(option.BASIS) ?? string.Empty,
                    Blank(option.BASIS_TEXT),
                    string.Equals(option.CHOSEN_IND, "Y", StringComparison.OrdinalIgnoreCase),
                    liftRelated);
            })
            .ToList();

        return new OgInterventionDecision(
            row.DECISION_ID ?? string.Empty,
            row.WELL_ID ?? string.Empty,
            Blank(well?.WELL_NAME),
            Blank(row.STATE) ?? OgInterventionDecision.DecidedState,
            Blank(row.RATIONALE),
            Blank(row.OWNER),
            row.TARGET_DATE,
            Blank(row.WORK_ORDER_REF),
            row.WORK_ORDER_AT,
            Blank(row.WORK_ORDER_BY),
            options,
            row.RECORDED_AT);
    }

    /// <summary>
    /// Each well's last reported day and the mean of up to seven recorded days before it. Same grain as PRD-01 and
    /// PRD-03: the highest amendment per entity/day/activity, production rows only, one unit per well.
    /// </summary>
    private static Dictionary<string, WellRate> RateHistory(
        InterventionModel model,
        IReadOnlyDictionary<string, string> entityToWell,
        DateTime asOf)
    {
        var result = new Dictionary<string, WellRate>(StringComparer.OrdinalIgnoreCase);

        var rows = model.Volumes
            .Where(row => !string.IsNullOrWhiteSpace(row.PDEN_ID) && entityToWell.ContainsKey(row.PDEN_ID!))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => row.VOLUME_DATE is { } date && date.Date <= asOf)
            .Where(row => OperationsCentreRules.IsProductionRow(row.ACTIVITY_TYPE))
            .GroupBy(row => (row.PDEN_ID!.Trim(), Day: row.VOLUME_DATE!.Value.Date,
                Activity: (row.ACTIVITY_TYPE ?? string.Empty).Trim().ToUpperInvariant()))
            .Select(group => group.OrderByDescending(row => row.AMENDMENT_SEQ_NO).First())
            .ToList();

        foreach (var group in rows.GroupBy(row => entityToWell[row.PDEN_ID!], StringComparer.OrdinalIgnoreCase))
        {
            var unit = OperationsCentreRules.DominantUnit(group.Select(row => row.OIL_VOLUME_OUOM));

            if (unit is null)
            {
                continue;
            }

            var daily = group
                .Where(row => string.Equals((row.OIL_VOLUME_OUOM ?? string.Empty).Trim(), unit,
                    StringComparison.OrdinalIgnoreCase))
                .GroupBy(row => row.VOLUME_DATE!.Value.Date)
                .ToDictionary(day => day.Key, day => day.Sum(row => row.OIL_VOLUME));

            if (daily.Count == 0)
            {
                continue;
            }

            var last = daily.Keys.Max();

            var history = daily
                .Where(pair => pair.Key < last)
                .OrderByDescending(pair => pair.Key)
                .Take(InterventionRules.RequiredHistoryDays)
                .ToList();

            result[group.Key] = new WellRate(
                last,
                daily[last],
                history.Count == 0 ? null : history.Average(pair => pair.Value),
                history.Count,
                unit);
        }

        return result;
    }

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
            && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record WellRate(DateTime Day, decimal Today, decimal? Average, int HistoryDays, string? Unit);

    private sealed record InterventionModel(
        IReadOnlyList<WELL> Wells,
        IReadOnlyList<WELL_STATUS> Statuses,
        IReadOnlyList<PDEN_WELL> PdenWells,
        IReadOnlyList<PDEN_VOL_SUMMARY> Volumes,
        IReadOnlyList<PRODUCTION_DEFERMENT> Deferments,
        IReadOnlyList<RECONCILIATION_VARIANCE> Variances,
        IReadOnlyList<INTERVENTION_DECISION> Decisions,
        IReadOnlyList<INTERVENTION_OPTION> Options,
        IReadOnlyList<R_INTERVENTION_METHOD> Methods);
}
