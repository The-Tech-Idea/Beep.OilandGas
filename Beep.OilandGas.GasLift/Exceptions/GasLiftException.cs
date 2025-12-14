using System;

namespace Beep.OilandGas.GasLift.Exceptions
{
    /// <summary>
    /// Base exception for gas lift calculations.
    /// </summary>
    public class GasLiftException : Exception
    {
        public GasLiftException()
            : base()
        {
        }

        public GasLiftException(string message)
            : base(message)
        {
        }

        public GasLiftException(string message, Exception innerException)
            : base(message, innerException)
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

