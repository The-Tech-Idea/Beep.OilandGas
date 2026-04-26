# Phase 3: API Integration

## Goal

Move HSE API endpoints onto the lifecycle/orchestrator boundary and remove any remaining dependence on schema-mismatched service calls.

## Work Items

- Update `HSEController` to use `IFieldOrchestrator.GetHSEService()`.
- Force active-field context on report, list, KPI, and HAZOP-study endpoints.
- Keep request and response records only where they represent aggregate API shapes rather than PPDM entities.
- Preserve business-process endpoints that run through `IProcessService`, but align them with the PPDM-backed incident ids and status mapping.
- Ensure process HSE endpoints use the real PPDM incident id as the process entity id and synchronize RCA / corrective-action / close-out operations through the field HSE domain service as well as workflow history.
- Remove any API code that writes placeholder values or relies on `FieldId = "current"` behavior.

## Endpoint Rules

- `GET /api/field/current/hse/incidents`: derive the field from lifecycle.
- `POST /api/field/current/hse/incidents`: overwrite incoming field id with active field id before persistence.
- `GET /api/field/current/hse/kpis`: calculate against incidents linked to the active field through PPDM component rows.
- RCA, barrier, corrective action, and HAZOP endpoints should continue to use incident id / study id routing but operate on PPDM-backed services.
- Process-side corrective-action endpoints must carry the real fields required by PPDM corrective-action storage, including action type and due date, rather than inventing controller-side defaults.

## Verification

- Build `Beep.OilandGas.ApiService`.
- Confirm Swagger contract still loads for HSE endpoints.