using Beep.OilandGas.Models.Processes;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Cross-role workflow definitions — Set 4: Asset Lifecycle & Planning (CRW-21 through CRW-25).
/// These model handoffs between Exploration, Development, Production, Reservoir, and Decommissioning.
/// Part of Phase 3 cross-role orchestration.
/// </summary>
public partial class ProcessDefinitionInitializer
{
    private async Task InitializeCrossRoleAssetLifecycleWorkflowsAsync(string userId)
    {
        await InitializeDiscoveryToDevelopmentAsync(userId);
        await InitializeFdpToFirstOilAsync(userId);
        await InitializeReservesRevisionAsync(userId);
        await InitializeDecommissioningPlanningAsync(userId);
        await InitializeAssetAcquisitionAsync(userId);
    }

    /// <summary>CRW-21: Discovery confirmed → reserves estimated → development concept → FDP initiated</summary>
    private async Task InitializeDiscoveryToDevelopmentAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_DISCOVERY_TO_DEVELOPMENT",
            ProcessName = "Discovery → Development Decision",
            ProcessType = "LIFECYCLE",
            EntityType = "FIELD",
            Description = "Discovery confirmed → reserves estimated → development concept selected → FDP initiated",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "DISCOVERY_CONFIRM", StepName = "Confirm Discovery", SequenceNumber = 1, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ExplorationGeologist"}, SlaHours = 168, NextStepId = "RESERVES_ESTIMATE", Description = "Exploration Geologist confirms commercial discovery with well test data" },
                new() { StepId = "RESERVES_ESTIMATE", StepName = "Estimate Reserves", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ReservoirEngineer"}, SlaHours = 240, NextStepId = "CONCEPT_SELECT", Description = "Reservoir Engineer estimates recoverable reserves and production profiles" },
                new() { StepId = "CONCEPT_SELECT", StepName = "Select Development Concept", SequenceNumber = 3, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"DevelopmentPlanner","ReservoirEngineer"}, SlaHours = 336, NextStepId = "FDP_INITIATE", Description = "Development Planner evaluates concepts and selects optimal development plan" },
                new() { StepId = "FDP_INITIATE", StepName = "Initiate FDP", SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Executive","Manager"}, SlaHours = 72, Description = "Executive approves initiation of Field Development Plan" },
            },
            Configuration = new() { ["category"] = "LIFECYCLE" }
        }, userId);
    }

    /// <summary>CRW-22: FDP approved → wells drilled → facilities built → first oil → handover to operations</summary>
    private async Task InitializeFdpToFirstOilAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_FDP_TO_FIRST_OIL",
            ProcessName = "FDP → First Oil",
            ProcessType = "LIFECYCLE",
            EntityType = "FIELD",
            Description = "FDP approved → wells drilled → facilities built → first oil → handover to Production",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "FDP_APPROVED", StepName = "FDP Approved", SequenceNumber = 1, StepType = "SYSTEM", IsRequired = true, NextStepId = "DRILLING_PHASE", Description = "FDP approved through gate review process" },
                new() { StepId = "DRILLING_PHASE", StepName = "Drilling Phase", SequenceNumber = 2, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"DrillingEngineer"}, SlaHours = 2160, NextStepId = "FACILITY_BUILD", Description = "Drilling Engineer executes drilling program" },
                new() { StepId = "FACILITY_BUILD", StepName = "Facility Construction", SequenceNumber = 3, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"FacilitiesEngineer"}, SlaHours = 4320, NextStepId = "COMMISSIONING", Description = "Facilities Engineer constructs production facilities" },
                new() { StepId = "COMMISSIONING", StepName = "Commissioning", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"FacilitiesEngineer","ProductionEngineer"}, SlaHours = 720, NextStepId = "FIRST_OIL", Description = "Joint commissioning by Facilities and Production" },
                new() { StepId = "FIRST_OIL", StepName = "First Oil", SequenceNumber = 5, StepType = "SYSTEM", IsRequired = true, NextStepId = "HANDOVER", Description = "First oil achieved — milestone recorded" },
                new() { StepId = "HANDOVER", StepName = "Handover to Operations", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"ProductionManager","AssetManager"}, SlaHours = 48, Description = "Formal handover from project to operations" },
            },
            Configuration = new() { ["category"] = "LIFECYCLE", ["slaTotal"] = 8064 }
        }, userId);
    }

    /// <summary>CRW-23: Annual reserves review → Reservoir calculates → peer review → Executive approval → auditor review</summary>
    private async Task InitializeReservesRevisionAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_RESERVES_REVISION",
            ProcessName = "Reserves Revision & Approval",
            ProcessType = "FINANCIAL",
            EntityType = "RESERVES_ESTIMATE",
            Description = "Annual reserves review → Reservoir calculates → peer review → Executive approval → external auditor review",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "RESERVES_CALC", StepName = "Calculate Reserves", SequenceNumber = 1, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"ReservoirEngineer"}, SlaHours = 240, NextStepId = "PEER_REVIEW", Description = "Reservoir Engineer calculates reserves using SEC definitions" },
                new() { StepId = "PEER_REVIEW", StepName = "Peer Review", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ReservoirEngineer"}, SlaHours = 120, NextStepId = "MANAGER_REVIEW", Description = "Independent peer review by another Reservoir Engineer (SoD: different person)" },
                new() { StepId = "MANAGER_REVIEW", StepName = "Manager Review", SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 72, NextStepId = "EXECUTIVE_APPROVE", Description = "Manager reviews reserves methodology and assumptions" },
                new() { StepId = "EXECUTIVE_APPROVE", StepName = "Executive Approval", SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Executive"}, SlaHours = 48, NextStepId = "AUDITOR_REVIEW", Description = "Executive approves reserves for external reporting" },
                new() { StepId = "AUDITOR_REVIEW", StepName = "External Auditor Review", SequenceNumber = 5, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Auditor"}, SlaHours = 336, Description = "External auditor reviews reserves for SEC compliance" },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "SEC Rule 4-10", ["schedule"] = "ANNUAL" }
        }, userId);
    }

    /// <summary>CRW-24: Asset reaches end of life → decommissioning plan → regulatory approval → execution → restoration</summary>
    private async Task InitializeDecommissioningPlanningAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_DECOMMISSIONING_PLANNING",
            ProcessName = "Decommissioning Planning & Execution",
            ProcessType = "DECOMMISSIONING",
            EntityType = "WELL",
            Description = "Asset reaches end of life → decommissioning plan → regulatory approval → execution → environmental restoration",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "EOL_ASSESSMENT", StepName = "End-of-Life Assessment", SequenceNumber = 1, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ProductionEngineer","ReservoirEngineer"}, SlaHours = 168, NextStepId = "DECOM_PLAN", Description = "Production and Reservoir assess that asset has reached economic limit" },
                new() { StepId = "DECOM_PLAN", StepName = "Decommissioning Plan", SequenceNumber = 2, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"DecommissioningCoordinator"}, SlaHours = 336, NextStepId = "REGULATORY_APPROVAL", Description = "Decommissioning Coordinator prepares plan including cost estimate and timeline" },
                new() { StepId = "REGULATORY_APPROVAL", StepName = "Regulatory Approval", SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"ComplianceOfficer"}, SlaHours = 720, NextStepId = "EXECUTION", Description = "Regulatory approval obtained for decommissioning plan" },
                new() { StepId = "EXECUTION", StepName = "Execute Decommissioning", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"DecommissioningCoordinator","DrillingEngineer"}, SlaHours = 2160, NextStepId = "RESTORATION", Description = "P&A operations, facility dismantling, pipeline purging" },
                new() { StepId = "RESTORATION", StepName = "Environmental Restoration", SequenceNumber = 5, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"DecommissioningCoordinator"}, SlaHours = 1440, NextStepId = "CLOSURE", Description = "Site restoration, soil remediation, revegetation" },
                new() { StepId = "CLOSURE", StepName = "Final Closure", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"ComplianceOfficer","HSEOfficer"}, SlaHours = 72, Description = "Regulatory sign-off and final closure report" },
            },
            Configuration = new() { ["category"] = "DECOMMISSIONING", ["regulation"] = "BSEE, state regulations" }
        }, userId);
    }

    /// <summary>CRW-25: Acquisition target identified → due diligence → valuation → Executive approval → Accountant books</summary>
    private async Task InitializeAssetAcquisitionAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_ASSET_ACQUISITION",
            ProcessName = "Asset Acquisition / Divestiture",
            ProcessType = "FINANCIAL",
            EntityType = "FIELD",
            Description = "Acquisition target identified → due diligence → valuation → Executive approval → Accountant books entry",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "TARGET_IDENTIFY", StepName = "Identify Target", SequenceNumber = 1, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"AssetManager"}, SlaHours = 168, NextStepId = "DUE_DILIGENCE", Description = "Asset Manager identifies acquisition or divestiture target" },
                new() { StepId = "DUE_DILIGENCE", StepName = "Due Diligence", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"AssetManager","ReservoirEngineer","Accountant"}, SlaHours = 720, NextStepId = "VALUATION", Description = "Cross-functional due diligence: technical, financial, legal, environmental" },
                new() { StepId = "VALUATION", StepName = "Valuation", SequenceNumber = 3, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant","ReservoirEngineer"}, SlaHours = 240, NextStepId = "EXECUTIVE_APPROVE", Description = "Accountant and Reservoir Engineer prepare valuation model" },
                new() { StepId = "EXECUTIVE_APPROVE", StepName = "Executive Approval", SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Executive"}, SlaHours = 72, NextStepId = "BOOK_ENTRY", Description = "Executive approves acquisition/divestiture" },
                new() { StepId = "BOOK_ENTRY", StepName = "Book Accounting Entry", SequenceNumber = 5, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 72, Description = "Accountant books the acquisition or divestiture journal entry" },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "FASB ASC 805", ["doaEnabled"] = true }
        }, userId);
    }
}
