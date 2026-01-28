using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.PPDM39.Models;


namespace Beep.OilandGas.LifeCycle.Services.DataMapping
{
    /// <summary>
    /// Maps PPDM39 WELL_TEST entities to WellTestAnalysis models.
    /// </summary>
    public class WellTestAnalysisMapper
    {
        private readonly Func<WELL_TEST, WellTestType>? _getTestType;
        private readonly Func<IEnumerable<WELL_TEST_FLOW>?, double>? _getFlowRate;
        private readonly Func<WELL, WELL_TUBULAR?, double>? _getWellboreRadius;
        private readonly Func<WELL, double>? _getFormationThickness;
        private readonly Func<WELL, double>? _getPorosity;
        private readonly Func<WELL, double>? _getTotalCompressibility;
        private readonly Func<WELL, double>? _getOilViscosity;
        private readonly Func<WELL, double>? _getOilFormationVolumeFactor;
        private readonly Func<IEnumerable<WELL_TEST_FLOW>?, double>? _getProductionTime;
        private readonly Func<WELL, bool>? _isGasWell;
        private readonly Func<WELL, double>? _getGasSpecificGravity;
        private readonly Func<WELL, double>? _getReservoirTemperature;
        private readonly Func<IEnumerable<WELL_TEST_PRESSURE>?, double>? _getInitialReservoirPressure;

        /// <summary>
        /// Initializes a new instance of WellTestAnalysisMapper with default value retrievers.
        /// </summary>
        public WellTestAnalysisMapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of WellTestAnalysisMapper with custom value retrievers.
        /// </summary>
        public WellTestAnalysisMapper(
            Func<WELL_TEST, WellTestType>? getTestType = null,
            Func<IEnumerable<WELL_TEST_FLOW>?, double>? getFlowRate = null,
            Func<WELL, WELL_TUBULAR?, double>? getWellboreRadius = null,
            Func<WELL, double>? getFormationThickness = null,
            Func<WELL, double>? getPorosity = null,
            Func<WELL, double>? getTotalCompressibility = null,
            Func<WELL, double>? getOilViscosity = null,
            Func<WELL, double>? getOilFormationVolumeFactor = null,
            Func<IEnumerable<WELL_TEST_FLOW>?, double>? getProductionTime = null,
            Func<WELL, bool>? isGasWell = null,
            Func<WELL, double>? getGasSpecificGravity = null,
            Func<WELL, double>? getReservoirTemperature = null,
            Func<IEnumerable<WELL_TEST_PRESSURE>?, double>? getInitialReservoirPressure = null)
        {
            _getTestType = getTestType;
            _getFlowRate = getFlowRate;
            _getWellboreRadius = getWellboreRadius;
            _getFormationThickness = getFormationThickness;
            _getPorosity = getPorosity;
            _getTotalCompressibility = getTotalCompressibility;
            _getOilViscosity = getOilViscosity;
            _getOilFormationVolumeFactor = getOilFormationVolumeFactor;
            _getProductionTime = getProductionTime;
            _isGasWell = isGasWell;
            _getGasSpecificGravity = getGasSpecificGravity;
            _getReservoirTemperature = getReservoirTemperature;
            _getInitialReservoirPressure = getInitialReservoirPressure;
        }

