using System;

namespace Beep.OilandGas.PumpPerformance.Exceptions
{
    /// <summary>
    /// Base exception class for pump performance calculation errors.
    /// </summary>
    public class PumpPerformanceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PumpPerformanceException"/> class.
        /// </summary>
        public PumpPerformanceException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PumpPerformanceException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PumpPerformanceException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PumpPerformanceException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PumpPerformanceException(string message, Exception innerException) : base(message, innerException) { }
    }
}

