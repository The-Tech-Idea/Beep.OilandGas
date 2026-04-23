using SkiaSharp;

namespace Beep.OilandGas.Drawing.Primitives
{
    /// <summary>
    /// Represents a corner anchor for shared drawing primitives.
    /// </summary>
    public enum OverlayAnchor
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    /// <summary>
    /// Resolves overlay bounds inside a canvas using a corner anchor and margin.
    /// </summary>
    public static class OverlayLayout
    {
        public static SKRect ResolveBounds(SKRect canvasBounds, float width, float height, OverlayAnchor anchor, float margin)
        {
            return anchor switch
            {
                OverlayAnchor.TopLeft => new SKRect(
                    canvasBounds.Left + margin,
                    canvasBounds.Top + margin,
                    canvasBounds.Left + margin + width,
                    canvasBounds.Top + margin + height),
                OverlayAnchor.TopRight => new SKRect(
                    canvasBounds.Right - margin - width,
                    canvasBounds.Top + margin,
                    canvasBounds.Right - margin,
                    canvasBounds.Top + margin + height),
                OverlayAnchor.BottomLeft => new SKRect(
                    canvasBounds.Left + margin,
                    canvasBounds.Bottom - margin - height,
                    canvasBounds.Left + margin + width,
                    canvasBounds.Bottom - margin),
                _ => new SKRect(
                    canvasBounds.Right - margin - width,
                    canvasBounds.Bottom - margin - height,
                    canvasBounds.Right - margin,
                    canvasBounds.Bottom - margin)
            };
        }
    }
}