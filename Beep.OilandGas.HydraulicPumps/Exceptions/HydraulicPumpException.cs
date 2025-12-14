using System;

namespace Beep.OilandGas.HydraulicPumps.Exceptions
{
    /// <summary>
    /// Base exception for hydraulic pump calculations.
    /// </summary>
    public class HydraulicPumpException : Exception
    {
        public HydraulicPumpException()
            : base()
        {
        }

        public HydraulicPumpException(string message)
            : base(message)
        {
        }

        public HydraulicPumpException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when well properties are invalid.
    /// </summary>
    public class InvalidWellPropertiesException : HydraulicPumpException
    {
        public InvalidWellPropertiesException()
            : base("Hydraulic pump well properties are invalid.")
        {
        }

        public InvalidWellPropertiesException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when pump properties are invalid.
    /// </summary>
    public class InvalidPumpPropertiesException : HydraulicPumpException
    {
        public InvalidPumpPropertiesException()
            : base("Hydraulic pump properties are invalid.")
        {
        }

        public InvalidPumpPropertiesException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when calculation parameters are out of valid range.
    /// </summary>
    public class HydraulicPumpParameterOutOfRangeException : HydraulicPumpException
    {
        public string ParameterName { get; }

        public HydraulicPumpParameterOutOfRangeException(string parameterName, string message)
            : base(message)
        {
            ParameterName = parameterName;
        }
    }
}

