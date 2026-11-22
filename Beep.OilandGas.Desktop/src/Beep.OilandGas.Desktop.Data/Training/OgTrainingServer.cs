using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Training;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Training;

/// <summary>What a sample pack install did (TRN-03, AC-1).</summary>
/// <param name="Reinstalled">True when the folder already held the pack, so the files were replaced - the feature's "reinstall" state.</param>
public sealed record OgPackInstall(
    string Folder,
    int Files,
    int Scenarios,
    int Courses,
    bool Reinstalled,
    IReadOnlyList<string> Written,
    IReadOnlyList<string> Notes)
{
    public string Describe() => Files.ToString(CultureInfo.InvariantCulture) + " file(s) - " +
                                Scenarios.ToString(CultureInfo.InvariantCulture) + " scenario(s) and " +
                                Courses.ToString(CultureInfo.InvariantCulture) + " course(s) - " +
                                (Reinstalled ? "reinstalled into " : "installed into ") + Folder;
}

/// <summary>What an export or import did (TRN-03, AC-6).</summary>
public sealed record OgPackTransfer(
    string Path,
    bool Exported,
    int Files,
    int Scenarios,
    int Courses,
    long Bytes,
    IReadOnlyList<string> Problems,
    string Message)
{
    public string Describe() => (Exported ? "exported " : "imported ") + Files.ToString(CultureInfo.InvariantCulture) +
                                " file(s) · " + Scenarios.ToString(CultureInfo.InvariantCulture) + " scenario(s) · " +
                                Courses.ToString(CultureInfo.InvariantCulture) + " course(s) · " +
                                (Bytes / 1024d).ToString("0.#", CultureInfo.InvariantCulture) + " KB · " + Path;

    public string ProblemsText => Problems.Count == 0
        ? "Every file was read."
        : string.Join(Environment.NewLine, Problems.Select(problem => "! " + problem));
}

/// <summary>One trainee the server has heard from since it started (TRN-03, AC-5).</summary>
/// <param name="Requests">Requests carrying this name - the count the server's observer saw.</param>
public sealed record OgServerSession(
    string Trainee,
    int Requests,
    int Attempts,
    DateTime FirstSeen,
    DateTime LastSeen)
{
    public double Minutes => Math.Max(0d, (LastSeen - FirstSeen).TotalMinutes);

    public string Describe() => (string.IsNullOrWhiteSpace(Trainee) ? "(no name given)" : Trainee) + " · " +
                                Requests.ToString(CultureInfo.InvariantCulture) + " request(s) · " +
                                Attempts.ToString(CultureInfo.InvariantCulture) + " attempt(s) · last seen " +
                                LastSeen.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The training server's state (TRN-03's <c>OgTrainingServer</c>).</summary>
public sealed record OgTrainingServer(
    bool Running,
    string? Url,
    int Port,
    string Bind,
    bool Loopback,
    bool ExposureConfirmed,
    IReadOnlyList<OgServerSession> Sessions,
    int RequestCount,
    int Attempts,
    string? LedgerPath,
    string? Failure)
{
    public int SessionCount => Sessions.Count;

    public string StateText => Running
        ? "Listening on " + Url + " · " + SessionCount.ToString(CultureInfo.InvariantCulture) + " trainee(s) seen · " +
          RequestCount.ToString(CultureInfo.InvariantCulture) + " request(s) · " +
          Attempts.ToString(CultureInfo.InvariantCulture) + " attempt(s) recorded"
        : "Stopped." + (Failure is null ? string.Empty : " " + Failure);

    public string SecurityText => Loopback
        ? "Bound to " + Bind + ", which is this machine only. The server has no sign-in: a trainee's name is typed, not verified, so " +
          "it is a practice tool and not an exam."
        : "Bound to " + Bind + ", which other machines can reach. There is NO sign-in, so anyone who can reach the port can read the " +
          "exercises and record attempts in any name. Put it behind your own access control, and stop it when the session ends.";

    public string SessionsText => Sessions.Count == 0
        ? "No trainee has made a request since the server started."
        : string.Join(Environment.NewLine, Sessions.Select(session => "· " + session.Describe()));
}

/// <summary>What the server page is asked for, and what it may do (TRN-03).</summary>
public sealed record OgServerRequest(int Port, string Bind, bool ConfirmExposure);

