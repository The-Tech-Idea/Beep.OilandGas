using System;

namespace Beep.OilandGas.ProductionAccounting.Exceptions
{
    /// <summary>
    /// A production accounting operation failed: carries the failure that stopped it, with the operation's context.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. This type wraps failures ("Failed to close period for field F", with the cause inside); the API reports
    /// it and answers 500 with its reference. A refusal of what the caller sent, or of the state of their data, is a
    /// <see cref="Beep.OilandGas.Models.Core.Refusals.RefusalException"/> — the families <see cref="AllocationException"/>,
    /// <see cref="AccountingException"/>, <see cref="RoyaltyException"/> and <see cref="ValidationException"/> derive from it —
    /// and the wrappers let a refusal pass through unwrapped.
    /// </remarks>
    public class ProductionAccountingException : Exception
    {
        public ProductionAccountingException(string message) : base(message) { }
        public ProductionAccountingException(string message, Exception innerException) : base(message, innerException) { }
    }
}
