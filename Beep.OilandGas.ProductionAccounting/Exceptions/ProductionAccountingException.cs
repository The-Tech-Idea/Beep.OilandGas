using System;

namespace Beep.OilandGas.ProductionAccounting.Exceptions
{
    /// <summary>
    /// Base exception class for production accounting errors.
    /// </summary>
    public class ProductionAccountingException : Exception
    {
        public ProductionAccountingException() : base() { }
        public ProductionAccountingException(string message) : base(message) { }
        public ProductionAccountingException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when crude oil data is invalid.
    /// </summary>
    public class InvalidCrudeOilDataException : ProductionAccountingException
    {
        public string ParameterName { get; }

        public InvalidCrudeOilDataException(string parameterName, string message)
            : base($"Invalid {parameterName}: {message}")
        {
            ParameterName = parameterName;
        }
    }

    /// <summary>
    /// Exception thrown when lease data is invalid.
    /// </summary>
    public class InvalidLeaseDataException : ProductionAccountingException
    {
        public string ParameterName { get; }

        public InvalidLeaseDataException(string parameterName, string message)
            : base($"Invalid {parameterName}: {message}")
        {
            ParameterName = parameterName;
        }
    }

    /// <summary>
    /// Exception thrown when measurement data is invalid.
    /// </summary>
    public class InvalidMeasurementDataException : ProductionAccountingException
    {
        public string ParameterName { get; }

        public InvalidMeasurementDataException(string parameterName, string message)
            : base($"Invalid {parameterName}: {message}")
        {
            ParameterName = parameterName;
        }
    }

    /// <summary>
    /// Exception thrown when allocation cannot be performed.
    /// </summary>
    public class AllocationException : ProductionAccountingException
    {
        public AllocationException(string message) : base(message) { }
    }
}

