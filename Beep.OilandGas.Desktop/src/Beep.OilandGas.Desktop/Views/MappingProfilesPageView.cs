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
/// INT-04: vendor and spreadsheet well data brought in by describing the mapping once - the profile list, the test panel with the
/// resulting model, and the write-back that proves its own round trip.
/// </summary>
/// <remarks>
/// The mapping is the tools' <c>MappedWellDataProvider</c> driven by a <c>MappingProfile</c>, so a CSV or an EDM export loads through
/// the same code (AC-1, AC-2). A required field the profile does not map fails the test with the field named (AC-3); a built-in is
/// read-only (AC-4); the written data is re-read through the same profile before the export is reported as done (AC-5); and the
/// workbook carries one sheet per entity (AC-6).
/// </remarks>
public sealed class MappingProfilesPageView : OgPageBase
{
    public const string Route = "mappingprofiles";

    private readonly OgApplication _application;
    private readonly IOgMappingProfiles _mapping;

    private readonly TextBlock _header;
    private readonly TextBlock _detail;
    private readonly TextBlock _summary;
    private readonly TextBlock _problems;
    private readonly TextBlock _warnings;
    private readonly TextBlock _model;
    private readonly TextBlock _writeBack;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _profileName;
    private readonly TextBox _sourcePath;
    private readonly TextBox _wellId;
    private readonly TextBox _folder;
    private readonly TextBox _json;
    private readonly ComboBox _kind;
    private readonly CheckBox _xlsx;
    private readonly CheckBox _overwrite;
    private readonly DataGrid _profileGrid;
    private readonly DataGrid _entityGrid;
    private readonly Button _listButton;
    private readonly Button _testButton;
    private readonly Button _saveButton;
    private readonly Button _writeButton;

    private IReadOnlyList<OgProfileSummary> _profiles = [];
    private OgMappingTest? _test;

