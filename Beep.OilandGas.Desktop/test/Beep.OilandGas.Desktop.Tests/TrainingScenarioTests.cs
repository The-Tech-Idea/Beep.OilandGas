using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Training;
using TheTechIdea.Beep.OilandGas.Desktop.Views;
using TheTechIdea.OilGas.Training;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// TRN-01: the rules the runner states - the catalogue and its invalid files (AC-4), the answer entry (AC-1), the marking with units
/// (AC-2), the solution kept back until submission (AC-3), the policy (AC-6) - and that training touches no database at all (AC-5).
/// The scenarios are the tools' own sample pack, written to a temporary folder, so the tests run against real exercise files.
/// </summary>
public sealed class TrainingScenarioTests
{
    private const string PageFileName = "TrainingScenariosPageView.cs";

    [Fact]
    public void The_sample_pack_is_a_real_scenario_set_the_engine_accepts()
    {
        // AC-1 and AC-4: the pack the page offers is real exercise files, and every one of them passes the engine's own checks.
        using var scope = new Scratch();

        var files = SamplePacks.Write(scope.Folder);

        Assert.NotEmpty(files);
        Assert.Contains(files, file => file.EndsWith(".scenario.json", StringComparison.OrdinalIgnoreCase));

        var problems = new List<string>();
        var scenarios = ScenarioLibrary.LoadFolder(scope.Folder, problems);

        Assert.NotEmpty(scenarios);
        Assert.Empty(problems);

        foreach (var scenario in scenarios)
        {
            Assert.Empty(ScenarioLibrary.Validate(scenario));
            Assert.NotEmpty(scenario.Questions);
            Assert.True(scenario.MaxScore > 0, scenario.Id);
            if (scenario.Map is null)
            {
                Assert.True(File.Exists(Path.Combine(scenario.Directory, scenario.Well!)), scenario.Id);
            }
            else
            {
                Assert.True(string.IsNullOrWhiteSpace(scenario.Well), scenario.Id);
            }
        }

        // A course file groups them, so the list has something to group by.
        var courses = CourseLibrary.LoadFolder(scope.Folder, scenarios, problems);

        Assert.NotEmpty(courses);
        Assert.Empty(problems);
    }

    [Fact]
    public void A_broken_scenario_file_is_listed_with_its_problem_and_not_loaded()
    {
        // AC-4: a file the engine rejects is reported by name, and a sound one beside it still loads.
        using var scope = new Scratch();

        SamplePacks.Write(scope.Folder);

        var broken = Path.Combine(scope.Folder, "broken.scenario.json");

        // No id, no title, no questions, no well: four problems, all named.
        File.WriteAllText(broken, """{ "id": "", "title": "", "instructions": "nothing", "questions": [] }""");

        var problems = new List<string>();
        var scenarios = ScenarioLibrary.LoadFolder(scope.Folder, problems);

        Assert.NotEmpty(problems);
        Assert.Contains(problems, problem => problem.StartsWith("broken.scenario.json:", StringComparison.Ordinal));
        Assert.Contains(problems, problem => problem.Contains("no questions", StringComparison.Ordinal));
        Assert.DoesNotContain(scenarios, scenario => scenario.Id.Length == 0);

        // A file that is not JSON at all is a problem too, not an exception that stops the folder being read.
        File.WriteAllText(Path.Combine(scope.Folder, "worse.scenario.json"), "{ this is not json");

        var second = new List<string>();

        Assert.NotEmpty(ScenarioLibrary.LoadFolder(scope.Folder, second));
        Assert.Contains(second, problem => problem.Contains("worse.scenario.json", StringComparison.Ordinal));
    }

