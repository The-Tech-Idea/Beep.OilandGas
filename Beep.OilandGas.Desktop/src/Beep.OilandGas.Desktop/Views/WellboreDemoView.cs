using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// VSB-03: loads a <c>WellboreDocument</c>, draws it through <see cref="SkiaViewSurfaceHost"/>, and lets
/// the person switch the track set, drop markers and annotations from a click, undo them, and read the
/// depth and element under the pointer.
/// </summary>
/// <remarks>
/// Until DAT-02 can seed the PPDM schema the page starts from the tools' demo documents and says so. The
/// editing calls go to <see cref="WellboreView"/> itself (<c>AddMarker</c>, <c>UndoMarker</c>,
/// <c>AddAnnotation</c>) — the page never edits the document behind the view's back.
/// </remarks>
public sealed class WellboreDemoView : OgPageBase
{
    private readonly OgApplication _application;
    private readonly WellboreView _view = new();
    private readonly SkiaViewSurfaceHost _host;
    private readonly TextBlock _summary;
    private readonly TextBlock _readout;
    private readonly TextBlock _notice;
    private Button _unitButton = null!;
    private TextBox _well = null!;
    private TextBlock _databaseNotice = null!;
    private OgTrackTemplate _template = OgTrackTemplate.TripleComboDemo;
    private EditMode _mode = EditMode.None;
    private int _sequence;

