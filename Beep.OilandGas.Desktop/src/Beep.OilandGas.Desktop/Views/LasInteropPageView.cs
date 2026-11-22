using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.WellLogs;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>One labelled fact on the LAS/DLIS page.</summary>
public sealed record OgLasRow(string Field, string Value);

/// <summary>
/// INT-01: load LAS 2.0 / LAS 3.0 / DLIS with every warning kept, then draw the curves and export them again.
/// </summary>
/// <remarks>
/// The import is <b>session-only</b>: PPDM persistence is DAT-02, so the page says so rather than implying the
/// file was written to a well. Export is real — it writes a LAS file that reloads identically.
/// </remarks>
public sealed class LasInteropPageView : OgPageBase
{
    private readonly OgApplication _application;
    private readonly WellLogView _view = new();
    private readonly SkiaViewSurfaceHost _host;

    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly DataGrid _rows;
    private readonly TextBox _source = new() { MinWidth = 360, Margin = new Thickness(0, 0, 8, 0) };
    private readonly ComboBox _encoding = new() { MinWidth = 130, Margin = new Thickness(0, 0, 8, 0) };
    private readonly TextBox _maxSamples = new() { MinWidth = 70, Margin = new Thickness(0, 0, 8, 0) };
    private readonly ComboBox _version = new() { MinWidth = 80, Margin = new Thickness(0, 0, 8, 0) };
    private readonly TextBox _destination = new() { MinWidth = 340, Margin = new Thickness(0, 0, 8, 0) };
    private readonly CheckBox _overwrite = new() { Content = "Overwrite", Margin = new Thickness(0, 0, 8, 0) };

    private OgLasImport? _loaded;

    public LasInteropPageView(OgApplication application)
        : base(application, "las")
    {
        _application = application;
        _ = application.Wellbore;

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _rows = new DataGrid
        {
            IsTabStop = true, // REL-03 AC-1: a read-only grid must be reachable by Tab.
            AutoGenerateColumns = false,
            IsReadOnly = true,
            MaxHeight = 170,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Field", nameof(OgLasRow.Field), 130),
                Star("Value", nameof(OgLasRow.Value)),
            },
        };

        _host = new SkiaViewSurfaceHost
        {
            FailureReporter = application.Diagnostics,
            MinHeight = 240,
        };

        _encoding.ItemsSource = new[] { "(detect)", "utf-8", "windows-1252", "windows-1251" };
        _encoding.SelectedIndex = 0;

        _version.ItemsSource = new[] { OgLasWorkspace.Las20, OgLasWorkspace.Las30 };
        _version.SelectedIndex = 1;

        _destination.Text = Path.Combine(
            OgPaths.ExportsDirectory,
            $"log-{DateTime.Now:yyyyMMdd-HHmmss}.las");

        var loadRow = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        loadRow.Children.Add(Caption("File"));
        loadRow.Children.Add(_source);
        loadRow.Children.Add(Caption("Encoding"));
        loadRow.Children.Add(_encoding);
        loadRow.Children.Add(Caption("Max samples"));
        loadRow.Children.Add(_maxSamples);

        loadRow.Children.Add(Button("Example", (_, _) => CreateExample()));
        loadRow.Children.Add(Button("Preview", (_, _) => Preview()));
        loadRow.Children.Add(Button("Import", (_, _) => Import()));

        var exportRow = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        exportRow.Children.Add(Caption("Export as"));
        exportRow.Children.Add(_version);
        exportRow.Children.Add(Caption("To"));
        exportRow.Children.Add(_destination);
        exportRow.Children.Add(_overwrite);
        exportRow.Children.Add(Button("Export", (_, _) => Export()));

        var panel = new DockPanel();
        DockPanel.SetDock(loadRow, Dock.Top);
        panel.Children.Add(loadRow);
        DockPanel.SetDock(exportRow, Dock.Top);
        panel.Children.Add(exportRow);
        DockPanel.SetDock(_summary, Dock.Top);
        panel.Children.Add(_summary);
        DockPanel.SetDock(_notice, Dock.Top);
        panel.Children.Add(_notice);
        DockPanel.SetDock(_rows, Dock.Bottom);
        panel.Children.Add(_rows);
        panel.Children.Add(_host);

