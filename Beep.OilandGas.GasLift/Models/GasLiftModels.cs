using System;
using System.Collections.Generic;

namespace Beep.OilandGas.GasLift.Models
{
    /// <summary>
    /// Represents well properties for gas lift analysis.
    /// </summary>
    public class GasLiftWellProperties
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
        /// Casing diameter in inches.
        /// </summary>
        public decimal CasingDiameter { get; set; }

        /// <summary>
        /// Wellhead pressure in psia.
        /// </summary>
        public decimal WellheadPressure { get; set; }

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
        /// Desired production rate in bbl/day.
        /// </summary>
        public decimal DesiredProductionRate { get; set; }
    }

    /// <summary>
    /// Represents gas lift valve properties.
    /// </summary>
    public class GasLiftValve
    {
        /// <summary>
        /// Valve depth in feet.
        /// </summary>
        public decimal Depth { get; set; }

        /// <summary>
        /// Valve port size in inches.
        /// </summary>
        public decimal PortSize { get; set; }

        /// <summary>
        /// Opening pressure in psia.
        /// </summary>
        public decimal OpeningPressure { get; set; }

        /// <summary>
        /// Closing pressure in psia.
        /// </summary>
        public decimal ClosingPressure { get; set; }

        /// <summary>
        /// Valve type.
        /// </summary>
        public GasLiftValveType ValveType { get; set; }

        /// <summary>
        /// Temperature at valve depth in Rankine.
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// Gas injection rate through valve in Mscf/day.
        /// </summary>
        public decimal GasInjectionRate { get; set; }
    }

    /// <summary>
    /// Gas lift valve type enumeration.
    /// </summary>
    public enum GasLiftValveType
    {
        /// <summary>
        /// Orifice valve.
        /// </summary>
        Orifice,

        /// <summary>
        /// Injection pressure operated valve.
        /// </summary>
        InjectionPressureOperated,

        /// <summary>
        /// Production pressure operated valve.
        /// </summary>
        ProductionPressureOperated,

        /// <summary>
        /// Throttle valve.
        /// </summary>
        Throttle
    }

    /// <summary>
    /// Represents gas lift potential analysis results.
    /// </summary>
    public class GasLiftPotentialResult
    {
        /// <summary>
        /// Optimal gas injection rate in Mscf/day.
        /// </summary>
        public decimal OptimalGasInjectionRate { get; set; }

        /// <summary>
        /// Maximum production rate in bbl/day.
        /// </summary>
        public decimal MaximumProductionRate { get; set; }

        /// <summary>
        /// Gas-liquid ratio at optimal point.
        /// </summary>
        public decimal OptimalGasLiquidRatio { get; set; }

        /// <summary>
        /// Production rate vs gas injection rate points.
        /// </summary>
        public List<GasLiftPerformancePoint> PerformancePoints { get; set; } = new();
    }

    /// <summary>
    /// Represents a point on gas lift performance curve.
    /// </summary>
    public class GasLiftPerformancePoint
    {
        /// <summary>
        /// Gas injection rate in Mscf/day.
        /// </summary>
        public decimal GasInjectionRate { get; set; }

        /// <summary>
        /// Production rate in bbl/day.
        /// </summary>
        public decimal ProductionRate { get; set; }

        /// <summary>
        /// Gas-liquid ratio.
        /// </summary>
        public decimal GasLiquidRatio { get; set; }

        /// <summary>
        /// Bottom hole pressure in psia.
        /// </summary>
        public decimal BottomHolePressure { get; set; }
    }

    /// <summary>
    /// Represents gas lift valve design results.
    /// </summary>
    public class GasLiftValveDesignResult
    {
        /// <summary>
        /// Designed valves.
        /// </summary>
        public List<GasLiftValve> Valves { get; set; } = new();

        /// <summary>
        /// Total gas injection rate in Mscf/day.
        /// </summary>
        public decimal TotalGasInjectionRate { get; set; }

        /// <summary>
        /// Expected production rate in bbl/day.
        /// </summary>
        public decimal ExpectedProductionRate { get; set; }

        /// <summary>
        /// System efficiency.
        /// </summary>
        public decimal SystemEfficiency { get; set; }
    }

    /// <summary>
    /// Represents gas lift valve spacing results.
    /// </summary>
    public class GasLiftValveSpacingResult
    {
        /// <summary>
        /// Valve depths in feet.
        /// </summary>
        public List<decimal> ValveDepths { get; set; } = new();

        /// <summary>
        /// Opening pressures in psia.
        /// </summary>
        public List<decimal> OpeningPressures { get; set; } = new();

        /// <summary>
        /// Number of valves.
        /// </summary>
        public int NumberOfValves { get; set; }

        /// <summary>
        /// Total depth coverage in feet.
        /// </summary>
        public decimal TotalDepthCoverage { get; set; }
    }
}

