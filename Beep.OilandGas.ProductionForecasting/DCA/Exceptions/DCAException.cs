using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.DCA.Exceptions
{
    /// <summary>
    /// Base exception class for all DCA-related exceptions: the production data or parameters a caller sent cannot be
    /// analysed.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Its two kinds — invalid data (<see cref="InvalidDataException"/>) and a fit that does not converge on
    /// the data given (<see cref="ConvergenceException"/>) — both refuse what the caller sent, so the family is a
    /// <see cref="RefusalException"/> of kind <see cref="RefusalKind.Invalid"/>, answered 400 with its sentence.
    /// </remarks>
    public class DCAException : RefusalException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DCAException"/> class.
        /// </summary>
        public DCAException()
            : base(RefusalKind.Invalid, "The decline curve analysis cannot be performed on the data given.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DCAException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DCAException(string message)
            : base(RefusalKind.Invalid, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DCAException"/> class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public DCAException(string message, Exception innerException)
            : base(RefusalKind.Invalid, message, innerException)
        {
        }
    }
}
