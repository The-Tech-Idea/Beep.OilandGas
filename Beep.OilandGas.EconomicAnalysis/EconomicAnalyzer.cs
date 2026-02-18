using Beep.OilandGas.EconomicAnalysis.Calculations;
using Beep.OilandGas.Models.Data.EconomicAnalysis;

namespace Beep.OilandGas.EconomicAnalysis
{
    /// <summary>
    /// Main analyzer class for economic analysis.
    /// </summary>
    public static class EconomicAnalyzer
    {
        /// <summary>
        /// Calculates NPV.
        /// </summary>
        public static double CalculateNPV(CashFlow[] cashFlows, double discountRate)
        {
            return EconomicCalculator.CalculateNPV(cashFlows, discountRate);
        }

        /// <summary>
        /// Calculates IRR.
        /// </summary>
        public static double CalculateIRR(CashFlow[] cashFlows, double initialGuess = 0.1)
        {
            return EconomicCalculator.CalculateIRR(cashFlows, initialGuess);
        }

        /// <summary>
        /// Performs comprehensive economic analysis.
        /// </summary>
        public static EconomicResult Analyze(CashFlow[] cashFlows, double discountRate,
            double financeRate = 0.1, double reinvestRate = 0.1)
        {
            return EconomicCalculator.Analyze(cashFlows, discountRate, financeRate, reinvestRate);
        }

        public static System.Collections.Generic.List<NPV_PROFILE_POINT> GenerateNPVProfile(
            CashFlow[] cashFlows, double minRate = 0.0, double maxRate = 1.0, int points = 50)
        {
            return EconomicCalculator.GenerateNPVProfile(cashFlows, minRate, maxRate, points);
        }

        /// <summary>
        /// Performs one-way sensitivity analysis.
        /// </summary>
        public static SENSITIVITY_RESULT RunSensitivityAnalysis(
            CashFlow[] cashFlows,
            double discountRate,
            string parameterName,
            double variationPercent,
            Func<CashFlow, double, CashFlow> scalingFunction)
        {
            return SensitivityAnalysis.PerformOneWaySensitivity(
                cashFlows, discountRate, parameterName, variationPercent, scalingFunction);
        }

        /// <summary>
        /// Runs a Monte Carlo simulation for risk analysis.
        /// </summary>
        public static MONTE_CARLO_RESULT RunMonteCarloSimulation(
            CashFlow[] cashFlows,
            double discountRate,
            int iterations = 1000,
            double revenueVolatility = 0.2,
            double costVolatility = 0.1)
        {
            return MonteCarloSimulation.RunSimulation(
                cashFlows, discountRate, iterations, revenueVolatility, costVolatility);
        }

        /// <summary>
        /// Calculates break-even price.
        /// </summary>
        public static double CalculateBreakEvenPrice(
            CashFlow[] cashFlows,
            double currentPrice,
            double discountRate)
        {
            return BreakEvenAnalysis.CalculateBreakEvenPrice(cashFlows, currentPrice, discountRate);
        }
    }
}

