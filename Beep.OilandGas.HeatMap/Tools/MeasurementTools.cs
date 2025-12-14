using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Beep.OilandGas.HeatMap.Tools
{
    /// <summary>
    /// Types of measurement tools.
    /// </summary>
    public enum MeasurementType
    {
        /// <summary>
        /// Distance measurement (ruler).
        /// </summary>
        Distance,

        /// <summary>
        /// Area measurement (polygon).
        /// </summary>
        Area,

        /// <summary>
        /// Angle measurement.
        /// </summary>
        Angle
    }

    /// <summary>
    /// Manages measurement tools for heatmap interaction.
    /// </summary>
    public class MeasurementTools
    {
        private List<SKPoint> measurementPoints;
        private MeasurementType currentType;
        private bool isActive;

        /// <summary>
        /// Gets or sets the current measurement type.
        /// </summary>
        public MeasurementType CurrentType
        {
            get => currentType;
            set
            {
                currentType = value;
                if (currentType != MeasurementType.Area)
                {
                    // For distance/angle, limit to 2-3 points
                    if (measurementPoints.Count > (currentType == MeasurementType.Angle ? 3 : 2))
                    {
                        measurementPoints = new List<SKPoint>(measurementPoints.Take(currentType == MeasurementType.Angle ? 3 : 2));
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets whether measurement is active.
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                if (!value)
                {
                    measurementPoints.Clear();
                }
            }
        }

        /// <summary>
        /// Gets the measurement points.
        /// </summary>
        public IReadOnlyList<SKPoint> MeasurementPoints => measurementPoints.AsReadOnly();

        /// <summary>
        /// Gets or sets the measurement line color.
        /// </summary>
        public SKColor LineColor { get; set; } = SKColors.Red;

        /// <summary>
        /// Gets or sets the line width.
        /// </summary>
        public float LineWidth { get; set; } = 2f;

        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        public SKColor TextColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the text font size.
        /// </summary>
        public float TextFontSize { get; set; } = 12f;

        /// <summary>
        /// Gets or sets the coordinate system scale (units per pixel).
        /// </summary>
        public double CoordinateScale { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the unit label (e.g., "m", "ft", "km").
        /// </summary>
        public string UnitLabel { get; set; } = "";

        /// <summary>
        /// Event raised when measurement is completed.
        /// </summary>
        public event EventHandler<MeasurementCompletedEventArgs> MeasurementCompleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementTools"/> class.
        /// </summary>
        public MeasurementTools()
        {
            measurementPoints = new List<SKPoint>();
            currentType = MeasurementType.Distance;
        }

        /// <summary>
        /// Starts a new measurement at the specified point.
        /// </summary>
        public void StartMeasurement(float x, float y)
        {
            isActive = true;
            measurementPoints.Clear();
            measurementPoints.Add(new SKPoint(x, y));
        }

        /// <summary>
        /// Adds a point to the measurement.
        /// </summary>
        public void AddPoint(float x, float y)
        {
            if (!isActive)
                return;

            int maxPoints = currentType switch
            {
                MeasurementType.Distance => 2,
                MeasurementType.Angle => 3,
                MeasurementType.Area => int.MaxValue,
                _ => 2
            };

            if (measurementPoints.Count < maxPoints)
            {
                measurementPoints.Add(new SKPoint(x, y));
            }
            else if (currentType == MeasurementType.Area)
            {
                measurementPoints.Add(new SKPoint(x, y));
            }
        }

        /// <summary>
        /// Updates the last point in the measurement.
        /// </summary>
        public void UpdateLastPoint(float x, float y)
        {
            if (!isActive || measurementPoints.Count == 0)
                return;

            measurementPoints[measurementPoints.Count - 1] = new SKPoint(x, y);
        }

        /// <summary>
        /// Completes the measurement.
        /// </summary>
        public void CompleteMeasurement()
        {
            if (!isActive)
                return;

            double result = CalculateMeasurement();
            isActive = false;

            MeasurementCompleted?.Invoke(this, new MeasurementCompletedEventArgs
            {
                MeasurementType = currentType,
                Result = result,
                Points = measurementPoints.ToList()
            });
        }

        /// <summary>
        /// Cancels the current measurement.
        /// </summary>
        public void CancelMeasurement()
        {
            isActive = false;
            measurementPoints.Clear();
        }

        /// <summary>
        /// Calculates the current measurement value.
        /// </summary>
        public double CalculateMeasurement()
        {
            if (measurementPoints.Count < 2)
                return 0;

            return currentType switch
            {
                MeasurementType.Distance => CalculateDistance(),
                MeasurementType.Area => CalculateArea(),
                MeasurementType.Angle => CalculateAngle(),
                _ => 0
            };
        }

        private double CalculateDistance()
        {
            if (measurementPoints.Count < 2)
                return 0;

            var p1 = measurementPoints[0];
            var p2 = measurementPoints[1];
            double dx = (p2.X - p1.X) * CoordinateScale;
            double dy = (p2.Y - p1.Y) * CoordinateScale;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private double CalculateArea()
        {
            if (measurementPoints.Count < 3)
                return 0;

            double area = 0;
            for (int i = 0; i < measurementPoints.Count; i++)
            {
                int j = (i + 1) % measurementPoints.Count;
                area += measurementPoints[i].X * measurementPoints[j].Y;
                area -= measurementPoints[j].X * measurementPoints[i].Y;
            }
            area = Math.Abs(area) / 2.0 * CoordinateScale * CoordinateScale;
            return area;
        }

        private double CalculateAngle()
        {
            if (measurementPoints.Count < 3)
                return 0;

            var p1 = measurementPoints[0];
            var p2 = measurementPoints[1];
            var p3 = measurementPoints[2];

            double angle1 = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            double angle2 = Math.Atan2(p3.Y - p2.Y, p3.X - p2.X);
            double angle = angle2 - angle1;

            // Normalize to [0, 2π]
            if (angle < 0) angle += 2 * Math.PI;

            return angle * 180.0 / Math.PI; // Convert to degrees
        }

        /// <summary>
        /// Renders the measurement on the canvas.
        /// </summary>
        public void Render(SKCanvas canvas)
        {
            if (!isActive || measurementPoints.Count < 2)
                return;

            switch (currentType)
            {
                case MeasurementType.Distance:
                    RenderDistance(canvas);
                    break;

                case MeasurementType.Area:
                    RenderArea(canvas);
                    break;

                case MeasurementType.Angle:
                    RenderAngle(canvas);
                    break;
            }
        }

        private void RenderDistance(SKCanvas canvas)
        {
            if (measurementPoints.Count < 2)
                return;

            var p1 = measurementPoints[0];
            var p2 = measurementPoints[1];

            // Draw line
            using (var paint = new SKPaint
            {
                Color = LineColor,
                StrokeWidth = LineWidth,
                IsAntialias = true
            })
            {
                canvas.DrawLine(p1, p2, paint);
            }

            // Draw endpoints
            using (var paint = new SKPaint
            {
                Color = LineColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                canvas.DrawCircle(p1, 4, paint);
                canvas.DrawCircle(p2, 4, paint);
            }

            // Draw measurement text
            double distance = CalculateDistance();
            float midX = (p1.X + p2.X) / 2;
            float midY = (p1.Y + p2.Y) / 2;
            string text = $"{distance:F2} {UnitLabel}";

            using (var textPaint = new SKPaint
            {
                Color = TextColor,
                TextSize = TextFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            })
            {
                // Draw background
                var bounds = new SKRect();
                textPaint.MeasureText(text, ref bounds);
                bounds.Offset(midX, midY - 15);
                bounds.Inflate(5, 3);

                using (var bgPaint = new SKPaint
                {
                    Color = new SKColor(255, 255, 255, 200),
                    Style = SKPaintStyle.Fill
                })
                {
                    canvas.DrawRect(bounds, bgPaint);
                }

                canvas.DrawText(text, midX, midY - 5, textPaint);
            }
        }

        private void RenderArea(SKCanvas canvas)
        {
            if (measurementPoints.Count < 3)
                return;

            // Draw polygon
            using (var path = new SKPath())
            {
                path.MoveTo(measurementPoints[0]);
                for (int i = 1; i < measurementPoints.Count; i++)
                {
                    path.LineTo(measurementPoints[i]);
                }
                path.Close();

                // Fill
                using (var fillPaint = new SKPaint
                {
                    Color = new SKColor(LineColor.Red, LineColor.Green, LineColor.Blue, 50),
                    Style = SKPaintStyle.Fill
                })
                {
                    canvas.DrawPath(path, fillPaint);
                }

                // Border
                using (var borderPaint = new SKPaint
                {
                    Color = LineColor,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = LineWidth,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, borderPaint);
                }
            }

            // Draw area text
            double area = CalculateArea();
            float centerX = measurementPoints.Average(p => p.X);
            float centerY = measurementPoints.Average(p => p.Y);
            string text = $"{area:F2} {UnitLabel}²";

            using (var textPaint = new SKPaint
            {
                Color = TextColor,
                TextSize = TextFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            })
            {
                var bounds = new SKRect();
                textPaint.MeasureText(text, ref bounds);
                bounds.Offset(centerX, centerY);
                bounds.Inflate(5, 3);

                using (var bgPaint = new SKPaint
                {
                    Color = new SKColor(255, 255, 255, 200),
                    Style = SKPaintStyle.Fill
                })
                {
                    canvas.DrawRect(bounds, bgPaint);
                }

                canvas.DrawText(text, centerX, centerY + textPaint.TextSize / 3, textPaint);
            }
        }

        private void RenderAngle(SKCanvas canvas)
        {
            if (measurementPoints.Count < 3)
                return;

            var p1 = measurementPoints[0];
            var p2 = measurementPoints[1];
            var p3 = measurementPoints[2];

            // Draw lines
            using (var paint = new SKPaint
            {
                Color = LineColor,
                StrokeWidth = LineWidth,
                IsAntialias = true
            })
            {
                canvas.DrawLine(p1, p2, paint);
                canvas.DrawLine(p2, p3, paint);
            }

            // Draw angle arc
            double angle1 = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            double angle2 = Math.Atan2(p3.Y - p2.Y, p3.X - p2.X);
            float radius = 30;
            float arcStart = (float)(angle1 * 180 / Math.PI);
            float sweepAngle = (float)((angle2 - angle1) * 180 / Math.PI);

            using (var path = new SKPath())
            {
                path.AddArc(new SKRect(p2.X - radius, p2.Y - radius, p2.X + radius, p2.Y + radius),
                    arcStart, sweepAngle);
                using (var paint = new SKPaint
                {
                    Color = LineColor,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = LineWidth,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }

            // Draw angle text
            double angle = CalculateAngle();
            string text = $"{angle:F1}°";

            using (var textPaint = new SKPaint
            {
                Color = TextColor,
                TextSize = TextFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            })
            {
                float textX = p2.X + (float)(radius * 1.5 * Math.Cos((angle1 + angle2) / 2));
                float textY = p2.Y + (float)(radius * 1.5 * Math.Sin((angle1 + angle2) / 2));

                var bounds = new SKRect();
                textPaint.MeasureText(text, ref bounds);
                bounds.Offset(textX, textY);
                bounds.Inflate(5, 3);

                using (var bgPaint = new SKPaint
                {
                    Color = new SKColor(255, 255, 255, 200),
                    Style = SKPaintStyle.Fill
                })
                {
                    canvas.DrawRect(bounds, bgPaint);
                }

                canvas.DrawText(text, textX, textY + textPaint.TextSize / 3, textPaint);
            }
        }
    }

    /// <summary>
    /// Event arguments for measurement completion.
    /// </summary>
    public class MeasurementCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the measurement type.
        /// </summary>
        public MeasurementType MeasurementType { get; set; }

        /// <summary>
        /// Gets or sets the measurement result.
        /// </summary>
        public double Result { get; set; }

        /// <summary>
        /// Gets or sets the measurement points.
        /// </summary>
        public List<SKPoint> Points { get; set; }
    }
}

