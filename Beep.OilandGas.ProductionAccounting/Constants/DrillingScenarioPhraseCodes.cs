namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// Substrings used to classify rig-released / drilling outcome text for cost routing
    /// (<see cref="Services.DrillingScenarioAccountingService"/>).
    /// </summary>
    public static class DrillingScenarioPhraseCodes
    {
        public const string Successful = "SUCCESSFUL";
        public const string Success = "SUCCESS";
        public const string DryHole = "DRY_HOLE";
        public const string Dry = "DRY";
        public const string Sidetrack = "SIDETRACK";
        public const string PlugBack = "PLUG_BACK";
        public const string Abandoned = "ABANDONED";
        public const string Failed = "FAILED";

        /// <summary>Matches <c>ABANDONED</c> and similar tokens via <c>Contains</c>.</summary>
        public const string AbandonStem = "ABANDON";
    }

    /// <summary>Default descriptions written to <c>ACCOUNTING_COST.DESCRIPTION</c> for drilling helpers.</summary>
    public static class DrillingCostDescriptionPhrases
    {
        public const string DryHoleSalvageRecovery = "Dry hole salvage recovery";
        public const string TestWellContribution = "Test well contribution";
    }
}
