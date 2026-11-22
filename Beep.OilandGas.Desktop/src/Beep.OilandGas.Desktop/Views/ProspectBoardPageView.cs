using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>One row of the flat list view.</summary>
public sealed record OgProspectListRow(
    string Prospect,
    string Play,
    string Stage,
    string Chance,
    string Owner,
    string Age);

/// <summary>A stage-history row, shown for the selected card.</summary>
public sealed record OgStageHistoryRow(string Stage, string Order, string Started, string Ended, string Outcome);

/// <summary>
/// EXP-02: the prospect portfolio as a stage board, with one advance command.
/// </summary>
/// <remarks>
/// Columns come back from the service already ordered by the recorded stage sequence, and the unclassified column
/// is explicit, so neither is this page's guess (AC-1, AC-2). A card whose risk is incomplete says "incomplete"
/// rather than showing a number (AC-3). Advancing is refused by the service with the reason, and the page reports
/// it without changing anything (AC-4). The drag entry point calls the same command (AC-5). A failed read keeps
/// the board that is on screen and reports the failure, rather than clearing it (AC-6).
/// </remarks>
public sealed class ProspectBoardPageView : OgPageBase
{
    public const string Route = "prospects";

    private readonly Bootstrapping.OgApplication _application;
    private readonly IOgProspectBoard _board;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _detail;
    private readonly TextBlock _filterText;
    private readonly ComboBox _play;
    private readonly ComboBox _owner;
    private readonly ComboBox _basin;
    private readonly TextBox _minChance;
    private readonly TextBox _maxAge;
    private readonly CheckBox _listView;
    private readonly ScrollViewer _boardScroll;
    private readonly DataGrid _grid;
    private readonly DataGrid _history;
    private readonly TextBox _targetStage;

    public ProspectBoardPageView(Bootstrapping.OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _board = application.ProspectBoard;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _detail = Body(string.Empty);
        _filterText = Body(string.Empty);

        _play = Picker("Play filter");
        _owner = Picker("Owner filter");
        _basin = Picker("Basin filter");

        _minChance = new TextBox { MinWidth = 90, Margin = new Thickness(0, 0, 8, 0), ToolTip = "Minimum chance percent" };
        Named(_minChance, "Minimum chance percent");

        _maxAge = new TextBox { MinWidth = 90, Margin = new Thickness(0, 0, 8, 0), ToolTip = "Maximum days in the current stage" };
        Named(_maxAge, "Maximum stage age in days");

        _listView = new CheckBox
        {
            Content = "List view",
            Margin = new Thickness(8, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center,
            IsTabStop = true,
        };

        Named(_listView, "List view");
        _listView.Checked += (_, _) => ApplyViewMode();
        _listView.Unchecked += (_, _) => ApplyViewMode();

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 220,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Visibility = Visibility.Collapsed,
            Columns =
            {
                Text("Prospect", nameof(OgProspectListRow.Prospect), 200),
                Text("Play", nameof(OgProspectListRow.Play), 150),
                Text("Stage", nameof(OgProspectListRow.Stage), 130),
                Text("Chance", nameof(OgProspectListRow.Chance), 90),
                Text("Owner", nameof(OgProspectListRow.Owner), 140),
                Text("Age", nameof(OgProspectListRow.Age), 80),
            },
        };

        Named(_grid, "Prospects (list)");
        _grid.SelectionChanged += (_, _) => SelectFromList();

        _boardScroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MinHeight = 240,
            MaxHeight = 420,
            Margin = new Thickness(0, 6, 0, 8),
            Focusable = false,
            Content = new WrapPanel { Orientation = Orientation.Horizontal },
        };

        Named(_boardScroll, "Prospect board");
        _boardScroll.IsTabStop = false;

        _history = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MaxHeight = 150,
            MinHeight = 60,
            Margin = new Thickness(0, 4, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            Columns =
            {
                Text("Stage", nameof(OgStageHistoryRow.Stage), 140),
                Text("Seq", nameof(OgStageHistoryRow.Order), 60),
                Text("Started", nameof(OgStageHistoryRow.Started), 100),
                Text("Ended", nameof(OgStageHistoryRow.Ended), 100),
                Text("Outcome", nameof(OgStageHistoryRow.Outcome), 120),
            },
        };

