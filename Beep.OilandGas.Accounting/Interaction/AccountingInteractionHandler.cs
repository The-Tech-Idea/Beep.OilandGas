using System;
using SkiaSharp;
using Beep.OilandGas.Accounting.Rendering;

namespace Beep.OilandGas.Accounting.Interaction
{
    public enum AccountingInteractionMode { Pan, Zoom, PointSelection }

    public class AccountingInteractionHandler
    {
        private readonly AccountingRenderer renderer;
        private bool isMouseDown;
        private SKPoint lastMousePosition;

        public AccountingInteractionMode Mode { get; set; } = AccountingInteractionMode.Pan;
        public bool EnablePan { get; set; } = true;
        public bool EnableZoom { get; set; } = true;
        public double ZoomSensitivity { get; set; } = 0.1;

        public event EventHandler ViewportChanged;
        public event EventHandler<PointSelectedEventArgs> PointSelected;

        public AccountingInteractionHandler(AccountingRenderer renderer)
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
            if (!isMouseDown || Mode != AccountingInteractionMode.Pan || !EnablePan)
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
            if (!EnableZoom || Mode != AccountingInteractionMode.Zoom)
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
        public DateTime Period { get; }
        public decimal Amount { get; }
        public PointSelectedEventArgs(DateTime period, decimal amount)
        {
            Period = period;
            Amount = amount;
        }
    }
}

