using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// HSE-02: the incident register and the report dialog.
/// </summary>
/// <remarks>
/// A report needs its type, tier, jurisdiction and an ISO timestamp with an offset (AC-1) and the jurisdiction is never
/// defaulted (AC-2); a company or field change while the list is loading clears the rows and discards the response (AC-3); a
/// failed read leaves no rows rather than an empty register (AC-4); a state change is not offered because the workflow engine
/// is not wired (AC-5); and the state filter is the recorded vocabulary, not a guess (AC-6).
/// </remarks>
public sealed class IncidentsPageView : OgPageBase
{
    public const string Route = "incidents";

    private readonly OgApplication _application;
    private readonly IOgIncidents _incidents;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _scope;
    private readonly TextBlock _states;
    private readonly TextBlock _selected;
    private readonly TextBlock _transition;
    private readonly TextBlock _failure;
    private readonly TextBox _filterField;
    private readonly TextBox _filterFrom;
    private readonly TextBox _filterTo;
    private readonly TextBox _intent;
    private readonly TextBox _type;
    private readonly TextBox _jurisdiction;
    private readonly TextBox _occurred;
    private readonly TextBox _field;
    private readonly TextBox _facility;
    private readonly TextBox _location;
    private readonly TextBox _description;
    private readonly TextBox _actions;
    private readonly TextBox _people;
    private readonly ComboBox _state;
    private readonly ComboBox _tier;
    private readonly ComboBox _reportTier;
    private readonly ComboBox _typeFilter;
    private readonly DataGrid _grid;
    private readonly Button _loadButton;
    private readonly Button _reportButton;

    private OgIncidentWorkspace? _workspace;
    private long _version;

    public IncidentsPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _incidents = application.Incidents;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _scope = Body(string.Empty);
        _scope.TextWrapping = TextWrapping.Wrap;
        _scope.Margin = new Thickness(0, 4, 0, 4);

        _states = Body(string.Empty);
        _states.TextWrapping = TextWrapping.Wrap;
        _states.Margin = new Thickness(0, 0, 0, 4);

        _selected = Body(string.Empty);
        _selected.TextWrapping = TextWrapping.Wrap;
        _selected.Margin = new Thickness(0, 4, 0, 4);

        _transition = Body(string.Empty);
        _transition.TextWrapping = TextWrapping.Wrap;
        _transition.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _filterField = Field("Filter field", 110);
        _filterFrom = Field("From yyyy-MM-dd", 130);
        _filterTo = Field("To yyyy-MM-dd", 130);

        _intent = Field("Intent key", 150);
        _type = Field("Incident type", 130);
        _jurisdiction = Field("Jurisdiction", 120);
        _occurred = Field("Occurred ISO with offset", 220);
        _field = Field("Field id", 110);
        _facility = Field("Facility id", 110);
        _location = Field("Location", 170);
        _description = Field("Description", 280);
        _actions = Field("Immediate actions", 200);
        _people = Field("People involved", 180);

