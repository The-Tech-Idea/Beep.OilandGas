using System;

namespace Beep.OilandGas.Client.Exceptions
{
    /// <summary>
    /// Base exception for all API client errors
    /// </summary>
    public class ApiClientException : Exception
    {
        public ApiClientException()
        {
        }

        public ApiClientException(string message) : base(message)
        {
        }

        public ApiClientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

