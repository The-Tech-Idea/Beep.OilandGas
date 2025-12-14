using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.FieldManagement.DataMapping;
using Beep.OilandGas.NodalAnalysis;
using Beep.OilandGas.NodalAnalysis.Calculations;
using Beep.OilandGas.NodalAnalysis.Models;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.FieldManagement.Integration
{
    /// <summary>
    /// Service that provides data flow between PPDM39 entities and analysis modules.
    /// </summary>
    public class DataFlowService
    {
        private readonly NodalAnalysisMapper _nodalMapper;

        public DataFlowService()
        {
            _nodalMapper = new NodalAnalysisMapper();
        }

        /// <summary>
        /// Runs nodal analysis for a well using PPDM39 data.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="reservoirData">Optional reservoir data (if not provided, will be mapped from well).</param>
        /// <returns>The nodal analysis results including operating point and curves.</returns>
        public NodalAnalysisResult RunNodalAnalysis(
            WELL well,
            ReservoirProperties? reservoirData = null)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            // Map PPDM39 WELL to WellboreProperties
            var wellboreProperties = _nodalMapper.MapToDomain(well);

            // Map PPDM39 WELL to ReservoirProperties (or use provided)
            var reservoirProperties = reservoirData ?? 
                ((IPPDM39Mapper<WELL, ReservoirProperties>)_nodalMapper).MapToDomain(well);

            // Generate IPR curve
            var iprCurve = IPRCalculator.GenerateIPRCurve(
                (decimal)reservoirProperties.ReservoirPressure,
                (decimal)reservoirProperties.BubblePointPressure,
                (decimal)reservoirProperties.ProductivityIndex,
                (decimal)reservoirProperties.WaterCut,
                (decimal)reservoirProperties.GasOilRatio,
                (decimal)reservoirProperties.OilGravity,
                (decimal)reservoirProperties.FormationVolumeFactor,
                (decimal)reservoirProperties.OilViscosity,
                20 // Number of points
            );

            // Generate VLP curve
            var vlpCurve = VLPCalculator.GenerateVLPCurve(
                (decimal)wellboreProperties.TubingDiameter,
                (decimal)wellboreProperties.TubingLength,
                (decimal)wellboreProperties.WellheadPressure,
                (decimal)wellboreProperties.WaterCut,
                (decimal)wellboreProperties.GasOilRatio,
                (decimal)wellboreProperties.OilGravity,
                (decimal)wellboreProperties.GasSpecificGravity,
                (decimal)wellboreProperties.WellheadTemperature,
                (decimal)wellboreProperties.BottomholeTemperature,
                (decimal)wellboreProperties.TubingRoughness,
                20, // Number of points
                VLPCalculator.CorrelationType.HagedornBrown
            );

            // Find operating point
            var operatingPoint = OperatingPointCalculator.FindOperatingPoint(iprCurve, vlpCurve);

            return new NodalAnalysisResult
            {
                WellUWI = well.UWI,
                OperatingPoint = operatingPoint,
                IPRCurve = iprCurve,
                VLPCurve = vlpCurve,
                WellboreProperties = wellboreProperties,
                ReservoirProperties = reservoirProperties
            };
        }

        /// <summary>
        /// Stores analysis results in PPDM39 and returns the analysis report.
        /// </summary>
        /// <param name="nodalResult">The nodal analysis result.</param>
        /// <returns>The created ANL_ANALYSIS_REPORT entity.</returns>
        public ANL_ANALYSIS_REPORT StoreNodalAnalysisResult(NodalAnalysisResult nodalResult)
        {
            if (nodalResult == null)
                throw new ArgumentNullException(nameof(nodalResult));

            return AnalysisResultStorage.StoreNodalAnalysisResult(
                nodalResult.WellUWI,
                nodalResult.OperatingPoint,
                nodalResult.IPRCurve,
                nodalResult.VLPCurve,
                DateTime.Now
            );
        }
    }

    /// <summary>
    /// Result of nodal analysis including all curves and properties.
    /// </summary>
    public class NodalAnalysisResult
    {
        public string WellUWI { get; set; } = string.Empty;
        public OperatingPoint OperatingPoint { get; set; } = new();
        public List<IPRPoint> IPRCurve { get; set; } = new();
        public List<VLPPoint> VLPCurve { get; set; } = new();
        public WellboreProperties WellboreProperties { get; set; } = new();
        public ReservoirProperties ReservoirProperties { get; set; } = new();
    }
}

