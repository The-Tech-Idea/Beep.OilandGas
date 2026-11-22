using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>
/// Switches a well view's depth unit while keeping the person looking at the same rock.
/// </summary>
/// <remarks>
/// The document's values are converted, so the visible window and the cursor must be converted too —
/// otherwise 8200–8300 ft becomes a window over 8200–8300 <em>metres</em>, which is a different part of
/// the well entirely. The window is taken before the conversion and re-applied after it in the new unit.
/// </remarks>
public static class OgDocumentUnits
{
    public static bool Switch(WellboreView view, DepthUnit target)
    {
        ArgumentNullException.ThrowIfNull(view);

        if (view.Document is not { } document || document.Axis.Unit == target)
        {
            return false;
        }

        var from = DepthUnits.Symbol(document.Axis.Unit);
        var to = DepthUnits.Symbol(target);

        var (top, bottom) = view.View.Window(document.Axis);
        var cursor = view.View.CursorDepth;

        document.ConvertTo(target);

        view.View.SetRange(
            document.Axis,
            (float)Units.Convert(top, from, to),
            (float)Units.Convert(bottom, from, to));

        if (cursor is { } depth)
        {
            view.View.CursorDepth = (float)Units.Convert(depth, from, to);
        }

        return true;
    }

    /// <summary>The unit a toggle should switch to next.</summary>
    public static DepthUnit Toggle(DepthUnit current) =>
        current == DepthUnit.Feet ? DepthUnit.Meters : DepthUnit.Feet;
}
