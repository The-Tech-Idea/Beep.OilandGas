using Beep.OilandGas.Models.Processes;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Oil & Gas Accounting Standards — Phases B, C, D.
/// B: COPAS & Joint Operations (JIB overhead, non-consent, imbalance, COPAS audit)
/// C: Hedging, Inventory & Contracts (ASC 815, ASC 330, Take-or-Pay, PSC)
/// D: Reserves, Decommissioning & Reporting (Estimate revision, RBL redetermination)
/// </summary>
public partial class ProcessDefinitionInitializer
{
    private async Task InitializeAccountingStandardsPhaseBCDAsync(string userId)
    {
        // Phase B — COPAS & Joint Operations
        await InitializeCopasOverheadJibAsync(userId);
        await InitializeNonConsentPenaltyAsync(userId);
        await InitializeProductionImbalanceAsync(userId);
        await InitializeCopasAuditAsync(userId);
        // Phase C — Hedging, Inventory & Contracts
        await InitializeHedgeEffectivenessAsync(userId);
        await InitializeInventoryLCMAsync(userId);
        await InitializeTakeOrPayAsync(userId);
        await InitializePSCAccountingAsync(userId);
        // Phase D — Reserves, Decommissioning & Reporting
        await InitializeDecomEstimateRevisionAsync(userId);
        await InitializeRBLRedeterminationAsync(userId);
    }

