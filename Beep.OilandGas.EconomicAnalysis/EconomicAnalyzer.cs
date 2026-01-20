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

        /// <summary>
        /// Generates NPV profile.
        /// </summary>
        public static System.Collections.Generic.List<NPVProfilePoint> GenerateNPVProfile(
            CashFlow[] cashFlows, double minRate = 0.0, double maxRate = 1.0, int points = 50)
        {
            return EconomicCalculator.GenerateNPVProfile(cashFlows, minRate, maxRate, points);
        }
    }
}

