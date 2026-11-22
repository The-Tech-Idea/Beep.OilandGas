using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdea.OilGas.Wellbore.Loaders;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>One labelled fact on the WITSML page.</summary>
public sealed record OgWitsmlRow(string Field, string Value);

/// <summary>One survey station as the import table shows it.</summary>
public sealed record OgWitsmlStationRow(double Md, double Inclination, double Azimuth);

/// <summary>
/// INT-02: WITSML import, 1.x/2.1 export and store check.
/// </summary>
/// <remarks>
/// The store password lives in this window's lifetime only. It is passed to the client for one check and never
/// stored, audited or included in a message — the plan's DPAPI store does not exist yet, and inventing one
/// here would be a security decision made by accident.
/// </remarks>
public sealed class WitsmlInteropPageView : OgPageBase
{
    private readonly OgApplication _application;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly TextBlock _storeState;
    private readonly DataGrid _facts;
    private readonly DataGrid _stations;

    private readonly TextBox _source = new() { MinWidth = 350, Margin = new Thickness(0, 0, 8, 0) };
    private readonly TextBox _folder = new() { MinWidth = 330, Margin = new Thickness(0, 0, 8, 0) };
    private readonly TextBox _endpoint = new() { MinWidth = 280, Margin = new Thickness(0, 0, 8, 0) };
    private readonly TextBox _user = new() { MinWidth = 110, Margin = new Thickness(0, 0, 8, 0) };
    private readonly PasswordBox _password = new() { MinWidth = 120, Margin = new Thickness(0, 0, 8, 0) };

    // INT-02 §3, the Store tab: which entity of which wellbore, and what the pull or the 2.1 check said.
    private readonly TextBox _uidWell = new() { MinWidth = 100, Margin = new Thickness(0, 0, 8, 0) };
    private readonly TextBox _uidWellbore = new() { MinWidth = 100, Margin = new Thickness(0, 0, 8, 0) };
    private readonly TextBox _uidObject = new() { MinWidth = 100, Margin = new Thickness(0, 0, 8, 0) };
    private readonly ComboBox _storeEntity = new() { MinWidth = 150, Margin = new Thickness(0, 0, 8, 0) };
    private readonly TextBlock _pullState;
    private readonly TextBlock _conformance;

    private WellModel? _model;

    public WitsmlInteropPageView(OgApplication application)
        : base(application, "witsml")
    {
        _application = application;
        _ = application.Wellbore;

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);
        _storeState = Body(string.Empty);

        _pullState = Body(string.Empty);
        _pullState.TextWrapping = TextWrapping.Wrap;
        _pullState.Margin = new Thickness(0, 0, 0, 8);

        _conformance = Body(string.Empty);
        _conformance.TextWrapping = TextWrapping.Wrap;
        _conformance.Margin = new Thickness(0, 0, 0, 8);

