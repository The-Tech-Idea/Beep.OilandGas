using System.Globalization;

namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>A colour with 8 bits per channel.</summary>
public readonly record struct OgRgb(byte R, byte G, byte B)
{
    /// <summary>Parses <c>#RRGGBB</c> or <c>#AARRGGBB</c> (alpha is ignored: contrast is about the visible colour).</summary>
    public static OgRgb Parse(string? hex)
    {
        var text = (hex ?? string.Empty).Trim().TrimStart('#');

        if (text.Length == 8)
        {
            text = text[2..];
        }

        if (text.Length != 6
            || !uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"'{hex}' is not a #RRGGBB colour.");
        }

        return new OgRgb((byte)(value >> 16), (byte)(value >> 8), (byte)value);
    }

    public override string ToString() => $"#{R:X2}{G:X2}{B:X2}";
}

/// <summary>One foreground/background pair and its verdict.</summary>
public sealed record OgContrastPair(string Foreground, string Background, double Ratio, double Minimum)
{
    public bool Passes => Ratio >= Minimum;

    public string Describe() =>
        $"{Foreground} on {Background} = {Ratio:0.00}:1 (needs {Minimum:0.0}:1) {(Passes ? "pass" : "FAIL")}";
}

/// <summary>
/// REL-03 AC-3: WCAG 2.2 contrast arithmetic, kept pure so every theme can be audited rather than eyeballed.
/// </summary>
/// <remarks>
/// The thresholds are the standard's: 4.5:1 for body text, 3:1 for large text and for the parts of a UI
/// component that carry its state. A ratio below 1 is impossible, and identical colours give exactly 1.
/// </remarks>
public static class OgContrast
{
    public const double AaText = 4.5;
    public const double AaUi = 3.0;
    public const double AaaText = 7.0;

    /// <summary>WCAG relative luminance.</summary>
    public static double RelativeLuminance(OgRgb colour) =>
        (0.2126 * Linear(colour.R)) + (0.7152 * Linear(colour.G)) + (0.0722 * Linear(colour.B));

    /// <summary>WCAG contrast ratio, from 1:1 (the same colour) to 21:1 (black on white).</summary>
    public static double Ratio(OgRgb a, OgRgb b)
    {
        var first = RelativeLuminance(a);
        var second = RelativeLuminance(b);
        var lighter = Math.Max(first, second);
        var darker = Math.Min(first, second);

        return (lighter + 0.05) / (darker + 0.05);
    }

    public static double Ratio(string foreground, string background) =>
        Ratio(OgRgb.Parse(foreground), OgRgb.Parse(background));

    public static OgContrastPair Check(string foreground, string background, double minimum = AaText) =>
        new(foreground, background, Ratio(foreground, background), minimum);

    /// <summary>
    /// REL-03 AC-3: the nearest colour to <paramref name="foreground"/> that reaches <paramref name="minimum"/> against
    /// <paramref name="background"/>, moving away from the background's luminance and keeping the hue.
    /// </summary>
    /// <remarks>
    /// A palette is a design decision and this does not take it away: the colour is blended toward black (on a light background) or toward
    /// white (on a dark one) only as far as it must be, so a hue that already passes is returned untouched and one that nearly passes moves a
    /// little. The search is a bisection on the blend, and the answer is verified against the ratio rather than trusted.
    /// </remarks>
    public static string Ensure(string foreground, string background, double minimum, out bool changed)
    {
        var front = OgRgb.Parse(foreground);
        var back = OgRgb.Parse(background);

        if (Ratio(front, back) >= minimum)
        {
            changed = false;
            return front.ToString();
        }

        changed = true;

        // Away from the background: a light background needs a darker foreground and a dark one a lighter.
        var towardLight = RelativeLuminance(back) < 0.5;

        static OgRgb Blend(OgRgb colour, byte target, double amount) => new(
            (byte)Math.Round(colour.R + ((target - colour.R) * amount)),
            (byte)Math.Round(colour.G + ((target - colour.G) * amount)),
            (byte)Math.Round(colour.B + ((target - colour.B) * amount)));

        var goal = towardLight ? (byte)255 : (byte)0;
        var low = 0d;
        var high = 1d;

        // Bisect for the smallest blend that passes: twenty passes settle well inside a byte.
        for (var step = 0; step < 20; step++)
        {
            var middle = (low + high) / 2;

            if (Ratio(Blend(front, goal, middle), back) >= minimum)
            {
                high = middle;
            }
            else
            {
                low = middle;
            }
        }

        var answer = Blend(front, goal, high);

        // The bisection can land a rounding step short of the threshold; one more step toward the goal settles it.
        if (Ratio(answer, back) < minimum)
        {
            answer = Blend(answer, goal, 0.02);
        }

        return answer.ToString();
    }

