using System;
using Beep.OilandGas.Models.Data.OilProperties;

namespace Beep.OilandGas.OilProperties.Calculations
{
    /// <summary>
    /// Provides oil property calculations.
    /// </summary>
    public static class OilPropertyCalculator
    {
        /// <summary>
        /// Calculates oil properties at given conditions.
        /// </summary>
        /// <param name="conditions">Oil property calculation conditions.</param>
        /// <returns>Oil properties.</returns>
        public static OilPropertyResult CalculateOilProperties(OIL_PROPERTY_CONDITIONS conditions)
        {
            if (conditions == null)
                throw new ArgumentNullException(nameof(conditions));

            var properties = new OilPropertyResult();

            // Calculate API gravity
            properties.API_GRAVITY = conditions.API_GRAVITY;

            // Calculate specific gravity from API gravity
            properties.SpecificGravity = CalculateSpecificGravity(conditions.API_GRAVITY);

            // Calculate solution GOR if not provided
            decimal solutionGOR = conditions.SOLUTION_GAS_OIL_RATIO ?? 
                                 CalculateSolutionGOR(conditions);

            properties.SOLUTION_GAS_OIL_RATIO = solutionGOR;

            // Calculate bubble point pressure if not provided
            decimal bubblePointPressure = conditions.BUBBLE_POINT_PRESSURE ?? 
                                         CalculateBubblePointPressure(conditions);

            // Calculate formation volume factor
            properties.FormationVolumeFactor = CalculateFormationVolumeFactor(
                conditions, solutionGOR, bubblePointPressure);

            // Calculate oil density
            properties.Density = CalculateOilDensity(
                conditions, properties.SpecificGravity, properties.FormationVolumeFactor);

            // Calculate oil viscosity
            properties.Viscosity = CalculateOilViscosity(
                conditions, properties.API_GRAVITY, solutionGOR);

            // Calculate compressibility
            properties.Compressibility = CalculateOilCompressibility(
                conditions, properties.FormationVolumeFactor, bubblePointPressure);

            return properties;
        }

        /// <summary>
        /// Calculates specific gravity from API gravity.
        /// </summary>
        public static decimal CalculateSpecificGravity(decimal apiGravity)
        {
            // SG = 141.5 / (API + 131.5)
            return 141.5m / (apiGravity + 131.5m);
        }

        /// <summary>
        /// Calculates API gravity from specific gravity.
        /// </summary>
        public static decimal CalculateApiGravity(decimal specificGravity)
        {
            // API = (141.5 / SG) - 131.5
            return (141.5m / specificGravity) - 131.5m;
        }

        /// <summary>
        /// Calculates solution gas-oil ratio using Standing correlation.
        /// </summary>
        public static decimal CalculateSolutionGOR(OIL_PROPERTY_CONDITIONS conditions)
        {
            // Standing correlation: Rs = Î³g * ((P / 18.2 + 1.4) * 10^(0.0125*API - 0.00091*T))^1.2048
            decimal temperatureF = conditions.TEMPERATURE - 460m; // Convert Rankine to Fahrenheit

            decimal term1 = conditions.PRESSURE / 18.2m + 1.4m;
            decimal term2 = (decimal)Math.Pow(10.0, (double)(0.0125m * conditions.API_GRAVITY - 0.00091m * temperatureF));
            decimal term3 = term1 * term2;

            decimal solutionGOR = conditions.GAS_SPECIFIC_GRAVITY * (decimal)Math.Pow((double)term3, 1.2048);

            return Math.Max(0m, solutionGOR);
        }

