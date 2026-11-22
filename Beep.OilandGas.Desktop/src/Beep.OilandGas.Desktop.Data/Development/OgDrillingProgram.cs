using System.Globalization;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Development;

/// <summary>One design check of a program (DEV-05 AC-2).</summary>
public sealed record OgDrillCheck(
    string Name,
    string Status,
    string? Reference,
    string? Note,
    string? CheckedBy,
    DateTime? CheckedAt)
{
    public const string NotRun = "NOT_RUN";
    public const string Passed = "PASS";
    public const string Failed = "FAIL";

    public bool IsRun => !string.Equals(Status, NotRun, StringComparison.OrdinalIgnoreCase);

    public bool IsPassed => string.Equals(Status, Passed, StringComparison.OrdinalIgnoreCase);

    public bool IsFailed => string.Equals(Status, Failed, StringComparison.OrdinalIgnoreCase);

    /// <summary>AC-2: a check that was never run says so; it never reads as passed.</summary>
    public string StatusText => IsRun ? Status.ToLowerInvariant() : "not run";

    public string Text => $"{Name.Replace('_', ' ').ToLowerInvariant()}: {StatusText}" +
                          (string.IsNullOrWhiteSpace(Reference) ? string.Empty : $" ({Reference})") +
                          (string.IsNullOrWhiteSpace(Note) ? string.Empty : $" · {Note}");
}

/// <summary>One hole section (DEV-05).</summary>
public sealed record OgDrillSection(
    string Id,
    int Sequence,
    string? Name,
    decimal? HoleSize,
    string? HoleSizeUnit,
    decimal? Top,
    decimal? Bottom,
    string? DepthUnit,
    decimal? CasingOd,
    string? CasingOdUnit,
    decimal? MudWeight,
    string? MudWeightUnit,
    decimal? DurationDays,
    decimal? Cost)
{
    /// <summary>AC-1: what a submitted program still needs from this section.</summary>
    public IReadOnlyList<string> MissingItems => OgDrillProgramService.MissingItems(this);

    public bool IsComplete => MissingItems.Count == 0;

    public string HoleText => HoleSize is { } size
        ? $"{size.ToString("0.###", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(HoleSizeUnit) ? "(unit not recorded)" : HoleSizeUnit!.Trim())
        : "(no hole size)";

    public string DepthText => Top is { } top && Bottom is { } bottom
        ? $"{top.ToString("0.##", CultureInfo.InvariantCulture)}–" +
          $"{bottom.ToString("0.##", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(DepthUnit) ? "(unit not recorded)" : DepthUnit!.Trim())
        : "(no interval)";

    public string MudText => MudWeight is { } weight
        ? $"{weight.ToString("0.##", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(MudWeightUnit) ? "(unit not recorded)" : MudWeightUnit!.Trim())
        : "no mud weight";

    public string DurationText => DurationDays is { } days
        ? $"{days.ToString("0.##", CultureInfo.InvariantCulture)} d"
        : "no duration";

    public string CostText => Cost is { } cost
        ? cost.ToString("#,0.###", CultureInfo.InvariantCulture)
        : "no cost";

    public string Describe() =>
        $"{Sequence.ToString(CultureInfo.InvariantCulture)}. {Name ?? "(unnamed section)"} · {HoleText} · " +
        $"{DepthText} · {MudText} · {DurationText} · cost {CostText}";
}

/// <summary>One recorded review decision (DEV-05).</summary>
public sealed record OgDrillReview(
    string Decision,
    int ProgramVersion,
    string? Comments,
    string? Reviewer,
    DateTime? ReviewedAt)
{
    public string Describe() =>
        $"{Decision} on v{ProgramVersion.ToString(CultureInfo.InvariantCulture)} by {Reviewer ?? "(unknown)"} · " +
        $"{Comments ?? "no comment"}";
}

/// <summary>
/// A drilling program version (DEV-05). The state is the program's own column, the checks are its recorded rows, and
/// the total cost is the sum of the sections' costs (AC-6).
/// </summary>
public sealed record OgDrillProgram(
    string Id,
    string LineageId,
    string WellId,
    string FieldId,
    string? BaselineId,
    int Version,
    string State,
    string? Name,
    string? Objective,
    decimal? MeasuredDepth,
    decimal? TrueVerticalDepth,
    string? DepthUnit,
    decimal? DurationDays,
    string? Currency,
    string? PreparedBy,
    DateTime? PreparedAt,
    IReadOnlyList<OgDrillSection> Sections,
    IReadOnlyList<OgDrillCheck> Checks,
    IReadOnlyList<OgDrillReview> Reviews,
    IReadOnlyList<string> DesignMissing,
    bool DesignFound)
{
    public const string Draft = "DRAFT";
    public const string Submitted = "SUBMITTED";
    public const string Approved = "APPROVED";
    public const string ApprovedWithComments = "APPROVED_WITH_COMMENTS";
    public const string Returned = "RETURNED";
    public const string Rejected = "REJECTED";

    public static IReadOnlyList<string> States { get; } =
        [Draft, Submitted, Approved, ApprovedWithComments, Returned, Rejected];

    public bool IsEditable =>
        string.Equals(State, Draft, StringComparison.OrdinalIgnoreCase)
        || string.Equals(State, Returned, StringComparison.OrdinalIgnoreCase);

    public bool IsSubmitted => string.Equals(State, Submitted, StringComparison.OrdinalIgnoreCase);

    public bool IsApproved =>
        string.Equals(State, Approved, StringComparison.OrdinalIgnoreCase)
        || string.Equals(State, ApprovedWithComments, StringComparison.OrdinalIgnoreCase);

    public bool IsTerminal => IsApproved || string.Equals(State, Rejected, StringComparison.OrdinalIgnoreCase);

    public string StateText => State.ToLowerInvariant();

    /// <summary>AC-6: the total is the sum of the section costs; a section without a cost makes it a partial sum.</summary>
    public decimal TotalCost => Sections.Sum(section => section.Cost ?? 0m);

    public bool TotalIsComplete => Sections.Count > 0 && Sections.All(section => section.Cost is not null);

    public string TotalCostText => Sections.Count == 0
        ? "no section recorded, so no total"
        : TotalIsComplete
            ? $"{TotalCost.ToString("#,0.###", CultureInfo.InvariantCulture)} {Currency ?? "(currency not recorded)"}".TrimEnd()
            : $"partial {TotalCost.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
              $"{Currency ?? "(currency not recorded)"}".TrimEnd() +
              " (a section has no cost recorded)";

    public IReadOnlyList<OgDrillSection> IncompleteSections =>
        Sections.Where(section => !section.IsComplete).ToList();

    /// <summary>AC-1: every section must be complete, and a program needs at least one.</summary>
    public IReadOnlyList<string> SubmissionGaps
    {
        get
        {
            var gaps = new List<string>();

            if (Sections.Count == 0)
            {
                gaps.Add("no hole section is recorded");
            }

            gaps.AddRange(IncompleteSections
                .OrderBy(section => section.Sequence)
                .Select(section =>
                    $"section {section.Sequence.ToString(CultureInfo.InvariantCulture)}: " +
                    string.Join(", ", section.MissingItems)));

            if (!TotalIsComplete && Sections.Count > 0)
            {
                gaps.Add("a section has no cost, so the total is not the sum of the section costs");
            }

            return gaps;
        }
    }

    /// <summary>DEV-03's gate: a program cannot be submitted while the well design is incomplete.</summary>
    public bool CanSubmit => DesignFound && DesignMissing.Count == 0 && SubmissionGaps.Count == 0;

    public string ChecksText => Checks.Count == 0
        ? "no design check is recorded"
        : string.Join(" · ", Checks.Select(check => check.Text));

    public bool AnyCheckFailed => Checks.Any(check => check.IsFailed);

    public int SectionCount => Sections.Count;

    /// <summary>One line for the grid: each check and its status, so "not run" is visible at a glance (AC-2).</summary>
    public string ChecksSummary => Checks.Count == 0
        ? "no check recorded"
        : string.Join(" · ", Checks.Select(check =>
            $"{check.Name.Replace('_', ' ').ToLowerInvariant()} {check.StatusText}"));

    public string Describe() =>
        $"v{Version.ToString(CultureInfo.InvariantCulture)} · {StateText} · {Sections.Count.ToString(CultureInfo.InvariantCulture)} " +
        $"section(s) · total {TotalCostText}";
}

