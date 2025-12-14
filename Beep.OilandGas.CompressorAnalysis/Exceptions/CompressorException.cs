using System;

namespace Beep.OilandGas.CompressorAnalysis.Exceptions
{
    /// <summary>
    /// Base exception for compressor calculations.
    /// </summary>
    public class CompressorException : Exception
    {
        public CompressorException()
            : base()
        {
        }

        public CompressorException(string message)
            : base(message)
        {
        }

        public CompressorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when operating conditions are invalid.
    /// </summary>
    public class InvalidOperatingConditionsException : CompressorException
    {
        public InvalidOperatingConditionsException()
            : base("Compressor operating conditions are invalid.")
        {
        }

        public InvalidOperatingConditionsException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when compressor properties are invalid.
    /// </summary>
    public class InvalidCompressorPropertiesException : CompressorException
    {
        public InvalidCompressorPropertiesException()
            : base("Compressor properties are invalid.")
        {
        }

        public InvalidCompressorPropertiesException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when calculation parameters are out of valid range.
    /// </summary>
    public class CompressorParameterOutOfRangeException : CompressorException
    {
        public string ParameterName { get; }

        public CompressorParameterOutOfRangeException(string parameterName, string message)
            : base(message)
        {
            ParameterName = parameterName;
        }
    }

    /// <summary>
    /// Exception thrown when compressor operation is not feasible.
    /// </summary>
    public class CompressorNotFeasibleException : CompressorException
    {
        public CompressorNotFeasibleException()
            : base("Compressor operation is not feasible with given conditions.")
        {
        }

        public CompressorNotFeasibleException(string message)
            : base(message)
        {
        }
    }
}

