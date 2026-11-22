using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Integrity;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;

/// <summary>One finding with the input that produced it (ENG-05, AC-4).</summary>
public sealed record OgIntegrityFinding(string Source, string Text)
{
    public string Describe() => Source + ": " + Text;
}

/// <summary>One well on the board (ENG-05's <c>OgWellIntegrity</c>).</summary>
/// <param name="Category">The guideline-117 category the owner's assessment produced - never computed here.</param>
/// <param name="Unassessed">True when no assessment is recorded, which is a different state from Green (AC-1).</param>
public sealed record OgWellIntegrity(
    string WellId,
    string? WellName,
    string? Field,
    string Category,
    bool Compliant,
    IReadOnlyList<OgIntegrityFinding> Findings,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> Envelopes,
    IReadOnlyList<string> MissingInputs,
    DateTime? AssessedAt,
    string? AssessedBy,
    double? AgeDays,
    bool Unassessed,
    bool Stale,
    int AnnuliAssessed,
    int FailingJoints,
    bool LeakToSurface,
    string StateText)
{
    public string AgeText => AgeDays is { } days
        ? days.ToString("0.#", CultureInfo.InvariantCulture) + " day(s) ago"
        : "never assessed";

    public string FindingsText => Findings.Count == 0
        ? "No finding was recorded."
        : string.Join(Environment.NewLine, Findings.Select(finding => "✗ " + finding.Describe()));

    public string ReasonsText => Reasons.Count == 0
        ? "The assessment recorded no reason."
        : string.Join(Environment.NewLine, Reasons);

    public string EnvelopesText => Envelopes.Count == 0
        ? "No barrier envelope is recorded."
        : string.Join(" · ", Envelopes);

    public string MissingText => MissingInputs.Count == 0
        ? "Every input was available."
        : "Partial inputs: " + string.Join(", ", MissingInputs);

    public string Describe() =>
        WellId + " · " + Category + " · " + StateText +
        (Findings.Count == 0 ? string.Empty : " · " + Findings.Count.ToString(CultureInfo.InvariantCulture) + " finding(s)");
}

/// <summary>One category's count on the board (ENG-05, AC-3).</summary>
public sealed record OgCategoryCount(string Category, int Count);

/// <summary>The integrity board for one field filter (ENG-05).</summary>
public sealed record OgIntegrityBoard(
    string? Field,
    string? CategoryFilter,
    double? OlderThanDays,
    IReadOnlyList<OgWellIntegrity> Wells,
    IReadOnlyList<WellIntegrity> Board,
    IReadOnlyList<OgCategoryCount> Counts,
    IReadOnlyList<string> Categories,
    int Scoped,
    DateTime ReadAt)
{
    public int Unassessed => Wells.Count(well => well.Unassessed);

    public string CountsText =>
        string.Join(" · ", Counts.Select(count => count.Category + " " + count.Count.ToString(CultureInfo.InvariantCulture))) +
        " · " + Scoped.ToString(CultureInfo.InvariantCulture) + " well(s) in scope · " +
        Unassessed.ToString(CultureInfo.InvariantCulture) + " unassessed";

    /// <summary>AC-2: the ordering is the owner's - category worst first, then the well id - so it is documented, not incidental.</summary>
    public const string OrderingText =
        "Worst first: Red, then Orange, then Yellow, then Green; ties are broken by the well id in ordinal order, so the " +
        "same field always lists the same way.";

    public string WellsText => Wells.Count == 0
        ? "No well matches the filter."
        : string.Join(Environment.NewLine, Wells.Select(well => well.Describe()));

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) +
        (CategoryFilter is null ? string.Empty : " · category " + CategoryFilter) +
        (OlderThanDays is null ? string.Empty : " · assessed more than " + OlderThanDays.Value.ToString("0.#", CultureInfo.InvariantCulture) + " day(s) ago");

    /// <summary>AC-5: the export is the owner's CSV over the same wells the board shows.</summary>
    public string Csv() => FieldIntegrity.ToCsv(Board);
}

