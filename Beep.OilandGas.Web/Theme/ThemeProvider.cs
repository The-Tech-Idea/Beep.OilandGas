using System.Text.Json;
using MudBlazor;
using Microsoft.Extensions.Logging;
using TheTechIdeaWeb.ThemeBranding;

namespace Beep.OilandGas.Web.Theme;

/// <summary>
/// Provides theme configuration from JSON theme files.
/// Uses the shared TheTechIdeaWeb.ThemeBranding library for BrandingConfig
/// and BrandingConfigMudThemeMapper for canonical MudTheme generation.
/// </summary>
public interface IThemeProvider
{
    MudTheme GetMudTheme();
    BrandingConfig GetBranding();
    bool IsDarkModeDefault { get; }
    IReadOnlyList<ThemePreset> GetAvailableThemes();
    BrandingConfig GetBrandingForTheme(ThemePreset preset);
    MudTheme GetMudThemeForPreset(ThemePreset preset);
    ThemePreset CurrentTheme { get; set; }
    event Action? OnThemeChanged;
}

public class ThemeProvider : IThemeProvider
{
    private BrandingConfig _branding;
    private MudTheme _mudTheme;
    private readonly Dictionary<ThemePreset, BrandingConfig> _brandingCache = new();
    private readonly Dictionary<ThemePreset, MudTheme> _themeCache = new();
    private ThemePreset _currentTheme = ThemePreset.OilGas;
    private readonly ILogger<ThemeProvider>? _logger;

    public ThemePreset CurrentTheme
    {
        get => _currentTheme;
        set
        {
            if (_currentTheme == value) return;
            _currentTheme = value;

            // Update MudTheme from cache, or fall back to OilGas
            if (_themeCache.TryGetValue(value, out var theme))
                _mudTheme = theme;
            else if (_themeCache.TryGetValue(ThemePreset.OilGas, out var fallback))
                _mudTheme = fallback;

            // Update branding from cache, or keep current
            if (_brandingCache.TryGetValue(value, out var branding))
                _branding = branding;

            OnThemeChanged?.Invoke();
        }
    }

    public event Action? OnThemeChanged;

    public ThemeProvider(IConfiguration configuration, IWebHostEnvironment environment, ILogger<ThemeProvider>? logger = null)
    {
        _logger = logger;

        // 1. Load the OilGas theme from the project's Theme folder (default for this app)
        var projectThemePath = Path.Combine(environment.ContentRootPath, "Theme", "OilGasTheme.json");
        var branding = LoadFromFile(projectThemePath);

        // 2. Fall back to appsettings.json Branding section
        if (branding == null)
        {
            branding = new BrandingConfig();
            configuration.GetSection("Branding").Bind(branding);
            branding.SynchronizeProperties();
        }

        _branding = branding;
        _brandingCache[ThemePreset.OilGas] = branding;

        // 3. Create MudTheme using the shared canonical mapper
        _mudTheme = BrandingConfigMudThemeMapper.CreateMudTheme(branding);
        _themeCache[ThemePreset.OilGas] = _mudTheme;

        // 4. Load all available theme presets from the shared library
        InitializeThemes(environment);
    }

    public BrandingConfig GetBranding() => _branding;
    public MudTheme GetMudTheme() => _mudTheme;
    public bool IsDarkModeDefault => _branding.DefaultDarkMode;

    public IReadOnlyList<ThemePreset> GetAvailableThemes() =>
        _brandingCache.Keys.OrderBy(k => k == ThemePreset.OilGas ? 0 : (int)k).ToList();

    public BrandingConfig GetBrandingForTheme(ThemePreset preset)
    {
        if (_brandingCache.TryGetValue(preset, out var branding))
            return branding;
        return _branding;
    }

    public MudTheme GetMudThemeForPreset(ThemePreset preset)
    {
        if (_themeCache.TryGetValue(preset, out var theme))
            return theme;
        return _mudTheme;
    }

    private void InitializeThemes(IWebHostEnvironment environment)
    {
        // Try to load standard themes from the ThemeBranding DLL's output directory
        var brandingAssembly = typeof(BrandingConfigMudThemeMapper).Assembly;
        var brandingLocation = Path.GetDirectoryName(brandingAssembly.Location);
        var themesDir = brandingLocation != null
            ? Path.Combine(brandingLocation, "Themes")
            : null;

        foreach (ThemePreset preset in Enum.GetValues<ThemePreset>())
        {
            if (preset == ThemePreset.OilGas) continue; // Already loaded from project Theme folder

            // Priority 1: Try ThemeBranding DLL output directory (production)
            var themePath = themesDir != null
                ? Path.Combine(themesDir, $"{preset}.json")
                : null;

            // Priority 2: Try relative path from ContentRoot (development)
            if (string.IsNullOrWhiteSpace(themePath) || !File.Exists(themePath))
            {
                themePath = Path.Combine(environment.ContentRootPath,
                    "..", "..", "..", "fahadTheTechIdea", "MyWebSite", "TheTechIdeaWeb",
                    "TheTechIdeaWeb.ThemeBranding", "Themes", $"{preset}.json");
            }

            if (string.IsNullOrWhiteSpace(themePath) || !File.Exists(themePath))
                continue;

            var branding = LoadFromFile(themePath);
            if (branding != null)
            {
                _brandingCache[preset] = branding;
                _themeCache[preset] = BrandingConfigMudThemeMapper.CreateMudTheme(branding);
            }
        }

        _logger?.LogInformation("Loaded {Count} theme presets (OilGas + {Others} standard)",
            _brandingCache.Count, _brandingCache.Count - 1);
    }

    private static BrandingConfig? LoadFromFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return null;

        try
        {
            var json = File.ReadAllText(filePath);
            return BrandingConfigLoader.LoadFromStream(
                new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));
        }
        catch (Exception)
        {
            return null;
        }
    }
}
