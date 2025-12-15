using System.Text.Json;
using MudBlazor;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Theme;

/// <summary>
/// Provides theme configuration from JSON theme files.
/// Loads the Oil & Gas theme for account pages.
/// </summary>
public interface IThemeProvider
{
    /// <summary>Get a MudBlazor theme based on the current branding</summary>
    MudTheme GetMudTheme();
    
    /// <summary>Get branding configuration</summary>
    BrandingConfig GetBranding();
    
    /// <summary>Is dark mode default?</summary>
    bool IsDarkModeDefault { get; }
}

public class ThemeProvider : IThemeProvider
{
    private readonly BrandingConfig _branding;
    private readonly MudTheme _mudTheme;

    public ThemeProvider(IConfiguration configuration, IWebHostEnvironment environment, ILogger<ThemeProvider>? logger = null)
    {
        // Try to load from project Theme folder first
        var projectThemePath = Path.Combine(environment.ContentRootPath, "Theme", "OilGasTheme.json");
        var branding = LoadFromFile(projectThemePath, logger);
        
        if (branding == null)
        {
            // Fallback to appsettings.json Branding section
            _branding = new BrandingConfig();
            configuration.GetSection("Branding").Bind(_branding);
        }
        else
        {
            _branding = branding;
        }

        _mudTheme = CreateMudTheme(_branding);
    }

    public BrandingConfig GetBranding() => _branding;

    public MudTheme GetMudTheme() => _mudTheme;

    public bool IsDarkModeDefault => _branding.DefaultDarkMode;

    private static BrandingConfig? LoadFromFile(string filePath, ILogger? logger)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                logger?.LogWarning("Theme file not found: {FilePath}", filePath);
                return null;
            }

            var json = File.ReadAllText(filePath);
            var branding = JsonSerializer.Deserialize<BrandingConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            logger?.LogInformation("Theme loaded from: {FilePath}", filePath);
            return branding;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error loading theme from: {FilePath}", filePath);
            return null;
        }
    }

    private static MudTheme CreateMudTheme(BrandingConfig branding)
    {
        var fontFamilies = branding.FontFamily?.Split(',').Select(f => f.Trim().Trim('\'')).ToArray() 
            ?? new[] { "Roboto", "sans-serif" };
        var headingFontFamilies = branding.HeadingFontFamily?.Split(',').Select(f => f.Trim().Trim('\'')).ToArray() 
            ?? fontFamilies;

        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = branding.PrimaryColor ?? "#7B2CBF",
                Secondary = branding.SecondaryColor ?? "#FF6B35",
                Tertiary = branding.TertiaryColor ?? "#4A90E2",
                Info = branding.InfoColor ?? "#00d4ff",
                Success = branding.SuccessColor ?? "#00ff88",
                Warning = branding.WarningColor ?? "#ffd866",
                Error = branding.ErrorColor ?? "#ff6b9d",
                AppbarBackground = branding.BackgroundColor ?? "#0d1117",
                AppbarText = branding.TextPrimaryColor ?? branding.TextPrimary ?? "#e6edf3",
                Surface = branding.SurfaceColor ?? "#161b22",
                Background = branding.BackgroundColor ?? "#0d1117",
                BackgroundGray = branding.SurfaceColor ?? "#161b22",
                DrawerBackground = branding.BackgroundColor ?? "#0d1117",
                DrawerText = branding.TextPrimaryColor ?? branding.TextPrimary ?? "#e6edf3",
                DrawerIcon = branding.PrimaryColor ?? "#7B2CBF",
                TextPrimary = branding.TextPrimaryColor ?? branding.TextPrimary ?? "#e6edf3",
                TextSecondary = branding.TextSecondaryColor ?? branding.TextSecondary ?? "#8b949e",
                ActionDefault = branding.TextSecondaryColor ?? branding.TextSecondary ?? "#8b949e",
                Divider = branding.BorderColor ?? "#30363d",
                DividerLight = branding.BorderColor ?? "#30363d",
                TableLines = branding.BorderColor ?? "#30363d",
                LinesDefault = branding.BorderColor ?? "#30363d",
                LinesInputs = branding.BorderColor ?? "#30363d"
            },
            PaletteDark = new PaletteDark
            {
                Primary = branding.PrimaryColorDark ?? branding.PrimaryColor ?? "#9D4EDD",
                Secondary = branding.SecondaryColorDark ?? branding.SecondaryColor ?? "#FF8C5A",
                Tertiary = branding.TertiaryColor ?? "#4A90E2",
                Info = branding.InfoColor ?? "#00d4ff",
                Success = branding.SuccessColor ?? "#00ff88",
                Warning = branding.WarningColor ?? "#ffd866",
                Error = branding.ErrorColor ?? "#ff6b9d",
                AppbarBackground = branding.BackgroundColorDark ?? branding.BackgroundDarkColor ?? branding.BackgroundColor ?? "#010409",
                AppbarText = branding.TextPrimaryDark ?? "#e6edf3",
                Surface = branding.SurfaceColorDark ?? branding.SurfaceColor ?? "#161b22",
                Background = branding.BackgroundColorDark ?? branding.BackgroundDarkColor ?? branding.BackgroundColor ?? "#010409",
                BackgroundGray = branding.SurfaceColorDark ?? branding.SurfaceColor ?? "#161b22",
                DrawerBackground = branding.BackgroundColorDark ?? branding.BackgroundDarkColor ?? branding.BackgroundColor ?? "#010409",
                DrawerText = branding.TextPrimaryDark ?? "#e6edf3",
                DrawerIcon = branding.PrimaryColorDark ?? branding.PrimaryColor ?? "#9D4EDD",
                TextPrimary = branding.TextPrimaryDark ?? "#e6edf3",
                TextSecondary = branding.TextSecondaryDark ?? "#8b949e",
                ActionDefault = branding.TextSecondaryDark ?? "#8b949e",
                Divider = branding.BorderColorDark ?? branding.BorderColor ?? "#30363d",
                DividerLight = branding.BorderColorDark ?? branding.BorderColor ?? "#30363d",
                TableLines = branding.BorderColorDark ?? branding.BorderColor ?? "#30363d",
                LinesDefault = branding.BorderColorDark ?? branding.BorderColor ?? "#30363d",
                LinesInputs = branding.BorderColorDark ?? branding.BorderColor ?? "#30363d"
            },
            Typography = new Typography
            {
                Default = new DefaultTypography
                {
                    FontFamily = fontFamilies
                },
                H1 = new H1Typography
                {
                    FontFamily = headingFontFamilies,
                    FontWeight = "700"
                },
                H2 = new H2Typography
                {
                    FontFamily = headingFontFamilies,
                    FontWeight = "600"
                },
                H3 = new H3Typography
                {
                    FontFamily = headingFontFamilies,
                    FontWeight = "600"
                },
                H4 = new H4Typography
                {
                    FontFamily = headingFontFamilies,
                    FontWeight = "600"
                },
                H5 = new H5Typography
                {
                    FontFamily = headingFontFamilies,
                    FontWeight = "600"
                },
                H6 = new H6Typography
                {
                    FontFamily = headingFontFamilies,
                    FontWeight = "600"
                },
                Button = new ButtonTypography
                {
                    FontFamily = headingFontFamilies,
                    FontWeight = "500",
                    TextTransform = "none"
                }
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = branding.BorderRadius ?? "8px"
            }
        };
    }
}

