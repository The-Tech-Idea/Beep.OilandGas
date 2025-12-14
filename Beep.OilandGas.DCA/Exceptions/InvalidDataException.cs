using System;

namespace Beep.OilandGas.DCA.Exceptions
{
    /// <summary>
    /// Exception thrown when invalid data is provided to DCA methods.
    /// </summary>
    public class InvalidDataException : DCAException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDataException"/> class.
        /// </summary>
        public InvalidDataException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDataException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidDataException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDataException"/> class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public InvalidDataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

