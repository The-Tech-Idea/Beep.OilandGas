using System;

using Beep.OilandGas.OilProperties.Constants;
using Beep.OilandGas.OilProperties.Exceptions;
using Beep.OilandGas.Models.OilProperties;

namespace Beep.OilandGas.OilProperties.Validation
{
    /// <summary>
    /// Provides validation for oil property calculations.
    /// </summary>
    public static class OilPropertyValidator
    {
        /// <summary>
        /// Validates oil property conditions.
        /// </summary>
        public static void ValidateOilPropertyConditions(OilPropertyConditions conditions)
        {
            if (conditions == null)
                throw new ArgumentNullException(nameof(conditions));

            if (conditions.Pressure <= 0)
                throw new InvalidOilPropertyConditionsException("Pressure must be greater than zero.");

            if (conditions.Temperature <= 0)
                throw new InvalidOilPropertyConditionsException("Temperature must be greater than zero.");

            if (conditions.ApiGravity < OilPropertyConstants.MinimumApiGravity || conditions.ApiGravity > OilPropertyConstants.MaximumApiGravity)
                throw new OilPropertyParameterOutOfRangeException(
                    nameof(conditions.ApiGravity),
                    $"API gravity must be between {OilPropertyConstants.MinimumApiGravity} and {OilPropertyConstants.MaximumApiGravity}.");

            if (conditions.GasSpecificGravity <= 0)
                throw new InvalidOilPropertyConditionsException("Gas specific gravity must be greater than zero.");

            if (conditions.SolutionGasOilRatio.HasValue && 
                (conditions.SolutionGasOilRatio.Value < OilPropertyConstants.MinimumSolutionGOR || 
                 conditions.SolutionGasOilRatio.Value > OilPropertyConstants.MaximumSolutionGOR))
            {
                throw new OilPropertyParameterOutOfRangeException(
                    nameof(conditions.SolutionGasOilRatio),
                    $"Solution GOR must be between {OilPropertyConstants.MinimumSolutionGOR} and {OilPropertyConstants.MaximumSolutionGOR} scf/STB.");
            }

            if (conditions.BubblePointPressure.HasValue && 
                (conditions.BubblePointPressure.Value < OilPropertyConstants.MinimumBubblePointPressure || 
                 conditions.BubblePointPressure.Value > OilPropertyConstants.MaximumBubblePointPressure))
            {
                throw new OilPropertyParameterOutOfRangeException(
                    nameof(conditions.BubblePointPressure),
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

