using System;

namespace Beep.OilandGas.GasProperties.Exceptions
{
    /// <summary>
    /// Base exception for gas properties calculations.
    /// </summary>
    public class GasPropertiesException : Exception
    {
        public GasPropertiesException()
            : base()
        {
        }

        public GasPropertiesException(string message)
            : base(message)
        {
        }

        public GasPropertiesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when gas composition is invalid.
    /// </summary>
    public class InvalidGasCompositionException : GasPropertiesException
    {
        public InvalidGasCompositionException()
            : base("Gas composition is invalid. Fractions must sum to 1.0.")
        {
        }

        public InvalidGasCompositionException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when calculation parameters are out of valid range.
    /// </summary>
    public class ParameterOutOfRangeException : GasPropertiesException
    {
        public string ParameterName { get; }

        public ParameterOutOfRangeException(string parameterName, string message)
            : base(message)
        {
            ParameterName = parameterName;
        }
    }

    /// <summary>
    /// Exception thrown when calculation fails to converge.
    /// </summary>
    public class CalculationConvergenceException : GasPropertiesException
    {
        public CalculationConvergenceException()
            : base("Calculation failed to converge.")
        {
        }

        public CalculationConvergenceException(string message)
            : base(message)
        {
        }
    }
}

