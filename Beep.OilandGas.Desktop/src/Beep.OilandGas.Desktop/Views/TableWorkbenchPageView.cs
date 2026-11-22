using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// DAT-04: one grid over any PPDM table - page, filter, sort, show the full row, export - without a screen per
/// table.
/// </summary>
/// <remarks>
/// Reads are server-side and bounded (page size is clamped by the gateway), a failed read is a failed state
/// rather than an empty grid, and the table/filter come from the tree through <see cref="OgWorkbenchNavigation"/>.
/// </remarks>
public sealed class TableWorkbenchPageView : OgPageBase
{
    public const string Route = "tableworkbench";

    private readonly OgApplication _application;
    private readonly IOgPpdmTableGateway _gateway;
    private readonly TextBlock _context;
    private readonly TextBlock _notice;
    private readonly TextBlock _footer;
    private readonly DataGrid _grid;
    private readonly ListBox _detail;
    private readonly TextBox _filter;
    private readonly TextBox _sort;
    private readonly TextBox _edit;

    private OgQuery _query = new();
    private OgPage<OgRow>? _page;

    public TableWorkbenchPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _gateway = application.TableGateway;

        _context = Body(string.Empty);
        _notice = Body(string.Empty);
        _footer = Body(string.Empty);

        _filter = new TextBox { MinWidth = 200, Margin = new Thickness(0, 0, 8, 0) };
        _sort = new TextBox { MinWidth = 160, Margin = new Thickness(0, 0, 8, 0) };

        Named(_filter, "Filter column and value");
        Named(_sort, "Sort column");

        _edit = new TextBox { MinWidth = 220, Margin = new Thickness(0, 0, 8, 0) };
        Named(_edit, "Edit column and value");

        _grid = new DataGrid
        {
            AutoGenerateColumns = true,
            IsReadOnly = true,
            IsTabStop = true,
            MaxHeight = 320,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
        };

        Named(_grid, "Table rows");

        // REL-03: a focusable list must be reachable by Tab.
        _detail = new ListBox { MinHeight = 120, MaxHeight = 200, Margin = new Thickness(0, 0, 0, 8), IsTabStop = true };

        Named(_detail, "Selected row");

        _grid.SelectionChanged += (_, _) => ShowDetail();

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        toolbar.Children.Add(Caption("Filter"));
        toolbar.Children.Add(_filter);
        toolbar.Children.Add(Button("Apply filter", (_, _) => ApplyFilter()));
        toolbar.Children.Add(Caption("Sort"));
        toolbar.Children.Add(_sort);
        toolbar.Children.Add(Button("Apply sort", (_, _) => ApplySort()));
        toolbar.Children.Add(Button("Previous", (_, _) => Move(-1)));
        toolbar.Children.Add(Button("Next", (_, _) => Move(1)));
        toolbar.Children.Add(Button("Refresh", (_, _) => Refresh()));
        toolbar.Children.Add(Caption("Edit"));
        toolbar.Children.Add(_edit);
        toolbar.Children.Add(Button("Save row", async (_, _) => await SaveSelectedAsync()));
        toolbar.Children.Add(Button("Delete row", async (_, _) => await DeleteSelectedAsync()));
        toolbar.Children.Add(Button("Export CSV", (_, _) => Export()));

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(_context);
        panel.Children.Add(_notice);
        panel.Children.Add(_grid);
        panel.Children.Add(_detail);
        panel.Children.Add(_footer);

        ShowContent(panel);

        Loaded += (_, _) => OpenPending();

