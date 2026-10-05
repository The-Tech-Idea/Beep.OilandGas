using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.ProductionAccounting.Exceptions
{
    /// <summary>
    /// A royalty cannot be calculated or paid as asked: the recorded terms, prices or obligations do not allow it, or the
    /// request does not (a refusal, answered with its sentence).
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01: by default <see cref="RefusalKind.Conflict"/> — most throws refuse because of the data as recorded
    /// (no effective royalty interest, a reservation awaiting reconciliation); a throw refusing the request itself names
    /// <see cref="RefusalKind.Invalid"/>, a missing record <see cref="RefusalKind.NotFound"/>. Where a failure leaves a
    /// reserved calculation or payment to reconcile, the service reports the failure first and then refuses with that
    /// instruction — the failure is never only wrapped.
    /// </remarks>
    public class RoyaltyException : RefusalException
    {
        public RoyaltyException(string message) : base(RefusalKind.Conflict, message) { }
        public RoyaltyException(RefusalKind kind, string message) : base(kind, message) { }
        public RoyaltyException(string message, Exception innerException) : base(RefusalKind.Conflict, message, innerException) { }
    }
}
