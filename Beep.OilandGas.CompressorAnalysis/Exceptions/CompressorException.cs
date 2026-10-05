using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.CompressorAnalysis.Exceptions
{
    /// <summary>
    /// Base exception for compressor calculations.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Every throw of this family refuses what the caller sent — compressor properties or operating
    /// conditions out of the range the calculation accepts — so it is a <see cref="RefusalException"/> (<see
    /// cref="RefusalKind.Invalid"/>) and the API answers it as a 400 with its sentence. Its message is written for
    /// the person, never taken from a caught exception.
    /// </remarks>
    public class CompressorException : RefusalException
    {
        public CompressorException()
            : base(RefusalKind.Invalid, "The compressor calculation could not be done with the values given.")
        {
        }

        public CompressorException(string message)
            : base(RefusalKind.Invalid, message)
        {
        }

        public CompressorException(string message, Exception innerException)
            : base(RefusalKind.Invalid, message, innerException)
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