    /// <summary>
    /// REL-03 AC-3: a pair brought to its minimum by moving whichever side can move. A label that is already white on a mid-tone accent
    /// cannot be lightened any further, so the accent is darkened instead - which is what a designer would do, and is why the pair is the unit
    /// rather than the foreground alone.
    /// </summary>
    /// <param name="Change">What moved, as the sentence the audit prints; empty when the pair already passed.</param>
    public static (string Foreground, string Background) EnsurePair(
        string foreground, string background, double minimum, out string change)
    {
        change = string.Empty;

        if (Ratio(foreground, background) >= minimum)
        {
            return (foreground, background);
        }

        // The foreground first: it is the cheaper of the two to move.
        var moved = Ensure(foreground, background, minimum, out var foregroundChanged);

        if (foregroundChanged && Ratio(moved, background) >= minimum)
        {
            change = Journal(foreground, moved, background, minimum);

            return (moved, background);
        }

        // The foreground has nowhere left to go: darken the background under it.
        var darker = Ensure(background, foreground, minimum, out var backgroundChanged);

        if (backgroundChanged && Ratio(foreground, darker) >= minimum)
        {
            change = Journal(background, darker, foreground, minimum);

            return (foreground, darker);
        }

        // Neither side alone is enough, so both move to their extremes and the background takes the rest.
        var extreme = Ratio("#FFFFFF", background) >= Ratio("#000000", background) ? "#FFFFFF" : "#000000";
        var rest = Ensure(background, extreme, minimum, out _);

        change = Journal(background, rest, extreme, minimum);

        return (extreme, rest);
    }

    /// <summary>Whether a pair reaches its minimum, stated as the sentence the audit prints.</summary>
    public static string Journal(string foreground, string adjusted, string background, double minimum) =>
        foreground + " -> " + adjusted + " on " + background + " (" +
        Ratio(foreground, background).ToString("0.00", CultureInfo.InvariantCulture) + ":1, needs " +
        minimum.ToString("0.0", CultureInfo.InvariantCulture) + ":1)";

    private static double Linear(byte channel)
    {
        var c = channel / 255.0;

        return c <= 0.04045
            ? c / 12.92
            : Math.Pow((c + 0.055) / 1.055, 2.4);
    }
}

/// <summary>The colours a theme must be audited for.</summary>
public sealed record OgThemeColours(
    string Background,
    string Surface,
    string SurfaceAlt,
    string TextPrimary,
    string TextSecondary,
    string Accent,
    string Danger,
    string Success,
    string Warning);

/// <summary>The result of auditing a theme.</summary>
public sealed record OgContrastReport(string Theme, IReadOnlyList<OgContrastPair> Pairs)
{
    public bool Passes => Pairs.All(pair => pair.Passes);

    public int Failures => Pairs.Count(pair => !pair.Passes);

    public string Describe() =>
        $"{Theme}: {(Passes ? "AA pass" : $"{Failures} failure(s)")} over {Pairs.Count} pair(s)";
}

/// <summary>
/// REL-03 AC-3: the pairs every theme must satisfy. Body text is checked at 4.5:1; accent and status colours
/// at 3:1 because they are used for large text and component state, never for small body copy.
/// </summary>
public static class OgThemeContrast
{
    public static OgContrastReport Audit(string theme, OgThemeColours colours)
    {
        ArgumentNullException.ThrowIfNull(colours);

        var pairs = new List<OgContrastPair>
        {
            // Body text on each of the three surfaces it appears on.
            OgContrast.Check(colours.TextPrimary, colours.Surface),
            OgContrast.Check(colours.TextPrimary, colours.Background),
            OgContrast.Check(colours.TextPrimary, colours.SurfaceAlt),
            OgContrast.Check(colours.TextSecondary, colours.Surface),
            OgContrast.Check(colours.TextSecondary, colours.SurfaceAlt),

            // Status / accent colours: large text and component state.
            OgContrast.Check(colours.Accent, colours.Surface, OgContrast.AaUi),
            OgContrast.Check(colours.Danger, colours.Surface, OgContrast.AaUi),
            OgContrast.Check(colours.Success, colours.Surface, OgContrast.AaUi),
            OgContrast.Check(colours.Warning, colours.Surface, OgContrast.AaUi),
        };

        return new OgContrastReport(theme, pairs);
    }

    /// <summary>
    /// The semantic colours a control-library skin publishes. Read from the skin's own brushes rather than
    /// assumed, so a skin that changes them is audited as it actually draws.
    /// </summary>
    public sealed record OgSkinColours(
        string Surface,
        string SurfaceVariant,
        string OnSurface,
        string OnSurfaceVariant,
        string Accent,
        string AccentForeground,
        string Danger,
        string Success,
        string Warning);

