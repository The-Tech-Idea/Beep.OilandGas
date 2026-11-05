# Localization, regional hosting and tenant lifecycle

Plan date: 2026-10-05. Task ownership: CA-45. Execution status lives only in the root master tracker. This document details [multinational requirements](Company-Multinational-Requirements.md) and extends the existing company phases; it does not introduce a parallel implementation.

## Dependencies and owners

Company release E; isolation before regional deployment.

Responsible implementation areas: Web/shared ThemeBranding, API integrations, setup, diagnostics, storage and deployment tooling. Inventory existing entity/service/API/client/UI/test owners before adding contracts. Shared business contracts belong in TheTechIdea.Data; clients use APIs; IdentityServer authenticates and app RBAC supplies standard roles. Follow applicable repository skills before code edits.

## Implementation sequence

1. Inventory resource strings, documents, date/number parsing and preferences; reuse active localization/theme infrastructure.
2. Separate user language, formatting, timezone and display units from company accounting/jurisdiction settings. Implement Arabic RTL and English LTR pilot using central CSS/JS and Mud palette rules.
3. Define tenant region and permitted transfers for databases, files, diagnostics, backups, analytics and external integrations. Record operator-approved residency constraints before deployment.
4. Provision regional module bindings and credentials through existing setup; keep secrets server-side and ensure jobs resolve only the assigned tenant region.
5. Document tenant migration, backup/restore, offboarding/export, deletion/legal hold, quotas and disaster recovery. Include authorization and data validation for each lifecycle step.
6. Verify mixed-language documents, invariant API serialization and cross-region recovery against approved placement/transfer rules. Central monitoring exposes only authorized metadata.

## Verification and exit gate

- Keyboard, RTL/LTR, localized dates/numbers and mixed-language export checks pass; changing locale cannot change financial meaning.
- Region inventory includes all replicas/backups/logs/attachments and integrations; residency assertions have deployment evidence.
- Restore/offboarding operates on one tenant without exposing or modifying another tenant; migrated connections and jobs retain isolation.
- Operator runbook includes tenant provisioning failure/retry, region outage recovery, secret rotation and approved retention behavior.

## Delivery and recovery

Split implementation into reviewable changes following the sequence above. Each change records chosen existing owners, migration impacts, success/failure behavior and relevant evidence. Backfill and schema changes require preflight ownership validation, resumable execution and post-migration reconciliation. No compatibility copies, guessed business defaults or duplicate stores. Keep unavailable/error/conflict states explicit through the typed client and UI.

After each slice update the root tracker with commit/environment, checks executed and remaining provider/browser/domain gates. Source-only implementation stays unverified until its acceptance evidence passes. Launch-market and topology discovery inputs are recorded rather than silently assumed.