/// <summary>The training rules TRN-03 states once.</summary>
public static class OgTrainingServerRules
{
    public const string CodeNoFolder = "pack-no-folder";
    public const string CodeInstallFailed = "pack-install-failed";
    public const string CodeActorRequired = "pack-actor-required";
    public const string CodeAlreadyRunning = "server-already-running";
    public const string CodeNotRunning = "server-not-running";
    public const string CodePortInUse = "server-port-in-use";
    public const string CodeExposure = "server-exposure-unconfirmed";
    public const string CodeBind = "server-bind-invalid";
    public const string CodeStartFailed = "server-start-failed";
    public const string CodeZip = "pack-zip-failed";
    public const string CodeTransferPath = "pack-transfer-path";
    public const string CodeReadFailed = "pack-read-failed";

    /// <summary>The port the page offers: the environment's, or 0 when it names none so the system picks a free one.</summary>
    public const string LoopbackBind = "127.0.0.1";

    /// <summary>Whether a bind address is one this machine only - the server's own question, so the page cannot answer it differently.</summary>
    public static bool IsLoopback(string? host) => TrainingServer.IsLoopback(host ?? string.Empty);

    /// <summary>
    /// AC-3: binding beyond loopback is refused unless it was confirmed. The exposure is a decision, not a default, because the server
    /// has no sign-in.
    /// </summary>
    public static bool NeedsConfirmation(string? host, bool confirmed) => !IsLoopback(host) && !confirmed;

    /// <summary>
    /// AC-4: whether a port is free on an address. Asked of the operating system's own list of listeners rather than by trying to bind
    /// and catching the failure - so the answer is a question, not an exception, and nothing has to be opened to find out.
    /// </summary>
    public static bool PortFree(string host, int port)
    {
        if (port <= 0)
        {
            return true;
        }

        var address = IPAddress.TryParse(host.Trim('[', ']'), out var ip) ? ip : IPAddress.Loopback;

        var taken = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties()
            .GetActiveTcpListeners()
            .Where(endpoint => endpoint.Port == port)
            .Any(endpoint => endpoint.Address.Equals(IPAddress.Any) || endpoint.Address.Equals(IPAddress.IPv6Any) ||
                             endpoint.Address.Equals(address));

        return !taken;
    }

    /// <summary>The sentence a port conflict is refused with, naming the port the feature asks for.</summary>
    public static string PortInUse(int port, string host) =>
        "Port " + port.ToString(CultureInfo.InvariantCulture) + " on " + host +
        " is already in use, so the server is not started. Choose another port, or 0 to let the system pick a free one.";
}

/// <summary>The package operations TRN-03 needs: the sample pack, a zip of a folder, and the server.</summary>
public interface IOgTrainingServer
{
    /// <summary>AC-1: writes the sample pack into the scenario folder and reports what it found there first.</summary>
    Task<OgResult<OgPackInstall>> InstallSamplePackAsync(string actor, CancellationToken cancellationToken = default);

    /// <summary>AC-6: zips a scenario folder - the scenario, well and course files together.</summary>
    Task<OgResult<OgPackTransfer>> ExportPackAsync(string folder, string zipPath, CancellationToken cancellationToken = default);

    /// <summary>AC-6: unpacks a zip into a folder and reads it back, so the round trip is measured rather than assumed.</summary>
    Task<OgResult<OgPackTransfer>> ImportPackAsync(string zipPath, string folder, CancellationToken cancellationToken = default);

    /// <summary>AC-2, AC-3, AC-4 and AC-5: starts the server, or reports why it did not.</summary>
    Task<OgResult<OgTrainingServer>> StartAsync(OgServerRequest request, CancellationToken cancellationToken = default);

    /// <summary>Stops the server; the sessions it saw are kept for the report.</summary>
    Task<OgResult<OgTrainingServer>> StopAsync(CancellationToken cancellationToken = default);

