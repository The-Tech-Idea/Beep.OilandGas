using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>What the pointer is over: depth, track and item, in the document's own units.</summary>
public sealed record OgCursorReadout(float? Depth, string Unit, string? TrackId, string? ItemId)
{
    /// <summary>
    /// Reads the cursor position from the surface's own state and hit test. The surface sets
    /// <c>View.CursorDepth</c> while the pointer moves and <c>HitTest</c> resolves the track/item under a
    /// point, so the readout never guesses from the page's own maths.
    /// </summary>
    public static OgCursorReadout At(WellboreView view, float surfaceX, float surfaceY)
    {
        ArgumentNullException.ThrowIfNull(view);

        var (trackId, itemId) = view.HitTest(surfaceX, surfaceY);
        var unit = view.Document?.Axis.Unit ?? DepthUnit.Meters;

        return new OgCursorReadout(view.View.CursorDepth, DepthUnits.Symbol(unit), trackId, itemId);
    }

    public string Describe() =>
        Depth is { } depth
            ? $"Depth {depth:0.0} {Unit}  ·  track {TrackId ?? "—"}  ·  item {ItemId ?? "—"}"
            : $"No cursor  ·  track {TrackId ?? "—"}  ·  item {ItemId ?? "—"}";
}
