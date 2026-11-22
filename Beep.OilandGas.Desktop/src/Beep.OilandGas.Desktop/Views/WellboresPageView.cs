using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// WEL-03: the wellbores of the well in the shared context - list, detail, parent/child tree, create and delete.
/// </summary>
/// <remarks>
/// The list is filtered by the selected well in SQL (AC-1). Creating a bore needs its well (AC-2) and a sidetrack
/// names its parent (AC-5). A depth shows its unit, and without one the survey/geometry actions are disabled with
/// the reason (AC-4). Deleting a bore that has linked surveys is refused with the count (AC-3).
/// </remarks>
public sealed class WellboresPageView : OgPageBase
{
    public const string Route = "wellbores";

    private readonly OgApplication _application;
    private readonly IOgWellbores _wellbores;

    private readonly TextBlock _context;
    private readonly TextBlock _notice;
    private readonly TextBlock _detail;
    private readonly TextBlock _survey;
    private readonly DataGrid _grid;
    private readonly TreeView _tree;
    private readonly TextBox _id;
    private readonly TextBox _name;
    private readonly ComboBox _kind;
    private readonly TextBox _parent;
    private readonly TextBox _status;
    private readonly TextBox _totalDepth;
    private readonly TextBox _depthUnit;

    private IReadOnlyList<OgWellboreSummary> _bores = [];
    private string? _uwi;

    public WellboresPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _wellbores = application.Wellbores;

        _context = Body(string.Empty);
        _notice = Body(string.Empty);
        _detail = Body(string.Empty);
        _survey = Body(string.Empty);

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MaxHeight = 220,
            Margin = new Thickness(0, 6, 8, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            Columns =
            {
                Text("Name", nameof(OgWellboreSummary.Name), 160),
                Text("Id", nameof(OgWellboreSummary.Id), 110),
                Text("Kind", nameof(OgWellboreSummary.Kind), 90),
                Text("From", nameof(OgWellboreSummary.ParentId), 100),
                Text("Status", nameof(OgWellboreSummary.Status), 100),
                Text("Spud", nameof(OgWellboreSummary.SpudDate), 90),
                Text("Total depth", nameof(OgWellboreSummary.DepthText), 130),
            },
        };

        Named(_grid, "Wellbores");
        _grid.SelectionChanged += (_, _) => ShowDetail();

        _tree = new TreeView { IsTabStop = true, MinHeight = 160, Margin = new Thickness(0, 6, 0, 8) };
        Named(_tree, "Wellbore parent and child tree");

        _id = Input("Wellbore id (required)");
        _name = Input("Name");
        _kind = new ComboBox { MinWidth = 120, Margin = new Thickness(0, 0, 8, 0) };
        _kind.ItemsSource = new[] { "ORIGINAL", "SIDETRACK", "REDRILL" };
        _kind.SelectedIndex = 0;
        Named(_kind, "Wellbore kind");

        _parent = Input("Parent wellbore (required for a sidetrack or re-drill)");
        _status = Input("Status");
        _totalDepth = Input("Total depth");
        _depthUnit = Input("Depth unit (m, ft)");

        Named(_id, "Wellbore id");
        Named(_name, "Wellbore name");
        Named(_parent, "Parent wellbore");
        Named(_status, "Wellbore status");
        Named(_totalDepth, "Total depth");
        Named(_depthUnit, "Depth unit");

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        toolbar.Children.Add(Button("Refresh", (_, _) => Open(_uwi)));
        toolbar.Children.Add(Button("Delete selected", (_, _) => _ = DeleteSelectedAsync()));
        toolbar.Children.Add(Button("Open well header", (_, _) => _application.Pages.Navigate(WellHeaderPageView.Route)));

        var create = new WrapPanel { Margin = new Thickness(0, 4, 0, 8) };
        create.Children.Add(Caption("New wellbore"));
        create.Children.Add(_id);
        create.Children.Add(_name);
        create.Children.Add(_kind);
        create.Children.Add(_parent);
        create.Children.Add(_status);
        create.Children.Add(Caption("TD"));
        create.Children.Add(_totalDepth);
        create.Children.Add(_depthUnit);
        create.Children.Add(Button("Create", (_, _) => _ = CreateAsync()));

        var columns = new Grid();
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var right = new StackPanel();
        right.Children.Add(_detail);
        right.Children.Add(_survey);
        right.Children.Add(_tree);

        Grid.SetColumn(_grid, 0);
        Grid.SetColumn(right, 1);
        columns.Children.Add(_grid);
        columns.Children.Add(right);

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(_context);
        panel.Children.Add(_notice);
        panel.Children.Add(columns);
        panel.Children.Add(create);

        ShowContent(panel);

