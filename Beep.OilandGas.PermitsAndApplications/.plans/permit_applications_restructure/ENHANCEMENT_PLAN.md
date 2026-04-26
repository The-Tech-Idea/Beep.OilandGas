# Permits & Applications — Enhancement Plan (Oil & Gas Regulatory Workflows)

This document is the **enhancement roadmap** for `Beep.OilandGas.PermitsAndApplications`: bring the library in line with how operators and regulators actually run permitting today—digital submissions, multi-agency tracks, explicit lifecycle states, conditions of approval, and tight links to field activities. Implementation detail stays in phase docs and in `MASTER-TODO-TRACKER.md` at the project root.

## Goals

1. **Workflow fidelity** — States and transitions match a defensible agency model (draft → submitted → under review → RFI → decision → post-approval changes → expiry/renewal).
2. **Jurisdiction depth** — JSON configs + validation rules stay the source of truth; expand coverage without forking core services.
3. **Submission readiness** — Completeness, fees, attachments, and form packets are first-class, testable outcomes.
4. **Operations integration** — Permits constrain and explain drilling, production, injection, and decommissioning activities (LifeCycle / Field modules).
5. **Auditability** — Status history, decisions, and document lineage are consistent across `PermitApplicationWorkflowService` and `PermitApplicationLifecycleService`.

## Industry workflow reference (target behavior)

| Stage | Operator / regulator practice | Library enhancement |
|--------|-------------------------------|------------------------|
| **Intake** | Docket / reference number, agency mailbox or portal receipt, clock start | Persist `RECEIVED_DATE`, agency reference, optional portal correlation id |
| **Parallel agencies** | State + federal + local (e.g. RRC + USACE + local floodplain) | Model **sub-applications** or **linked applications** with independent status (or explicit `REGULATORY_AUTHORITY` children) |
| **Technical review** | Specialist review, site visits, modeling | Map to `UNDER_REVIEW`; optional review milestones table or remarks taxonomy |
| **RFI / deficiency** | Formal information request, resubmission | Use `ADDITIONAL_INFO_REQUIRED` → resubmit to `SUBMITTED` (already in transition rules); add RFI metadata (due date, letter id) |
| **Public / stakeholder** | Notice, comment period | Optional extension: notice dates, comment deadline, response attachment types |
| **Decision** | Approved with conditions, denied, withdrawn | `DECISION` + structured **conditions** (rows or JSON), `EFFECTIVE_DATE` / `EXPIRY_DATE` |
| **Compliance in operations** | MIT, monitoring, incident reporting | Link permits to injection / environmental sub-records; compliance check + reporting services |
| **Change management** | Amendments, transfers, renewals | New types or flags: amendment vs new; `RENEWED` path from `EXPIRED` / `APPROVED` |
| **Decommissioning** | Plugging, P&A, site closure permits | Explicit permit types + LifeCycle hooks for abandonment phase |

## Enhancement themes (prioritized)

### Theme A — Regulatory state machine (high)

- **Single source of truth** for allowed transitions (`PermitStatusTransitionRules`); all entry points validate (workflow service, lifecycle service, status history service).
- **Withdraw** and **start review** as explicit API/domain operations where product needs them (today: use `PermitStatusHistoryService.UpdateStatusAsync` with normalized strings).
- **Decision vocabulary** — Document approved / rejected / administratively withdrawn; align string constants across controllers and services.

**Target files:** `Services/PermitStatusTransitionRules.cs`, `Services/PermitApplicationLifecycleService.cs`, `Services/PermitApplicationWorkflowService.cs`, `Services/PermitStatusHistoryService.cs`

**Verification:** Unit tests for every allowed and forbidden transition; lifecycle + workflow behave identically on submit/decision.

### Theme B — Jurisdiction & validation (high)

- **Config inventory** — `Configs/jurisdiction_*.json` aligned with `RegulatoryAuthority` and `JurisdictionHelper`.
- **Validation engine** — Expand `PermitValidationRulesFactory` + rules: blocking vs warning taxonomy; per-authority overlays (RRC, AER, BOEM/BSEE, NDIC, etc.).
- **Form templates** — Keep registry-driven payloads; add golden-file tests for JSON/HTML packets.

