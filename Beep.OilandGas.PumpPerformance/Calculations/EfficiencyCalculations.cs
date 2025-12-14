using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.PumpPerformance.Constants;
using Beep.OilandGas.PumpPerformance.Exceptions;
using Beep.OilandGas.PumpPerformance.Validation;
using static Beep.OilandGas.PumpPerformance.Constants.PumpConstants;

namespace Beep.OilandGas.PumpPerformance.Calculations
{
    /// <summary>
    /// Provides comprehensive efficiency calculations for pumps.
    /// </summary>
    public static class EfficiencyCalculations
    {
        /// <summary>
        /// Calculates overall pump efficiency.
        /// Formula: η = (Q * H * SG) / (3960 * BHP)
        /// </summary>
        /// <param name="flowRate">Flow rate in gallons per minute (GPM).</param>
        /// <param name="head">Head in feet.</param>
        /// <param name="brakeHorsepower">Brake horsepower (BHP).</param>
        /// <param name="specificGravity">Specific gravity of the fluid (default: 1.0 for water).</param>
        /// <returns>Overall efficiency as a decimal (0 to 1).</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateOverallEfficiency(
            double flowRate,
            double head,
            double brakeHorsepower,
            double specificGravity = WaterSpecificGravity)
        {
            PumpDataValidator.ValidateFlowRate(flowRate, nameof(flowRate));
            PumpDataValidator.ValidateHead(head, nameof(head));
            PumpDataValidator.ValidatePower(brakeHorsepower, nameof(brakeHorsepower));
            PumpDataValidator.ValidateSpecificGravity(specificGravity, nameof(specificGravity));

            if (Math.Abs(brakeHorsepower) < Epsilon)
                throw new InvalidInputException(nameof(brakeHorsepower), 
                    "Brake horsepower cannot be zero for efficiency calculation.");

            double hydraulicPower = (flowRate * head * specificGravity) / HorsepowerConversionFactor;
            return hydraulicPower / brakeHorsepower;
        }

        /// <summary>
        /// Calculates overall efficiency for arrays of data points.
        /// </summary>
        /// <param name="flowRates">Array of flow rates in GPM.</param>
        /// <param name="heads">Array of heads in feet.</param>
        /// <param name="brakeHorsepowers">Array of brake horsepower values.</param>
        /// <param name="specificGravity">Specific gravity of the fluid (default: 1.0).</param>
        /// <returns>Array of efficiency values.</returns>
        /// <exception cref="InvalidInputException">Thrown when input arrays are invalid.</exception>
        public static double[] CalculateOverallEfficiency(
            double[] flowRates,
            double[] heads,
            double[] brakeHorsepowers,
            double specificGravity = WaterSpecificGravity)
        {
            PumpDataValidator.ValidateFlowRates(flowRates, nameof(flowRates));
            PumpDataValidator.ValidateHeads(heads, nameof(heads));
            PumpDataValidator.ValidatePowers(brakeHorsepowers, nameof(brakeHorsepowers));
            PumpDataValidator.ValidateMatchingLengths(flowRates, heads, nameof(flowRates), nameof(heads));
            PumpDataValidator.ValidateMatchingLengths(flowRates, brakeHorsepowers, nameof(flowRates), nameof(brakeHorsepowers));
            PumpDataValidator.ValidateSpecificGravity(specificGravity, nameof(specificGravity));

            double[] efficiencies = new double[flowRates.Length];

            for (int i = 0; i < flowRates.Length; i++)
            {
                if (Math.Abs(brakeHorsepowers[i]) < Epsilon)
                {
                    efficiencies[i] = 0.0;
                }
                else
                {
                    efficiencies[i] = CalculateOverallEfficiency(
                        flowRates[i], heads[i], brakeHorsepowers[i], specificGravity);
                }
            }

            return efficiencies;
        }

