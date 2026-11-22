using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Training;
using TheTechIdea.OilGas.Training;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// TRN-01 end to end through the service the page drives, against the tools' own sample pack written to a temporary folder: the
/// catalogue and its courses load with no problems, an exercise opens without its answer key, a correct attempt marks every point and
/// is recorded on disk, a partly correct one is marked question by question, a broken file is listed with its problem, and the attempt
/// policy decides which attempt counts (AC-1, AC-2, AC-3, AC-4, AC-6).
/// </summary>
/// <remarks>
/// This feature is file-to-file by design: a scenario's well is a JSON file beside it and the service takes no data source at all
/// (AC-5), so there is no database to open. The switch guards it because it writes under the data root.
/// </remarks>
public sealed class TrainingScenarioGatewayTests
{
    [IntegrationFact]
    public async Task The_sample_pack_runs_end_to_end_and_the_policy_decides_which_attempt_counts()
    {        using var scope = new Scratch();

        var reporter = new Recording();
        var folder = scope.Folder;
        var ledger = Path.Combine(scope.Root, "Training", "results.json");

        var service = new OgTrainingScenariosService(reporter, () => folder, () => folder, () => ledger);

        // ---- the sample pack is written by the service itself, then read back ----
        var written = await service.WriteSamplePackAsync("sara");

        Assert.True(written.Success, written.Message);

        var loaded = await service.LoadAsync("trainee-1");

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var catalogue = loaded.Value!;

        Assert.True(catalogue.FolderConfigured);
        Assert.NotEmpty(catalogue.Scenarios);
        Assert.NotEmpty(catalogue.Courses);
        Assert.Empty(catalogue.Problems);
        Assert.All(catalogue.Scenarios, scenario => Assert.True(scenario.Valid, scenario.ProblemsText));

        // AC-6: the policy in force is stated as the engine defines it.
        Assert.Contains(catalogue.AttemptPolicy, OgTrainingRules.Policies);
        Assert.Contains("attempt counts", catalogue.AttemptPolicyText, StringComparison.Ordinal);

        // ---- AC-1 and AC-3: a well exercise opens without its key, then reveals the solution ----
        var scenario = catalogue.Scenarios.First(candidate =>
            candidate.Valid && Scenario.Load(candidate.File).Map is null);

        var key = Scenario.Load(scenario.File);

        var opened = await service.StartAsync(scenario.Id);

        Assert.True(opened.Success, opened.Message);

        var run = opened.Value!;

        Assert.False(run.Submitted);
        Assert.False(run.SolutionAvailable);
        Assert.Empty(run.Outcomes);
        Assert.NotNull(run.Document);
        Assert.NotEmpty(run.Document!.Tracks);

        // The opened run carries the questions, and the count of those still unanswered is all of them.
        Assert.Equal(key.Questions.Count, run.Questions.Count);
        Assert.Equal(key.Questions.Count, run.Missing.Count);

        // An attempt with a question missing is refused by id, and nothing is written.
        var partialAnswers = CorrectAnswers(key).Take(key.Questions.Count - 1)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        var incomplete = await service.SubmitAsync(scenario.Id, partialAnswers, "trainee-1");

        Assert.False(incomplete.Success);
        Assert.Equal(OgTrainingRules.CodeIncomplete, incomplete.Reference);
        Assert.False(File.Exists(ledger));

        // ---- AC-2: a correct attempt marks every point, and the key is shown with the marking ----
        var correct = await service.SubmitAsync(scenario.Id, CorrectAnswers(key), "trainee-1");

        Assert.True(correct.Success, correct.Message + " :: " + string.Join(" | ", reporter.Failures));

        var marked = correct.Value!;

        Assert.True(marked.Submitted);
        Assert.True(marked.SolutionAvailable);
        Assert.All(marked.Outcomes, outcome => Assert.True(outcome.Correct, outcome.Describe()));
        Assert.Equal(marked.MaxScore, marked.Score, 6);
        Assert.All(marked.Outcomes, outcome => Assert.False(string.IsNullOrWhiteSpace(outcome.Expected)));
        Assert.NotNull(marked.AttemptId);
        Assert.NotNull(marked.SubmittedAt);

        // The solution's view is the engine's, and the ledger is on disk.
        Assert.NotNull(marked.Document);
        Assert.True(File.Exists(ledger));

        // ---- a partly correct attempt is marked question by question ----
        var oneWrong = CorrectAnswers(key);

        var numeric = key.Questions.FirstOrDefault(question =>
            question.Type is QuestionType.Depth or QuestionType.Number);

        if (numeric is not null)
        {
            // A depth or number answered in the wrong unit: marked as a wrong answer with the expected value and unit shown.
            oneWrong[numeric.Id] = new OgScenarioAnswer(numeric.Id, null, [], 1, "ft", 0);
        }
        else
        {
            var multi = key.Questions.First(question => question.Type == QuestionType.Multi);

            oneWrong[multi.Id] = new OgScenarioAnswer(multi.Id, null, [multi.Options[0].Id], null, null, 0);
        }

        var second = await service.SubmitAsync(scenario.Id, oneWrong, "trainee-1");

        Assert.True(second.Success, second.Message + " :: " + string.Join(" | ", reporter.Failures));

        var secondMarked = second.Value!;

        Assert.True(secondMarked.Score < secondMarked.MaxScore);
        Assert.Contains(secondMarked.Outcomes, outcome => !outcome.Correct);
        Assert.All(secondMarked.Outcomes, outcome =>
            Assert.False(string.IsNullOrWhiteSpace(outcome.Expected)));

        // ---- AC-6: the policy decides which attempt counts, and the history marks it ----
        var afterTwo = await service.LoadAsync("trainee-1");

        Assert.True(afterTwo.Success, afterTwo.Message);

        var history = afterTwo.Value!.History
            .Where(row => row.ScenarioId == scenario.Id)
            .OrderBy(row => row.Utc)
            .ToList();

        Assert.Equal(2, history.Count);
        Assert.Single(history.Where(row => row.Counted));

        // The environment's policy is what the engine applies; the test sets each in turn and restores it, which is what an
        // administrator changing training.attemptPolicy would do.
        var original = TrainingToolbox.Settings.AttemptPolicy;

        try
        {
            TrainingToolbox.Settings.AttemptPolicy = "best";

            var best = await service.LoadAsync("trainee-1");
            var countedBest = best.Value!.History.Single(row => row.ScenarioId == scenario.Id && row.Counted);

            Assert.Equal(secondMarked.MaxScore, countedBest.MaxScore, 6);
            Assert.Equal(marked.Score, countedBest.Score, 6);          // the higher of the two, which is the first

            TrainingToolbox.Settings.AttemptPolicy = "first";

            var first = await service.LoadAsync("trainee-1");
            var countedFirst = first.Value!.History.Single(row => row.ScenarioId == scenario.Id && row.Counted);

            Assert.Equal(marked.Score, countedFirst.Score, 6);

            TrainingToolbox.Settings.AttemptPolicy = "latest";

            var latest = await service.LoadAsync("trainee-1");
            var countedLatest = latest.Value!.History.Single(row => row.ScenarioId == scenario.Id && row.Counted);

            Assert.Equal(secondMarked.Score, countedLatest.Score, 6);
        }
        finally
        {
            TrainingToolbox.Settings.AttemptPolicy = original;
        }

        // ---- AC-4: a broken file beside the good ones is listed with its problem, and the rest still load ----
        File.WriteAllText(Path.Combine(folder, "broken.scenario.json"),
            """{ "id": "broken", "title": "Broken", "instructions": "nothing", "questions": [] }""");

        var withBroken = await service.LoadAsync("trainee-1");

        Assert.True(withBroken.Success, withBroken.Message);
        Assert.NotEmpty(withBroken.Value!.Problems);
        Assert.Contains(withBroken.Value.Problems, problem => problem.Contains("no questions", StringComparison.Ordinal));
        Assert.NotEmpty(withBroken.Value.Runnable > 0 ? withBroken.Value.Scenarios : []);

        // ---- AC-3 again: a map exercise is an image, and the solution is still withheld until the attempt ----
        var mapScenario = withBroken.Value.Scenarios.FirstOrDefault(candidate =>
            candidate.Valid && Scenario.Load(candidate.File).Map is not null);

        if (mapScenario is not null)
        {
            var mapRun = await service.StartAsync(mapScenario.Id);

            Assert.True(mapRun.Success, mapRun.Message);
            Assert.True(mapRun.Value!.IsMap);
            Assert.False(mapRun.Value.SolutionAvailable);
            Assert.Null(mapRun.Value.Document);
            Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, mapRun.Value.Exhibit![..4]);
        }

