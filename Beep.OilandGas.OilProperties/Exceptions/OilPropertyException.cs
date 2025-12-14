using System;

namespace Beep.OilandGas.OilProperties.Exceptions
{
    /// <summary>
    /// Base exception for oil property calculations.
    /// </summary>
    public class OilPropertyException : Exception
    {
        public OilPropertyException()
            : base()
        {
        }

        public OilPropertyException(string message)
            : base(message)
        {
        }

        public OilPropertyException(string message, Exception innerException)
            : base(message, innerException)
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

