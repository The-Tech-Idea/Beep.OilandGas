using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.HydraulicPumps.Exceptions
{
    /// <summary>
    /// Base exception for hydraulic pump calculations.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Every throw of this family refuses what the caller sent — well or pump properties out of the
    /// range the calculation accepts — so it is a <see cref="RefusalException"/> (<see cref="RefusalKind.Invalid"/>)
    /// and the API answers it as a 400 with its sentence. Its message is written for the person, never taken from a
    /// caught exception.
    /// </remarks>
    public class HydraulicPumpException : RefusalException
    {
        public HydraulicPumpException()
            : base(RefusalKind.Invalid, "The hydraulic pump calculation could not be done with the values given.")
        {
        }

        public HydraulicPumpException(string message)
            : base(RefusalKind.Invalid, message)
        {
        }

        public HydraulicPumpException(string message, Exception innerException)
            : base(RefusalKind.Invalid, message, innerException)
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