    public WellboreDemoView(OgApplication application)
        : base(application, "wellbore")
    {
        _application = application;

        // VSB-02: the environment must be loaded before any surface is constructed.
        _ = application.Wellbore;

        _host = new SkiaViewSurfaceHost
        {
            FailureReporter = application.Diagnostics,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinHeight = 300,
        };

        _host.PointerMoved += OnPointerMoved;
        _host.Clicked += OnClicked;

        _summary = Body(string.Empty);
        _readout = Body(application.Loc.Get("Wellbore.NoCursor"));
        _notice = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 8),
            FontSize = 11.5,
            Visibility = Visibility.Collapsed,
            Foreground = (System.Windows.Media.Brush?)Application.Current?.TryFindResource("BeepWarningBrush")
                         ?? System.Windows.Media.Brushes.DarkGoldenrod,
        };

        _databaseNotice = Body(string.Empty);
        _databaseNotice.TextWrapping = TextWrapping.Wrap;
        _databaseNotice.Margin = new Thickness(0, 0, 0, 6);

        var panel = new DockPanel();
        var toolbar = BuildToolbar();
        DockPanel.SetDock(toolbar, Dock.Top);
        panel.Children.Add(toolbar);
        DockPanel.SetDock(_databaseNotice, Dock.Top);
        panel.Children.Add(_databaseNotice);
        DockPanel.SetDock(_notice, Dock.Top);
        panel.Children.Add(_notice);
        DockPanel.SetDock(_summary, Dock.Top);
        panel.Children.Add(_summary);
        DockPanel.SetDock(_readout, Dock.Bottom);
        panel.Children.Add(_readout);
        panel.Children.Add(_host);

        ShowContent(panel);
        Loaded += (_, _) => LoadTrackTemplate();
    }

    /// <summary>How many frames the host has produced — the smoke run and tests read it.</summary>
    public int RenderCount => _host.RenderCount;

    public WellboreDocument? Document => _view.Document;

    private enum EditMode
    {
        None,
        Marker,
        Annotation,
    }

    private FrameworkElement BuildToolbar()
    {
        var bar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };

        bar.Children.Add(Caption("Well"));
        _well = new TextBox { MinWidth = 140, Margin = new Thickness(6, 0, 8, 0), ToolTip = "Well UWI" };
        Named(_well, "Well");
        bar.Children.Add(_well);
        bar.Children.Add(Button("Load well from database", (_, _) => LoadFromDatabase()));

        bar.Children.Add(Caption(_application.Loc.Get("Wellbore.Template")));

        var templates = new ComboBox
        {
            ItemsSource = OgDocumentMapper.Templates,
            SelectedItem = _template,
            MinWidth = 190,
            Margin = new Thickness(6, 0, 12, 0),
        };
        templates.SelectionChanged += (_, _) =>
        {
            if (templates.SelectedItem is OgTrackTemplate chosen)
            {
                _template = chosen;
                LoadTrackTemplate();
            }
        };
        Named(templates, "Track set");
        bar.Children.Add(templates);

        bar.Children.Add(Toggle(_application.Loc.Get("Wellbore.MarkerMode"), EditMode.Marker));
        bar.Children.Add(Toggle(_application.Loc.Get("Wellbore.AnnotationMode"), EditMode.Annotation));

        _unitButton = Named(Button(string.Empty, OnToggleUnit), "Toggle depth unit");
        bar.Children.Add(_unitButton);

        bar.Children.Add(Button(_application.Loc.Get("Wellbore.UndoMarker"), OnUndoMarker));
        bar.Children.Add(Button(_application.Loc.Get("Wellbore.RemoveAnnotation"), OnRemoveAnnotation));

        return bar;
    }

    private ToggleButton Toggle(string text, EditMode mode)
    {
        var button = new ToggleButton
        {
            Content = text,
            Padding = new Thickness(10, 4, 10, 4),
            Margin = new Thickness(0, 0, 8, 0),
        };

        button.Checked += (_, _) =>
        {
            _mode = mode;
            UncheckOthers(button);
        };

        button.Unchecked += (_, _) =>
        {
            if (_mode == mode)
            {
                _mode = EditMode.None;
            }
        };

        _toggles[mode] = button;
        return button;
    }

    private readonly Dictionary<EditMode, ToggleButton> _toggles = [];

    private void UncheckOthers(ToggleButton keep)
    {
        foreach (var other in _toggles.Values)
        {
            if (!ReferenceEquals(other, keep))
            {
                other.IsChecked = false;
            }
        }
    }

    private static Button Button(string text, RoutedEventHandler handler)
    {
        var button = new Button
        {
            Content = text,
            Padding = new Thickness(10, 4, 10, 4),
            Margin = new Thickness(0, 0, 8, 0),
        };

        button.Click += handler;
        return button;
    }

    /// <summary>The last well read from the database, or null while a template is shown.</summary>
    public OgWellboreView? DatabaseView { get; private set; }

    /// <summary>
    /// VSB-03 AC-1: draws the well named in the picker from its own records. The template stays on screen when the read is
    /// refused, and the reason is stated rather than replacing the canvas with an error inside it.
    /// </summary>
    public bool LoadFromDatabase()
    {
        var wellId = _well.Text?.Trim();

        if (string.IsNullOrWhiteSpace(wellId))
        {
            _databaseNotice.Text = "Enter a well to draw it from the database; the template stays on screen.";
            return false;
        }

        var loaded = _application.WellboreSource.LoadAsync(wellId).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            DatabaseView = null;
            _databaseNotice.Text = loaded.Message ?? "The well could not be drawn.";
            return false;
        }

        var view = loaded.Value;

        DatabaseView = view;
        _view.Document = view.Document;
        _host.Surface = _view;

        _summary.Text = view.WellName + " · " + view.RangeText + " · " + view.TrackText + " · read from the database";
        _databaseNotice.Text = view.WarningsText;
        UpdateUnitLabel();

        return _host.Redraw();
    }

    /// <summary>Loads the chosen track set and draws it.</summary>
    public bool LoadTrackTemplate()
    {
        try
        {
            var document = OgDocumentMapper.Create(_template);
            _view.Document = document;
            _host.Surface = _view;

            _summary.Text = Describe(OgDocumentMapper.Summarise(document));
            UpdateNotice(document);
            UpdateUnitLabel();

            return _host.Redraw();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "loading the wellbore document",
                "the well view was not drawn",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The wellbore document could not be drawn.", reference);
            return false;
        }
    }

    /// <summary>Adds a marker at the current cursor depth. Returns the marker, or null when there is no depth.</summary>
    public DepthMarker? AddMarkerAtCursor()
    {
        if (_view.View.CursorDepth is not { } depth)
        {
            return null;
        }

        _sequence++;
        var marker = _view.AddMarker($"M{_sequence}", depth);
        AfterEdit();
        return marker;
    }

    /// <summary>Adds a note annotation at the current cursor depth.</summary>
    public WellboreAnnotation? AddAnnotationAtCursor()
    {
        if (_view.View.CursorDepth is not { } depth)
        {
            return null;
        }

        _sequence++;
        var annotation = new WellboreAnnotation
        {
            Kind = AnnotationKind.Note,
            Depth = depth,
            Text = $"Note {_sequence}",
        };

        _view.AddAnnotation(annotation);
        AfterEdit();
        return annotation;
    }

    /// <summary>Undoes the last marker change through the view's own history.</summary>
    public bool UndoMarker()
    {
        var undone = _view.UndoMarker();
        AfterEdit();
        return undone;
    }

    /// <summary>Removes the most recent annotation.</summary>
    public bool RemoveLastAnnotation()
    {
        var last = _view.Document?.Annotations.LastOrDefault();
        if (last is null)
        {
            return false;
        }

        var removed = _view.RemoveAnnotation(last.Id);
        AfterEdit();
        return removed;
    }

    private void AfterEdit()
    {
        _host.Redraw();

        if (_view.Document is { } document)
        {
            _summary.Text = Describe(OgDocumentMapper.Summarise(document));
            UpdateNotice(document);
            UpdateUnitLabel();
        }
    }

    /// <summary>AC-5: the mapper's warnings are shown, and never block the view.</summary>
    private void UpdateNotice(WellboreDocument document)
    {
        var warnings = OgDocumentMapper.Warnings(document);

        _notice.Text = string.Join("  ·  ", warnings);
        _notice.Visibility = warnings.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>AC-6: the button names the unit it will switch to.</summary>
    private void UpdateUnitLabel()
    {
        var current = _view.Document?.Axis.Unit ?? DepthUnit.Meters;
        _unitButton.Content =
            $"{_application.Loc.Get("Wellbore.Unit")}: {DepthUnits.Symbol(current)} → {DepthUnits.Symbol(OgDocumentUnits.Toggle(current))}";
    }

    /// <summary>AC-6: switches the depth unit and keeps the visible window over the same interval.</summary>
    public bool ToggleUnit()
    {
        var current = _view.Document?.Axis.Unit ?? DepthUnit.Meters;

        if (!OgDocumentUnits.Switch(_view, OgDocumentUnits.Toggle(current)))
        {
            return false;
        }

        AfterEdit();
        return true;
    }

    private void OnToggleUnit(object sender, RoutedEventArgs e) => ToggleUnit();

    private string Describe(OgDocumentSummary summary) =>
        $"{summary.Name} · {summary.TrackCount} tracks · {summary.MarkerCount} markers · " +
        $"{_view.Document?.Annotations.Count ?? 0} annotations · " +
        $"{summary.Top:0.#}–{summary.Bottom:0.#} {summary.DepthUnit}" +
        (Document is { } document && document.Properties.TryGetValue("source", out var source)
            ? $"   |   source: {source}"
            : string.Empty);

    private void OnPointerMoved(object? sender, OgSurfacePointer pointer) =>
        _readout.Text = OgCursorReadout
            .At(_view, pointer.SurfacePoint.X, pointer.SurfacePoint.Y)
            .Describe();

    private void OnClicked(object? sender, OgSurfacePointer pointer)
    {
        switch (_mode)
        {
            case EditMode.Marker:
                AddMarkerAtCursor();
                break;
            case EditMode.Annotation:
                AddAnnotationAtCursor();
                break;
            case EditMode.None:
                // A plain click is the view's own selection; it still updates the readout.
                _readout.Text = OgCursorReadout
                    .At(_view, pointer.SurfacePoint.X, pointer.SurfacePoint.Y)
                    .Describe();
                break;
        }
    }

    private void OnUndoMarker(object sender, RoutedEventArgs e) => UndoMarker();

    private void OnRemoveAnnotation(object sender, RoutedEventArgs e) => RemoveLastAnnotation();

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 4, 0),
        FontSize = 11.5,
        Foreground = (System.Windows.Media.Brush?)Application.Current?.TryFindResource("BeepTextSecondaryBrush")
                     ?? System.Windows.Media.Brushes.Gray,
    };
}
