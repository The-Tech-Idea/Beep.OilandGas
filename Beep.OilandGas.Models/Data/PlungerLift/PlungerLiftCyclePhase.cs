
namespace Beep.OilandGas.Models.Data.PlungerLift
{
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
}
