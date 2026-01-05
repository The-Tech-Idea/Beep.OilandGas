namespace Beep.OilandGas.Client.Connection
{
    /// <summary>
    /// Utility for connection name resolution
    /// </summary>
    public static class ConnectionHelper
    {
        /// <summary>
        /// Resolves connection name using priority order:
        /// 1. Explicit connection name (highest priority)
        /// 2. Context connection name
        /// 3. Default connection name from options
        /// 4. System default ("PPDM39")
        /// </summary>
        public static string ResolveConnectionName(
            string? explicitConnection,
            string? contextConnection = null,
            string? defaultConnection = null)
        {
            return explicitConnection
                ?? contextConnection
                ?? defaultConnection
                ?? "PPDM39";
        }
    }
}