        Named(_history, "Prospect stage history");

        _targetStage = new TextBox
        {
            MinWidth = 150,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = "Stage to advance to",
        };

        Named(_targetStage, "Stage to advance to");

        var filters = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        filters.Children.Add(Caption("Play"));
        filters.Children.Add(_play);
        filters.Children.Add(Caption("Owner"));
        filters.Children.Add(_owner);
        filters.Children.Add(Caption("Basin"));
        filters.Children.Add(_basin);
        filters.Children.Add(Caption("Min chance"));
        filters.Children.Add(_minChance);
        filters.Children.Add(Caption("Max age"));
        filters.Children.Add(_maxAge);
        filters.Children.Add(Button("Apply", (_, _) => _ = LoadAsync()));
        filters.Children.Add(Button("Clear", (_, _) => ClearFilters()));
        filters.Children.Add(_listView);

        var actions = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actions.Children.Add(Button("Refresh", (_, _) => _ = LoadAsync()));
        actions.Children.Add(Button("Open prospect", (_, _) => OpenSelected()));
        actions.Children.Add(Caption("Advance to"));
        actions.Children.Add(_targetStage);
        actions.Children.Add(Button("Advance stage", (_, _) => _ = AdvanceSelectedAsync()));

        var panel = new StackPanel();
        panel.Children.Add(actions);
        panel.Children.Add(filters);
        panel.Children.Add(_header);
        panel.Children.Add(_filterText);
        panel.Children.Add(_notice);
        panel.Children.Add(_boardScroll);
        panel.Children.Add(_grid);
        panel.Children.Add(_detail);
        panel.Children.Add(_history);

        ShowContent(panel);

