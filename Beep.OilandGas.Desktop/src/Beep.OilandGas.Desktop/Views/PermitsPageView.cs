using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// HSE-07: the permit inbox, a new application and its decision.
/// </summary>
/// <remarks>
/// Only the owner's own legal transitions are offered (AC-1); the jurisdiction is explicit and never defaulted (AC-2); a
/// foreign asset is refused (AC-3); every decision records actor, date, rationale and conditions, and the applicant cannot
/// decide their own application (AC-4); the document is written from the same record the screen shows, while the PDF export is
/// disabled with its reason (AC-5); and the history is append-only (AC-6).
/// </remarks>
public sealed class PermitsPageView : OgPageBase
{
    public const string Route = "permits";

    private readonly OgApplication _application;
    private readonly IOgPermits _permits;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _renderer;
    private readonly TextBlock _counts;
    private readonly TextBlock _failure;
    private readonly TextBlock _selected;
    private readonly TextBlock _legal;
    private readonly TextBox _document;
    private readonly TextBox _intent;
    private readonly TextBox _country;
    private readonly TextBox _stateProvince;
    private readonly TextBox _assetId;
    private readonly TextBox _scope;
    private readonly TextBox _rationale;
    private readonly TextBox _conditions;
    private readonly ComboBox _type;
    private readonly ComboBox _assetKind;
    private readonly ComboBox _target;
    private readonly DataGrid _applicationGrid;
    private readonly DataGrid _historyGrid;
    private readonly Button _loadButton;
    private readonly Button _createButton;
    private readonly Button _decideButton;

    private OgPermitWorkspace? _workspace;
    private OgPermitApplication? _selectedApplication;

    public PermitsPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _permits = application.Permits;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _renderer = Body(string.Empty);
        _renderer.TextWrapping = TextWrapping.Wrap;
        _renderer.Margin = new Thickness(0, 4, 0, 4);

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _selected = Body(string.Empty);
        _selected.TextWrapping = TextWrapping.Wrap;
        _selected.Margin = new Thickness(0, 4, 0, 4);

        _legal = Body(string.Empty);
        _legal.TextWrapping = TextWrapping.Wrap;
        _legal.Margin = new Thickness(0, 0, 0, 4);

        _document = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 130,
            MaxHeight = 200,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        Named(_document, "Application document");

        _intent = Field("Intent key", 140);
        _country = Field("Country", 130);
        _stateProvince = Field("State or province", 130);
        _assetId = Field("Asset id", 140);
        _scope = Field("Scope", 260);
        _rationale = Field("Rationale", 240);
        _conditions = Field("Conditions", 240);

        _type = Picker("Application type", 150);
        _assetKind = Picker("Asset kind", 110);
        _target = Picker("Next state", 170);

