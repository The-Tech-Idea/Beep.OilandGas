using System;

namespace Beep.OilandGas.Accounting.Exceptions
{
    /// <summary>
    /// Base exception class for accounting errors.
    /// </summary>
    public class AccountingException : Exception
    {
        public AccountingException() : base() { }
        public AccountingException(string message) : base(message) { }
        public AccountingException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when accounting data is invalid.
    /// </summary>
    public class InvalidAccountingDataException : AccountingException
    {
        public string ParameterName { get; }

        public InvalidAccountingDataException(string parameterName, string message)
            : base($"Invalid {parameterName}: {message}")
        {
            ParameterName = parameterName;
        }
    }

    /// <summary>
    /// Exception thrown when reserves are insufficient for calculation.
    /// </summary>
    public class InsufficientReservesException : AccountingException
    {
        public InsufficientReservesException(string message) : base($"Insufficient reserves: {message}") { }
    }
}

