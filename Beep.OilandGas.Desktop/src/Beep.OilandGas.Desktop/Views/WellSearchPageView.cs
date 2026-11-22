using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Core.Wells;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// WEL-01: the well index - free text over name, UWI and API, filters, server-side paging, saved searches, and a
/// row action that sets the shared well context.
/// </summary>
public sealed class WellSearchPageView : OgPageBase
{
    public const string Route = "wells";

    private readonly OgApplication _application;
    private readonly IOgWellSearch _search;
    private readonly OgSavedWellSearchStore _savedStore;

    private readonly TextBox _text;
    private readonly TextBox _field;
    private readonly TextBox _status;
    private readonly TextBox _type;
    private readonly TextBox _spudFrom;
    private readonly TextBox _spudTo;
    private readonly TextBox _saveName;
    private readonly ComboBox _saved;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly DataGrid _grid;

    private OgPage<OgWellSummary>? _page;
    private IReadOnlyList<OgSavedWellSearch> _savedSearches = [];

    public WellSearchPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _search = application.WellSearch;
        _savedStore = application.SavedWellSearches;

        _text = Input(240, "text: name, UWI or API");
        _field = Input(110, "field");
        _status = Input(110, "status");
        _type = Input(110, "well type");
        _spudFrom = Input(70, "spud year from");
        _spudTo = Input(70, "spud year to");
        _saveName = Input(140, "saved search name");

        Named(_text, "Search text");
        Named(_field, "Field filter");
        Named(_status, "Status filter");
        Named(_type, "Well type filter");
        Named(_spudFrom, "Spud year from");
        Named(_spudTo, "Spud year to");
        Named(_saveName, "Saved search name");

