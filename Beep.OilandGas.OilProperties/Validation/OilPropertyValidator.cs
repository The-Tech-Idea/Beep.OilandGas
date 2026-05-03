using System;

using Beep.OilandGas.OilProperties.Constants;
using Beep.OilandGas.OilProperties.Exceptions;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.OilProperties;

namespace Beep.OilandGas.OilProperties.Validation
{
    /// <summary>
    /// Provides validation for oil property calculations.
    /// </summary>
    public static class OilPropertyValidator
    {
        /// <summary>Validates Rankine temperature for Standing / Beggs–Robinson screening.</summary>
        public static void ValidateTemperatureRankine(decimal temperatureRankine)
        {
            if (temperatureRankine <= 0m)
                throw new InvalidOilPropertyConditionsException("Temperature must be greater than zero (Rankine).");

            if (temperatureRankine < OilPropertyConstants.MinimumTemperatureRankine)
                throw new InvalidOilPropertyConditionsException(
                    $"Temperature must be at least {OilPropertyConstants.MinimumTemperatureRankine} °R (~0 °F) for Standing / Beggs–Robinson correlations.");
        }

        /// <summary>Validates composition and P–T sweep for matrix-style calculations.</summary>
        public static void ValidateCompositionForPvtSweeps(
            OilComposition composition,
            decimal minPressurePsia,
            decimal maxPressurePsia,
            decimal minTemperatureRankine,
            decimal maxTemperatureRankine)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            if (composition.GasOilRatio < 0m)
            {
                throw new OilPropertyParameterOutOfRangeException(
                    nameof(composition.GasOilRatio),
                    "Gas-oil ratio cannot be negative.");
            }

            ValidatePressureTemperatureSweep(
                minPressurePsia,
                maxPressurePsia,
                minTemperatureRankine,
                maxTemperatureRankine,
                composition.OilGravity);
        }

        /// <summary>Validates P–T window for matrix / trend sweeps (Rankine throughout).</summary>
        public static void ValidatePressureTemperatureSweep(
            decimal minPressurePsia,
            decimal maxPressurePsia,
            decimal minTemperatureRankine,
            decimal maxTemperatureRankine,
            decimal apiGravity)
        {
            if (minPressurePsia <= 0m)
                throw new InvalidOilPropertyConditionsException("Minimum pressure must be greater than zero (psia).");

            if (maxPressurePsia < minPressurePsia)
                throw new InvalidOilPropertyConditionsException("Maximum pressure must be greater than or equal to minimum pressure.");

            ValidateTemperatureRankine(minTemperatureRankine);
            ValidateTemperatureRankine(maxTemperatureRankine);

            if (apiGravity < OilPropertyConstants.MinimumApiGravity || apiGravity > OilPropertyConstants.MaximumApiGravity)
            {
                throw new OilPropertyParameterOutOfRangeException(
                    nameof(apiGravity),
                    $"API gravity must be between {OilPropertyConstants.MinimumApiGravity} and {OilPropertyConstants.MaximumApiGravity}.");
            }
        }

        /// <summary>Validates composition and pressure trend at fixed Rankine temperature.</summary>
        public static void ValidateCompositionForPressureTrend(
            OilComposition composition,
            decimal minPressurePsia,
            decimal maxPressurePsia,
            decimal temperatureRankine)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            if (composition.GasOilRatio < 0m)
            {
                throw new OilPropertyParameterOutOfRangeException(
                    nameof(composition.GasOilRatio),
                    "Gas-oil ratio cannot be negative.");
            }

