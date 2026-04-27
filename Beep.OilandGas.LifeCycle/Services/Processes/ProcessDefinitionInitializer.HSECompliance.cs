using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.LifeCycle.Services.Processes
{
    /// <summary>
    /// HSE & Safety Workflows and Compliance & Regulatory Workflows.
    /// Phase 2 Business Process Branch — 16 additional process definitions.
    ///
    /// Standards: API RP 754 (Tier 1-4 incidents), IEC 61882 (HAZOP), ISO 31000 (risk),
    /// IOGP 2022e, OSHA PSM, EPA 40 CFR 98 Subpart W (GHG), BSEE 30 CFR 250,
    /// AER Directive 023/056, NEB/CER OPR-99.
    /// </summary>
    public partial class ProcessDefinitionInitializer
    {
        // ─────────────────────────────────────────────────────────────────
        //  Category: HSE & Safety Workflows (8 processes)
        // ─────────────────────────────────────────────────────────────────

        public async Task InitializeHSEWorkflowsAsync(string userId)
        {
            await InitializeIncidentReportingAsync(userId);
            await InitializeNearMissReportingAsync(userId);
            await InitializeHazardIdentificationAsync(userId);
            await InitializeHSEAuditAsync(userId);
            await InitializeEmergencyResponseAsync(userId);
            await InitializeEnvironmentalIncidentAsync(userId);
            await InitializeHAZOPReviewAsync(userId);
            await InitializeSafetyDrillAsync(userId);
        }

        private async Task InitializeIncidentReportingAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "HSE_INCIDENT_REPORTING",
                ProcessName  = "IncidentReporting",
                ProcessType  = "HSE",
                EntityType   = "HSE_INCIDENT",
                Description  = "API RP 754 Tier 1-4 incident reporting — report, RCA, corrective actions, close-out.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "INC_REPORT",         StepName = "Incident Report",                SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "INC_CLASSIFICATION" },
                    new() { StepId = "INC_CLASSIFICATION", StepName = "Tier Classification (API RP 754)",SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "INC_NOTIFICATION" },
                    new() { StepId = "INC_NOTIFICATION",   StepName = "Management & Regulatory Notification",SequenceNumber = 3, StepType = "ACTION", IsRequired = true, NextStepId = "INC_INVESTIGATION" },
                    new() { StepId = "INC_INVESTIGATION",  StepName = "Investigation",                   SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "INC_RCA" },
                    new() { StepId = "INC_RCA",            StepName = "Root Cause Analysis",             SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "INC_CORRECTIVE_ACTIONS" },
                    new() { StepId = "INC_CORRECTIVE_ACTIONS", StepName = "Corrective Actions",          SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = "INC_VERIFICATION" },
                    new() { StepId = "INC_VERIFICATION",   StepName = "Action Effectiveness Verification",SequenceNumber = 7, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "INC_CLOSURE" },
                    new() { StepId = "INC_CLOSURE",        StepName = "Incident Closure",                SequenceNumber = 8, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeNearMissReportingAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "HSE_NEAR_MISS",
                ProcessName  = "NearMissReporting",
                ProcessType  = "HSE",
                EntityType   = "HSE_INCIDENT",
                Description  = "Near miss and unsafe condition reporting — IOGP 2022e KPI protocol.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "NM_REPORT",          StepName = "Near Miss Report",               SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "NM_REVIEW" },
                    new() { StepId = "NM_REVIEW",          StepName = "Supervisor Review",              SequenceNumber = 2, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "NM_PREVENTIVE_ACTION" },
                    new() { StepId = "NM_PREVENTIVE_ACTION", StepName = "Preventive Action",            SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "NM_CLOSURE" },
                    new() { StepId = "NM_CLOSURE",         StepName = "Close Out & Learning Shared",    SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeHazardIdentificationAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "HSE_HAZID",
                ProcessName  = "HazardIdentification",
                ProcessType  = "HSE",
                EntityType   = "FACILITY",
                Description  = "HAZID — systematic facility-level hazard identification per ISO 31000 and IEC 61882.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "HAZID_SCOPE",        StepName = "Define Scope & Nodes",           SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "HAZID_TEAM" },
                    new() { StepId = "HAZID_TEAM",         StepName = "Assemble Multidisciplinary Team", SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "HAZID_STUDY" },
                    new() { StepId = "HAZID_STUDY",        StepName = "Hazard Identification Study",    SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "HAZID_RISK_RANKING" },
                    new() { StepId = "HAZID_RISK_RANKING", StepName = "Risk Ranking & Bow-Tie",         SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "HAZID_ACTIONS" },
                    new() { StepId = "HAZID_ACTIONS",      StepName = "Assign Risk Reduction Actions",  SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "HAZID_REVIEW" },
                    new() { StepId = "HAZID_REVIEW",       StepName = "Management Review & Sign-Off",   SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeHSEAuditAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "HSE_AUDIT",
                ProcessName  = "HSEAudit",
                ProcessType  = "HSE",
                EntityType   = "FACILITY",
                Description  = "Periodic HSE audit — pre-audit, field audit, findings, corrective action plan.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "AUDIT_PLAN",         StepName = "Audit Plan & Checklist",         SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "AUDIT_FIELDWORK" },
                    new() { StepId = "AUDIT_FIELDWORK",    StepName = "Field Audit Execution",          SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "AUDIT_FINDINGS" },
                    new() { StepId = "AUDIT_FINDINGS",     StepName = "Findings & Observations Report", SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "AUDIT_CAP" },
                    new() { StepId = "AUDIT_CAP",          StepName = "Corrective Action Plan",         SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "AUDIT_CLOSE" },
                    new() { StepId = "AUDIT_CLOSE",        StepName = "Audit Closure",                  SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeEmergencyResponseAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "HSE_EMERGENCY_RESPONSE",
                ProcessName  = "EmergencyResponse",
                ProcessType  = "HSE",
                EntityType   = "FACILITY",
                Description  = "Emergency response activation — oil spill / blowout / fire response chain.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "ER_ALERT",            StepName = "Alert & Muster",                SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "ER_INCIDENT_CONTROL" },
                    new() { StepId = "ER_INCIDENT_CONTROL", StepName = "Incident Commander Activated",  SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "ER_REGULATORY_NOTIFY" },
                    new() { StepId = "ER_REGULATORY_NOTIFY",StepName = "Regulatory Notification",       SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "ER_CONTAINMENT" },
                    new() { StepId = "ER_CONTAINMENT",      StepName = "Containment / Suppression",     SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "ER_ALL_CLEAR" },
                    new() { StepId = "ER_ALL_CLEAR",        StepName = "All Clear",                     SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "ER_INCIDENT_REPORT" },
                    new() { StepId = "ER_INCIDENT_REPORT",  StepName = "Incident Report Filed",         SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeEnvironmentalIncidentAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "HSE_ENVIRONMENTAL_INCIDENT",
                ProcessName  = "EnvironmentalIncident",
                ProcessType  = "HSE",
                EntityType   = "ENVIRONMENTAL_RESTORATION",
                Description  = "Environmental incident and spill response — EPA 40 CFR 112 / SPCC plan.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "ENV_DETECTION",      StepName = "Spill / Release Detection",     SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "ENV_NOTIFICATION" },
                    new() { StepId = "ENV_NOTIFICATION",   StepName = "Regulatory Notification (SPCC)", SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "ENV_CONTAINMENT" },
                    new() { StepId = "ENV_CONTAINMENT",    StepName = "Containment & Recovery",         SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "ENV_REMEDIATION" },
                    new() { StepId = "ENV_REMEDIATION",    StepName = "Site Remediation",               SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "ENV_SITE_VERIFY" },
                    new() { StepId = "ENV_SITE_VERIFY",    StepName = "Site Verification & Sampling",   SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "ENV_REGULATOR_CLOSURE" },
                    new() { StepId = "ENV_REGULATOR_CLOSURE", StepName = "Regulator Closure Confirmation", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeHAZOPReviewAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "HSE_HAZOP",
                ProcessName  = "HAZOPReview",
                ProcessType  = "HSE",
                EntityType   = "FACILITY",
                Description  = "HAZOP study per IEC 61882:2016 — node study, deviation analysis, action tracking.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "HAZOP_PREP",         StepName = "P&ID & Node Sheet Preparation",  SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "HAZOP_STUDY" },
                    new() { StepId = "HAZOP_STUDY",        StepName = "HAZOP Study Sessions",            SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "HAZOP_RECOMMENDATIONS" },
                    new() { StepId = "HAZOP_RECOMMENDATIONS", StepName = "Recommendations Register",     SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "HAZOP_CLOSE_OUT" },
                    new() { StepId = "HAZOP_CLOSE_OUT",    StepName = "Action Close-Out Review",         SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "HAZOP_SIGN_OFF" },
                    new() { StepId = "HAZOP_SIGN_OFF",     StepName = "HAZOP Report Sign-Off",           SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeSafetyDrillAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "HSE_SAFETY_DRILL",
                ProcessName  = "SafetyDrill",
                ProcessType  = "HSE",
                EntityType   = "FACILITY",
                Description  = "Emergency safety drill — plan, execute, debrief, and update ERP.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "DRILL_PLAN",         StepName = "Drill Scenario Planning",        SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "DRILL_EXECUTE" },
                    new() { StepId = "DRILL_EXECUTE",      StepName = "Drill Execution",                SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "DRILL_DEBRIEF" },
                    new() { StepId = "DRILL_DEBRIEF",      StepName = "Debrief & Lessons Learned",      SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "DRILL_ERP_UPDATE" },
                    new() { StepId = "DRILL_ERP_UPDATE",   StepName = "Update Emergency Response Plan", SequenceNumber = 4, StepType = "ACTION",   IsRequired = false, NextStepId = "DRILL_CLOSE" },
                    new() { StepId = "DRILL_CLOSE",        StepName = "Drill Record Closed",            SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Category: Compliance & Regulatory Workflows (8 processes)
        // ─────────────────────────────────────────────────────────────────

        public async Task InitializeComplianceWorkflowsAsync(string userId)
        {
            await InitializeProductionReportingComplianceAsync(userId);
            await InitializeGHGReportingComplianceAsync(userId);
            await InitializeWellPermitComplianceAsync(userId);
            await InitializePipelineIntegrityComplianceAsync(userId);
            await InitializeObligationManagementAsync(userId);
            await InitializeRegulatoryAuditAsync(userId);
            await InitializeFlareReportingAsync(userId);
            await InitializeWaterDisposalPermitAsync(userId);
        }

        private async Task InitializeProductionReportingComplianceAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "COMPLIANCE_PRODUCTION_REPORTING",
                ProcessName  = "ProductionReportingCompliance",
                ProcessType  = "COMPLIANCE",
                EntityType   = "PDEN_VOL_SUMMARY",
                Description  = "Monthly production reporting — AER Directive 023 (CA) / BSEE OGOR (US).",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "PROD_DATA_GATHER",   StepName = "Gather Production Data",         SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "PROD_DATA_VERIFY" },
                    new() { StepId = "PROD_DATA_VERIFY",   StepName = "Data Verification & QC",         SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "PROD_REPORT_PREPARE" },
                    new() { StepId = "PROD_REPORT_PREPARE",StepName = "Report Preparation",             SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "PROD_SUPERVISOR_REVIEW" },
                    new() { StepId = "PROD_SUPERVISOR_REVIEW", StepName = "Supervisor Review",          SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "PROD_SUBMISSION" },
                    new() { StepId = "PROD_SUBMISSION",    StepName = "Regulatory Submission",          SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "PROD_CONFIRM" },
                    new() { StepId = "PROD_CONFIRM",       StepName = "Submission Confirmation",        SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeGHGReportingComplianceAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "COMPLIANCE_GHG_REPORTING",
                ProcessName  = "GHGReportingCompliance",
                ProcessType  = "COMPLIANCE",
                EntityType   = "OBLIGATION",
                Description  = "Annual GHG reporting — EPA 40 CFR 98 Subpart W / EC GHG MRVA.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "GHG_EMISSIONS_CALC",  StepName = "Emissions Calculation",         SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "GHG_QA_QC" },
                    new() { StepId = "GHG_QA_QC",           StepName = "QA/QC & Third-Party Verification",SequenceNumber = 2, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "GHG_REPORT_PREPARE" },
                    new() { StepId = "GHG_REPORT_PREPARE",  StepName = "Report Preparation",             SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "GHG_MANAGEMENT_REVIEW" },
                    new() { StepId = "GHG_MANAGEMENT_REVIEW", StepName = "Management Review",            SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "GHG_SUBMISSION" },
                    new() { StepId = "GHG_SUBMISSION",      StepName = "Regulatory Submission",          SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "GHG_CONFIRM" },
                    new() { StepId = "GHG_CONFIRM",         StepName = "Confirmation Receipt",           SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeWellPermitComplianceAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "COMPLIANCE_WELL_PERMIT",
                ProcessName  = "WellPermitCompliance",
                ProcessType  = "COMPLIANCE",
                EntityType   = "WELL",
                Description  = "Well permit lifecycle — APD/ABD submission, BSEE 30 CFR 250 / AER Directive 056.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "WP_APPLICATION",     StepName = "Permit Application",             SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "WP_INTERNAL_REVIEW" },
                    new() { StepId = "WP_INTERNAL_REVIEW", StepName = "Internal Review",                SequenceNumber = 2, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "WP_SUBMISSION" },
                    new() { StepId = "WP_SUBMISSION",      StepName = "Regulatory Submission",          SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "WP_AWAIT_APPROVAL" },
                    new() { StepId = "WP_AWAIT_APPROVAL",  StepName = "Await Regulatory Decision",      SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "WP_RECEIVED" },
                    new() { StepId = "WP_RECEIVED",        StepName = "Permit Received",                SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "WP_CONDITIONS_TRACK" },
                    new() { StepId = "WP_CONDITIONS_TRACK",StepName = "Track Permit Conditions",        SequenceNumber = 6, StepType = "ACTION",   IsRequired = false, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializePipelineIntegrityComplianceAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "COMPLIANCE_PIPELINE_INTEGRITY",
                ProcessName  = "PipelineIntegrityManagement",
                ProcessType  = "COMPLIANCE",
                EntityType   = "PIPE_STRING",
                Description  = "Pipeline integrity management program — API 1160 / DOT 49 CFR 192/195.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "PIM_THREAT_ASSESSMENT",  StepName = "Threat Assessment",          SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "PIM_RISK_RANKING" },
                    new() { StepId = "PIM_RISK_RANKING",       StepName = "Risk Ranking",                SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "PIM_INSPECTION_PLAN" },
                    new() { StepId = "PIM_INSPECTION_PLAN",    StepName = "Inspection Plan (ILI / ECDA)",SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "PIM_EXECUTE_INSPECTION" },
                    new() { StepId = "PIM_EXECUTE_INSPECTION", StepName = "Execute Inspection",          SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "PIM_ANOMALY_ASSESSMENT" },
                    new() { StepId = "PIM_ANOMALY_ASSESSMENT", StepName = "Anomaly Assessment",          SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "PIM_REMEDIATION" },
                    new() { StepId = "PIM_REMEDIATION",        StepName = "Remediation Actions",         SequenceNumber = 6, StepType = "ACTION",   IsRequired = false, NextStepId = "PIM_PERIODIC_UPDATE" },
                    new() { StepId = "PIM_PERIODIC_UPDATE",    StepName = "Periodic IMP Update",         SequenceNumber = 7, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeObligationManagementAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "COMPLIANCE_OBLIGATION_MGMT",
                ProcessName  = "ObligationManagement",
                ProcessType  = "COMPLIANCE",
                EntityType   = "OBLIGATION",
                Description  = "Regulatory obligation tracking — due date, responsible party, evidence upload, close.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "OBL_REGISTER",       StepName = "Register Obligation",            SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "OBL_ASSIGN" },
                    new() { StepId = "OBL_ASSIGN",         StepName = "Assign Responsible Party",       SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "OBL_TRACK" },
                    new() { StepId = "OBL_TRACK",          StepName = "Progress Tracking",              SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "OBL_EVIDENCE" },
                    new() { StepId = "OBL_EVIDENCE",       StepName = "Evidence Collection",            SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "OBL_COMPLIANCE_REVIEW" },
                    new() { StepId = "OBL_COMPLIANCE_REVIEW", StepName = "Compliance Review",           SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "OBL_CLOSE" },
                    new() { StepId = "OBL_CLOSE",          StepName = "Obligation Closed",              SequenceNumber = 6, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeRegulatoryAuditAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "COMPLIANCE_REGULATORY_AUDIT",
                ProcessName  = "RegulatoryAudit",
                ProcessType  = "COMPLIANCE",
                EntityType   = "OBLIGATION",
                Description  = "Regulatory inspection / audit readiness — BSEE SEMS / AER Area Inspection.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "RAUD_NOTIFICATION",  StepName = "Inspection Notification Received",SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "RAUD_SELF_ASSESSMENT" },
                    new() { StepId = "RAUD_SELF_ASSESSMENT",StepName = "Self-Assessment",                SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "RAUD_DOCUMENT_PREP" },
                    new() { StepId = "RAUD_DOCUMENT_PREP", StepName = "Document Preparation",           SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "RAUD_INSPECTION" },
                    new() { StepId = "RAUD_INSPECTION",    StepName = "Regulator Inspection",           SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "RAUD_FINDINGS" },
                    new() { StepId = "RAUD_FINDINGS",      StepName = "Findings Response",              SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "RAUD_CAP" },
                    new() { StepId = "RAUD_CAP",           StepName = "Corrective Action Plan",         SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "RAUD_CLOSE" },
                    new() { StepId = "RAUD_CLOSE",         StepName = "Audit Closed with Regulator",    SequenceNumber = 7, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeFlareReportingAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "COMPLIANCE_FLARE_REPORTING",
                ProcessName  = "FlareReporting",
                ProcessType  = "COMPLIANCE",
                EntityType   = "PDEN_VOL_SUMMARY",
                Description  = "Routine / non-routine flare reporting — EPA 40 CFR 60 / AER Directive 060.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "FLARE_DATA",         StepName = "Collect Flare Volume Data",      SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "FLARE_CLASSIFY" },
                    new() { StepId = "FLARE_CLASSIFY",     StepName = "Classify Routine / Non-Routine", SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "FLARE_REPORT_PREPARE" },
                    new() { StepId = "FLARE_REPORT_PREPARE",StepName = "Report Preparation",            SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "FLARE_REVIEW" },
                    new() { StepId = "FLARE_REVIEW",       StepName = "Supervisor Review",              SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true,  RequiresApproval = true, NextStepId = "FLARE_SUBMISSION" },
                    new() { StepId = "FLARE_SUBMISSION",   StepName = "Regulatory Submission",          SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeWaterDisposalPermitAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId    = "COMPLIANCE_WATER_DISPOSAL",
                ProcessName  = "WaterDisposalPermit",
                ProcessType  = "COMPLIANCE",
                EntityType   = "OBLIGATION",
                Description  = "Produced water disposal permit — EPA UIC Class II / AER Directive 051.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "WD_APPLICATION",     StepName = "Permit Application Preparation",  SequenceNumber = 1, StepType = "ACTION",   IsRequired = true,  NextStepId = "WD_WELLBORE_EVAL" },
                    new() { StepId = "WD_WELLBORE_EVAL",   StepName = "Disposal Wellbore Evaluation",    SequenceNumber = 2, StepType = "ACTION",   IsRequired = true,  NextStepId = "WD_SUBMISSION" },
                    new() { StepId = "WD_SUBMISSION",      StepName = "Regulatory Submission",           SequenceNumber = 3, StepType = "ACTION",   IsRequired = true,  NextStepId = "WD_AWAIT" },
                    new() { StepId = "WD_AWAIT",           StepName = "Await Permit Decision",           SequenceNumber = 4, StepType = "ACTION",   IsRequired = true,  NextStepId = "WD_RECEIVED" },
                    new() { StepId = "WD_RECEIVED",        StepName = "Permit Received & Conditions Set",SequenceNumber = 5, StepType = "ACTION",   IsRequired = true,  NextStepId = "WD_MONITORING" },
                    new() { StepId = "WD_MONITORING",      StepName = "Ongoing Disposal Monitoring",     SequenceNumber = 6, StepType = "ACTION",   IsRequired = false, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }
    }
}
