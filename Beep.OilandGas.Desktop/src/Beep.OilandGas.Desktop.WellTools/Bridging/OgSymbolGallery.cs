using SkiaSharp;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.WellSchematic;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>One row of the symbol gallery.</summary>
public sealed record OgSymbolRow(string Id, string Name, string Category, string Group, bool IsEquipment);

/// <summary>What the gallery reports above the grid.</summary>
public sealed record OgSymbolSummary(int Symbols, int Categories, int Shown, string Source, string? Notice);

/// <summary>One assignment change, with what it replaced.</summary>
public sealed record OgSymbolAssignment(
    string EquipmentId,
    string EquipmentName,
    string PreviousSymbolId,
    string SymbolId)
{
    public string Describe() => $"{EquipmentName}: {PreviousSymbolId} → {SymbolId}";
}

/// <summary>
/// VSB-08: the symbol gallery and schematic symbol editing.
/// </summary>
/// <remarks>
/// <para>
/// The catalog, the symbols and the drawing are the tools': this class lists them, filters them, renders a
/// preview with <see cref="SymbolRenderer"/> and changes an equipment row's symbol id. Nothing here invents
/// a symbol or a shape.
/// </para>
/// <para>
/// A custom catalog is parsed and validated <b>before</b> it replaces the active one, so a bad file leaves
/// the gallery exactly as it was (AC-5). Parsing may throw; the page reports that through the failure
/// reporter and keeps the active catalog, rather than this class swallowing it.
/// </para>
/// </remarks>
public static class OgSymbolGallery
{
    public const string CodeCatalogEmpty = "catalog-empty";

    public static IReadOnlyList<OgSymbolRow> Rows(
        SymbolCatalog catalog,
        string? category = null,
        string? search = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        IEnumerable<SymbolDefinition> symbols = catalog.Symbols;

        if (!string.IsNullOrWhiteSpace(category))
        {
            symbols = symbols.Where(symbol =>
                string.Equals(symbol.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var needle = search.Trim();
            symbols = symbols.Where(symbol =>
                symbol.Id.Contains(needle, StringComparison.OrdinalIgnoreCase)
                || symbol.Name.Contains(needle, StringComparison.OrdinalIgnoreCase)
                || (symbol.Abbreviation ?? string.Empty).Contains(needle, StringComparison.OrdinalIgnoreCase));
        }

        return symbols
            .OrderBy(symbol => symbol.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(symbol => symbol.Name, StringComparer.OrdinalIgnoreCase)
            .Select(symbol => new OgSymbolRow(
                symbol.Id,
                symbol.Name,
                symbol.Category ?? string.Empty,
                symbol.Group ?? string.Empty,
                symbol.IsEquipment))
            .ToList();
    }

    public static IReadOnlyList<string> Categories(SymbolCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return catalog.Symbols
            .Select(symbol => symbol.Category ?? string.Empty)
            .Where(category => category.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(category => category, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static OgSymbolSummary Summarise(
        SymbolCatalog catalog,
        string source,
        string? category = null,
        string? search = null,
        string? notice = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return new OgSymbolSummary(
            catalog.Symbols.Count,
            Categories(catalog).Count,
            Rows(catalog, category, search).Count,
            source,
            notice);
    }

    /// <summary>
    /// Renders one symbol to a BGRA frame (top-down, premultiplied), the same shape the host consumes.
    /// Returns null when the symbol has nothing to draw.
    /// </summary>
    public static byte[]? Preview(SymbolDefinition symbol, int width = 96, int height = 96)
    {
        ArgumentNullException.ThrowIfNull(symbol);

        if (width <= 0 || height <= 0)
        {
            return null;
        }

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        SymbolRenderer.Draw(
            canvas,
            symbol,
            new SKRect(4, 4, width - 4, height - 4),
            stretch: false,
            drawLabels: false);

        canvas.Flush();

        using var image = surface.Snapshot();
        using var bitmap = SKBitmap.FromImage(image);
        var frame = new byte[width * height * 4];
        bitmap.GetPixelSpan().CopyTo(frame);

        return frame;
    }

    /// <summary>
    /// Assigns <paramref name="symbolId"/> to an equipment row. The undo action restores the previous id,
    /// and the caller redraws (AC-3, AC-4).
    /// </summary>
    public static OgResult<OgSymbolAssignment> Assign(
        WellSchematicDocument document,
        string equipmentId,
        string symbolId,
        OgLogEditHistory history)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(history);
        ArgumentException.ThrowIfNullOrWhiteSpace(equipmentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(symbolId);

        var equipment = document.Equipment.FirstOrDefault(item =>
            string.Equals(item.Id, equipmentId, StringComparison.Ordinal));

        if (equipment is null)
        {
            return OgResult<OgSymbolAssignment>.Failed(
                "That equipment is not on this schematic.",
                "symbol-equipment-not-found");
        }

        var previous = equipment.SymbolId;
        equipment.SymbolId = symbolId;

        var change = new OgSymbolAssignment(
            equipment.Id,
            string.IsNullOrWhiteSpace(equipment.Label) ? equipment.Id : equipment.Label,
            previous,
            symbolId);

        history.Record(
            new OgLogEdit(OgLogEditKind.Symbol, change.Describe(), previous, symbolId),
            () => equipment.SymbolId = previous);

        return OgResult<OgSymbolAssignment>.Ok(change);
    }

    /// <summary>What a component type would be drawn with, without changing anything.</summary>
    public static string? SymbolFor(string componentType, SymbolCatalog? catalog = null) =>
        WellModelConverter.SymbolFor(componentType, catalog);

    public static IReadOnlyList<string> EquipmentChoices(WellSchematicDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.Equipment
            .Select(item => string.IsNullOrWhiteSpace(item.Label) ? item.Id : item.Label!)
            .ToList();
    }

    public static TheTechIdea.OilGas.WellSchematic.SchematicEquipment? FindEquipment(
        WellSchematicDocument document,
        string labelOrId) =>
        document.Equipment.FirstOrDefault(item =>
            string.Equals(item.Id, labelOrId, StringComparison.Ordinal)
            || string.Equals(item.Label, labelOrId, StringComparison.Ordinal));
}

/// <summary>
/// The catalog the gallery is showing: the tools' default, or a customer file that passed validation.
/// </summary>
public sealed class OgSymbolCatalogState
{
    public SymbolCatalog Active { get; private set; } = SymbolCatalog.Current;

    public string Source { get; private set; } = "default";

    /// <summary>
    /// Replaces the active catalog with a parsed one. A parse failure propagates (the caller reports it);
    /// a catalog with no symbols is refused and the active one is untouched.
    /// </summary>
    public OgResult LoadCustom(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var parsed = SymbolCatalog.FromJson(json);

        if (parsed.Symbols.Count == 0)
        {
            return OgResult.Failed("The catalog contains no symbols; the active catalog is unchanged.", OgSymbolGallery.CodeCatalogEmpty);
        }

        Active = parsed;
        Source = "custom";
        return OgResult.Ok($"{parsed.Symbols.Count} symbol(s) loaded from the custom catalog.");
    }

    /// <summary>AC-6: an explicit action brings the tools' default catalog back.</summary>
    public void RestoreDefault()
    {
        Active = SymbolCatalog.Default;
        Source = "default";
    }
}
