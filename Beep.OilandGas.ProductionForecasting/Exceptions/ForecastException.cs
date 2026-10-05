using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.ProductionForecasting.Exceptions
{
    /// <summary>
    /// Base exception for production forecasting calculations: the values a caller sent cannot be forecast.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Every throw of this family refuses what the caller sent — a reservoir property, a parameter out of
    /// range, a calculation the values given cannot complete — so it is a <see cref="RefusalException"/> of kind
    /// <see cref="RefusalKind.Invalid"/>, answered 400 with its sentence rather than as a failure.
    /// </remarks>
    public class ForecastException : RefusalException
    {
        public ForecastException()
            : base(RefusalKind.Invalid, "The forecast cannot be calculated from the values given.")
        {
        }

        public ForecastException(string message)
            : base(RefusalKind.Invalid, message)
        {
        }

        public ForecastException(string message, Exception innerException)
            : base(RefusalKind.Invalid, message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when reservoir properties are invalid.
    /// </summary>
    public class InvalidReservoirPropertiesException : ForecastException
    {
        public InvalidReservoirPropertiesException()
            : base("Reservoir properties are invalid.")
        {
        }

        public InvalidReservoirPropertiesException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when forecast parameters are out of valid range.
    /// </summary>
    public class ForecastParameterOutOfRangeException : ForecastException
    {
        public string ParameterName { get; }

        public ForecastParameterOutOfRangeException(string parameterName, string message)
            : base(message)
        {
            ParameterName = parameterName;
        }
    }

    /// <summary>
    /// Exception thrown when forecast calculation fails to converge.
    /// </summary>
    public class ForecastConvergenceException : ForecastException
    {
        public ForecastConvergenceException()
            : base("Forecast calculation failed to converge.")
        {
        }

        public ForecastConvergenceException(string message)
            : base(message)
        {
        }
    }
}
