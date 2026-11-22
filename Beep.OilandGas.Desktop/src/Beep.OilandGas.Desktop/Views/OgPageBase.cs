using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>Base for every page: sets the shared chrome and hosts content inside the five-state presenter.</summary>
public abstract class OgPageBase : PageFrame
{
    private readonly StatePresenter _state = new() { State = OgViewState.Ready };

    protected OgPageBase(OgApplication application, string route)
    {
        ArgumentNullException.ThrowIfNull(application);

        Title = application.Loc.Get($"Page.{route}");
        ContextText = application.StatusLine;
        Content = _state;
    }

    /// <summary>
    /// REL-03 AC-6: page content scrolls vertically rather than clipping, so 200 % scaling and a short window
    /// reach everything. Horizontal scrolling is disabled: primary content must reflow, not require sideways
    /// scrolling.
    /// </summary>
    protected void ShowContent(FrameworkElement content) => _state.Content = new ScrollViewer
    {
        Content = content,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        Focusable = false,
        Padding = new Thickness(0),
    };

    protected void ShowState(OgViewState state, string message, string? reference = null)
    {
        _state.State = state;
        _state.Message = message;
        _state.Reference = reference;
    }

    /// <summary>REL-03 AC-1: gives a control the name a screen reader announces.</summary>
    protected static T Named<T>(T element, string name) where T : FrameworkElement
    {
        AutomationProperties.SetName(element, name);
        return element;
    }

    protected static TextBlock Body(string text) => new()
    {
        Text = text,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 0, 0, 8),
        Foreground = (Brush?)Application.Current?.TryFindResource("BeepTextPrimaryBrush") ?? Brushes.Black,
    };
}

/// <summary>The landing page. Stage 0 states plainly what is running.</summary>
public sealed class DashboardView : OgPageBase
{
    public DashboardView(OgApplication application)
        : base(application, "dashboard")
    {
        var panel = new StackPanel();
        panel.Children.Add(Body(application.Loc.Get("Dashboard.Welcome")));
        panel.Children.Add(Body(application.Loc.Get("Dashboard.Body")));
        panel.Children.Add(Body($"Data root: {OgPaths.DataRoot}"));
        panel.Children.Add(Body($"Deployment: {application.Session.Mode}"));
        ShowContent(panel);
    }
}

/// <summary>A page whose feature has not landed yet. It says which stage owns it, never fakes data.</summary>
public sealed class PlaceholderView : OgPageBase
{
    public PlaceholderView(OgApplication application, string route, string stageNote)
        : base(application, route)
    {
        var panel = new StackPanel();
        panel.Children.Add(Body(stageNote));
        ShowContent(panel);
    }
}

/// <summary>Shows where the application writes, which is the evidence for ADR-013.</summary>
public sealed class StorageSummaryView : OgPageBase
{
    public StorageSummaryView(OgApplication application)
        : base(application, "settings")
    {
        var panel = new StackPanel();
        panel.Children.Add(Body($"Data root: {OgPaths.DataRoot}"));
        panel.Children.Add(Body($"Failure store: {OgPaths.FailureStorePath}"));
        panel.Children.Add(Body($"Logs: {OgPaths.LogDirectory}"));
        panel.Children.Add(Body($"Inside the data root: {OgPaths.IsWritableLocation(OgPaths.FailureStorePath)}"));
        panel.Children.Add(Body("Nothing is written beside the executable."));
        ShowContent(panel);
    }
}

/// <summary>Proves the pipeline is wired: store counts and degradation state.</summary>
public sealed class DiagnosticsView : OgPageBase
{
    public DiagnosticsView(OgApplication application)
        : base(application, "diagnostics")
    {
        var panel = new StackPanel();
        panel.Children.Add(Body($"Failures recorded: {application.Failures.Count()}"));
        panel.Children.Add(Body($"Audit rows recorded: {application.Audit.Count()}"));
        panel.Children.Add(Body($"Pipeline degraded: {application.Diagnostics.IsDegraded}"));

        try
        {
            var snapshot = application.Environment.Current();
            panel.Children.Add(Body(
                $"Wellbore environment: {(snapshot.UsingDefaults ? "built-in defaults" : snapshot.Path)}"));
            panel.Children.Add(Body(
                $"Environment findings: {snapshot.Findings.Count} (errors: {snapshot.Findings.Count(f => f.Severity == "Error")})"));
            panel.Children.Add(Body(
                $"Toolboxes: {string.Join(", ", snapshot.RegisteredToolboxes)}"));
            panel.Children.Add(Body(
                snapshot.MissingToolboxes.Count == 0
                    ? "Missing toolboxes: none"
                    : $"Missing toolboxes: {string.Join(", ", snapshot.MissingToolboxes)}"));
        }
        catch (Exception ex)
        {
            var reference = application.Diagnostics.ReportHandled(
                ex,
                "reading the wellbore environment for diagnostics",
                "the environment rows are not shown",
                FailureSeverity.Error);

            panel.Children.Add(Body($"Wellbore environment unavailable. Reference: {reference}"));
        }

        panel.Children.Add(Body(
            application.Diagnostics.DegradedReason is { Length: > 0 } reason
                ? $"Reason: {reason}"
                : "The failure and audit stores are healthy."));
        ShowContent(panel);
    }
}

