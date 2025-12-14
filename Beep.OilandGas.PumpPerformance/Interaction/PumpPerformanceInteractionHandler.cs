using System;
using SkiaSharp;
using Beep.OilandGas.PumpPerformance.Rendering;

namespace Beep.OilandGas.PumpPerformance.Interaction
{
    /// <summary>
    /// Interaction mode for pump performance plots.
    /// </summary>
    public enum PumpPerformanceInteractionMode
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
    /// Handles user interaction events (mouse, touch) for pump performance plots.
    /// </summary>
    public class PumpPerformanceInteractionHandler
    {
        private readonly PumpPerformanceRenderer renderer;
        private bool isMouseDown;
        private SKPoint lastMousePosition;
        private float canvasWidth = 800f;
        private float canvasHeight = 600f;

        /// <summary>
        /// Gets or sets the current interaction mode.
        /// </summary>
        public PumpPerformanceInteractionMode Mode { get; set; } = PumpPerformanceInteractionMode.Pan;

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
        public Func<float, float, (double flowRate, double head)> ScreenToDataTransform { get; set; }

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
        /// Initializes a new instance of the <see cref="PumpPerformanceInteractionHandler"/> class.
        /// </summary>
        /// <param name="renderer">The pump performance renderer.</param>
        public PumpPerformanceInteractionHandler(PumpPerformanceRenderer renderer)
        {
            this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        /// <summary>
        /// Sets the canvas size for coordinate transformations.
        /// </summary>
        public void SetCanvasSize(float width, float height)
        {
            canvasWidth = width;
            canvasHeight = height;
        }

        /// <summary>
        /// Handles mouse down event.
        /// </summary>
        public void OnMouseDown(float x, float y)
        {
            isMouseDown = true;
            lastMousePosition = new SKPoint(x, y);

            if (Mode == PumpPerformanceInteractionMode.PointSelection)
            {
                HandlePointSelection(x, y);
            }
        }

        /// <summary>
        /// Handles mouse move event.
        /// </summary>
        public void OnMouseMove(float x, float y)
        {
            if (!isMouseDown)
                return;

            if (Mode == PumpPerformanceInteractionMode.Pan && EnablePan)
            {
                float deltaX = x - lastMousePosition.X;
                float deltaY = y - lastMousePosition.Y;

                var currentPan = renderer.PanOffset;
                renderer.PanOffset = new SKPoint(currentPan.X + deltaX, currentPan.Y + deltaY);

                ViewportChanged?.Invoke(this, EventArgs.Empty);
            }

            lastMousePosition = new SKPoint(x, y);
        }

        /// <summary>
        /// Handles mouse up event.
        /// </summary>
        public void OnMouseUp(float x, float y)
        {
            isMouseDown = false;
        }

        /// <summary>
        /// Handles mouse wheel event for zooming.
        /// </summary>
        public void OnMouseWheel(float x, float y, float delta)
        {
            if (!EnableZoom || Mode != PumpPerformanceInteractionMode.Zoom)
                return;

            // Calculate zoom factor
            double zoomFactor = 1.0 + (delta > 0 ? ZoomSensitivity : -ZoomSensitivity);
            double newZoom = renderer.Zoom * zoomFactor;

            // Zoom towards mouse position
            if (ScreenToDataTransform != null)
            {
                var dataPoint = ScreenToDataTransform(x, y);
                renderer.Zoom = newZoom;

                // Adjust pan to zoom towards mouse position
                if (DataToScreenTransform != null)
                {
                    var newScreenPoint = DataToScreenTransform(dataPoint.flowRate, dataPoint.head);
                    var currentPan = renderer.PanOffset;
                    var deltaX = x - newScreenPoint.screenX;
                    var deltaY = y - newScreenPoint.screenY;
                    renderer.PanOffset = new SKPoint(currentPan.X + deltaX, currentPan.Y + deltaY);
                }
            }
            else
            {
                renderer.Zoom = newZoom;
            }

            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles touch down event.
        /// </summary>
        public void OnTouchDown(float x, float y)
        {
            OnMouseDown(x, y);
        }

        /// <summary>
        /// Handles touch move event.
        /// </summary>
        public void OnTouchMove(float x, float y)
        {
            OnMouseMove(x, y);
        }

        /// <summary>
        /// Handles touch up event.
        /// </summary>
        public void OnTouchUp(float x, float y)
        {
            OnMouseUp(x, y);
        }

        /// <summary>
        /// Handles pinch gesture for zooming.
        /// </summary>
        public void OnPinch(float centerX, float centerY, float scale)
        {
            if (!EnableZoom)
                return;

            double newZoom = renderer.Zoom * scale;
            renderer.Zoom = newZoom;

            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Resets zoom and pan to default values.
        /// </summary>
        public void ResetView()
        {
            renderer.Zoom = 1.0;
            renderer.PanOffset = SKPoint.Empty;
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Zooms to fit all data.
        /// </summary>
        public void ZoomToFit()
        {
            // Reset zoom and pan
            ResetView();
            // Additional logic can be added here to calculate optimal zoom/pan
        }

        /// <summary>
        /// Handles point selection.
        /// </summary>
        private void HandlePointSelection(float x, float y)
        {
            if (ScreenToDataTransform == null)
                return;

            var dataPoint = ScreenToDataTransform(x, y);
            PointSelected?.Invoke(this, new PointSelectedEventArgs(dataPoint.flowRate, dataPoint.head));
        }
    }

    /// <summary>
    /// Event arguments for point selection.
    /// </summary>
    public class PointSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the selected flow rate.
        /// </summary>
        public double FlowRate { get; }

        /// <summary>
        /// Gets the selected head.
        /// </summary>
        public double Head { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointSelectedEventArgs"/> class.
        /// </summary>
        public PointSelectedEventArgs(double flowRate, double head)
        {
            FlowRate = flowRate;
            Head = head;
        }
    }
}

