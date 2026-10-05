using Blazored.LocalStorage;
using Microsoft.JSInterop;
using MudBlazor;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.Web.Theme;

/// <summary>
/// Light or dark, for one person on their circuit — the router draws the one theme provider from it, and the app bar's
/// toggle changes it. Remembered in the browser for the next visit.
/// </summary>
/// <remarks>
/// Scoped — one per circuit. Each layout held its own theme provider and its own dark-mode flag, so the account pages
/// ignored the choice made on the others (S3-06 §8). The remembered choice is a per-browser convenience (local storage),
/// never state anybody else relies on.
/// </remarks>
public sealed class ThemeState(IThemeProvider themes, ILocalStorageService storage, IFailureReporter failures)
{
    private const string DarkModeKey = "dark-mode";

    public bool IsDarkMode { get; private set; } = themes.IsDarkModeDefault;

    public MudTheme Theme => themes.GetMudTheme();

    /// <summary>
    /// Raised when this circuit's choice changes, so the router — the one component drawing the theme — redraws. Awaited,
    /// so a failure to redraw reaches the caller instead of vanishing on a discarded task.
    /// </summary>
    public event Func<Task>? Changed;

    /// <summary>Applies what this browser chose last time. Called after the first render, when local storage is reachable.</summary>
    public async Task RestoreAsync()
    {
        try
        {
            if (await storage.GetItemAsync<bool?>(DarkModeKey).ConfigureAwait(false) is { } saved)
            {
                IsDarkMode = saved;
                await RaiseChangedAsync().ConfigureAwait(false);
            }
        }
        catch (JSException exception)
        {
            // Handled: a preference the browser would not give back leaves the application's own default in place.
            failures.ReportHandled(
                exception,
                "reading the saved dark-mode preference from the browser",
                consequence: "the application's own default applies for this visit",
                FailureSeverity.Degraded);
        }
    }

    public async Task SetDarkModeAsync(bool dark)
    {
        IsDarkMode = dark;
        await RaiseChangedAsync().ConfigureAwait(false);

        try
        {
            await storage.SetItemAsync(DarkModeKey, dark).ConfigureAwait(false);
        }
        catch (JSException exception)
        {
            // Handled: the choice applies to this visit; it is not remembered for the next one.
            failures.ReportHandled(
                exception,
                "saving the dark-mode preference in the browser",
                consequence: "the choice applies to this visit and is not remembered for the next one",
                FailureSeverity.Degraded);
        }
    }

    private Task RaiseChangedAsync() => Changed?.Invoke() ?? Task.CompletedTask;
}