        _facts = new DataGrid
        {
            IsTabStop = true, // REL-03 AC-1: a read-only grid must be reachable by Tab.
            AutoGenerateColumns = false,
            IsReadOnly = true,
            MaxHeight = 140,
            Margin = new Thickness(0, 6, 0, 6),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Field", nameof(OgWitsmlRow.Field), 120),
                Star("Value", nameof(OgWitsmlRow.Value)),
            },
        };

        _stations = new DataGrid
        {
            IsTabStop = true, // REL-03 AC-1: a read-only grid must be reachable by Tab.
            AutoGenerateColumns = false,
            IsReadOnly = true,
            MaxHeight = 160,
            Margin = new Thickness(0, 0, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("MD", nameof(OgWitsmlStationRow.Md), 90),
                Text("Inclination", nameof(OgWitsmlStationRow.Inclination), 100),
                Text("Azimuth", nameof(OgWitsmlStationRow.Azimuth), 90),
            },
        };

        _folder.Text = Path.Combine(OgPaths.ExportsDirectory, "witsml");
        _endpoint.Text = "https://witsml.example.test/Store";
        _storeEntity.ItemsSource = Enum.GetValues<OgWitsmlEntity>();
        _storeEntity.SelectedIndex = 2;

        var importRow = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        importRow.Children.Add(Caption("File"));
        importRow.Children.Add(_source);
        importRow.Children.Add(Button("Example", (_, _) => CreateExample()));
        importRow.Children.Add(Button("Preview", (_, _) => PreviewFile()));
        importRow.Children.Add(Button("Import", (_, _) => ImportFile()));

        var exportRow = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        exportRow.Children.Add(Caption("Export to"));
        exportRow.Children.Add(_folder);
        exportRow.Children.Add(Button("Export 1.x", (_, _) => ExportOneX()));
        exportRow.Children.Add(Button("Export 2.1", (_, _) => ExportTwoOne()));

        var storeRow = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        storeRow.Children.Add(Caption("Store"));
        storeRow.Children.Add(_endpoint);
        storeRow.Children.Add(Caption("User"));
        storeRow.Children.Add(_user);
        storeRow.Children.Add(Caption("Password"));
        storeRow.Children.Add(_password);
        storeRow.Children.Add(Button("Check store", async (_, _) => await CheckStoreAsync()));

        var pullRow = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        pullRow.Children.Add(Caption("Pull"));
        pullRow.Children.Add(_storeEntity);
        pullRow.Children.Add(Caption("Well"));
        pullRow.Children.Add(_uidWell);
        pullRow.Children.Add(Caption("Wellbore"));
        pullRow.Children.Add(_uidWellbore);
        pullRow.Children.Add(Caption("Object"));
        pullRow.Children.Add(_uidObject);
        pullRow.Children.Add(Button("Query store", async (_, _) => await PullAsync()));

        // The growing log needs a live subscription to be meaningful, so it is shown disabled with the reason rather than hidden.
        var growingRow = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        growingRow.Children.Add(Caption("Growing log"));
        growingRow.Children.Add(new TextBlock
        {
            Text = "Not offered: a growing log needs a live store subscription, and this build's client polls with GetFromStore.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = System.Windows.SystemColors.GrayTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        });

        var panel = new StackPanel();
        panel.Children.Add(importRow);
        panel.Children.Add(_summary);
        panel.Children.Add(_notice);
        panel.Children.Add(_facts);
        panel.Children.Add(_stations);
        panel.Children.Add(exportRow);
        panel.Children.Add(_conformance);
        panel.Children.Add(storeRow);
        panel.Children.Add(pullRow);
        panel.Children.Add(growingRow);
        panel.Children.Add(_storeState);
        panel.Children.Add(_pullState);

        Named(_facts, "WITSML facts");
        Named(_stations, "Survey stations");
        Named(_source, "WITSML file");
        Named(_folder, "Export folder");
        Named(_storeEntity, "Store entity");
        Named(_uidWell, "Well UID");
        Named(_uidWellbore, "Wellbore UID");
        Named(_uidObject, "Object UID");
        Named(_endpoint, "Store endpoint");
        Named(_user, "Store user");
        Named(_password, "Store password");
        ShowContent(panel);
    }

    public OgWitsmlImport? Preview { get; private set; }

    public IReadOnlyList<OgWitsmlStationRow> Stations { get; private set; } = [];

    public IReadOnlyList<OgWitsmlRow> Rows { get; private set; } = [];

    public OgWitsmlStoreCheck? StoreCheck { get; private set; }

    public IReadOnlyList<string> ExportedFiles { get; private set; } = [];

    public OgWitsmlPull? Pull { get; private set; }

    public IReadOnlyList<OgWitsmlConformance> Conformance { get; private set; } = [];

    public string SummaryText => _summary.Text;

    /// <summary>Where the example XML is written and exports go; the smoke run reads this.</summary>
    public string ExportFolder => _folder.Text;

    /// <summary>Writes the three example documents and previews the trajectory.</summary>
    public bool CreateExample()
    {
        try
        {
            Directory.CreateDirectory(OgPaths.ExportsDirectory);

            var trajectory = Path.Combine(OgPaths.ExportsDirectory, "witsml-trajectory.xml");
            File.WriteAllText(trajectory, OgWitsmlWorkspace.ExampleTrajectory);

            File.WriteAllText(
                Path.Combine(OgPaths.ExportsDirectory, "witsml-markers.xml"),
                OgWitsmlWorkspace.ExampleMarkers);

            File.WriteAllText(
                Path.Combine(OgPaths.ExportsDirectory, "witsml-logs.xml"),
                OgWitsmlWorkspace.ExampleLogs);

            _source.Text = trajectory;
            return PreviewFile();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "writing the WITSML example files",
                "no example was written",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The example files could not be written.", reference);
            return false;
        }
    }

    /// <summary>Reads the file and reports what it contributes, without changing the imported well.</summary>
    public bool PreviewFile()
    {
        try
        {
            var path = _source.Text.Trim();

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                _notice.Text = string.IsNullOrWhiteSpace(path)
                    ? "Choose a WITSML file first."
                    : $"'{path}' does not exist.";
                return false;
            }

            // INT-02's unsupported-version state: a document that declares a version this build does not read is refused naming it.
            var checkedNow = OgWitsmlWorkspace.LoadChecked(File.ReadAllText(path), Path.GetFileName(path));

            if (!checkedNow.Success || checkedNow.Value is null)
            {
                _notice.Text = checkedNow.Message ?? string.Empty;
                ShowState(OgViewState.Failed, "The WITSML document is not read.", checkedNow.Reference);
                return false;
            }

            Preview = checkedNow.Value;
            Show(Preview);
            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "previewing a WITSML file",
                "the file was not read",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The WITSML file could not be parsed.", reference);
            return false;
        }
    }

    /// <summary>Loads the file into this session and fills the station table.</summary>
    public bool ImportFile()
    {
        if (!PreviewFile() || Preview is null)
        {
            return false;
        }

        if (Preview.Stations == 0 && Preview.Markers == 0 && Preview.Logs == 0)
        {
            ShowState(OgViewState.Failed, "The file contributed no trajectory, markers or depth log.", "witsml-empty");
            return false;
        }

        try
        {
            _model = WitsmlReader.Load(File.ReadAllText(_source.Text.Trim())).Model;

            Stations = _model.Survey
                .Select(station => new OgWitsmlStationRow(
                    station.Md.In("m"),
                    station.Inclination,
                    station.Azimuth))
                .ToList();

            _stations.ItemsSource = Stations;
            _summary.Text = Preview.Describe() + "   ·   session only: persistence needs DAT-02";
            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "importing a WITSML file",
                "nothing was imported",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The WITSML file could not be imported.", reference);
            return false;
        }
    }

    /// <summary>Writes the loaded well as 1.x trajectory, markers and log XML.</summary>
    public bool ExportOneX()
    {
        if (!EnsureModel())
        {
            return false;
        }

        // The feature's "export folder not writable" state: the path is checked before a single file is attempted.
        var folder = OgWitsmlWorkspace.EnsureFolder(_folder.Text, _application.Diagnostics);

        if (!folder.Success || folder.Value is null)
        {
            _notice.Text = folder.Message ?? string.Empty;
            ShowState(OgViewState.Failed, "The export folder cannot be written.", folder.Reference);
            return false;
        }

        try
        {
            var written = new List<string>();

            foreach (var (name, content) in new[]
                     {
                         ("trajectory.xml", OgWitsmlWorkspace.WriteTrajectory(_model!)),
                         ("markers.xml", OgWitsmlWorkspace.WriteMarkers(_model!)),
                         ("logs.xml", OgWitsmlWorkspace.WriteLogs(_model!)),
                     })
            {
                if (!content.Success || content.Value is null)
                {
                    // A well may legitimately have no markers or logs; say which part was skipped.
                    _notice.Text = content.Message ?? string.Empty;
                    continue;
                }

                var path = Path.Combine(folder.Value, name);
                File.WriteAllText(path, content.Value);
                written.Add(path);
            }

            ExportedFiles = written;
            _summary.Text = $"Exported {written.Count} WITSML 1.x file(s) to {folder.Value}";
            _notice.Text = string.Empty;
            return written.Count > 0;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "exporting WITSML 1.x files",
                "no file was written",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The WITSML export failed.", reference);
            return false;
        }
    }

    /// <summary>Writes the 2.1 file set (Well, Wellbore, Trajectory, logs) into a sub-folder.</summary>
    public bool ExportTwoOne()
    {
        if (!EnsureModel())
        {
            return false;
        }

        var target = OgWitsmlWorkspace.EnsureFolder(Path.Combine(_folder.Text, "witsml21"), _application.Diagnostics);

        if (!target.Success || target.Value is null)
        {
            _notice.Text = target.Message ?? string.Empty;
            ShowState(OgViewState.Failed, "The export folder cannot be written.", target.Reference);
            return false;
        }

        try
        {
            var folder = target.Value;
            var files = OgWitsmlWorkspace.Export21(_model!, out var warnings);
            var written = new List<string>();

            foreach (var file in files)
            {
                var path = Path.Combine(folder, file.Name);
                File.WriteAllText(path, file.Content);
                written.Add(path);
            }

            ExportedFiles = written;

            // AC-2 as far as a build can go without the Energistics schemas: structure and cross-file references, stated as such.
            Conformance = OgWitsmlWorkspace.Conform21(files, _application.Diagnostics);

            var failed = Conformance.Where(item => !item.Conforms).ToList();

            _conformance.Text = "Structural checks (not schema validation - the Energistics XSDs are not shipped): " +
                                Conformance.Count(item => item.Conforms).ToString() + " of " +
                                Conformance.Count.ToString() + " file(s) conform" +
                                (failed.Count == 0
                                    ? "."
                                    : "." + Environment.NewLine + string.Join(Environment.NewLine,
                                        failed.Select(item => "✗ " + item.Describe())));

            _summary.Text = $"Exported {written.Count} WITSML 2.1 file(s) to {folder}";
            _notice.Text = warnings.Count == 0 ? string.Empty : string.Join("  ·  ", warnings);

            return written.Count > 0 && failed.Count == 0;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "exporting WITSML 2.1 files",
                "no file was written",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The WITSML 2.1 export failed.", reference);
            return false;
        }
    }

    /// <summary>Checks the store with the settings on screen. The password is used for this call only.</summary>
    public async Task<bool> CheckStoreAsync(HttpMessageHandler? handler = null)
    {
        try
        {
            var options = new WitsmlStoreOptions
            {
                Endpoint = new Uri(_endpoint.Text.Trim(), UriKind.Absolute),
                UserName = string.IsNullOrWhiteSpace(_user.Text) ? null : _user.Text.Trim(),
                Password = string.IsNullOrEmpty(_password.Password) ? null : _password.Password,
                Timeout = TimeSpan.FromSeconds(10),
                Retries = 0,
            };

            StoreCheck = await OgWitsmlWorkspace
                .CheckStoreAsync(options, _application.Diagnostics, handler)
                .ConfigureAwait(true);

            _storeState.Text = $"{StoreCheck.State}: {StoreCheck.Detail}";
            return StoreCheck.IsOk;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "checking the WITSML store",
                "the store was not checked",
                FailureSeverity.Error);

            StoreCheck = new OgWitsmlStoreCheck(
                OgWitsmlStoreState.Refused,
                _endpoint.Text,
                "The settings are not usable.",
                reference);

            _storeState.Text = $"Refused: {StoreCheck.Detail}  ({reference})";
            return false;
        }
    }

    private bool EnsureModel()
    {
        if (_model is not null)
        {
            return true;
        }

        _notice.Text = "Import a WITSML file before exporting.";
        return false;
    }

    /// <summary>
    /// Queries the store for one entity of one wellbore and reads what comes back into the same view a file import fills. The credentials are
    /// the ones on screen, used for this call only, and never placed in a sentence.
    /// </summary>
    public async Task<bool> PullAsync(HttpMessageHandler? handler = null)
    {
        OgWitsmlPull pull;

        try
        {
            var options = new WitsmlStoreOptions
            {
                Endpoint = new Uri(_endpoint.Text.Trim(), UriKind.Absolute),
                UserName = string.IsNullOrWhiteSpace(_user.Text) ? null : _user.Text.Trim(),
                Password = string.IsNullOrEmpty(_password.Password) ? null : _password.Password,
                Timeout = TimeSpan.FromSeconds(15),
                Retries = 0,
            };

            pull = await OgWitsmlWorkspace
                .PullAsync(options, _storeEntity.SelectedItem is OgWitsmlEntity entity ? entity : OgWitsmlEntity.Trajectory,
                    _uidWell.Text, _uidWellbore.Text, _uidObject.Text, _application.Diagnostics, handler)
                .ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is UriFormatException or ArgumentException)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex, "reading the WITSML store endpoint", "the pull was not attempted", FailureSeverity.Degraded);

            _pullState.Text = "The store endpoint is not a URL, so the pull was not attempted (reference " + reference + ").";
            ShowState(OgViewState.Failed, "The store endpoint is not usable.", reference);
            return false;
        }

        Pull = pull;

        _pullState.Text = pull.Describe();

        if (!pull.IsOk || pull.Import is null)
        {
            ShowState(OgViewState.Failed, "The store pull did not return a document.", pull.Reference);
            return false;
        }

        // The pull fills the same view as a file, so a person reads the counts and the station table either way.
        Preview = pull.Import;
        Show(pull.Import);

        _summary.Text = pull.Import.Describe() + "   ·   pulled from " + pull.Endpoint;

        return pull.Rows > 0;
    }

    private void Show(OgWitsmlImport preview)
    {
        Rows =
        [
            new("Source", preview.Source),
            new("Version", preview.Version),
            new("Stations", preview.Stations.ToString()),
            new("Markers", preview.Markers.ToString()),
            new("Depth logs", preview.Logs.ToString()),
            new("Partial", preview.IsPartial ? "yes" : "no"),
        ];

        _facts.ItemsSource = Rows;
        _summary.Text = preview.Describe();
        _notice.Text = preview.Warnings.Count == 0
            ? string.Empty
            : string.Join("  ·  ", preview.Warnings);
    }

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

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
    };

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path) { StringFormat = "{0:0.###}" },
        Width = new DataGridLength(width),
    };

    private static DataGridTextColumn Star(string header, string path) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
    };
}
