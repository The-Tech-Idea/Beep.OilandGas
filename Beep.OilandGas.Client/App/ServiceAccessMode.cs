namespace Beep.OilandGas.Client.App
{
    /// <summary>
    /// Enum for service access mode
    /// </summary>
    public enum ServiceAccessMode
    {
        /// <summary>
        /// Remote mode - uses HTTP API client
        /// </summary>
        Remote,

        /// <summary>
        /// Local mode - uses direct service injection via DI
        /// </summary>
        Local,

        /// <summary>
        /// Auto mode - automatically detects which mode to use based on configuration
        /// </summary>
        Auto
    }
}

