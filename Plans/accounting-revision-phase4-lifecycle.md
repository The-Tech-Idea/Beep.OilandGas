# Phase 4 — LifeCycle Workflow Completion

> **Status:** Not Started | **Depends on:** Phase 1-3 | **Est. Effort:** 2 weeks
> **Standards:** [standards.md](standards.md)

---

## Current Gaps Found

1. **`PPDMAccountingService`** (LifeCycle/Services/Accounting/) — stub implementations:
   - `ReconcileVolumesAsync` returns zero-value defaults
   - `AllocateCostsAsync` returns empty cost allocation
   - `CalculateRoyaltiesAsync` returns zero-rate defaults
   - `ApproveSalesTransactionAsync` does nothing observable
   - Should delegate to ProductionAccounting/Accounting services

2. **`WorkOrderAccountingService`** — no interface, no DI registration

3. **Domain process services** vary in quality:
   - `ExplorationProcessService` — best example: uses CancellationToken, typed reference codes, prerequisite validation
   - `ProductionProcessService` — good structured data flow but hardcoded strings
   - `DecommissioningProcessService` — hardcoded strings, no CancellationToken
   - `WellManagementProcessService` — hardcoded strings, no CancellationToken

4. **`PeriodClosingService`** — commented-out code, placeholder comments

---

## Task Details

| ID | Task | Description |
|----|------|-------------|
| A4-01 | Delegate PPDMAccountingService to real services | Replace stubs with calls to `IRevenueService`, `ICostAllocationService`, etc. |
| A4-02 | Add interface to WorkOrderAccountingService | Extract `IWorkOrderAccountingService` |
| A4-03 | Fix PeriodClosingService placeholders | Uncomment and implement `PeriodEndDate`, `PeriodName`, `CloseDate` |
| A4-04 | Standardize DecommissioningProcessService | Add CancellationToken, extract step constants |
| A4-05 | Standardize WellManagementProcessService | Add CancellationToken, extract step constants |
| A4-06 | Standardize ProductionProcessService | Extract step constants |
| A4-07 | Add CancellationToken to all process services | Follow `ExplorationProcessService` pattern |
| A4-08 | Extract ProcessStepReferenceCodes | Create shared constants for step names |
| A4-09 | Ensure all definitions in ProcessDefinitionInitializer | Verify 80+ definitions registered |
| A4-10 | Add cross-role workflow seed integration | Wire Phase 3 cross-role definitions into initializer |