        Named(_source, "LAS or DLIS file");
        Named(_encoding, "Encoding");
        Named(_maxSamples, "Maximum samples");
        Named(_version, "Export version");
        Named(_destination, "Export destination");
        Named(_rows, "File facts");
        ShowContent(panel);
    }

    /// <summary>The facts of the last preview; the smoke run and tests read these.</summary>
    public IReadOnlyList<OgLasRow> Rows { get; private set; } = [];

    public OgLasPreview? PreviewResult { get; private set; }

    public IReadOnlyList<string> Warnings => PreviewResult?.Warnings ?? [];

    public int DlisFrames { get; private set; }

    public string SummaryText => _summary.Text;

    /// <summary>Where Export will write; the smoke run reads the file back from here.</summary>
    public string ExportDestination => _destination.Text;

    /// <summary>Writes the built-in example LAS to the export folder and previews it.</summary>
    public bool CreateExample()
    {
        try
        {
            Directory.CreateDirectory(OgPaths.ExportsDirectory);

            var path = Path.Combine(OgPaths.ExportsDirectory, "example-log.las");
            File.WriteAllText(path, OgLasWorkspace.ExampleLas);

            _source.Text = path;
            return Preview();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "writing the example LAS file",
                "no example was written",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The example file could not be written.", reference);
            return false;
        }
    }

    /// <summary>Reads the file and shows what it contains without changing what is displayed.</summary>
    public bool Preview()
    {
        try
        {
            var path = _source.Text.Trim();

            if (string.IsNullOrWhiteSpace(path))
            {
                _notice.Text = "Choose a LAS or DLIS file first.";
                return false;
            }

            if (!File.Exists(path))
            {
                _notice.Text = $"'{path}' does not exist.";
                return false;
            }

            if (path.EndsWith(".dlis", StringComparison.OrdinalIgnoreCase))
            {
                return PreviewDlis(path);
            }

            _loaded = OgLasWorkspace.Load(path, SelectedEncoding(), SelectedMaxSamples());
            PreviewResult = _loaded.Preview;
            DlisFrames = 0;

            FillRows(_loaded.Preview);
            _notice.Text = _loaded.Preview.Warnings.Count == 0
                ? string.Empty
                : string.Join("  ·  ", _loaded.Preview.Warnings);

            _summary.Text = _loaded.Preview.Describe();
            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "previewing a LAS file",
                "the file was not read",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The file could not be read.", reference);
            return false;
        }
    }

    /// <summary>Loads the file and draws its curves. Session-only: nothing is persisted (DAT-02).</summary>
    public bool Import()
    {
        if (!Preview() || _loaded is null)
        {
            return false;
        }

        if (_loaded.Preview.HasErrors)
        {
            ShowState(
                OgViewState.Failed,
                "The file was read with errors; nothing was imported.",
                "las-import-errors");
            return false;
        }

        return Draw(_loaded.Model);
    }

    /// <summary>Writes the loaded log as LAS. Returns false with the reason shown when it cannot.</summary>
    public bool Export()
    {
        try
        {
            if (_loaded is null && !Preview())
            {
                return false;
            }

            if (_loaded is null)
            {
                return false;
            }

            var version = _version.SelectedItem as string ?? OgLasWorkspace.Las20;
            var result = OgLasWorkspace.ExportFile(
                _loaded.Model,
                _loaded.Log,
                version,
                _destination.Text.Trim(),
                _overwrite.IsChecked == true);

            if (!result.Success)
            {
                _notice.Text = $"{result.Message}  ({result.Reference})";
                return false;
            }

            _summary.Text =
                $"Exported LAS {version} · {new FileInfo(result.Value!).Length / 1024.0:0.#} KB · {result.Value}";

            // A LAS 3.0 export is checked with the tools' own compliance checker before it is called done.
            if (version == OgLasWorkspace.Las30)
            {
                var findings = OgLasWorkspace.CheckLas3(File.ReadAllText(result.Value!));
                _notice.Text = findings.Count == 0
                    ? "LAS 3.0 check: compliant."
                    : "LAS 3.0 check: " + string.Join("  ·  ", findings);
            }
            else
            {
                _notice.Text = string.Empty;
            }

            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "exporting a LAS file",
                "no file was written",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The export failed.", reference);
            return false;
        }
    }

    private bool PreviewDlis(string path)
    {
        var dlis = OgLasWorkspace.LoadDlis(path);

        _loaded = new OgLasImport(
            dlis.Model,
            dlis.LogSets.FirstOrDefault(),
            new OgLasPreview(
                Standard: "DLIS",
                Encoding: null,
                IndexType: dlis.LogSets.FirstOrDefault()?.IndexType.ToString() ?? "(none)",
                IndexUnit: dlis.LogSets.FirstOrDefault()?.IndexUnit ?? string.Empty,
                Curves: dlis.LogSets.FirstOrDefault()?.Curves.Select(curve => curve.Mnemonic).ToList() ?? [],
                Rows: dlis.LogSets.FirstOrDefault()?.Index.Length ?? 0,
                Top: dlis.LogSets.FirstOrDefault()?.Index.FirstOrDefault() ?? 0,
                Bottom: dlis.LogSets.FirstOrDefault()?.Index.LastOrDefault() ?? 0,
                Decimated: false,
                DecimationNote: null,
                HasErrors: dlis.LogSets.Count == 0,
                Warnings: dlis.Warnings));

        PreviewResult = _loaded.Preview;
        DlisFrames = dlis.Frames;

        FillRows(_loaded.Preview);
        _notice.Text = dlis.Warnings.Count == 0 ? string.Empty : string.Join("  ·  ", dlis.Warnings);
        _summary.Text = $"DLIS · {dlis.Frames} frame(s) · {_loaded.Preview.Describe()}";

        return true;
    }

    private bool Draw(WellModel model)
    {
        try
        {
            _view.Document = WellLogDocuments.FromModel(model);
            _host.Surface = _view;
            _host.Redraw();

            _summary.Text = _loaded!.Preview.Describe() + "   ·   session only: persistence needs DAT-02";
            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "drawing an imported log",
                "the curves are not shown",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The imported log could not be drawn.", reference);
            return false;
        }
    }

    private void FillRows(OgLasPreview preview)
    {
        Rows =
        [
            new("Standard", preview.Standard),
            new("Encoding", preview.Encoding ?? "(not stated)"),
            new("Index", $"{preview.IndexType} in {preview.IndexUnit}"),
            new("Rows", preview.Rows.ToString()),
            new("Range", $"{preview.Top:0.###}–{preview.Bottom:0.###}"),
            new("Curves", preview.Curves.Count == 0 ? "(none)" : string.Join(", ", preview.Curves)),
            new("Decimated", preview.Decimated ? preview.DecimationNote ?? "yes" : "no"),
            new("Errors", preview.HasErrors ? "yes" : "no"),
        ];

        _rows.ItemsSource = Rows;
    }

    private string? SelectedEncoding() =>
        _encoding.SelectedItem is string value && value != "(detect)" ? value : null;

    private int? SelectedMaxSamples() =>
        int.TryParse(_maxSamples.Text.Trim(), out var value) && value > 1 ? value : null;

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
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };

    private static DataGridTextColumn Star(string header, string path) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
    };
}
