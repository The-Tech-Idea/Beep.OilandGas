using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.WellTestAnalysis.Exceptions
{
    /// <summary>
    /// Base exception class for well test analysis errors.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Every throw of this family refuses what the caller sent — test data out of range or of the wrong
    /// test type, or data the analysis cannot find a straight-line region in — so it is a <see cref="RefusalException"/>
    /// (<see cref="RefusalKind.Invalid"/>) and the API answers it as a 400 with its sentence. Its message is written for
    /// the person, never taken from a caught exception.
    /// </remarks>
    public class WellTestException : RefusalException
    {
        public WellTestException() : base(RefusalKind.Invalid, "The well test could not be analysed with the data given.") { }

        public WellTestException(string message) : base(RefusalKind.Invalid, message) { }

        public WellTestException(string message, Exception innerException) : base(RefusalKind.Invalid, message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when input data is invalid.
    /// </summary>
    public class InvalidWellTestDataException : WellTestException
    {
        public string ParameterName { get; }

        public InvalidWellTestDataException(string parameterName, string message)
            : base(message)
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

