using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// INT-05: the customer's wellbore environment, stood up from a country preset through the tools' own setup engine - the steps it
/// judges, the sources it tests, the coverage it computes, the mappings it proposes and the hand-over it gates.
/// </summary>
/// <remarks>
/// Every judgement on this page is the engine's: the steps and their states, the source tests, the coverage, the suggestions and the
/// verification come from <c>SetupSession</c>, so the wizard cannot disagree with the file the application loads. A suggestion is
/// only applied by the command that accepts it (AC-3), coverage that cannot be computed says so (AC-4), and a hand-over with a
/// failing case is refused (AC-5). After a write the application re-reads the environment, so the change takes effect without a
/// restart.
/// </remarks>
public sealed class WellboreSetupPageView : OgPageBase
{
    public const string Route = "wellboresetup";

    private readonly OgApplication _application;
    private readonly IOgWellboreSetup _setup;

    private readonly TextBlock _header;
    private readonly TextBlock _summary;
    private readonly TextBlock _sources;
    private readonly TextBlock _coverage;
    private readonly TextBlock _validation;
    private readonly TextBlock _handover;
    private readonly TextBlock _warnings;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _companyName;
    private readonly TextBox _sourceName;
    private readonly TextBox _definition;
    private readonly TextBox _settingPath;
    private readonly TextBox _settingValue;
    private readonly TextBox _archive;
    private readonly ComboBox _country;
    private readonly DataGrid _stepGrid;
    private readonly DataGrid _suggestionGrid;
    private readonly DataGrid _verificationGrid;
    private readonly Button _createButton;
    private readonly Button _loadButton;
    private readonly Button _addSourceButton;
    private readonly Button _removeSourceButton;
    private readonly Button _testButton;
    private readonly Button _coverageButton;
    private readonly Button _suggestButton;
    private readonly Button _acceptButton;
    private readonly Button _setButton;
    private readonly Button _verifyButton;
    private readonly Button _handoverButton;

    private OgSetupWorkspace? _workspace;

    public WellboreSetupPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _setup = application.WellboreSetup;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _sources = Body(string.Empty);
        _sources.TextWrapping = TextWrapping.Wrap;
        _sources.Margin = new Thickness(0, 0, 0, 4);

        _coverage = Body(string.Empty);
        _coverage.TextWrapping = TextWrapping.Wrap;
        _coverage.Margin = new Thickness(0, 0, 0, 4);

        _validation = Body(string.Empty);
        _validation.TextWrapping = TextWrapping.Wrap;
        _validation.Margin = new Thickness(0, 0, 0, 4);

        _handover = Body(string.Empty);
        _handover.TextWrapping = TextWrapping.Wrap;
        _handover.Margin = new Thickness(0, 0, 0, 4);

        _warnings = Body(string.Empty);
        _warnings.TextWrapping = TextWrapping.Wrap;
        _warnings.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _companyName = Field("Company name", 200);
        _sourceName = Field("Source name", 120);
        _archive = Field("Archive folder", 280);

