using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.EconomicAnalysis;

namespace Beep.OilandGas.EconomicAnalysis.Calculations
{
    /// <summary>
    /// Provides break-even analysis calculations.
    /// </summary>
    public static class BreakEvenAnalysis
    {
        /// <summary>
        /// Calculates the break-even multiplier for a specific cash flow component (e.g., Revenue Price).
        /// Finds the multiplier X such that NPV(Revenue * X + Cost) = 0.
        /// </summary>
        /// <param name="cashFlows">Project cash flows.</param>
        /// <param name="discountRate">Discount rate.</param>
        /// <param name="targetComponentSelector">Function to select which component to vary (returns true if should vary).</param>
        /// <returns>Break-even multiplier (e.g. 0.8 means price can drop to 80% of current).</returns>
        public static double CalculateBreakEvenMultiplier(
            CashFlow[] cashFlows,
            double discountRate,
            Func<CashFlow, bool> targetComponentSelector)
        {
            if (cashFlows == null || cashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty.");

            // Split into Fixed PV and Variable PV
            double pvFixed = 0;
            double pvVariable = 0;

            foreach (var cf in cashFlows)
            {
                double pv = cf.Amount / Math.Pow(1 + discountRate, cf.Period);
                if (targetComponentSelector(cf))
                {
                    pvVariable += pv;
                }
                else
                {
                    pvFixed += pv;
                }
            }

            // We want: Fixed + Variable * Multiplier = 0
            // Variable * Multiplier = -Fixed
            // Multiplier = -Fixed / Variable
            
            if (Math.Abs(pvVariable) < 1e-9) return 0; // Cannot break even by varying this component

            double multiplier = -pvFixed / pvVariable;
            return multiplier;
        }

        /// <summary>
        /// Calculates break-even Oil Price assuming all positive cash flows are oil revenue.
        /// </summary>
        /// <param name="cashFlows">Project cash flows.</param>
        /// <param name="currentOilPrice">Current oil price used in forecast.</param>
        /// <param name="discountRate">Discount rate.</param>
        /// <returns>Break-even oil price.</returns>
        public static double CalculateBreakEvenPrice(
            CashFlow[] cashFlows,
            double currentOilPrice,
            double discountRate)
        {
            // Assume Average Price model: Revenue is linear with Price.
            // Target all positive cash flows (Revenue)
            
            double multiplier = CalculateBreakEvenMultiplier(
                cashFlows, 
                discountRate, 
                cf => cf.Amount > 0); // Vary Revenues
            
            return currentOilPrice * multiplier;
        }
    }
}
