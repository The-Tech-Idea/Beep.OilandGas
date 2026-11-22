using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.WPF.Controls;
using TheTechIdea.Beep.WPF.Controls.Styling;
using TheTechIdea.Beep.WPF.Controls.ThemeManagement;

namespace TheTechIdea.Beep.OilandGas.Desktop.Theme;

/// <summary>One choice in the skin picker: a Beep design system and its display name.</summary>
public sealed record OgSkinOption(BeepControlStyle Style, string DisplayName)
{
    public override string ToString() => DisplayName;
}

/// <summary>
/// Applies the Beep design-system skins to the desktop. The skin supplies geometry, chrome, templates
/// and state treatment; <c>Themes/Tokens.xaml</c> supplies the colour tokens the views bind with
/// <c>DynamicResource</c>. One skin is active at a time and is stored per data root.
/// </summary>
/// <remarks>
/// <para>
/// Mirrors <c>ErpSkins</c> in the ERP reference. The class deliberately does not catch: a skin that
/// cannot be applied is reported by the caller through <c>OgDiagnosticsPipeline</c>.
/// </para>
/// <para>
/// REL-03 AC-3: after a skin is applied its palette is checked, and any colour that does not reach WCAG 2.2 AA against the surface it is drawn
/// on is replaced with the nearest one that does - the hue is kept and only darkened or lightened as far as the threshold needs. The library's
/// own skins ship ambers and bright accents that fail (1.89:1 where 3:1 or 4.5:1 is required), so the guarantee has to be made here, where the
/// palette becomes the one the application actually draws with.
/// </para>
/// </remarks>
public static class OgSkins
{
    private static bool _initialized;

    public static IReadOnlyList<OgSkinOption> All { get; } = Enum.GetValues<BeepControlStyle>()
        .Where(style => style != BeepControlStyle.None)
        .Select(style => new OgSkinOption(style, BeepChrome.GetDesignSystemName(style)))
        .OrderBy(option => option.DisplayName, StringComparer.OrdinalIgnoreCase)
        .ToList();

    public static BeepControlStyle Current { get; private set; } = BeepControlStyle.Material3;

    public static string CurrentDisplayName =>
        All.FirstOrDefault(option => option.Style == Current)?.DisplayName
        ?? BeepChrome.GetDesignSystemName(Current);

    public static string SettingsPath => Path.Combine(OgPaths.DataRoot, "skin.json");

