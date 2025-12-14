using System;

namespace Beep.OilandGas.SuckerRodPumping.Exceptions
{
    /// <summary>
    /// Base exception for sucker rod pumping calculations.
    /// </summary>
    public class SuckerRodException : Exception
    {
        public SuckerRodException()
            : base()
        {
        }

        public SuckerRodException(string message)
            : base(message)
        {
        }

        public SuckerRodException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when system properties are invalid.
    /// </summary>
    public class InvalidSystemPropertiesException : SuckerRodException
    {
        public InvalidSystemPropertiesException()
            : base("Sucker rod system properties are invalid.")
        {
        }

        public InvalidSystemPropertiesException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when rod string configuration is invalid.
    /// </summary>
    public class InvalidRodStringException : SuckerRodException
    {
        public InvalidRodStringException()
            : base("Rod string configuration is invalid.")
        {
        }

        public InvalidRodStringException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when calculation parameters are out of valid range.
    /// </summary>
    public class SuckerRodParameterOutOfRangeException : SuckerRodException
    {
        public string ParameterName { get; }

        public SuckerRodParameterOutOfRangeException(string parameterName, string message)
            : base(message)
        {
            ParameterName = parameterName;
        }
    }

    /// <summary>
    /// Exception thrown when rod stress exceeds safe limits.
    /// </summary>
    public class RodStressExceededException : SuckerRodException
    {
        public decimal CalculatedStress { get; }
        public decimal MaximumAllowableStress { get; }

        public RodStressExceededException(decimal calculatedStress, decimal maximumAllowableStress)
            : base($"Rod stress ({calculatedStress:F2} psi) exceeds maximum allowable stress ({maximumAllowableStress:F2} psi).")
        {
            CalculatedStress = calculatedStress;
            MaximumAllowableStress = maximumAllowableStress;
        }
    }
}

