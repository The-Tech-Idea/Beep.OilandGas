# Accounting & LifeCycle Workflow Revision — Master Plan

> **Status:** Planning | **Created:** 2026-07-03 | **Based on:** 50+ Accounting services, 15+ LifeCycle domain process services, ProductionAccounting services
> **Standards:** [standards.md](standards.md)

---

## Executive Summary

The Accounting and LifeCycle workflow codebase has grown organically across multiple projects. This revision plan addresses:

1. **Missing interfaces** — Many Accounting services lack `IService` interfaces, making DI and testing difficult
2. **Naming inconsistencies** — Mix of `Async`/non-`Async` suffixes, inconsistent CRUD prefixes
3. **Parameter conventions** — Inconsistent `connectionName` defaults, `CancellationToken` usage, null-check patterns
4. **Constructor patterns** — Some services use different DI patterns than the standard
5. **LifeCycle workflow gaps** — Domain-specific process services have varying levels of completeness
6. **Cross-project duplication** — Accounting vs ProductionAccounting boundaries are blurred

---

## Phase Overview

| Phase | Name | Scope | Est. Effort |
|-------|------|-------|-------------|
| **1** | Interface Extraction | Extract `I*Service` interfaces for all Accounting services lacking them | 1-2 weeks |
| **2** | Naming & Signature Standardization | Align method names, parameter order, Async suffixes | 1 week |
| **3** | Constructor & DI Alignment | Standardize constructor patterns, add null-guards, logger injection | 1 week |
| **4** | LifeCycle Workflow Completion | Fill gaps in domain process services, standardize step definitions | 2 weeks |
| **5** | Boundary Clarification | Delineate Accounting vs ProductionAccounting, remove duplication | 1-2 weeks |
| **6** | Validation & Error Handling | Consistent error patterns, input validation, logging | 1 week |

**Total estimated effort:** 7-9 weeks

---

## Master Task Tracker

Legend: `[ ]` = Not Started, `[~]` = In Progress, `[x]` = Complete

### Phase 1 — Interface Extraction
| ID | Task | Status |
|----|------|--------|
| A1-01 | Extract `IJournalEntryService` from `JournalEntryService` | [ ] |
| A1-02 | Extract `IGLAccountService` from `GLAccountService` | [ ] |
| A1-03 | Extract `IBudgetService` from `BudgetService` | [ ] |
| A1-04 | Extract `IAuditService` from `AuditService` | [ ] |
| A1-05 | Extract `IBankReconciliationService` | [ ] |
| A1-06 | Extract `ITaxProvisionService` from `TaxProvisionService` | [ ] |
| A1-07 | Extract `IRevenueService` from `RevenueService` (ProductionAccounting) | [ ] |
| A1-08 | Extract `IRoyaltyService` from `RoyaltyService` | [ ] |
| A1-09 | Extract `IAfeService` from `AfeService` | [ ] |
| A1-10 | Extract `IAllocationService` from `AllocationService` | [ ] |
| A1-11 | Extract `IAmortizationService` from `AmortizationService` | [ ] |
| A1-12 | Extract `ICostAllocationService` from `CostAllocationService` | [ ] |
| A1-13 | Extract `IPeriodClosingService` from `PeriodClosingService` | [ ] |
| A1-14 | Extract `IFinancialReportingService` from `FinancialReportingService` | [ ] |
| A1-15 | Extract `IInternalControlService` from `InternalControlService` | [ ] |
| A1-16 | Audit all remaining Accounting services for missing interfaces | [ ] |

### Phase 2 — Naming & Signature Standardization
| ID | Task | Status |
|----|------|--------|
| A2-01 | Add `Async` suffix to all async methods missing it | [ ] |
| A2-02 | Standardize CRUD prefixes (`Get`, `Create`, `Update`, `Delete`) | [ ] |
| A2-03 | Standardize parameter order: required params → userId → CancellationToken | [ ] |
| A2-04 | Add `CancellationToken` to all async methods missing it | [ ] |
| A2-05 | Standardize `connectionName` default to `"PPDM39"` | [ ] |
| A2-06 | Standardize `ILogger<T>?` injection in all services | [ ] |
| A2-07 | Rename boolean-returning methods to `Has`/`Is`/`Can` prefix | [ ] |
| A2-08 | Audit all LifeCycle process services for naming consistency | [ ] |

### Phase 3 — Constructor & DI Alignment
| ID | Task | Status |
|----|------|--------|
| A3-01 | Standardize Accounting service constructors to mandatory pattern | [ ] |
| A3-02 | Add null-guards (`?? throw new ArgumentNullException`) to all constructors | [ ] |
| A3-03 | Standardize LifeCycle process service constructors | [ ] |
| A3-04 | Audit DI registrations in Program.cs for missing services | [ ] |
| A3-05 | Create `AccountingServiceCollectionExtensions.AddAccountingServices()` | [ ] |
| A3-06 | Verify all services are registered in DI | [ ] |

### Phase 4 — LifeCycle Workflow Completion
| ID | Task | Status |
|----|------|--------|
| A4-01 | Audit all domain process services for method completeness | [ ] |
| A4-02 | Fill gaps in `ExplorationProcessService` | [ ] |
| A4-03 | Fill gaps in `DevelopmentProcessService` | [ ] |
| A4-04 | Fill gaps in `ProductionProcessService` | [ ] |
| A4-05 | Fill gaps in `DecommissioningProcessService` | [ ] |
| A4-06 | Fill gaps in `WellManagementProcessService` | [ ] |
| A4-07 | Fill gaps in `FacilityManagementProcessService` | [ ] |
| A4-08 | Fill gaps in `PipelineManagementProcessService` | [ ] |
| A4-09 | Standardize step definition creation pattern | [ ] |
| A4-10 | Ensure all process definitions register via ProcessDefinitionInitializer | [ ] |

### Phase 5 — Boundary Clarification
| ID | Task | Status |
|----|------|--------|
| A5-01 | Identify duplicate services between Accounting and ProductionAccounting | [ ] |
| A5-02 | Define clear ownership: Accounting = GL/Financial, ProductionAccounting = O&G operations | [ ] |
| A5-03 | Move misplaced services to correct project | [ ] |
| A5-04 | Remove duplicate implementations | [ ] |
| A5-05 | Update project references after moves | [ ] |
| A5-06 | Document ownership boundaries in CLAUDE.md | [ ] |

### Phase 6 — Validation & Error Handling
| ID | Task | Status |
|----|------|--------|
| A6-01 | Add input validation to all public Accounting methods | [ ] |
| A6-02 | Standardize error response types | [ ] |
| A6-03 | Add structured logging to all service methods | [ ] |
| A6-04 | Add XML doc comments to all public methods | [ ] |
| A6-05 | Standardize exception propagation (throw vs return error result) | [ ] |
| A6-06 | Add audit logging for financial transactions | [ ] |

**Total tasks: 52**

---

## Related Documents

- [Phase 1 — Interface Extraction](accounting-revision-phase1-interfaces.md)
- [Phase 2 — Naming & Signatures](accounting-revision-phase2-naming.md)
- [Phase 3 — Constructor & DI](accounting-revision-phase3-di.md)
- [Phase 4 — LifeCycle Workflows](accounting-revision-phase4-lifecycle.md)
- [Phase 5 — Boundaries](accounting-revision-phase5-boundaries.md)
- [Phase 6 — Validation](accounting-revision-phase6-validation.md)
- [Coding Standards](standards.md)
- [Workflow-RBAC Architecture](workflow-rbac-master-plan.md)

---

*Last updated: 2026-07-03*
