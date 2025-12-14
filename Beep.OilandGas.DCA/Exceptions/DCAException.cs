using System;

namespace Beep.OilandGas.DCA.Exceptions
{
    /// <summary>
    /// Base exception class for all DCA-related exceptions.
    /// </summary>
    public class DCAException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DCAException"/> class.
        /// </summary>
        public DCAException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DCAException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DCAException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DCAException"/> class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public DCAException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

