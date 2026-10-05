using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.FlashCalculations.Exceptions
{
    /// <summary>
    /// Base exception for flash calculations.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Every throw of this family refuses what the caller sent — flash conditions or components out
    /// of range, or a feed the flash cannot converge on — so it is a <see cref="RefusalException"/> (<see
    /// cref="RefusalKind.Invalid"/>) and the API answers it as a 400 with its sentence. Its message is written for
    /// the person, never taken from a caught exception.
    /// </remarks>
    public class FlashException : RefusalException
    {
        public FlashException()
            : base(RefusalKind.Invalid, "The flash calculation could not be done with the values given.")
        {
        }

        public FlashException(string message)
            : base(RefusalKind.Invalid, message)
        {
        }

        public FlashException(string message, Exception innerException)
            : base(RefusalKind.Invalid, message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when flash conditions are invalid.
    /// </summary>
    public class InvalidFlashConditionsException : FlashException
    {
        public InvalidFlashConditionsException()
            : base("Flash calculation conditions are invalid.")
        {
        }

        public InvalidFlashConditionsException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when component properties are invalid.
    /// </summary>
    public class InvalidComponentException : FlashException
    {
        public InvalidComponentException()
            : base("Component properties are invalid.")
        {
        }

        public InvalidComponentException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when flash calculation fails to converge.
    /// </summary>
    public class FlashConvergenceException : FlashException
    {
        public FlashConvergenceException()
            : base("Flash calculation failed to converge.")
        {
        }

        public FlashConvergenceException(string message)
            : base(message)
        {
        }
    }
}