        _saved = new ComboBox { MinWidth = 160, Margin = new Thickness(0, 0, 8, 0) };
        Named(_saved, "Saved searches");

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MaxHeight = 380,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Well", nameof(OgWellSummary.Name), 180),
                Text("UWI", nameof(OgWellSummary.Id), 130),
                Text("API", nameof(OgWellSummary.ApiNumber), 110),
                Text("Field", nameof(OgWellSummary.Field), 120),
                Text("Status", nameof(OgWellSummary.Status), 100),
                Text("Type", nameof(OgWellSummary.WellType), 100),
                Text("Spud", nameof(OgWellSummary.SpudDate), 95),
                Text("Last activity", nameof(OgWellSummary.LastActivity), 105),
            },
        };

        Named(_grid, "Wells");

        _grid.MouseDoubleClick += (_, _) => OpenSelected();

        var filters = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        filters.Children.Add(Caption("Search"));
        filters.Children.Add(_text);
        filters.Children.Add(Button("Find", (_, _) => _ = SearchAsync()));
        filters.Children.Add(Button("Clear", (_, _) => ClearFilters()));
        filters.Children.Add(Caption("Field"));
        filters.Children.Add(_field);
        filters.Children.Add(Caption("Status"));
        filters.Children.Add(_status);
        filters.Children.Add(Caption("Type"));
        filters.Children.Add(_type);
        filters.Children.Add(Caption("Spud"));
        filters.Children.Add(_spudFrom);
        filters.Children.Add(_spudTo);

        var actions = new WrapPanel { Margin = new Thickness(0, 4, 0, 6) };
        actions.Children.Add(Button("Previous", (_, _) => Move(-1)));
        actions.Children.Add(Button("Next", (_, _) => Move(1)));
        actions.Children.Add(Button("Open selected", (_, _) => OpenSelected()));
        actions.Children.Add(Button("Compare wells", (_, _) => _application.Pages.Navigate(WellComparePageView.Route)));
        actions.Children.Add(Caption("Saved"));
        actions.Children.Add(_saved);
        actions.Children.Add(Button("Load", (_, _) => LoadSaved()));
        actions.Children.Add(_saveName);
        actions.Children.Add(Button("Save current", (_, _) => SaveCurrent()));
        actions.Children.Add(Button("Delete", (_, _) => DeleteSaved()));

        var panel = new StackPanel();
        panel.Children.Add(filters);
        panel.Children.Add(actions);
        panel.Children.Add(_summary);
        panel.Children.Add(_notice);
        panel.Children.Add(_grid);

        ShowContent(panel);

        RefreshSaved();
        _notice.Text = "Search by well name, UWI or API number, then open a row to set the well context.";
    }

    /// <summary>The last page of wells, or null before the first search.</summary>
    public OgPage<OgWellSummary>? Page => _page;

    public IReadOnlyList<OgWellSummary> Wells => _page?.Items ?? [];

    public IReadOnlyList<OgSavedWellSearch> SavedSearches => _savedSearches;

    public string SummaryText => _summary.Text;

    public string NoticeText => _notice.Text;

    /// <summary>True when the last search failed; the grid is then empty <b>and</b> in a failed state (AC-3).</summary>
    public bool LastSearchFailed { get; private set; }

    /// <summary>The query the page would run for its current controls.</summary>
    public OgWellQuery Query =>
        new(
            Text: Empty(_text.Text),
            Field: Empty(_field.Text),
            Status: Empty(_status.Text),
            WellType: Empty(_type.Text),
            SpudYearFrom: Year(_spudFrom.Text),
            SpudYearTo: Year(_spudTo.Text),
            Page: _page?.PageIndex ?? 0,
            PageSize: OgWellSearch.DefaultPageSize);

    /// <summary>Runs the search. A later search (or a well selection) cancels this one.</summary>
    public async Task<bool> SearchAsync()
    {
        var (token, version) = _application.WellContext.BeginLoad();
        var query = Query;
        var result = await _search.SearchAsync(query, token).ConfigureAwait(true);

        // AC-4: a result for a context that has moved on never lands on the page.
        if (!_application.WellContext.IsCurrent(version))
        {
            return false;
        }

        if (!result.Success || result.Value is null)
        {
            _page = null;
            _grid.ItemsSource = null;
            LastSearchFailed = true;
            _summary.Text = string.Empty;
            _notice.Text = result.Message ?? "The well index could not be read.";
            ShowState(OgViewState.Failed, "The well index could not be read.", result.Reference);
            return false;
        }

        LastSearchFailed = false;
        _page = result.Value;
        _grid.ItemsSource = Wells;

        var totalPageCount = _page.PageSize == 0
            ? 1
            : Math.Max(1, (int)Math.Ceiling(_page.Total / (double)_page.PageSize));

        _summary.Text =
            $"{_page.Total:N0} well(s)" +
            (query.HasFilters ? $" matching {query.Describe()}" : string.Empty) +
            $" · page {_page.PageIndex + 1} of {totalPageCount}";

        _notice.Text = _page.Total switch
        {
            0 when !query.HasFilters => "There are no wells in this company or field.",
            0 => "No wells match these filters. Clear them to see every well.",
            _ => $"Showing {Wells.Count} of {_page.Total:N0} well(s). Narrow the search, or page through the results.",
        };

        ShowState(Wells.Count == 0 ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    /// <summary>Resets every filter and searches again.</summary>
    public bool ClearFilters()
    {
        _text.Text = string.Empty;
        _field.Text = string.Empty;
        _status.Text = string.Empty;
        _type.Text = string.Empty;
        _spudFrom.Text = string.Empty;
        _spudTo.Text = string.Empty;
        _page = null;
        _ = SearchAsync();
        return true;
    }

    /// <summary>Moves a page, keeping every filter.</summary>
    public bool Move(int delta)
    {
        if (_page is null)
        {
            return false;
        }

        var target = _page.PageIndex + delta;

        if (target < 0 || (target > 0 && target * _page.PageSize >= _page.Total))
        {
            _notice.Text = "That is the end of the results in this direction.";
            return false;
        }

        _page = _page with { PageIndex = target };
        _ = SearchAsync();
        return true;
    }

    /// <summary>AC-4: selects the row's well into the shared context.</summary>
    public bool OpenSelected()
    {
        if (_grid.SelectedItem is not OgWellSummary well)
        {
            _notice.Text = "Select a well first.";
            return false;
        }

        _application.WellContext.Select(well);
        _application.AuditLog.Add("well.context", "well", well.Id, "ok", well.Describe());

        var navigated = _application.Pages.Navigate(WellHeaderPageView.Route);
        _notice.Text = navigated
            ? $"Opening {well.Describe()}"
            : $"Well context set to {well.Describe()}.";
        return true;
    }

    /// <summary>AC-5: loads the saved searches from the per-user store.</summary>
    public void RefreshSaved()
    {
        var file = _savedStore.Load();
        _savedSearches = file.Searches;
        _saved.ItemsSource = _savedSearches.Select(search => search.Name).ToList();

        if (!file.Readable)
        {
            _notice.Text = file.Problem ?? string.Empty;
        }
    }

    /// <summary>Saves the current controls under the typed name.</summary>
    public bool SaveCurrent()
    {
        var name = _saveName.Text.Trim();

        if (name.Length == 0)
        {
            _notice.Text = "Type a name for this search before saving it.";
            return false;
        }

        var query = Query;
        var file = _savedStore.Upsert(new OgSavedWellSearch(
            name,
            query.Text,
            query.Field,
            query.Status,
            query.WellType,
            query.SpudYearFrom,
            query.SpudYearTo));

        _savedSearches = file.Searches;
        _saved.ItemsSource = _savedSearches.Select(search => search.Name).ToList();
        _notice.Text = file.Problem ?? $"Saved search '{name}'.";
        return file.Readable;
    }

    /// <summary>Applies the selected saved search.</summary>
    public bool LoadSaved()
    {
        var name = _saved.SelectedItem as string;

        var search = _savedSearches.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase));

        if (search is null)
        {
            _notice.Text = "Choose a saved search first.";
            return false;
        }

        _text.Text = search.Text ?? string.Empty;
        _field.Text = search.Field ?? string.Empty;
        _status.Text = search.Status ?? string.Empty;
        _type.Text = search.WellType ?? string.Empty;
        _spudFrom.Text = search.SpudYearFrom?.ToString() ?? string.Empty;
        _spudTo.Text = search.SpudYearTo?.ToString() ?? string.Empty;

        _ = SearchAsync();
        return true;
    }

    /// <summary>Deletes the selected saved search.</summary>
    public bool DeleteSaved()
    {
        if (_saved.SelectedItem is not string name)
        {
            _notice.Text = "Choose a saved search first.";
            return false;
        }

        var file = _savedStore.Remove(name);
        _savedSearches = file.Searches;
        _saved.ItemsSource = _savedSearches.Select(search => search.Name).ToList();
        _notice.Text = file.Problem ?? $"Removed saved search '{name}'.";
        return file.Readable;
    }

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? Year(string? text) =>
        int.TryParse(text, out var year) && year is >= 1900 and <= 2200 ? year : null;

    private static TextBox Input(double width, string toolTip) =>
        new() { MinWidth = width, Margin = new Thickness(0, 0, 8, 0), ToolTip = toolTip };

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
        Binding = new Binding(path) { StringFormat = path is nameof(OgWellSummary.SpudDate) or nameof(OgWellSummary.LastActivity) ? "{0:yyyy-MM-dd}" : null },
        Width = new DataGridLength(width),
    };
}