    public MappingProfilesPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _mapping = application.MappingProfiles;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 4, 0, 4);

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.FontWeight = FontWeights.SemiBold;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _problems = Body(string.Empty);
        _problems.TextWrapping = TextWrapping.Wrap;
        _problems.Margin = new Thickness(0, 0, 0, 4);

        _warnings = Body(string.Empty);
        _warnings.TextWrapping = TextWrapping.Wrap;
        _warnings.Margin = new Thickness(0, 0, 0, 4);

        _model = Body(string.Empty);
        _model.TextWrapping = TextWrapping.Wrap;
        _model.Margin = new Thickness(0, 0, 0, 4);

        _writeBack = Body(string.Empty);
        _writeBack.TextWrapping = TextWrapping.Wrap;
        _writeBack.Margin = new Thickness(0, 4, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _profileName = Field("Profile", 150);
        _sourcePath = Field("File or folder", 320);
        _wellId = Field("Well id (optional)", 130);
        _folder = Field("Write-back folder", 300);

        _kind = new ComboBox { MinWidth = 140, Margin = new Thickness(0, 0, 8, 0) };
        _kind.ItemsSource = OgMappingRules.SourceKinds.ToList();
        _kind.SelectedIndex = 0;
        Named(_kind, "Source kind");

        _xlsx = new CheckBox { Content = "Excel workbook (one sheet per entity)", Margin = new Thickness(0, 0, 12, 0) };
        Named(_xlsx, "Excel workbook");

        _overwrite = new CheckBox { Content = "Confirm overwrite", Margin = new Thickness(0, 0, 12, 0) };
        Named(_overwrite, "Confirm overwrite");

        _json = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 90,
            MaxHeight = 150,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "The profile JSON to save under the name above - a built-in must be saved under another name",
        };

        Named(_json, "Profile JSON");

        _profileGrid = Grid("Profiles", ("Key", "Key", 130), ("Name", "Name", 200), ("System", "System", 90), ("Version", "Version", 70),
            ("Kind", "IsBuiltIn", 80), ("Entities", "Entities", 200), ("Fields", "FieldCount", 70),
            ("State", "Valid", 70));

        _profileGrid.SelectionChanged += (_, _) => ShowSelected();

        _entityGrid = Grid("Entities", ("Entity", "Entity", 120), ("Table", "Table", 140), ("Rows", "Rows", 70),
            ("Present", "Present", 80), ("Problems", "Problems", 320));

        _listButton = Button("List profiles", (_, _) => Load());
        _testButton = Button("Test profile", (_, _) => _ = TestAsync());
        _saveButton = Button("Save profile", (_, _) => _ = SaveAsync());
        _writeButton = Button("Write back", (_, _) => _ = WriteBackAsync());

        var profileBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        profileBar.Children.Add(Caption("Profile"));
        profileBar.Children.Add(_profileName);
        profileBar.Children.Add(_listButton);

        var sourceBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        sourceBar.Children.Add(Caption("Source"));
        sourceBar.Children.Add(_kind);
        sourceBar.Children.Add(_sourcePath);
        sourceBar.Children.Add(Caption("Well"));
        sourceBar.Children.Add(_wellId);
        sourceBar.Children.Add(_testButton);

        var writeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        writeBar.Children.Add(Caption("Write to"));
        writeBar.Children.Add(_folder);
        writeBar.Children.Add(_xlsx);
        writeBar.Children.Add(_overwrite);
        writeBar.Children.Add(_writeButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(profileBar);
        panel.Children.Add(Caption("Profiles"));
        panel.Children.Add(_profileGrid);
        panel.Children.Add(Caption("Selected profile"));
        panel.Children.Add(_detail);
        panel.Children.Add(sourceBar);
        panel.Children.Add(_summary);
        panel.Children.Add(Caption("Entities"));
        panel.Children.Add(_entityGrid);
        panel.Children.Add(_problems);
        panel.Children.Add(Caption("Warnings from the load"));
        panel.Children.Add(_warnings);
        panel.Children.Add(Caption("The model the profile produced"));
        panel.Children.Add(_model);
        panel.Children.Add(writeBar);
        panel.Children.Add(_writeBack);
        panel.Children.Add(Caption("Save a profile"));
        panel.Children.Add(_json);
        panel.Children.Add(_saveButton);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "List the profiles, then test one against a file.");
    }

    public IReadOnlyList<OgProfileSummary> Profiles => _profiles;

    public OgMappingTest? Test => _test;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string DetailText => _detail.Text;

    public string SummaryText => _summary.Text;

    public string ProblemsText => _problems.Text;

    public string WarningsText => _warnings.Text;

    public string ModelText => _model.Text;

    public string WriteBackText => _writeBack.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public bool Load()
    {
        _header.Text = "Data · mapping profiles";

        var listed = _mapping.ListAsync().GetAwaiter().GetResult();

        if (!listed.Success || listed.Value is null)
        {
            LastFailed = true;
            _profiles = [];
            Clear();
            _failure.Text = listed.Message ?? "The profiles could not be listed.";
            ShowState(OgViewState.Failed, "The profiles are not shown.", listed.Reference);
            return false;
        }

        LastFailed = false;
        _profiles = listed.Value;
        _profileGrid.ItemsSource = _profiles.ToList();
        _profileGrid.SelectedItem = _profiles.FirstOrDefault();
        ShowSelected();

        _refreshed.Text = _profiles.Count.ToString(CultureInfo.InvariantCulture) + " profile(s): " +
                          _profiles.Count(profile => profile.IsBuiltIn).ToString(CultureInfo.InvariantCulture) +
                          " built in, " +
                          _profiles.Count(profile => !profile.IsBuiltIn).ToString(CultureInfo.InvariantCulture) + " custom.";

        _failure.Text = string.Empty;
        ShowState(_profiles.Count == 0 ? OgViewState.Empty : OgViewState.Ready, _refreshed.Text);
        return true;
    }

    public async Task<bool> TestAsync()
    {
        var tested = await _mapping.TestAsync(
            ChosenProfileName(),
            _kind.SelectedItem as string ?? OgMappingRules.Csv,
            _sourcePath.Text?.Trim() ?? string.Empty,
            string.IsNullOrWhiteSpace(_wellId.Text) ? null : _wellId.Text.Trim())
            .ConfigureAwait(true);

        if (!tested.Success || tested.Value is null)
        {
            LastFailed = true;
            _test = null;
            _failure.Text = tested.Message ?? "The test was refused.";
            _entityGrid.ItemsSource = null;
            _problems.Text = string.Empty;
            _model.Text = string.Empty;
            _summary.Text = string.Empty;
            ShowState(OgViewState.Failed, "The test is refused.", tested.Reference);
            return false;
        }

        LastFailed = false;
        _test = tested.Value;

        _summary.Text = tested.Value.SummaryText;
        _entityGrid.ItemsSource = tested.Value.Entities.ToList();
        _problems.Text = tested.Value.ProblemsText;
        _warnings.Text = tested.Value.WarningsText;
        _model.Text = tested.Value.ModelText;

        _failure.Text = string.Empty;
        _notice.Text = tested.Value.Passed ? "Test passed." : "Test failed with its reason.";

        ShowState(tested.Value.Passed ? OgViewState.Ready : OgViewState.Failed,
            tested.Value.SummaryText, tested.Value.Passed ? null : OgMappingRules.CodeRoundTrip);

        return tested.Value.Passed;
    }

    public async Task<bool> SaveAsync()
    {
        var saved = await _mapping.SaveAsync(
            ChosenProfileName(),
            _json.Text ?? string.Empty,
            _application.Session.UserId).ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _failure.Text = saved.Message ?? "The profile was not saved.";
            return false;
        }

        LastFailed = false;

        _application.AuditLog.Add("interop.mapping.write", "interop", saved.Value.Name, "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = "Profile saved as '" + saved.Value.Name + "'.";

        return Load();
    }

    public async Task<bool> WriteBackAsync()
    {
        var written = await _mapping.WriteBackAsync(
            new OgWriteBackRequest(
                ChosenProfileName(),
                _kind.SelectedItem as string ?? OgMappingRules.Csv,
                _sourcePath.Text?.Trim() ?? string.Empty,
                string.IsNullOrWhiteSpace(_wellId.Text) ? null : _wellId.Text.Trim(),
                _folder.Text?.Trim() ?? string.Empty,
                _xlsx.IsChecked == true,
                _overwrite.IsChecked == true),
            _application.Session.UserId).ConfigureAwait(true);

        if (!written.Success || written.Value is null)
        {
            LastFailed = true;
            _writeBack.Text = written.Message ?? "Nothing was written.";
            _failure.Text = written.Message ?? "Nothing was written.";
            return false;
        }

        LastFailed = false;

        _application.AuditLog.Add("interop.mapping.export", "interop", written.Value.ProfileName, "ok",
            _application.Session.UserId);

        _writeBack.Text = written.Value.Describe() + Environment.NewLine + written.Value.DifferencesText;
        _failure.Text = string.Empty;
        _notice.Text = "Written and re-read through the same profile.";

        return true;
    }

    private void ShowSelected()
    {
        if (_profileGrid.SelectedItem is not OgProfileSummary profile)
        {
            _detail.Text = "No profile is selected.";
            return;
        }

        _profileName.Text = profile.Key;

        _detail.Text = profile.Describe() + Environment.NewLine +
                       "Entities: " + string.Join(", ", profile.Entities) + Environment.NewLine +
                       (profile.Path is null ? "Built into the application." : "File: " + profile.Path) + Environment.NewLine +
                       (profile.Issues.Count == 0
                           ? "The profile passes its own structural checks."
                           : string.Join(Environment.NewLine, profile.Issues.Select(issue => "✗ " + issue)));
    }

    private string ChosenProfileName() =>
        string.IsNullOrWhiteSpace(_profileName.Text)
            ? (_profileGrid.SelectedItem as OgProfileSummary)?.Key ?? string.Empty
            : _profileName.Text.Trim();

    private void Clear()
    {
        _detail.Text = string.Empty;
        _summary.Text = string.Empty;
        _problems.Text = string.Empty;
        _warnings.Text = string.Empty;
        _model.Text = string.Empty;
        _writeBack.Text = string.Empty;
        _refreshed.Text = string.Empty;
        _profileGrid.ItemsSource = null;
        _entityGrid.ItemsSource = null;
        _notice.Text = string.Empty;
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
