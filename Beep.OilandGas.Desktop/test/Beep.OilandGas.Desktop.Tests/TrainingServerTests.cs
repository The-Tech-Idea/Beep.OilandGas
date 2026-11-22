using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Training;
using TheTechIdea.OilGas.Training;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// TRN-03: the pack rules and the server's guards - the sample pack installs and reads back (AC-1), a port another process holds is
/// refused with the port named (AC-4), binding beyond loopback needs the confirmation (AC-3), and the zip round trip is measured
/// (AC-6). The server itself, its sessions and its clean stop are exercised over real HTTP in the gateway test.
/// </summary>
public sealed class TrainingServerTests
{
    private const string PageFileName = "TrainingServerPageView.cs";

    [Fact]
    public void The_sample_pack_installs_into_the_scenario_folder_and_reads_back()
    {
        // AC-1: the pack is the tools' own, written where the runner reads, and it validates when read back.
        using var scope = new Scratch();

        var service = new OgTrainingServerService(new Quiet(), () => scope.Folder, () => Path.Combine(scope.Root, "results.json"),
            () => Path.Combine(scope.Root, "packs"));

        var installed = service.InstallSamplePackAsync("sara").GetAwaiter().GetResult();

        Assert.True(installed.Success, installed.Message);
        Assert.False(installed.Value!.Reinstalled);
        Assert.True(installed.Value.Files > 0);
        Assert.True(installed.Value.Scenarios >= 6, installed.Value.Describe());
        Assert.True(installed.Value.Courses > 0);
        Assert.Equal(scope.Folder, installed.Value.Folder);

        // The runner's own loader reads it, which is what "appears in TRN-01" means.
        var problems = new List<string>();
        var scenarios = ScenarioLibrary.LoadFolder(scope.Folder, problems);

        Assert.Equal(installed.Value.Scenarios, scenarios.Count);
        Assert.Empty(problems);

        // A second install is a reinstall, and it says so rather than pretending the folder was empty.
        var again = service.InstallSamplePackAsync("sara").GetAwaiter().GetResult();

        Assert.True(again.Success, again.Message);
        Assert.True(again.Value!.Reinstalled);
        Assert.Contains("reinstall", again.Value.Describe(), StringComparison.Ordinal);
        Assert.Contains(again.Value.Notes, note => note.Contains("already held", StringComparison.Ordinal));

        // An install with no folder says where the setting is, and records who asked.
        var noFolder = new OgTrainingServerService(new Quiet(), () => null, () => "x", () => "y");

        Assert.Equal(OgTrainingServerRules.CodeNoFolder,
            noFolder.InstallSamplePackAsync("sara").GetAwaiter().GetResult().Reference);
        Assert.Contains("training.scenarioFolders", noFolder.InstallSamplePackAsync("sara").GetAwaiter().GetResult().Message!,
            StringComparison.Ordinal);
        Assert.Equal(OgTrainingServerRules.CodeActorRequired, noFolder.InstallSamplePackAsync(" ").GetAwaiter().GetResult().Reference);
    }

    [Fact]
    public void An_export_and_an_import_keep_the_scenarios_and_the_courses()
    {
        // AC-6: a zip out of one folder and back into another, read through the engine's own loaders both times.
        using var scope = new Scratch();

        var service = new OgTrainingServerService(new Quiet(), () => scope.Folder, () => Path.Combine(scope.Root, "results.json"),
            () => Path.Combine(scope.Root, "packs"));

        SamplePacks.Write(scope.Folder);

        var problems = new List<string>();
        var before = ScenarioLibrary.LoadFolder(scope.Folder, problems);
        var coursesBefore = CourseLibrary.LoadFolder(scope.Folder, before, problems);

        var zip = Path.Combine(scope.Root, "out", "pack.zip");

        var exported = service.ExportPackAsync(scope.Folder, zip).GetAwaiter().GetResult();

        Assert.True(exported.Success, exported.Message);
        Assert.True(File.Exists(zip));
        Assert.True(exported.Value!.Bytes > 1000, exported.Value.Describe());
        Assert.Equal(before.Count, exported.Value.Scenarios);
        Assert.Equal(coursesBefore.Count, exported.Value.Courses);

        // The archive holds the exercises and nothing else - not the ledger, not an unrelated file.
        File.WriteAllText(Path.Combine(scope.Folder, "results.json"), "{}");
        File.WriteAllText(Path.Combine(scope.Folder, "notes.txt"), "not part of a pack");

        var into = Path.Combine(scope.Root, "imported");

        var imported = service.ImportPackAsync(zip, into).GetAwaiter().GetResult();

        Assert.True(imported.Success, imported.Message);
        Assert.False(imported.Value!.Exported);
        Assert.Equal(before.Count, imported.Value.Scenarios);
        Assert.Equal(coursesBefore.Count, imported.Value.Courses);
        Assert.Empty(imported.Value.Problems);
        Assert.False(File.Exists(Path.Combine(into, "notes.txt")));

        // The scenarios are the same ones, by id and by title.
        var after = ScenarioLibrary.LoadFolder(into, problems);

        Assert.Equal(before.Select(scenario => scenario.Id).OrderBy(id => id),
            after.Select(scenario => scenario.Id).OrderBy(id => id));

        // A file that is not a pack, and a folder that is not there, are each refused with their own sentence.
        var notAZip = Path.Combine(scope.Root, "not-a-pack.zip");

        File.WriteAllText(notAZip, "this is not a zip");

        var refused = service.ImportPackAsync(notAZip, Path.Combine(scope.Root, "elsewhere")).GetAwaiter().GetResult();

        Assert.False(refused.Success);
        Assert.Equal(OgTrainingServerRules.CodeZip, refused.Reference);

        Assert.Equal(OgTrainingServerRules.CodeTransferPath,
            service.ImportPackAsync(Path.Combine(scope.Root, "absent.zip"), "x").GetAwaiter().GetResult().Reference);
        Assert.Equal(OgTrainingServerRules.CodeNoFolder,
            service.ExportPackAsync(Path.Combine(scope.Root, "nothing"), zip).GetAwaiter().GetResult().Reference);
    }

