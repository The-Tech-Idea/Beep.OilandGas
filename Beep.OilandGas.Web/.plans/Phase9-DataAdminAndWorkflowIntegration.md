# Phase 9 — Data, Admin, and Workflow Integration

> Status: Done  
> Depends On: Phase 8  
> Goal: Rationalize PPDM39 data surfaces, admin tools, compliance/permits, accounting, and workflow pages into a coherent support layer around the engineer-facing workflows.
> Detailed subplans: `Phase9-DataAdminAndWorkflowIntegration/README.md`

---

## Objective

Keep the engineer workflow surfaces clean while still giving administrators, data stewards, accountants, and compliance users first-class tools.

This phase focuses on the non-calculation, non-core-operations support systems:

- PPDM39 setup and reference data
- data quality, audit, validation, versioning
- access control and role-driven admin pages
- business process dashboards and timelines
- accounting and finance pages
- compliance and permits

---

## Structural Cleanup Targets

| Current Overlap | Required Decision |
|-----------------|-------------------|
| `Pages/Data/*` vs `Pages/PPDM39/Data/*` | define one canonical admin/data route tree |
| setup and seeding pages spread across multiple PPDM39 locations | define one setup flow rooted under `/ppdm39/setup` |
| workflow pages under business-process and HSE/compliance sections | keep process workflows separate from domain CRUD/admin pages |
| accounting pages split between legacy and process-first intent | align dashboards, AFE, cost allocation, royalties, and volume reconciliation to typed accounting clients |

---

## Pass Plan

### Pass A — Canonical Data/Admin Route Tree

- designate canonical routes for setup, schema, metadata, audit, versioning, quality, and validation
- mark or retire legacy duplicates
- align admin navigation with role-based layouts

### Pass B — Typed Clients For Support Domains

- expand or split support clients for PPDM39 admin, compliance/permits, business process, and accounting
- remove raw `ApiClient` usage from complex admin pages where repeated domain logic exists

### Pass C — Workflow and Compliance Alignment

- connect permits/compliance pages to `PermitsAndApplications`
- connect business-process pages to lifecycle/process orchestration clients
- connect accounting surfaces to `ProductionAccounting` and `Accounting` services with clear field and period context

---

## Todo

| ID | Task | Status |
|----|------|--------|
| 9.1 | Choose the canonical admin/data route tree | Done |
| 9.2 | Consolidate PPDM39 setup, seeding, and schema pages under one flow | Done |
| 9.3 | Add or refine typed clients for data admin, workflow, permits/compliance, and accounting | Done |
| 9.4 | Refactor accounting pages to a single finance integration style | Done |
| 9.5 | Refactor workflow and compliance pages to use dedicated process/compliance clients | Done |
| 9.6 | Align role-based layouts and navigation to canonical routes | Done |

---

## Exit Criteria

- one canonical route tree exists for data/admin pages
- support-domain pages use typed clients instead of repeated raw HTTP calls where practical
- compliance, workflow, and accounting pages align to their owning domain projects
- engineer-facing routes remain cleanly separated from data-steward/admin tools
