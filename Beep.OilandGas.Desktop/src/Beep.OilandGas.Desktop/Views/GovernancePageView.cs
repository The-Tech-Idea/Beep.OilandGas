using System.Windows;
using System.Windows.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// DAT-08: the three questions an auditor asks - what changed, who read what, and is this copy in step.
/// </summary>
public sealed class GovernancePageView : OgPageBase
{
    public const string Route = "governance";

    private readonly OgApplication _application;
    private readonly IOgVersionStore _versions;
    private readonly IOgAccessAudit _audit;
    private readonly IOgAuditChain _chain;

    private readonly TextBox _reader;
    private readonly TextBox _table;
    private readonly TextBlock _auditSummary;
    private readonly DataGrid _auditGrid;
    private readonly DataGrid _conflicts;
    private readonly TextBlock _sync;
    private readonly TextBlock _chainState;
    private readonly TextBlock _notice;

    private List<OgSyncConflict> _open = [];
    private OgChainCheck? _chainCheck;

    public GovernancePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _versions = application.Versions;
        _audit = application.AccessAudit;
        _chain = application.AuditChain;

        _reader = new TextBox { MinWidth = 120, Margin = new Thickness(0, 0, 8, 0) };
        _table = new TextBox { MinWidth = 120, Margin = new Thickness(0, 0, 8, 0) };
        Named(_reader, "Reader filter");
        Named(_table, "Table filter");

        _auditSummary = Body(string.Empty);
        _sync = Body(string.Empty);
        _chainState = Body(string.Empty);
        _notice = Body(string.Empty);

        _auditGrid = Grid(
            ("When (UTC)", nameof(OgAccessAuditEntry.AtUtc), 150),
            ("Reader", nameof(OgAccessAuditEntry.Reader), 120),
            ("Table", nameof(OgAccessAuditEntry.Table), 140),
            ("Filter", nameof(OgAccessAuditEntry.Filter), 220),
            ("Rows", nameof(OgAccessAuditEntry.RowCount), 70));

        Named(_auditGrid, "Access audit");

        _conflicts = Grid(
            ("Table", nameof(OgSyncConflict.Table), 130),
            ("Key", nameof(OgSyncConflict.Key), 120),
            ("State", nameof(OgSyncConflict.Describe), 420));

        Named(_conflicts, "Sync conflicts");

        var auditBar = new WrapPanel { Margin = new Thickness(0, 4, 0, 6) };
        auditBar.Children.Add(Caption("Reader"));
        auditBar.Children.Add(_reader);
        auditBar.Children.Add(Caption("Table"));
        auditBar.Children.Add(_table);
        auditBar.Children.Add(Button("Apply filter", (_, _) => LoadAudit()));
        auditBar.Children.Add(Button("Export exactly this", (_, _) => ExportAudit()));

        var conflictBar = new WrapPanel { Margin = new Thickness(0, 4, 0, 6) };
        conflictBar.Children.Add(Button("Keep local", (_, _) => Resolve(OgConflictChoice.KeepLocal)));
        conflictBar.Children.Add(Button("Keep remote", (_, _) => Resolve(OgConflictChoice.KeepRemote)));

        var chainBar = new WrapPanel { Margin = new Thickness(0, 4, 0, 6) };
        chainBar.Children.Add(Button("Verify audit chain", (_, _) => VerifyChain()));
        chainBar.Children.Add(Button("Acknowledge break", (_, _) => AcknowledgeChain()));

        var panel = new StackPanel();
        panel.Children.Add(Heading("Synchronisation"));
        panel.Children.Add(_sync);
        panel.Children.Add(_conflicts);
        panel.Children.Add(conflictBar);
        panel.Children.Add(Heading("Access audit"));
        panel.Children.Add(auditBar);
        panel.Children.Add(_auditSummary);
        panel.Children.Add(_auditGrid);
        panel.Children.Add(Heading("Audit chain"));
        panel.Children.Add(_chainState);
        panel.Children.Add(chainBar);
        panel.Children.Add(_notice);

        ShowContent(panel);

        SyncStatus = new OgSyncStatus("company-api", OgSyncState.NeverRun);

        _sync.Text = $"{SyncStatus.Badge} - {SyncStatus.Describe()}";
        _chainState.Text = "Not checked yet.";
        _notice.Text = "Choose a conflict and a resolution; nothing is merged without one.";

