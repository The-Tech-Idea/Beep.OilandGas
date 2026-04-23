using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beep.WellSchematics
{
    public class LogManager
    {
 
        private void DrawLog(SKCanvas canvas, int width, int height,int padding)
        {
            // Set the position and size of the log
            int logWidth = 150; // Adjust the width of the log as needed
            int logHeight = height - 2 * padding; // The height of the log will match the height of the wellborehole visualization
            int logX = width - logWidth - padding; // Position the log to the right of the wellborehole visualization
            int logY = padding; // Position the log at the top of the wellborehole visualization
           
            // Draw the log background
            SKPaint logBackgroundPaint = new SKPaint
            {
                Color = SKColors.WhiteSmoke,
                Style = SKPaintStyle.Fill
            };
            SKRect logRect = new SKRect(logX, logY, logX + logWidth, logY + logHeight);
            canvas.DrawRect(logRect, logBackgroundPaint);
            // Draw the log title
            SKPaint titlePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 16,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText("Log Name", logX + logWidth / 2, logY + 30, titlePaint);

            // Draw the horizontal scale width above the log title
            int horizontalScaleWidth = 100; // Modify the width as needed
            SKPaint scaleWidthPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 12,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText(horizontalScaleWidth.ToString(), logX + logWidth / 2, logY + 60, scaleWidthPaint);
            // Draw the log content (sample data for illustration)
            SKPaint logLinePaint = new SKPaint
            {
                Color = SKColors.DarkBlue,
                StrokeWidth = 2,
                IsAntialias = true
            };

            SKPoint[] logPoints = new SKPoint[]
            {
        new SKPoint(logX + 20, logY + logHeight / 4),
        new SKPoint(logX + 40, logY + logHeight / 2),
        new SKPoint(logX + 60, logY + logHeight / 3),
        new SKPoint(logX + 80, logY + logHeight * 3 / 4),
        new SKPoint(logX + 100, logY + logHeight / 2),
        new SKPoint(logX + 120, logY + logHeight / 4),
            };

            // Draw lines connecting the log points
            for (int i = 0; i < logPoints.Length - 1; i++)
            {
                canvas.DrawLine(logPoints[i], logPoints[i + 1], logLinePaint);
            }

            // Draw data points on the log
            SKPaint dataPointPaint = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true
            };
            float dataPointRadius = 4;
            foreach (var point in logPoints)
            {
                canvas.DrawCircle(point, dataPointRadius, dataPointPaint);
            }

            // Draw log labels and text
            SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 12,
                IsAntialias = true
            };
            canvas.DrawText("Log Values", logX + 10, logY + 20, textPaint);

            // ... Add more labels or text as needed ...

            // Draw a border around the log
            SKPaint logBorderPaint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };
            canvas.DrawRect(logRect, logBorderPaint);
        }

    }
}
