# Implementation 185 — Independent matching-policy commands

Date: 2026-10-07. Persona priority: accountant draft preparation and independent controller review, supporting Feature 04 and Persona 01. The existing company policy owner now supports verified draft/approval through native Beep, independently guarded API and typed accounting client. Submission/reconciliation screens, retained circuit intent, task routing, effective policy selection and variance/financial decisions remain open. No phase closes.

## Canonical command and preparation evidence

TheTechIdea.Data owns CompanyMatchingPolicyCommand, its bound command review/submission/result and stored proof. Commands explicitly carry company/account, policy identity, DRAFT or APPROVE action, full supported rules, UTC effective/expiry dates, original complete history revision, production binding revision, stable intent and reason. Draft identity cannot already exist. Approval requires an active unchanged draft with verified preparation by another account, unchanged rules/dates and no overlapping recorded approved policy for the same currency/effective window. Recorded legacy draft status without proof is insufficient, even with an approval grant.

The existing ACCOUNTING_METHOD gains nullable bounded MATCHING_POLICY_COMMAND_JSON (24000) and an explicit 128-character identity declaration. The existing PRODUCTION Beep MigrationManager entity manifest already owns this type. Read-only legacy history remains available under its prior checks; writes additionally require fresh bounded canonical identity and nullable proof schema. No live migration was applied and no raw SQL or business EF lane is introduced.

Draft proof records the original intent and exact complete prepared row after common-column audit. Approval proof retains the original verified draft and its preparer, then proves only status plus current reviewer/change timestamp changed. Prepared rules, dates and audit cannot be overwritten. Unknown/missing proof fields, null command, changed row/content, another intent, duplicate or foreign identity refuse. This is authoritative recorded application/provider evidence, not cryptographic tamper-proof certification or permission to match an invoice.

## Existing native Beep owner

AccountingPolicyService.RecordCompanyMatchingPolicyAsync freezes original command/history before awaits, requires an independent current-authority callback, captures request-bound PRODUCTION physical source and requires an owned Serializable transaction. It checks schema/global policy identity, refreshes complete company history, reconstructs the original history only for the exact proven recorded intent, and applies the existing PPDM repository with a narrowly constrained insert/update policy after canonical audit handling.

Exact read-back, full history and current authority are checked before transaction release; committed exact row/proof and authority are checked after release. Write/read-back/authority/binding failures roll back. Unknown commit outcomes cannot produce success or create a new intent automatically. Exact retained-command retry can confirm the original row without another write. Draft retry after a different approval intent is not silently reinterpreted; broader command-lifetime/durable reconciliation remains open.

## API and typed client

Existing AP routes add GET matching-policies/command-review?action=DRAFT or APPROVE and POST matching-policies/commands. The prior read-only history route reuses the same capture owner. Standard app-assigned roles and Accounting.View remain mandatory. Explicit Accounting.PrepareMatchingPolicy or Accounting.ApproveMatchingPolicy is independently required for its action; no live grant or role assignment is made.

Bound review captures the original action, company/account, full history and production binding. API write guards recheck original actor/company/binding and unchanged role/permission sets at each native callback, then validate complete returned proof. Forbidden authority remains forbidden; invalid/changed/unknown outcomes return fixed conflicts. The existing IAccountingServiceClient/AccountingServiceClient freeze submissions, retain caller-provided intent, use shared company HTTP boundaries and validate bound review/result. IdentityServer remains authentication-only.

## Focused verification

Actual Data build: zero warnings/errors, 1.81 seconds; log %TEMP%/oilgas-policy-185-data.log.

Final affected gate: 1088 passed, zero failed/skipped, terminal exit 0, 28.9829 seconds; log %TEMP%/oilgas-policy-185-delivery.log. This adds 49 cases to Implementation 184's 1039 (33 native/Data and 16 HTTP/client). Smaller source gate: 519 passed, zero failed/skipped, terminal exit 0, 9.7736 seconds; log %TEMP%/oilgas-policy-185-source.log. Counts overlap. The final affected composition has 350 warnings, including eight additional source/cached-assembly conflicts from compiling the actual writer and fixture, with no new command-source nullable warning. Whitespace checks pass.

Tests verify exact native draft/approval/retries, separate accountant and controller accounts/standard roles/current grants, unchanged prepared rules/audit, self-approval refusal even with both grants, legacy/overlapping-policy refusal, owned transaction/schema, rollback and commit uncertainty, original body/actor/company/history/binding refusal, post-write revocation and complete typed API results. Invoice status stays DRAFT; no allocation, matching, journal or payment command is executed by policy approval.

Checkpoint gates passed 1063 native-expanded checks and 1078 native/HTTP checks. A later overlap test exposed its fixture incorrectly selecting a single record from a two-policy history; selection now uses the original policy identity. Corrected checkpoint passed 1087, followed by final 1088 with the additional actual HTTP self-approval case. Physical datasource/isolation/durability/metadata and authentication remain test seams; full host/DI/OIDC/browser and actual provider/concurrency are unverified.

## Next persona delivery

Wire the existing payables landing/detail workspace to explicit draft preparation and controller approval, with frozen bound review, retained original intent, record/reconcile handlers, unavailable states and current app-role/grant checks. Then connect verified effective policy to accountant variance review and independent controller financial decisions through the existing source/task owners. Approval must not silently choose a policy, convert units/currency, apply tax/rounding defaults or authorize posting/payment.

Policy retirement/correction, automatic policy-specific task routing, durable new-circuit recovery, stock/returns, book/period/journal linkage, broader personas/dashboards, multinational requirements and live release gates remain active. No live provider write/migration/grant, commit, push or deployment occurred.
