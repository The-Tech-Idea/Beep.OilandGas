using System;

namespace Beep.OilandGas.Client.Exceptions
{
    /// <summary>
    /// Exception thrown when an API request times out
    /// </summary>
    public class ApiTimeoutException : ApiClientException
    {
        /// <summary>
        /// The timeout duration that was exceeded
        /// </summary>
        public TimeSpan? Timeout { get; }

        public ApiTimeoutException() : base("The request timed out")
        {
        }

        public ApiTimeoutException(string message) : base(message)
        {
        }

        public ApiTimeoutException(string message, TimeSpan timeout) : base(message)
        {
            Timeout = timeout;
        }

        public ApiTimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ApiTimeoutException(string message, TimeSpan timeout, Exception innerException) : base(message, innerException)
        {
            Timeout = timeout;
        }
    }
}

