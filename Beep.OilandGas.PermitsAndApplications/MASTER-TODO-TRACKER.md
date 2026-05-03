# PermitsAndApplications Master Tracker

## Phase Rollup
- [x] Phase 0 - Planning baseline and governance docs
- [x] Phase 1 - Canonical model and contract reconciliation
- [x] Phase 2 - Data access canonicalization
- [x] Phase 3 - Reference catalog and idempotent seeding
- [x] Phase 4 - Service architecture cleanup
- [x] Phase 5 - API + lifecycle integration alignment
- [x] Phase 6 - Tests and hardening
- [x] Phase 7 - Rollout and exit reconciliation

## Active TODOs
- [x] Create module-local `.plans` and tracker artifacts.
- [x] Define PPDM-first matrix and model shape boundaries.
- [x] Keep single canonical permit lifecycle persistence path active.
- [x] Add idempotent reference seed catalog.
- [x] Add focused API/module tests for status/seeding behavior.

## Verification Criteria
- Permits services compile and use consistent model contracts.
- Module seeding is idempotent (no duplicate `(REFERENCE_SET, REFERENCE_CODE)` keys).
- API and lifecycle permit paths are explicitly aligned/documented.
- Focused tests pass for permits controller and seed integrity.
- Full solution gate is green (`dotnet build Beep.OilandGas.sln`).
# MASTER-TODO-TRACKER — Beep.OilandGas.PermitsAndApplications

Single checklist for **restructure** work and **enhancement** work (oil & gas regulatory workflows). Detailed narrative: `.plans/permit_applications_restructure/ENHANCEMENT_PLAN.md`. Phase playbooks: same folder, `phase*.md`.

---

## Phase 0 — Enhancement baseline (workflow & docs)

| ID | Task | Target / owner | Verification |
|----|------|----------------|---------------|
| 0.1 | Read and align team on `ENHANCEMENT_PLAN.md` | `.plans/permit_applications_restructure/ENHANCEMENT_PLAN.md` | Review signed off |
| 0.2 | Align `JURISDICTION_SUPPORT.md` + `SUPPORTED_JURISDICTIONS.md` with `Configs/jurisdiction_*.json` and `RegulatoryAuthority` enum | `.plans/**`, `Configs/`, `Data/PermitsAndApplications/Projections/RegulatoryAuthority.cs` | Lists match filenames **(done: config table + cross-link)** |
| 0.3 | Ensure all services use `PermitStatusTransitionRules` for user-facing transitions | `Services/*Permit*.cs` | Code review + transition tests **(done: lifecycle submit/decision; history + workflow persist canonical status keys via `PermitApplicationStatusCodes`)** |

---

## Phase 1 — Audit & duplicate removal

| ID | Task | Target | Verification |
|----|------|--------|---------------|
| 1.1 | List table/entity classes vs PPDM39 canonical | `phase1_audit_remove_duplicates.md` | Inventory appendix complete |
| 1.2 | Remove or merge duplicates; fix references | Project-wide | `dotnet build` solution |
| 1.3 | Update DATA_MODEL_AUDIT | `DATA_MODEL_AUDIT.md` | No orphan entities |

---

## Phase 2 — Canonical models in code

| ID | Task | Target | Verification |
|----|------|--------|---------------|
| 2.1 | Refactor mappers to canonical `PERMIT_*` types | `DataMapping/` | Mapper unit tests |
| 2.2 | Domain vs persistence boundaries clear for lifecycle | `PermitApplicationMapper.cs`, services | Create/update round-trip |

---

## Phase 3 — Extension tables & regulatory extensions

| ID | Task | Target | Verification |
|----|------|--------|---------------|
| 3.1 | Design linked applications / sub-dockets (parallel agencies) | `phase3_extension_tables.md`, schema notes | Design reviewed |
| 3.2 | Optional: conditions of approval storage | SQL scripts / models | Migration dry-run |
| 3.3 | RFI metadata (due date, letter id) | Extension tables or JSON | API contract documented |

---

## Phase 4 — Services, mappers, API

| ID | Task | Target | Verification |
|----|------|--------|---------------|
| 4.1 | API parity for submit, decision, status history, attachments | `Beep.OilandGas.ApiService` (if applicable) | Integration tests |
| 4.2 | Validation ruleset per jurisdiction (blocking vs warning) | `Validation/` | Per-config tests |
| 4.3 | Compliance reporting + expiring permit rules | `PermitComplianceReportService.cs` | Golden report snapshot |

---

## Phase 5 — Testing, validation, documentation

| ID | Task | Target | Verification |
|----|------|--------|---------------|
| 5.1 | Unit tests: transitions, mappers, validation | `Beep.OilandGas.PermitsAndApplications.Tests` | CI green **(started: transition + status code tests)** |
| 5.2 | Form packet golden files | `Forms/`, test data | Diff stable |
| 5.3 | Update `PROJECT_SUMMARY.md` + main `README.md` | Project root | No contradictions with code |

---

## Phase 6 — Enhancement backlog (from ENHANCEMENT_PLAN)

| ID | Task | Target | Verification |
|----|------|--------|---------------|
| 6.1 | LifeCycle hooks: gate processes on permit status | `Beep.OilandGas.LifeCycle` | Manual / integration scenario |
| 6.2 | Notification interfaces (renewal / RFI) | New abstractions + DI | Mocked tests |
| 6.3 | Batch regulator status import | Design + optional impl | Sample file ingested |

---

## Status legend

Use in commits/PRs: **todo** | **doing** | **done** | **n/a** (deferred with reason in PR description).

---

## Maintenance

- After each epic, update **this file** and `.plans/permit_applications_restructure/todo_tracker.md` so they do not diverge.
- Prefer one PR per phase slice (e.g. “Phase 4.2 — AER drilling rules”).
