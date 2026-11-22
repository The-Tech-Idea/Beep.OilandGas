using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    /// <summary>
    /// The analysis columns of a pipeline result (WTA-03): the mode, the capacity with its unit, the regime and the Reynolds
    /// number, the erosion threshold with its source, the verdict, the notes and the actor.
    /// </summary>
    /// <remarks>
    /// The generated table held a flow rate, a pressure drop, a velocity, a Reynolds number, a friction factor and a regime
    /// but no mode, no capacity unit, no erosion threshold and no threshold source, which AC-2 and AC-6 require. These
    /// columns are additive, so an existing row stays valid.
    /// </remarks>
    public partial class PIPELINE_ANALYSIS_RESULT
    {
        /// <summary>GAS or LIQUID - which capacity relation produced the result (AC-6).</summary>
        [MaxLength(16)]
        public string? ANALYSIS_MODE
        {
            get => ModeValue;
            set => SetProperty(ref ModeValue, value);
        }

        private string? ModeValue;

        /// <summary>The maximum rate the line can carry, in <see cref="CAPACITY_OUOM"/>.</summary>
        public decimal? CAPACITY
        {
            get => CapacityValue;
            set => SetProperty(ref CapacityValue, value);
        }

        private decimal? CapacityValue;

        /// <summary>Mscf/d for gas and bbl/d for liquid, stored so the number cannot be read in the wrong unit.</summary>
        [MaxLength(16)]
        public string? CAPACITY_OUOM
        {
            get => CapacityOuomValue;
            set => SetProperty(ref CapacityOuomValue, value);
        }

        private string? CapacityOuomValue;

        /// <summary>The flow regime, from the module's own Reynolds thresholds (AC-3).</summary>
        [MaxLength(16)]
        public string? REGIME_CODE
        {
            get => RegimeValue;
            set => SetProperty(ref RegimeValue, value);
        }

        private string? RegimeValue;

        /// <summary>The sand erosion rate in mils per year, when a sand rate was given.</summary>
        public decimal? EROSION_RATE_MPY
        {
            get => ErosionRateValue;
            set => SetProperty(ref ErosionRateValue, value);
        }

        private decimal? ErosionRateValue;

        /// <summary>The erosional velocity limit, ft/s.</summary>
        public decimal? EROSION_THRESHOLD_FPS
        {
            get => ErosionThresholdValue;
            set => SetProperty(ref ErosionThresholdValue, value);
        }

        private decimal? ErosionThresholdValue;

        /// <summary>The actual velocity over the limit; above 1 exceeds it.</summary>
        public decimal? VELOCITY_RATIO
        {
            get => VelocityRatioValue;
            set => SetProperty(ref VelocityRatioValue, value);
        }

        private decimal? VelocityRatioValue;

        /// <summary>
        /// Where the threshold came from. AC-2: a limit is never stored without its source, and a result whose source is not
        /// configured says so here rather than leaving the column empty.
        /// </summary>
        [MaxLength(256)]
        public string? THRESHOLD_SOURCE
        {
            get => ThresholdSourceValue;
            set => SetProperty(ref ThresholdSourceValue, value);
        }

        private string? ThresholdSourceValue;

        /// <summary>'Y' when nothing failed: the regime was decided and the velocity is at or below the limit.</summary>
        [MaxLength(1)]
        public string PIPELINE_FEASIBLE_IND
        {
            get => FeasibleValue;
            set => SetProperty(ref FeasibleValue, value);
        }

        private string FeasibleValue = "N";

        /// <summary>The notes and refusals, one per line - empty when there is nothing to say.</summary>
        public string? PIPELINE_NOTES
        {
            get => NotesValue;
            set => SetProperty(ref NotesValue, value);
        }

        private string? NotesValue;

        // ---- the inputs, so a result can be re-run from its own row ----

        public decimal? GAS_SPECIFIC_GRAVITY
        {
            get => GasGravityValue;
            set => SetProperty(ref GasGravityValue, value);
        }

        private decimal? GasGravityValue;

        public decimal? LIQUID_SPECIFIC_GRAVITY
        {
            get => LiquidGravityValue;
            set => SetProperty(ref LiquidGravityValue, value);
        }

        private decimal? LiquidGravityValue;

        public decimal? LIQUID_VISCOSITY_CP
        {
            get => ViscosityValue;
            set => SetProperty(ref ViscosityValue, value);
        }

        private decimal? ViscosityValue;

        [MaxLength(16)]
        public string? TEMPERATURE_OUOM
        {
            get => TemperatureOuomValue;
            set => SetProperty(ref TemperatureOuomValue, value);
        }

        private string? TemperatureOuomValue;

        [MaxLength(16)]
        public string? EROSION_SERVICE
        {
            get => ErosionServiceValue;
            set => SetProperty(ref ErosionServiceValue, value);
        }

        private string? ErosionServiceValue;

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
    }
}
