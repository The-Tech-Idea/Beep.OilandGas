using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>One row in the relationship list.</summary>
public sealed record OgRelationRow(string Table, string Kind);

/// <summary>
/// DAT-03: the PPDM tree - categories, tables, a filter, and a summary of the selected table.
/// </summary>
/// <remarks>
/// The tree is built from the metadata registry and no row count is read until the person asks for one (AC-5).
/// A restricted table stays in the tree with a note rather than disappearing (AC-6), and a registry failure is a
/// failed state, never an empty tree (AC-4).
/// </remarks>
public sealed class PpdmTreePageView : OgPageBase
{
    public const string Route = "ppdmtree";

    /// <summary>Where "Open in workbench" would take the person once DAT-04 exists.</summary>
    public const string WorkbenchRoute = Views.TableWorkbenchPageView.Route;

    private readonly OgApplication _application;
    private readonly IOgPpdmTreeBuilder _builder;
    private readonly TextBox _search;
    private readonly TreeView _tree;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly DataGrid _relations;
    private readonly Button _count;
    private readonly Button _open;

    private OgTreeNode? _root;
    private OgTableSummary? _selected;

    public PpdmTreePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _builder = application.PpdmTree;

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _search = new TextBox
        {
            MinWidth = 260,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = "Filter by table name or subject area",
        };

        Named(_search, "Filter PPDM tables");

        _search.TextChanged += (_, _) => Populate();

        _count = Button("Count rows", async (_, _) => await CountSelectedAsync());
        _open = Button("Open in workbench", (_, _) => OpenSelected());

        _tree = new TreeView
        {
            Margin = new Thickness(0, 6, 8, 0),
            MinWidth = 280,
            IsTabStop = true,
        };

        Named(_tree, "PPDM tables");

        _tree.SelectedItemChanged += async (_, args) =>
        {
            if (args.NewValue is TreeViewItem { Tag: OgTreeNode node } && node.Kind == OgTreeNodeKind.Table)
            {
                await ShowSummaryAsync(node.TableName!);
            }
        };

