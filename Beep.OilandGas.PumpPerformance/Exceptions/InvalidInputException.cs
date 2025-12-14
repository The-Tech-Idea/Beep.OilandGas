using System;

namespace Beep.OilandGas.PumpPerformance.Exceptions
{
    /// <summary>
    /// Exception thrown when input parameters are invalid.
    /// </summary>
    public class InvalidInputException : PumpPerformanceException
    {
        /// <summary>
        /// Gets the name of the invalid parameter.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInputException"/> class.
        /// </summary>
        public InvalidInputException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInputException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public InvalidInputException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInputException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidInputException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInputException"/> class.
        /// </summary>
        /// <param name="parameterName">The name of the invalid parameter.</param>
        /// <param name="message">The error message.</param>
        public InvalidInputException(string parameterName, string message) : base(message)
        {
            ParameterName = parameterName;
        }
    }
}

