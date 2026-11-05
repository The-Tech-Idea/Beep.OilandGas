# Coverage, architecture and reproducible delivery

Created 2026-10-05. Task mapping: CA-01/02; FG-00/01/02/10/60. Detailed feature plan for [the existing phase](Company-Phase-A-Foundation.md). The root master tracker owns implementation status; this plan defines work and gates.

## Implementation sequence

1. Inventory every route, process form, endpoint, domain owner and test across accountants, engineers, operators, commercial, HSE, managers and administrators. Classify partial versus verified workflows and include HR/payroll, documents, telemetry and integration needs.
2. Resolve the OilGas-to-canonical-ApiService hosting boundary; inventory TheTechIdea.Data contract migrations and BeepDiA management consumers. Remove duplicate contracts only after migrating all consumers together.
3. Audit reachable Web registrations and dependencies for direct domain database access. Retain pure rendering/calculation dependencies only when needed; Web business reads/writes must use typed APIs.
4. Align CI with master, establish clean-worker external dependency acquisition, controlled restore and build, ownership guards and relevant test suites. Separate existing warning debt from new failures.

## Acceptance and evidence

Exit: complete role/route/owner matrix; recorded architecture decisions; clean-worker evidence and reachable API-only Web paths. Historical suite counts do not satisfy current baseline.

For each promoted workflow, verify successful completion, unauthorized access, validation failure, reload, correction and retry using the active API/client/UI/persistence chain. Use two companies with colliding local identifiers where scope is relevant. Report source-only, automated, provider and browser evidence separately. Capture domain review for numerical/financial outputs; no fabricated data or assumed commercial inputs.

## Dependencies and ownership

Use [company roadmap](Company-App-Roadmap.md), [architecture alignment](02-Architecture-and-Plan-Alignment.md) and [multinational requirements](Company-Multinational-Requirements.md). CA-05/06 gates persistent business workflows. Locate the existing entity, service, registration, endpoint, client, route and test before implementation; name chosen owners in each change. TheTechIdea.Data owns shared business contracts, canonical ApiService owns client-facing domain access, app RBAC supplies standard roles and IdentityServer authenticates. Core master-data admin uses BeepDiA.

## Delivery slices and recovery

Implement the numbered sequence in reviewable slices, updating consumers together and retiring redundant paths. Before migrations, inventory data ownership and preflight ambiguous records; preserve recoverable migration and reconciliation evidence. For multi-step writes declare transaction boundary, idempotency and recovery behavior. Reuse current workflow/setup/posting stores; no compatibility shims, parallel models or success-shaped failures.

Follow applicable repository skills before code changes. Record executed checks, remaining gates and the next slice in the root tracker. Do not mark the feature complete until its full acceptance scope is evidenced.

