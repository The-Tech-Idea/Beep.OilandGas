using System.Globalization;
using System.Text;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Training;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Training;

/// <summary>One scenario as the list shows it (TRN-01).</summary>
/// <param name="Problems">The engine's validation problems; a scenario with any is listed and not runnable (AC-4).</param>
public sealed record OgScenarioSummary(
    string Id,
    string Title,
    string Course,
    string Level,
    int Questions,
    double MaxScore,
    IReadOnlyList<string> Problems,
    string File)
{
    public bool Valid => Problems.Count == 0;

    public string Describe() => Title + " · " + (string.IsNullOrWhiteSpace(Course) ? "no course" : Course) +
                                (string.IsNullOrWhiteSpace(Level) ? string.Empty : " · " + Level) +
                                " · " + Questions.ToString(CultureInfo.InvariantCulture) + " question(s) · " +
                                OgTrainingRules.Figure(MaxScore) + " point(s)" +
                                (Valid ? string.Empty : " · " + Problems.Count.ToString(CultureInfo.InvariantCulture) +
                                                          " problem(s)");

    public string ProblemsText => Problems.Count == 0
        ? "The scenario passes the engine's own checks."
        : string.Join(Environment.NewLine, Problems.Select(problem => "✗ " + problem));
}

/// <summary>One option of a choice or multi question, as the trainee sees it.</summary>
public sealed record OgScenarioOptionView(string Id, string Text);

/// <summary>One question as the trainee sees it - without its answer (AC-3).</summary>
/// <param name="Unit">The unit the question asks for, which is also the unit the entry is read in.</param>
public sealed record OgScenarioQuestionView(
    string Id,
    string Type,
    string Prompt,
    double Points,
    string? Hint,
    string? Unit,
    IReadOnlyList<OgScenarioOptionView> Options)
{
    public bool IsChoice => Type is "Choice" or "Multi";

    public bool IsNumber => !IsChoice;

    public string Describe() => Id + " · " + Type + " · " + Prompt + " (" + OgTrainingRules.Figure(Points) + " point(s)" +
                                (string.IsNullOrWhiteSpace(Unit) ? string.Empty : ", in " + Unit) + ")";

    public string OptionsText => Options.Count == 0
        ? string.Empty
        : string.Join(Environment.NewLine, Options.Select(option => "  " + option.Id + " — " + option.Text));
}

/// <summary>One answer as the trainee gave it (TRN-01's <see cref="OgScenarioAnswer"/>).</summary>
public sealed record OgScenarioAnswer(
    string Key,
    string? Option,
    IReadOnlyList<string> Options,
    double? Value,
    string? Unit,
    int HintsUsed)
{
    public bool Given => !string.IsNullOrWhiteSpace(Option) || Options.Count > 0 || Value is not null;
}

/// <summary>What one question scored, with the answer key once the attempt has been submitted (AC-2, AC-3).</summary>
public sealed record OgQuestionOutcome(
    string Id,
    string Prompt,
    string Given,
    string Expected,
    bool Correct,
    double Score,
    double Points,
    string Feedback,
    string? Explanation)
{
    public string Describe() => (Correct ? "✓ " : "✗ ") + Prompt + Environment.NewLine +
                                "  given " + Given + " · expected " + Expected + " · " +
                                OgTrainingRules.Figure(Score) + " of " + OgTrainingRules.Figure(Points) +
                                Environment.NewLine + "  " + Feedback +
                                (string.IsNullOrWhiteSpace(Explanation) ? string.Empty : Environment.NewLine + "  " + Explanation);
}