        // ---- a scenario that is not there is refused, not returned as an empty run ----
        var unknown = await service.StartAsync("no-such-scenario");

        Assert.False(unknown.Success);
        Assert.Equal(OgTrainingRules.CodeScenarioUnknown, unknown.Reference);
        Assert.Null(unknown.Value);
    }

    /// <summary>The scenario's own answer key, as the trainee would have to answer to earn every point.</summary>
    private static Dictionary<string, OgScenarioAnswer> CorrectAnswers(Scenario key)
    {
        var answers = new Dictionary<string, OgScenarioAnswer>(StringComparer.Ordinal);

        foreach (var question in key.Questions)
        {
            answers[question.Id] = question.Type switch
            {
                QuestionType.Choice => new OgScenarioAnswer(question.Id, question.CorrectOption, [], null, null, 0),
                QuestionType.Multi => new OgScenarioAnswer(question.Id, null, question.CorrectOptions.ToList(), null, null, 0),
                QuestionType.Depth or QuestionType.Number =>
                    new OgScenarioAnswer(question.Id, null, [], question.CorrectValue, question.Unit, 0),
            };
        }

        return answers;
    }

    private sealed class Scratch : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "trn01-it-" + Guid.NewGuid().ToString("n")[..8]);

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

    private sealed class Recording : IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Failures.Add($"{operation} | {consequence}");
            return FailureReferenceFormat.New();
        }
    }
}
