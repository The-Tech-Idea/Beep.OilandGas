using MudBlazor;
using TheTechIdeaWeb.ThemeBranding;

namespace Beep.OilandGas.Web.Theme;

/// <summary>
/// The OilGas theme, read once from <c>Theme/OilGasTheme.json</c> through the shared ThemeBranding library. Read-only:
/// what one person sees (light or dark) is their circuit's (<see cref="ThemeState"/>).
/// </summary>
public interface IThemeProvider
{
    MudTheme GetMudTheme();
    BrandingConfig GetBranding();
    bool IsDarkModeDefault { get; }
}

/// <remarks>
/// It carried a <c>CurrentTheme</c> setter and change event on this singleton — one person's choice would have re-themed
/// every open page — and loaded the library's presets for that switch, falling back to a path into an obsolete checkout.
/// Nothing offered the switch, so it went with its presets (S3-06 §8). The theme file ships with the application and is
/// required: a missing one stops startup, naming it, where it fell back to an appsettings section.
/// </remarks>
public sealed class ThemeProvider : IThemeProvider
{
    private readonly BrandingConfig _branding;
    private readonly MudTheme _mudTheme;

    public ThemeProvider(IWebHostEnvironment environment, ILogger<ThemeProvider> logger)
    {
        var path = Path.Combine(environment.ContentRootPath, "Theme", "OilGasTheme.json");
        _branding = BrandingConfigLoader.LoadFromFile(path, logger)
            ?? throw new InvalidOperationException($"The OilGas theme could not be loaded from {path}; it ships with the application.");
        _mudTheme = BrandingConfigMudThemeMapper.CreateMudTheme(_branding);
    }

    public BrandingConfig GetBranding() => _branding;

    public MudTheme GetMudTheme() => _mudTheme;

    public bool IsDarkModeDefault => _branding.DefaultDarkMode;
}