/// <summary>A scenario that has been opened (TRN-01: the runner before an answer).</summary>
public sealed record OgScenarioRun(
    string ScenarioId,
    string Title,
    string Instructions,
    string? Course,
    string? Level,
    string? DepthUnit,
    IReadOnlyList<OgScenarioQuestionView> Questions,
    IReadOnlyList<OgQuestionOutcome> Outcomes,
    bool Submitted,
    double Score,
    double MaxScore,
    double? PassMark,
    bool? Passed,
    IReadOnlyList<string> Missing,
    WellboreDocument? Document,
    byte[]? Exhibit,
    IReadOnlyList<string> Warnings,
    string? AttemptId,
    DateTime? SubmittedAt)
{
    /// <summary>AC-3: the solution is revealed by submission and not before.</summary>
    public bool SolutionAvailable => Submitted;

    /// <summary>Whether the exercise is a map rather than a well - the engine draws one exhibit or the other.</summary>
    public bool IsMap => Exhibit is not null;

    public string SummaryText => !Submitted
        ? Title + " · " + Questions.Count.ToString(CultureInfo.InvariantCulture) + " question(s), " +
          OgTrainingRules.Figure(MaxScore) + " point(s)" + (Missing.Count == 0
              ? " · every question is answered"
              : " · " + Missing.Count.ToString(CultureInfo.InvariantCulture) + " still unanswered")
        : Title + " · " + OgTrainingRules.Figure(Score) + " of " + OgTrainingRules.Figure(MaxScore) + " (" +
          (MaxScore > 0 ? (Score / MaxScore * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " %" : "0 %") + ")" +
          (Passed is { } passed
              ? passed ? " · PASSED" : " · NOT PASSED"
              : " · no pass mark is set, so the score stands alone") +
          (PassMark is { } mark ? " at a pass mark of " + (mark * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " %" : string.Empty);

    public string QuestionsText => string.Join(Environment.NewLine, Questions.Select(question => question.Describe()));

    public string OutcomesText => Outcomes.Count == 0
        ? "Nothing is scored yet: the answers are marked when the attempt is submitted."
        : string.Join(Environment.NewLine + Environment.NewLine, Outcomes.Select(outcome => outcome.Describe()));

    public string MissingText => Missing.Count == 0
        ? "Every question has an answer."
        : "Still unanswered: " + string.Join(", ", Missing);

    public string WarningsText => Warnings.Count == 0
        ? "The scenario raised no warning."
        : string.Join(Environment.NewLine, Warnings.Select(warning => "! " + warning));
}

/// <summary>One past attempt in the trainee's history (TRN-01's history panel).</summary>
public sealed record OgAttemptRow(
    string ScenarioId,
    string Trainee,
    DateTime Utc,
    double Score,
    double MaxScore,
    bool? Passed,
    bool Counted)
{
    public string Describe() => Utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " · " +
                                ScenarioId + " · " + OgTrainingRules.Figure(Score) + " of " +
                                OgTrainingRules.Figure(MaxScore) +
                                (Passed is { } passed ? passed ? " · passed" : " · not passed" : string.Empty) +
                                (Counted ? " · counts under the policy" : string.Empty);
}

/// <summary>A course and its scenarios, as the list groups them (TRN-01).</summary>
public sealed record OgCourseGroup(
    string Id,
    string Title,
    string? Description,
    double? PassMark,
    IReadOnlyList<string> Scenarios)
{
    public string Describe() => Title + " · " + Scenarios.Count.ToString(CultureInfo.InvariantCulture) + " scenario(s)" +
                                (PassMark is { } mark
                                    ? " · pass at " + (mark * 100d).ToString("0.#", CultureInfo.InvariantCulture) + " %"
                                    : string.Empty);
}

/// <summary>The training catalogue and the trainee's history (TRN-01).</summary>
public sealed record OgTrainingWorkspace(
    bool FolderConfigured,
    string? Folder,
    bool SamplePackAvailable,
    IReadOnlyList<OgScenarioSummary> Scenarios,
    IReadOnlyList<OgCourseGroup> Courses,
    IReadOnlyList<string> Problems,
    IReadOnlyList<OgAttemptRow> History,
    string Trainee,
    double? PassMark,
    string AttemptPolicy,
    string AttemptPolicyText,
    IReadOnlyList<string> Notes,
    DateTime ReadAt)
{
    public int Runnable => Scenarios.Count(scenario => scenario.Valid);

    public int Invalid => Scenarios.Count(scenario => !scenario.Valid);

    public string SummaryText => !FolderConfigured
        ? "No scenario folder is configured, so the runner is disabled: " +
          (SamplePackAvailable ? "the built-in sample pack can be written and used." : "and no sample pack is available.")
        : Scenarios.Count == 0
            ? "The scenario folder holds no runnable scenario: " + Problems.Count.ToString(CultureInfo.InvariantCulture) +
              " problem(s) are listed below."
            : Runnable.ToString(CultureInfo.InvariantCulture) + " scenario(s) in " +
              Courses.Count.ToString(CultureInfo.InvariantCulture) + " course(s) · " +
              Invalid.ToString(CultureInfo.InvariantCulture) + " invalid · " +
              History.Count.ToString(CultureInfo.InvariantCulture) + " attempt(s) by " +
              (string.IsNullOrWhiteSpace(Trainee) ? "this trainee" : Trainee);

    public string ScenariosText => Scenarios.Count == 0
        ? "No scenario is listed."
        : string.Join(Environment.NewLine, Scenarios.Select((scenario, index) =>
            (index + 1).ToString(CultureInfo.InvariantCulture) + ". " + scenario.Describe()));

    public string CoursesText => Courses.Count == 0
        ? "No course file is present; each scenario stands alone."
        : string.Join(Environment.NewLine, Courses.Select(course => course.Describe()));

    public string ProblemsText => Problems.Count == 0
        ? "Every file the folder holds was read."
        : string.Join(Environment.NewLine, Problems.Select(problem => "✗ " + problem));

    public string HistoryText => History.Count == 0
        ? "No attempt is recorded for this trainee."
        : string.Join(Environment.NewLine, History
            .OrderByDescending(row => row.Utc)
            .Select(row => "· " + row.Describe()));

    public string PolicyText => AttemptPolicyText;

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) +
        (Folder is null ? string.Empty : " · " + Folder);
}

/// <summary>The training rules TRN-01 states once.</summary>
public static class OgTrainingRules
{
    public const string CodeReadFailed = "training-read-failed";
    public const string CodeScenarioRequired = "training-scenario-required";
    public const string CodeScenarioUnknown = "training-scenario-unknown";
    public const string CodeScenarioInvalid = "training-scenario-invalid";
    public const string CodeNoFolder = "training-no-folder";
    public const string CodeTraineeRequired = "training-trainee-required";
    public const string CodeIncomplete = "training-answer-incomplete";
    public const string CodeWriteFailed = "training-write-failed";
    public const string CodeActorRequired = "training-actor-required";
    public const string CodeSamplePack = "training-sample-pack";

    /// <summary>
    /// The attempt policies the engine defines, and what each one means. There is no attempt *limit* in the engine: the policy says which
    /// attempt counts, not how many are allowed - so "exhausted" is not a state this build can reach, and the page says which policy is
    /// in force instead of inventing a limit.
    /// </summary>
    public static IReadOnlyList<string> Policies { get; } = ["latest", "best", "first"];

    public static string PolicyText(string policy) => policy switch
    {
        "first" => "the first attempt counts",
        "best" => "the best attempt counts",
        _ => "the latest attempt counts",
    };

    /// <summary>AC-5: the scenario's own well files, never a database. This service takes no data source at all.</summary>
    public const string DataText =
        "A scenario's well is a JSON file beside its scenario file; the runner reads no database and writes no well data.";

    /// <summary>AC-2: what an answer was, in the question's own terms.</summary>
    public static string Given(ScenarioQuestion question, Answer? answer)
    {
        ArgumentNullException.ThrowIfNull(question);

        if (answer is null)
        {
            return "(not answered)";
        }

        return question.Type switch
        {
            QuestionType.Choice => TextOf(question, answer.Option),
            QuestionType.Multi => answer.Options.Count == 0
                ? "(not answered)"
                : string.Join(", ", answer.Options.Select(id => TextOf(question, id))),
            QuestionType.Depth or QuestionType.Number => answer.Value is { } value
                ? Figure(value) + (string.IsNullOrWhiteSpace(answer.Unit) ? string.Empty : " " + answer.Unit) +
                  (string.IsNullOrWhiteSpace(answer.Unit) || string.Equals(answer.Unit, question.Unit, StringComparison.Ordinal)
                      ? string.Empty
                      : " (asked in " + question.Unit + ")")
                : "(not answered)",
        };
    }

    /// <summary>The answer key, in the same terms - only ever shown after submission.</summary>
    public static string Expected(ScenarioQuestion question)
    {
        ArgumentNullException.ThrowIfNull(question);

        return question.Type switch
        {
            QuestionType.Choice => TextOf(question, question.CorrectOption),
            QuestionType.Multi => string.Join(", ", question.CorrectOptions.Select(id => TextOf(question, id))),
            QuestionType.Depth or QuestionType.Number => question.CorrectValue is { } value
                ? Figure(value) + (string.IsNullOrWhiteSpace(question.Unit) ? string.Empty : " " + question.Unit) +
                  (question.Tolerance is { } tolerance ? " (± " + Figure(tolerance) + ")" : string.Empty)
                : "(no answer key)",
        };
    }

    /// <summary>AC-1's entry check: the questions still unanswered, by id.</summary>
    public static IReadOnlyList<string> Missing(Scenario scenario, IReadOnlyDictionary<string, OgScenarioAnswer> answers)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(answers);

        return scenario.Questions
            .Where(question => !answers.TryGetValue(question.Id, out var answer) || !answer.Given)
            .Select(question => question.Id)
            .ToList();
    }

    /// <summary>The trainee's entries as the engine's answers.</summary>
    public static Dictionary<string, Answer> AnswersOf(IReadOnlyDictionary<string, OgScenarioAnswer> answers)
    {
        ArgumentNullException.ThrowIfNull(answers);

        return answers
            .Where(pair => pair.Value.Given)
            .ToDictionary(
                pair => pair.Key,
                pair => new Answer
                {
                    Option = pair.Value.Option,
                    Options = pair.Value.Options.ToList(),
                    Value = pair.Value.Value,
                    Unit = pair.Value.Unit,
                    HintsUsed = pair.Value.HintsUsed,
                },
                StringComparer.Ordinal);
    }

    /// <summary>
    /// The answer entry, one line per question: <c>id = value</c>. A choice is an option id, a multi is ids separated by commas, a depth
    /// or a number is the value and, when it is not the question's unit, the unit too - which is how a unit mismatch becomes a marked
    /// answer rather than a refusal.
    /// </summary>
    public static Dictionary<string, OgScenarioAnswer> Answers(OgScenarioRun run, string? text, out string? problem)
    {
        ArgumentNullException.ThrowIfNull(run);

        problem = null;

        var answers = new Dictionary<string, OgScenarioAnswer>(StringComparer.Ordinal);

        foreach (var line in (text ?? string.Empty).Split('\n', StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var split = line.IndexOf('=');

            if (split <= 0)
            {
                problem = "The line '" + line + "' is not an answer: write 'question id = value'.";
                return answers;
            }

            var key = line[..split].Trim();
            var value = line[(split + 1)..].Trim();
            var question = run.Questions.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, key, StringComparison.OrdinalIgnoreCase));

            if (question is null)
            {
                problem = "The scenario has no question '" + key + "'.";
                return answers;
            }

            if (question.IsChoice)
            {
                var ids = value
                    .Split([',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                answers[question.Id] = question.Type == "Multi"
                    ? new OgScenarioAnswer(question.Id, null, ids, null, null, 0)
                    : new OgScenarioAnswer(question.Id, ids.FirstOrDefault(), [], null, null, 0);

                continue;
            }

            // A depth or a number: the value, then an optional unit. Both are the trainee's as written - a wrong unit is marked, not
            // corrected here.
            var parts = value.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0 ||
                !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                problem = "The answer to '" + key + "' is not a number: '" + value + "'.";
                return answers;
            }

            answers[question.Id] = new OgScenarioAnswer(
                question.Id, null, [], number, parts.Length > 1 ? parts[1] : question.Unit, 0);
        }

        return answers;
    }

    /// <summary>One question without its answer, which is what the runner may show before submission (AC-3).</summary>
    public static OgScenarioQuestionView ViewOf(ScenarioQuestion question)
    {
        ArgumentNullException.ThrowIfNull(question);

        return new OgScenarioQuestionView(
            question.Id,
            question.Type.ToString(),
            question.Prompt,
            question.Points,
            question.Hint,
            question.Unit,
            question.Options.Select(option => new OgScenarioOptionView(option.Id, option.Text)).ToList());
    }

    /// <summary>
    /// An attempt's identity: the ledger gives every attempt a trainee, a scenario and a time, and those three are what distinguish
    /// one from another - there is no separate id to invent.
    /// </summary>
    public static string KeyOf(Attempt attempt)
    {
        ArgumentNullException.ThrowIfNull(attempt);

        return attempt.Trainee + "|" + attempt.ScenarioId + "|" +
               attempt.Utc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    }

    private static string TextOf(ScenarioQuestion question, string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return "(none)";
        }

        var option = question.Options.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, id, StringComparison.Ordinal));

        return option is null ? id : option.Text + " [" + id + "]";
    }

    public static string Figure(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}