        _state = Picker("State filter", 160);
        _tier = Picker("Tier filter", 120);
        _reportTier = Picker("Report tier", 120);
        _typeFilter = Picker("Type filter", 150);

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 150,
            MaxHeight = 280,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Number", "Number", 110),
                Text("Occurred", "OccurredText", 130),
                Text("Type", "Type", 110),
                Text("Tier", "Tier", 90),
                Text("Severity", "Severity", 150),
                Text("Field", "FieldId", 100),
                Text("Facility", "FacilityId", 100),
                Text("Jurisdiction", "JurisdictionText", 110),
                Text("State", "State", 150),
                Text("Investigator", "Investigator", 110),
                Text("Open actions", "OpenActions", 100),
            },
        };

        Named(_grid, "Incidents");
        _grid.SelectionChanged += (_, _) => SelectIncident();

        _loadButton = Button("Load", (_, _) => Load());
        _reportButton = Button("Report incident", (_, _) => _ = ReportAsync());

        var filterBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        filterBar.Children.Add(Caption("State"));
        filterBar.Children.Add(_state);
        filterBar.Children.Add(Caption("Tier"));
        filterBar.Children.Add(_tier);
        filterBar.Children.Add(Caption("Type"));
        filterBar.Children.Add(_typeFilter);
        filterBar.Children.Add(Caption("Field"));
        filterBar.Children.Add(_filterField);
        filterBar.Children.Add(Caption("From"));
        filterBar.Children.Add(_filterFrom);
        filterBar.Children.Add(Caption("To"));
        filterBar.Children.Add(_filterTo);
        filterBar.Children.Add(_loadButton);

        var reportBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        reportBar.Children.Add(Caption("Type"));
        reportBar.Children.Add(_type);
        reportBar.Children.Add(Caption("Tier"));
        reportBar.Children.Add(_reportTier);
        reportBar.Children.Add(Caption("Jurisdiction"));
        reportBar.Children.Add(_jurisdiction);
        reportBar.Children.Add(Caption("Occurred"));
        reportBar.Children.Add(_occurred);

        var reportBar2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        reportBar2.Children.Add(Caption("Field"));
        reportBar2.Children.Add(_field);
        reportBar2.Children.Add(Caption("Facility"));
        reportBar2.Children.Add(_facility);
        reportBar2.Children.Add(Caption("Intent"));
        reportBar2.Children.Add(_intent);
        reportBar2.Children.Add(Caption("Location"));
        reportBar2.Children.Add(_location);

        var reportBar3 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        reportBar3.Children.Add(Caption("Description"));
        reportBar3.Children.Add(_description);
        reportBar3.Children.Add(Caption("Immediate actions"));
        reportBar3.Children.Add(_actions);
        reportBar3.Children.Add(Caption("People"));
        reportBar3.Children.Add(_people);
        reportBar3.Children.Add(_reportButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_scope);
        panel.Children.Add(_states);
        panel.Children.Add(_failure);
        panel.Children.Add(Caption("Filters"));
        panel.Children.Add(filterBar);
        panel.Children.Add(Caption("Report an incident"));
        panel.Children.Add(reportBar);
        panel.Children.Add(reportBar2);
        panel.Children.Add(reportBar3);
        panel.Children.Add(Caption("Register"));
        panel.Children.Add(_grid);
        panel.Children.Add(Caption("Selected incident"));
        panel.Children.Add(_selected);
        panel.Children.Add(_transition);

        ShowContent(panel);

        var session = application.Session;
        _field.Text = session.FieldId ?? string.Empty;
        _intent.Text = NewIntent();
        _occurred.Text = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

        Load();
    }

    public OgIncidentWorkspace? Workspace => _workspace;

    public OgIncident? Selected => _workspace?.Selected;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string ScopeText => _scope.Text;

    public string StatesText => _states.Text;

    public string SelectedText => _selected.Text;

    public string TransitionText => _transition.Text;

    public string FailureText => _failure.Text;

    /// <summary>The report the dialog would submit, so a test can inspect it without the UI.</summary>
    public OgIncidentReport Report() =>
        new(
            Empty(_intent.Text),
            Empty(_type.Text),
            _reportTier.SelectedItem as string,
            Empty(_jurisdiction.Text),
            Empty(_occurred.Text),
            Empty(_field.Text),
            Empty(_facility.Text),
            _description.Text ?? string.Empty,
            Empty(_location.Text),
            Empty(_actions.Text),
            Empty(_people.Text));

    public OgIncidentFilter Filter() =>
        new(
            _state.SelectedItem as string,
            _tier.SelectedItem as string,
            _typeFilter.SelectedItem as string,
            Date(_filterFrom.Text),
            Date(_filterTo.Text),
            Empty(_filterField.Text));

    public bool Load()
    {
        var session = _application.Session;
        var scope = OgHseScope.FromSession(session);

        _version = _incidents.BeginLoad();

        var loaded = _incidents.LoadAsync(
            _version,
            scope,
            () => OgHseScope.FromSession(_application.Session),
            Filter(),
            session.TimeZoneId).GetAwaiter().GetResult();

        _header.Text = "HSE · incidents";

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;

            // AC-3 and AC-4: the rows are cleared, never left stale and never shown as an empty register.
            _grid.ItemsSource = null;
            _selected.Text = string.Empty;
            _scope.Text = string.Empty;
            _states.Text = string.Empty;
            _failure.Text = loaded.Message ?? "The incident register could not be read.";
            _notice.Text = loaded.Reference == IncidentRules.CodeScopeChanged
                ? "The field or company changed while the register was loading. The rows were discarded; load again."
                : string.Empty;

            ShowState(OgViewState.Failed, "The incident register is not shown.", loaded.Reference);

            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);

        return true;
    }

    public async Task<bool> ReportAsync()
    {
        var session = _application.Session;
        var outcome = await _incidents
            .ReportAsync(Report(), OgHseScope.FromSession(session), session.UserId, session.TimeZoneId)
            .ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The incident was not reported.";
            ShowState(OgViewState.Failed, "The incident was not reported.", outcome.Reference);

            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add("hse.incident.report", "hse", outcome.Value.Selected?.Number ?? "(incident)", "ok",
            session.UserId);

        _notice.Text = "Incident reported.";
        _failure.Text = string.Empty;
        _intent.Text = NewIntent();
        _occurred.Text = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

        return true;
    }

    private void Apply(OgIncidentWorkspace workspace)
    {
        _workspace = workspace;

        _scope.Text = workspace.ScopeText + " · " + workspace.ReadText;
        _states.Text = workspace.FiltersText;

        // AC-6: the filters offer the recorded vocabulary; a value already chosen is kept when it is still offered.
        BindPicker(_state, workspace.States);
        BindPicker(_tier, workspace.Tiers);
        BindPicker(_reportTier, workspace.Tiers);
        BindPicker(_typeFilter, workspace.Types);

        _grid.ItemsSource = workspace.Incidents.ToList();
        _grid.SelectedItem = workspace.Selected;

        _notice.Text = workspace.Incidents.Count.ToString(CultureInfo.InvariantCulture) + " incident(s).";
        _failure.Text = string.Empty;

        // AC-5: the workflow engine is not wired, so no state-change command is offered - the page says why.
        _transition.Text = OgIncidentWorkspace.TransitionNotice;

        ShowSelected();

        ShowState(workspace.Incidents.Count == 0 ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
    }

    private void SelectIncident()
    {
        if (_grid.SelectedItem is not OgIncident selected)
        {
            return;
        }

        _workspace = _workspace is null ? null : _workspace with { Selected = selected };
        ShowSelected();
    }

    private void ShowSelected()
    {
        if (_workspace?.Selected is not { } incident)
        {
            _selected.Text = "no incident is selected";
            return;
        }

        _selected.Text = incident.Describe() +
                         (string.IsNullOrWhiteSpace(incident.Description) ? string.Empty : Environment.NewLine +
                             "Description: " + incident.Description) +
                         (string.IsNullOrWhiteSpace(incident.Location) ? string.Empty : Environment.NewLine +
                             "Location: " + incident.Location);
    }

    private static void BindPicker(ComboBox picker, IReadOnlyList<string> values)
    {
        var previous = picker.SelectedItem as string;

        picker.ItemsSource = values.ToList();

        picker.SelectedItem = previous is not null && values.Contains(previous, StringComparer.OrdinalIgnoreCase)
            ? values.First(value => string.Equals(value, previous, StringComparison.OrdinalIgnoreCase))
            : null;
    }

    private static string NewIntent() => "hse-" + Guid.NewGuid().ToString("N")[..12];

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTime? Date(string? text) =>
        DateTime.TryParse(text?.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value
            : null;

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

    private static ComboBox Picker(string name, double width)
    {
        var picker = new ComboBox
        {
            MinWidth = width,
            Margin = new Thickness(0, 0, 8, 0),
        };

        Named(picker, name);
        return picker;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
        TextWrapping = TextWrapping.Wrap,
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
