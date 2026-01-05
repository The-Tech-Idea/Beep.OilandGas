namespace Beep.OilandGas.Client.Models
{
    /// <summary>
    /// API error response model
    /// </summary>
    public class ApiError
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Detailed error message
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Error details
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Validation errors (if applicable)
        /// </summary>
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}

