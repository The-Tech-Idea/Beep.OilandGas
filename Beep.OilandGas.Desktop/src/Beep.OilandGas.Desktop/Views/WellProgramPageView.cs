using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>One row of the approval queue.</summary>
public sealed record OgWellProgramRow(
    string Program,
    string Prospect,
    string Author,
    string Submitted,
    string Age,
    string Cost,
    string State,
    string Risk);

/// <summary>One row of the decision history.</summary>
public sealed record OgWellProgramHistoryRow(
    string Decision,
    string Result,
    string Decided,
    string Actor,
    string Rationale,
    string Conditions);

/// <summary>
/// EXP-05: the gate between "we want to drill this prospect" and "we may spend money drilling it".
/// </summary>
/// <remarks>
/// The decision is recorded with its actor, date, rationale and conditions (AC-1). The service refuses an approval
/// while the prospect's risk is incomplete (AC-2), refuses the author deciding their own program (AC-3), and
/// refuses a second decision on a program somebody else already decided (AC-4). Conditions are stored on the
/// approved program, where the drill program (DEV-05) reads them as constraints (AC-5). A decision that is not
/// acknowledged is retained and can be retried unchanged (AC-6).
/// </remarks>
public sealed class WellProgramPageView : OgPageBase
{
    public const string Route = "wellprogram";

    private readonly Bootstrapping.OgApplication _application;
    private readonly IOgWellProgramApprovals _gate;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _detail;
    private readonly ComboBox _decision;
    private readonly TextBox _rationale;
    private readonly TextBox _conditions;
    private readonly DataGrid _queue;
    private readonly DataGrid _history;

