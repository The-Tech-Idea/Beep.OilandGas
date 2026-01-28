using System;
using SkiaSharp;
using Beep.OilandGas.HeatMap.Tools;
using Beep.OilandGas.HeatMap.Rendering;

namespace Beep.OilandGas.HeatMap.Interaction
{
    /// <summary>
    /// Handles user interaction events (mouse, touch) and coordinates with heatmap tools.
    /// This class bridges UI framework events to heatmap interaction tools.
    /// </summary>
    public class InteractionHandler
    {
        private readonly HeatMapRenderer renderer;
        private bool isMouseDown;
        private SKPoint lastMousePosition;
        private float canvasWidth = 800f;
        private float canvasHeight = 600f;

        /// <summary>
        /// Gets or sets the current interaction mode.
        /// </summary>
        public InteractionMode Mode { get; set; } = InteractionMode.Pan;

        /// <summary>
        /// Gets or sets whether to enable panning.
        /// </summary>
        public bool EnablePan { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to enable zooming.
        /// </summary>
        public bool EnableZoom { get; set; } = true;

        /// <summary>
        /// Gets or sets the zoom sensitivity (default: 0.1).
        /// </summary>
        public double ZoomSensitivity { get; set; } = 0.1;

        /// <summary>
        /// Gets or sets the coordinate transformation function (screen to data coordinates).
        /// </summary>
        public Func<float, float, (double dataX, double dataY)> ScreenToDataTransform { get; set; }

        /// <summary>
        /// Gets or sets the coordinate transformation function (data to screen coordinates).
        /// </summary>
        public Func<double, double, (float screenX, float screenY)> DataToScreenTransform { get; set; }

        /// <summary>
        /// Event raised when the viewport changes (zoom/pan).
        /// </summary>
        public event EventHandler ViewportChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionHandler"/> class.
        /// </summary>
        /// <param name="renderer">The heatmap renderer to interact with.</param>
        public InteractionHandler(HeatMapRenderer renderer)
        {
            this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        /// <summary>
        /// Handles mouse down event.
        /// </summary>
        /// <param name="x">Mouse X coordinate in screen space.</param>
        /// <param name="y">Mouse Y coordinate in screen space.</param>
        /// <param name="button">Mouse button (0=left, 1=right, 2=middle).</param>
        public void HandleMouseDown(float x, float y, int button = 0)
        {
            isMouseDown = true;
            lastMousePosition = new SKPoint(x, y);

            switch (Mode)
            {
                case InteractionMode.BrushSelection:
                    if (button == 0) // Left button
                    {
                        renderer.BrushSelection.StartSelection(x, y);
                    }
                    break;

                case InteractionMode.Measurement:
                    if (button == 0) // Left button
                    {
                        renderer.MeasurementTools.StartMeasurement(x, y);
                    }
                    break;

                case InteractionMode.Pan:
                    // Pan starts on mouse down
                    break;

                case InteractionMode.PointSelection:
                    if (button == 0) // Left button
                    {
                        HandlePointSelection(x, y);
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles mouse move event.
        /// </summary>
        /// <param name="x">Mouse X coordinate in screen space.</param>
        /// <param name="y">Mouse Y coordinate in screen space.</param>
        public void HandleMouseMove(float x, float y)
        {
            var currentPosition = new SKPoint(x, y);

            if (isMouseDown)
            {
                switch (Mode)
                {
                    case InteractionMode.BrushSelection:
                        renderer.BrushSelection.UpdateSelection(x, y);
                        break;

                    case InteractionMode.Measurement:
                        renderer.MeasurementTools.UpdateLastPoint(x, y);
                        break;

                    case InteractionMode.Pan:
                        if (EnablePan)
                        {
                            float deltaX = x - lastMousePosition.X;
                            float deltaY = y - lastMousePosition.Y;
                            renderer.PanOffset = new SKPoint(
                                renderer.PanOffset.X + deltaX,
                                renderer.PanOffset.Y + deltaY);
                            ViewportChanged?.Invoke(this, EventArgs.Empty);
                        }
                        break;
                }
            }
            else
            {
                // Handle hover/tooltip
                if (renderer.Interaction.TooltipsEnabled)
                {
                    HandleHover(x, y);
                }
            }

            lastMousePosition = currentPosition;
        }

        /// <summary>
        /// Handles mouse up event.
        /// </summary>
        /// <param name="x">Mouse X coordinate in screen space.</param>
        /// <param name="y">Mouse Y coordinate in screen space.</param>
        /// <param name="button">Mouse button (0=left, 1=right, 2=middle).</param>
        public void HandleMouseUp(float x, float y, int button = 0)
        {
            if (!isMouseDown)
                return;

            isMouseDown = false;

            switch (Mode)
            {
                case InteractionMode.BrushSelection:
                    if (button == 0)
                    {
                        renderer.BrushSelection.CompleteSelection();
                    }
                    break;

                case InteractionMode.Measurement:
                    if (button == 0)
                    {
                        renderer.MeasurementTools.AddPoint(x, y);
                        // For distance/angle, complete after 2/3 points
                        if ((renderer.MeasurementTools.CurrentType == MeasurementType.Distance && 
                             renderer.MeasurementTools.MeasurementPoints.Count >= 2) ||
                            (renderer.MeasurementTools.CurrentType == MeasurementType.Angle && 
                             renderer.MeasurementTools.MeasurementPoints.Count >= 3))
                        {
                            renderer.MeasurementTools.CompleteMeasurement();
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles mouse wheel event for zooming.
        /// </summary>
        /// <param name="x">Mouse X coordinate in screen space.</param>
        /// <param name="y">Mouse Y coordinate in screen space.</param>
        /// <param name="delta">Wheel delta (positive = zoom in, negative = zoom out).</param>
        public void HandleMouseWheel(float x, float y, float delta)
        {
            if (!EnableZoom)
                return;

            double zoomFactor = 1.0 + (delta > 0 ? ZoomSensitivity : -ZoomSensitivity);
            double newZoom = renderer.Zoom * zoomFactor;

            // Clamp zoom (using default min/max if not accessible)
            newZoom = Math.Max(0.1, Math.Min(100.0, newZoom));

            // Zoom towards mouse position
            float zoomRatio = (float)(newZoom / renderer.Zoom);
            renderer.PanOffset = new SKPoint(
                x - (x - renderer.PanOffset.X) * zoomRatio,
                y - (y - renderer.PanOffset.Y) * zoomRatio);

            renderer.Zoom = newZoom;
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles double-click event.
        /// </summary>
        /// <param name="x">Mouse X coordinate in screen space.</param>
        /// <param name="y">Mouse Y coordinate in screen space.</param>
        public void HandleDoubleClick(float x, float y)
        {
            // Zoom to fit on double-click
            renderer.ZoomToFit(this.canvasWidth, this.canvasHeight);
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles point selection at the specified screen coordinates.
        /// </summary>
        private void HandlePointSelection(float x, float y)
        {
            // Convert screen coordinates to data coordinates
            var (dataX, dataY) = ScreenToDataTransform?.Invoke(x, y) ?? (x, y);

            // Find nearest point
            var nearestPoint = FindNearestPoint(dataX, dataY);
            if (nearestPoint != null)
            {
                // Use HandleClick instead of SelectPoint
                renderer.Interaction.HandleClick(x, y, renderer.GetDataPoints(), 
                    renderer.Zoom, renderer.PanOffset);
            }
        }

        /// <summary>
        /// Handles hover/tooltip at the specified screen coordinates.
        /// </summary>
        private void HandleHover(float x, float y)
        {
            // Convert screen coordinates to data coordinates
            var (dataX, dataY) = ScreenToDataTransform?.Invoke(x, y) ?? (x, y);

            // Find nearest point
            var nearestPoint = FindNearestPoint(dataX, dataY);
            if (nearestPoint != null)
            {
                var (screenX, screenY) = DataToScreenTransform?.Invoke(nearestPoint.X, nearestPoint.Y) ?? 
                    ((float)nearestPoint.X, (float)nearestPoint.Y);
                renderer.Interaction.HandleHover(screenX, screenY, 
                    renderer.GetDataPoints(), renderer.Zoom, renderer.PanOffset);
            }
        }

        /// <summary>
        /// Finds the nearest data point to the specified coordinates.
        /// </summary>
        private HEAT_MAP_DATA_POINT FindNearestPoint(double x, double y, double maxDistance = 10.0)
        {
            HEAT_MAP_DATA_POINT nearest = null;
            double minDistance = double.MaxValue;

            foreach (var point in renderer.GetDataPoints())
            {
                double distance = Math.Sqrt(
                    Math.Pow(point.X - x, 2) + Math.Pow(point.Y - y, 2));

                if (distance < minDistance && distance <= maxDistance)
                {
                    minDistance = distance;
                    nearest = point;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Sets up coordinate transformation functions based on renderer bounds.
        /// </summary>
        public void SetupCoordinateTransforms(float canvasWidth, float canvasHeight)
        {
            this.canvasWidth = canvasWidth;
            this.canvasHeight = canvasHeight;
            
            var bounds = renderer.GetBounds();
            double dataWidth = bounds.maxX - bounds.minX;
            double dataHeight = bounds.maxY - bounds.minY;
            
            if (dataWidth <= 0) dataWidth = 1;
            if (dataHeight <= 0) dataHeight = 1;

            ScreenToDataTransform = (screenX, screenY) =>
            {
                // Account for zoom and pan
                float dataX = (screenX - renderer.PanOffset.X) / (float)(renderer.Zoom * canvasWidth / dataWidth);
                float dataY = (screenY - renderer.PanOffset.Y) / (float)(renderer.Zoom * canvasHeight / dataHeight);
                return (bounds.minX + dataX, bounds.minY + dataY);
            };

            DataToScreenTransform = (dataX, dataY) =>
            {
                float screenX = (float)((dataX - bounds.minX) * canvasWidth / dataWidth * renderer.Zoom + renderer.PanOffset.X);
                float screenY = (float)((dataY - bounds.minY) * canvasHeight / dataHeight * renderer.Zoom + renderer.PanOffset.Y);
                return (screenX, screenY);
            };
        }
    }

    /// <summary>
    /// Interaction modes for the heatmap.
    /// </summary>
    public enum InteractionMode
    {
        /// <summary>
        /// Pan mode - drag to pan the viewport.
        /// </summary>
        Pan,

        /// <summary>
        /// Brush selection mode - select area with brush tools.
        /// </summary>
        BrushSelection,

        /// <summary>
        /// Measurement mode - measure distances, areas, angles.
        /// </summary>
        Measurement,

        /// <summary>
        /// Point selection mode - click to select individual points.
        /// </summary>
        PointSelection
    }
}