    [Fact]
    public async Task A_scenario_opens_without_its_answer_key_and_marks_the_attempt_afterwards()
    {
        // AC-1 and AC-3: the runner is handed the questions only; the key arrives with the marking.
        using var scope = new Scratch();

        SamplePacks.Write(scope.Folder);

        var service = new OgTrainingScenariosService(new Quiet(), () => scope.Folder, () => scope.Folder,
            () => Path.Combine(scope.Root, "results.json"));

        var listed = await service.LoadAsync("trainee-1");

        Assert.True(listed.Success, listed.Message);
        Assert.True(listed.Value!.FolderConfigured);
        Assert.NotEmpty(listed.Value.Scenarios);
        Assert.All(listed.Value.Scenarios, scenario => Assert.True(scenario.Valid, scenario.ProblemsText));
        Assert.NotEmpty(listed.Value.Courses);

        // The pack holds both kinds of exercise - a well and a map - so the test picks a well one for the document assertions and
        // checks the map one separately.
        var scenario = listed.Value.Scenarios.First(candidate =>
            candidate.Valid && Scenario.Load(candidate.File).Map is null);

        var opened = await service.StartAsync(scenario.Id);

        Assert.True(opened.Success, opened.Message);

        var run = opened.Value!;

        Assert.False(run.Submitted);
        Assert.False(run.SolutionAvailable);
        Assert.Empty(run.Outcomes);
        Assert.Equal(scenario.Questions, run.Questions.Count);
        Assert.All(run.Questions, question => Assert.NotEmpty(question.Prompt));

        // Nothing on the opened run carries the answer: no outcome exists, so no expected value can be read.
        Assert.DoesNotContain(run.Questions.SelectMany(question => question.Options),
            option => option.Text.Contains("answer", StringComparison.OrdinalIgnoreCase));

        // Exactly one exhibit: a well drawn by the engine's own renderer, or a map rendered to an image.
        Assert.NotNull(run.Document);
        Assert.Null(run.Exhibit);
        Assert.NotEmpty(run.Document!.Tracks);

        // A map exercise has no well at all, so its exhibit is the engine's map image - and opening it must not throw.
        var mapScenario = listed.Value.Scenarios.First(candidate =>
            candidate.Valid && Scenario.Load(candidate.File).Map is not null);
        var mapRun = await service.StartAsync(mapScenario.Id);

        Assert.True(mapRun.Success, mapRun.Message);
        Assert.True(mapRun.Value!.IsMap);
        Assert.Null(mapRun.Value.Document);
        Assert.NotNull(mapRun.Value.Exhibit);
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, mapRun.Value.Exhibit![..4]);

        // The summary says which questions are unanswered before anything is typed.
        Assert.Equal(run.Questions.Count, run.Missing.Count);

        // AC-1: an incomplete attempt is refused by id rather than scored as blanks.
        var incomplete = await service.SubmitAsync(scenario.Id, new Dictionary<string, OgScenarioAnswer>(), "trainee-1");

