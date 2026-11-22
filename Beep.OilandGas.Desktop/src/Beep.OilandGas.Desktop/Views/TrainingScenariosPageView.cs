using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Training;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// TRN-01: a scenario run - the exercise's well on the page, its questions, the marking and the solution once the attempt is submitted.
/// </summary>
/// <remarks>
/// The scenario is a file, the view is its <c>WellboreDocument</c> drawn by the same code as every other well view, the marking is the
/// engine's <c>ScenarioScoring</c> and the ledger is the engine's <c>ResultsLedger</c>. The page is handed a scenario **without its
/// answers** until an attempt exists, so the solution cannot appear early (AC-3); it takes no database at all, so training cannot reach
/// production data (AC-5); and the policy in force - which attempt counts - is printed rather than implied (AC-6).
/// </remarks>
public sealed class TrainingScenariosPageView : OgPageBase
{
    public const string Route = "training";

    private readonly OgApplication _application;
    private readonly IOgTrainingScenarios _training;
    private readonly WellboreView _view = new();
    private readonly SkiaViewSurfaceHost _host;
    private readonly System.Windows.Controls.Image _exhibit = new();

    private readonly TextBlock _header;
    private readonly TextBlock _summary;
    private readonly TextBlock _courses;
    private readonly TextBlock _problems;
    private readonly TextBlock _instructions;
    private readonly TextBlock _questions;
    private readonly TextBlock _missing;
    private readonly TextBlock _outcomes;
    private readonly TextBlock _warnings;
    private readonly TextBlock _history;
    private readonly TextBlock _policy;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _trainee;
    private readonly TextBox _answers;
    private readonly DataGrid _scenarioGrid;
    private readonly Button _loadButton;
    private readonly Button _startButton;
    private readonly Button _submitButton;
    private readonly Button _samplesButton;

    private OgTrainingWorkspace? _workspace;
    private OgScenarioRun? _run;

    public TrainingScenariosPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _training = application.TrainingScenarios;

        _host = new SkiaViewSurfaceHost
        {
            FailureReporter = application.Diagnostics,
            MinHeight = 420,
        };

        // A map exercise has no well, so its exhibit is the engine's own map image rather than a document.
        _exhibit.Stretch = System.Windows.Media.Stretch.Uniform;
        _exhibit.MaxHeight = 620;
        _exhibit.HorizontalAlignment = HorizontalAlignment.Left;
        _exhibit.Visibility = Visibility.Collapsed;
        Named(_exhibit, "Map exhibit");

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.FontWeight = FontWeights.SemiBold;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _courses = Body(string.Empty);
        _courses.TextWrapping = TextWrapping.Wrap;
        _courses.Margin = new Thickness(0, 0, 0, 4);

        _problems = Body(string.Empty);
        _problems.TextWrapping = TextWrapping.Wrap;
        _problems.Margin = new Thickness(0, 0, 0, 4);

        _instructions = Body(string.Empty);
        _instructions.TextWrapping = TextWrapping.Wrap;
        _instructions.Margin = new Thickness(0, 4, 0, 4);

        _questions = Body(string.Empty);
        _questions.TextWrapping = TextWrapping.Wrap;
        _questions.Margin = new Thickness(0, 0, 0, 4);

        _missing = Body(string.Empty);
        _missing.TextWrapping = TextWrapping.Wrap;
        _missing.FontWeight = FontWeights.SemiBold;
        _missing.Margin = new Thickness(0, 0, 0, 4);

        _outcomes = Body(string.Empty);
        _outcomes.TextWrapping = TextWrapping.Wrap;
        _outcomes.Margin = new Thickness(0, 4, 0, 4);

        _warnings = Body(string.Empty);
        _warnings.TextWrapping = TextWrapping.Wrap;
        _warnings.Margin = new Thickness(0, 0, 0, 4);

        _history = Body(string.Empty);
        _history.TextWrapping = TextWrapping.Wrap;
        _history.Margin = new Thickness(0, 0, 0, 4);

