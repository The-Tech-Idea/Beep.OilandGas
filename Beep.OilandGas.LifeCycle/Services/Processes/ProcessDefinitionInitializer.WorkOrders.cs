using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.ProspectIdentification;

namespace Beep.OilandGas.LifeCycle.Services.Processes
{
    /// <summary>
    /// Work Order Workflows and Approval/Gate Review workflows.
    /// Phase 2 Business Process Branch — 14 additional process definitions.
    ///
    /// Standards: ISO 55001:2018 (asset management), ISO 14224:2016 (equipment failure),
    /// IOGP S-501 (contractor management), SPE-PRMS 2018 (resource classification gate reviews).
    /// </summary>
    public partial class ProcessDefinitionInitializer
    {
        // ─────────────────────────────────────────────────────────────────
        //  Category: Work Order Workflows (6 processes)
        // ─────────────────────────────────────────────────────────────────

        public async Task InitializeWorkOrderWorkflowsAsync(string userId)
        {
            await InitializePreventiveMaintenanceWOAsync(userId);
            await InitializeCorrectiveMaintenanceWOAsync(userId);
            await InitializeInspectionWOAsync(userId);
            await InitializeHotWorkPermitWOAsync(userId);
            await InitializeConfinedSpaceEntryWOAsync(userId);
            await InitializeModificationWOAsync(userId);
        }