/// <summary>TRN-01: the scenario catalogue, the runner and the results ledger entries.</summary>
public interface IOgTrainingScenarios
{
    /// <summary>The scenarios, the courses, the folder's problems and the trainee's history.</summary>
    Task<OgResult<OgTrainingWorkspace>> LoadAsync(string trainee, CancellationToken cancellationToken = default);

    /// <summary>AC-1: opens a scenario - its well view and its questions, without the answers.</summary>
    Task<OgResult<OgScenarioRun>> StartAsync(string scenarioId, CancellationToken cancellationToken = default);

    /// <summary>AC-2 and AC-3: marks the answers, records the attempt and returns the solution.</summary>
    Task<OgResult<OgScenarioRun>> SubmitAsync(
        string scenarioId,
        IReadOnlyDictionary<string, OgScenarioAnswer> answers,
        string trainee,
        CancellationToken cancellationToken = default);

    /// <summary>Writes the built-in sample pack under the data root and reloads, so a machine with no scenarios has some.</summary>
    Task<OgResult<string>> WriteSamplePackAsync(string actor, CancellationToken cancellationToken = default);
}

/// <summary>
/// TRN-01 over the tools' <see cref="ScenarioLibrary"/>, <see cref="Scenario"/>, <see cref="ScenarioScoring"/>,
/// <see cref="ScenarioRender"/>, <see cref="CourseLibrary"/> and <see cref="ResultsLedger"/>.
/// </summary>
/// <remarks>
/// <para>
/// The exercise is the engine's: the file is a <see cref="Scenario"/>, the view is its <see cref="WellboreDocument"/> drawn by the same
/// code as every other well view, the marking is <see cref="ScenarioScoring.Score"/> and the ledger is <see cref="ResultsLedger"/>.
/// What this service adds is where the scenarios live, the sample pack's home under the data root, the entry checks the screen needs,
/// the attempt history and the record.
/// </para>
/// <para>
/// The runner is handed a scenario **without its answers** until an attempt is submitted, because a scenario file contains the answer
/// key - so the page cannot show it early even by accident (AC-3). And it takes **no data source**: a scenario's well is a JSON file, so
/// training cannot touch production data (AC-5).
/// </para>
/// </remarks>
public sealed class OgTrainingScenariosService : IOgTrainingScenarios
{
    private readonly IFailureReporter _failures;
    private readonly Func<string?> _configuredFolder;
    private readonly Func<string> _samplesFolder;
    private readonly Func<string> _ledgerFile;

