namespace Beep.OilandGas.Models.WellTestAnalysis
{
    /// <summary>
    /// Well test type enumeration.
    /// </summary>
    public enum WellTestType
    {
        /// <summary>
        /// Pressure build-up test.
        /// </summary>
        BuildUp,

        /// <summary>
        /// Pressure drawdown test.
        /// </summary>
        Drawdown
    }

    /// <summary>
    /// Reservoir model types for well test interpretation.
    /// </summary>
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



