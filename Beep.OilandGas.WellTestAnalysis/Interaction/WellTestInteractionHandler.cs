using System;
using SkiaSharp;
using Beep.OilandGas.WellTestAnalysis.Rendering;

namespace Beep.OilandGas.WellTestAnalysis.Interaction
{
    /// <summary>
    /// Interaction mode for well test plots.
    /// </summary>
    public enum WellTestInteractionMode
    {
        Pan,
        Zoom,
        PointSelection
    }

    /// <summary>
    /// Handles user interaction events for well test plots.
    /// </summary>
    public class WellTestInteractionHandler
    {
        private readonly WellTestRenderer renderer;
        private bool isMouseDown;
        private SKPoint lastMousePosition;
        private float canvasWidth = 800f;
        private float canvasHeight = 600f;

        public WellTestInteractionMode Mode { get; set; } = WellTestInteractionMode.Pan;
        public bool EnablePan { get; set; } = true;
        public bool EnableZoom { get; set; } = true;
        public double ZoomSensitivity { get; set; } = 0.1;

        public Func<float, float, (double time, double pressure)> ScreenToDataTransform { get; set; }
        public Func<double, double, (float screenX, float screenY)> DataToScreenTransform { get; set; }

        public event EventHandler ViewportChanged;
        public event EventHandler<PointSelectedEventArgs> PointSelected;

        public WellTestInteractionHandler(WellTestRenderer renderer)
        {
            this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        public void SetCanvasSize(float width, float height)
        {
            canvasWidth = width;
            canvasHeight = height;
        }

        public void OnMouseDown(float x, float y)
        {
            isMouseDown = true;
            lastMousePosition = new SKPoint(x, y);

            if (Mode == WellTestInteractionMode.PointSelection)
            {
                HandlePointSelection(x, y);
            }
        }

        public void OnMouseMove(float x, float y)
        {
            if (!isMouseDown)
                return;

            if (Mode == WellTestInteractionMode.Pan && EnablePan)
            {
                float deltaX = x - lastMousePosition.X;
                float deltaY = y - lastMousePosition.Y;

                var currentPan = renderer.PanOffset;
                renderer.PanOffset = new SKPoint(currentPan.X + deltaX, currentPan.Y + deltaY);

                ViewportChanged?.Invoke(this, EventArgs.Empty);
            }

            lastMousePosition = new SKPoint(x, y);
        }

        public void OnMouseUp(float x, float y)
        {
            isMouseDown = false;
        }

        public void OnMouseWheel(float x, float y, float delta)
        {
            if (!EnableZoom || Mode != WellTestInteractionMode.Zoom)
                return;

            double zoomFactor = 1.0 + (delta > 0 ? ZoomSensitivity : -ZoomSensitivity);
            double newZoom = renderer.Zoom * zoomFactor;

            if (ScreenToDataTransform != null)
            {
                var dataPoint = ScreenToDataTransform(x, y);
                renderer.Zoom = newZoom;

                if (DataToScreenTransform != null)
                {
                    var newScreenPoint = DataToScreenTransform(dataPoint.time, dataPoint.pressure);
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

        public void ResetView()
        {
            renderer.Zoom = 1.0;
            renderer.PanOffset = SKPoint.Empty;
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        private void HandlePointSelection(float x, float y)
        {
            if (ScreenToDataTransform == null)
                return;

            var dataPoint = ScreenToDataTransform(x, y);
            PointSelected?.Invoke(this, new PointSelectedEventArgs(dataPoint.time, dataPoint.pressure));
        }
    }

    /// <summary>
    /// Event arguments for point selection.
    /// </summary>
    public class PointSelectedEventArgs : EventArgs
    {
        public double Time { get; }
        public double Pressure { get; }

        public PointSelectedEventArgs(double time, double pressure)
        {
            Time = time;
            Pressure = pressure;
        }
    }
}

