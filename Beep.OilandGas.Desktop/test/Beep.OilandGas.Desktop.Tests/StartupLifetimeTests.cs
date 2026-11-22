using System.Windows;
using Microsoft.Extensions.Logging.Abstractions;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdea.Beep.OilandGas.Desktop.Headless;
using TheTechIdea.Beep.OilandGas.Desktop.Shell;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// The remaining FND-01 and FND-06 criteria that do not need a decision: the dispatcher half of AC-7, the
/// root a second smoke run starts from (AC-6), and the rule that only the shell closing ends the process
/// (AC-5).
/// </summary>
public sealed class StartupLifetimeTests
{
    [Fact]
    public void An_exception_escaping_a_user_action_is_reported_handled_and_shown_only_as_a_reference()
    {
        var store = new SqliteFailureStore();
        var pipeline = new OgDiagnosticsPipeline(
            new FailureStoreAccessor(store),
            new SqliteAuditStore(),
            new OgRuntimeContext { UserId = "operator", CompanyId = "c1" },
            NullLogger<OgDiagnosticsPipeline>.Instance);

        string? shown = null;
        var handler = new OgDispatcherFailureHandler(pipeline, reference => shown = reference);

        var handled = handler.Handle(
            new InvalidOperationException("probe: a click failed with password=hunter2"),
            out var reference);

        Assert.True(handled);

        // AC-7: the failure reached the reporter and is durable under the reference shown.
        Assert.True(FailureReferenceFormat.IsWellFormed(reference));
        var stored = store.Find(reference);
        Assert.NotNull(stored);
        Assert.Equal("dispatching a user action", stored!.Operation);
        Assert.Equal(FailureSeverity.Error, stored.Severity);

        // AC-4: what the person sees is the reference, and the secret never survived the store either.
        Assert.Equal(reference, shown);
        Assert.DoesNotContain("hunter2", shown!, StringComparison.Ordinal);
        Assert.DoesNotContain("hunter2", stored.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_second_smoke_run_starts_from_an_empty_root()
    {
        var baseDirectory = Path.Combine(OgPaths.DataRoot, "smoke-root-" + Guid.NewGuid().ToString("n")[..8]);
        Directory.CreateDirectory(baseDirectory);

        var first = OgSmokeRoot.Prepare(baseDirectory);
        Assert.True(Directory.Exists(first));

        // Leave what an earlier run would have left: a store, a setup flag, a result file.
        var diagnostics = Path.Combine(first, "Diagnostics");
        Directory.CreateDirectory(diagnostics);
        File.WriteAllText(Path.Combine(diagnostics, "failures.db"), "stale");
        File.WriteAllText(Path.Combine(first, "setup.json"), "{}");
        File.WriteAllText(Path.Combine(first, "smoke-result.txt"), "result: ok");

        var second = OgSmokeRoot.Prepare(baseDirectory);

        // AC-6: the same root, recreated empty — a stale result must not be able to make a run look green.
        Assert.Equal(first, second);
        Assert.True(Directory.Exists(second));
        Assert.Empty(Directory.EnumerateFiles(second, "*", SearchOption.AllDirectories));
    }

    [Fact]
    public void The_smoke_root_is_created_when_it_never_existed()
    {
        var baseDirectory = Path.Combine(OgPaths.DataRoot, "smoke-fresh-" + Guid.NewGuid().ToString("n")[..8]);
        Directory.CreateDirectory(baseDirectory);

        var root = OgSmokeRoot.Prepare(baseDirectory);

        Assert.True(Directory.Exists(root));
        Assert.Empty(Directory.EnumerateFileSystemEntries(root));
    }

    [Fact]
    public void Closing_the_shell_ends_the_process_exactly_once()
    {
        RunOnSta(() =>
        {
            using var application = new OgBootstrapper().Create();

            var closed = 0;
            var shell = new OgShellWindow(application, onClosed: () => closed++);

            shell.Close();

            Assert.Equal(1, closed);
        });
    }

    [Fact]
    public void A_dialog_closing_does_not_end_the_process_because_shutdown_is_explicit()
    {
        // App.xaml is OnExplicitShutdown: the first-run, sign-in and repair dialogs can open and close
        // without ending the session, and only the shell's close calls Shutdown (the test above proves the
        // call happens; this proves no framework default can forestall or pre-empt it).
        var app = File.ReadAllText(
            Path.Combine(SourceRoot.Locate(), "src", "Beep.OilandGas.Desktop", "App.xaml"));

        Assert.Contains("ShutdownMode=\"OnExplicitShutdown\"", app, StringComparison.Ordinal);
    }

    [Fact]
    public void The_shell_is_the_only_window_that_shuts_the_application_down()
    {
        var offenders = SourceRoot.AllSources()
            .Where(file => !file.EndsWith("App.xaml.cs", StringComparison.OrdinalIgnoreCase))
            .Where(file => File.ReadAllText(file).Contains(".Shutdown(", StringComparison.Ordinal))
            .Where(file => !file.EndsWith("OgShellWindow.xaml.cs", StringComparison.Ordinal))
            .Select(Path.GetFileName)
            .ToArray();

        // App.xaml.cs calls Shutdown for startup failure and for the early exits; the shell is the only
        // window allowed to end a running session.
        Assert.True(
            offenders.Length == 0,
            $"Only the shell may call Application.Shutdown. Offenders: {string.Join(", ", offenders)}");
    }

    private static void RunOnSta(Action action)
    {
        Exception? failure = null;

        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            throw failure;
        }
    }
}
