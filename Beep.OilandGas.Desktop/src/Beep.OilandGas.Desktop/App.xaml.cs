using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdea.Beep.OilandGas.Desktop.Headless;
using TheTechIdea.Beep.OilandGas.Desktop.Shell;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop;

public partial class App : Application
{
    private OgApplication? _application;
    private ProcessFailureSubscription? _processSubscription;

    static App()
    {
        // The smoke run must choose its data root before any path is read. A static constructor runs
        // first, so this is the only safe place.
        if (Environment.GetCommandLineArgs().Contains("--smoke", StringComparer.OrdinalIgnoreCase))
        {
            // AC-6: a second run must start from a clean installation; a stale root would hide a first-run bug.
            Environment.SetEnvironmentVariable(OgPaths.DataRootVariable, OgSmokeRoot.Prepare());
        }

        // BeepDM's rolling file logger writes beside the executable by default. Point it at the writable
        // data root so an install under Program Files never tries to write there (ADR-013).
        Environment.SetEnvironmentVariable(OgPaths.LogDirectoryVariable, OgPaths.LogDirectory);
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;

        var command = OgCommandLine.Parse(e.Args);

        try
        {
            _application = new OgBootstrapper().Create();

            // FND-04: one design system for the whole desktop, applied before any window exists.
            TheTechIdea.Beep.OilandGas.Desktop.Theme.OgSkins.Initialize();

            // Report what escapes every other handler through the same reporter, from now on.
            _processSubscription = ProcessFailureSubscription.Start(
                _application.Diagnostics,
                whenEnding: "the process ended before the failure could be shown");

            // FND-01 AC-2: every headless verb — including --smoke — runs without opening a window and
            // returns its documented exit code.
            if (command.IsHeadless)
            {
                var exitCode = OgHeadlessRunner.Run(_application, command, Console.Out);
                Shutdown(exitCode);
                return;
            }

            // FND-07: the shell never opens on an installation that has not been set up or has not signed in. Every
            // fact comes from the database: whether this machine has a connection, whether that database holds a
            // company record and accounts, and whether somebody has signed in. No setting is read from a file.
            var probed = await _application.ProbeInstallationAsync();

            if (!probed.Success || probed.Value is null)
            {
                // A configured database that cannot be opened is a state a person must act on; it is not a first
                // run, and it is not something the application can invent its way past.
                var reference = _application.Diagnostics.ReportHandled(
                    new InvalidOperationException(probed.Message ?? "The dashboard could not be reached."),
                    "probing the installation's database",
                    "the application did not open",
                    FailureSeverity.Critical);

                ShowReferenceOnly(reference);
                _application.Dispose();
                Shutdown(OgExitCode.Failed);
                return;
            }

            var installation = probed.Value;

            // REL-04: "application start to sign-in prompt" ends when the gate has decided.
            OgStartupTimeline.Mark("gate");

            var gate = Setup.OgStartupGate.Decide(
                connectionConfigured: installation.ConnectionConfigured,
                companyRecorded: installation.CompanyRecorded,
                accountsExist: installation.AccountsExist,
                signedIn: _application.IsSignedIn);

            if (gate == Setup.OgStartupAction.RunSetup)
            {
                var wizard = new Setup.FirstRunWindow(_application);
                if (wizard.ShowDialog() != true)
                {
                    _application.Dispose();
                    Shutdown(0);
                    return;
                }

                probed = await _application.ProbeInstallationAsync();

                if (!probed.Success || probed.Value is null)
                {
                    ShowReferenceOnly(_application.Diagnostics.ReportHandled(
                        new InvalidOperationException(probed.Message ?? "The database could not be reached."),
                        "probing the installation after setup",
                        "the application did not open",
                        FailureSeverity.Critical));

                    _application.Dispose();
                    Shutdown(OgExitCode.Failed);
                    return;
                }

                installation = probed.Value;
                gate = Setup.OgStartupGate.Decide(
                    installation.ConnectionConfigured,
                    installation.CompanyRecorded,
                    installation.AccountsExist,
                    _application.IsSignedIn);
            }

            if (gate != Setup.OgStartupAction.Ready && !_application.IsSignedIn)
            {
                var signIn = new Views.SignInWindow(_application);
                if (signIn.ShowDialog() != true)
                {
                    _application.Dispose();
                    Shutdown(0);
                    return;
                }
            }

            // SETUP-03 AC-4: the account a setup created carries a provisional password. Its owner replaces it before
            // the shell exists, so the password whoever installed the product chose is never the working one.
            if (_application.MustChangePassword)
            {
                var change = new Views.ChangePasswordWindow(_application);
                if (change.ShowDialog() != true)
                {
                    _application.Dispose();
                    Shutdown(0);
                    return;
                }
            }

            // After the user is authenticated, not before: the company settings, the roles and the environment live
            // in the company database, and they are that signed-in person's to read. A personal installation reads
            // its own record. A failure never blocks the shell — it is shown as a notice and the installation opens
            // on what it has.
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                var settings = await _application.LoadSettingsAsync();

                if (!settings.Success)
                {
                    _application.Notifications.Publish(new Diagnostics.OgDiagnosticEvent(
                        settings.Reference ?? "settings-unavailable",
                        FailureSeverity.Degraded,
                        settings.Message ?? "The installation's settings could not be read."));
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            var shell = new OgShellWindow(_application);
            MainWindow = shell;
            shell.Show();
            OgStartupTimeline.Mark("shell-shown");
        }
#pragma warning disable BEEP0003 // startup has no window yet: the reference is reported here and shown below
        catch (Exception ex)
#pragma warning restore BEEP0003
        {
            var reference = _application is not null
                ? _application.Diagnostics.ReportHandled(
                    ex,
                    "starting the application",
                    "the application did not start",
                    FailureSeverity.Critical)
                : FailureForUncomposedStartup(ex);

            // FND-01 AC-3: a headless run must leave its evidence behind and must never open a dialog, so a
            // scheduled smoke or diagnostics run cannot hang on a MessageBox a console cannot dismiss.
            if (command.IsHeadless)
            {
                OgHeadlessFailureReport.Write(command, reference, Console.Error);
                Shutdown(OgExitCode.Failed);
                return;
            }

            ShowReferenceOnly(reference);
            Shutdown(OgExitCode.Failed);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        DispatcherUnhandledException -= OnDispatcherUnhandledException;
        _processSubscription?.Dispose();
        _application?.Dispose();
        base.OnExit(e);
    }

    /// <summary>Reports a dispatcher failure and tells the person only what failed and the reference.</summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        if (_application is null)
        {
            return;
        }

        // FND-06 AC-7: the rule lives in OgDispatcherFailureHandler so it can be tested without a window.
        var handler = new OgDispatcherFailureHandler(_application.Diagnostics, ShowReferenceOnly);
        e.Handled = handler.Handle(e.Exception, out _);
    }

    /// <summary>
    /// The single approved <c>MessageBox</c> in the product: startup has no window to toast into, so the
    /// reference is shown directly. It never shows exception text.
    /// </summary>
    private static string FailureForUncomposedStartup(Exception exception)
    {
        // Nothing is composed yet; make a reference so it can still be quoted, and log it.
        var reference = FailureReferenceFormat.New();
        LastResortLog.Write(
            () => Console.Error.WriteLine($"[startup] {reference}: {exception.GetType().FullName}"),
            "a startup failure");
        return reference;
    }

    private static void ShowReferenceOnly(string reference) =>
        MessageBox.Show(
            $"The application could not continue.\n\nReference: {reference}\n\nQuote this reference when you report the problem.",
            "Beep Oil & Gas Desktop",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
}
