using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.HydraulicPumps
{
    /// <summary>
    /// One hydraulic pump design (LFT-06): the kind, the well, every jet or piston input, the result, the findings and the
    /// actor.
    /// </summary>
    /// <remarks>
    /// The HydraulicPumps entities in this namespace are calculation objects (well properties, pump properties, results)
    /// and none of them is a table with a key, so a design had nowhere to persist. This is that record. It stores the
    /// inputs as values as well as the result, because AC-6 asks for the type and all inputs: a saved design must be
    /// re-runnable from its own row, not only readable as a number.
    /// </remarks>
    public class HYDRAULIC_PUMP_DESIGN : ModelEntityBase
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

        /// <summary>JET or PISTON - from <c>HydraulicPumpKinds</c>.</summary>
        [MaxLength(16)]
        public string KIND
        {
            get => KindValue;
            set => SetProperty(ref KindValue, value);
        }

        private string KindValue = string.Empty;

        // ---- the well inputs ----

        public decimal WELL_DEPTH_FT
        {
            get => WellDepthValue;
            set => SetProperty(ref WellDepthValue, value);
        }

        private decimal WellDepthValue;

        public decimal TUBING_DIAMETER_IN
        {
            get => TubingValue;
            set => SetProperty(ref TubingValue, value);
        }

        private decimal TubingValue;

        public decimal CASING_DIAMETER_IN
        {
            get => CasingValue;
            set => SetProperty(ref CasingValue, value);
        }

        private decimal CasingValue;

        public decimal WELLHEAD_PRESSURE_PSIA
        {
            get => WellheadPressureValue;
            set => SetProperty(ref WellheadPressureValue, value);
        }

        private decimal WellheadPressureValue;

        public decimal BOTTOM_HOLE_PRESSURE_PSIA
        {
            get => BottomHolePressureValue;
            set => SetProperty(ref BottomHolePressureValue, value);
        }

        private decimal BottomHolePressureValue;

        public decimal WELLHEAD_TEMPERATURE_R
        {
            get => WellheadTemperatureValue;
            set => SetProperty(ref WellheadTemperatureValue, value);
        }

        private decimal WellheadTemperatureValue;

        public decimal BOTTOM_HOLE_TEMPERATURE_R
        {
            get => BottomHoleTemperatureValue;
            set => SetProperty(ref BottomHoleTemperatureValue, value);
        }

        private decimal BottomHoleTemperatureValue;

        public decimal OIL_GRAVITY_API
        {
            get => OilGravityValue;
            set => SetProperty(ref OilGravityValue, value);
        }

        private decimal OilGravityValue;

        public decimal WATER_CUT
        {
            get => WaterCutValue;
            set => SetProperty(ref WaterCutValue, value);
        }

        private decimal WaterCutValue;

        public decimal GAS_OIL_RATIO_SCF_STB
        {
            get => GorValue;
            set => SetProperty(ref GorValue, value);
        }

        private decimal GorValue;

        public decimal GAS_SPECIFIC_GRAVITY
        {
            get => GasGravityValue;
            set => SetProperty(ref GasGravityValue, value);
        }

        private decimal GasGravityValue;

        public decimal DESIRED_PRODUCTION_RATE_BBLD
        {
            get => DesiredRateValue;
            set => SetProperty(ref DesiredRateValue, value);
        }

        private decimal DesiredRateValue;

        public decimal PUMP_DEPTH_FT
        {
            get => PumpDepthValue;
            set => SetProperty(ref PumpDepthValue, value);
        }

        private decimal PumpDepthValue;

        // ---- the jet inputs ----

        public decimal? NOZZLE_DIAMETER_IN
        {
            get => NozzleValue;
            set => SetProperty(ref NozzleValue, value);
        }

        private decimal? NozzleValue;

        public decimal? THROAT_DIAMETER_IN
        {
            get => ThroatValue;
            set => SetProperty(ref ThroatValue, value);
        }

        private decimal? ThroatValue;

        public decimal? DIFFUSER_DIAMETER_IN
        {
            get => DiffuserValue;
            set => SetProperty(ref DiffuserValue, value);
        }

        private decimal? DiffuserValue;

        public decimal? AREA_RATIO
        {
            get => AreaRatioValue;
            set => SetProperty(ref AreaRatioValue, value);
        }

        private decimal? AreaRatioValue;

        // ---- the piston inputs ----

        public decimal? PISTON_DIAMETER_IN
        {
            get => PistonValue;
            set => SetProperty(ref PistonValue, value);
        }

        private decimal? PistonValue;

        public decimal? ROD_DIAMETER_IN
        {
            get => RodValue;
            set => SetProperty(ref RodValue, value);
        }

        private decimal? RodValue;

        public decimal? STROKE_LENGTH_IN
        {
            get => StrokeValue;
            set => SetProperty(ref StrokeValue, value);
        }

        private decimal? StrokeValue;

        public decimal? STROKES_PER_MINUTE
        {
            get => SpmValue;
            set => SetProperty(ref SpmValue, value);
        }

        private decimal? SpmValue;

        // ---- the power fluid, shared ----

        public decimal POWER_FLUID_PRESSURE_PSIA
        {
            get => PowerFluidPressureValue;
            set => SetProperty(ref PowerFluidPressureValue, value);
        }

        private decimal PowerFluidPressureValue;

        public decimal POWER_FLUID_RATE_BBLD
        {
            get => PowerFluidRateValue;
            set => SetProperty(ref PowerFluidRateValue, value);
        }

        private decimal PowerFluidRateValue;

        public decimal POWER_FLUID_SPECIFIC_GRAVITY
        {
            get => PowerFluidGravityValue;
            set => SetProperty(ref PowerFluidGravityValue, value);
        }

        private decimal PowerFluidGravityValue;

        // ---- the result ----

        public decimal PRODUCTION_RATE_BBLD
        {
            get => ProductionRateValue;
            set => SetProperty(ref ProductionRateValue, value);
        }

        private decimal ProductionRateValue;

        public decimal TOTAL_FLOW_RATE_BBLD
        {
            get => TotalFlowValue;
            set => SetProperty(ref TotalFlowValue, value);
        }

        private decimal TotalFlowValue;

        /// <summary>The produced rate over the power fluid rate; null for a piston pump, which does not report it.</summary>
        public decimal? PRODUCTION_RATIO
        {
            get => ProductionRatioValue;
            set => SetProperty(ref ProductionRatioValue, value);
        }

        private decimal? ProductionRatioValue;

        /// <summary>The pump's own efficiency as the engine computes it (jet: the three-factor product; piston: volumetric).</summary>
        public decimal PUMP_EFFICIENCY
        {
            get => PumpEfficiencyValue;
            set => SetProperty(ref PumpEfficiencyValue, value);
        }

        private decimal PumpEfficiencyValue;

        /// <summary>The system efficiency after the engine's [0, 1] clamp (AC-3).</summary>
        public decimal SYSTEM_EFFICIENCY
        {
            get => SystemEfficiencyValue;
            set => SetProperty(ref SystemEfficiencyValue, value);
        }

        private decimal SystemEfficiencyValue;

        /// <summary>The ratio before the clamp; equal to <see cref="SYSTEM_EFFICIENCY"/> unless the engine clamped it.</summary>
        public decimal? RAW_SYSTEM_EFFICIENCY
        {
            get => RawSystemEfficiencyValue;
            set => SetProperty(ref RawSystemEfficiencyValue, value);
        }

        private decimal? RawSystemEfficiencyValue;

        public decimal POWER_FLUID_HORSEPOWER
        {
            get => PowerFluidHorsepowerValue;
            set => SetProperty(ref PowerFluidHorsepowerValue, value);
        }

        private decimal PowerFluidHorsepowerValue;

        public decimal HYDRAULIC_HORSEPOWER
        {
            get => HydraulicHorsepowerValue;
            set => SetProperty(ref HydraulicHorsepowerValue, value);
        }

        private decimal HydraulicHorsepowerValue;

        public decimal PUMP_INTAKE_PRESSURE_PSIA
        {
            get => IntakePressureValue;
            set => SetProperty(ref IntakePressureValue, value);
        }

        private decimal IntakePressureValue;

        public decimal PUMP_DISCHARGE_PRESSURE_PSIA
        {
            get => DischargePressureValue;
            set => SetProperty(ref DischargePressureValue, value);
        }

        private decimal DischargePressureValue;

        public decimal? PUMP_DISPLACEMENT_BBLD
        {
            get => DisplacementValue;
            set => SetProperty(ref DisplacementValue, value);
        }

        private decimal? DisplacementValue;

        // ---- the cavitation index (AC-2) ----

        public decimal CAVITATION_AVAILABLE_PSI
        {
            get => CavitationAvailableValue;
            set => SetProperty(ref CavitationAvailableValue, value);
        }

        private decimal CavitationAvailableValue;

        public decimal CAVITATION_REQUIRED_PSI
        {
            get => CavitationRequiredValue;
            set => SetProperty(ref CavitationRequiredValue, value);
        }

        private decimal CavitationRequiredValue;

        public decimal CAVITATION_MARGIN_PSI
        {
            get => CavitationMarginValue;
            set => SetProperty(ref CavitationMarginValue, value);
        }

        private decimal CavitationMarginValue;

        /// <summary>'Y' when the margin is negative.</summary>
        [MaxLength(1)]
        public string CAVITATION_RISK_IND
        {
            get => CavitationRiskValue;
            set => SetProperty(ref CavitationRiskValue, value);
        }

        private string CavitationRiskValue = "N";

        // ---- the findings (AC-3, AC-4) ----

        /// <summary>Every finding, one per line, with its severity and code.</summary>
        public string? FINDINGS_TEXT
        {
            get => FindingsTextValue;
            set => SetProperty(ref FindingsTextValue, value);
        }

        private string? FindingsTextValue;

        /// <summary>The worst severity present: INFO, WARNING or ERROR.</summary>
        [MaxLength(16)]
        public string WORST_SEVERITY
        {
            get => WorstSeverityValue;
            set => SetProperty(ref WorstSeverityValue, value);
        }

        private string WorstSeverityValue = "INFO";

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
    }
}
