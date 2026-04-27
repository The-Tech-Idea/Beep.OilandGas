using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.LifeCycle.Services.Processes
{
    /// <summary>
    /// Asset Lifecycle Workflows: Well Lifecycle, Facility Lifecycle,
    /// Reservoir Management, and Pipeline Infrastructure.
    /// Phase 2 Business Process Branch — 32 additional process definitions.
    ///
    /// Standards: API RP 100-1/100-2 (well integrity), API 1160 / ASME B31.8S (pipeline integrity),
    /// SPE-PRMS 2018 (reserves), ISO 15663 (LCC), API RP 14J (facility design).
    /// </summary>
    public partial class ProcessDefinitionInitializer
    {
        // ─────────────────────────────────────────────────────────────────
        //  Category: Well Lifecycle Workflows (8 processes)
        // ─────────────────────────────────────────────────────────────────

        public async Task InitializeWellLifecycleWorkflowsAsync(string userId)
        {
            await InitializeWellSpudToTDAsync(userId);
            await InitializeWellCompletionAsync(userId);
            await InitializeWellTieinAndStartupAsync(userId);
            await InitializeWellWorkoverAsync(userId);
            await InitializeWellRecompletionAsync(userId);
            await InitializeWellSurveillanceAsync(userId);
            await InitializeWellIntegrityAssessmentAsync(userId);
            await InitializeWellPlugAndAbandonAsync(userId);
        }

        private async Task InitializeWellSpudToTDAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "WELL_LIFECYCLE_SPUD_TO_TD",
                ProcessName  = "WellSpudToTD",
                ProcessType  = "WELL_LIFECYCLE",
                EntityType   = "WELL",
                Description  = "Drill well from spud to total depth — API RP 100-1 well integrity framework.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "SPUD_RIG_UP",        StepName = "Rig Up & Spud",                  SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "SPUD_DRILL_SURFACE" },
                    new() { StepId = "SPUD_DRILL_SURFACE", StepName = "Drill & Case Surface",            SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "SPUD_DRILL_INTERMEDIATE" },
                    new() { StepId = "SPUD_DRILL_INTERMEDIATE", StepName = "Drill Intermediate Sections",SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "SPUD_DRILL_PRODUCTION" },
                    new() { StepId = "SPUD_DRILL_PRODUCTION", StepName = "Drill Production Section",     SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "SPUD_LOGGING" },
                    new() { StepId = "SPUD_LOGGING",       StepName = "Wireline Logging",                SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "SPUD_CASING_RUN" },
                    new() { StepId = "SPUD_CASING_RUN",    StepName = "Run & Cement Production Casing",  SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = "SPUD_TD_REACHED" },
                    new() { StepId = "SPUD_TD_REACHED",    StepName = "TD Reached — Confirm",            SequenceNumber = 7, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeWellCompletionAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "WELL_LIFECYCLE_COMPLETION",
                ProcessName  = "WellCompletion",
                ProcessType  = "WELL_LIFECYCLE",
                EntityType   = "WELL",
                Description  = "Well completion — perforation, stimulation, wellhead installation, pre-production test.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "COMP_PERFORATE",     StepName = "Perforation",                    SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "COMP_STIMULATION" },
                    new() { StepId = "COMP_STIMULATION",   StepName = "Hydraulic Fracturing / Stimulation",SequenceNumber = 2, StepType = "ACTION", IsRequired = false, NextStepId = "COMP_TUBING" },
                    new() { StepId = "COMP_TUBING",        StepName = "Run Tubing & Packer",             SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "COMP_WELLHEAD" },
                    new() { StepId = "COMP_WELLHEAD",      StepName = "Install Wellhead & Christmas Tree",SequenceNumber = 4, StepType = "ACTION",  IsRequired = true,  NextStepId = "COMP_PRESSURE_TEST" },
                    new() { StepId = "COMP_PRESSURE_TEST", StepName = "Pressure Test",                   SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "COMP_FLOWBACK" },
                    new() { StepId = "COMP_FLOWBACK",      StepName = "Flowback & Well Test",            SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = "COMP_APPROVAL" },
                    new() { StepId = "COMP_APPROVAL",      StepName = "Completion Acceptance",           SequenceNumber = 7, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeWellTieinAndStartupAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "WELL_LIFECYCLE_TIEIN",
                ProcessName  = "WellTieinAndStartup",
                ProcessType  = "WELL_LIFECYCLE",
                EntityType   = "WELL",
                Description  = "Well tie-in to facility and production startup — commissioning & first production.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "TIEIN_FLOWLINE",     StepName = "Flowline / Manifold Connection",  SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "TIEIN_INSTRUMENT" },
                    new() { StepId = "TIEIN_INSTRUMENT",   StepName = "Instrumentation & Safety Check",  SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "TIEIN_SAFETY_REVIEW" },
                    new() { StepId = "TIEIN_SAFETY_REVIEW",StepName = "Pre-Startup Safety Review",       SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "TIEIN_CHOKE_IN" },
                    new() { StepId = "TIEIN_CHOKE_IN",     StepName = "Choke Well In",                   SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "TIEIN_METERING" },
                    new() { StepId = "TIEIN_METERING",     StepName = "Metering Verification",           SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "TIEIN_PRODUCTION_START" },
                    new() { StepId = "TIEIN_PRODUCTION_START", StepName = "Production Start Confirmed",  SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeWellWorkoverAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "WELL_LIFECYCLE_WORKOVER",
                ProcessName  = "WellWorkover",
                ProcessType  = "WELL_LIFECYCLE",
                EntityType   = "WELL",
                Description  = "Well workover — kill, pull tubing, repair, re-run, put on production.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "WO_PROGRAM",         StepName = "Workover Program Preparation",   SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "WO_KILL" },
                    new() { StepId = "WO_KILL",            StepName = "Kill Well",                      SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "WO_PULL_TUBING" },
                    new() { StepId = "WO_PULL_TUBING",     StepName = "Pull Tubing String",             SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "WO_REMEDIATION" },
                    new() { StepId = "WO_REMEDIATION",     StepName = "Downhole Remediation",           SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "WO_RUN_TUBING" },
                    new() { StepId = "WO_RUN_TUBING",      StepName = "Run Tubing String",              SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "WO_PRESSURE_TEST" },
                    new() { StepId = "WO_PRESSURE_TEST",   StepName = "Pressure Test",                  SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = "WO_PUT_ON_PRODUCTION" },
                    new() { StepId = "WO_PUT_ON_PRODUCTION",StepName = "Return Well to Production",     SequenceNumber = 7, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeWellRecompletionAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "WELL_LIFECYCLE_RECOMPLETION",
                ProcessName  = "WellRecompletion",
                ProcessType  = "WELL_LIFECYCLE",
                EntityType   = "WELL",
                Description  = "Well recompletion to new zone — abandon old zone, perforate new, test.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "RECOMP_PROGRAM",     StepName = "Recompletion Program",           SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "RECOMP_ABANDON_OLD" },
                    new() { StepId = "RECOMP_ABANDON_OLD", StepName = "Abandon Existing Zone",          SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "RECOMP_PERFORATE" },
                    new() { StepId = "RECOMP_PERFORATE",   StepName = "Perforate New Zone",             SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "RECOMP_STIMULATE" },
                    new() { StepId = "RECOMP_STIMULATE",   StepName = "Stimulate (Optional)",           SequenceNumber = 4, StepType = "ACTION",   IsRequired = false, NextStepId = "RECOMP_PRODUCTION_TEST" },
                    new() { StepId = "RECOMP_PRODUCTION_TEST", StepName = "Production Test",            SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "RECOMP_APPROVAL" },
                    new() { StepId = "RECOMP_APPROVAL",    StepName = "Recompletion Acceptance",        SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeWellSurveillanceAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "WELL_LIFECYCLE_SURVEILLANCE",
                ProcessName  = "WellSurveillance",
                ProcessType  = "WELL_LIFECYCLE",
                EntityType   = "WELL",
                Description  = "Routine well surveillance — production test, pressure survey, fluid analysis.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "SURV_SCHEDULE",      StepName = "Schedule Surveillance",          SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "SURV_WELL_TEST" },
                    new() { StepId = "SURV_WELL_TEST",     StepName = "Well Test (Separator / MPFM)",   SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "SURV_PRESSURE_SURVEY" },
                    new() { StepId = "SURV_PRESSURE_SURVEY",StepName = "Pressure Survey (BHP/BHT)",     SequenceNumber = 3, StepType = "ACTION",   IsRequired = false, NextStepId = "SURV_FLUID_SAMPLE" },
                    new() { StepId = "SURV_FLUID_SAMPLE",  StepName = "Fluid Sampling & Analysis",      SequenceNumber = 4, StepType = "ACTION",   IsRequired = false, NextStepId = "SURV_ANALYSIS" },
                    new() { StepId = "SURV_ANALYSIS",      StepName = "Performance Analysis",           SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "SURV_RECOMMENDATIONS" },
                    new() { StepId = "SURV_RECOMMENDATIONS",StepName = "Optimization Recommendations",  SequenceNumber = 6, StepType = "ACTION",   IsRequired = false, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeWellIntegrityAssessmentAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "WELL_LIFECYCLE_INTEGRITY",
                ProcessName  = "WellIntegrityAssessment",
                ProcessType  = "WELL_LIFECYCLE",
                EntityType   = "WELL",
                Description  = "Annual well integrity assessment — API RP 100-2 / NORSOK D-010 barrier verification.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "WI_BARRIER_REVIEW",  StepName = "Barrier Element Review",         SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "WI_ANNULUS_PRESSURE" },
                    new() { StepId = "WI_ANNULUS_PRESSURE",StepName = "Annulus Pressure Monitoring",    SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "WI_XMAS_TREE_TEST" },
                    new() { StepId = "WI_XMAS_TREE_TEST",  StepName = "Christmas Tree Function Test",   SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "WI_RISK_CLASSIFICATION" },
                    new() { StepId = "WI_RISK_CLASSIFICATION", StepName = "Well Risk Classification",   SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "WI_REMEDIATION" },
                    new() { StepId = "WI_REMEDIATION",     StepName = "Remediation Actions (if any)",   SequenceNumber = 5, StepType = "ACTION",   IsRequired = false, NextStepId = "WI_SIGN_OFF" },
                    new() { StepId = "WI_SIGN_OFF",        StepName = "Well Integrity Sign-Off",         SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeWellPlugAndAbandonAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "WELL_LIFECYCLE_PA",
                ProcessName  = "WellPlugAndAbandon",
                ProcessType  = "WELL_LIFECYCLE",
                EntityType   = "WELL",
                Description  = "Well plug and abandonment — BSEE 30 CFR 250.1700 / NORSOK D-010 P&A programme.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "PA_PROGRAM",         StepName = "P&A Programme Preparation",      SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "PA_PERMIT" },
                    new() { StepId = "PA_PERMIT",          StepName = "Regulatory P&A Permit",          SequenceNumber = 2, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "PA_KILL" },
                    new() { StepId = "PA_KILL",            StepName = "Kill Well",                      SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "PA_PULL_TUBING" },
                    new() { StepId = "PA_PULL_TUBING",     StepName = "Pull Tubing & Packers",          SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "PA_SET_PLUGS" },
                    new() { StepId = "PA_SET_PLUGS",       StepName = "Set Cement Plugs",               SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "PA_VERIFY_PLUGS" },
                    new() { StepId = "PA_VERIFY_PLUGS",    StepName = "Verify Plug Integrity",          SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = "PA_CUTOFF_CASING" },
                    new() { StepId = "PA_CUTOFF_CASING",   StepName = "Cut & Cap Casing",               SequenceNumber = 7, StepType = "ACTION",   IsRequired = true,  NextStepId = "PA_SITE_RESTORE" },
                    new() { StepId = "PA_SITE_RESTORE",    StepName = "Site Restoration",               SequenceNumber = 8, StepType = "ACTION",   IsRequired = true,  NextStepId = "PA_REGULATORY_CONFIRM" },
                    new() { StepId = "PA_REGULATORY_CONFIRM", StepName = "Regulatory Abandonment Confirmation", SequenceNumber = 9, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Category: Facility Lifecycle Workflows (8 processes)
        // ─────────────────────────────────────────────────────────────────

        public async Task InitializeFacilityLifecycleWorkflowsAsync(string userId)
        {
            await InitializeFacilityFEEDAsync(userId);
            await InitializeFacilityDetailDesignAsync(userId);
            await InitializeFacilityConstructionAsync(userId);
            await InitializeFacilityCommissioningAsync(userId);
            await InitializeFacilityModificationAsync(userId);
            await InitializeFacilityTurnaroundAsync(userId);
            await InitializeFacilityIntegrityAsync(userId);
            await InitializeFacilityDecommissioningAsync(userId);
        }

        private async Task InitializeFacilityFEEDAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "FACILITY_LIFECYCLE_FEED",
                ProcessName  = "FacilityFEED",
                ProcessType  = "FACILITY_LIFECYCLE",
                EntityType   = "FACILITY",
                Description  = "Front-End Engineering Design — concept select, FEED deliverables, cost estimate ± 15%.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "FEED_CONCEPT",       StepName = "Concept Selection",              SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "FEED_PROCESS_DESIGN" },
                    new() { StepId = "FEED_PROCESS_DESIGN",StepName = "Process Design (PFD, HAZOP)",    SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "FEED_DETAILED_ENGINEERING" },
                    new() { StepId = "FEED_DETAILED_ENGINEERING", StepName = "Detailed Engineering Basis",SequenceNumber = 3, StepType = "ACTION",  IsRequired = true,  NextStepId = "FEED_COST_ESTIMATE" },
                    new() { StepId = "FEED_COST_ESTIMATE", StepName = "Cost Estimate ± 15%",            SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "FEED_REVIEW" },
                    new() { StepId = "FEED_REVIEW",        StepName = "FEED Completion Review",         SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "FEED_SANCTION" },
                    new() { StepId = "FEED_SANCTION",      StepName = "FEED Sanction to Detailed Design",SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeFacilityDetailDesignAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "FACILITY_LIFECYCLE_DETAIL_DESIGN",
                ProcessName  = "FacilityDetailDesign",
                ProcessType  = "FACILITY_LIFECYCLE",
                EntityType   = "FACILITY",
                Description  = "Detailed design — IFR, IFC deliverables, equipment procurement, ASME / API codes.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "DD_IFR",             StepName = "Issue For Review (IFR)",         SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "DD_DESIGN_REVIEW" },
                    new() { StepId = "DD_DESIGN_REVIEW",   StepName = "Design Review & HAZOP",          SequenceNumber = 2, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "DD_PROCUREMENT" },
                    new() { StepId = "DD_PROCUREMENT",     StepName = "Equipment Procurement",          SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "DD_IFC" },
                    new() { StepId = "DD_IFC",             StepName = "Issue For Construction (IFC)",   SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "DD_APPROVAL" },
                    new() { StepId = "DD_APPROVAL",        StepName = "Design Package Approval",        SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeFacilityConstructionAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "FACILITY_LIFECYCLE_CONSTRUCTION",
                ProcessName  = "FacilityConstruction",
                ProcessType  = "FACILITY_LIFECYCLE",
                EntityType   = "FACILITY",
                Description  = "Fabrication, construction, site civil, mechanical completion — ISO 14731 welding.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "CONST_SITE_PREP",    StepName = "Site Preparation",               SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "CONST_CIVIL" },
                    new() { StepId = "CONST_CIVIL",        StepName = "Civil & Structural Works",       SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "CONST_MECHANICAL" },
                    new() { StepId = "CONST_MECHANICAL",   StepName = "Mechanical Installation",        SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "CONST_PIPING" },
                    new() { StepId = "CONST_PIPING",       StepName = "Piping & Pressure Testing",      SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "CONST_ELECTRICAL" },
                    new() { StepId = "CONST_ELECTRICAL",   StepName = "Electrical & Instrumentation",   SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "CONST_MC" },
                    new() { StepId = "CONST_MC",           StepName = "Mechanical Completion (MC)",     SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeFacilityCommissioningAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "FACILITY_LIFECYCLE_COMMISSIONING",
                ProcessName  = "FacilityCommissioning",
                ProcessType  = "FACILITY_LIFECYCLE",
                EntityType   = "FACILITY",
                Description  = "Facility commissioning — pre-commissioning, commissioning, RFSU, first oil.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "COMM_PUNCH_LIST",    StepName = "Complete Punch List",            SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "COMM_PRECOMM" },
                    new() { StepId = "COMM_PRECOMM",       StepName = "Pre-Commissioning",              SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "COMM_COMMISSIONING" },
                    new() { StepId = "COMM_COMMISSIONING", StepName = "Commissioning",                  SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "COMM_RFSU" },
                    new() { StepId = "COMM_RFSU",          StepName = "Ready For Start Up (RFSU)",      SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "COMM_STARTUP" },
                    new() { StepId = "COMM_STARTUP",       StepName = "Start Up & First Production",    SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "COMM_HANDOVER" },
                    new() { StepId = "COMM_HANDOVER",      StepName = "Handover to Operations",         SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeFacilityModificationAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "FACILITY_LIFECYCLE_MODIFICATION",
                ProcessName  = "FacilityModification",
                ProcessType  = "FACILITY_LIFECYCLE",
                EntityType   = "FACILITY",
                Description  = "Brownfield facility modification — MoC, HAZOP, isolation, reinstatement.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "FMOD_MOC",           StepName = "MoC Initiation",                 SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "FMOD_HAZOP" },
                    new() { StepId = "FMOD_HAZOP",         StepName = "HAZOP / SIL Review",             SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "FMOD_DESIGN" },
                    new() { StepId = "FMOD_DESIGN",        StepName = "Modification Design",            SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "FMOD_ISOLATE" },
                    new() { StepId = "FMOD_ISOLATE",       StepName = "Safe Isolation",                 SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "FMOD_EXECUTE" },
                    new() { StepId = "FMOD_EXECUTE",       StepName = "Execute Modification",           SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "FMOD_TEST" },
                    new() { StepId = "FMOD_TEST",          StepName = "Test & Verify",                  SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = "FMOD_REINSTATE" },
                    new() { StepId = "FMOD_REINSTATE",     StepName = "Reinstate to Service",           SequenceNumber = 7, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeFacilityTurnaroundAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "FACILITY_LIFECYCLE_TURNAROUND",
                ProcessName  = "FacilityTurnaround",
                ProcessType  = "FACILITY_LIFECYCLE",
                EntityType   = "FACILITY",
                Description  = "Planned turnaround / shutdown — scope, safe isolation, execution, startup.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "TAR_SCOPE",          StepName = "Scope Development",              SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "TAR_PLANNING" },
                    new() { StepId = "TAR_PLANNING",       StepName = "Detailed Planning",              SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "TAR_MANAGEMENT_APPROVE" },
                    new() { StepId = "TAR_MANAGEMENT_APPROVE", StepName = "Management Approval",        SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "TAR_SHUTDOWN" },
                    new() { StepId = "TAR_SHUTDOWN",       StepName = "Facility Shutdown & Isolation",  SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "TAR_EXECUTION" },
                    new() { StepId = "TAR_EXECUTION",      StepName = "Turnaround Execution",           SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "TAR_RESTART" },
                    new() { StepId = "TAR_RESTART",        StepName = "Controlled Restart",             SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "TAR_POST_REVIEW" },
                    new() { StepId = "TAR_POST_REVIEW",    StepName = "Post-TAR Review",                SequenceNumber = 7, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeFacilityIntegrityAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "FACILITY_LIFECYCLE_INTEGRITY",
                ProcessName  = "FacilityIntegrityManagement",
                ProcessType  = "FACILITY_LIFECYCLE",
                EntityType   = "FACILITY",
                Description  = "Facility integrity management — API RP 754 / ISO 15663 lifecycle cost framework.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "FIM_ASSET_REGISTER",  StepName = "Asset Register Review",         SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "FIM_INSPECTION_PLAN" },
                    new() { StepId = "FIM_INSPECTION_PLAN", StepName = "Inspection Plan Development",   SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "FIM_EXECUTE_INSPECTION" },
                    new() { StepId = "FIM_EXECUTE_INSPECTION", StepName = "Execute Inspection Program", SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "FIM_ANOMALY" },
                    new() { StepId = "FIM_ANOMALY",         StepName = "Anomaly & Degradation Review",  SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "FIM_REMEDIATION" },
                    new() { StepId = "FIM_REMEDIATION",     StepName = "Remediation & Life Extension",  SequenceNumber = 5, StepType = "ACTION",   IsRequired = false, NextStepId = "FIM_SIGN_OFF" },
                    new() { StepId = "FIM_SIGN_OFF",        StepName = "Integrity Sign-Off",            SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeFacilityDecommissioningAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "FACILITY_LIFECYCLE_DECOMMISSION",
                ProcessName  = "FacilityDecommissioning",
                ProcessType  = "FACILITY_LIFECYCLE",
                EntityType   = "FACILITY_DECOMMISSIONING",
                Description  = "Facility decommission — OSPAR 98/3 / BSEE decommission plan submission.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "DCOM_PLAN",           StepName = "Decommissioning Plan",           SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "DCOM_REGULATORY_SUBMIT" },
                    new() { StepId = "DCOM_REGULATORY_SUBMIT", StepName = "Regulatory Submission",       SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "DCOM_APPROVAL" },
                    new() { StepId = "DCOM_APPROVAL",       StepName = "Regulatory Approval",            SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "DCOM_ISOLATION" },
                    new() { StepId = "DCOM_ISOLATION",      StepName = "Isolation & Depressurisation",   SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "DCOM_DEMOLITION" },
                    new() { StepId = "DCOM_DEMOLITION",     StepName = "Demolition & Removal",           SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "DCOM_SITE_RESTORE" },
                    new() { StepId = "DCOM_SITE_RESTORE",   StepName = "Site Restoration",               SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = "DCOM_REGULATOR_CONFIRM" },
                    new() { StepId = "DCOM_REGULATOR_CONFIRM", StepName = "Regulatory Closure Confirmation", SequenceNumber = 7, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Category: Reservoir Management Workflows (8 processes)
        // ─────────────────────────────────────────────────────────────────

        public async Task InitializeReservoirManagementWorkflowsAsync(string userId)
        {
            await InitializeReservoirSurveillancePlanAsync(userId);
            await InitializeReservesRevisionAsync(userId);
            await InitializeMaterialBalanceStudyAsync(userId);
            await InitializeEORScreeningAsync(userId);
            await InitializeUnitizationNegotiationAsync(userId);
            await InitializeReservoirModelUpdateAsync(userId);
            await InitializeAquiferManagementAsync(userId);
            await InitializeReservoirPressureMaintenanceAsync(userId);
        }

        private async Task InitializeReservoirSurveillancePlanAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "RESERVOIR_SURVEILLANCE_PLAN",
                ProcessName  = "ReservoirSurveillancePlan",
                ProcessType  = "RESERVOIR_MANAGEMENT",
                EntityType   = "POOL",
                Description  = "Annual reservoir surveillance plan — pressure transient analysis, PVT, production data.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "RSP_DATA_REVIEW",    StepName = "Historical Data Review",         SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "RSP_SURVEILLANCE_PROGRAM" },
                    new() { StepId = "RSP_SURVEILLANCE_PROGRAM", StepName = "Design Surveillance Program", SequenceNumber = 2, StepType = "ACTION",  IsRequired = true,  NextStepId = "RSP_APPROVAL" },
                    new() { StepId = "RSP_APPROVAL",       StepName = "Plan Approval",                  SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "RSP_EXECUTE" },
                    new() { StepId = "RSP_EXECUTE",        StepName = "Execute Surveillance",           SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "RSP_INTERPRET" },
                    new() { StepId = "RSP_INTERPRET",      StepName = "Data Interpretation",            SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "RSP_RECOMMENDATIONS" },
                    new() { StepId = "RSP_RECOMMENDATIONS",StepName = "Reservoir Management Recommendations", SequenceNumber = 6, StepType = "ACTION", IsRequired = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeReservesRevisionAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "RESERVOIR_RESERVES_REVISION",
                ProcessName  = "ReservesRevision",
                ProcessType  = "RESERVOIR_MANAGEMENT",
                EntityType   = "POOL",
                Description  = "Annual reserves revision — SPE-PRMS 2018, SEC Rule 4-10(a) deterministic / probabilistic.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "RRV_DATA_COMPILE",   StepName = "Compile Production & Pressure Data", SequenceNumber = 1, StepType = "ACTION", IsRequired = true, NextStepId = "RRV_RESERVES_ESTIMATE" },
                    new() { StepId = "RRV_RESERVES_ESTIMATE", StepName = "Reserves Estimate Update",      SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "RRV_CATEGORY_CLASSIFICATION" },
                    new() { StepId = "RRV_CATEGORY_CLASSIFICATION", StepName = "PRMS Category Classification",SequenceNumber = 3, StepType = "ACTION", IsRequired = true, NextStepId = "RRV_PEER_REVIEW" },
                    new() { StepId = "RRV_PEER_REVIEW",    StepName = "Peer Review",                     SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "RRV_MANAGEMENT_REVIEW" },
                    new() { StepId = "RRV_MANAGEMENT_REVIEW", StepName = "Management Review",             SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "RRV_CERTIFY" },
                    new() { StepId = "RRV_CERTIFY",        StepName = "Qualified Reserves Evaluator Sign-Off", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeMaterialBalanceStudyAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "RESERVOIR_MATERIAL_BALANCE",
                ProcessName  = "MaterialBalanceStudy",
                ProcessType  = "RESERVOIR_MANAGEMENT",
                EntityType   = "POOL",
                Description  = "Reservoir material balance study — Havlena-Odeh / tank model, OOIP/OGIP estimate.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "MB_PVT_COMPILE",     StepName = "PVT & Pressure Data Compilation", SequenceNumber = 1, StepType = "ACTION",  IsRequired = true,  NextStepId = "MB_MB_CALC" },
                    new() { StepId = "MB_MB_CALC",         StepName = "Material Balance Calculation",    SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "MB_DRIVE_MECHANISM" },
                    new() { StepId = "MB_DRIVE_MECHANISM", StepName = "Drive Mechanism Identification",  SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "MB_OOIP_ESTIMATE" },
                    new() { StepId = "MB_OOIP_ESTIMATE",   StepName = "OOIP / OGIP Estimate",            SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "MB_REVIEW" },
                    new() { StepId = "MB_REVIEW",          StepName = "Study Review",                    SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeEORScreeningAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "RESERVOIR_EOR_SCREENING",
                ProcessName  = "EORScreening",
                ProcessType  = "RESERVOIR_MANAGEMENT",
                EntityType   = "POOL",
                Description  = "EOR / IOR screening — technical screening, pilot design, economic justification.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "EOR_TECHNICAL_SCREEN", StepName = "Technical Screening",           SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "EOR_LABORATORY" },
                    new() { StepId = "EOR_LABORATORY",     StepName = "Laboratory Studies",             SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "EOR_SIMULATION" },
                    new() { StepId = "EOR_SIMULATION",     StepName = "Reservoir Simulation",           SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "EOR_ECONOMICS" },
                    new() { StepId = "EOR_ECONOMICS",      StepName = "Economic Evaluation",            SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "EOR_REVIEW" },
                    new() { StepId = "EOR_REVIEW",         StepName = "Management Review & Decision",   SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "EOR_PILOT" },
                    new() { StepId = "EOR_PILOT",          StepName = "Pilot Project Sanction",         SequenceNumber = 6, StepType = "ACTION",   IsRequired = false, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeUnitizationNegotiationAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "RESERVOIR_UNITIZATION",
                ProcessName  = "UnitizationNegotiation",
                ProcessType  = "RESERVOIR_MANAGEMENT",
                EntityType   = "POOL",
                Description  = "Cross-boundary reservoir unitization — tract participation, allocation schedule, JOA.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "UNIT_TECHNICAL",     StepName = "Technical Studies (Equity Determination)", SequenceNumber = 1, StepType = "ACTION", IsRequired = true, NextStepId = "UNIT_REDETERMINATION_TRIGGER" },
                    new() { StepId = "UNIT_REDETERMINATION_TRIGGER", StepName = "Redetermination Trigger Review", SequenceNumber = 2, StepType = "ACTION", IsRequired = true, NextStepId = "UNIT_NEGOTIATION" },
                    new() { StepId = "UNIT_NEGOTIATION",   StepName = "Inter-Party Negotiation",        SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "UNIT_REGULATOR_APPROVAL" },
                    new() { StepId = "UNIT_REGULATOR_APPROVAL", StepName = "Regulatory Approval",       SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "UNIT_AGREEMENT" },
                    new() { StepId = "UNIT_AGREEMENT",     StepName = "Unitization Agreement Signed",   SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeReservoirModelUpdateAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "RESERVOIR_MODEL_UPDATE",
                ProcessName  = "ReservoirModelUpdate",
                ProcessType  = "RESERVOIR_MANAGEMENT",
                EntityType   = "POOL",
                Description  = "Reservoir simulation model update — history match, forecast update, uncertainty.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "RMU_DATA_GATHER",    StepName = "Data Gathering & QC",            SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "RMU_HISTORY_MATCH" },
                    new() { StepId = "RMU_HISTORY_MATCH",  StepName = "History Matching",               SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "RMU_SENSITIVITY" },
                    new() { StepId = "RMU_SENSITIVITY",    StepName = "Sensitivity & Uncertainty",      SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "RMU_FORECAST" },
                    new() { StepId = "RMU_FORECAST",       StepName = "Production Forecast Update",     SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "RMU_REVIEW" },
                    new() { StepId = "RMU_REVIEW",         StepName = "Peer Review",                    SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeAquiferManagementAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "RESERVOIR_AQUIFER_MANAGEMENT",
                ProcessName  = "AquiferManagement",
                ProcessType  = "RESERVOIR_MANAGEMENT",
                EntityType   = "POOL",
                Description  = "Aquifer influx management — water encroachment monitoring, injection plan.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "AQF_WATER_CUT_TREND",StepName = "Monitor Water Cut Trend",        SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "AQF_AQUIFER_STUDY" },
                    new() { StepId = "AQF_AQUIFER_STUDY",  StepName = "Aquifer Influx Study",           SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "AQF_MITIGATION" },
                    new() { StepId = "AQF_MITIGATION",     StepName = "Mitigation Planning (WI / GI)",  SequenceNumber = 3, StepType = "ACTION",   IsRequired = false, NextStepId = "AQF_REVIEW" },
                    new() { StepId = "AQF_REVIEW",         StepName = "Engineering Review",             SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "AQF_IMPLEMENT" },
                    new() { StepId = "AQF_IMPLEMENT",      StepName = "Implement Mitigation",           SequenceNumber = 5, StepType = "ACTION",   IsRequired = false, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeReservoirPressureMaintenanceAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "RESERVOIR_PRESSURE_MAINTENANCE",
                ProcessName  = "ReservoirPressureMaintenance",
                ProcessType  = "RESERVOIR_MANAGEMENT",
                EntityType   = "POOL",
                Description  = "Pressure maintenance program — water / gas injection design and monitoring.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "RPM_VOIDAGE_CALC",   StepName = "Voidage Replacement Calculation",SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "RPM_INJECTION_DESIGN" },
                    new() { StepId = "RPM_INJECTION_DESIGN",StepName = "Injection Well Design",         SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "RPM_PERMIT" },
                    new() { StepId = "RPM_PERMIT",         StepName = "Injection Permit",               SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "RPM_INJECTION_START" },
                    new() { StepId = "RPM_INJECTION_START",StepName = "Start Injection",                SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "RPM_MONITOR" },
                    new() { StepId = "RPM_MONITOR",        StepName = "Monitor Pressure Response",      SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "RPM_ADJUST" },
                    new() { StepId = "RPM_ADJUST",         StepName = "Adjust Injection Rates",         SequenceNumber = 6, StepType = "ACTION",   IsRequired = false, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Category: Pipeline Infrastructure Workflows (8 processes)
        // ─────────────────────────────────────────────────────────────────

        public async Task InitializePipelineInfrastructureWorkflowsAsync(string userId)
        {
            await InitializePipelineDesignReviewAsync(userId);
            await InitializePipelineConstructionAsync(userId);
            await InitializePipelineCommissioningAsync(userId);
            await InitializePipelineILIProgamAsync(userId);
            await InitializePipelineCathodicProtectionAsync(userId);
            await InitializePipelineLeakResponseAsync(userId);
            await InitializePipelineCapacityManagementAsync(userId);
            await InitializePipelineDecommissioningAsync(userId);
        }

        private async Task InitializePipelineDesignReviewAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "PIPELINE_DESIGN_REVIEW",
                ProcessName  = "PipelineDesignReview",
                ProcessType  = "PIPELINE_INFRASTRUCTURE",
                EntityType   = "PIPE_STRING",
                Description  = "Pipeline design review — ASME B31.8 (gas) / B31.4 (liquid) design code compliance.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "PDR_HYDRAULIC",      StepName = "Hydraulic & Process Design",     SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "PDR_MECHANICAL_DESIGN" },
                    new() { StepId = "PDR_MECHANICAL_DESIGN", StepName = "Mechanical Design (Wall Thickness)", SequenceNumber = 2, StepType = "ACTION", IsRequired = true, NextStepId = "PDR_CORROSION" },
                    new() { StepId = "PDR_CORROSION",      StepName = "Corrosion Allowance Review",     SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "PDR_ASME_COMPLIANCE" },
                    new() { StepId = "PDR_ASME_COMPLIANCE",StepName = "ASME Code Compliance Check",     SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "PDR_APPROVAL" },
                    new() { StepId = "PDR_APPROVAL",       StepName = "Design Approval",                SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializePipelineConstructionAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "PIPELINE_CONSTRUCTION",
                ProcessName  = "PipelineConstruction",
                ProcessType  = "PIPELINE_INFRASTRUCTURE",
                EntityType   = "PIPE_STRING",
                Description  = "Pipeline construction — right-of-way, welding, coating, hydrostatic test, reinstatement.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "PCON_ROW",           StepName = "Right-of-Way Clearance",         SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "PCON_PIPE_STRINGING" },
                    new() { StepId = "PCON_PIPE_STRINGING",StepName = "Pipe Stringing & Welding",       SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "PCON_NDE" },
                    new() { StepId = "PCON_NDE",           StepName = "Non-Destructive Examination",    SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "PCON_COATING" },
                    new() { StepId = "PCON_COATING",       StepName = "Coating & Cathodic Protection",  SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "PCON_HYDRO_TEST" },
                    new() { StepId = "PCON_HYDRO_TEST",    StepName = "Hydrostatic Pressure Test",      SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "PCON_REINSTATEMENT" },
                    new() { StepId = "PCON_REINSTATEMENT", StepName = "Backfill & Reinstatement",       SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = "PCON_ACCEPTANCE" },
                    new() { StepId = "PCON_ACCEPTANCE",    StepName = "Pipeline Acceptance",            SequenceNumber = 7, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializePipelineCommissioningAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "PIPELINE_COMMISSIONING",
                ProcessName  = "PipelineCommissioning",
                ProcessType  = "PIPELINE_INFRASTRUCTURE",
                EntityType   = "PIPE_STRING",
                Description  = "Pipeline commissioning — dewater, dry, introduce product, steady state check.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "PCOMM_DEWATER",      StepName = "Dewatering & Drying",            SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "PCOMM_PIGGING" },
                    new() { StepId = "PCOMM_PIGGING",      StepName = "Gauging / Cleaning Pig Run",     SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "PCOMM_INTRODUCE_PRODUCT" },
                    new() { StepId = "PCOMM_INTRODUCE_PRODUCT", StepName = "Introduce Product",          SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "PCOMM_STEADY_STATE" },
                    new() { StepId = "PCOMM_STEADY_STATE", StepName = "Steady State Verification",      SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "PCOMM_HANDOVER" },
                    new() { StepId = "PCOMM_HANDOVER",     StepName = "Handover to Operations",         SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializePipelineILIProgamAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "PIPELINE_ILI_PROGRAM",
                ProcessName  = "PipelineILIProgram",
                ProcessType  = "PIPELINE_INFRASTRUCTURE",
                EntityType   = "PIPE_STRING",
                Description  = "In-line inspection program — ASME B31.8S / API 1160 ILI run, anomaly assessment.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "ILI_TOOL_SELECT",    StepName = "ILI Tool Selection (MFL / UT)",  SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "ILI_PRE_RUN" },
                    new() { StepId = "ILI_PRE_RUN",        StepName = "Pre-ILI Gauging Pig Run",        SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "ILI_RUN" },
                    new() { StepId = "ILI_RUN",            StepName = "ILI Run Execution",              SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "ILI_DATA_ANALYSIS" },
                    new() { StepId = "ILI_DATA_ANALYSIS",  StepName = "Data Analysis & NDE Verify",     SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "ILI_ANOMALY_ASSESS" },
                    new() { StepId = "ILI_ANOMALY_ASSESS", StepName = "Anomaly Assessment (B31G / RSTRENG)", SequenceNumber = 5, StepType = "ACTION", IsRequired = true, NextStepId = "ILI_REMEDIATION" },
                    new() { StepId = "ILI_REMEDIATION",    StepName = "Remediation & Repair",           SequenceNumber = 6, StepType = "ACTION",   IsRequired = false, NextStepId = "ILI_CLOSE" },
                    new() { StepId = "ILI_CLOSE",          StepName = "ILI Programme Close-Out",        SequenceNumber = 7, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializePipelineCathodicProtectionAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "PIPELINE_CP_SURVEY",
                ProcessName  = "PipelineCathodicProtectionSurvey",
                ProcessType  = "PIPELINE_INFRASTRUCTURE",
                EntityType   = "PIPE_STRING",
                Description  = "Annual cathodic protection survey — NACE SP0169 / ISO 15589-1 close interval survey.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "CP_ROUTE_SURVEY",    StepName = "Close Interval Potential Survey",SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "CP_DATA_ANALYSIS" },
                    new() { StepId = "CP_DATA_ANALYSIS",   StepName = "Data Analysis & Criteria Check", SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "CP_DEFICIENCY" },
                    new() { StepId = "CP_DEFICIENCY",      StepName = "Deficiency Identification",      SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "CP_REMEDIATION" },
                    new() { StepId = "CP_REMEDIATION",     StepName = "CP Remediation (Anode / TR)",    SequenceNumber = 4, StepType = "ACTION",   IsRequired = false, NextStepId = "CP_SIGN_OFF" },
                    new() { StepId = "CP_SIGN_OFF",        StepName = "CP Survey Sign-Off",             SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializePipelineLeakResponseAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "PIPELINE_LEAK_RESPONSE",
                ProcessName  = "PipelineLeakResponse",
                ProcessType  = "PIPELINE_INFRASTRUCTURE",
                EntityType   = "PIPE_STRING",
                Description  = "Pipeline leak / rupture emergency response — DOT 49 CFR 192/195 emergency plan.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "PLR_DETECT",         StepName = "Leak Detection / Notification",  SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "PLR_ISOLATE" },
                    new() { StepId = "PLR_ISOLATE",        StepName = "Isolate & Shut In Pipeline",     SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "PLR_REGULATORY_NOTIFY" },
                    new() { StepId = "PLR_REGULATORY_NOTIFY", StepName = "Regulatory Notification",     SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "PLR_CONTAINMENT" },
                    new() { StepId = "PLR_CONTAINMENT",    StepName = "Containment & Product Recovery", SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "PLR_REPAIR" },
                    new() { StepId = "PLR_REPAIR",         StepName = "Repair & Pressure Test",         SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "PLR_REINSTATE" },
                    new() { StepId = "PLR_REINSTATE",      StepName = "Reinstate Pipeline Service",     SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "PLR_RCA" },
                    new() { StepId = "PLR_RCA",            StepName = "Root Cause Analysis",            SequenceNumber = 7, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializePipelineCapacityManagementAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "PIPELINE_CAPACITY_MANAGEMENT",
                ProcessName  = "PipelineCapacityManagement",
                ProcessType  = "PIPELINE_INFRASTRUCTURE",
                EntityType   = "PIPE_STRING",
                Description  = "Pipeline capacity management — throughput optimization, compression, looping.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "PCM_THROUGHPUT_REVIEW", StepName = "Throughput & Capacity Review", SequenceNumber = 1, StepType = "ACTION",  IsRequired = true,  NextStepId = "PCM_BOTTLENECK" },
                    new() { StepId = "PCM_BOTTLENECK",      StepName = "Bottleneck Identification",      SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "PCM_OPTIONS" },
                    new() { StepId = "PCM_OPTIONS",         StepName = "Capacity Enhancement Options",   SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "PCM_ECONOMICS" },
                    new() { StepId = "PCM_ECONOMICS",       StepName = "Economic Evaluation",            SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "PCM_APPROVAL" },
                    new() { StepId = "PCM_APPROVAL",        StepName = "Management Approval",            SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "PCM_IMPLEMENT" },
                    new() { StepId = "PCM_IMPLEMENT",       StepName = "Implement Enhancement",          SequenceNumber = 6, StepType = "ACTION",   IsRequired = false, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializePipelineDecommissioningAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "PIPELINE_DECOMMISSION",
                ProcessName  = "PipelineDecommissioning",
                ProcessType  = "PIPELINE_INFRASTRUCTURE",
                EntityType   = "PIPE_STRING",
                Description  = "Pipeline decommission — purge, abandon-in-place or remove, PHMSA notification.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "PDEC_PLAN",           StepName = "Decommissioning Plan",           SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "PDEC_REGULATORY_NOTIFY" },
                    new() { StepId = "PDEC_REGULATORY_NOTIFY", StepName = "Regulatory Notification",     SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "PDEC_ISOLATE" },
                    new() { StepId = "PDEC_ISOLATE",        StepName = "Isolate & Purge Pipeline",       SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "PDEC_REMOVE_OR_AIP" },
                    new() { StepId = "PDEC_REMOVE_OR_AIP",  StepName = "Remove or Abandon-in-Place",     SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "PDEC_SITE_RESTORE" },
                    new() { StepId = "PDEC_SITE_RESTORE",   StepName = "Site Restoration",               SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "PDEC_REGULATOR_CONFIRM" },
                    new() { StepId = "PDEC_REGULATOR_CONFIRM", StepName = "Regulatory Closure Confirmation", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }
    }
}
