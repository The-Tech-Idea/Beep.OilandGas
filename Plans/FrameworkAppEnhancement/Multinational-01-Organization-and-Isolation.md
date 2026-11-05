# Organization, membership and tenant isolation

Plan date: 2026-10-05. Task ownership: CA-05/06. Execution status lives only in the root master tracker. This document details [multinational requirements](Company-Multinational-Requirements.md) and extends the existing company phases; it does not introduce a parallel implementation.

## Dependencies and owners

Foundation A; before persistent workflow promotion.

Responsible implementation areas: TheTechIdea.Data, Repository, API request pipeline, module setup/bindings, typed clients and role transformation. Inventory existing entity/service/API/client/UI/test owners before adding contracts. Shared business contracts belong in TheTechIdea.Data; clients use APIs; IdentityServer authenticates and app RBAC supplies standard roles. Follow applicable repository skills before code edits.

## Implementation sequence

1. Inventory TenantId, company/business-associate records, memberships, role assignments, field ownership and connection routing. Record one authoritative owner for each; nullable identity TenantId is insufficient enforcement.
2. Define customer tenant, group, legal entity, operating unit and asset relations. Preserve explicit joint-venture interests and scoped operator grants.
3. Resolve immutable request context from authenticated membership. Validate selected company/legal entity against membership on every API request. Do not accept arbitrary headers or persona selection as authority.
4. Implement company switch through the existing client/API role bridge. Clear previous company selections and cached results; refresh role claims and asset grants. Handle concurrent tabs without shared mutable company state.
5. Bind repositories, background work, caches, file storage, exports, diagnostics and hub groups to validated tenant context. Scope uniqueness/idempotency by tenant and applicable legal entity.
6. Provision new tenants and module connections through the active setup orchestration. Migrate existing records only with explicit ownership evidence; reject ambiguous/unowned data rather than default to a company.

## Verification and exit gate

- Two tenants with identical IDs cannot read/write each other's assets, files, reports or diagnostics; test direct HTTP calls and forged company context.
- Switch and revocation tests cover concurrent browser tabs, circuits, delayed requests and queued jobs. Delayed responses cannot populate the new company workspace.
- Negative tests cover repository reads, batch/import/export, search, cache collisions, SignalR, signed downloads and support access.
- A failed provisioning step is recoverable without exposing another tenant's connection or granting default authority.

## Delivery and recovery

Split implementation into reviewable changes following the sequence above. Each change records chosen existing owners, migration impacts, success/failure behavior and relevant evidence. Backfill and schema changes require preflight ownership validation, resumable execution and post-migration reconciliation. No compatibility copies, guessed business defaults or duplicate stores. Keep unavailable/error/conflict states explicit through the typed client and UI.

After each slice update the root tracker with commit/environment, checks executed and remaining provider/browser/domain gates. Source-only implementation stays unverified until its acceptance evidence passes. Launch-market and topology discovery inputs are recorded rather than silently assumed.