        LoadAudit();
    }

    public OgSyncStatus SyncStatus { get; private set; }

    public IReadOnlyList<OgAccessAuditEntry> AuditRows { get; private set; } = [];

    public IReadOnlyList<OgSyncConflict> Conflicts => _open;

    public OgChainCheck? ChainCheck => _chainCheck;

    public string AuditSummaryText => _auditSummary.Text;

    public string NoticeText => _notice.Text;

    public string? ExportedPath { get; private set; }

    /// <summary>Replaces the pending conflicts (the sync service would hand these over).</summary>
    public void SetConflicts(IEnumerable<OgSyncConflict> conflicts)
    {
        _open = conflicts.ToList();
        _conflicts.ItemsSource = _open;
    }

    /// <summary>Replaces the sync status.</summary>
    public void SetSyncStatus(OgSyncStatus status)
    {
        SyncStatus = status ?? throw new ArgumentNullException(nameof(status), "A sync status is required.");
        _sync.Text = $"{status.Badge} - {status.Describe()}";
    }

    public bool LoadAudit()
    {
        var query = new OgAccessAuditQuery(
            Reader: string.IsNullOrWhiteSpace(_reader.Text) ? null : _reader.Text.Trim(),
            Table: string.IsNullOrWhiteSpace(_table.Text) ? null : _table.Text.Trim());

        AuditRows = _audit.Query(query);
        _auditGrid.ItemsSource = AuditRows;
        _auditSummary.Text = $"{AuditRows.Count} read(s) match the filter.";

        return true;
    }

    /// <summary>Exports exactly the rows the filter shows (AC-5).</summary>
    public bool ExportAudit()
    {
        try
        {
            var query = new OgAccessAuditQuery(
                Reader: string.IsNullOrWhiteSpace(_reader.Text) ? null : _reader.Text.Trim(),
                Table: string.IsNullOrWhiteSpace(_table.Text) ? null : _table.Text.Trim());

            var csv = _audit.ToCsv(query);

            Directory.CreateDirectory(OgPaths.ExportsDirectory);

            var path = Path.Combine(
                OgPaths.ExportsDirectory,
                $"access-audit-{DateTime.Now:yyyyMMdd-HHmmss}.csv");

            File.WriteAllText(path, csv);
            ExportedPath = path;
            _notice.Text = $"{AuditRows.Count} read(s) exported to {path}.";
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "exporting the access audit",
                "no file was written",
                FailureSeverity.Error);

            _notice.Text = $"The audit could not be exported. Reference: {reference}";
            return false;
        }
    }

    /// <summary>Resolves the selected conflict with an explicit, attributed choice (AC-4).</summary>
    public bool Resolve(OgConflictChoice choice)
    {
        if (_conflicts.SelectedItem is not OgSyncConflict conflict)
        {
            _notice.Text = "Select a conflict first.";
            return false;
        }

        var resolved = OgSyncConflicts.Resolve(conflict, choice, _application.Session.UserId);

        if (!resolved.Success)
        {
            _notice.Text = resolved.Message ?? "The conflict was not resolved.";
            return false;
        }

        _open = _open.Select(item => item.Id == conflict.Id ? resolved.Value! : item).ToList();
        _conflicts.ItemsSource = null;
        _conflicts.ItemsSource = _open;

        _notice.Text = $"{conflict.Table} / {conflict.Key} resolved as {choice} by {resolved.Value!.ResolvedBy}.";
        _application.AuditLog.Add(
            "sync.resolve",
            "conflict",
            conflict.Id,
            "ok",
            $"table={conflict.Table} key={conflict.Key} choice={choice} actor={resolved.Value.ResolvedBy}");

        return true;
    }

    /// <summary>Walks the chain and reports a break (AC-6).</summary>
    public bool VerifyChain()
    {
        _chainCheck = _chain.Verify();
        _chainState.Text = _chainCheck.Describe();

        if (!_chainCheck.IsIntact)
        {
            _application.AuditLog.Add(
                "audit.chain",
                "chain",
                "PPDM",
                "broken",
                $"entry={_chainCheck.BrokenAtSequence}");
        }

        return _chainCheck.IsIntact;
    }

    /// <summary>Acknowledges the current break, which re-opens governed writes (AC-6).</summary>
    public bool AcknowledgeChain()
    {
        if (_chainCheck is not { IsIntact: false } check)
        {
            _notice.Text = "There is no reported break to acknowledge.";
            return false;
        }

        var acknowledgement = new OgChainAcknowledgement(
            check.BrokenAtSequence!.Value,
            _application.Session.UserId,
            DateTimeOffset.UtcNow);

        _application.ChainGate.Acknowledge(check.BrokenAtSequence!.Value, acknowledgement.Actor);
        var allowed = _chain.WritesAllowed(check, acknowledgement);

        _chainState.Text = check.Describe() +
                           (allowed ? $" Acknowledged by {acknowledgement.Actor}; writes to governed tables are open." : string.Empty);

        _application.AuditLog.Add(
            "audit.chain.acknowledge",
            "chain",
            "PPDM",
            "ok",
            $"entry={acknowledgement.BrokenAtSequence} actor={acknowledgement.Actor}");

        return allowed;
    }

    private static DataGrid Grid(params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MaxHeight = 200,
            Margin = new Thickness(0, 4, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
        };

        foreach (var (header, path, width) in columns)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = header,
                Binding = new System.Windows.Data.Binding(path),
                Width = new DataGridLength(width),
            });
        }

        return grid;
    }

    private static TextBlock Heading(string text) => new()
    {
        Text = text,
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(0, 10, 0, 2),
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

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
    };
}