        private async Task InitializePreventiveMaintenanceWOAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "WO_PREVENTIVE_MAINTENANCE",
                ProcessName  = "PreventiveMaintenance",
                ProcessType  = "WORK_ORDER",
                EntityType   = "EQUIPMENT",
                Description  = "ISO 55001 planned maintenance work order — schedule, execute, verify, close.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "PM_SCHEDULE",   StepName = "Schedule PM",             SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "PM_PARTS_CHECK" },
                    new() { StepId = "PM_PARTS_CHECK",StepName = "Parts & Material Check",   SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "PM_SAFETY_REVIEW" },
                    new() { StepId = "PM_SAFETY_REVIEW",StepName = "Safety Review (PTW)",    SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "PM_EXECUTION" },
                    new() { StepId = "PM_EXECUTION",  StepName = "Execute Maintenance",      SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "PM_INSPECTION" },
                    new() { StepId = "PM_INSPECTION", StepName = "Post-Work Inspection",     SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "PM_CLOSURE" },
                    new() { StepId = "PM_CLOSURE",    StepName = "WO Closure & KPI Record",  SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeCorrectiveMaintenanceWOAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "WO_CORRECTIVE_MAINTENANCE",
                ProcessName  = "CorrectiveMaintenance",
                ProcessType  = "WORK_ORDER",
                EntityType   = "EQUIPMENT",
                Description  = "ISO 14224 corrective maintenance — failure report, fault isolation, repair, test.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "CM_FAILURE_REPORT",   StepName = "Failure Report",          SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "CM_FAULT_ISOLATION" },
                    new() { StepId = "CM_FAULT_ISOLATION",  StepName = "Fault Isolation",          SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "CM_PRIORITY_ASSIGN" },
                    new() { StepId = "CM_PRIORITY_ASSIGN",  StepName = "Priority Assignment",      SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "CM_REPAIR" },
                    new() { StepId = "CM_REPAIR",           StepName = "Repair Execution",         SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "CM_FUNCTION_TEST" },
                    new() { StepId = "CM_FUNCTION_TEST",    StepName = "Function Test",            SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "CM_CLOSURE" },
                    new() { StepId = "CM_CLOSURE",          StepName = "Close-Out & RCA Update",   SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeInspectionWOAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "WO_INSPECTION",
                ProcessName  = "InspectionWorkOrder",
                ProcessType  = "WORK_ORDER",
                EntityType   = "EQUIPMENT",
                Description  = "Scheduled inspection work order — plan, execute NDT, record findings, follow up.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "INSP_PLAN",      StepName = "Inspection Plan",         SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "INSP_EXECUTE" },
                    new() { StepId = "INSP_EXECUTE",   StepName = "Execute Inspection / NDT", SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "INSP_FINDINGS" },
                    new() { StepId = "INSP_FINDINGS",  StepName = "Record Findings",          SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "INSP_REVIEW" },
                    new() { StepId = "INSP_REVIEW",    StepName = "Engineering Review",       SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "INSP_FOLLOWUP" },
                    new() { StepId = "INSP_FOLLOWUP",  StepName = "Follow-Up Actions",        SequenceNumber = 5, StepType = "ACTION",   IsRequired = false, NextStepId = "INSP_CLOSURE" },
                    new() { StepId = "INSP_CLOSURE",   StepName = "Inspection Closure",       SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeHotWorkPermitWOAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "WO_HOT_WORK_PERMIT",
                ProcessName  = "HotWorkPermit",
                ProcessType  = "WORK_ORDER",
                EntityType   = "FACILITY",
                Description  = "Hot work permit (welding, grinding, cutting) — NFPA 51B / API RP 505 compliant.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "HW_REQUEST",      StepName = "Hot Work Request",          SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "HW_AREA_CHECK" },
                    new() { StepId = "HW_AREA_CHECK",   StepName = "Area Hazard Check",          SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "HW_GAS_TEST" },
                    new() { StepId = "HW_GAS_TEST",     StepName = "Gas Test",                   SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "HW_APPROVAL" },
                    new() { StepId = "HW_APPROVAL",     StepName = "Safety Officer Approval",    SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "HW_EXECUTION" },
                    new() { StepId = "HW_EXECUTION",    StepName = "Hot Work Execution",         SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "HW_AREA_CLEAR" },
                    new() { StepId = "HW_AREA_CLEAR",   StepName = "Area Clear & Permit Close",  SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeConfinedSpaceEntryWOAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "WO_CONFINED_SPACE_ENTRY",
                ProcessName  = "ConfinedSpaceEntry",
                ProcessType  = "WORK_ORDER",
                EntityType   = "FACILITY",
                Description  = "Confined space entry permit — OSHA 29 CFR 1910.146 / API RP 2217A compliant.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "CSE_PERMIT_REQUEST",  StepName = "Entry Permit Request",       SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "CSE_ATMOSPHERIC_TEST" },
                    new() { StepId = "CSE_ATMOSPHERIC_TEST",StepName = "Atmospheric Testing",         SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "CSE_RESCUE_PLAN" },
                    new() { StepId = "CSE_RESCUE_PLAN",     StepName = "Rescue Plan Review",          SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "CSE_SUPERVISOR_APPROVE" },
                    new() { StepId = "CSE_SUPERVISOR_APPROVE", StepName = "Supervisor Approval",      SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "CSE_ENTRY" },
                    new() { StepId = "CSE_ENTRY",           StepName = "Entry and Work Execution",    SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "CSE_EXIT_VERIFY" },
                    new() { StepId = "CSE_EXIT_VERIFY",     StepName = "Exit Verification & Closure", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeModificationWOAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "WO_MODIFICATION",
                ProcessName  = "ModificationWorkOrder",
                ProcessType  = "WORK_ORDER",
                EntityType   = "FACILITY",
                Description  = "Management of Change (MoC) work order — OSHA PSM / API RP 750 MoC process.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "MOD_CHANGE_REQUEST",  StepName = "Change Request",              SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "MOD_RISK_ASSESSMENT" },
                    new() { StepId = "MOD_RISK_ASSESSMENT", StepName = "Risk Assessment (HAZOP/MOC)",  SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "MOD_ENGINEERING_REVIEW" },
                    new() { StepId = "MOD_ENGINEERING_REVIEW", StepName = "Engineering Review",        SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "MOD_APPROVAL" },
                    new() { StepId = "MOD_APPROVAL",        StepName = "Management Approval",          SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "MOD_IMPLEMENTATION" },
                    new() { StepId = "MOD_IMPLEMENTATION",  StepName = "Implementation",               SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "MOD_COMMISSIONING" },
                    new() { StepId = "MOD_COMMISSIONING",   StepName = "Commissioning & Documentation", SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = "MOD_CLOSURE" },
                    new() { StepId = "MOD_CLOSURE",         StepName = "MoC Closure",                  SequenceNumber = 7, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Category: Approval & Gate Reviews (8 processes)
        // ─────────────────────────────────────────────────────────────────

        public async Task InitializeGateReviewWorkflowsAsync(string userId)
        {
            await InitializeExplorationGateReviewAsync(userId);
            await InitializeDevelopmentFDPGateAsync(userId);
            await InitializeFinalInvestmentDecisionGateAsync(userId);
            await InitializeFirstOilGateAsync(userId);
            await InitializeAFEApprovalGateAsync(userId);
            await InitializeWellDesignApprovalGateAsync(userId);
            await InitializePipelineRouteApprovalGateAsync(userId);
            await InitializeAbandonmentApprovalGateAsync(userId);
        }

        private async Task InitializeExplorationGateReviewAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = ExplorationReferenceCodes.ProcessIdGateExplorationReview,
                ProcessName  = ExplorationReferenceCodes.ProcessNameExplorationGateReview,
                ProcessType  = ExplorationReferenceCodes.ProcessTypeGateReview,
                EntityType   = ExplorationReferenceCodes.EntityTypeExplorationGateReview,
                Description  = "SPE-PRMS 2018 exploration gate — risked resources, well plan, budget sanction.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = ExplorationReferenceCodes.StepGateExplorationPackage,   StepName = "Prepare Exploration Package",  SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = ExplorationReferenceCodes.StepGateExplorationResources },
                    new() { StepId = ExplorationReferenceCodes.StepGateExplorationResources, StepName = "Risked Resources Estimate",     SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = ExplorationReferenceCodes.StepGateExplorationEconomics },
                    new() { StepId = ExplorationReferenceCodes.StepGateExplorationEconomics, StepName = "Economic Screening (EMV)",      SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = ExplorationReferenceCodes.StepGateExplorationApproval },
                    new() { StepId = ExplorationReferenceCodes.StepGateExplorationApproval,  StepName = "Management Gate Approval",      SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = ExplorationReferenceCodes.StepGateExplorationCommit },
                    new() { StepId = ExplorationReferenceCodes.StepGateExplorationCommit,    StepName = "Drill Commitment & AFE",        SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeDevelopmentFDPGateAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "GATE_FDP_REVIEW",
                ProcessName  = "FDPGateReview",
                ProcessType  = "GATE_REVIEW",
                EntityType   = "PROJECT",
                Description  = "Field Development Plan gate — FDP submission, PRMS 2C upgrade, partner alignment.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "FDP_DRAFT",          StepName = "FDP Drafting",                  SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "FDP_RESERVES_CLASS" },
                    new() { StepId = "FDP_RESERVES_CLASS", StepName = "PRMS Reserves Classification",   SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "FDP_ECONOMICS" },
                    new() { StepId = "FDP_ECONOMICS",      StepName = "Project Economics Review",        SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "FDP_PARTNER_ALIGN" },
                    new() { StepId = "FDP_PARTNER_ALIGN",  StepName = "Partner Alignment",               SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "FDP_APPROVAL" },
                    new() { StepId = "FDP_APPROVAL",       StepName = "Board / Regulator Approval",      SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "FDP_SANCTION" },
                    new() { StepId = "FDP_SANCTION",       StepName = "Project Sanction",                SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeFinalInvestmentDecisionGateAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "GATE_FID",
                ProcessName  = "FinalInvestmentDecision",
                ProcessType  = "GATE_REVIEW",
                EntityType   = "PROJECT",
                Description  = "FID gate — project sanction with full cost estimate, schedule, risk register.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "FID_COST_ESTIMATE",  StepName = "Final Cost Estimate (± 10%)",   SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "FID_SCHEDULE" },
                    new() { StepId = "FID_SCHEDULE",       StepName = "Project Schedule Baseline",      SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "FID_RISK_REGISTER" },
                    new() { StepId = "FID_RISK_REGISTER",  StepName = "Risk Register Sign-Off",         SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "FID_BOARD_REVIEW" },
                    new() { StepId = "FID_BOARD_REVIEW",   StepName = "Board Review",                   SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "FID_APPROVAL" },
                    new() { StepId = "FID_APPROVAL",       StepName = "FID Approval",                   SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "FID_CONTRACTS" },
                    new() { StepId = "FID_CONTRACTS",      StepName = "Contract Award",                 SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeFirstOilGateAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "GATE_FIRST_OIL",
                ProcessName  = "FirstOilGate",
                ProcessType  = "GATE_REVIEW",
                EntityType   = "FACILITY",
                Description  = "First Oil readiness gate — punch list, safety case, regulatory clearance.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "FO_PUNCH_LIST",      StepName = "Punch List Completion",          SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "FO_SAFETY_CASE" },
                    new() { StepId = "FO_SAFETY_CASE",     StepName = "Safety Case Acceptance",         SequenceNumber = 2, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "FO_REG_CLEARANCE" },
                    new() { StepId = "FO_REG_CLEARANCE",   StepName = "Regulatory Clearance",           SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "FO_FIRST_FLOW" },
                    new() { StepId = "FO_FIRST_FLOW",      StepName = "First Flow Achieved",            SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "FO_PRODUCTION_RAMP" },
                    new() { StepId = "FO_PRODUCTION_RAMP", StepName = "Production Ramp-Up",             SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeAFEApprovalGateAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "GATE_AFE_APPROVAL",
                ProcessName  = "AFEApproval",
                ProcessType  = "GATE_REVIEW",
                EntityType   = "FINANCE",
                Description  = "Authority for Expenditure approval gate — SEC 10-K capital expenditure governance.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "AFE_PREPARE",        StepName = "AFE Preparation",                SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "AFE_ENGINEERING_REVIEW" },
                    new() { StepId = "AFE_ENGINEERING_REVIEW", StepName = "Engineering & Cost Review",  SequenceNumber = 2, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "AFE_PARTNER_VOTE" },
                    new() { StepId = "AFE_PARTNER_VOTE",   StepName = "Partner Vote (JOA)",             SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "AFE_AUTHORIZATION" },
                    new() { StepId = "AFE_AUTHORIZATION",  StepName = "Final Authorization",            SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "AFE_ACTIVE" },
                    new() { StepId = "AFE_ACTIVE",         StepName = "AFE Active — Cost Tracking",     SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeWellDesignApprovalGateAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "GATE_WELL_DESIGN",
                ProcessName  = "WellDesignApproval",
                ProcessType  = "GATE_REVIEW",
                EntityType   = "WELL",
                Description  = "Well design approval — API RP 100-1/100-2 well design review and regulatory APD.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "WD_WELL_PROGRAM",    StepName = "Well Program Preparation",       SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "WD_CASING_DESIGN" },
                    new() { StepId = "WD_CASING_DESIGN",   StepName = "Casing & Cement Design",         SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "WD_ENGINEERING_REVIEW" },
                    new() { StepId = "WD_ENGINEERING_REVIEW", StepName = "Engineering Review",          SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "WD_APD_SUBMISSION" },
                    new() { StepId = "WD_APD_SUBMISSION",  StepName = "APD / Permit Submission",        SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "WD_REGULATORY_APPROVE" },
                    new() { StepId = "WD_REGULATORY_APPROVE", StepName = "Regulatory APD Approval",    SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "WD_SPUD_AUTH" },
                    new() { StepId = "WD_SPUD_AUTH",       StepName = "Spud Authorization",             SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializePipelineRouteApprovalGateAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "GATE_PIPELINE_ROUTE",
                ProcessName  = "PipelineRouteApproval",
                ProcessType  = "GATE_REVIEW",
                EntityType   = "PIPE_STRING",
                Description  = "Pipeline route approval — ASME B31.8 (gas) / B31.4 (liquid) design gate.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "PR_ROUTE_SURVEY",    StepName = "Route Survey",                   SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "PR_ENVIRONMENTAL" },
                    new() { StepId = "PR_ENVIRONMENTAL",   StepName = "Environmental Assessment",        SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "PR_DESIGN_REVIEW" },
                    new() { StepId = "PR_DESIGN_REVIEW",   StepName = "Engineering Design Review",       SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "PR_REGULATORY_PERMIT" },
                    new() { StepId = "PR_REGULATORY_PERMIT", StepName = "Regulatory Permit",             SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "PR_APPROVED" },
                    new() { StepId = "PR_APPROVED",        StepName = "Route Approval — Proceed to FID", SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeAbandonmentApprovalGateAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "GATE_ABANDONMENT_APPROVAL",
                ProcessName  = "AbandonmentApproval",
                ProcessType  = "GATE_REVIEW",
                EntityType   = "WELL",
                Description  = "Well / facility abandonment sanction gate — SEC reserve de-booking, regulatory P&A.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "AB_ECON_REVIEW",     StepName = "Economic Review",                SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "AB_RESERVES_DEBOOK" },
                    new() { StepId = "AB_RESERVES_DEBOOK", StepName = "Reserves De-Booking (PRMS)",     SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "AB_ABANDONMENT_PLAN" },
                    new() { StepId = "AB_ABANDONMENT_PLAN",StepName = "Abandonment Plan Preparation",   SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "AB_PARTNER_CONSENT" },
                    new() { StepId = "AB_PARTNER_CONSENT", StepName = "Partner Consent (JOA)",          SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "AB_REGULATORY" },
                    new() { StepId = "AB_REGULATORY",      StepName = "Regulatory Sanction",            SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "AB_EXECUTE" },
                    new() { StepId = "AB_EXECUTE",         StepName = "Proceed to P&A",                 SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }
    }
}
