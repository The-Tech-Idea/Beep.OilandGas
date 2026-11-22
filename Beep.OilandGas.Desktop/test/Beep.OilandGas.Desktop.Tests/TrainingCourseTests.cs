using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Training;
using TheTechIdea.OilGas.Training;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// TRN-02: the course rules - the version a file is stamped with (AC-4), the pass rate as passed ÷ attempted (AC-5), "not attempted"
/// worded apart from "failed" (AC-2) - and the recording path against the tools' own sample pack, including a ledger that cannot be
/// written (AC-3).
/// </summary>
public sealed class TrainingCourseTests
{
    private const string PageFileName = "TrainingCoursesPageView.cs";

    [Fact]
    public void A_file_is_versioned_by_its_own_content_so_a_change_is_a_new_version()
    {
        // AC-4: a scenario and a course are files with no version field, so the stamp is a fingerprint of the content - which is what
        // makes a corrected answer key visible against the attempt that used the older one.
        using var scope = new Scratch();

        var file = scope.File("a.scenario.json");

        File.WriteAllText(file, """{ "id": "a", "title": "A" }""");

        var first = OgCourseRules.VersionOf(file);

        Assert.NotNull(first);
        Assert.Equal(OgCourseRules.VersionLength, first!.Length);
        Assert.Matches("^[0-9a-f]+$", first);

        // The same content is the same version, which is what makes it a version rather than a timestamp.
        Assert.Equal(first, OgCourseRules.VersionOf(file));

        File.WriteAllText(file, """{ "id": "a", "title": "A (corrected)" }""");

        var second = OgCourseRules.VersionOf(file);

        Assert.NotEqual(first, second);

        // A file that is not there has no version, rather than an empty string that reads like one.
        Assert.Null(OgCourseRules.VersionOf(Path.Combine(scope.Root, "absent.json")));
        Assert.Null(OgCourseRules.VersionOf(" "));
    }

    [Fact]
    public void The_pass_rate_is_passed_over_attempted_and_unknown_when_nothing_was_attempted()
    {
        // AC-5: the cohort's rate is exactly passed ÷ attempted, and an empty cohort is unknown rather than zero - a zero would read as
        // "everybody failed".
        Assert.Equal(0.5, OgCourseRules.PassRate(1, 2)!.Value, 9);
        Assert.Equal(1d, OgCourseRules.PassRate(3, 3)!.Value, 9);
        Assert.Equal(0d, OgCourseRules.PassRate(0, 4)!.Value, 9);
        Assert.Null(OgCourseRules.PassRate(0, 0));
        Assert.Null(OgCourseRules.PassRate(2, 0));
    }

    [Fact]
    public void Not_attempted_is_worded_apart_from_failed()
    {
        // AC-2: the engine reports an unattempted course as a zero score, so the page has to say which zero it is.
        var notAttempted = Result(attempted: false, completed: false, passed: null, score: 0, max: 10);
        var failed = Result(attempted: true, completed: true, passed: false, score: 4, max: 10);
        var passed = Result(attempted: true, completed: true, passed: true, score: 9, max: 10);

        Assert.Equal("not attempted", notAttempted.Verdict);
        Assert.Equal("NOT PASSED", failed.Verdict);
        Assert.Equal("PASSED", passed.Verdict);

        // A course under way is neither: it says how far it has got.
        var underway = Result(attempted: true, completed: false, passed: null, score: 4, max: 10);

        Assert.StartsWith("in progress", underway.Verdict, StringComparison.Ordinal);
        Assert.Contains("4 of 10", underway.Verdict, StringComparison.Ordinal);
    }

