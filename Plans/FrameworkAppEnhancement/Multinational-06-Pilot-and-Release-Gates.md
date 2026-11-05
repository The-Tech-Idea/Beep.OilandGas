# Multinational pilot and release acceptance

Plan date: 2026-10-05. Task ownership: CA-46. Execution status lives only in the root master tracker. This document details [multinational requirements](Company-Multinational-Requirements.md) and extends the existing company phases; it does not introduce a parallel implementation.

## Dependencies and owners

Company release E; depends on all applicable CA-05/06/15/16/24/45 gates.

Responsible implementation areas: API/Web integration tests, supported-provider suites, domain reviewers and release evidence. Inventory existing entity/service/API/client/UI/test owners before adding contracts. Shared business contracts belong in TheTechIdea.Data; clients use APIs; IdentityServer authenticates and app RBAC supplies standard roles. Follow applicable repository skills before code edits.

## Implementation sequence

1. Define evidence dataset: two independent tenants with colliding local IDs; one tenant group with two legal entities operating in distinct currencies/timezones; distinct memberships and restricted support identity.
2. Run engineer study -> approved intervention -> work order -> production capture -> allocation -> commercial settlement -> journal -> entity close -> consolidation. Add rejection/correction/reopen/recovery paths.
3. Run accountant purchase-to-payment and HSE/permit handoffs; validate attachments, notifications and reports under company switching and permission revocation.
4. Exercise cross-tenant object references, altered headers/URLs, cache collisions, imports/exports, background jobs and diagnostics access. Re-test middleware through actual HTTP, not only controller mocks.
5. Exercise supported-provider races, restart after commit, retry/idempotency, migration and backup/restore. Collect browser evidence in both pilot locales and agreed regional deployment topology.
6. Record commit, environment, commands, outcomes, benchmarks, domain approvals and unresolved gates in the root tracker. Release only capabilities whose entire applicable evidence chain passes.

## Verification and exit gate

- Each promoted role journey has UI, API, persistence, authorization, audit and recovery proof across tenant boundaries.
- Entity and consolidated finance reconciles to reviewed expected amounts; numerical studies retain independent benchmark evidence.
- Residency, regional restore and locale/timezone/unit tests have actual environment evidence.
- No broad completion claim from narrow unit tests; skipped, unsupported and deferred markets/features are explicitly identified.

## Delivery and recovery

Split implementation into reviewable changes following the sequence above. Each change records chosen existing owners, migration impacts, success/failure behavior and relevant evidence. Backfill and schema changes require preflight ownership validation, resumable execution and post-migration reconciliation. No compatibility copies, guessed business defaults or duplicate stores. Keep unavailable/error/conflict states explicit through the typed client and UI.

After each slice update the root tracker with commit/environment, checks executed and remaining provider/browser/domain gates. Source-only implementation stays unverified until its acceptance evidence passes. Launch-market and topology discovery inputs are recorded rather than silently assumed.

