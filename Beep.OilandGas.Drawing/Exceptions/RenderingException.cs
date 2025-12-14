using System;

namespace Beep.OilandGas.Drawing.Exceptions
{
    /// <summary>
    /// Exception thrown when rendering operations fail.
    /// </summary>
    public class RenderingException : DrawingException
    {
        /// <summary>
        /// Gets the rendering operation that failed.
        /// </summary>
        public string RenderingOperation { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderingException"/> class.
        /// </summary>
        public RenderingException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderingException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public RenderingException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderingException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public RenderingException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderingException"/> class.
        /// </summary>
        /// <param name="renderingOperation">The rendering operation that failed.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public RenderingException(string renderingOperation, string message, Exception innerException = null) 
            : base(message, innerException)
        {
            RenderingOperation = renderingOperation;
        }
    }
}

