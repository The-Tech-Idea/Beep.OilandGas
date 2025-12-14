using System;
using Beep.OilandGas.PlungerLift.Models;
using Beep.OilandGas.PlungerLift.Constants;
using Beep.OilandGas.PlungerLift.Exceptions;

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
        public static void ValidateWellProperties(PlungerLiftWellProperties wellProperties)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (wellProperties.WellDepth <= 0)
                throw new InvalidWellPropertiesException("Well depth must be greater than zero.");

            if (wellProperties.TubingDiameter <= 0)
                throw new InvalidWellPropertiesException("Tubing diameter must be greater than zero.");

            if (wellProperties.PlungerDiameter <= 0)
                throw new InvalidWellPropertiesException("Plunger diameter must be greater than zero.");

            if (wellProperties.PlungerDiameter >= wellProperties.TubingDiameter)
                throw new InvalidWellPropertiesException("Plunger diameter must be less than tubing diameter.");

            if (wellProperties.WellheadPressure < 0)
                throw new InvalidWellPropertiesException("Wellhead pressure cannot be negative.");

            if (wellProperties.CasingPressure <= wellProperties.WellheadPressure)
                throw new InvalidWellPropertiesException("Casing pressure must be greater than wellhead pressure.");

            if (wellProperties.BottomHolePressure <= wellProperties.WellheadPressure)
                throw new InvalidWellPropertiesException("Bottom hole pressure must be greater than wellhead pressure.");

            if (wellProperties.WaterCut < 0 || wellProperties.WaterCut > 1)
                throw new InvalidWellPropertiesException("Water cut must be between 0 and 1.");

            if (wellProperties.GasSpecificGravity <= 0)
                throw new InvalidWellPropertiesException("Gas specific gravity must be greater than zero.");

            if (wellProperties.LiquidProductionRate <= 0)
                throw new InvalidWellPropertiesException("Liquid production rate must be greater than zero.");

            // Validate pressure differential
            decimal pressureDifferential = wellProperties.CasingPressure - wellProperties.WellheadPressure;
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
        public static void ValidateCycleResult(PlungerLiftCycleResult cycleResult)
        {
            if (cycleResult == null)
                throw new ArgumentNullException(nameof(cycleResult));

            if (cycleResult.CycleTime <= 0)
                throw new PlungerLiftParameterOutOfRangeException(
                    nameof(cycleResult.CycleTime),
                    "Cycle time must be greater than zero.");

            if (cycleResult.CycleTime > PlungerLiftConstants.MaximumCycleTime)
            {
                throw new PlungerLiftParameterOutOfRangeException(
                    nameof(cycleResult.CycleTime),
                    $"Cycle time ({cycleResult.CycleTime:F2} minutes) exceeds maximum ({PlungerLiftConstants.MaximumCycleTime} minutes).");
            }

            if (cycleResult.FallTime <= 0 || cycleResult.RiseTime <= 0)
            {
                throw new PlungerLiftParameterOutOfRangeException(
                    nameof(cycleResult),
                    "Fall time and rise time must be greater than zero.");
            }
        }

        /// <summary>
        /// Validates gas requirements.
        /// </summary>
        public static void ValidateGasRequirements(PlungerLiftGasRequirements gasRequirements)
        {
            if (gasRequirements == null)
                throw new ArgumentNullException(nameof(gasRequirements));

            if (gasRequirements.RequiredGasLiquidRatio < PlungerLiftConstants.MinimumGasLiquidRatio)
            {
                throw new PlungerLiftParameterOutOfRangeException(
                    nameof(gasRequirements.RequiredGasLiquidRatio),
                    $"Required GLR ({gasRequirements.RequiredGasLiquidRatio:F2} scf/bbl) is below minimum ({PlungerLiftConstants.MinimumGasLiquidRatio} scf/bbl).");
            }

            if (gasRequirements.RequiredGasLiquidRatio > PlungerLiftConstants.MaximumGasLiquidRatio)
            {
                throw new PlungerLiftParameterOutOfRangeException(
                    nameof(gasRequirements.RequiredGasLiquidRatio),
                    $"Required GLR ({gasRequirements.RequiredGasLiquidRatio:F2} scf/bbl) exceeds maximum ({PlungerLiftConstants.MaximumGasLiquidRatio} scf/bbl).");
            }
        }

        /// <summary>
        /// Validates all calculation parameters.
        /// </summary>
        public static void ValidateCalculationParameters(PlungerLiftWellProperties wellProperties)
        {
            ValidateWellProperties(wellProperties);
        }
    }
}

