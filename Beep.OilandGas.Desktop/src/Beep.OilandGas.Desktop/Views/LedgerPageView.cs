using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ACC-02: the ledger - journals, posting, the trial balance and the period close that locks the books.
/// </summary>
/// <remarks>
/// A journal that does not balance is refused with the exact imbalance (AC-1, SG-10-1), posting is a transition of the row
/// that exists so a duplicate cannot create a second entry (AC-2, SG-10-2), a write into a closed period is refused naming
/// the period (AC-3, SG-10-3), the author cannot post their own journal (AC-4, SG-10-4), the trial balance is shown with
/// its two totals and agrees or does not (AC-5), reopening records actor, reason and time (AC-6), and a trial-balance row
/// drills down to the journal lines behind it (AC-7).
/// </remarks>
public sealed class LedgerPageView : OgPageBase
{
    public const string Route = "ledger";

    private readonly OgApplication _application;
    private readonly IOgLedger _ledger;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _reference;
    private readonly TextBlock _period;
    private readonly TextBlock _trial;
    private readonly TextBlock _journalDetail;
    private readonly TextBlock _detail;
    private readonly TextBox _date;
    private readonly ComboBox _type;
    private readonly TextBox _source;
    private readonly TextBox _referenceNumber;
    private readonly TextBox _description;
    private readonly TextBox _lines;
    private readonly TextBox _closeNote;
    private readonly TextBox _reopenReason;
    private readonly DataGrid _journals;
    private readonly DataGrid _trialGrid;
    private readonly DataGrid _lineGrid;
    private readonly DataGrid _periodGrid;
    private readonly Button _createButton;
    private readonly Button _postButton;
    private readonly Button _reverseButton;
    private readonly Button _closeButton;
    private readonly Button _reopenButton;

    private OgLedgerWorkspace? _workspace;
    private OgJournal? _selected;
    private OgPeriod? _selectedPeriod;

    public LedgerPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _ledger = application.Ledger;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _missing = Body(string.Empty);
        _missing.TextWrapping = TextWrapping.Wrap;
        _missing.Margin = new Thickness(0, 4, 0, 4);

        _reference = Body(string.Empty);
        _reference.TextWrapping = TextWrapping.Wrap;
        _reference.Margin = new Thickness(0, 0, 0, 4);

        _period = Body(string.Empty);
        _period.TextWrapping = TextWrapping.Wrap;
        _period.Margin = new Thickness(0, 4, 0, 4);

        _trial = Body(string.Empty);
        _trial.TextWrapping = TextWrapping.Wrap;
        _trial.Margin = new Thickness(0, 0, 0, 4);

