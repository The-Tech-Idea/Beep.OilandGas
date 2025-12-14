using SkiaSharp;
using Beep.OilandGas.Drawing.Core;

namespace Beep.OilandGas.Drawing.Layers
{
    /// <summary>
    /// Base class for layers with common functionality.
    /// </summary>
    public abstract class LayerBase : ILayer
    {
        /// <summary>
        /// Gets or sets the layer name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets whether the layer is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets the Z-order (higher values render on top).
        /// </summary>
        public int ZOrder { get; set; } = 0;

        /// <summary>
        /// Gets or sets the layer opacity (0.0 to 1.0).
        /// </summary>
        public float Opacity
        {
            get => _opacity;
            set => _opacity = System.Math.Max(0.0f, System.Math.Min(1.0f, value));
        }
        private float _opacity = 1.0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerBase"/> class.
        /// </summary>
        /// <param name="name">The layer name.</param>
        protected LayerBase(string name)
        {
            Name = name ?? "Unnamed Layer";
        }

        /// <summary>
        /// Renders the layer to the canvas.
        /// </summary>
        /// <param name="canvas">The SkiaSharp canvas to render to.</param>
        /// <param name="viewport">The viewport for coordinate transformations.</param>
        public void Render(SKCanvas canvas, Viewport viewport)
        {
            if (!IsVisible || Opacity <= 0)
                return;

            // Save canvas state
            canvas.Save();

            try
            {
                // Apply opacity
                if (Opacity < 1.0f)
                {
                    var paint = new SKPaint { Color = new SKColor(255, 255, 255, (byte)(Opacity * 255)) };
                    canvas.SaveLayer(paint);
                }

                // Render layer content
                RenderContent(canvas, viewport);
            }
            finally
            {
                // Restore canvas state
                canvas.Restore();
            }
        }

        /// <summary>
        /// Renders the layer content. Override this method to implement layer-specific rendering.
        /// </summary>
        /// <param name="canvas">The SkiaSharp canvas to render to.</param>
        /// <param name="viewport">The viewport for coordinate transformations.</param>
        protected abstract void RenderContent(SKCanvas canvas, Viewport viewport);

        /// <summary>
        /// Gets the bounding rectangle of the layer content.
        /// </summary>
        /// <returns>The bounding rectangle.</returns>
        public abstract SKRect GetBounds();
    }
}

