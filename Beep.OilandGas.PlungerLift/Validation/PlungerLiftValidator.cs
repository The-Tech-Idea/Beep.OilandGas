using System;

using Beep.OilandGas.PlungerLift.Constants;
using Beep.OilandGas.PlungerLift.Exceptions;
using Beep.OilandGas.Models.Data.PlungerLift;

namespace Beep.OilandGas.PlungerLift.Validation
{
    /// <summary>
    /// Provides validation for plunger lift calculations.
    /// </summary>
    public static class PlungerLiftValidator
    {
        /// <summary>
        /// Validates well properties.
        /// </summary>
        public static void ValidateWellProperties(PLUNGER_LIFT_WELL_PROPERTIES wellProperties)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (wellProperties.WELL_DEPTH <= 0)
                throw new InvalidWellPropertiesException("Well depth must be greater than zero.");

            if (wellProperties.TUBING_DIAMETER <= 0)
                throw new InvalidWellPropertiesException("Tubing diameter must be greater than zero.");

            if (wellProperties.PLUNGER_DIAMETER <= 0)
                throw new InvalidWellPropertiesException("Plunger diameter must be greater than zero.");

            if (wellProperties.PLUNGER_DIAMETER >= wellProperties.TUBING_DIAMETER)
                throw new InvalidWellPropertiesException("Plunger diameter must be less than tubing diameter.");

            if (wellProperties.WELLHEAD_PRESSURE < 0)
                throw new InvalidWellPropertiesException("Wellhead pressure cannot be negative.");

            if (wellProperties.CASING_PRESSURE <= wellProperties.WELLHEAD_PRESSURE)
                throw new InvalidWellPropertiesException("Casing pressure must be greater than wellhead pressure.");

            if (wellProperties.BOTTOM_HOLE_PRESSURE <= wellProperties.WELLHEAD_PRESSURE)
                throw new InvalidWellPropertiesException("Bottom hole pressure must be greater than wellhead pressure.");

            if (wellProperties.WATER_CUT < 0 || wellProperties.WATER_CUT > 1)
                throw new InvalidWellPropertiesException("Water cut must be between 0 and 1.");

            if (wellProperties.GAS_SPECIFIC_GRAVITY <= 0)
                throw new InvalidWellPropertiesException("Gas specific gravity must be greater than zero.");

            if (wellProperties.LIQUID_PRODUCTION_RATE <= 0)
                throw new InvalidWellPropertiesException("Liquid production rate must be greater than zero.");

            // Validate pressure differential
            decimal pressureDifferential = wellProperties.CASING_PRESSURE - wellProperties.WELLHEAD_PRESSURE;
            if (pressureDifferential < PlungerLiftConstants.MinimumPressureDifferential)
            {
                throw new PlungerLiftParameterOutOfRangeException(
                    nameof(pressureDifferential),
                    $"Pressure differential ({pressureDifferential:F2} psi) is below minimum ({PlungerLiftConstants.MinimumPressureDifferential} psi).");
            }
        }

        /// <summary>
        /// Validates cycle result.
        /// </summary>
        public static void ValidateCycleResult(PLUNGER_LIFT_CYCLE_RESULT cycleResult)
        {
            if (cycleResult == null)
                throw new ArgumentNullException(nameof(cycleResult));

            if (cycleResult.CYCLE_TIME <= 0)
                throw new PlungerLiftParameterOutOfRangeException(
                    nameof(cycleResult.CYCLE_TIME),
                    "Cycle time must be greater than zero.");

            if (cycleResult.CYCLE_TIME > PlungerLiftConstants.MaximumCycleTime)
            {
                throw new PlungerLiftParameterOutOfRangeException(
                    nameof(cycleResult.CYCLE_TIME),
                    $"Cycle time ({cycleResult.CYCLE_TIME:F2} minutes) exceeds maximum ({PlungerLiftConstants.MaximumCycleTime} minutes).");
            }

            if (cycleResult.FALL_TIME <= 0 || cycleResult.RISE_TIME <= 0)
            {
                throw new PlungerLiftParameterOutOfRangeException(
                    nameof(cycleResult),
                    "Fall time and rise time must be greater than zero.");
            }
        }

        /// <summary>
        /// Validates gas requirements.
        /// </summary>
        public static void ValidateGasRequirements(PLUNGER_LIFT_GAS_REQUIREMENTS gasRequirements)
        {
            if (gasRequirements == null)
                throw new ArgumentNullException(nameof(gasRequirements));

            if (gasRequirements.REQUIRED_GAS_LIQUID_RATIO < PlungerLiftConstants.MinimumGasLiquidRatio)
            {
                throw new PlungerLiftParameterOutOfRangeException(
                    nameof(gasRequirements.REQUIRED_GAS_LIQUID_RATIO),
                    $"Required GLR ({gasRequirements.REQUIRED_GAS_LIQUID_RATIO:F2} scf/bbl) is below minimum ({PlungerLiftConstants.MinimumGasLiquidRatio} scf/bbl).");
            }

            if (gasRequirements.REQUIRED_GAS_LIQUID_RATIO > PlungerLiftConstants.MaximumGasLiquidRatio)
            {
                throw new PlungerLiftParameterOutOfRangeException(
                    nameof(gasRequirements.REQUIRED_GAS_LIQUID_RATIO),
                    $"Required GLR ({gasRequirements.REQUIRED_GAS_LIQUID_RATIO:F2} scf/bbl) exceeds maximum ({PlungerLiftConstants.MaximumGasLiquidRatio} scf/bbl).");
            }
        }

        /// <summary>
        /// Validates all calculation parameters.
        /// </summary>
        public static void ValidateCalculationParameters(PLUNGER_LIFT_WELL_PROPERTIES wellProperties)
        {
            ValidateWellProperties(wellProperties);
        }
    }
}

