namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>One destination in the command palette.</summary>
public sealed record OgPaletteEntry(string Route, string Title, string Group);

/// <summary>
/// REL-03 AC-7: the command palette's model. Filtering and keyboard movement are pure, so the palette can be
/// driven — and tested — without a window or a mouse.
/// </summary>
/// <remarks>
/// Matching is deliberately forgiving: case-insensitive, and every whitespace-separated term must appear in the
/// title, route or group. An empty query lists everything, so opening the palette and pressing Enter returns to
/// the first page rather than doing nothing.
/// </remarks>
public static class OgCommandPalette
{
    public static IReadOnlyList<OgPaletteEntry> Filter(
        IReadOnlyList<OgPaletteEntry> entries,
        string? query)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var terms = (query ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (terms.Length == 0)
        {
            return entries;
        }

        return entries
            .Where(entry => terms.All(term =>
                entry.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                || entry.Route.Contains(term, StringComparison.OrdinalIgnoreCase)
                || entry.Group.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    /// <summary>Moves the selection by <paramref name="delta"/>, wrapping at both ends.</summary>
    public static int Move(int current, int delta, int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        var next = (current + delta) % count;

        return next < 0 ? next + count : next;
    }

    /// <summary>The entry at the selection, or null when the filtered list is empty.</summary>
    public static OgPaletteEntry? Selected(IReadOnlyList<OgPaletteEntry> entries, int index) =>
        entries.Count == 0 ? null : entries[Math.Clamp(index, 0, entries.Count - 1)];

    /// <summary>Every navigable page, with its title and group, for the palette to show.</summary>
    public static IReadOnlyList<OgPaletteEntry> FromCatalog(Func<string, string> title)
    {
        ArgumentNullException.ThrowIfNull(title);

        return OgModuleCatalog.Pages
            .Select(page => new OgPaletteEntry(page.Route, title(page.Route), page.ModuleKey))
            .ToList();
    }
}
