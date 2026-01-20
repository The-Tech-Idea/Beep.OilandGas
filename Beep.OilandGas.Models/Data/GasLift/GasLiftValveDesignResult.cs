using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.GasLift
{
    /// <summary>
    /// Result of gas lift valve design calculation
    /// DTO for calculations - Entity class: GAS_LIFT_VALVE_DESIGN_RESULT
    /// </summary>
    public class GasLiftValveDesignResult : ModelEntityBase
    {
        /// <summary>
        /// List of designed valves
        /// </summary>
        public List<GasLiftValve> Valves { get; set; } = new List<GasLiftValve>();

        /// <summary>
        /// Total gas injection rate for all valves
        /// </summary>
        public decimal TotalGasInjectionRate { get; set; }

        /// <summary>
        /// Expected production rate
        /// </summary>
        public decimal ExpectedProductionRate { get; set; }

        /// <summary>
        /// System efficiency
        /// </summary>
        public decimal SystemEfficiency { get; set; }
    }

    /// <summary>
    /// Represents a gas lift valve
    /// DTO for calculations - Entity class: GAS_LIFT_VALVE
    /// </summary>
    public class GasLiftValve : ModelEntityBase
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
}



