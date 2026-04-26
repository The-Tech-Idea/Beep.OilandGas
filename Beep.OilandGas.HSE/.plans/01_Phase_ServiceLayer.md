# Phase 1: Service Layer Refactor

## Goal

Replace schema-mismatched HSE service logic with typed PPDM entity usage and explicit table-to-workflow mapping.

## Work Items

- Refactor `HSEIncidentService` to use:
  - `HSE_INCIDENT` for header identity, reporter, dates, work-related flags, and primary remark.
  - `HSE_INCIDENT_DETAIL` for incident type and severity.
  - `HSE_INCIDENT_COMPONENT` for field linkage, jurisdiction, location context, and impacted assets.
  - `HSE_INCIDENT_BA` for investigator and involved-party assignments.
  - `HSE_INCIDENT_RESPONSE` for workflow state and transition history.
- Refactor `RCAService` to use `HSE_INCIDENT_CAUSE` properties actually present in PPDM:
  - `CAUSE_OBS_NO`
  - `CAUSE_CODE`
  - `CAUSE_TYPE`
  - `REMARK`
- Refactor `BarrierManagementService` to use `HSE_INCIDENT_COMPONENT` real fields:
  - `EQUIPMENT_ID`
  - `COMPONENT_TYPE`
  - `COMPONENT_ROLE`
  - `FIELD_ID`
  - `JURISDICTION`
  - `REMARK`
- Refactor `HSEKPIService` to derive field scope from `HSE_INCIDENT_COMPONENT.FIELD_ID` and incident ids rather than a non-existent field on `HSE_INCIDENT`.
- Validate `CorrectiveActionService` and `HAZOPService` against PPDM project tables and keep them only where no dedicated HSE table exists.
- Use `PROJECT` + `PROJECT_STEP` + `PROJECT_STEP_BA` + `PROJECT_STATUS` for corrective action plans, assignees, and step state instead of guessed `STEP_SEQ` / `STEP_STATUS` / `ROLE_CODE` fields.
- Use `PROJECT` + `PROJECT_COMPONENT` + `PROJECT_STEP` + `PROJECT_CONDITION` + `PROJECT_STATUS` for HAZOP study, field context, nodes, deviations, and state.
- Where PPDM project tables do not expose native per-node deviation text/consequence columns, store the extra deviation payload in PPDM text fields with an explicit service-side convention rather than inventing non-PPDM entities.

## Mapping Policy

- Treat `HSEIncidentRecord` as an aggregate projection, not a storage entity.
- Store workflow status in `HSE_INCIDENT_RESPONSE` rows.
- Store investigator assignment as an `HSE_INCIDENT_BA` row with a clear `INVOLVED_BA_ROLE` convention.
- Store field association in `HSE_INCIDENT_COMPONENT` using a primary component row for the field context.
- Use severity ids like `TIER_1`, `TIER_2`, `TIER_3`, `TIER_4` unless a stricter reference-table convention is already present in the environment.
- Store corrective action step state in `PROJECT_STATUS` rows keyed by `STEP_ID`, not as ad hoc columns on `PROJECT_STEP`.
- Store HAZOP field association in `PROJECT_COMPONENT.FIELD_ID` and HAZOP study/deviation status in `PROJECT_STATUS` because `PROJECT` and `PROJECT_CONDITION` do not carry those dedicated fields directly.

## Verification

- Build `Beep.OilandGas.PPDM39.DataManagement`.
- Build `Beep.OilandGas.ApiService` to confirm interface compatibility.
- Confirm no HSE service still queries guessed columns such as `FIELD_ID` on `HSE_INCIDENT`, `CAUSE_SEQ` on `HSE_INCIDENT_CAUSE`, or `EQUIP_ID` on `HSE_INCIDENT_COMPONENT`.