        /// <summary>
        /// Maps PPDM39 WELL_TEST and related entities to WELL_TEST_DATA.
        /// </summary>
        /// <param name="wellTest">The PPDM39 WELL_TEST entity.</param>
        /// <param name="pressureData">Pressure data points from WELL_TEST_PRESSURE.</param>
        /// <param name="flowData">Flow data points from WELL_TEST_FLOW.</param>
        /// <param name="well">Optional WELL entity for well properties.</param>
        /// <param name="tubular">Optional WELL_TUBULAR for wellbore radius.</param>
        /// <returns>The mapped WELL_TEST_DATA.</returns>
        public WELL_TEST_DATA MapToWellTestData(
            WELL_TEST wellTest,
            IEnumerable<WELL_TEST_PRESSURE>? pressureData = null,
            IEnumerable<WELL_TEST_FLOW>? flowData = null,
            WELL? well = null,
            WELL_TUBULAR? tubular = null)
        {
            if (wellTest == null)
                throw new ArgumentNullException(nameof(wellTest));

            var getTestType = _getTestType ?? DetermineTestType;
            var getFlowRate = _getFlowRate ?? GetFlowRate;
            var getWellboreRadius = _getWellboreRadius ?? ValueRetrievers.GetWellboreRadius;
            var getFormationThickness = _getFormationThickness ?? ((w) => throw new InvalidOperationException("Formation thickness not available. Provide getFormationThickness function."));
            var getPorosity = _getPorosity ?? ((w) => throw new InvalidOperationException("Porosity not available. Provide getPorosity function."));
            var getTotalCompressibility = _getTotalCompressibility ?? ((w) => throw new InvalidOperationException("Total compressibility not available. Provide getTotalCompressibility function."));
            var getOilViscosity = _getOilViscosity ?? ValueRetrievers.GetOilViscosity;
            var getOilFormationVolumeFactor = _getOilFormationVolumeFactor ?? ValueRetrievers.GetFormationVolumeFactor;
            var getProductionTime = _getProductionTime ?? GetProductionTime;
            var isGasWell = _isGasWell ?? IsGasWellType;
            var getGasSpecificGravity = _getGasSpecificGravity ?? ValueRetrievers.GetGasSpecificGravity;
            var getReservoirTemperature = _getReservoirTemperature ?? ValueRetrievers.GetReservoirTemperatureFahrenheit;
            var getInitialReservoirPressure = _getInitialReservoirPressure ?? GetInitialReservoirPressure;

            var WELL_TEST_DATA = new WELL_TEST_DATA
            {
                TestType = getTestType(wellTest),
                FlowRate = getFlowRate(flowData),
                WellboreRadius = well != null ? getWellboreRadius(well, tubular) : throw new ArgumentNullException(nameof(well), "WELL entity is required for wellbore radius."),
                FormationThickness = well != null ? getFormationThickness(well) : throw new ArgumentNullException(nameof(well), "WELL entity is required for formation thickness."),
                Porosity = well != null ? getPorosity(well) : throw new ArgumentNullException(nameof(well), "WELL entity is required for porosity."),
                TotalCompressibility = well != null ? getTotalCompressibility(well) : throw new ArgumentNullException(nameof(well), "WELL entity is required for total compressibility."),
                OilViscosity = well != null ? getOilViscosity(well) : throw new ArgumentNullException(nameof(well), "WELL entity is required for oil viscosity."),
                OilFormationVolumeFactor = well != null ? getOilFormationVolumeFactor(well) : throw new ArgumentNullException(nameof(well), "WELL entity is required for oil formation volume factor."),
                ProductionTime = getProductionTime(flowData),
                IsGasWell = well != null ? isGasWell(well) : false,
                GasSpecificGravity = well != null ? getGasSpecificGravity(well) : 0.65,
                ReservoirTemperature = well != null ? getReservoirTemperature(well) : 200.0,
                InitialReservoirPressure = getInitialReservoirPressure(pressureData)
            };

            // Map pressure and time data from WELL_TEST_PRESSURE
            if (pressureData != null && pressureData.Any())
            {
                var sortedPressure = pressureData.OrderBy(p => p.EFFECTIVE_DATE).ToList();
                var startDate = sortedPressure.First().EFFECTIVE_DATE;
                
                WELL_TEST_DATA.Time = sortedPressure.Select(p => 
                    {
                         DateTime effDate = p.EFFECTIVE_DATE ?? DateTime.MinValue;
                         DateTime start = startDate ?? DateTime.MinValue;
                         return (effDate - start).TotalHours;
                    }).ToList();
                
                // Use START_PRESSURE or END_PRESSURE based on test type
                WELL_TEST_DATA.Pressure = sortedPressure.Select(p => 
                    (double)(p.START_PRESSURE > 0 ? p.START_PRESSURE : p.END_PRESSURE)).ToList();
            }

            return WELL_TEST_DATA;
        }

