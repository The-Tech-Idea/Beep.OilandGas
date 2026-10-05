using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.NodalAnalysis.Exceptions
{
    /// <summary>
    /// Base exception class for nodal analysis errors.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. The family refuses what the caller sent — invalid nodal data, IPR and VLP curves that do not
    /// intersect — so it is a <see cref="RefusalException"/> (<see cref="RefusalKind.Invalid"/>) and the API answers it as
    /// a 400 with its sentence. Its message is written for the person, never taken from a caught exception.
    /// </remarks>
    public class NodalException : RefusalException
    {
        public NodalException() : base(RefusalKind.Invalid, "The nodal analysis could not be done with the values given.") { }
        public NodalException(string message) : base(RefusalKind.Invalid, message) { }
        public NodalException(string message, Exception innerException) : base(RefusalKind.Invalid, message, innerException) { }
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

