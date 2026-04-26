# HSE PPDM Refactor Overview

## Objective

Replace the current HSE implementation that writes assumed incident columns with a PPDM-backed implementation that uses verified PPDM entities and tables first, then composes higher-level workflow and UI shapes only where PPDM does not provide a single direct entity.

## Verified PPDM Source Of Truth

The local generated PPDM model classes are the schema anchor for this work.

- `HSE_INCIDENT`: incident header, dates, reporter, basic remark, lost time / work related indicators.
- `HSE_INCIDENT_DETAIL`: incident classification and severity (`INCIDENT_TYPE_ID`, `INCIDENT_SEVERITY_ID`, `DETAIL_TYPE`, `REMARK`).
- `HSE_INCIDENT_COMPONENT`: field / facility / equipment / well / jurisdiction links and barrier/component context.
- `HSE_INCIDENT_BA`: involved parties, investigator assignment, injured-person and responsible-party roles.
- `HSE_INCIDENT_CAUSE`: RCA observations (`CAUSE_OBS_NO`, `CAUSE_CODE`, `CAUSE_TYPE`, `REMARK`).
- `HSE_INCIDENT_RESPONSE`: workflow/status history and response outcomes.
- `PROJECT`, `PROJECT_STEP`, `PROJECT_STEP_CONDITION`, `PROJECT_STEP_BA`: fallback PPDM process tables for HAZOP and corrective-action workflows when no dedicated HSE table exists.

## Required Architectural Direction

- Use PPDM entity classes in data services whenever a PPDM entity exists.
- Remove guessed HSE columns and guessed property names from the current HSE services.
- Keep non-PPDM models only for composed request/response or UI aggregate shapes that span multiple PPDM tables.
- Route field-scoped HSE operations through lifecycle orchestration instead of bypassing `FieldOrchestrator`.
- Preserve seeded HSE process definitions already present in lifecycle process initialization.

## Phase Order

1. Service layer refactor against verified PPDM entities.
2. Lifecycle integration with a field-scoped HSE service.
3. API integration through the orchestrator boundary.
4. Web integration for live incident workflows and dashboards.
5. Validation and hardening.

## Planned Deliverables

- PPDM-backed HSE incident, RCA, barrier, KPI, corrective-action, and HAZOP services.
- Field-scoped lifecycle HSE wrapper exposed from `IFieldOrchestrator`.
- API controllers that normalize field context and stop calling schema-mismatched code.
- Web HSE pages and clients bound to the real API flow with no placeholder submission behavior.
- A tracked execution plan in this `.plans` folder.

## Key Risks

- Existing HSE DTOs currently imply columns not present in PPDM. Those DTOs must become aggregates, not pseudo-entities.
- Some workflow concepts such as state, close-out progression, and investigator ownership span multiple PPDM tables and need a consistent mapping policy.
- KPI calculations must be rebuilt around incident-component field linkage instead of a non-existent `FIELD_ID` on `HSE_INCIDENT`.

## Rollback Strategy

- Land the refactor in vertical slices: service layer first, then lifecycle/API, then web.
- Validate each slice with a targeted build before widening scope.
- Avoid changing unrelated process definitions or non-HSE phase services.