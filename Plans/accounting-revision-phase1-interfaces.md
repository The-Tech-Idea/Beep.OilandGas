# Phase 1 — Interface Extraction

> **Status:** Not Started | **Depends on:** None | **Est. Effort:** 1–2 weeks
> **Standards:** [standards.md](standards.md) | **Master:** [accounting-revision-master-plan.md](accounting-revision-master-plan.md)

---

## Current State (from code survey)

| Project | Services | With Interface | Without Interface |
|---------|----------|---------------|-------------------|
| **Beep.OilandGas.Accounting** | 78 | 7 (9%) | 71 (91%) |
| **Beep.OilandGas.ProductionAccounting** | 40 | 35 (88%) | 5 (12%) |
| **Beep.OilandGas.LifeCycle** (process) | 12 | 0 (0%) | 12 (100%) |
| **Beep.OilandGas.LifeCycle** (other) | 3 | 1 (ApprovalWorkflowEngine=0) | 2 |

**Core problem:** Accounting services are consumed by concrete type via `AccountingServices` facade (72 constructor params). No DI, no mocking, no testing.

---

## Task Details

### Batch 1 — Core Accounting Services (GL + Financial)

These are the most heavily used services. Interfaces go in `Beep.OilandGas.Models\Core\Interfaces\`.

| ID | Service File | Interface to Create | Key Methods to Expose |
|----|-------------|-------------------|----------------------|
| A1-01 | `GLAccountService.cs` | `IGLAccountService` | Create, Get, Update, Deactivate, GetByCategory, GetChartOfAccounts |
| A1-02 | `BudgetService.cs` | `IBudgetService` | CreateBudget, GetBudget, UpdateBudget, GetVarianceReport |
| A1-03 | `TrialBalanceService.cs` | `ITrialBalanceService` | GenerateTrialBalance, GetAccountBalances |
| A1-04 | `CashFlowService.cs` | `ICashFlowService` | GenerateStatement, GetCashFlowByPeriod |
| A1-05 | `BalanceSheetService.cs` | `IBalanceSheetService` | GenerateBalanceSheet, GetAssetsByCategory, GetLiabilitiesByCategory |
| A1-06 | `IncomeStatementService.cs` | `IIncomeStatementService` | GenerateIncomeStatement, GetRevenueByCategory, GetExpensesByCategory |
| A1-07 | `FinancialDashboardService.cs` | `IFinancialDashboardService` | GetDashboard, GetKPIs |
| A1-08 | `ConsolidationService.cs` | `IConsolidationService` | ConsolidateEntities, GetConsolidatedBalance |

### Batch 2 — AP/AR Services

| ID | Service File | Interface to Create | Key Methods to Expose |
|----|-------------|-------------------|----------------------|
| A1-09 | `APInvoiceService.cs` | `IAPInvoiceService` | Create, Get, Update, Approve, GetAging, RecordPayment |
| A1-10 | `APPaymentService.cs` | `IAPPaymentService` | Create, Get, Apply, GetPaymentsByInvoice |
| A1-11 | `APApprovalService.cs` | `IAPApprovalService` | SubmitForApproval, Approve, Reject, GetApprovalStatus |
| A1-12 | `AccountMappingService.cs` | (already has `IAccountMappingService` inline) | — no action needed — |

### Batch 3 — Period Closing & Audit

| ID | Service File | Interface to Create | Key Methods to Expose |
|----|-------------|-------------------|----------------------|
| A1-13 | `PeriodClosingService.cs` | `IPeriodClosingService` | ValidateReadiness, Close, Reopen, GetStatus, LockPeriod |
| A1-14 | `PeriodClosingValidationService.cs` | `IPeriodClosingValidationService` | ValidateAccounts, ValidateBalances, GetChecklist |
| A1-15 | `AuditService.cs` | `IAuditService` | CreateAuditTrail, GetAuditEntries, GetChangesByUser |
| A1-16 | `InternalControlService.cs` | `IInternalControlService` (if different from ProductionAccounting's) | ValidateControls, GetControlReport |

### Batch 4 — Revenue, Tax & Specialized

| ID | Service File | Interface to Create | Key Methods to Expose |
|----|-------------|-------------------|----------------------|
| A1-17 | `TaxProvisionService.cs` | `ITaxProvisionService` | Calculate, GetDeferredTax, GetEffectiveRate |
| A1-18 | `TaxCalculationService.cs` | `ITaxCalculationService` | CalculateTax, GetTaxLiability |
| A1-19 | `DepreciationService.cs` | `IDepreciationService` | CalculateDepreciation, GetSchedule, GetNetBookValue |
| A1-20 | `AmortizationService.cs` | `IAmortizationService` | CalculateAmortization, GetSchedule |
| A1-21 | `IntercompanyService.cs` | `IIntercompanyService` | CreateEntry, GetBalances, Reconcile |
| A1-22 | `FixedAssetLifecycleService.cs` | `IFixedAssetLifecycleService` | Register, Depreciate, Dispose, Transfer |
| A1-23 | `AccountingPolicyService.cs` | `IAccountingPolicyService` | GetPolicy, ApplyPolicy, ValidateCompliance |
| A1-24 | `CurrencyTranslationService.cs` | `ICurrencyTranslationService` | Translate, GetRate, ApplyRate |

### Batch 5 — LifeCycle Process Services

| ID | Service File | Interface to Create |
|----|-------------|-------------------|
| A1-25 | `ApprovalWorkflowEngine.cs` | `IApprovalWorkflowEngine` |
| A1-26 | `WellManagementProcessService.cs` | `IWellManagementProcessService` |
| A1-27 | `ProductionProcessService.cs` | `IProductionProcessService` |
| A1-28 | `ExplorationProcessService.cs` | `IExplorationProcessService` |
| A1-29 | `DecommissioningProcessService.cs` | `IDecommissioningProcessService` |
| A1-30 | `DevelopmentProcessService.cs` | `IDevelopmentProcessService` |
| A1-31 | `FacilityManagementProcessService.cs` | `IFacilityManagementProcessService` |
| A1-32 | `PipelineManagementProcessService.cs` | `IPipelineManagementProcessService` |
| A1-33 | `WorkOrderProcessService.cs` | `IWorkOrderProcessService` |
| A1-34 | `OperationsProcessService.cs` | `IOperationsProcessService` |

### Batch 6 — Fix IPPeriodClosingWorkflow

| ID | Task |
|----|------|
| A1-35 | Have `PeriodClosingService` implement `IPeriodClosingWorkflow` (existing interface in Models) |
| A1-36 | Align method signatures in `PeriodClosingService` to match `IPeriodClosingWorkflow` |

---

## Interface Pattern to Follow

Every new interface must follow this exact pattern (from [standards.md](standards.md)):

```csharp
namespace Beep.OilandGas.Models.Core.Interfaces;