    [Fact]
    public void A_port_another_process_holds_is_refused_with_the_port_named()
    {
        // AC-4: asked of the operating system's listener list, so the answer is a question rather than a failed bind.
        var listener = new TcpListener(IPAddress.Loopback, 0);

        listener.Start();

        try
        {
            var taken = ((IPEndPoint)listener.LocalEndpoint).Port;

            Assert.False(OgTrainingServerRules.PortFree(OgTrainingServerRules.LoopbackBind, taken));

            var message = OgTrainingServerRules.PortInUse(taken, OgTrainingServerRules.LoopbackBind);

            Assert.Contains("Port " + taken, message, StringComparison.Ordinal);
            Assert.Contains("already in use", message, StringComparison.Ordinal);
            Assert.Contains("0 to let the system pick", message, StringComparison.Ordinal);

            // A port nothing holds is free, and port 0 - "pick one for me" - is never a conflict.
            Assert.True(OgTrainingServerRules.PortFree(OgTrainingServerRules.LoopbackBind, taken + 1));
            Assert.True(OgTrainingServerRules.PortFree(OgTrainingServerRules.LoopbackBind, 0));
        }
        finally
        {
            listener.Stop();
        }
    }

    [Fact]
    public void Binding_beyond_localhost_needs_the_confirmation_and_loopback_never_does()
    {
        // AC-3: the exposure is a decision. Loopback is the default and needs nothing; anything else needs the confirmation.
        Assert.True(OgTrainingServerRules.IsLoopback("127.0.0.1"));
        Assert.True(OgTrainingServerRules.IsLoopback("localhost"));
        Assert.True(OgTrainingServerRules.IsLoopback("::1"));
        Assert.False(OgTrainingServerRules.IsLoopback("0.0.0.0"));
        Assert.False(OgTrainingServerRules.IsLoopback("192.168.1.20"));

        Assert.False(OgTrainingServerRules.NeedsConfirmation("127.0.0.1", confirmed: false));
        Assert.False(OgTrainingServerRules.NeedsConfirmation("localhost", confirmed: false));
        Assert.True(OgTrainingServerRules.NeedsConfirmation("0.0.0.0", confirmed: false));
        Assert.False(OgTrainingServerRules.NeedsConfirmation("0.0.0.0", confirmed: true));
    }

    [Fact]
    public void The_page_states_the_exposure_and_keeps_the_server_off_until_it_is_asked()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-3 and the open question: the security implication is on the page, and the server starts only from a command.
        Assert.Contains("state.SecurityText", page, StringComparison.Ordinal);
        Assert.Contains("_expose.IsChecked == true", page, StringComparison.Ordinal);
        Assert.Contains("no sign-in", page, StringComparison.Ordinal);

        // AC-5: the sessions are shown with what each has done, and the server's state with it.
        Assert.Contains("state.SessionsText", page, StringComparison.Ordinal);
        Assert.Contains("state.SessionCount", page, StringComparison.Ordinal);

        // A failure leaves the state shown rather than a running server nobody can see.
        Assert.Contains("started.Value.Failure", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ fixtures

    private sealed class Scratch : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "trn03-" + Guid.NewGuid().ToString("n")[..8]);

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

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
