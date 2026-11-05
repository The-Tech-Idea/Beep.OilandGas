# Implementation 256 — Durable commercial preparer screen recovery

## Save before execution on the existing amendment path

CommercialAmendmentWorkspaceState now saves its frozen reviewed submission through the typed intent API before executing an amendment. The returned native journal must restore the exact original command. A company/account/owner/generation change after saving stops execution; the original proposal remains pending and can be recovered from its saved process. Saving and business execution retain separate confirmation rules.

ApiService also saves and validates the exact submission before invoking the existing commercial amendment owner. This makes staging mandatory for the company process mutation endpoint, including callers that do not use the screen. It repeats the closed original process/current-authority checks after staging and before execution. The native writer requires the exact saved proposal during its reads for process-linked amendments; an absent or competing journal refuses mutation. The existing unbound internal native amendment tests keep their original scope; no separate public unbound amendment route is introduced.

An uncertain journal commit does not execute a contract amendment. Recovery first confirms the original saved command, then a fresh source review must still equal its frozen review before retry. The original UUID is reused, and the already saved entry is not inserted again. If contract execution rolls back after staging, the original intent remains durable while the contract retains its predecessor. A failed execution cannot erase that saved proposal or authorize a replacement.

## Fresh circuit adoption and historical reconciliation

Every workspace load first queries the original process's saved intent. It validates original company/account/process ownership and restores the full submission. A pending or confirmed local command must exactly match the saved submission; another saved proposal refuses and preserves the original local intent. A fresh workspace adopts the saved command before reviewing native history. Copies of displayed entries cannot mutate retained evidence.

If current native history contains the exact preparation and amendment origin, the workspace confirms the recorded version without executing or rewriting it. Current independent decision/history remains attached to that version. If the preparation is not recorded, the original rejected predecessor/process and exact fresh source must still match before CanRetry becomes true. Changed revenue or native origin blocks the original proposal instead of replacing it with current source values.

Displayed saved evidence clears on invalidation, reload and failed execution. The state separately remembers a confirmed saved identity so a later missing entry cannot be treated as permission to retry a new proposal. Owner disposal cannot clear another panel's ownership. Company/account/generation guards stop late saving/read replies from publishing or triggering execution.

This supports a proposal that reached the API and was durably saved. A proposal whose save never reached the server still requires the original session; it cannot be reconstructed from absent server evidence. The screen distinguishes this state from a confirmed saved proposal.

## Preparer persona screen

The existing amendment workbench displays a saved-proposal notice, original intent/account and UTC recording audit after current recovery review. It directs the preparer back to the original process after reconnecting. A saved uncertain proposal displays its retained terms/reason and retry control; changed source leaves retry disabled and new-version inputs absent. Confirmed native history retains the existing exact-version handoff recovery and predecessor navigation.

The screen uses the existing MudBlazor components and theme. It adds no CSS, JavaScript, browser storage, direct client database access, replacement business contracts or alternate persistence lane. Durable storage remains the PRODUCTION Beep journal declared in implementation 255. Actual provisioning through that migration remains required; no live migration or business operation was performed.

## Focused verification

The source checkpoint passes all 218 cases with zero failures/skips in 58 seconds after mandatory-stage expectations were corrected. The previous 206-pass/12-fail checkpoint identified old write counts and uncertain/rollback expectations: staging now adds a separate durable insert, and an uncertain save leaves the contract unchanged. Failed evidence is retained and is not counted as acceptance.

The actual rendered preparer journey loses the save commit response, then creates a separate service provider, renderer and fresh workspace without the old in-memory command. It recovers the same saved UUID. Changed native revenue blocks retry and hides replacement inputs; invoking the handler adds no write. Restoring the exact reviewed source permits the fresh screen to retry the original proposal, recording one amendment without another journal insert. The original screen then reconciles the same committed version. Existing configured handoff recovery and the real RBAC/profile/native COMMERCIAL inbox -> independent reviewer screen -> decision -> task completion remain included. Global FIELD selection is unchanged.

The retained-state cases now additionally reconstruct fresh owners from saved commands, preserve uncertain read-back/origin failures, verify copied saved evidence and reconcile before/after-write outcomes without replay. New cases cover a late save response that must not execute, a previously saved entry that disappears, and a different canonical saved proposal. They preserve original pending identity and refuse replacement/retry under changed evidence.

Final affected source/Razor/local HTTP/native/migration-metadata gate passes 397 tests with zero failures/skips in one minute thirty-two seconds, terminal exit 0. The preceding expanded checkpoint passed 394 cases before the three saved-identity/context cases were added; scopes overlap and are not summed. Canonical Data is unchanged from implementation 255's clean build. The affected gate compiles the current Web/Razor/API/native sources and includes source/workflow/inbox plus current migration-boundary/metadata checks. Reproduce with Scripts/Test-PurchaseOrderOwnership.ps1, the canonical Data DLL, IncludeWorkflowInbox and FieldRevenueSourceTests|CompanyProcessStart|CompanyStep|PersonaInbox|CommercialIntentMigration|ModuleSchemaVerification. Temporary log: oilgas-256-final-source.log. Focused dependency/source-composition warnings remain; no full solution/provider/browser result is claimed.

## Remaining delivery

Continue discovery/landing presentation of unresolved commercial proposals and historical advancement across multiple amended versions, then approved delivery/effective terms -> invoice/recognition -> ledger read-back. Continue royalty/partner queues, wider thirteen-persona landing/dashboard/workflows and multinational acceptance. Saved recovery requires current preparer command grants; broader read-only/reassignment/cancellation decisions remain separate requirements, not implicit permission to modify the journal.

Provider concurrency/isolation, actual browser reconnect/OIDC, keyboard/accessibility/RTL, live provisioning and full multinational acceptance remain open. No CA/FG phase or overall goal closes. The user's final-delivery instruction remains in the master tracker: commit and push only after the complete goal and plan pass their completion audit.
