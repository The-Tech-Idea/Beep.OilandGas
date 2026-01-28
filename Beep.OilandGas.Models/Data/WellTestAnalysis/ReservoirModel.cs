
namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public enum ReservoirModel
    {
        /// <summary>
        /// Infinite acting reservoir.
        /// </summary>
        InfiniteActing,

        /// <summary>
        /// Closed boundary reservoir.
        /// </summary>
        ClosedBoundary,

        /// <summary>
        /// Constant pressure boundary.
        /// </summary>
        ConstantPressureBoundary,

        /// <summary>
        /// Single fault.
        /// </summary>
        SingleFault,

        /// <summary>
        /// Multiple faults.
        /// </summary>
        MultipleFaults,

        /// <summary>
        /// Channel reservoir.
        /// </summary>
        ChannelReservoir,

        /// <summary>
        /// Dual porosity.
        /// </summary>
        DualPorosity,

        /// <summary>
        /// Dual permeability.
        /// </summary>
        DualPermeability
    }
}
