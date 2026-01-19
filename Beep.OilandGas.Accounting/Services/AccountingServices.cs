namespace Beep.OilandGas.Accounting.Services
{
    public interface IAccountingServices
    {
        GLAccountService GlAccounts { get; }
        JournalEntryService JournalEntries { get; }
        TrialBalanceService TrialBalance { get; }
        FinancialStatementService FinancialStatements { get; }
        GeneralLedgerReportService GeneralLedgerReports { get; }
        BankReconciliationService BankReconciliation { get; }
        BudgetService Budgets { get; }
        DashboardService Dashboard { get; }
        TaxCalculationService Taxes { get; }
        DepreciationService Depreciation { get; }
        CostAllocationService CostAllocations { get; }
        CostCenterService CostCenters { get; }
        InventoryService Inventory { get; }
        PurchaseOrderService PurchaseOrders { get; }
        APInvoiceService AccountsPayableInvoices { get; }
        APPaymentService AccountsPayablePayments { get; }
        ARInvoiceService AccountsReceivableInvoices { get; }
        ARService AccountsReceivable { get; }
        InvoiceService Invoices { get; }
        PeriodClosingService PeriodClosing { get; }
    }

    /// <summary>
    /// Aggregates all Accounting services to simplify integration.
    /// </summary>
    public class AccountingServices : IAccountingServices
    {
        public AccountingServices(
            GLAccountService glAccounts,
            JournalEntryService journalEntries,
            TrialBalanceService trialBalance,
            FinancialStatementService financialStatements,
            GeneralLedgerReportService generalLedgerReports,
            BankReconciliationService bankReconciliation,
            BudgetService budgets,
            DashboardService dashboard,
            TaxCalculationService taxes,
            DepreciationService depreciation,
            CostAllocationService costAllocations,
            CostCenterService costCenters,
            InventoryService inventory,
            PurchaseOrderService purchaseOrders,
            APInvoiceService accountsPayableInvoices,
            APPaymentService accountsPayablePayments,
            ARInvoiceService accountsReceivableInvoices,
            ARService accountsReceivable,
            InvoiceService invoices,
            PeriodClosingService periodClosing)
        {
            GlAccounts = glAccounts ?? throw new ArgumentNullException(nameof(glAccounts));
            JournalEntries = journalEntries ?? throw new ArgumentNullException(nameof(journalEntries));
            TrialBalance = trialBalance ?? throw new ArgumentNullException(nameof(trialBalance));
            FinancialStatements = financialStatements ?? throw new ArgumentNullException(nameof(financialStatements));
            GeneralLedgerReports = generalLedgerReports ?? throw new ArgumentNullException(nameof(generalLedgerReports));
            BankReconciliation = bankReconciliation ?? throw new ArgumentNullException(nameof(bankReconciliation));
            Budgets = budgets ?? throw new ArgumentNullException(nameof(budgets));
            Dashboard = dashboard ?? throw new ArgumentNullException(nameof(dashboard));
            Taxes = taxes ?? throw new ArgumentNullException(nameof(taxes));
            Depreciation = depreciation ?? throw new ArgumentNullException(nameof(depreciation));
            CostAllocations = costAllocations ?? throw new ArgumentNullException(nameof(costAllocations));
            CostCenters = costCenters ?? throw new ArgumentNullException(nameof(costCenters));
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            PurchaseOrders = purchaseOrders ?? throw new ArgumentNullException(nameof(purchaseOrders));
            AccountsPayableInvoices = accountsPayableInvoices ?? throw new ArgumentNullException(nameof(accountsPayableInvoices));
            AccountsPayablePayments = accountsPayablePayments ?? throw new ArgumentNullException(nameof(accountsPayablePayments));
            AccountsReceivableInvoices = accountsReceivableInvoices ?? throw new ArgumentNullException(nameof(accountsReceivableInvoices));
            AccountsReceivable = accountsReceivable ?? throw new ArgumentNullException(nameof(accountsReceivable));
            Invoices = invoices ?? throw new ArgumentNullException(nameof(invoices));
            PeriodClosing = periodClosing ?? throw new ArgumentNullException(nameof(periodClosing));
        }

        public GLAccountService GlAccounts { get; }
        public JournalEntryService JournalEntries { get; }
        public TrialBalanceService TrialBalance { get; }
        public FinancialStatementService FinancialStatements { get; }
        public GeneralLedgerReportService GeneralLedgerReports { get; }
        public BankReconciliationService BankReconciliation { get; }
        public BudgetService Budgets { get; }
        public DashboardService Dashboard { get; }
        public TaxCalculationService Taxes { get; }
        public DepreciationService Depreciation { get; }
        public CostAllocationService CostAllocations { get; }
        public CostCenterService CostCenters { get; }
        public InventoryService Inventory { get; }
        public PurchaseOrderService PurchaseOrders { get; }
        public APInvoiceService AccountsPayableInvoices { get; }
        public APPaymentService AccountsPayablePayments { get; }
        public ARInvoiceService AccountsReceivableInvoices { get; }
        public ARService AccountsReceivable { get; }
        public InvoiceService Invoices { get; }
        public PeriodClosingService PeriodClosing { get; }
    }
}
