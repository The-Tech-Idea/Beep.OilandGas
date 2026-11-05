# Implementation 167 — API-reviewed company handoff and typed client

Date: 2026-10-06. Status: company review/submission and typed client HTTP journey pass focused verification. Source-screen controls, workflow selection and durable recovery UI remain open.

## API and canonical contracts

The existing CompanyAccountingProcessController and CompanyAccountingProcessReads now expose POST start-review and POST start. Query/review/intent/receipt contracts remain in TheTechIdea.Data. Review captures the original company, resolved actor and combined PRODUCTION/LIFECYCLE binding, then checks fresh app-owned standard roles plus Workflow.Start and the source-specific Accounting.View or Procurement.View permission. IdentityServer remains authentication-only.

The actual AP/PO source readers supply explicit owned header/line evidence, source revision and recorded creation audit. An optional ExpectedSourceRevision binds review to the source currently displayed on the screen. The active definition must match the exact source type; the recorded workflow name/version/initial assigned role and definition revision accompany the review. Two complete captures must agree. No frontend assertion establishes source ownership, maker identity, role or revision.

The API requires complete consistent creation audit for the header and all lines, with no recorded changed actor/date. Missing audit, differing/unknown line makers, the SYSTEM default identity, inactive/foreign sources and edited source evidence refuse review. The SourcePreparerUserId field records this initial source creation actor. Creation audit does not prove all historical financial preparation or future source edits; those require their own reviewed command/action evidence. Edited/ambiguous sources remain blocked for reconciliation rather than inferring a preparer.

## Write authority and transaction boundaries

Inspection confirmed UserAssetAccessService accepts FIELD/WELL/POOL/FACILITY/WORK_ORDER and rejects AP_INVOICE/PURCHASE_ORDER; it has no reviewed AP/PO WRITE grant path. This handoff creates LIFECYCLE workflow records and does not modify the source transaction. Accordingly, Workflow.Start is its app-owned write permission, combined with exact owned source READ review. The previous tracker's proposed native AP/PO WRITE check is superseded by this explicit boundary. No unsupported resource grant, parallel authorization lane or automatic permission grant is introduced. Later invoice/order mutations and financial acceptance still require their own command authority.

Submission is frozen and must match the original company/actor/binding and fresh API source/definition/maker review before mutation. The bound existing native command creates process/steps/history/persona tasks atomically. Inside its owned LIFECYCLE transaction, the API callback rechecks original company/binding/app-role/permission authority without crossing to PRODUCTION. Authority loss rolls back the complete handoff.

After native commit, the receipt must match the retained intent, and the API repeats full PRODUCTION source/definition review and current authority before returning success. Source drift after commit returns an unconfirmed outcome; the recorded handoff can already exist. The original intent must be retained and reconciled. This is not an atomic PRODUCTION/LIFECYCLE source snapshot. Financial approval, posting or payment is not performed.

## Typed Web consumption

The complete existing BusinessProcessServiceClient and interface now review and submit company handoffs through these endpoints. Review validates exact company/actor/workflow/source and optional displayed revision. Submission sends one frozen intent, validates the receipt's exact original company/actor/source/instance/task identities and discards late company results. Clearing/changing company cancels outstanding transport; there is no automatic retry or intent regeneration.

An actual typed-client-to-HTTP case reviews the source, submits the native handoff, reads its recorded process/history and confirms unchanged retry without extra writes. A separate post-commit company-change case returns no successful client receipt while the recorded six-row handoff exists. Source-screen selection, pending intent state and cross-navigation/session recovery are the next visible work; no complete submission screen is claimed.

## Focused evidence

Actual Data build: zero warnings/errors, 1.72 seconds. Log: %TEMP%/oilgas-start-api-data-167-final.log.

Final actual-source affected gate: Scripts/Test-PurchaseOrderOwnership.ps1 with the actual Data assembly and -IncludeWorkflowInbox. **655 passed, zero failed/skipped, terminal exit 0, 10.9193 seconds**. Log: %TEMP%/oilgas-start-api-167-verified.log. Whitespace verification passes. Expanded composition reports 301 dependency/source-precedence and nullable warnings; additional missing-audit fixture assignments intentionally exercise unavailable evidence. No new API/client compiler errors or warnings are reported.

There are 25 new HTTP/typed-client cases. They cover AP and PO review/create/read-back/unchanged retry; authentication/standard-role/start/source permissions; missing creator/date/line maker, edited/default SYSTEM/foreign source or mismatched definition; changed request company/actor/binding/source/definition/preparer assertions; authority loss inside the transaction; source drift after commit and retained-intent reconciliation; displayed revision drift; actual typed client reads; and post-commit company cancellation.

The first expanded checkpoint passed 644 and failed three existing binding-change tests (647 total). The optional request-fixture callback accidentally skipped incrementing its counter when absent; the counter now increments independently. Subsequent 649/652/654 checkpoints passed and are superseded by the final 655-case source; counts are not added across gates.

The gate compiles/runs actual Data contracts, API controller/service, AP/PO owners, native process/task/transaction owners and complete typed clients. Current company access/binding and resolved account/reporting seams remain explicit fixtures; existing SQLite app RBAC/configuration checks run elsewhere in this affected composition. Physical native provider transactions/records are mocks. Full production context/DI/host/OIDC, provider locking/concurrency/durability, browser/RTL/accessibility and whole-solution verification remain open.

## Next persona delivery

Expose available recorded company accounting workflow definitions for source selection, then connect the existing payables/procurement detail screens to review and submission. Show recorded company/source revision, initial source maker, workflow/version/assigned role, frozen pending intent, confirmed process/task links and unconfirmed reconciliation state. Preserve pending outcomes on company/source changes and show recorded read-back on landing queues/process detail/dashboard drill-downs.

Company actions and fresh financial preparer/reviewer evidence, receipt/allocation acceptance policy, concurrent consumption, approval/post/payment, progressed-process recovery/outbox, stored-definition/schema reconciliation and all other persona/multinational requirements remain open. No live database write, migration, grant or financial action was performed. Beep MigrationManager remains the business schema owner; no SQL, new business EF lane or phase closure.
