using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.PlungerLift
{
    /// <summary>
    /// One plunger lift assessment (LFT-05): the well, the plunger type, the cycle the screen designed, the feasibility
    /// verdict with its criteria and the actor.
    /// </summary>
    /// <remarks>
    /// The PlungerLift entities in this namespace are calculation objects; none is a table, so an assessment had nowhere
    /// to persist. This is that record, with the criteria outcome as a column so the verdict can be read without
    /// re-running the screen (AC-6).
    /// </remarks>
    public class PLUNGER_DESIGN : ModelEntityBase
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

        /// <summary>BAR, PAD, BRUSH or CONTINUOUS - from <c>PlungerLiftConstants</c> (AC-4).</summary>
        [MaxLength(16)]
        public string PLUNGER_TYPE
        {
            get => PlungerTypeValue;
            set => SetProperty(ref PlungerTypeValue, value);
        }

        private string PlungerTypeValue = string.Empty;

        public decimal DEPTH_FT
        {
            get => DepthValue;
            set => SetProperty(ref DepthValue, value);
        }

        private decimal DepthValue;

        public decimal TUBING_DIAMETER_IN
        {
            get => TubingValue;
            set => SetProperty(ref TubingValue, value);
        }

        private decimal TubingValue;

        public decimal CASING_PRESSURE_PSIA
        {
            get => CasingPressureValue;
            set => SetProperty(ref CasingPressureValue, value);
        }

        private decimal CasingPressureValue;

        public decimal LINE_PRESSURE_PSIA
        {
            get => LinePressureValue;
            set => SetProperty(ref LinePressureValue, value);
        }

        private decimal LinePressureValue;

        public decimal GAS_RATE_MSCFD
        {
            get => GasRateValue;
            set => SetProperty(ref GasRateValue, value);
        }

        private decimal GasRateValue;

        public decimal LIQUID_RATE_BBLD
        {
            get => LiquidRateValue;
            set => SetProperty(ref LiquidRateValue, value);
        }

        private decimal LiquidRateValue;

        /// <summary>The gas-liquid ratio the criteria were judged at, scf/bbl.</summary>
        public decimal GLR_SCF_BBL
        {
            get => GlrValue;
            set => SetProperty(ref GlrValue, value);
        }

        private decimal GlrValue;

        public decimal FALL_VELOCITY_FPM
        {
            get => FallVelocityValue;
            set => SetProperty(ref FallVelocityValue, value);
        }

        private decimal FallVelocityValue;

        public decimal RISE_VELOCITY_FPM
        {
            get => RiseVelocityValue;
            set => SetProperty(ref RiseVelocityValue, value);
        }

        private decimal RiseVelocityValue;

        public decimal FALL_TIME_MIN
        {
            get => FallTimeValue;
            set => SetProperty(ref FallTimeValue, value);
        }

        private decimal FallTimeValue;

        public decimal RISE_TIME_MIN
        {
            get => RiseTimeValue;
            set => SetProperty(ref RiseTimeValue, value);
        }

        private decimal RiseTimeValue;

        public decimal SHUT_IN_TIME_MIN
        {
            get => ShutInTimeValue;
            set => SetProperty(ref ShutInTimeValue, value);
        }

        private decimal ShutInTimeValue;

        public decimal CYCLE_TIME_MIN
        {
            get => CycleTimeValue;
            set => SetProperty(ref CycleTimeValue, value);
        }

        private decimal CycleTimeValue;

        public decimal CYCLES_PER_DAY
        {
            get => CyclesPerDayValue;
            set => SetProperty(ref CyclesPerDayValue, value);
        }

        private decimal CyclesPerDayValue;

        /// <summary>The liquid lifted per cycle, bbl.</summary>
        public decimal RATE_PER_CYCLE_BBL
        {
            get => RatePerCycleValue;
            set => SetProperty(ref RatePerCycleValue, value);
        }

        private decimal RatePerCycleValue;

        /// <summary>The daily liquid rate the cycle supports; null when the assessment is infeasible (AC-2).</summary>
        public decimal? DAILY_RATE_BBLD
        {
            get => DailyRateValue;
            set => SetProperty(ref DailyRateValue, value);
        }

        private decimal? DailyRateValue;

        /// <summary>FEASIBLE, INFEASIBLE or UNECONOMIC (AC-3).</summary>
        [MaxLength(16)]
        public string FEASIBILITY
        {
            get => FeasibilityValue;
            set => SetProperty(ref FeasibilityValue, value);
        }

        private string FeasibilityValue = string.Empty;

        /// <summary>'Y' when the well is feasible and economic.</summary>
        [MaxLength(1)]
        public string ECONOMIC_IND
        {
            get => EconomicIndValue;
            set => SetProperty(ref EconomicIndValue, value);
        }

        private string EconomicIndValue = "N";

        /// <summary>Why the verdict is uneconomic, when it is.</summary>
        [MaxLength(512)]
        public string? ECONOMIC_BASIS
        {
            get => EconomicBasisValue;
            set => SetProperty(ref EconomicBasisValue, value);
        }

        private string? EconomicBasisValue;

        /// <summary>Every criterion with its outcome, one per line (AC-1).</summary>
        public string? CRITERIA_TEXT
        {
            get => CriteriaTextValue;
            set => SetProperty(ref CriteriaTextValue, value);
        }

        private string? CriteriaTextValue;

        /// <summary>The criteria that failed, comma separated; null when none did.</summary>
        [MaxLength(512)]
        public string? FAILED_CRITERIA
        {
            get => FailedCriteriaValue;
            set => SetProperty(ref FailedCriteriaValue, value);
        }

        private string? FailedCriteriaValue;

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
