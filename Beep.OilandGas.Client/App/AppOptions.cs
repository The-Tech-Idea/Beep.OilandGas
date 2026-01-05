using System;

namespace Beep.OilandGas.Client.App
{
    /// <summary>
    /// Configuration options for AppClass
    /// </summary>
    public class AppOptions
    {
        /// <summary>
        /// Access mode (Remote, Local, or Auto)
        /// </summary>
        public ServiceAccessMode AccessMode { get; set; } = ServiceAccessMode.Auto;

        /// <summary>
        /// API base URL (required for Remote mode)
        /// </summary>
        public string? ApiBaseUrl { get; set; }

        /// <summary>
        /// Username for authentication (Remote mode with credentials)
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Password for authentication (Remote mode with credentials)
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Default connection name
        /// </summary>
        public string DefaultConnectionName { get; set; } = "PPDM39";

        /// <summary>
        /// Use local services if available (for Auto mode)
        /// </summary>
        public bool UseLocalServices { get; set; } = true;

        /// <summary>
        /// Client ID for authentication
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Client secret for authentication
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// Identity server URL (for token acquisition)
        /// </summary>
        public string? IdentityServerUrl { get; set; }

        /// <summary>
        /// Request timeout
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Maximum number of retries for failed requests
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Delay between retries
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    }
}