        _applicationGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 120,
            MaxHeight = 200,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Reference", "Number", 150),
                Text("Type", "Type", 120),
                Text("Jurisdiction", "JurisdictionText", 130),
                Text("Asset", "AssetId", 120),
                Text("State", "State", 170),
            },
        };

        Named(_applicationGrid, "Permit applications");
        _applicationGrid.SelectionChanged += (_, _) => SelectApplication();

        _historyGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 160,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("At", "At", 140),
                Text("State", "State", 160),
                Text("By", "By", 110),
                Text("Remarks", "Remarks", 320),
            },
        };

        Named(_historyGrid, "Status history");

        _loadButton = Button("Load", (_, _) => Load());
        _createButton = Button("File application", (_, _) => _ = CreateAsync());
        _decideButton = Button("Apply decision", (_, _) => _ = DecideAsync());

        var loadBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        loadBar.Children.Add(_loadButton);

        var createBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        createBar.Children.Add(Caption("Intent"));
        createBar.Children.Add(_intent);
        createBar.Children.Add(_type);
        createBar.Children.Add(Caption("Country"));
        createBar.Children.Add(_country);
        createBar.Children.Add(Caption("State"));
        createBar.Children.Add(_stateProvince);

        var createBar2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        createBar2.Children.Add(_assetKind);
        createBar2.Children.Add(Caption("Asset"));
        createBar2.Children.Add(_assetId);
        createBar2.Children.Add(Caption("Scope"));
        createBar2.Children.Add(_scope);
        createBar2.Children.Add(_createButton);

        var decideBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        decideBar.Children.Add(Caption("Decision"));
        decideBar.Children.Add(_target);
        decideBar.Children.Add(Caption("Rationale"));
        decideBar.Children.Add(_rationale);
        decideBar.Children.Add(Caption("Conditions"));
        decideBar.Children.Add(_conditions);
        decideBar.Children.Add(_decideButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_renderer);
        panel.Children.Add(_counts);
        panel.Children.Add(_failure);
        panel.Children.Add(loadBar);
        panel.Children.Add(Caption("Inbox"));
        panel.Children.Add(_applicationGrid);
        panel.Children.Add(Caption("Selected application"));
        panel.Children.Add(_selected);
        panel.Children.Add(_legal);
        panel.Children.Add(_historyGrid);
        panel.Children.Add(Caption("Document"));
        panel.Children.Add(_document);
        panel.Children.Add(Caption("Decision"));
        panel.Children.Add(decideBar);
        panel.Children.Add(Caption("New application"));
        panel.Children.Add(createBar);
        panel.Children.Add(createBar2);

        ShowContent(panel);

        var session = application.Session;
        _intent.Text = "permit-" + Guid.NewGuid().ToString("N")[..8];
        _assetId.Text = session.FieldId ?? string.Empty;

        Load();
    }

    public OgPermitWorkspace? Workspace => _workspace;

    public OgPermitApplication? Application => _selectedApplication;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string RendererText => _renderer.Text;

    public string CountsText => _counts.Text;

    public string FailureText => _failure.Text;

    public string SelectedText => _selected.Text;

    public string LegalText => _legal.Text;

    public string DocumentText => _document.Text;

    public bool Load()
    {
        _header.Text = "HSE · permits";

        // AC-5's own state: the export is disabled and the reason is on the page whatever else it says.
        _renderer.Text = OgPermitWorkspace.RendererNotice;

        var loaded = _permits.LoadAsync(OgHseScope.FromSession(_application.Session)).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _applicationGrid.ItemsSource = null;
            _historyGrid.ItemsSource = null;
            _counts.Text = string.Empty;
            _selected.Text = string.Empty;
            _legal.Text = string.Empty;
            _document.Text = string.Empty;
            _notice.Text = string.Empty;
            _failure.Text = loaded.Message ?? "The permit inbox could not be read.";
            ShowState(OgViewState.Failed, "The permit inbox is not shown.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public Task<bool> CreateAsync() =>
        RunAsync(() => _permits.CreateAsync(
                new OgPermitInputs(
                    Empty(_intent.Text),
                    _type.SelectedItem as string,
                    Empty(_country.Text),
                    Empty(_stateProvince.Text),
                    _assetKind.SelectedItem as string,
                    Empty(_assetId.Text),
                    _application.Session.FieldId,
                    Empty(_scope.Text)),
                OgHseScope.FromSession(_application.Session),
                _application.Session.UserId),
            "permits.write",
            "Application filed");

    public Task<bool> DecideAsync()
    {
        if (_selectedApplication is not { } application)
        {
            _failure.Text = "Select the application to decide.";
            return Task.FromResult(false);
        }

        var target = _target.SelectedItem as string;

        if (string.IsNullOrWhiteSpace(target))
        {
            _failure.Text = "There is no legal next state to apply from this one.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _permits.DecideAsync(
                application.Id,
                target,
                _rationale.Text ?? string.Empty,
                Empty(_conditions.Text),
                OgHseScope.FromSession(_application.Session),
                _application.Session.UserId),
            "permits.decide",
            "Application moved to " + target);
    }

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgPermitWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The permit command was refused.";
            ShowState(OgViewState.Failed, "The permit command was refused.", outcome.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add(auditAction, "hse", outcome.Value.Selected?.Id ?? "(permit)", "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = done + ".";
        _intent.Text = "permit-" + Guid.NewGuid().ToString("N")[..8];

        return true;
    }

    private void Apply(OgPermitWorkspace workspace)
    {
        _workspace = workspace;

        _counts.Text = workspace.CountsText + " · " + workspace.ReadText;
        _notice.Text = workspace.Notice;

        // AC-2: the type and asset-kind pickers offer the recorded vocabularies, never a guessed default.
        Bind(_type, Enum.GetNames<PermitApplicationType>());
        Bind(_assetKind, ["WELL", "FACILITY", "FIELD"]);

        _applicationGrid.ItemsSource = workspace.Applications.ToList();
        _selectedApplication = workspace.Selected;
        _applicationGrid.SelectedItem = _selectedApplication;

        ShowSelected();

        _failure.Text = string.Empty;
        RefreshEnabled();

        ShowState(workspace.Applications.Count == 0 ? OgViewState.Empty : OgViewState.Ready, workspace.CountsText);
    }

    private void ShowSelected()
    {
        if (_selectedApplication is not { } application)
        {
            _selected.Text = "no application is selected";
            _legal.Text = string.Empty;
            _historyGrid.ItemsSource = null;
            _document.Text = string.Empty;
            return;
        }

        _selected.Text = application.Describe();
        _historyGrid.ItemsSource = application.History.ToList();

        // AC-1: the decision picker offers exactly the legal next states.
        Bind(_target, application.LegalNextStates);

        _legal.Text = application.LegalNextStates.Count == 0
            ? "This application has no legal next state."
            : "Legal next states: " + string.Join(", ", application.LegalNextStates);

        // AC-5: the document is the record's own content, so it cannot drift from the grid.
        _document.Text = application.Document();
    }

    private void SelectApplication()
    {
        if (_applicationGrid.SelectedItem is not OgPermitApplication selected)
        {
            return;
        }

        _selectedApplication = selected;
        ShowSelected();
        RefreshEnabled();
    }

    private void RefreshEnabled()
    {
        var loaded = _workspace is not null;

        _createButton.IsEnabled = loaded;
        _decideButton.IsEnabled = loaded && _selectedApplication is { LegalNextStates.Count: > 0 };
    }

    private static void Bind(ComboBox picker, IReadOnlyList<string> values)
    {
        var previous = picker.SelectedItem as string;

        picker.ItemsSource = values.ToList();
        picker.SelectedItem = previous is not null && values.Contains(previous, StringComparer.OrdinalIgnoreCase)
            ? values.First(value => string.Equals(value, previous, StringComparison.OrdinalIgnoreCase))
            : values.Count > 0
                ? values[0]
                : null;
    }

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
