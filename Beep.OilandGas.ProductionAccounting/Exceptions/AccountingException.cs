using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.ProductionAccounting.Exceptions
{
    /// <summary>
    /// An accounting record or amount the caller sent cannot be accepted (a refusal, answered with its sentence).
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01: every throw of this family refuses the caller's values or names a record that is not there, so it is
    /// a <see cref="RefusalException"/> — <see cref="RefusalKind.Invalid"/> unless the throw names another kind.
    /// </remarks>
    public class AccountingException : RefusalException
    {
        public AccountingException(string message) : base(RefusalKind.Invalid, message) { }
        public AccountingException(RefusalKind kind, string message) : base(kind, message) { }
        public AccountingException(string message, Exception innerException) : base(RefusalKind.Invalid, message, innerException) { }
    }
}
