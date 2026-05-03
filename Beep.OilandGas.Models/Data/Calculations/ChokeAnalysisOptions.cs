using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeAnalysisOptions : ModelEntityBase
    {
        /// <summary>
        /// Optional downstream/upstream critical pressure ratio threshold in (0,1) for regime labeling on the empirical multiphase orchestration path only.
        /// </summary>
        public decimal? CriticalPressureRatioOverride { get; set; }

        /// <summary>
        /// Correlation selection (<c>GAS_SINGLE_PHASE</c>, <c>GILBERT</c>, <c>ROS</c>, …). Omit or use <c>GAS_SINGLE_PHASE</c> for single-phase gas when upstream and downstream pressures are supplied.
        /// </summary>
        public string? CorrelationMethod { get; set; }
    }
}
