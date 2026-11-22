using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ChokeAnalysis
{
    /// <summary>
    /// The analysis columns of a choke flow result (LFT-07): the correlation chosen and the one actually used, the choke,
    /// the inputs, the erosion assessment and the actor.
    /// </summary>
    /// <remarks>
    /// The generated table held the rate, the pressures, the pressure ratios and the regime, which cannot satisfy AC-6 (the
    /// correlation and the regime with the result) or AC-3 (an erosion threshold that names its source). These columns are
    /// additive, so an existing row stays valid.
    /// </remarks>
    public partial class CHOKE_FLOW_RESULT
    {
        /// <summary>The correlation the engineer asked for.</summary>
        [MaxLength(32)]
        public string? CORRELATION_CHOSEN
        {
            get => CorrelationChosenValue;
            set => SetProperty(ref CorrelationChosenValue, value);
        }

        private string? CorrelationChosenValue;

        /// <summary>
        /// The correlation the module actually evaluated. It differs from <see cref="CORRELATION_CHOSEN"/> when the module
        /// has no model for the chosen one and falls back to the Gilbert family - the state table's "correlation chosen but
        /// out of range", which must be marked rather than mislabelled.
        /// </summary>
        [MaxLength(32)]
        public string? CORRELATION_USED
        {
            get => CorrelationUsedValue;
            set => SetProperty(ref CorrelationUsedValue, value);
        }

        private string? CorrelationUsedValue;

        public decimal CHOKE_DIAMETER_IN
        {
            get => ChokeDiameterValue;
            set => SetProperty(ref ChokeDiameterValue, value);
        }

        private decimal ChokeDiameterValue;

        /// <summary>The bean size in 64ths of an inch, which is how a choke is specified in the field.</summary>
        public decimal? CHOKE_SIZE_64THS
        {
            get => ChokeSizeValue;
            set => SetProperty(ref ChokeSizeValue, value);
        }

        private decimal? ChokeSizeValue;

        /// <summary>BEAN, ADJUSTABLE, POSITIVE, CAGE, NEEDLE, PLUG or UNKNOWN - from <c>ChokeType</c> (AC-4).</summary>
        [MaxLength(16)]
        public string? CHOKE_TYPE
        {
            get => ChokeTypeValue;
            set => SetProperty(ref ChokeTypeValue, value);
        }

        private string? ChokeTypeValue;

        public decimal? DISCHARGE_COEFFICIENT
        {
            get => DischargeCoefficientValue;
            set => SetProperty(ref DischargeCoefficientValue, value);
        }

        private decimal? DischargeCoefficientValue;

        // ---- the inputs, so a result can be re-run from its own row ----

        public decimal? GAS_SPECIFIC_GRAVITY
        {
            get => GasGravityValue;
            set => SetProperty(ref GasGravityValue, value);
        }

        private decimal? GasGravityValue;

        public decimal? TEMPERATURE_F
        {
            get => TemperatureValue;
            set => SetProperty(ref TemperatureValue, value);
        }

        private decimal? TemperatureValue;

        public decimal? Z_FACTOR
        {
            get => ZFactorValue;
            set => SetProperty(ref ZFactorValue, value);
        }

        private decimal? ZFactorValue;

        public decimal? LIQUID_RATE_BBLD
        {
            get => LiquidRateValue;
            set => SetProperty(ref LiquidRateValue, value);
        }

        private decimal? LiquidRateValue;

        public decimal? GAS_RATE_MSCFD
        {
            get => GasRateValue;
            set => SetProperty(ref GasRateValue, value);
        }

        private decimal? GasRateValue;

        public decimal? GLR_SCF_BBL
        {
            get => GlrValue;
            set => SetProperty(ref GlrValue, value);
        }

        private decimal? GlrValue;

        // ---- the erosion assessment (AC-3) ----

        /// <summary>The mixture velocity through the throat, ft/s.</summary>
        public decimal? EROSION_VELOCITY_FPS
        {
            get => ErosionVelocityValue;
            set => SetProperty(ref ErosionVelocityValue, value);
        }

        private decimal? ErosionVelocityValue;

        /// <summary>The erosional velocity limit, ft/s.</summary>
        public decimal? EROSION_LIMIT_FPS
        {
            get => ErosionLimitValue;
            set => SetProperty(ref ErosionLimitValue, value);
        }

        private decimal? ErosionLimitValue;

        /// <summary>Velocity over limit; above 1 exceeds the threshold.</summary>
        public decimal? EROSION_RATIO
        {
            get => ErosionRatioValue;
            set => SetProperty(ref ErosionRatioValue, value);
        }

        private decimal? ErosionRatioValue;

        /// <summary>LOW, MODERATE, HIGH or SEVERE - from the module's <c>ErosionRisk</c>.</summary>
        [MaxLength(16)]
        public string? EROSION_RISK
        {
            get => ErosionRiskValue;
            set => SetProperty(ref ErosionRiskValue, value);
        }

        private string? ErosionRiskValue;

        /// <summary>
        /// Where the threshold came from. AC-3: a threshold is never stored without its source, so this is written
        /// whenever a limit is.
        /// </summary>
        [MaxLength(128)]
        public string? THRESHOLD_SOURCE
        {
            get => ThresholdSourceValue;
            set => SetProperty(ref ThresholdSourceValue, value);
        }

        private string? ThresholdSourceValue;

        /// <summary>True when any warning was raised (a fallback correlation, erosion above the limit).</summary>
        [MaxLength(1)]
        public string WARNING_IND
        {
            get => WarningIndValue;
            set => SetProperty(ref WarningIndValue, value);
        }

        private string WarningIndValue = "N";

        /// <summary>The warnings and notes, one per line.</summary>
        public string? NOTES
        {
            get => NotesValue;
            set => SetProperty(ref NotesValue, value);
        }

        private string? NotesValue;

        [MaxLength(16)]
        public string? RATE_OUOM
        {
            get => RateOuomValue;
            set => SetProperty(ref RateOuomValue, value);
        }

        private string? RateOuomValue;

        [MaxLength(16)]
        public string? PRESSURE_OUOM
        {
            get => PressureOuomValue;
            set => SetProperty(ref PressureOuomValue, value);
        }

        private string? PressureOuomValue;

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
