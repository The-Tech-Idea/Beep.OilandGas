using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.WellSchematic;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// VSB-08: the symbol gallery. It lists the catalog's symbols by category with a rendered preview, assigns a
/// symbol to a component on the schematic, and undoes that assignment.
/// </summary>
/// <remarks>
/// The schematic sits beside the gallery so an assignment is visible at once (AC-3) rather than described in
/// a message. The catalog is the tools'; a custom one replaces it only after it parses and contains symbols,
/// and the default is one explicit action away (AC-5, AC-6).
/// </remarks>
public sealed class SymbolGalleryPageView : OgPageBase
{
    private readonly OgApplication _application;
    private readonly OgSymbolCatalogState _catalog = new();
    private readonly OgLogEditHistory _history = new();

    private readonly DataGrid _grid;
    private readonly ComboBox _category = new() { MinWidth = 150, Margin = new Thickness(0, 0, 8, 0) };
    private readonly TextBox _search = new() { MinWidth = 160, Margin = new Thickness(0, 0, 8, 0) };
    private readonly ComboBox _equipment = new() { MinWidth = 170, Margin = new Thickness(0, 0, 8, 0) };
    private readonly Image _preview = new() { Width = 96, Height = 96, Stretch = Stretch.None };
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;

    private readonly SkiaViewSurfaceHost _host;
    private readonly WellSchematicView _view = new();
    private WellSchematicDocument _document = OgSchematicMapper.Demo();

    public SymbolGalleryPageView(OgApplication application)
        : base(application, "symbols")
    {
        _application = application;
        _ = application.Wellbore;

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _grid = new DataGrid
        {
            IsTabStop = true, // REL-03 AC-1: a read-only grid must be reachable by Tab.
            AutoGenerateColumns = false,
            IsReadOnly = true,
            MaxHeight = 220,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Id", nameof(OgSymbolRow.Id), 150),
                Text("Name", nameof(OgSymbolRow.Name), 190),
                Text("Category", nameof(OgSymbolRow.Category), 120),
                Text("Group", nameof(OgSymbolRow.Group), 90),
            },
        };
        _grid.SelectionChanged += (_, _) => ShowPreview();

        _host = new SkiaViewSurfaceHost
        {
            FailureReporter = application.Diagnostics,
            MinHeight = 240,
        };

        var panel = new DockPanel();

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        toolbar.Children.Add(Caption("Category"));
        toolbar.Children.Add(_category);
        toolbar.Children.Add(Caption("Search"));
        toolbar.Children.Add(_search);

        var apply = new Button { Content = "Apply filter", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(0, 0, 8, 0) };
        apply.Click += (_, _) => Reload(notice: null);
        toolbar.Children.Add(apply);

        toolbar.Children.Add(Caption("Component"));
        toolbar.Children.Add(_equipment);

        var assign = new Button { Content = "Assign", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(0, 0, 8, 0) };
        assign.Click += (_, _) => AssignSelected();
        toolbar.Children.Add(assign);

        var undo = new Button { Content = "Undo", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(0, 0, 8, 0) };
        undo.Click += (_, _) =>
        {
            _history.Undo();
            _view.CommitDocumentChanges();
            RedrawSchematic();
            RefreshSummary();
        };
        toolbar.Children.Add(undo);

        var restore = new Button { Content = "Restore default catalog", Padding = new Thickness(10, 3, 10, 3) };
        restore.Click += (_, _) =>
        {
            _catalog.RestoreDefault();
            Reload("The default catalog is active again.");
        };
        toolbar.Children.Add(restore);

        DockPanel.SetDock(toolbar, Dock.Top);
        panel.Children.Add(toolbar);
        DockPanel.SetDock(_summary, Dock.Top);
        panel.Children.Add(_summary);
        DockPanel.SetDock(_notice, Dock.Top);
        panel.Children.Add(_notice);

        var body = new DockPanel { Margin = new Thickness(0, 6, 0, 0) };
        var left = new StackPanel { Width = 560 };
        left.Children.Add(_preview);
        left.Children.Add(_grid);
        body.Children.Add(left);
        body.Children.Add(_host);
        DockPanel.SetDock(_preview, Dock.Top);

        panel.Children.Add(body);

        Named(_category, "Category");
        Named(_search, "Search symbols");
        Named(_equipment, "Component");
        Named(_grid, "Symbols");
        ShowContent(panel);

