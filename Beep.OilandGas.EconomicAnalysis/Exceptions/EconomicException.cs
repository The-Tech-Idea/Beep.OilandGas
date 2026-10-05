using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.EconomicAnalysis.Exceptions
{
    /// <summary>
    /// The economic values a caller sent cannot be analysed.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Its members — invalid economic data, an IRR that does not converge on the cash flows given — refuse
    /// what the caller sent, so the family is a <see cref="RefusalException"/> of kind <see cref="RefusalKind.Invalid"/>,
    /// answered 400 with its sentence rather than as a failure.
    /// </remarks>
    public class EconomicException : RefusalException
    {
        public EconomicException() : base(RefusalKind.Invalid, "The economic analysis cannot be performed on the values given.") { }
        public EconomicException(string message) : base(RefusalKind.Invalid, message) { }
        public EconomicException(string message, Exception innerException) : base(RefusalKind.Invalid, message, innerException) { }
    }

    public class InvalidEconomicDataException : EconomicException
    {
        public string ParameterName { get; }

        public InvalidEconomicDataException(string parameterName, string message)
            : base($"Invalid {parameterName}: {message}")
        {
            ParameterName = parameterName;
        }
    }

    public class IRRConvergenceException : EconomicException
    {
        public IRRConvergenceException(string message) : base($"IRR calculation failed to converge: {message}") { }
    }
}

