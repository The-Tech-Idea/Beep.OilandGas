using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.GasLift.Exceptions
{
    /// <summary>
    /// Base exception for gas lift calculations.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Every throw of this family refuses what the caller sent — well properties, injection
    /// pressure, rate or valve count out of range — so it is a <see cref="RefusalException"/> (<see
    /// cref="RefusalKind.Invalid"/>) and the API answers it as a 400 with its sentence. Its message is written for
    /// the person, never taken from a caught exception.
    /// </remarks>
    public class GasLiftException : RefusalException
    {
        public GasLiftException()
            : base(RefusalKind.Invalid, "The gas lift calculation could not be done with the values given.")
        {
        }

        public GasLiftException(string message)
            : base(RefusalKind.Invalid, message)
        {
        }

        public GasLiftException(string message, Exception innerException)
            : base(RefusalKind.Invalid, message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when well properties are invalid.
    /// </summary>
    public class InvalidWellPropertiesException : GasLiftException
    {
        public InvalidWellPropertiesException()
            : base("Well properties are invalid.")
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
    public class GasLiftParameterOutOfRangeException : GasLiftException
    {
        public string ParameterName { get; }

        public GasLiftParameterOutOfRangeException(string parameterName, string message)
            : base(message)
        {
            ParameterName = parameterName;
        }
    }

    /// <summary>
    /// Exception thrown when gas lift design fails.
    /// </summary>
    public class GasLiftDesignException : GasLiftException
    {
        public GasLiftDesignException()
            : base("Gas lift design failed.")
        {
        }

        public GasLiftDesignException(string message)
            : base(message)
        {
        }
    }
}

