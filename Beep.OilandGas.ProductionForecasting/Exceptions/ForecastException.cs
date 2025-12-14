using System;

namespace Beep.OilandGas.ProductionForecasting.Exceptions
{
    /// <summary>
    /// Base exception for production forecasting calculations.
    /// </summary>
    public class ForecastException : Exception
    {
        public ForecastException()
            : base()
        {
        }

        public ForecastException(string message)
            : base(message)
        {
        }

        public ForecastException(string message, Exception innerException)
            : base(message, innerException)
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

