using Beep.OilandGas.Models.Processes;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Oil & Gas Accounting Standards Workflows — Phase A (Critical Compliance).
/// Covers FASB ASC 932 (DD&A), SEC Reg S-X 4-10 (Ceiling Test),
/// FASB ASC 410-20 (ARO), and FASB ASC 360 (Impairment).
/// </summary>
public partial class ProcessDefinitionInitializer
{
    private async Task InitializeAccountingStandardsWorkflowsAsync(string userId)
    {
        await InitializeDDACalculationAsync(userId);
        await InitializeCeilingTestAsync(userId);
        await InitializeAROAccountingAsync(userId);
        await InitializeImpairmentAssessmentAsync(userId);
        await InitializeASC606RevenueRecognitionAsync(userId);
    }

    /// <summary>
    /// CRW_DDA_CALCULATION — DD&A Calculation & Approval (FASB ASC 932-360).
    /// Units-of-Production depletion linked to proved reserves. Monthly cycle.
    /// </summary>
    private async Task InitializeDDACalculationAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_DDA_CALCULATION",
            ProcessName = "DD&A Calculation & Approval",
            ProcessType = "FINANCIAL",
            EntityType = "WELL",
            Description = "Monthly DD&A: Units-of-Production depletion rate calculated from proved reserves, reviewed by Accountant, approved by Controller. FASB ASC 932-360 / SEC Reg S-X compliant.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "RESERVES_VALIDATE", StepName = "Validate Reserves Data", SequenceNumber = 1, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ReservoirEngineer"}, SlaHours = 48, NextStepId = "DDA_CALCULATE", Description = "Reservoir Engineer validates latest SEC proved reserves for UOP depletion rate" },
                new() { StepId = "DDA_CALCULATE", StepName = "Calculate DD&A", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "DDA_REVIEW", Description = "System: UOP depletion = (Period Production × Capitalized Cost) / Proved Reserves. Includes well costs, leasehold, equipment, facilities." },
                new() { StepId = "DDA_REVIEW", StepName = "Accountant Review", SequenceNumber = 3, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "DDA_APPROVE", Description = "Accountant reviews depletion rate, checks reasonableness against prior periods, validates reserve data" },
                new() { StepId = "DDA_APPROVE", StepName = "Controller Approval", SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 24, NextStepId = "DDA_POST", Description = "Controller approves DD&A entry. SoD: Calculator ≠ Reviewer ≠ Approver" },
                new() { StepId = "DDA_POST", StepName = "Post DD&A Journal Entry", SequenceNumber = 5, StepType = "SYSTEM", IsRequired = true, Description = "System posts: Dr DD&A Expense, Cr Accumulated DD&A. Updates depletion rollforward." },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "FASB ASC 932-360, SEC Reg S-X", ["schedule"] = "MONTHLY", ["sodEnforced"] = true }
        }, userId);
    }

    /// <summary>
    /// CRW_CEILING_TEST — Full Cost Ceiling Test (SEC Reg S-X Rule 4-10).
    /// Quarterly comparison of capitalized costs to PV-10 ceiling. Impairment if exceeded.
    /// </summary>
    private async Task InitializeCeilingTestAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_CEILING_TEST",
            ProcessName = "Full Cost Ceiling Test",
            ProcessType = "FINANCIAL",
            EntityType = "FIELD",
            Description = "Quarterly SEC-required ceiling test: compares capitalized costs to PV-10 of proved reserves. Impairment write-down if costs exceed ceiling. SEC Reg S-X Rule 4-10(c)(4) compliant.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "RESERVES_GATHER", StepName = "Gather Reserves & Pricing", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"ReservoirEngineer"}, SlaHours = 72, NextStepId = "PV10_CALCULATE", Description = "Gather SEC proved reserves + 12-month average first-day-of-month pricing" },
                new() { StepId = "PV10_CALCULATE", StepName = "Calculate PV-10", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "CEILING_COMPARE", Description = "System: PV-10 = discounted future net revenue at 10%. Ceiling = PV-10 + costs excluded + tax effects." },
                new() { StepId = "CEILING_COMPARE", StepName = "Compare to Capitalized Costs", SequenceNumber = 3, StepType = "SYSTEM", IsRequired = true, NextStepId = "CEILING_REVIEW", Description = "System: If net capitalized costs > ceiling → impairment required. Calculate write-down amount." },
                new() { StepId = "CEILING_REVIEW", StepName = "Accountant Review", SequenceNumber = 4, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "CEILING_APPROVE", Description = "Accountant reviews ceiling test calculation, verifies pricing and reserve data inputs" },
                new() { StepId = "CEILING_APPROVE", StepName = "Controller Approval", SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 24, NextStepId = "IMPAIRMENT_POST", Description = "Controller approves ceiling test and any resulting impairment" },
                new() { StepId = "IMPAIRMENT_POST", StepName = "Post Impairment (if needed)", SequenceNumber = 6, StepType = "SYSTEM", IsRequired = true, NextStepId = "DISCLOSURE", Description = "If impairment: Dr Impairment Loss, Cr Accumulated DD&A. Update footnote disclosure." },
                new() { StepId = "DISCLOSURE", StepName = "Prepare SEC Disclosure", SequenceNumber = 7, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 72, Description = "Prepare quarterly/annual disclosure: impairment amount, reasons, impact on future DD&A rates" },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "SEC Reg S-X 4-10", ["schedule"] = "QUARTERLY", ["triggerOnPriceDrop"] = "20%", ["sodEnforced"] = true }
        }, userId);
    }

    /// <summary>
    /// CRW_ARO_ACCOUNTING — Asset Retirement Obligation (FASB ASC 410-20).
    /// Initial recognition, monthly accretion, quarterly estimate revision, settlement accounting.
    /// </summary>
    private async Task InitializeAROAccountingAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_ARO_ACCOUNTING",
            ProcessName = "Asset Retirement Obligation (ARO)",
            ProcessType = "FINANCIAL",
            EntityType = "WELL",
            Description = "ARO lifecycle: initial fair value measurement, monthly accretion, quarterly estimate revision, settlement accounting. FASB ASC 410-20 compliant.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "ARO_IDENTIFY", StepName = "Identify ARO Trigger", SequenceNumber = 1, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"DrillingEngineer","ProductionEngineer"}, SlaHours = 72, NextStepId = "ARO_FAIR_VALUE", Description = "Identify legal obligation: well P&A, facility removal, site restoration. Triggered by well spud or regulatory change." },
                new() { StepId = "ARO_FAIR_VALUE", StepName = "Calculate Fair Value", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "ARO_INITIAL", Description = "System: Fair value = expected PV of future cash flows using credit-adjusted risk-free rate" },
                new() { StepId = "ARO_INITIAL", StepName = "Initial Recognition", SequenceNumber = 3, StepType = "SYSTEM", IsRequired = true, NextStepId = "ARO_ACCRETION", Description = "System: Dr Long-Lived Asset (capitalize ARO), Cr ARO Liability. Increases asset carrying amount." },
                new() { StepId = "ARO_ACCRETION", StepName = "Monthly Accretion", SequenceNumber = 4, StepType = "SYSTEM", IsRequired = true, NextStepId = "ARO_REVIEW", Description = "System (monthly): Dr Accretion Expense, Cr ARO Liability. Accretion = PV × discount rate / 12." },
                new() { StepId = "ARO_REVIEW", StepName = "Quarterly Estimate Review", SequenceNumber = 5, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant","DecommissioningCoordinator"}, SlaHours = 72, NextStepId = "ARO_REVISION", Description = "Review ARO estimates: cost changes, timing changes, new obligations, regulation changes" },
                new() { StepId = "ARO_REVISION", StepName = "Apply Revision (if needed)", SequenceNumber = 6, StepType = "SYSTEM", IsRequired = false, NextStepId = "ARO_APPROVE", Description = "If revised: adjust ARO liability and asset carrying amount. Upward revision → capitalize. Downward → reduce liability." },
                new() { StepId = "ARO_APPROVE", StepName = "Controller Approval", SequenceNumber = 7, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 24, Description = "Controller approves ARO estimate and any revisions" },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "FASB ASC 410-20", ["schedule"] = "MONTHLY_ACCRETION;QUARTERLY_REVIEW", ["sodEnforced"] = true }
        }, userId);
    }

    /// <summary>
    /// CRW_IMPAIRMENT — Impairment Assessment (FASB ASC 360-10).
    /// Two-step test: recoverability (undiscounted CF vs carrying amount), then fair value measurement.
    /// </summary>
    private async Task InitializeImpairmentAssessmentAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_IMPAIRMENT",
            ProcessName = "Impairment Assessment (ASC 360)",
            ProcessType = "FINANCIAL",
            EntityType = "FIELD",
            Description = "Two-step impairment test per FASB ASC 360-10: (1) Recoverability — undiscounted future net cash flows vs carrying amount, (2) Fair value measurement. Triggered by impairment indicators.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "TRIGGER_CHECK", StepName = "Check Impairment Indicators", SequenceNumber = 1, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"ReservoirEngineer","Accountant"}, SlaHours = 72, NextStepId = "STEP1_RECOVER", Description = "Check: significant price decline (>20%), negative reserve revision, dry hole, P&A decision, regulatory change, cost overrun" },
                new() { StepId = "STEP1_RECOVER", StepName = "Step 1: Recoverability Test", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "STEP1_REVIEW", Description = "System: Compare undiscounted future net cash flows to carrying amount. If CF > carrying → no impairment. If CF < carrying → proceed to Step 2." },
                new() { StepId = "STEP1_REVIEW", StepName = "Review Step 1 Results", SequenceNumber = 3, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 24, NextStepId = "STEP2_FAIR_VALUE", Description = "Accountant reviews recoverability test. If no impairment indicated → document and close." },
                new() { StepId = "STEP2_FAIR_VALUE", StepName = "Step 2: Fair Value Measurement", SequenceNumber = 4, StepType = "SYSTEM", IsRequired = false, NextStepId = "IMPAIRMENT_CALC", Description = "System: Fair value = PV-10 or market comparable. Impairment = carrying amount - fair value." },
                new() { StepId = "IMPAIRMENT_CALC", StepName = "Calculate Impairment", SequenceNumber = 5, StepType = "SYSTEM", IsRequired = false, NextStepId = "IMPAIRMENT_REVIEW", Description = "Calculate impairment write-down. Allocate to individual assets if CGU-level impairment." },
                new() { StepId = "IMPAIRMENT_REVIEW", StepName = "Accountant Review", SequenceNumber = 6, StepType = "REVIEW", IsRequired = false, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "IMPAIRMENT_APPROVE", Description = "Accountant reviews impairment calculation and allocation" },
                new() { StepId = "IMPAIRMENT_APPROVE", StepName = "Controller Approval", SequenceNumber = 7, StepType = "APPROVAL", IsRequired = false, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 24, NextStepId = "IMPAIRMENT_POST", Description = "Controller approves impairment write-down" },
                new() { StepId = "IMPAIRMENT_POST", StepName = "Post Impairment & Disclosure", SequenceNumber = 8, StepType = "SYSTEM", IsRequired = false, Description = "Dr Impairment Loss, Cr Accumulated DD&A. Prepare footnote disclosure: amount, reasons, assets affected." },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "FASB ASC 360-10", ["trigger"] = "EVENT_DRIVEN;ANNUAL", ["sodEnforced"] = true }
        }, userId);
    }

    /// <summary>
    /// CRW_ASC606_REVENUE — Revenue Recognition (FASB ASC 606 / IFRS 15).
    /// Five-step model for O&G production revenue: contract → obligations → price → allocation → recognition.
    /// </summary>
    private async Task InitializeASC606RevenueRecognitionAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_ASC606_REVENUE",
            ProcessName = "ASC 606 Revenue Recognition (5-Step)",
            ProcessType = "FINANCIAL",
            EntityType = "REVENUE_TRANSACTION",
            Description = "FASB ASC 606 five-step model for O&G production. Customer = BA (BA_CATEGORY='Customer'). Contract linked via BA_XREF to SALES_AGREEMENT. (1) Identify contract with BA, (2) Identify performance obligations, (3) Determine transaction price, (4) Allocate price, (5) Recognize revenue at delivery point.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "STEP1_CONTRACT", StepName = "Step 1: Identify Contract", SequenceNumber = 1, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 24, NextStepId = "STEP2_OBLIGATIONS", Description = "Identify contract with customer: sales agreement, division order, or spot sale. Verify: approval, rights, payment terms, commercial substance, collectability." },
                new() { StepId = "STEP2_OBLIGATIONS", StepName = "Step 2: Performance Obligations", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "STEP3_PRICE", Description = "Identify distinct performance obligations: oil delivery, gas delivery, NGL delivery. Each product is typically a separate obligation." },
                new() { StepId = "STEP3_PRICE", StepName = "Step 3: Transaction Price", SequenceNumber = 3, StepType = "SYSTEM", IsRequired = true, NextStepId = "STEP4_ALLOCATE", Description = "Determine transaction price: index price ± differential, less deductions. Estimate variable consideration (price fluctuations, volume adjustments). Apply constraint if needed." },
                new() { StepId = "STEP4_ALLOCATE", StepName = "Step 4: Allocate Price", SequenceNumber = 4, StepType = "SYSTEM", IsRequired = true, NextStepId = "STEP5_RECOGNIZE", Description = "Allocate transaction price to each performance obligation based on standalone selling price (SSP). Use index price as SSP proxy for commodities." },
                new() { StepId = "STEP5_RECOGNIZE", StepName = "Step 5: Recognize Revenue", SequenceNumber = 5, StepType = "SYSTEM", IsRequired = true, NextStepId = "REVENUE_REVIEW", Description = "Recognize revenue when control transfers (at delivery point). Dr Accounts Receivable, Cr Revenue. Post to GL." },
                new() { StepId = "REVENUE_REVIEW", StepName = "Revenue Review", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48, NextStepId = "DISCLOSURE", Description = "Manager reviews revenue recognition for proper application of ASC 606" },
                new() { StepId = "DISCLOSURE", StepName = "Revenue Disclosure", SequenceNumber = 7, StepType = "DATA_ENTRY", IsRequired = false, RequiredRoles = new(){"Accountant"}, SlaHours = 72, Description = "Prepare ASC 606 disclosures: disaggregation of revenue, contract balances, performance obligations, significant judgments" },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "FASB ASC 606, IFRS 15", ["schedule"] = "MONTHLY", ["sodEnforced"] = true }
        }, userId);
    }
}
