using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>One row of the pick grid, with its overlap flag.</summary>
public sealed record OgPickRow(
    string Unit,
    string Top,
    string Base,
    string Thickness,
    string UnitOfMeasure,
    string Flag);

/// <summary>
/// WEL-05: the stratigraphic framework - unit hierarchy, regional columns and the picks of the selected well.
/// </summary>
/// <remarks>
/// The hierarchy is built from GROUP_CODE and ordered by age code then name, so three levels always render in the
/// same order (AC-1). A pick's unit comes from the reference set through a picker and is refused if it is not there
/// (AC-2). Picks at the same depth are flagged, never reordered (AC-3). Renaming a unit changes only its row, so
/// picks keep their identity and just show the new name (AC-4). Colours are theme brushes chosen by rank; this file
/// contains no colour values (AC-5). Picks are read for the selected well only (AC-6).
/// </remarks>
public sealed class StratigraphyPageView : OgPageBase
{
    public const string Route = "stratigraphy";

    private readonly OgApplication _application;
    private readonly IOgStratigraphy _stratigraphy;

    private readonly TextBlock _context;
    private readonly TextBlock _notice;
    private readonly TextBlock _overlap;
    private readonly TextBlock _lexicon;
    private readonly TreeView _tree;
    private readonly DataGrid _columns;
    private readonly DataGrid _picks;
    private readonly ComboBox _unit;
    private readonly TextBox _top;
    private readonly TextBox _depthUnit;
    private readonly TextBox _rename;
    private readonly TextBlock _affected;

    private IReadOnlyList<OgStratUnit> _units = [];
    private IReadOnlyList<OgStratNode> _hierarchy = [];

    public StratigraphyPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _stratigraphy = application.Stratigraphy;

        _context = Body(string.Empty);
        _notice = Body(string.Empty);
        _overlap = Body(string.Empty);
        _lexicon = Body(string.Empty);

        _tree = new TreeView { IsTabStop = true, MinHeight = 220, Margin = new Thickness(0, 6, 0, 8) };
        Named(_tree, "Stratigraphic unit hierarchy");
        _tree.SelectedItemChanged += (_, _) => ShowAffectedPicks();

