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
        public static void ValidateWellProperties(HydraulicPumpWellProperties wellProperties)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (wellProperties.WellDepth <= 0)
                throw new InvalidWellPropertiesException("Well depth must be greater than zero.");

            if (wellProperties.TubingDiameter <= 0)
                throw new InvalidWellPropertiesException("Tubing diameter must be greater than zero.");

            if (wellProperties.WellheadPressure < 0)
                throw new InvalidWellPropertiesException("Wellhead pressure cannot be negative.");

            if (wellProperties.BottomHolePressure <= wellProperties.WellheadPressure)
                throw new InvalidWellPropertiesException("Bottom hole pressure must be greater than wellhead pressure.");

            if (wellProperties.WaterCut < 0 || wellProperties.WaterCut > 1)
                throw new InvalidWellPropertiesException("Water cut must be between 0 and 1.");

            if (wellProperties.GasSpecificGravity <= 0)
                throw new InvalidWellPropertiesException("Gas specific gravity must be greater than zero.");

            if (wellProperties.DesiredProductionRate <= 0)
                throw new InvalidWellPropertiesException("Desired production rate must be greater than zero.");
        }

        /// <summary>
        /// Validates jet pump properties.
        /// </summary>
        public static void ValidateJetPumpProperties(HydraulicJetPumpProperties pumpProperties)
        {
            if (pumpProperties == null)
                throw new ArgumentNullException(nameof(pumpProperties));

            if (pumpProperties.NozzleDiameter <= 0)
                throw new InvalidPumpPropertiesException("Nozzle diameter must be greater than zero.");

            if (pumpProperties.ThroatDiameter <= 0)
                throw new InvalidPumpPropertiesException("Throat diameter must be greater than zero.");

            if (pumpProperties.NozzleDiameter >= pumpProperties.ThroatDiameter)
                throw new InvalidPumpPropertiesException("Nozzle diameter must be less than throat diameter.");

            if (pumpProperties.PowerFluidPressure <= 0)
                throw new InvalidPumpPropertiesException("Power fluid pressure must be greater than zero.");

            if (pumpProperties.PowerFluidRate <= 0)
                throw new InvalidPumpPropertiesException("Power fluid rate must be greater than zero.");

            if (pumpProperties.PowerFluidSpecificGravity <= 0)
                throw new InvalidPumpPropertiesException("Power fluid specific gravity must be greater than zero.");
        }

        /// <summary>
        /// Validates piston pump properties.
        /// </summary>
        public static void ValidatePistonPumpProperties(HydraulicPistonPumpProperties pumpProperties)
        {
            if (pumpProperties == null)
                throw new ArgumentNullException(nameof(pumpProperties));

            if (pumpProperties.PistonDiameter <= 0)
                throw new InvalidPumpPropertiesException("Piston diameter must be greater than zero.");

            if (pumpProperties.RodDiameter >= pumpProperties.PistonDiameter)
                throw new InvalidPumpPropertiesException("Rod diameter must be less than piston diameter.");

            if (pumpProperties.StrokeLength <= 0)
                throw new InvalidPumpPropertiesException("Stroke length must be greater than zero.");

            if (pumpProperties.StrokesPerMinute <= 0)
                throw new InvalidPumpPropertiesException("Strokes per minute must be greater than zero.");

            if (pumpProperties.PowerFluidPressure <= 0)
                throw new InvalidPumpPropertiesException("Power fluid pressure must be greater than zero.");

            if (pumpProperties.PowerFluidRate <= 0)
                throw new InvalidPumpPropertiesException("Power fluid rate must be greater than zero.");

            if (pumpProperties.PowerFluidSpecificGravity <= 0)
                throw new InvalidPumpPropertiesException("Power fluid specific gravity must be greater than zero.");
        }

        /// <summary>
        /// Validates all calculation parameters for jet pump.
        /// </summary>
        public static void ValidateJetPumpCalculationParameters(
            HydraulicPumpWellProperties wellProperties,
            HydraulicJetPumpProperties pumpProperties)
        {
            ValidateWellProperties(wellProperties);
            ValidateJetPumpProperties(pumpProperties);
        }

        /// <summary>
        /// Validates all calculation parameters for piston pump.
        /// </summary>
        public static void ValidatePistonPumpCalculationParameters(
            HydraulicPumpWellProperties wellProperties,
            HydraulicPistonPumpProperties pumpProperties)
        {
            ValidateWellProperties(wellProperties);
            ValidatePistonPumpProperties(pumpProperties);
        }
    }
}

