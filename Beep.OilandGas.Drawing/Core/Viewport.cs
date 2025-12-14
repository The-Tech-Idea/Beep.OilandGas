using SkiaSharp;
using System;

namespace Beep.OilandGas.Drawing.Core
{
    /// <summary>
    /// Represents a viewport for coordinate transformations and camera control.
    /// </summary>
    public class Viewport
    {
        private float zoom = 1.0f;
        private float panX = 0.0f;
        private float panY = 0.0f;
        private readonly int canvasWidth;
        private readonly int canvasHeight;

        /// <summary>
        /// Gets or sets the zoom factor (1.0 = 100%).
        /// </summary>
        public float Zoom
        {
            get => zoom;
            set => zoom = Math.Max(0.1f, Math.Min(10.0f, value));
        }

        /// <summary>
        /// Gets or sets the horizontal pan offset.
        /// </summary>
        public float PanX
        {
            get => panX;
            set => panX = value;
        }

        /// <summary>
        /// Gets or sets the vertical pan offset.
        /// </summary>
        public float PanY
        {
            get => panY;
            set => panY = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Viewport"/> class.
        /// </summary>
        /// <param name="canvasWidth">Canvas width in pixels.</param>
        /// <param name="canvasHeight">Canvas height in pixels.</param>
        public Viewport(int canvasWidth, int canvasHeight)
        {
            this.canvasWidth = canvasWidth;
            this.canvasHeight = canvasHeight;
        }

        /// <summary>
        /// Transforms world coordinates to screen coordinates.
        /// </summary>
        /// <param name="worldX">World X coordinate.</param>
        /// <param name="worldY">World Y coordinate.</param>
        /// <returns>Screen coordinates.</returns>
        public SKPoint WorldToScreen(float worldX, float worldY)
        {
            float screenX = (worldX + panX) * zoom + canvasWidth / 2.0f;
            float screenY = (worldY + panY) * zoom + canvasHeight / 2.0f;
            return new SKPoint(screenX, screenY);
        }

        /// <summary>
        /// Transforms screen coordinates to world coordinates.
        /// </summary>
        /// <param name="screenX">Screen X coordinate.</param>
        /// <param name="screenY">Screen Y coordinate.</param>
        /// <returns>World coordinates.</returns>
        public SKPoint ScreenToWorld(float screenX, float screenY)
        {
            float worldX = (screenX - canvasWidth / 2.0f) / zoom - panX;
            float worldY = (screenY - canvasHeight / 2.0f) / zoom - panY;
            return new SKPoint(worldX, worldY);
        }

        /// <summary>
        /// Zooms to fit a rectangle in the viewport.
        /// </summary>
        /// <param name="bounds">The rectangle to fit.</param>
        /// <param name="width">Viewport width.</param>
        /// <param name="height">Viewport height.</param>
        public void ZoomToFit(SKRect bounds, int width, int height)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            float scaleX = width / bounds.Width;
            float scaleY = height / bounds.Height;
            zoom = Math.Min(scaleX, scaleY) * 0.9f; // 90% to add padding

            panX = -(bounds.MidX);
            panY = -(bounds.MidY);
        }

        /// <summary>
        /// Sets the zoom factor.
        /// </summary>
        /// <param name="newZoom">The new zoom factor.</param>
        public void SetZoom(float newZoom)
        {
            Zoom = newZoom;
        }

        /// <summary>
        /// Zooms in by a factor.
        /// </summary>
        /// <param name="factor">The zoom increment factor (default: 1.2).</param>
        public void ZoomIn(float factor = 1.2f)
        {
            Zoom *= factor;
        }

        /// <summary>
        /// Zooms out by a factor.
        /// </summary>
        /// <param name="factor">The zoom decrement factor (default: 1.2).</param>
        public void ZoomOut(float factor = 1.2f)
        {
            Zoom /= factor;
        }

        /// <summary>
        /// Pans the viewport.
        /// </summary>
        /// <param name="deltaX">Horizontal pan delta.</param>
        /// <param name="deltaY">Vertical pan delta.</param>
        public void Pan(float deltaX, float deltaY)
        {
            panX += deltaX / zoom;
            panY += deltaY / zoom;
        }

        /// <summary>
        /// Resets the viewport to default.
        /// </summary>
        public void Reset()
        {
            zoom = 1.0f;
            panX = 0.0f;
            panY = 0.0f;
        }

        /// <summary>
        /// Gets the transformation matrix.
        /// </summary>
        /// <returns>The transformation matrix.</returns>
        public SKMatrix GetMatrix()
        {
            return SKMatrix.CreateScale(zoom, zoom)
                .PostConcat(SKMatrix.CreateTranslation(
                    panX * zoom + canvasWidth / 2.0f,
                    panY * zoom + canvasHeight / 2.0f));
        }
    }
}