            ValidatePressureTrendInputs(minPressurePsia, maxPressurePsia, temperatureRankine, composition.OilGravity);
        }

        /// <summary>Validates pressure sweep at fixed Rankine temperature (trend lines).</summary>
        public static void ValidatePressureTrendInputs(
            decimal minPressurePsia,
            decimal maxPressurePsia,
            decimal temperatureRankine,
            decimal apiGravity)
        {
            if (minPressurePsia <= 0m)
                throw new InvalidOilPropertyConditionsException("Minimum pressure must be greater than zero (psia).");

            if (maxPressurePsia < minPressurePsia)
                throw new InvalidOilPropertyConditionsException("Maximum pressure must be greater than or equal to minimum pressure.");

            ValidateTemperatureRankine(temperatureRankine);

            if (apiGravity < OilPropertyConstants.MinimumApiGravity || apiGravity > OilPropertyConstants.MaximumApiGravity)
            {
                throw new OilPropertyParameterOutOfRangeException(
                    nameof(apiGravity),
                    $"API gravity must be between {OilPropertyConstants.MinimumApiGravity} and {OilPropertyConstants.MaximumApiGravity}.");
            }
        }

        /// <summary>Validates pressure, Rankine temperature, and API for simple oil screening calls.</summary>
        public static void ValidateSimpleScreeningInputs(decimal pressurePsia, decimal temperatureRankine, decimal apiGravity)
        {
            if (pressurePsia <= 0m)
                throw new InvalidOilPropertyConditionsException("Pressure must be greater than zero (psia).");

            ValidateTemperatureRankine(temperatureRankine);

            if (apiGravity < OilPropertyConstants.MinimumApiGravity || apiGravity > OilPropertyConstants.MaximumApiGravity)
            {
                throw new OilPropertyParameterOutOfRangeException(
                    nameof(apiGravity),
                    $"API gravity must be between {OilPropertyConstants.MinimumApiGravity} and {OilPropertyConstants.MaximumApiGravity}.");
            }
        }

        /// <summary>Validates <see cref="OIL_PROPERTY_CONDITIONS"/> before black-oil calculations.</summary>
        public static void ValidateOilPropertyConditions(OIL_PROPERTY_CONDITIONS conditions)
        {
            if (conditions == null)
                throw new ArgumentNullException(nameof(conditions));

            if (conditions.PRESSURE <= 0)
                throw new InvalidOilPropertyConditionsException("Pressure must be greater than zero.");

            ValidateTemperatureRankine(conditions.TEMPERATURE);

            if (conditions.API_GRAVITY < OilPropertyConstants.MinimumApiGravity || conditions.API_GRAVITY > OilPropertyConstants.MaximumApiGravity)
                throw new OilPropertyParameterOutOfRangeException(
                    nameof(conditions.API_GRAVITY),
                    $"API gravity must be between {OilPropertyConstants.MinimumApiGravity} and {OilPropertyConstants.MaximumApiGravity}.");

            if (conditions.GAS_SPECIFIC_GRAVITY <= 0)
                throw new InvalidOilPropertyConditionsException("Gas specific gravity must be greater than zero.");

            // Optional: treat null or <= 0 as "not provided"
            if (conditions.SOLUTION_GAS_OIL_RATIO is decimal rs && rs > 0m &&
                (rs < OilPropertyConstants.MinimumSolutionGOR ||
                 rs > OilPropertyConstants.MaximumSolutionGOR))
            {
                throw new OilPropertyParameterOutOfRangeException(
                    nameof(conditions.SOLUTION_GAS_OIL_RATIO),
                    $"Solution GOR must be between {OilPropertyConstants.MinimumSolutionGOR} and {OilPropertyConstants.MaximumSolutionGOR} scf/STB.");
            }

            if (conditions.BUBBLE_POINT_PRESSURE is decimal pbp && pbp > 0m &&
                (pbp < OilPropertyConstants.MinimumBubblePointPressure ||
                 pbp > OilPropertyConstants.MaximumBubblePointPressure))
            {
                throw new OilPropertyParameterOutOfRangeException(
                    nameof(conditions.BUBBLE_POINT_PRESSURE),
                    $"Bubble point pressure must be between {OilPropertyConstants.MinimumBubblePointPressure} and {OilPropertyConstants.MaximumBubblePointPressure} psia.");
            }
        }

        /// <summary>
        /// Validates oil properties.
        /// </summary>
        public static void ValidateOilProperties(OilPropertyResult properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            if (properties.Density <= 0)
                throw new InvalidOilPropertyConditionsException("Oil density must be greater than zero.");

            if (properties.Viscosity < OilPropertyConstants.MinimumViscosity || properties.Viscosity > OilPropertyConstants.MaximumViscosity)
                throw new OilPropertyParameterOutOfRangeException(
                    nameof(properties.Viscosity),
                    $"Oil viscosity must be between {OilPropertyConstants.MinimumViscosity} and {OilPropertyConstants.MaximumViscosity} cp.");

            if (properties.FormationVolumeFactor < OilPropertyConstants.MinimumFormationVolumeFactor || 
                properties.FormationVolumeFactor > OilPropertyConstants.MaximumFormationVolumeFactor)
            {
                throw new OilPropertyParameterOutOfRangeException(
                    nameof(properties.FormationVolumeFactor),
                    $"Formation volume factor must be between {OilPropertyConstants.MinimumFormationVolumeFactor} and {OilPropertyConstants.MaximumFormationVolumeFactor}.");
            }
        }
    }
}

