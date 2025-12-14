using System;

namespace Beep.OilandGas.PipelineAnalysis.Exceptions
{
    /// <summary>
    /// Base exception for pipeline calculations.
    /// </summary>
    public class PipelineException : Exception
    {
        public PipelineException()
            : base()
        {
        }

        public PipelineException(string message)
            : base(message)
        {
        }

        public PipelineException(string message, Exception innerException)
            : base(message, innerException)
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

