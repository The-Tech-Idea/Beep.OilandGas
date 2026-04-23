using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.Layers;
using Beep.OilandGas.Drawing.Exceptions;
using Beep.OilandGas.Drawing.Interaction;
using Beep.OilandGas.Drawing.Measurements;
using Beep.OilandGas.Drawing.Scenes;

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
        /// Gets the active typed scene, if one has been attached.
        /// </summary>
        public DrawingScene ActiveScene { get; private set; }

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
        /// Attaches a typed scene contract to the drawing engine.
        /// </summary>
        public DrawingEngine UseScene(DrawingScene scene)
        {
            ActiveScene = scene ?? throw new ArgumentNullException(nameof(scene));
            ActiveScene.Validate();

            if (ActiveScene.WorldBounds.HasValue)
            {
                viewport.ZoomToFit(ActiveScene.WorldBounds.Value, Width, Height);
            }
            else
            {
                viewport.ApplyState(ActiveScene.ViewportState);
            }

            SyncSceneViewportState();
            return this;
        }

        /// <summary>
        /// Captures the current viewport state.
        /// </summary>
        public SceneViewportState CaptureViewportState()
        {
            return viewport.GetState();
        }

        /// <summary>
        /// Converts a screen-space point to world coordinates using the current viewport.
        /// </summary>
        public SKPoint ScreenToWorld(SKPoint screenPoint)
        {
            return viewport.ScreenToWorld(screenPoint.X, screenPoint.Y);
        }

        /// <summary>
        /// Converts a world-space point to screen coordinates using the current viewport.
        /// </summary>
        public SKPoint WorldToScreen(SKPoint worldPoint)
        {
            return viewport.WorldToScreen(worldPoint.X, worldPoint.Y);
        }

        /// <summary>
        /// Measures a distance between two screen-space points using the active scene units.
        /// </summary>
        public SceneMeasurementResult MeasureDistance(SKPoint startScreenPoint, SKPoint endScreenPoint)
        {
            return MeasurePathDistance(new[] { startScreenPoint, endScreenPoint });
        }

        /// <summary>
        /// Measures a path length from screen-space vertices using the active scene units.
        /// </summary>
        public SceneMeasurementResult MeasurePathDistance(IEnumerable<SKPoint> screenPoints)
        {
            EnsureSceneMeasurementAvailable();
            return SceneMeasurementService.MeasureDistance(ActiveScene, MapScreenPointsToWorld(screenPoints));
        }

        /// <summary>
        /// Measures a polygon area from screen-space vertices using the active scene units.
        /// </summary>
        public SceneMeasurementResult MeasureArea(IEnumerable<SKPoint> screenPolygon)
        {
            EnsureSceneMeasurementAvailable();
            return SceneMeasurementService.MeasureArea(ActiveScene, MapScreenPointsToWorld(screenPolygon));
        }

        /// <summary>
        /// Resolves the topmost interactive feature beneath a screen-space point.
        /// </summary>
        public LayerHitResult HitTest(SKPoint screenPoint, float screenTolerance = 6f)
        {
            var worldPoint = ScreenToWorld(screenPoint);
            float worldTolerance = GetWorldTolerance(screenTolerance);

            foreach (var layer in layers.OrderByDescending(candidate => candidate.ZOrder))
            {
                if (!layer.IsVisible || layer is not IInteractiveLayer interactiveLayer)
                    continue;

                var hit = interactiveLayer.HitTest(worldPoint, worldTolerance);
                if (hit != null)
                    return hit;
            }

            return null;
        }

        /// <summary>
        /// Persists a resolved feature hit into the active scene interaction state.
        /// </summary>
        public SceneSelectionAnnotation RecordSelection(LayerHitResult hit, bool replaceExisting = false)
        {
            if (hit == null)
                throw new ArgumentNullException(nameof(hit));

            EnsureSceneMeasurementAvailable();
            return ActiveScene.InteractionState.AddSelection(hit, replaceExisting);
        }

        /// <summary>
        /// Persists a measurement annotation from screen-space vertices into the active scene interaction state.
        /// </summary>
        public SceneMeasurementAnnotation RecordMeasurement(SceneMeasurementResult measurement, IEnumerable<SKPoint> screenVertices, string label = null)
        {
            EnsureSceneMeasurementAvailable();
            return ActiveScene.InteractionState.AddMeasurement(measurement, MapScreenPointsToWorld(screenVertices), label);
        }

        /// <summary>
        /// Renders all layers to a SkiaSharp surface.
        /// </summary>
        /// <returns>The rendered surface.</returns>
        public SKSurface Render()
        {
            var surface = SKSurface.Create(new SKImageInfo(Width, Height));
            RenderToCanvas(surface.Canvas);
            return surface;
        }

        /// <summary>
        /// Renders all visible layers to an existing canvas.
        /// </summary>
        /// <param name="canvas">The destination canvas.</param>
        /// <param name="clearCanvas">True to clear the canvas before rendering.</param>
        public void RenderToCanvas(SKCanvas canvas, bool clearCanvas = true)
        {
            if (canvas == null)
                throw new ArgumentNullException(nameof(canvas));

            RenderingStarted?.Invoke(this, EventArgs.Empty);

            if (clearCanvas)
            {
                canvas.Clear(BackgroundColor);
            }

            foreach (var layer in layers.OrderBy(l => l.ZOrder))
            {
                if (!layer.IsVisible)
                    continue;

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

            SceneInteractionRenderer.Render(canvas, viewport, ActiveScene);

            RenderingCompleted?.Invoke(this, EventArgs.Empty);
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
                SyncSceneViewportState();
            }
        }

        /// <summary>
        /// Zooms to a specific rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to zoom to.</param>
        public void ZoomToRect(SKRect rect)
        {
            viewport.ZoomToFit(rect, Width, Height);
            SyncSceneViewportState();
        }

        /// <summary>
        /// Sets the zoom factor.
        /// </summary>
        /// <param name="zoom">The zoom factor (1.0 = 100%).</param>
        public void SetZoom(float zoom)
        {
            viewport.SetZoom(zoom);
            SyncSceneViewportState();
        }

        /// <summary>
        /// Pans the viewport.
        /// </summary>
        /// <param name="deltaX">Horizontal pan delta.</param>
        /// <param name="deltaY">Vertical pan delta.</param>
        public void Pan(float deltaX, float deltaY)
        {
            viewport.Pan(deltaX, deltaY);
            SyncSceneViewportState();
        }

        /// <summary>
        /// Resets the viewport to default.
        /// </summary>
        public void ResetViewport()
        {
            viewport.Reset();
            SyncSceneViewportState();
        }

        private IEnumerable<SKPoint> MapScreenPointsToWorld(IEnumerable<SKPoint> screenPoints)
        {
            if (screenPoints == null)
                throw new ArgumentNullException(nameof(screenPoints));

            return screenPoints.Select(ScreenToWorld).ToList();
        }

        private float GetWorldTolerance(float screenTolerance)
        {
            return Math.Max(0.1f, screenTolerance) / Math.Max(0.0001f, viewport.Zoom);
        }

        private void EnsureSceneMeasurementAvailable()
        {
            if (ActiveScene == null)
                throw new InvalidOperationException("Measurements require an active typed scene with coordinate metadata.");
        }

        private void SyncSceneViewportState()
        {
            if (ActiveScene == null)
                return;

            var state = viewport.GetState();
            ActiveScene.ViewportState.Zoom = state.Zoom;
            ActiveScene.ViewportState.PanX = state.PanX;
            ActiveScene.ViewportState.PanY = state.PanY;
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