    /// <summary>Loads the saved skin, ensures the token dictionary is merged and applies it once.</summary>
    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        Load();
        EnsureBaseTokens();
        Apply(Current, persist: false);
    }

    public static void Apply(BeepControlStyle style, bool persist = true)
    {
        if (style == BeepControlStyle.None)
        {
            style = BeepControlStyle.Material3;
        }

        EnsureBaseTokens();
        Current = style;

        WpfThemesManager.ApplyStyle(style);

        // REL-03 AC-3: the skin's palette is enforced before anything draws with it, so what a person sees is what the audit measured.
        EnforceContrast();

        if (Application.Current is { } application)
        {
            foreach (Window window in application.Windows)
            {
                BeepChrome.SetControlStyle(window, style);
            }
        }

        if (persist)
        {
            Save();
        }
    }

    /// <summary>Applies the saved skin to a window opened after the initial application.</summary>
    public static void ApplyTo(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        BeepChrome.SetControlStyle(window, Current);
    }

    /// <summary>What the smoke run and the diagnostics page report.</summary>
    public static IReadOnlyList<string> SelfCheck()
    {
        var application = Application.Current;
        var tokenDictionaryLoaded = application?.Resources.MergedDictionaries.Any(dictionary =>
            dictionary.Source?.OriginalString.Contains("/Themes/", StringComparison.OrdinalIgnoreCase) == true) == true;

        return
        [
            $"skin-current: {Current}; display={CurrentDisplayName}; skins={All.Count}; tokens-merged={tokenDictionaryLoaded}",
            $"skin-contrast-adjustments: {(LastContrastAdjustments.Count == 0 ? "(none)" : string.Join(" | ", LastContrastAdjustments))}",
        ];
    }

    /// <summary>REL-03 AC-3: what the guard changed the last time a skin was applied, one line per colour.</summary>
    public static IReadOnlyList<string> LastContrastAdjustments { get; private set; } = [];

    /// <summary>
    /// The active skin's semantic colours, read from the brushes the controls actually draw with. The resource keys are tried in order, so a
    /// skin that names them the library's way and one that falls back to the older names are both read correctly.
    /// </summary>
    public static OgThemeContrast.OgSkinColours ReadPalette()
    {
        string Colour(params string[] keys) => Resolve(keys)?.Colour ?? "#FFFFFF";

        return new OgThemeContrast.OgSkinColours(
            Surface: Colour("BeepStyleSurfaceBrush", "BeepSurfaceBrush"),
            SurfaceVariant: Colour("BeepStyleSurfaceVariantBrush", "BeepSurfaceVariantBrush", "BeepSurfaceAltBrush"),
            OnSurface: Colour("BeepStyleOnSurfaceBrush", "BeepOnSurfaceBrush", "BeepTextPrimaryBrush"),
            OnSurfaceVariant: Colour("BeepStyleOnSurfaceVariantBrush", "BeepOnSurfaceVariantBrush", "BeepTextSecondaryBrush"),
            Accent: Colour("BeepStyleAccentBrush", "BeepAccentBrush"),
            AccentForeground: Colour("BeepStyleAccentForegroundBrush", "BeepAccentForegroundBrush"),
            Danger: Colour("BeepStyleDangerBrush", "BeepDangerBrush"),
            Success: Colour("BeepStyleSuccessBrush", "BeepSuccessBrush"),
            Warning: Colour("BeepStyleWarningBrush", "BeepWarningBrush"));
    }

    /// <summary>
    /// REL-03 AC-3: makes the active skin's palette meet WCAG 2.2 AA, writing the adjusted colours into one dictionary of its own that sits last
    /// in the merge chain - so it wins over the skin's own brushes and is replaced, not accumulated, when the skin changes. A colour that
    /// already passes is left exactly as the skin defines it, and nothing is written at all when the skin passes.
    /// </summary>
    /// <remarks>
    /// The read that decides what to adjust is taken with this dictionary detached, because otherwise a previous skin's adjustment would be
    /// what got measured - the audit would be reading its own last answer and every skin after the first would appear to pass. That was the
    /// first version's bug: 66 skins "fixed" by measuring one palette 66 times.
    /// </remarks>
    public static IReadOnlyList<string> EnforceContrast()
    {
        if (Application.Current is not { } application)
        {
            LastContrastAdjustments = [];

            return LastContrastAdjustments;
        }

        var overrides = ContrastOverrides;
        var merged = application.Resources.MergedDictionaries;

        // Detach our own adjustments, read what the skin itself defines, then rebuild them from that reading.
        var wasAttached = merged.Remove(overrides);
        var before = ReadPalette();
        var (after, changes) = OgThemeContrast.Enforce(before);

        overrides.Clear();

        void Keep(string adjusted, string original, params string[] keys)
        {
            if (string.Equals(adjusted, original, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(adjusted));

            brush.Freeze();

            // Only the keys the skin actually defines: writing a key a skin does not use would invent a resource.
            foreach (var key in keys)
            {
                if (Application.Current?.TryFindResource(key) is not null || MergedContains(merged, key))
                {
                    overrides[key] = brush;
                }
            }
        }

        Keep(after.OnSurface, before.OnSurface, "BeepStyleOnSurfaceBrush", "BeepOnSurfaceBrush", "BeepTextPrimaryBrush");
        Keep(after.OnSurfaceVariant, before.OnSurfaceVariant,
            "BeepStyleOnSurfaceVariantBrush", "BeepOnSurfaceVariantBrush", "BeepTextSecondaryBrush");
        Keep(after.AccentForeground, before.AccentForeground,
            "BeepStyleAccentForegroundBrush", "BeepAccentForegroundBrush");
        Keep(after.Accent, before.Accent, "BeepStyleAccentBrush", "BeepAccentBrush");
        Keep(after.Danger, before.Danger, "BeepStyleDangerBrush", "BeepDangerBrush");
        Keep(after.Success, before.Success, "BeepStyleSuccessBrush", "BeepSuccessBrush");
        Keep(after.Warning, before.Warning, "BeepStyleWarningBrush", "BeepWarningBrush");

        // Last in the chain, because a later dictionary is the one a lookup finds.
        if (overrides.Count > 0 || wasAttached)
        {
            merged.Add(overrides);
        }

        LastContrastAdjustments = changes;

        return changes;
    }

    /// <summary>The dictionary the contrast guard writes into; kept across skins and cleared when a skin changes.</summary>
    private static ResourceDictionary ContrastOverrides { get; } = new();

    private static bool MergedContains(System.Collections.ObjectModel.Collection<ResourceDictionary> merged, string key)
    {
        foreach (var dictionary in merged)
        {
            if (dictionary.Contains(key))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>The first key of a group the application has, and its colour.</summary>
    private static (string Colour, string Key)? Resolve(string[] keys)
    {
        foreach (var key in keys)
        {
            if (Application.Current?.TryFindResource(key) is SolidColorBrush brush)
            {
                return ($"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}", key);
            }
        }

        return null;
    }

    private static void EnsureBaseTokens()
    {
        if (Application.Current is not { } application)
        {
            return;
        }

        var tokens = new Uri(
            "pack://application:,,,/TheTechIdea.Beep.OilandGas.Desktop.Controls;component/Themes/Tokens.xaml",
            UriKind.Absolute);
        var applicationRoot = new Uri("pack://application:,,,/");

        var merged = application.Resources.MergedDictionaries;
        if (merged.Any(dictionary => dictionary.Source is { } source
                                     && new Uri(applicationRoot, source) == tokens))
        {
            return;
        }

        merged.Insert(0, new ResourceDictionary { Source = tokens });
    }

    private static void Load()
    {
        if (!File.Exists(SettingsPath))
        {
            return;
        }

        var settings = JsonSerializer.Deserialize<OgSkinSettings>(File.ReadAllText(SettingsPath));

        if (settings is { } value
            && Enum.TryParse<BeepControlStyle>(value.Skin, ignoreCase: true, out var style)
            && style != BeepControlStyle.None)
        {
            Current = style;
        }
    }

    private static void Save()
    {
        var directory = Path.GetDirectoryName(SettingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporary = SettingsPath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(new OgSkinSettings { Skin = Current.ToString() }));
        File.Move(temporary, SettingsPath, overwrite: true);
    }

    private sealed class OgSkinSettings
    {
        public string Skin { get; set; } = nameof(BeepControlStyle.Material3);
    }
}
