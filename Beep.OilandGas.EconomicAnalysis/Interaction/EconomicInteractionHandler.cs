using System;
using SkiaSharp;
using Beep.OilandGas.EconomicAnalysis.Rendering;

namespace Beep.OilandGas.EconomicAnalysis.Interaction
{
    public enum EconomicInteractionMode { Pan, Zoom, PointSelection }

    public class EconomicInteractionHandler
    {
        private readonly EconomicRenderer renderer;
        private bool isMouseDown;
        private SKPoint lastMousePosition;

        public EconomicInteractionMode Mode { get; set; } = EconomicInteractionMode.Pan;
        public bool EnablePan { get; set; } = true;
        public bool EnableZoom { get; set; } = true;
        public double ZoomSensitivity { get; set; } = 0.1;

        public event EventHandler ViewportChanged;
        public event EventHandler<PointSelectedEventArgs> PointSelected;

        public EconomicInteractionHandler(EconomicRenderer renderer)
        {
            this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        public void OnMouseDown(float x, float y)
        {
            isMouseDown = true;
            lastMousePosition = new SKPoint(x, y);
        }

        public void OnMouseMove(float x, float y)
        {
            if (!isMouseDown || Mode != EconomicInteractionMode.Pan || !EnablePan)
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
            if (!EnableZoom || Mode != EconomicInteractionMode.Zoom)
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
    }

    public class PointSelectedEventArgs : EventArgs
    {
        public int Period { get; }
        public double Value { get; }
        public PointSelectedEventArgs(int period, double value)
        {
            Period = period;
            Value = value;
        }
    }
}

