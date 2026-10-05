using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.PipelineAnalysis.Exceptions
{
    /// <summary>
    /// Base exception for pipeline calculations.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Every throw of this family refuses what the caller sent — pipeline or flow properties out of
    /// range — so it is a <see cref="RefusalException"/> (<see cref="RefusalKind.Invalid"/>) and the API answers it
    /// as a 400 with its sentence. Its message is written for the person, never taken from a caught exception.
    /// </remarks>
    public class PipelineException : RefusalException
    {
        public PipelineException()
            : base(RefusalKind.Invalid, "The pipeline calculation could not be done with the values given.")
        {
        }

        public PipelineException(string message)
            : base(RefusalKind.Invalid, message)
        {
        }

        public PipelineException(string message, Exception innerException)
            : base(RefusalKind.Invalid, message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when pipeline properties are invalid.
    /// </summary>
    public class InvalidPipelinePropertiesException : PipelineException
    {
        public InvalidPipelinePropertiesException()
            : base("Pipeline properties are invalid.")
        {
        }

        public InvalidPipelinePropertiesException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when flow properties are invalid.
    /// </summary>
    public class InvalidFlowPropertiesException : PipelineException
    {
        public InvalidFlowPropertiesException()
            : base("Flow properties are invalid.")
        {
        }

        public InvalidFlowPropertiesException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when calculation parameters are out of valid range.
    /// </summary>
    public class PipelineParameterOutOfRangeException : PipelineException
    {
        public string ParameterName { get; }

        public PipelineParameterOutOfRangeException(string parameterName, string message)
            : base(message)
        {
            ParameterName = parameterName;
        }
    }
}