        /// <summary>
        /// Calculates hydraulic efficiency.
        /// Formula: η_h = (Actual Head) / (Theoretical Head)
        /// For simplified calculation: η_h ≈ Overall Efficiency / Mechanical Efficiency
        /// </summary>
        /// <param name="actualHead">Actual head developed by the pump in feet.</param>
        /// <param name="theoreticalHead">Theoretical head in feet.</param>
        /// <returns>Hydraulic efficiency as a decimal (0 to 1).</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateHydraulicEfficiency(
            double actualHead,
            double theoreticalHead)
        {
            PumpDataValidator.ValidateHead(actualHead, nameof(actualHead));
            PumpDataValidator.ValidateHead(theoreticalHead, nameof(theoreticalHead));

            if (Math.Abs(theoreticalHead) < Epsilon)
                throw new InvalidInputException(nameof(theoreticalHead), 
                    "Theoretical head cannot be zero for hydraulic efficiency calculation.");

            return Math.Min(1.0, actualHead / theoreticalHead);
        }

        /// <summary>
        /// Calculates mechanical efficiency.
        /// Formula: η_m = (Brake Horsepower - Mechanical Losses) / Brake Horsepower
        /// Simplified: η_m ≈ Overall Efficiency / (Hydraulic Efficiency * Volumetric Efficiency)
        /// </summary>
        /// <param name="brakeHorsepower">Brake horsepower (BHP).</param>
        /// <param name="mechanicalLosses">Mechanical losses in horsepower.</param>
        /// <returns>Mechanical efficiency as a decimal (0 to 1).</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateMechanicalEfficiency(
            double brakeHorsepower,
            double mechanicalLosses)
        {
            PumpDataValidator.ValidatePower(brakeHorsepower, nameof(brakeHorsepower));

            if (brakeHorsepower < 0)
                throw new InvalidInputException(nameof(brakeHorsepower), 
                    "Brake horsepower cannot be negative.");

            if (mechanicalLosses < 0)
                throw new InvalidInputException(nameof(mechanicalLosses), 
                    "Mechanical losses cannot be negative.");

            if (Math.Abs(brakeHorsepower) < Epsilon)
                throw new InvalidInputException(nameof(brakeHorsepower), 
                    "Brake horsepower cannot be zero for mechanical efficiency calculation.");

            if (mechanicalLosses > brakeHorsepower)
                throw new InvalidInputException(nameof(mechanicalLosses), 
                    "Mechanical losses cannot exceed brake horsepower.");

            return Math.Max(0.0, (brakeHorsepower - mechanicalLosses) / brakeHorsepower);
        }

        /// <summary>
        /// Calculates volumetric efficiency.
        /// Formula: η_v = (Actual Flow Rate) / (Theoretical Flow Rate)
        /// </summary>
        /// <param name="actualFlowRate">Actual flow rate in GPM.</param>
        /// <param name="theoreticalFlowRate">Theoretical flow rate in GPM.</param>
        /// <returns>Volumetric efficiency as a decimal (0 to 1).</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateVolumetricEfficiency(
            double actualFlowRate,
            double theoreticalFlowRate)
        {
            PumpDataValidator.ValidateFlowRate(actualFlowRate, nameof(actualFlowRate));
            PumpDataValidator.ValidateFlowRate(theoreticalFlowRate, nameof(theoreticalFlowRate));

            if (Math.Abs(theoreticalFlowRate) < Epsilon)
                throw new InvalidInputException(nameof(theoreticalFlowRate), 
                    "Theoretical flow rate cannot be zero for volumetric efficiency calculation.");

            return Math.Min(1.0, actualFlowRate / theoreticalFlowRate);
        }

        /// <summary>
        /// Calculates the Best Efficiency Point (BEP) from efficiency data.
        /// </summary>
        /// <param name="flowRates">Array of flow rates.</param>
        /// <param name="efficiencies">Array of efficiency values.</param>
        /// <returns>Tuple containing (flowRate, efficiency) at BEP.</returns>
        /// <exception cref="InvalidInputException">Thrown when input arrays are invalid.</exception>
        public static (double flowRate, double efficiency) FindBestEfficiencyPoint(
            double[] flowRates,
            double[] efficiencies)
        {
            PumpDataValidator.ValidateFlowRates(flowRates, nameof(flowRates));
            PumpDataValidator.ValidateMatchingLengths(flowRates, efficiencies, nameof(flowRates), nameof(efficiencies));

            if (efficiencies == null || efficiencies.Length == 0)
                throw new InvalidInputException(nameof(efficiencies), 
                    "Efficiencies array cannot be null or empty.");

            int maxIndex = 0;
            double maxEfficiency = efficiencies[0];

            for (int i = 1; i < efficiencies.Length; i++)
            {
                if (efficiencies[i] > maxEfficiency)
                {
                    maxEfficiency = efficiencies[i];
                    maxIndex = i;
                }
            }

            return (flowRates[maxIndex], maxEfficiency);
        }

        /// <summary>
        /// Calculates efficiency curve data points.
        /// </summary>
        /// <param name="flowRates">Array of flow rates.</param>
        /// <param name="heads">Array of heads.</param>
        /// <param name="powers">Array of power values.</param>
        /// <param name="specificGravity">Specific gravity (default: 1.0).</param>
        /// <returns>Array of efficiency values corresponding to flow rates.</returns>
        public static double[] CalculateEfficiencyCurve(
            double[] flowRates,
            double[] heads,
            double[] powers,
            double specificGravity = WaterSpecificGravity)
        {
            return CalculateOverallEfficiency(flowRates, heads, powers, specificGravity);
        }
    }
}