        OpenPending();
    }

    /// <summary>The well whose bores are shown.</summary>
    public string? WellId => _uwi;

    public IReadOnlyList<OgWellboreSummary> Bores => _bores;

    public string NoticeText => _notice.Text;

    public string DetailText => _detail.Text;

    public string SurveyText => _survey.Text;

    /// <summary>True when the last read failed; the list is then empty <b>and</b> failed.</summary>
    public bool LastFailed { get; private set; }

    /// <summary>True when the well has no bores at all (the "a well must have at least one" state).</summary>
    public bool NeedsFirstWellbore => !LastFailed && _uwi is not null && _bores.Count == 0;

    /// <summary>Opens the well the header or index selected.</summary>
    public bool OpenPending()
    {
        var well = _application.WellContext.Current;

        return well is not null && Open(well.Id);
    }

    /// <summary>Loads the bores of one well.</summary>
    public bool Open(string? uwi)
    {
        if (string.IsNullOrWhiteSpace(uwi))
        {
            _uwi = null;
            _bores = [];
            _grid.ItemsSource = null;
            _tree.ItemsSource = null;
            _context.Text = "No well selected.";
            _notice.Text = "Choose a well in the well index first.";
            ShowState(OgViewState.Empty, "No well selected.");
            return false;
        }

        _uwi = uwi.Trim();
        var listed = _wellbores.ListAsync(_uwi).GetAwaiter().GetResult();

        if (!listed.Success || listed.Value is null)
        {
            _bores = [];
            _grid.ItemsSource = null;
            _tree.ItemsSource = null;
            LastFailed = true;
            _context.Text = $"Well {_uwi}";
            _notice.Text = listed.Message ?? "The wellbores could not be read.";
            ShowState(OgViewState.Failed, "The wellbores could not be read.", listed.Reference);
            return false;
        }

        LastFailed = false;
        _bores = listed.Value;
        _grid.ItemsSource = _bores;
        _context.Text = $"Well {_uwi} · {_bores.Count} wellbore(s)";

        _tree.ItemsSource = _bores
            .Where(bore => string.IsNullOrWhiteSpace(bore.ParentId))
            .Select(bore => Node(bore, _bores))
            .ToList();

        _notice.Text = NeedsFirstWellbore
            ? "A well must have at least one wellbore. Create the original bore below."
            : "Select a wellbore for its detail; a sidetrack must name its parent when it is created.";

        ShowState(_bores.Count == 0 ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        ShowDetail();
        return true;
    }

    /// <summary>Creates the bore described by the controls (AC-2, AC-4, AC-5).</summary>
    public async Task<bool> CreateAsync()
    {
        if (_uwi is null)
        {
            _notice.Text = "Choose a well first.";
            return false;
        }

        if (!decimal.TryParse(_totalDepth.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var depth)
            && !string.IsNullOrWhiteSpace(_totalDepth.Text))
        {
            _notice.Text = "Total depth must be a number.";
            return false;
        }

        var request = new OgWellboreRequest(
            _uwi,
            _id.Text.Trim(),
            _name.Text.Trim(),
            _parent.Text.Trim(),
            _kind.SelectedItem as string,
            _status.Text.Trim(),
            SpudDate: null,
            TotalDepth: string.IsNullOrWhiteSpace(_totalDepth.Text) ? null : depth,
            DepthUnit: _depthUnit.Text.Trim());

        var created = await _wellbores.CreateAsync(request, _application.Session.UserId).ConfigureAwait(true);

        _notice.Text = created.Success
            ? created.Message ?? "Wellbore created."
            : created.Message ?? "The wellbore was not created.";

        if (!created.Success)
        {
            return false;
        }

        _application.AuditLog.Add(
            "wellbore.create",
            "wellbore",
            $"{_uwi}/{request.Id}",
            "ok",
            $"kind={request.Kind ?? "ORIGINAL"} parent={request.ParentId ?? "(none)"}");

        Open(_uwi);
        return true;
    }

    /// <summary>Deletes the selected bore; the refusal names the survey count (AC-3).</summary>
    public async Task<bool> DeleteSelectedAsync()
    {
        if (_uwi is null || _grid.SelectedItem is not OgWellboreSummary bore)
        {
            _notice.Text = "Select a wellbore first.";
            return false;
        }

        var deleted = await _wellbores.DeleteAsync(_uwi, bore.Id, _application.Session.UserId).ConfigureAwait(true);

        _notice.Text = deleted.Success
            ? deleted.Message ?? "Wellbore deleted."
            : deleted.Message ?? "The wellbore was not deleted.";

        if (!deleted.Success)
        {
            return false;
        }

        _application.AuditLog.Add("wellbore.delete", "wellbore", $"{_uwi}/{bore.Id}", "ok", string.Empty);
        Open(_uwi);
        return true;
    }

    private void ShowDetail()
    {
        if (_grid.SelectedItem is not OgWellboreSummary bore)
        {
            _detail.Text = "Select a wellbore for its detail.";
            _survey.Text = string.Empty;
            return;
        }

        _detail.Text =
            $"{bore.Describe()} · id {bore.Id}" +
            (bore.SpudDate is { } spud ? $" · spud {spud:yyyy-MM-dd}" : string.Empty);

        _survey.Text = bore.SurveyEnabled
            ? "Survey stations and geometry are available for this bore."
            : "Total depth has no unit, so survey and geometry screens are disabled until one is recorded.";
    }

    private static TreeViewItem Node(OgWellboreSummary bore, IReadOnlyList<OgWellboreSummary> all)
    {
        var item = new TreeViewItem
        {
            Header = $"{bore.Name ?? bore.Id} ({bore.Kind})",
            Tag = bore,
            IsExpanded = true,
        };

        foreach (var child in all.Where(candidate =>
                     string.Equals(candidate.ParentId, bore.Id, StringComparison.OrdinalIgnoreCase)))
        {
            item.Items.Add(Node(child, all));
        }

        return item;
    }

    private static TextBox Input(string toolTip) =>
        new() { MinWidth = 120, Margin = new Thickness(0, 0, 8, 0), ToolTip = toolTip };

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
        Binding = new Binding(path)
        {
            StringFormat = path == nameof(OgWellboreSummary.SpudDate) ? "{0:yyyy-MM-dd}" : null,
        },
        Width = new DataGridLength(width),
    };
}
