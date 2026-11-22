using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// WFL-05: one list of everything assigned to the signed-in person.
/// </summary>
/// <remarks>
/// The queue tabs are Today, Overdue, Unscheduled, All and Completed recently. A missing deadline is unscheduled,
/// never overdue (AC-1); an unconfirmed timestamp is never counted as overdue (AC-2); every tab's count uses the same
/// predicate as its grid (AC-3); a missing source record is shown as a broken link (AC-4); a persona change starts a
/// new load and a late response for the old persona is discarded (AC-5); a deadline is shown in UTC and the company
/// zone (AC-6). The expiring-lease tasks of LSE-03 are consumed here.
/// </remarks>
public sealed class TaskInboxPageView : OgPageBase
{
    public const string Route = "tasks";

    private readonly OgApplication _application;
    private readonly IOgTaskInbox _inbox;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _counts;
    private readonly TextBlock _detail;
    private readonly ComboBox _tab;
    private readonly DataGrid _grid;

    public TaskInboxPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _inbox = application.TaskInbox;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 8, 0, 8);

        _tab = new ComboBox
        {
            MinWidth = 160,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = new[] { "Today", "Overdue", "Unscheduled", "All", "Completed recently" },
            SelectedIndex = 0,
        };

        Named(_tab, "Task queue");
        _tab.SelectionChanged += (_, _) => Load();

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 200,
            MaxHeight = 320,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Task", "Title", 240),
                Text("Module", "Module", 90),
                Text("Due", "DueText", 260),
                Text("State", "StateText", 90),
                Text("Source", "SourceRoute", 120),
            },
        };

        Named(_grid, "Task inbox");
        _grid.SelectionChanged += (_, _) => ShowSelected();

        var actions = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actions.Children.Add(Caption("Queue"));
        actions.Children.Add(_tab);
        actions.Children.Add(Button("Refresh", (_, _) => Load()));
        actions.Children.Add(Button("Open source", (_, _) => OpenSelected()));

        var panel = new StackPanel();
        panel.Children.Add(actions);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_counts);
        panel.Children.Add(_grid);
        panel.Children.Add(_detail);

        ShowContent(panel);

        Load();
    }

    public OgTaskInboxSnapshot? Snapshot { get; private set; }

    public IReadOnlyList<OgTask> Tasks => Snapshot?.Tasks ?? [];

    public OgTask? Selected { get; private set; }

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string CountsText => _counts.Text;

    public string DetailText => _detail.Text;

    public OgTaskTab Tab => _tab.SelectedIndex switch
    {
        1 => OgTaskTab.Overdue,
        2 => OgTaskTab.Unscheduled,
        3 => OgTaskTab.All,
        4 => OgTaskTab.CompletedRecently,
        _ => OgTaskTab.Today,
    };

    /// <summary>AC-5: a load begun for a newer persona wins; the earlier response is discarded.</summary>
    public async Task<bool> LoadAsync()
    {
        var session = _application.Session;
        var version = _inbox.BeginLoad();

        var loaded = await _inbox
            .LoadAsync(
                session.PersonaCode,
                _application.Permissions.Roles,
                session.FieldId,
                session.FieldName,
                Tab,
                DateTime.Today,
                session.TimeZoneId)
            .ConfigureAwait(true);

        if (!_inbox.IsCurrent(version))
        {
            // A newer persona or tab started while this read was in flight: do not render it over the new one.
            return false;
        }

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Snapshot = null;
            Selected = null;
            _grid.ItemsSource = null;
            _header.Text = "My tasks";
            _counts.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The task inbox could not be read.";
            ShowState(OgViewState.Failed, "The task inbox could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Snapshot = loaded.Value;
        Selected = null;

        _grid.ItemsSource = Tasks.ToList();

        _header.Text =
            $"{Snapshot.TabLabel} · {Snapshot.PersonaCode} · " +
            $"{(string.IsNullOrWhiteSpace(session.FieldName) ? "(no field selected)" : session.FieldName)}";

        _counts.Text = Snapshot.Counts.Summary;

        _notice.Text = Snapshot.IsEmpty
            ? Snapshot.Tab switch
            {
                OgTaskTab.Overdue => "Nothing is overdue. Only a confirmed deadline in the past is overdue.",
                OgTaskTab.Unscheduled => "Every open task has a deadline.",
                OgTaskTab.CompletedRecently => "Nothing has been completed recently.",
                OgTaskTab.Today => "Nothing is due today.",
                OgTaskTab.All => "No task is assigned to this persona.",
            }
            : Snapshot.Counts.Broken == 0
                ? "Every task's source record is present."
                : $"{Snapshot.Counts.Broken.ToString(CultureInfo.InvariantCulture)} task(s) point at a source " +
                  "record that cannot be found; the grid marks the broken link.";

        _detail.Text = "Select a task for its detail and its source link.";
        ShowState(Snapshot.IsEmpty ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    public bool Load() => LoadAsync().GetAwaiter().GetResult();

    /// <summary>Opens the source page; a broken or missing link is stated, not followed.</summary>
    public bool OpenSelected()
    {
        if (Selected is not { } task)
        {
            _notice.Text = "Select a task first.";
            return false;
        }

        if (!task.SourceExists)
        {
            _notice.Text = $"{task.Title}: the source record cannot be found, so the link is not followed.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(task.SourceRoute))
        {
            _notice.Text = $"{task.Title} has no source page.";
            return false;
        }

        return _application.Pages.Navigate(task.SourceRoute);
    }

    private void ShowSelected()
    {
        var index = _grid.SelectedIndex;

        if (index < 0 || index >= Tasks.Count)
        {
            return;
        }

        Selected = Tasks[index];

        _detail.Text = string.Join(
            Environment.NewLine,
            new[]
            {
                Selected.Describe(),
                $"Module {Selected.Module} · kind {Selected.Kind} · subject {Selected.SubjectKind} {Selected.SubjectId}",
                Selected.Description,
                string.IsNullOrWhiteSpace(Selected.BrokenText)
                    ? $"Source {Selected.SourceRoute}"
                    : $"Source {Selected.SourceRoute} · {Selected.BrokenText}",
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
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
