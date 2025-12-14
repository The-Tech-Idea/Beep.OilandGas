using System;

namespace Beep.OilandGas.Drawing.Exceptions
{
    /// <summary>
    /// Base exception class for drawing framework errors.
    /// </summary>
    public class DrawingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingException"/> class.
        /// </summary>
        public DrawingException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public DrawingException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DrawingException(string message, Exception innerException) : base(message, innerException) { }
    }
}