/// <summary>
/// Branding configuration loaded from JSON
/// </summary>
public class BrandingConfig
{
    // Application Identity
    public string? AppName { get; set; }
    public string? AppShortName { get; set; }
    public string? Tagline { get; set; }
    public string? Copyright { get; set; }
    public string? Version { get; set; }
    
    // Logos and Images
    public string? LogoUrl { get; set; }
    public string? LogoDarkUrl { get; set; }
    public string? IconUrl { get; set; }
    public string? FaviconUrl { get; set; }
    
    // Colors - Primary
    public string? PrimaryColor { get; set; }
    public string? PrimaryColorDark { get; set; }
    public string? PrimaryHoverColor { get; set; }
    public string? PrimaryTransparentColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? SecondaryColorDark { get; set; }
    public string? TertiaryColor { get; set; }
    
    // Colors - Background
    public string? BackgroundColor { get; set; }
    public string? BackgroundDarkColor { get; set; }
    public string? BackgroundColorDark { get; set; }
    public string? SurfaceColor { get; set; }
    public string? SurfaceColorDark { get; set; }
    public string? SurfaceElevatedColor { get; set; }
    public string? BorderColor { get; set; }
    public string? BorderColorDark { get; set; }
    
    // Colors - Text
    public string? TextPrimary { get; set; }
    public string? TextPrimaryColor { get; set; }
    public string? TextPrimaryDark { get; set; }
    public string? TextSecondary { get; set; }
    public string? TextSecondaryColor { get; set; }
    public string? TextSecondaryDark { get; set; }
    public string? TextOnPrimaryColor { get; set; }
    public string? TextOnPrimary { get; set; }
    
    // Colors - Status
    public string? SuccessColor { get; set; }
    public string? WarningColor { get; set; }
    public string? ErrorColor { get; set; }
    public string? InfoColor { get; set; }
    
    // Typography
    public string? FontFamily { get; set; }
    public string? HeadingFontFamily { get; set; }
    public string? FontMonoFamily { get; set; }
    public string? BaseFontSize { get; set; }
    public string? RtlFontFamily { get; set; }
    public string? RtlHeadingFontFamily { get; set; }
    public bool SupportRtl { get; set; }
    public string? BorderRadius { get; set; }
    public string? BorderRadiusSm { get; set; }
    public string? BorderRadiusMd { get; set; }
    public string? BorderRadiusLg { get; set; }
    public string? BorderRadiusXl { get; set; }
    
    // Login/Auth Specific
    public string? LoginBackgroundUrl { get; set; }
    public string? LoginBackgroundImage { get; set; }
    public string? LoginTitle { get; set; }
    public string? LoginWelcomeMessage { get; set; }
    public string? LoginSubtitle { get; set; }
    public string? RegisterTitle { get; set; }
    public string? RegisterSubtitle { get; set; }
    
    // Feature Flags
    public bool DefaultDarkMode { get; set; }
    public bool IsDarkModeDefault { get; set; }
    public bool EnableDarkMode { get; set; }
    public bool AllowThemeSwitching { get; set; }
    public bool ShowVersionInFooter { get; set; }
    public bool UseTerminalStyle { get; set; }
    public bool ShowSocialLogins { get; set; }
    public bool ShowRememberMe { get; set; }
    
    // SaaS Design System
    public string? ShadowSm { get; set; }
    public string? ShadowMd { get; set; }
    public string? ShadowLg { get; set; }
    public string? ShadowXl { get; set; }
    public string? TransitionDuration { get; set; }
    public string? TransitionEasing { get; set; }
}