        Load();
    }

    /// <summary>The last board read, or null before the first successful read.</summary>
    public OgProspectBoardSnapshot? Snapshot { get; private set; }

    public IReadOnlyList<OgProspectColumn> Columns => Snapshot?.Columns ?? [];

    public IReadOnlyList<OgProspectCard> Cards => Snapshot?.Cards ?? [];

    /// <summary>The card chosen on the board or in the list.</summary>
    public OgProspectCard? SelectedCard { get; private set; }

    /// <summary>True when the last read failed; the board keeps showing what it had (AC-6).</summary>
    public bool LastFailed { get; private set; }

    public bool IsListView => _listView.IsChecked == true;

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string FilterText => _filterText.Text;

    public string DetailText => _detail.Text;

    /// <summary>The filter the controls describe.</summary>
    public OgProspectFilter Filter =>
        new(
            Play: Empty(_play.SelectedItem as string),
            Owner: Empty(_owner.SelectedItem as string),
            Basin: Empty(_basin.SelectedItem as string),
            MinimumChancePercent: Number(_minChance.Text),
            MaximumStageAgeDays: Days(_maxAge.Text));

    public bool Load() => LoadAsync().GetAwaiter().GetResult();

    /// <summary>
    /// AC-6: what the board keeps after a read. A failed read leaves the last good snapshot and the selected card
    /// exactly as they were, so the board is never silently emptied; a successful read replaces them.
    /// </summary>
    public static (OgProspectBoardSnapshot? Snapshot, OgProspectCard? Selected, bool Failed) AfterRead(
        OgProspectBoardSnapshot? current,
        OgProspectCard? selected,
        bool success,
        OgProspectBoardSnapshot? loaded)
    {
        if (success && loaded is not null)
        {
            return (loaded, null, false);
        }

        return (current, selected, true);
    }

    /// <summary>Reads the board for the signed-in field.</summary>
    public async Task<bool> LoadAsync()
    {
        var session = _application.Session;
        var loaded = await _board
            .LoadAsync(session.FieldId, session.FieldName, Filter, DateTime.Today)
            .ConfigureAwait(true);

        var kept = AfterRead(Snapshot, SelectedCard, loaded.Success, loaded.Value);

        Snapshot = kept.Snapshot;
        SelectedCard = kept.Selected;
        LastFailed = kept.Failed;

        if (kept.Failed)
        {
            // AC-6: the board on screen is left alone; the failure is stated with its reference.
            _notice.Text = loaded.Message ?? "The prospect board could not be read.";
            ShowState(
                Snapshot is null ? OgViewState.Failed : OgViewState.Ready,
                _notice.Text,
                loaded.Reference);
            return false;
        }

        RenderOptions();
        RenderBoard(Snapshot!);
        RenderList(Snapshot!);
        RenderDetail(SelectedCard);

        _header.Text =
            $"{Snapshot.FieldName} · {Snapshot.CardCount.ToString(CultureInfo.InvariantCulture)} prospect(s) · " +
            $"{Snapshot.ColumnCount.ToString(CultureInfo.InvariantCulture)} stage(s) · as of " +
            $"{Snapshot.AsOf.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

        _filterText.Text = Filter.HasFilters
            ? $"Filters: {Filter.Describe()}"
            : "No filters: every prospect in this field is shown.";

        _notice.Text = Snapshot.IsEmpty
            ? (Filter.HasFilters
                ? "No prospect matches these filters. Clear them to see the whole portfolio."
                : "No exploration activity recorded for this field.")
            : Snapshot.IncompleteRiskCount == 0
                ? "Every prospect shown has a complete five-element risk assessment."
                : $"{Snapshot.IncompleteRiskCount.ToString(CultureInfo.InvariantCulture)} prospect(s) shown have " +
                  "incomplete risk; their card says \"incomplete\" and no chance is shown.";

        ShowState(Snapshot.IsEmpty ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    /// <summary>AC-2 / AC-4: the menu command, refused with the service's stated reason.</summary>
    public async Task<bool> AdvanceSelectedAsync()
    {
        if (SelectedCard is not { } card)
        {
            _notice.Text = "Select a prospect card first.";
            return false;
        }

        var advanced = await _board
            .AdvanceAsync(card, _targetStage.Text, _application.Session.UserId)
            .ConfigureAwait(true);

        _notice.Text = advanced.Success
            ? advanced.Message ?? "Stage advanced."
            : advanced.Message ?? "The stage was not changed.";

        if (advanced.Success)
        {
            await LoadAsync().ConfigureAwait(true);
        }
        else
        {
            _application.AuditLog.Add(
                "prospect.advance.refused",
                "prospect",
                card.Id,
                "refused",
                $"{advanced.Reference}: from {card.StageLabel} to {_targetStage.Text}");
        }

        return advanced.Success;
    }

    /// <summary>
    /// AC-5: the drag-and-drop entry point. It is the same command the menu runs, so a drop cannot take a path the
    /// menu would refuse.
    /// </summary>
    public async Task<bool> MoveSelectedAsync(string targetStage)
    {
        if (SelectedCard is not { } card)
        {
            _notice.Text = "Select a prospect card first.";
            return false;
        }

        var moved = await _board
            .MoveAsync(card, targetStage, _application.Session.UserId)
            .ConfigureAwait(true);

        _notice.Text = moved.Success
            ? moved.Message ?? "Stage advanced."
            : moved.Message ?? "The stage was not changed.";

        return moved.Success;
    }

    /// <summary>Records the card in the shared prospect context for the editor that follows (EXP-03).</summary>
    public bool OpenSelected()
    {
        if (SelectedCard is not { } card)
        {
            _notice.Text = "Select a prospect card first.";
            return false;
        }

        _application.ProspectContext.Select(new OgProspectSelection(card.Id, card.Name));
        _application.AuditLog.Add("prospect.select", "prospect", card.Id, "ok", card.Describe());

        var navigated = _application.Pages.Navigate(ProspectEditorPageView.Route);
        _notice.Text = navigated
            ? $"Opening {card.Name}."
            : $"{card.Name} is selected.";
        return true;
    }

    /// <summary>Clears every filter and reloads.</summary>
    public bool ClearFilters()
    {
        _play.SelectedItem = null;
        _owner.SelectedItem = null;
        _basin.SelectedItem = null;
        _minChance.Text = string.Empty;
        _maxAge.Text = string.Empty;

        return Load();
    }

    /// <summary>Resolves a card's refusal for a named target without reading again (used by the drag path's test).</summary>
    public async Task<OgResult> RefusalForAsync(OgProspectCard card, string targetStage) =>
        await _board.AdvanceAsync(card, targetStage, _application.Session.UserId).ConfigureAwait(true);

    private void RenderOptions()
    {
        if (Snapshot is null)
        {
            return;
        }

        Fill(_play, Snapshot.Plays);
        Fill(_owner, Snapshot.Owners);
        Fill(_basin, Snapshot.Basins);
    }

    private void RenderBoard(OgProspectBoardSnapshot snapshot)
    {
        var wrap = (WrapPanel)_boardScroll.Content;

        wrap.Children.Clear();

        foreach (var column in snapshot.Columns)
        {
            var cards = new ListBox
            {
                MinWidth = 190,
                MaxHeight = 260,
                MinHeight = 60,
                Margin = new Thickness(0, 4, 0, 4),
                IsTabStop = true,
                Tag = column,
                ItemsSource = column.Cards.Select(card => card.Describe()).ToList(),
            };

            Named(cards, $"Cards in {column.Stage}");
            cards.SelectionChanged += (_, _) => SelectFromColumn(cards);

            var box = new StackPanel { Margin = new Thickness(0, 0, 10, 0), MinWidth = 200 };

            box.Children.Add(new TextBlock
            {
                Text = column.Header,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
            });

            box.Children.Add(new TextBlock
            {
                Text = column.RiskText,
                FontSize = 11.5,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 0),
            });

            box.Children.Add(cards);

            wrap.Children.Add(new Border
            {
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 8, 0),
                BorderThickness = new Thickness(1),
                BorderBrush = SystemColors.ControlDarkBrush,
                Child = box,
            });
        }
    }

    private void RenderList(OgProspectBoardSnapshot snapshot)
    {
        _grid.ItemsSource = snapshot.Cards
            .Select(card => new OgProspectListRow(
                card.Name,
                card.Play ?? string.Empty,
                card.StageLabel,
                card.ChanceText,
                card.Owner ?? string.Empty,
                card.AgeText))
            .ToList();
    }

    private void RenderDetail(OgProspectCard? card)
    {
        if (card is null)
        {
            _detail.Text = "Select a card for its detail and its stage history.";
            _history.ItemsSource = null;
            return;
        }

        _detail.Text =
            $"{card.Describe()} · owner {card.Owner ?? "-"} · basin {card.Basin ?? "-"} · " +
            $"in {card.StageLabel} for {card.AgeText}" +
            (card.RiskComplete ? string.Empty : " · risk is incomplete, so no chance is shown");

        _history.ItemsSource = card.History
            .Select(row => new OgStageHistoryRow(
                row.Stage,
                row.Order.ToString(CultureInfo.InvariantCulture),
                row.StartedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                row.EndedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                row.Outcome ?? string.Empty))
            .ToList();
    }

    private void SelectFromColumn(ListBox list)
    {
        if (list.Tag is not OgProspectColumn column)
        {
            return;
        }

        var index = list.SelectedIndex;

        if (index < 0 || index >= column.Cards.Count)
        {
            return;
        }

        SelectedCard = column.Cards[index];
        RenderDetail(SelectedCard);
        _notice.Text =
            $"{SelectedCard.Name} selected. Advancing it is refused until the process engine is wired (STG-14).";
    }

    private void SelectFromList()
    {
        var index = _grid.SelectedIndex;

        if (Snapshot is null || index < 0 || index >= Snapshot.Cards.Count)
        {
            return;
        }

        SelectedCard = Snapshot.Cards[index];
        RenderDetail(SelectedCard);
    }

    private void ApplyViewMode()
    {
        var list = IsListView;

        _grid.Visibility = list ? Visibility.Visible : Visibility.Collapsed;
        _boardScroll.Visibility = list ? Visibility.Collapsed : Visibility.Visible;
    }

    private static void Fill(ComboBox picker, IReadOnlyList<string> values)
    {
        var current = picker.SelectedItem as string;

        picker.ItemsSource = values;
        picker.SelectedItem = values.FirstOrDefault(value =>
            string.Equals(value, current, StringComparison.OrdinalIgnoreCase));
    }

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal? Number(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : null;

    private static int? Days(string? text) =>
        int.TryParse(text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value >= 0
            ? value
            : null;

    private static ComboBox Picker(string name)
    {
        var picker = new ComboBox { MinWidth = 130, Margin = new Thickness(0, 0, 8, 0) };

        Named(picker, name);
        return picker;
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
