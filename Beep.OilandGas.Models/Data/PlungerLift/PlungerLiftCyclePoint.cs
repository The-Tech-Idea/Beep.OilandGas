namespace Beep.OilandGas.Models.Data.PlungerLift
{
    /// <summary>
    /// Represents a point in plunger lift cycle
    /// DTO for calculations - Entity class: PLUNGER_LIFT_CYCLE_POINT
    /// </summary>
    public class PlungerLiftCyclePoint : ModelEntityBase
    {
        /// <summary>
        /// Time in minutes from cycle start
        /// </summary>
        public decimal Time { get; set; }

        /// <summary>
        /// Cycle phase
        /// </summary>
        public PlungerLiftCyclePhase Phase { get; set; }

        /// <summary>
        /// Plunger depth in feet
        /// </summary>
        public decimal PlungerDepth { get; set; }

        /// <summary>
        /// Casing pressure in psia
        /// </summary>
        public decimal CasingPressure { get; set; }

        /// <summary>
        /// Tubing pressure in psia
        /// </summary>
        public decimal TubingPressure { get; set; }

        /// <summary>
        /// Liquid production rate in bbl/day
        /// </summary>
        public decimal ProductionRate { get; set; }
    }
}



