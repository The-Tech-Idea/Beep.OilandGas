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
        InventoryLcmService InventoryLcm { get; }
        PurchaseOrderService PurchaseOrders { get; }
        APInvoiceService AccountsPayableInvoices { get; }
        APPaymentService AccountsPayablePayments { get; }
        ARService AccountsReceivable { get; }
        InvoiceService Invoices { get; }
        ReconciliationService Reconciliation { get; }
        CashFlowService CashFlow { get; }
        AccountingPolicyService AccountingPolicies { get; }
        ErrorCorrectionService ErrorCorrections { get; }
        FixedAssetLifecycleService FixedAssets { get; }
        ProvisionService Provisions { get; }
        PerformanceObligationService PerformanceObligations { get; }
        LeaseAccountingService Leases { get; }
        PresentationService Presentation { get; }
        EventsAfterReportingService EventsAfterReporting { get; }
        TaxProvisionService TaxProvisions { get; }
        CurrencyTranslationService CurrencyTranslation { get; }
        ImpairmentService Impairments { get; }
        IntangibleAssetService Intangibles { get; }
        RelatedPartyDisclosureService RelatedParties { get; }
        EmployeeBenefitsService EmployeeBenefits { get; }
        GovernmentGrantService GovernmentGrants { get; }
        BorrowingCostService BorrowingCosts { get; }
        RetirementBenefitPlanService RetirementBenefitPlans { get; }
        SeparateFinancialStatementService SeparateFinancialStatements { get; }
        EquityMethodInvestmentService EquityInvestments { get; }
        HyperinflationRestatementService Hyperinflation { get; }
        FinancialInstrumentService FinancialInstruments { get; }
        EarningsPerShareService EarningsPerShare { get; }
        InterimReportingService InterimReporting { get; }
        InvestmentPropertyService InvestmentProperties { get; }
        AgricultureService Agriculture { get; }
        InsuranceContractsService InsuranceContracts { get; }
        ExpectedCreditLossService ExpectedCreditLoss { get; }
        FairValueMeasurementService FairValueMeasurement { get; }
        FirstTimeAdoptionService FirstTimeAdoption { get; }
        ShareBasedPaymentService ShareBasedPayments { get; }
        BusinessCombinationService BusinessCombinations { get; }
        AssetsHeldForSaleService AssetsHeldForSale { get; }
        ExplorationEvaluationService ExplorationEvaluation { get; }
        OperatingSegmentService OperatingSegments { get; }
        ConsolidationService Consolidation { get; }
        JointArrangementsService JointArrangements { get; }
        InvestmentDisclosureService InvestmentDisclosures { get; }
        RegulatoryDeferralService RegulatoryDeferrals { get; }
        FinancialInstrumentDisclosureService FinancialInstrumentDisclosures { get; }
        PresentationEnhancementService PresentationEnhancements { get; }
        SubsidiaryDisclosureService SubsidiaryDisclosures { get; }
        CeclService Cecl { get; }
        GaapRevenueRecognitionService GaapRevenueRecognition { get; }
        GaapLeaseAccountingService GaapLeases { get; }
        AccountingBasisPostingService BasisPosting { get; }
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
            InventoryLcmService inventoryLcm,
            PurchaseOrderService purchaseOrders,
            APInvoiceService accountsPayableInvoices,
            APPaymentService accountsPayablePayments,
            ARService accountsReceivable,
            InvoiceService invoices,
            ReconciliationService reconciliation,
            CashFlowService cashFlow,
            AccountingPolicyService accountingPolicies,
            ErrorCorrectionService errorCorrections,
            FixedAssetLifecycleService fixedAssets,
            ProvisionService provisions,
            PerformanceObligationService performanceObligations,
            LeaseAccountingService leases,
            PresentationService presentation,
            EventsAfterReportingService eventsAfterReporting,
            TaxProvisionService taxProvisions,
            CurrencyTranslationService currencyTranslation,
            ImpairmentService impairments,
            IntangibleAssetService intangibles,
            RelatedPartyDisclosureService relatedParties,
            EmployeeBenefitsService employeeBenefits,
            GovernmentGrantService governmentGrants,
            BorrowingCostService borrowingCosts,
            RetirementBenefitPlanService retirementBenefitPlans,
            SeparateFinancialStatementService separateFinancialStatements,
            EquityMethodInvestmentService equityInvestments,
            HyperinflationRestatementService hyperinflation,
            FinancialInstrumentService financialInstruments,
            EarningsPerShareService earningsPerShare,
            InterimReportingService interimReporting,
            InvestmentPropertyService investmentProperties,
            AgricultureService agriculture,
            InsuranceContractsService insuranceContracts,
            ExpectedCreditLossService expectedCreditLoss,
            FairValueMeasurementService fairValueMeasurement,
            FirstTimeAdoptionService firstTimeAdoption,
            ShareBasedPaymentService shareBasedPayments,
            BusinessCombinationService businessCombinations,
            AssetsHeldForSaleService assetsHeldForSale,
            ExplorationEvaluationService explorationEvaluation,
            OperatingSegmentService operatingSegments,
            ConsolidationService consolidation,
            JointArrangementsService jointArrangements,
            InvestmentDisclosureService investmentDisclosures,
            RegulatoryDeferralService regulatoryDeferrals,
            FinancialInstrumentDisclosureService financialInstrumentDisclosures,
            PresentationEnhancementService presentationEnhancements,
            SubsidiaryDisclosureService subsidiaryDisclosures,
            CeclService cecl,
            GaapRevenueRecognitionService gaapRevenueRecognition,
            GaapLeaseAccountingService gaapLeases,
            AccountingBasisPostingService basisPosting,
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
            InventoryLcm = inventoryLcm ?? throw new ArgumentNullException(nameof(inventoryLcm));
            PurchaseOrders = purchaseOrders ?? throw new ArgumentNullException(nameof(purchaseOrders));
            AccountsPayableInvoices = accountsPayableInvoices ?? throw new ArgumentNullException(nameof(accountsPayableInvoices));
            AccountsPayablePayments = accountsPayablePayments ?? throw new ArgumentNullException(nameof(accountsPayablePayments));
            AccountsReceivable = accountsReceivable ?? throw new ArgumentNullException(nameof(accountsReceivable));
            Invoices = invoices ?? throw new ArgumentNullException(nameof(invoices));
            Reconciliation = reconciliation ?? throw new ArgumentNullException(nameof(reconciliation));
            CashFlow = cashFlow ?? throw new ArgumentNullException(nameof(cashFlow));
            AccountingPolicies = accountingPolicies ?? throw new ArgumentNullException(nameof(accountingPolicies));
            ErrorCorrections = errorCorrections ?? throw new ArgumentNullException(nameof(errorCorrections));
            FixedAssets = fixedAssets ?? throw new ArgumentNullException(nameof(fixedAssets));
            Provisions = provisions ?? throw new ArgumentNullException(nameof(provisions));
            PerformanceObligations = performanceObligations ?? throw new ArgumentNullException(nameof(performanceObligations));
            Leases = leases ?? throw new ArgumentNullException(nameof(leases));
            Presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
            EventsAfterReporting = eventsAfterReporting ?? throw new ArgumentNullException(nameof(eventsAfterReporting));
            TaxProvisions = taxProvisions ?? throw new ArgumentNullException(nameof(taxProvisions));
            CurrencyTranslation = currencyTranslation ?? throw new ArgumentNullException(nameof(currencyTranslation));
            Impairments = impairments ?? throw new ArgumentNullException(nameof(impairments));
            Intangibles = intangibles ?? throw new ArgumentNullException(nameof(intangibles));
            RelatedParties = relatedParties ?? throw new ArgumentNullException(nameof(relatedParties));
            EmployeeBenefits = employeeBenefits ?? throw new ArgumentNullException(nameof(employeeBenefits));
            GovernmentGrants = governmentGrants ?? throw new ArgumentNullException(nameof(governmentGrants));
            BorrowingCosts = borrowingCosts ?? throw new ArgumentNullException(nameof(borrowingCosts));
            RetirementBenefitPlans = retirementBenefitPlans ?? throw new ArgumentNullException(nameof(retirementBenefitPlans));
            SeparateFinancialStatements = separateFinancialStatements ?? throw new ArgumentNullException(nameof(separateFinancialStatements));
            EquityInvestments = equityInvestments ?? throw new ArgumentNullException(nameof(equityInvestments));
            Hyperinflation = hyperinflation ?? throw new ArgumentNullException(nameof(hyperinflation));
            FinancialInstruments = financialInstruments ?? throw new ArgumentNullException(nameof(financialInstruments));
            EarningsPerShare = earningsPerShare ?? throw new ArgumentNullException(nameof(earningsPerShare));
            InterimReporting = interimReporting ?? throw new ArgumentNullException(nameof(interimReporting));
            InvestmentProperties = investmentProperties ?? throw new ArgumentNullException(nameof(investmentProperties));
            Agriculture = agriculture ?? throw new ArgumentNullException(nameof(agriculture));
            InsuranceContracts = insuranceContracts ?? throw new ArgumentNullException(nameof(insuranceContracts));
            ExpectedCreditLoss = expectedCreditLoss ?? throw new ArgumentNullException(nameof(expectedCreditLoss));
            FairValueMeasurement = fairValueMeasurement ?? throw new ArgumentNullException(nameof(fairValueMeasurement));
            FirstTimeAdoption = firstTimeAdoption ?? throw new ArgumentNullException(nameof(firstTimeAdoption));
            ShareBasedPayments = shareBasedPayments ?? throw new ArgumentNullException(nameof(shareBasedPayments));
            BusinessCombinations = businessCombinations ?? throw new ArgumentNullException(nameof(businessCombinations));
            AssetsHeldForSale = assetsHeldForSale ?? throw new ArgumentNullException(nameof(assetsHeldForSale));
            ExplorationEvaluation = explorationEvaluation ?? throw new ArgumentNullException(nameof(explorationEvaluation));
            OperatingSegments = operatingSegments ?? throw new ArgumentNullException(nameof(operatingSegments));
            Consolidation = consolidation ?? throw new ArgumentNullException(nameof(consolidation));
            JointArrangements = jointArrangements ?? throw new ArgumentNullException(nameof(jointArrangements));
            InvestmentDisclosures = investmentDisclosures ?? throw new ArgumentNullException(nameof(investmentDisclosures));
            RegulatoryDeferrals = regulatoryDeferrals ?? throw new ArgumentNullException(nameof(regulatoryDeferrals));
            FinancialInstrumentDisclosures = financialInstrumentDisclosures ?? throw new ArgumentNullException(nameof(financialInstrumentDisclosures));
            PresentationEnhancements = presentationEnhancements ?? throw new ArgumentNullException(nameof(presentationEnhancements));
            SubsidiaryDisclosures = subsidiaryDisclosures ?? throw new ArgumentNullException(nameof(subsidiaryDisclosures));
            Cecl = cecl ?? throw new ArgumentNullException(nameof(cecl));
            GaapRevenueRecognition = gaapRevenueRecognition ?? throw new ArgumentNullException(nameof(gaapRevenueRecognition));
            GaapLeases = gaapLeases ?? throw new ArgumentNullException(nameof(gaapLeases));
            BasisPosting = basisPosting ?? throw new ArgumentNullException(nameof(basisPosting));
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
        public InventoryLcmService InventoryLcm { get; }
        public PurchaseOrderService PurchaseOrders { get; }
        public APInvoiceService AccountsPayableInvoices { get; }
        public APPaymentService AccountsPayablePayments { get; }
        public ARService AccountsReceivable { get; }
        public InvoiceService Invoices { get; }
        public ReconciliationService Reconciliation { get; }
        public CashFlowService CashFlow { get; }
        public AccountingPolicyService AccountingPolicies { get; }
        public ErrorCorrectionService ErrorCorrections { get; }
        public FixedAssetLifecycleService FixedAssets { get; }
        public ProvisionService Provisions { get; }
        public PerformanceObligationService PerformanceObligations { get; }
        public LeaseAccountingService Leases { get; }
        public PresentationService Presentation { get; }
        public EventsAfterReportingService EventsAfterReporting { get; }
        public TaxProvisionService TaxProvisions { get; }
        public CurrencyTranslationService CurrencyTranslation { get; }
        public ImpairmentService Impairments { get; }
        public IntangibleAssetService Intangibles { get; }
        public RelatedPartyDisclosureService RelatedParties { get; }
        public EmployeeBenefitsService EmployeeBenefits { get; }
        public GovernmentGrantService GovernmentGrants { get; }
        public BorrowingCostService BorrowingCosts { get; }
        public RetirementBenefitPlanService RetirementBenefitPlans { get; }
        public SeparateFinancialStatementService SeparateFinancialStatements { get; }
        public EquityMethodInvestmentService EquityInvestments { get; }
        public HyperinflationRestatementService Hyperinflation { get; }
        public FinancialInstrumentService FinancialInstruments { get; }
        public EarningsPerShareService EarningsPerShare { get; }
        public InterimReportingService InterimReporting { get; }
        public InvestmentPropertyService InvestmentProperties { get; }
        public AgricultureService Agriculture { get; }
        public InsuranceContractsService InsuranceContracts { get; }
        public ExpectedCreditLossService ExpectedCreditLoss { get; }
        public FairValueMeasurementService FairValueMeasurement { get; }
        public FirstTimeAdoptionService FirstTimeAdoption { get; }
        public ShareBasedPaymentService ShareBasedPayments { get; }
        public BusinessCombinationService BusinessCombinations { get; }
        public AssetsHeldForSaleService AssetsHeldForSale { get; }
        public ExplorationEvaluationService ExplorationEvaluation { get; }
        public OperatingSegmentService OperatingSegments { get; }
        public ConsolidationService Consolidation { get; }
        public JointArrangementsService JointArrangements { get; }
        public InvestmentDisclosureService InvestmentDisclosures { get; }
        public RegulatoryDeferralService RegulatoryDeferrals { get; }
        public FinancialInstrumentDisclosureService FinancialInstrumentDisclosures { get; }
        public PresentationEnhancementService PresentationEnhancements { get; }
        public SubsidiaryDisclosureService SubsidiaryDisclosures { get; }
        public CeclService Cecl { get; }
        public GaapRevenueRecognitionService GaapRevenueRecognition { get; }
        public GaapLeaseAccountingService GaapLeases { get; }
        public AccountingBasisPostingService BasisPosting { get; }
        public PeriodClosingService PeriodClosing { get; }
    }
}