**Target files:** `Configs/*.json`, `Validation/**`, `Forms/PermitFormTemplateRegistry.cs`, `Forms/PermitFormPayloadBuilder.cs`

**Verification:** Rule tests per jurisdiction sample; no regression on `ValidatePermitApplicationAsync`.

### Theme C — Data model & PPDM alignment (medium)

- Canonical **PERMIT_APPLICATION** + extension tables (drilling / environmental / injection) without duplicate table classes.
- First-class **APPLIC_BA**, **APPLIC_DESC**, **APPLIC_REMARK**, **BA_PERMIT**, **FACILITY_LICENSE**, **WELL_PERMIT_TYPE** where APIs expose them.
- Optional: conditions-of-approval table or JSON column strategy (document in DATA_MODEL_AUDIT).

**Target files:** `DataMapping/*`, `Data/PermitsAndApplications/**`, `.plans/.../DATA_MODEL_AUDIT.md`, `Beep.OilandGas.Models` scripts if schema evolves

**Verification:** Mapper round-trip tests; FK / required-field checks documented.

### Theme D — Compliance & reporting (medium)

- **Expiration and renewal** — Lead-time rules, dashboards (`PermitComplianceReportService` + projections).
- **Violation / non-compliance** — Optional extension: link to HSE or operations incidents.
- **Exports** — Regulatory submission packages (zip of attachments + manifest).

**Target files:** `Services/PermitComplianceReportService.cs`, `Services/PermitComplianceCheckService.cs`, `Core/Interfaces/*`

**Verification:** Report generation tests with seeded applications; date-boundary tests for expiry.

### Theme E — Integration & automation (medium–lower)

- **LifeCycle** — `Beep.OilandGas.LifeCycle` process steps that read permit status before drilling/production transitions (see og-life skill / `ProcessIntegrationHelper`).
- **Notifications** — Renewal / RFI due (outbound events or integration hooks; implementation can be stub + interface).
- **Batch / portal** — Import status updates from regulator CSV/JSON (design only in early increments if no portal yet).

**Target files:** `Beep.OilandGas.LifeCycle/**`, `Beep.OilandGas.ApiService/**` (controllers), client packages if applicable

**Verification:** Integration test or manual runbook: “permit blocked → unblocked after approval.”

## Phased delivery (maps to existing phase docs)

| Phase doc | Enhancement focus |
|-----------|-------------------|
| `phase1_audit_remove_duplicates.md` | Theme C — duplicates, canonical models |
| `phase3_extension_tables.md` | Theme C — extensions + conditions / linked apps |
| `phase4_service_mapper_api.md` | Themes A, B, D — services, APIs, reporting |
| `phase5_testing_validation.md` | Verification for A–D |
| `IMPLEMENTATION_PLAN.md` | Broad lifecycle + jurisdiction reference (keep in sync with this file) |

## Success criteria (enhancement “done”)

- Transition rules enforced consistently on create/submit/decision/status update.
- At least **three** jurisdictions with blocking validation tests (e.g. RRC, AER, one federal or secondary state from configs).
- Compliance report covers expiring permits with configurable horizon.
- README / `JURISDICTION_SUPPORT.md` list authorities that match `Configs/` and enums.
- `MASTER-TODO-TRACKER.md` shows all enhancement epics checked or explicitly deferred with rationale.

## Out of scope (unless product asks)

- Live portal robots or e-sign inside this library (prefer integration boundaries).
- Full PDF government form replicas (use payloads + attachments as interim).

## Related documents

- `README.md` (this folder) — index of all plans  
- `../../MASTER-TODO-TRACKER.md` — single checklist across phases  
- `JURISDICTION_SUPPORT.md` — jurisdiction usage  
- `IMPLEMENTATION_PLAN.md` — field lifecycle + entity mapping  
- `todo_tracker.md` — granular restructure tasks (merge with master tracker over time)
