namespace Beep.OilandGas.ProspectIdentification
{
    /// <summary>
    /// Canonical codes for exploration reference tables and process orchestration.
    /// Align with <c>R_LEAD_STATUS</c>/<c>R_EXPLORATION_REFERENCE_CODE</c> seeding in <c>ExplorationModule</c>
    /// and exploration rows in <c>ProcessDefinitionInitializer</c> (LifeCycle).
    /// </summary>
    public static partial class ExplorationReferenceCodes
    {
        public const string LeadStatusActive = "ACTIVE";

        /// <summary>Written to <c>LEAD.LEAD_STATUS</c> after a prospect is created from the lead workflow.</summary>
        public const string LeadStatusPromotedToProspect = "PROSPECT";

        public const string LeadStatusClosed = "CLOSED";

        /// <summary>
        /// Shared literal where the product uses the same token for module registration, process-definition type (<c>IProcessService</c>), and exploratory <c>WELL.WELL_TYPE</c> filtering.
        /// </summary>
        public const string ExplorationCategoryToken = "EXPLORATION";

        /// <summary><c>GetProcessDefinitionsByTypeAsync</c> and related process catalog filtering.</summary>
        public const string ProcessTypeExploration = ExplorationCategoryToken;

        /// <summary><c>ExplorationModule.ModuleId</c> — module pipeline / schema grouping.</summary>
        public const string ExplorationModuleRegistryId = ExplorationCategoryToken;

        /// <summary>PPDM <c>WELL.WELL_TYPE</c> when listing exploratory wells for a field.</summary>
        public const string PpdmWellTypeExploration = ExplorationCategoryToken;

        /// <summary>Initial field lifecycle phase for new fields; same literal as <see cref="ExplorationCategoryToken"/> (LifeCycle field phase tables).</summary>
        public const string FieldLifecyclePhaseExploration = ExplorationCategoryToken;
    }
}
