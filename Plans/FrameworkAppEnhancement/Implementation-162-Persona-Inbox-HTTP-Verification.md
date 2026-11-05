# Implementation 162: Persona inbox HTTP verification

## Company role to assigned work to recorded source

The complete WorkflowTasksController now compiles and runs in the affected focused gate. Actual RepositoryDbContext mappings, RepositoryAccessService and RepositoryPersonaService resolve active company membership, standard AspNetUserRoles plus approved company-scoped role metadata, role/workspace mappings and the current persona from SQLite. The same loopback Kestrel host uses the complete company accounting process controller, shared process/native read boundary and actual native AP/PO services.

Accountant/AP and buyer/purchase-order cases load the matching persona inbox for two companies with colliding local task/source identities, follow the exact encoded process link and confirm the recorded owned source identity. Counts and the filtered task endpoint use the same reviewed assignments. FIELD cases retain initial and closing FIELD/entity READ admission and perform no accounting native read.

The existing CrossPersonaTaskRouter interface moves unchanged into ICrossPersonaTaskRouter.cs so focused HTTP composition can use the actual contract without compiling the unrelated task writer. Its namespace, methods and owning assembly stay the same; no parallel router contract is added. The task model source is compiled unchanged with the current Data base explicitly selected in the focused gate.

## Failed read behavior

WorkflowTasksController wraps its existing evidence read for all three endpoints. Expected authorization failures are reported and return 403. Invalid operation/evidence changes are reported and return a fixed 409 response asking the user to reload company access and the inbox. A failed read never becomes an empty 200 queue or partial counts, and provider/task exception text is not sent to the user. Existing initial workspace denial reporting remains in place. Cancellation is not swallowed.

## Focused evidence

`Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath <current Data DLL> -IncludeWorkflowInbox` passes **505 tests**, zero failed/skipped, terminal exit 0, in 7.2145 seconds. Log: `%TEMP%/oilgas-workflow-inbox-http-162.log`. The existing 261 dependency/source-precedence warnings remain; the new controller, repository composition and tests add no reported compiler warning or error. Whitespace verification passes.

Eighteen new complete-controller HTTP cases extend the previous 487 affected checks. The gate now includes 81 HTTP cases overall. New cases cover accountant and buyer workspaces, two-company ownership with encoded process identities, anonymous and mismatched persona denial before task reads, expired company membership and missing scoped role despite a retained standard role, absent standard role, foreign process/native ownership, changed/deleted-from-pending task evidence, closing role revocation, initial/closing FIELD access, counts/list projection and closing company required-role/actor/step-completion changes. No native financial/task write or transaction is invoked.

The first added HTTP run passed all prior 487 checks but failed all fifteen initial inbox cases because the test company membership omitted its required grant actor FK. The fixture now records its actor and uses membership expiry to exercise denial without violating FKs. The corrected fifteen-case checkpoint passed 502 checks; three additional closing-assignment cases produce the final 505 passing gate. A stale test namespace import was removed before the HTTP execution gate. Production FK constraints and business rules were not relaxed.

## Exact verification boundary

WORKFLOW_INBOX_FOCUSED explicitly includes test-only resolved party_id account and failure-reporting interfaces because the current shared identity/diagnostics contracts are unavailable in this checkout. The controller source is compiled unchanged; no source-body replacement or missing production project is created. These seams do not certify the production ActingUser/PartyIdClaims contract, OIDC, claims transformation, diagnostics persistence or full integrated application. Native process and task persistence transports use explicit mocks; repository company/persona/role queries and mappings use actual SQLite EF code.

No whole Web parent/client assembly, production module/context/DI pipeline, browser/RTL/keyboard/accessibility, live native provider, task-writer binding, durable producer ownership or atomic task/process/native snapshot is certified. The prior 55 inbox state/panel checks remain the unchanged UI evidence and are not summed with this gate. Broader Models/Data consolidation also remains open.

## Next visible delivery

Verify the typed inbox client and full landing/inbox-to-process-to-source composition with current company selection and standard role resolution. Complete durable company process/task creation and reviewer assignment, exact invoice-preparer separation, acceptance/company policy/concurrent consumption and reviewed submission/recovery. Approval/post/payment, production/maintenance and other persona journeys and all multinational requirements remain active. Beep MigrationManager owns business schema changes; no SQL, business EF lane, live migration, grant or financial command is introduced. No phase closes.