/// <summary>The programs of one field, newest version per lineage.</summary>
public sealed record OgDrillProgramWorkspace(
    string FieldName,
    IReadOnlyList<OgDrillProgram> Programs,
    IReadOnlyList<OgWellDesignOption> Wells)
{
    public bool IsEmpty => Programs.Count == 0;

    /// <summary>The wells that have a design and therefore can be authored into a program.</summary>
    public IReadOnlyList<OgWellDesignOption> DesigndWells => Wells.Where(well => well.DesignFound).ToList();
}

/// <summary>One designed well the program picker offers.</summary>
public sealed record OgWellDesignOption(
    string WellId,
    string? WellName,
    string DesignId,
    string DesignState,
    bool DesignFound,
    IReadOnlyList<string> Missing)
{
    public string Label => $"{WellName ?? WellId} · design {DesignState.ToLowerInvariant()}" +
                           (Missing.Count == 0
                               ? string.Empty
                               : $" · incomplete: {string.Join(", ", Missing)}");
}

/// <summary>The program header, as the page saves it.</summary>
public sealed record OgDrillProgramValues(
    string Name,
    string? Objective,
    decimal? MeasuredDepth,
    decimal? TrueVerticalDepth,
    string? DepthUnit,
    decimal? DurationDays,
    string? Currency);

/// <summary>One section, as the page saves it.</summary>
public sealed record OgDrillSectionValues(
    int Sequence,
    string? Name,
    decimal? HoleSize,
    string? HoleSizeUnit,
    decimal? Top,
    decimal? Bottom,
    string? DepthUnit,
    decimal? CasingOd,
    string? CasingOdUnit,
    decimal? MudWeight,
    string? MudWeightUnit,
    decimal? DurationDays,
    decimal? Cost);

/// <summary>One review decision, as the page submits it.</summary>
public sealed record OgDrillDecision(
    string Decision,
    string? Comments,
    string Reviewer,
    DateTimeOffset? DecidedAt = null)
{
    public const string Approve = "APPROVE";
    public const string ApproveWithComments = "APPROVE_WITH_COMMENTS";
    public const string Return = "RETURN";
    public const string Reject = "REJECT";

    public static IReadOnlyList<string> Decisions { get; } = [Approve, ApproveWithComments, Return, Reject];

    public string ResultingState => Decision.Trim().ToUpperInvariant() switch
    {
        Approve => OgDrillProgram.Approved,
        ApproveWithComments => OgDrillProgram.ApprovedWithComments,
        Return => OgDrillProgram.Returned,
        Reject => OgDrillProgram.Rejected,
        _ => string.Empty,
    };
}

