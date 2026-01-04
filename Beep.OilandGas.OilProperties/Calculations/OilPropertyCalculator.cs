using System;
using Beep.OilandGas.Models.OilProperties;

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
        public static OilPropertyResult CalculateOilProperties(OilPropertyConditions conditions)
        {
            if (conditions == null)
                throw new ArgumentNullException(nameof(conditions));

            var properties = new OilPropertyResult();

            // Calculate API gravity
            properties.ApiGravity = conditions.ApiGravity;

            // Calculate specific gravity from API gravity
            properties.SpecificGravity = CalculateSpecificGravity(conditions.ApiGravity);

            // Calculate solution GOR if not provided
            decimal solutionGOR = conditions.SolutionGasOilRatio ?? 
                                 CalculateSolutionGOR(conditions);

            properties.SolutionGasOilRatio = solutionGOR;

            // Calculate bubble point pressure if not provided
            decimal bubblePointPressure = conditions.BubblePointPressure ?? 
                                         CalculateBubblePointPressure(conditions);

            // Calculate formation volume factor
            properties.FormationVolumeFactor = CalculateFormationVolumeFactor(
                conditions, solutionGOR, bubblePointPressure);

            // Calculate oil density
            properties.Density = CalculateOilDensity(
                conditions, properties.SpecificGravity, properties.FormationVolumeFactor);

            // Calculate oil viscosity
            properties.Viscosity = CalculateOilViscosity(
                conditions, properties.ApiGravity, solutionGOR);

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
        public static decimal CalculateSolutionGOR(OilPropertyConditions conditions)
        {
            // Standing correlation: Rs = γg * ((P / 18.2 + 1.4) * 10^(0.0125*API - 0.00091*T))^1.2048
            decimal temperatureF = conditions.Temperature - 460m; // Convert Rankine to Fahrenheit

            decimal term1 = conditions.Pressure / 18.2m + 1.4m;
            decimal term2 = (decimal)Math.Pow(10.0, (double)(0.0125m * conditions.ApiGravity - 0.00091m * temperatureF));
            decimal term3 = term1 * term2;

            decimal solutionGOR = conditions.GasSpecificGravity * (decimal)Math.Pow((double)term3, 1.2048);

            return Math.Max(0m, solutionGOR);
        }

        /// <summary>
        /// Calculates bubble point pressure using Standing correlation.
        /// </summary>
        public static decimal CalculateBubblePointPressure(OilPropertyConditions conditions)
        {
            // Standing correlation: Pb = 18.2 * ((Rs / γg)^0.83 * 10^(0.00091*T - 0.0125*API) - 1.4)
            decimal temperatureF = conditions.Temperature - 460m; // Convert Rankine to Fahrenheit

            decimal solutionGOR = conditions.SolutionGasOilRatio ?? CalculateSolutionGOR(conditions);

            if (solutionGOR <= 0)
                return 0m;

            decimal term1 = solutionGOR / conditions.GasSpecificGravity;
            decimal term2 = (decimal)Math.Pow((double)term1, 0.83);
            decimal term3 = (decimal)Math.Pow(10.0, (double)(0.00091m * temperatureF - 0.0125m * conditions.ApiGravity));
            decimal term4 = term2 * term3 - 1.4m;

            decimal bubblePointPressure = 18.2m * term4;

            return Math.Max(0m, bubblePointPressure);
        }

        /// <summary>
        /// Calculates oil formation volume factor using Standing correlation.
        /// </summary>
        public static decimal CalculateFormationVolumeFactor(
            OilPropertyConditions conditions,
            decimal solutionGOR,
            decimal bubblePointPressure)
        {
            // Standing correlation: Bo = 0.9759 + 0.000120 * (Rs * (γg/γo)^0.5 + 1.25*T)^1.2
            decimal temperatureF = conditions.Temperature - 460m; // Convert Rankine to Fahrenheit
            decimal specificGravity = CalculateSpecificGravity(conditions.ApiGravity);

            decimal term1 = solutionGOR * (decimal)Math.Sqrt((double)(conditions.GasSpecificGravity / specificGravity));
            decimal term2 = 1.25m * temperatureF;
            decimal term3 = term1 + term2;
            decimal term4 = (decimal)Math.Pow((double)term3, 1.2);

            decimal formationVolumeFactor = 0.9759m + 0.000120m * term4;

            // Adjust for pressure above bubble point
            if (conditions.Pressure > bubblePointPressure && bubblePointPressure > 0)
            {
                decimal pressureDifference = conditions.Pressure - bubblePointPressure;
                decimal compressibility = 0.00001m; // Approximate compressibility
                formationVolumeFactor *= (1.0m - compressibility * pressureDifference);
            }

            return Math.Max(1.0m, formationVolumeFactor);
        }

        /// <summary>
        /// Calculates oil density.
        /// </summary>
        public static decimal CalculateOilDensity(
            OilPropertyConditions conditions,
            decimal specificGravity,
            decimal formationVolumeFactor)
        {
            // Density = (γo * 62.4) / Bo
            decimal density = (specificGravity * 62.4m) / formationVolumeFactor;

            return Math.Max(0m, density);
        }

        /// <summary>
        /// Calculates oil viscosity using Beggs-Robinson correlation.
        /// </summary>
        public static decimal CalculateOilViscosity(
            OilPropertyConditions conditions,
            decimal apiGravity,
            decimal solutionGOR)
        {
            // Beggs-Robinson correlation
            decimal temperatureF = conditions.Temperature - 460m; // Convert Rankine to Fahrenheit

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
            OilPropertyConditions conditions,
            decimal formationVolumeFactor,
            decimal bubblePointPressure)
        {
            // Simplified compressibility calculation
            // Co = (1 / Bo) * (dBo / dP)
            // Approximate: Co ≈ 10^-5 * (1 + 3 * Rs) / (P - Pb) for P > Pb

            if (conditions.Pressure <= bubblePointPressure || bubblePointPressure <= 0)
            {
                // Below bubble point, compressibility is very small
                return 0.000001m;
            }

            decimal solutionGOR = conditions.SolutionGasOilRatio ?? 0m;
            decimal pressureDifference = conditions.Pressure - bubblePointPressure;

            if (pressureDifference <= 0)
                return 0.000001m;

            decimal compressibility = 0.00001m * (1.0m + 3.0m * solutionGOR / 1000m) / pressureDifference;

            return Math.Max(0.000001m, Math.Min(0.001m, compressibility));
        }

        /// <summary>
        /// Calculates bubble point pressure and solution GOR.
        /// </summary>
        public static BubblePointResult CalculateBubblePoint(OilPropertyConditions conditions)
        {
            if (conditions == null)
                throw new ArgumentNullException(nameof(conditions));

            var result = new BubblePointResult();

            // If solution GOR is provided, calculate bubble point pressure
            if (conditions.SolutionGasOilRatio.HasValue)
            {
                result.SolutionGasOilRatio = conditions.SolutionGasOilRatio.Value;
                result.BubblePointPressure = CalculateBubblePointPressure(conditions);
            }
            else
            {
                // If bubble point pressure is provided, calculate solution GOR
                if (conditions.BubblePointPressure.HasValue)
                {
                    result.BubblePointPressure = conditions.BubblePointPressure.Value;
                    
                    // Iterate to find solution GOR that gives this bubble point
                    decimal solutionGOR = CalculateSolutionGOR(conditions);
                    var testConditions = new OilPropertyConditions
                    {
                        Pressure = result.BubblePointPressure,
                        Temperature = conditions.Temperature,
                        ApiGravity = conditions.ApiGravity,
                        GasSpecificGravity = conditions.GasSpecificGravity,
                        SolutionGasOilRatio = solutionGOR
                    };

                    decimal calculatedBubblePoint = CalculateBubblePointPressure(testConditions);
                    int iterations = 0;
                    while (Math.Abs(calculatedBubblePoint - result.BubblePointPressure) > 1m && iterations < 20)
                    {
                        if (calculatedBubblePoint > result.BubblePointPressure)
                            solutionGOR *= 0.95m;
                        else
                            solutionGOR *= 1.05m;

                        testConditions.SolutionGasOilRatio = solutionGOR;
                        calculatedBubblePoint = CalculateBubblePointPressure(testConditions);
                        iterations++;
                    }

                    result.SolutionGasOilRatio = solutionGOR;
                }
                else
                {
                    // Calculate both from conditions
                    result.SolutionGasOilRatio = CalculateSolutionGOR(conditions);
                    result.BubblePointPressure = CalculateBubblePointPressure(conditions);
                }
            }

            return result;
        }
    }
}

