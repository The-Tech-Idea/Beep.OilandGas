using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.EconomicAnalysis;

namespace Beep.OilandGas.EconomicAnalysis.Calculations
{
    /// <summary>
    /// Provides sensitivity analysis calculations.
    /// </summary>
    public static class SensitivityAnalysis
    {
        /// <summary>
        /// Performs one-way sensitivity analysis on a single parameter.
        /// </summary>
        /// <param name="baseCaseCashFlows">Base case cash flows.</param>
        /// <param name="discountRate">Discount rate.</param>
        /// <param name="parameterName">Name of the parameter being varied (e.g., "Oil Price").</param>
        /// <param name="variationPercent">Percentage variation to apply (e.g., 0.10 for +/- 10%).</param>
        /// <param name="scalingFunction">Function to scale cash flows based on variation. 
        /// Input is (OriginalCashFlow, VariationFactor). Factor is 1.0 +/- percent.</param>
        /// <returns>Sensitivity result containing base, low, and high NPVs.</returns>
        public static SENSITIVITY_RESULT PerformOneWaySensitivity(
            CashFlow[] baseCaseCashFlows,
            double discountRate,
            string parameterName,
            double variationPercent,
            Func<CashFlow, double, CashFlow> scalingFunction)
        {
            if (baseCaseCashFlows == null || baseCaseCashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty.", nameof(baseCaseCashFlows));

            if (scalingFunction == null)
                throw new ArgumentNullException(nameof(scalingFunction));

            // Calculate Base Case
            double baseNPV = EconomicCalculator.CalculateNPV(baseCaseCashFlows, discountRate);
            double baseIRR = EconomicCalculator.CalculateIRR(baseCaseCashFlows);

            // Calculate Low Case (Base * (1 - variation))
            double lowFactor = 1.0 - variationPercent;
            var lowCashFlows = baseCaseCashFlows.Select(cf => scalingFunction(cf, lowFactor)).ToArray();
            double lowNPV = EconomicCalculator.CalculateNPV(lowCashFlows, discountRate);
            double lowIRR = EconomicCalculator.CalculateIRR(lowCashFlows);

            // Calculate High Case (Base * (1 + variation))
            double highFactor = 1.0 + variationPercent;
            var highCashFlows = baseCaseCashFlows.Select(cf => scalingFunction(cf, highFactor)).ToArray();
            double highNPV = EconomicCalculator.CalculateNPV(highCashFlows, discountRate);
            double highIRR = EconomicCalculator.CalculateIRR(highCashFlows);

            return new SENSITIVITY_RESULT
            {
                PARAMETER_NAME = parameterName,
                VARIATION_PERCENT = variationPercent,
                BASE_NPV = (decimal)baseNPV,
                LOW_NPV = (decimal)lowNPV,
                HIGH_NPV = (decimal)highNPV,
                BASE_IRR = (decimal)baseIRR,
                LOW_IRR = (decimal)lowIRR,
                HIGH_IRR = (decimal)highIRR
            };
        }

        /// <summary>
        /// Helper to scale only Revenue cash flows (positive values).
        /// Useful for Price or Production sensitivity.
        /// </summary>
        public static CashFlow ScaleRevenue(CashFlow original, double factor)
        {
            if (original.Amount > 0)
            {
                return new CashFlow(original.Period, original.Amount * factor, original.Description);
            }
            return original;
        }

        /// <summary>
        /// Helper to scale only Cost cash flows (negative values).
        /// Useful for CAPEX or OPEX sensitivity.
        /// </summary>
        public static CashFlow ScaleCost(CashFlow original, double factor)
        {
            if (original.Amount < 0)
            {
                return new CashFlow(original.Period, original.Amount * factor, original.Description);
            }
            return original;
        }

        /// <summary>
        /// Helper to scale Initial Investment (Period 0 negative).
        /// Useful for CAPEX sensitivity specifically.
        /// </summary>
        public static CashFlow ScaleCapex(CashFlow original, double factor)
        {
            if (original.Period == 0 && original.Amount < 0)
            {
                return new CashFlow(original.Period, original.Amount * factor, original.Description);
            }
            return original;
        }
    }

    /// <summary>
    /// Result structure for sensitivity analysis.
    /// </summary>
    public class SENSITIVITY_RESULT
    {
        public string PARAMETER_NAME { get; set; }
        public double VARIATION_PERCENT { get; set; }
        public decimal BASE_NPV { get; set; }
        public decimal LOW_NPV { get; set; }
        public decimal HIGH_NPV { get; set; }
        public decimal BASE_IRR { get; set; }
        public decimal LOW_IRR { get; set; }
        public decimal HIGH_IRR { get; set; }
    }
}
