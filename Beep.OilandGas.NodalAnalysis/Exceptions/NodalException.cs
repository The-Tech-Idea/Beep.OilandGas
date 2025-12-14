using System;

namespace Beep.OilandGas.NodalAnalysis.Exceptions
{
    /// <summary>
    /// Base exception class for nodal analysis errors.
    /// </summary>
    public class NodalException : Exception
    {
        public NodalException() : base() { }
        public NodalException(string message) : base(message) { }
        public NodalException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when input data is invalid.
    /// </summary>
    public class InvalidNodalDataException : NodalException
    {
        public string ParameterName { get; }

        public InvalidNodalDataException(string parameterName, string message)
            : base($"Invalid {parameterName}: {message}")
        {
            ParameterName = parameterName;
        }
    }

    /// <summary>
    /// Exception thrown when curves do not intersect.
    /// </summary>
    public class NoIntersectionException : NodalException
    {
        public NoIntersectionException(string message) : base($"No intersection found: {message}") { }
    }
}

