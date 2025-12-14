using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Beep.OilandGas.HeatMap.Annotations
{
    /// <summary>
    /// Represents a text annotation on the heat map.
    /// </summary>
    public class TextAnnotation
    {
        /// <summary>
        /// Gets or sets the annotation text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the X coordinate (in data space).
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate (in data space).
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        public float FontSize { get; set; } = 12f;

        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        public SKColor Color { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the text alignment.
        /// </summary>
        public SKTextAlign TextAlign { get; set; } = SKTextAlign.Center;

        /// <summary>
        /// Gets or sets whether to show a background box.
        /// </summary>
        public bool ShowBackground { get; set; } = false;

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public SKColor BackgroundColor { get; set; } = SKColors.White;

        /// <summary>
        /// Gets or sets the background padding.
        /// </summary>
        public float BackgroundPadding { get; set; } = 4f;

        /// <summary>
        /// Gets or sets the rotation angle in degrees.
        /// </summary>
        public float Rotation { get; set; } = 0f;

        /// <summary>
        /// Gets or sets whether the annotation is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }

    /// <summary>
    /// Represents a callout annotation with an arrow pointing to a location.
    /// </summary>
    public class CalloutAnnotation : TextAnnotation
    {
        /// <summary>
        /// Gets or sets the target X coordinate (where arrow points).
        /// </summary>
        public double TargetX { get; set; }

        /// <summary>
        /// Gets or sets the target Y coordinate (where arrow points).
        /// </summary>
        public double TargetY { get; set; }

        /// <summary>
        /// Gets or sets the arrow color.
        /// </summary>
        public SKColor ArrowColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the arrow line width.
        /// </summary>
        public float ArrowLineWidth { get; set; } = 2f;

        /// <summary>
        /// Gets or sets the arrow head size.
        /// </summary>
        public float ArrowHeadSize { get; set; } = 8f;
    }

    /// <summary>
    /// Represents a value annotation showing a numeric value at a point.
    /// </summary>
    public class ValueAnnotation
    {
        /// <summary>
        /// Gets or sets the X coordinate (in data space).
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate (in data space).
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the value to display.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the format string (e.g., "F2" for 2 decimal places).
        /// </summary>
        public string Format { get; set; } = "F2";

        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        public float FontSize { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        public SKColor Color { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets whether to show a background box.
        /// </summary>
        public bool ShowBackground { get; set; } = true;

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public SKColor BackgroundColor { get; set; } = SKColors.White;

        /// <summary>
        /// Gets or sets the offset from the point (in pixels).
        /// </summary>
        public SKPoint Offset { get; set; } = new SKPoint(0, -15);

        /// <summary>
        /// Gets or sets whether the annotation is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }

    /// <summary>
    /// Manages annotations for heat maps.
    /// </summary>
    public class HeatMapAnnotations
    {
        private readonly List<TextAnnotation> textAnnotations = new List<TextAnnotation>();
        private readonly List<CalloutAnnotation> calloutAnnotations = new List<CalloutAnnotation>();
        private readonly List<ValueAnnotation> valueAnnotations = new List<ValueAnnotation>();

        /// <summary>
        /// Gets or sets the map title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the map subtitle.
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets the title font size.
        /// </summary>
        public float TitleFontSize { get; set; } = 18f;

        /// <summary>
        /// Gets or sets the subtitle font size.
        /// </summary>
        public float SubtitleFontSize { get; set; } = 14f;

        /// <summary>
        /// Gets or sets the title color.
        /// </summary>
        public SKColor TitleColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets whether to show title.
        /// </summary>
        public bool ShowTitle { get; set; } = true;

        /// <summary>
        /// Gets or sets data source information.
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// Gets or sets copyright information.
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// Gets or sets timestamp information.
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Adds a text annotation.
        /// </summary>
        public void AddTextAnnotation(TextAnnotation annotation)
        {
            if (annotation != null)
                textAnnotations.Add(annotation);
        }

        /// <summary>
        /// Adds a callout annotation.
        /// </summary>
        public void AddCalloutAnnotation(CalloutAnnotation annotation)
        {
            if (annotation != null)
                calloutAnnotations.Add(annotation);
        }

        /// <summary>
        /// Adds a value annotation.
        /// </summary>
        public void AddValueAnnotation(ValueAnnotation annotation)
        {
            if (annotation != null)
                valueAnnotations.Add(annotation);
        }

        /// <summary>
        /// Removes all annotations.
        /// </summary>
        public void Clear()
        {
            textAnnotations.Clear();
            calloutAnnotations.Clear();
            valueAnnotations.Clear();
        }

        /// <summary>
        /// Renders all annotations on the canvas.
        /// </summary>
        public void Render(SKCanvas canvas, float width, float height, 
            double minX, double maxX, double minY, double maxY)
        {
            // Calculate coordinate transformation
            float scaleX = width / (float)(maxX - minX);
            float scaleY = height / (float)(maxY - minY);
            float offsetX = (float)-minX * scaleX;
            float offsetY = (float)-minY * scaleY;

            // Render title and subtitle
            if (ShowTitle)
            {
                RenderTitle(canvas, width);
            }

            // Render text annotations
            foreach (var annotation in textAnnotations.Where(a => a.IsVisible))
            {
                RenderTextAnnotation(canvas, annotation, scaleX, scaleY, offsetX, offsetY);
            }

            // Render callout annotations
            foreach (var callout in calloutAnnotations.Where(a => a.IsVisible))
            {
                RenderCalloutAnnotation(canvas, callout, scaleX, scaleY, offsetX, offsetY);
            }

            // Render value annotations
            foreach (var value in valueAnnotations.Where(a => a.IsVisible))
            {
                RenderValueAnnotation(canvas, value, scaleX, scaleY, offsetX, offsetY);
            }

            // Render metadata (data source, copyright, timestamp)
            RenderMetadata(canvas, width, height);
        }

        private void RenderTitle(SKCanvas canvas, float width)
        {
            if (string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Subtitle))
                return;

            canvas.Save();

            var titlePaint = new SKPaint
            {
                Color = TitleColor,
                IsAntialias = true,
                TextSize = TitleFontSize,
                TextAlign = SKTextAlign.Center,
                FakeBoldText = true
            };

            float y = 20;

            if (!string.IsNullOrEmpty(Title))
            {
                canvas.DrawText(Title, width / 2, y, titlePaint);
                y += TitleFontSize + 5;
            }

            if (!string.IsNullOrEmpty(Subtitle))
            {
                titlePaint.TextSize = SubtitleFontSize;
                titlePaint.FakeBoldText = false;
                canvas.DrawText(Subtitle, width / 2, y, titlePaint);
            }

            canvas.Restore();
        }

        private void RenderTextAnnotation(SKCanvas canvas, TextAnnotation annotation, 
            float scaleX, float scaleY, float offsetX, float offsetY)
        {
            float x = (float)(annotation.X * scaleX + offsetX);
            float y = (float)(annotation.Y * scaleY + offsetY);

            canvas.Save();
            canvas.Translate(x, y);
            if (annotation.Rotation != 0)
            {
                canvas.RotateDegrees(annotation.Rotation);
            }

            var textPaint = new SKPaint
            {
                Color = annotation.Color,
                TextSize = annotation.FontSize,
                TextAlign = annotation.TextAlign,
                IsAntialias = true
            };

            // Draw background if enabled
            if (annotation.ShowBackground)
            {
                var textBounds = new SKRect();
                textPaint.MeasureText(annotation.Text, ref textBounds);
                textBounds.Left -= annotation.BackgroundPadding;
                textBounds.Right += annotation.BackgroundPadding;
                textBounds.Top -= annotation.BackgroundPadding;
                textBounds.Bottom += annotation.BackgroundPadding;

                var bgPaint = new SKPaint
                {
                    Color = annotation.BackgroundColor,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                canvas.DrawRect(textBounds, bgPaint);
            }

            canvas.DrawText(annotation.Text, 0, 0, textPaint);
            canvas.Restore();
        }

        private void RenderCalloutAnnotation(SKCanvas canvas, CalloutAnnotation callout,
            float scaleX, float scaleY, float offsetX, float offsetY)
        {
            float textX = (float)(callout.X * scaleX + offsetX);
            float textY = (float)(callout.Y * scaleY + offsetY);
            float targetX = (float)(callout.TargetX * scaleX + offsetX);
            float targetY = (float)(callout.TargetY * scaleY + offsetY);

            // Draw arrow line
            var arrowPaint = new SKPaint
            {
                Color = callout.ArrowColor,
                StrokeWidth = callout.ArrowLineWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            canvas.DrawLine(textX, textY, targetX, targetY, arrowPaint);

            // Draw arrow head
            float angle = (float)Math.Atan2(targetY - textY, targetX - textX);
            float arrowX1 = targetX - callout.ArrowHeadSize * (float)Math.Cos(angle - Math.PI / 6);
            float arrowY1 = targetY - callout.ArrowHeadSize * (float)Math.Sin(angle - Math.PI / 6);
            float arrowX2 = targetX - callout.ArrowHeadSize * (float)Math.Cos(angle + Math.PI / 6);
            float arrowY2 = targetY - callout.ArrowHeadSize * (float)Math.Sin(angle + Math.PI / 6);

            var arrowPath = new SKPath();
            arrowPath.MoveTo(targetX, targetY);
            arrowPath.LineTo(arrowX1, arrowY1);
            arrowPath.LineTo(arrowX2, arrowY2);
            arrowPath.Close();

            arrowPaint.Style = SKPaintStyle.Fill;
            canvas.DrawPath(arrowPath, arrowPaint);

            // Draw text annotation
            RenderTextAnnotation(canvas, callout, scaleX, scaleY, offsetX, offsetY);
        }

        private void RenderValueAnnotation(SKCanvas canvas, ValueAnnotation annotation,
            float scaleX, float scaleY, float offsetX, float offsetY)
        {
            float x = (float)(annotation.X * scaleX + offsetX) + annotation.Offset.X;
            float y = (float)(annotation.Y * scaleY + offsetY) + annotation.Offset.Y;

            string text = annotation.Value.ToString(annotation.Format);

            var textPaint = new SKPaint
            {
                Color = annotation.Color,
                TextSize = annotation.FontSize,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true
            };

            // Draw background if enabled
            if (annotation.ShowBackground)
            {
                var textBounds = new SKRect();
                textPaint.MeasureText(text, ref textBounds);
                textBounds.Offset(x, y);
                textBounds.Left -= 4;
                textBounds.Right += 4;
                textBounds.Top -= 2;
                textBounds.Bottom += 2;

                var bgPaint = new SKPaint
                {
                    Color = annotation.BackgroundColor,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                canvas.DrawRect(textBounds, bgPaint);
            }

            canvas.DrawText(text, x, y, textPaint);
        }

        private void RenderMetadata(SKCanvas canvas, float width, float height)
        {
            var metadataPaint = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = 9f,
                TextAlign = SKTextAlign.Left,
                IsAntialias = true
            };

            float y = height - 10;
            float x = 10;

            // Data source
            if (!string.IsNullOrEmpty(DataSource))
            {
                canvas.DrawText($"Data: {DataSource}", x, y, metadataPaint);
                y -= 12;
            }

            // Copyright
            if (!string.IsNullOrEmpty(Copyright))
            {
                canvas.DrawText($"© {Copyright}", x, y, metadataPaint);
                y -= 12;
            }

            // Timestamp
            if (Timestamp.HasValue)
            {
                canvas.DrawText($"Date: {Timestamp.Value:yyyy-MM-dd HH:mm}", x, y, metadataPaint);
            }
        }
    }
}

