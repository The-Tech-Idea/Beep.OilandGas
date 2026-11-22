using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.CompressorAnalysis.Data
{
    /// <summary>
    /// The analysis columns of a compressor power result (LFT-08): the machine kind, the staging, the inlet and outlet
    /// conditions, the feasibility verdict with its reasons, the units and the actor.
    /// </summary>
    /// <remarks>
    /// The generated table held the power, the heads, the ratio and the efficiencies, which cannot satisfy AC-6 (the type,
    /// the staging and the efficiency with the result) or AC-3 (an infeasible verdict with every reason). These columns
    /// are additive, so an existing row stays valid.
    /// </remarks>
    public partial class COMPRESSOR_POWER_RESULT
    {
        /// <summary>CENTRIFUGAL or RECIPROCATING - from <c>CompressorAnalysisWellKnown.CompressorType</c> (AC-4).</summary>
        [MaxLength(16)]
        public string? KIND
        {
            get => KindValue;
            set => SetProperty(ref KindValue, value);
        }

        private string? KindValue;

        /// <summary>How many stages the result is for (AC-6).</summary>
        public int? STAGES
        {
            get => StagesValue;
            set => SetProperty(ref StagesValue, value);
        }

        /// <summary>The per-stage compression ratio the staging produced.</summary>
        public decimal? STAGE_RATIO
        {
            get => StageRatioValue;
            set => SetProperty(ref StageRatioValue, value);
        }

        /// <summary>The hottest stage discharge temperature the staging produced, in Rankine (AC-2).</summary>
        public decimal? MAX_STAGE_TEMPERATURE_R
        {
            get => MaxStageTemperatureValue;
            set => SetProperty(ref MaxStageTemperatureValue, value);
        }

        /// <summary>The interstage temperature limit it was judged against, in Rankine (AC-2).</summary>
        public decimal? STAGE_TEMPERATURE_LIMIT_R
        {
            get => StageTemperatureLimitValue;
            set => SetProperty(ref StageTemperatureLimitValue, value);
        }

        /// <summary>The largest per-stage ratio it was judged against.</summary>
        public decimal? STAGE_RATIO_LIMIT
        {
            get => StageRatioLimitValue;
            set => SetProperty(ref StageRatioLimitValue, value);
        }

        /// <summary>'Y' when the machine can meet the duty; 'N' when a reason says it cannot (AC-3).</summary>
        [MaxLength(1)]
        public string FEASIBLE_IND
        {
            get => FeasibleIndValue;
            set => SetProperty(ref FeasibleIndValue, value);
        }

        private string FeasibleIndValue = "N";

        /// <summary>Why it cannot, one per line - empty when it can (AC-3).</summary>
        public string? REASONS
        {
            get => ReasonsValue;
            set => SetProperty(ref ReasonsValue, value);
        }

        private string? ReasonsValue;

        /// <summary>The power figure is carried only when the machine is feasible, so a refusal never reads as an answer.</summary>
        [MaxLength(1)]
        public string POWER_ACHIEVED_IND
        {
            get => PowerAchievedIndValue;
            set => SetProperty(ref PowerAchievedIndValue, value);
        }

        private string PowerAchievedIndValue = "N";

        // ---- the inlet and outlet conditions the result was computed at ----

        public decimal? INLET_PRESSURE_PSIA
        {
            get => InletPressureValue;
            set => SetProperty(ref InletPressureValue, value);
        }

        private decimal? InletPressureValue;

        public decimal? OUTLET_PRESSURE_PSIA
        {
            get => OutletPressureValue;
            set => SetProperty(ref OutletPressureValue, value);
        }

        private decimal? OutletPressureValue;

        public decimal? INLET_TEMPERATURE_R
        {
            get => InletTemperatureValue;
            set => SetProperty(ref InletTemperatureValue, value);
        }

        private decimal? InletTemperatureValue;

        public decimal? GAS_FLOW_MSCFD
        {
            get => GasFlowValue;
            set => SetProperty(ref GasFlowValue, value);
        }

        private decimal? GasFlowValue;

        public decimal? GAS_SPECIFIC_GRAVITY
        {
            get => GasGravityValue;
            set => SetProperty(ref GasGravityValue, value);
        }

        private decimal? GasGravityValue;

        /// <summary>The surge margin, when the machine is centrifugal and the engine reports one.</summary>
        public decimal? SURGE_MARGIN
        {
            get => SurgeMarginValue;
            set => SetProperty(ref SurgeMarginValue, value);
        }

        private decimal? SurgeMarginValue;

        [MaxLength(16)]
        public string? HEAD_OUOM
        {
            get => HeadOuomValue;
            set => SetProperty(ref HeadOuomValue, value);
        }

        private string? HeadOuomValue;

        [MaxLength(16)]
        public string? POWER_OUOM
        {
            get => PowerOuomValue;
            set => SetProperty(ref PowerOuomValue, value);
        }

        private string? PowerOuomValue;

        [MaxLength(128)]
        public string? ACTOR
        {
            get => ActorValue;
            set => SetProperty(ref ActorValue, value);
        }

        private string? ActorValue;

        public DateTime? RECORDED_AT
        {
            get => RecordedAtValue;
            set => SetProperty(ref RecordedAtValue, value);
        }

        private DateTime? RecordedAtValue;
            private int? StagesValue;
        private decimal? StageRatioValue;
        private decimal? MaxStageTemperatureValue;
        private decimal? StageTemperatureLimitValue;
        private decimal? StageRatioLimitValue;
}
}
