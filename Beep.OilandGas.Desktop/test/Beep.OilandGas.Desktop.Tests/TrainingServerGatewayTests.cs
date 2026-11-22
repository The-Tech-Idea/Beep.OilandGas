using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Training;
using TheTechIdea.OilGas.Training;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// TRN-03 end to end: the sample pack installed into the configured folder and served over real HTTP, a trainee's request appearing in
/// the session view, the server stopping cleanly and its port being free again, a port another process holds refused with the port
/// named, and binding beyond loopback refused until it is confirmed (AC-1..AC-5).
/// </summary>
/// <remarks>
/// Opt-in because it opens a listening socket and writes under the data root's Training folder.
/// </remarks>
public sealed class TrainingServerGatewayTests
{
    [IntegrationFact]
    public async Task The_pack_installs_the_server_serves_a_trainee_and_stops_cleanly()
    {        using var scope = new Scratch();

        var reporter = new Recording();
        var service = new OgTrainingServerService(reporter, () => scope.Folder,
            () => Path.Combine(scope.Root, "results.json"), () => Path.Combine(scope.Root, "packs"));

        // ---- AC-1: the pack installs where the runner reads it ----
        var installed = await service.InstallSamplePackAsync("sara");

        Assert.True(installed.Success, installed.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.True(installed.Value!.Scenarios >= 6, installed.Value.Describe());

        var problems = new List<string>();

        Assert.NotEmpty(ScenarioLibrary.LoadFolder(scope.Folder, problems));

        // ---- AC-3: beyond loopback is refused until it is confirmed ----
        var exposed = await service.StartAsync(new OgServerRequest(0, "0.0.0.0", ConfirmExposure: false));

        Assert.False(exposed.Success);
        Assert.Equal(OgTrainingServerRules.CodeExposure, exposed.Reference);
        Assert.Contains("no sign-in", exposed.Message!, StringComparison.Ordinal);
        Assert.Contains("Confirm the exposure", exposed.Message!, StringComparison.Ordinal);

        // ---- AC-4: a port another process holds is refused with the port named ----
        var squatter = new TcpListener(IPAddress.Loopback, 0);

        squatter.Start();

        var takenPort = ((IPEndPoint)squatter.LocalEndpoint).Port;

        try
        {
            var conflict = await service.StartAsync(new OgServerRequest(takenPort, OgTrainingServerRules.LoopbackBind, false));

            Assert.False(conflict.Success);
            Assert.Equal(OgTrainingServerRules.CodePortInUse, conflict.Reference);
            Assert.Contains("Port " + takenPort, conflict.Message!, StringComparison.Ordinal);
        }
        finally
        {
            squatter.Stop();
        }

        // ---- AC-2: it starts on a free port, chosen by the system ----
        var started = await service.StartAsync(new OgServerRequest(0, OgTrainingServerRules.LoopbackBind, false));

        Assert.True(started.Success, started.Message + " :: " + string.Join(" | ", reporter.Failures));

        var state = started.Value!;

        // The test asserts the server is actually running rather than skipping when it is not: a machine that refuses a loopback
        // listener cannot verify this feature, and saying so is better than a green tick over an untested state.
        Assert.True(state.Running, state.StateText);

        Assert.True(state.Port > 0);
        Assert.True(state.Loopback);
        Assert.Contains("http://", state.Url!, StringComparison.Ordinal);
        Assert.False(state.ExposureConfirmed);
        Assert.Contains("no sign-in", state.SecurityText, StringComparison.Ordinal);

        try
        {
            // ---- AC-5: a trainee's request is seen and counted ----
            using var http = new HttpClient(new HttpClientHandler { UseProxy = false })
            {
                BaseAddress = new Uri(state.Url!),
            };

            var page = await http.GetStringAsync("/");

            Assert.False(string.IsNullOrWhiteSpace(page));

            var courses = await http.GetStringAsync("/api/courses");

            Assert.Contains("id", courses, StringComparison.OrdinalIgnoreCase);

            // A request carrying a trainee's name is what the session view counts.
            var progress = await http.GetStringAsync("/api/progress?trainee=practice-1");

            Assert.False(string.IsNullOrWhiteSpace(progress));

            var after = await service.StatusAsync();

            Assert.True(after.Success, after.Message);
            Assert.True(after.Value!.Running);
            Assert.True(after.Value.RequestCount >= 3, after.Value.StateText);

            var session = after.Value.Sessions.FirstOrDefault(row => row.Trainee == "practice-1");

            Assert.NotNull(session);
            Assert.True(session!.Requests >= 1, session.Describe());
            Assert.Equal(after.Value.Sessions.Sum(row => row.Requests), after.Value.RequestCount);

            // The page's own wording separates a named trainee from an anonymous request.
            Assert.Contains(after.Value.Sessions, row => string.IsNullOrWhiteSpace(row.Trainee));
            Assert.All(after.Value.Sessions, row => Assert.True(row.LastSeen >= row.FirstSeen));
        }
        finally
        {
            // ---- AC-2: it stops cleanly, and the port is free again ----
            var stopped = await service.StopAsync();

            Assert.True(stopped.Success, stopped.Message);
            Assert.False(stopped.Value!.Running);
            Assert.Null(stopped.Value.Url);

            // The trainees it saw are still reported after the stop, which is what makes the report useful.
            Assert.NotEmpty(stopped.Value.Sessions);

            Assert.True(OgTrainingServerRules.PortFree(OgTrainingServerRules.LoopbackBind, state.Port),
                "the port is free after the stop");
        }

        // ---- a second start after a stop works, and a start while running is refused ----
        var again = await service.StartAsync(new OgServerRequest(0, OgTrainingServerRules.LoopbackBind, false));

        Assert.True(again.Success, again.Message);

        if (again.Value!.Running)
        {
            var whileRunning = await service.StartAsync(new OgServerRequest(0, OgTrainingServerRules.LoopbackBind, false));

            Assert.False(whileRunning.Success);
            Assert.Equal(OgTrainingServerRules.CodeAlreadyRunning, whileRunning.Reference);

            Assert.True((await service.StopAsync()).Success);
        }

        // ---- and a start with no scenario folder says so ----
        var noFolder = new OgTrainingServerService(new Quiet(), () => null,
            () => Path.Combine(scope.Root, "results.json"), () => Path.Combine(scope.Root, "packs"));

        var refused = await noFolder.StartAsync(new OgServerRequest(0, OgTrainingServerRules.LoopbackBind, false));

        Assert.False(refused.Success);
        Assert.Equal(OgTrainingServerRules.CodeNoFolder, refused.Reference);
    }

    private sealed class Scratch : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "trn03-it-" + Guid.NewGuid().ToString("n")[..8]);

        public Scratch() => Directory.CreateDirectory(_root);

        public string Root => _root;

        public string Folder
        {
            get
            {
                var folder = Path.Combine(_root, "scenarios");
                Directory.CreateDirectory(folder);
                return folder;
            }
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_root))
                {
                    Directory.Delete(_root, recursive: true);
                }
            }
            catch (IOException)
            {
                // The OS temp folder is its own to clean.
            }
        }
    }

    private sealed class Recording : IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Failures.Add($"{operation} | {consequence}");
            return FailureReferenceFormat.New();
        }
    }

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
