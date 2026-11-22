using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>One module as the setup grid shows it.</summary>
public sealed record OgSetupModuleRow(
    string ModuleId,
    string ModuleName,
    int Order,
    int EntityTypes,
    bool AlreadyCompleted,
    string State,
    int Records,
    string? Error);

/// <summary>
/// DAT-02: plan the database module setup, run it, resume what failed, and read the log while it runs.
/// </summary>
/// <remarks>
/// The runner owns schema planning/apply and per-module seeding through the owning projects; this page
/// sequences it and reports per-module outcomes. "Plan" is a dry run: it writes nothing.
/// </remarks>
public sealed class ModuleSetupPageView : OgPageBase
{
    private readonly OgApplication _application;
    private readonly IOgModuleSetupRunner _runner;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly DataGrid _grid;
    private readonly ProgressBar _progress;
    private readonly TextBox _log;
    private readonly CheckBox _confirmDemo;

    private CancellationTokenSource? _cancellation;

    public ModuleSetupPageView(OgApplication application)
        : base(application, "modulesetup")
    {
        _application = application;
        _runner = _application.ModuleSetup;

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MaxHeight = 320,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Module", nameof(OgSetupModuleRow.ModuleId), 150),
                Text("Name", nameof(OgSetupModuleRow.ModuleName), 230),
                Text("Order", nameof(OgSetupModuleRow.Order), 60),
                Text("Entities", nameof(OgSetupModuleRow.EntityTypes), 70),
                Text("Completed", nameof(OgSetupModuleRow.AlreadyCompleted), 80),
                Text("State", nameof(OgSetupModuleRow.State), 90),
                Text("Records", nameof(OgSetupModuleRow.Records), 80),
                Star("Detail", nameof(OgSetupModuleRow.Error)),
            },
        };

        _progress = new ProgressBar { Height = 16, Minimum = 0, Maximum = 1, Margin = new Thickness(0, 0, 0, 8) };

        _log = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MinHeight = 120,
            MaxHeight = 200,
            Margin = new Thickness(0, 0, 0, 8),
        };

        // AC-5: the confirmation names the connection, so it cannot be carried over to another database.
        _confirmDemo = new CheckBox
        {
            Content = $"Load demo data into '{_runner.ConnectionName}'",
            IsChecked = false,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 4, 12, 4),
        };

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        toolbar.Children.Add(Button("Plan (dry run)", (_, _) => Plan()));
        toolbar.Children.Add(Button("Apply", async (_, _) => await ApplyAsync()));
        toolbar.Children.Add(Button("Resume", async (_, _) => await ResumeAsync()));
        toolbar.Children.Add(Button("Cancel", (_, _) => Cancel()));
        toolbar.Children.Add(_confirmDemo);

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(_summary);
        panel.Children.Add(_notice);
        panel.Children.Add(_progress);
        panel.Children.Add(_grid);
        panel.Children.Add(_log);

        Named(_grid, "Setup modules");
        Named(_confirmDemo, "Confirm demo data for this connection");
        Named(_progress, "Setup progress");
        Named(_log, "Setup log");
        ShowContent(panel);

        Loaded += (_, _) => Plan();
    }

    /// <summary>What the page is showing; the smoke run and tests read these.</summary>
    public IReadOnlyList<OgSetupModuleRow> Rows { get; private set; } = [];

    public string SummaryText => _summary.Text;

    public string LogText => _log.Text;

    /// <summary>The last note or refusal, for the smoke run and tests.</summary>
    public string NoticeText => _notice.Text;

    /// <summary>The confirmation this page would send (AC-5).</summary>
    public OgSetupConfirmation Confirmation => new(_runner.ConnectionName, _confirmDemo.IsChecked == true);

    /// <summary>Sets the demo-data confirmation as the person would.</summary>
    public void SetDemoConfirmed(bool confirmed) => _confirmDemo.IsChecked = confirmed;

    public bool IsRunning { get; private set; }

    public string ConnectionName => _runner.ConnectionName;

    /// <summary>The plan last read, for the smoke run and tests (AC-5's confirmation flag lives here).</summary>
    public TheTechIdea.Beep.OilandGas.Desktop.Data.Setup.OgSetupPlan? PlanModel { get; private set; }

    /// <summary>The dry run: what the setup will do, read from the registered modules, writing nothing.</summary>
    public bool Plan()
    {
        try
        {
            var planned = _runner.PlanAsync().GetAwaiter().GetResult();

            if (!planned.Success || planned.Value is null)
            {
                _notice.Text = $"{planned.Message}  ({planned.Reference})";
                _summary.Text = "No plan: the module list could not be read.";
                return false;
            }

            var plan = planned.Value;
            PlanModel = plan;

            Rows = plan.Modules
                .Select(module => new OgSetupModuleRow(
                    module.ModuleId,
                    module.ModuleName,
                    module.Order,
                    module.EntityTypeCount,
                    module.AlreadyCompleted,
                    "planned",
                    0,
                    null))
                .ToList();

            _grid.ItemsSource = Rows;
            _progress.Value = 0;
            _progress.Maximum = Math.Max(1, Rows.Count);
            _summary.Text =
                $"{plan.Modules.Count} module(s) · {plan.EntityTypeCount} entity type(s) · connection {plan.ConnectionName}" +
                (plan.IsComplete ? " · already complete" : string.Empty);
            _notice.Text = plan.IsComplete
                ? "Every module has completed; Apply would write nothing."
                : "Plan only: no table was created and no row was written.";

            Append($"planned {plan.Modules.Count} module(s) on {plan.ConnectionName}");
            return true;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "planning the module setup",
                "no plan is shown",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The module setup could not be planned.", reference);
            return false;
        }
    }

    /// <summary>Runs schema apply and seeding, reporting each module as it completes.</summary>
    public async Task<bool> ApplyAsync()
    {
        return await RunAsync(resume: false).ConfigureAwait(true);
    }

    /// <summary>Runs the same, skipping modules that already completed in an earlier run.</summary>
    public async Task<bool> ResumeAsync()
    {
        return await RunAsync(resume: true).ConfigureAwait(true);
    }

    public void Cancel() => _cancellation?.Cancel();

    private async Task<bool> RunAsync(bool resume)
    {
        if (IsRunning)
        {
            _notice.Text = "A setup run is already in progress.";
            return false;
        }

        IsRunning = true;
        _cancellation = new CancellationTokenSource();

        var progress = new Progress<OgSetupProgress>(report =>
        {
            _progress.Maximum = Math.Max(1, report.Total);
            _progress.Value = Math.Min(report.Completed, report.Total);
            Append($"{report.Phase} {report.ModuleId} ({report.Completed}/{report.Total}, {report.Records} record(s))");
        });

        try
        {
            var confirmation = Confirmation;
            var work = resume
                ? _runner.ResumeAsync(confirmation, progress, _cancellation.Token)
                : _runner.ApplyAsync(confirmation, progress, _cancellation.Token);

            // A cancel is an ordinary answer: read it from the task's state rather than catching it.
            await Task.WhenAny(work).ConfigureAwait(true);

            if (work.IsCanceled)
            {
                _notice.Text = "Cancelled. Apply again or use Resume to continue from what completed.";
                Append("cancelled");
                return false;
            }

            if (work.IsFaulted)
            {
                throw work.Exception!.GetBaseException();
            }

            var run = work.Result;

            if (!run.Success || run.Value is null)
            {
                var reference = run.Reference;
                _notice.Text = reference is null
                    ? run.Message ?? "The setup run did not complete."
                    : $"{run.Message}  ({reference})";

                if (run.Reference is not null)
                {
                    _application.Diagnostics.Add(
                        "module-setup",
                        "run",
                        ConnectionName,
                        "failed",
                        run.Message ?? string.Empty);
                }

                return false;
            }

            var outcome = run.Value;

            Rows = outcome.Modules
                .Select(module => new OgSetupModuleRow(
                    module.ModuleId,
                    module.ModuleId,
                    0,
                    0,
                    module.Error == "already completed",
                    module.Skipped ? "skipped" : module.Success ? "ok" : "failed",
                    module.RecordsInserted,
                    module.Error))
                .ToList();

            _grid.ItemsSource = Rows;
            _progress.Value = _progress.Maximum;
            _summary.Text =
                $"{outcome.ModulesSucceeded}/{outcome.ModulesRun} module(s) succeeded, " +
                $"{outcome.RecordsInserted} record(s), {(resume ? "resumed" : "applied")}" +
                (outcome.CanResume ? ", resume available" : string.Empty) +
                (outcome.DemoDataSkipped ? ", demo data not loaded (not confirmed)" : string.Empty);
            _notice.Text = outcome.SchemaMessage ?? string.Empty;

            Append($"{(outcome.Success ? "completed" : "incomplete")}: {outcome.ModulesSucceeded}/{outcome.ModulesRun}, {outcome.RecordsInserted} record(s)");

            _application.AuditLog.Add(
                "module-setup",
                "database",
                ConnectionName,
                outcome.Success ? "ok" : "failed",
                $"{outcome.ModulesSucceeded}/{outcome.ModulesRun} modules, {outcome.RecordsInserted} records, {(resume ? "resume" : "apply")}");

            return outcome.Success;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "running the module setup",
                "the database is not fully set up",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The module setup failed.", reference);
            return false;
        }
        finally
        {
            IsRunning = false;
        }
    }

    private void Append(string line)
    {
        _log.AppendText($"{DateTime.Now:HH:mm:ss}  {line}{Environment.NewLine}");
        _log.ScrollToEnd();
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

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };

    private static DataGridTextColumn Star(string header, string path) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
    };
}
