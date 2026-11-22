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
/// WEL-04: the status of the well in the shared context - the current status, the legal next statuses, the append-only
/// history and the change form.
/// </summary>
/// <remarks>
/// A change appends history and never edits it (AC-1). The codes on offer are R_WELL_STATUS's active codes (AC-2); an
/// illegal transition is refused with the legal set (AC-3). A change that is not acknowledged is retained and retried
/// unchanged (AC-4). The effective date is kept apart from the recorded date (AC-5), and every change carries the actor,
/// from, to and reason (AC-6).
/// </remarks>
public sealed class WellStatusPageView : OgPageBase
{
    public const string Route = "wellstatus";

    private readonly OgApplication _application;
    private readonly IOgWellStatus _status;

    private readonly TextBlock _context;
    private readonly TextBlock _notice;
    private readonly TextBlock _current;
    private readonly TextBlock _legal;
    private readonly TextBlock _detail;
    private readonly DataGrid _history;
    private readonly ComboBox _option;
    private readonly TextBox _effective;
    private readonly TextBox _reason;
    private readonly TextBox _note;

    private OgWellStatusView? _view;
    private string? _uwi;

    public WellStatusPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _status = application.WellStatus;

        _context = Body(string.Empty);
        _context.FontSize = 15;
        _context.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _current = Body(string.Empty);
        _current.TextWrapping = TextWrapping.Wrap;
        _current.Margin = new Thickness(0, 4, 0, 4);

