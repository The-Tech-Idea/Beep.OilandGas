using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    /// <summary>
    /// One saved pipeline analysis (WTA-03): the mode, the capacity with its unit, the regime, the erosion threshold with
    /// its source, the inputs it was run with, the verdict and the actor.
    /// </summary>
    /// <remarks>
    /// <see cref="PIPELINE_ANALYSIS_RESULT"/> is the module's own result record, but it declares ten pairs of properties
    /// whose names differ only by case (<c>STATUS</c> and <c>Status</c>, <c>FLOW_RATE</c> and <c>FlowRate</c>,
    /// <c>INLET_PRESSURE</c> and <c>InletPressure</c>, and so on). A case-insensitive store cannot create one table for
    /// that, and the unit of work's property map throws <c>An item with the same key has already been added. Key: Status</c>
    /// before it writes anything, so the table cannot be persisted through at all. Both sets are in use by existing code -
    /// the module's <c>PipelineAnalysisService.DataManagement</c> writes the upper-case names and
    /// <c>PPDMCalculationService</c> writes the mixed-case ones - so neither can be removed here. This record is the
    /// additive persistence contract for the screen until the owner collapses the duplicates.
    /// </remarks>
    public class PIPELINE_ANALYSIS_REGISTER : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string REGISTER_ID
        {
            get => RegisterValue;
            set => SetProperty(ref RegisterValue, value);
        }

        private string RegisterValue = string.Empty;

        /// <summary>GAS or LIQUID - which capacity relation produced the result (AC-6).</summary>
        [MaxLength(16)]
        public string MODE
        {
            get => ModeValue;
            set => SetProperty(ref ModeValue, value);
        }

        private string ModeValue = string.Empty;

        /// <summary>The well or the line the analysis is for.</summary>
        [MaxLength(128)]
        public string? WELL_UWI
        {
            get => WellValue;
            set => SetProperty(ref WellValue, value);
        }

        private string? WellValue;

        [MaxLength(128)]
        public string? PIPELINE_ID
        {
            get => PipelineValue;
            set => SetProperty(ref PipelineValue, value);
        }

        private string? PipelineValue;

        public DateTime? ANALYSIS_DATE
        {
            get => AnalysisDateValue;
            set => SetProperty(ref AnalysisDateValue, value);
        }

        private DateTime? AnalysisDateValue;

        // ---- geometry and conditions ----

        public decimal? DIAMETER_IN
        {
            get => DiameterValue;
            set => SetProperty(ref DiameterValue, value);
        }

        private decimal? DiameterValue;

        public decimal? LENGTH_MILES
        {
            get => LengthValue;
            set => SetProperty(ref LengthValue, value);
        }

        private decimal? LengthValue;

        public decimal? ROUGHNESS_FT
        {
            get => RoughnessValue;
            set => SetProperty(ref RoughnessValue, value);
        }

        private decimal? RoughnessValue;

        public decimal? INLET_PRESSURE
        {
            get => InletValue;
            set => SetProperty(ref InletValue, value);
        }

        private decimal? InletValue;

        public decimal? OUTLET_PRESSURE
        {
            get => OutletValue;
            set => SetProperty(ref OutletValue, value);
        }

        private decimal? OutletValue;

        public decimal? AVERAGE_TEMPERATURE_R
        {
            get => TemperatureValue;
            set => SetProperty(ref TemperatureValue, value);
        }

        private decimal? TemperatureValue;

        // ---- fluid properties ----

        public decimal? GAS_SPECIFIC_GRAVITY
        {
            get => GasGravityValue;
            set => SetProperty(ref GasGravityValue, value);
        }

        private decimal? GasGravityValue;

        public decimal? BASE_PRESSURE
        {
            get => BasePressureValue;
            set => SetProperty(ref BasePressureValue, value);
        }

        private decimal? BasePressureValue;

        public decimal? BASE_TEMPERATURE_R
        {
            get => BaseTemperatureValue;
            set => SetProperty(ref BaseTemperatureValue, value);
        }

        private decimal? BaseTemperatureValue;

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

        // ---- the result ----

        public decimal CAPACITY
        {
            get => CapacityValue;
            set => SetProperty(ref CapacityValue, value);
        }

        private decimal CapacityValue;

        /// <summary>Mscf/d for gas and bbl/d for liquid.</summary>
        [MaxLength(16)]
        public string CAPACITY_OUOM
        {
            get => CapacityOuomValue;
            set => SetProperty(ref CapacityOuomValue, value);
        }

        private string CapacityOuomValue = string.Empty;

        [MaxLength(16)]
        public string FLOW_REGIME
        {
            get => RegimeValue;
            set => SetProperty(ref RegimeValue, value);
        }

        private string RegimeValue = string.Empty;

        public decimal? REYNOLDS_NUMBER
        {
            get => ReynoldsValue;
            set => SetProperty(ref ReynoldsValue, value);
        }

        private decimal? ReynoldsValue;

        public decimal? FRICTION_FACTOR
        {
            get => FrictionValue;
            set => SetProperty(ref FrictionValue, value);
        }

        private decimal? FrictionValue;

        public decimal? PRESSURE_DROP_PSI
        {
            get => PressureDropValue;
            set => SetProperty(ref PressureDropValue, value);
        }

        private decimal? PressureDropValue;

        public decimal? VELOCITY_FPS
        {
            get => VelocityValue;
            set => SetProperty(ref VelocityValue, value);
        }

        private decimal? VelocityValue;

        // ---- the erosion check (AC-2) ----

        public decimal? EROSION_RATE_MPY
        {
            get => ErosionRateValue;
            set => SetProperty(ref ErosionRateValue, value);
        }

        private decimal? ErosionRateValue;

        public decimal? EROSION_THRESHOLD_FPS
        {
            get => ErosionThresholdValue;
            set => SetProperty(ref ErosionThresholdValue, value);
        }

        private decimal? ErosionThresholdValue;

        public decimal? VELOCITY_RATIO
        {
            get => VelocityRatioValue;
            set => SetProperty(ref VelocityRatioValue, value);
        }

        private decimal? VelocityRatioValue;

        /// <summary>
        /// The standard behind the threshold. AC-2: a limit is never stored without its source, and a result with none
        /// stores the sentence that says so.
        /// </summary>
        [MaxLength(256)]
        public string? THRESHOLD_SOURCE
        {
            get => ThresholdSourceValue;
            set => SetProperty(ref ThresholdSourceValue, value);
        }

        private string? ThresholdSourceValue;

        [MaxLength(16)]
        public string? EROSION_SERVICE
        {
            get => ErosionServiceValue;
            set => SetProperty(ref ErosionServiceValue, value);
        }

        private string? ErosionServiceValue;

        // ---- the verdict ----

        [MaxLength(1)]
        public string FEASIBLE_IND
        {
            get => FeasibleValue;
            set => SetProperty(ref FeasibleValue, value);
        }

        private string FeasibleValue = "N";

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
            get => RecordedValue;
            set => SetProperty(ref RecordedValue, value);
        }

        private DateTime? RecordedValue;
    }
}
