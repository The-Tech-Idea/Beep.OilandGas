using System;

namespace Beep.OilandGas.FlashCalculations.Exceptions
{
    /// <summary>
    /// Base exception for flash calculations.
    /// </summary>
    public class FlashException : Exception
    {
        public FlashException()
            : base()
        {
        }

        public FlashException(string message)
            : base(message)
        {
        }

        public FlashException(string message, Exception innerException)
            : base(message, innerException)
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

