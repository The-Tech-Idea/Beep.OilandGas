using Beep.OilandGas.Models.Processes;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Cross-role workflow definitions — Set 3: HSE, Compliance & Regulatory (CRW-15 through CRW-20).
/// These model handoffs between Field Engineers, HSE Officers, and Compliance Officers.
/// Part of Phase 3 cross-role orchestration.
/// </summary>
public partial class ProcessDefinitionInitializer
{
    private async Task InitializeCrossRoleHSEComplianceWorkflowsAsync(string userId)
    {
        await InitializeIncidentToCorrectiveActionAsync(userId);
        await InitializeNearMissToRiskAssessmentAsync(userId);
        await InitializePermitToWorkAsync(userId);
        await InitializeRegulatoryFilingReviewAsync(userId);
        await InitializeEnvironmentalSpillResponseAsync(userId);
        await InitializeGhgEmissionsReportAsync(userId);
    }

    /// <summary>CRW-15: Incident → Investigation → Corrective Action → Verified</summary>
    private async Task InitializeIncidentToCorrectiveActionAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_INCIDENT_TO_CORRECTIVE",
            ProcessName = "Incident → Investigation → Corrective Action",
            ProcessType = "HSE",
            EntityType = "HSE_INCIDENT",
            Description = "Incident reported → HSE classifies → investigation team assigned → root cause → corrective action → verified",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "INCIDENT_REPORT", StepName = "Report Incident", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"FieldEngineer","HSEOfficer"}, SlaHours = 4, NextStepId = "HSE_CLASSIFY", Description = "Any worker reports incident with initial details" },
                new() { StepId = "HSE_CLASSIFY", StepName = "Classify Incident", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"HSEOfficer"}, SlaHours = 24, NextStepId = "INVESTIGATION", Description = "HSE Officer classifies severity (Tier 1-4) and assigns investigation team" },
                new() { StepId = "INVESTIGATION", StepName = "Investigation", SequenceNumber = 3, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"HSEOfficer","FieldEngineer"}, SlaHours = 168, NextStepId = "ROOT_CAUSE", Description = "Investigation team gathers evidence, interviews witnesses, analyzes root cause" },
                new() { StepId = "ROOT_CAUSE", StepName = "Root Cause Analysis", SequenceNumber = 4, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"HSEOfficer"}, SlaHours = 72, NextStepId = "CORRECTIVE_ACTION", Description = "Root cause analysis completed using 5-Why or Fishbone methodology" },
                new() { StepId = "CORRECTIVE_ACTION", StepName = "Corrective Action Plan", SequenceNumber = 5, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"FieldEngineer","HSEOfficer"}, SlaHours = 72, NextStepId = "ACTION_EXECUTE", Description = "Corrective and preventive actions defined with owners and deadlines" },
                new() { StepId = "ACTION_EXECUTE", StepName = "Execute Corrective Actions", SequenceNumber = 6, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"FieldEngineer"}, SlaHours = 336, NextStepId = "HSE_VERIFY", Description = "Assigned personnel execute corrective actions" },
                new() { StepId = "HSE_VERIFY", StepName = "HSE Verification", SequenceNumber = 7, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"HSEOfficer","ComplianceOfficer"}, SlaHours = 72, Description = "HSE Officer verifies corrective actions are effective and closes incident" },
            },
            Configuration = new() { ["category"] = "HSE", ["regulation"] = "OSHA, ISO 45001", ["reportableThreshold"] = "Tier 1, Tier 2" }
        }, userId);
    }

    /// <summary>CRW-16: Any worker reports near miss → HSE assesses → risk level → preventive action</summary>
    private async Task InitializeNearMissToRiskAssessmentAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_NEAR_MISS_TO_RISK",
            ProcessName = "Near Miss → Risk Assessment",
            ProcessType = "HSE",
            EntityType = "HSE_INCIDENT",
            Description = "Any worker reports near miss → HSE Officer assesses → risk level determined → preventive action implemented",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "NEAR_MISS_REPORT", StepName = "Report Near Miss", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, SlaHours = 8, NextStepId = "HSE_ASSESS", Description = "Any persona can report — near miss reporting is everyone's responsibility" },
                new() { StepId = "HSE_ASSESS", StepName = "HSE Assessment", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"HSEOfficer"}, SlaHours = 48, NextStepId = "RISK_LEVEL", Description = "HSE Officer assesses near miss and determines risk level" },
                new() { StepId = "RISK_LEVEL", StepName = "Determine Risk Level", SequenceNumber = 3, StepType = "SYSTEM", IsRequired = true, NextStepId = "PREVENTIVE_ACTION", Description = "System calculates risk matrix (likelihood × consequence)" },
                new() { StepId = "PREVENTIVE_ACTION", StepName = "Implement Preventive Action", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"FieldEngineer"}, SlaHours = 120, Description = "Preventive action implemented to avoid recurrence" },
            },
            Configuration = new() { ["category"] = "HSE", ["anyPersonaCanInitiate"] = true }
        }, userId);
    }

    /// <summary>CRW-17: Engineer requests permit → HSE reviews → permit issued → work executed → permit closed</summary>
    private async Task InitializePermitToWorkAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_PERMIT_TO_WORK",
            ProcessName = "Permit to Work",
            ProcessType = "HSE",
            EntityType = "PERMIT",
            Description = "Engineer requests permit → HSE reviews hazards → permit issued → work executed → permit closed",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "PERMIT_REQUEST", StepName = "Request Permit", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"FieldEngineer","ProductionEngineer","DrillingEngineer","FacilitiesEngineer"}, SlaHours = 24, NextStepId = "HAZARD_REVIEW", Description = "Engineer requests permit to work with scope and duration" },
                new() { StepId = "HAZARD_REVIEW", StepName = "Hazard Review", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"HSEOfficer"}, SlaHours = 24, NextStepId = "PERMIT_ISSUE", Description = "HSE Officer reviews hazards, isolation requirements, and PPE needs" },
                new() { StepId = "PERMIT_ISSUE", StepName = "Issue Permit", SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"HSEOfficer"}, SlaHours = 8, NextStepId = "WORK_EXECUTE", Description = "HSE Officer issues permit with conditions and validity period" },
                new() { StepId = "WORK_EXECUTE", StepName = "Execute Work", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"FieldEngineer"}, SlaHours = 72, NextStepId = "PERMIT_CLOSE", Description = "Engineer executes work within permit conditions" },
                new() { StepId = "PERMIT_CLOSE", StepName = "Close Permit", SequenceNumber = 5, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"FieldEngineer"}, SlaHours = 4, Description = "Engineer closes permit after work completion and site restoration" },
            },
            Configuration = new() { ["category"] = "HSE", ["regulation"] = "OSHA 1910" }
        }, userId);
    }

    /// <summary>CRW-18: Filing prepared → Compliance reviews → Legal reviews → Executive signs → submitted</summary>
    private async Task InitializeRegulatoryFilingReviewAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_REGULATORY_FILING",
            ProcessName = "Regulatory Filing Review",
            ProcessType = "COMPLIANCE",
            EntityType = "REGULATORY_FILING",
            Description = "Filing prepared → Compliance reviews → Legal reviews (if needed) → Executive signs → submitted to regulator",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "FILING_PREPARE", StepName = "Prepare Filing", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"ComplianceOfficer"}, SlaHours = 120, NextStepId = "COMPLIANCE_REVIEW", Description = "Compliance Officer prepares regulatory filing with supporting data" },
                new() { StepId = "COMPLIANCE_REVIEW", StepName = "Compliance Review", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ComplianceOfficer"}, SlaHours = 72, NextStepId = "LEGAL_REVIEW", Description = "Internal compliance review for accuracy and completeness" },
                new() { StepId = "LEGAL_REVIEW", StepName = "Legal Review", SequenceNumber = 3, StepType = "REVIEW", IsRequired = false, RequiredRoles = new(){"ComplianceOfficer"}, SlaHours = 120, NextStepId = "EXECUTIVE_SIGN", Description = "Optional legal review for complex or high-risk filings" },
                new() { StepId = "EXECUTIVE_SIGN", StepName = "Executive Sign-Off", SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Executive"}, SlaHours = 48, NextStepId = "SUBMIT", Description = "Executive reviews and signs off on regulatory filing" },
                new() { StepId = "SUBMIT", StepName = "Submit to Regulator", SequenceNumber = 5, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"ComplianceOfficer"}, SlaHours = 24, Description = "Filing submitted to regulatory agency with confirmation tracking" },
            },
            Configuration = new() { ["category"] = "COMPLIANCE" }
        }, userId);
    }

    /// <summary>CRW-19: Spill detected → immediate response → HSE notified → regulatory notification → remediation → closure</summary>
    private async Task InitializeEnvironmentalSpillResponseAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_ENV_SPILL_RESPONSE",
            ProcessName = "Environmental Spill Response",
            ProcessType = "HSE",
            EntityType = "HSE_INCIDENT",
            Description = "Spill detected → immediate response → HSE notified → regulatory notification (if reportable) → remediation → closure",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "SPILL_DETECT", StepName = "Spill Detected", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, SlaHours = 1, NextStepId = "IMMEDIATE_RESPONSE", Description = "Any worker detects and reports spill immediately" },
                new() { StepId = "IMMEDIATE_RESPONSE", StepName = "Immediate Response", SequenceNumber = 2, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"FieldEngineer","ProductionEngineer"}, SlaHours = 4, NextStepId = "HSE_NOTIFY", Description = "Stop source, contain spill, initiate emergency response if needed" },
                new() { StepId = "HSE_NOTIFY", StepName = "Notify HSE", SequenceNumber = 3, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"FieldEngineer"}, SlaHours = 2, NextStepId = "REGULATORY_CHECK", Description = "HSE Officer notified with spill details" },
                new() { StepId = "REGULATORY_CHECK", StepName = "Regulatory Notification Check", SequenceNumber = 4, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"HSEOfficer","ComplianceOfficer"}, SlaHours = 4, NextStepId = "REMEDIATION", Description = "Determine if spill exceeds reportable quantities — notify regulator if required" },
                new() { StepId = "REMEDIATION", StepName = "Remediation", SequenceNumber = 5, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"FieldEngineer"}, SlaHours = 720, NextStepId = "CLOSURE", Description = "Execute remediation plan, soil/water sampling, waste disposal" },
                new() { StepId = "CLOSURE", StepName = "Closure & Reporting", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"HSEOfficer","ComplianceOfficer"}, SlaHours = 72, Description = "HSE and Compliance verify remediation complete and file closure report" },
            },
            Configuration = new() { ["category"] = "HSE", ["regulation"] = "EPA, NPDES", ["priority"] = "CRITICAL" }
        }, userId);
    }

    /// <summary>CRW-20: Emissions collected → Production validates → Compliance compiles → regulatory submission</summary>
    private async Task InitializeGhgEmissionsReportAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_GHG_EMISSIONS_REPORT",
            ProcessName = "GHG Emissions Report",
            ProcessType = "COMPLIANCE",
            EntityType = "EMISSION_RECORD",
            Description = "Emissions data collected → Production validates → Compliance compiles → regulatory submission",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "DATA_COLLECT", StepName = "Collect Emissions Data", SequenceNumber = 1, StepType = "SYSTEM", IsRequired = true, NextStepId = "PROD_VALIDATE", Description = "System collects emissions data from meters and calculations" },
                new() { StepId = "PROD_VALIDATE", StepName = "Validate Emissions Data", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ProductionEngineer"}, SlaHours = 72, NextStepId = "COMPLIANCE_COMPILE", Description = "Production Engineer validates emissions data accuracy" },
                new() { StepId = "COMPLIANCE_COMPILE", StepName = "Compile GHG Report", SequenceNumber = 3, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"ComplianceOfficer"}, SlaHours = 120, NextStepId = "MANAGER_APPROVE", Description = "Compliance Officer compiles GHG emissions report per regulatory format" },
                new() { StepId = "MANAGER_APPROVE", StepName = "Manager Approval", SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48, NextStepId = "SUBMIT", Description = "Manager approves emissions report" },
                new() { StepId = "SUBMIT", StepName = "Submit to Regulator", SequenceNumber = 5, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"ComplianceOfficer"}, SlaHours = 24, Description = "Submit GHG report to environmental regulator" },
            },
            Configuration = new() { ["category"] = "COMPLIANCE", ["regulation"] = "EPA GHG Reporting Program", ["schedule"] = "ANNUAL" }
        }, userId);
    }
}
