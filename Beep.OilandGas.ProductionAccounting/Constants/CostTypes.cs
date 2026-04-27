namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// Cost type constants per ASC 932 (Oil &amp; Gas Accounting).
    /// </summary>
    public static class CostTypes
    {
        public const string Exploration = "EXPLORATION";
        public const string Development = "DEVELOPMENT";
        public const string Acquisition = "ACQUISITION";
        public const string Production = "PRODUCTION";

        /// <summary>Impairment of unproved property / leasehold (ASC 360 / industry practice).</summary>
        public const string Impairment = "IMPAIRMENT";

        /// <summary>Write-off triggered when lease expires with unproved costs.</summary>
        public const string ExpiryWriteOff = "EXPIRY_WRITE_OFF";
    }
}

