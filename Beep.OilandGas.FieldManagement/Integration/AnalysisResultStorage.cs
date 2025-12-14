using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Beep.OilandGas.NodalAnalysis.Models;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.FieldManagement.Integration
{
    /// <summary>
    /// Service for storing analysis results in PPDM39 ANL_ANALYSIS_REPORT entities.
    /// </summary>
    public class AnalysisResultStorage
    {
        /// <summary>
        /// Stores nodal analysis results in PPDM39 ANL_ANALYSIS_REPORT.
        /// </summary>
        /// <param name="wellUWI">The well UWI.</param>
        /// <param name="operatingPoint">The operating point result.</param>
        /// <param name="iprCurve">The IPR curve points.</param>
        /// <param name="vlpCurve">The VLP curve points.</param>
        /// <param name="analysisDate">The analysis date.</param>
        /// <returns>The created ANL_ANALYSIS_REPORT entity.</returns>
        public static ANL_ANALYSIS_REPORT StoreNodalAnalysisResult(
            string wellUWI,
            OperatingPoint operatingPoint,
            List<IPRPoint> iprCurve,
            List<VLPPoint> vlpCurve,
            DateTime analysisDate)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty.", nameof(wellUWI));
            if (operatingPoint == null)
                throw new ArgumentNullException(nameof(operatingPoint));

            var analysisReport = new ANL_ANALYSIS_REPORT
            {
                ANALYSIS_ID = GenerateAnalysisId("NODAL", wellUWI, analysisDate),
                ANL_SOURCE = "NODAL_ANALYSIS",
                ANALYSIS_DATE = analysisDate,
                ANALYSIS_PURPOSE = "Well Performance Analysis",
                ACTIVE_IND = "Y",
                REMARK = $"Nodal Analysis for Well {wellUWI}"
            };

            // Store results as JSON in REMARK or create related entities
            var results = new
            {
                OperatingPoint = new
                {
                    FlowRate = operatingPoint.FlowRate,
                    BottomholePressure = operatingPoint.BottomholePressure,
                    WellheadPressure = operatingPoint.WellheadPressure
                },
                IPRCurve = iprCurve?.Select(p => new { p.FlowRate, p.FlowingBottomholePressure }),
                VLPCurve = vlpCurve?.Select(p => new { p.FlowRate, p.RequiredBottomholePressure })
            };

            analysisReport.REMARK = JsonSerializer.Serialize(results);

            return analysisReport;
        }

        /// <summary>
        /// Generates a unique analysis ID.
        /// </summary>
        /// <param name="analysisType">The type of analysis (e.g., "NODAL", "DCA").</param>
        /// <param name="wellUWI">The well UWI.</param>
        /// <param name="analysisDate">The analysis date.</param>
        /// <returns>A unique analysis ID.</returns>
        private static string GenerateAnalysisId(string analysisType, string wellUWI, DateTime analysisDate)
        {
            var dateStr = analysisDate.ToString("yyyyMMddHHmmss");
            return $"{analysisType}_{wellUWI}_{dateStr}";
        }
    }
}

