using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Beep.OilandGas.HeatMap.Tools
{
    /// <summary>
    /// Types of brush selection tools.
    /// </summary>
    public enum BrushSelectionType
    {
        /// <summary>
        /// Rectangular selection box.
        /// </summary>
        Rectangle,

        /// <summary>
        /// Lasso/polygon selection.
        /// </summary>
        Lasso,

        /// <summary>
        /// Freehand selection.
        /// </summary>
        Freehand,

        /// <summary>
        /// Circular selection.
        /// </summary>
        Circle
    }

    /// <summary>
    /// Manages brush selection tools for heatmap interaction.
    /// </summary>
    public class BrushSelection
    {
        private List<SKPoint> selectionPoints;
        private BrushSelectionType selectionType;
        private bool isActive;

        /// <summary>
        /// Gets or sets the selection type.
        /// </summary>
        public BrushSelectionType SelectionType
        {
            get => selectionType;
            set
            {
                selectionType = value;
                if (selectionType != BrushSelectionType.Lasso && selectionType != BrushSelectionType.Freehand)
                {
                    // Clear points for non-polygon selections
                    if (selectionPoints.Count > 2)
                        selectionPoints = new List<SKPoint> { selectionPoints[0], selectionPoints[selectionPoints.Count - 1] };
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the selection is active.
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                if (!value)
                {
                    selectionPoints.Clear();
                }
            }
        }

        /// <summary>
        /// Gets the selection points.
        /// </summary>
        public IReadOnlyList<SKPoint> SelectionPoints => selectionPoints.AsReadOnly();

        /// <summary>
        /// Gets or sets the selection color.
        /// </summary>
        public SKColor SelectionColor { get; set; } = new SKColor(0, 100, 255, 150);

        /// <summary>
        /// Gets or sets the selection border color.
        /// </summary>
        public SKColor BorderColor { get; set; } = SKColors.Blue;

        /// <summary>
        /// Gets or sets the border width.
        /// </summary>
        public float BorderWidth { get; set; } = 2f;

        /// <summary>
        /// Gets or sets whether to show selection fill.
        /// </summary>
        public bool ShowFill { get; set; } = true;

        /// <summary>
        /// Event raised when selection is completed.
        /// </summary>
        public event EventHandler<SelectionCompletedEventArgs> SelectionCompleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrushSelection"/> class.
        /// </summary>
        public BrushSelection()
        {
            selectionPoints = new List<SKPoint>();
            selectionType = BrushSelectionType.Rectangle;
        }

        /// <summary>
        /// Starts a new selection at the specified point.
        /// </summary>
        public void StartSelection(float x, float y)
        {
            isActive = true;
            selectionPoints.Clear();
            selectionPoints.Add(new SKPoint(x, y));
        }

        /// <summary>
        /// Updates the selection with a new point.
        /// </summary>
        public void UpdateSelection(float x, float y)
        {
            if (!isActive)
                return;

            if (selectionType == BrushSelectionType.Rectangle || selectionType == BrushSelectionType.Circle)
            {
                // For rectangle/circle, only keep start and current point
                if (selectionPoints.Count > 1)
                    selectionPoints.RemoveAt(1);
                selectionPoints.Add(new SKPoint(x, y));
            }
            else
            {
                // For lasso/freehand, add point to path
                selectionPoints.Add(new SKPoint(x, y));
            }
        }

        /// <summary>
        /// Completes the selection.
        /// </summary>
        public void CompleteSelection()
        {
            if (!isActive || selectionPoints.Count < 2)
                return;

            isActive = false;

            var bounds = GetSelectionBounds();
            var selectedPoints = GetSelectedPoints(new List<HeatMapDataPoint>(), 0, 0, 1.0f, 0, 0); // Placeholder

            SelectionCompleted?.Invoke(this, new SelectionCompletedEventArgs
            {
                SelectionBounds = bounds,
                SelectionPoints = selectionPoints.ToList()
            });
        }

        /// <summary>
        /// Cancels the current selection.
        /// </summary>
        public void CancelSelection()
        {
            isActive = false;
            selectionPoints.Clear();
        }

        /// <summary>
        /// Gets the bounding rectangle of the selection.
        /// </summary>
        public SKRect GetSelectionBounds()
        {
            if (selectionPoints.Count == 0)
                return SKRect.Empty;

            float minX = selectionPoints.Min(p => p.X);
            float maxX = selectionPoints.Max(p => p.X);
            float minY = selectionPoints.Min(p => p.Y);
            float maxY = selectionPoints.Max(p => p.Y);

            return new SKRect(minX, minY, maxX, maxY);
        }

        /// <summary>
        /// Gets data points within the selection.
        /// </summary>
        public List<HeatMapDataPoint> GetSelectedPoints(
            List<HeatMapDataPoint> dataPoints,
            float scaleX, float scaleY, float zoom,
            float offsetX, float offsetY)
        {
            if (selectionPoints.Count < 2)
                return new List<HeatMapDataPoint>();

            var selected = new List<HeatMapDataPoint>();

            foreach (var point in dataPoints)
            {
                // Transform point to screen coordinates
                float screenX = (float)(point.X * scaleX * zoom + offsetX);
                float screenY = (float)(point.Y * scaleY * zoom + offsetY);

                if (IsPointInSelection(screenX, screenY))
                {
                    selected.Add(point);
                }
            }

            return selected;
        }

        /// <summary>
        /// Checks if a point is within the selection.
        /// </summary>
        private bool IsPointInSelection(float x, float y)
        {
            if (selectionPoints.Count < 2)
                return false;

            switch (selectionType)
            {
                case BrushSelectionType.Rectangle:
                    var rect = GetSelectionBounds();
                    return rect.Contains(x, y);

                case BrushSelectionType.Circle:
                    var center = new SKPoint(
                        (selectionPoints[0].X + selectionPoints[1].X) / 2,
                        (selectionPoints[0].Y + selectionPoints[1].Y) / 2);
                    float radius = (float)Math.Sqrt(
                        Math.Pow(selectionPoints[1].X - selectionPoints[0].X, 2) +
                        Math.Pow(selectionPoints[1].Y - selectionPoints[0].Y, 2)) / 2;
                    float distance = (float)Math.Sqrt(
                        Math.Pow(x - center.X, 2) + Math.Pow(y - center.Y, 2));
                    return distance <= radius;

                case BrushSelectionType.Lasso:
                case BrushSelectionType.Freehand:
                    return IsPointInPolygon(x, y, selectionPoints);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if a point is inside a polygon using ray casting algorithm.
        /// </summary>
        private bool IsPointInPolygon(float x, float y, List<SKPoint> polygon)
        {
            if (polygon.Count < 3)
                return false;

            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (((polygon[i].Y > y) != (polygon[j].Y > y)) &&
                    (x < (polygon[j].X - polygon[i].X) * (y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        /// <summary>
        /// Renders the selection on the canvas.
        /// </summary>
        public void Render(SKCanvas canvas)
        {
            if (!isActive || selectionPoints.Count < 2)
                return;

            switch (selectionType)
            {
                case BrushSelectionType.Rectangle:
                    RenderRectangle(canvas);
                    break;

                case BrushSelectionType.Circle:
                    RenderCircle(canvas);
                    break;

                case BrushSelectionType.Lasso:
                case BrushSelectionType.Freehand:
                    RenderPolygon(canvas);
                    break;
            }
        }

        private void RenderRectangle(SKCanvas canvas)
        {
            var rect = GetSelectionBounds();
            
            if (ShowFill)
            {
                using (var fillPaint = new SKPaint
                {
                    Color = SelectionColor,
                    Style = SKPaintStyle.Fill
                })
                {
                    canvas.DrawRect(rect, fillPaint);
                }
            }

            using (var borderPaint = new SKPaint
            {
                Color = BorderColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = BorderWidth,
                IsAntialias = true
            })
            {
                canvas.DrawRect(rect, borderPaint);
            }
        }

        private void RenderCircle(SKCanvas canvas)
        {
            var center = new SKPoint(
                (selectionPoints[0].X + selectionPoints[1].X) / 2,
                (selectionPoints[0].Y + selectionPoints[1].Y) / 2);
            float radius = (float)Math.Sqrt(
                Math.Pow(selectionPoints[1].X - selectionPoints[0].X, 2) +
                Math.Pow(selectionPoints[1].Y - selectionPoints[0].Y, 2)) / 2;

            if (ShowFill)
            {
                using (var fillPaint = new SKPaint
                {
                    Color = SelectionColor,
                    Style = SKPaintStyle.Fill
                })
                {
                    canvas.DrawCircle(center, radius, fillPaint);
                }
            }

            using (var borderPaint = new SKPaint
            {
                Color = BorderColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = BorderWidth,
                IsAntialias = true
            })
            {
                canvas.DrawCircle(center, radius, borderPaint);
            }
        }

        private void RenderPolygon(SKCanvas canvas)
        {
            if (selectionPoints.Count < 3)
                return;

            using (var path = new SKPath())
            {
                path.MoveTo(selectionPoints[0]);
                for (int i = 1; i < selectionPoints.Count; i++)
                {
                    path.LineTo(selectionPoints[i]);
                }
                path.Close();

                if (ShowFill)
                {
                    using (var fillPaint = new SKPaint
                    {
                        Color = SelectionColor,
                        Style = SKPaintStyle.Fill
                    })
                    {
                        canvas.DrawPath(path, fillPaint);
                    }
                }

                using (var borderPaint = new SKPaint
                {
                    Color = BorderColor,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = BorderWidth,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, borderPaint);
                }
            }
        }
    }

    /// <summary>
    /// Event arguments for selection completion.
    /// </summary>
    public class SelectionCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the selection bounds.
        /// </summary>
        public SKRect SelectionBounds { get; set; }

        /// <summary>
        /// Gets or sets the selection points.
        /// </summary>
        public List<SKPoint> SelectionPoints { get; set; }
    }
}

