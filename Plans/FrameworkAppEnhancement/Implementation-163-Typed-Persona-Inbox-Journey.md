# Implementation 163: Typed persona inbox journey

## Shared response ownership

The Data-owned WorkflowInboxReview is now the single validation/snapshot owner for a recorded company/persona queue. It requires the original company, actor and persona, explicit task scope and exact internal route, a role from current selected-company authority, valid pending task identities/priorities, unique task identities, exact priority buckets and counts derived from those same rows. It freezes the response before publication.

UnifiedTaskInboxService captures the selected company before transport and applies that contract before returning a queue. Its filtered task method checks the same task/role/route contract and refuses duplicate task identities. Missing selected company refuses before sending; API failures remain failures. PersonaTaskInboxState reuses the shared contract instead of maintaining duplicate response-validation logic and retains its existing company/persona generation checks, clearing, filters and consumer snapshots. The shared PersonaTaskInbox component declares its existing production namespace explicitly for consistent composition.

## Rendered accountant and buyer composition

The affected focused gate now compiles the actual ApiClient, CompanyClientContext/response stream, complete BusinessProcessServiceClient and interface, PersonaClient, PersonaContextService, UnifiedTaskInboxService, inbox state, PersonaTaskInbox component, TaskInbox page and CompanyAccountingProcessDetail page. The actual PersonasController/profile/workspace endpoints and repository role/persona services are included alongside the accounting/process/task controllers and native read services.

For accountant/AP and buyer/purchase-order workspaces, actual typed persona clients resolve the recorded profile and company role/workspace review over Kestrel. The actual inbox client/state and full inbox parent render the recorded counts, assigned role and exact encoded company process link. The actual process client/full parent renders the recorded role/source detail and exact owned invoice/order workbench link. Clearing company selection clears the state and loaded persona. The static renderer does not perform a browser navigation; each screen gets its own render/provider lifetime.

A failed native ownership read is exercised through the actual controller, typed client and full inbox parent. It shows the fixed reload message and displays no successful count or task/process link. These are focused read journeys; task creation, review decisions and financial commands are not exercised or implied.

## Focused evidence

Actual-all-source isolated Data build passes with zero warnings/errors in 1.65 seconds. The expanded affected gate passes **519 tests**, zero failed/skipped, terminal exit 0, in 7.5298 seconds. Log: `%TEMP%/oilgas-persona-client-163.log`. Fourteen new cases extend the prior 505 checks: two native typed/Razor persona compositions, one failed ownership parent render, six typed queue response refusals, two filtered task role/duplicate refusals, no-company/no-transport refusal, late away-and-back company cancellation and explicit API conflict propagation.

The affected gate retains the prior 81 HTTP cases and adds the three typed/parent HTTP compositions. It reports **267 known dependency/source-precedence warnings**: the previous 261 plus six RefusalException/RefusalKind precedence warnings in the newly included complete PersonasController. No new response/state/client/Razor warning is reported. Whitespace verification passes.

The separate actual state/shared assigned-work panel gate passes **55 tests**, zero failed/skipped, terminal exit 0, in 1.1873 seconds, with no reported compiler warning/error. Log: `%TEMP%/oilgas-persona-inbox-state-163.log`. It validates the existing affected company/persona clearing, count/filter, route/role and rendered panel cases against the new shared Data contract. Counts across gates are not summed as a unique regression total.

Initial complete process-client compilation exposed the cached Models/current Data ProcessInstanceRequest collision. The focused composition now explicitly selects the current Data-owned request via its existing actualData alias; the complete client and interface then compile unchanged. This does not close wider Models/Data consolidation.

## Exact limits and next work

The production shared identity/diagnostics libraries remain unavailable. WORKFLOW_INBOX_FOCUSED explicitly supplies resolved-account/reporting, error-sentence reading, reference-format and UI wording fixtures. The real ApiClient and OilGasApiException source compile and run, but production shared error wording/reference policy, OIDC, account resolution and claims transformation are not certified. These seams do not replace production projects or source bodies. Authentication state and module/process/task transports remain explicit fixtures. Company/persona/RBAC queries use actual SQLite EF code; business source reads use the actual native owners with the explicit data-source fixture.

No entire landing selector/dashboard, live browser navigation/RTL/keyboard/accessibility, production context/DI/host, real native provider, task-writer binding, durable producer ownership or atomic task/process/source snapshot is claimed. Broader workflow methods in the compiled process client are not newly exercised by the company read journey.

Next implement the durable company accounting handoff in the existing process/task owners: securely record explicit company/native source binding, reviewed source/revision, actual invoice preparer and independent reviewer, then show recorded handoff/read-back on these screens. Accepted receipt/company policy/concurrent consumption, approval/post/payment, workflow definition/schema reconciliation and other persona/multinational requirements remain open. No SQL, business EF lane, live migration/grant or financial/task write is introduced. Beep MigrationManager remains the business schema owner; no phase closes.
