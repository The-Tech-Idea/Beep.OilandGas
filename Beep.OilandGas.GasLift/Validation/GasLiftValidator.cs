using System;
using Beep.OilandGas.GasLift.Models;
using Beep.OilandGas.GasLift.Constants;
using Beep.OilandGas.GasLift.Exceptions;

namespace Beep.OilandGas.GasLift.Validation
{
    /// <summary>
    /// Provides validation for gas lift calculations.
    /// </summary>
    public static class GasLiftValidator
    {
        /// <summary>
        /// Validates well properties.
        /// </summary>
        public static void ValidateWellProperties(GasLiftWellProperties wellProperties)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (wellProperties.WellDepth <= 0)
                throw new InvalidWellPropertiesException("Well depth must be greater than zero.");

            if (wellProperties.TubingDiameter <= 0)
                throw new InvalidWellPropertiesException("Tubing diameter must be greater than zero.");

            if (wellProperties.WellheadPressure <= 0)
                throw new InvalidWellPropertiesException("Wellhead pressure must be greater than zero.");

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
        /// Validates gas injection rate.
        /// </summary>
        public static void ValidateGasInjectionRate(decimal gasInjectionRate)
        {
            if (gasInjectionRate < GasLiftConstants.MinimumGasInjectionRate)
                throw new GasLiftParameterOutOfRangeException(
                    nameof(gasInjectionRate),
                    $"Gas injection rate ({gasInjectionRate} Mscf/day) is below minimum ({GasLiftConstants.MinimumGasInjectionRate} Mscf/day).");

            if (gasInjectionRate > GasLiftConstants.MaximumGasInjectionRate)
                throw new GasLiftParameterOutOfRangeException(
                    nameof(gasInjectionRate),
                    $"Gas injection rate ({gasInjectionRate} Mscf/day) exceeds maximum ({GasLiftConstants.MaximumGasInjectionRate} Mscf/day).");
        }

        /// <summary>
        /// Validates number of valves.
        /// </summary>
        public static void ValidateNumberOfValves(int numberOfValves)
        {
            if (numberOfValves < GasLiftConstants.MinimumNumberOfValves)
                throw new GasLiftParameterOutOfRangeException(
                    nameof(numberOfValves),
                    $"Number of valves ({numberOfValves}) is below minimum ({GasLiftConstants.MinimumNumberOfValves}).");

            if (numberOfValves > GasLiftConstants.MaximumNumberOfValves)
                throw new GasLiftParameterOutOfRangeException(
                    nameof(numberOfValves),
                    $"Number of valves ({numberOfValves}) exceeds maximum ({GasLiftConstants.MaximumNumberOfValves}).");
        }

        /// <summary>
        /// Validates gas injection pressure.
        /// </summary>
        public static void ValidateGasInjectionPressure(
            decimal gasInjectionPressure,
            decimal wellheadPressure)
        {
            if (gasInjectionPressure <= 0)
                throw new GasLiftParameterOutOfRangeException(
                    nameof(gasInjectionPressure),
                    "Gas injection pressure must be greater than zero.");

            if (gasInjectionPressure <= wellheadPressure)
                throw new GasLiftParameterOutOfRangeException(
                    nameof(gasInjectionPressure),
                    $"Gas injection pressure ({gasInjectionPressure} psia) must be greater than wellhead pressure ({wellheadPressure} psia).");
        }

        /// <summary>
        /// Validates all calculation parameters.
        /// </summary>
        public static void ValidateCalculationParameters(
            GasLiftWellProperties wellProperties,
            decimal gasInjectionPressure,
            int numberOfValves)
        {
            ValidateWellProperties(wellProperties);
            ValidateGasInjectionPressure(gasInjectionPressure, wellProperties.WellheadPressure);
            ValidateNumberOfValves(numberOfValves);
        }
    }
}

