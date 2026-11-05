# Company workspace, management reporting and integrations

Data analyst update: the landing links to a company-scoped review of up to 20 wells, with a bounded visible-well sample and a field-presence summary for visible requested records. The fixed-connection quality dashboard, metrics and alerts API entry points and their unused clients were removed; table-wide quality remains unimplemented. Twenty-one focused route/repository/reviewer/screen cases passed with controlled boundaries. A Beep-migrated local SQLite WELL test used a five-column test projection and controlled row policy to check company filtering; production schemas, hosted authorization and browser remain open. See [persona workflows](Company-Persona-Data-Workflows.md).

Current implementation: [77 — company persona state and landing](Implementation-77-Company-Persona-State-and-Landing.md). Six state checks pass; current Razor wiring is source-only. Role eligibility and the persona journeys remain open. Installed substitute UI/auth/form guidance is approved.

Persona-first delivery: [company persona data workflows](Company-Persona-Data-Workflows.md) and 13 detailed persona plans now define the execution view: company/role landing -> daily queue -> source/detail -> permitted action -> next-role handoff -> recorded outcome -> KPI drill-down. Existing CA/FG owners remain; prioritize missing backend behavior needed by these visible journeys.

Created 2026-10-05. Task mapping: CA-40/41/42/43; FG-51/52. Detailed feature plan for [the existing phase](Company-Phase-E-Company-Release.md). The root master tracker owns implementation status; this plan defines work and gates.

## Implementation sequence

1. Connect role workspace, search, unified inbox, notifications and asset history across studies, work orders, readings and journals. Persona selection never grants rights.
2. Define executive metric source, period, unit/currency, freshness and reconciliation; every promoted metric drills into authoritative records and restricted exports.
3. Inventory native versus integrated HR/payroll, ERP/bank, telemetry, documents and regulatory exchange. Record deferred functions explicitly; integrations declare one authoritative master-data owner.
4. Implement mapping, credentials, idempotency, delivery/retry and reconciliation through existing orchestration; centralize branding/CSS/JS and validate keyboard, dark mode and RTL.

## Acceptance and evidence

Exit: cross-role browser tasks usable without table-name knowledge; unavailable data never zero; integration failure/replay visible; search/export isolation and CA-45 localization/residency pass.

For each promoted workflow, verify successful completion, unauthorized access, validation failure, reload, correction and retry using the active API/client/UI/persistence chain. Use two companies with colliding local identifiers where scope is relevant. Report source-only, automated, provider and browser evidence separately. Capture domain review for numerical/financial outputs; no fabricated data or assumed commercial inputs.

## Dependencies and ownership

Use [company roadmap](Company-App-Roadmap.md), [architecture alignment](02-Architecture-and-Plan-Alignment.md) and [multinational requirements](Company-Multinational-Requirements.md). CA-05/06 gates persistent business workflows. Locate the existing entity, service, registration, endpoint, client, route and test before implementation; name chosen owners in each change. TheTechIdea.Data owns shared business contracts, canonical ApiService owns client-facing domain access, app RBAC supplies standard roles and IdentityServer authenticates. Core master-data admin uses BeepDiA.

## Delivery slices and recovery

Implement the numbered sequence in reviewable slices, updating consumers together and retiring redundant paths. Before migrations, inventory data ownership and preflight ambiguous records; preserve recoverable migration and reconciliation evidence. For multi-step writes declare transaction boundary, idempotency and recovery behavior. Reuse current workflow/setup/posting stores; no compatibility shims, parallel models or success-shaped failures.

Follow applicable repository skills before code changes. Record executed checks, remaining gates and the next slice in the root tracker. Do not mark the feature complete until its full acceptance scope is evidenced.


Current persona implementation: [Implementation 78](Implementation-78-Company-Role-Workspace-Review.md). 17 state and 14 SQLite store tests pass. API/host/rendered screens and full persona journeys remain open; next is mapping administration and actual landing/source workflow.

Latest persona slice: [Implementation 79](Implementation-79-Reviewed-Role-Workspace-Administration.md): existing role screen gains reviewed workspace assignments/readback; 20 SQLite/store tests pass, including canonical company-role resolution. Actual HTTP/host/rendered UI and accountant daily workflow remain open.
