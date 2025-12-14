using SkiaSharp;

namespace Beep.OilandGas.HeatMap.ColorSchemes
{
    /// <summary>
    /// Enumeration of predefined color schemes for heat maps.
    /// </summary>
    public enum ColorSchemeType
    {
        /// <summary>
        /// Viridis - perceptually uniform, colorblind-friendly
        /// </summary>
        Viridis,

        /// <summary>
        /// Plasma - perceptually uniform, colorblind-friendly
        /// </summary>
        Plasma,

        /// <summary>
        /// Inferno - perceptually uniform, colorblind-friendly
        /// </summary>
        Inferno,

        /// <summary>
        /// Magma - perceptually uniform, colorblind-friendly
        /// </summary>
        Magma,

        /// <summary>
        /// Cividis - colorblind-friendly, optimized for color vision deficiency
        /// </summary>
        Cividis,

        /// <summary>
        /// Red to Blue - traditional heat map
        /// </summary>
        RedToBlue,

        /// <summary>
        /// Red to Green - traditional heat map
        /// </summary>
        RedToGreen,

        /// <summary>
        /// Blue to Red - traditional heat map
        /// </summary>
        BlueToRed,

        /// <summary>
        /// Grayscale - monochrome
        /// </summary>
        Grayscale,

        /// <summary>
        /// Rainbow - full spectrum
        /// </summary>
        Rainbow,

        /// <summary>
        /// Custom - user-defined color scheme
        /// </summary>
        Custom
    }

    /// <summary>
    /// Provides predefined color schemes for heat map visualization.
    /// </summary>
    public static class ColorScheme
    {
        /// <summary>
        /// Gets a color scheme by type.
        /// </summary>
        /// <param name="schemeType">The type of color scheme.</param>
        /// <param name="steps">Number of color steps in the gradient (default: 256).</param>
        /// <returns>Array of SKColor values representing the color scheme.</returns>
        public static SKColor[] GetColorScheme(ColorSchemeType schemeType, int steps = 256)
        {
            return schemeType switch
            {
                ColorSchemeType.Viridis => GenerateViridis(steps),
                ColorSchemeType.Plasma => GeneratePlasma(steps),
                ColorSchemeType.Inferno => GenerateInferno(steps),
                ColorSchemeType.Magma => GenerateMagma(steps),
                ColorSchemeType.Cividis => GenerateCividis(steps),
                ColorSchemeType.RedToBlue => GenerateRedToBlue(steps),
                ColorSchemeType.RedToGreen => GenerateRedToGreen(steps),
                ColorSchemeType.BlueToRed => GenerateBlueToRed(steps),
                ColorSchemeType.Grayscale => GenerateGrayscale(steps),
                ColorSchemeType.Rainbow => GenerateRainbow(steps),
                _ => GenerateGrayscale(steps)
            };
        }

        /// <summary>
        /// Creates a custom color scheme from a list of colors.
        /// </summary>
        /// <param name="colors">Array of colors to interpolate between.</param>
        /// <param name="steps">Number of color steps in the gradient.</param>
        /// <returns>Array of SKColor values representing the custom color scheme.</returns>
        public static SKColor[] CreateCustom(SKColor[] colors, int steps = 256)
        {
            if (colors == null || colors.Length < 2)
                throw new ArgumentException("At least two colors are required for a custom color scheme.");

            var result = new SKColor[steps];
            double segmentSize = (double)(colors.Length - 1) / (steps - 1);

            for (int i = 0; i < steps; i++)
            {
                double position = i * segmentSize;
                int segmentIndex = (int)Math.Floor(position);
                double t = position - segmentIndex;

                if (segmentIndex >= colors.Length - 1)
                {
                    result[i] = colors[colors.Length - 1];
                }
                else
                {
                    result[i] = InterpolateColor(colors[segmentIndex], colors[segmentIndex + 1], t);
                }
            }

            return result;
        }

        /// <summary>
        /// Interpolates between two colors.
        /// </summary>
        private static SKColor InterpolateColor(SKColor color1, SKColor color2, double t)
        {
            t = Math.Max(0, Math.Min(1, t)); // Clamp t to [0, 1]
            byte r = (byte)(color1.Red + (color2.Red - color1.Red) * t);
            byte g = (byte)(color1.Green + (color2.Green - color1.Green) * t);
            byte b = (byte)(color1.Blue + (color2.Blue - color1.Blue) * t);
            return new SKColor(r, g, b);
        }

        #region Predefined Color Schemes

        /// <summary>
        /// Generates Viridis color scheme (perceptually uniform, colorblind-friendly).
        /// </summary>
        private static SKColor[] GenerateViridis(int steps)
        {
            // Viridis color map RGB values (key points)
            var keyColors = new[]
            {
                new SKColor(68, 1, 84),      // Dark purple
                new SKColor(59, 82, 139),    // Blue
                new SKColor(33, 144, 140),  // Teal
                new SKColor(92, 200, 99),    // Green
                new SKColor(253, 231, 37)    // Yellow
            };

            return CreateCustom(keyColors, steps);
        }

