namespace Beep.OilandGas.Models.ProductionForecasting
{
    /// <summary>
    /// Forecast type enumeration.
    /// </summary>
    public enum ForecastType
    {
        /// <summary>
        /// Pseudo-steady state single-phase.
        /// </summary>
        PseudoSteadyStateSinglePhase,

        /// <summary>
        /// Pseudo-steady state two-phase.
        /// </summary>
        PseudoSteadyStateTwoPhase,

        /// <summary>
        /// Transient flow.
        /// </summary>
        Transient,

        /// <summary>
        /// Gas well forecast.
        /// </summary>
        GasWell,

        /// <summary>
        /// Decline curve analysis (Arps decline methods).
        /// </summary>
        Decline
    }
}



