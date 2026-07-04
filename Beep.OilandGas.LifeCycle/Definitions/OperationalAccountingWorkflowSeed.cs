using Beep.OilandGas.Models.Processes;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Basic operational accounting workflows — Procure-to-Pay, Order-to-Cash, Record-to-Report,
/// Fixed Assets, Bank Reconciliation, Expense Management, Journal Entry, Vendor/Customer, Cash.
/// These are the day-to-day accounting operations every company needs.
/// </summary>
public partial class ProcessDefinitionInitializer
{
    public async Task InitializeOperationalAccountingWorkflowsAsync(string userId)
    {
        await InitProcureToPayAsync(userId);
        await InitOrderToCashAsync(userId);
        await InitRecordToReportAsync(userId);
        await InitFixedAssetLifecycleAsync(userId);
        await InitBankReconciliationAsync(userId);
        await InitExpenseManagementAsync(userId);
        await InitJournalEntryApprovalAsync(userId);
        await InitVendorManagementAsync(userId);
        await InitCustomerManagementAsync(userId);
        await InitCashManagementAsync(userId);
    }

    private async Task InitProcureToPayAsync(string userId) => await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
    {
        ProcessId = "ACCT_PROCURE_TO_PAY", ProcessName = "Procure-to-Pay (P2P)", ProcessType = "FINANCIAL", EntityType = "PURCHASE_ORDER",
        Description = "Complete P2P: requisition → PO → goods receipt → 3-way match → invoice approval → payment. SoD enforced.",
        IsActive = true, Steps = new List<ProcessStepDefinition>
        {
            new() { StepId = "REQUISITION", StepName = "Purchase Requisition", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"FieldEngineer","Manager"}, SlaHours = 48, NextStepId = "REQ_APPROVE" },
            new() { StepId = "REQ_APPROVE", StepName = "Requisition Approval", SequenceNumber = 2, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48, NextStepId = "PO_CREATE" },
            new() { StepId = "PO_CREATE", StepName = "Create Purchase Order", SequenceNumber = 3, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "PO_APPROVE" },
            new() { StepId = "PO_APPROVE", StepName = "PO Approval", SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48, NextStepId = "GOODS_RECEIPT" },
            new() { StepId = "GOODS_RECEIPT", StepName = "Goods Receipt", SequenceNumber = 5, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"FieldEngineer"}, SlaHours = 24, NextStepId = "INVOICE_RECEIVE" },
            new() { StepId = "INVOICE_RECEIVE", StepName = "Receive Invoice", SequenceNumber = 6, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "THREE_WAY_MATCH" },
            new() { StepId = "THREE_WAY_MATCH", StepName = "Three-Way Match", SequenceNumber = 7, StepType = "SYSTEM", IsRequired = true, NextStepId = "INVOICE_APPROVE", Description = "System: PO vs Receipt vs Invoice. Tolerances: qty ±5%, price ±2%." },
            new() { StepId = "INVOICE_APPROVE", StepName = "Invoice Approval", SequenceNumber = 8, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 72, NextStepId = "PAYMENT_RUN" },
            new() { StepId = "PAYMENT_RUN", StepName = "Payment Run", SequenceNumber = 9, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "PAYMENT_APPROVE" },
            new() { StepId = "PAYMENT_APPROVE", StepName = "Payment Approval", SequenceNumber = 10, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 24, NextStepId = "PAYMENT_EXECUTE" },
            new() { StepId = "PAYMENT_EXECUTE", StepName = "Execute Payment", SequenceNumber = 11, StepType = "SYSTEM", IsRequired = true },
        },
        Configuration = new() { ["category"] = "FINANCIAL", ["doaEnabled"] = true, ["threeWayMatch"] = true }
    }, userId);

    private async Task InitOrderToCashAsync(string userId) => await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
    {
        ProcessId = "ACCT_ORDER_TO_CASH", ProcessName = "Order-to-Cash (O2C)", ProcessType = "FINANCIAL", EntityType = "SALES_ORDER",
        Description = "Complete O2C: sales order → credit check → delivery → invoice → collection → cash application.",
        IsActive = true, Steps = new List<ProcessStepDefinition>
        {
            new() { StepId = "SALES_ORDER", StepName = "Create Sales Order", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 24, NextStepId = "CREDIT_CHECK" },
            new() { StepId = "CREDIT_CHECK", StepName = "Credit Check", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "ORDER_APPROVE", Description = "System checks credit limit, aging, payment history." },
            new() { StepId = "ORDER_APPROVE", StepName = "Order Approval", SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 24, NextStepId = "DELIVERY" },
            new() { StepId = "DELIVERY", StepName = "Delivery / Shipment", SequenceNumber = 4, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"ProductionEngineer"}, SlaHours = 72, NextStepId = "INVOICE_CREATE" },
            new() { StepId = "INVOICE_CREATE", StepName = "Create Invoice", SequenceNumber = 5, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "INVOICE_APPROVE" },
            new() { StepId = "INVOICE_APPROVE", StepName = "Invoice Approval", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 24, NextStepId = "COLLECTION" },
            new() { StepId = "COLLECTION", StepName = "Collections", SequenceNumber = 7, StepType = "SYSTEM", IsRequired = true, NextStepId = "CASH_APPLY", Description = "AR aging: <30d normal, 30-60 reminder, 60-90 letter, >90d escalation." },
            new() { StepId = "CASH_APPLY", StepName = "Apply Cash", SequenceNumber = 8, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, Description = "Apply payment to invoices. Post: Dr Cash, Cr AR." },
        },
        Configuration = new() { ["category"] = "FINANCIAL", ["creditCheck"] = true, ["agingBuckets"] = "30,60,90" }
    }, userId);

    private async Task InitRecordToReportAsync(string userId) => await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
    {
        ProcessId = "ACCT_RECORD_TO_REPORT", ProcessName = "Record-to-Report (R2R)", ProcessType = "FINANCIAL", EntityType = "JOURNAL_ENTRY",
        Description = "Complete R2R: JE preparation → review → approval → GL posting → trial balance → reconciliations → period close → financial statements. SOX 404 compliant.",
        IsActive = true, Steps = new List<ProcessStepDefinition>
        {
            new() { StepId = "JE_PREPARE", StepName = "Prepare Journal Entry", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 24, NextStepId = "JE_REVIEW" },
            new() { StepId = "JE_REVIEW", StepName = "JE Peer Review", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "JE_APPROVE", Description = "SoD: reviewer ≠ preparer." },
            new() { StepId = "JE_APPROVE", StepName = "JE Approval", SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48, NextStepId = "JE_POST" },
            new() { StepId = "JE_POST", StepName = "Post to GL", SequenceNumber = 4, StepType = "SYSTEM", IsRequired = true, NextStepId = "TB_REVIEW" },
            new() { StepId = "TB_REVIEW", StepName = "Trial Balance Review", SequenceNumber = 5, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 72, NextStepId = "RECONCILIATION" },
            new() { StepId = "RECONCILIATION", StepName = "Account Reconciliation", SequenceNumber = 6, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 72, NextStepId = "PERIOD_CLOSE" },
            new() { StepId = "PERIOD_CLOSE", StepName = "Period Close", SequenceNumber = 7, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 24, NextStepId = "FINANCIAL_STATEMENTS" },
            new() { StepId = "FINANCIAL_STATEMENTS", StepName = "Financial Statements", SequenceNumber = 8, StepType = "SYSTEM", IsRequired = true, NextStepId = "FS_REVIEW", Description = "Generate: Balance Sheet, Income Statement, Cash Flow." },
            new() { StepId = "FS_REVIEW", StepName = "FS Review", SequenceNumber = 9, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant","Manager"}, SlaHours = 72, NextStepId = "FS_APPROVE" },
            new() { StepId = "FS_APPROVE", StepName = "FS Approval", SequenceNumber = 10, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Executive"}, SlaHours = 48 },
        },
        Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "SOX 404", ["jeThreshold"] = 100000 }
    }, userId);

    private async Task InitFixedAssetLifecycleAsync(string userId) => await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
    {
        ProcessId = "ACCT_FIXED_ASSET_LIFECYCLE", ProcessName = "Fixed Asset Lifecycle", ProcessType = "FINANCIAL", EntityType = "EQUIPMENT",
        Description = "Fixed asset lifecycle: register → capitalize → depreciate → review → dispose. IAS 16 compliant.",
        IsActive = true, Steps = new List<ProcessStepDefinition>
        {
            new() { StepId = "ASSET_REGISTER", StepName = "Register Asset", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "CAPITALIZE" },
            new() { StepId = "CAPITALIZE", StepName = "Capitalize", SequenceNumber = 2, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48, NextStepId = "DEPRECIATE" },
            new() { StepId = "DEPRECIATE", StepName = "Monthly Depreciation", SequenceNumber = 3, StepType = "SYSTEM", IsRequired = true, NextStepId = "DEPR_REVIEW" },
            new() { StepId = "DEPR_REVIEW", StepName = "Depreciation Review", SequenceNumber = 4, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "IMPAIRMENT_CHECK" },
            new() { StepId = "IMPAIRMENT_CHECK", StepName = "Impairment Check", SequenceNumber = 5, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 240, NextStepId = "DISPOSAL_CHECK" },
            new() { StepId = "DISPOSAL_CHECK", StepName = "Disposal Review", SequenceNumber = 6, StepType = "REVIEW", IsRequired = false, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "DISPOSAL" },
            new() { StepId = "DISPOSAL", StepName = "Process Disposal", SequenceNumber = 7, StepType = "APPROVAL", IsRequired = false, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 24 },
        },
        Configuration = new() { ["category"] = "FINANCIAL", ["regulation"] = "IAS 16", ["schedule"] = "MONTHLY" }
    }, userId);

    private async Task InitBankReconciliationAsync(string userId) => await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
    {
        ProcessId = "ACCT_BANK_RECONCILIATION", ProcessName = "Bank Reconciliation", ProcessType = "FINANCIAL", EntityType = "BANK_STATEMENT",
        Description = "Monthly bank rec: import statement → auto-match → manual match → reconciling items → review → approve. SOX control.",
        IsActive = true, Steps = new List<ProcessStepDefinition>
        {
            new() { StepId = "IMPORT_STMT", StepName = "Import Statement", SequenceNumber = 1, StepType = "SYSTEM", IsRequired = true, NextStepId = "AUTO_MATCH" },
            new() { StepId = "AUTO_MATCH", StepName = "Auto-Match", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "MANUAL_MATCH", Description = "Match: amount + date ±2 days. Flag unmatched." },
            new() { StepId = "MANUAL_MATCH", StepName = "Manual Match", SequenceNumber = 3, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "REC_ITEMS" },
            new() { StepId = "REC_ITEMS", StepName = "Reconciling Items", SequenceNumber = 4, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 24, NextStepId = "REC_REVIEW" },
            new() { StepId = "REC_REVIEW", StepName = "Reconciliation Review", SequenceNumber = 5, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 24, NextStepId = "REC_APPROVE", Description = "Investigate items > 30 days old." },
            new() { StepId = "REC_APPROVE", StepName = "Reconciliation Approval", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48 },
        },
        Configuration = new() { ["category"] = "FINANCIAL", ["soxRelevant"] = true, ["schedule"] = "MONTHLY" }
    }, userId);

    private async Task InitExpenseManagementAsync(string userId) => await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
    {
        ProcessId = "ACCT_EXPENSE_MANAGEMENT", ProcessName = "Expense Management", ProcessType = "FINANCIAL", EntityType = "EXPENSE_REPORT",
        Description = "Employee expense: submit → policy check → manager approval → finance review → reimbursement.",
        IsActive = true, Steps = new List<ProcessStepDefinition>
        {
            new() { StepId = "EXPENSE_SUBMIT", StepName = "Submit Expenses", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, SlaHours = 120, NextStepId = "POLICY_CHECK" },
            new() { StepId = "POLICY_CHECK", StepName = "Policy Check", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "MANAGER_APPROVE", Description = "Check: per diem limits, receipts, duplicates." },
            new() { StepId = "MANAGER_APPROVE", StepName = "Manager Approval", SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 72, NextStepId = "FINANCE_REVIEW" },
            new() { StepId = "FINANCE_REVIEW", StepName = "Finance Review", SequenceNumber = 4, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 72, NextStepId = "REIMBURSEMENT" },
            new() { StepId = "REIMBURSEMENT", StepName = "Process Reimbursement", SequenceNumber = 5, StepType = "SYSTEM", IsRequired = true },
        },
        Configuration = new() { ["category"] = "FINANCIAL", ["policyDriven"] = true }
    }, userId);

    private async Task InitJournalEntryApprovalAsync(string userId) => await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
    {
        ProcessId = "ACCT_JOURNAL_ENTRY_APPROVAL", ProcessName = "Journal Entry Approval", ProcessType = "FINANCIAL", EntityType = "JOURNAL_ENTRY",
        Description = "Standalone JE: create → attach support → peer review → manager approval → GL post. SoD enforced. SOX control.",
        IsActive = true, Steps = new List<ProcessStepDefinition>
        {
            new() { StepId = "JE_CREATE", StepName = "Create JE", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "JE_ATTACH" },
            new() { StepId = "JE_ATTACH", StepName = "Attach Support", SequenceNumber = 2, StepType = "ACTION", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 24, NextStepId = "JE_REVIEW" },
            new() { StepId = "JE_REVIEW", StepName = "Peer Review", SequenceNumber = 3, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 72, NextStepId = "JE_APPROVE", Description = "SoD: reviewer ≠ preparer." },
            new() { StepId = "JE_APPROVE", StepName = "Manager Approval", SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48, NextStepId = "JE_POST" },
            new() { StepId = "JE_POST", StepName = "Post to GL", SequenceNumber = 5, StepType = "SYSTEM", IsRequired = true },
        },
        Configuration = new() { ["category"] = "FINANCIAL", ["soxRelevant"] = true, ["doaEnabled"] = true }
    }, userId);

    private async Task InitVendorManagementAsync(string userId) => await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
    {
        ProcessId = "ACCT_VENDOR_MANAGEMENT", ProcessName = "Vendor Setup & Management", ProcessType = "FINANCIAL", EntityType = "BUSINESS_ASSOCIATE",
        Description = "Vendor master in PPDM BA (BUSINESS_ASSOCIATE + BA_ADDRESS + BA_CONTACT_INFO + BA_PREFERENCE). BA_CATEGORY='Vendor'. Fraud prevention: TIN match, OFAC check, duplicate detection, banking verification.",
        IsActive = true, Steps = new List<ProcessStepDefinition>
        {
            new() { StepId = "VENDOR_REQUEST", StepName = "New Vendor Request", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "TAX_VERIFY" },
            new() { StepId = "TAX_VERIFY", StepName = "Tax & OFAC Check", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "DUPLICATE_CHECK" },
            new() { StepId = "DUPLICATE_CHECK", StepName = "Duplicate Check", SequenceNumber = 3, StepType = "SYSTEM", IsRequired = true, NextStepId = "BANKING_VERIFY" },
            new() { StepId = "BANKING_VERIFY", StepName = "Banking Verification", SequenceNumber = 4, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "VENDOR_APPROVE" },
            new() { StepId = "VENDOR_APPROVE", StepName = "Vendor Approval", SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48 },
        },
        Configuration = new() { ["category"] = "FINANCIAL", ["fraudPrevention"] = true }
    }, userId);

    private async Task InitCustomerManagementAsync(string userId) => await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
    {
        ProcessId = "ACCT_CUSTOMER_MANAGEMENT", ProcessName = "Customer Setup & Credit", ProcessType = "FINANCIAL", EntityType = "BUSINESS_ASSOCIATE",
        Description = "Customer master in PPDM BA (BUSINESS_ASSOCIATE + BA_ADDRESS + BA_CONTACT_INFO + BA_PREFERENCE). BA_CATEGORY='Customer'. Credit assessment, limit assignment, SOX revenue control.",
        IsActive = true, Steps = new List<ProcessStepDefinition>
        {
            new() { StepId = "CUSTOMER_REQUEST", StepName = "New Customer Request", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 48, NextStepId = "CREDIT_ASSESS" },
            new() { StepId = "CREDIT_ASSESS", StepName = "Credit Assessment", SequenceNumber = 2, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 72, NextStepId = "CREDIT_LIMIT" },
            new() { StepId = "CREDIT_LIMIT", StepName = "Set Credit Limit", SequenceNumber = 3, StepType = "DATA_ENTRY", IsRequired = true, RequiredRoles = new(){"Manager"}, SlaHours = 24, NextStepId = "CUSTOMER_APPROVE" },
            new() { StepId = "CUSTOMER_APPROVE", StepName = "Customer Approval", SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Manager"}, SlaHours = 48 },
        },
        Configuration = new() { ["category"] = "FINANCIAL", ["soxRelevant"] = true }
    }, userId);

    private async Task InitCashManagementAsync(string userId) => await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
    {
        ProcessId = "ACCT_CASH_MANAGEMENT", ProcessName = "Cash Management", ProcessType = "FINANCIAL", EntityType = "CASH_POSITION",
        Description = "Daily cash: position → forecast → treasury action → approval. Dual control for wires > $100K.",
        IsActive = true, Steps = new List<ProcessStepDefinition>
        {
            new() { StepId = "CASH_POSITION", StepName = "Daily Cash Position", SequenceNumber = 1, StepType = "SYSTEM", IsRequired = true, NextStepId = "CASH_FORECAST" },
            new() { StepId = "CASH_FORECAST", StepName = "Cash Forecast (13-week)", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "CASH_REVIEW" },
            new() { StepId = "CASH_REVIEW", StepName = "Cash Review", SequenceNumber = 3, StepType = "REVIEW", IsRequired = true, RequiredRoles = new(){"Accountant"}, SlaHours = 24, NextStepId = "TREASURY_ACTION" },
            new() { StepId = "TREASURY_ACTION", StepName = "Treasury Action", SequenceNumber = 4, StepType = "ACTION", IsRequired = false, RequiredRoles = new(){"Manager"}, SlaHours = 24, NextStepId = "CASH_APPROVE" },
            new() { StepId = "CASH_APPROVE", StepName = "Treasury Approval", SequenceNumber = 5, StepType = "APPROVAL", IsRequired = false, RequiresApproval = true, RequiredRoles = new(){"Executive"}, SlaHours = 24 },
        },
        Configuration = new() { ["category"] = "FINANCIAL", ["schedule"] = "DAILY", ["wireThreshold"] = 100000 }
    }, userId);
}
