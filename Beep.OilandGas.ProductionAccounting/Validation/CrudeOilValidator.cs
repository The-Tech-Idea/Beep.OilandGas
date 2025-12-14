using System;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.ProductionAccounting.Exceptions;

namespace Beep.OilandGas.ProductionAccounting.Validation
{
    /// <summary>
    /// Validates crude oil properties and data.
    /// </summary>
    public static class CrudeOilValidator
    {
        /// <summary>
        /// Minimum valid API gravity.
        /// </summary>
        public const decimal MinApiGravity = -50m;

        /// <summary>
        /// Maximum valid API gravity.
        /// </summary>
        public const decimal MaxApiGravity = 100m;

        /// <summary>
        /// Maximum valid BS&W percentage.
        /// </summary>
        public const decimal MaxBSWPercentage = 100m;

        /// <summary>
        /// Validates crude oil properties.
        /// </summary>
        public static void Validate(CrudeOilProperties properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            ValidateApiGravity(properties.ApiGravity);
            ValidateSulfurContent(properties.SulfurContent);
            ValidateBSW(properties.BSW);
            ValidateWaterContent(properties.WaterContent);
        }

        /// <summary>
        /// Validates API gravity.
        /// </summary>
        public static void ValidateApiGravity(decimal apiGravity)
        {
            if (apiGravity < MinApiGravity || apiGravity > MaxApiGravity)
                throw new InvalidCrudeOilDataException(
                    nameof(apiGravity),
                    $"API gravity must be between {MinApiGravity} and {MaxApiGravity} degrees.");
        }

        /// <summary>
        /// Validates sulfur content.
        /// </summary>
        public static void ValidateSulfurContent(decimal sulfurContent)
        {
            if (sulfurContent < 0 || sulfurContent > 10)
                throw new InvalidCrudeOilDataException(
                    nameof(sulfurContent),
                    "Sulfur content must be between 0 and 10 weight percent.");
        }

        /// <summary>
        /// Validates BS&W percentage.
        /// </summary>
        public static void ValidateBSW(decimal bsw)
        {
            if (bsw < 0 || bsw > MaxBSWPercentage)
                throw new InvalidCrudeOilDataException(
                    nameof(bsw),
                    $"BS&W must be between 0 and {MaxBSWPercentage} percent.");
        }

        /// <summary>
        /// Validates water content.
        /// </summary>
        public static void ValidateWaterContent(decimal waterContent)
        {
            if (waterContent < 0 || waterContent > 100)
                throw new InvalidCrudeOilDataException(
                    nameof(waterContent),
                    "Water content must be between 0 and 100 percent.");
        }

        /// <summary>
        /// Validates volume.
        /// </summary>
        public static void ValidateVolume(decimal volume)
        {
            if (volume < 0)
                throw new InvalidCrudeOilDataException(
                    nameof(volume),
                    "Volume cannot be negative.");
        }
    }
}