        /// <summary>
        /// Calculates bubble point pressure using Standing correlation.
        /// </summary>
        public static decimal CalculateBubblePointPressure(OIL_PROPERTY_CONDITIONS conditions)
        {
            // Standing correlation: Pb = 18.2 * ((Rs / Î³g)^0.83 * 10^(0.00091*T - 0.0125*API) - 1.4)
            decimal temperatureF = conditions.TEMPERATURE - 460m; // Convert Rankine to Fahrenheit

            decimal solutionGOR = conditions.SOLUTION_GAS_OIL_RATIO ?? CalculateSolutionGOR(conditions);

            if (solutionGOR <= 0)
                return 0m;

            decimal term1 = solutionGOR / conditions.GAS_SPECIFIC_GRAVITY;
            decimal term2 = (decimal)Math.Pow((double)term1, 0.83);
            decimal term3 = (decimal)Math.Pow(10.0, (double)(0.00091m * temperatureF - 0.0125m * conditions.API_GRAVITY));
            decimal term4 = term2 * term3 - 1.4m;

            decimal bubblePointPressure = 18.2m * term4;

            return Math.Max(0m, bubblePointPressure);
        }

        /// <summary>
        /// Calculates oil formation volume factor using Standing correlation.
        /// </summary>
        public static decimal CalculateFormationVolumeFactor(
            OIL_PROPERTY_CONDITIONS conditions,
            decimal solutionGOR,
            decimal bubblePointPressure)
        {
            // Standing correlation: Bo = 0.9759 + 0.000120 * (Rs * (Î³g/Î³o)^0.5 + 1.25*T)^1.2
            decimal temperatureF = conditions.TEMPERATURE - 460m; // Convert Rankine to Fahrenheit
            decimal specificGravity = CalculateSpecificGravity(conditions.API_GRAVITY);

            decimal term1 = solutionGOR * (decimal)Math.Sqrt((double)(conditions.GAS_SPECIFIC_GRAVITY / specificGravity));
            decimal term2 = 1.25m * temperatureF;
            decimal term3 = term1 + term2;
            decimal term4 = (decimal)Math.Pow((double)term3, 1.2);

            decimal formationVolumeFactor = 0.9759m + 0.000120m * term4;

            // Adjust for pressure above bubble point
            if (conditions.PRESSURE > bubblePointPressure && bubblePointPressure > 0)
            {
                decimal pressureDifference = conditions.PRESSURE - bubblePointPressure;
                decimal compressibility = 0.00001m; // Approximate compressibility
                formationVolumeFactor *= (1.0m - compressibility * pressureDifference);
            }

            return Math.Max(1.0m, formationVolumeFactor);
        }

        /// <summary>
        /// Calculates oil density.
        /// </summary>
        public static decimal CalculateOilDensity(
            OIL_PROPERTY_CONDITIONS conditions,
            decimal specificGravity,
            decimal formationVolumeFactor)
        {
            // Density = (Î³o * 62.4) / Bo
            decimal density = (specificGravity * 62.4m) / formationVolumeFactor;

            return Math.Max(0m, density);
        }

        /// <summary>
        /// Calculates oil viscosity using Beggs-Robinson correlation.
        /// </summary>
        public static decimal CalculateOilViscosity(
            OIL_PROPERTY_CONDITIONS conditions,
            decimal apiGravity,
            decimal solutionGOR)
        {
            // Beggs-Robinson correlation
            decimal temperatureF = conditions.TEMPERATURE - 460m; // Convert Rankine to Fahrenheit

            // Dead oil viscosity
            decimal x = (decimal)Math.Pow(10.0, (double)(3.0324m - 0.02023m * apiGravity));
            decimal deadOilViscosity = (decimal)Math.Pow(10.0, (double)(x * (decimal)Math.Pow((double)(temperatureF - 460m), -1.163))) - 1.0m;

            if (deadOilViscosity < 0)
                deadOilViscosity = 0.1m;

            // Live oil viscosity (with solution gas)
            decimal a = 10.715m * (decimal)Math.Pow((double)(solutionGOR + 100m), -0.515);
            decimal b = 5.44m * (decimal)Math.Pow((double)(solutionGOR + 150m), -0.338);

            decimal liveOilViscosity = a * (decimal)Math.Pow((double)deadOilViscosity, (double)b);

            return Math.Max(0.1m, liveOilViscosity);
        }

