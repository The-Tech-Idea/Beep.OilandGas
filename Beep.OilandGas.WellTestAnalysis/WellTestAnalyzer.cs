using Beep.OilandGas.WellTestAnalysis.Calculations;
using Beep.OilandGas.WellTestAnalysis.Models;
using Beep.OilandGas.WellTestAnalysis.Validation;

namespace Beep.OilandGas.WellTestAnalysis
{
    /// <summary>
    /// Main analyzer class for well test analysis.
    /// </summary>
    public static class WellTestAnalyzer
    {
        /// <summary>
        /// Analyzes a build-up test using Horner method.
        /// </summary>
        public static WellTestAnalysisResult AnalyzeBuildUp(WellTestData data)
        {
            WellTestDataValidator.Validate(data);
            return BuildUpAnalysis.AnalyzeHorner(data);
        }

        /// <summary>
        /// Analyzes a build-up test using MDH method.
        /// </summary>
        public static WellTestAnalysisResult AnalyzeBuildUpMDH(WellTestData data)
        {
            WellTestDataValidator.Validate(data);
            return BuildUpAnalysis.AnalyzeMDH(data);
        }

        /// <summary>
        /// Calculates pressure derivative for diagnostic plots.
        /// </summary>
        public static System.Collections.Generic.List<PressureTimePoint> CalculateDerivative(
            System.Collections.Generic.List<PressureTimePoint> data, 
            double smoothingFactor = Constants.WellTestConstants.DefaultDerivativeSmoothing)
        {
            return DerivativeAnalysis.CalculateDerivative(data, smoothingFactor);
        }

        /// <summary>
        /// Identifies reservoir model from derivative signature.
        /// </summary>
        public static ReservoirModel IdentifyReservoirModel(
            System.Collections.Generic.List<PressureTimePoint> derivativeData)
        {
            return DerivativeAnalysis.IdentifyModel(derivativeData);
        }
    }
}

