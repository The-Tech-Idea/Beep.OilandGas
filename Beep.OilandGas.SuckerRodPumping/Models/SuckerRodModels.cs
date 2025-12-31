using System;
using System.Collections.Generic;

namespace Beep.OilandGas.SuckerRodPumping.Models
{
    /// <summary>
    /// Represents sucker rod pumping system properties.
    /// </summary>
    public class SuckerRodSystemProperties
    {
        /// <summary>
        /// Well depth in feet.
        /// </summary>
        public decimal WellDepth { get; set; }

        /// <summary>
        /// Tubing diameter in inches.
        /// </summary>
        public decimal TubingDiameter { get; set; }

        /// <summary>
        /// Rod diameter in inches.
        /// </summary>
        public decimal RodDiameter { get; set; }

        /// <summary>
        /// Pump diameter in inches.
        /// </summary>
        public decimal PumpDiameter { get; set; }

        /// <summary>
        /// Stroke length in inches.
        /// </summary>
        public decimal StrokeLength { get; set; }

        /// <summary>
        /// Strokes per minute (SPM).
        /// </summary>
        public decimal StrokesPerMinute { get; set; }

        /// <summary>
        /// Wellhead pressure in psia.
        /// </summary>
        public decimal WellheadPressure { get; set; }

        /// <summary>
        /// Bottom hole pressure in psia.
        /// </summary>
        public decimal BottomHolePressure { get; set; }

        /// <summary>
        /// Oil gravity in API.
        /// </summary>
        public decimal OilGravity { get; set; }

        /// <summary>
        /// Water cut (fraction, 0-1).
        /// </summary>
        public decimal WaterCut { get; set; }

        /// <summary>
        /// Gas-oil ratio in scf/bbl.
        /// </summary>
        public decimal GasOilRatio { get; set; }

        /// <summary>
        /// Gas specific gravity (relative to air).
        /// </summary>
        public decimal GasSpecificGravity { get; set; }

        /// <summary>
        /// Rod material density in lb/ft³.
        /// </summary>
        public decimal RodDensity { get; set; } = 490m; // Steel

        /// <summary>
        /// Pump efficiency (0-1).
        /// </summary>
        public decimal PumpEfficiency { get; set; } = 0.85m;
        public decimal FluidLevel { get; set; }
        public decimal FluidDensity { get; set; }
    }

    /// <summary>
    /// Represents sucker rod load analysis results.
    /// </summary>
    public class SuckerRodLoadResult
    {
        /// <summary>
        /// Peak load in pounds.
        /// </summary>
        public decimal PeakLoad { get; set; }

        /// <summary>
        /// Minimum load in pounds.
        /// </summary>
        public decimal MinimumLoad { get; set; }

        /// <summary>
        /// Polished rod load in pounds.
        /// </summary>
        public decimal PolishedRodLoad { get; set; }

        /// <summary>
        /// Rod string weight in pounds.
        /// </summary>
        public decimal RodStringWeight { get; set; }

        /// <summary>
        /// Fluid load in pounds.
        /// </summary>
        public decimal FluidLoad { get; set; }

        /// <summary>
        /// Dynamic load in pounds.
        /// </summary>
        public decimal DynamicLoad { get; set; }

        /// <summary>
        /// Load range in pounds.
        /// </summary>
        public decimal LoadRange { get; set; }

        /// <summary>
        /// Stress range in psi.
        /// </summary>
        public decimal StressRange { get; set; }

        /// <summary>
        /// Maximum stress in psi.
        /// </summary>
        public decimal MaximumStress { get; set; }

        /// <summary>
        /// Load factor (safety factor).
        /// </summary>
        public decimal LoadFactor { get; set; }
    }

    /// <summary>
    /// Represents sucker rod flow rate and power results.
    /// </summary>
    public class SuckerRodFlowRatePowerResult
    {
        /// <summary>
        /// Production rate in bbl/day.
        /// </summary>
        public decimal ProductionRate { get; set; }

        /// <summary>
        /// Pump displacement in bbl/day.
        /// </summary>
        public decimal PumpDisplacement { get; set; }

        /// <summary>
        /// Volumetric efficiency (0-1).
        /// </summary>
        public decimal VolumetricEfficiency { get; set; }

        /// <summary>
        /// Polished rod horsepower.
        /// </summary>
        public decimal PolishedRodHorsepower { get; set; }

        /// <summary>
        /// Hydraulic horsepower.
        /// </summary>
        public decimal HydraulicHorsepower { get; set; }

        /// <summary>
        /// Friction horsepower.
        /// </summary>
        public decimal FrictionHorsepower { get; set; }

        /// <summary>
        /// Total horsepower required.
        /// </summary>
        public decimal TotalHorsepower { get; set; }

        /// <summary>
        /// Motor horsepower.
        /// </summary>
        public decimal MotorHorsepower { get; set; }

        /// <summary>
        /// System efficiency (0-1).
        /// </summary>
        public decimal SystemEfficiency { get; set; }

        /// <summary>
        /// Energy consumption in kWh/day.
        /// </summary>
        public decimal EnergyConsumption { get; set; }
    }

    /// <summary>
    /// Represents sucker rod string configuration.
    /// </summary>
    public class SuckerRodString
    {
        /// <summary>
        /// Rod sections.
        /// </summary>
        public List<RodSection> Sections { get; set; } = new();

        /// <summary>
        /// Total length in feet.
        /// </summary>
        public decimal TotalLength { get; set; }

        /// <summary>
        /// Total weight in pounds.
        /// </summary>
        public decimal TotalWeight { get; set; }
    }

    /// <summary>
    /// Represents a section of sucker rod string.
    /// </summary>
    public class RodSection
    {
        /// <summary>
        /// Rod diameter in inches.
        /// </summary>
        public decimal Diameter { get; set; }

        /// <summary>
        /// Section length in feet.
        /// </summary>
        public decimal Length { get; set; }

        /// <summary>
        /// Rod material density in lb/ft³.
        /// </summary>
        public decimal Density { get; set; } = 490m; // Steel

        /// <summary>
        /// Section weight in pounds.
        /// </summary>
        public decimal Weight { get; set; }
    }

    ///// <summary>
    ///// Represents pump card (load vs position).
    ///// </summary>
   

    ///// <summary>
    ///// Represents a point on pump card.
    ///// </summary>
    //public class PumpCardPoint
    //{
    //    /// <summary>
    //    /// Position (0-1, where 0 = bottom of stroke, 1 = top of stroke).
    //    /// </summary>
    //    public decimal Position { get; set; }

    //    /// <summary>
    //    /// Load in pounds.
    //    /// </summary>
    //    public decimal Load { get; set; }
    //}
}

