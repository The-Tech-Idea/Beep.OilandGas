# Phase 4: Web Integration

## Goal

Replace placeholder HSE UI behavior with live API-backed workflows that reflect the PPDM-backed incident model.

## Work Items

- Update the HSE web client to keep using the field-current API paths while relying on server-side field normalization.
- Remove hardcoded submission values that fake field context or jurisdiction defaults.
- Update the incident management page to:
  - load incidents from the PPDM-backed API,
  - submit new incidents through the real report endpoint,
  - use transition endpoints for close-out rather than snackbar-only behavior,
  - display aggregate fields composed from PPDM header/detail/component/BA rows.
- Keep UI-specific view models only where no single PPDM entity can drive the page directly.

## MudBlazor Constraint

Before `.razor` edits, read the local MudBlazor docs required by repo instructions and preserve the existing MudBlazor visual language.

## Verification

- Build `Beep.OilandGas.Web`.
- Smoke-test incident list, report, assign, transition, RCA, and KPI page flows against the API.