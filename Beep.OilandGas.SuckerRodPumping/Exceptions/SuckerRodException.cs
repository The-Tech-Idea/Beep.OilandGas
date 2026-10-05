using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.SuckerRodPumping.Exceptions
{
    /// <summary>
    /// Base exception for sucker rod pumping calculations.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Every throw of this family refuses what the caller sent — system or rod-string properties out
    /// of range, or a design whose rod stress exceeds the safe limit — so it is a <see cref="RefusalException"/>
    /// (<see cref="RefusalKind.Invalid"/>) and the API answers it as a 400 with its sentence. Its message is written
    /// for the person, never taken from a caught exception.
    /// </remarks>
    public class SuckerRodException : RefusalException
    {
        public SuckerRodException()
            : base(RefusalKind.Invalid, "The sucker rod calculation could not be done with the values given.")
        {
        }

        public SuckerRodException(string message)
            : base(RefusalKind.Invalid, message)
        {
        }

        public SuckerRodException(string message, Exception innerException)
            : base(RefusalKind.Invalid, message, innerException)
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

