using System.Net.Http;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>One labelled fact on the OSDU page.</summary>
public sealed record OgOsduRow(string Field, string Value);

/// <summary>
/// INT-03: export OSDU records, see the ingest plan, send it, retry only what failed, and review duplicate
/// well proposals without merging them.
/// </summary>
/// <remarks>
/// A dry run is the default: planning is pure and only <b>Send</b> touches the network. The token lives in this
/// window for the session only — persisting it needs the plan's DPAPI store, which does not exist (ADR-014
/// chose a one-way derivation for sign-in).
/// </remarks>
/// <summary>A reconciliation a person accepted or rejected: what was decided, by whom, and when (INT-03 §3, AC-3).</summary>
public sealed record OgOsduDecision(string Left, string Right, string Decision, string By, DateTime At)
{
    public string Describe() => Decision + " · " + Left + " / " + Right + " · " + By + " · " +
                                At.ToLocalTime().ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
}

public sealed class OsduInteropPageView : OgPageBase
{
    private readonly OgApplication _application;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly DataGrid _facts;
    private readonly DataGrid _candidates;
    private readonly CheckBox _kindWellbore;
    private readonly CheckBox _kindTrajectory;
    private readonly CheckBox _kindLog;
    private readonly TextBlock _decisions;
    private readonly Button _sendButton;
    private readonly Button _retryButton;
    private readonly Button _acceptButton;
    private readonly Button _rejectButton;

    private IReadOnlyList<JsonObject> _records = [];
    private OgOsduResult? _lastSend;

    public OsduInteropPageView(OgApplication application)
        : base(application, "osdu")
    {
        _application = application;
        _ = application.Wellbore;

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _facts = new DataGrid
        {
            IsTabStop = true, // REL-03 AC-1: a read-only grid must be reachable by Tab.
            AutoGenerateColumns = false,
            IsReadOnly = true,
            MaxHeight = 150,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Field", nameof(OgOsduRow.Field), 130),
                Star("Value", nameof(OgOsduRow.Value)),
            },
        };