        /// <summary>
        /// Calculates oil compressibility.
        /// </summary>
        public static decimal CalculateOilCompressibility(
            OIL_PROPERTY_CONDITIONS conditions,
            decimal formationVolumeFactor,
            decimal bubblePointPressure)
        {
            // Simplified compressibility calculation
            // Co = (1 / Bo) * (dBo / dP)
            // Approximate: Co â‰ˆ 10^-5 * (1 + 3 * Rs) / (P - Pb) for P > Pb

            if (conditions.PRESSURE <= bubblePointPressure || bubblePointPressure <= 0)
            {
                // Below bubble point, compressibility is very small
                return 0.000001m;
            }

            decimal solutionGOR = conditions.SOLUTION_GAS_OIL_RATIO ?? 0m;
            decimal pressureDifference = conditions.PRESSURE - bubblePointPressure;

            if (pressureDifference <= 0)
                return 0.000001m;

            decimal compressibility = 0.00001m * (1.0m + 3.0m * solutionGOR / 1000m) / pressureDifference;

            return Math.Max(0.000001m, Math.Min(0.001m, compressibility));
        }

        /// <summary>
        /// Calculates bubble point pressure and solution GOR.
        /// </summary>
        public static BUBBLE_POINT_RESULT CalculateBubblePoint(OIL_PROPERTY_CONDITIONS conditions)
        {
            if (conditions == null)
                throw new ArgumentNullException(nameof(conditions));

            var result = new BUBBLE_POINT_RESULT();

            // If solution GOR is provided, calculate bubble point pressure
            if (conditions.SOLUTION_GAS_OIL_RATIO.HasValue)
            {
                result.SOLUTION_GAS_OIL_RATIO = conditions.SOLUTION_GAS_OIL_RATIO.Value;
                result.BUBBLE_POINT_PRESSURE = CalculateBubblePointPressure(conditions);
            }
            else
            {
                // If bubble point pressure is provided, calculate solution GOR
                if (conditions.BUBBLE_POINT_PRESSURE.HasValue)
                {
                    result.BUBBLE_POINT_PRESSURE = conditions.BUBBLE_POINT_PRESSURE.Value;
                    
                    // Iterate to find solution GOR that gives this bubble point
                    decimal solutionGOR = CalculateSolutionGOR(conditions);
                    var testConditions = new OIL_PROPERTY_CONDITIONS
                    {
                        Pressure = result.BUBBLE_POINT_PRESSURE,
                        Temperature = conditions.TEMPERATURE,
                        ApiGravity = conditions.API_GRAVITY,
                        GasSpecificGravity = conditions.GAS_SPECIFIC_GRAVITY,
                        SolutionGasOilRatio = solutionGOR
                    };

                    decimal calculatedBubblePoint = CalculateBubblePointPressure(testConditions);
                    int iterations = 0;
                    while (Math.Abs(calculatedBubblePoint - result.BUBBLE_POINT_PRESSURE) > 1m && iterations < 20)
                    {
                        if (calculatedBubblePoint > result.BUBBLE_POINT_PRESSURE)
                            solutionGOR *= 0.95m;
                        else
                            solutionGOR *= 1.05m;

                        testConditions.SOLUTION_GAS_OIL_RATIO = solutionGOR;
                        calculatedBubblePoint = CalculateBubblePointPressure(testConditions);
                        iterations++;
                    }

                    result.SOLUTION_GAS_OIL_RATIO = solutionGOR;
                }
                else
                {
                    // Calculate both from conditions
                    result.SOLUTION_GAS_OIL_RATIO = CalculateSolutionGOR(conditions);
                    result.BUBBLE_POINT_PRESSURE = CalculateBubblePointPressure(conditions);
                }
            }

            return result;
        }
    }
}

