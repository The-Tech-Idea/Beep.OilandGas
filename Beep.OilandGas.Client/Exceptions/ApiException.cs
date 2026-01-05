using System;
using System.Net;

namespace Beep.OilandGas.Client.Exceptions
{
    /// <summary>
    /// Exception thrown when an HTTP API error occurs
    /// </summary>
    public class ApiException : ApiClientException
    {
        /// <summary>
        /// HTTP status code of the error response
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Response content from the API
        /// </summary>
        public string? ResponseContent { get; }

        public ApiException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public ApiException(HttpStatusCode statusCode, string message, string? responseContent) : base(message)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }

        public ApiException(HttpStatusCode statusCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public ApiException(HttpStatusCode statusCode, string message, string? responseContent, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }
    }
}

