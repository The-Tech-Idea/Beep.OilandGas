using System;

namespace Beep.OilandGas.WellTestAnalysis.Exceptions
{
    /// <summary>
    /// Base exception class for well test analysis errors.
    /// </summary>
    public class WellTestException : Exception
    {
        public WellTestException() : base() { }

        public WellTestException(string message) : base(message) { }

        public WellTestException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when input data is invalid.
    /// </summary>
    public class InvalidWellTestDataException : WellTestException
    {
        public string ParameterName { get; }

        public InvalidWellTestDataException(string parameterName, string message) 
            : base($"Invalid {parameterName}: {message}")
        {
            ParameterName = parameterName;
        }
    }

    /// <summary>
    /// Exception thrown when analysis fails to converge.
    /// </summary>
    public class AnalysisConvergenceException : WellTestException
    {
        public AnalysisConvergenceException(string message) : base($"Analysis failed to converge: {message}") { }
    }

    /// <summary>
    /// Exception thrown when insufficient data is provided.
    /// </summary>
    public class InsufficientDataException : WellTestException
    {
        public InsufficientDataException(string message) : base($"Insufficient data: {message}") { }
    }
}