        _definition = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 46,
            MaxHeight = 80,
            MinWidth = 420,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "The data source definition, as JSON: its kind and its path",
        };

        _definition.Text = """{ "kind": "csv", "path": "wells" }""";
        Named(_definition, "Source definition");

        _settingPath = Field("Setting path", 240);
        _settingValue = Field("Value", 240);
        _settingPath.Text = "name";

        _country = new ComboBox { MinWidth = 240, Margin = new Thickness(0, 0, 8, 0) };
        Named(_country, "Country");

        _stepGrid = Grid("Steps", ("Step", "Key", 110), ("Title", "Title", 200), ("State", "State", 110),
            ("Detail", "Detail", 460));

        _suggestionGrid = Grid("Suggestions", ("Area", "Area", 80), ("Where", "Where", 130), ("Value", "Value", 140),
            ("Target", "Target", 160), ("Score", "Score", 70), ("By", "Source", 110));

        _verificationGrid = Grid("Verification", ("Case", "Name", 240), ("Kind", "Kind", 120), ("Passed", "Passed", 80),
            ("Error", "Error", 260));

        _createButton = Button("Create from preset", (_, _) => _ = CreateAsync());
        _loadButton = Button("Reload", (_, _) => Load());
        _addSourceButton = Button("Add / replace", (_, _) => _ = AddSourceAsync());
        _removeSourceButton = Button("Remove", (_, _) => _ = RemoveSourceAsync());
        _testButton = Button("Test", (_, _) => _ = TestAsync());
        _coverageButton = Button("Coverage", (_, _) => _ = CoverageAsync());
        _suggestButton = Button("Suggest", (_, _) => _ = SuggestAsync());
        _acceptButton = Button("Accept selected", (_, _) => _ = AcceptAsync());
        _setButton = Button("Set", (_, _) => _ = SetAsync());
        _verifyButton = Button("Verify", (_, _) => _ = VerifyAsync());
        _handoverButton = Button("Hand over", (_, _) => _ = HandoverAsync());

        var createBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        createBar.Children.Add(Caption("Country"));
        createBar.Children.Add(_country);
        createBar.Children.Add(Caption("Company"));
        createBar.Children.Add(_companyName);
        createBar.Children.Add(_createButton);
        createBar.Children.Add(_loadButton);

        var sourceBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        sourceBar.Children.Add(Caption("Source"));
        sourceBar.Children.Add(_sourceName);
        sourceBar.Children.Add(_addSourceButton);
        sourceBar.Children.Add(_removeSourceButton);
        sourceBar.Children.Add(_testButton);
        sourceBar.Children.Add(_coverageButton);
        sourceBar.Children.Add(_suggestButton);

        var acceptBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        acceptBar.Children.Add(Caption("Proposal"));
        acceptBar.Children.Add(_acceptButton);

        var settingBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        settingBar.Children.Add(Caption("Setting"));
        settingBar.Children.Add(_settingPath);
        settingBar.Children.Add(_settingValue);
        settingBar.Children.Add(_setButton);

        var verifyBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        verifyBar.Children.Add(_verifyButton);
        verifyBar.Children.Add(Caption("Archive"));
        verifyBar.Children.Add(_archive);
        verifyBar.Children.Add(_handoverButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(createBar);
        panel.Children.Add(_summary);
        panel.Children.Add(Caption("Steps the engine judges"));
        panel.Children.Add(_stepGrid);
        panel.Children.Add(sourceBar);
        panel.Children.Add(_definition);
        panel.Children.Add(Caption("Sources"));
        panel.Children.Add(_sources);
        panel.Children.Add(Caption("Coverage"));
        panel.Children.Add(_coverage);
        panel.Children.Add(Caption("Proposals - shown, never applied until accepted"));
        panel.Children.Add(_suggestionGrid);
        panel.Children.Add(acceptBar);
        panel.Children.Add(settingBar);
        panel.Children.Add(Caption("Validation"));
        panel.Children.Add(_validation);
        panel.Children.Add(Caption("Verification"));
        panel.Children.Add(_verificationGrid);
        panel.Children.Add(_handover);
        panel.Children.Add(verifyBar);
        panel.Children.Add(Caption("Warnings"));
        panel.Children.Add(_warnings);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Choose a country preset, or reload the environment already in place.");
    }

    public OgSetupWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string SummaryText => _summary.Text;

    public string SourcesText => _sources.Text;

    public string CoverageText => _coverage.Text;

    public string ValidationText => _validation.Text;

    public string HandoverText => _handover.Text;

    public string WarningsText => _warnings.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public bool Load()
    {
        _header.Text = "Settings · wellbore setup";

        var listed = _setup.CountriesAsync().GetAwaiter().GetResult();

        if (listed.Success && listed.Value is not null)
        {
            var chosen = _country.SelectedItem as OgCountryOption;

            _country.ItemsSource = listed.Value;

            if (chosen is not null)
            {
                _country.SelectedItem = listed.Value.FirstOrDefault(option =>
                    string.Equals(option.Code, chosen.Code, StringComparison.OrdinalIgnoreCase));
            }

            _country.SelectedIndex = _country.SelectedIndex < 0 && listed.Value.Count > 0 ? 0 : _country.SelectedIndex;
        }

        var loaded = _setup.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            Clear();
            _failure.Text = loaded.Message ?? "The environment could not be read.";
            ShowState(OgViewState.Failed, "The setup is not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> CreateAsync()
    {
        if (_country.SelectedItem is not OgCountryOption country)
        {
            LastFailed = true;
            _failure.Text = "Choose a country preset first; an environment starts from one.";
            return false;
        }

        var created = await _setup.CreateAsync(
            country.Code,
            string.IsNullOrWhiteSpace(_companyName.Text) ? null : _companyName.Text.Trim(),
            _application.Session.UserId).ConfigureAwait(true);

        return Settle(created, "Environment created from " + country.Name + ".");
    }

    public async Task<bool> AddSourceAsync()
    {
        var added = await _setup.AddSourceAsync(
            _sourceName.Text?.Trim() ?? string.Empty,
            _definition.Text ?? string.Empty,
            _application.Session.UserId).ConfigureAwait(true);

        return Settle(added, "Data source recorded.");
    }

    public async Task<bool> RemoveSourceAsync()
    {
        var removed = await _setup.RemoveSourceAsync(
            _sourceName.Text?.Trim() ?? string.Empty,
            _application.Session.UserId).ConfigureAwait(true);

        return Settle(removed, "Data source removed.");
    }

    public async Task<bool> TestAsync()
    {
        var tested = await _setup.TestSourceAsync(_sourceName.Text?.Trim() ?? string.Empty, null).ConfigureAwait(true);

        return Settle(tested, "Source tested; the result is on the step it belongs to.");
    }

    public async Task<bool> CoverageAsync()
    {
        var covered = await _setup.CoverageAsync(_sourceName.Text?.Trim() ?? string.Empty).ConfigureAwait(true);

        return Settle(covered, "Coverage computed - or reported as unknown with its reason.");
    }

    public async Task<bool> SuggestAsync()
    {
        var suggested = await _setup.SuggestAsync(_sourceName.Text?.Trim() ?? string.Empty).ConfigureAwait(true);

        return Settle(suggested, "Proposals shown; nothing has been applied.");
    }

    public async Task<bool> AcceptAsync()
    {
        if (_suggestionGrid.SelectedItem is not OgSetupSuggestion suggestion)
        {
            LastFailed = true;
            _failure.Text = "Select the proposal to accept; a proposal is never applied without one being chosen.";
            return false;
        }

        var accepted = await _setup.AcceptAsync(
            _sourceName.Text?.Trim() ?? string.Empty,
            suggestion.Area,
            suggestion.Where,
            suggestion.Value,
            suggestion.Target,
            _application.Session.UserId).ConfigureAwait(true);

        return Settle(accepted, "Proposal accepted and applied.");
    }

    public async Task<bool> SetAsync()
    {
        var written = await _setup.SetAsync(
            _settingPath.Text?.Trim() ?? string.Empty,
            _settingValue.Text,
            _application.Session.UserId).ConfigureAwait(true);

        return Settle(written, "Setting written; a blank value removes it and lets the preset show through.");
    }

    public async Task<bool> VerifyAsync()
    {
        var verified = await _setup.VerifyAsync().ConfigureAwait(true);

        return Settle(verified, "Verification run.");
    }

    public async Task<bool> HandoverAsync()
    {
        var handed = await _setup.HandoverAsync(
            _archive.Text?.Trim() ?? string.Empty,
            _application.Session.UserId).ConfigureAwait(true);

        if (!handed.Success || handed.Value is null)
        {
            LastFailed = true;
            _failure.Text = handed.Message ?? "Nothing was handed over.";
            _handover.Text = _failure.Text;
            ShowState(OgViewState.Failed, "The hand-over was refused.", handed.Reference);
            return false;
        }

        return Settle(handed, "Environment handed over.");
    }

    /// <summary>Applies a result, and re-reads the environment so a change takes effect without a restart.</summary>
    private bool Settle(OgResult<OgSetupWorkspace> result, string notice)
    {
        if (!result.Success || result.Value is null)
        {
            LastFailed = true;
            _failure.Text = result.Message ?? "The command was refused.";
            ShowState(OgViewState.Failed, "The command was refused.", result.Reference);
            return false;
        }

        LastFailed = false;
        Apply(result.Value);

        _failure.Text = string.Empty;

        // The wizard edits the file the application loads, so it asks the host to re-read it rather than leaving the change inert.
        var reloaded = _application.ReloadWellboreEnvironment();

        _notice.Text = notice +
                       (reloaded.Success
                           ? " The application re-read the environment."
                           : " The environment was written but the application could not re-read it: " + reloaded.Message);

        _application.AuditLog.Add("settings.wellbore.setup", "settings", result.Value.EnvironmentFile ?? "wellbore", "ok",
            _application.Session.UserId);

        return true;
    }

    private void Apply(OgSetupWorkspace workspace)
    {
        _workspace = workspace;

        if (string.IsNullOrWhiteSpace(_companyName.Text) && workspace.CompanyName is { } company)
        {
            _companyName.Text = company;
        }

        _summary.Text = workspace.SummaryText;
        _sources.Text = workspace.SourcesText;
        _coverage.Text = workspace.CoverageText;
        _validation.Text = workspace.ValidationText;
        _handover.Text = workspace.HandoverText;
        _warnings.Text = workspace.WarningsText;
        _refreshed.Text = workspace.ReadText;

        _stepGrid.ItemsSource = workspace.Steps.ToList();
        _suggestionGrid.ItemsSource = workspace.Suggestions.ToList();
        _verificationGrid.ItemsSource = workspace.Verification.ToList();

        _handoverButton.IsEnabled = workspace.CanHandOver;
        _createButton.IsEnabled = !workspace.Exists;
        _acceptButton.IsEnabled = workspace.Suggestions.Count > 0;

        ShowState(workspace.Exists ? OgViewState.Ready : OgViewState.Empty, workspace.SummaryText);
    }

    private void Clear()
    {
        _summary.Text = string.Empty;
        _sources.Text = string.Empty;
        _coverage.Text = string.Empty;
        _validation.Text = string.Empty;
        _handover.Text = string.Empty;
        _warnings.Text = string.Empty;
        _refreshed.Text = string.Empty;
        _stepGrid.ItemsSource = null;
        _suggestionGrid.ItemsSource = null;
        _verificationGrid.ItemsSource = null;
        _handoverButton.IsEnabled = false;
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
