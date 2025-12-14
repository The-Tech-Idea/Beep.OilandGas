using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PlungerLift.Models
{
    /// <summary>
    /// Represents plunger lift well properties.
    /// </summary>
    public class PlungerLiftWellProperties
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
        /// Plunger diameter in inches.
        /// </summary>
        public decimal PlungerDiameter { get; set; }

        /// <summary>
        /// Wellhead pressure in psia.
        /// </summary>
        public decimal WellheadPressure { get; set; }

        /// <summary>
        /// Casing pressure in psia.
        /// </summary>
        public decimal CasingPressure { get; set; }

        /// <summary>
        /// Bottom hole pressure in psia.
        /// </summary>
        public decimal BottomHolePressure { get; set; }

        /// <summary>
        /// Wellhead temperature in Rankine.
        /// </summary>
        public decimal WellheadTemperature { get; set; }

        /// <summary>
        /// Bottom hole temperature in Rankine.
        /// </summary>
        public decimal BottomHoleTemperature { get; set; }

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
        /// Liquid production rate in bbl/day.
        /// </summary>
        public decimal LiquidProductionRate { get; set; }
    }

    /// <summary>
    /// Represents plunger lift cycle analysis results.
    /// </summary>
    public class PlungerLiftCycleResult
    {
        /// <summary>
        /// Cycle time in minutes.
        /// </summary>
        public decimal CycleTime { get; set; }

        /// <summary>
        /// Plunger fall time in minutes.
        /// </summary>
        public decimal FallTime { get; set; }

        /// <summary>
        /// Plunger rise time in minutes.
        /// </summary>
        public decimal RiseTime { get; set; }

        /// <summary>
        /// Shut-in time in minutes.
        /// </summary>
        public decimal ShutInTime { get; set; }

        /// <summary>
        /// Plunger fall velocity in ft/min.
        /// </summary>
        public decimal FallVelocity { get; set; }

        /// <summary>
        /// Plunger rise velocity in ft/min.
        /// </summary>
        public decimal RiseVelocity { get; set; }

        /// <summary>
        /// Liquid slug size in bbl.
        /// </summary>
        public decimal LiquidSlugSize { get; set; }

        /// <summary>
        /// Production per cycle in bbl.
        /// </summary>
        public decimal ProductionPerCycle { get; set; }

        /// <summary>
        /// Daily production rate in bbl/day.
        /// </summary>
        public decimal DailyProductionRate { get; set; }

        /// <summary>
        /// Cycles per day.
        /// </summary>
        public decimal CyclesPerDay { get; set; }
    }

    /// <summary>
    /// Represents plunger lift gas requirements.
    /// </summary>
    public class PlungerLiftGasRequirements
    {
        /// <summary>
        /// Required gas injection rate in Mscf/day.
        /// </summary>
        public decimal RequiredGasInjectionRate { get; set; }

        /// <summary>
        /// Available gas from well in Mscf/day.
        /// </summary>
        public decimal AvailableGas { get; set; }

        /// <summary>
        /// Additional gas required in Mscf/day.
        /// </summary>
        public decimal AdditionalGasRequired { get; set; }

        /// <summary>
        /// Gas-liquid ratio required.
        /// </summary>
        public decimal RequiredGasLiquidRatio { get; set; }

        /// <summary>
        /// Minimum casing pressure in psia.
        /// </summary>
        public decimal MinimumCasingPressure { get; set; }

        /// <summary>
        /// Maximum casing pressure in psia.
        /// </summary>
        public decimal MaximumCasingPressure { get; set; }
    }

    /// <summary>
    /// Represents plunger lift performance analysis.
    /// </summary>
    public class PlungerLiftPerformanceResult
    {
        /// <summary>
        /// Cycle analysis results.
        /// </summary>
        public PlungerLiftCycleResult CycleResult { get; set; }

        /// <summary>
        /// Gas requirements.
        /// </summary>
        public PlungerLiftGasRequirements GasRequirements { get; set; }

        /// <summary>
        /// System efficiency (0-1).
        /// </summary>
        public decimal SystemEfficiency { get; set; }

        /// <summary>
        /// Whether system is feasible.
        /// </summary>
        public bool IsFeasible { get; set; }

        /// <summary>
        /// Feasibility reasons.
        /// </summary>
        public List<string> FeasibilityReasons { get; set; } = new();
    }

    /// <summary>
    /// Represents plunger lift cycle phase.
    /// </summary>
    public enum PlungerLiftCyclePhase
    {
        /// <summary>
        /// Shut-in phase (building pressure).
        /// </summary>
        ShutIn,

        /// <summary>
        /// Plunger fall phase.
        /// </summary>
        Fall,

        /// <summary>
        /// Plunger rise phase (lifting liquid).
        /// </summary>
        Rise,

        /// <summary>
        /// Afterflow phase.
        /// </summary>
        Afterflow
    }

    /// <summary>
    /// Represents a point in plunger lift cycle.
    /// </summary>
    public class PlungerLiftCyclePoint
    {
        /// <summary>
        /// Time in minutes from cycle start.
        /// </summary>
        public decimal Time { get; set; }

        /// <summary>
        /// Cycle phase.
        /// </summary>
        public PlungerLiftCyclePhase Phase { get; set; }

        /// <summary>
        /// Plunger depth in feet.
        /// </summary>
        public decimal PlungerDepth { get; set; }

        /// <summary>
        /// Casing pressure in psia.
        /// </summary>
        public decimal CasingPressure { get; set; }

        /// <summary>
        /// Tubing pressure in psia.
        /// </summary>
        public decimal TubingPressure { get; set; }

        /// <summary>
        /// Liquid production rate in bbl/day.
        /// </summary>
        public decimal ProductionRate { get; set; }
    }
}

