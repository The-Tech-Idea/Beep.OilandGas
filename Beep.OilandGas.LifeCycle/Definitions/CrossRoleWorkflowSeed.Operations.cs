using Beep.OilandGas.Models.Processes;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Cross-role workflow definitions — Set 2: Operations & Engineering (CRW-09 through CRW-14).
/// These model handoffs between Drilling, Production, Reservoir, and Facilities engineers.
/// Part of Phase 3 cross-role orchestration.
/// </summary>
public partial class ProcessDefinitionInitializer
{
    private async Task InitializeCrossRoleOperationsWorkflowsAsync(string userId)
    {
        await InitializeWellHandoffDrillingToProductionAsync(userId);
        await InitializeWorkoverProposalAsync(userId);
        await InitializeFacilityModificationAsync(userId);
        await InitializeProductionOptimizationAsync(userId);
        await InitializeWellTestToReservoirModelAsync(userId);
        await InitializePipelineCapacityReviewAsync(userId);
    }

    /// <summary>CRW-09: Well completed → handoff package → Production accepts</summary>
    private async Task InitializeWellHandoffDrillingToProductionAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_WELL_HANDOFF_DRILLING_PROD",
            ProcessName = "Well Handoff: Drilling → Production",
            ProcessType = "OPERATIONAL",
            EntityType = "WELL",
            Description = "Well completed by Drilling → handoff package (wellbore diagram, completion report, test data) → Production accepts",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "PREPARE_HANDOFF", StepName = "Prepare Handoff Package", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"DrillingEngineer"}, SlaHours = 48, NextStepId = "DRILLING_SIGNOFF", Description = "Drilling Engineer prepares handoff documentation" },
                new() { StepId = "DRILLING_SIGNOFF", StepName = "Drilling Sign-Off", SequenceNumber = 2, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"DrillingEngineer"}, SlaHours = 24, NextStepId = "PROD_ACCEPT", Description = "Drilling signs off that well is ready for production" },
                new() { StepId = "PROD_ACCEPT", StepName = "Production Acceptance", SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"ProductionEngineer"}, SlaHours = 48, NextStepId = "WELL_STARTUP", Description = "Production Engineer accepts handoff and initiates well startup" },
                new() { StepId = "WELL_STARTUP", StepName = "Well Startup", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"ProductionEngineer"}, SlaHours = 72, Description = "Production Engineer performs well startup procedures" },
            },
            Configuration = new() { ["category"] = "OPERATIONS" }
        }, userId);
    }

    /// <summary>CRW-10: Production identifies declining well → workover proposal → AFE → execution</summary>
    private async Task InitializeWorkoverProposalAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_WORKOVER_PROPOSAL",
            ProcessName = "Workover Proposal → Approval → Execution",
            ProcessType = "OPERATIONAL",
            EntityType = "WELL",
            Description = "Production identifies declining well → workover proposal → Drilling evaluates → AFE → executes",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "IDENTIFY_CANDIDATE", StepName = "Identify Workover Candidate", SequenceNumber = 1, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ProductionEngineer"}, SlaHours = 72, NextStepId = "PROPOSAL", Description = "Production Engineer identifies declining well and prepares workover proposal" },
                new() { StepId = "PROPOSAL", StepName = "Workover Proposal", SequenceNumber = 2, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"ProductionEngineer"}, SlaHours = 48, NextStepId = "DRILLING_EVAL", Description = "Production Engineer submits detailed workover proposal with cost estimate" },
                new() { StepId = "DRILLING_EVAL", StepName = "Drilling Evaluation", SequenceNumber = 3, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"DrillingEngineer"}, SlaHours = 72, NextStepId = "AFE_SUBMIT", Description = "Drilling Engineer evaluates technical feasibility and provides execution plan" },
                new() { StepId = "AFE_SUBMIT", StepName = "Submit AFE for Approval", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"DrillingEngineer"}, SlaHours = 24, NextStepId = "EXECUTE", Description = "AFE submitted through DoA approval chain" },
                new() { StepId = "EXECUTE", StepName = "Execute Workover", SequenceNumber = 5, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"DrillingEngineer"}, SlaHours = 336, Description = "Drilling executes workover and reports results" },
            },
            Configuration = new() { ["category"] = "OPERATIONS", ["doaEnabled"] = true }
        }, userId);
    }

    /// <summary>CRW-11: Production needs modification → Facilities assesses → design → construction → handback</summary>
    private async Task InitializeFacilityModificationAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_FACILITY_MODIFICATION",
            ProcessName = "Facility Modification Request",
            ProcessType = "OPERATIONAL",
            EntityType = "FACILITY",
            Description = "Production needs facility modification → Facilities assesses → AFE → design → construction → handback to Production",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "MOD_REQUEST", StepName = "Submit Modification Request", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"ProductionEngineer"}, SlaHours = 48, NextStepId = "FACILITY_ASSESS", Description = "Production Engineer submits facility modification request with justification" },
                new() { StepId = "FACILITY_ASSESS", StepName = "Facilities Assessment", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"FacilitiesEngineer"}, SlaHours = 120, NextStepId = "DESIGN", Description = "Facilities Engineer assesses feasibility, cost, and timeline" },
                new() { StepId = "DESIGN", StepName = "Detail Design", SequenceNumber = 3, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"FacilitiesEngineer"}, SlaHours = 240, NextStepId = "CONSTRUCTION", Description = "Facilities Engineer completes detail design and procurement" },
                new() { StepId = "CONSTRUCTION", StepName = "Construction & Commissioning", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"FacilitiesEngineer"}, SlaHours = 720, NextStepId = "HANDBACK", Description = "Construction and commissioning of facility modification" },
                new() { StepId = "HANDBACK", StepName = "Handback to Production", SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"ProductionEngineer","ProductionManager"}, SlaHours = 48, Description = "Production accepts modified facility back into operations" },
            },
            Configuration = new() { ["category"] = "OPERATIONS", ["doaEnabled"] = true }
        }, userId);
    }

    /// <summary>CRW-12: Production data analyzed → Reservoir evaluates → recommends changes</summary>
    private async Task InitializeProductionOptimizationAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_PRODUCTION_OPTIMIZATION",
            ProcessName = "Production Optimization Review",
            ProcessType = "OPERATIONAL",
            EntityType = "WELL",
            Description = "Production data analyzed → Reservoir Engineer evaluates → recommends changes → implemented → results tracked",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "DATA_ANALYSIS", StepName = "Analyze Production Data", SequenceNumber = 1, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ProductionEngineer"}, SlaHours = 72, NextStepId = "RESERVOIR_EVAL", Description = "Production Engineer analyzes well performance trends" },
                new() { StepId = "RESERVOIR_EVAL", StepName = "Reservoir Evaluation", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ReservoirEngineer"}, SlaHours = 120, NextStepId = "RECOMMENDATION", Description = "Reservoir Engineer evaluates and makes optimization recommendations" },
                new() { StepId = "RECOMMENDATION", StepName = "Submit Recommendations", SequenceNumber = 3, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"ReservoirEngineer"}, SlaHours = 48, NextStepId = "IMPLEMENT", Description = "Recommendations submitted (e.g., choke change, gas lift adjustment, pump optimization)" },
                new() { StepId = "IMPLEMENT", StepName = "Implement Changes", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"ProductionEngineer"}, SlaHours = 72, NextStepId = "TRACK_RESULTS", Description = "Production Engineer implements optimization changes" },
                new() { StepId = "TRACK_RESULTS", StepName = "Track Results", SequenceNumber = 5, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ProductionEngineer"}, SlaHours = 168, Description = "Results tracked over 30/60/90 days, reported back to Reservoir" },
            },
            Configuration = new() { ["category"] = "OPERATIONS" }
        }, userId);
    }

    /// <summary>CRW-13: Well test completed → data validated → reservoir model updated → reserves impact</summary>
    private async Task InitializeWellTestToReservoirModelAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_WELL_TEST_TO_MODEL",
            ProcessName = "Well Test → Reservoir Model Update",
            ProcessType = "OPERATIONAL",
            EntityType = "WELL_TEST",
            Description = "Well test completed → data validated → reservoir model updated → reserves impact assessed",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "TEST_EXECUTE", StepName = "Execute Well Test", SequenceNumber = 1, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"ProductionEngineer"}, SlaHours = 72, NextStepId = "DATA_VALIDATE", Description = "Production Engineer executes well test and collects data" },
                new() { StepId = "DATA_VALIDATE", StepName = "Validate Test Data", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ReservoirEngineer"}, SlaHours = 48, NextStepId = "MODEL_UPDATE", Description = "Reservoir Engineer validates test data quality and consistency" },
                new() { StepId = "MODEL_UPDATE", StepName = "Update Reservoir Model", SequenceNumber = 3, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"ReservoirEngineer"}, SlaHours = 120, NextStepId = "RESERVES_IMPACT", Description = "Reservoir Engineer updates reservoir model with new test data" },
                new() { StepId = "RESERVES_IMPACT", StepName = "Assess Reserves Impact", SequenceNumber = 4, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ReservoirEngineer"}, SlaHours = 72, Description = "Reservoir Engineer assesses impact on reserves and reports findings" },
            },
            Configuration = new() { ["category"] = "OPERATIONS" }
        }, userId);
    }

    /// <summary>CRW-14: Production forecast → pipeline capacity check → bottleneck → debottlenecking plan</summary>
    private async Task InitializePipelineCapacityReviewAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_PIPELINE_CAPACITY",
            ProcessName = "Pipeline Capacity Review",
            ProcessType = "OPERATIONAL",
            EntityType = "PIPELINE",
            Description = "Production forecast → pipeline capacity check → bottleneck identified → debottlenecking plan",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "FORECAST_REVIEW", StepName = "Review Production Forecast", SequenceNumber = 1, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ProductionEngineer"}, SlaHours = 48, NextStepId = "CAPACITY_CHECK", Description = "Production Engineer provides 12-month production forecast" },
                new() { StepId = "CAPACITY_CHECK", StepName = "Check Pipeline Capacity", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"FacilitiesEngineer"}, SlaHours = 72, NextStepId = "BOTTLENECK_REPORT", Description = "Facilities Engineer checks pipeline capacity against forecast" },
                new() { StepId = "BOTTLENECK_REPORT", StepName = "Bottleneck Analysis", SequenceNumber = 3, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"FacilitiesEngineer"}, SlaHours = 120, NextStepId = "PLAN", Description = "Facilities Engineer identifies bottlenecks and capacity constraints" },
                new() { StepId = "PLAN", StepName = "Debottlenecking Plan", SequenceNumber = 4, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"FacilitiesEngineer"}, SlaHours = 168, Description = "Debottlenecking plan with cost estimates and timeline" },
            },
            Configuration = new() { ["category"] = "OPERATIONS" }
        }, userId);
    }
}
