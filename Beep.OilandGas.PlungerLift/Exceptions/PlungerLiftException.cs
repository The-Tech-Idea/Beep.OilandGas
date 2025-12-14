using System;

namespace Beep.OilandGas.PlungerLift.Exceptions
{
    /// <summary>
    /// Base exception for plunger lift calculations.
    /// </summary>
    public class PlungerLiftException : Exception
    {
        public PlungerLiftException()
            : base()
        {
        }

        public PlungerLiftException(string message)
            : base(message)
        {
        }

        public PlungerLiftException(string message, Exception innerException)
            : base(message, innerException)
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