/// <summary>The integrity-board rules ENG-05 states once.</summary>
public static class IntegrityBoardRules
{
    public const string CodeNoConnection = "integrity-board-no-connection";
    public const string CodeReadFailed = "integrity-board-read-failed";
    public const string CodeWriteFailed = "integrity-board-write-failed";
    public const string CodeActorRequired = "integrity-board-actor-required";
    public const string CodeCategoryUnknown = "integrity-board-category-unknown";

    public const string UnassessedState = "unassessed";

    /// <summary>The sources a finding can name (AC-4).</summary>
    public const string BarrierSource = "barrier set";
    public const string CasingPressureSource = "casing pressure";
    public const string MetalLossSource = "metal loss";
    public const string LeakSource = "leak";

    public static IReadOnlyList<string> Sources { get; } = [BarrierSource, CasingPressureSource, MetalLossSource, LeakSource];

    /// <summary>The four categories the board counts, in the owner's order.</summary>
    public static IReadOnlyList<string> Categories { get; } =
        Enum.GetValues<IntegrityCategory>().Select(category => category.ToString()).ToList();

    /// <summary>AC-3: the counts are the owner's, over the wells in scope.</summary>
    public static IReadOnlyList<OgCategoryCount> CountsOf(IEnumerable<WellIntegrity> wells)
    {
        ArgumentNullException.ThrowIfNull(wells);

        var counts = FieldIntegrity.Counts(wells);

        return Categories
            .Select(category => new OgCategoryCount(
                category,
                counts.TryGetValue(Enum.Parse<IntegrityCategory>(category), out var count) ? count : 0))
            .ToList();
    }

    /// <summary>
    /// AC-1: the state sentence. An unassessed well says so before its category, so the two can never be read as the same.
    /// </summary>
    public static string StateOf(string category, bool unassessed, bool stale, double? ageDays, IReadOnlyList<string> missing)
    {
        var parts = new List<string>();

        parts.Add(unassessed ? UnassessedState : category.ToLowerInvariant());

        if (!unassessed)
        {
            parts.Add("assessed " + (ageDays?.ToString("0.#", CultureInfo.InvariantCulture) ?? "at an unrecorded time") + " day(s) ago");
        }

        if (stale)
        {
            parts.Add("stale");
        }

        if (missing.Count > 0)
        {
            parts.Add("partial inputs");
        }

        return string.Join(" · ", parts);
    }

    /// <summary>A category filter the board understands, or null when the name is not one of the four.</summary>
    public static string? NormaliseCategory(string? category) =>
        string.IsNullOrWhiteSpace(category)
            ? null
            : Categories.FirstOrDefault(candidate => string.Equals(candidate, category.Trim(), StringComparison.OrdinalIgnoreCase));
}

