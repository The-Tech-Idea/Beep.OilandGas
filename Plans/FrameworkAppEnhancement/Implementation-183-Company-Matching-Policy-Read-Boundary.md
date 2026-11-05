# Implementation 183 — Company matching-policy read boundary

Date: 2026-10-07. Persona priority: accountant preparation and independent controller review; prerequisite to Feature 04 purchase-to-pay and Persona 01. Partial delivery: native read boundary is verified; policy API, typed client, screen and independent approval remain open. No phase closes.

## Existing owner and canonical data

AccountingPolicyService already owns ACCOUNTING_METHOD and effective-dated accounting configuration. This slice extends that owner rather than adding a parallel matching-policy store. ACCOUNTING_METHOD moves from Models to TheTechIdea.Data with its existing namespace, inherited audit fields and notifying properties preserved. Its identity now has the Key declaration. Nullable bounded OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, MATCHING_POLICY_JSON and MATCHING_POLICY_STATUS allow explicit company-owned matching records without inventing ownership or rules for legacy records.

The existing PRODUCTION module entity list declares ACCOUNTING_METHOD for Beep MigrationManager. No migration was applied to a live provider; provider schema must be reviewed before the read can succeed. No handwritten SQL or business EF lane is added.

## Explicit evidence for accountant/controller review

CompanyMatchingPolicyRules requires an explicit bounded version, currency and both quantity/price percentage tolerances (0 through 100). Missing tolerance cannot default to zero. Currently supported rules require exact recorded units and currency, exact source amounts, and separate review for tax and non-PO invoices. Conversion, implicit rounding, automatic tax treatment and non-PO permission cannot be inferred. Unknown JSON rule fields refuse instead of silently discarding unsupported options.

CompanyMatchingPolicyReview freezes all company matching records, including inactive history. It checks exact company, AP_MATCHING method, distinct bounded identities, company-level scope, known activity/status, UTC effective/expiry dates, complete rules, content revision and provenance. Empty history remains explicit. Content revision is stable across independent readers; each response still validates its explicit current account separately. Recorded APPROVED status alone is not independent approval evidence and must not authorize invoice matching, posting or payment.

## Native Beep read

AccountingPolicyService.GetCompanyMatchingPoliciesAsync requires the request-bound PRODUCTION resolver. It captures the physical source identity, checks binding before and after the read, validates the fresh unique identity and four nullable typed/bounded schema columns, and queries exact tenant/company/AP_MATCHING filters through the existing PPDM repository. Missing/foreign/duplicate/incompatible source evidence refuses. It does not select a newest active row or supply global-connection defaults.

The old companyless RecordPolicyChangeAsync and GetActivePolicyAsync now refuse AP_MATCHING, preventing that legacy lane from bypassing the company policy workflow. Other existing policy methods remain outside this slice. No new write or independent approval command is implemented.

## Focused verification

Actual Data build: zero warnings/errors, 1.69 seconds; log %TEMP%/oilgas-policy-183-data.log.

Affected native/API/client/state/rendering/RBAC composition: 1014 passed, zero failed/skipped, terminal exit 0, 35.1470 seconds; log %TEMP%/oilgas-policy-183-final.log. This adds 28 policy cases to the previous 986. The smaller source composition passes 519 checks, zero failed/skipped, terminal exit 0, 9.3799 seconds; log %TEMP%/oilgas-policy-183-source.log. Counts overlap.

New cases cover frozen complete company history, explicit empty history, foreign/duplicate/invalid records, missing/ambiguous/undersized/nonnullable schema, explicit supported rules, unsupported JSON fields, reader-independent revisions with account/content refusal, blocked legacy matching calls, missing binding and binding changes before/after reads. Tests exercise the actual AccountingPolicyService and native Beep repository with mocked physical datasource/metadata seams; no provider certification is implied.

The affected composition has 342 warnings, including three additional existing-source/cached-assembly type conflicts from compiling the policy owner and its fixture. The test nullable warning found in the first successful checkpoint was fixed. An initial composition failed compilation because the new fixture lacked its defaults-repository import; the import was fixed. Checkpoint then passed 1012 checks; the final run includes two additional binding-change cases. Whitespace checks pass.

## Next persona delivery

Expose this same company-bound policy evidence through independently authorized API, existing typed accounting client and accountant/controller workspace, with explicit loading/empty/unavailable/recorded-status states. Add independently authorized effective policy creation/review/approval and evidence-bound variance/financial decisions through the existing source/task owners. Do not replace current unavailable tolerance/financial-authority diagnostics with recorded status alone.

Corrections/returns, stock, book/period/journal linkage, financial commands, durable recovery, receipt-specific workflow policy, routing/delegation, other dashboards/personas and multinational requirements remain active. Live migration, provider concurrency, full host/DI/OIDC/browser/RTL/accessibility and complete purchase-to-pay acceptance are unverified. No live grant, commit, push or deployment was performed.
