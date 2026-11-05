# Authoritative period close and record-to-report

Created 2026-10-05. Task mapping: CA-04/13; FG-41. Detailed feature plan for [the existing phase](Company-Phase-B-Finance.md). The root master tracker owns implementation status; this plan defines work and gates.

## Implementation sequence

1. Inventory PeriodClose.razor, accounting client/controller/service, financial claims and persisted close records. Replace hardcoded close history and current_user actor with API-owned records using existing owners.
2. Persist checklist/readiness evidence, closing notes, effective period, actor and timestamps. Local checkboxes cannot authorize close; validate readiness on server against reconciliations and pending work.
3. Connect journal approval, ledger, bank reconciliation, depreciation and statements. Enforce closed-period writes, authorized reopen and audited reversal with declared transaction boundaries.
4. Prove concurrent close/post, timeout after commit, replay and interrupted-operation recovery on supported providers. Apply CA-15/16 to entity books, currencies and policies.

## Acceptance and evidence

Exit: fresh history empty; persisted close reloads with authenticated actor; trial balance/subledgers reconcile; unauthorized or blocked close fails; approved statements drill down to source entries.

For each promoted workflow, verify successful completion, unauthorized access, validation failure, reload, correction and retry using the active API/client/UI/persistence chain. Use two companies with colliding local identifiers where scope is relevant. Report source-only, automated, provider and browser evidence separately. Capture domain review for numerical/financial outputs; no fabricated data or assumed commercial inputs.

## Dependencies and ownership

Use [company roadmap](Company-App-Roadmap.md), [architecture alignment](02-Architecture-and-Plan-Alignment.md) and [multinational requirements](Company-Multinational-Requirements.md). CA-05/06 gates persistent business workflows. Locate the existing entity, service, registration, endpoint, client, route and test before implementation; name chosen owners in each change. TheTechIdea.Data owns shared business contracts, canonical ApiService owns client-facing domain access, app RBAC supplies standard roles and IdentityServer authenticates. Core master-data admin uses BeepDiA.

## Delivery slices and recovery

Implement the numbered sequence in reviewable slices, updating consumers together and retiring redundant paths. Before migrations, inventory data ownership and preflight ambiguous records; preserve recoverable migration and reconciliation evidence. For multi-step writes declare transaction boundary, idempotency and recovery behavior. Reuse current workflow/setup/posting stores; no compatibility shims, parallel models or success-shaped failures.

Follow applicable repository skills before code changes. Record executed checks, remaining gates and the next slice in the root tracker. Do not mark the feature complete until its full acceptance scope is evidenced.

