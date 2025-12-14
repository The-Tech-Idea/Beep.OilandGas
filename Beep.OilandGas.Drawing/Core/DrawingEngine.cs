using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.Layers;
using Beep.OilandGas.Drawing.Exceptions;

namespace Beep.OilandGas.Drawing.Core
{
    /// <summary>
    /// Main drawing engine for oil and gas visualizations.
    /// Provides unified rendering pipeline for all visualization types.
    /// </summary>
    public class DrawingEngine : IDisposable
    {
        private readonly List<ILayer> layers;
        private readonly Viewport viewport;
        private bool disposed = false;

        /// <summary>
        /// Gets or sets the canvas width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the canvas height.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public SKColor BackgroundColor { get; set; } = SKColors.White;

        /// <summary>
        /// Gets the viewport for coordinate transformations.
        /// </summary>
        public Viewport Viewport => viewport;

        /// <summary>
        /// Gets the list of layers.
        /// </summary>
        public IReadOnlyList<ILayer> Layers => layers.AsReadOnly();

        /// <summary>
        /// Event raised before rendering starts.
        /// </summary>
        public event EventHandler RenderingStarted;

        /// <summary>
        /// Event raised after rendering completes.
        /// </summary>
        public event EventHandler RenderingCompleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingEngine"/> class.
        /// </summary>
        /// <param name="width">Canvas width in pixels.</param>
        /// <param name="height">Canvas height in pixels.</param>
        public DrawingEngine(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentException("Width must be positive.", nameof(width));
            if (height <= 0)
                throw new ArgumentException("Height must be positive.", nameof(height));

            Width = width;
            Height = height;
            layers = new List<ILayer>();
            viewport = new Viewport(width, height);
        }

        /// <summary>
        /// Adds a layer to the drawing engine.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        /// <returns>The drawing engine instance for method chaining.</returns>
        public DrawingEngine AddLayer(ILayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            layers.Add(layer);
            return this;
        }

        /// <summary>
        /// Removes a layer from the drawing engine.
        /// </summary>
        /// <param name="layer">The layer to remove.</param>
        /// <returns>True if the layer was removed, false otherwise.</returns>
        public bool RemoveLayer(ILayer layer)
        {
            return layers.Remove(layer);
        }

        /// <summary>
        /// Clears all layers.
        /// </summary>
        public void ClearLayers()
        {
            layers.Clear();
        }

        /// <summary>
        /// Renders all layers to a SkiaSharp surface.
        /// </summary>
        /// <returns>The rendered surface.</returns>
        public SKSurface Render()
        {
            RenderingStarted?.Invoke(this, EventArgs.Empty);

            var surface = SKSurface.Create(new SKImageInfo(Width, Height));
            var canvas = surface.Canvas;

            // Clear background
            canvas.Clear(BackgroundColor);

            // Render layers in order
            foreach (var layer in layers.OrderBy(l => l.ZOrder))
            {
                if (layer.IsVisible)
                {
                    try
                    {
                        layer.Render(canvas, viewport);
                    }
                    catch (Exception ex)
                    {
                        throw new RenderingException($"Layer '{layer.Name}'", 
                            $"Failed to render layer: {ex.Message}", ex);
                    }
                }
            }

            RenderingCompleted?.Invoke(this, EventArgs.Empty);
            return surface;
        }

        /// <summary>
        /// Renders to an image.
        /// </summary>
        /// <returns>The rendered image.</returns>
        public SKImage RenderToImage()
        {
            using (var surface = Render())
            {
                return surface.Snapshot();
            }
        }

        /// <summary>
        /// Gets the bounds of all visible layers.
        /// </summary>
        /// <returns>The bounding rectangle.</returns>
        public SKRect GetBounds()
        {
            if (layers.Count == 0)
                return new SKRect(0, 0, Width, Height);

            var visibleLayers = layers.Where(l => l.IsVisible).ToList();
            if (visibleLayers.Count == 0)
                return new SKRect(0, 0, Width, Height);

            var bounds = visibleLayers[0].GetBounds();
            foreach (var layer in visibleLayers.Skip(1))
            {
                var layerBounds = layer.GetBounds();
                bounds = SKRect.Union(bounds, layerBounds);
            }

            return bounds;
        }

        /// <summary>
        /// Zooms to fit all visible layers.
        /// </summary>
        public void ZoomToFit()
        {
            var bounds = GetBounds();
            if (bounds.Width > 0 && bounds.Height > 0)
            {
                viewport.ZoomToFit(bounds, Width, Height);
            }
        }

        /// <summary>
        /// Zooms to a specific rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to zoom to.</param>
        public void ZoomToRect(SKRect rect)
        {
            viewport.ZoomToFit(rect, Width, Height);
        }

        /// <summary>
        /// Sets the zoom factor.
        /// </summary>
        /// <param name="zoom">The zoom factor (1.0 = 100%).</param>
        public void SetZoom(float zoom)
        {
            viewport.SetZoom(zoom);
        }

        /// <summary>
        /// Pans the viewport.
        /// </summary>
        /// <param name="deltaX">Horizontal pan delta.</param>
        /// <param name="deltaY">Vertical pan delta.</param>
        public void Pan(float deltaX, float deltaY)
        {
            viewport.Pan(deltaX, deltaY);
        }

        /// <summary>
        /// Resets the viewport to default.
        /// </summary>
        public void ResetViewport()
        {
            viewport.Reset();
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    foreach (var layer in layers.OfType<IDisposable>())
                    {
                        layer.Dispose();
                    }
                    layers.Clear();
                }
                disposed = true;
            }
        }
    }
}

