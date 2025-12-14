using System;

namespace Beep.OilandGas.EconomicAnalysis.Exceptions
{
    public class EconomicException : Exception
    {
        public EconomicException() : base() { }
        public EconomicException(string message) : base(message) { }
        public EconomicException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InvalidEconomicDataException : EconomicException
    {
        public string ParameterName { get; }

        public InvalidEconomicDataException(string parameterName, string message)
            : base($"Invalid {parameterName}: {message}")
        {
            ParameterName = parameterName;
        }
    }

    public class IRRConvergenceException : EconomicException
    {
        public IRRConvergenceException(string message) : base($"IRR calculation failed to converge: {message}") { }
    }
}

