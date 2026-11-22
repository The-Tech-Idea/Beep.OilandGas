using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Training;
using TheTechIdea.OilGas.Training;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// TRN-03: the sample pack a new installation can install and run, and the optional local training server for trainees on other
/// machines.
/// </summary>
/// <remarks>
/// The pack is the tools' <c>SamplePacks</c> and the server is the tools' <c>TrainingServer</c>. The page adds where the pack goes, the
/// zip round trip, the exposure decision and the session view. The server is <b>off until it is asked for</b> and bound to loopback
/// unless a person confirms otherwise, because it has no sign-in (AC-3); a port another process holds is refused before anything is
/// opened (AC-4); and the sessions shown are the names the server has heard from, counted through its request observer - not
/// authenticated sessions, and the page says so (AC-5).
/// </remarks>
public sealed class TrainingServerPageView : OgPageBase
{
    public const string Route = "trainingserver";

    private readonly OgApplication _application;
    private readonly IOgTrainingServer _server;

    private readonly TextBlock _header;
    private readonly TextBlock _summary;
    private readonly TextBlock _pack;
    private readonly TextBlock _transfer;
    private readonly TextBlock _state;
    private readonly TextBlock _security;
    private readonly TextBlock _sessions;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _port;
    private readonly TextBox _bind;
    private readonly TextBox _zipPath;
    private readonly TextBox _importFolder;
    private readonly CheckBox _expose;
    private readonly DataGrid _sessionGrid;
    private readonly Button _installButton;
    private readonly Button _exportButton;
    private readonly Button _importButton;
    private readonly Button _startButton;
    private readonly Button _stopButton;
    private readonly Button _statusButton;
    private readonly Button _folderButton;

    private OgTrainingServer? _state_;

    public TrainingServerPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _server = application.TrainingServer;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.FontWeight = FontWeights.SemiBold;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _pack = Body(string.Empty);
        _pack.TextWrapping = TextWrapping.Wrap;
        _pack.Margin = new Thickness(0, 0, 0, 4);

        _transfer = Body(string.Empty);
        _transfer.TextWrapping = TextWrapping.Wrap;
        _transfer.Margin = new Thickness(0, 0, 0, 4);

        _state = Body(string.Empty);
        _state.TextWrapping = TextWrapping.Wrap;
        _state.Margin = new Thickness(0, 4, 0, 4);

        _security = Body(string.Empty);
        _security.TextWrapping = TextWrapping.Wrap;
        _security.FontWeight = FontWeights.SemiBold;
        _security.Margin = new Thickness(0, 0, 0, 4);

        _sessions = Body(string.Empty);
        _sessions.TextWrapping = TextWrapping.Wrap;
        _sessions.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _port = Field("Port (0 picks a free one)", 110);
        _port.Text = TrainingToolbox.Settings.Server.Port.ToString(CultureInfo.InvariantCulture);

        _bind = Field("Bind address", 130);
        _bind.Text = OgTrainingServerRules.LoopbackBind;

        _zipPath = Field("Pack file", 300);
        _importFolder = Field("Import into", 300);

        _expose = new CheckBox
        {
            Content = "I understand the server has no sign-in and confirm binding beyond this machine",
            Margin = new Thickness(0, 0, 12, 0),
        };

        Named(_expose, "Confirm network exposure");

        _sessionGrid = Grid("Sessions", ("Trainee", "Trainee", 180), ("Requests", "Requests", 90),
            ("Attempts", "Attempts", 90), ("First seen", "FirstSeen", 150), ("Last seen", "LastSeen", 150),
            ("Minutes", "Minutes", 80));

        _installButton = Button("Install the sample pack", (_, _) => _ = InstallAsync());
        _exportButton = Button("Export pack", (_, _) => _ = ExportAsync());
        _importButton = Button("Import pack", (_, _) => _ = ImportAsync());
        _startButton = Button("Start server", (_, _) => _ = StartAsync());
        _stopButton = Button("Stop", (_, _) => _ = StopAsync());
        _statusButton = Button("Status", (_, _) => _ = StatusAsync());
        _folderButton = Button("Default pack path", (_, _) => DefaultPaths());

        var packBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        packBar.Children.Add(_installButton);
        packBar.Children.Add(Caption("Pack"));
        packBar.Children.Add(_zipPath);
        packBar.Children.Add(_exportButton);
        packBar.Children.Add(_importButton);

