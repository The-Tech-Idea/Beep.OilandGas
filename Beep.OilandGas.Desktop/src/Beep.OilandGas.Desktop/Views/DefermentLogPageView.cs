using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Production;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// PRD-03: the deferment log for one field - which wells are deferred, why, for how long and for how much volume.
/// </summary>
/// <remarks>
/// The status board itself belongs to WEL-04 (route <c>wellstatus</c>); this page is the deferment record that explains
/// the lost volume. A deferment cannot be saved without a reason code from the reference set (AC-1); overlapping
/// deferments are flagged and never merged (AC-2); a deferred volume carries its unit (AC-3); the totals by reason are
/// built from the same rows the list shows (AC-4); and an ongoing deferment is a different state from a closed one, with
/// its elapsed hours (AC-5). A failed read is the failure panel.
/// </remarks>
public sealed class DefermentLogPageView : OgPageBase
{
    public const string Route = "deferments";

    private readonly OgApplication _application;
    private readonly IOgWellStatusDeferment _board;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _groups;
    private readonly TextBlock _totals;
    private readonly TextBlock _reasons;
    private readonly TextBlock _detail;
    private readonly TextBox _day;
    private readonly DataGrid _boardGrid;
    private readonly DataGrid _deferments;

    private readonly ComboBox _wellPicker;
    private readonly TextBox _start;
    private readonly TextBox _end;
    private readonly ComboBox _reasonPicker;
    private readonly TextBox _volume;
    private readonly TextBox _unit;
    private readonly TextBox _comment;
    private readonly TextBox _reporter;

    private readonly TextBox _closeEnd;

    private OgDeferment? _selectedDeferment;

    public DefermentLogPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _board = application.WellStatusDeferment;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _groups = Body(string.Empty);
        _groups.TextWrapping = TextWrapping.Wrap;
        _groups.Margin = new Thickness(0, 4, 0, 4);

        _totals = Body(string.Empty);
        _totals.TextWrapping = TextWrapping.Wrap;
        _totals.Margin = new Thickness(0, 0, 0, 4);

