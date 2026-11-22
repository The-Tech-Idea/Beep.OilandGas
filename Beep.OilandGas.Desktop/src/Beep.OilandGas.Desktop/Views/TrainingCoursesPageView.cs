using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Training;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// TRN-02: the courses a scenario belongs to, the trainee's progress through them, the results ledger and the cohort an instructor
/// reads.
/// </summary>
/// <remarks>
/// The grouping, the counted attempt and the course verdict are the engine's: <c>ResultsLedger.Counted</c> applies the attempt policy
/// and <c>ResultsLedger.Course</c> produces a trainee's result, so the policy changes the recorded result because the engine counts a
/// different attempt - not because this page re-implements the rule (AC-1). "Not attempted" is a case the page words differently from
/// "failed", because the engine reports both as a zero score (AC-2). Recording stamps the attempt with the scenario and course versions
/// (AC-4) and, when the ledger cannot be written, still shows the marking and marks it not recorded (AC-3). The cohort's pass rate is
/// passed ÷ attempted (AC-5), and every result is looked up by trainee, so one trainee never overwrites another's row (AC-6).
/// </remarks>
public sealed class TrainingCoursesPageView : OgPageBase
{
    public const string Route = "courses";

    private readonly OgApplication _application;
    private readonly IOgTrainingCourses _courses;
    private readonly IOgTrainingScenarios _scenarios;

    private readonly TextBlock _header;
    private readonly TextBlock _summary;
    private readonly TextBlock _progress;
    private readonly TextBlock _results;
    private readonly TextBlock _cohort;
    private readonly TextBlock _weakest;
    private readonly TextBlock _ledger;
    private readonly TextBlock _problems;
    private readonly TextBlock _notes;
    private readonly TextBlock _recording;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _trainee;
    private readonly TextBox _answers;
    private readonly DataGrid _courseGrid;
    private readonly DataGrid _resultGrid;
    private readonly DataGrid _cohortGrid;
    private readonly DataGrid _weakGrid;
    private readonly Button _loadButton;
    private readonly Button _recordButton;

    private OgCoursesWorkspace? _workspace;
    private OgScenarioRun? _openScenario;
    private bool _loading;

    public TrainingCoursesPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _courses = application.TrainingCourses;
        _scenarios = application.TrainingScenarios;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.FontWeight = FontWeights.SemiBold;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _progress = Body(string.Empty);
        _progress.TextWrapping = TextWrapping.Wrap;
        _progress.Margin = new Thickness(0, 0, 0, 4);

        _results = Body(string.Empty);
        _results.TextWrapping = TextWrapping.Wrap;
        _results.Margin = new Thickness(0, 0, 0, 4);

        _cohort = Body(string.Empty);
        _cohort.TextWrapping = TextWrapping.Wrap;
        _cohort.Margin = new Thickness(0, 0, 0, 4);

        _weakest = Body(string.Empty);
        _weakest.TextWrapping = TextWrapping.Wrap;
        _weakest.Margin = new Thickness(0, 0, 0, 4);

        _ledger = Body(string.Empty);
        _ledger.TextWrapping = TextWrapping.Wrap;
        _ledger.Margin = new Thickness(0, 0, 0, 4);

        _problems = Body(string.Empty);
        _problems.TextWrapping = TextWrapping.Wrap;
        _problems.Margin = new Thickness(0, 0, 0, 4);

        _notes = Body(string.Empty);
        _notes.TextWrapping = TextWrapping.Wrap;
        _notes.Margin = new Thickness(0, 0, 0, 4);

        _recording = Body(string.Empty);
        _recording.TextWrapping = TextWrapping.Wrap;
        _recording.FontWeight = FontWeights.SemiBold;
        _recording.Margin = new Thickness(0, 4, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _trainee = Field("Trainee", 180);
        _trainee.Text = application.Session.UserId;

        _answers = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 70,
            MaxHeight = 130,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "One answer per line: question id = value - the same entry the scenarios page uses",
        };

        Named(_answers, "Answers");

        _courseGrid = Grid("Courses", ("Id", "Id", 150), ("Title", "Title", 220), ("Scenarios", "ScenarioCount", 80),
            ("Pass mark", "PassMark", 90), ("Policy", "AttemptPolicy", 90));

        _courseGrid.SelectionChanged += (_, _) => ShowCourse();

        _resultGrid = Grid("Results", ("Course", "CourseTitle", 200), ("Trainee", "Trainee", 130),
            ("Attempts", "Attempts", 80), ("Best", "Best", 80), ("Latest", "Latest", 80),
            ("Score", "Score", 80), ("Max", "MaxScore", 80), ("Verdict", "Verdict", 200));