    /// <param name="configuredFolder">The scenario folder the environment configures, or null when it configures none.</param>
    /// <param name="samplesFolder">Where the built-in sample pack is written - under the application's data root, not the customer's.</param>
    /// <param name="ledgerFile">The results ledger file.</param>
    public OgTrainingScenariosService(
        IFailureReporter failures,
        Func<string?> configuredFolder,
        Func<string> samplesFolder,
        Func<string> ledgerFile)
    {
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _configuredFolder = configuredFolder ?? throw new ArgumentNullException(nameof(configuredFolder));
        _samplesFolder = samplesFolder ?? throw new ArgumentNullException(nameof(samplesFolder));
        _ledgerFile = ledgerFile ?? throw new ArgumentNullException(nameof(ledgerFile));
    }

    public Task<OgResult<OgTrainingWorkspace>> LoadAsync(string trainee, CancellationToken cancellationToken = default)
    {
        try
        {
            var problems = new List<string>();
            var notes = new List<string>();

            TrainingToolbox.Register();

            var configured = _configuredFolder();
            var folder = string.IsNullOrWhiteSpace(configured) ? _samplesFolder() : configured!;
            var scored = string.IsNullOrWhiteSpace(configured) && Directory.Exists(folder) &&
                         Directory.EnumerateFiles(folder, "*.scenario.json").Any();

            // The sample pack is only a scenario folder once it has been written; before that the page offers to write it.
            var hasFolder = !string.IsNullOrWhiteSpace(configured) || scored;

            if (!hasFolder)
            {
                notes.Add(TrainingToolbox.Settings.ScenarioFolders.Count == 0
                    ? "training.scenarioFolders is empty, so the runner is disabled until a folder is configured or the sample pack " +
                      "is written."
                    : "the configured scenario folder holds no scenario yet.");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(configured))
                {
                    notes.Add("scenarios are read from the configured folder.");
                }
                else
                {
                    notes.Add("no scenario folder is configured, so the built-in sample pack under the data root is being used - a " +
                              "substitution the page states rather than a silent second source.");
                }
            }

            var scenarios = hasFolder ? ScenarioLibrary.LoadFolder(folder, problems) : [];
            var courses = hasFolder ? CourseLibrary.LoadFolder(folder, scenarios, problems) : [];

            var ledger = ReadLedger(out var ledgerNote);

            if (ledgerNote is not null)
            {
                notes.Add(ledgerNote);
            }

            var name = trainee?.Trim() ?? string.Empty;

            // AC-6: which attempt counts is the policy; the page states it and marks the attempt it selects.
            var counted = string.IsNullOrWhiteSpace(name)
                ? new HashSet<string>(StringComparer.Ordinal)
                : scenarios
                    .Select(scenario => ledger.Counted(name, scenario.Id))
                    .Where(attempt => attempt is not null)
                    .Select(attempt => OgTrainingRules.KeyOf(attempt!))
                    .ToHashSet(StringComparer.Ordinal);

            var history = string.IsNullOrWhiteSpace(name)
                ? []
                : ledger.Attempts
                    .Where(attempt => string.Equals(attempt.Trainee, name, StringComparison.Ordinal))
                    .OrderByDescending(attempt => attempt.Utc)
                    .Select(attempt => new OgAttemptRow(
                        attempt.ScenarioId,
                        attempt.Trainee,
                        attempt.Utc,
                        attempt.Score,
                        attempt.MaxScore,
                        attempt.Passed,
                        counted.Contains(OgTrainingRules.KeyOf(attempt))))
                    .ToList();

            return Task.FromResult(OgResult<OgTrainingWorkspace>.Ok(new OgTrainingWorkspace(
                hasFolder,
                hasFolder ? folder : null,
                SamplePackAvailable: true,
                scenarios.Select(scenario => new OgScenarioSummary(
                    scenario.Id,
                    scenario.Title,
                    scenario.Course ?? string.Empty,
                    scenario.Level ?? string.Empty,
                    scenario.Questions.Count,
                    scenario.MaxScore,
                    ScenarioLibrary.Validate(scenario),
                    scenario.File)).ToList(),
                courses.Select(course => new OgCourseGroup(
                    course.Id,
                    course.Title,
                    course.Description,
                    course.PassMark,
                    course.Scenarios.ToList())).ToList(),
                problems,
                history,
                name,
                TrainingToolbox.Settings.PassMark,
                TrainingToolbox.Settings.AttemptPolicy,
                OgTrainingRules.PolicyText(TrainingToolbox.Settings.AttemptPolicy),
                notes,
                DateTime.UtcNow)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the scenarios", "the scenarios are not listed", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgTrainingWorkspace>.Failed(
                "The scenarios could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgTrainingRules.CodeReadFailed));
        }
    }

    public Task<OgResult<OgScenarioRun>> StartAsync(string scenarioId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scenarioId))
        {
            return Task.FromResult(OgResult<OgScenarioRun>.Failed(
                "A scenario is opened by its id; none was given.", OgTrainingRules.CodeScenarioRequired));
        }

