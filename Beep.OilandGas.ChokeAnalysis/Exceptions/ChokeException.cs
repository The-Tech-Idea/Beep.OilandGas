using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.ChokeAnalysis.Exceptions
{
    /// <summary>
    /// Base exception for choke flow calculations.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Every throw of this family refuses what the caller sent — choke or gas properties out of the
    /// range the correlations accept, or inputs the calculation cannot converge on — so it is a <see
    /// cref="RefusalException"/> (<see cref="RefusalKind.Invalid"/>) and the API answers it as a 400 with its
    /// sentence. Its message is written for the person, never taken from a caught exception.
    /// </remarks>
    public class ChokeException : RefusalException
    {
        public ChokeException()
            : base(RefusalKind.Invalid, "The choke calculation could not be done with the values given.")
        {
        }

        public ChokeException(string message)
            : base(RefusalKind.Invalid, message)
        {
        }

        public ChokeException(string message, Exception innerException)
            : base(RefusalKind.Invalid, message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when choke properties are invalid.
    /// </summary>
    public class InvalidChokePropertiesException : ChokeException
    {
        public InvalidChokePropertiesException()
            : base("Choke properties are invalid.")
        {
        }

        public InvalidChokePropertiesException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when calculation parameters are out of valid range.
    /// </summary>
    public class ChokeParameterOutOfRangeException : ChokeException
    {
        public string ParameterName { get; }

        public ChokeParameterOutOfRangeException(string parameterName, string message)
            : base(message)
        {
            ParameterName = parameterName;
        }
    }

    /// <summary>
    /// Exception thrown when calculation fails to converge.
    /// </summary>
    public class ChokeConvergenceException : ChokeException
    {
        public ChokeConvergenceException()
            : base("Choke calculation failed to converge.")
        {
        }

        public ChokeConvergenceException(string message)
            : base(message)
        {
        }
    }
}

