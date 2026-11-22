using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using System.Windows.Media;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Shell;

/// <summary>
/// FND-02: the data-path health window behind the status chip. It shows the mode in force, why it was
/// chosen, and what a health check found — with the three states a person can act on: no answer, refused
/// this caller, database missing.
/// </summary>
public sealed class ConnectionHealthWindow : Window
{
    private readonly OgApplication _application;
    private readonly TextBlock _state;
    private readonly TextBlock _detail;
    private readonly TextBlock _read;

    public ConnectionHealthWindow(OgApplication application)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));

        Title = "Beep Oil & Gas — data path";
        Width = 560;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush("BeepBackgroundBrush", Brushes.White);
        FlowDirection = application.Loc.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        _state = new TextBlock { FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 8, 0, 0) };
        _detail = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0) };
        _read = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) };

        var check = new Button
        {
            Content = "Check again",
            Padding = new Thickness(14, 6, 14, 6),
            MinWidth = 120,
            Margin = new Thickness(0, 14, 8, 0),
        };
        check.Click += async (_, _) => await CheckAsync();

        // FND-02 AC-3: a business read says "refused" or "unreachable" rather than showing no rows.
        var read = new Button
        {
            Content = "Test business read",
            Padding = new Thickness(14, 6, 14, 6),
            MinWidth = 150,
            Margin = new Thickness(0, 14, 8, 0),
        };
        read.Click += async (_, _) => await TestReadAsync();

        var close = new Button
        {
            Content = "Close",
            Padding = new Thickness(14, 6, 14, 6),
            MinWidth = 90,
            Margin = new Thickness(0, 14, 0, 0),
            IsCancel = true,
        };

        var buttons = new StackPanel { Orientation = Orientation.Horizontal };
        buttons.Children.Add(check);
        buttons.Children.Add(read);
        buttons.Children.Add(close);

        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(Heading($"Mode: {application.Access.Mode}"));
        panel.Children.Add(Body(application.Access.Reason));
        panel.Children.Add(Body($"Preference: {application.Access.Settings.Preference}"));
        panel.Children.Add(_state);
        panel.Children.Add(_detail);
        panel.Children.Add(_read);
        panel.Children.Add(buttons);

        Content = panel;

        Loaded += async (_, _) => await CheckAsync();
    }

    /// <summary>What the last check found; null before it has run.</summary>
    public OgAccessHealth? LastState { get; private set; }

    /// <summary>What the last business read found; null before it has run (FND-02 AC-3).</summary>
    public Services.OgRemoteReadOutcome? LastReadOutcome { get; private set; }

    public string ReadText => _read.Text;

    /// <summary>
    /// Runs one business read through the company API. Public so the smoke run can drive it; a person uses the
    /// button, which calls the same method.
    /// </summary>
    public async Task<Services.OgRemoteReadOutcome> TestReadAsync()
    {
        var result = await _application.RemoteReader
            .ReadAsync<JsonElement>("api/wells/comparison-fields")
            .ConfigureAwait(true);

        LastReadOutcome = result.Outcome;
        _read.Text = result.Describe();

        // A failed read is also audited, so "why is the grid empty?" has an answer later.
        _application.AuditLog.Add(
            "api.read",
            "endpoint",
            "api/wells/comparison-fields",
            result.Success ? "ok" : "failed",
            result.Describe());

        return result.Outcome;
    }

    private async Task CheckAsync()
    {
        try
        {
            var health = await _application.Access.CheckHealthAsync();

            if (!health.Success || health.Value is null)
            {
                LastState = null;
                _state.Text = "The data path could not be checked.";
                _detail.Text = health.Reference is { Length: > 0 } reference
                    ? $"Reference: {reference}"
                    : string.Empty;
                return;
            }

            LastState = health.Value.State;
            _state.Text = $"Health: {health.Value.State}";
            _detail.Text = health.Value.Detail;
            // REL-03 AC-3/AC-4: the state is in the words, not in the colour. The warning token is 3.9:1 on
            // white - fine for a component marker, not for 12 px text - so the sentence keeps the text colour
            // and an explicit symbol carries the state for a colour-blind reader.
            _state.Foreground = Brush("BeepTextPrimaryBrush", Brushes.Black);
            _state.Text = health.Value.State == OgAccessHealth.Ok
                ? $"Health: OK ({health.Value.State})"
                : $"Health: attention needed ({health.Value.State})";
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "checking the data-path health",
                "the health of the data path is unknown",
                FailureSeverity.Degraded);

            LastState = null;
            _state.Text = "The data path could not be checked.";
            _detail.Text = $"Reference: {reference}";
        }
    }

    private static TextBlock Heading(string text) => new()
    {
        Text = text,
        FontWeight = FontWeights.SemiBold,
        FontSize = 15,
    };

    private static TextBlock Body(string text) => new()
    {
        Text = text,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 4, 0, 0),
    };

    private static Brush Brush(string key, Brush fallback) =>
        Application.Current?.TryFindResource(key) as Brush ?? fallback;
}
