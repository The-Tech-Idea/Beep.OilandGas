# Implementation 198 - Controller to accountant correction handoff

Date: 2026-10-07. Status: an explicitly configured correction-request workflow can close its controller task and route the pending accountant correction task through the existing native lifecycle owner. Actual correction editing, versioned history and completion remain open; no persona phase is closed.

## Configured request and correction work

The existing StepConfiguration.AccountingEvidence contract now recognizes AP_INVOICE_VARIANCE_CORRECTION_REQUEST for an approval step and AP_INVOICE_VARIANCE_CORRECTION_WORK for a non-approval ACTION step. The existing AP_INVOICE_VARIANCE_DISPOSITION action keeps its prior behavior. These are distinct evidence requirements in the same workflow owner, not parallel stores or role catalogs.

A correction-request step requires a verified original proposal, an independent recorded controller decision of REQUEST_CORRECTION, the recorded controller identity, current configured standard role/action grants and unchanged full source/decision evidence. Missing or accepted dispositions cannot route correction work. The native sequential command requires the request step to be immediately followed by its configured correction-work step, and the correction step to have that predecessor. Unsupported or unpaired definitions refuse before writes.

Successful controller completion closes its exact existing task set, records the existing workflow history/approval and advances to the configured accountant role/persona assignment in the owned Serializable LIFECYCLE transaction. Exact original retry confirms the persisted handoff and adds no tasks or writes. No role name or company assignment is seeded by this feature.

## Accountant ownership and screens

The next correction review requires the original recorded proposal preparer, rather than any account holding the accountant role. Current Workflow.Execute and read grants remain necessary. Another preparer receives ORIGINAL_PREPARER_REQUIRED; the original preparer receives CORRECTION_DATA_REQUIRED while no recorded corrected invoice exists. Missing current action permission is separately refused. Forging eligibility cannot complete the task. The native lifecycle owner also rejects current correction-work completion until recorded correction-command evidence has an implementation, even when invoked with a forged eligible review and a no-op authority callback. The invoice stays DRAFT and the proposal/controller bytes and financial amounts remain intact.

The shared persona inbox/dashboard panel exposes accountant-correction-needed and original-preparer-required counts and drill-downs, with an explanation on each row and its exact process link. Process detail also explains these requirements. Existing canonical inbox/client validation admits the configured non-independent ACTION review but refuses an eligible execution review without correction evidence. The correction task remains visibly pending; this handoff does not claim that correction editing or completion has been implemented.

## Verification

Actual canonical Data builds with zero warnings/errors in 1.86 seconds. The final overlapping source gate passes 519 checks in 8.6413 seconds. Final affected gate passes 1402 checks with zero failures/skips in 2.1698 minutes, including six new correction journey cases and the direct native forged-completion refusal. Composition warnings remain 366. Whitespace verification passes and all command handles reached terminal exit. Gates overlap; these are focused results, not a full solution or live-provider pass. New journey cases exercise actual API/typed client/native task completion and exact retry, next-step/task/role identities, current app RBAC inbox reads, original/other preparer checks, missing grants, accepted/missing disposition and unpaired configuration refusal. The shared panel is statically rendered from actual inbox-backed state to verify correction counts, filters, explanations and process links.

Initial affected gate passed 1397 of 1400 cases. Three correction routes encountered the fixture's strict persona reader without an accountant-role mapping; explicit configured mapping is now supplied. The fixture also uses the canonical workspace-persona claim type. These fixture fixes do not create live roles, mappings or grants.

Provider transactions, configured role/persona mapping and production read authority remain explicit test seams. PRODUCTION evidence reads stay outside the LIFECYCLE transaction; no cross-database atomicity is claimed. Browser/OIDC/accessibility, live Beep schema/provider and deployment remain unverified. No SQL, new business EF lane or live migration/grant was added.

## Next work

Implement the original accountant's correction editor and native versioned correction command/history, preserving original proposal and controller request, independent rereview, retained uncertain intent and source/contributor evidence. Only that confirmed correction may make the correction task completable. Workflow definition provisioning, cancellation/history/lifetime recovery, tax/non-PO/book/period/journal evidence, policy tasks, stock/returns, broader personas/dashboards and multinational acceptance remain open.

Checkpoint: 1402 affected checks passed in 2.3475 minutes before the final native correction-completion guard and its direct refusal assertion. Two test diagnostics (nullable route and Assert.Single filtering) were corrected; final composition warnings are back to the existing 366. That checkpoint is not the final source claim.