        _journalDetail = Body(string.Empty);
        _journalDetail.TextWrapping = TextWrapping.Wrap;
        _journalDetail.Margin = new Thickness(0, 0, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _date = Field("Entry date yyyy-MM-dd", 160);
        _date.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _source = Field("Source", 130);
        _source.Text = "MANUAL";
        _referenceNumber = Field("Reference", 140);
        _description = Field("Description", 260);
        _closeNote = Field("Close note", 220);
        _reopenReason = Field("Reopen reason", 240);

        _lines = new TextBox
        {
            AcceptsReturn = true,
            IsTabStop = true,
            MinHeight = 68,
            MaxHeight = 130,
            Margin = new Thickness(0, 0, 0, 6),
            TextWrapping = TextWrapping.NoWrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "account number, debit, credit, description - one line per entry",
        };

        Named(_lines, "Journal lines, one per line");
        _lines.Text = "# account, debit, credit, description" + Environment.NewLine + "1000, 1000.00, , cash in";

        _type = Picker("Journal type", 150);

        _journals = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 110,
            MaxHeight = 180,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Number", "Number", 110),
                Text("Date", "Date", 100),
                Text("Type", "Type", 100),
                Text("Source", "Source", 100),
                Text("Debit / credit", "AmountText", 150),
                Text("Balance", "BalanceText", 140),
                Text("Status", "Status", 100),
                Text("Author", "Author", 100),
            },
        };

        Named(_journals, "Journals");
        _journals.SelectionChanged += (_, _) => SelectJournal();

        _trialGrid = new DataGrid
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
                Text("Account", "AccountNumber", 100),
                Text("Name", "AccountName", 200),
                Text("Debit", "Debit", 120),
                Text("Credit", "Credit", 120),
                Text("Balance", "Balance", 120),
                Text("Lines", "Lines", 70),
            },
        };

        Named(_trialGrid, "Trial balance for the selected period");
        _trialGrid.SelectionChanged += (_, _) => SelectTrialRow();

        _lineGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 150,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Account", "AccountNumber", 100),
                Text("Line", "LineNumber", 60),
                Text("Debit", "Debit", 110),
                Text("Credit", "Credit", 110),
                Text("Description", "Description", 260),
            },
        };

        Named(_lineGrid, "Journal lines behind the selected trial-balance row");

        _periodGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 80,
            MaxHeight = 140,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Period", "Code", 90),
                Text("Start", "Start", 100),
                Text("End", "End", 100),
                Text("State", "Status", 90),
                Text("Closed by", "ClosedBy", 100),
                Text("Reopened by", "ReopenedBy", 100),
            },
        };

        Named(_periodGrid, "Periods");
        _periodGrid.SelectionChanged += (_, _) => SelectPeriod();

        _createButton = Button("Create draft", (_, _) => _ = CreateAsync());
        _postButton = Button("Post", (_, _) => _ = PostAsync());
        _postButton.IsEnabled = false;
        _reverseButton = Button("Reverse", (_, _) => _ = ReverseAsync());
        _reverseButton.IsEnabled = false;
        _closeButton = Button("Close period", (_, _) => _ = CloseAsync());
        _closeButton.IsEnabled = false;
        _reopenButton = Button("Reopen period", (_, _) => _ = ReopenAsync());
        _reopenButton.IsEnabled = false;

        var entryBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        entryBar.Children.Add(Caption("Date"));
        entryBar.Children.Add(_date);
        entryBar.Children.Add(Caption("Type"));
        entryBar.Children.Add(_type);
        entryBar.Children.Add(Caption("Source"));
        entryBar.Children.Add(_source);
        entryBar.Children.Add(Caption("Reference"));
        entryBar.Children.Add(_referenceNumber);

        var descriptionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        descriptionBar.Children.Add(Caption("Description"));
        descriptionBar.Children.Add(_description);
        descriptionBar.Children.Add(_createButton);

        var actionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar.Children.Add(_postButton);
        actionBar.Children.Add(_reverseButton);

        var periodBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        periodBar.Children.Add(Caption("Close note"));
        periodBar.Children.Add(_closeNote);
        periodBar.Children.Add(_closeButton);
        periodBar.Children.Add(Caption("Reopen reason"));
        periodBar.Children.Add(_reopenReason);
        periodBar.Children.Add(_reopenButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_reference);
        panel.Children.Add(Caption("New journal"));
        panel.Children.Add(entryBar);
        panel.Children.Add(descriptionBar);
        panel.Children.Add(Caption("Lines"));
        panel.Children.Add(_lines);
        panel.Children.Add(_missing);
        panel.Children.Add(Caption("Posting"));
        panel.Children.Add(actionBar);
        panel.Children.Add(Caption("Period"));
        panel.Children.Add(periodBar);
        panel.Children.Add(_period);
        panel.Children.Add(Caption("Journals"));
        panel.Children.Add(_journals);
        panel.Children.Add(Caption("Selected journal"));
        panel.Children.Add(_journalDetail);
        panel.Children.Add(Caption("Trial balance"));
        panel.Children.Add(_trial);
        panel.Children.Add(_trialGrid);
        panel.Children.Add(Caption("Drill-down"));
        panel.Children.Add(_lineGrid);
        panel.Children.Add(Caption("Periods"));
        panel.Children.Add(_periodGrid);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[] { _date, _source, _referenceNumber, _description, _lines, _closeNote, _reopenReason })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        _type.SelectionChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgLedgerWorkspace? Workspace => _workspace;

    public OgJournal? Journal => _selected;

    public OgPeriod? Period => _selectedPeriod;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string ReferenceText => _reference.Text;

    public string PeriodText => _period.Text;

    public string TrialBalanceText => _trial.Text;

    public string JournalText => _journalDetail.Text;

    public string DetailText => _detail.Text;

    public bool CanCreate => LedgerRules.Missing(Inputs()).Count == 0;

    public bool CanPost => _selected is { IsDraft: true };

    public bool CanReverse => _selected is { IsReversible: true };

    public bool CanClose => _selectedPeriod is { IsOpen: true };

    public bool CanReopen => _selectedPeriod is { IsOpen: false };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    private OgLedgerContext Context => OgLedgerContext.FromSession(_application.Session);

    public bool Load()
    {
        var loaded = _ledger.LoadAsync(Context).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _journals.ItemsSource = null;
            _header.Text = "Ledger";
            _notice.Text = loaded.Message ?? "The ledger could not be read.";
            ShowState(OgViewState.Failed, "The ledger could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public Task<bool> CreateAsync() =>
        RunAsync(() => _ledger.CreateJournalAsync(Inputs(), Context, _application.Session.UserId),
            "accounting.journal.write", "Draft created");

    public Task<bool> PostAsync()
    {
        if (_selected is not { } journal)
        {
            _notice.Text = "Select the journal to post.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _ledger.PostAsync(journal.Id, Context, _application.Session.UserId),
            "accounting.journal.post", "Posted");
    }

    public Task<bool> ReverseAsync()
    {
        if (_selected is not { } journal)
        {
            _notice.Text = "Select the journal to reverse.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _ledger.ReverseAsync(journal.Id, Context, _application.Session.UserId),
            "accounting.journal.reverse", "Reversed with a counter-entry");
    }

    public Task<bool> CloseAsync()
    {
        if (_selectedPeriod is not { } period)
        {
            _notice.Text = "Select the period to close.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _ledger.ClosePeriodAsync(period.Code, Context, _closeNote.Text ?? string.Empty,
                _application.Session.UserId),
            "accounting.period.close", "Period closed");
    }

    public Task<bool> ReopenAsync()
    {
        if (_selectedPeriod is not { } period)
        {
            _notice.Text = "Select the period to reopen.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _ledger.ReopenPeriodAsync(period.Code, Context, _reopenReason.Text ?? string.Empty,
                _application.Session.UserId),
            "accounting.period.reopen", "Period reopened");
    }

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgLedgerWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _notice.Text = outcome.Message ?? "The ledger refused the action.";
            ShowState(OgViewState.Failed, "The ledger refused the action.", outcome.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(outcome.Value);

        _application.AuditLog.Add(
            auditAction,
            "accounting",
            outcome.Value.Selected?.Number ?? outcome.Value.PeriodCode,
            "ok",
            _application.Session.UserId);

        _notice.Text = done + ". " + outcome.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    private void Apply(OgLedgerWorkspace workspace)
    {
        _workspace = workspace;

        _type.ItemsSource = workspace.JournalTypes.ToList();
        _type.SelectedIndex = _type.SelectedIndex < 0 ? 0 : _type.SelectedIndex;

        _journals.ItemsSource = workspace.Journals.ToList();
        _trialGrid.ItemsSource = workspace.TrialBalance.ToList();
        _periodGrid.ItemsSource = workspace.Periods.ToList();

        _header.Text = "Accounting · ledger";
        _reference.Text =
            "books " + Context.CompanyName + " (" + Context.BookCode + ") · " + Context.Currency +
            " · money is decimal end to end · the period lock is the company ledger period";
        _notice.Text = workspace.HistoryFailed
            ? "The ledger is unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText;
        _period.Text = "Period " + workspace.PeriodCode + " · " + (workspace.SelectedPeriod?.Describe() ?? "not closed yet");
        _trial.Text = workspace.TrialBalanceText;

        _selected = workspace.Selected;
        _selectedPeriod = workspace.SelectedPeriod;
        ShowJournal();
        _lineGrid.ItemsSource = workspace.TrialBalance.FirstOrDefault()?.Lines.ToList();
        RefreshEnabled();
    }

    private void ShowJournal()
    {
        if (_selected is not { } journal)
        {
            _journalDetail.Text = "no journal is selected";
            _detail.Text = string.Empty;
            return;
        }

        _journalDetail.Text = journal.Describe() + " · " + journal.PostingText + " · " + journal.BalanceText +
                              " · period " + journal.PeriodCode;
        _detail.Text = journal.Lines.Count == 0
            ? "no line"
            : string.Join(Environment.NewLine, journal.Lines.Select(line => line.Describe()));
        _lineGrid.ItemsSource = journal.Lines.ToList();
    }

    private void SelectJournal()
    {
        if (_journals.SelectedItem is not OgJournal journal)
        {
            return;
        }

        _selected = journal;
        ShowJournal();
        RefreshEnabled();
    }

    private void SelectTrialRow()
    {
        if (_trialGrid.SelectedItem is not OgTrialBalanceRow row)
        {
            return;
        }

        // AC-7: the drill-down shows the journal lines behind the balance.
        _lineGrid.ItemsSource = row.Lines.ToList();
    }

    private void SelectPeriod()
    {
        if (_periodGrid.SelectedItem is not OgPeriod period)
        {
            return;
        }

        _selectedPeriod = period;
        _period.Text = "Period " + period.Code + " · " + period.Describe();
        RefreshEnabled();
    }

    private void RefreshEnabled()
    {
        var missing = LedgerRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The journal needs " + string.Join(", ", missing) + ".";

        _createButton.IsEnabled = missing.Count == 0;
        _postButton.IsEnabled = CanPost;
        _reverseButton.IsEnabled = CanReverse;
        _closeButton.IsEnabled = CanClose;
        _reopenButton.IsEnabled = CanReopen;
    }

    private OgJournalInputs Inputs() =>
        new(
            Date(_date.Text),
            _type.SelectedItem as string,
            Empty(_source.Text),
            Empty(_referenceNumber.Text),
            Empty(_description.Text),
            Lines());

    /// <summary>Reads the line editor: <c>account, debit, credit, description</c> per line; <c>#</c> starts a comment.</summary>
    private IReadOnlyList<OgJournalLineInput> Lines()
    {
        var lines = new List<OgJournalLineInput>();

        foreach (var row in (_lines.Text ?? string.Empty)
                     .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (row.StartsWith('#'))
            {
                continue;
            }

            var parts = row.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length < 3)
            {
                continue;
            }

            lines.Add(new OgJournalLineInput(
                string.IsNullOrWhiteSpace(parts[0]) ? null : parts[0],
                Parse(parts[1]),
                Parse(parts[2]),
                parts.Length > 3 && !string.IsNullOrWhiteSpace(parts[3]) ? parts[3] : null));
        }

        return lines;
    }

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal? Parse(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

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

    private static ComboBox Picker(string name, double width)
    {
        var picker = new ComboBox
        {
            MinWidth = width,
            Margin = new Thickness(0, 0, 8, 0),
        };

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
