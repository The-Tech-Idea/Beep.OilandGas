using Beep.OilandGas.Models.Processes;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Cross-role workflow definitions — Set 1: Finance & Accounting (CRW-01 through CRW-08).
/// These model the Engineer → Accountant handoff scenarios that are critical for SOX compliance.
/// Part of Phase 3 cross-role orchestration.
/// </summary>
public partial class ProcessDefinitionInitializer
{
    private async Task InitializeCrossRoleFinanceWorkflowsAsync(string userId)
    {
        await InitializeProductionToRevenueAsync(userId);
        await InitializeAfeCostTrackingAsync(userId);
        await InitializeRoyaltyCalculationAsync(userId);
        await InitializeJointInterestBillingAsync(userId);
        await InitializeCapitalVsExpenseAsync(userId);
        await InitializeProductionTaxFilingAsync(userId);
        await InitializePeriodCloseAsync(userId);
        await InitializeBudgetVsActualsAsync(userId);
    }

    /// <summary>CRW-01: Production volumes posted → auto-create revenue transaction → revenue recognition workflow</summary>
    private async Task InitializeProductionToRevenueAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_PRODUCTION_TO_REVENUE",
            ProcessName = "Production → Revenue Recognition",
            ProcessType = "FINANCIAL",
            EntityType = "PDEN_VOL_SUMMARY",
            Description = "Monthly production volumes posted by Engineer → Accountant creates revenue transaction → Manager approves → Revenue posted",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "PROD_VALIDATE", StepName = "Validate Production Data", SequenceNumber = 1, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ProductionEngineer"}, SlaHours = 48, NextStepId = "REVENUE_CREATE", Description = "Production Engineer validates monthly volumes and submits" },
                new() { StepId = "REVENUE_CREATE", StepName = "Create Revenue Transaction", SequenceNumber = 2, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 72, NextStepId = "REVENUE_REVIEW", Description = "Accountant creates REVENUE_TRANSACTION from validated production data" },
                new() { StepId = "REVENUE_REVIEW", StepName = "Revenue Review", SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48, NextStepId = "REVENUE_POST", Description = "Manager reviews revenue recognition for accuracy and completeness" },
                new() { StepId = "REVENUE_POST", StepName = "Post Revenue", SequenceNumber = 4, StepType = "SYSTEM", IsRequired = true, Description = "System posts revenue to GL and triggers royalty calculation workflow" },
            },
            Configuration = new() { ["category"] = "FINANCE", ["triggers"] = "CRW_ROYALTY_CALCULATION", ["slaTotal"] = 168 }
        }, userId);
    }

    /// <summary>CRW-02: AFE actual costs updated → cost review → journal entry posting</summary>
    private async Task InitializeAfeCostTrackingAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_AFE_COST_TRACKING",
            ProcessName = "AFE Cost Tracking → Journal Entry",
            ProcessType = "FINANCIAL",
            EntityType = "AFE",
            Description = "Drilling Engineer updates AFE actual costs → Accountant reviews → Cost journal posted → Variance analysis",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "COST_UPDATE", StepName = "Update AFE Actual Costs", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"DrillingEngineer","PetroleumEngineer"}, SlaHours = 72, NextStepId = "COST_REVIEW", Description = "Engineer updates actual costs against AFE line items" },
                new() { StepId = "COST_REVIEW", StepName = "Cost Review", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "COST_CLASSIFY", Description = "Accountant reviews cost coding (capital vs expense classification)" },
                new() { StepId = "COST_CLASSIFY", StepName = "Classify Costs", SequenceNumber = 3, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 24, NextStepId = "JOURNAL_POST", Description = "Accountant classifies costs and prepares journal entry" },
                new() { StepId = "JOURNAL_POST", StepName = "Post Cost Journal", SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48, NextStepId = "VARIANCE_REPORT", Description = "Manager approves journal entry posting" },
                new() { StepId = "VARIANCE_REPORT", StepName = "Variance Analysis", SequenceNumber = 5, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 72, Description = "Accountant generates budget vs actual variance report" },
            },
            Configuration = new() { ["category"] = "FINANCE", ["doaEnabled"] = true }
        }, userId);
    }

    /// <summary>CRW-03: Production allocated → royalty calculated → payment processed → owner statement</summary>
    private async Task InitializeRoyaltyCalculationAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_ROYALTY_CALCULATION",
            ProcessName = "Royalty Calculation & Payment",
            ProcessType = "FINANCIAL",
            EntityType = "ROYALTY_CALCULATION",
            Description = "Revenue posted → royalty calculated per owner (BA with BA_CATEGORY='Royalty Owner' via BA_XREF to DIVISION_ORDER) → payment processed → owner statement sent to BA_ADDRESS",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "ROYALTY_CALC", StepName = "Calculate Royalties", SequenceNumber = 1, StepType = "SYSTEM", IsRequired = true, NextStepId = "ROYALTY_REVIEW", Description = "System calculates royalties based on revenue, lease terms, and royalty interests" },
                new() { StepId = "ROYALTY_REVIEW", StepName = "Review Royalty Calculation", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "ROYALTY_APPROVE", Description = "Accountant reviews royalty calculation for accuracy" },
                new() { StepId = "ROYALTY_APPROVE", StepName = "Approve Royalty Payment", SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48, NextStepId = "OWNER_STATEMENT", Description = "Manager approves royalty disbursement" },
                new() { StepId = "OWNER_STATEMENT", StepName = "Generate Owner Statement", SequenceNumber = 4, StepType = "SYSTEM", IsRequired = true, Description = "System generates and sends royalty owner statement" },
            },
            Configuration = new() { ["category"] = "FINANCE" }
        }, userId);
    }

    /// <summary>CRW-04: Costs allocated to partners → JIB statement → partner approval → payment</summary>
    private async Task InitializeJointInterestBillingAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_JIB_PROCESSING",
            ProcessName = "Joint Interest Billing (JIB)",
            ProcessType = "FINANCIAL",
            EntityType = "COST_TRANSACTION",
            Description = "Costs allocated to partners (BA with BA_CATEGORY='Working Interest Owner') → JIB statement generated → partner approval → payment collected. Uses BA_ADDRESS for billing address, BA_PREFERENCE for payment terms.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "COST_ALLOCATE", StepName = "Allocate Costs to Partners", SequenceNumber = 1, StepType = "SYSTEM", IsRequired = true, NextStepId = "JIB_GENERATE", Description = "System allocates joint costs per working interest percentages" },
                new() { StepId = "JIB_GENERATE", StepName = "Generate JIB Statement", SequenceNumber = 2, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "JIB_REVIEW", Description = "Accountant generates and reviews JIB statement" },
                new() { StepId = "JIB_REVIEW", StepName = "Review JIB Statement", SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 72, NextStepId = "JIB_SEND", Description = "Manager approves JIB statement before sending to partners" },
                new() { StepId = "JIB_SEND", StepName = "Send to Partners", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 24, Description = "Accountant sends JIB statements to partners and tracks responses" },
            },
            Configuration = new() { ["category"] = "FINANCE" }
        }, userId);
    }

    /// <summary>CRW-05: Engineer classifies costs → Accountant reviews → Manager approves → GL posted</summary>
    private async Task InitializeCapitalVsExpenseAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_CAPITAL_VS_EXPENSE",
            ProcessName = "Capital vs Expense Classification",
            ProcessType = "FINANCIAL",
            EntityType = "AFE_LINE_ITEM",
            Description = "Engineer classifies AFE line items as capital or expense → Accountant reviews → Manager approves → GL posted",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "CLASSIFY_INITIAL", StepName = "Initial Classification", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"DrillingEngineer","PetroleumEngineer"}, SlaHours = 48, NextStepId = "ACCOUNTANT_REVIEW", Description = "Engineer classifies each AFE line item as capital or expense per accounting policy" },
                new() { StepId = "ACCOUNTANT_REVIEW", StepName = "Accountant Review", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "MANAGER_APPROVE", Description = "Accountant validates classification against tax and accounting standards" },
                new() { StepId = "MANAGER_APPROVE", StepName = "Manager Approval", SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48, NextStepId = "GL_POST", Description = "Manager approves final classification" },
                new() { StepId = "GL_POST", StepName = "Post to GL", SequenceNumber = 4, StepType = "SYSTEM", IsRequired = true, Description = "System posts classified costs to appropriate GL accounts" },
            },
            Configuration = new() { ["category"] = "FINANCE", ["regulation"] = "SOX 404" }
        }, userId);
    }

    /// <summary>CRW-06: Revenue → tax calculated → compliance review → regulatory filing</summary>
    private async Task InitializeProductionTaxFilingAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_PRODUCTION_TAX_FILING",
            ProcessName = "Production Tax Filing",
            ProcessType = "COMPLIANCE",
            EntityType = "REVENUE_TRANSACTION",
            Description = "Revenue recognized → tax calculated → Compliance Officer reviews → regulatory filing submitted",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "TAX_CALC", StepName = "Calculate Production Taxes", SequenceNumber = 1, StepType = "SYSTEM", IsRequired = true, NextStepId = "TAX_REVIEW", Description = "System calculates severance, ad valorem, and production taxes" },
                new() { StepId = "TAX_REVIEW", StepName = "Review Tax Calculation", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "COMPLIANCE_REVIEW", Description = "Accountant reviews tax calculations" },
                new() { StepId = "COMPLIANCE_REVIEW", StepName = "Compliance Review", SequenceNumber = 3, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ComplianceOfficer"}, SlaHours = 72, NextStepId = "FILE_TAX", Description = "Compliance Officer reviews for regulatory compliance" },
                new() { StepId = "FILE_TAX", StepName = "File Tax Return", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"ComplianceOfficer"}, SlaHours = 24, Description = "Compliance Officer files tax return with regulatory agency" },
            },
            Configuration = new() { ["category"] = "COMPLIANCE", ["regulation"] = "SEC, IRS" }
        }, userId);
    }

    /// <summary>CRW-07: Month-end entries posted → Accountant reconciles → Manager reviews → Executive signs off</summary>
    private async Task InitializePeriodCloseAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_PERIOD_CLOSE",
            ProcessName = "Period Close",
            ProcessType = "FINANCIAL",
            EntityType = "JOURNAL_ENTRY",
            Description = "Month-end journal entries posted → Accountant reconciles → Manager reviews → Executive sign-off",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "JE_POST", StepName = "Post Month-End Entries", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 72, NextStepId = "RECONCILE", Description = "Accountant posts accruals, depreciation, depletion, and amortization entries" },
                new() { StepId = "RECONCILE", StepName = "Reconcile Accounts", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "MANAGER_REVIEW", Description = "Accountant reconciles all balance sheet and P&L accounts" },
                new() { StepId = "MANAGER_REVIEW", StepName = "Manager Review", SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48, NextStepId = "EXECUTIVE_SIGNOFF", Description = "Manager reviews financial statements and supporting schedules" },
                new() { StepId = "EXECUTIVE_SIGNOFF", StepName = "Executive Sign-Off", SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Executive"}, SlaHours = 24, Description = "Executive reviews and signs off on period-end financials" },
            },
            Configuration = new() { ["category"] = "FINANCE", ["regulation"] = "SOX 404", ["slaTotal"] = 192 }
        }, userId);
    }

    /// <summary>CRW-08: Monthly actuals vs AFE estimate → variance analysis → corrective action</summary>
    private async Task InitializeBudgetVsActualsAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_BUDGET_VS_ACTUALS",
            ProcessName = "Budget vs Actuals Review",
            ProcessType = "FINANCIAL",
            EntityType = "AFE",
            Description = "Monthly actual costs vs AFE estimate → variance analysis → explanation → corrective action if needed",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "VARIANCE_REPORT", StepName = "Generate Variance Report", SequenceNumber = 1, StepType = "SYSTEM", IsRequired = true, NextStepId = "ENGINEER_REVIEW", Description = "System generates budget vs actual variance report for all active AFEs" },
                new() { StepId = "ENGINEER_REVIEW", StepName = "Engineer Review", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"PetroleumEngineer","DrillingEngineer"}, SlaHours = 72, NextStepId = "ACCOUNTANT_REVIEW", Description = "Engineer explains variances > 10% and provides updated forecast" },
                new() { StepId = "ACCOUNTANT_REVIEW", StepName = "Accountant Review", SequenceNumber = 3, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "MANAGER_APPROVE", Description = "Accountant validates variance explanations and updates cost forecasts" },
                new() { StepId = "MANAGER_APPROVE", StepName = "Manager Approval", SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48, Description = "Manager reviews and approves variance report" },
            },
            Configuration = new() { ["category"] = "FINANCE", ["autoSchedule"] = "MONTHLY" }
        }, userId);
    }
}
