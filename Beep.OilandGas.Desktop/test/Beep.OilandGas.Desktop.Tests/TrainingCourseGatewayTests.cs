using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Training;
using TheTechIdea.OilGas.Training;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// TRN-02 end to end through the service the page drives, against the tools' own sample pack and a real ledger file: a course's
/// scenarios are recorded in order, the policy decides which attempt counts, "not attempted" is distinct from "failed", a ledger that
/// cannot be written is reported rather than losing the result, and one trainee's row never overwrites another's (AC-1..AC-6).
/// </summary>
/// <remarks>
/// The feature is files and a ledger, not a database, so this opt-in test drives the real service against a temporary folder.
/// </remarks>
public sealed class TrainingCourseGatewayTests
{
    [IntegrationFact]
    public async Task Courses_are_grouped_recorded_and_the_policy_decides_the_recorded_result()
    {        using var scope = new Scratch();

        var reporter = new Recording();
        var ledger = Path.Combine(scope.Root, "Training", "results.json");

        SamplePacks.Write(scope.Folder);

        var service = new OgTrainingCoursesService(reporter, () => scope.Folder, () => ledger);

        // ---- the catalogue: courses group the scenarios, with their pass marks and the policy ----
        var loaded = await service.LoadAsync("trainee-1");

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.NotEmpty(loaded.Value!.Courses);
        Assert.All(loaded.Value.Courses, course => Assert.True(course.ScenarioCount > 0, course.Describe()));
        Assert.Empty(loaded.Value.Problems);

        var course = loaded.Value.Courses.First(candidate => candidate.ScenarioCount > 1);

        Assert.Contains(course.AttemptPolicy, OgTrainingRules.Policies);
        Assert.Contains("attempt counts", course.AttemptPolicyText, StringComparison.Ordinal);

        // The ledger does not exist yet, and the page is told so rather than being left to guess.
        Assert.False(loaded.Value.Ledger.Exists);
        Assert.True(loaded.Value.Ledger.Readable);
        Assert.True(loaded.Value.Ledger.Writable);

        // ---- AC-2: before anything is recorded, the course reads "not attempted" - not a failure ----
        var detail = await service.LoadAsync("trainee-1", course.Id);

        Assert.True(detail.Success, detail.Message);
        Assert.Equal(course.ScenarioCount, detail.Value!.Progress.Count);
        Assert.All(detail.Value.Progress, row => Assert.False(row.Attempted));
        Assert.All(detail.Value.Progress, row => Assert.Contains("not attempted", row.Describe(), StringComparison.Ordinal));

        var before = detail.Value.Results.Single(result => result.CourseId == course.Id);

        Assert.False(before.Attempted);
        Assert.Equal("not attempted", before.Verdict);

        // ---- a course is taken scenario by scenario, in the course's own order ----
        var problems = new List<string>();
        var scenarios = ScenarioLibrary.LoadFolder(scope.Folder, problems);
        var courseFile = CourseLibrary.LoadFolder(scope.Folder, scenarios, problems)
            .First(candidate => candidate.Id == course.Id);

        foreach (var id in courseFile.Scenarios)
        {
            var scenario = scenarios.First(candidate => candidate.Id == id);
            var recorded = await service.RecordAsync(id, CorrectAnswers(scenario), "trainee-1", course.Id);

            Assert.True(recorded.Success, recorded.Message + " :: " + string.Join(" | ", reporter.Failures));
            Assert.True(recorded.Value!.Recorded, recorded.Value.Describe());

            // AC-4: every attempt carries the versions of the files it was scored against.
            Assert.Equal(OgCourseRules.VersionOf(scenario.File), recorded.Value.ScenarioVersion);
            Assert.Equal(OgCourseRules.VersionOf(courseFile.File), recorded.Value.CourseVersion);
        }

        Assert.True(File.Exists(ledger));

        // ---- the course is now complete and passed, and the versions are on the stored attempts ----
        var complete = await service.LoadAsync("trainee-1", course.Id);

        Assert.True(complete.Success, complete.Message);

        var result = complete.Value!.Results.Single(row => row.CourseId == course.Id);

        Assert.True(result.Attempted);
        Assert.True(result.Completed);
        Assert.Equal(course.ScenarioCount, result.Attempts);
        // The verdict is one of the three the page words: a complete course either passed, did not, or has no pass mark to judge by.
        Assert.Contains(result.Verdict,
            new[] { "PASSED", "NOT PASSED", "complete · no pass mark, so the score stands alone" });

        // The ledger on disk holds the version stamps, so a corrected key is visible against the attempt that used the older file.
        var stored = ResultsLedger.FromJson(File.ReadAllText(ledger));

        Assert.Equal(course.ScenarioCount, stored.Attempts.Count);
        Assert.All(stored.Attempts, attempt => Assert.False(string.IsNullOrWhiteSpace(attempt.ScenarioVersion)));
        Assert.All(stored.Attempts, attempt => Assert.Equal(OgCourseRules.VersionOf(courseFile.File), attempt.CourseVersion));

        // ---- AC-1: the policy decides the recorded result ----
        var scenario1 = scenarios.First(candidate => candidate.Id == courseFile.Scenarios[0]);
        var original = TrainingToolbox.Settings.AttemptPolicy;

        try
        {
            // A second, worse attempt of the first scenario.
            var worse = CorrectAnswers(scenario1);

            var wrong = scenario1.Questions.First();

            worse[wrong.Id] = wrong.Type == QuestionType.Choice
                ? new OgScenarioAnswer(wrong.Id, wrong.Options.First(option => option.Id != wrong.CorrectOption).Id, [], null, null, 0)
                : new OgScenarioAnswer(wrong.Id, null, [], (wrong.CorrectValue ?? 0) + 1_000, wrong.Unit, 0);

            var second = await service.RecordAsync(scenario1.Id, worse, "trainee-1", course.Id);

            Assert.True(second.Success, second.Message);
            Assert.True(second.Value!.Score <= complete.Value.Progress.Single(row => row.ScenarioId == scenario1.Id).MaxScore);

            // latest: the worse attempt counts, so the course's score does not rise.
            TrainingToolbox.Settings.AttemptPolicy = "latest";

            var latest = await service.LoadAsync("trainee-1", course.Id);
            var latestProgress = latest.Value!.Progress.Single(row => row.ScenarioId == scenario1.Id);

            Assert.True(latestProgress.Counted);
            Assert.Equal(2, latestProgress.Attempts);
            Assert.Equal(second.Value!.Score, latestProgress.Score, 6);

            // best: the better first attempt counts instead, so the recorded result is not lowered by the later one.
            TrainingToolbox.Settings.AttemptPolicy = "best";

            var best = await service.LoadAsync("trainee-1", course.Id);
            var bestProgress = best.Value!.Progress.Single(row => row.ScenarioId == scenario1.Id);

            Assert.True(bestProgress.Score >= latestProgress.Score - 1e-9);

            // first: the first attempt counts, and the later ones are recorded but not counted.
            TrainingToolbox.Settings.AttemptPolicy = "first";

            var first = await service.LoadAsync("trainee-1", course.Id);
            var firstProgress = first.Value!.Progress.Single(row => row.ScenarioId == scenario1.Id);

            Assert.Equal(2, firstProgress.Attempts);
            Assert.True(firstProgress.Counted);
        }
        finally
        {
            TrainingToolbox.Settings.AttemptPolicy = original;
        }

        // ---- AC-6: another trainee is their own row, and the first trainee's is unchanged ----
        var second2 = await service.RecordAsync(courseFile.Scenarios[0], CorrectAnswers(scenario1), "trainee-2", course.Id);

        Assert.True(second2.Success, second2.Message);

        var both = await service.LoadAsync("trainee-1", course.Id);

        Assert.Equal(2, both.Value!.Cohort.Count);

        // trainee-2 took one scenario; trainee-1 took the whole course and then a second attempt of the first scenario.
        Assert.Equal(1, both.Value.Cohort.Single(row => row.Trainee == "trainee-2").Attempts);
        Assert.Equal(course.ScenarioCount + 1, both.Value.Results.Single(row => row.CourseId == course.Id).Attempts);
        Assert.Equal(course.ScenarioCount + 1, both.Value.Cohort.Single(row => row.Trainee == "trainee-1").Attempts);

        // ---- AC-5: the cohort's pass rate is passed ÷ attempted, and the weakest scenarios are named ----
        Assert.NotNull(both.Value.CohortPassRate);

        var offered = both.Value.Cohort.Sum(row => row.Offered);
        var passed = both.Value.Cohort.Sum(row => row.Passed);

        Assert.Equal(OgCourseRules.PassRate(passed, offered)!.Value, both.Value.CohortPassRate!.Value, 9);

        Assert.NotEmpty(both.Value.Weakest);
        Assert.Equal(both.Value.Weakest.OrderBy(row => row.MeanFraction).First().ScenarioId, both.Value.Weakest[0].ScenarioId);

        // ---- AC-3: a ledger that cannot be written is visible, and the marking survives it ----
        var blockedFolder = Path.Combine(scope.Root, "blocked");

        File.WriteAllText(blockedFolder, "not a folder");

        var blocked = new OgTrainingCoursesService(reporter, () => scope.Folder,
            () => Path.Combine(blockedFolder, "results.json"));

        var unrecorded = await blocked.RecordAsync(courseFile.Scenarios[1], CorrectAnswers(
            scenarios.First(candidate => candidate.Id == courseFile.Scenarios[1])), "trainee-3", course.Id);

        Assert.True(unrecorded.Success, unrecorded.Message);
        Assert.False(unrecorded.Value!.Recorded);
        Assert.NotNull(unrecorded.Value.WriteFailure);
        Assert.Contains("NOT RECORDED", unrecorded.Value.Describe(), StringComparison.Ordinal);
        Assert.Equal(unrecorded.Value.MaxScore, unrecorded.Value.Score, 6);

        // The page is told the ledger cannot be written before an attempt is lost to it.
        var blockedView = await blocked.LoadAsync("trainee-3", course.Id);

        Assert.True(blockedView.Success, blockedView.Message);
        Assert.False(blockedView.Value!.Ledger.Writable);

        // ---- refusals ----
        Assert.Equal(OgCourseRules.CodeTraineeRequired,
            (await service.RecordAsync(course.Id, new Dictionary<string, OgScenarioAnswer>(), " ", course.Id)).Reference);

        var unknownCourse = await service.RecordAsync(courseFile.Scenarios[0],
            CorrectAnswers(scenario1), "trainee-1", "no-such-course");

        Assert.False(unknownCourse.Success);
        Assert.Equal(OgCourseRules.CodeCourseUnknown, unknownCourse.Reference);
        Assert.Null(unknownCourse.Value);
    }

    // ------------------------------------------------------------------ fixtures

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
        private readonly string _root = Path.Combine(Path.GetTempPath(), "trn02-it-" + Guid.NewGuid().ToString("n")[..8]);

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