        Loaded += (_, _) =>
        {
            Reload(notice: null);
            RedrawSchematic();
        };
    }

    public IReadOnlyList<OgSymbolRow> Rows { get; private set; } = [];

    public OgSymbolSummary Summary { get; private set; } =
        new(0, 0, 0, "default", null);

    public IReadOnlyList<OgLogEdit> History => _history.Entries;

    public string? SelectedSymbolId => (_grid.SelectedItem as OgSymbolRow)?.Id;

    /// <summary>Reloads the gallery from the active catalog with the current filters.</summary>
    public void Reload(string? notice)
    {
        try
        {
            var category = _category.SelectedItem as string;
            var search = _search.Text;

            Rows = OgSymbolGallery.Rows(_catalog.Active, category, search);
            _grid.ItemsSource = Rows;

            Summary = OgSymbolGallery.Summarise(_catalog.Active, _catalog.Source, category, search, notice);
            _notice.Text = notice ?? string.Empty;

            var categories = new List<string> { "(all)" };
            categories.AddRange(OgSymbolGallery.Categories(_catalog.Active));
            _category.ItemsSource = categories;

            if (_category.SelectedIndex < 0)
            {
                _category.SelectedIndex = 0;
            }

            _equipment.ItemsSource = OgSymbolGallery.EquipmentChoices(_document);
            if (_equipment.SelectedIndex < 0 && _equipment.Items.Count > 0)
            {
                _equipment.SelectedIndex = 0;
            }

            RefreshSummary();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "reading the symbol catalog",
                "the gallery is not shown",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The symbol catalog could not be read.", reference);
        }
    }

    /// <summary>Loads a customer catalog from a JSON file. On failure the active catalog is untouched.</summary>
    public bool LoadCustom(string path)
    {
        try
        {
            var result = _catalog.LoadCustom(File.ReadAllText(path));

            if (!result.Success)
            {
                Reload(result.Message);
                return false;
            }

            Reload(result.Message);
            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "loading a custom symbol catalog",
                "the active catalog is unchanged",
                FailureSeverity.Error);

            Reload($"The custom catalog was rejected; the active catalog is unchanged. Reference: {reference}");
            return false;
        }
    }

    /// <summary>Assigns the selected symbol to the selected component and redraws (AC-3).</summary>
    public bool AssignSelected()
    {
        if (_grid.SelectedItem is not OgSymbolRow row)
        {
            _notice.Text = "Select a symbol first.";
            return false;
        }

        var equipment = OgSymbolGallery.FindEquipment(_document, _equipment.SelectedItem as string ?? string.Empty);
        if (equipment is null)
        {
            _notice.Text = "Select a component first.";
            return false;
        }

        var result = OgSymbolGallery.Assign(_document, equipment.Id, row.Id, _history);
        if (!result.Success)
        {
            _notice.Text = result.Message ?? "The symbol was not assigned.";
            return false;
        }

        _notice.Text = result.Value!.Describe();

        // The layout is cached against the view document's revision, so an in-place edit must be committed
        // before the redraw or the change stays invisible (the tools' own instruction).
        _view.CommitDocumentChanges();
        RedrawSchematic();
        RefreshSummary();
        return true;
    }

    private void ShowPreview()
    {
        if (_grid.SelectedItem is not OgSymbolRow row)
        {
            return;
        }

        var frame = OgSymbolGallery.Preview(_catalog.Active.Get(row.Id));
        if (frame is null)
        {
            return;
        }

        var bitmap = new WriteableBitmap(96, 96, 96, 96, PixelFormats.Pbgra32, null);
        bitmap.WritePixels(new Int32Rect(0, 0, 96, 96), frame, 96 * 4, 0);
        _preview.Source = bitmap;
    }

    private void RedrawSchematic()
    {
        try
        {
            _view.Schematic = _document;
            _host.Surface = _view;
            _host.Redraw();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "drawing the schematic after a symbol change",
                "the change is not visible",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The schematic could not be drawn.", reference);
        }
    }

    private void RefreshSummary()
    {
        _summary.Text =
            $"{Summary.Symbols} symbol(s) · {Summary.Categories} categories · {Summary.Shown} shown · " +
            $"catalog: {Summary.Source}   |   edits: {_history.Entries.Count}";
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 4, 0),
        FontSize = 11.5,
    };

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };
}