    [Fact]
    public void An_attempt_is_stamped_with_both_versions_and_a_failed_write_is_not_silent()
    {
        using var scope = new Scratch();

        SamplePacks.Write(scope.Folder);

        // The ledger's folder is a file, so neither creating the folder nor writing the ledger can succeed - the state AC-3 asks to
        // be visible. A directory at the ledger's own path was tried first and the write did not fail there, so the test uses the
        // folder rather than relying on that.
        var blockedFolder = Path.Combine(scope.Root, "blocked");

        File.WriteAllText(blockedFolder, "not a folder");

        var blocked = Path.Combine(blockedFolder, "results.json");

        var service = new OgTrainingCoursesService(new Quiet(), () => scope.Folder, () => blocked);

        var problems = new List<string>();
        var scenarios = ScenarioLibrary.LoadFolder(scope.Folder, problems);
        var courses = CourseLibrary.LoadFolder(scope.Folder, scenarios, problems);
        var course = courses.First(candidate => candidate.Scenarios.Count > 1);
        var scenario = scenarios.First(candidate => candidate.Id == course.Scenarios[0]);

        var recorded = service.RecordAsync(scenario.Id, CorrectAnswers(scenario), "trainee-1", course.Id)
            .GetAwaiter().GetResult();

        Assert.True(recorded.Success, recorded.Message);

        // AC-3: the marking is still there, the record is not - and it says so rather than pretending.
        Assert.False(recorded.Value!.Recorded);
        Assert.NotNull(recorded.Value.WriteFailure);
        Assert.Equal(recorded.Value.MaxScore, recorded.Value.Score, 6);
        Assert.Contains("NOT RECORDED", recorded.Value.Describe(), StringComparison.Ordinal);

        // AC-4: and the versions are stamped from the files themselves, whatever happened to the write.
        Assert.Equal(OgCourseRules.VersionOf(scenario.File), recorded.Value.ScenarioVersion);
        Assert.Equal(OgCourseRules.VersionOf(course.File), recorded.Value.CourseVersion);

        // The page is told the ledger cannot be written before an attempt is lost to it.
        var loaded = service.LoadAsync("trainee-1", course.Id).GetAwaiter().GetResult();

        Assert.True(loaded.Success, loaded.Message);
        Assert.False(loaded.Value!.Ledger.Writable);
        Assert.Contains("NOT WRITABLE", loaded.Value.LedgerText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_recorded_attempt_reaches_the_course_result_the_ledger_and_the_cohort()
    {
        // AC-1 and AC-6: the result comes from the ledger through the engine's own course function, per trainee.
        using var scope = new Scratch();

        SamplePacks.Write(scope.Folder);

        var ledger = Path.Combine(scope.Root, "results.json");
        var service = new OgTrainingCoursesService(new Quiet(), () => scope.Folder, () => ledger);

        var problems = new List<string>();
        var scenarios = ScenarioLibrary.LoadFolder(scope.Folder, problems);
        var courses = CourseLibrary.LoadFolder(scope.Folder, scenarios, problems);
        var course = courses.First(candidate => candidate.Scenarios.Count > 1);
        var scenario = scenarios.First(candidate => candidate.Id == course.Scenarios[0]);

        var first = await service.RecordAsync(scenario.Id, CorrectAnswers(scenario), "trainee-1", course.Id);

        Assert.True(first.Success, first.Message);
        Assert.True(first.Value!.Recorded);
        Assert.True(File.Exists(ledger));

        // The trainee's result exists and is in progress, because the course has more than one scenario.
        var loaded = await service.LoadAsync("trainee-1", course.Id);

        Assert.True(loaded.Success, loaded.Message);

        var result = loaded.Value!.Results.Single(row => row.CourseId == course.Id);

        Assert.True(result.Attempted);
        Assert.False(result.Completed);
        Assert.StartsWith("in progress", result.Verdict, StringComparison.Ordinal);
        Assert.Equal(1, result.Attempts);
        Assert.True(result.Best > 0d);

        // The course's own progress names the scenario as attempted and counts it under the policy.
        var progress = loaded.Value.Progress.Single(row => row.ScenarioId == scenario.Id);

        Assert.True(progress.Attempted);
        Assert.True(progress.Counted, progress.Describe());
        Assert.Equal(1, progress.Attempts);

        // AC-6: another trainee has their own row, and the first trainee's is untouched.
        var second = await service.RecordAsync(scenario.Id, CorrectAnswers(scenario), "trainee-2", course.Id);

        Assert.True(second.Success, second.Message);

        var both = await service.LoadAsync("trainee-1", course.Id);

        Assert.Equal(2, both.Value!.Cohort.Count);
        Assert.Equal(1, both.Value.Results.Single(row => row.CourseId == course.Id).Attempts);
        Assert.Equal(["trainee-1", "trainee-2"], both.Value.Cohort.Select(row => row.Trainee));

        // AC-5: the cohort's rate is passed ÷ attempted over the courses offered.
        Assert.NotNull(both.Value.CohortPassRate);
        Assert.Equal(OgCourseRules.PassRate(both.Value.Cohort.Sum(row => row.Passed), both.Value.Cohort.Sum(row => row.Offered))!.Value,
            both.Value.CohortPassRate!.Value, 9);

        // The weakest scenarios are named from the recorded attempts.
        Assert.NotEmpty(both.Value.Weakest);
        Assert.All(both.Value.Weakest, row => Assert.True(row.Attempts > 0));

        // A scenario that the course does not contain is refused rather than recorded against it.
        var foreign = scenarios.First(candidate => !course.Scenarios.Contains(candidate.Id));

        var refused = await service.RecordAsync(foreign.Id, CorrectAnswers(foreign), "trainee-1", course.Id);

        Assert.False(refused.Success);
        Assert.Equal(OgCourseRules.CodeScenarioNotInCourse, refused.Reference);

        // And an incomplete attempt is refused by id, writing nothing.
        var before = File.ReadAllText(ledger);
        var incomplete = await service.RecordAsync(scenario.Id, new Dictionary<string, OgScenarioAnswer>(), "trainee-1", course.Id);

        Assert.False(incomplete.Success);
        Assert.Equal(OgCourseRules.CodeIncomplete, incomplete.Reference);
        Assert.Equal(before, File.ReadAllText(ledger));
    }

    [Fact]
    public void The_page_states_the_policy_the_ledger_and_the_three_verdicts()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-1 and AC-6: the policy and the ledger's state are on the page, not implied.
        Assert.Contains("loaded.Value.Ledger.Writable", page, StringComparison.Ordinal);
        Assert.Contains("NOT RECORDED", page, StringComparison.Ordinal);
        Assert.Contains("recorded.Value.Describe()", page, StringComparison.Ordinal);
        Assert.Contains("OgCourseRules.PassRate", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ fixtures

    private static OgCourseResult Result(bool attempted, bool completed, bool? passed, double score, double max) =>
        new("course", "Course", "trainee", attempted ? 1 : 0, 0.4, 0.4, score, max, attempted, completed, 0.7, passed,
            attempted ? DateTime.UtcNow : null);

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
        private readonly string _root = Path.Combine(Path.GetTempPath(), "trn02-" + Guid.NewGuid().ToString("n")[..8]);

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

        public string File(string name) => Path.Combine(_root, name);

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