    // ═══════════════════════════════════════════════════════════════
    // Phase B — COPAS & Joint Operations
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Enhanced JIB with COPAS Overhead (extends CRW-04). COPAS MRP 2 / MFI-22.</summary>
    private async Task InitializeCopasOverheadJibAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_JIB_COPAS_OVERHEAD",
            ProcessName = "JIB with COPAS Overhead & Non-Consent",
            ProcessType = "FINANCIAL",
            EntityType = "COST_TRANSACTION",
            Description = "Enhanced JIB processing per COPAS MRP 2: apply overhead rates, identify non-consenting partners, calculate penalties, track audit adjustment window.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "COST_GATHER", StepName = "Gather Joint Costs", SequenceNumber = 1, StepType = "SYSTEM", IsRequired = true, NextStepId = "OVERHEAD_APPLY", Description = "System gathers all joint account costs: drilling, workover, equipment, services, labor" },
                new() { StepId = "OVERHEAD_APPLY", StepName = "Apply COPAS Overhead Rates", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "WI_ALLOCATE", Description = "Apply COPAS overhead: fixed rate + (variable rate × well count × depth factor). Per COPAS MFI-22 accounting procedure." },
                new() { StepId = "WI_ALLOCATE", StepName = "Allocate to Working Interest Owners", SequenceNumber = 3, StepType = "SYSTEM", IsRequired = true, NextStepId = "NONCONSENT_CHECK", Description = "Allocate costs per working interest percentages from division order / JOA" },
                new() { StepId = "NONCONSENT_CHECK", StepName = "Identify Non-Consenting Partners", SequenceNumber = 4, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "PENALTY_APPLY", Description = "Check AFE responses: identify non-consenting WI owners. Per JOA Article VI.B." },
                new() { StepId = "PENALTY_APPLY", StepName = "Apply Non-Consent Penalty", SequenceNumber = 5, StepType = "SYSTEM", IsRequired = false, NextStepId = "JIB_GENERATE", Description = "Apply penalty rate (100-500% per JOA) to non-consenting owner's share. Track cost recoupment from future production." },
                new() { StepId = "JIB_GENERATE", StepName = "Generate JIB Statement", SequenceNumber = 6, StepType = "SYSTEM", IsRequired = true, NextStepId = "JIB_REVIEW", Description = "Generate JIB statement with: participant detail, cost categories, overhead, penalties, net amount due" },
                new() { StepId = "JIB_REVIEW", StepName = "Accountant Review", SequenceNumber = 7, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "JIB_APPROVE", Description = "Accountant reviews JIB: verify interest percentages, overhead rates, penalty calculations" },
                new() { StepId = "JIB_APPROVE", StepName = "Manager Approval", SequenceNumber = 8, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48, NextStepId = "JIB_SEND", Description = "Manager approves JIB statements before distribution to partners" },
                new() { StepId = "JIB_SEND", StepName = "Distribute to Partners", SequenceNumber = 9, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 24, NextStepId = "AUDIT_WINDOW", Description = "Send JIB statements to partners. Begin 2-year COPAS audit adjustment window." },
                new() { StepId = "AUDIT_WINDOW", StepName = "Track Audit Window", SequenceNumber = 10, StepType = "SYSTEM", IsRequired = true, Description = "Track COPAS 2-year audit window per MRP 2. Flag statements approaching expiry for partner audit rights." },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "COPAS MRP 2, MFI-22", ["schedule"] = "MONTHLY", ["auditWindowMonths"] = 24, ["sodEnforced"] = true }
        }, userId);
    }

    /// <summary>Non-Consent Penalty Calculation (COPAS / JOA Article VI).</summary>
    private async Task InitializeNonConsentPenaltyAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_NONCONSENT",
            ProcessName = "Non-Consent Penalty Calculation",
            ProcessType = "FINANCIAL",
            EntityType = "AFE",
            Description = "Calculate non-consent penalties per JOA Art VI. WI owners identified via BA (BA_CATEGORY='Working Interest Owner') linked to AFE via BA_XREF. Track penalty recoupment from production revenue.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "AFE_DISTRIBUTE", StepName = "Distribute AFE to WI Owners", SequenceNumber = 1, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "RESPONSE_TRACK", Description = "Send AFE to all working interest owners with response deadline (typically 30 days per JOA)" },
                new() { StepId = "RESPONSE_TRACK", StepName = "Track Responses", SequenceNumber = 2, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 720, NextStepId = "PENALTY_CALC", Description = "Track each owner's response: consent, non-consent, or no response (deemed non-consent per JOA)" },
                new() { StepId = "PENALTY_CALC", StepName = "Calculate Non-Consent Penalty", SequenceNumber = 3, StepType = "SYSTEM", IsRequired = true, NextStepId = "RECOUP_TRACK", Description = "Penalty = non-consenting WI share × penalty rate (100-500%). Rate per JOA Exhibit A. Penalty applied to cost recovery from production." },
                new() { StepId = "RECOUP_TRACK", StepName = "Track Cost Recoupment", SequenceNumber = 4, StepType = "SYSTEM", IsRequired = true, NextStepId = "RECOUP_COMPLETE", Description = "Track penalty recoupment from non-consenting owner's production revenue. Monthly: apply production proceeds to penalty balance." },
                new() { StepId = "RECOUP_COMPLETE", StepName = "Recoupment Complete", SequenceNumber = 5, StepType = "SYSTEM", IsRequired = false, Description = "Once penalty fully recouped (usually 100-300% of share), revert to standard WI. Notify partners." },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "COPAS MRP 2, JOA Art VI", ["penaltyRange"] = "100-500%", ["responseDays"] = 30 }
        }, userId);
    }

    /// <summary>Production Imbalance Settlement (COPAS MFI-1).</summary>
    private async Task InitializeProductionImbalanceAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_PRODUCTION_IMBALANCE",
            ProcessName = "Production Imbalance Settlement",
            ProcessType = "FINANCIAL",
            EntityType = "PDEN_VOL_SUMMARY",
            Description = "Track over/under-lift imbalances between WI owners (BA with BA_CATEGORY='Working Interest Owner'). Entitlement per BA linked via DIVISION_ORDER. Cash settlement or make-up delivery per COPAS MFI-1.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "ENTITLEMENT_CALC", StepName = "Calculate Entitlements", SequenceNumber = 1, StepType = "SYSTEM", IsRequired = true, NextStepId = "ACTUAL_RECORD", Description = "Calculate each owner's entitled share: WI% × NRI% × monthly production volume" },
                new() { StepId = "ACTUAL_RECORD", StepName = "Record Actual Lifts", SequenceNumber = 2, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 72, NextStepId = "IMBALANCE_CALC", Description = "Record actual volumes lifted by each owner from run tickets / pipeline statements" },
                new() { StepId = "IMBALANCE_CALC", StepName = "Calculate Imbalances", SequenceNumber = 3, StepType = "SYSTEM", IsRequired = true, NextStepId = "THRESHOLD_CHECK", Description = "Imbalance = Entitled - Actual. Track cumulative imbalance per owner. Calculate monetary value at current market price." },
                new() { StepId = "THRESHOLD_CHECK", StepName = "Threshold Check", SequenceNumber = 4, StepType = "SYSTEM", IsRequired = true, NextStepId = "SETTLEMENT", Description = "If cumulative imbalance > threshold (typically 5% of monthly entitlement) → trigger settlement" },
                new() { StepId = "SETTLEMENT", StepName = "Settlement", SequenceNumber = 5, StepType = "ACTION", IsRequired = false, RequiredRoles = new(){"Accountant"}, SlaHours = 168, NextStepId = "JOURNAL_POST", Description = "Execute settlement: cash payment for over-lift OR make-up delivery for under-lift. Per operating agreement terms." },
                new() { StepId = "JOURNAL_POST", StepName = "Post Settlement Entry", SequenceNumber = 6, StepType = "SYSTEM", IsRequired = false, Description = "Post GL entry: Dr/Cr Accounts Receivable/Payable, Cr/Dr Revenue for settlement" },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "COPAS MFI-1", ["schedule"] = "MONTHLY", ["imbalanceThreshold"] = "5%" }
        }, userId);
    }

    /// <summary>COPAS Overhead Rate Audit (COPAS MFI-22).</summary>
    private async Task InitializeCopasAuditAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_COPAS_AUDIT",
            ProcessName = "COPAS Overhead Rate Audit",
            ProcessType = "FINANCIAL",
            EntityType = "COST_TRANSACTION",
            Description = "Annual COPAS overhead rate proposal, partner review, and audit. Operator proposes rates with cost support; non-operators review and challenge. Per COPAS MFI-22.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "RATE_PROPOSE", StepName = "Propose Overhead Rates", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 168, NextStepId = "COST_SUPPORT", Description = "Operator proposes fixed and variable overhead rates for next year. Rate = actual costs ÷ activity base (well count, depth, etc.)." },
                new() { StepId = "COST_SUPPORT", StepName = "Provide Cost Support", SequenceNumber = 2, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 72, NextStepId = "PARTNER_REVIEW", Description = "Provide cost support package: actual overhead expenses by category, well counts, depth data, calculation methodology" },
                new() { StepId = "PARTNER_REVIEW", StepName = "Partner Review Period", SequenceNumber = 3, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 720, NextStepId = "CHALLENGE_RESOLVE", Description = "Non-operators review rates (30-60 day review period). May request additional data or challenge rates." },
                new() { StepId = "CHALLENGE_RESOLVE", StepName = "Resolve Challenges", SequenceNumber = 4, StepType = "REVIEW", IsRequired = false, RequiredRoles = new(){"Accountant","Manager"}, SlaHours = 240, NextStepId = "RATE_FINAL", Description = "Negotiate challenged rates. If unresolved → default COPAS rate per MFI-22 Appendix A." },
                new() { StepId = "RATE_FINAL", StepName = "Finalize Rates", SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48, NextStepId = "QUARTERLY_RECONCILE", Description = "Finalize approved overhead rates for the year. Update system with new rates effective January 1." },
                new() { StepId = "QUARTERLY_RECONCILE", StepName = "Quarterly Reconciliation", SequenceNumber = 6, StepType = "SYSTEM", IsRequired = true, Description = "Quarterly: compare actual overhead costs to recovered overhead. Report variance to partners per MFI-22." },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "COPAS MFI-22", ["schedule"] = "ANNUAL;QUARTERLY_RECONCILE", ["reviewPeriodDays"] = 60 }
        }, userId);
    }

    // ═══════════════════════════════════════════════════════════════
    // Phase C — Hedging, Inventory & Contracts
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Hedge Effectiveness Testing (FASB ASC 815).</summary>
    private async Task InitializeHedgeEffectivenessAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_HEDGE_EFFECTIVENESS",
            ProcessName = "Hedge Effectiveness Testing (ASC 815)",
            ProcessType = "FINANCIAL",
            EntityType = "FINANCIAL_INSTRUMENT",
            Description = "Quarterly hedge effectiveness testing per FASB ASC 815. Regression or dollar-offset method. Split changes into effective (OCI) and ineffective (P&L) portions.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "HEDGE_DESIGNATE", StepName = "Designate Hedge Relationship", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 72, NextStepId = "EFFECTIVENESS_TEST", Description = "Formal hedge designation: hedged item (forecast production), hedging instrument (swap/option/collar), risk (commodity price), assessment method" },
                new() { StepId = "EFFECTIVENESS_TEST", StepName = "Perform Effectiveness Test", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "FAIR_VALUE", Description = "Quarterly: regression analysis (R² ≥ 0.80) or dollar-offset (80-125%). If outside range → ineffective." },
                new() { StepId = "FAIR_VALUE", StepName = "Mark-to-Market Valuation", SequenceNumber = 3, StepType = "SYSTEM", IsRequired = true, NextStepId = "EFFECTIVE_SPLIT", Description = "Calculate fair value of derivative using current forward curve. MtM change = total change in fair value." },
                new() { StepId = "EFFECTIVE_SPLIT", StepName = "Split Effective vs Ineffective", SequenceNumber = 4, StepType = "SYSTEM", IsRequired = true, NextStepId = "HEDGE_REVIEW", Description = "Effective portion → OCI (cash flow hedge reserve). Ineffective portion → P&L (current earnings)." },
                new() { StepId = "HEDGE_REVIEW", StepName = "Accountant Review", SequenceNumber = 5, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "HEDGE_APPROVE", Description = "Accountant reviews effectiveness test, fair value, and OCI/P&L split" },
                new() { StepId = "HEDGE_APPROVE", StepName = "Controller Approval", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 24, NextStepId = "HEDGE_POST", Description = "Controller approves hedge accounting treatment and journal entries" },
                new() { StepId = "HEDGE_POST", StepName = "Post Hedge Entries & Disclosures", SequenceNumber = 7, StepType = "SYSTEM", IsRequired = true, Description = "Post OCI and P&L entries. Update hedge disclosure: notional, fair value, maturity, gains/losses in OCI, amounts reclassified to earnings." },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "FASB ASC 815", ["schedule"] = "QUARTERLY", ["effectivenessThreshold"] = "80-125%", ["sodEnforced"] = true }
        }, userId);
    }

    /// <summary>Inventory LCM Assessment (FASB ASC 330).</summary>
    private async Task InitializeInventoryLCMAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_INVENTORY_LCM",
            ProcessName = "Inventory LCM Assessment",
            ProcessType = "FINANCIAL",
            EntityType = "INVENTORY",
            Description = "Lower of Cost or Market assessment for O&G inventory: tubular goods, wellbore materials, chemicals, oil in tanks. FASB ASC 330 compliant.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "INVENTORY_GATHER", StepName = "Gather Inventory Balances", SequenceNumber = 1, StepType = "SYSTEM", IsRequired = true, NextStepId = "MARKET_PRICE", Description = "Gather inventory by category: tubular goods, wellbore materials, chemicals, crude oil in tanks, NGLs" },
                new() { StepId = "MARKET_PRICE", StepName = "Get Market Prices", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "LCM_COMPARE", Description = "Current market price for each category: steel prices for tubulars, spot oil price for oil in tanks, chemical indices" },
                new() { StepId = "LCM_COMPARE", StepName = "Compare Cost vs Market", SequenceNumber = 3, StepType = "SYSTEM", IsRequired = true, NextStepId = "NRV_CHECK", Description = "For each category: if market < cost → potential write-down. Calculate LCM adjustment amount." },
                new() { StepId = "NRV_CHECK", StepName = "NRV Check (Oil in Tanks)", SequenceNumber = 4, StepType = "SYSTEM", IsRequired = false, NextStepId = "LCM_REVIEW", Description = "For oil in tanks: NRV = spot price - lifting costs. If NRV < carrying cost → write-down." },
                new() { StepId = "LCM_REVIEW", StepName = "Accountant Review", SequenceNumber = 5, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "LCM_APPROVE", Description = "Accountant reviews LCM calculation: verify prices, categories, write-down amounts" },
                new() { StepId = "LCM_APPROVE", StepName = "Manager Approval", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 24, NextStepId = "LCM_POST", Description = "Manager approves LCM adjustment. Note: GAAP does not allow LCM reversal if prices recover." },
                new() { StepId = "LCM_POST", StepName = "Post LCM Entry", SequenceNumber = 7, StepType = "SYSTEM", IsRequired = true, Description = "Dr Inventory Write-Down (COGS), Cr Inventory Reserve. Disclose in financial statement notes." },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "FASB ASC 330", ["schedule"] = "QUARTERLY", ["sodEnforced"] = true }
        }, userId);
    }

    /// <summary>Take-or-Pay Contract Accounting (FASB ASC 440).</summary>
    private async Task InitializeTakeOrPayAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_TAKE_OR_PAY",
            ProcessName = "Take-or-Pay Contract Accounting",
            ProcessType = "FINANCIAL",
            EntityType = "CONTRACT",
            Description = "Track take-or-pay contract obligations: minimum volume commitments, deficiency calculations, liability recognition, makeup right tracking. FASB ASC 440 compliant.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "CONTRACT_LOAD", StepName = "Load Contract Terms", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "DEFICIENCY_CALC", Description = "Load TOP contract: minimum volume, price, period, makeup rights, expiry terms" },
                new() { StepId = "DEFICIENCY_CALC", StepName = "Calculate Deficiency", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "LIABILITY_CHECK", Description = "Deficiency = Minimum Volume - Actual Deliveries (period-to-date). Value at contract price." },
                new() { StepId = "LIABILITY_CHECK", StepName = "Assess Liability Recognition", SequenceNumber = 3, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "LIABILITY_POST", Description = "If probable that deficiency will result in payment → recognize liability. Dr Loss on TOP, Cr TOP Liability." },
                new() { StepId = "LIABILITY_POST", StepName = "Post Liability (if needed)", SequenceNumber = 4, StepType = "SYSTEM", IsRequired = false, NextStepId = "MAKEUP_TRACK", Description = "Post liability for expected deficiency payment." },
                new() { StepId = "MAKEUP_TRACK", StepName = "Track Makeup Rights", SequenceNumber = 5, StepType = "SYSTEM", IsRequired = true, NextStepId = "MAKEUP_EXPIRY", Description = "Track makeup deliveries: future volumes above minimum offset prior deficiencies. Reduce liability as makeup rights are exercised." },
                new() { StepId = "MAKEUP_EXPIRY", StepName = "Handle Expired Makeup Rights", SequenceNumber = 6, StepType = "SYSTEM", IsRequired = false, Description = "If makeup rights expire unused → reverse liability, recognize gain. Dr TOP Liability, Cr Gain on TOP Expiry." },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "FASB ASC 440", ["schedule"] = "MONTHLY" }
        }, userId);
    }

    /// <summary>Production Sharing Contract Accounting (IFRS 6 / Industry practice).</summary>
    private async Task InitializePSCAccountingAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_PSC_ACCOUNTING",
            ProcessName = "Production Sharing Contract (PSC) Accounting",
            ProcessType = "FINANCIAL",
            EntityType = "FIELD",
            Description = "PSC accounting per IFRS 6: cost oil recovery, profit oil split, government share, contractor entitlement. Supports R-factor and cost recovery ceiling models.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "COST_POOL", StepName = "Determine Cost Recovery Pool", SequenceNumber = 1, StepType = "SYSTEM", IsRequired = true, NextStepId = "COST_OIL", Description = "Gather recoverable costs: exploration, development, operating costs. Apply cost recovery ceiling (typically 40-60% of production)." },
                new() { StepId = "COST_OIL", StepName = "Calculate Cost Oil", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "PROFIT_OIL", Description = "Cost Oil = min(recoverable costs, cost recovery ceiling × gross production). Operator recovers costs from production revenue." },
                new() { StepId = "PROFIT_OIL", StepName = "Calculate Profit Oil Split", SequenceNumber = 3, StepType = "SYSTEM", IsRequired = true, NextStepId = "R_FACTOR", Description = "Profit Oil = Gross Revenue - Cost Oil - Royalty. Split per PSC terms (government share typically 50-80%, varies by cumulative production)." },
                new() { StepId = "R_FACTOR", StepName = "Apply R-Factor (if applicable)", SequenceNumber = 4, StepType = "SYSTEM", IsRequired = false, NextStepId = "TAX_CALC", Description = "R-Factor = Cumulative Revenue / Cumulative Costs. As R-factor increases, government share of profit oil increases per PSC sliding scale." },
                new() { StepId = "TAX_CALC", StepName = "Calculate Government Tax", SequenceNumber = 5, StepType = "SYSTEM", IsRequired = true, NextStepId = "ENTITLEMENT", Description = "Calculate income tax on contractor's profit oil share. Apply PSC tax rate (often different from statutory rate)." },
                new() { StepId = "ENTITLEMENT", StepName = "Calculate Net Entitlement", SequenceNumber = 6, StepType = "SYSTEM", IsRequired = true, NextStepId = "PSC_REVIEW", Description = "Contractor Net Entitlement = Cost Oil + (Contractor Share × Profit Oil) - Tax. This is the contractor's take from production." },
                new() { StepId = "PSC_REVIEW", StepName = "Accountant Review", SequenceNumber = 7, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "PSC_APPROVE", Description = "Accountant reviews PSC calculation: cost recovery, R-factor, profit split, tax, entitlement" },
                new() { StepId = "PSC_APPROVE", StepName = "Manager Approval", SequenceNumber = 8, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 24, NextStepId = "PSC_POST", Description = "Manager approves PSC calculation and journal entries" },
                new() { StepId = "PSC_POST", StepName = "Post PSC Entries", SequenceNumber = 9, StepType = "SYSTEM", IsRequired = true, Description = "Post entries: Dr Cost Oil Recovery, Dr Contractor Profit Oil, Cr Government Share, Cr Revenue, Dr Tax Expense" },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "IFRS 6", ["schedule"] = "MONTHLY", ["costRecoveryCeiling"] = "40-60%", ["sodEnforced"] = true }
        }, userId);
    }

    // ═══════════════════════════════════════════════════════════════
    // Phase D — Reserves, Decommissioning & Reporting
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Decommissioning Cost Estimate Revision (FASB ASC 410-20).</summary>
    private async Task InitializeDecomEstimateRevisionAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_DECOM_ESTIMATE_REVISION",
            ProcessName = "Decommissioning Cost Estimate Revision",
            ProcessType = "FINANCIAL",
            EntityType = "WELL",
            Description = "Annual (or event-driven) revision of decommissioning cost estimates. Updates ARO liability per revised estimates. FASB ASC 410-20-35 compliant.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "REVISION_TRIGGER", StepName = "Trigger Estimate Revision", SequenceNumber = 1, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"DecommissioningCoordinator"}, SlaHours = 72, NextStepId = "COST_UPDATE", Description = "Annual review OR event-driven: new regulation, cost change > 10%, timing change, new obligation identified" },
                new() { StepId = "COST_UPDATE", StepName = "Update Cost Estimates", SequenceNumber = 2, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"DecommissioningCoordinator"}, SlaHours = 168, NextStepId = "INFLATION_APPLY", Description = "Update: plugging cost, facility removal, site restoration, monitoring. Per well or per field as appropriate." },
                new() { StepId = "INFLATION_APPLY", StepName = "Apply Inflation & Discounting", SequenceNumber = 3, StepType = "SYSTEM", IsRequired = true, NextStepId = "ARO_ADJUST", Description = "Apply inflation to future costs (2-3% typical). Discount to present value using credit-adjusted risk-free rate (ASC 410-20-30)." },
                new() { StepId = "ARO_ADJUST", StepName = "Calculate ARO Adjustment", SequenceNumber = 4, StepType = "SYSTEM", IsRequired = true, NextStepId = "REVISION_REVIEW", Description = "New PV - Old PV = ARO adjustment. Upward revision → Dr Asset, Cr ARO. Downward → Dr ARO (pro rata), Cr Asset." },
                new() { StepId = "REVISION_REVIEW", StepName = "Accountant Review", SequenceNumber = 5, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "REVISION_APPROVE", Description = "Accountant reviews revision: cost changes, inflation rate, discount rate, ARO adjustment calculation" },
                new() { StepId = "REVISION_APPROVE", StepName = "Controller Approval", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 24, NextStepId = "REVISION_POST", Description = "Controller approves ARO revision. Note: significant change may require disclosure in financial statements." },
                new() { StepId = "REVISION_POST", StepName = "Post ARO Revision", SequenceNumber = 7, StepType = "SYSTEM", IsRequired = true, Description = "Post ARO adjustment entry. Update ARO rollforward. If material → prepare footnote disclosure." },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "FASB ASC 410-20-35", ["schedule"] = "ANNUAL;EVENT_DRIVEN", ["materialityThreshold"] = "10%", ["sodEnforced"] = true }
        }, userId);
    }

    /// <summary>Reserves-Based Lending Redetermination (Industry practice).</summary>
    private async Task InitializeRBLRedeterminationAsync(string userId)
    {
        await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
        {
            ProcessId = "CRW_RBL_REDETERMINATION",
            ProcessName = "Reserves-Based Lending Redetermination",
            ProcessType = "FINANCIAL",
            EntityType = "FIELD",
            Description = "Semi-annual RBL borrowing base redetermination. Updated reserves → PV-10 → borrowing base → covenant check → lender submission. Industry standard practice.",
            IsActive = true,
            Steps = new List<ProcessStepDefinition>
            {
                new() { StepId = "RESERVES_UPDATE", StepName = "Update Reserves Report", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"ReservoirEngineer"}, SlaHours = 336, NextStepId = "PV10_CALC", Description = "Update proved developed producing (PDP) reserves. Include: production history, well performance, price deck (lender's pricing)." },
                new() { StepId = "PV10_CALC", StepName = "Calculate PV-10 at Strip Pricing", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "BORROWING_BASE", Description = "PV-10 of PDP reserves using lender's price deck (typically NYMEX strip). Discount at 10% per SEC / industry convention." },
                new() { StepId = "BORROWING_BASE", StepName = "Calculate Borrowing Base", SequenceNumber = 3, StepType = "SYSTEM", IsRequired = true, NextStepId = "COVENANT_CHECK", Description = "Borrowing Base = PV-10 × Advance Rate (typically 65-75% for PDP). Apply concentration limits, hedging requirements, and other lender constraints." },
                new() { StepId = "COVENANT_CHECK", StepName = "Check Financial Covenants", SequenceNumber = 4, StepType = "SYSTEM", IsRequired = true, NextStepId = "PACKAGE_PREPARE", Description = "Check: Current Ratio > 1.0, Debt/EBITDA < 3.5x, Interest Coverage > 2.5x, Asset Coverage > 1.5x per credit agreement" },
                new() { StepId = "PACKAGE_PREPARE", StepName = "Prepare Redetermination Package", SequenceNumber = 5, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 120, NextStepId = "LENDER_SUBMIT", Description = "Prepare package: reserves report, PV-10 calculation, financial statements, covenant compliance, hedge position, production forecast" },
                new() { StepId = "LENDER_SUBMIT", StepName = "Submit to Lenders", SequenceNumber = 6, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 24, NextStepId = "LENDER_REVIEW", Description = "Submit redetermination package to bank group. Lender engineering review period begins." },
                new() { StepId = "LENDER_REVIEW", StepName = "Lender Engineering Review", SequenceNumber = 7, StepType = "REVIEW", IsRequired = true, SlaHours = 1440, NextStepId = "NEW_BASE", Description = "Lenders review reserves, run independent PV-10, assess risk. Typical review period: 30-60 days." },
                new() { StepId = "NEW_BASE", StepName = "New Borrowing Base Effective", SequenceNumber = 8, StepType = "SYSTEM", IsRequired = true, NextStepId = "DEFICIENCY_CHECK", Description = "New borrowing base effective date. Update credit facility records." },
                new() { StepId = "DEFICIENCY_CHECK", StepName = "Deficiency Assessment", SequenceNumber = 9, StepType = "SYSTEM", IsRequired = false, Description = "If borrowing base reduced below outstanding balance → deficiency. Prepare repayment plan per credit agreement (typically 6 months)." },
            },
            Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "Credit Agreement", ["schedule"] = "SEMI_ANNUAL", ["advanceRate"] = "65-75%", ["reviewPeriodDays"] = 60 }
        }, userId);
    }
}