        /// <summary>
        /// Determines the test type from PPDM39 WELL_TEST.TEST_TYPE.
        /// </summary>
        private static WellTestType DetermineTestType(WELL_TEST wellTest)
        {
            if (string.IsNullOrWhiteSpace(wellTest.TEST_TYPE))
                throw new InvalidOperationException("Test type not available. Provide getTestType function or ensure WELL_TEST.TEST_TYPE is set.");

            var testType = wellTest.TEST_TYPE.ToUpper();
            if (testType.Contains("BUILD") || testType.Contains("BU") || testType.Contains("SHUTIN"))
                return WellTestType.BuildUp;
            
            if (testType.Contains("DRAWDOWN") || testType.Contains("DD") || testType.Contains("FLOW"))
                return WellTestType.Drawdown;
            
            throw new InvalidOperationException($"Unknown test type: {wellTest.TEST_TYPE}. Provide getTestType function.");
        }

        /// <summary>
        /// Gets the flow rate from WELL_TEST_FLOW.MAX_FLUID_RATE.
        /// </summary>
        private static double GetFlowRate(IEnumerable<WELL_TEST_FLOW>? flowData)
        {
            if (flowData != null && flowData.Any())
            {
                var maxFlow = flowData.Max(f => f.MAX_FLUID_RATE);
                if (maxFlow > 0)
                    return (double)maxFlow;
            }
            
            throw new InvalidOperationException("Flow rate not available. Provide getFlowRate function or WELL_TEST_FLOW data.");
        }

        /// <summary>
        /// Gets production time from WELL_TEST_FLOW.TTS_TIME_ELAPSED.
        /// </summary>
        private static double GetProductionTime(IEnumerable<WELL_TEST_FLOW>? flowData)
        {
            if (flowData != null && flowData.Any())
            {
                var totalTime = flowData.Sum(f => f.TTS_TIME_ELAPSED);
                if (totalTime > 0)
                {
                    // Convert to hours if needed (check OUOM)
                    var timeInHours = (double)totalTime;
                    var firstFlow = flowData.First();
                    if (firstFlow.TTS_TIME_ELAPSED_OUOM?.ToUpper() == "DAY" || firstFlow.TTS_TIME_ELAPSED_OUOM?.ToUpper() == "D")
                    {
                        timeInHours *= 24.0;
                    }
                    return timeInHours;
                }
            }
            
            throw new InvalidOperationException("Production time not available. Provide getProductionTime function or WELL_TEST_FLOW data.");
        }

        /// <summary>
        /// Determines if well is a gas well from WELL.WELL_TYPE.
        /// </summary>
        private static bool IsGasWellType(WELL well)
        {
            if (well != null && !string.IsNullOrWhiteSpace(well.PROFILE_TYPE))
            {
                var wellType = well.PROFILE_TYPE.ToUpper();
                return wellType.Contains("GAS") || wellType == "G";
            }
            
            return false; // Default to oil well
        }

        /// <summary>
        /// Gets initial reservoir pressure from WELL_TEST_PRESSURE.
        /// </summary>
        private static double GetInitialReservoirPressure(IEnumerable<WELL_TEST_PRESSURE>? pressureData)
        {
            if (pressureData != null && pressureData.Any())
            {
                // Use START_PRESSURE from first pressure point
                var firstPressure = pressureData.OrderBy(p => p.EFFECTIVE_DATE).First();
                if (firstPressure.START_PRESSURE > 0)
                    return (double)firstPressure.START_PRESSURE;
            }
            
            throw new InvalidOperationException("Initial reservoir pressure not available. Provide getInitialReservoirPressure function or WELL_TEST_PRESSURE data.");
        }
    }
}

