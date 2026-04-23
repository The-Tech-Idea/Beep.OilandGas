using SkiaSharp;

namespace Beep.OilandGas.Drawing.Interaction
{
    /// <summary>
    /// Provides optional hit testing for layers that expose selectable or hoverable geometry.
    /// </summary>
    public interface IInteractiveLayer
    {
        /// <summary>
        /// Attempts to resolve the nearest interactive feature at the supplied world-space point.
        /// </summary>
        LayerHitResult HitTest(SKPoint worldPoint, float worldTolerance);
    }
}