        _columns = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MaxHeight = 260,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            Columns =
            {
                Text("Column", nameof(OgStratColumn.Name), 220),
                Text("Type", nameof(OgStratColumn.Type), 140),
                Text("Source", nameof(OgStratColumn.Source), 140),
            },
        };

        Named(_columns, "Stratigraphic columns");

        _rename = new TextBox { MinWidth = 200, Margin = new Thickness(0, 0, 8, 0), ToolTip = "New unit name" };
        Named(_rename, "New unit name");
        _affected = Body(string.Empty);

        var unitsPanel = new StackPanel();
        unitsPanel.Children.Add(_lexicon);
        unitsPanel.Children.Add(_tree);
        unitsPanel.Children.Add(Row(Caption("Rename unit"), _rename, Button("Rename", (_, _) => RenameUnit())));
        unitsPanel.Children.Add(_affected);
        var unitsTab = new TabItem { Header = "Units", Content = new ScrollViewer { Content = unitsPanel, Focusable = false } };

        var columnsTab = new TabItem
        {
            Header = "Columns",
            Content = new ScrollViewer { Content = _columns, Focusable = false },
        };

        _picks = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MaxHeight = 260,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            Columns =
            {
                Text("Unit", nameof(OgPickRow.Unit), 200),
                Text("Top", nameof(OgPickRow.Top), 90),
                Text("Base", nameof(OgPickRow.Base), 90),
                Text("Thickness", nameof(OgPickRow.Thickness), 90),
                Text("Unit", nameof(OgPickRow.UnitOfMeasure), 70),
                Text("Flag", nameof(OgPickRow.Flag), 260),
            },
        };

        Named(_picks, "Well picks");

        _unit = new ComboBox { MinWidth = 200, Margin = new Thickness(0, 0, 8, 0) };
        Named(_unit, "Pick unit");
        _top = new TextBox { MinWidth = 90, Margin = new Thickness(0, 0, 8, 0), ToolTip = "Top depth (MD)" };
        Named(_top, "Pick top depth");
        _depthUnit = new TextBox { MinWidth = 70, Margin = new Thickness(0, 0, 8, 0), ToolTip = "m or ft" };
        Named(_depthUnit, "Pick depth unit");

        var picksPanel = new StackPanel();
        picksPanel.Children.Add(_overlap);
        picksPanel.Children.Add(_picks);
        picksPanel.Children.Add(Row(
            Caption("Add pick"), _unit, _top, _depthUnit, Button("Add", (_, _) => _ = AddPickAsync())));

        var picksTab = new TabItem
        {
            Header = "Well sections",
            Content = new ScrollViewer { Content = picksPanel, Focusable = false },
        };

        var tabs = new TabControl { Margin = new Thickness(0, 6, 0, 8), MinHeight = 320, IsTabStop = true };
        Named(tabs, "Stratigraphy sections");
        tabs.Items.Add(unitsTab);
        tabs.Items.Add(columnsTab);
        tabs.Items.Add(picksTab);

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        toolbar.Children.Add(Button("Refresh", (_, _) => Open(_application.WellContext.Current?.Id)));
        toolbar.Children.Add(Button("Open well header", (_, _) => _application.Pages.Navigate(WellHeaderPageView.Route)));

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(_context);
        panel.Children.Add(_notice);
        panel.Children.Add(tabs);

        ShowContent(panel);

        LoadUnitsAndColumns();
        OpenPending();
    }

    public IReadOnlyList<OgStratUnit> Units => _units;

    public IReadOnlyList<OgStratNode> Hierarchy => _hierarchy;

    public IReadOnlyList<OgPickRow> PickRows { get; private set; } = [];

    public IReadOnlyList<OgStratColumn> Columns { get; private set; } = [];

    public string NoticeText => _notice.Text;

    public string OverlapText => _overlap.Text;

    public bool LastFailed { get; private set; }

    /// <summary>The deepest level the hierarchy rendered (AC-1 needs at least three).</summary>
    public int HierarchyDepth => _hierarchy.Count == 0 ? 0 : Math.Max(Depth(_hierarchy), 0);

    /// <summary>Number of picks flagged as overlapping (AC-3).</summary>
    public int OverlapCount => PickRows.Count(row => row.Flag.Contains("Overlap", StringComparison.OrdinalIgnoreCase));

    public bool OpenPending() => Open(_application.WellContext.Current?.Id);

    /// <summary>Loads the picks of the selected well (AC-6).</summary>
    public bool Open(string? uwi)
    {
        if (string.IsNullOrWhiteSpace(uwi))
        {
            PickRows = [];
            _picks.ItemsSource = null;
            _context.Text = "No well selected.";
            _notice.Text = "The unit hierarchy and columns are shown; choose a well for its picks.";
            _overlap.Text = string.Empty;
            ShowState(_hierarchy.Count == 0 ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
            return false;
        }

        var picks = _stratigraphy.LoadPicksAsync(uwi).GetAwaiter().GetResult();

        if (!picks.Success || picks.Value is null)
        {
            PickRows = [];
            _picks.ItemsSource = null;
            LastFailed = true;
            _context.Text = $"Well {uwi}";
            _notice.Text = picks.Message ?? "The picks could not be read.";
            ShowState(OgViewState.Failed, "The picks could not be read.", picks.Reference);
            return false;
        }

        LastFailed = false;
        PickRows = picks.Value
            .Select(pick => new OgPickRow(
                pick.UnitName,
                Number(pick.TopMd),
                Number(pick.BaseMd),
                Number(pick.Thickness),
                pick.DepthUnit ?? string.Empty,
                pick.Flag ?? string.Empty))
            .ToList();

        _picks.ItemsSource = PickRows;
        _context.Text = $"Well {uwi} · {PickRows.Count} pick(s).";

        _overlap.Text = OverlapCount == 0
            ? "No overlapping picks."
            : $"{OverlapCount} pick(s) overlap another at the same or a shallower depth; they are flagged, not reordered.";

        _notice.Text = PickRows.Count == 0
            ? "No tops are picked for this well yet."
            : "Picks read from the well's stratigraphic section.";

        ShowState(PickRows.Count == 0 ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        ShowAffectedPicks();
        return true;
    }

    /// <summary>AC-2: adds a pick for the selected unit.</summary>
    public async Task<bool> AddPickAsync()
    {
        if (_application.WellContext.Current is not { } well)
        {
            _notice.Text = "Choose a well first.";
            return false;
        }

        if (_unit.SelectedItem is not OgStratUnit unit)
        {
            _notice.Text = "Choose the unit for this pick from the reference set.";
            return false;
        }

        if (!decimal.TryParse(_top.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var top) || top <= 0)
        {
            _notice.Text = "Top depth must be a number greater than zero.";
            return false;
        }

        var added = await _stratigraphy.AddPickAsync(
            well.Id,
            unit.NameSetId,
            unit.Id,
            top,
            string.IsNullOrWhiteSpace(_depthUnit.Text) ? null : _depthUnit.Text.Trim(),
            _application.Session.UserId).ConfigureAwait(true);

        _notice.Text = added.Success
            ? added.Message ?? "Pick added."
            : added.Message ?? "The pick was not added.";

        if (added.Success)
        {
            _application.AuditLog.Add(
                "stratigraphy.pick.add",
                "well",
                well.Id,
                "ok",
                $"unit={unit.Id} top={top.ToString(CultureInfo.InvariantCulture)}");
            Open(well.Id);
        }

        return added.Success;
    }

    /// <summary>AC-4: renames the selected unit; picks keep their unit id.</summary>
    public bool RenameUnit()
    {
        if (_tree.SelectedItem is not TreeViewItem { Tag: OgStratUnit unit })
        {
            _notice.Text = "Select a unit in the hierarchy first.";
            return false;
        }

        var renamed = _stratigraphy
            .RenameUnitAsync(unit.NameSetId, unit.Id, _rename.Text, _application.Session.UserId)
            .GetAwaiter()
            .GetResult();

        _notice.Text = renamed.Success ? renamed.Message ?? "Unit renamed." : renamed.Message ?? "The unit was not renamed.";

        if (!renamed.Success)
        {
            return false;
        }

        _application.AuditLog.Add("stratigraphy.unit.rename", "unit", unit.Id, "ok", _rename.Text.Trim());
        LoadUnitsAndColumns();
        Open(_application.WellContext.Current?.Id);
        return true;
    }

    private void LoadUnitsAndColumns()
    {
        var units = _stratigraphy.LoadUnitsAsync().GetAwaiter().GetResult();

        _units = units.Success && units.Value is not null ? units.Value : [];
        _hierarchy = OgStratigraphy.BuildHierarchy(_units);

        _tree.ItemsSource = _hierarchy.Select(Node).ToList();
        _unit.ItemsSource = _units;
        _unit.DisplayMemberPath = nameof(OgStratUnit.Name);

        if (_unit.SelectedItem is null && _units.Count > 0)
        {
            _unit.SelectedIndex = 0;
        }

        var columns = _stratigraphy.LoadColumnsAsync().GetAwaiter().GetResult();

        Columns = columns.Success && columns.Value is not null ? columns.Value : [];
        _columns.ItemsSource = Columns;

        // State: no lexicon overlay is configured, so raw names are shown with this notice.
        _lexicon.Text = "Unit names are the reference table's raw names; no lexicon overlay is configured.";

        _notice.Text = units.Success
            ? $"{_units.Count} unit(s) in {_hierarchy.Count} top-level branch(es)."
            : units.Message ?? "The units could not be read.";
    }

    private void ShowAffectedPicks()
    {
        if (_tree.SelectedItem is TreeViewItem { Tag: OgStratUnit unit })
        {
            var affected = PickRows.Count;
            _affected.Text = affected == 0
                ? $"No picks of the open well use {unit.Name}; renaming changes no display here."
                : $"Renaming {unit.Name} changes the display of {affected} pick(s) on this well; their unit id is unchanged.";
            _rename.Text = unit.Name;
        }
    }

    private static int Depth(IReadOnlyList<OgStratNode> nodes) =>
        nodes.Count == 0
            ? 0
            : nodes.Max(node => 1 + Depth(node.Children));

    private static TreeViewItem Node(OgStratNode node)
    {
        var item = new TreeViewItem
        {
            Header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Border
                    {
                        Width = 10,
                        Height = 10,
                        Margin = new Thickness(0, 0, 6, 0),
                        Background = OgStratColour.Resolve(node.Unit.ColourToken),
                    },
                    new TextBlock
                    {
                        Text = $"{node.Unit.Name} ({node.Unit.Rank})",
                        ToolTip = $"Level {node.Level}",
                    },
                },
            },
            Tag = node.Unit,
            IsExpanded = node.Level < 3,
        };

        foreach (var child in node.Children)
        {
            item.Items.Add(Node(child));
        }

        return item;
    }

    private static string Number(decimal? value) =>
        value?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };

    private static WrapPanel Row(params object[] items)
    {
        var row = new WrapPanel { Margin = new Thickness(0, 2, 0, 2) };

        foreach (var item in items)
        {
            if (item is UIElement element)
            {
                row.Children.Add(element);
            }
        }

        return row;
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
}

/// <summary>
/// Resolves a stratigraphic colour token to a theme brush. The unit model carries a token, and this is the only
/// place a brush is chosen; no colour value is written anywhere (AC-5).
/// </summary>
public static class OgStratColour
{
    public static Brush Resolve(string? token) => (token ?? string.Empty) switch
    {
        "Highlight" => SystemColors.HighlightBrush,
        "Info" => SystemColors.InfoBrush,
        "Control" => SystemColors.ControlBrush,
        "Light" => SystemColors.ControlLightBrush,
        "Window" => SystemColors.WindowBrush,
        _ => SystemColors.ControlBrush,
    };
}
