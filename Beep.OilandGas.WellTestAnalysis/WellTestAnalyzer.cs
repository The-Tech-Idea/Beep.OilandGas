using Beep.OilandGas.WellTestAnalysis.Calculations;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
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
        public static WELL_TEST_ANALYSIS_RESULT AnalyzeBuildUp(WELL_TEST_DATA data)
        {
            WellTestDataValidator.Validate(data);
            return BuildUpAnalysis.AnalyzeHorner(data);
        }

        /// <summary>
        /// Analyzes a build-up test using MDH method.
        /// </summary>
        public static WELL_TEST_ANALYSIS_RESULT AnalyzeBuildUpMDH(WELL_TEST_DATA data)
        {
            WellTestDataValidator.Validate(data);
            return BuildUpAnalysis.AnalyzeMDH(data);
        }

        /// <summary>
        /// Calculates pressure derivative for diagnostic plots.
        /// </summary>
        public static System.Collections.Generic.List<PRESSURE_TIME_POINT> CalculateDerivative(
            System.Collections.Generic.List<PRESSURE_TIME_POINT> data, 
            double smoothingFactor = Constants.WellTestConstants.DefaultDerivativeSmoothing)
        {
            return DerivativeAnalysis.CalculateDerivative(data, smoothingFactor);
        }

        /// <summary>
        /// Analyzes a drawdown test (constant rate).
        /// </summary>
        public static WELL_TEST_ANALYSIS_RESULT AnalyzeDrawdown(WELL_TEST_DATA data)
        {
            WellTestDataValidator.Validate(data);
            return DrawdownAnalysis.AnalyzeDrawdown(data);
        }

        /// <summary>
        /// Analyzes a gas well build-up test using Pseudo-Pressure m(p) method.
        /// </summary>
        /// <param name="data">Well test data.</param>
        /// <param name="gasGravity">Gas specific gravity (Air=1.0).</param>
        /// <param name="reservoirTemperature">Reservoir temperature in Rankin.</param>
        /// <param name="n2">Mole fraction of Nitrogen (optional).</param>
        /// <param name="co2">Mole fraction of CO2 (optional).</param>
        /// <param name="h2s">Mole fraction of H2S (optional).</param>
        public static WELL_TEST_ANALYSIS_RESULT AnalyzeGasBuildUp(
            WELL_TEST_DATA data, 
            double gasGravity, 
            double reservoirTemperature,
            double n2 = 0,
            double co2 = 0,
            double h2s = 0)
        {
            WellTestDataValidator.Validate(data);
            return GasWellAnalysis.AnalyzeGasBuildUp(data, gasGravity, reservoirTemperature, n2, co2, h2s);
        }

        /// <summary>
        /// Identifies reservoir model from derivative signature.
        /// </summary>
        public static ReservoirModel IdentifyReservoirModel(
            System.Collections.Generic.List<PRESSURE_TIME_POINT> derivativeData)
        {
            return DerivativeAnalysis.IdentifyModel(derivativeData);
        }
    }
}