        _legal = Body(string.Empty);
        _legal.TextWrapping = TextWrapping.Wrap;
        _legal.Margin = new Thickness(0, 0, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _history = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 110,
            MaxHeight = 200,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Status", nameof(OgWellStatusHistoryEntry.Status), 110),
                Text("From", nameof(OgWellStatusHistoryEntry.FromCode), 100),
                Text("Effective", nameof(OgWellStatusHistoryEntry.EffectiveDate), 110),
                Text("Recorded", nameof(OgWellStatusHistoryEntry.RecordedAt), 110),
                Text("Reason", nameof(OgWellStatusHistoryEntry.Reason), 110),
                Text("Actor", nameof(OgWellStatusHistoryEntry.Actor), 110),
                Text("Note", nameof(OgWellStatusHistoryEntry.Note), 220),
            },
        };

        Named(_history, "Status history");

        _option = new ComboBox { MinWidth = 240, Margin = new Thickness(0, 0, 8, 0) };
        Named(_option, "New status");

        _effective = Input("Effective date yyyy-MM-dd");
        _reason = Input("Reason code");
        _note = Input("Note");

        var change = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        change.Children.Add(Caption("Change to"));
        change.Children.Add(_option);
        change.Children.Add(Caption("Effective"));
        change.Children.Add(_effective);
        change.Children.Add(_reason);
        change.Children.Add(_note);
        change.Children.Add(Button("Change status", (_, _) => _ = ChangeAsync()));
        change.Children.Add(Button("Retry", (_, _) => _ = RetryAsync()));
        change.Children.Add(Button("Reload", (_, _) => OpenPending()));

        var panel = new StackPanel();
        panel.Children.Add(_context);
        panel.Children.Add(_notice);
        panel.Children.Add(_current);
        panel.Children.Add(_legal);
        panel.Children.Add(Caption("History"));
        panel.Children.Add(_history);
        panel.Children.Add(change);
        panel.Children.Add(_detail);

        ShowContent(panel);

        OpenPending();
    }

    /// <summary>The well whose status is shown.</summary>
    public string? WellId => _uwi;

    public OgWellStatusView? View => _view;

    public string NoticeText => _notice.Text;

    public string DetailText => _detail.Text;

    public string LegalText => _legal.Text;

    /// <summary>The change waiting for a retry, if any (AC-4).</summary>
    public OgWellStatusChange? Pending => _status.Pending;

    /// <summary>True when the last read failed; the page then shows the failure, not an empty history.</summary>
    public bool LastFailed { get; private set; }

    /// <summary>AC-2 state: R_WELL_STATUS has no active codes, so no status can be chosen.</summary>
    public bool ReferenceDataMissing => _view?.ReferenceDataMissing ?? false;

    /// <summary>Opens the well the header or the index selected.</summary>
    public bool OpenPending()
    {
        var well = _application.WellContext.Current;

        if (well is null)
        {
            _uwi = null;
            _view = null;
            _history.ItemsSource = null;
            _option.ItemsSource = null;
            _context.Text = "Well status";
            _current.Text = string.Empty;
            _legal.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = "Select a well to see and change its status.";
            ShowState(OgViewState.Empty, _notice.Text);
            return false;
        }

        return Open(well.Id);
    }

    /// <summary>Loads one well's status, its history and the codes on offer.</summary>
    public bool Open(string? uwi)
    {
        if (string.IsNullOrWhiteSpace(uwi))
        {
            return false;
        }

        _uwi = uwi.Trim();

        var loaded = _status.LoadAsync(_uwi).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _view = null;
            _history.ItemsSource = null;
            _option.ItemsSource = null;
            _context.Text = $"Well {_uwi}";
            _current.Text = string.Empty;
            _legal.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The well status could not be read.";
            ShowState(OgViewState.Failed, "The well status could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Show(loaded.Value);
        return true;
    }

    public async Task<bool> ChangeAsync()
    {
        if (_view is null || _uwi is null)
        {
            _notice.Text = "Select a well first.";
            return false;
        }

        if (_option.SelectedItem is not OgWellStatusOption choice)
        {
            _notice.Text = ReferenceDataMissing
                ? "No status code is recorded, so no status can be chosen."
                : "Choose the status to change to.";
            return false;
        }

        var change = new OgWellStatusChange(
            _uwi,
            _view.Current?.Status,
            choice.Status,
            Date(_effective.Text),
            Empty(_reason.Text),
            Empty(_note.Text),
            _application.Session.UserId);

        var changed = await _status.ChangeAsync(change).ConfigureAwait(true);

        return AfterWrite(changed, "well.status.change", _uwi);
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _status.RetryAsync().ConfigureAwait(true);

        return AfterWrite(retried, "well.status.retry", _uwi ?? "(no well)");
    }

    private void Show(OgWellStatusView view)
    {
        _view = view;

        _history.ItemsSource = view.History.ToList();
        _option.ItemsSource = view.Options.ToList();
        _option.SelectedIndex = view.Options.Count > 0 ? 0 : -1;

        _context.Text = $"Well {_uwi}";

        _current.Text = view.Current is { } current
            ? $"Current status: {current.Status.ToLowerInvariant()}" +
              (string.IsNullOrWhiteSpace(current.Label) ? string.Empty : $" ({current.Label})") +
              $" · since {current.Since?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "not recorded"}" +
              (string.IsNullOrWhiteSpace(current.Actor) ? string.Empty : $" · by {current.Actor}")
            : "No current status is recorded for this well.";

        _legal.Text = view.LegalText ?? "No further status is allowed from this one.";

        _notice.Text = view.ReferenceDataMissing
            ? "No status code is recorded in R_WELL_STATUS, so no status can be chosen."
            : view.History.Count == 0
                ? "No status history is recorded for this well."
                : $"{view.History.Count.ToString(CultureInfo.InvariantCulture)} status row(s); history is appended, never edited.";

        _detail.Text = string.Join(
            Environment.NewLine,
            new[]
            {
                _status.Pending is { } pending
                    ? $"A change to {pending.ToCode} was not acknowledged and is waiting to be retried."
                    : "No change is waiting to be retried.",
                view.Options.Count == 0
                    ? "The status picker is empty."
                    : "Codes on offer: " + string.Join(", ", view.Options.Select(option => option.Status)),
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

        ShowState(
            view.ReferenceDataMissing ? OgViewState.Empty : OgViewState.Ready,
            _notice.Text);
    }

    private bool AfterWrite(OgResult<OgWellStatusView> result, string verb, string subject)
    {
        _notice.Text = result.Success
            ? result.Message ?? "Saved."
            : result.Message ?? "The change was not applied.";

        if (result.Success)
        {
            _application.AuditLog.Add(verb, "well", subject, "ok", _application.Session.UserId);
        }

        if (result.Value is not null)
        {
            LastFailed = false;
            Show(result.Value);
            return result.Success;
        }

        LastFailed = !result.Success;
        ShowState(OgViewState.Failed, _notice.Text, result.Reference);
        return false;
    }

    private static DateTime? Date(string? text) =>
        DateTime.TryParse(text?.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value
            : null;

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static TextBox Input(string name)
    {
        var box = new TextBox
        {
            MinWidth = 120,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = name,
        };

        Named(box, name);
        return box;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
    };

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

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };
}