        _cohortGrid = Grid("Cohort", ("Trainee", "Trainee", 160), ("Courses", "Courses", 80),
            ("Offered", "Offered", 80), ("Passed", "Passed", 80), ("Pass rate", "PassRate", 90),
            ("Attempts", "Attempts", 90));

        _weakGrid = Grid("Weakest", ("Scenario", "ScenarioId", 170), ("Title", "Title", 200),
            ("Attempts", "Attempts", 80), ("Fully correct", "FullyCorrect", 100),
            ("Mean", "MeanFraction", 90));

        _loadButton = Button("Load", (_, _) => Load());
        _recordButton = Button("Record the attempt", (_, _) => _ = RecordAsync());

        var traineeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        traineeBar.Children.Add(Caption("Trainee"));
        traineeBar.Children.Add(_trainee);
        traineeBar.Children.Add(_loadButton);

        var recordBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        recordBar.Children.Add(Caption("Answers for the selected course's first unanswered scenario"));
        recordBar.Children.Add(_recordButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(traineeBar);
        panel.Children.Add(_summary);
        panel.Children.Add(Caption("Courses"));
        panel.Children.Add(_courseGrid);
        panel.Children.Add(_progress);
        panel.Children.Add(Caption("Your results"));
        panel.Children.Add(_resultGrid);
        panel.Children.Add(_results);
        panel.Children.Add(Caption("Cohort"));
        panel.Children.Add(_cohortGrid);
        panel.Children.Add(_cohort);
        panel.Children.Add(Caption("Weakest scenarios"));
        panel.Children.Add(_weakGrid);
        panel.Children.Add(_weakest);
        panel.Children.Add(Caption("The ledger"));
        panel.Children.Add(_ledger);
        panel.Children.Add(Caption("Problems with the folder's files"));
        panel.Children.Add(_problems);
        panel.Children.Add(Caption("Record an attempt"));
        panel.Children.Add(_answers);
        panel.Children.Add(recordBar);
        panel.Children.Add(_recording);
        panel.Children.Add(Caption("Notes"));
        panel.Children.Add(_notes);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Name the trainee and load the courses.");
    }

    public OgCoursesWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string SummaryText => _summary.Text;

    public string ProgressText => _progress.Text;

    public string ResultsText => _results.Text;

    public string CohortText => _cohort.Text;

    public string WeakestText => _weakest.Text;

    public string LedgerText => _ledger.Text;

    public string ProblemsText => _problems.Text;

    public string RecordingText => _recording.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public OgCourseRow? ChosenCourse => _courseGrid.SelectedItem as OgCourseRow;

