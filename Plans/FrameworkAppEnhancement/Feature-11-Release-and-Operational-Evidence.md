# Release verification and operational readiness

Created 2026-10-05. Task mapping: CA-44; FG-60/61/62. Detailed feature plan for [the existing phase](Company-Phase-E-Company-Release.md). The root master tracker owns implementation status; this plan defines work and gates.

## Implementation sequence

1. Establish evidence requirements per promoted workflow: source owners, API/UI behavior, authorization, persistence, audit, correction and recovery. Record SHA/environment with each result.
2. Run reproducible CI and supported-provider migrations, transaction/concurrency/restart tests, ownership checks and authenticated browser/OIDC role journeys.
3. Exercise backup/restore, diagnostic configuration, operator runbooks and representative load targets agreed during baseline; identify external provider/domain review requirements explicitly.
4. Execute company pilot across accountant, engineer, operator, HSE, manager and admin, including rejected requests and cross-role handoffs. Extend with multinational CA-46 pilot rather than a second release process.

## Acceptance and evidence

Exit: every promoted requirement has direct scoped evidence; skipped/unsupported/deferred gates remain open; no full release claim from controller mocks, file inventory or narrow green suites.

For each promoted workflow, verify successful completion, unauthorized access, validation failure, reload, correction and retry using the active API/client/UI/persistence chain. Use two companies with colliding local identifiers where scope is relevant. Report source-only, automated, provider and browser evidence separately. Capture domain review for numerical/financial outputs; no fabricated data or assumed commercial inputs.

## Dependencies and ownership

Use [company roadmap](Company-App-Roadmap.md), [architecture alignment](02-Architecture-and-Plan-Alignment.md) and [multinational requirements](Company-Multinational-Requirements.md). CA-05/06 gates persistent business workflows. Locate the existing entity, service, registration, endpoint, client, route and test before implementation; name chosen owners in each change. TheTechIdea.Data owns shared business contracts, canonical ApiService owns client-facing domain access, app RBAC supplies standard roles and IdentityServer authenticates. Core master-data admin uses BeepDiA.

## Delivery slices and recovery

Implement the numbered sequence in reviewable slices, updating consumers together and retiring redundant paths. Before migrations, inventory data ownership and preflight ambiguous records; preserve recoverable migration and reconciliation evidence. For multi-step writes declare transaction boundary, idempotency and recovery behavior. Reuse current workflow/setup/posting stores; no compatibility shims, parallel models or success-shaped failures.

Follow applicable repository skills before code changes. Record executed checks, remaining gates and the next slice in the root tracker. Do not mark the feature complete until its full acceptance scope is evidenced.