        var serverBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        serverBar.Children.Add(Caption("Port"));
        serverBar.Children.Add(_port);
        serverBar.Children.Add(Caption("Bind"));
        serverBar.Children.Add(_bind);
        serverBar.Children.Add(_startButton);
        serverBar.Children.Add(_stopButton);
        serverBar.Children.Add(_statusButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(packBar);
        panel.Children.Add(_pack);
        panel.Children.Add(_transfer);
        panel.Children.Add(Caption("The server"));
        panel.Children.Add(serverBar);
        panel.Children.Add(_expose);
        panel.Children.Add(_state);
        panel.Children.Add(Caption("What this exposes"));
        panel.Children.Add(_security);
        panel.Children.Add(Caption("Trainees seen since it started"));
        panel.Children.Add(_sessionGrid);
        panel.Children.Add(_sessions);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "The server is off. Install the sample pack, or start the server over your own scenarios.");
    }

    public OgTrainingServer? ServerState => _state_;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string SummaryText => _summary.Text;

    public string PackText => _pack.Text;

    public string TransferText => _transfer.Text;

    public string StateText => _state.Text;

    public string SecurityText => _security.Text;

    public string SessionsText => _sessions.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public bool ServerRunning => _state_?.Running ?? false;

    public int SessionCount => _state_?.SessionCount ?? 0;

    public bool Load()
    {
        _header.Text = "Training · server and packs";

        var status = _server.StatusAsync().GetAwaiter().GetResult();

        if (!status.Success || status.Value is null)
        {
            LastFailed = true;
            _failure.Text = status.Message ?? "The server's state could not be read.";
            ShowState(OgViewState.Failed, "The server's state is not shown.", status.Reference);
            return false;
        }

        LastFailed = false;
        Apply(status.Value);

        DefaultPaths();

        return true;
    }

    public async Task<bool> InstallAsync()
    {
        var installed = await _server.InstallSamplePackAsync(_application.Session.UserId).ConfigureAwait(true);

        if (!installed.Success || installed.Value is null)
        {
            LastFailed = true;
            _failure.Text = installed.Message ?? "The sample pack was not installed.";
            ShowState(OgViewState.Failed, "The pack is not installed.", installed.Reference);
            return false;
        }

        LastFailed = false;
        _pack.Text = installed.Value.Describe() + Environment.NewLine +
                     (installed.Value.Notes.Count == 0
                         ? "The pack is in the folder the scenarios page reads, so its exercises are listed there."
                         : string.Join(Environment.NewLine, installed.Value.Notes.Select(note => "! " + note)));

        _application.AuditLog.Add("training.pack.install", "training", installed.Value.Folder, "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = installed.Value.Reinstalled
            ? "Sample pack reinstalled - the scenarios page will now list it."
            : "Sample pack installed - the scenarios page will now list it.";

        return true;
    }

    public async Task<bool> ExportAsync()
    {
        var path = string.IsNullOrWhiteSpace(_zipPath.Text) ? DefaultZip() : _zipPath.Text!.Trim();

        var exported = await _server.ExportPackAsync(_application.Session.UserId is null ? string.Empty : Folder(), path)
            .ConfigureAwait(true);

        if (!exported.Success || exported.Value is null)
        {
            LastFailed = true;
            _transfer.Text = exported.Message ?? "The pack was not exported.";
            _failure.Text = exported.Message ?? "The pack was not exported.";
            ShowState(OgViewState.Failed, "The pack is not exported.", exported.Reference);
            return false;
        }

        LastFailed = false;
        _zipPath.Text = exported.Value.Path;
        _transfer.Text = exported.Value.Describe() + Environment.NewLine + exported.Value.ProblemsText;
        _failure.Text = string.Empty;

        _application.AuditLog.Add("training.pack.export", "training", exported.Value.Path, "ok",
            _application.Session.UserId);

        _notice.Text = "Pack written to " + exported.Value.Path + ".";

        return true;
    }

    public async Task<bool> ImportAsync()
    {
        var path = _zipPath.Text?.Trim() ?? string.Empty;
        var folder = string.IsNullOrWhiteSpace(_importFolder.Text) ? Folder() : _importFolder.Text!.Trim();

        var imported = await _server.ImportPackAsync(path, folder).ConfigureAwait(true);

        if (!imported.Success || imported.Value is null)
        {
            LastFailed = true;
            _transfer.Text = imported.Message ?? "The pack was not imported.";
            _failure.Text = imported.Message ?? "The pack was not imported.";
            ShowState(OgViewState.Failed, "The pack is not imported.", imported.Reference);
            return false;
        }

        LastFailed = false;
        _transfer.Text = imported.Value.Describe() + Environment.NewLine + imported.Value.ProblemsText;
        _failure.Text = string.Empty;

        _application.AuditLog.Add("training.pack.import", "training", imported.Value.Path, "ok",
            _application.Session.UserId);

        _notice.Text = "Pack read back into " + imported.Value.Path + ": " +
                       imported.Value.Scenarios.ToString(CultureInfo.InvariantCulture) + " scenario(s) and " +
                       imported.Value.Courses.ToString(CultureInfo.InvariantCulture) + " course(s).";

        return true;
    }

    public async Task<bool> StartAsync()
    {
        var started = await _server
            .StartAsync(new OgServerRequest(Port(), _bind.Text?.Trim() ?? string.Empty, _expose.IsChecked == true))
            .ConfigureAwait(true);

        if (!started.Success || started.Value is null)
        {
            LastFailed = true;
            Apply(_state_ ?? Empty());
            _failure.Text = started.Message ?? "The server was not started.";
            ShowState(OgViewState.Failed, "The server is not running.", started.Reference);
            return false;
        }

        LastFailed = false;
        Apply(started.Value);

        _failure.Text = string.Empty;

        _application.AuditLog.Add("training.server.start", "training", started.Value.Url ?? "(not running)", "ok",
            _application.Session.UserId);

        _notice.Text = started.Value.Running
            ? "Server listening on " + started.Value.Url + ". Share that address with the trainees on this network."
            : "The server did not start: " + (started.Value.Failure ?? "no reason was given");

        return started.Value.Running;
    }

    public async Task<bool> StopAsync()
    {
        var stopped = await _server.StopAsync().ConfigureAwait(true);

        if (!stopped.Success || stopped.Value is null)
        {
            LastFailed = true;
            _failure.Text = stopped.Message ?? "The server was not stopped.";
            return false;
        }

        LastFailed = false;
        Apply(stopped.Value);

        _application.AuditLog.Add("training.server.stop", "training", "stopped", "ok", _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = "Server stopped. The trainees it saw are still listed above.";

        return true;
    }

    public async Task<bool> StatusAsync()
    {
        var status = await _server.StatusAsync().ConfigureAwait(true);

        if (!status.Success || status.Value is null)
        {
            LastFailed = true;
            _failure.Text = status.Message ?? "The server's state could not be read.";
            return false;
        }

        LastFailed = false;
        Apply(status.Value);

        return status.Value.Running;
    }

    /// <summary>The two paths the page offers by default: the pack file and the folder a pack is imported into.</summary>
    public void DefaultPaths()
    {
        if (string.IsNullOrWhiteSpace(_zipPath.Text))
        {
            _zipPath.Text = DefaultZip();
        }

        if (string.IsNullOrWhiteSpace(_importFolder.Text))
        {
            _importFolder.Text = Folder();
        }
    }

    private void Apply(OgTrainingServer state)
    {
        _state_ = state;

        _summary.Text = "Running: " + (state.Running ? "yes" : "no") + " · " +
                        (state.Url ?? "(no address)") + " · port " +
                        (state.Port > 0 ? state.Port.ToString(CultureInfo.InvariantCulture) : "(chosen at start)") +
                        " · " + state.SessionCount.ToString(CultureInfo.InvariantCulture) + " trainee(s) seen";

        _state.Text = state.StateText;
        _security.Text = state.SecurityText;
        _sessions.Text = state.SessionsText;
        _refreshed.Text = "read at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) +
                          (state.LedgerPath is null ? string.Empty : " · ledger " + state.LedgerPath);

        _sessionGrid.ItemsSource = state.Sessions.ToList();

        _startButton.IsEnabled = !state.Running;
        _stopButton.IsEnabled = state.Running;
        _expose.IsChecked = state.ExposureConfirmed;

        ShowState(state.Running ? OgViewState.Ready : OgViewState.Empty, state.StateText);

        if (!state.Running && state.Failure is not null)
        {
            _failure.Text = state.Failure;
        }
    }

    private int Port() =>
        int.TryParse(_port.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var port) ? port : 0;

    /// <summary>The scenario folder the page works against, taken from the configured list.</summary>
    private static string Folder()
    {
        TrainingToolbox.Register();

        var folders = TrainingToolbox.Settings.ScenarioFolders;

        return folders.Count == 0 ? string.Empty : folders[0];
    }

    private static string DefaultZip() =>
        Path.Combine(OgPaths.ExportsDirectory,
            "training-pack-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + ".zip");

    private static OgTrainingServer Empty() =>
        new(false, null, 0, OgTrainingServerRules.LoopbackBind, true, false, [], 0, 0, null, null);

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 190,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
        };

        foreach (var (header, path, width) in columns)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding(path),
                Width = new DataGridLength(width),
            });
        }

        Named(grid, name);
        return grid;
    }

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
        Margin = new Thickness(0, 6, 0, 0),
        FontSize = 11.5,
        FontWeight = FontWeights.SemiBold,
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
}
