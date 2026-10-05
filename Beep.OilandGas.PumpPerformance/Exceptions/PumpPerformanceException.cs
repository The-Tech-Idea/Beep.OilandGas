using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.PumpPerformance.Exceptions
{
    /// <summary>
    /// Base exception class for pump performance calculation errors.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Every throw of this family refuses what the caller sent — a pump curve, speed, diameter, head or
    /// efficiency out of the range the calculation accepts — so it is a <see cref="RefusalException"/>
    /// (<see cref="RefusalKind.Invalid"/>) and the API answers it as a 400 with its sentence. Its message is written for
    /// the person, never taken from a caught exception.
    /// </remarks>
    public class PumpPerformanceException : RefusalException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PumpPerformanceException"/> class.
        /// </summary>
        public PumpPerformanceException() : base(RefusalKind.Invalid, "The pump performance could not be calculated from the values given.") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PumpPerformanceException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PumpPerformanceException(string message) : base(RefusalKind.Invalid, message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PumpPerformanceException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PumpPerformanceException(string message, Exception innerException) : base(RefusalKind.Invalid, message, innerException) { }
    }
}

