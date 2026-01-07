using System;

using Beep.OilandGas.SuckerRodPumping.Constants;
using Beep.OilandGas.SuckerRodPumping.Exceptions;
using Beep.OilandGas.Models.SuckerRodPumping;

namespace Beep.OilandGas.SuckerRodPumping.Validation
{
    /// <summary>
    /// Provides validation for sucker rod pumping calculations.
    /// </summary>
    public static class SuckerRodValidator
    {
        /// <summary>
        /// Validates sucker rod system properties.
        /// </summary>
        public static void ValidateSystemProperties(SuckerRodSystemProperties systemProperties)
        {
            if (systemProperties == null)
                throw new ArgumentNullException(nameof(systemProperties));

            if (systemProperties.WellDepth <= 0)
                throw new InvalidSystemPropertiesException("Well depth must be greater than zero.");

            if (systemProperties.TubingDiameter <= 0)
                throw new InvalidSystemPropertiesException("Tubing diameter must be greater than zero.");

            if (systemProperties.RodDiameter <= 0)
                throw new InvalidSystemPropertiesException("Rod diameter must be greater than zero.");

            if (systemProperties.PumpDiameter <= 0)
                throw new InvalidSystemPropertiesException("Pump diameter must be greater than zero.");

            if (systemProperties.PumpDiameter >= systemProperties.TubingDiameter)
                throw new InvalidSystemPropertiesException("Pump diameter must be less than tubing diameter.");

            if (systemProperties.RodDiameter >= systemProperties.TubingDiameter)
                throw new InvalidSystemPropertiesException("Rod diameter must be less than tubing diameter.");

            if (systemProperties.StrokeLength <= 0)
                throw new InvalidSystemPropertiesException("Stroke length must be greater than zero.");

            if (systemProperties.StrokesPerMinute <= 0)
                throw new InvalidSystemPropertiesException("Strokes per minute must be greater than zero.");

            if (systemProperties.WellheadPressure < 0)
                throw new InvalidSystemPropertiesException("Wellhead pressure cannot be negative.");

            if (systemProperties.BottomHolePressure <= systemProperties.WellheadPressure)
                throw new InvalidSystemPropertiesException("Bottom hole pressure must be greater than wellhead pressure.");

            if (systemProperties.WaterCut < 0 || systemProperties.WaterCut > 1)
                throw new InvalidSystemPropertiesException("Water cut must be between 0 and 1.");

            if (systemProperties.PumpEfficiency <= 0 || systemProperties.PumpEfficiency > 1)
                throw new InvalidSystemPropertiesException("Pump efficiency must be between 0 and 1.");
        }

        /// <summary>
        /// Validates rod string configuration.
        /// </summary>
        public static void ValidateRodString(SuckerRodString rodString)
        {
            if (rodString == null)
                throw new ArgumentNullException(nameof(rodString));

            if (rodString.Sections == null || rodString.Sections.Count == 0)
                throw new InvalidRodStringException("Rod string must have at least one section.");

            foreach (var section in rodString.Sections)
            {
                if (section.Diameter <= 0)
                    throw new InvalidRodStringException($"Rod section diameter must be greater than zero.");

                if (section.Length <= 0)
                    throw new InvalidRodStringException($"Rod section length must be greater than zero.");

                if (section.Density <= 0)
                    throw new InvalidRodStringException($"Rod section density must be greater than zero.");
            }

            // Calculate total length
            decimal totalLength = 0m;
            foreach (var section in rodString.Sections)
            {
                totalLength += section.Length;
            }
            rodString.TotalLength = totalLength;
        }

        /// <summary>
        /// Validates rod stress against safe limits.
        /// </summary>
        public static void ValidateRodStress(
            decimal calculatedStress,
            decimal maximumAllowableStress,
            decimal safetyFactor = SuckerRodConstants.MinimumSafetyFactor)
        {
            decimal safeStress = maximumAllowableStress / safetyFactor;

            if (calculatedStress > safeStress)
            {
                throw new RodStressExceededException(calculatedStress, safeStress);
            }
        }

        /// <summary>
        /// Validates all calculation parameters.
        /// </summary>
        public static void ValidateCalculationParameters(
            SuckerRodSystemProperties systemProperties,
            SuckerRodString rodString)
        {
            ValidateSystemProperties(systemProperties);
            ValidateRodString(rodString);

            // Validate that rod string length matches well depth (within tolerance)
            decimal tolerance = 100m; // feet
            if (Math.Abs(rodString.TotalLength - systemProperties.WellDepth) > tolerance)
            {
                throw new InvalidRodStringException(
                    $"Rod string length ({rodString.TotalLength:F0} ft) does not match well depth ({systemProperties.WellDepth:F0} ft) within tolerance ({tolerance:F0} ft).");
            }
        }
    }
}

