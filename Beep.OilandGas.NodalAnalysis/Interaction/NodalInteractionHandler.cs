using System;
using SkiaSharp;
using Beep.OilandGas.NodalAnalysis.Rendering;

namespace Beep.OilandGas.NodalAnalysis.Interaction
{
    public enum NodalInteractionMode { Pan, Zoom, PointSelection }

    public class NodalInteractionHandler
    {
        private readonly NodalRenderer renderer;
        private bool isMouseDown;
        private SKPoint lastMousePosition;

        public NodalInteractionMode Mode { get; set; } = NodalInteractionMode.Pan;
        public bool EnablePan { get; set; } = true;
        public bool EnableZoom { get; set; } = true;
        public double ZoomSensitivity { get; set; } = 0.1;

        public Func<float, float, (double flowRate, double pressure)> ScreenToDataTransform { get; set; }
        public event EventHandler ViewportChanged;
        public event EventHandler<PointSelectedEventArgs> PointSelected;

        public NodalInteractionHandler(NodalRenderer renderer)
        {
            this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        public void OnMouseDown(float x, float y)
        {
            isMouseDown = true;
            lastMousePosition = new SKPoint(x, y);
            if (Mode == NodalInteractionMode.PointSelection)
                HandlePointSelection(x, y);
        }

        public void OnMouseMove(float x, float y)
        {
            if (!isMouseDown || Mode != NodalInteractionMode.Pan || !EnablePan)
                return;

            float deltaX = x - lastMousePosition.X;
            float deltaY = y - lastMousePosition.Y;
            var currentPan = renderer.PanOffset;
            renderer.PanOffset = new SKPoint(currentPan.X + deltaX, currentPan.Y + deltaY);
            ViewportChanged?.Invoke(this, EventArgs.Empty);
            lastMousePosition = new SKPoint(x, y);
        }

        public void OnMouseUp(float x, float y) => isMouseDown = false;

        public void OnMouseWheel(float x, float y, float delta)
        {
            if (!EnableZoom || Mode != NodalInteractionMode.Zoom)
                return;

            double zoomFactor = 1.0 + (delta > 0 ? ZoomSensitivity : -ZoomSensitivity);
            renderer.Zoom = renderer.Zoom * zoomFactor;
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
            PointSelected?.Invoke(this, new PointSelectedEventArgs(dataPoint.flowRate, dataPoint.pressure));
        }
    }

    public class PointSelectedEventArgs : EventArgs
    {
        public double FlowRate { get; }
        public double Pressure { get; }
        public PointSelectedEventArgs(double flowRate, double pressure)
        {
            FlowRate = flowRate;
            Pressure = pressure;
        }
    }
}

