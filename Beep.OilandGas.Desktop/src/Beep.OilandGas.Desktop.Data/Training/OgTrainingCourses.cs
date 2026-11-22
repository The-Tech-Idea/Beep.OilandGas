using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Training;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Training;

/// <summary>One course as the list shows it (TRN-02).</summary>
public sealed record OgCourseRow(
    string Id,
    string Title,
    string? Description,
    double? PassMark,
    int ScenarioCount,
    string AttemptPolicy,
    string AttemptPolicyText)
{
    public string Describe() => Title + " · " + ScenarioCount.ToString(CultureInfo.InvariantCulture) + " scenario(s) · " +
                                (PassMark is { } mark
                                    ? "pass at " + (mark * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " %"
                                    : "no pass mark, so a result reports its score") +
                                " · " + AttemptPolicyText;
}

/// <summary>One scenario's place in a course, and where the trainee stands on it (TRN-02).</summary>
/// <param name="Attempts">How many attempts are recorded - none of them is the feature's "not attempted".</param>
public sealed record OgCourseProgress(
    string ScenarioId,
    string Title,
    bool Attempted,
    bool Counted,
    int Attempts,
    double Score,
    double MaxScore,
    bool? Passed,
    DateTime? LastUtc)
{
    public string Describe() => ScenarioId +
                                (string.IsNullOrWhiteSpace(Title) ? string.Empty : " (" + Title + ")") + " · " +
                                (!Attempted
                                    ? "not attempted"
                                    : OgTrainingRules.Figure(Score) + " of " + OgTrainingRules.Figure(MaxScore) +
                                      " in the counting attempt" + (Counted ? string.Empty : " (none counts)") +
                                      (Passed is { } passed ? passed ? " · passed" : " · not passed" : string.Empty) +
                                      " · " + Attempts.ToString(CultureInfo.InvariantCulture) + " attempt(s)") +
                                (Attempted && LastUtc is { } at
                                    ? " · last " + at.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                                    : string.Empty);
}

/// <summary>One trainee's result for one course (TRN-02's <c>OgCourseResult</c>).</summary>
/// <param name="Attempted">False when no scenario of the course has been attempted - "not attempted", which is not "failed".</param>
/// <param name="Best">The best single scenario attempt's fraction, which the page shows even when the policy is not "best".</param>
public sealed record OgCourseResult(
    string CourseId,
    string CourseTitle,
    string Trainee,
    int Attempts,
    double Best,
    double Latest,
    double Score,
    double MaxScore,
    bool Attempted,
    bool Completed,
    double? PassMark,
    bool? Passed,
    DateTime? At)
{
    public string Verdict => !Attempted
        ? "not attempted"
        : !Completed
            ? "in progress · " + Score.ToString("0.#", CultureInfo.InvariantCulture) + " of " +
              MaxScore.ToString("0.#", CultureInfo.InvariantCulture) + " point(s)"
            : Passed is { } passed
                ? passed ? "PASSED" : "NOT PASSED"
                : "complete · no pass mark, so the score stands alone";

    public string Describe() => CourseTitle + " · " + Verdict +
                                (Attempted
                                    ? " · " + Attempts.ToString(CultureInfo.InvariantCulture) + " attempt(s) · best " +
                                      (Best * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " % · latest " +
                                      (Latest * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " %"
                                    : string.Empty) +
                                (At is { } at
                                    ? " · " + at.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                                    : string.Empty);
}

/// <summary>One trainee in the cohort view (TRN-02).</summary>
public sealed record OgCohortRow(string Trainee, int Courses, int Offered, int Passed, int Attempts)
{
    public double? PassRate => Offered == 0 ? null : (double)Passed / Offered;

    public string Describe() => Trainee + " · " + Passed.ToString(CultureInfo.InvariantCulture) + " of " +
                                Offered.ToString(CultureInfo.InvariantCulture) + " course(s) passed" +
                                (PassRate is { } rate
                                    ? " (" + (rate * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " %)"
                                    : string.Empty) + " · " +
                                Attempts.ToString(CultureInfo.InvariantCulture) + " attempt(s)";
}

/// <summary>A scenario the cohort does worst on (TRN-02's "weakest scenarios").</summary>
public sealed record OgWeakScenario(string ScenarioId, string Title, int Attempts, int FullyCorrect, double MeanFraction)
{
    public double? CorrectRate => Attempts == 0 ? null : (double)FullyCorrect / Attempts;

    public string Describe() => ScenarioId + (string.IsNullOrWhiteSpace(Title) ? string.Empty : " (" + Title + ")") + " · " +
                                Attempts.ToString(CultureInfo.InvariantCulture) + " attempt(s) · " +
                                FullyCorrect.ToString(CultureInfo.InvariantCulture) + " fully correct · mean " +
                                (MeanFraction * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " %";
}

/// <summary>What the ledger file is, and whether it can be read and written (TRN-02, AC-3 and the feature's open question).</summary>
public sealed record OgLedgerStatus(string Path, bool Exists, bool Readable, bool Writable, int Attempts, string? Problem)
{
    public string Describe() => Path + " · " + (Exists ? "exists" : "not yet written") +
                                " · " + Attempts.ToString(CultureInfo.InvariantCulture) + " attempt(s)" +
                                (Readable ? string.Empty : " · NOT READABLE") +
                                (Writable ? string.Empty : " · NOT WRITABLE") +
                                (Problem is null ? string.Empty : " · " + Problem);
}

/// <summary>The courses, the trainee's results and the cohort (TRN-02).</summary>
public sealed record OgCoursesWorkspace(
    string Trainee,
    IReadOnlyList<OgCourseRow> Courses,
    IReadOnlyList<OgCourseProgress> Progress,
    IReadOnlyList<OgCourseResult> Results,
    IReadOnlyList<OgCohortRow> Cohort,
    IReadOnlyList<OgWeakScenario> Weakest,
    OgLedgerStatus Ledger,
    IReadOnlyList<string> Problems,
    IReadOnlyList<string> Notes,
    DateTime ReadAt)
{
    public double? CohortPassRate
    {
        get
        {
            var attempted = Cohort.Where(row => row.Offered > 0).ToList();

            return attempted.Count == 0 ? null : (double)attempted.Sum(row => row.Passed) / attempted.Sum(row => row.Offered);
        }
    }

    public string SummaryText => Courses.Count == 0
        ? "No course file is present, so there is nothing to group: each scenario stands alone (TRN-01's runner)."
        : Courses.Count.ToString(CultureInfo.InvariantCulture) + " course(s) · " +
          Results.Count(result => result.Passed == true).ToString(CultureInfo.InvariantCulture) + " passed by " +
          (string.IsNullOrWhiteSpace(Trainee) ? "this trainee" : Trainee) + " · cohort pass rate " +
          (CohortPassRate is { } rate
              ? (rate * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " %"
              : "unknown, no course has been attempted") + " · " +
          Ledger.Attempts.ToString(CultureInfo.InvariantCulture) + " attempt(s) in the ledger";

    public string CoursesText => Courses.Count == 0
        ? "No course is listed."
        : string.Join(Environment.NewLine, Courses.Select((course, index) =>
            (index + 1).ToString(CultureInfo.InvariantCulture) + ". " + course.Describe()));

    public string ProgressText => Progress.Count == 0
        ? "No course is selected."
        : string.Join(Environment.NewLine, Progress.Select(row => "· " + row.Describe()));

    public string ResultsText => Results.Count == 0
        ? "No course result is available."
        : string.Join(Environment.NewLine, Results.Select(result => "· " + result.Describe()));

    public string CohortText => Cohort.Count == 0
        ? "No trainee has an attempt in any course."
        : string.Join(Environment.NewLine, Cohort.Select(row => "· " + row.Describe()));

    public string WeakestText => Weakest.Count == 0
        ? "No scenario has been attempted, so no weak scenario can be named."
        : string.Join(Environment.NewLine, Weakest.Select(row => "· " + row.Describe()));

    public string LedgerText => Ledger.Describe();

    public string ProblemsText => Problems.Count == 0
        ? "Every course and scenario file the folder holds was read."
        : string.Join(Environment.NewLine, Problems.Select(problem => "✗ " + problem));

    public string NotesText => Notes.Count == 0
        ? "The page recorded nothing further."
        : string.Join(Environment.NewLine, Notes.Select(note => "! " + note));

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>One attempt recorded against a course (TRN-02).</summary>
/// <param name="Recorded">False when the ledger could not be written: the marking is still shown, marked not recorded.</param>
public sealed record OgCourseRecording(
    string ScenarioId,
    string Title,
    string? CourseId,
    double Score,
    double MaxScore,
    bool FullyCorrect,
    bool? Passed,
    string? ScenarioVersion,
    string? CourseVersion,
    bool Recorded,
    string? WriteFailure,
    DateTime At)
{
    public string Describe() => Title + " · " + OgTrainingRules.Figure(Score) + " of " +
                                OgTrainingRules.Figure(MaxScore) +
                                (Passed is { } passed ? passed ? " · passed" : " · not passed" : " · no pass mark") +
                                " · scenario version " + (ScenarioVersion ?? "(not stamped)") +
                                (CourseId is null
                                    ? string.Empty
                                    : " · course " + CourseId + " version " + (CourseVersion ?? "(not stamped)")) +
                                " · " + (Recorded ? "recorded" : "NOT RECORDED: " + (WriteFailure ?? "the ledger was not written"));
}

/// <summary>The course rules TRN-02 states once.</summary>
public static class OgCourseRules
{
    public const string CodeReadFailed = "course-read-failed";
    public const string CodeCourseRequired = "course-course-required";
    public const string CodeCourseUnknown = "course-course-unknown";
    public const string CodeScenarioRequired = "course-scenario-required";
    public const string CodeTraineeRequired = "course-trainee-required";
    public const string CodeIncomplete = "course-answer-incomplete";
    public const string CodeScenarioNotInCourse = "course-scenario-not-in-course";
    public const string CodeLedger = "course-ledger";

    /// <summary>How many characters of the content fingerprint are shown: enough to tell two versions apart at a glance.</summary>
    public const int VersionLength = 16;

    /// <summary>
    /// A file's version: a fingerprint of its content. A scenario and a course have no version field of their own, so this is what the
    /// attempt is stamped with - a corrected answer key or a reordered course then reads as a different version against the attempt that
    /// used the older one (AC-4).
    /// </summary>
    public static string? VersionOf(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);

        return Convert.ToHexString(hash)[..VersionLength].ToLowerInvariant();
    }

    /// <summary>AC-5: the cohort pass rate is passed ÷ attempted, and null when nothing was attempted - not zero.</summary>
    public static double? PassRate(int passed, int attempted) =>
        attempted <= 0 ? null : (double)passed / attempted;

    /// <summary>The pass mark in force for a course: the course's, else the toolbox setting, else none.</summary>
    public static double? PassMarkOf(CoursePack course)
    {
        ArgumentNullException.ThrowIfNull(course);

        return course.PassMark ?? TrainingToolbox.Settings.PassMark;
    }

    public static string PolicyText(string policy) => OgTrainingRules.PolicyText(policy);

    public static string Figure(double value) => OgTrainingRules.Figure(value);
}

/// <summary>TRN-02: the courses, the results ledger and the cohort.</summary>
public interface IOgTrainingCourses
{
    /// <summary>The courses, the trainee's progress and results, the cohort and the ledger's state.</summary>
    Task<OgResult<OgCoursesWorkspace>> LoadAsync(
        string trainee, string? courseId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// AC-4 and AC-3: marks an attempt against a course, stamps it with the versions of the files it was scored against, records it -
    /// and when the ledger cannot be written, still returns the marking, marked not recorded.
    /// </summary>
    Task<OgResult<OgCourseRecording>> RecordAsync(
        string scenarioId,
        IReadOnlyDictionary<string, OgScenarioAnswer> answers,
        string trainee,
        string courseId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// TRN-02 over the tools' <see cref="CourseLibrary"/>, <see cref="CoursePack"/>, <see cref="CourseResult"/> and
/// <see cref="ResultsLedger"/>, with the marking TRN-01 already drives.
/// </summary>
/// <remarks>
/// <para>
/// The grouping, the counted attempt, the course verdict and the ledger are the engine's: <see cref="ResultsLedger.Counted"/> applies
/// the policy, <see cref="ResultsLedger.Course(string, CoursePack, IReadOnlyList{Scenario})"/> produces a trainee's course result and
/// the cohort comes from the same function for each trainee. What the desktop adds is the version stamp the engine has no field for,
/// the wording that separates "not attempted" from "failed", the weakest-scenario view and the record.
/// </para>
/// <para>
/// A ledger write failure is not an exception to the person: the marking is the engine's and it is shown, with the attempt marked not
/// recorded and the reason beside it (AC-3).
/// </para>
/// </remarks>
public sealed class OgTrainingCoursesService : IOgTrainingCourses
{
    private readonly IFailureReporter _failures;
    private readonly Func<string?> _folder;
    private readonly Func<string> _ledgerFile;

    public OgTrainingCoursesService(IFailureReporter failures, Func<string?> folder, Func<string> ledgerFile)
    {
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _folder = folder ?? throw new ArgumentNullException(nameof(folder));
        _ledgerFile = ledgerFile ?? throw new ArgumentNullException(nameof(ledgerFile));
    }

    public Task<OgResult<OgCoursesWorkspace>> LoadAsync(
        string trainee, string? courseId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            TrainingToolbox.Register();

            var problems = new List<string>();
            var notes = new List<string>();
            var folder = _folder();
            var name = trainee?.Trim() ?? string.Empty;

            var scenarios = folder is null ? [] : ScenarioLibrary.LoadFolder(folder, problems);
            var courses = folder is null ? [] : CourseLibrary.LoadFolder(folder, scenarios, problems);

            if (folder is null)
            {
                notes.Add("No scenario folder is configured, so no course can be read. Write the sample pack on the scenarios page " +
                          "or set training.scenarioFolders.");
            }

            var ledger = ReadLedger(out var ledgerNote, out var ledgerProblem);

            if (ledgerNote is not null)
            {
                notes.Add(ledgerNote);
            }

            var rows = courses.Select(course => new OgCourseRow(
                course.Id,
                course.Title,
                course.Description,
                OgCourseRules.PassMarkOf(course),
                course.Scenarios.Count,
                TrainingToolbox.Settings.AttemptPolicy,
                OgCourseRules.PolicyText(TrainingToolbox.Settings.AttemptPolicy))).ToList();

            // The engine produces the trainee's course result; the page only words it. "Not attempted" is the case where no scenario of
            // the course has an attempt at all - which is not a failure.
            var results = new List<OgCourseResult>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                foreach (var course in courses)
                {
                    var result = ledger.Course(name, course, scenarios);
                    var mine = ledger.Attempts
                        .Where(attempt => string.Equals(attempt.Trainee, name, StringComparison.Ordinal) &&
                                          course.Scenarios.Contains(attempt.ScenarioId, StringComparer.Ordinal))
                        .ToList();

                    var attempted = mine.Count > 0;

                    results.Add(new OgCourseResult(
                        course.Id,
                        course.Title,
                        name,
                        mine.Count,
                        mine.Count == 0 ? 0 : mine.Max(attempt => attempt.Fraction),
                        mine.Count == 0 ? 0 : mine.OrderBy(attempt => attempt.Utc).Last().Fraction,
                        result.Score,
                        result.MaxScore,
                        attempted,
                        result.Completed,
                        OgCourseRules.PassMarkOf(course),
                        attempted ? result.Passed : null,
                        mine.Count == 0 ? null : mine.Max(attempt => attempt.Utc)));
                }
            }

            var progress = new List<OgCourseProgress>();

            if (courseId is not null && courses.FirstOrDefault(course =>
                    string.Equals(course.Id, courseId.Trim(), StringComparison.Ordinal)) is { } chosen)
            {
                foreach (var id in chosen.Scenarios)
                {
                    var scenario = scenarios.FirstOrDefault(candidate =>
                        string.Equals(candidate.Id, id, StringComparison.Ordinal));

                    var mine = string.IsNullOrWhiteSpace(name)
                        ? []
                        : ledger.Attempts
                            .Where(attempt => string.Equals(attempt.Trainee, name, StringComparison.Ordinal) &&
                                              string.Equals(attempt.ScenarioId, id, StringComparison.Ordinal))
                            .ToList();

                    var counted = string.IsNullOrWhiteSpace(name) ? null : ledger.Counted(name, id);

                    progress.Add(new OgCourseProgress(
                        id,
                        scenario?.Title ?? "(no such scenario in the folder)",
                        mine.Count > 0,
                        counted is not null,
                        mine.Count,
                        counted?.Score ?? 0,
                        counted?.MaxScore ?? scenario?.MaxScore ?? 0,
                        counted?.Passed,
                        mine.Count == 0 ? null : mine.Max(attempt => attempt.Utc)));
                }
            }

            // The cohort is every trainee with an attempt in a course, and the same engine function gives each one's result.
            var cohort = new List<OgCohortRow>();

            foreach (var course in courses)
            {
                foreach (var result in ledger.Course(course, scenarios))
                {
                    var index = cohort.FindIndex(row => string.Equals(row.Trainee, result.Trainee, StringComparison.Ordinal));

                    if (index < 0)
                    {
                        cohort.Add(new OgCohortRow(result.Trainee, 0, 0, 0, 0));
                        index = cohort.Count - 1;
                    }

                    var row = cohort[index];

                    cohort[index] = row with
                    {
                        Courses = row.Courses + 1,
                        Offered = row.Offered + 1,
                        Passed = row.Passed + (result.Passed == true ? 1 : 0),
                        Attempts = row.Attempts + ledger.Attempts.Count(attempt =>
                            string.Equals(attempt.Trainee, result.Trainee, StringComparison.Ordinal) &&
                            course.Scenarios.Contains(attempt.ScenarioId, StringComparer.Ordinal)),
                    };
                }
            }

            // The weakest scenarios are the ones the cohort scores least well on, by mean fraction over every recorded attempt.
            var weakest = ledger.Attempts
                .GroupBy(attempt => attempt.ScenarioId, StringComparer.Ordinal)
                .Select(group => new OgWeakScenario(
                    group.Key,
                    scenarios.FirstOrDefault(scenario => string.Equals(scenario.Id, group.Key, StringComparison.Ordinal))?.Title ??
                    string.Empty,
                    group.Count(),
                    group.Count(attempt => attempt.Questions.Count > 0 && attempt.Questions.All(question => question.FullyCorrect)),
                    group.Average(attempt => attempt.Fraction)))
                .OrderBy(row => row.MeanFraction)
                .ThenBy(row => row.ScenarioId, StringComparer.Ordinal)
                .Take(5)
                .ToList();

            var path = _ledgerFile();

            return Task.FromResult(OgResult<OgCoursesWorkspace>.Ok(new OgCoursesWorkspace(
                name,
                rows,
                progress,
                results,
                cohort.OrderBy(row => row.Trainee, StringComparer.Ordinal).ToList(),
                weakest,
                new OgLedgerStatus(path, File.Exists(path), ledgerProblem is null, Writable(path), ledger.Attempts.Count,
                    ledgerProblem),
                problems,
                notes,
                DateTime.UtcNow)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the courses and the results ledger", "the courses are not shown", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgCoursesWorkspace>.Failed(
                "The courses could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgCourseRules.CodeReadFailed));
        }
    }

    public Task<OgResult<OgCourseRecording>> RecordAsync(
        string scenarioId,
        IReadOnlyDictionary<string, OgScenarioAnswer> answers,
        string trainee,
        string courseId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(trainee))
        {
            return Task.FromResult(OgResult<OgCourseRecording>.Failed(
                "An attempt is recorded against a trainee; none was named.", OgCourseRules.CodeTraineeRequired));
        }

        if (string.IsNullOrWhiteSpace(scenarioId))
        {
            return Task.FromResult(OgResult<OgCourseRecording>.Failed(
                "An attempt is recorded against a scenario; none was named.", OgCourseRules.CodeScenarioRequired));
        }

        if (string.IsNullOrWhiteSpace(courseId))
        {
            return Task.FromResult(OgResult<OgCourseRecording>.Failed(
                "A course attempt is recorded in a course; none was named.", OgCourseRules.CodeCourseRequired));
        }

        try
        {
            var problems = new List<string>();
            var folder = _folder();

            if (folder is null)
            {
                return Task.FromResult(OgResult<OgCourseRecording>.Failed(
                    "No scenario folder is configured, so no course can be taken.", OgCourseRules.CodeCourseUnknown));
            }

            var scenarios = ScenarioLibrary.LoadFolder(folder, problems);
            var courses = CourseLibrary.LoadFolder(folder, scenarios, problems);

            var course = courses.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, courseId.Trim(), StringComparison.Ordinal));

            if (course is null)
            {
                return Task.FromResult(OgResult<OgCourseRecording>.Failed(
                    "There is no course '" + courseId.Trim() + "' in '" + folder + "'.", OgCourseRules.CodeCourseUnknown));
            }

            if (!course.Scenarios.Contains(scenarioId.Trim(), StringComparer.Ordinal))
            {
                return Task.FromResult(OgResult<OgCourseRecording>.Failed(
                    "Scenario '" + scenarioId.Trim() + "' is not one of the course '" + course.Id + "'.",
                    OgCourseRules.CodeScenarioNotInCourse));
            }

            var scenario = scenarios.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, scenarioId.Trim(), StringComparison.Ordinal));

            if (scenario is null)
            {
                return Task.FromResult(OgResult<OgCourseRecording>.Failed(
                    "No scenario '" + scenarioId.Trim() + "' is in '" + folder + "'.", OgTrainingRules.CodeScenarioUnknown));
            }

            var missing = OgTrainingRules.Missing(scenario, answers);

            if (missing.Count > 0)
            {
                return Task.FromResult(OgResult<OgCourseRecording>.Failed(
                    "The attempt is not recorded: " + string.Join(", ", missing) + " still unanswered.",
                    OgCourseRules.CodeIncomplete));
            }

            var ledger = ReadLedger(out _, out _);
            var mark = OgCourseRules.PassMarkOf(course);

            // The ledger scores and records; the versions are stamped on the attempt it returns, so the record says which files it was
            // scored against (AC-4).
            var attempt = ledger.Record(trainee.Trim(), scenario, OgTrainingRules.AnswersOf(answers), course.Id, mark);

            attempt.ScenarioVersion = OgCourseRules.VersionOf(scenario.File);
            attempt.CourseVersion = OgCourseRules.VersionOf(course.File);

            string? failure = null;

            try
            {
                WriteLedger(ledger);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                // AC-3: the marking is the engine's and it is shown; what failed is the writing, and that is said rather than hidden.
                failure = _failures.ReportHandled(
                    ex, "writing the results ledger", "the attempt is shown but not recorded", FailureSeverity.Error);
            }

            var scored = ScenarioScoring.Score(scenario, OgTrainingRules.AnswersOf(answers));

            return Task.FromResult(OgResult<OgCourseRecording>.Ok(new OgCourseRecording(
                scenario.Id,
                scenario.Title,
                course.Id,
                scored.Score,
                scored.MaxScore,
                scored.Questions.All(question => question.FullyCorrect),
                mark is { } pass ? scored.Fraction >= pass - 1e-9 : null,
                attempt.ScenarioVersion,
                attempt.CourseVersion,
                Recorded: failure is null,
                WriteFailure: failure,
                attempt.Utc)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "recording the attempt for " + scenarioId, "the attempt is not recorded", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgCourseRecording>.Failed(
                "The attempt could not be recorded" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgCourseRules.CodeLedger));
        }
    }

    // ------------------------------------------------------------------ the ledger file

    private ResultsLedger ReadLedger(out string? note, out string? problem)
    {
        note = null;
        problem = null;

        var file = _ledgerFile();

        if (!File.Exists(file))
        {
            return new ResultsLedger();
        }

        try
        {
            return ResultsLedger.FromJson(File.ReadAllText(file));
        }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or FormatException or IOException or
                                       UnauthorizedAccessException)
        {
            // A ledger that cannot be read is reported, and the page says so: a result shown from an unreadable ledger would be a
            // result the application invented.
            var reference = _failures.ReportHandled(
                ex, "reading the training results ledger", "the recorded attempts are not shown", FailureSeverity.Degraded);

            problem = "the ledger could not be read (reference " + reference + ")";

            return new ResultsLedger();
        }
    }

    private void WriteLedger(ResultsLedger ledger)
    {
        var file = _ledgerFile();
        var folder = Path.GetDirectoryName(file);

        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        // Written to a temporary file and moved into place, so a failed write cannot leave half a ledger behind.
        var temporary = file + ".tmp";

        File.WriteAllText(temporary, ledger.ToJson(), new UTF8Encoding(false));
        File.Move(temporary, file, overwrite: true);
    }

    /// <summary>
    /// Whether the ledger can be written, checked by opening it for writing and closing it again. A ledger that cannot be written is
    /// exactly what AC-3 must not hide, so the refusal is reported and the page states it before a person loses an attempt to it.
    /// </summary>
    private bool Writable(string file)
    {
        try
        {
            var folder = Path.GetDirectoryName(file);

            if (string.IsNullOrWhiteSpace(folder))
            {
                return false;
            }

            Directory.CreateDirectory(folder);

            // A file where the ledger should be, or a read-only one, both mean the next write will fail.
            if (File.Exists(file))
            {
                using var stream = File.Open(file, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
            }

            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            _failures.ReportHandled(
                ex, "checking that the results ledger can be written",
                "the ledger is reported as not writable, so an attempt would be marked not recorded", FailureSeverity.Degraded);

            return false;
        }
    }
}
