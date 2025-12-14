using System;

namespace Beep.OilandGas.ChokeAnalysis.Exceptions
{
    /// <summary>
    /// Base exception for choke flow calculations.
    /// </summary>
    public class ChokeException : Exception
    {
        public ChokeException()
            : base()
        {
        }

        public ChokeException(string message)
            : base(message)
        {
        }

        public ChokeException(string message, Exception innerException)
            : base(message, innerException)
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

