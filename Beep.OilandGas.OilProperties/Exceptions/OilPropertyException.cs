using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.OilProperties.Exceptions
{
    /// <summary>
    /// Base exception for oil property calculations: the calculation refusing the values it was given.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Every throw of this family refuses what the caller sent (a pressure, temperature, API gravity or
    /// GOR outside the correlations' range), so it is a <see cref="RefusalException"/> (<see cref="RefusalKind.Invalid"/>)
    /// and the API answers it as a 400 with its sentence. Its message is written for the person.
    /// </remarks>
    public class OilPropertyException : RefusalException
    {
        public OilPropertyException()
            : base(RefusalKind.Invalid, "The oil properties could not be calculated from the values given.")
        {
        }

        public OilPropertyException(string message)
            : base(RefusalKind.Invalid, message)
        {
        }

        public OilPropertyException(string message, Exception innerException)
            : base(RefusalKind.Invalid, message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when oil property conditions are invalid.
    /// </summary>
    public class InvalidOilPropertyConditionsException : OilPropertyException
    {
        public InvalidOilPropertyConditionsException()
            : base("Oil property calculation conditions are invalid.")
        {
        }

        public InvalidOilPropertyConditionsException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when calculation parameters are out of valid range.
    /// </summary>
    public class OilPropertyParameterOutOfRangeException : OilPropertyException
    {
        public string ParameterName { get; }

        public OilPropertyParameterOutOfRangeException(string parameterName, string message)
            : base(message)
        {
            ParameterName = parameterName;
        }
    }
}