        _relations = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MaxHeight = 180,
            Margin = new Thickness(0, 6, 0, 0),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            Columns =
            {
                Text("Related table", nameof(OgRelationRow.Table), 240),
                Text("Kind", nameof(OgRelationRow.Kind), 120),
            },
        };

        Named(_relations, "Related tables");

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        toolbar.Children.Add(Caption("Filter"));
        toolbar.Children.Add(_search);
        toolbar.Children.Add(_count);
        toolbar.Children.Add(_open);

        var right = new StackPanel();
        right.Children.Add(_summary);
        right.Children.Add(_notice);
        right.Children.Add(_relations);

        var body = new Grid { Margin = new Thickness(0, 4, 0, 0) };
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

        Grid.SetColumn(_tree, 0);
        Grid.SetColumn(right, 1);
        body.Children.Add(_tree);
        body.Children.Add(right);

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(body);

        ShowContent(panel);

        Loaded += async (_, _) => await LoadAsync();

        _ = LoadAsync();
    }

    /// <summary>The tree currently shown; the smoke run and tests read it.</summary>
    public OgTreeNode? Root => _root;

    public IReadOnlyList<OgTreeNode> VisibleTables { get; private set; } = [];

    public OgTableSummary? Selected => _selected;

    public string SummaryText => _summary.Text;

    public string NoticeText => _notice.Text;

    /// <summary>How many row counts have been read; AC-5 asserts this is 0 after the first open.</summary>
    public int CountReads => _builder is OgPpdmTreeBuilder concrete ? concrete.CountReads : -1;

    /// <summary>The table and filter the workbench would be opened with (AC-3).</summary>
    public OgTableOpenRequest? PendingOpen { get; private set; }

    /// <summary>Builds the tree from the registry and shows it.</summary>
    public async Task<bool> LoadAsync()
    {
        var built = await _builder.BuildAsync(new OgTreeOptions()).ConfigureAwait(true);

        if (!built.Success || built.Value is null)
        {
            _root = null;
            _tree.ItemsSource = null;
            VisibleTables = [];
            _summary.Text = "No tree.";
            _notice.Text = built.Message ?? "The PPDM table registry could not be read.";

            ShowState(OgViewState.Failed, "The PPDM tree could not be loaded.", built.Reference);
            return false;
        }

        _root = built.Value;
        Populate();

        _summary.Text =
            $"{_root.Count} table(s) in {_root.ChildNodes.Count} categor{( _root.ChildNodes.Count == 1 ? "y" : "ies")}.";

        _notice.Text =
            "Row counts are read only when you ask for one, so opening the tree stays cheap.";

        return true;
    }

    /// <summary>Applies the filter and rebuilds the visible nodes.</summary>
    public void Populate()
    {
        if (_root is null)
        {
            return;
        }

        var filtered = OgPpdmSearch.Filter(_root, _search.Text);
        var visibleRoot = filtered.Count == 1 ? filtered[0] : _root;

        VisibleTables = OgPpdmSearch.MatchingTables(visibleRoot, _search.Text);

        _tree.ItemsSource = visibleRoot.ChildNodes
            .Select(category => BuildItem(category))
            .ToList();

        if (_search.Text.Trim().Length > 0)
        {
            _notice.Text = $"{VisibleTables.Count} table(s) match '{_search.Text.Trim()}'.";
        }
    }

    /// <summary>Reads the selected table's row count - the one lazy read the tree allows.</summary>
    public async Task<bool> CountSelectedAsync()
    {
        if (_selected is null)
        {
            _notice.Text = "Select a table first; counts are read per table, on request.";
            return false;
        }

        var counted = await _builder.CountRowsAsync(_selected.TableName).ConfigureAwait(true);

        if (!counted.Success)
        {
            _notice.Text = counted.Message ?? "The row count could not be read.";
            return false;
        }

        _selected = _selected with { RowCount = counted.Value, RowCountPending = false };
        _summary.Text = $"{_selected.Describe()} · {counted.Value} row(s)";
        _relations.ItemsSource = _selected.RelatedTables
            .Select(table => new OgRelationRow(table, "references"))
            .ToList();

        return true;
    }

    /// <summary>Selects a table by name and opens it, as the tree would (deep link and smoke run).</summary>
    public async Task<bool> OpenTableAsync(string tableName, string? filter = null)
    {
        var summary = await _builder.SummarizeAsync(tableName).ConfigureAwait(true);

        if (!summary.Success || summary.Value is null)
        {
            _selected = null;
            _notice.Text = summary.Message ?? "No summary is available.";
            return false;
        }

        _selected = summary.Value;

        if (filter is not null)
        {
            _search.Text = filter;
        }

        return OpenSelected();
    }

    /// <summary>
    /// Opens the selected table in the workbench, carrying the filter that is active here (AC-3).
    /// </summary>
    public bool OpenSelected()
    {
        if (_selected is null)
        {
            _notice.Text = "Select a table to open in the workbench.";
            return false;
        }

        PendingOpen = new OgTableOpenRequest(_selected.TableName, _search.Text.Trim());

        // DAT-03 AC-3: the workbench is opened with this table and the filter that was active here.
        _application.Workbench.Pending = PendingOpen;
        var navigated = _application.Pages.Navigate(WorkbenchRoute);

        _notice.Text = navigated
            ? $"Opening {_selected.TableName} in the workbench" +
              (PendingOpen.Filter.Length == 0 ? "." : $" with filter '{PendingOpen.Filter}'.")
            : $"Workbench request recorded for {_selected.TableName}; the shell is not available to open it.";

        _application.AuditLog.Add(
            "ppdm.tree.open",
            "table",
            _selected.TableName,
            "recorded",
            $"filter='{PendingOpen.Filter}'; workbench route '{WorkbenchRoute}' not built");

        return true;
    }

    private async Task ShowSummaryAsync(string tableName)
    {
        var summary = await _builder.SummarizeAsync(tableName).ConfigureAwait(true);

        if (!summary.Success || summary.Value is null)
        {
            _selected = null;
            _relations.ItemsSource = null;
            _notice.Text = summary.Message ?? "No summary is available.";
            return;
        }

        _selected = summary.Value;
        _summary.Text = _selected.Describe() +
                        (_selected.RowCount is { } rows ? $" · {rows} row(s)" : " · count not read");

        _notice.Text = _selected.RowCountPending
            ? "Choose Count rows to read this table's row count."
            : "Row counts are not available for this connection.";

        _relations.ItemsSource = _selected.RelatedTables
            .Select(table => new OgRelationRow(table, "references"))
            .ToList();
    }

    private TreeViewItem BuildItem(OgTreeNode node)
    {
        var item = new TreeViewItem
        {
            Header = node.PolicyRestricted ? $"{node.Title} (restricted)" : node.Title,
            Tag = node,
            IsExpanded = node.Kind == OgTreeNodeKind.Category && node.ChildNodes.Count <= 12,
        };

        if (node.PolicyNote is not null)
        {
            item.ToolTip = node.PolicyNote;
        }

        foreach (var child in node.ChildNodes)
        {
            item.Items.Add(BuildItem(child));
        }

        return item;
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
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };
}

