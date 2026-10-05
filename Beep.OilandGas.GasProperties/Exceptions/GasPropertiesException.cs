using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.GasProperties.Exceptions
{
    /// <summary>
    /// Base exception for gas properties calculations: the calculation refusing the values it was given.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Every throw of this family refuses what the caller sent — a pressure, temperature or gravity out
    /// of the correlations' range, a composition that does not sum to one, inputs the iteration cannot converge on — so
    /// it is a <see cref="RefusalException"/> (<see cref="RefusalKind.Invalid"/>) and the API answers it as a 400 with
    /// its sentence. Its message is written for the person, never taken from a caught exception.
    /// </remarks>
    public class GasPropertiesException : RefusalException
    {
        public GasPropertiesException()
            : base(RefusalKind.Invalid, "The gas properties could not be calculated from the values given.")
        {
        }

        public GasPropertiesException(string message)
            : base(RefusalKind.Invalid, message)
        {
        }

        public GasPropertiesException(string message, Exception innerException)
            : base(RefusalKind.Invalid, message, innerException)
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
