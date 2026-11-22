using System.Text.RegularExpressions;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// SG-00-8 and FND-05: one colour authority, and every catalog page has a localisation key.
/// </summary>
public sealed class ThemeAndLocalizationTests
{
    private static readonly Regex HexColour = new(@"#[0-9A-Fa-f]{6}\b", RegexOptions.Compiled);
    private static readonly Regex InlineColour = new(@"Color\s*=\s*""", RegexOptions.Compiled);

    [Fact]
    public void No_xaml_file_outside_the_token_dictionary_writes_a_colour()
    {
        var themeDirectory = Path.Combine(
            SourceRoot.Locate(), "src", "Beep.OilandGas.Desktop.Controls", "Themes");

        var offenders = Directory
            .GetFiles(Path.Combine(SourceRoot.Locate(), "src"), "*.xaml", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                           && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !string.Equals(Path.GetFullPath(Path.GetDirectoryName(path)!),
                                          Path.GetFullPath(themeDirectory),
                                          StringComparison.OrdinalIgnoreCase))
            .Where(path =>
            {
                var text = File.ReadAllText(path);
                return HexColour.IsMatch(text) || InlineColour.IsMatch(text);
            })
            .Select(Path.GetFileName)
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            $"Colours belong in Themes/Tokens.xaml only. Offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void The_token_dictionary_defines_every_key_the_views_bind()
    {
        var tokens = File.ReadAllText(Path.Combine(
            SourceRoot.Locate(), "src", "Beep.OilandGas.Desktop.Controls", "Themes", "Tokens.xaml"));

        string[] required =
        [
            "BeepBackgroundBrush", "BeepSurfaceBrush", "BeepSurfaceAltBrush", "BeepBorderBrush",
            "BeepTextPrimaryBrush", "BeepTextSecondaryBrush", "BeepAccentBrush", "BeepOnAccentBrush",
            "BeepSuccessBrush", "BeepWarningBrush", "BeepDangerBrush", "BeepInfoBrush",
            "BeepSuccessSurfaceBrush", "BeepWarningSurfaceBrush", "BeepDangerSurfaceBrush", "BeepInfoSurfaceBrush",
            "BeepBodyFontFamily", "BeepMonoFontFamily",
            "BeepTitleFontSize", "BeepHeadingFontSize", "BeepBodyFontSize", "BeepCaptionFontSize",
            "BeepSpacingS", "BeepSpacingM", "BeepSpacingL", "BeepCornerRadius",
        ];

        var missing = required.Where(key => !tokens.Contains($"\"{key}\"", StringComparison.Ordinal)).ToArray();

        Assert.True(missing.Length == 0, $"Missing token keys: {string.Join(", ", missing)}");
    }

    [Fact]
    public void Every_catalog_page_has_a_localisation_key()
    {
        var loc = new OgLoc();

        var missing = OgModuleCatalog.Pages
            .Where(page => string.Equals(loc.Get($"Page.{page.Route}"), $"Page.{page.Route}", StringComparison.Ordinal))
            .Select(page => page.Route)
            .ToArray();

        Assert.True(missing.Length == 0, $"Missing Page.* keys for: {string.Join(", ", missing)}");
    }

    [Fact]
    public void The_loc_provider_never_returns_an_empty_string()
    {
        var loc = new OgLoc();

        Assert.False(string.IsNullOrWhiteSpace(loc.Get("Fail.Body", "a thing")));
        Assert.Equal("Unknown.Key", loc.Get("Unknown.Key"));
    }
}
