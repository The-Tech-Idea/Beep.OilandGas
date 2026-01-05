using System;

namespace Beep.OilandGas.Client.Exceptions
{
    /// <summary>
    /// Exception thrown when authentication fails
    /// </summary>
    public class ApiAuthenticationException : ApiClientException
    {
        public ApiAuthenticationException()
        {
        }

        public ApiAuthenticationException(string message) : base(message)
        {
        }

        public ApiAuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

