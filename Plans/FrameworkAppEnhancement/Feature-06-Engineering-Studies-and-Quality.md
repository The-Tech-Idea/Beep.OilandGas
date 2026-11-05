# Engineering study workbench and numerical quality

Created 2026-10-05. Task mapping: CA-20/21; FG-30/31/52. Detailed feature plan for [the existing phase](Company-Phase-C-Engineering.md). The root master tracker owns implementation status; this plan defines work and gates.

## Implementation sequence

1. Inventory active reservoir/PVT/nodal/lift/well-test/property/forecast/DCA calculators, input mappings and result stores. Reuse existing DCA rather than creating a separate module.
2. Implement select asset, inspect provenance, validate inputs, calculate, save version, compare and approve using current role workspaces and canonical contracts.
3. Record units, original source/time, assumptions, applicability range, algorithm version and warnings. Remove silent business/engineering defaults; missing required inputs produce validation failures.
4. Approve a specific study version and issue an authorized intervention/work order without changing operating state merely by calculating. Implement CA-24 units/reference conditions and time semantics.

## Acceptance and evidence

Exit: independently reviewed benchmark datasets and tolerances for each promoted calculator; invalid-domain and conversion cases; browser save/reload/compare/approve and changed-source/asset rejection evidence.

For each promoted workflow, verify successful completion, unauthorized access, validation failure, reload, correction and retry using the active API/client/UI/persistence chain. Use two companies with colliding local identifiers where scope is relevant. Report source-only, automated, provider and browser evidence separately. Capture domain review for numerical/financial outputs; no fabricated data or assumed commercial inputs.

## Dependencies and ownership

Use [company roadmap](Company-App-Roadmap.md), [architecture alignment](02-Architecture-and-Plan-Alignment.md) and [multinational requirements](Company-Multinational-Requirements.md). CA-05/06 gates persistent business workflows. Locate the existing entity, service, registration, endpoint, client, route and test before implementation; name chosen owners in each change. TheTechIdea.Data owns shared business contracts, canonical ApiService owns client-facing domain access, app RBAC supplies standard roles and IdentityServer authenticates. Core master-data admin uses BeepDiA.

## Delivery slices and recovery

Implement the numbered sequence in reviewable slices, updating consumers together and retiring redundant paths. Before migrations, inventory data ownership and preflight ambiguous records; preserve recoverable migration and reconciliation evidence. For multi-step writes declare transaction boundary, idempotency and recovery behavior. Reuse current workflow/setup/posting stores; no compatibility shims, parallel models or success-shaped failures.

Follow applicable repository skills before code changes. Record executed checks, remaining gates and the next slice in the root tracker. Do not mark the feature complete until its full acceptance scope is evidenced.


Current development foundation: [Implementation 60](Implementation-60-Company-Development-Request-and-Failure-Boundary.md) verifies the actual development request/repository/resource/failure boundary and compiles its complete class plus real required calculators/contracts. The final affected gate passes 339, including 90 new cases. Missing definitions and failed saves refuse; unique FIELD/resource membership is required. Canonical development plan/design, drilling/construction/handover, facility composite contracts, PIPELINE/child/reference policy, typed keys/FKs/transaction/readback, numerical defaults/unit/provenance and real provider/HTTP/host acceptance remain required. HSE/exploration/process/concrete host owners continue. No feature or phase closure.