    /// <summary>The server's state as it stands, including who has been seen.</summary>
    Task<OgResult<OgTrainingServer>> StatusAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// TRN-03 over the tools' <see cref="SamplePacks"/>, <see cref="TrainingserverPlaceholder"/>, <see cref="TrainingServerOptions"/> and the
/// folder loaders TRN-01 already uses.
/// </summary>
/// <remarks>
/// <para>
/// The sample pack is <see cref="SamplePacks.Write"/> and the server is <see cref="TrainingServer"/>; the desktop adds where the pack
/// goes, the zip round trip, the exposure decision, the port check and the session view. The server has no sessions of its own - an HTTP
/// request is not a session and a trainee's name is typed - so the page supplies the notion through the server's request observer and
/// names it for what it is: the names seen, with how much each has done.
/// </para>
/// <para>
/// The server is off until it is asked for, and it is bound to loopback unless a person confirms otherwise (AC-3): an embedded web
/// server in a desktop application is unusual, and the honest way to offer it is to say so on the page rather than to open it quietly.
/// </para>
/// </remarks>
public sealed class OgTrainingServerService : IOgTrainingServer
{
    private readonly IFailureReporter _failures;
    private readonly Func<string?> _scenarioFolder;
    private readonly Func<string> _ledgerFile;
    private readonly Func<string> _packsFolder;

    private readonly object _gate = new();
    private readonly Dictionary<string, (int Requests, DateTime First, DateTime Last)> _seen =
        new(StringComparer.OrdinalIgnoreCase);

    private TrainingServer? _server;
    private ResultsLedger? _ledger;
    private OgTrainingServer? _last;

    public OgTrainingServerService(
        IFailureReporter failures,
        Func<string?> scenarioFolder,
        Func<string> ledgerFile,
        Func<string> packsFolder)
    {
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _scenarioFolder = scenarioFolder ?? throw new ArgumentNullException(nameof(scenarioFolder));
        _ledgerFile = ledgerFile ?? throw new ArgumentNullException(nameof(ledgerFile));
        _packsFolder = packsFolder ?? throw new ArgumentNullException(nameof(packsFolder));
    }

    public Task<OgResult<OgPackInstall>> InstallSamplePackAsync(
        string actor, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Task.FromResult(OgResult<OgPackInstall>.Failed(
                "Installing the sample pack records who did it.", OgTrainingServerRules.CodeActorRequired));
        }

        var folder = _scenarioFolder();

        if (string.IsNullOrWhiteSpace(folder))
        {
            return Task.FromResult(OgResult<OgPackInstall>.Failed(
                "No scenario folder is configured, so there is nowhere to install the pack. Set training.scenarioFolders " +
                "first - or use the scenarios page, which writes the same pack under the data root.",
                OgTrainingServerRules.CodeNoFolder));
        }

