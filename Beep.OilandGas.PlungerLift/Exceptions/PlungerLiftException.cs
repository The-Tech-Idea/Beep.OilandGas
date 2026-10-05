using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.PlungerLift.Exceptions
{
    /// <summary>
    /// Base exception for plunger lift calculations.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Every throw of this family refuses what the caller sent — well properties out of range, or a
    /// well for which plunger lift is not feasible — so it is a <see cref="RefusalException"/>
    /// (<see cref="RefusalKind.Invalid"/>) and the API answers it as a 400 with its sentence. Its message is written for
    /// the person, never taken from a caught exception.
    /// </remarks>
    public class PlungerLiftException : RefusalException
    {
        public PlungerLiftException()
            : base(RefusalKind.Invalid, "The plunger lift calculation could not be done with the values given.")
        {
        }

        public PlungerLiftException(string message)
            : base(RefusalKind.Invalid, message)
        {
        }

        public PlungerLiftException(string message, Exception innerException)
            : base(RefusalKind.Invalid, message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when well properties are invalid.
    /// </summary>
    public class InvalidWellPropertiesException : PlungerLiftException
    {
        public InvalidWellPropertiesException()
            : base("Plunger lift well properties are invalid.")
        {
        }

        public InvalidWellPropertiesException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when calculation parameters are out of valid range.
    /// </summary>
    public class PlungerLiftParameterOutOfRangeException : PlungerLiftException
    {
        public string ParameterName { get; }

        public PlungerLiftParameterOutOfRangeException(string parameterName, string message)
            : base(message)
        {
            ParameterName = parameterName;
        }
    }

    /// <summary>
    /// Exception thrown when plunger lift system is not feasible.
    /// </summary>
    public class PlungerLiftNotFeasibleException : PlungerLiftException
    {
        public PlungerLiftNotFeasibleException()
            : base("Plunger lift system is not feasible for this well.")
        {
        }

        public PlungerLiftNotFeasibleException(string message)
            : base(message)
        {
        }
    }
}

