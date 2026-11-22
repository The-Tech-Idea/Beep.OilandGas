using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.PumpPerformance
{
    /// <summary>
    /// One pump duty-point analysis (LFT-03): the pump, the curve it was checked against, the duty point, the NPSH margin,
    /// the affinity result and the actor.
    /// </summary>
    /// <remarks>
    /// The ESP tables in this namespace describe an ESP string (motor, cable, stages). A duty-point check on a published
    /// H-Q curve is a different record - it may be a centrifugal, jet or positive-displacement pump and it carries the
    /// curve it was judged against - so it is its own table rather than a fourth meaning for ESP_DESIGN_RESULT.
    /// </remarks>
    public class PUMP_DESIGN : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string DESIGN_ID
        {
            get => DesignIdValue;
            set => SetProperty(ref DesignIdValue, value);
        }

        private string DesignIdValue = string.Empty;

        [MaxLength(128)]
        public string? WELL_UWI
        {
            get => WellUwiValue;
            set => SetProperty(ref WellUwiValue, value);
        }

        public DateTime? DESIGN_DATE
        {
            get => DesignDateValue;
            set => SetProperty(ref DesignDateValue, value);
        }

        /// <summary>ESP, CENTRIFUGAL, POSITIVE_DISPLACEMENT or JET.</summary>
        [MaxLength(32)]
        public string PUMP_TYPE
        {
            get => PumpTypeValue;
            set => SetProperty(ref PumpTypeValue, value);
        }

        /// <summary>Where the curve came from - ENTERED when the engineer typed its points (AC-6).</summary>
        [MaxLength(32)]
        public string CURVE_SOURCE
        {
            get => CurveSourceValue;
            set => SetProperty(ref CurveSourceValue, value);
        }

        /// <summary>The published curve, one rate/head/power triple per line, so the check can be repeated.</summary>
        public string? CURVE_POINTS
        {
            get => CurvePointsValue;
            set => SetProperty(ref CurvePointsValue, value);
        }

        public int STAGES
        {
            get => StagesValue;
            set => SetProperty(ref StagesValue, value);
        }

        public decimal DUTY_RATE
        {
            get => DutyRateValue;
            set => SetProperty(ref DutyRateValue, value);
        }

        public decimal DUTY_HEAD
        {
            get => DutyHeadValue;
            set => SetProperty(ref DutyHeadValue, value);
        }

        /// <summary>The head of the whole string: the per-stage head times the stages.</summary>
        public decimal TOTAL_HEAD
        {
            get => TotalHeadValue;
            set => SetProperty(ref TotalHeadValue, value);
        }

        /// <summary>0 to 1.</summary>
        public decimal EFFICIENCY
        {
            get => EfficiencyValue;
            set => SetProperty(ref EfficiencyValue, value);
        }

        public decimal BRAKE_HORSEPOWER
        {
            get => BrakeHorsepowerValue;
            set => SetProperty(ref BrakeHorsepowerValue, value);
        }

        public decimal POWER_KW
        {
            get => PowerKwValue;
            set => SetProperty(ref PowerKwValue, value);
        }

        public decimal NPSH_AVAILABLE
        {
            get => NpshAvailableValue;
            set => SetProperty(ref NpshAvailableValue, value);
        }

        private decimal NpshAvailableValue;

        public decimal NPSH_REQUIRED
        {
            get => NpshRequiredValue;
            set => SetProperty(ref NpshRequiredValue, value);
        }

        private decimal NpshRequiredValue;

        public decimal NPSH_MARGIN
        {
            get => NpshMarginValue;
            set => SetProperty(ref NpshMarginValue, value);
        }

        private decimal NpshMarginValue;

        /// <summary>The minimum margin the module states, from PumpConstants (AC-2).</summary>
        public decimal NPSH_MINIMUM_MARGIN
        {
            get => NpshMinimumMarginValue;
            set => SetProperty(ref NpshMinimumMarginValue, value);
        }

        private decimal NpshMinimumMarginValue;

        /// <summary>'Y' when the margin is below the module's minimum.</summary>
        [MaxLength(1)]
        public string NPSH_WARNING_IND
        {
            get => NpshWarningIndValue;
            set => SetProperty(ref NpshWarningIndValue, value);
        }

        private string NpshWarningIndValue = "N";

        /// <summary>'Y' when the duty point lies inside the published curve; only such a design is saved (AC-1).</summary>
        [MaxLength(1)]
        public string OPERATING_POINT_VALID
        {
            get => OperatingPointValidValue;
            set => SetProperty(ref OperatingPointValidValue, value);
        }

        private string OperatingPointValidValue = "N";

        [MaxLength(16)]
        public string? RATE_OUOM
        {
            get => RateOuomValue;
            set => SetProperty(ref RateOuomValue, value);
        }

        private string? RateOuomValue;

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

        /// <summary>The affinity result, and whether it left the published range (AC-3).</summary>
        [MaxLength(512)]
        public string? AFFINITY_TEXT
        {
            get => AffinityTextValue;
            set => SetProperty(ref AffinityTextValue, value);
        }

        private string? AffinityTextValue;

        [MaxLength(1)]
        public string EXTRAPOLATED_IND
        {
            get => ExtrapolatedIndValue;
            set => SetProperty(ref ExtrapolatedIndValue, value);
        }

        private string ExtrapolatedIndValue = "N";

        public string? NOTES
        {
            get => NotesValue;
            set => SetProperty(ref NotesValue, value);
        }

        private string? NotesValue;

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
            private string? WellUwiValue;
        private DateTime? DesignDateValue;
        private string PumpTypeValue;
        private string CurveSourceValue;
        private string? CurvePointsValue;
        private int StagesValue;
        private decimal DutyRateValue;
        private decimal DutyHeadValue;
        private decimal TotalHeadValue;
        private decimal EfficiencyValue;
        private decimal BrakeHorsepowerValue;
        private decimal PowerKwValue;
}
}
