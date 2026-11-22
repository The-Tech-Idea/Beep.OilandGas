using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>One row of the survey grid.</summary>
public sealed record OgSeismicSurveyRow(
    string Survey,
    string Kind,
    string Area,
    string Started,
    string Completed,
    string Acquisition,
    string Status,
    string Processing);

/// <summary>One row of the processing panel.</summary>
public sealed record OgSeismicProcessingRow(
    string Run,
    string Status,
    string Company,
    string Started,
    string Completed,
    string Extent);

/// <summary>
/// EXP-04: the seismic register - surveys, their lines and their processing runs, with a map panel.
/// </summary>
/// <remarks>
/// The register records metadata only, and the notice saying so is always on the page (AC-3). The survey grid
/// carries the acquisition state (from the recorded dates) and the processing state resolved from the reference
/// code (AC-1, AC-5). Lines are read for the selected survey only (AC-2). The map panel is enabled only when a
/// coordinate box is recorded, and says why when it is not (AC-4).
/// </remarks>
public sealed class SeismicPageView : OgPageBase
{
    public const string Route = "seismic";

    private readonly Bootstrapping.OgApplication _application;
    private readonly IOgSeismicRegister _register;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _linesText;
    private readonly TextBlock _mapText;
    private readonly TextBlock _processingText;
    private readonly Canvas _map;
    private readonly DataGrid _surveys;
    private readonly DataGrid _lines;
    private readonly DataGrid _processing;

