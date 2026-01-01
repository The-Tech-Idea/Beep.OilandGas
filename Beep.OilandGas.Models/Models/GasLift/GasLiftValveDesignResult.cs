using System.Collections.Generic;

namespace Beep.OilandGas.Models.GasLift
{
    /// <summary>
    /// Result of gas lift valve design
    /// </summary>
    public class GasLiftValveDesignResult
    {
        /// <summary>
        /// Designed valves
        /// </summary>
        public List<GasLiftValve> Valves { get; set; } = new List<GasLiftValve>();

        /// <summary>
        /// Total gas injection rate (Mscf/day or m³/day)
        /// </summary>
        public decimal TotalGasInjectionRate { get; set; }
    }

    /// <summary>
    /// Gas lift valve design
    /// </summary>
    public class GasLiftValve
    {
        /// <summary>
        /// Valve depth (feet or meters)
        /// </summary>
        public decimal Depth { get; set; }

        /// <summary>
        /// Port size (inches or mm)
        /// </summary>
        public decimal PortSize { get; set; }

        /// <summary>
        /// Opening pressure (psia or kPa)
        /// </summary>
        public decimal OpeningPressure { get; set; }

        /// <summary>
        /// Closing pressure (psia or kPa)
        /// </summary>
        public decimal ClosingPressure { get; set; }

        /// <summary>
        /// Valve type
        /// </summary>
        public GasLiftValveType ValveType { get; set; }

        /// <summary>
        /// Temperature at valve depth (°F or °C)
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// Gas injection rate through this valve (Mscf/day or m³/day)
        /// </summary>
        public decimal GasInjectionRate { get; set; }
    }

    /// <summary>
    /// Gas lift valve type
    /// </summary>
    public enum GasLiftValveType
    {
        InjectionPressureOperated,
        ProductionPressureOperated,
        CombinationOperated
    }
}
