# Implementation 259 — Commercial landing and multiple-version recovery journey

## Persona workflow evidence

The existing commercial local HTTP/typed-client/native/rendered journey now continues through three real lifecycle processes on one company-owned sales contract. It uses the current canonical service/API/workspace owners, configured approval/rejection-review definition, app RBAC/profile fixture and COMMERCIAL inbox. Synthetic native paging tests from implementation 258 are no longer the only evidence for older-version landing recovery.

1. The first version is independently rejected and its original preparer closes rejection review.
2. The preparer's rendered amendment form saves and records the distinct second version, then creates its separate configured handoff. The existing retained intent and exact source/native origin rules remain enforced.
3. The separate reviewer follows the second process from the COMMERCIAL inbox, records an independent rejection and completes the review step. The process remains in progress and returns its rejection-review task to the original preparer.
4. The shared-role reviewer can see the returned task as COMMERCIAL_PREPARER_REQUIRED but cannot complete it through the API. The original preparer receives the same task as ELIGIBLE, completes the rendered rejection-review step and records REJECTION_REVIEWED.
5. The preparer enters explicit third-version terms through the rendered form, saves and records another distinct proposal, and creates a third configured handoff. Both previous versions and their rejection decisions remain in native history.
6. The separate reviewer opens the third process from assigned work, records approval and completes its task. The current reviewer inbox has no remaining task.
7. The original preparer's saved landing page reports both journals as recorded. All proposals on this page exposes the actual generated links for the two original closed rejection-review processes.
8. A fresh provider/workspace follows the first landing link after current native revenue changes again. It recovers the exact second-version saved UUID and its independent rejection from older native history, with the completed second process as its existing handoff. It offers no new proposal, retry or approval control.
9. With the current source restored, another fresh provider/workspace follows the second link and recovers the approved third-version saved UUID and completed third handoff. Both business and lifecycle row graphs remain byte-for-byte unchanged by discovery and recovery.

Business writes total eight: first preparation/decision, second journal/preparation/decision, third journal/preparation/decision. Recovery and blocked reviewer completion add no business write or lifecycle mutation. Global FIELD selection remains unchanged. Both prior native proof revisions, decisions, distinct preparation identities and process histories are checked against the original records.

## Landing outcome presentation

The saved-proposal table now shows Independent decision: Reject, Approve or Pending for a preparation confirmed in native version history. The decision comes from the canonical row's validated recorded evidence. It does not infer approval from journal existence or a completed unrelated process. Pending means no recorded independent decision on that preparation; it does not claim billing readiness.

Existing unresolved/recorded page filters and original-process links remain in use. No new persistence, model, authorization mechanism, CSS, JavaScript or page-local theme is introduced. No SQL, schema or EF business migration is added. The existing Beep journal and native commercial version history remain the only business owners.

## Focused verification

The first journey checkpoint passed 42 cases and failed the new case because the test assumed a shared-role reviewer inbox would be empty after returning the second rejection. Current behavior correctly exposes the returned task as blocked with COMMERCIAL_PREPARER_REQUIRED. The corrected test asserts this state and verifies that original-preparer work cannot be adopted through the API. Failed evidence is preserved; it is not acceptance.

The corrected 43-case journey gate passes with zero failures/skips in one minute seven seconds, terminal exit 0. The expanded affected source/Razor/local HTTP/native/migration-metadata gate, including direct reviewer refusal and both fresh landing links, passes 447 cases, zero failures/skips, in one minute thirty seconds, terminal exit 0. This checkpoint preceded the independent-decision table caption. The final UI-source affected gate passes all 447 cases, zero failures/skips, in one minute thirty-nine seconds, terminal exit 0, including the actual landing decision captions and both fresh link recoveries.

Canonical Data is unchanged from implementation 258's clean build. Reproduce with Scripts/Test-PurchaseOrderOwnership.ps1, canonical Data DLL, IncludeWorkflowInbox and FieldRevenueSourceTests|CompanyProcessStart|CompanyStep|PersonaInbox|CommercialIntentMigration|ModuleSchemaVerification. Logs: oilgas-259-journey.log, oilgas-259-corrected-journey.log, oilgas-259-final-source.log and oilgas-259-final-ui-source.log. Scopes overlap and are not summed; focused dependency/source warnings remain.

This is automated current-source evidence using local Kestrel, actual typed clients/native command and lifecycle owners, rendered component handlers/form bindings, app RBAC/profile fixtures and mock Beep datasources. It does not prove browser clicks, OIDC/circuit reconnect, production roles, physical database isolation/locking or cross-page snapshots. The fresh provider is a new scoped workspace, not a browser acceptance claim.

## Remaining delivery

Continue approved measured delivery and effective commercial terms into invoice/receivable/recognition and ledger read-back through existing owners. Preserve explicit missing quantity/unit/price, dispute, partial settlement, source revisions and independent approvals; missing inputs cannot create a financial success. Continue royalty/partner/project-cost queues, wider thirteen-persona workflows/landing/dashboard and multinational acceptance.

Physical provider paging/order/continuation/concurrency/performance, host/OIDC/browser reconnect, keyboard/screen-reader/responsive/RTL, live Beep provisioning and complete company/multinational acceptance remain open. No CA/FG phase or overall goal closes. Commit and push remain authorized after the full goal and plan pass the completion audit.