    /// <summary>
    /// REL-03 AC-3: audits a control-library skin's own palette. Text pairs are checked at 4.5:1; the accent,
    /// status colours and accent text are checked at 3:1 because they are used for large text and component
    /// state. Disabled text is not checked: WCAG exempts inactive components.
    /// </summary>
    public static OgContrastReport AuditSkin(string skin, OgSkinColours colours)
    {
        ArgumentNullException.ThrowIfNull(colours);

        var pairs = new List<OgContrastPair>
        {
            OgContrast.Check(colours.OnSurface, colours.Surface),
            OgContrast.Check(colours.OnSurface, colours.SurfaceVariant),
            OgContrast.Check(colours.OnSurfaceVariant, colours.Surface),
            OgContrast.Check(colours.AccentForeground, colours.Accent),
            OgContrast.Check(colours.Accent, colours.Surface, OgContrast.AaUi),
            OgContrast.Check(colours.Danger, colours.Surface, OgContrast.AaUi),
            OgContrast.Check(colours.Success, colours.Surface, OgContrast.AaUi),
            OgContrast.Check(colours.Warning, colours.Surface, OgContrast.AaUi),
        };

        return new OgContrastReport(skin, pairs);
    }

    /// <summary>
    /// REL-03 AC-3: a skin's palette with every audited pair made to pass, and a line for each colour it moved.
    /// </summary>
    /// <remarks>
    /// This is what closes the failure the audit found: the control library ships several skins whose accent or warning colour is a bright
    /// amber on a near-white surface (1.89:1 where 3:1 or 4.5:1 is required). The accent is the brand, so the hue is kept and only darkened as
    /// far as the threshold needs; accent text on the accent is settled by moving the label colour instead, which is the standard remedy.
    /// </remarks>
    public static (OgSkinColours Colours, IReadOnlyList<string> Changes) Enforce(OgSkinColours colours)
    {
        ArgumentNullException.ThrowIfNull(colours);

        var changes = new List<string>();

        string Move(string foreground, string background, double minimum)
        {
            var adjusted = OgContrast.Ensure(foreground, background, minimum, out var changed);

            if (!changed)
            {
                return foreground;
            }

            changes.Add(OgContrast.Journal(foreground, adjusted, background, minimum));

            return adjusted;
        }

        // Body text on both surfaces it appears on; the stricter result wins because one colour serves both.
        var onSurface = Move(Move(colours.OnSurface, colours.Surface, OgContrast.AaText),
            colours.SurfaceVariant, OgContrast.AaText);
        var onSurfaceVariant = Move(colours.OnSurfaceVariant, colours.Surface, OgContrast.AaText);

        // The status and accent colours as text and as component state on the surface - the accent first, because the label is measured
        // against the accent as it will finally be: enforcing the label against the original amber and then darkening the amber leaves a
        // pair that fails again.
        var accent = Move(colours.Accent, colours.Surface, OgContrast.AaUi);

        // Accent text on the accent. The label moves first; if it has nowhere to go - white on a mid-tone amber cannot reach 4.5:1 - the
        // accent itself is darkened under it, which is the pair being brought to the threshold rather than one side of it.
        var (accentForeground, settledAccent) = OgContrast.EnsurePair(
            colours.AccentForeground, accent, OgContrast.AaText, out var accentPairChange);

        if (!string.IsNullOrEmpty(accentPairChange))
        {
            changes.Add(accentPairChange);
            accent = settledAccent;
        }
        var danger = Move(colours.Danger, colours.Surface, OgContrast.AaUi);
        var success = Move(colours.Success, colours.Surface, OgContrast.AaUi);
        var warning = Move(colours.Warning, colours.Surface, OgContrast.AaUi);

        return (colours with
        {
            OnSurface = onSurface,
            OnSurfaceVariant = onSurfaceVariant,
            AccentForeground = accentForeground,
            Accent = accent,
            Danger = danger,
            Success = success,
            Warning = warning,
        }, changes);
    }

    /// <summary>The shipped default palette (Controls/Themes/Tokens.xaml).</summary>
    public static OgThemeColours Default { get; } = new(
        Background: "#F4F6F8",
        Surface: "#FFFFFF",
        SurfaceAlt: "#EDF1F5",
        TextPrimary: "#16202B",
        TextSecondary: "#5A6775",
        Accent: "#0B6E4F",
        Danger: "#B3261E",
        Success: "#1E7A45",
        Warning: "#B26A00");
}