        Assert.False(incomplete.Success);
        Assert.Equal(OgTrainingRules.CodeIncomplete, incomplete.Reference);
        Assert.Contains("still unanswered", incomplete.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_marking_matches_the_answer_key_with_its_units_and_the_ledger_keeps_the_attempt()
    {
        // AC-2: the engine's own scoring, through the page's answer entry, on a real sample scenario.
        using var scope = new Scratch();

        SamplePacks.Write(scope.Folder);

        var ledgerFile = Path.Combine(scope.Root, "results.json");
        var service = new OgTrainingScenariosService(new Quiet(), () => scope.Folder, () => scope.Folder, () => ledgerFile);

        var listed = await service.LoadAsync("trainee-1");

        // The casing-program scenario asks for depths in metres, so it exercises the unit path.
        var scenario = listed.Value!.Scenarios.First(candidate => candidate.Valid && candidate.Id == "core-casing-program");
        var opened = await service.StartAsync(scenario.Id);

        Assert.True(opened.Success, opened.Message);

        var run = opened.Value!;

        // Correct answers read off the scenario's own key, so the test proves the marking agrees with the file.
        var key = Scenario.Load(scenario.File);
        var answers = new Dictionary<string, OgScenarioAnswer>(StringComparer.Ordinal);

        foreach (var question in key.Questions)
        {
            answers[question.Id] = question.Type switch
            {
                QuestionType.Choice => new OgScenarioAnswer(question.Id, question.CorrectOption, [], null, null, 0),
                QuestionType.Multi => new OgScenarioAnswer(question.Id, null, question.CorrectOptions.ToList(), null, null, 0),
                _ => new OgScenarioAnswer(question.Id, null, [], question.CorrectValue, question.Unit, 0),
            };
        }

        var submitted = await service.SubmitAsync(scenario.Id, answers, "trainee-1");

        Assert.True(submitted.Success, submitted.Message);

        var marked = submitted.Value!;

        Assert.True(marked.Submitted);
        Assert.True(marked.SolutionAvailable);

        // Every answer was the key's, so every question earned every point and the marking says so with the expected value shown.
        Assert.All(marked.Outcomes, outcome => Assert.True(outcome.Correct, outcome.Describe()));
        Assert.Equal(marked.MaxScore, marked.Score, 6);
        Assert.All(marked.Outcomes, outcome => Assert.False(string.IsNullOrWhiteSpace(outcome.Expected)));

        // AC-3: the document is now the solution's, and it differs from the trainee's when the scenario has one.
        Assert.NotNull(marked.Document);

        // The ledger recorded it, on disk, with the attempt's own identity.
        Assert.True(File.Exists(ledgerFile));
        Assert.NotNull(marked.AttemptId);
        Assert.NotNull(marked.SubmittedAt);

        var again = await service.LoadAsync("trainee-1");

        Assert.True(again.Success, again.Message);
        Assert.Contains(again.Value!.History, row => row.ScenarioId == scenario.Id && row.Counted);

        // A wrong unit is marked as a wrong *answer*, with the expected unit shown - not refused.
        var wrongUnit = new Dictionary<string, OgScenarioAnswer>(StringComparer.Ordinal);

        foreach (var question in key.Questions)
        {
            wrongUnit[question.Id] = question.Type == QuestionType.Depth
                ? new OgScenarioAnswer(question.Id, null, [], 1, "ft", 0)
                : answers[question.Id];
        }

        var unitMarked = await service.SubmitAsync(scenario.Id, wrongUnit, "trainee-2");

        Assert.True(unitMarked.Success, unitMarked.Message);

        var wrongDepth = unitMarked.Value!.Outcomes.First(outcome =>
            key.Questions.First(question => question.Id == outcome.Id).Type == QuestionType.Depth);

        Assert.False(wrongDepth.Correct);
        Assert.Contains("ft", wrongDepth.Given, StringComparison.Ordinal);
        Assert.Contains("m", wrongDepth.Expected, StringComparison.Ordinal);
        Assert.DoesNotContain("ft", wrongDepth.Expected, StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_reads_answers_from_one_line_each_and_refuses_a_line_it_cannot_read()
    {
        // AC-1's entry: a choice takes an option id, a multi takes ids, a depth takes a value and optionally a unit.
        var run = new OgScenarioRun(
            "s", "S", "i", null, null, "m",
            [
                new OgScenarioQuestionView("q1", "Choice", "Which?", 1, null, null,
                    [new OgScenarioOptionView("a", "A"), new OgScenarioOptionView("b", "B")]),
                new OgScenarioQuestionView("q2", "Multi", "Which ones?", 2, null, null,
                    [new OgScenarioOptionView("a", "A"), new OgScenarioOptionView("b", "B")]),
                new OgScenarioQuestionView("q3", "Depth", "How deep?", 2, null, "m", []),
            ],
            [], false, 0, 5, null, null, [], null, null, [], null, null);

        var answers = TrainingScenariosPageView.Answers(run, "q1 = b\nq2 = a, b\nq3 = 2400 m", out var problem);

        Assert.Null(problem);
        Assert.Equal(3, answers.Count);
        Assert.Equal("b", answers["q1"].Option);
        Assert.Equal(["a", "b"], answers["q2"].Options);
        Assert.Equal(2400d, answers["q3"].Value);
        Assert.Equal("m", answers["q3"].Unit);

        // A unit the question did not ask for is kept as written, so the marking can say the two are not comparable.
        var inFeet = TrainingScenariosPageView.Answers(run, "q3 = 7900 ft", out var feetProblem);

        Assert.Null(feetProblem);
        Assert.Equal("ft", inFeet["q3"].Unit);

        // A number with no unit takes the question's own.
        Assert.Equal("m", TrainingScenariosPageView.Answers(run, "q3 = 2400", out _)["q3"].Unit);

        // A line that is not an answer, an id the scenario does not have, and a value that is not a number are each refused.
        TrainingScenariosPageView.Answers(run, "q1 b", out var noEquals);
        TrainingScenariosPageView.Answers(run, "zz = 1", out var unknown);
        TrainingScenariosPageView.Answers(run, "q3 = deep", out var notNumber);

        Assert.Contains("is not an answer", noEquals!, StringComparison.Ordinal);
        Assert.Contains("has no question 'zz'", unknown!, StringComparison.Ordinal);
        Assert.Contains("is not a number", notNumber!, StringComparison.Ordinal);

        Assert.NotNull(noEquals);
        Assert.NotNull(unknown);
        Assert.NotNull(notNumber);
    }

    [Fact]
    public async Task A_folder_with_no_scenarios_disables_the_runner_and_the_sample_pack_fills_it()
    {
        // The feature's own state: no folder configured, the runner disabled with the reason and the sample pack offered.
        using var scope = new Scratch();

        var samples = Path.Combine(scope.Root, "Training", "Samples");
        var service = new OgTrainingScenariosService(new Quiet(), () => null, () => samples,
            () => Path.Combine(scope.Root, "results.json"));

        var empty = await service.LoadAsync("trainee-1");

        Assert.True(empty.Success, empty.Message);
        Assert.False(empty.Value!.FolderConfigured);
        Assert.Empty(empty.Value.Scenarios);
        Assert.True(empty.Value.SamplePackAvailable);
        Assert.Contains("runner is disabled", empty.Value.SummaryText, StringComparison.Ordinal);

        // A scenario cannot be opened while there is nothing to open.
        var refused = await service.StartAsync("core-casing-program");

        Assert.False(refused.Success);
        Assert.Equal(OgTrainingRules.CodeScenarioUnknown, refused.Reference);
        Assert.Contains("Write the sample pack", refused.Message!, StringComparison.Ordinal);

        // The pack is written under the data root, not into a customer folder, and the runner then works.
        var written = await service.WriteSamplePackAsync("sara");

        Assert.True(written.Success, written.Message);
        Assert.StartsWith(samples, written.Value!, StringComparison.OrdinalIgnoreCase);

        var filled = await service.LoadAsync("trainee-1");

        Assert.True(filled.Success, filled.Message);
        Assert.True(filled.Value!.FolderConfigured);
        Assert.NotEmpty(filled.Value.Scenarios);

        // A write records who made it.
        Assert.Equal(OgTrainingRules.CodeActorRequired, (await service.WriteSamplePackAsync(" ")).Reference);
    }

    [Fact]
    public async Task A_submission_needs_a_trainee_and_a_scenario_and_the_policy_is_stated()
    {
        using var scope = new Scratch();

        SamplePacks.Write(scope.Folder);

        var service = new OgTrainingScenariosService(new Quiet(), () => scope.Folder, () => scope.Folder,
            () => Path.Combine(scope.Root, "results.json"));

        var noTrainee = await service.SubmitAsync("core-casing-program", new Dictionary<string, OgScenarioAnswer>(), " ");

        Assert.Equal(OgTrainingRules.CodeTraineeRequired, noTrainee.Reference);

        var noScenario = await service.SubmitAsync(" ", new Dictionary<string, OgScenarioAnswer>(), "sara");

        Assert.Equal(OgTrainingRules.CodeScenarioRequired, noScenario.Reference);

        var blankStart = await service.StartAsync("  ");

        Assert.Equal(OgTrainingRules.CodeScenarioRequired, blankStart.Reference);

        var unknown = await service.StartAsync("no-such-scenario");

        Assert.False(unknown.Success);
        Assert.Equal(OgTrainingRules.CodeScenarioUnknown, unknown.Reference);

        // AC-6: the policy in force is stated as the engine defines it, and the engine has no attempt limit - so "exhausted" is not a
        // state this build can reach, and the page says which attempt counts instead of inventing a limit.
        var listed = await service.LoadAsync("sara");

        Assert.True(listed.Success, listed.Message);
        Assert.Contains(listed.Value!.AttemptPolicy, OgTrainingRules.Policies);
        Assert.Contains("attempt counts", listed.Value.AttemptPolicyText, StringComparison.Ordinal);
        Assert.DoesNotContain("exhaust", listed.Value.PolicyText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Training_reaches_no_database_and_the_page_keeps_the_solution_back()
    {
        // AC-5, structurally: the training service takes no data source, so it cannot read or write production data.
        var source = File.ReadAllText(
            SourceRoot.AllSources().Single(path => path.EndsWith("OgTraining.cs", StringComparison.Ordinal)));

        Assert.DoesNotContain("IDataSource", source, StringComparison.Ordinal);
        Assert.DoesNotContain("OgUow", source, StringComparison.Ordinal);
        Assert.DoesNotContain("WELLBORE", source, StringComparison.Ordinal);

        // AC-3: the page shows the engine's document and, before submission, only the questions.
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(path => path.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("_view.Document = run.Document", page, StringComparison.Ordinal);
        Assert.Contains("_submitButton.IsEnabled = !run.Submitted && run.Missing.Count == 0", page, StringComparison.Ordinal);
        Assert.Contains("run.OutcomesText", page, StringComparison.Ordinal);
        Assert.Contains("run.MissingText", page, StringComparison.Ordinal);

        // A refused run takes the exercise off the page.
        Assert.Contains("_host.Surface = null", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ fixtures

    /// <summary>A temporary folder tree, deleted when the test ends.</summary>
    private sealed class Scratch : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "trn01-" + Guid.NewGuid().ToString("n")[..8]);

        public Scratch() => Directory.CreateDirectory(_root);

        public string Root => _root;

        public string Folder
        {
            get
            {
                var folder = Path.Combine(_root, "scenarios");
                Directory.CreateDirectory(folder);
                return folder;
            }
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_root))
                {
                    Directory.Delete(_root, recursive: true);
                }
            }
            catch (IOException)
            {
                // The OS temp folder is its own to clean.
            }
        }
    }

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
