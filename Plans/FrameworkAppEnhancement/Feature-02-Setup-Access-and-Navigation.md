# Setup, access, navigation and failure states

Created 2026-10-05. Task mapping: CA-03; FG-11/12/13/20/21/50. Detailed feature plan for [the existing phase](Company-Phase-A-Foundation.md). The root master tracker owns implementation status; this plan defines work and gates.

## Implementation sequence

1. Reproduce fresh-install module binding and wizard flows; verify integrated-security/provider choices and prerequisites against current implementation. Extend active setup orchestrator rather than introduce a second wizard.
2. Verify API-backed standard role transformation, revocation and field access under app-owned RBAC. Company membership and selected context must satisfy CA-05/06.
3. Reproduce custom menu activator behavior for accountant, engineering, navigation and notifications; reuse established MudBlazor activation patterns. Validate links against actual routes.
4. Unify loading, empty, unauthorized, unavailable, validation and conflict states through active typed clients, inbox and notification services. Failed requests must never appear as zero work or successful save.

## Acceptance and evidence

Exit: empty-install setup completes; two identities cannot cross tenant/assets or notifications; all menus usable by keyboard; failure/retry preserves valid input. Include browser/OIDC and direct HTTP evidence.

For each promoted workflow, verify successful completion, unauthorized access, validation failure, reload, correction and retry using the active API/client/UI/persistence chain. Use two companies with colliding local identifiers where scope is relevant. Report source-only, automated, provider and browser evidence separately. Capture domain review for numerical/financial outputs; no fabricated data or assumed commercial inputs.

## Dependencies and ownership

Use [company roadmap](Company-App-Roadmap.md), [architecture alignment](02-Architecture-and-Plan-Alignment.md) and [multinational requirements](Company-Multinational-Requirements.md). CA-05/06 gates persistent business workflows. Locate the existing entity, service, registration, endpoint, client, route and test before implementation; name chosen owners in each change. TheTechIdea.Data owns shared business contracts, canonical ApiService owns client-facing domain access, app RBAC supplies standard roles and IdentityServer authenticates. Core master-data admin uses BeepDiA.

## Delivery slices and recovery

Implement the numbered sequence in reviewable slices, updating consumers together and retiring redundant paths. Before migrations, inventory data ownership and preflight ambiguous records; preserve recoverable migration and reconciliation evidence. For multi-step writes declare transaction boundary, idempotency and recovery behavior. Reuse current workflow/setup/posting stores; no compatibility shims, parallel models or success-shaped failures.

Follow applicable repository skills before code changes. Record executed checks, remaining gates and the next slice in the root tracker. Do not mark the feature complete until its full acceptance scope is evidenced.

