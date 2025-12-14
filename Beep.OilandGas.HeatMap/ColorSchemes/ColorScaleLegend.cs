using SkiaSharp;

namespace Beep.OilandGas.HeatMap.ColorSchemes
{
    /// <summary>
    /// Represents a color scale legend for heat maps.
    /// </summary>
    public class ColorScaleLegend
    {
        /// <summary>
        /// Gets or sets the color scheme used for the legend.
        /// </summary>
        public SKColor[] ColorScheme { get; set; }

        /// <summary>
        /// Gets or sets the minimum value displayed in the legend.
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value displayed in the legend.
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the width of the legend bar in pixels.
        /// </summary>
        public float Width { get; set; } = 30f;

        /// <summary>
        /// Gets or sets the height of the legend bar in pixels.
        /// </summary>
        public float Height { get; set; } = 200f;

        /// <summary>
        /// Gets or sets the position of the legend (X coordinate).
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the position of the legend (Y coordinate).
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Gets or sets whether to show tick marks.
        /// </summary>
        public bool ShowTicks { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of tick marks to display.
        /// </summary>
        public int TickCount { get; set; } = 5;

        /// <summary>
        /// Gets or sets the font size for labels.
        /// </summary>
        public float FontSize { get; set; } = 12f;

        /// <summary>
        /// Gets or sets the label format string (e.g., "F2" for 2 decimal places).
        /// </summary>
        public string LabelFormat { get; set; } = "F2";

        /// <summary>
        /// Gets or sets the title of the legend.
        /// </summary>
        public string Title { get; set; } = "Value";

        /// <summary>
        /// Gets or sets whether the legend is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorScaleLegend"/> class.
        /// </summary>
        public ColorScaleLegend()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorScaleLegend"/> class.
        /// </summary>
        /// <param name="colorScheme">The color scheme to display.</param>
        /// <param name="minValue">Minimum value.</param>
        /// <param name="maxValue">Maximum value.</param>
        public ColorScaleLegend(SKColor[] colorScheme, double minValue, double maxValue)
        {
            ColorScheme = colorScheme ?? throw new ArgumentNullException(nameof(colorScheme));
            MinValue = minValue;
            MaxValue = maxValue;
        }

        /// <summary>
        /// Draws the color scale legend on the canvas.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        public void Draw(SKCanvas canvas)
        {
            if (!IsVisible || ColorScheme == null || ColorScheme.Length == 0)
                return;

            // Draw the color gradient bar
            DrawColorBar(canvas);

            // Draw tick marks and labels
            if (ShowTicks)
            {
                DrawTicks(canvas);
            }

            // Draw title
            if (!string.IsNullOrEmpty(Title))
            {
                DrawTitle(canvas);
            }
        }

        /// <summary>
        /// Draws the color gradient bar.
        /// </summary>
        private void DrawColorBar(SKCanvas canvas)
        {
            int colorCount = ColorScheme.Length;
            float segmentHeight = Height / colorCount;

            for (int i = 0; i < colorCount; i++)
            {
                var paint = new SKPaint
                {
                    Color = ColorScheme[i],
                    IsAntialias = true
                };

                float y = Y + Height - (i + 1) * segmentHeight;
                canvas.DrawRect(X, y, Width, segmentHeight + 1, paint); // +1 to avoid gaps
            }
        }

        /// <summary>
        /// Draws tick marks and labels.
        /// </summary>
        private void DrawTicks(SKCanvas canvas)
        {
            var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                TextSize = FontSize,
                TextAlign = SKTextAlign.Left
            };

            float tickLength = 5f;
            float labelSpacing = 5f;

            for (int i = 0; i <= TickCount; i++)
            {
                double value = MinValue + (MaxValue - MinValue) * i / TickCount;
                float y = Y + Height - (Height * i / TickCount);

                // Draw tick mark
                canvas.DrawLine(X + Width, y, X + Width + tickLength, y, textPaint);

                // Draw label
                string label = value.ToString(LabelFormat);
                float textWidth = textPaint.MeasureText(label);
                canvas.DrawText(label, X + Width + tickLength + labelSpacing, y + FontSize / 3, textPaint);
            }
        }

        /// <summary>
        /// Draws the legend title.
        /// </summary>
        private void DrawTitle(SKCanvas canvas)
        {
            var titlePaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                TextSize = FontSize + 2,
                TextAlign = SKTextAlign.Center,
                FakeBoldText = true
            };

            float titleX = X + Width / 2;
            float titleY = Y - FontSize - 5;
            canvas.DrawText(Title, titleX, titleY, titlePaint);
        }

        /// <summary>
        /// Gets the color corresponding to a value based on the legend's scale.
        /// </summary>
        /// <param name="value">The value to get the color for.</param>
        /// <returns>The color corresponding to the value.</returns>
        public SKColor GetColorForValue(double value)
        {
            if (ColorScheme == null || ColorScheme.Length == 0)
                return SKColors.Gray;

            // Normalize value to [0, 1]
            double normalized = (value - MinValue) / (MaxValue - MinValue);
            normalized = Math.Max(0, Math.Min(1, normalized)); // Clamp to [0, 1]

            // Get color index
            int index = (int)(normalized * (ColorScheme.Length - 1));
            return ColorScheme[index];
        }
    }
}

