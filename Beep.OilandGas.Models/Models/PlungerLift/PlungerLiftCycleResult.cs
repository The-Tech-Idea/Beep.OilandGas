namespace Beep.OilandGas.Models.PlungerLift
{
    /// <summary>
    /// Represents plunger lift cycle analysis results
    /// DTO for calculations - Entity class: PLUNGER_LIFT_CYCLE_RESULT
    /// </summary>
    public class PlungerLiftCycleResult
    {
        /// <summary>
        /// Cycle time in minutes
        /// </summary>
        public decimal CycleTime { get; set; }

        /// <summary>
        /// Plunger fall time in minutes
        /// </summary>
        public decimal FallTime { get; set; }

        /// <summary>
        /// Plunger rise time in minutes
        /// </summary>
        public decimal RiseTime { get; set; }

        /// <summary>
        /// Shut-in time in minutes
        /// </summary>
        public decimal ShutInTime { get; set; }

        /// <summary>
        /// Plunger fall velocity in ft/min
        /// </summary>
        public decimal FallVelocity { get; set; }

        /// <summary>
        /// Plunger rise velocity in ft/min
        /// </summary>
        public decimal RiseVelocity { get; set; }

        /// <summary>
        /// Liquid slug size in bbl
        /// </summary>
        public decimal LiquidSlugSize { get; set; }

        /// <summary>
        /// Production per cycle in bbl
        /// </summary>
        public decimal ProductionPerCycle { get; set; }

        /// <summary>
        /// Daily production rate in bbl/day
        /// </summary>
        public decimal DailyProductionRate { get; set; }

        /// <summary>
        /// Cycles per day
        /// </summary>
        public decimal CyclesPerDay { get; set; }
    }
}



