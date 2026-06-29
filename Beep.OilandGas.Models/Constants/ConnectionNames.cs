namespace Beep.OilandGas.Models.Constants
{
    /// <summary>
    /// Centralized connection name constants for the Beep Oil &amp; Gas platform.
    /// Replaces hardcoded "PPDM39" strings across the solution.
    /// </summary>
    public static class ConnectionNames
    {
        /// <summary>
        /// Default PPDM 3.9 database connection name used across all services, repositories,
        /// and the API layer. Should match the connection string key in appsettings.json.
        /// </summary>
        public const string PPDM39 = "PPDM39";
    }
}