        _reasons = Body(string.Empty);
        _reasons.TextWrapping = TextWrapping.Wrap;
        _reasons.Margin = new Thickness(0, 0, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _day = Field("Day yyyy-MM-dd", 120);
        _day.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        _boardGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 110,
            MaxHeight = 190,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Well", "NameText", 140),
                Text("Status", "StatusText", 120),
                Text("Since", "SinceText", 130),
                Text("Rate", "RateText", 120),
                Text("Deferment", "DefermentText", 200),
                Text("Reason", "ReasonText", 180),
            },
        };

        Named(_boardGrid, "Well status board");

        _deferments = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 170,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Well", "WellName", 130),
                Text("Reason", "ReasonText", 160),
                Text("Start", "StartText", 130),
                Text("End", "EndText", 130),
                Text("Duration", "DurationText", 130),
                Text("Volume", "VolumeText", 110),
                Text("Overlaps", "OverlapText", 130),
            },
        };

        Named(_deferments, "Deferred production log");
        _deferments.SelectionChanged += (_, _) => ShowSelectedDeferment();

        _wellPicker = new ComboBox { MinWidth = 170, Margin = new Thickness(0, 0, 8, 0) };
        Named(_wellPicker, "Well");

        _start = Field("Start yyyy-MM-dd HH:mm", 145);
        _end = Field("End yyyy-MM-dd HH:mm (blank if ongoing)", 190);

        _reasonPicker = new ComboBox { MinWidth = 140, Margin = new Thickness(0, 0, 8, 0) };
        Named(_reasonPicker, "Reason code");

        _volume = Field("Deferred volume", 100);
        _unit = Field("Volume unit", 80);
        _comment = Field("Comment", 180);
        _reporter = Field("Reporter", 110);
        _reporter.Text = _application.Session.UserId;

        _closeEnd = Field("Close at yyyy-MM-dd HH:mm", 160);

        var top = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        top.Children.Add(Caption("Day"));
        top.Children.Add(_day);
        top.Children.Add(Button("Refresh", (_, _) => Load()));

        var logger = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        logger.Children.Add(Caption("Deferment"));
        logger.Children.Add(_wellPicker);
        logger.Children.Add(_start);
        logger.Children.Add(_end);
        logger.Children.Add(_reasonPicker);
        logger.Children.Add(_volume);
        logger.Children.Add(_unit);
        logger.Children.Add(Button("Log deferment", (_, _) => _ = LogAsync()));

        var close = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        close.Children.Add(Caption("Close the selected deferment"));
        close.Children.Add(_closeEnd);
        close.Children.Add(Button("Close deferment", (_, _) => _ = CloseAsync()));

        var comments = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        comments.Children.Add(Caption("Notes"));
        comments.Children.Add(_comment);
        comments.Children.Add(_reporter);

        var panel = new StackPanel();
        panel.Children.Add(top);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(Caption("Status board"));
        panel.Children.Add(_groups);
        panel.Children.Add(_boardGrid);
        panel.Children.Add(Caption("Deferments"));
        panel.Children.Add(_deferments);
        panel.Children.Add(Caption("Totals by reason"));
        panel.Children.Add(_totals);
        panel.Children.Add(logger);
        panel.Children.Add(comments);
        panel.Children.Add(close);
        panel.Children.Add(Caption("Reason reference"));
        panel.Children.Add(_reasons);
        panel.Children.Add(_detail);

        ShowContent(panel);

        Load();
    }

    public OgWellStatusBoard? Board { get; private set; }

    public OgDeferment? SelectedDeferment => _selectedDeferment;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string GroupsText => _groups.Text;

    public string TotalsText => _totals.Text;

    public string ReasonsText => _reasons.Text;

    public string DetailText => _detail.Text;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _board
            .LoadAsync(session.FieldId, session.FieldName, Date(_day.Text))
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Board = null;
            _selectedDeferment = null;
            _boardGrid.ItemsSource = null;
            _deferments.ItemsSource = null;
            _wellPicker.ItemsSource = null;
            _reasonPicker.ItemsSource = null;
            _header.Text = "Deferments";
            _groups.Text = string.Empty;
            _totals.Text = string.Empty;
            _reasons.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The deferment log could not be read.";
            ShowState(OgViewState.Failed, "The deferment log could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Board = loaded.Value;

        var board = loaded.Value;

        _boardGrid.ItemsSource = board.Wells.ToList();
        _deferments.ItemsSource = board.Deferments.ToList();
        _wellPicker.ItemsSource = board.Wells.ToList();
        _wellPicker.SelectedIndex = board.Wells.Count > 0 ? 0 : -1;
        _reasonPicker.ItemsSource = board.Reasons.ToList();
        _reasonPicker.SelectedIndex = board.Reasons.Count > 0 ? 0 : -1;

        _header.Text = $"{board.FieldName} · {board.Summary}";

        _notice.Text = board.ReasonReferenceMissing
            ? board.ReasonsText
            : board.OverlapCount > 0
                ? $"{board.OverlapCount.ToString(CultureInfo.InvariantCulture)} deferment(s) overlap another " +
                  "deferment of the same well; they are listed and flagged, not merged."
                : "Statuses come from the PPDM status history; deferments carry the coded reason and the volume.";

        _groups.Text = board.GroupText;
        _totals.Text = board.TotalsText + " · total " + board.TotalText;
        _reasons.Text = board.ReasonsText;

        _detail.Text = string.Join(
            Environment.NewLine,
            board.Deferments
                .Take(5)
                .Select(deferment => deferment.Describe())
                .DefaultIfEmpty("No deferment is recorded for this field."));

        ShowState(board.WellCount == 0 ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    public async Task<bool> LogAsync()
    {
        var values = new OgDefermentValues(
            (_wellPicker.SelectedItem as OgWellCard)?.WellId,
            Date(_start.Text),
            Date(_end.Text),
            _reasonPicker.SelectedItem as string,
            decimal.TryParse(_volume.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var volume)
                ? volume
                : null,
            _unit.Text,
            _comment.Text,
            _reporter.Text);

        var logged = await _board
            .LogDefermentAsync(values, _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(logged, "production.deferment.log", values.WellId ?? "(no well)");
    }

    public async Task<bool> CloseAsync()
    {
        if (_selectedDeferment is not { } deferment)
        {
            _notice.Text = "Select a deferment in the log first.";
            return false;
        }

        if (Date(_closeEnd.Text) is not { } end)
        {
            _notice.Text = "Enter the date and time the deferment ended.";
            return false;
        }

        var closed = await _board
            .CloseDefermentAsync(deferment.Id, end, _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(closed, "production.deferment.close", deferment.Id);
    }

    private void ShowSelectedDeferment()
    {
        if (_deferments.SelectedItem is not OgDeferment deferment)
        {
            return;
        }

        _selectedDeferment = deferment;
        _closeEnd.Text = (deferment.End ?? DateTime.Today).ToString(
            "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

        _detail.Text = deferment.Describe() +
                       (deferment.Overlaps
                           ? Environment.NewLine + "This deferment overlaps: " + string.Join(", ", deferment.OverlapWith)
                           : string.Empty);
    }

    private bool AfterWrite(OgResult<OgWellStatusBoard> result, string verb, string subject)
    {
        _notice.Text = result.Success
            ? result.Message ?? "Saved."
            : result.Message ?? "The change was not applied.";

        if (result.Success)
        {
            _application.AuditLog.Add(verb, "production", subject, "ok", _application.Session.UserId);
        }

        Load();
        return result.Success;
    }

    private static DateTime? Date(string? text) =>
        DateTime.TryParse(text?.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value
            : null;

    private static TextBox Field(string name, double width)
    {
        var box = new TextBox
        {
            MinWidth = width,
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
