using SkiaSharp;
using Beep.OilandGas.Drawing.Core;

namespace Beep.OilandGas.Drawing.Layers
{
    /// <summary>
    /// Interface for a drawable layer in the drawing engine.
    /// </summary>
    public interface ILayer
    {
        /// <summary>
        /// Gets or sets the layer name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets whether the layer is visible.
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the Z-order (higher values render on top).
        /// </summary>
        int ZOrder { get; set; }

        /// <summary>
        /// Gets or sets the layer opacity (0.0 to 1.0).
        /// </summary>
        float Opacity { get; set; }

        /// <summary>
        /// Renders the layer to the canvas.
        /// </summary>
        /// <param name="canvas">The SkiaSharp canvas to render to.</param>
        /// <param name="viewport">The viewport for coordinate transformations.</param>
        void Render(SKCanvas canvas, Viewport viewport);

        /// <summary>
        /// Gets the bounding rectangle of the layer content.
        /// </summary>
        /// <returns>The bounding rectangle.</returns>
        SKRect GetBounds();
    }
}