/// <summary>DEV-05: authoring a drilling program and its independent review.</summary>
public interface IOgDrillingPrograms
{
    Task<OgResult<OgDrillProgramWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        CancellationToken cancellationToken = default);

    /// <summary>Every version of one program lineage, newest first (the version history).</summary>
    Task<OgResult<IReadOnlyList<OgDrillProgram>>> LoadVersionsAsync(
        string lineageId,
        CancellationToken cancellationToken = default);

    /// <summary>Creates version 1 for a designed well; its three checks start as not run (AC-2).</summary>
    Task<OgResult<OgDrillProgram>> CreateAsync(
        string companyId,
        string designId,
        OgDrillProgramValues values,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgDrillProgram>> SaveHeaderAsync(
        string programId,
        OgDrillProgramValues values,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgDrillProgram>> SaveSectionAsync(
        string programId,
        OgDrillSectionValues values,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Records a design check's status, including not run.</summary>
    Task<OgResult<OgDrillProgram>> RecordCheckAsync(
        string programId,
        string checkName,
        string status,
        string? reference,
        string? note,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Submits a complete program; an incomplete section or design is refused (AC-1, DEV-03).</summary>
    Task<OgResult<OgDrillProgram>> SubmitAsync(
        string programId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Records the reviewer's decision; the author cannot approve (AC-3).</summary>
    Task<OgResult<OgDrillProgram>> ReviewAsync(
        string programId,
        OgDrillDecision decision,
        CancellationToken cancellationToken = default);

    /// <summary>Appends a new version from an approved or returned program; the old version is untouched (AC-4).</summary>
    Task<OgResult<OgDrillProgram>> ReviseAsync(
        string programId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The submit that was not acknowledged, or null.</summary>
    OgDrillSubmit? Pending { get; }

    Task<OgResult<OgDrillProgram>> RetrySubmitAsync(CancellationToken cancellationToken = default);
}

/// <summary>One submit, retained when the acknowledgement was lost.</summary>
public sealed record OgDrillSubmit(string ProgramId, string Actor);

/// <summary>
/// The program over the DrillingAndConstruction-owned <c>DEVELOPMENT_DRILLING_PROGRAM</c>, <c>DRILLING_PROGRAM_SECTION</c>,
/// <c>DRILLING_PROGRAM_CHECK</c> and <c>DRILLING_PROGRAM_REVIEW</c>, with the DEV-03 design read for the completeness
/// gate, all through their entity models and a unit of work.
/// </summary>
/// <remarks>
/// This screen authors the plan-stage program in <c>DEVELOPMENT_DRILLING_PROGRAM</c>: the funded company program in
/// <c>DRILLING_PROGRAM</c> requires an AFE, an investment handoff and the bounded original draft proof, none of which
/// exists before the money is approved, so the two are separate records in the same module. The company program is
/// created from this one by the module's own flow; nothing here writes or reads the funded row set.
/// </remarks>
public sealed class OgDrillProgramService : IOgDrillingPrograms
{
    public const string CodeNoConnection = "drill-program-no-connection";
    public const string CodeReadFailed = "drill-program-read-failed";
    public const string CodeWriteFailed = "drill-program-write-failed";
    public const string CodeUnknown = "drill-program-unknown";
    public const string CodeDesignRequired = "drill-program-design-required";
    public const string CodeDesignUnknown = "drill-program-design-unknown";
    public const string CodeDesignIncomplete = "drill-program-design-incomplete";
    public const string CodeActorRequired = "drill-program-actor-required";
    public const string CodeNameRequired = "drill-program-name-required";
    public const string CodeDepthUnitRequired = "drill-program-depth-unit-required";
    public const string CodeDepthInvalid = "drill-program-depth-invalid";
    public const string CodeDurationInvalid = "drill-program-duration-invalid";
    public const string CodeSectionRequired = "drill-program-section-required";
    public const string CodeSectionIncomplete = "drill-program-section-incomplete";
    public const string CodeUnitRequired = "drill-program-unit-required";
    public const string CodeIntervalInvalid = "drill-program-interval-invalid";
    public const string CodeCostInvalid = "drill-program-cost-invalid";
    public const string CodeCurrencyRequired = "drill-program-currency-required";
    public const string CodeLocked = "drill-program-locked";
    public const string CodeAlreadySubmitted = "drill-program-already-submitted";
    public const string CodeTerminal = "drill-program-terminal";
    public const string CodeNotSubmitted = "drill-program-not-submitted";
    public const string CodeDecisionRequired = "drill-program-decision-required";
    public const string CodeCommentsRequired = "drill-program-comments-required";
    public const string CodeAuthorCannotApprove = "drill-program-author-cannot-approve";
    public const string CodeRevisionNeedsDecided = "drill-program-revision-needs-decision";
    public const string CodeChecksRequired = "drill-program-checks-required";
    public const string CodeCheckStatusInvalid = "drill-program-check-status-invalid";
    public const string CodeNothingPending = "drill-program-nothing-pending";

    /// <summary>The three design checks the plan names; each starts as not run (AC-2).</summary>
    public static IReadOnlyList<string> CheckNames { get; } = ["HYDRAULICS", "TORQUE_DRAG", "CASING_DESIGN"];

    public static IReadOnlyList<string> CheckStatuses { get; } =
        [OgDrillCheck.NotRun, OgDrillCheck.Passed, OgDrillCheck.Failed];

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgDrillProgramService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgDrillSubmit? Pending { get; private set; }

    /// <summary>AC-1: what a section still needs before a program can be submitted.</summary>
    public static IReadOnlyList<string> MissingItems(OgDrillSection section)
    {
        ArgumentNullException.ThrowIfNull(section);

        var missing = new List<string>();

        if (section.HoleSize is not > 0m)
        {
            missing.Add("hole size");
        }

        if (section.HoleSize is > 0m && string.IsNullOrWhiteSpace(section.HoleSizeUnit))
        {
            missing.Add("hole size unit");
        }

        if (section.Top is null)
        {
            missing.Add("top depth");
        }

        if (section.Bottom is null)
        {
            missing.Add("bottom depth");
        }

        if (string.IsNullOrWhiteSpace(section.DepthUnit))
        {
            missing.Add("depth unit");
        }

        if (section.DurationDays is not > 0m)
        {
            missing.Add("duration");
        }

        if (section.Cost is null)
        {
            missing.Add("cost");
        }

        if (section.CasingOd is > 0m && string.IsNullOrWhiteSpace(section.CasingOdUnit))
        {
            missing.Add("casing diameter unit");
        }

        if (section.MudWeight is > 0m && string.IsNullOrWhiteSpace(section.MudWeightUnit))
        {
            missing.Add("mud weight unit");
        }

        return missing;
    }

    /// <summary>The shared program header rules.</summary>
    public static (string Code, string Message)? ValidateProgram(OgDrillProgramValues values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(values.Name))
        {
            return (CodeNameRequired, "A drilling program needs a name.");
        }

        if (values.MeasuredDepth is { } md && md <= 0m)
        {
            return (CodeDepthInvalid, "The measured depth must be greater than zero.");
        }

        if (values.TrueVerticalDepth is { } tvd && tvd <= 0m)
        {
            return (CodeDepthInvalid, "The true vertical depth must be greater than zero.");
        }

        if (values.MeasuredDepth is { } measured && values.TrueVerticalDepth is { } vertical && vertical > measured)
        {
            return (CodeDepthInvalid, "The true vertical depth cannot exceed the measured depth.");
        }

        // AC-5: a depth without its unit is refused.
        if ((values.MeasuredDepth is > 0m || values.TrueVerticalDepth is > 0m)
            && string.IsNullOrWhiteSpace(values.DepthUnit))
        {
            return (CodeDepthUnitRequired, "A planned depth needs the unit it is measured in (m or ft).");
        }

        if (values.DurationDays is < 0m)
        {
            return (CodeDurationInvalid, "A planned duration cannot be negative.");
        }

        return null;
    }

    /// <summary>The shared section rules; a present value must carry its unit (AC-5).</summary>
    public static (string Code, string Message)? ValidateSection(OgDrillSectionValues values, string? programCurrency)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Sequence < 1)
        {
            return (CodeSectionRequired, "A section needs a sequence number of 1 or more.");
        }

        if (values.HoleSize is > 0m && string.IsNullOrWhiteSpace(values.HoleSizeUnit))
        {
            return (CodeUnitRequired, "A hole size needs the unit it is measured in (in or mm).");
        }

        if (values.HoleSize is < 0m)
        {
            return (CodeSectionIncomplete, "A hole size cannot be negative.");
        }

        if (values.Top is < 0m)
        {
            return (CodeIntervalInvalid, "A section's top depth cannot be negative.");
        }

        if (values.Top is { } top && values.Bottom is { } bottom && bottom <= top)
        {
            return (CodeIntervalInvalid,
                $"The section ends at {bottom.ToString("0.##", CultureInfo.InvariantCulture)}, which is not below its " +
                $"top at {top.ToString("0.##", CultureInfo.InvariantCulture)}.");
        }

        if ((values.Top is not null || values.Bottom is not null) && string.IsNullOrWhiteSpace(values.DepthUnit))
        {
            return (CodeDepthUnitRequired, "A section depth needs the unit it is measured in (m or ft).");
        }

        if (values.CasingOd is > 0m && string.IsNullOrWhiteSpace(values.CasingOdUnit))
        {
            return (CodeUnitRequired, "A casing diameter needs the unit it is measured in (in or mm).");
        }

        if (values.MudWeight is > 0m && string.IsNullOrWhiteSpace(values.MudWeightUnit))
        {
            return (CodeUnitRequired, "A mud weight needs its unit (for example ppg or sg).");
        }

        if (values.DurationDays is < 0m)
        {
            return (CodeDurationInvalid, "A section duration cannot be negative.");
        }

        if (values.Cost is < 0m)
        {
            return (CodeCostInvalid, "A section cost cannot be negative.");
        }

        // AC-6: the total is the sum of the section costs, so every cost is in the program's currency.
        if (values.Cost is > 0m && string.IsNullOrWhiteSpace(programCurrency))
        {
            return (CodeCurrencyRequired, "A section cost needs the program's currency.");
        }

        return null;
    }

    public async Task<OgResult<OgDrillProgramWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        CancellationToken cancellationToken = default)
    {        var source = _source();

        if (source is null)
        {
            return OgResult<OgDrillProgramWorkspace>.Failed(
                "No connection is open, so the drilling programs cannot be read.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            var designs = CurrentDesigns(model.Designs, model.Casing, model.DesignChecks);

            var wells = designs
                .Where(design => FieldMatches(design.FieldId, fieldId, fieldName))
                .OrderBy(design => design.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var scopedField = string.IsNullOrWhiteSpace(fieldName) ? null : fieldName.Trim();

            // Only this screen's own programs: the funded company rows carry an AFE and a handoff this screen cannot show.
            var newest = model.Programs
                .Where(program => FieldMatches(program.FIELD_ID, fieldId, fieldName))
                .GroupBy(program => Lineage(program), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.OrderByDescending(program => program.VERSION).First())
                .OrderBy(program => program.PROGRAM_NAME, StringComparer.OrdinalIgnoreCase)
                .ThenBy(program => program.PROGRAM_ID, StringComparer.Ordinal)
                .Select(program => ToProgram(program, model, designs))
                .ToList();

            return OgResult<OgDrillProgramWorkspace>.Ok(new OgDrillProgramWorkspace(
                string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : scopedField!,
                newest,
                wells.Select(design => new OgWellDesignOption(
                    design.WellId,
                    design.WellName,
                    design.DesignId,
                    design.DesignState,
                    true,
                    design.Missing)).ToList()));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the drilling programs",
                "the programs are not shown",
                FailureSeverity.Error);

            return OgResult<OgDrillProgramWorkspace>.Failed(
                "The drilling programs could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<IReadOnlyList<OgDrillProgram>>> LoadVersionsAsync(
        string lineageId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lineageId);

        var source = _source();

        if (source is null)
        {
            return OgResult<IReadOnlyList<OgDrillProgram>>.Failed(
                "No connection is open, so the program versions cannot be read.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var designs = CurrentDesigns(model.Designs, model.Casing, model.DesignChecks);

            var versions = model.Programs
                .Where(program => string.Equals(Lineage(program), lineageId.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(program => program.VERSION)
                .Select(program => ToProgram(program, model, designs))
                .ToList();

            return versions.Count == 0
                ? OgResult<IReadOnlyList<OgDrillProgram>>.Failed(
                    $"Program '{lineageId}' is not in this database.", CodeUnknown)
                : OgResult<IReadOnlyList<OgDrillProgram>>.Ok(versions);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reading the versions of program {lineageId}",
                "the version history is not shown",
                FailureSeverity.Error);

            return OgResult<IReadOnlyList<OgDrillProgram>>.Failed(
                "The program versions could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgDrillProgram>> CreateAsync(
        string companyId,
        string designId,
        OgDrillProgramValues values,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgDrillProgram>.Failed(
                "A program must record who created it.", CodeActorRequired);
        }

        if (ValidateProgram(values) is { } refusal)
        {
            return OgResult<OgDrillProgram>.Failed(refusal.Message, refusal.Code);
        }

        if (string.IsNullOrWhiteSpace(designId))
        {
            return OgResult<OgDrillProgram>.Failed(
                "Choose the designed well the program is for.", CodeDesignRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgDrillProgram>.Failed(
                "No connection is open, so the program cannot be created.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var design = CurrentDesigns(model.Designs, model.Casing, model.DesignChecks).FirstOrDefault(row =>
                string.Equals(row.DesignId, designId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (design is null)
            {
                return OgResult<OgDrillProgram>.Failed(
                    $"Design '{designId}' is not in this database.", CodeDesignUnknown);
            }

            var plan = model.Plans
                .Where(row => string.Equals(row.FDP_ID, model.Designs
                    .First(designRow => string.Equals(designRow.DESIGN_ID, design.DesignId, StringComparison.OrdinalIgnoreCase))
                    .FDP_ID, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            var baseline = plan is null
                ? null
                : model.Baselines
                    .Where(row => string.Equals(row.FDP_ID, plan.FDP_ID, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(row => row.VERSION)
                    .FirstOrDefault();

            var now = DateTime.UtcNow;
            var programId = Guid.NewGuid().ToString("N");

            var unit = OgUow.For<DEVELOPMENT_DRILLING_PROGRAM>(source);
            unit.PrimaryKey = "PROGRAM_ID";

            unit.Add(new DEVELOPMENT_DRILLING_PROGRAM
            {
                PROGRAM_ID = programId,
                LINEAGE_ID = programId,
                FDP_BASELINE_ID = baseline?.BASELINE_ID ?? string.Empty,
                VERSION = 1,
                WELL_ID = design.WellId,
                FIELD_ID = plan?.FIELD_ID ?? string.Empty,
                PROGRAM_NAME = values.Name.Trim(),
                DESCRIPTION_OBJECTIVE = Blank(values.Objective) ?? string.Empty,
                STATE = OgDrillProgram.Draft,
                PLANNED_MEASURED_DEPTH = values.MeasuredDepth,
                PLANNED_TRUE_VERTICAL_DEPTH = values.TrueVerticalDepth,
                DEPTH_OUOM = Blank(values.DepthUnit),
                PLANNED_DURATION_DAYS = values.DurationDays,
                CURRENCY = Blank(values.Currency)?.Trim().ToUpperInvariant(),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgDrillProgram>.Failed(
                    OgUow.Message(committed, "The program was not created."), CodeWriteFailed);
            }

            // AC-2: the three checks exist from the start, each recorded as not run.
            await EnsureChecksAsync(source, programId, actor.Trim(), now, cancellationToken).ConfigureAwait(false);

            return await ReloadAsync(source, programId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"creating a drilling program for design {designId}",
                "the program was not created",
                FailureSeverity.Error);

            return OgResult<OgDrillProgram>.Failed(
                "The program could not be created.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgDrillProgram>> SaveHeaderAsync(
        string programId,
        OgDrillProgramValues values,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgDrillProgram>.Failed(
                "A program change must record who made it.", CodeActorRequired);
        }

        if (ValidateProgram(values) is { } refusal)
        {
            return OgResult<OgDrillProgram>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgDrillProgram>.Failed(
                "No connection is open, so the program cannot be saved.", CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<DEVELOPMENT_DRILLING_PROGRAM>(source);
            unit.PrimaryKey = "PROGRAM_ID";

            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("PROGRAM_ID", programId.Trim())], cancellationToken)
                .ConfigureAwait(false);
            var program = rows.FirstOrDefault();

            if (program is null)
            {
                return OgResult<OgDrillProgram>.Failed(
                    $"Program '{programId}' is not in this database.", CodeUnknown);
            }

            if (!IsEditable(program.STATE))
            {
                return OgResult<OgDrillProgram>.Failed(
                    $"This program is {program.STATE}; revise it to a new version instead of editing it.",
                    CodeLocked);
            }

            program.PROGRAM_NAME = values.Name.Trim();
            program.DESCRIPTION_OBJECTIVE = Blank(values.Objective) ?? string.Empty;
            program.PLANNED_MEASURED_DEPTH = values.MeasuredDepth;
            program.PLANNED_TRUE_VERTICAL_DEPTH = values.TrueVerticalDepth;
            program.DEPTH_OUOM = Blank(values.DepthUnit);
            program.PLANNED_DURATION_DAYS = values.DurationDays;
            program.CURRENCY = Blank(values.Currency)?.Trim().ToUpperInvariant();
            program.ROW_CHANGED_BY = actor.Trim();
            program.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(program);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgDrillProgram>.Failed(
                    OgUow.Message(updated, "The program was not saved."), CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? await ReloadAsync(source, program.PROGRAM_ID!, cancellationToken).ConfigureAwait(false)
                : OgResult<OgDrillProgram>.Failed(
                    OgUow.Message(committed, "The program was not saved."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving drilling program {programId}",
                "the program was not saved",
                FailureSeverity.Error);

            return OgResult<OgDrillProgram>.Failed(
                "The program could not be saved.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgDrillProgram>> SaveSectionAsync(
        string programId,
        OgDrillSectionValues values,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgDrillProgram>.Failed(
                "A section change must record who made it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgDrillProgram>.Failed(
                "No connection is open, so the section cannot be saved.", CodeNoConnection);
        }

        try
        {
            var program = await ReadProgramAsync(source, programId.Trim(), cancellationToken).ConfigureAwait(false);

            if (program is null)
            {
                return OgResult<OgDrillProgram>.Failed(
                    $"Program '{programId}' is not in this database.", CodeUnknown);
            }

            if (!IsEditable(program.STATE))
            {
                return OgResult<OgDrillProgram>.Failed(
                    $"This program is {program.STATE}; revise it to a new version instead of editing it.",
                    CodeLocked);
            }

            if (ValidateSection(values, Blank(program.CURRENCY)) is { } refusal)
            {
                return OgResult<OgDrillProgram>.Failed(refusal.Message, refusal.Code);
            }

            var existing = (await OgUow.ReadAllAsync<DRILLING_PROGRAM_SECTION>(source).ConfigureAwait(false))
                .Where(row => string.Equals(row.PROGRAM_ID, program.PROGRAM_ID, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault(row => row.SECTION_SEQ == values.Sequence);

            var now = DateTime.UtcNow;
            var unit = OgUow.For<DRILLING_PROGRAM_SECTION>(source);
            unit.PrimaryKey = "SECTION_ID";

            if (existing is null)
            {
                unit.Add(new DRILLING_PROGRAM_SECTION
                {
                    SECTION_ID = Guid.NewGuid().ToString("N"),
                    PROGRAM_ID = program.PROGRAM_ID!,
                    SECTION_SEQ = values.Sequence,
                    SECTION_NAME = Blank(values.Name),
                    HOLE_SIZE = values.HoleSize ?? 0m,
                    HOLE_SIZE_OUOM = Blank(values.HoleSizeUnit)?.Trim(),
                    TOP_MD = values.Top ?? 0m,
                    BOTTOM_MD = values.Bottom ?? 0m,
                    DEPTH_OUOM = Blank(values.DepthUnit)?.Trim(),
                    CASING_OD = values.CasingOd ?? 0m,
                    CASING_OD_OUOM = Blank(values.CasingOdUnit)?.Trim(),
                    MUD_WEIGHT = values.MudWeight ?? 0m,
                    MUD_WEIGHT_OUOM = Blank(values.MudWeightUnit)?.Trim(),
                    DURATION_DAYS = values.DurationDays,
                    COST = values.Cost,
                    COST_CURRENCY = Blank(program.CURRENCY),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }
            else
            {
                existing.SECTION_NAME = Blank(values.Name);
                existing.HOLE_SIZE = values.HoleSize ?? 0m;
                existing.HOLE_SIZE_OUOM = Blank(values.HoleSizeUnit)?.Trim();
                existing.TOP_MD = values.Top ?? 0m;
                existing.BOTTOM_MD = values.Bottom ?? 0m;
                existing.DEPTH_OUOM = Blank(values.DepthUnit)?.Trim();
                existing.CASING_OD = values.CasingOd ?? 0m;
                existing.CASING_OD_OUOM = Blank(values.CasingOdUnit)?.Trim();
                existing.MUD_WEIGHT = values.MudWeight ?? 0m;
                existing.MUD_WEIGHT_OUOM = Blank(values.MudWeightUnit)?.Trim();
                existing.DURATION_DAYS = values.DurationDays;
                existing.COST = values.Cost;
                existing.COST_CURRENCY = Blank(program.CURRENCY);
                existing.ROW_CHANGED_BY = actor.Trim();
                existing.ROW_CHANGED_DATE = now;

                var updated = unit.Update(existing);

                if (!OgUow.IsOk(updated))
                {
                    return OgResult<OgDrillProgram>.Failed(
                        OgUow.Message(updated, "The section was not saved."), CodeWriteFailed);
                }
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? await ReloadAsync(source, program.PROGRAM_ID!, cancellationToken).ConfigureAwait(false)
                : OgResult<OgDrillProgram>.Failed(
                    OgUow.Message(committed, "The section was not saved."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving a section of drilling program {programId}",
                "the section was not saved",
                FailureSeverity.Error);

            return OgResult<OgDrillProgram>.Failed(
                "The section could not be saved.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgDrillProgram>> RecordCheckAsync(
        string programId,
        string checkName,
        string status,
        string? reference,
        string? note,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgDrillProgram>.Failed(
                "A check must record who ran it.", CodeActorRequired);
        }

        var name = (checkName ?? string.Empty).Trim().ToUpperInvariant();

        if (!CheckNames.Contains(name, StringComparer.Ordinal))
        {
            return OgResult<OgDrillProgram>.Failed(
                $"'{checkName}' is not a design check; use {string.Join(", ", CheckNames)}.", CodeChecksRequired);
        }

        var state = (status ?? string.Empty).Trim().ToUpperInvariant();

        if (!CheckStatuses.Contains(state, StringComparer.Ordinal))
        {
            return OgResult<OgDrillProgram>.Failed(
                $"'{status}' is not a check status; use {string.Join(", ", CheckStatuses)}.", CodeCheckStatusInvalid);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgDrillProgram>.Failed(
                "No connection is open, so the check cannot be recorded.", CodeNoConnection);
        }

        try
        {
            var program = await ReadProgramAsync(source, programId.Trim(), cancellationToken).ConfigureAwait(false);

            if (program is null)
            {
                return OgResult<OgDrillProgram>.Failed(
                    $"Program '{programId}' is not in this database.", CodeUnknown);
            }

            if (!IsEditable(program.STATE))
            {
                return OgResult<OgDrillProgram>.Failed(
                    $"This program is {program.STATE}; a check belongs to the version that is being authored.",
                    CodeLocked);
            }

            var rows = (await OgUow.ReadAllAsync<DRILLING_PROGRAM_CHECK>(source).ConfigureAwait(false))
                .Where(row => string.Equals(row.PROGRAM_ID, program.PROGRAM_ID, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault(row => string.Equals(row.CHECK_NAME, name, StringComparison.OrdinalIgnoreCase));

            var now = DateTime.UtcNow;
            var unit = OgUow.For<DRILLING_PROGRAM_CHECK>(source);
            unit.PrimaryKey = "CHECK_ID";

            if (rows is null)
            {
                unit.Add(new DRILLING_PROGRAM_CHECK
                {
                    CHECK_ID = Guid.NewGuid().ToString("N"),
                    PROGRAM_ID = program.PROGRAM_ID!,
                    CHECK_NAME = name,
                    STATUS = state,
                    REFERENCE = Blank(reference),
                    NOTE = Blank(note),
                    CHECKED_BY = state == OgDrillCheck.NotRun ? null : actor.Trim(),
                    CHECKED_AT = state == OgDrillCheck.NotRun ? null : now,
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }
            else
            {
                rows.STATUS = state;
                rows.REFERENCE = Blank(reference);
                rows.NOTE = Blank(note);
                rows.CHECKED_BY = state == OgDrillCheck.NotRun ? null : actor.Trim();
                rows.CHECKED_AT = state == OgDrillCheck.NotRun ? null : now;
                rows.ROW_CHANGED_BY = actor.Trim();
                rows.ROW_CHANGED_DATE = now;

                var updated = unit.Update(rows);

                if (!OgUow.IsOk(updated))
                {
                    return OgResult<OgDrillProgram>.Failed(
                        OgUow.Message(updated, "The check was not recorded."), CodeWriteFailed);
                }
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? await ReloadAsync(source, program.PROGRAM_ID!, cancellationToken).ConfigureAwait(false)
                : OgResult<OgDrillProgram>.Failed(
                    OgUow.Message(committed, "The check was not recorded."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference2 = _failures.ReportHandled(
                ex,
                $"recording the {checkName} check of program {programId}",
                "the check was not recorded",
                FailureSeverity.Error);

            return OgResult<OgDrillProgram>.Failed(
                "The check could not be recorded.", reference2 ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgDrillProgram>> SubmitAsync(
        string programId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var submitted = await SubmitCoreAsync(programId, actor, cancellationToken).ConfigureAwait(false);

        if (submitted.Success)
        {
            Pending = null;
        }

        return submitted;
    }

    public async Task<OgResult<OgDrillProgram>> ReviewAsync(
        string programId,
        OgDrillDecision decision,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(decision);

        if (string.IsNullOrWhiteSpace(decision.Reviewer))
        {
            return OgResult<OgDrillProgram>.Failed(
                "A review must record who decided.", CodeActorRequired);
        }

        var normalised = (decision.Decision ?? string.Empty).Trim().ToUpperInvariant();

        if (!OgDrillDecision.Decisions.Contains(normalised, StringComparer.Ordinal))
        {
            return OgResult<OgDrillProgram>.Failed(
                $"'{decision.Decision}' is not a review decision; use {string.Join(", ", OgDrillDecision.Decisions)}.",
                CodeDecisionRequired);
        }

        // A return, a rejection or an approval with comments is a decision with a reason.
        if (normalised != OgDrillDecision.Approve && string.IsNullOrWhiteSpace(decision.Comments))
        {
            return OgResult<OgDrillProgram>.Failed(
                $"A {normalised.Replace('_', ' ').ToLowerInvariant()} decision needs comments.", CodeCommentsRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgDrillProgram>.Failed(
                "No connection is open, so the review cannot be recorded.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var program = model.Programs.FirstOrDefault(row =>
                string.Equals(row.PROGRAM_ID, programId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (program is null)
            {
                return OgResult<OgDrillProgram>.Failed(
                    $"Program '{programId}' is not in this database.", CodeUnknown);
            }

            if (!string.Equals(program.STATE, OgDrillProgram.Submitted, StringComparison.OrdinalIgnoreCase))
            {
                // A retry after a partially applied review must not append a second decision for the same version.
                if (model.Reviews.Any(row =>
                        string.Equals(row.PROGRAM_ID, program.PROGRAM_ID, StringComparison.OrdinalIgnoreCase)
                        && row.PROGRAM_VERSION == program.VERSION))
                {
                    return await ReloadAsync(source, program.PROGRAM_ID!, cancellationToken).ConfigureAwait(false);
                }

                return OgResult<OgDrillProgram>.Failed(
                    $"This program is {program.STATE}; only a submitted program can be reviewed.",
                    CodeNotSubmitted);
            }

            // AC-3: the author cannot approve (or review) their own program.
            if (!string.IsNullOrWhiteSpace(program.ROW_CREATED_BY)
                && string.Equals(program.ROW_CREATED_BY.Trim(), decision.Reviewer.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgDrillProgram>.Failed(
                    $"{decision.Reviewer.Trim()} prepared this program, so they cannot review it. Another reviewer must.",
                    CodeAuthorCannotApprove);
            }

            var decidedAt = decision.DecidedAt ?? DateTimeOffset.UtcNow;
            var state = new OgDrillDecision(normalised, decision.Comments, decision.Reviewer).ResultingState;

            var unit = OgUow.For<DEVELOPMENT_DRILLING_PROGRAM>(source);
            unit.PrimaryKey = "PROGRAM_ID";

            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("PROGRAM_ID", program.PROGRAM_ID)], cancellationToken)
                .ConfigureAwait(false);
            var current = rows.FirstOrDefault();

            if (current is null)
            {
                return OgResult<OgDrillProgram>.Failed(
                    $"Program '{programId}' is not in this database.", CodeUnknown);
            }

            current.STATE = state;
            current.ROW_CHANGED_BY = decision.Reviewer.Trim();
            current.ROW_CHANGED_DATE = decidedAt.UtcDateTime;

            var updated = unit.Update(current);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgDrillProgram>.Failed(
                    OgUow.Message(updated, "The review was not recorded."), CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgDrillProgram>.Failed(
                    OgUow.Message(committed, "The review was not recorded."), CodeWriteFailed);
            }

            await AppendReviewAsync(
                source,
                current.PROGRAM_ID!,
                current.VERSION,
                normalised,
                decision.Comments,
                decision.Reviewer.Trim(),
                decidedAt.UtcDateTime,
                cancellationToken).ConfigureAwait(false);

            return await ReloadAsync(source, current.PROGRAM_ID!, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"recording the review of drilling program {programId}",
                "the review was not recorded",
                FailureSeverity.Error);

            return OgResult<OgDrillProgram>.Failed(
                "The review could not be recorded.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgDrillProgram>> ReviseAsync(
        string programId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgDrillProgram>.Failed(
                "A revision must record who made it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgDrillProgram>.Failed(
                "No connection is open, so the program cannot be revised.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var previous = model.Programs.FirstOrDefault(row =>
                string.Equals(row.PROGRAM_ID, programId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (previous is null)
            {
                return OgResult<OgDrillProgram>.Failed(
                    $"Program '{programId}' is not in this database.", CodeUnknown);
            }

            if (!string.Equals(previous.STATE, OgDrillProgram.Approved, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(previous.STATE, OgDrillProgram.ApprovedWithComments, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(previous.STATE, OgDrillProgram.Returned, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(previous.STATE, OgDrillProgram.Rejected, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgDrillProgram>.Failed(
                    "A revision starts from an approved or returned program; a working draft is edited in place.",
                    CodeRevisionNeedsDecided);
            }

            var lineage = Lineage(previous);
            var version = model.Programs
                .Where(row => string.Equals(Lineage(row), lineage, StringComparison.OrdinalIgnoreCase))
                .Select(row => row.VERSION)
                .DefaultIfEmpty(0)
                .Max() + 1;

            var now = DateTime.UtcNow;
            var newId = Guid.NewGuid().ToString("N");

            var unit = OgUow.For<DEVELOPMENT_DRILLING_PROGRAM>(source);
            unit.PrimaryKey = "PROGRAM_ID";

            unit.Add(new DEVELOPMENT_DRILLING_PROGRAM
            {
                PROGRAM_ID = newId,
                LINEAGE_ID = lineage,
                FDP_BASELINE_ID = previous.FDP_BASELINE_ID,
                VERSION = version,
                WELL_ID = previous.WELL_ID,
                FIELD_ID = previous.FIELD_ID,
                PROGRAM_NAME = previous.PROGRAM_NAME,
                DESCRIPTION_OBJECTIVE = previous.DESCRIPTION_OBJECTIVE,
                STATE = OgDrillProgram.Draft,
                PLANNED_MEASURED_DEPTH = previous.PLANNED_MEASURED_DEPTH,
                PLANNED_TRUE_VERTICAL_DEPTH = previous.PLANNED_TRUE_VERTICAL_DEPTH,
                DEPTH_OUOM = previous.DEPTH_OUOM,
                PLANNED_DURATION_DAYS = previous.PLANNED_DURATION_DAYS,
                CURRENCY = previous.CURRENCY,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgDrillProgram>.Failed(
                    OgUow.Message(committed, "The revision was not created."), CodeWriteFailed);
            }

            // The sections carry forward; the checks do not, because they were run against the previous version.
            await CopySectionsAsync(source, previous.PROGRAM_ID!, newId, actor.Trim(), now, cancellationToken)
                .ConfigureAwait(false);
            await EnsureChecksAsync(source, newId, actor.Trim(), now, cancellationToken).ConfigureAwait(false);

            return await ReloadAsync(source, newId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"revising drilling program {programId}",
                "the revision was not created",
                FailureSeverity.Error);

            return OgResult<OgDrillProgram>.Failed(
                "The program could not be revised.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgDrillProgram>> RetrySubmitAsync(CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgDrillProgram>.Failed(
                "There is no submit waiting to be retried.", CodeNothingPending);
        }

        var retried = await SubmitCoreAsync(pending.ProgramId, pending.Actor, cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    private async Task<OgResult<OgDrillProgram>> SubmitCoreAsync(
        string programId,
        string actor,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgDrillProgram>.Failed("A submit must record who sent it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgDrillProgram>.Failed(
                "No connection is open, so the program cannot be submitted.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var program = model.Programs.FirstOrDefault(row =>
                string.Equals(row.PROGRAM_ID, programId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (program is null)
            {
                return OgResult<OgDrillProgram>.Failed(
                    $"Program '{programId}' is not in this database.", CodeUnknown);
            }

            if (string.Equals(program.STATE, OgDrillProgram.Submitted, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgDrillProgram>.Failed(
                    "This program is already submitted.", CodeAlreadySubmitted);
            }

            if (!IsEditable(program.STATE))
            {
                return OgResult<OgDrillProgram>.Failed(
                    "This program is closed; start a revision instead of submitting it again.", CodeTerminal);
            }

            var designs = CurrentDesigns(model.Designs, model.Casing, model.DesignChecks);
            var design = designs.FirstOrDefault(row =>
                string.Equals(row.WellId, program.WELL_ID, StringComparison.OrdinalIgnoreCase));

            // DEV-03's gate: an incomplete design blocks authoring, and the refusal names the same items DEV-03 names.
            if (design is null)
            {
                return OgResult<OgDrillProgram>.Failed(
                    $"Well '{program.WELL_ID}' has no current design, so the program cannot be submitted.",
                    CodeDesignUnknown);
            }

            if (design.Missing.Count > 0)
            {
                return OgResult<OgDrillProgram>.Failed(
                    $"The design of {program.WELL_ID} is incomplete, so the program cannot be submitted. " +
                    $"It is missing: {string.Join(", ", design.Missing)}.",
                    CodeDesignIncomplete);
            }

            var current = ToProgram(program, model, designs);
            var gaps = current.SubmissionGaps;

            if (gaps.Count > 0)
            {
                return OgResult<OgDrillProgram>.Failed(
                    "The program cannot be submitted: " + string.Join("; ", gaps) + ".",
                    gaps[0].StartsWith("no hole section", StringComparison.OrdinalIgnoreCase)
                        ? CodeSectionRequired
                        : CodeSectionIncomplete);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<DEVELOPMENT_DRILLING_PROGRAM>(source);
            unit.PrimaryKey = "PROGRAM_ID";

            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("PROGRAM_ID", program.PROGRAM_ID)], cancellationToken)
                .ConfigureAwait(false);
            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgDrillProgram>.Failed(
                    $"Program '{programId}' is not in this database.", CodeUnknown);
            }

            row.STATE = OgDrillProgram.Submitted;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = now;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                Pending = new OgDrillSubmit(row.PROGRAM_ID!, actor.Trim());
                return OgResult<OgDrillProgram>.Failed(
                    OgUow.Message(updated, "The submit was not acknowledged; retry sends the same submit."),
                    CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                Pending = new OgDrillSubmit(row.PROGRAM_ID!, actor.Trim());
                return OgResult<OgDrillProgram>.Failed(
                    OgUow.Message(committed, "The submit was not acknowledged; retry sends the same submit."),
                    CodeWriteFailed);
            }

            return await ReloadAsync(source, row.PROGRAM_ID!, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = new OgDrillSubmit(programId.Trim(), actor.Trim());
            var reference = _failures.ReportHandled(
                ex,
                $"submitting drilling program {programId}",
                "the submit was not acknowledged; the original is retained for retry",
                FailureSeverity.Error);

            return OgResult<OgDrillProgram>.Failed(
                "The program could not be submitted; retry sends the same submit.", reference ?? CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ reads

    private static async Task<ProgramModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var programs = await OgUow.ReadAllAsync<DEVELOPMENT_DRILLING_PROGRAM>(source).ConfigureAwait(false);
        var sections = await OgUow.ReadAllAsync<DRILLING_PROGRAM_SECTION>(source).ConfigureAwait(false);
        var checks = await OgUow.ReadAllAsync<DRILLING_PROGRAM_CHECK>(source).ConfigureAwait(false);
        var reviews = await OgUow.ReadAllAsync<DRILLING_PROGRAM_REVIEW>(source).ConfigureAwait(false);
        var designs = await OgUow.ReadAllAsync<DEVELOPMENT_WELL_DESIGN>(source).ConfigureAwait(false);
        var casing = await OgUow.ReadAllAsync<DEVELOPMENT_CASING_STRING>(source).ConfigureAwait(false);
        var designChecks = await OgUow.ReadAllAsync<DEVELOPMENT_CASING_CHECK>(source).ConfigureAwait(false);
        var plans = await OgUow.ReadAllAsync<FIELD_DEVELOPMENT_PLAN>(source).ConfigureAwait(false);
        var baselines = await OgUow.ReadAllAsync<FDP_BASELINE>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new ProgramModel(programs, sections, checks, reviews, designs, casing, designChecks, plans, baselines);
    }

    private static async Task<DEVELOPMENT_DRILLING_PROGRAM?> ReadProgramAsync(
        IDataSource source,
        string programId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<DEVELOPMENT_DRILLING_PROGRAM>(source);
        unit.PrimaryKey = "PROGRAM_ID";

        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("PROGRAM_ID", programId)], cancellationToken)
            .ConfigureAwait(false);

        return rows.FirstOrDefault();
    }

    /// <summary>The current version of each well design, with DEV-03's completeness answer applied.</summary>
    private static IReadOnlyList<DesignLine> CurrentDesigns(
        IReadOnlyList<DEVELOPMENT_WELL_DESIGN> designs,
        IReadOnlyList<DEVELOPMENT_CASING_STRING> casing,
        IReadOnlyList<DEVELOPMENT_CASING_CHECK> checks) =>
        designs
            .Where(design => string.Equals(design.CURRENT_IND, "Y", StringComparison.OrdinalIgnoreCase)
                             || string.IsNullOrWhiteSpace(design.CURRENT_IND))
            .GroupBy(design => design.PLANNED_WELL_ID ?? design.DESIGN_ID, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(design => design.VERSION).First())
            .Select(design =>
            {
                var well = OgWellDesignService.ToWell(design, casing, checks);

                return new DesignLine(
                    design.DESIGN_ID ?? string.Empty,
                    well.Name,
                    design.PLANNED_WELL_ID ?? string.Empty,
                    design.FDP_ID ?? string.Empty,
                    design.FIELD_ID ?? string.Empty,
                    well.DesignState,
                    well.MissingItems);
            })
            .ToList();

    private static OgDrillProgram ToProgram(
        DEVELOPMENT_DRILLING_PROGRAM program,
        ProgramModel model,
        IReadOnlyList<DesignLine> designs)
    {
        var sections = model.Sections
            .Where(row => string.Equals(row.PROGRAM_ID, program.PROGRAM_ID, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.SECTION_SEQ)
            .Select(row => new OgDrillSection(
                row.SECTION_ID ?? string.Empty,
                row.SECTION_SEQ,
                Blank(row.SECTION_NAME),
                row.HOLE_SIZE > 0m ? row.HOLE_SIZE : null,
                Blank(row.HOLE_SIZE_OUOM),
                row.TOP_MD > 0m || row.BOTTOM_MD > 0m ? row.TOP_MD : null,
                row.BOTTOM_MD > 0m ? row.BOTTOM_MD : null,
                Blank(row.DEPTH_OUOM),
                row.CASING_OD > 0m ? row.CASING_OD : null,
                Blank(row.CASING_OD_OUOM),
                row.MUD_WEIGHT > 0m ? row.MUD_WEIGHT : null,
                Blank(row.MUD_WEIGHT_OUOM),
                row.DURATION_DAYS,
                row.COST))
            .ToList();

        var checks = model.Checks
            .Where(row => string.Equals(row.PROGRAM_ID, program.PROGRAM_ID, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => CheckOrder(row.CHECK_NAME))
            .Select(row => new OgDrillCheck(
                Blank(row.CHECK_NAME) ?? "(unnamed check)",
                Blank(row.STATUS) ?? OgDrillCheck.NotRun,
                Blank(row.REFERENCE),
                Blank(row.NOTE),
                Blank(row.CHECKED_BY),
                row.CHECKED_AT))
            .ToList();

        var reviews = model.Reviews
            .Where(row => string.Equals(row.PROGRAM_ID, program.PROGRAM_ID, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.REVIEWED_AT)
            .Select(row => new OgDrillReview(
                Blank(row.DECISION) ?? string.Empty,
                row.PROGRAM_VERSION,
                Blank(row.COMMENTS),
                Blank(row.REVIEWER),
                row.REVIEWED_AT))
            .ToList();

        var design = designs.FirstOrDefault(row =>
            string.Equals(row.WellId, program.WELL_ID, StringComparison.OrdinalIgnoreCase)
            || string.Equals(row.DesignWellId, program.WELL_ID, StringComparison.OrdinalIgnoreCase));

        return new OgDrillProgram(
            program.PROGRAM_ID ?? string.Empty,
            Lineage(program),
            program.WELL_ID ?? string.Empty,
            program.FIELD_ID ?? string.Empty,
            Blank(program.FDP_BASELINE_ID),
            program.VERSION,
            Blank(program.STATE) ?? OgDrillProgram.Draft,
            Blank(program.PROGRAM_NAME),
            Blank(program.DESCRIPTION_OBJECTIVE),
            program.PLANNED_MEASURED_DEPTH,
            program.PLANNED_TRUE_VERTICAL_DEPTH,
            Blank(program.DEPTH_OUOM),
            program.PLANNED_DURATION_DAYS,
            Blank(program.CURRENCY),
            Blank(program.ROW_CREATED_BY),
            program.ROW_CREATED_DATE,
            sections,
            checks,
            reviews,
            design?.Missing ?? [],
            design is not null);
    }

    private async Task<OgResult<OgDrillProgram>> ReloadAsync(
        IDataSource source,
        string programId,
        CancellationToken cancellationToken)
    {
        var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
        var program = model.Programs.FirstOrDefault(row =>
            string.Equals(row.PROGRAM_ID, programId, StringComparison.OrdinalIgnoreCase));

        if (program is null)
        {
            return OgResult<OgDrillProgram>.Failed(
                $"Program '{programId}' is not in this database.", CodeUnknown);
        }

        return OgResult<OgDrillProgram>.Ok(
            ToProgram(program, model, CurrentDesigns(model.Designs, model.Casing, model.DesignChecks)));
    }

    // ----------------------------------------------------------------- writes

    /// <summary>AC-2: the three checks exist as not run from the moment the program is created.</summary>
    private static async Task EnsureChecksAsync(
        IDataSource source,
        string programId,
        string actor,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var existing = (await OgUow.ReadAllAsync<DRILLING_PROGRAM_CHECK>(source).ConfigureAwait(false))
            .Where(row => string.Equals(row.PROGRAM_ID, programId, StringComparison.OrdinalIgnoreCase))
            .Select(row => row.CHECK_NAME ?? string.Empty)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = CheckNames.Where(name => !existing.Contains(name)).ToList();

        if (missing.Count == 0)
        {
            return;
        }

        var unit = OgUow.For<DRILLING_PROGRAM_CHECK>(source);
        unit.PrimaryKey = "CHECK_ID";

        foreach (var name in missing)
        {
            unit.Add(new DRILLING_PROGRAM_CHECK
            {
                CHECK_ID = Guid.NewGuid().ToString("N"),
                PROGRAM_ID = programId,
                CHECK_NAME = name,
                STATUS = OgDrillCheck.NotRun,
                ROW_CREATED_BY = actor,
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor,
                ROW_CHANGED_DATE = now,
            });
        }

        var committed = await unit.Commit().ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        if (!OgUow.IsOk(committed))
        {
            throw new InvalidOperationException(
                OgUow.Message(committed, "The program's check rows were not created."));
        }
    }

    private async Task CopySectionsAsync(
        IDataSource source,
        string fromProgramId,
        string toProgramId,
        string actor,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var sections = (await OgUow.ReadAllAsync<DRILLING_PROGRAM_SECTION>(source).ConfigureAwait(false))
            .Where(row => string.Equals(row.PROGRAM_ID, fromProgramId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.SECTION_SEQ)
            .ToList();

        if (sections.Count == 0)
        {
            return;
        }

        var unit = OgUow.For<DRILLING_PROGRAM_SECTION>(source);
        unit.PrimaryKey = "SECTION_ID";

        foreach (var row in sections)
        {
            unit.Add(new DRILLING_PROGRAM_SECTION
            {
                SECTION_ID = Guid.NewGuid().ToString("N"),
                PROGRAM_ID = toProgramId,
                SECTION_SEQ = row.SECTION_SEQ,
                SECTION_NAME = row.SECTION_NAME,
                HOLE_SIZE = row.HOLE_SIZE,
                HOLE_SIZE_OUOM = row.HOLE_SIZE_OUOM,
                TOP_MD = row.TOP_MD,
                BOTTOM_MD = row.BOTTOM_MD,
                DEPTH_OUOM = row.DEPTH_OUOM,
                CASING_OD = row.CASING_OD,
                CASING_OD_OUOM = row.CASING_OD_OUOM,
                MUD_WEIGHT = row.MUD_WEIGHT,
                MUD_WEIGHT_OUOM = row.MUD_WEIGHT_OUOM,
                DURATION_DAYS = row.DURATION_DAYS,
                COST = row.COST,
                COST_CURRENCY = row.COST_CURRENCY,
                ROW_CREATED_BY = actor,
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor,
                ROW_CHANGED_DATE = now,
            });
        }

        var committed = await unit.Commit().ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        if (!OgUow.IsOk(committed))
        {
            _failures.ReportHandled(
                new InvalidOperationException(
                    $"The sections of program {fromProgramId} were not copied to {toProgramId}: " +
                    OgUow.Message(committed, "the rows were not inserted")),
                $"carrying the sections into the new program version",
                "the new version is recorded but it has no sections",
                FailureSeverity.Degraded);
        }
    }

    private async Task AppendReviewAsync(
        IDataSource source,
        string programId,
        int version,
        string decision,
        string? comments,
        string reviewer,
        DateTime decidedAt,
        CancellationToken cancellationToken)
    {
        try
        {
            var unit = OgUow.For<DRILLING_PROGRAM_REVIEW>(source);
            unit.PrimaryKey = "REVIEW_ID";

            unit.Add(new DRILLING_PROGRAM_REVIEW
            {
                REVIEW_ID = Guid.NewGuid().ToString("N"),
                PROGRAM_ID = programId,
                PROGRAM_VERSION = version,
                DECISION = decision,
                COMMENTS = Blank(comments),
                REVIEWER = reviewer,
                REVIEWED_AT = decidedAt,
                ROW_CREATED_BY = reviewer,
                ROW_CREATED_DATE = decidedAt,
                ROW_CHANGED_BY = reviewer,
                ROW_CHANGED_DATE = decidedAt,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            if (!OgUow.IsOk(committed))
            {
                _failures.ReportHandled(
                    new InvalidOperationException(
                        $"The review row for program {programId} was not written: " +
                        OgUow.Message(committed, "the row was not inserted")),
                    $"recording the review of program {programId}",
                    "the decision stands but its history row is missing",
                    FailureSeverity.Degraded);
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            _failures.ReportHandled(
                ex,
                $"recording the review of program {programId}",
                "the decision stands but its history row is missing",
                FailureSeverity.Degraded);
        }
    }

    // ---------------------------------------------------------------- helpers

    private static bool IsEditable(string? state) =>
        string.Equals(state, OgDrillProgram.Draft, StringComparison.OrdinalIgnoreCase)
        || string.Equals(state, OgDrillProgram.Returned, StringComparison.OrdinalIgnoreCase);

    private static string Lineage(DEVELOPMENT_DRILLING_PROGRAM program) =>
        string.IsNullOrWhiteSpace(program.LINEAGE_ID)
            ? program.PROGRAM_ID ?? string.Empty
            : program.LINEAGE_ID!.Trim();

    private static int CheckOrder(string? name) =>
        name is null ? int.MaxValue : CheckNames.ToList().FindIndex(
            candidate => string.Equals(candidate, name.Trim(), StringComparison.OrdinalIgnoreCase)) is var index && index >= 0
            ? index
            : int.MaxValue;

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
            && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record DesignLine(
        string DesignId,
        string? Name,
        string DesignWellId,
        string PlanId,
        string FieldId,
        string DesignState,
        IReadOnlyList<string> Missing)
    {
        /// <summary>The planned well id is what a program is keyed to.</summary>
        public string WellId => DesignWellId;

        public string? WellName => Name;
    }

    private sealed record ProgramModel(
        IReadOnlyList<DEVELOPMENT_DRILLING_PROGRAM> Programs,
        IReadOnlyList<DRILLING_PROGRAM_SECTION> Sections,
        IReadOnlyList<DRILLING_PROGRAM_CHECK> Checks,
        IReadOnlyList<DRILLING_PROGRAM_REVIEW> Reviews,
        IReadOnlyList<DEVELOPMENT_WELL_DESIGN> Designs,
        IReadOnlyList<DEVELOPMENT_CASING_STRING> Casing,
        IReadOnlyList<DEVELOPMENT_CASING_CHECK> DesignChecks,
        IReadOnlyList<FIELD_DEVELOPMENT_PLAN> Plans,
        IReadOnlyList<FDP_BASELINE> Baselines);
}
