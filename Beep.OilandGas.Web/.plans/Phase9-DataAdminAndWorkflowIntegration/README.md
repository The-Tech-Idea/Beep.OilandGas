# Phase 9 Folder Plan — Data, Admin, and Workflow Integration

> Detailed execution plan for Phase 9  
> Companion overview: `../Phase9-DataAdminAndWorkflowIntegration.md`
> Project knowledge base: `../Projects/INDEX.md`
> Evidence manifest: `ScanEvidence.md`

---

## Purpose

Phase 9 is the support-layer phase for the entire solution. It rationalizes PPDM data/admin experiences, support-domain typed clients, workflow pages, compliance, permits, and accounting without polluting the engineer-facing routes.

---

## Whole-Solution Pass Model

All Beep.OilandGas solution projects participate in this phase as `Primary`, `Supporting`, or `Validation` roles. Use `ProjectCoverageMatrix.md` as the source of truth for project-by-project participation.

Use the per-project docs in `../Projects/Core/`, `../Projects/FinanceAndSupport/`, `../Projects/Presentation/`, and related categories as the canonical ownership baseline.

---

## Project-Driven Priorities

| Project set | Planning rule |
|-------------|---------------|
| `Beep.OilandGas.PPDM39`, `Beep.OilandGas.PPDM39.DataManagement`, `Beep.OilandGas.Models` | Treat as the canonical data/admin backbone |
| `Beep.OilandGas.Web`, `Beep.OilandGas.ApiService` | Treat as the primary UI/API route owners for admin and workflow consolidation |
| `Accounting`, `ProductionAccounting`, `PermitsAndApplications` | Treat as service-rich but under-surfaced domains that need explicit UI/API decisions |
| `UserManagement`, `DataManager`, `Branchs`, `Drawing`, `PermitsAndApplications.Pdf.Wkhtmltopdf` | Keep in support roles unless a direct admin surface is intentionally created |
| `LifeCycle` | Use as the main workflow/process owner when page-level workflow routing is rationalized |

---

## Pass Set

| Pass | Focus | Document |
|------|-------|----------|
| A | Canonical admin/data route tree and ownership | `PassA-CanonicalDataAdminRouteTree.md` |
| B | Support-domain typed clients and integration seams | `PassB-SupportDomainTypedClients.md` |
| C | Workflow, compliance, accounting, and governance alignment | `PassC-WorkflowComplianceAccountingAlignment.md` |

---

## Phase Deliverables

- one canonical PPDM/data admin route tree
- one canonical setup/schema/seeding flow
- typed clients for admin, workflow, compliance, permits, and accounting domains
- role-aware admin navigation and page ownership
- clean separation between engineer workflow pages and data-steward/admin tooling

---

## Scan-Based Findings That Shape This Phase

| Finding | Planning Impact |
|---------|-----------------|
| `Pages/Data/*` and `Pages/PPDM39/Data/*` are both live route trees | Canonical admin/data route selection is a real consolidation task |
| `PPDM39` and `PPDM39.DataManagement` are the actual data-layer backbone with broad API coverage | Admin tooling should converge around PPDM-backed service seams |
| `PermitsAndApplications` is validation-heavy with PDF support but weak direct UI/API surfacing | Permit/compliance support requires explicit surfacing work |
| `Accounting` and `ProductionAccounting` are service-heavy with thin web presence | Finance surfacing must be treated as build-out, not only rationalization |
| `LifeCycle` owns important workflow and work-order logic | Workflow pages should converge toward LifeCycle-backed orchestration |
| `UserManagement`, `Branchs`, `DataManager`, and `Drawing` are support projects | They remain support/infrastructure roles, not peer workflow domains |
