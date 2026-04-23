using System;
using Beep.OilandGas.Drawing.Validation;

namespace Beep.OilandGas.Drawing.Exceptions
{
    /// <summary>
    /// Exception thrown when scene or normalized model validation fails.
    /// </summary>
    public class DrawingValidationException : DrawingException
    {
        /// <summary>
        /// Gets the validation report that caused the exception.
        /// </summary>
        public DrawingValidationReport Report { get; }

        public DrawingValidationException(string message, DrawingValidationReport report)
            : base(message)
        {
            Report = report ?? throw new ArgumentNullException(nameof(report));
        }

        public DrawingValidationException(string message, DrawingValidationReport report, Exception innerException)
            : base(message, innerException)
        {
            Report = report ?? throw new ArgumentNullException(nameof(report));
        }
    }
}