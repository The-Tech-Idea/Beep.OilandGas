using System;

namespace Beep.OilandGas.HeatMap.ColorSchemes
{
    /// <summary>
    /// Types of color mapping functions.
    /// </summary>
    public enum ColorMappingFunction
    {
        /// <summary>
        /// Linear mapping (default).
        /// </summary>
        Linear,

        /// <summary>
        /// Logarithmic mapping.
        /// </summary>
        Logarithmic,

        /// <summary>
        /// Square root mapping.
        /// </summary>
        SquareRoot,

        /// <summary>
        /// Power mapping (custom exponent).
        /// </summary>
        Power,

        /// <summary>
        /// Symmetric logarithmic mapping.
        /// </summary>
        SymmetricLog,

        /// <summary>
        /// Custom function mapping.
        /// </summary>
        Custom
    }

    /// <summary>
    /// Provides color mapping functions for value normalization.
    /// </summary>
    public static class ColorMapping
    {
        /// <summary>
        /// Applies a mapping function to normalize a value.
        /// </summary>
        /// <param name="value">The value to map (should be in [0, 1] range).</param>
        /// <param name="function">The mapping function to apply.</param>
        /// <param name="parameter">Optional parameter for the function (e.g., power exponent, log base).</param>
        /// <returns>Mapped value in [0, 1] range.</returns>
        public static double ApplyMapping(double value, ColorMappingFunction function, double parameter = 2.0)
        {
            if (value < 0) value = 0;
            if (value > 1) value = 1;

            return function switch
            {
                ColorMappingFunction.Linear => value,
                ColorMappingFunction.Logarithmic => ApplyLogarithmic(value, parameter),
                ColorMappingFunction.SquareRoot => Math.Sqrt(value),
                ColorMappingFunction.Power => Math.Pow(value, parameter),
                ColorMappingFunction.SymmetricLog => ApplySymmetricLog(value, parameter),
                ColorMappingFunction.Custom => value, // Custom function should be applied externally
                _ => value
            };
        }

        /// <summary>
        /// Applies logarithmic mapping.
        /// </summary>
        private static double ApplyLogarithmic(double value, double logBase)
        {
            if (value <= 0)
                return 0;

            if (logBase <= 1)
                logBase = 10.0;

            // Map [0, 1] to [1, logBase] then take log
            double mappedValue = 1 + value * (logBase - 1);
            double logValue = Math.Log(mappedValue, logBase);
            
            // Normalize to [0, 1]
            return logValue / Math.Log(logBase, logBase);
        }

        /// <summary>
        /// Applies symmetric logarithmic mapping.
        /// </summary>
        private static double ApplySymmetricLog(double value, double linthresh)
        {
            // Symmetric log: sign(x) * log(1 + |x| / linthresh) / log(1 + 1 / linthresh)
            // For [0, 1] range, we use positive values
            if (value <= 0)
                return 0;

            if (linthresh <= 0)
                linthresh = 0.1;

            double sign = value >= 0.5 ? 1 : -1;
            double absValue = Math.Abs(value - 0.5) * 2; // Map to [0, 1] centered at 0.5
            
            double logValue = sign * Math.Log(1 + absValue / linthresh) / Math.Log(1 + 1 / linthresh);
            
            // Map back to [0, 1]
            return (logValue + 1) / 2;
        }

        /// <summary>
        /// Applies a custom mapping function.
        /// </summary>
        /// <param name="value">The value to map (in [0, 1] range).</param>
        /// <param name="customFunction">The custom function to apply.</param>
        /// <returns>Mapped value in [0, 1] range.</returns>
        public static double ApplyCustomMapping(double value, Func<double, double> customFunction)
        {
            if (value < 0) value = 0;
            if (value > 1) value = 1;

            double mapped = customFunction(value);
            
            // Clamp to [0, 1]
            if (mapped < 0) mapped = 0;
            if (mapped > 1) mapped = 1;
            
            return mapped;
        }

        /// <summary>
        /// Maps a raw value to normalized [0, 1] range using min/max bounds.
        /// </summary>
        /// <param name="value">The raw value.</param>
        /// <param name="minValue">Minimum value in the dataset.</param>
        /// <param name="maxValue">Maximum value in the dataset.</param>
        /// <param name="function">The mapping function to apply after normalization.</param>
        /// <param name="parameter">Optional parameter for the function.</param>
        /// <returns>Mapped value in [0, 1] range.</returns>
        public static double MapValue(double value, double minValue, double maxValue,
            ColorMappingFunction function = ColorMappingFunction.Linear, double parameter = 2.0)
        {
            // First normalize to [0, 1]
            double normalized = 0;
            if (maxValue > minValue)
            {
                normalized = (value - minValue) / (maxValue - minValue);
            }

            // Apply mapping function
            return ApplyMapping(normalized, function, parameter);
        }
    }
}