        _policy = Body(string.Empty);
        _policy.TextWrapping = TextWrapping.Wrap;
        _policy.Margin = new Thickness(0, 0, 0, 4);

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

        // One line per answer: the question id, then what the trainee gives. The type decides the rest, which the question list shows.
        _answers = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 90,
            MaxHeight = 170,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "One answer per line: question id = value. A choice takes its option id; a multi takes ids separated by commas; " +
                      "a depth or number takes the value and, if it is not the question's unit, the unit too",
        };

        Named(_answers, "Answers");

        _scenarioGrid = Grid("Scenarios", ("Id", "Id", 160), ("Title", "Title", 220), ("Course", "Course", 150),
            ("Level", "Level", 90), ("Questions", "Questions", 80), ("Points", "MaxScore", 70),
            ("Runnable", "Valid", 80));

        _scenarioGrid.SelectionChanged += (_, _) => ShowSelected();

        _loadButton = Button("Reload", (_, _) => Load());
        _startButton = Button("Start", (_, _) => _ = StartAsync());
        _submitButton = Button("Submit attempt", (_, _) => _ = SubmitAsync());
        _samplesButton = Button("Write the sample pack", (_, _) => _ = SamplesAsync());

        var traineeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        traineeBar.Children.Add(Caption("Trainee"));
        traineeBar.Children.Add(_trainee);
        traineeBar.Children.Add(_loadButton);
        traineeBar.Children.Add(_samplesButton);

        var runBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        runBar.Children.Add(_startButton);
        runBar.Children.Add(_submitButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(traineeBar);
        panel.Children.Add(_summary);
        panel.Children.Add(Caption("Scenarios"));
        panel.Children.Add(_scenarioGrid);
        panel.Children.Add(Caption("Courses"));
        panel.Children.Add(_courses);
        panel.Children.Add(Caption("Problems with the folder's files"));
        panel.Children.Add(_problems);
        panel.Children.Add(runBar);
        panel.Children.Add(_instructions);
        panel.Children.Add(Caption("The exercise"));
        panel.Children.Add(_host);
        panel.Children.Add(_exhibit);
        panel.Children.Add(Caption("Questions - the answer key appears only after the attempt is submitted"));
        panel.Children.Add(_questions);
        panel.Children.Add(Caption("Your answers"));
        panel.Children.Add(_answers);
        panel.Children.Add(_missing);
        panel.Children.Add(Caption("Marking"));
        panel.Children.Add(_outcomes);
        panel.Children.Add(Caption("Warnings"));
        panel.Children.Add(_warnings);
        panel.Children.Add(Caption("Your attempt history"));
        panel.Children.Add(_history);
        panel.Children.Add(Caption("Attempt policy"));
        panel.Children.Add(_policy);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Name the trainee and load the scenarios.");
    }

    public OgTrainingWorkspace? Workspace => _workspace;

    public OgScenarioRun? Run => _run;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string SummaryText => _summary.Text;

    public string CoursesText => _courses.Text;

    public string ProblemsText => _problems.Text;

    public string InstructionsText => _instructions.Text;

    public string QuestionsText => _questions.Text;

    public string MissingText => _missing.Text;

    public string OutcomesText => _outcomes.Text;

    public string WarningsText => _warnings.Text;

    public string HistoryText => _history.Text;

    public string PolicyText => _policy.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    /// <summary>Whether the submit command is offered: it is not while a question is unanswered (the feature's own state).</summary>
    public bool CanSubmit => _run is { Submitted: false, Missing.Count: 0 };

    public bool ExerciseShown => _host.Surface is not null;

    public bool Load()
    {
        _header.Text = "Training · scenarios";

        var loaded = _training.LoadAsync(_trainee.Text?.Trim() ?? string.Empty).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            Clear();
            _failure.Text = loaded.Message ?? "The scenarios could not be read.";
            ShowState(OgViewState.Failed, "The scenarios are not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        _workspace = loaded.Value;

        _summary.Text = loaded.Value.SummaryText;
        _courses.Text = loaded.Value.CoursesText;
        _problems.Text = loaded.Value.ProblemsText;
        _history.Text = loaded.Value.HistoryText;
        _policy.Text = loaded.Value.PolicyText + ". " + OgTrainingRules.DataText +
                       (loaded.Value.Notes.Count == 0 ? string.Empty : Environment.NewLine + string.Join(
                           Environment.NewLine, loaded.Value.Notes.Select(note => "! " + note)));
        _refreshed.Text = loaded.Value.ReadText;

        _scenarioGrid.ItemsSource = loaded.Value.Scenarios.ToList();
        _scenarioGrid.SelectedItem = loaded.Value.Scenarios.FirstOrDefault(scenario => scenario.Valid);

        _failure.Text = string.Empty;
        ShowSelected();

        ShowState(loaded.Value.Runnable > 0 ? OgViewState.Ready : OgViewState.Empty, loaded.Value.SummaryText);

        return loaded.Value.Runnable > 0;
    }

    public async Task<bool> StartAsync()
    {
        var scenario = Chosen();

        if (scenario is null)
        {
            LastFailed = true;
            _failure.Text = "Select a scenario first; the runner opens one at a time.";
            return false;
        }

        if (!scenario.Valid)
        {
            LastFailed = true;
            _failure.Text = "That scenario is not runnable: " + string.Join(" ", scenario.Problems);
            return false;
        }

        var started = await _training.StartAsync(scenario.Id).ConfigureAwait(true);

        if (!started.Success || started.Value is null)
        {
            LastFailed = true;
            _run = null;
            ClearRun();
            _failure.Text = started.Message ?? "The scenario could not be opened.";
            ShowState(OgViewState.Failed, "The scenario is not shown.", started.Reference);
            return false;
        }

        LastFailed = false;
        Apply(started.Value);

        _failure.Text = string.Empty;
        _notice.Text = "Scenario opened. Answer every question, then submit.";

        return true;
    }

    public async Task<bool> SubmitAsync()
    {
        var scenario = Chosen();

        if (scenario is null || _run is null)
        {
            LastFailed = true;
            _failure.Text = "Open a scenario before submitting an attempt.";
            return false;
        }

        var answers = Answers(_run, _answers.Text, out var problem);

        if (problem is not null)
        {
            LastFailed = true;
            _failure.Text = problem;
            return false;
        }

        var submitted = await _training
            .SubmitAsync(scenario.Id, answers, _trainee.Text?.Trim() ?? string.Empty)
            .ConfigureAwait(true);

        if (!submitted.Success || submitted.Value is null)
        {
            LastFailed = true;
            _failure.Text = submitted.Message ?? "The attempt was refused.";
            ShowState(OgViewState.Failed, "The attempt is not marked.", submitted.Reference);
            return false;
        }

        LastFailed = false;
        Apply(submitted.Value);

        _application.AuditLog.Add("training.attempt", "training", scenario.Id, "ok", _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = submitted.Value.Passed switch
        {
            true => "Attempt recorded and passed.",
            false => "Attempt recorded and not passed - the marking shows where.",
            null => "Attempt recorded; no pass mark is set, so the score stands alone.",
        };

        // The history and the folder are re-read, so the attempt is on the list as recorded rather than as remembered.
        Load();

        return true;
    }

    public async Task<bool> SamplesAsync()
    {
        var written = await _training.WriteSamplePackAsync(_application.Session.UserId).ConfigureAwait(true);

        if (!written.Success || written.Value is null)
        {
            LastFailed = true;
            _failure.Text = written.Message ?? "The sample pack was not written.";
            return false;
        }

        LastFailed = false;
        _notice.Text = "Sample pack written to " + written.Value + ".";
        _application.AuditLog.Add("training.samples", "training", written.Value, "ok", _application.Session.UserId);

        return Load();
    }

    /// <summary>
    /// The answer entry, one line per question - parsed by the same rule the course page uses, so the two cannot disagree about what a
    /// trainee wrote.
    /// </summary>
    public static Dictionary<string, OgScenarioAnswer> Answers(
        OgScenarioRun run, string? text, out string? problem) =>
        OgTrainingRules.Answers(run, text, out problem);

    private void Apply(OgScenarioRun run)
    {
        _run = run;

        _instructions.Text = run.Title + Environment.NewLine + run.Instructions +
                            (string.IsNullOrWhiteSpace(run.Course) ? string.Empty : Environment.NewLine + "Course: " + run.Course) +
                            (string.IsNullOrWhiteSpace(run.Level) ? string.Empty : " · " + run.Level);

        // AC-3: before an attempt the list carries the questions only; the key and the explanations arrive with the marking.
        _questions.Text = run.Submitted
            ? run.QuestionsText
            : string.Join(Environment.NewLine + Environment.NewLine,
                run.Questions.Select(question => question.Describe() + Environment.NewLine + question.OptionsText));

        _missing.Text = run.Submitted ? string.Empty : run.MissingText;
        _outcomes.Text = run.OutcomesText;
        _warnings.Text = run.WarningsText;

        _summary.Text = run.SummaryText;

        // The exhibit is the engine's: the trainee's before submission, the solution's after - a well drawn as a document, or a map
        // rendered to an image when the exercise has no well.
        if (run.Exhibit is { Length: > 0 } png)
        {
            _host.Visibility = Visibility.Collapsed;

            _exhibit.Source = ToBitmap(png);
            _exhibit.Visibility = Visibility.Visible;
        }
        else
        {
            _exhibit.Source = null;
            _exhibit.Visibility = Visibility.Collapsed;

            _view.Document = run.Document ?? _view.Document;
            _host.Surface = _view;
            _host.Visibility = Visibility.Visible;
            _host.Redraw();
        }

        _submitButton.IsEnabled = !run.Submitted && run.Missing.Count == 0;

        ShowState(OgViewState.Ready, run.SummaryText);
    }

    private void ClearRun()
    {
        _instructions.Text = string.Empty;
        _questions.Text = string.Empty;
        _missing.Text = string.Empty;
        _outcomes.Text = string.Empty;
        _warnings.Text = string.Empty;

        // A refused run takes the exercise off the page rather than leaving the last scenario's well standing.
        _host.Surface = null;
        _host.Redraw();
        _exhibit.Source = null;
        _exhibit.Visibility = Visibility.Collapsed;

        _submitButton.IsEnabled = false;
    }

    private void Clear()
    {
        _summary.Text = string.Empty;
        _courses.Text = string.Empty;
        _problems.Text = string.Empty;
        _history.Text = string.Empty;
        _policy.Text = string.Empty;
        _refreshed.Text = string.Empty;
        _scenarioGrid.ItemsSource = null;
        ClearRun();
        _notice.Text = string.Empty;
    }

    private void ShowSelected()
    {
        if (_scenarioGrid.SelectedItem is not OgScenarioSummary scenario)
        {
            return;
        }

        _instructions.Text = scenario.Describe() + Environment.NewLine + scenario.ProblemsText;
    }

    private OgScenarioSummary? Chosen() => _scenarioGrid.SelectedItem as OgScenarioSummary;

    /// <summary>A PNG the engine drew, as a WPF bitmap - kept on the UI thread, which is where the page runs.</summary>
    private static System.Windows.Media.Imaging.BitmapImage ToBitmap(byte[] png)
    {
        var image = new System.Windows.Media.Imaging.BitmapImage();

        using (var stream = new MemoryStream(png))
        {
            image.BeginInit();
            image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
        }

        return image;
    }

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 110,
            MaxHeight = 200,
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