        _candidates = new DataGrid
        {
            IsTabStop = true, // REL-03 AC-1: a read-only grid must be reachable by Tab.
            AutoGenerateColumns = false,
            IsReadOnly = true,
            MaxHeight = 150,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("A", nameof(OgOsduCandidate.A), 150),
                Text("B", nameof(OgOsduCandidate.B), 150),
                Text("Score", nameof(OgOsduCandidate.Score), 70),
                Star("Evidence", nameof(OgOsduCandidate.Reasons)),
            },
        };

        var token = new PasswordBox { MinWidth = 220, Margin = new Thickness(0, 0, 8, 0) };
        token.PasswordChanged += (_, _) => AccessToken = token.Password;

        _decisions = Body(string.Empty);
        _decisions.TextWrapping = TextWrapping.Wrap;
        _decisions.Margin = new Thickness(0, 0, 0, 8);

        // INT-03 §3: the Export tab selects the record kinds, and the send commands are only offered when the plan can be sent.
        _kindWellbore = new CheckBox { Content = "Wellbore", IsChecked = true, Margin = new Thickness(0, 0, 12, 0) };
        _kindTrajectory = new CheckBox { Content = "WellboreTrajectory", IsChecked = true, Margin = new Thickness(0, 0, 12, 0) };
        _kindLog = new CheckBox { Content = "WellLog", IsChecked = true, Margin = new Thickness(0, 0, 12, 0) };

        _sendButton = Button("Send", async (_, _) => await SendAsync());
        _retryButton = Button("Retry failed", async (_, _) => await RetryAsync());
        _acceptButton = Button("Accept selected", (_, _) => Decide(accepted: true));
        _rejectButton = Button("Reject selected", (_, _) => Decide(accepted: false));

        _sendButton.IsEnabled = false;
        _retryButton.IsEnabled = false;

        var kindRow = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        kindRow.Children.Add(Caption("Kinds"));
        kindRow.Children.Add(_kindWellbore);
        kindRow.Children.Add(_kindTrajectory);
        kindRow.Children.Add(_kindLog);

        var exportRow = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        exportRow.Children.Add(Button("Export JSON", (_, _) => ExportJson()));
        exportRow.Children.Add(Button("Dry run", (_, _) => DryRun()));
        exportRow.Children.Add(Caption("Token"));
        exportRow.Children.Add(token);
        exportRow.Children.Add(_sendButton);
        exportRow.Children.Add(_retryButton);
        exportRow.Children.Add(Button("Reconcile", (_, _) => Reconcile()));

        var decideRow = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        decideRow.Children.Add(Caption("Reconciliation"));
        decideRow.Children.Add(_acceptButton);
        decideRow.Children.Add(_rejectButton);

        var panel = new StackPanel();
        panel.Children.Add(kindRow);
        panel.Children.Add(exportRow);
        panel.Children.Add(decideRow);
        panel.Children.Add(_summary);
        panel.Children.Add(_notice);
        panel.Children.Add(_facts);
        panel.Children.Add(_candidates);
        panel.Children.Add(_decisions);

        Named(_facts, "OSDU plan");
        Named(_candidates, "Duplicate candidates");
        Named(token, "Access token");
        ShowContent(panel);
    }

    /// <summary>The token for a send. Session only; never written anywhere.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>The records the page is working with; the smoke run drives the bridge with these.</summary>
    public IReadOnlyList<JsonObject> Records => _records;

    public OgOsduPlan? Plan { get; private set; }

    public OgOsduResult? Result => _lastSend;

    public IReadOnlyList<OgOsduCandidate> Candidates { get; private set; } = [];

    public IReadOnlyList<OgOsduRow> Rows { get; private set; } = [];

    public string? ExportedPath { get; private set; }

    public string SummaryText => _summary.Text;

    /// <summary>Writes the records as JSON and reports the per-record validity check.</summary>
    public bool ExportJson()
    {
        try
        {
            // INT-03's "schema-invalid record" state: the export refuses and names the record, rather than writing a file the platform rejects.
            var exported = OgOsduWorkspace.Export(OgOsduWorkspace.ExampleWell(), SelectedKinds());

            if (!exported.Success || exported.Value is null)
            {
                _notice.Text = exported.Message ?? "The export was refused.";
                ShowState(OgViewState.Failed, "The OSDU export is refused.", exported.Reference);
                return false;
            }

            _records = exported.Value.Records;

            var folder = Path.Combine(OgPaths.ExportsDirectory, "osdu");
            Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, $"osdu-{DateTime.Now:yyyyMMdd-HHmmss}.json");
            File.WriteAllText(path, exported.Value.Json);

            ExportedPath = path;
            Plan = OgOsduWorkspace.Plan(_records);
            Show(Plan, exported.Value.Problems, exported.Value.Warnings);
            GateSend();

            _summary.Text =
                $"Exported {_records.Count} record(s) of {SelectedKinds().Count} kind(s) · " +
                $"{new FileInfo(path).Length / 1024.0:0.#} KB · {path}";
            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "exporting OSDU records",
                "no file was written",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The OSDU export failed.", reference);
            return false;
        }
    }

    /// <summary>Plans without sending (AC-5).</summary>
    public bool DryRun()
    {
        try
        {
            EnsureRecords();

            Plan = OgOsduWorkspace.Plan(_records);
            Show(Plan, OgOsduWorkspace.Validate(_records), []);

            _notice.Text = Plan.CanSend
                ? "Dry run: nothing was sent. Send uses the token above."
                : $"Dry run: the plan cannot be sent. {string.Join("  ·  ", Plan.Problems)}";

            GateSend();

            return Plan.CanSend;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "planning an OSDU ingest",
                "no plan was produced",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The OSDU plan could not be built.", reference);
            return false;
        }
    }

    /// <summary>Sends the plan. A handler lets a test or the smoke run drive it without a platform.</summary>
    public async Task<bool> SendAsync(HttpMessageHandler? handler = null)
    {
        try
        {
            EnsureRecords();

            using var http = handler is null ? new HttpClient() : new HttpClient(handler);

            _lastSend = await OgOsduWorkspace
                .SendAsync(_records, AccessToken, http)
                .ConfigureAwait(true);

            Plan = OgOsduWorkspace.Plan(_records);
            Report(_lastSend);

            return _lastSend.Succeeded;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "sending OSDU records",
                "the records were not ingested",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The OSDU send failed.", reference);
            return false;
        }
    }

    /// <summary>Resends only the batches that failed or were never attempted (AC-2).</summary>
    public async Task<bool> RetryAsync(HttpMessageHandler? handler = null)
    {
        if (_lastSend is null)
        {
            _notice.Text = "Nothing has been sent yet.";
            return false;
        }

        try
        {
            EnsureRecords();

            using var http = handler is null ? new HttpClient() : new HttpClient(handler);

            _lastSend = await OgOsduWorkspace
                .RetryFailedAsync(_records, _lastSend, AccessToken, http)
                .ConfigureAwait(true);

            Report(_lastSend);
            return _lastSentSucceeded;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "retrying the failed OSDU batches",
                "the retry did not complete",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The OSDU retry failed.", reference);
            return false;
        }
    }

    /// <summary>Proposes duplicate wells; nothing is merged (AC-3).</summary>
    public bool Reconcile()
    {
        try
        {
            var first = OgOsduWorkspace.ExampleWell();
            var second = OgOsduWorkspace.ExampleWell();
            second.Header.Id = first.Header.Id + "-COPY";

            Candidates = OgOsduWorkspace.Reconcile([first, second], "desktop");
            _candidates.ItemsSource = Candidates;

            _notice.Text = Candidates.Count == 0
                ? "No duplicate candidates were found."
                : $"{Candidates.Count} candidate pair(s) proposed; nothing is merged. Accepting a match needs the crosswalk store (DAT-02).";

            return Candidates.Count > 0;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "proposing duplicate wells",
                "no candidates were produced",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "Reconciliation could not run.", reference);
            return false;
        }
    }

    private bool _lastSentSucceeded;

    private void Report(OgOsduResult result)
    {
        _lastSentSucceeded = result.Succeeded;
        _retryButton.IsEnabled = result.CanRetry;

        _summary.Text = result.Describe();
        _notice.Text = result.Refusal
                       ?? (result.TokenExpired
                           ? "The platform rejected the token. Sign in again; this is not retried automatically."
                           : result.CanRetry
                               ? "Use Retry failed to resend only the batches that did not succeed."
                               : string.Empty);

        Rows = result.Batches
            .Select(batch => new OgOsduRow(
                $"Batch {batch.Index}",
                $"{batch.State} · HTTP {batch.StatusCode} · {batch.RecordCount} record(s)"))
            .ToList();

        _facts.ItemsSource = Rows;
    }

    private void Show(OgOsduPlan plan, IReadOnlyList<OgOsduProblem> problems, IReadOnlyList<string> warnings)
    {
        Rows =
        [
            new("Records", plan.RecordCount.ToString()),
            new("Batches", plan.BatchCount.ToString()),
            new("Kinds", plan.Kinds.Count == 0 ? "(none)" : string.Join(", ", plan.Kinds)),
            new("Can send", plan.CanSend ? "yes" : "no"),
            new("Problems", plan.Problems.Count == 0 ? "(none)" : string.Join("  ·  ", plan.Problems)),
            new("Record checks", problems.Count == 0 ? "(none)" : string.Join("  ·  ", problems.Select(problem => $"{problem.RecordId}: {problem.Problem}"))),
        ];

        _facts.ItemsSource = Rows;

        if (warnings.Count > 0)
        {
            _notice.Text = string.Join("  ·  ", warnings);
        }
    }

    private void EnsureRecords()
    {
        if (_records.Count == 0)
        {
            _records = OgOsduWorkspace.Records(OgOsduWorkspace.ExampleWell(), SelectedKinds(), out _);
        }
    }

    /// <summary>What was accepted or rejected on this page, in order. A decision is recorded; nothing is merged (AC-3).</summary>
    public List<OgOsduDecision> Decisions { get; } = [];

    /// <summary>The record kinds the person has selected for the export.</summary>
    public IReadOnlyList<OgOsduRecordKind> SelectedKinds()
    {
        var kinds = new List<OgOsduRecordKind>();

        if (_kindWellbore.IsChecked == true)
        {
            kinds.Add(OgOsduRecordKind.Wellbore);
        }

        if (_kindTrajectory.IsChecked == true)
        {
            kinds.Add(OgOsduRecordKind.WellboreTrajectory);
        }

        if (_kindLog.IsChecked == true)
        {
            kinds.Add(OgOsduRecordKind.WellLog);
        }

        return kinds;
    }

    /// <summary>
    /// Accepts or rejects the selected candidate. This is the only path that changes anything about a reconciliation, it needs a person to
    /// press it, and it records the decision rather than merging two wells - the crosswalk itself is a persisted record (DAT-02).
    /// </summary>
    public bool Decide(bool accepted)
    {
        if (_candidates.SelectedItem is not OgOsduCandidate candidate)
        {
            _notice.Text = "Select a candidate pair first; nothing is decided on its own.";
            return false;
        }

        var decision = new OgOsduDecision(
            candidate.A,
            candidate.B,
            accepted ? "accepted" : "rejected",
            _application.Session.UserId,
            DateTime.UtcNow);

        Decisions.Add(decision);

        _application.AuditLog.Add(
            "interop.osdu.reconcile",
            "interop",
            candidate.A + "/" + candidate.B,
            accepted ? "accepted" : "rejected",
            _application.Session.UserId);

        _decisions.Text = string.Join(Environment.NewLine, Decisions.Select(item => "· " + item.Describe())) +
                          Environment.NewLine +
                          "Recorded, not merged: the crosswalk a merge would write is a persisted record (DAT-02), so the wells are unchanged.";

        return true;
    }

    /// <summary>The send commands follow the plan: they are offered only when it can be sent, with the reason on the page when it cannot.</summary>
    private void GateSend()
    {
        var canSend = Plan?.CanSend == true;

        _sendButton.IsEnabled = canSend;
        _retryButton.IsEnabled = canSend && Result?.CanRetry == true;

        if (Plan is not null && !canSend && Plan.Problems.Count > 0)
        {
            _notice.Text = "Sending is disabled: " + string.Join("  ·  ", Plan.Problems) +
                           " (set dataai.osduStorageUrl in the environment).";
        }
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
        Binding = new Binding(path) { StringFormat = "{0:0.##}" },
        Width = new DataGridLength(width),
    };

    private static DataGridTextColumn Star(string header, string path) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
    };
}