/// <summary>
/// Service for managing general ledger accounts.
/// </summary>
public interface IGLAccountService
{
    /// <summary>
    /// Creates a new GL account.
    /// </summary>
    Task<GL_ACCOUNT> CreateAsync(GL_ACCOUNT account, string userId, string connectionName = "PPDM39");

    /// <summary>
    /// Gets a GL account by its ID.
    /// </summary>
    Task<GL_ACCOUNT?> GetAsync(string accountId, string connectionName = "PPDM39");

    /// <summary>
    /// Gets all GL accounts, optionally filtered by category.
    /// </summary>
    Task<List<GL_ACCOUNT>> GetAllAsync(string? category = null, string connectionName = "PPDM39");

    /// <summary>
    /// Updates an existing GL account.
    /// </summary>
    Task<GL_ACCOUNT> UpdateAsync(GL_ACCOUNT account, string userId, string connectionName = "PPDM39");

    /// <summary>
    /// Deactivates a GL account (soft delete).
    /// </summary>
    Task<bool> DeactivateAsync(string accountId, string userId, string connectionName = "PPDM39");
}
```

**Mandatory rules for Phase 1 interfaces:**
1. `string connectionName = "PPDM39"` — NOT nullable, NOT named `cn`
2. All methods end with `Async`
3. XML doc comments on every method
4. `string userId` after entity params, before `connectionName`
5. Interfaces go in `Beep.OilandGas.Models\Core\Interfaces\`

---

## DI Registration Pattern

After extracting interfaces, update `Program.cs` or create `AccountingServiceCollectionExtensions.AddAccountingServices()`:

```csharp
services.AddScoped<IGLAccountService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var cch = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<GLAccountService>();
    return new GLAccountService(editor, cch, defaults, metadata, connectionName, logger);
});
```

---

## Phase 1 Completion Checklist

- [ ] 24 Accounting interfaces extracted (A1-01 through A1-24)
- [ ] 8 LifeCycle process interfaces extracted (A1-25 through A1-32)
- [ ] 2 WorkOrder/Operations process interfaces extracted (A1-33, A1-34)
- [ ] `PeriodClosingService` implements `IPeriodClosingWorkflow` (A1-35, A1-36)
- [ ] All interfaces follow standard parameter pattern (`connectionName`, not `cn`)
- [ ] All interfaces have XML doc comments
- [ ] `AccountingServiceCollectionExtensions` created with DI registrations
- [ ] `AccountingServices` facade updated to use interfaces instead of concrete types
- [ ] 0 compilation errors
- [ ] 0 breaking changes (existing code resolves via DI)

---

*Next: [Phase 2 — Naming & Signature Standardization](accounting-revision-phase2-naming.md)*
