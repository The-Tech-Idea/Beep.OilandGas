# Comprehensive Plan: Evolving Oil & Gas Production Accounting

## Objective
Make `Beep.OilandGas.Accounting` the foundational accounting library (GL/AP/AR/Inventory/Close/Reporting) and evolve `Beep.OilandGas.ProductionAccounting` into the production-domain layer (measurement, run tickets, allocation, pricing, revenue/royalty, imbalances, upstream financial methods) that *depends on* Accounting for all postings and core accounting behaviors.

## Current State (from existing `*.md` + code)
- Documentation describes a large, modular system, but the current implementation is primarily a service set under `Beep.OilandGas.ProductionAccounting/Services`.
- There is functional overlap between projects today:
  - `Beep.OilandGas.Accounting/Services`: GL/AP/AR/PO/Inventory/Close/Reporting + accounting ops services
  - `Beep.OilandGas.ProductionAccounting/Services`: Allocation/Revenue/Royalty/Measurement/Imbalance + upstream accounting methods
  - Overlapping services exist in both: `InventoryService`, `JournalEntryService`, `PeriodClosingService`.
- PPDM integration is a core design requirement (`PPDMGenericRepository`, PPDM metadata-driven entity types).

## Target Architecture (dependency direction)
**Hard rule**: `Beep.OilandGas.Accounting` must never reference `Beep.OilandGas.ProductionAccounting`. Only ProductionAccounting references Accounting.

### Projects and responsibilities
- `Beep.OilandGas.Models`
  - Shared entity classes (inherit `Entity` + implement `IPPDMEntity`) and cross-module interfaces (no DTO/model layer).
- `Beep.OilandGas.Accounting` (base)
  - GL, journal entry posting rules, trial balance, financial statements
  - AP/AR, purchasing, inventory accounting and valuation
  - Period close framework + reconciliations and controls
  - Generic reporting primitives for accounting views
- `Beep.OilandGas.ProductionAccounting`
  - Measurement/run tickets and operational volume capture
  - Allocation engine (well/lease/tract/unit/owner)
  - Pricing and valuation inputs (indices, differentials)
  - Revenue recognition inputs (sales transactions) and royalty/JIB/imbalance
  - Upstream financial accounting methods (Successful Efforts / Full Cost / Amortization), but GL postings go through Accounting

### Integration pattern (composition over duplication)
- ProductionAccounting services orchestrate workflows and call Accounting services for:
  - Journal entries (header/lines, balancing, posting status)
  - Inventory impacts (if any) and cost/revenue postings
  - Period close checklists, locking, and reconciliations

## Roadmap (phased, with acceptance criteria)

### Phase 0 - Establish Accounting as the base (structure)
**Deliverables**
- ProductionAccounting references Accounting at the project level.
- A single documented dependency graph and module ownership list.

**Acceptance criteria**
- `dotnet build` succeeds for `Beep.OilandGas.Accounting` and `Beep.OilandGas.ProductionAccounting` (or failures are isolated and documented).

### Phase 1 - De-duplicate core accounting behavior
**Goal**: There is exactly one source of truth for accounting primitives.

**Deliverables**
- Move/standardize core accounting primitives into `Beep.OilandGas.Accounting`:
  - Journal entry posting (balanced entries, line-level persistence, account balance computation)
  - Period close orchestration, lockout rules, and close status
  - Inventory accounting/valuation primitives (if ProductionAccounting needs them)
- In ProductionAccounting, keep thin adapters/facades where needed (same public surface, but delegates to Accounting).

**Acceptance criteria**
- No duplicated GL posting logic between the two projects.
- ProductionAccounting compiles while referencing Accounting implementations.

### Phase 2 - Production -> Revenue/Royalty -> GL end-to-end workflow
**Goal**: A single deterministic workflow that can be run for a period close.

**Deliverables**
- Define canonical month-end workflow (inputs, steps, outputs):
  1. Validate measurement/run tickets
  2. Run allocation (store results + audit)
  3. Apply pricing (valuation snapshot)
  4. Generate sales/revenue transactions
  5. Calculate royalties/JIB/imbalances
  6. Post to GL via Accounting (journal entry + GL entry lines)
  7. Run reconciliations (volume-to-revenue, allocation-to-royalty, subledger-to-GL)
- Add error/exception standardization so failures are traceable and retryable.

**Acceptance criteria**
- A "happy path" period can be processed end-to-end without manual GL intervention.
- Reversals are supported (allocation reversal, royalty recalculation, GL reversal).

### Phase 3 - Controls, auditability, and compliance hardening
**Deliverables**
- Approval workflow hooks (AFE approvals, non-op costs, adjustments).
- Audit reports and change tracking for critical records (ownership, contracts, prices, allocations).
- Reconciliation reports suitable for month-end close and audit evidence.

**Acceptance criteria**
- Every posted financial impact can be traced back to a production/contract/ownership source record.

### Phase 4 - Data model and PPDM alignment (cleanup + scripts)
**Deliverables**
- Confirm "PPDM-first" table strategy (use PPDM tables when possible; create new only when necessary).
- Add/standardize missing entity properties used by services (via model updates or partial extensions).
- Create/refresh schema scripts where new tables are required.

**Acceptance criteria**
- No service filters on nonexistent fields; metadata mappings are consistent across environments.

## Immediate Implementation Backlog (next 2-4 iterations)
- Align `JournalEntryService`, `PeriodClosingService`, and `InventoryService` so ProductionAccounting delegates to Accounting.
- Apply consistent query filtering: `ACTIVE_IND = 'Y'`, connection name defaulting, and date-range filtering where applicable.
- Consolidate duplicated constants into a single source of truth (prefer Accounting for accounting constants; ProductionAccounting for production-domain constants).

## Notes
- Existing docs in this folder contain module-level plans and best-practice guidance; this document is the "single roadmap" to sequence that work and enforce project ownership.
