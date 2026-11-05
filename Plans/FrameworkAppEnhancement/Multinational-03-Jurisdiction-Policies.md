# Country and jurisdiction policies

Plan date: 2026-10-05. Task ownership: CA-16. Execution status lives only in the root master tracker. This document details [multinational requirements](Company-Multinational-Requirements.md) and extends the existing company phases; it does not introduce a parallel implementation.

## Dependencies and owners

Finance B and governance D; depends on organization scope and launch-market discovery.

Responsible implementation areas: Existing tax, royalty, ownership, invoice, HSE, permit and compliance services; BeepDiA configuration. Inventory existing entity/service/API/client/UI/test owners before adding contracts. Shared business contracts belong in TheTechIdea.Data; clients use APIs; IdentityServer authenticates and app RBAC supplies standard roles. Follow applicable repository skills before code edits.

## Implementation sequence

1. Create launch-market capability matrix: legal entity/asset jurisdiction, required policies, authoritative source, responsible reviewer and supported/unavailable status.
2. Research current official jurisdiction sources during implementation. Capture effective dates, policy versions, expected examples and review evidence; no generic country label establishes compliance.
3. Extend existing services with approved effective-dated policy selection; language, client IP and user locale cannot determine legal jurisdiction.
4. Configure tax/royalty treatment, document numbering, required invoice fields, fiscal rules, permit deadlines and retention where applicable. Maintain separate commercial contract terms.
5. Manage approval, activation and retirement through existing admin/workflow APIs. Prevent changing historical calculations by mutating a previously used policy.
6. Route transactions spanning jurisdictions through explicitly reviewed selection rules; unsupported combinations block affected actions with useful explanations.

## Verification and exit gate

- Reviewed policy examples pass for each promoted market, including effective-date boundaries and unsupported cases.
- Re-running historical reports preserves the originally approved policy and source inputs.
- Restricted policy editing, dual review where required, activation and audit are verified via direct API and UI.
- Country-specific outputs and retention behavior require named domain/legal review evidence before release.

## Delivery and recovery

Split implementation into reviewable changes following the sequence above. Each change records chosen existing owners, migration impacts, success/failure behavior and relevant evidence. Backfill and schema changes require preflight ownership validation, resumable execution and post-migration reconciliation. No compatibility copies, guessed business defaults or duplicate stores. Keep unavailable/error/conflict states explicit through the typed client and UI.

After each slice update the root tracker with commit/environment, checks executed and remaining provider/browser/domain gates. Source-only implementation stays unverified until its acceptance evidence passes. Launch-market and topology discovery inputs are recorded rather than silently assumed.

