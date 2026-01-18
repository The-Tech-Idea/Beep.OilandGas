namespace Beep.OilandGas.Models.Enums
{
    /// <summary>
    /// JIB (Joint Interest Billing) Status constants per COPAS standards.
    /// Tracks the lifecycle state of joint interest billing statements and allocations.
    /// </summary>
    public static class JIBStatus
    {
        /// <summary>
        /// Initial state - JIB statement created but not yet calculated
        /// </summary>
        public const string Draft = "DRAFT";

        /// <summary>
        /// Allocations have been calculated using COPAS formula
        /// </summary>
        public const string Calculated = "CALCULATED";

        /// <summary>
        /// Allocations have been distributed to all participants
        /// </summary>
        public const string Distributed = "DISTRIBUTED";

        /// <summary>
        /// All payments have been settled and statement is closed
        /// </summary>
        public const string Settled = "SETTLED";

        /// <summary>
        /// Statement has been voided and is no longer valid
        /// </summary>
        public const string Void = "VOID";
    }
}
