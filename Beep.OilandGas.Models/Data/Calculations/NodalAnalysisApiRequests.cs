using System.Collections.Generic;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Request payload for nodal sensitivity analysis (economic scenarios).
    /// </summary>
    public class NodalSensitivityAnalysisRequest : ModelEntityBase
    {
        public string WellUWI { get; set; } = string.Empty;
        public NodalAnalysisParameters? BaselineParameters { get; set; }
        public List<string>? ParametersToVary { get; set; }
    }

    /// <summary>
    /// Request payload for artificial lift ranking / scoring.
    /// </summary>
    public class NodalArtificialLiftRequest : ModelEntityBase
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal CurrentProduction { get; set; }
        public decimal TargetProduction { get; set; }
        public decimal WellDepth { get; set; }
        public decimal WaterCut { get; set; }
    }

    /// <summary>
    /// Request payload for well performance diagnostics.
    /// </summary>
    public class NodalWellDiagnosticsRequest : ModelEntityBase
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal ExpectedProduction { get; set; }
        public decimal ActualProduction { get; set; }
        public decimal WellheadPressure { get; set; }
        public decimal BottomholePressure { get; set; }
    }

    /// <summary>
    /// Request payload for decline-based production forecast from nodal workflows.
    /// </summary>
    public class NodalProductionForecastRequest : ModelEntityBase
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal CurrentProduction { get; set; }
        public decimal DeclineRate { get; set; }
        public int ForecastMonths { get; set; }
    }

    /// <summary>
    /// Request payload for pressure maintenance strategy screening.
    /// </summary>
    public class NodalPressureMaintenanceRequest : ModelEntityBase
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal CurrentReservoirPressure { get; set; }
        public decimal BubblePointPressure { get; set; }
        public decimal ProductivityIndex { get; set; }
    }
}