/// <summary>ENG-05: the field integrity board and the recorded assessments.</summary>
public interface IOgIntegrityBoard
{
    Task<OgResult<OgIntegrityBoard>> LoadAsync(
        string? field,
        string? category,
        double? olderThanDays,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records an assessment for every well the board currently shows, so "last assessed" is a fact rather than the read time.
    /// </summary>
    Task<OgResult<OgIntegrityBoard>> RecordAsync(
        string? field,
        string? category,
        double? olderThanDays,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ENG-05 over the owner's <c>IntegrityStatus</c>, <c>FieldIntegrity</c> and <c>IntegrityBoardView</c>, reading the added
/// <c>WELL_BARRIER</c> and <c>INTEGRITY_ASSESSMENT</c> with the annulus tests ENG-04 records and the joint re-ratings ENG-03
/// records.
/// </summary>
/// <remarks>
/// <para>
/// The category is always <see cref="IntegrityStatus.Assess"/> - the guideline-117 reading of the barrier envelopes plus the
/// annulus and wall-loss evidence. The desktop supplies the inputs, names which input produced each finding, and records the
/// result; it never decides a colour.
/// </para>
/// <para>
/// The ordering is <see cref="FieldIntegrity.WorstFirst"/> and the export is <see cref="FieldIntegrity.ToCsv"/>, over the same
/// wells, so the board, the list and the CSV cannot disagree.
/// </para>
/// </remarks>
public sealed class OgIntegrityBoardService : IOgIntegrityBoard
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgIntegrityBoardService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgIntegrityBoard>> LoadAsync(
        string? field,
        string? category,
        double? olderThanDays,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgIntegrityBoard>.Failed(
                "No connection is open, so the integrity board cannot be read.",
                IntegrityBoardRules.CodeNoConnection);
        }

        try
        {
            return await ReadAsync(source, field, category, olderThanDays, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the integrity board", "the board is not shown; no category is invented", FailureSeverity.Error);

            return OgResult<OgIntegrityBoard>.Failed(
                "The integrity board could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? IntegrityBoardRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgIntegrityBoard>> RecordAsync(
        string? field,
        string? category,
        double? olderThanDays,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgIntegrityBoard>.Failed(
                "A recorded assessment records who made it.", IntegrityBoardRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgIntegrityBoard>.Failed(
                "No connection is open, so the assessment cannot be recorded.",
                IntegrityBoardRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, field, category, olderThanDays, cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var board = read.Value;
            var unit = OgUow.For<INTEGRITY_ASSESSMENT>(source);
            unit.PrimaryKey = nameof(INTEGRITY_ASSESSMENT.ASSESSMENT_ID);
            var now = DateTime.UtcNow;

            foreach (var well in board.Wells)
            {
                unit.Add(new INTEGRITY_ASSESSMENT
                {
                    ASSESSMENT_ID = Guid.NewGuid().ToString("N"),
                    UWI = well.WellId,
                    FIELD_NAME = well.Field,
                    CATEGORY = well.Category,
                    COMPLIANT = well.Compliant ? "Y" : "N",
                    REASONS_TEXT = string.Join("\n", well.Reasons),
                    FINDINGS_JSON = JsonSerializer.Serialize(well.Findings),
                    ENVELOPES_JSON = JsonSerializer.Serialize(well.Envelopes),
                    ANNULI_COUNT = well.AnnuliAssessed,
                    JOINTS_COUNT = well.FailingJoints,
                    LEAK_TO_SURFACE = well.LeakToSurface ? "Y" : "N",
                    ASSESSED_BY = actor.Trim(),
                    ASSESSED_AT_UTC = now,
                    ACTIVE_IND = "Y",
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgIntegrityBoard>.Failed(
                    OgUow.Message(committed, "The assessments were not recorded."),
                    IntegrityBoardRules.CodeWriteFailed);
            }

            return await ReadAsync(source, field, category, olderThanDays, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "recording the integrity assessment", "the assessment is not recorded", FailureSeverity.Error);

            return OgResult<OgIntegrityBoard>.Failed(
                "The assessment could not be recorded" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? IntegrityBoardRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the read model

    private async Task<OgResult<OgIntegrityBoard>> ReadAsync(
        IDataSource source,
        string? field,
        string? category,
        double? olderThanDays,
        CancellationToken cancellationToken)
    {
        var wells = await OgUow.ReadAllAsync<WELL>(source).ConfigureAwait(false);
        var tubulars = await OgUow.ReadAllAsync<WELL_TUBULAR>(source).ConfigureAwait(false);
        var barriers = await OgUow.ReadAllAsync<WELL_BARRIER>(source).ConfigureAwait(false);
        var annuli = await OgUow.ReadAllAsync<WELL_ANNULUS>(source).ConfigureAwait(false);
        var tests = await OgUow.ReadAllAsync<CASING_PRESSURE_TEST>(source).ConfigureAwait(false);
        var results = await OgUow.ReadAllAsync<MFC_RESULT>(source).ConfigureAwait(false);
        var joints = await OgUow.ReadAllAsync<MFC_JOINT>(source).ConfigureAwait(false);
        var assessments = await OgUow.ReadAllAsync<INTEGRITY_ASSESSMENT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var wanted = IntegrityBoardRules.NormaliseCategory(category);

        if (!string.IsNullOrWhiteSpace(category) && wanted is null)
        {
            return OgResult<OgIntegrityBoard>.Failed(
                "'" + category!.Trim() + "' is not a category; use " + string.Join(", ", IntegrityBoardRules.Categories) + ".",
                IntegrityBoardRules.CodeCategoryUnknown);
        }

        var scoped = wells
            .Where(well => !string.Equals(well.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(well => string.IsNullOrWhiteSpace(field) ||
                           string.Equals(well.ASSIGNED_FIELD?.Trim(), field.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderBy(well => well.UWI, StringComparer.Ordinal)
            .ToList();

        var views = new List<OgWellIntegrity>();
        var owner = new List<WellIntegrity>();

        foreach (var well in scoped)
        {
            var model = BuildModel(well.UWI.Trim(), tubulars, barriers);
            var (inputs, annuliCount, failingJoints) = InputsOf(well.UWI.Trim(), annuli, tests, results, joints);

            var assessment = IntegrityStatus.Assess(model, inputs);
            owner.Add(assessment);

            var latest = assessments
                .Where(row => string.Equals(row.UWI?.Trim(), well.UWI.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(row => row.ASSESSED_AT_UTC)
                .FirstOrDefault();

            var age = latest is null ? (double?)null : (DateTime.UtcNow - latest.ASSESSED_AT_UTC).TotalDays;
            var unassessed = latest is null;
            var stale = olderThanDays is { } limit && age is { } days && days > limit;

            var findings = FindingsOf(model, inputs, assessment);
            var missing = MissingInputs(model, annuliCount, failingJoints);

            views.Add(new OgWellIntegrity(
                well.UWI,
                well.WELL_NAME,
                well.ASSIGNED_FIELD,
                assessment.Category.ToString(),
                assessment.Compliant,
                findings,
                assessment.Reasons.ToList(),
                assessment.Envelopes.Select(pair => pair.Key + ": " + pair.Value).ToList(),
                missing,
                latest?.ASSESSED_AT_UTC,
                latest?.ASSESSED_BY,
                age,
                unassessed,
                stale,
                annuliCount,
                failingJoints,
                inputs.LeakToSurface,
                IntegrityBoardRules.StateOf(assessment.Category.ToString(), unassessed, stale, age, missing)));
        }

        // AC-3: the counts are the owner's, over the wells in scope (the field), before the list filters.
        var counts = IntegrityBoardRules.CountsOf(owner);

        // AC-2: the ordering is the owner's; the views follow the same sequence.
        var ordered = FieldIntegrity.WorstFirst(owner);
        var byWell = views.ToDictionary(view => view.WellId, StringComparer.OrdinalIgnoreCase);

        var listed = ordered
            .Select(assessment => byWell[assessment.Well])
            .Where(view => wanted is null || string.Equals(view.Category, wanted, StringComparison.OrdinalIgnoreCase))
            .Where(view => olderThanDays is not { } limit || (view.AgeDays is { } days && days > limit))
            .ToList();

        return OgResult<OgIntegrityBoard>.Ok(new OgIntegrityBoard(
            string.IsNullOrWhiteSpace(field) ? null : field.Trim(),
            wanted,
            olderThanDays,
            listed,
            FieldIntegrity.WorstFirst(owner).Where(assessment =>
                    wanted is null || string.Equals(assessment.Category.ToString(), wanted, StringComparison.OrdinalIgnoreCase))
                .Where(assessment => olderThanDays is not { } limit ||
                                     (views.First(view => string.Equals(view.WellId, assessment.Well, StringComparison.OrdinalIgnoreCase))
                                         .AgeDays is { } days && days > limit))
                .ToList(),
            counts,
            IntegrityBoardRules.Categories,
            scoped.Count,
            DateTime.UtcNow));
    }

    /// <summary>AC-4: every finding names the input it came from.</summary>
    private static IReadOnlyList<OgIntegrityFinding> FindingsOf(
        WellModel model,
        IntegrityInputs inputs,
        WellIntegrity assessment)
    {
        var findings = new List<OgIntegrityFinding>();

        if (model.Barriers.Count == 0)
        {
            findings.Add(new OgIntegrityFinding(
                IntegrityBoardRules.BarrierSource,
                "no barrier set is recorded for the well, so no envelope is verified"));
        }
        else
        {
            foreach (var (role, state) in assessment.Envelopes.Where(pair => pair.Value != BarrierElementStatus.Intact))
            {
                findings.Add(new OgIntegrityFinding(
                    IntegrityBoardRules.BarrierSource,
                    role.ToLowerInvariant() + " barrier is " +
                    (state == BarrierElementStatus.Impaired ? "degraded" : state.ToString().ToLowerInvariant())));
            }

            var failed = model.Barriers
                .SelectMany(set => set.Envelopes.SelectMany(envelope => envelope.Elements
                    .Where(element => element.Status == BarrierElementStatus.Failed)
                    .Select(element => envelope.Role.ToString().ToLowerInvariant() + " " + element.DisplayName)))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            foreach (var element in failed)
            {
                findings.Add(new OgIntegrityFinding(IntegrityBoardRules.BarrierSource, element + " has failed"));
            }
        }

        foreach (var (name, finding) in inputs.Annuli)
        {
            if (!finding.AboveThreshold)
            {
                continue;
            }

            var text = name + ": " +
                       (finding.BledToZero
                           ? "pressure above its criteria"
                           : "does not bleed to zero (leak rate above its criteria)") +
                       (finding.ExceedsMawop == true ? " and above its MAWOP" : string.Empty);

            findings.Add(new OgIntegrityFinding(IntegrityBoardRules.CasingPressureSource, text));
        }

        foreach (var joint in inputs.Joints.Where(joint => joint.Pass == false))
        {
            findings.Add(new OgIntegrityFinding(
                IntegrityBoardRules.MetalLossSource,
                "joint " + joint.Joint.Number.ToString(CultureInfo.InvariantCulture) + " of " +
                (joint.StringName ?? "an unnamed string") + " fails its design factors after corrosion (wall used " +
                (joint.Wall?.ToString("0.###", CultureInfo.InvariantCulture) ?? "?") + " in)"));
        }

        if (inputs.LeakToSurface)
        {
            findings.Add(new OgIntegrityFinding(IntegrityBoardRules.LeakSource, "a leak to surface is reported"));
        }

        return findings;
    }

    private static IReadOnlyList<string> MissingInputs(WellModel model, int annuli, int joints)
    {
        var missing = new List<string>();

        if (model.Barriers.Count == 0)
        {
            missing.Add("no barrier set");
        }

        if (annuli == 0)
        {
            missing.Add("no casing-pressure test");
        }

        if (joints == 0)
        {
            missing.Add("no caliper result");
        }

        return missing;
    }

    /// <summary>
    /// The inputs the owner's assessment takes: each annulus's most recent test re-run through the same classifier with the MAWOP
    /// it was judged against, and the joint re-ratings of the most recent caliper run.
    /// </summary>
    private (IntegrityInputs Inputs, int Annuli, int Joints) InputsOf(
        string wellId,
        IReadOnlyList<WELL_ANNULUS> annuli,
        IReadOnlyList<CASING_PRESSURE_TEST> tests,
        IReadOnlyList<MFC_RESULT> results,
        IReadOnlyList<MFC_JOINT> joints)
    {
        var inputs = new IntegrityInputs();
        var counted = 0;

        foreach (var annulus in annuli.Where(row =>
                     string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase)))
        {
            var latest = tests
                .Where(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
                .Where(row => string.Equals(row.ANNULUS_NAME?.Trim(), annulus.ANNULUS_NAME?.Trim(), StringComparison.OrdinalIgnoreCase))
                .Where(row => string.Equals(row.COMPLETE?.Trim(), "Y", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(row => row.RECORDED_AT_UTC)
                .FirstOrDefault();

            if (latest?.VALVE_CLOSED_UTC is not { } closed || latest.MAWOP is null)
            {
                continue;
            }

            var test = new CasingPressureTest
            {
                Annulus = latest.ANNULUS_NAME ?? annulus.ANNULUS_NAME,
                BleedStart = latest.BLEED_START_UTC,
                ValveClosed = closed,
            };

            foreach (var reading in ReadingsOf(latest.READINGS_JSON))
            {
                test.Readings.Add(reading);
            }

            if (test.Readings.Count == 0)
            {
                continue;
            }

            inputs.Annuli[latest.ANNULUS_NAME ?? annulus.ANNULUS_NAME] =
                CasingPressureDiagnostic.Classify(test, (double)latest.MAWOP.Value);

            counted++;
        }

        var result = results
            .Where(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.CHECKED_AT_UTC)
            .FirstOrDefault();

        var failing = 0;

        if (result is not null)
        {
            foreach (var row in joints.Where(row =>
                         string.Equals(row.MFC_RESULT_ID, result.MFC_RESULT_ID, StringComparison.OrdinalIgnoreCase)))
            {
                var pass = string.IsNullOrWhiteSpace(row.PASSES)
                    ? (bool?)null
                    : string.Equals(row.PASSES.Trim(), "Y", StringComparison.OrdinalIgnoreCase);

                inputs.Joints.Add(new JointRating
                {
                    Joint = new MfcJoint
                    {
                        Number = row.JOINT_NUMBER,
                        Unit = row.DEPTH_OUOM ?? "m",
                        Top = (double)row.TOP_DEPTH,
                        Bottom = (double)row.BOTTOM_DEPTH,
                        MaxPenetration = row.MAX_PENETRATION is { } penetration ? (double)penetration : null,
                        MaxPenetrationDepth = row.PENETRATION_DEPTH is { } depth ? (double)depth : null,
                        MaxMetalLoss = row.MAX_METAL_LOSS is { } loss ? (double)loss : null,
                        MinRemainingWall = row.MIN_REMAINING_WALL is { } wall ? (double)wall : null,
                    },
                    StringName = row.STRING_NAME,
                    Grade = row.GRADE,
                    Wall = row.WALL_USED is { } used ? (double)used : null,
                    NominalWall = row.NOMINAL_WALL is { } nominal ? (double)nominal : null,
                    Burst = row.BURST is { } burst ? (double)burst : null,
                    Collapse = row.COLLAPSE is { } collapse ? (double)collapse : null,
                    Tension = row.TENSION is { } tension ? (double)tension : null,
                    Pass = pass,
                });

                if (pass == false)
                {
                    failing++;
                }
            }
        }

        return (inputs, counted, failing);
    }

    private List<AnnulusPressureReading> ReadingsOf(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        StoredReading[]? stored;

        try
        {
            stored = JsonSerializer.Deserialize<StoredReading[]>(json, Json);
        }
        catch (JsonException ex)
        {
            // A stored series that no longer parses is reported, and the annulus is left out of the assessment rather than
            // judged on a partial series - the finding then comes from the barrier set and the other inputs.
            _failures.ReportHandled(
                ex, "reading a recorded pressure series", "the annulus is left out of the assessment",
                FailureSeverity.Degraded);

            return [];
        }

        return stored is null
            ? []
            : stored.Select(reading => new AnnulusPressureReading
            {
                Date = reading.At,
                Pressure = new Quantity((double)reading.Pressure,
                    string.IsNullOrWhiteSpace(reading.Unit) ? "psi" : reading.Unit),
            }).ToList();
    }

    /// <summary>
    /// The model the assessment reads: the well's strings by the id the annulus records reference, and its barrier sets grouped
    /// from the recorded elements.
    /// </summary>
    private static WellModel BuildModel(
        string wellId,
        IReadOnlyList<WELL_TUBULAR> tubulars,
        IReadOnlyList<WELL_BARRIER> barriers)
    {
        var model = new WellModel();
        model.Header.Name = wellId;

        foreach (var row in tubulars
                     .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                     .Where(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
                     .OrderBy(row => row.OUTSIDE_DIAMETER))
        {
            var unit = row.BASE_DEPTH_OUOM?.Trim() ?? "m";

            var str = new TubularString
            {
                Id = StringId(row),
                Name = string.IsNullOrWhiteSpace(row.TUBING_TYPE) ? "TUBULAR" : row.TUBING_TYPE!.Trim(),
                Kind = StringKind.Production,
                Top = new Quantity(row.HUNG_TOP_DEPTH > 0m ? (double)row.HUNG_TOP_DEPTH : 0d, unit),
                Bottom = new Quantity(row.SHOE_DEPTH > 0m ? (double)row.SHOE_DEPTH : (double)row.BASE_DEPTH, unit),
            };

            if (row.OUTSIDE_DIAMETER > 0m)
            {
                str.Attributes.OuterDiameter = new Quantity((double)row.OUTSIDE_DIAMETER, row.OUTSIDE_DIAMETER_OUOM ?? "in");
            }

            if (row.INSIDE_DIAMETER > 0m)
            {
                str.Attributes.InnerDiameter = new Quantity((double)row.INSIDE_DIAMETER, row.INSIDE_DIAMETER_OUOM ?? "in");
            }

            if (!string.IsNullOrWhiteSpace(row.TUBING_GRADE))
            {
                str.Attributes.Grade = row.TUBING_GRADE;
            }

            model.Strings.Add(str);
        }

        foreach (var set in barriers
                     .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                     .Where(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
                     .GroupBy(row => (row.PHASE ?? "Production") + "\u001f" + (row.REVISION ?? string.Empty) + "\u001f" +
                                     row.SET_DATE?.ToString("O", CultureInfo.InvariantCulture), StringComparer.Ordinal))
        {
            var first = set.First();

            var document = new WellBarrierSet
            {
                Phase = first.PHASE ?? "Production",
                Status = string.Equals(first.SET_STATUS?.Trim(), "Planned", StringComparison.OrdinalIgnoreCase)
                    ? BarrierDocumentStatus.Planned
                    : BarrierDocumentStatus.AsBuilt,
                Revision = first.REVISION,
                Date = first.SET_DATE,
                PreparedBy = first.PREPARED_BY,
                VerifiedBy = first.VERIFIED_BY,
            };

            foreach (var envelope in set.GroupBy(row => row.ENVELOPE_ROLE ?? "Primary", StringComparer.OrdinalIgnoreCase))
            {
                var role = Enum.TryParse<BarrierRole>(envelope.Key, ignoreCase: true, out var parsed)
                    ? parsed
                    : BarrierRole.Primary;

                var group = new BarrierEnvelope { Role = role };

                foreach (var row in envelope)
                {
                    group.Elements.Add(new BarrierElement
                    {
                        Id = row.BARRIER_ID,
                        Type = row.ELEMENT_TYPE,
                        Name = row.ELEMENT_NAME,
                        TargetKind = Enum.TryParse<BarrierTargetKind>(row.TARGET_KIND, ignoreCase: true, out var kind)
                            ? kind
                            : BarrierTargetKind.Component,
                        TargetId = row.TARGET_ID,
                        Top = row.TOP_DEPTH is { } top ? new Quantity((double)top, row.TOP_DEPTH_OUOM ?? "m") : null,
                        Bottom = row.BOTTOM_DEPTH is { } bottom ? new Quantity((double)bottom, row.BOTTOM_DEPTH_OUOM ?? "m") : null,
                        Status = Enum.TryParse<BarrierElementStatus>(row.ELEMENT_STATUS, ignoreCase: true, out var status)
                            ? status
                            : BarrierElementStatus.Unverified,
                        VerifiedDate = row.VERIFIED_DATE,
                        VerificationMethod = row.VERIFICATION_METHOD,
                        AcceptanceRef = row.ACCEPTANCE_REF,
                        Notes = row.NOTES,
                    });
                }

                document.Envelopes.Add(group);
            }

            model.Barriers.Add(document);
        }

        return model;
    }

    /// <summary>The identity an annulus and a barrier record reference: the tubular row's own key.</summary>
    public static string StringId(WELL_TUBULAR row) =>
        (row.UWI ?? string.Empty).Trim() + "|" + (row.SOURCE ?? string.Empty).Trim() + "|" +
        (row.TUBING_TYPE ?? string.Empty).Trim() + "|" +
        row.TUBING_OBS_NO.ToString(CultureInfo.InvariantCulture);

    private sealed record StoredReading(DateTime At, decimal Pressure, string? Unit);
}