    public WellProgramPageView(Bootstrapping.OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _gate = application.WellProgramGate;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;
        _detail = Body(string.Empty);

        _decision = new ComboBox { MinWidth = 200, Margin = new Thickness(0, 0, 8, 0) };
        _decision.ItemsSource = OgWellProgramDecision.Decisions;
        _decision.SelectedIndex = 0;
        Named(_decision, "Gate decision");

        _rationale = new TextBox
        {
            MinWidth = 260,
            MinHeight = 52,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = "Rationale (required except for a plain approval)",
        };

        Named(_rationale, "Decision rationale");

        _conditions = new TextBox
        {
            MinWidth = 260,
            MinHeight = 52,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = "Conditions, one per line",
        };

        Named(_conditions, "Decision conditions, one per line");

        _queue = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 140,
            MaxHeight = 260,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Program", nameof(OgWellProgramRow.Program), 170),
                Text("Prospect", nameof(OgWellProgramRow.Prospect), 140),
                Text("Author", nameof(OgWellProgramRow.Author), 100),
                Text("Submitted", nameof(OgWellProgramRow.Submitted), 95),
                Text("Age", nameof(OgWellProgramRow.Age), 60),
                Text("Cost", nameof(OgWellProgramRow.Cost), 120),
                Text("State", nameof(OgWellProgramRow.State), 150),
                Text("Risk", nameof(OgWellProgramRow.Risk), 110),
            },
        };

        Named(_queue, "Well program approval queue");
        _queue.SelectionChanged += (_, _) => SelectFromQueue();

        _history = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 180,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            Columns =
            {
                Text("Decision", nameof(OgWellProgramHistoryRow.Decision), 160),
                Text("Result", nameof(OgWellProgramHistoryRow.Result), 150),
                Text("Decided", nameof(OgWellProgramHistoryRow.Decided), 95),
                Text("Actor", nameof(OgWellProgramHistoryRow.Actor), 100),
                Text("Rationale", nameof(OgWellProgramHistoryRow.Rationale), 200),
                Text("Conditions", nameof(OgWellProgramHistoryRow.Conditions), 220),
            },
        };

        Named(_history, "Well program decision history");

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        toolbar.Children.Add(Button("Refresh", (_, _) => Load()));
        toolbar.Children.Add(Button(
            "Exploration workspace",
            (_, _) => _application.Pages.Navigate(ExplorationWorkspacePageView.Route)));

        var decisionPanel = new WrapPanel { Margin = new Thickness(0, 4, 0, 8) };
        decisionPanel.Children.Add(Caption("Decision"));
        decisionPanel.Children.Add(_decision);
        decisionPanel.Children.Add(Caption("Rationale"));
        decisionPanel.Children.Add(_rationale);
        decisionPanel.Children.Add(Caption("Conditions"));
        decisionPanel.Children.Add(_conditions);
        decisionPanel.Children.Add(Button("Record decision", (_, _) => _ = DecideAsync()));
        decisionPanel.Children.Add(Button("Retry", (_, _) => _ = RetryAsync()));

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_detail);
        panel.Children.Add(decisionPanel);
        panel.Children.Add(_queue);
        panel.Children.Add(_history);

        ShowContent(panel);

        Load();
    }

    public IReadOnlyList<OgWellProgram> Programs { get; private set; } = [];

    public OgWellProgram? Selected { get; private set; }

    public IReadOnlyList<OgWellProgramHistoryRow> HistoryRows { get; private set; } = [];

    public bool LastFailed { get; private set; }

    /// <summary>AC-6: the decision waiting for a retry, or null.</summary>
    public OgWellProgramDecision? Pending => _gate.Pending;

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string DetailText => _detail.Text;

    public string AlwaysVisibleNotice =>
        "The generic approval engine (WFL-04) is not wired in this build, so this gate records its own decision; " +
        "delegation, escalation and quorum are not available.";

    public bool Load()
    {
        var session = _application.Session;
        var loaded = _gate
            .LoadQueueAsync(session.FieldId, session.FieldName, DateTime.Today)
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Programs = [];
            Selected = null;
            _queue.ItemsSource = null;
            _history.ItemsSource = null;
            _header.Text = "Well program approval";
            _detail.Text = string.Empty;
            // The "engine not wired" sentence is stated even on failure, so it cannot be lost with the data.
            _notice.Text = AlwaysVisibleNotice + " " +
                           (loaded.Message ?? "The well program queue could not be read.");
            ShowState(OgViewState.Failed, "The well program queue could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Programs = loaded.Value;
        Selected = null;

        _queue.ItemsSource = Programs
            .Select(program => new OgWellProgramRow(
                program.Name,
                program.ProspectName ?? program.ProspectId ?? string.Empty,
                program.Author ?? string.Empty,
                program.SubmittedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                program.AgeText,
                program.CostText,
                program.StateText,
                program.ProspectRiskComplete ? "complete" : "incomplete"))
            .ToList();

        _history.ItemsSource = null;
        HistoryRows = [];
        _detail.Text = "Select a program to see its objective, target and the decision panel.";

        var waiting = Programs.Count(program => program.IsPending);

        _header.Text =
            $"{Programs.Count.ToString(CultureInfo.InvariantCulture)} well program(s) · " +
            $"{waiting.ToString(CultureInfo.InvariantCulture)} awaiting a decision";

        _notice.Text = AlwaysVisibleNotice +
                       (Programs.Count == 0
                           ? " No well program is recorded for this field."
                           : string.Empty);

        ShowState(
            Programs.Count == 0 ? OgViewState.Empty : OgViewState.Ready,
            _notice.Text);

        return true;
    }

    /// <summary>Selects one program and shows its detail and history.</summary>
    public bool OpenProgram(string programId)
    {
        var loaded = _gate.LoadAsync(programId, DateTime.Today).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            Selected = null;
            _detail.Text = loaded.Message ?? "The well program could not be read.";
            ShowState(OgViewState.Failed, "The well program could not be read.", loaded.Reference);
            return false;
        }

        Selected = loaded.Value;
        RenderDetail(Selected);
        return true;
    }

    /// <summary>AC-1..AC-5: records the decision the panel describes.</summary>
    public async Task<bool> DecideAsync()
    {
        if (Selected is not { } program)
        {
            _notice.Text = "Select a well program first.";
            return false;
        }

        var decision = new OgWellProgramDecision(
            _decision.SelectedItem as string ?? string.Empty,
            _rationale.Text,
            _conditions.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            _application.Session.UserId);

        var decided = await _gate.DecideAsync(program, decision).ConfigureAwait(true);

        _notice.Text = decided.Success
            ? $"{program.Name}: {decision.ResultingState} recorded by {decision.Actor}."
            : decided.Message ?? "The decision was not recorded.";

        if (!decided.Success)
        {
            // AC-6: the service retains the decision; the page says a retry is available.
            _notice.Text += Pending is null ? string.Empty : " Retry re-sends the same decision.";
            ShowState(
                OgViewState.Ready,
                _notice.Text,
                Pending is null ? null : decided.Reference);
            return false;
        }

        _application.AuditLog.Add(
            "wellprogram.decide",
            "wellprogram",
            program.Id,
            "ok",
            $"{decision.ResultingState} by {decision.Actor}; conditions {_conditions.Text.Replace('\n', ';')}");

        Selected = decided.Value;
        Load();

        if (Selected is not null)
        {
            OpenProgram(Selected.Id);
        }

        return true;
    }

    /// <summary>AC-6: re-sends the retained decision unchanged.</summary>
    public async Task<bool> RetryAsync()
    {
        if (Pending is null)
        {
            _notice.Text = "There is no decision waiting to be retried.";
            return false;
        }

        var retried = await _gate.RetryAsync(DateTime.Today).ConfigureAwait(true);

        _notice.Text = retried.Success
            ? retried.Value is { } value
                ? $"{value.Name}: {value.StateText} recorded."
                : "The decision was recorded."
            : retried.Message ?? "The decision was not recorded.";

        if (retried.Success)
        {
            Selected = retried.Value;
            Load();
        }

        return retried.Success;
    }

    private void SelectFromQueue()
    {
        var index = _queue.SelectedIndex;

        if (index < 0 || index >= Programs.Count)
        {
            return;
        }

        OpenProgram(Programs[index].Id);
    }

    private void RenderDetail(OgWellProgram program)
    {
        _detail.Text =
            $"{program.Describe()}" +
            Environment.NewLine +
            $"Objective: {program.Objective ?? "(not recorded)"}" +
            Environment.NewLine +
            $"Target depth: {program.DepthText} · estimated cost: {program.CostText} · {program.RiskText}";

        if (program.HasConditions)
        {
            _detail.Text += Environment.NewLine + "Conditions: " + string.Join("; ", program.Conditions);
        }

        if (!program.IsPending && program.DecidedAt is { } decided)
        {
            _detail.Text += Environment.NewLine +
                            $"Decided by {program.DecidedBy ?? "(unknown)"} on " +
                            $"{decided.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}" +
                            (string.IsNullOrWhiteSpace(program.Rationale)
                                ? string.Empty
                                : $": {program.Rationale}");
        }

        HistoryRows = program.History
            .Select(entry => new OgWellProgramHistoryRow(
                entry.Decision,
                entry.ResultingState,
                entry.DecidedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                entry.Actor,
                entry.Rationale ?? string.Empty,
                string.Join("; ", entry.Conditions)))
            .ToList();

        _history.ItemsSource = HistoryRows;

        _decision.SelectedItem = program.IsPending
            ? OgWellProgramDecision.Approve
            : _decision.SelectedItem;

        _rationale.Text = program.Rationale ?? string.Empty;
        _conditions.Text = string.Join(Environment.NewLine, program.Conditions);

        ShowState(OgViewState.Ready, _notice.Text);
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
