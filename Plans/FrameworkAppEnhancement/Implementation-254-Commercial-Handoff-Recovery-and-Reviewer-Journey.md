# Implementation 254 — Commercial handoff recovery and reviewer journey

## Recover the exact version's recorded handoff

The commercial amendment workspace now discovers an existing handoff through ApiService before exposing workflow creation. The Data-owned CompanyCommercialHandoffReview binds the selected company/account, contract and exact preparation digest to the current process binding, complete candidate-set digest and optional original start/process revision/status. Confirmed absence is explicit; an unsuccessful read is not absence.

The API repeats current standard role, Accounting.View, selected company/account, process-service and binding checks around two native captures. It validates the current commercial preparation/source and enumerates lifecycle processes for the exact SALES_CONTRACT. Explicit foreign-company and FIELD-owned processes are excluded; malformed company ownership refuses discovery. Matching company-owned starts must retain their original identity, binding, source preparer and exact preparation revision. Their full owned process read must equal the enumerated candidate. Multiple matching handoffs refuse reconciliation rather than choosing one by date. Both candidate sets and source snapshots must remain identical between captures.

The existing native GetProcessInstancesForEntityAsync reader now refuses malformed, duplicate, unavailable or identity-changing listed process rows. It previously skipped rows that could not be loaded, which could incorrectly report absence. This uses existing Beep repositories and the native lifecycle reader; it introduces no database schema or alternate persistence owner. Captures are sequential and do not establish transactional consistency between PRODUCTION and LIFECYCLE.

The typed client freezes the query, checks the original company/account generation after HTTP and validates/copies the response. The circuit state discovers after exact native amendment confirmation and after reconstructing a committed amendment from its process-linked native history. Late context replies cannot publish a handoff. Pending amendment confirmation remains distinct from discovery: a failed discovery cannot permit a replacement proposal or a new handoff.

## Persona screens and independent decision

The original preparer's amendment panel shows the recorded workflow status and an Open recorded amendment handoff link when discovery finds the exact version's process. Only confirmed absence plus an undecided amendment and Workflow.Start exposes the existing configured handoff panel. Failed discovery shows a reload warning. No global FIELD mutation, direct client database access, custom CSS/JavaScript or parallel business contract is introduced.

The focused form journeys exercise the actual company RBAC/profile repository and native COMMERCIAL task router. After original-preparer rejection review, explicit amended terms and configured handoff, they reload the workspace, reconstruct a fresh state and confirm the same recorded process without another write. Duplicate native rows and malformed ownership refuse discovery. The separate reviewer resolves the new-version task through the actual persona inbox, opens its rendered process/terms screen, records an independent approval, reviews the current step and completes it. The amended task disappears; the original rejected process retains its captured version and decision, while the amended process retains its separate approval.

The existing uncertain commit journey still retains the original amendment intent until exact native history read-back. Fresh-state recovery resolves committed history and an existing handoff; durable recovery of an unsaved or uncertain command across circuit loss remains open.

## Focused verification

Canonical Data builds with zero warnings/errors. Final focused source/Razor/local HTTP gate passes 355 tests with zero failures/skips in one minute, with terminal exit 0. The commercial checkpoint passes all 35 cases with zero failures/skips in 55 seconds; this overlaps the final gate and is not added to its count. Existing focused dependency/source-composition warnings remain. Reproduce through Scripts/Test-PurchaseOrderOwnership.ps1 with the canonical Data DLL, IncludeWorkflowInbox and FieldRevenueSourceTests|CompanyProcessStart|CompanyStep|PersonaInbox. Temporary logs are oilgas-254-data.log and oilgas-254-final-source.log.

The first combined checkpoint passed 353 tests and failed two rendered link assertions because the harness stopped waiting when the native record appeared, before handoff discovery and replacement-component rendering finished. The harness now waits for record, handoff and operation completion before rendering the current component. This checkpoint is retained as failed evidence; it is not the final acceptance result.

These tests use native source fixtures, local HTTP, rendered Razor and the existing EF app Identity/RBAC fixture. They are not browser/OIDC or real provider acceptance. No handwritten SQL or new business EF migration lane was added; Beep Migration remains the business schema owner. Full solution checks, provider/isolation/concurrency, browser/keyboard/accessibility/RTL and multinational acceptance remain open.

## Remaining delivery

Continue durable uncertain-command recovery across circuit loss and historical advancement. Extend approved delivery/effective terms to invoice/recognition and ledger read-back, royalty/partner queues and the wider thirteen-persona landing/dashboard/workflow coverage. No CA/FG phase or overall implementation goal closes. No live business write, role grant, commit/push or deployment was performed.
