# Phase 2 — Naming & Signature Standardization

> **Status:** Not Started | **Depends on:** Phase 1 | **Est. Effort:** 1 week
> **Standards:** [standards.md](standards.md)

---

## Current Inconsistencies Found

### 2A. Connection Name Parameter (3 Patterns → 1)

| Pattern | Where Used | Fix |
|---------|-----------|-----|
| `string? connectionName = null` (nullable) | `IARService`, `IInvoiceService`, `PPDMAccountingService` | → `string connectionName = "PPDM39"` |
| `string cn = "PPDM39"` (abbreviated) | `IJournalEntryService`, `ICostAllocationService`, `ICostCenterService`, ProductionAccounting services | → `string connectionName = "PPDM39"` |
| `string connectionName = "PPDM39"` (standard) | `ProductionAccountingService`, LifeCycle constructors | ✅ Already correct |

### 2B. userId Parameter (2 Patterns → 1)

| Pattern | Where Used | Fix |
|---------|-----------|-----|
| `string userId` (required, no default) | Most services | ✅ Already correct |
| `string userId = "SYSTEM"` (optional with default) | Some LifeCycle seed services | Remove default — caller must always provide |

### 2C. Missing CancellationToken

| Where | Count | Fix |
|-------|-------|-----|
| All domain process services EXCEPT `ExplorationProcessService` | 11 services | Add `CancellationToken cancellationToken = default` as last param |
| Accounting services without CancellationToken | ~60 of 78 | Add to all public async methods |

### 2D. Hardcoded Strings vs Constants

| Service | Pattern | Fix |
|---------|---------|-----|
| `DecommissioningProcessService` | `"WELL_PLUGGING"`, `"SITE_RESTORATION"` | Extract to reference code constants or ProcessDefinition IDs |
| `WellManagementProcessService` | `"PRODUCTION_APPROVAL"` | Extract constants |
| `ProductionProcessService` | Hardcoded step names | Extract constants |
| `ExplorationProcessService` | `ExplorationReferenceCodes.StepLeadEvaluation` | ✅ Already correct — use as template |

---

## Task Details

### Task Group A — Connection Name Unification

| ID | Task | Files Affected |
|----|------|---------------|
| A2-01 | Rename `cn` → `connectionName` in `IJournalEntryService` + implementation | `Models/Core/Interfaces/IJournalEntryService.cs`, `Accounting/Services/JournalEntryService.cs` |
| A2-02 | Rename `cn` → `connectionName` in `ICostAllocationService` + impl | Interface + `CostAllocationService.cs` |
| A2-03 | Rename `cn` → `connectionName` in `ICostCenterService` + impl | Interface + `CostCenterService.cs` |
| A2-04 | Rename `cn` → `connectionName` in `IInventoryLcmService` + impl | Interface + `InventoryLcmService.cs` |
| A2-05 | Rename `cn` → `connectionName` in ALL ProductionAccounting services | ~35 services |
| A2-06 | Change `string? connectionName = null` → `string connectionName = "PPDM39"` in `IARService` + impl | Interface + `ARService.cs` |
| A2-07 | Same null→non-null change in `IInvoiceService` + impl | Interface + `InvoiceService.cs` |
| A2-08 | Same null→non-null change in `PPDMAccountingService` | `LifeCycle/Services/Accounting/PPDMAccountingService.cs` |

### Task Group B — CancellationToken Addition

| ID | Task | Files Affected |
|----|------|---------------|
| A2-09 | Add `CancellationToken` to all 11 domain process services | WellManagement, Production, Decommissioning, etc. |
| A2-10 | Add `CancellationToken` to all Accounting services | ~60 files in `Beep.OilandGas.Accounting/Services/` |
| A2-11 | Add `CancellationToken` to all LifeCycle process engine methods | `PPDMProcessService`, `ProcessServiceBase`, `ApprovalWorkflowEngine` |

### Task Group C — userId Default Removal

| ID | Task |
|----|------|
| A2-12 | Remove ` = "SYSTEM"` default from all userId params in LifeCycle seeders |
| A2-13 | Remove ` = "system"` default from all userId params in Accounting facade |

### Task Group D — Step Name Constants

| ID | Task |
|----|------|
| A2-14 | Extract step name strings to constants in `DecommissioningProcessService` |
| A2-15 | Extract step name strings to constants in `WellManagementProcessService` |
| A2-16 | Extract step name strings to constants in `ProductionProcessService` |
| A2-17 | Create `ProcessStepReferenceCodes` class (following `ExplorationReferenceCodes` pattern) |

---

## Phase 2 Completion Checklist

- [ ] `connectionName` is the ONLY parameter name used (no `cn`)
- [ ] `connectionName` is always `string` (not `string?`) with default `"PPDM39"`
- [ ] All async methods have `CancellationToken` as last parameter
- [ ] No `userId` parameters have default values
- [ ] All domain process services use constants for step names
- [ ] 0 compilation errors
- [ ] 0 breaking changes (parameter renames with defaults are source-compatible)

---

*Previous: [Phase 1 — Interface Extraction](accounting-revision-phase1-interfaces.md)*
*Next: [Phase 3 — Constructor & DI](accounting-revision-phase3-di.md)*
