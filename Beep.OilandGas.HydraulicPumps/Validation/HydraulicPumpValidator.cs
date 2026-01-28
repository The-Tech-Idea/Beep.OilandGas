using System;

using Beep.OilandGas.HydraulicPumps.Exceptions;

namespace Beep.OilandGas.HydraulicPumps.Validation
{
    /// <summary>
    /// Provides validation for hydraulic pump calculations.
    /// </summary>
    public static class HydraulicPumpValidator
    {
        /// <summary>
        /// Validates well properties.
        /// </summary>
        public static void ValidateWellProperties(HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (wellProperties.WELL_DEPTH <= 0)
                throw new InvalidWellPropertiesException("Well depth must be greater than zero.");

            if (wellProperties.TUBING_DIAMETER <= 0)
                throw new InvalidWellPropertiesException("Tubing diameter must be greater than zero.");

            if (wellProperties.WELLHEAD_PRESSURE < 0)
                throw new InvalidWellPropertiesException("Wellhead pressure cannot be negative.");

            if (wellProperties.BOTTOM_HOLE_PRESSURE <= wellProperties.WELLHEAD_PRESSURE)
                throw new InvalidWellPropertiesException("Bottom hole pressure must be greater than wellhead pressure.");

            if (wellProperties.WATER_CUT < 0 || wellProperties.WATER_CUT > 1)
                throw new InvalidWellPropertiesException("Water cut must be between 0 and 1.");

            if (wellProperties.GAS_SPECIFIC_GRAVITY <= 0)
                throw new InvalidWellPropertiesException("Gas specific gravity must be greater than zero.");

            if (wellProperties.DESIRED_PRODUCTION_RATE <= 0)
                throw new InvalidWellPropertiesException("Desired production rate must be greater than zero.");
        }

        /// <summary>
        /// Validates jet pump properties.
        /// </summary>
        public static void ValidateJetPumpProperties(HYDRAULIC_JET_PUMP_PROPERTIES pumpProperties)
        {
            if (pumpProperties == null)
                throw new ArgumentNullException(nameof(pumpProperties));

            if (pumpProperties.NOZZLE_DIAMETER <= 0)
                throw new InvalidPumpPropertiesException("Nozzle diameter must be greater than zero.");

            if (pumpProperties.THROAT_DIAMETER <= 0)
                throw new InvalidPumpPropertiesException("Throat diameter must be greater than zero.");

            if (pumpProperties.NOZZLE_DIAMETER >= pumpProperties.THROAT_DIAMETER)
                throw new InvalidPumpPropertiesException("Nozzle diameter must be less than throat diameter.");

            if (pumpProperties.POWER_FLUID_PRESSURE <= 0)
                throw new InvalidPumpPropertiesException("Power fluid pressure must be greater than zero.");

            if (pumpProperties.POWER_FLUID_RATE <= 0)
                throw new InvalidPumpPropertiesException("Power fluid rate must be greater than zero.");

            if (pumpProperties.POWER_FLUID_SPECIFIC_GRAVITY <= 0)
                throw new InvalidPumpPropertiesException("Power fluid specific gravity must be greater than zero.");
        }

        /// <summary>
        /// Validates piston pump properties.
        /// </summary>
        public static void ValidatePistonPumpProperties(HYDRAULIC_PISTON_PUMP_PROPERTIES pumpProperties)
        {
            if (pumpProperties == null)
                throw new ArgumentNullException(nameof(pumpProperties));

            if (pumpProperties.PISTON_DIAMETER <= 0)
                throw new InvalidPumpPropertiesException("Piston diameter must be greater than zero.");

            if (pumpProperties.ROD_DIAMETER >= pumpProperties.PISTON_DIAMETER)
                throw new InvalidPumpPropertiesException("Rod diameter must be less than piston diameter.");

            if (pumpProperties.STROKE_LENGTH <= 0)
                throw new InvalidPumpPropertiesException("Stroke length must be greater than zero.");

            if (pumpProperties.STROKES_PER_MINUTE <= 0)
                throw new InvalidPumpPropertiesException("Strokes per minute must be greater than zero.");

            if (pumpProperties.POWER_FLUID_PRESSURE <= 0)
                throw new InvalidPumpPropertiesException("Power fluid pressure must be greater than zero.");

            if (pumpProperties.POWER_FLUID_RATE <= 0)
                throw new InvalidPumpPropertiesException("Power fluid rate must be greater than zero.");

            if (pumpProperties.POWER_FLUID_SPECIFIC_GRAVITY <= 0)
                throw new InvalidPumpPropertiesException("Power fluid specific gravity must be greater than zero.");
        }

        /// <summary>
        /// Validates all calculation parameters for jet pump.
        /// </summary>
        public static void ValidateJetPumpCalculationParameters(
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties,
            HYDRAULIC_JET_PUMP_PROPERTIES pumpProperties)
        {
            ValidateWellProperties(wellProperties);
            ValidateJetPumpProperties(pumpProperties);
        }

        /// <summary>
        /// Validates all calculation parameters for piston pump.
        /// </summary>
        public static void ValidatePistonPumpCalculationParameters(
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties,
            HYDRAULIC_PISTON_PUMP_PROPERTIES pumpProperties)
        {
            ValidateWellProperties(wellProperties);
            ValidatePistonPumpProperties(pumpProperties);
        }
    }
}

