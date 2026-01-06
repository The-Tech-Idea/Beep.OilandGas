namespace Beep.OilandGas.Models.ChokeAnalysis
{
    /// <summary>
    /// Choke type enumeration.
    /// </summary>
    public enum ChokeType
    {
        /// <summary>
        /// Fixed bean choke.
        /// </summary>
        Bean,

        /// <summary>
        /// Adjustable choke.
        /// </summary>
        Adjustable,

        /// <summary>
        /// Positive choke.
        /// </summary>
        Positive
    }

    /// <summary>
    /// Flow regime enumeration.
    /// </summary>
    public enum FlowRegime
    {
        /// <summary>
        /// Subsonic flow.
        /// </summary>
        Subsonic,

        /// <summary>
        /// Sonic (critical) flow.
        /// </summary>
        Sonic
    }
}



