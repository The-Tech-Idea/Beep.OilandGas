using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    /// <summary>
    /// One sucker rod design (LFT-04): the pump and stroke, the rod string with its grade and taper, the loads, the stress
    /// and safety factor, the pump card, the rate and power, and the actor.
    /// </summary>
    /// <remarks>
    /// The sucker-rod entities in this namespace are calculation types (system properties, string, load result, flow-rate
    /// result, card) and none of them is a table, so a design had nowhere to persist. This is that record; it stores the
    /// numbers and the taper rather than pointers, so a saved design can be read without re-running the engine.
    /// </remarks>
    public class SRP_DESIGN : ModelEntityBase
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

        private string? WellUwiValue;

        public DateTime? DESIGN_DATE
        {
            get => DesignDateValue;
            set => SetProperty(ref DesignDateValue, value);
        }

        private DateTime? DesignDateValue;

        public decimal PUMP_DIAMETER_IN
        {
            get => PumpDiameterValue;
            set => SetProperty(ref PumpDiameterValue, value);
        }

        private decimal PumpDiameterValue;

        public decimal STROKE_LENGTH_IN
        {
            get => StrokeLengthValue;
            set => SetProperty(ref StrokeLengthValue, value);
        }

        private decimal StrokeLengthValue;

        public decimal STROKES_PER_MINUTE
        {
            get => SpmValue;
            set => SetProperty(ref SpmValue, value);
        }

        private decimal SpmValue;

        public decimal PUMP_DEPTH_FT
        {
            get => PumpDepthValue;
            set => SetProperty(ref PumpDepthValue, value);
        }

        private decimal PumpDepthValue;

        /// <summary>C, D, HL or HY - from <c>SuckerRodConstants</c> (AC-2).</summary>
        [MaxLength(8)]
        public string GRADE
        {
            get => GradeValue;
            set => SetProperty(ref GradeValue, value);
        }

        private string GradeValue = string.Empty;

        /// <summary>The taper, one <c>diameter,length</c> pair per line (in and ft), so the string can be read back (AC-6).</summary>
        public string? TAPER
        {
            get => TaperValue;
            set => SetProperty(ref TaperValue, value);
        }

        private string? TaperValue;

        public int SECTION_COUNT
        {
            get => SectionCountValue;
            set => SetProperty(ref SectionCountValue, value);
        }

        private int SectionCountValue;

        public decimal TOTAL_LENGTH_FT
        {
            get => TotalLengthValue;
            set => SetProperty(ref TotalLengthValue, value);
        }

        private decimal TotalLengthValue;

        public decimal PEAK_LOAD_LB
        {
            get => PeakLoadValue;
            set => SetProperty(ref PeakLoadValue, value);
        }

        private decimal PeakLoadValue;

        public decimal MIN_LOAD_LB
        {
            get => MinLoadValue;
            set => SetProperty(ref MinLoadValue, value);
        }

        private decimal MinLoadValue;

        public decimal FLUID_LOAD_LB
        {
            get => FluidLoadValue;
            set => SetProperty(ref FluidLoadValue, value);
        }

        private decimal FluidLoadValue;

        public decimal MAX_STRESS_PSI
        {
            get => MaxStressValue;
            set => SetProperty(ref MaxStressValue, value);
        }

        private decimal MaxStressValue;

        public decimal STRESS_RANGE_PSI
        {
            get => StressRangeValue;
            set => SetProperty(ref StressRangeValue, value);
        }

        private decimal StressRangeValue;

        /// <summary>The minimum safety factor across the sections (AC-6).</summary>
        public decimal MIN_SAFETY_FACTOR
        {
            get => MinSafetyFactorValue;
            set => SetProperty(ref MinSafetyFactorValue, value);
        }

        private decimal MinSafetyFactorValue;

        /// <summary>The threshold the design was judged against, from the module's constants.</summary>
        public decimal REQUIRED_SAFETY_FACTOR
        {
            get => RequiredSafetyFactorValue;
            set => SetProperty(ref RequiredSafetyFactorValue, value);
        }

        private decimal RequiredSafetyFactorValue;

        /// <summary>The modified-Goodman ratio of the critical section; above 1 predicts fatigue failure.</summary>
        public decimal GOODMAN_RATIO
        {
            get => GoodmanRatioValue;
            set => SetProperty(ref GoodmanRatioValue, value);
        }

        private decimal GoodmanRatioValue;

        /// <summary>The critical section's order (1 = topmost) - the section AC-1 names when it refuses.</summary>
        public int CRITICAL_SECTION
        {
            get => CriticalSectionValue;
            set => SetProperty(ref CriticalSectionValue, value);
        }

        private int CriticalSectionValue;

        [MaxLength(32)]
        public string? RISK
        {
            get => RiskValue;
            set => SetProperty(ref RiskValue, value);
        }

        private string? RiskValue;

        public decimal PRODUCTION_RATE_BPD
        {
            get => ProductionRateValue;
            set => SetProperty(ref ProductionRateValue, value);
        }

        private decimal ProductionRateValue;

        public decimal MOTOR_HORSEPOWER
        {
            get => MotorHorsepowerValue;
            set => SetProperty(ref MotorHorsepowerValue, value);
        }

        private decimal MotorHorsepowerValue;

        public decimal CARD_PEAK_LB
        {
            get => CardPeakValue;
            set => SetProperty(ref CardPeakValue, value);
        }

        private decimal CardPeakValue;

        public decimal CARD_MIN_LB
        {
            get => CardMinValue;
            set => SetProperty(ref CardMinValue, value);
        }

        private decimal CardMinValue;

        public int CARD_POINTS
        {
            get => CardPointsValue;
            set => SetProperty(ref CardPointsValue, value);
        }

        private int CardPointsValue;

        /// <summary>The units of the stored numbers, stated once (AC-5).</summary>
        [MaxLength(128)]
        public string? UNITS_TEXT
        {
            get => UnitsTextValue;
            set => SetProperty(ref UnitsTextValue, value);
        }

        private string? UnitsTextValue;

        /// <summary>'Y' when the default steel density was accepted rather than entered (the state table).</summary>
        [MaxLength(1)]
        public string DENSITY_ASSUMED_IND
        {
            get => DensityAssumedIndValue;
            set => SetProperty(ref DensityAssumedIndValue, value);
        }

        private string DensityAssumedIndValue = "N";

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
