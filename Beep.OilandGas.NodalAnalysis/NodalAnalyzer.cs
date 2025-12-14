using Beep.OilandGas.NodalAnalysis.Calculations;
using Beep.OilandGas.NodalAnalysis.Models;

namespace Beep.OilandGas.NodalAnalysis
{
    /// <summary>
    /// Main analyzer class for nodal analysis.
    /// </summary>
    public static class NodalAnalyzer
    {
        /// <summary>
        /// Generates IPR curve using Vogel method.
        /// </summary>
        public static System.Collections.Generic.List<IPRPoint> GenerateVogelIPR(
            ReservoirProperties reservoir, double maxFlowRate = 5000, int points = 50)
        {
            return IPRCalculator.GenerateVogelIPR(reservoir, maxFlowRate, points);
        }

        /// <summary>
        /// Generates IPR curve using Fetkovich method.
        /// </summary>
        public static System.Collections.Generic.List<IPRPoint> GenerateFetkovichIPR(
            ReservoirProperties reservoir,
            System.Collections.Generic.List<(double flowRate, double pressure)> testPoints,
            double maxFlowRate = 5000, int points = 50)
        {
            return IPRCalculator.GenerateFetkovichIPR(reservoir, testPoints, maxFlowRate, points);
        }

        /// <summary>
        /// Generates VLP curve.
        /// </summary>
        public static System.Collections.Generic.List<VLPPoint> GenerateVLP(
            WellboreProperties wellbore, double[] flowRates)
        {
            return VLPCalculator.GenerateVLP(wellbore, flowRates);
        }

        /// <summary>
        /// Finds the operating point.
        /// </summary>
        public static OperatingPoint FindOperatingPoint(
            System.Collections.Generic.List<IPRPoint> iprCurve,
            System.Collections.Generic.List<VLPPoint> vlpCurve)
        {
            return Calculations.NodalAnalyzer.FindOperatingPoint(iprCurve, vlpCurve);
        }
    }
}

