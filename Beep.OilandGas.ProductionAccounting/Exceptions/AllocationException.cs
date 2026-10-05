using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.ProductionAccounting.Exceptions
{
    /// <summary>
    /// An allocation cannot be made from what the caller sent or from the ownership recorded (a refusal).
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01: every throw of this family refuses the caller's values (<see cref="RefusalKind.Invalid"/>, the
    /// default) or the recorded data the allocation needs (<see cref="RefusalKind.Conflict"/>, named at the throw).
    /// </remarks>
    public class AllocationException : RefusalException
    {
        public AllocationException(string message) : base(RefusalKind.Invalid, message) { }
        public AllocationException(RefusalKind kind, string message) : base(kind, message) { }
        public AllocationException(string message, Exception innerException) : base(RefusalKind.Invalid, message, innerException) { }
    }
}