    public bool Load()
    {
        _header.Text = "Training · courses";

        var chosen = ChosenCourse?.Id;
        var loaded = _courses.LoadAsync(_trainee.Text?.Trim() ?? string.Empty, chosen).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            Clear();
            _failure.Text = loaded.Message ?? "The courses could not be read.";
            ShowState(OgViewState.Failed, "The courses are not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        _workspace = loaded.Value;

        _summary.Text = loaded.Value.SummaryText;
        _progress.Text = loaded.Value.ProgressText;
        _results.Text = loaded.Value.ResultsText;
        _cohort.Text = loaded.Value.CohortText;
        _weakest.Text = loaded.Value.WeakestText;
        _ledger.Text = loaded.Value.LedgerText;
        _problems.Text = loaded.Value.ProblemsText;
        _notes.Text = loaded.Value.NotesText;
        _refreshed.Text = loaded.Value.ReadText;

        _courseGrid.ItemsSource = loaded.Value.Courses.ToList();
        _resultGrid.ItemsSource = loaded.Value.Results.ToList();
        _cohortGrid.ItemsSource = loaded.Value.Cohort.ToList();
        _weakGrid.ItemsSource = loaded.Value.Weakest.ToList();

        _loading = true;

        try
        {
            _courseGrid.SelectedItem = chosen is not null
                ? loaded.Value.Courses.FirstOrDefault(course => course.Id == chosen)
                : loaded.Value.Courses.FirstOrDefault();
        }
        finally
        {
            _loading = false;
        }

        _failure.Text = string.Empty;

        ShowState(loaded.Value.Courses.Count > 0 ? OgViewState.Ready : OgViewState.Empty, loaded.Value.SummaryText);

        // AC-3: a ledger that cannot be written is on the page before an attempt is lost to it, not after.
        if (!loaded.Value.Ledger.Writable || !loaded.Value.Ledger.Readable)
        {
            _notice.Text = "The results ledger cannot be read or written: an attempt will still be marked and will be shown as " +
                           "NOT RECORDED.";
        }

        return loaded.Value.Courses.Count > 0;
    }

    public async Task<bool> RecordAsync()
    {
        var course = ChosenCourse;

        if (course is null)
        {
            LastFailed = true;
            _failure.Text = "Select a course first; an attempt is recorded against a course.";
            return false;
        }

        // The scenario is the course's first one this trainee has not answered, so recording follows the course's own order.
        var next = _workspace?.Progress.FirstOrDefault(row => row.ScenarioId.Length > 0 && !row.Attempted);

        if (next is null)
        {
            LastFailed = true;
            _failure.Text = "Every scenario of '" + course.Title + "' has an attempt, so there is nothing to record in order.";
            return false;
        }

        var opened = await _scenarios.StartAsync(next.ScenarioId).ConfigureAwait(true);

        if (!opened.Success || opened.Value is null)
        {
            LastFailed = true;
            _failure.Text = opened.Message ?? "The scenario could not be opened.";
            return false;
        }

        _openScenario = opened.Value;

        var answers = OgTrainingRules.Answers(opened.Value, _answers.Text, out var problem);

        if (problem is not null)
        {
            LastFailed = true;
            _failure.Text = problem;
            return false;
        }

        var recorded = await _courses
            .RecordAsync(next.ScenarioId, answers, _trainee.Text?.Trim() ?? string.Empty, course.Id)
            .ConfigureAwait(true);

        if (!recorded.Success || recorded.Value is null)
        {
            LastFailed = true;
            _failure.Text = recorded.Message ?? "The attempt was refused.";
            ShowState(OgViewState.Failed, "The attempt is not recorded.", recorded.Reference);
            return false;
        }

        LastFailed = false;
        _recording.Text = recorded.Value.Describe();
        _failure.Text = string.Empty;

        _application.AuditLog.Add("training.course.attempt", "training", course.Id + "/" + next.ScenarioId,
            recorded.Value.Recorded ? "ok" : "not-recorded", _application.Session.UserId);

        _notice.Text = recorded.Value.Recorded
            ? "Attempt recorded against " + course.Title + "."
            : "Attempt marked but NOT RECORDED: " + (recorded.Value.WriteFailure ?? "the ledger was not written") +
              ". The result is shown; fix the ledger before relying on it.";

        // The progress and the results are re-read, so what is shown is what the ledger holds.
        Load();

        return recorded.Value.Recorded;
    }

    /// <summary>AC-5: the cohort's pass rate is passed ÷ attempted, and null - not zero - when nothing was attempted.</summary>
    public static double? PassRate(int passed, int attempted) => OgCourseRules.PassRate(passed, attempted);

    /// <summary>
    /// Shows the selected course's progress. Selecting a course re-reads it, because the progress belongs to the selected course - and
    /// the flag stops Load's own selection from re-entering this.
    /// </summary>
    private void ShowCourse()
    {
        if (_loading || _courseGrid.SelectedItem is not OgCourseRow course)
        {
            return;
        }

        _loading = true;

        try
        {
            var reload = _courses.LoadAsync(_trainee.Text?.Trim() ?? string.Empty, course.Id).GetAwaiter().GetResult();

            if (!reload.Success || reload.Value is null)
            {
                return;
            }

            _workspace = reload.Value;
            _progress.Text = reload.Value.ProgressText;
        }
        finally
        {
            _loading = false;
        }
    }

    private void Clear()
    {
        _summary.Text = string.Empty;
        _progress.Text = string.Empty;
        _results.Text = string.Empty;
        _cohort.Text = string.Empty;
        _weakest.Text = string.Empty;
        _ledger.Text = string.Empty;
        _problems.Text = string.Empty;
        _notes.Text = string.Empty;
        _recording.Text = string.Empty;
        _refreshed.Text = string.Empty;
        _courseGrid.ItemsSource = null;
        _resultGrid.ItemsSource = null;
        _cohortGrid.ItemsSource = null;
        _weakGrid.ItemsSource = null;
        _notice.Text = string.Empty;
    }

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 190,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
        };

        foreach (var (header, path, width) in columns)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding(path),
                Width = new DataGridLength(width),
            });
        }

        Named(grid, name);
        return grid;
    }

    private static TextBox Field(string name, double width)
    {
        var box = new TextBox
        {
            MinWidth = width,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = name,
        };

        Named(box, name);
        return box;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 6, 0, 0),
        FontSize = 11.5,
        FontWeight = FontWeights.SemiBold,
    };

    private static Button Button(string text, RoutedEventHandler onClick)
    {
        var button = new Button
        {
            Content = text,
            Padding = new Thickness(10, 3, 10, 3),
            Margin = new Thickness(0, 0, 8, 0),
        };

        button.Click += onClick;
        return button;
    }
}