    public SeismicPageView(Bootstrapping.OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _register = application.SeismicRegister;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.FontWeight = FontWeights.SemiBold;
        _notice.TextWrapping = TextWrapping.Wrap;

        _linesText = Body(string.Empty);
        _mapText = Body(string.Empty);
        _processingText = Body(string.Empty);

        _surveys = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 150,
            MaxHeight = 260,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Survey", nameof(OgSeismicSurveyRow.Survey), 200),
                Text("Kind", nameof(OgSeismicSurveyRow.Kind), 70),
                Text("Area", nameof(OgSeismicSurveyRow.Area), 110),
                Text("Started", nameof(OgSeismicSurveyRow.Started), 90),
                Text("Completed", nameof(OgSeismicSurveyRow.Completed), 90),
                Text("Acquisition", nameof(OgSeismicSurveyRow.Acquisition), 90),
                Text("Status", nameof(OgSeismicSurveyRow.Status), 150),
                Text("Processing", nameof(OgSeismicSurveyRow.Processing), 190),
            },
        };

        Named(_surveys, "Seismic surveys");
        _surveys.SelectionChanged += (_, _) => SelectFromGrid();

        _lines = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 120,
            MaxHeight = 220,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            Columns =
            {
                Text("Line", nameof(OgSeismicLine.Name), 180),
                Text("Id", nameof(OgSeismicLine.Id), 120),
                Text("Dimension", nameof(OgSeismicLine.Dimension), 90),
                Text("Method", nameof(OgSeismicLine.Method), 130),
            },
        };

        Named(_lines, "Survey seismic lines");

        _processing = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 200,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            Columns =
            {
                Text("Run", nameof(OgSeismicProcessingRow.Run), 160),
                Text("State", nameof(OgSeismicProcessingRow.Status), 180),
                Text("Company", nameof(OgSeismicProcessingRow.Company), 150),
                Text("Started", nameof(OgSeismicProcessingRow.Started), 90),
                Text("Completed", nameof(OgSeismicProcessingRow.Completed), 90),
                Text("Extent", nameof(OgSeismicProcessingRow.Extent), 260),
            },
        };

        Named(_processing, "Processing runs");

        _map = new Canvas
        {
            Width = 320,
            Height = 180,
            Background = SystemColors.ControlLightBrush,
            Margin = new Thickness(0, 4, 0, 8),
            Focusable = false,
            IsHitTestVisible = false,
        };

        var mapPanel = new StackPanel { Margin = new Thickness(0, 0, 12, 0) };
        mapPanel.Children.Add(Caption("Map (survey extent)"));
        mapPanel.Children.Add(_map);
        mapPanel.Children.Add(_mapText);

        var processingPanel = new StackPanel();
        processingPanel.Children.Add(Caption("Processing"));
        processingPanel.Children.Add(_processingText);
        processingPanel.Children.Add(_processing);

        var columns = new Grid();
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

        Grid.SetColumn(mapPanel, 0);
        Grid.SetColumn(processingPanel, 1);
        columns.Children.Add(mapPanel);
        columns.Children.Add(processingPanel);

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        toolbar.Children.Add(Button("Refresh", (_, _) => Load()));
        toolbar.Children.Add(Button(
            "Exploration workspace",
            (_, _) => _application.Pages.Navigate(ExplorationWorkspacePageView.Route)));

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_surveys);
        panel.Children.Add(_linesText);
        panel.Children.Add(_lines);
        panel.Children.Add(columns);

        ShowContent(panel);

        Load();
    }

    public IReadOnlyList<OgSeismicSurvey> Surveys { get; private set; } = [];

    public IReadOnlyList<OgSeismicLine> Lines { get; private set; } = [];

    public OgSeismicSurvey? SelectedSurvey { get; private set; }

    public bool LastFailed { get; private set; }

    /// <summary>AC-4: the map is offered only when the selected survey has a coordinate box.</summary>
    public bool MapAvailable => SelectedSurvey?.HasGeometry == true;

    /// <summary>AC-3: the notice is a constant, so it cannot be hidden by a state.</summary>
    public string AlwaysVisibleNotice => OgSeismicRegister.NoDataLoadingNotice;

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MapText => _mapText.Text;

    public string ProcessingText => _processingText.Text;

    public string LinesText => _linesText.Text;

    public bool Load()
    {
        var session = _application.Session;
        var loaded = _register
            .LoadSurveysAsync(session.FieldId, session.FieldName)
            .GetAwaiter()
            .GetResult();

        // AC-3: the notice is set before anything can fail, and is never cleared.
        _notice.Text = OgSeismicRegister.NoDataLoadingNotice;

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Surveys = [];
            Lines = [];
            SelectedSurvey = null;
            _surveys.ItemsSource = null;
            _lines.ItemsSource = null;
            _processing.ItemsSource = null;
            _header.Text = "Seismic register";
            _linesText.Text = string.Empty;
            _mapText.Text = string.Empty;
            _processingText.Text = string.Empty;
            _map.Children.Clear();
            ShowState(OgViewState.Failed, "The seismic register could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Surveys = loaded.Value;
        SelectedSurvey = null;
        Lines = [];

        _surveys.ItemsSource = Surveys
            .Select(survey => new OgSeismicSurveyRow(
                survey.Name,
                survey.Kind,
                survey.AreaId ?? string.Empty,
                Date(survey.StartDate),
                Date(survey.EndDate),
                survey.AcquisitionState,
                survey.StatusText,
                survey.ProcessingState))
            .ToList();

        _lines.ItemsSource = null;
        _processing.ItemsSource = null;
        _linesText.Text = "Select a survey to see its lines.";
        _mapText.Text = "Select a survey to see its extent.";
        _processingText.Text = string.Empty;
        _map.Children.Clear();

        _header.Text =
            $"{Surveys.Count.ToString(CultureInfo.InvariantCulture)} seismic survey(s) for " +
            $"{_application.Session.FieldName}";

        ShowState(
            Surveys.Count == 0 ? OgViewState.Empty : OgViewState.Ready,
            Surveys.Count == 0 ? "No seismic survey is recorded for this field." : string.Empty);

        return true;
    }

    /// <summary>AC-2: loads the lines of one survey only.</summary>
    public bool OpenSurvey(string surveyId)
    {
        SelectedSurvey = Surveys.FirstOrDefault(survey =>
            string.Equals(survey.Id, surveyId, StringComparison.OrdinalIgnoreCase));

        if (SelectedSurvey is null)
        {
            _linesText.Text = "That survey is not in the register.";
            return false;
        }

        var loaded = _register.LoadLinesAsync(SelectedSurvey.Id).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            Lines = [];
            _lines.ItemsSource = null;
            _linesText.Text = loaded.Message ?? "The seismic lines could not be read.";
            ShowState(OgViewState.Failed, "The seismic lines could not be read.", loaded.Reference);
            return false;
        }

        Lines = loaded.Value;

        // AC-2: every line returned belongs to the selected survey, and the page says so.
        var foreign = Lines.Count(line =>
            !string.Equals(line.SurveyId, SelectedSurvey.Id, StringComparison.OrdinalIgnoreCase));

        _linesText.Text =
            $"{Lines.Count.ToString(CultureInfo.InvariantCulture)} line(s) for {SelectedSurvey.Name}" +
            (foreign == 0 ? "." : $"; {foreign} belong to another survey and were not shown.")
            + " Direction, length and shot count are not recorded in SEIS_LINE.";

        _lines.ItemsSource = Lines
            .Where(line => string.Equals(line.SurveyId, SelectedSurvey.Id, StringComparison.OrdinalIgnoreCase))
            .ToList();

        RenderProcessing(SelectedSurvey);
        RenderMap(SelectedSurvey);

        return true;
    }

    private void SelectFromGrid()
    {
        var index = _surveys.SelectedIndex;

        if (index < 0 || index >= Surveys.Count)
        {
            return;
        }

        OpenSurvey(Surveys[index].Id);
    }

    private void RenderProcessing(OgSeismicSurvey survey)
    {
        if (survey.Processing.Count == 0)
        {
            _processingText.Text = "No processing run is recorded for this survey.";
            _processing.ItemsSource = null;
            return;
        }

        _processingText.Text =
            $"{survey.Processing.Count.ToString(CultureInfo.InvariantCulture)} processing run(s); " +
            "the state is the reference code PROCESS_STATUS resolved through R_SEIS_PROC_STATUS.";

        _processing.ItemsSource = survey.Processing
            .Select(run => new OgSeismicProcessingRow(
                run.Name ?? run.Id,
                run.StatusText,
                run.Company ?? string.Empty,
                Date(run.StartDate),
                Date(run.EndDate),
                run.ExtentText))
            .ToList();
    }

    /// <summary>AC-4: the extent is drawn when it exists; otherwise the panel states why it is empty.</summary>
    private void RenderMap(OgSeismicSurvey survey)
    {
        _map.Children.Clear();

        var run = survey.Processing.FirstOrDefault(candidate => candidate.HasExtent);

        if (run is null)
        {
            _mapText.Text = survey.MapReason;
            return;
        }

        var minLat = run.MinLatitude!.Value;
        var maxLat = run.MaxLatitude!.Value;
        var minLong = run.MinLongitude!.Value;
        var maxLong = run.MaxLongitude!.Value;

        var width = _map.Width - 20;
        var height = _map.Height - 20;

        // The box keeps the extent's aspect ratio inside the canvas, centred.
        var spanLong = Math.Max((double)(maxLong - minLong), 0.000001d);
        var spanLat = Math.Max((double)(maxLat - minLat), 0.000001d);
        var scale = Math.Min(width / spanLong, height / spanLat);

        var boxWidth = Math.Max(6d, spanLong * scale);
        var boxHeight = Math.Max(6d, spanLat * scale);

        var box = new Rectangle
        {
            Width = boxWidth,
            Height = boxHeight,
            Stroke = SystemColors.ControlDarkDarkBrush,
            StrokeThickness = 1,
            Fill = SystemColors.ControlBrush,
            Opacity = 0.6,
        };

        Canvas.SetLeft(box, 10 + (width - boxWidth) / 2);
        Canvas.SetTop(box, 10 + (height - boxHeight) / 2);
        _map.Children.Add(box);

        _mapText.Text =
            $"Survey extent from the processing run {run.Name ?? run.Id}: {run.ExtentText}. " +
            "Seismic data is not loaded; the box is the recorded extent only.";
    }

    private static string Date(DateTime? value) =>
        value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(0, 2, 0, 2),
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

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };
}