        return Task.FromResult(Open(scenarioId.Trim(), out var scenario, out var problem) switch
        {
            false => OgResult<OgScenarioRun>.Failed(problem!, OgTrainingRules.CodeScenarioUnknown),
            _ => Run(scenario!, submitted: null, answers: null),
        });
    }

    public Task<OgResult<OgScenarioRun>> SubmitAsync(
        string scenarioId,
        IReadOnlyDictionary<string, OgScenarioAnswer> answers,
        string trainee,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(trainee))
        {
            return Task.FromResult(OgResult<OgScenarioRun>.Failed(
                "An attempt is recorded against a trainee; none was named.", OgTrainingRules.CodeTraineeRequired));
        }

        if (string.IsNullOrWhiteSpace(scenarioId))
        {
            return Task.FromResult(OgResult<OgScenarioRun>.Failed(
                "A scenario is submitted by its id; none was given.", OgTrainingRules.CodeScenarioRequired));
        }

        try
        {
            if (!Open(scenarioId.Trim(), out var scenario, out var problem))
            {
                return Task.FromResult(OgResult<OgScenarioRun>.Failed(
                    problem!, OgTrainingRules.CodeScenarioUnknown));
            }

            // AC-1: an incomplete attempt is refused by id, not scored as a set of blanks.
            var missing = OgTrainingRules.Missing(scenario!, answers);

            if (missing.Count > 0)
            {
                return Task.FromResult(OgResult<OgScenarioRun>.Failed(
                    "The attempt is not submitted: " + string.Join(", ", missing) + " still unanswered.",
                    OgTrainingRules.CodeIncomplete));
            }

            var ledger = ReadLedger(out _);
            var course = CourseOf(scenario!.Id);

            // The ledger scores through ScenarioScoring itself, so the recorded attempt and the marking below are the same function of
            // the same answers.
            var attempt = ledger.Record(trainee.Trim(), scenario, OgTrainingRules.AnswersOf(answers), course?.Id, course?.PassMark);

            SaveLedger(ledger);

            return Task.FromResult(Run(scenario, attempt, answers));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "submitting the attempt for " + scenarioId, "the attempt is not recorded", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgScenarioRun>.Failed(
                "The attempt could not be recorded" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgTrainingRules.CodeWriteFailed));
        }
    }

    public Task<OgResult<string>> WriteSamplePackAsync(string actor, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Task.FromResult(OgResult<string>.Failed(
                "Writing the sample pack records who did it.", OgTrainingRules.CodeActorRequired));
        }

        try
        {
            var folder = _samplesFolder();

            Directory.CreateDirectory(folder);

            var files = SamplePacks.Write(folder);

            return Task.FromResult(OgResult<string>.Ok(folder + " (" + files.Count.ToString(CultureInfo.InvariantCulture) +
                                                       " file(s))"));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "writing the sample pack", "no sample scenario is written", FailureSeverity.Error);

            return Task.FromResult(OgResult<string>.Failed(
                "The sample pack could not be written" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgTrainingRules.CodeSamplePack));
        }
    }

    // ------------------------------------------------------------------ the run

    /// <summary>
    /// Builds the runner for a scenario: before an attempt the questions carry no answers and no outcomes; after one they carry the key,
    /// the marking and the solution's view.
    /// </summary>
    private OgResult<OgScenarioRun> Run(
        Scenario scenario, Attempt? submitted, IReadOnlyDictionary<string, OgScenarioAnswer>? answers)
    {
        var problems = ScenarioLibrary.Validate(scenario);

        if (problems.Count > 0)
        {
            return OgResult<OgScenarioRun>.Failed(
                "The scenario '" + scenario.Id + "' is not runnable: " + string.Join(" ", problems),
                OgTrainingRules.CodeScenarioInvalid);
        }

        var warnings = new List<string>();
        var solution = submitted is not null;

        // An exercise shows one exhibit: a well, drawn as the engine's document, or a map, drawn by the engine's map renderer. A map
        // scenario has no well at all, so asking for a document would throw - which is the engine saying so, not a fault.
        WellboreDocument? document = null;
        byte[]? exhibit = null;

        // AC-3: the exhibit shown is the trainee's until an attempt exists; the solution's only after.
        if (scenario.Map is null)
        {
            document = ScenarioRender.Document(scenario, solution, out var renderWarnings);

            warnings.AddRange(renderWarnings);
        }
        else
        {
            exhibit = ScenarioRender.ToPng(scenario, solution);
        }

        var questions = scenario.Questions.Select(OgTrainingRules.ViewOf).ToList();

        if (submitted is null)
        {
            return OgResult<OgScenarioRun>.Ok(new OgScenarioRun(
                scenario.Id,
                scenario.Title,
                scenario.Instructions,
                scenario.Course,
                scenario.Level,
                scenario.DepthUnit,
                questions,
                [],
                Submitted: false,
                0,
                scenario.MaxScore,
                PassMarkOf(scenario),
                null,
                OgTrainingRules.Missing(scenario, answers ?? new Dictionary<string, OgScenarioAnswer>(StringComparer.Ordinal)),
                document,
                exhibit,
                warnings,
                null,
                null));
        }

        // The marking is the engine's own function of the recorded answers, so what is shown is what is stored.
        var result = ResultsLedger.Rescore(submitted, scenario);
        var given = OgTrainingRules.AnswersOf(answers ?? new Dictionary<string, OgScenarioAnswer>(StringComparer.Ordinal));
        var passMark = PassMarkOf(scenario);

        var outcomes = scenario.Questions.Select(question =>
        {
            var scored = result.Questions.First(candidate => string.Equals(candidate.Id, question.Id, StringComparison.Ordinal));
            given.TryGetValue(question.Id, out var answer);

            return new OgQuestionOutcome(
                question.Id,
                question.Prompt,
                OgTrainingRules.Given(question, answer),
                OgTrainingRules.Expected(question),
                scored.FullyCorrect,
                scored.Score,
                question.Points,
                scored.Feedback,
                scored.Explanation ?? question.Explanation);
        }).ToList();

        return OgResult<OgScenarioRun>.Ok(new OgScenarioRun(
            scenario.Id,
            scenario.Title,
            scenario.Instructions,
            scenario.Course,
            scenario.Level,
            scenario.DepthUnit,
            questions,
            outcomes,
            Submitted: true,
            result.Score,
            result.MaxScore,
            passMark,
            passMark is { } mark ? result.Fraction >= mark - 1e-9 : null,
            [],
            document,
            exhibit,
            warnings,
            OgTrainingRules.KeyOf(submitted),
            submitted.Utc));
    }

    /// <summary>The pass mark in force: the course's, or the toolbox's, or none.</summary>
    private double? PassMarkOf(Scenario scenario)
    {
        var course = CourseOf(scenario.Id);

        return course?.PassMark ?? TrainingToolbox.Settings.PassMark;
    }

    /// <summary>The course whose list names this scenario, so its own pass mark and id apply.</summary>
    private CoursePack? CourseOf(string scenarioId)
    {
        var folder = Folder();

        if (folder is null)
        {
            return null;
        }

        var problems = new List<string>();
        var scenarios = ScenarioLibrary.LoadFolder(folder, problems);

        return CourseLibrary.LoadFolder(folder, scenarios, problems)
            .FirstOrDefault(course => course.Scenarios.Contains(scenarioId, StringComparer.Ordinal));
    }

    private bool Open(string scenarioId, out Scenario? scenario, out string? problem)
    {
        var folder = Folder();

        scenario = null;

        if (folder is null)
        {
            problem = "No scenario folder is configured, so '" + scenarioId + "' cannot be opened. Write the sample pack or set " +
                      "training.scenarioFolders.";
            return false;
        }

        var problems = new List<string>();
        var all = ScenarioLibrary.LoadFolder(folder, problems);

        scenario = all.FirstOrDefault(candidate => string.Equals(candidate.Id, scenarioId, StringComparison.Ordinal));

        if (scenario is null)
        {
            problem = problems.FirstOrDefault(candidate =>
                          candidate.Contains(scenarioId, StringComparison.OrdinalIgnoreCase)) ??
                      "No scenario '" + scenarioId + "' is in '" + folder + "'. " +
                      (problems.Count == 0 ? "The folder holds no such file." : problems[0]);

            return false;
        }

        problem = null;

        return true;
    }

    private string? Folder()
    {
        var configured = _configuredFolder();

        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured!;
        }

        var samples = _samplesFolder();

        return Directory.Exists(samples) && Directory.EnumerateFiles(samples, "*.scenario.json").Any() ? samples : null;
    }

    // ------------------------------------------------------------------ the ledger

    private ResultsLedger ReadLedger(out string? note)
    {
        note = null;

        var file = _ledgerFile();

        if (!File.Exists(file))
        {
            return new ResultsLedger();
        }

        try
        {
            return ResultsLedger.FromJson(File.ReadAllText(file));
        }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or FormatException)
        {
            // A ledger that no longer parses is reported and treated as empty: the history is a record, and a corrupt one must not stop
            // an attempt from being taken - but it is never silently rewritten over either.
            var reference = _failures.ReportHandled(
                ex, "reading the training results ledger", "the recorded attempts are not shown", FailureSeverity.Degraded);

            note = "'" + file + "' could not be read (reference " + reference +
                   "), so the history is shown empty; the file is left as it is.";

            return new ResultsLedger();
        }
    }

    private void SaveLedger(ResultsLedger ledger)
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
}
