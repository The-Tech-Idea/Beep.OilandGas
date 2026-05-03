using System;
using SkiaSharp;
using Beep.OilandGas.DCA.Rendering;

namespace Beep.OilandGas.DCA.Interaction
{
    /// <summary>
    /// Interaction mode for DCA plots.
    /// </summary>
    public enum DCAInteractionMode
    {
        /// <summary>
        /// Pan mode - drag to pan the plot.
        /// </summary>
        Pan,

        /// <summary>
        /// Zoom mode - scroll to zoom.
        /// </summary>
        Zoom,

        /// <summary>
        /// Point selection mode - click to select points.
        /// </summary>
        PointSelection
    }

    /// <summary>
    /// Handles user interaction events (mouse, touch) for DCA plots.
    /// </summary>
    public class DCAInteractionHandler
    {
        private readonly DCARenderer renderer;
        private bool isMouseDown;
        private SKPoint lastMousePosition;
        private float canvasWidth = 800f;
        private float canvasHeight = 600f;

        /// <summary>
        /// Gets or sets the current interaction mode.
        /// </summary>
        public DCAInteractionMode Mode { get; set; } = DCAInteractionMode.Pan;

        /// <summary>
        /// Gets or sets whether to enable panning.
        /// </summary>
        public bool EnablePan { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to enable zooming.
        /// </summary>
        public bool EnableZoom { get; set; } = true;

        /// <summary>
        /// Gets or sets the zoom sensitivity.
        /// </summary>
        public double ZoomSensitivity { get; set; } = 0.1;

        /// <summary>
        /// Gets or sets the coordinate transformation function (screen to data coordinates).
        /// </summary>
        public Func<float, float, (double time, double rate)> ScreenToDataTransform { get; set; }

        /// <summary>
        /// Gets or sets the coordinate transformation function (data to screen coordinates).
        /// </summary>
        public Func<double, double, (float screenX, float screenY)> DataToScreenTransform { get; set; }

        /// <summary>
        /// Event raised when the viewport changes (zoom/pan).
        /// </summary>
        public event EventHandler ViewportChanged;

        /// <summary>
        /// Event raised when a point is selected.
        /// </summary>
        public event EventHandler<PointSelectedEventArgs> PointSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="DCAInteractionHandler"/> class.
        /// </summary>
        /// <param name="renderer">The DCA renderer to interact with.</param>
        public DCAInteractionHandler(DCARenderer renderer)
        {
            this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            SetupCoordinateTransforms();
        }

        /// <summary>
        /// Sets up coordinate transformation functions.
        /// </summary>
        public void SetupCoordinateTransforms(float canvasWidth, float canvasHeight)
        {
            this.canvasWidth = canvasWidth;
            this.canvasHeight = canvasHeight;
            
            var bounds = renderer.GetBounds();
            double timeRange = bounds.maxTime - bounds.minTime;
            double rateRange = bounds.maxRate - bounds.minRate;

            ScreenToDataTransform = (screenX, screenY) =>
            {
                // Account for margins and zoom/pan
                float plotAreaX = renderer.GetConfiguration().LeftMargin;
                float plotAreaY = renderer.GetConfiguration().TopMargin;
                float plotAreaWidth = canvasWidth - plotAreaX - renderer.GetConfiguration().RightMargin;
                float plotAreaHeight = canvasHeight - plotAreaY - renderer.GetConfiguration().BottomMargin;

                // Convert to plot area coordinates
                float plotX = (screenX - plotAreaX) / (float)renderer.Zoom - renderer.PanOffset.X;
                float plotY = (screenY - plotAreaY) / (float)renderer.Zoom - renderer.PanOffset.Y;

                // Normalize to [0, 1]
                double normalizedX = Math.Max(0, Math.Min(1, plotX / plotAreaWidth));
                double normalizedY = Math.Max(0, Math.Min(1, 1.0 - (plotY / plotAreaHeight))); // Invert Y

                // Convert to data coordinates
                double time = bounds.minTime + normalizedX * timeRange;
                double rate = bounds.minRate + normalizedY * rateRange;

                return (time, rate);
            };

            DataToScreenTransform = (time, rate) =>
            {
                // Normalize data coordinates
                double normalizedX = (time - bounds.minTime) / timeRange;
                double normalizedY = (rate - bounds.minRate) / rateRange;

                float plotAreaX = renderer.GetConfiguration().LeftMargin;
                float plotAreaY = renderer.GetConfiguration().TopMargin;
                float plotAreaWidth = canvasWidth - plotAreaX - renderer.GetConfiguration().RightMargin;
                float plotAreaHeight = canvasHeight - plotAreaY - renderer.GetConfiguration().BottomMargin;

                // Convert to screen coordinates
                float screenX = plotAreaX + (float)(normalizedX * plotAreaWidth * renderer.Zoom + renderer.PanOffset.X);
                float screenY = plotAreaY + (float)((1.0 - normalizedY) * plotAreaHeight * renderer.Zoom + renderer.PanOffset.Y);

                return (screenX, screenY);
            };
        }

        /// <summary>
        /// Sets up coordinate transforms using current canvas size.
        /// </summary>
        private void SetupCoordinateTransforms()
        {
            SetupCoordinateTransforms(canvasWidth, canvasHeight);
        }

        /// <summary>
        /// Handles mouse down event.
        /// </summary>
        public void HandleMouseDown(float x, float y, int button = 0)
        {
            isMouseDown = true;
            lastMousePosition = new SKPoint(x, y);

            if (Mode == DCAInteractionMode.PointSelection && button == 0)
            {
                HandlePointSelection(x, y);
            }
        }

        /// <summary>
        /// Handles mouse move event.
        /// </summary>
        public void HandleMouseMove(float x, float y)
        {
            if (isMouseDown && Mode == DCAInteractionMode.Pan && EnablePan)
            {
                SKPoint delta = new SKPoint(x - lastMousePosition.X, y - lastMousePosition.Y);
                renderer.PanOffset = new SKPoint(
                    renderer.PanOffset.X + delta.X,
                    renderer.PanOffset.Y + delta.Y);
                
                lastMousePosition = new SKPoint(x, y);
                ViewportChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles mouse up event.
        /// </summary>
        public void HandleMouseUp(float x, float y, int button = 0)
        {
            isMouseDown = false;
        }

        /// <summary>
        /// Handles mouse wheel event for zooming.
        /// </summary>
        public void HandleMouseWheel(float x, float y, float delta)
        {
            if (!EnableZoom)
                return;

            // Calculate zoom factor
            double zoomFactor = 1.0 + (delta > 0 ? ZoomSensitivity : -ZoomSensitivity);
            double newZoom = renderer.Zoom * zoomFactor;

            // Clamp zoom
            newZoom = Math.Max(renderer.GetConfiguration().MinZoom, 
                Math.Min(renderer.GetConfiguration().MaxZoom, newZoom));

            // Zoom towards mouse position
            if (ScreenToDataTransform != null)
            {
                var (time, rate) = ScreenToDataTransform(x, y);
                var (oldScreenX, oldScreenY) = DataToScreenTransform(time, rate);

                renderer.Zoom = newZoom;

                var (newScreenX, newScreenY) = DataToScreenTransform(time, rate);

                renderer.PanOffset = new SKPoint(
                    renderer.PanOffset.X + (oldScreenX - newScreenX),
                    renderer.PanOffset.Y + (oldScreenY - newScreenY));
            }
            else
            {
                renderer.Zoom = newZoom;
            }

            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles double-click event (zoom to fit).
        /// </summary>
        public void HandleDoubleClick(float x, float y)
        {
            renderer.ZoomToFit(canvasWidth, canvasHeight);
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles point selection.
        /// </summary>
        private void HandlePointSelection(float x, float y)
        {
            if (ScreenToDataTransform == null)
                return;

            var (time, rate) = ScreenToDataTransform(x, y);
            
            // Find nearest point (simplified - would need access to plot data)
            PointSelected?.Invoke(this, new PointSelectedEventArgs(time, rate));
        }

        /// <summary>
        /// Pans the plot by the specified delta.
        /// </summary>
        public void Pan(float deltaX, float deltaY)
        {
            if (!EnablePan)
                return;

            renderer.PanOffset = new SKPoint(
                renderer.PanOffset.X + deltaX,
                renderer.PanOffset.Y + deltaY);
            
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Zooms the plot by the specified factor.
        /// </summary>
        public void Zoom(double factor, float centerX, float centerY)
        {
            if (!EnableZoom)
                return;

            double newZoom = renderer.Zoom * factor;
            newZoom = Math.Max(renderer.GetConfiguration().MinZoom, 
                Math.Min(renderer.GetConfiguration().MaxZoom, newZoom));

            if (ScreenToDataTransform != null)
            {
                var (time, rate) = ScreenToDataTransform(centerX, centerY);
                var (oldScreenX, oldScreenY) = DataToScreenTransform(time, rate);

                renderer.Zoom = newZoom;

                var (newScreenX, newScreenY) = DataToScreenTransform(time, rate);

                renderer.PanOffset = new SKPoint(
                    renderer.PanOffset.X + (oldScreenX - newScreenX),
                    renderer.PanOffset.Y + (oldScreenY - newScreenY));
            }
            else
            {
                renderer.Zoom = newZoom;
            }

            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Zooms to fit all data.
        /// </summary>
        public void ZoomToFit()
        {
            renderer.ZoomToFit(canvasWidth, canvasHeight);
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Event arguments for point selection.
    /// </summary>
    public class PointSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the selected time value.
        /// </summary>
        public double Time { get; }

        /// <summary>
        /// Gets the selected production rate value.
        /// </summary>
        public double Rate { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointSelectedEventArgs"/> class.
        /// </summary>
        public PointSelectedEventArgs(double time, double rate)
        {
            Time = time;
            Rate = rate;
        }
    }
}