        OpenPending();
    }

    /// <summary>The table open, or null before the first open.</summary>
    public string? TableName { get; private set; }

    public IReadOnlyList<OgRow> Rows => _page?.Items ?? [];

    public int Total => _page?.Total ?? 0;

    public int PageSize => _page?.PageSize ?? 0;

    public int PageIndex => _page?.PageIndex ?? 0;

    public IReadOnlyList<string> KeyColumns { get; private set; } = [];

    public OgRow? SelectedRow => _grid.SelectedItem as OgRow;

    public string ContextText => _context.Text;

    public string FooterText => _footer.Text;

    public string NoticeText => _notice.Text;

    public string? ExportedPath { get; private set; }

    /// <summary>The row-policy sentence shown while a predicate is in force (AC-6), or null.</summary>
    public string? PolicyNote { get; private set; }

    /// <summary>True when the last save or delete found the row changed by somebody else (AC-4).</summary>
    public bool LastConflict { get; private set; }

    /// <summary>True when the last read failed; the grid is then empty <b>and</b> in a failed state (AC-5).</summary>
    public bool LastReadFailed { get; private set; }

    /// <summary>Opens whatever the tree left in <see cref="OgWorkbenchNavigation"/>.</summary>
    public bool OpenPending()
    {
        var pending = _application.Workbench.Take();

        return pending is not null && Open(pending);
    }

    /// <summary>Opens a table with the filter the caller had active.</summary>
    public bool Open(OgTableOpenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        TableName = request.TableName;

        var filters = string.IsNullOrWhiteSpace(request.Filter)
            ? []
            : new List<OgFilter> { new(ParseFilterColumn(request.Filter), ParseFilterValue(request.Filter)) };

        _query = new OgQuery(
            Page: 0,
            PageSize: OgPpdmTableGateway.DefaultPageSize,
            Filters: filters);

        _filter.Text = request.Filter;

        var keys = _gateway.KeyColumnsAsync(request.TableName).GetAwaiter().GetResult();
        KeyColumns = keys.Success ? keys.Value ?? [] : [];

        return Refresh();
    }

    /// <summary>Re-reads the current page.</summary>
    public bool Refresh()
    {
        if (TableName is null)
        {
            _context.Text = "No table selected.";
            _notice.Text = "Open a table from the PPDM browser.";

            ShowState(OgViewState.Empty, "No table is open.");
            return false;
        }

        var read = _gateway.QueryAsync(TableName, _query).GetAwaiter().GetResult();

        if (!read.Success || read.Value is null)
        {
            _page = null;
            _grid.ItemsSource = null;
            _detail.ItemsSource = null;
            _context.Text = $"Table {TableName}";
            _notice.Text = read.Message ?? "The table could not be read.";

            // AC-5: a failed read is a failed state, never an empty grid.
            LastReadFailed = true;
            ShowState(OgViewState.Failed, "The table could not be read.", read.Reference);
            return false;
        }

        _page = read.Value;
        _grid.ItemsSource = Rows.Select(row => row.Values).ToList();

        _context.Text =
            $"{TableName} · {_page.Total} row(s) · key {(KeyColumns.Count == 0 ? "(none recorded)" : string.Join("+", KeyColumns))}" +
            (_query.Filters is { Count: > 0 } ? $" · filtered by {string.Join(", ", _query.Filters.Select(f => $"{f.Column}~{f.Value}"))}" : string.Empty);

        PolicyNote = (_gateway as OgPpdmTableGateway)?.PolicyNote;

        _notice.Text = string.Join(
            "  ·  ",
            new[] { _page.Message, PolicyNote }.Where(part => !string.IsNullOrWhiteSpace(part)));
        _footer.Text = PageSize == 0
            ? string.Empty
            : $"Page {_page.PageIndex + 1} of {Math.Max(1, (int)Math.Ceiling(_page.Total / (double)_page.PageSize))} · " +
              $"showing {_page.Items.Count} · {_page.Total} match";

        // DAT-08: every successful read is in the access audit, with the filter that produced it.
        _application.AccessAudit.Record(
            _application.Session.UserId,
            TableName,
            _query.Filters is { Count: > 0 }
                ? string.Join(" AND ", _query.Filters.Select(filter => $"{filter.Column}~{filter.Value}"))
                : "none",
            _page.Items.Count);

        ShowState(
            Rows.Count == 0 ? OgViewState.Empty : OgViewState.Ready,
            Rows.Count == 0 ? "This table has no rows." : string.Empty);
        return true;
    }

    public bool ApplyFilter()
    {
        var text = _filter.Text.Trim();

        _query = _query with
        {
            Page = 0,
            Filters = text.Length == 0
                ? []
                : [new OgFilter(ParseFilterColumn(text), ParseFilterValue(text))],
        };

        return Refresh();
    }

    public bool ApplySort()
    {
        var column = _sort.Text.Trim();

        _query = _query with
        {
            Page = 0,
            Sort = column.Length == 0 ? [] : [new OgSort(column)],
        };

        return Refresh();
    }

    public bool Move(int delta)
    {
        if (_page is null || TableName is null)
        {
            return false;
        }

        var target = _page.PageIndex + delta;

        if (target < 0 || (target > 0 && target * _page.PageSize >= _page.Total))
        {
            _notice.Text = "That is the end of the table in this direction.";
            return false;
        }

        _query = _query with { Page = target };
        return Refresh();
    }

    /// <summary>Writes the rows the active filter selects - not the whole table (AC-7).</summary>
    public bool Export()
    {
        if (TableName is null)
        {
            _notice.Text = "There is no table to export.";
            return false;
        }

        var exported = _gateway.ExportAsync(TableName, _query).GetAwaiter().GetResult();

        if (!exported.Success || exported.Value is null)
        {
            _notice.Text = exported.Message ?? "The export failed.";
            return false;
        }

        try
        {
            Directory.CreateDirectory(OgPaths.ExportsDirectory);

            var path = Path.Combine(
                OgPaths.ExportsDirectory,
                $"{TableName}-{DateTime.Now:yyyyMMdd-HHmmss}.csv");

            File.WriteAllBytes(path, exported.Value);
            ExportedPath = path;
            _notice.Text = $"Exported {Rows.Count} row(s) to {path}";

            _application.AuditLog.Add("table.export", "table", TableName, "ok", Path.GetFileName(path));
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "writing the workbench export",
                "no file was written",
                FailureSeverity.Error);

            _notice.Text = $"The export could not be written. Reference: {reference}";
            return false;
        }
    }

    /// <summary>
    /// Saves the selected row with one column changed. The update is guarded by the values the row was read
    /// with, so a row somebody else changed is reported as a conflict instead of overwritten (AC-3, AC-4).
    /// </summary>
    public async Task<bool> SaveSelectedAsync()
    {
        if (SelectedRow is not { } original || TableName is null)
        {
            _notice.Text = "Select a row first.";
            return false;
        }

        var text = _edit.Text.Trim();
        var space = text.IndexOf(' ');

        if (space <= 0 || space == text.Length - 1)
        {
            _notice.Text = "Type the edit as: COLUMN value (for example BAND Z).";
            return false;
        }

        var column = text[..space];
        var value = text[(space + 1)..].Trim();

        // DAT-08 AC-6: a governed write is refused while the audit chain is broken and unacknowledged.
        if (!_application.ChainGate.Allows(_application.AuditChain))
        {
            _notice.Text =
                "The audit chain is broken, so writes to governed tables are blocked until it is acknowledged " +
                "on the Governance page.";
            return false;
        }

        var values = new Dictionary<string, object?>(original.Values, StringComparer.OrdinalIgnoreCase)
        {
            [column] = value,
        };

        var saved = await _gateway.UpdateAsync(TableName, new OgRow(values), original).ConfigureAwait(true);

        if (!saved.Success)
        {
            LastConflict = saved.Reference == OgPpdmTableGateway.CodeConflict;
            _notice.Text = LastConflict
                ? "Somebody else changed this row; it was not overwritten. Refresh and try again."
                : saved.Message ?? "The row was not saved.";

            if (!LastConflict)
            {
                ShowState(OgViewState.Failed, "The row could not be saved.", saved.Reference);
            }

            return false;
        }

        LastConflict = false;
        _notice.Text = saved.Message ?? "Row saved.";

        // DAT-08: the new state of the row is appended as a version; the old versions are untouched.
        var key = KeyOf(original);
        _application.Versions.Record(TableName, key, values, _application.Session.UserId);
        _application.AuditChain.Append(
            "row.update",
            $"{TableName}/{key} {column} changed",
            _application.Session.UserId);

        _application.AuditLog.Add("table.update", "table", TableName, "ok", $"{column} changed; version recorded");

        return Refresh();
    }

    /// <summary>Deletes the selected row, guarded by the values it was read with (AC-4).</summary>
    public async Task<bool> DeleteSelectedAsync()
    {
        if (SelectedRow is not { } row || TableName is null)
        {
            _notice.Text = "Select a row first.";
            return false;
        }

        if (!_application.ChainGate.Allows(_application.AuditChain))
        {
            _notice.Text =
                "The audit chain is broken, so writes to governed tables are blocked until it is acknowledged " +
                "on the Governance page.";
            return false;
        }

        var deleted = await _gateway.DeleteAsync(TableName, row).ConfigureAwait(true);

        if (!deleted.Success)
        {
            LastConflict = deleted.Reference == OgPpdmTableGateway.CodeConflict;
            _notice.Text = LastConflict
                ? "Somebody else changed this row; it was not deleted. Refresh and try again."
                : deleted.Message ?? "The row was not deleted.";
            return false;
        }

        LastConflict = false;
        _notice.Text = deleted.Message ?? "Row deleted.";

        // DAT-08: the row's last state is kept as a version, then the removal is chained.
        var key = KeyOf(row);
        _application.Versions.Record(TableName, key, row.Values, _application.Session.UserId);
        _application.AuditChain.Append(
            "row.delete",
            $"{TableName}/{key} removed",
            _application.Session.UserId);

        _application.AuditLog.Add("table.delete", "table", TableName, "ok", "row removed; version recorded");

        return Refresh();
    }

    /// <summary>The row's key as one string, in the table's key order.</summary>
    private string KeyOf(OgRow row) =>
        string.Join(
            "+",
            KeyColumns.Select(column =>
                Convert.ToString(
                    row.Values.TryGetValue(column, out var value) ? value : null,
                    System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty));

    private void ShowDetail()
    {
        if (SelectedRow is not { } row)
        {
            _detail.ItemsSource = null;
            return;
        }

        _detail.ItemsSource = row.Values
            .Select(pair => new OgDetailRow(
                pair.Key,
                Convert.ToString(pair.Value, CultureInfo.InvariantCulture) ?? "(null)",
                KeyColumns.Contains(pair.Key, StringComparer.OrdinalIgnoreCase)))
            .ToList();
    }

    /// <summary>Reads "COLUMN value" into its two parts; a bare word filters on the first key column.</summary>
    private string ParseFilterColumn(string text)
    {
        var space = text.IndexOf(' ');
        return space > 0 ? text[..space] : KeyColumns.FirstOrDefault() ?? "ID";
    }

    private string ParseFilterValue(string text)
    {
        var space = text.IndexOf(' ');
        return space > 0 ? text[(space + 1)..].Trim() : text.Trim();
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

/// <summary>One line of the row detail pane: column, value, and whether it is part of the key.</summary>
public sealed record OgDetailRow(string Column, string Value, bool IsKey);
