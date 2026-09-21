# Phase 5: coherent oil and gas application experience

Dependencies: FG-10 through FG-13 and the corresponding data/calculation/workflow APIs. Responsible areas: Web, Client, external ThemeBranding and BeepDiA integration. Findings: R06, R08.

| Task | Deliverable | Acceptance |
|---|---|---|
| FG-50 | Standard loading, empty, unauthorized, unavailable, validation and conflict states in existing workbenches/inbox. | API failure never appears as zero work or successful save. Retry preserves valid inputs; field changes cancel/ignore old responses; no stale data from the previous field remains. |
| FG-51 | Apply BrandingConfig/Mud theme mapping consistently; consolidate custom CSS/JS into app.css/site.js and remove literal palette colors. | Representative pages work in light/dark and Arabic RTL; no page-local themes/style/script blocks remain in migrated scope; charts have readable labels and non-color status cues. |
| FG-52 | Complete persona journeys using existing navigation, field context and typed clients; support comparison, export and audit drill-down. | An authorized user can complete each promoted journey without manual API calls; deep links enforce the same roles/resources; keyboard interaction and validation focus work. |

## Concrete application flows

| Persona/experience | Flow | Required states |
|---|---|---|
| Production engineer | Choose permitted field -> review measurements -> analyze well -> compare saved scenarios -> submit intervention. | Missing input, stale data, solver failure, submission conflict and saved result. |
| Accountant | Review production allocation -> inspect reconciliation differences -> approve/post -> close or authorized reopen. | Rounding variance, duplicate submission, closed period, forbidden approval and audited reversal. |
| HSE/permit user | Review assigned work -> attach evidence -> hand off -> track due dates and closure. | Restricted asset, upload failure, overdue task and reassignment. |
| Manager | Open scoped inbox -> inspect evidence and assumptions -> approve/reject -> observe outcome. | Separation-of-duties rejection, expired delegation, concurrent action and API outage. |
| Administrator | Use BeepDiA management workflows against approved setup/resource endpoints. | Preflight, validation preview, progress, interrupted operation and retry outcome. |

## Implementation guidance

Reuse existing MudBlazor components and BrandingConfig mapping. Move component styles into scoped class names in app.css; use Mud palette tokens, including diagram/chart colors. Keep JS in site.js. Treat persona selection as navigation context; it must never grant roles. Provide explicit engineering units and assumption badges near inputs/results, with provenance available in details/export.

For large grids, extend existing server-side filtering/paging rather than fetching whole tables into a circuit. Define URL/deep-link context for field, asset and scenario while validating every identifier at the API. Exported results include context and units and are generated only from authorized data.

## Verification

Use authenticated browser tests for the production and manager journeys first. Check two users/two fields, keyboard-only completion, focus after errors, RTL layouts, dark-theme plots, slow API, disconnected hub and retry. Set representative data volumes in fixtures and record measured timings; performance targets must be agreed before making responsiveness claims. Exit requires working business outcomes and failure states, not screenshot-only approval.
