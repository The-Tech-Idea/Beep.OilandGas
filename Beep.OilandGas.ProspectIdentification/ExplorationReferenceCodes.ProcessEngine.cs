namespace Beep.OilandGas.ProspectIdentification
{
    /// <summary>Process definition names, IDs, entity anchors, step IDs, and step outcomes for exploration workflows.</summary>
    public static partial class ExplorationReferenceCodes
    {
        public const string ProcessNameLeadToProspect = "LeadToProspect";
        public const string ProcessNameProspectToDiscovery = "ProspectToDiscovery";
        public const string ProcessNameDiscoveryToDevelopment = "DiscoveryToDevelopment";

        public const string ProcessIdLeadToProspect = "LEAD_TO_PROSPECT";
        public const string ProcessIdProspectToDiscovery = "PROSPECT_TO_DISCOVERY";
        public const string ProcessIdDiscoveryToDevelopment = "DISCOVERY_TO_DEVELOPMENT";

        /// <summary>Exploration gate review (seeded with <c>LifeCycle</c> <c>ProcessDefinitionInitializer</c> gate review workflows).</summary>
        public const string ProcessIdGateExplorationReview = "GATE_EXPLORATION_REVIEW";

        public const string ProcessNameExplorationGateReview = "ExplorationGateReview";

        /// <summary>Shared <c>ProcessDefinition.ProcessType</c> for SPE-PRMS-style gate definitions.</summary>
        public const string ProcessTypeGateReview = "GATE_REVIEW";

        /// <summary>Anchor <c>EntityType</c> on the exploration gate definition (matches seed; product may later align to prospect/portfolio).</summary>
        public const string EntityTypeExplorationGateReview = "POOL";

        public const string StepGateExplorationPackage = "GATE_EXP_PACKAGE";
        public const string StepGateExplorationResources = "GATE_EXP_RESOURCES";
        public const string StepGateExplorationEconomics = "GATE_EXP_ECONOMICS";
        public const string StepGateExplorationApproval = "GATE_EXP_APPROVAL";
        public const string StepGateExplorationCommit = "GATE_EXP_COMMIT";

        public const string EntityTypeLead = "LEAD";
        public const string EntityTypeProspect = "PROSPECT";
        public const string EntityTypeDiscovery = "DISCOVERY";

        public const string StepLeadCreation = "LEAD_CREATION";
        public const string StepLeadEvaluation = "LEAD_EVALUATION";
        public const string StepLeadApproval = "LEAD_APPROVAL";
        public const string StepProspectCreation = "PROSPECT_CREATION";
        public const string StepProspectAssessment = "PROSPECT_ASSESSMENT";
        public const string StepRiskAssessment = "RISK_ASSESSMENT";
        public const string StepVolumeEstimation = "VOLUME_ESTIMATION";
        public const string StepEconomicEvaluation = "ECONOMIC_EVALUATION";
        public const string StepDrillingDecision = "DRILLING_DECISION";
        public const string StepDiscoveryRecording = "DISCOVERY_RECORDING";
        public const string StepAppraisal = "APPRAISAL";
        public const string StepReserveEstimation = "RESERVE_ESTIMATION";

        /// <summary>Discovery-to-development economic step (matches <c>ProcessDefinitionInitializer</c> seed).</summary>
        public const string StepDevelopmentEconomicAnalysis = "ECONOMIC_ANALYSIS";

        public const string StepDevelopmentApproval = "DEVELOPMENT_APPROVAL";

        public const string OutcomeApproved = "APPROVED";
        public const string OutcomeRejected = "REJECTED";
        public const string OutcomeSuccess = "SUCCESS";
    }
}