        try
        {
            var existing = Directory.Exists(folder)
                ? Directory.GetFiles(folder, "*.scenario.json").Length
                : 0;

            var files = SamplePacks.Write(folder);

            var problems = new List<string>();
            var scenarios = ScenarioLibrary.LoadFolder(folder, problems);
            var courses = CourseLibrary.LoadFolder(folder, scenarios, problems);

            var notes = new List<string>();

            if (existing > 0)
            {
                notes.Add("the folder already held " + existing.ToString(CultureInfo.InvariantCulture) +
                          " scenario file(s), so this is a reinstall: the pack's files were replaced and any other file in the " +
                          "folder was left alone");
            }

            notes.AddRange(problems);

            return Task.FromResult(OgResult<OgPackInstall>.Ok(new OgPackInstall(
                folder,
                files.Count,
                scenarios.Count,
                courses.Count,
                Reinstalled: existing > 0,
                files,
                notes)));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            var reference = _failures.ReportHandled(
                ex, "installing the sample pack into " + folder, "the pack is not installed", FailureSeverity.Degraded);

            return Task.FromResult(OgResult<OgPackInstall>.Failed(
                "The sample pack could not be written to '" + folder + "' (reference " + reference + ").",
                OgTrainingServerRules.CodeInstallFailed));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "installing the sample pack into " + folder, "the pack is not installed", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgPackInstall>.Failed(
                "The sample pack could not be installed (reference " + reference + ").",
                reference ?? OgTrainingServerRules.CodeInstallFailed));
        }
    }

    public Task<OgResult<OgPackTransfer>> ExportPackAsync(
        string folder, string zipPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            return Task.FromResult(OgResult<OgPackTransfer>.Failed(
                "There is no scenario folder to export at '" + folder + "'.", OgTrainingServerRules.CodeNoFolder));
        }

        if (string.IsNullOrWhiteSpace(zipPath))
        {
            return Task.FromResult(OgResult<OgPackTransfer>.Failed(
                "An export writes a file; no path was given.", OgTrainingServerRules.CodeTransferPath));
        }

        try
        {
            // The pack is the exercise files: a scenario, the well beside it, a course. Anything else in the folder is left out, so an
            // export cannot carry an unrelated file to another machine.
            var files = Directory.GetFiles(folder, "*.scenario.json")
                .Concat(Directory.GetFiles(folder, "*.course.json"))
                .Concat(Directory.GetFiles(folder, "*.well.json"))
                .Concat(Directory.GetFiles(folder, "*.solution.json"))
                .Concat(Directory.GetFiles(folder, "*.json").Where(path =>
                    path.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
                    !path.EndsWith(".scenario.json", StringComparison.OrdinalIgnoreCase) &&
                    !path.EndsWith(".course.json", StringComparison.OrdinalIgnoreCase) &&
                    !path.EndsWith(".well.json", StringComparison.OrdinalIgnoreCase) &&
                    !path.EndsWith(".solution.json", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(Path.GetFileName(path), "results.json", StringComparison.OrdinalIgnoreCase)))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            cancellationToken.ThrowIfCancellationRequested();

            var parent = Path.GetDirectoryName(Path.GetFullPath(zipPath));

            if (!string.IsNullOrWhiteSpace(parent))
            {
                Directory.CreateDirectory(parent);
            }

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    archive.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                }
            }

            var problems = new List<string>();
            var scenarios = ScenarioLibrary.LoadFolder(folder, problems);
            var courses = CourseLibrary.LoadFolder(folder, scenarios, problems);

            return Task.FromResult(OgResult<OgPackTransfer>.Ok(new OgPackTransfer(
                zipPath,
                Exported: true,
                files.Count,
                scenarios.Count,
                courses.Count,
                new FileInfo(zipPath).Length,
                problems,
                "written")));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            var reference = _failures.ReportHandled(
                ex, "exporting the pack to " + zipPath, "the pack is not exported", FailureSeverity.Degraded);

            return Task.FromResult(OgResult<OgPackTransfer>.Failed(
                "The pack could not be written to '" + zipPath + "' (reference " + reference + ").",
                OgTrainingServerRules.CodeZip));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "exporting the pack to " + zipPath, "the pack is not exported", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgPackTransfer>.Failed(
                "The pack could not be exported (reference " + reference + ").",
                reference ?? OgTrainingServerRules.CodeZip));
        }
    }

    public Task<OgResult<OgPackTransfer>> ImportPackAsync(
        string zipPath, string folder, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(zipPath) || !File.Exists(zipPath))
        {
            return Task.FromResult(OgResult<OgPackTransfer>.Failed(
                "There is no pack to import at '" + zipPath + "'.", OgTrainingServerRules.CodeTransferPath));
        }

        if (string.IsNullOrWhiteSpace(folder))
        {
            return Task.FromResult(OgResult<OgPackTransfer>.Failed(
                "An import needs a folder to unpack into; none was given.", OgTrainingServerRules.CodeNoFolder));
        }

        try
        {
            Directory.CreateDirectory(folder);

            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // A zip may carry a path that climbs out of the folder; only a plain file name is taken, which is what an export
                    // writes and all an exercise needs.
                    var name = Path.GetFileName(entry.FullName);

                    if (string.IsNullOrWhiteSpace(name) || entry.Length == 0 && name.Length == 0)
                    {
                        continue;
                    }

                    entry.ExtractToFile(Path.Combine(folder, name), overwrite: true);
                }
            }

            // The round trip is measured: the unpacked folder is read back through the engine's own loaders.
            var problems = new List<string>();
            var scenarios = ScenarioLibrary.LoadFolder(folder, problems);
            var courses = CourseLibrary.LoadFolder(folder, scenarios, problems);

            return Task.FromResult(OgResult<OgPackTransfer>.Ok(new OgPackTransfer(
                folder,
                Exported: false,
                Directory.GetFiles(folder).Length,
                scenarios.Count,
                courses.Count,
                new FileInfo(zipPath).Length,
                problems,
                "read back")));
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            var reference = _failures.ReportHandled(
                ex, "importing the pack " + zipPath, "the pack is not imported", FailureSeverity.Degraded);

            return Task.FromResult(OgResult<OgPackTransfer>.Failed(
                "'" + zipPath + "' could not be read as a pack (reference " + reference + ").",
                OgTrainingServerRules.CodeZip));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "importing the pack " + zipPath, "the pack is not imported", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgPackTransfer>.Failed(
                "The pack could not be imported (reference " + reference + ").",
                reference ?? OgTrainingServerRules.CodeZip));
        }
    }

    public Task<OgResult<OgTrainingServer>> StartAsync(
        OgServerRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (_server is not null)
        {
            return Task.FromResult(OgResult<OgTrainingServer>.Failed(
                "The training server is already running at " + (_last?.Url ?? "") + ". Stop it before starting it again.",
                OgTrainingServerRules.CodeAlreadyRunning));
        }

        var folder = _scenarioFolder();

        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            return Task.FromResult(OgResult<OgTrainingServer>.Failed(
                "No scenario folder is configured, so there is nothing to serve.", OgTrainingServerRules.CodeNoFolder));
        }

        var bind = string.IsNullOrWhiteSpace(request.Bind) ? OgTrainingServerRules.LoopbackBind : request.Bind.Trim();

        // AC-3: beyond loopback is a decision, not a default.
        if (OgTrainingServerRules.NeedsConfirmation(bind, request.ConfirmExposure))
        {
            return Task.FromResult(OgResult<OgTrainingServer>.Failed(
                "Binding to '" + bind + "' makes the server reachable from other machines, and it has no sign-in: anyone who can " +
                "reach the port could read the exercises and record attempts in any name. Confirm the exposure to continue, or bind " +
                "to " + OgTrainingServerRules.LoopbackBind + ".",
                OgTrainingServerRules.CodeExposure));
        }

        // AC-4: a port another process holds is refused with the port named, before anything is opened.
        if (!OgTrainingServerRules.PortFree(bind, request.Port))
        {
            return Task.FromResult(OgResult<OgTrainingServer>.Failed(
                OgTrainingServerRules.PortInUse(request.Port, bind), OgTrainingServerRules.CodePortInUse));
        }

        try
        {
            var problems = new List<string>();
            var scenarios = ScenarioLibrary.LoadFolder(folder, problems);
            var courses = CourseLibrary.LoadFolder(folder, scenarios, problems);

            if (scenarios.Count == 0)
            {
                return Task.FromResult(OgResult<OgTrainingServer>.Failed(
                    "The scenario folder holds no scenario, so there is nothing to serve. " +
                    (problems.Count == 0 ? string.Empty : problems[0]), OgTrainingServerRules.CodeNoFolder));
            }

            var path = _ledgerFile();
            var ledger = File.Exists(path)
                ? ResultsLedger.FromJson(File.ReadAllText(path))
                : new ResultsLedger();

            lock (_gate)
            {
                _seen.Clear();
            }

            var options = new TrainingServerOptions
            {
                Host = bind,
                Port = request.Port,
                LedgerPath = path,
                Observe = (trainee, requestPath, utc) => Note(trainee, utc),
            };

            var server = new TrainingServer(scenarios, courses, ledger, options);

            server.Start();

            _server = server;
            _ledger = ledger;

            var url = server.Url;

            var state = Snap(url, bind, request.ConfirmExposure, path, null);

            return Task.FromResult(OgResult<OgTrainingServer>.Ok(state));
        }
        catch (HttpListenerException ex)
        {
            var reference = _failures.ReportHandled(
                ex, "starting the training server on " + bind + ":" + request.Port,
                "the server does not start", FailureSeverity.Degraded);

            // The sentence is ours; the code tells the kinds apart (5 is access denied, which on Windows means the address needs a
            // URL reservation), so the person is told what to do rather than what the runtime objected to.
            var reason = ex.ErrorCode == 5
                ? "Windows refused the address " + bind + ":" + request.Port.ToString(CultureInfo.InvariantCulture) +
                  " (access denied): that address needs a URL reservation, or bind to " +
                  OgTrainingServerRules.LoopbackBind + ". (reference " + reference + ")"
                : "The server could not listen on " + bind + ":" + request.Port.ToString(CultureInfo.InvariantCulture) +
                  ". (reference " + reference + ")";

            // AC-4's other face, and the state the feature asks for: the reason is shown and the state returns to stopped.
            var state = Snap(null, bind, request.ConfirmExposure, _ledgerFile(), reason);

            _server = null;

            return Task.FromResult(OgResult<OgTrainingServer>.Ok(state));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "starting the training server", "the server does not start", FailureSeverity.Error);

            _server = null;

            return Task.FromResult(OgResult<OgTrainingServer>.Failed(
                "The training server could not be started (reference " + reference + ").",
                reference ?? OgTrainingServerRules.CodeStartFailed));
        }
    }

    public Task<OgResult<OgTrainingServer>> StopAsync(CancellationToken cancellationToken = default)
    {
        var server = _server;

        if (server is null)
        {
            return Task.FromResult(OgResult<OgTrainingServer>.Ok(
                Snap(null, OgTrainingServerRules.LoopbackBind, false, null, null, stopped: true)));
        }

        try
        {
            server.Dispose();

            var state = Snap(null, OgTrainingServerRules.LoopbackBind, false, _ledgerFile(), null, stopped: true);

            _server = null;

            return Task.FromResult(OgResult<OgTrainingServer>.Ok(state));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "stopping the training server", "the server is no longer serving", FailureSeverity.Error);

            _server = null;

            return Task.FromResult(OgResult<OgTrainingServer>.Ok(
                Snap(null, OgTrainingServerRules.LoopbackBind, false, null, "stopped with a fault (reference " + reference + ")",
                    stopped: true)));
        }
    }

    public Task<OgResult<OgTrainingServer>> StatusAsync(CancellationToken cancellationToken = default)
    {
        var server = _server;

        return Task.FromResult(OgResult<OgTrainingServer>.Ok(server is null
            ? _last is { Running: false } stopped
                ? stopped
                : Snap(null, OgTrainingServerRules.LoopbackBind, false, null, null)
            : Snap(server.Url, BindOf(server.Url), _last?.ExposureConfirmed ?? false, _last?.LedgerPath, null)));
    }

    // ------------------------------------------------------------------ the sessions and the state

    /// <summary>
    /// One request seen: a trainee name, counted and time-stamped. This is the page's own notion of a session, and it is named for what
    /// it is - a name the server has heard from, not an authenticated session.
    /// </summary>
    private void Note(string trainee, DateTime utc)
    {
        lock (_gate)
        {
            var key = trainee;

            if (_seen.TryGetValue(key, out var existing))
            {
                _seen[key] = (existing.Requests + 1, existing.First, utc);
            }
            else
            {
                _seen[key] = (1, utc, utc);
            }
        }
    }

    private OgTrainingServer Snap(
        string? url, string bind, bool confirmed, string? ledgerPath, string? failure, bool stopped = false)
    {
        List<OgServerSession> sessions;

        lock (_gate)
        {
            sessions = _seen
                .Select(pair => new OgServerSession(
                    pair.Key,
                    pair.Value.Requests,
                    AttemptsFor(pair.Key),
                    pair.Value.First,
                    pair.Value.Last))
                .OrderByDescending(session => session.LastSeen)
                .ToList();
        }

        var port = url is null
            ? 0
            : Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.Port : 0;

        var running = url is not null && !stopped;

        var state = new OgTrainingServer(
            running,
            url,
            port,
            bind,
            OgTrainingServerRules.IsLoopback(bind),
            confirmed,
            sessions,
            sessions.Sum(session => session.Requests),
            _ledger?.Attempts.Count ?? CountAttempts(ledgerPath),
            ledgerPath,
            failure);

        _last = state;

        return _last;
    }

    private int AttemptsFor(string trainee)
    {
        if (_ledger is null || string.IsNullOrWhiteSpace(trainee))
        {
            return 0;
        }

        lock (_gate)
        {
            return _ledger.Attempts.Count(attempt =>
                string.Equals(attempt.Trainee, trainee, StringComparison.OrdinalIgnoreCase));
        }
    }

    private int CountAttempts(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return 0;
        }

        try
        {
            return ResultsLedger.FromJson(File.ReadAllText(path)).Attempts.Count;
        }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or FormatException or IOException)
        {
            // Counted as zero attempts here, and reported: the courses page words an unreadable ledger itself, but the count must not
            // be quietly wrong.
            _failures.ReportHandled(
                ex, "counting the attempts in the training ledger", "the attempt count is shown as zero",
                FailureSeverity.Degraded);

            return 0;
        }
    }

    private static string BindOf(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.Host : OgTrainingServerRules.LoopbackBind;

    /// <summary>The folder a pack is written to and exported from, for the page's default.</summary>
    public string PacksFolder() => _packsFolder();
}