        /// <summary>
        /// Generates Plasma color scheme (perceptually uniform, colorblind-friendly).
        /// </summary>
        private static SKColor[] GeneratePlasma(int steps)
        {
            var keyColors = new[]
            {
                new SKColor(13, 8, 135),    // Dark blue
                new SKColor(75, 3, 161),    // Purple
                new SKColor(125, 3, 168),    // Magenta
                new SKColor(168, 42, 135),  // Pink
                new SKColor(203, 71, 120),  // Red-pink
                new SKColor(229, 107, 93),  // Orange-red
                new SKColor(248, 148, 65),  // Orange
                new SKColor(253, 231, 37)   // Yellow
            };

            return CreateCustom(keyColors, steps);
        }

        /// <summary>
        /// Generates Inferno color scheme (perceptually uniform, colorblind-friendly).
        /// </summary>
        private static SKColor[] GenerateInferno(int steps)
        {
            var keyColors = new[]
            {
                new SKColor(0, 0, 4),       // Black
                new SKColor(20, 11, 57),    // Dark purple
                new SKColor(55, 9, 108),    // Purple
                new SKColor(93, 15, 125),   // Magenta
                new SKColor(133, 25, 139),  // Pink
                new SKColor(170, 40, 150),  // Pink-red
                new SKColor(204, 59, 153),  // Red
                new SKColor(237, 100, 137), // Orange-red
                new SKColor(252, 141, 89),  // Orange
                new SKColor(252, 191, 73),  // Yellow-orange
                new SKColor(252, 255, 164)  // Yellow-white
            };

            return CreateCustom(keyColors, steps);
        }

        /// <summary>
        /// Generates Magma color scheme (perceptually uniform, colorblind-friendly).
        /// </summary>
        private static SKColor[] GenerateMagma(int steps)
        {
            var keyColors = new[]
            {
                new SKColor(0, 0, 4),       // Black
                new SKColor(18, 13, 59),    // Dark blue
                new SKColor(51, 10, 104),   // Blue-purple
                new SKColor(88, 10, 135),   // Purple
                new SKColor(125, 19, 152),   // Magenta
                new SKColor(161, 38, 150),  // Pink
                new SKColor(195, 54, 134),  // Red-pink
                new SKColor(222, 71, 119),  // Orange-red
                new SKColor(246, 95, 93),   // Orange
                new SKColor(252, 146, 82),  // Yellow-orange
                new SKColor(252, 197, 102), // Yellow
                new SKColor(252, 253, 191)  // Yellow-white
            };

            return CreateCustom(keyColors, steps);
        }

        /// <summary>
        /// Generates Cividis color scheme (optimized for color vision deficiency).
        /// </summary>
        private static SKColor[] GenerateCividis(int steps)
        {
            var keyColors = new[]
            {
                new SKColor(0, 32, 76),     // Dark blue
                new SKColor(0, 62, 92),    // Blue
                new SKColor(0, 92, 102),   // Cyan-blue
                new SKColor(0, 119, 103),  // Teal
                new SKColor(0, 142, 99),   // Green-teal
                new SKColor(0, 163, 91),   // Green
                new SKColor(0, 183, 79),   // Yellow-green
                new SKColor(0, 202, 63),   // Yellow
                new SKColor(0, 220, 40),   // Bright yellow
                new SKColor(0, 237, 0),    // Yellow-white
                new SKColor(255, 255, 255) // White
            };

            return CreateCustom(keyColors, steps);
        }

        /// <summary>
        /// Generates Red to Blue color scheme.
        /// </summary>
        private static SKColor[] GenerateRedToBlue(int steps)
        {
            var keyColors = new[]
            {
                SKColors.Red,
                SKColors.Blue
            };

            return CreateCustom(keyColors, steps);
        }

        /// <summary>
        /// Generates Red to Green color scheme.
        /// </summary>
        private static SKColor[] GenerateRedToGreen(int steps)
        {
            var keyColors = new[]
            {
                SKColors.Red,
                SKColors.Green
            };

            return CreateCustom(keyColors, steps);
        }

        /// <summary>
        /// Generates Blue to Red color scheme.
        /// </summary>
        private static SKColor[] GenerateBlueToRed(int steps)
        {
            var keyColors = new[]
            {
                SKColors.Blue,
                SKColors.Red
            };

            return CreateCustom(keyColors, steps);
        }

        /// <summary>
        /// Generates Grayscale color scheme.
        /// </summary>
        private static SKColor[] GenerateGrayscale(int steps)
        {
            var result = new SKColor[steps];
            for (int i = 0; i < steps; i++)
            {
                byte intensity = (byte)(255 * i / (steps - 1));
                result[i] = new SKColor(intensity, intensity, intensity);
            }
            return result;
        }

        /// <summary>
        /// Generates Rainbow color scheme.
        /// </summary>
        private static SKColor[] GenerateRainbow(int steps)
        {
            var keyColors = new[]
            {
                new SKColor(128, 0, 128),   // Purple
                new SKColor(0, 0, 255),     // Blue
                new SKColor(0, 255, 255),   // Cyan
                new SKColor(0, 255, 0),     // Green
                new SKColor(255, 255, 0),   // Yellow
                new SKColor(255, 128, 0),   // Orange
                new SKColor(255, 0, 0)      // Red
            };

            return CreateCustom(keyColors, steps);
        }

        #endregion
    }
}

