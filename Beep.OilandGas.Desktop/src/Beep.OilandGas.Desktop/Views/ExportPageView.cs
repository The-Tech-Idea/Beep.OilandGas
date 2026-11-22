using System.Windows;
using System.Windows.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Export;
using TheTechIdea.OilGas.WellLogs;
using TheTechIdea.OilGas.WellSchematic;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// VSB-09: export a well visual as PDF, SVG or PNG through the tools' own exporter.
/// </summary>
/// <remarks>
/// The page plans first and exports second, so the page count is on screen before a large PDF is written.
/// The destination defaults under the per-user data root (ADR-013) and is shown in full, because an export
/// the person cannot find is an export that did not happen.
/// </remarks>
public sealed class ExportPageView : OgPageBase
{
    private readonly OgApplication _application;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;

    private readonly ComboBox _source = new() { MinWidth = 170, Margin = new Thickness(0, 0, 8, 0) };
    private readonly ComboBox _format = new() { MinWidth = 80, Margin = new Thickness(0, 0, 8, 0) };
    private readonly ComboBox _scale = new() { MinWidth = 90, Margin = new Thickness(0, 0, 8, 0) };
    private readonly ComboBox _dpi = new() { MinWidth = 80, Margin = new Thickness(0, 0, 8, 0) };
    private readonly CheckBox _georeferenced = new() { Content = "Georeferenced", IsChecked = true, Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBox _destination = new() { MinWidth = 420, Margin = new Thickness(0, 0, 8, 0) };

    public ExportPageView(OgApplication application)
        : base(application, "export")
    {
        _application = application;
        _ = application.Wellbore;

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _source.ItemsSource = new[] { "Wellbore", "Well log", "Well schematic", "Well map" };
        _source.SelectedIndex = 0;
        _format.ItemsSource = Enum.GetValues<OgExportFormat>();
        _format.SelectedIndex = 0;
        _scale.ItemsSource = new[] { "Fit", "1:100", "1:500" };
        _scale.SelectedIndex = 0;
        _dpi.ItemsSource = new[] { 96f, 192f, 300f };
        _dpi.SelectedIndex = 0;

        _destination.Text = Path.Combine(
            OgPaths.ExportsDirectory,
            $"wellbore-{DateTime.Now:yyyyMMdd-HHmmss}.pdf");

        _format.SelectionChanged += (_, _) => SyncExtension();
        _source.SelectionChanged += (_, _) => OnSourceChanged();

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        toolbar.Children.Add(Caption("Source"));
        toolbar.Children.Add(_source);
        toolbar.Children.Add(Caption("Format"));
        toolbar.Children.Add(_format);
        toolbar.Children.Add(Caption("Scale"));
        toolbar.Children.Add(_scale);
        toolbar.Children.Add(Caption("DPI"));
        toolbar.Children.Add(_dpi);
        toolbar.Children.Add(_georeferenced);

        var plan = new Button { Content = "Plan", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(0, 0, 8, 0) };
        plan.Click += (_, _) => ShowPlan();
        toolbar.Children.Add(plan);

        var export = new Button { Content = "Export", Padding = new Thickness(10, 3, 10, 3) };
        export.Click += (_, _) => RunExport();
        toolbar.Children.Add(export);

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(Caption("Destination"));
        panel.Children.Add(_destination);
        panel.Children.Add(_summary);
        panel.Children.Add(_notice);

        Named(_source, "Export source");
        Named(_format, "Export format");
        Named(_scale, "Export scale");
        Named(_dpi, "Export resolution");
        Named(_destination, "Export destination");
        Named(_georeferenced, "Georeferenced map");
        ShowContent(panel);

        Loaded += (_, _) => ShowPlan();
    }

    /// <summary>The last map export, when the map source was used; the smoke run reads it.</summary>
    public OgMapExportResult? LastMapExport { get; private set; }

    /// <summary>True when the selected source is the well map (which has its own georeferencing path).</summary>
    public bool IsMapSource => string.Equals(_source.SelectedItem as string, "Well map", StringComparison.Ordinal);

    /// <summary>The destination on screen; settable so a test or the smoke run can use a disposable path.</summary>
    public string Destination
    {
        get => _destination.Text;
        set => _destination.Text = value;
    }

    /// <summary>Selects a source as the person would; public so the smoke run can exercise the map path.</summary>
    public void SelectSource(string source)
    {
        _source.SelectedItem = source;
        OnSourceChanged();
    }

    /// <summary>Sets the georeferencing option as the person would.</summary>
    public void SetGeoreferenced(bool georeferenced) => _georeferenced.IsChecked = georeferenced;

    /// <summary>The last plan or export sentence, for tests and the smoke run.</summary>
    public string SummaryText => _summary.Text;

    public IReadOnlyList<string> LastWarnings { get; private set; } = [];

    /// <summary>The document the selected source exports.</summary>
    public WellboreDocument DocumentFor(string source) => source switch
    {
        "Well log" => WellLogTemplates.CreateTripleComboDemo(),
        "Well schematic" => WellSchematicView.Build(OgSchematicMapper.Demo()),
        _ => OgDocumentMapper.TripleComboDemo(),
    };

    /// <summary>Builds the request from the controls; public so the smoke run can reuse the page's defaults.</summary>
    public OgExportRequest BuildRequest()
    {
        var format = _format.SelectedItem is OgExportFormat chosen ? chosen : OgExportFormat.Pdf;

        return new OgExportRequest(
            format,
            _destination.Text,
            ScaleRatio: (_scale.SelectedItem as string) switch
            {
                "1:100" => 100f,
                "1:500" => 500f,
                _ => 0f,
            },
            Dpi: _dpi.SelectedItem is float dpi ? dpi : 96f,
            Title: _source.SelectedItem as string);
    }

    /// <summary>Paginates and reports the page count without writing (VSB-09 §5).</summary>
    public bool ShowPlan()
    {
        if (IsMapSource)
        {
            return ShowMapPlan();
        }

        try
        {
            var document = DocumentFor(_source.SelectedItem as string ?? "Wellbore");
            var planned = _application.Export.Plan(document, BuildRequest());

            if (!planned.Success || planned.Value is null)
            {
                _notice.Text = $"{planned.Message}  Reference: {planned.Reference}";
                return false;
            }

            _summary.Text =
                $"{planned.Value.Pages} page(s) · {planned.Value.Format.ToUpperInvariant()} · " +
                $"scale {planned.Value.ScaleRatio:0.#} · depth {planned.Value.Top:0.#}–{planned.Value.Bottom:0.#}";
            _notice.Text = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "planning the export",
                "no page count is shown",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The export could not be planned.", reference);
            return false;
        }
    }

    /// <summary>Writes the export and reports where it went.</summary>
    public bool RunExport()
    {
        if (IsMapSource)
        {
            return RunMapExport();
        }

        try
        {
            var document = DocumentFor(_source.SelectedItem as string ?? "Wellbore");
            var exported = _application.Export.Export(document, BuildRequest());

            if (!exported.Success || exported.Value is null)
            {
                _notice.Text = $"{exported.Message}  Reference: {exported.Reference}";
                return false;
            }

            LastWarnings = exported.Value.Warnings;

            _summary.Text =
                $"{exported.Value.Pages} page(s) · {exported.Value.Bytes / 1024.0:0.#} KB · {exported.Value.Path}";
            _notice.Text = LastWarnings.Count == 0 ? string.Empty : string.Join("  ·  ", LastWarnings);
            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "exporting the well view",
                "no file was produced",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The export failed.", reference);
            return false;
        }
    }

    private void OnSourceChanged()
    {
        _georeferenced.IsEnabled = IsMapSource;

        if (IsMapSource)
        {
            // A map is raster; PNG is the format a world file can accompany.
            _format.SelectedItem = OgExportFormat.Png;
            SyncExtension();
        }
    }

    /// <summary>Reports what the map export will produce, without writing (VSB-09 §5).</summary>
    public bool ShowMapPlan()
    {
        var map = OgMapExport.DemoMap();
        var missing = map.Wells
            .Where(well => string.IsNullOrWhiteSpace(well.EffectiveCrs))
            .Select(well => well.Title)
            .ToList();

        if (_georeferenced.IsChecked == true && missing.Count > 0)
        {
            _summary.Text = "The map cannot be georeferenced.";
            _notice.Text = $"No coordinate reference system for {string.Join(", ", missing)}. Name a CRS or untick Georeferenced.";
            return false;
        }

        _summary.Text = _georeferenced.IsChecked == true
            ? $"1 page(s) · PNG · georeferenced {map.Wells[0].EffectiveCrs} · {map.Wells.Count} well(s)"
            : $"1 page(s) · PNG · image only · {map.Wells.Count} well(s)";

        _notice.Text = _georeferenced.IsChecked == true
            ? "The world file (.pgw) and coordinate metadata (.geo.json) are written beside the image."
            : "No world file is written; the image carries no georeference.";

        return true;
    }

    /// <summary>Writes the map, with or without its georeference, and reports both files.</summary>
    public bool RunMapExport()
    {
        try
        {
            var destination = Path.ChangeExtension(_destination.Text, ".png");
            var exported = OgMapExport.Export(
                OgMapExport.DemoMap(),
                destination,
                width: 1_000,
                height: 800,
                georeferenced: _georeferenced.IsChecked == true,
                overwrite: true);

            if (!exported.Success || exported.Value is null)
            {
                _notice.Text = $"{exported.Message}  ({exported.Reference})";
                _summary.Text = "The map was not written.";
                return false;
            }

            LastMapExport = exported.Value;
            _summary.Text = exported.Value.Describe();
            _notice.Text = exported.Value.WorldFilePath is null
                ? $"Image only: {exported.Value.ImagePath}"
                : $"World file: {exported.Value.WorldFilePath}  ·  metadata: {exported.Value.MetadataPath}";

            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "exporting the well map",
                "no map image was written",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The map could not be exported.", reference);
            return false;
        }
    }

    private void SyncExtension()
    {
        var format = _format.SelectedItem is OgExportFormat chosen ? chosen : OgExportFormat.Pdf;
        var extension = format.ToString().ToLowerInvariant();
        var current = _destination.Text;

        if (string.IsNullOrWhiteSpace(current))
        {
            return;
        }

        var withoutExtension = Path.ChangeExtension(current, null);
        _destination.Text = withoutExtension + "." + extension;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 4, 0),
        FontSize = 11.5,
    };
}
