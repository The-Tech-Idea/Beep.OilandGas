# Implementation 255 — Durable commercial amendment intent API

## Preserve the reviewed proposal before changing terms

COMMERCIAL_TERMS_INTENT is a Data-owned, append-only PRODUCTION entity. It retains the complete CompanyCommercialAmendmentSubmission: original company/account, closed process and binding revisions, rejected predecessor/native proof, fresh revenue/source review, explicit new effective terms, reason and original UUID. Company, process, contract, command and immutable UTC recording audit are bound by a canonical entry digest. Restore refuses changed identity, audit, inactive status, malformed/noncanonical JSON, invalid command or digest. Evidence is copied before returning it.

This separate record preserves the original lifecycle process revision and native contract history. It does not change terms, approve a version, create a handoff or complete a task. It provides the durable owner needed to recover an uncertain proposal when the previous circuit's memory is gone. The new schema is declared in ProductionAccountingModuleSetup for the existing Beep migration path; no handwritten SQL or domain EF migration is introduced. Provisioning through the reviewed Beep migration remains a deployment prerequisite, and no live migration was performed.

## Native journal owner

The production owner verifies complete nonnullable bounded string metadata and one native primary key. It reads the complete company/account/process intent set twice and refuses unavailable, malformed, duplicate or changing rows. A read may confirm absence; it never truncates or chooses one row by date. Identity collision checks include the complete native UUID query.

Saving requires an owned Serializable Beep transaction, stable datasource/current authority, exact native rejected predecessor and the same normalized fresh source review. An existing exact command returns the original entry without another insert; another proposal for that process refuses. The repository mutation policy permits only insertion of the exact frozen entry, restores its immutable UTC audit and grants no generic reads, updates or deletes. Full native read-back is checked before commit and again afterward. An uncertain commit must be reconciled using the original process rather than replaced.

The native commercial amendment writer also checks any saved process-linked proposal during its owned reads. A competing amendment cannot bypass the journal through the existing mutation endpoint. At this checkpoint, an absent journal entry still permits the existing amendment flow; mandatory saving and fresh-circuit workspace adoption are the next integration step. The journal does not introduce a second execution owner.

## API and typed client

GET and POST /api/accounting/processes/{id}/commercial-amendment-intent use the existing company process controller. Saving verifies the exact closed original preparer process, rejected version and full lifecycle revision before and after the production operation. Recovery resolves the captured original rejected version, validates the saved command against its current original process/binding and repeats the closed-process/current-authority checks. Current standard roles, Accounting.View, ProductionAccounting.Adjust, selected company/account and inherited entitled FIELD WRITE are independently enforced. Production transaction guards capture authority/binding only; they do not read LIFECYCLE repositories inside a PRODUCTION transaction.

Recovery uses a Data-owned CompanyCommercialAmendmentIntentReview envelope. Its explicit optional entry distinguishes a confirmed missing intent from a missing response. The typed client freezes commands, checks original company/account generations and validates/copies the original restored command; missing/foreign/late evidence does not become a new proposal. A returned saved command is evidence, not permission to execute it against changed source.

Source and lifecycle verification remain sequential across module boundaries. Actual provider phantom protection, simultaneous staging/submission, transaction isolation and binding concurrency require provider acceptance. The bounded primary key prevents duplicate UUIDs; the complete per-process query and Serializable transaction have fixture evidence, not a claim of verified provider uniqueness under concurrent clients.

## Beep migration boundary correction

The existing module boundary rejected every type in TheTechIdea.Data.OilGas, including canonical business entities already declared in PRODUCTION. It now admits only keyed ModelEntityBase business entities from that canonical namespace. Repository Identity/RBAC types, standard Identity namespaces and canonical read contracts remain excluded. This permits the new commercial journal and existing stock/link evidence to use their Beep schema owner without moving app RBAC out of its EF repository. The existing native metadata verification gate remains unchanged.

## Focused evidence

Canonical Data builds with zero warnings/errors. Thirteen native journal cases pass: exact save/retry/recovery, uncertain commit, competing proposal, fresh-source drift, missing authority, mid-write revocation/rollback, incomplete schema, write failure, duplicate/tampered/foreign rows and borrowed/wrong-isolation refusal. They verify the original contract is unchanged and retries add no insert.

Seven local HTTP/SDK journeys exercise actual native commercial and closed lifecycle evidence. They verify confirmed absence, exact save/read/retry, uncertain save commit, recovered original UUID, preserved process/contract, competing save and mutation denial, recovery after later revenue drift and after amendment execution, and current role/grant/FIELD/process/source denials. The source checkpoint passes 218 tests with zero failures/skips in 59 seconds. Nine migration-boundary/metadata cases pass and protect repository identities and read contracts. These counts overlap the final affected gate and must not be summed.

The initial native checkpoint failed in setup because source maker identities were absent; the fixture now supplies recorded source provenance. The first HTTP checkpoint passed 211 tests and failed seven absence reads because MVC emitted an empty response for null. The explicit recovery envelope fixes that boundary; those failed checkpoints are retained as evidence, not reported as acceptance.

Final affected source/Razor/local HTTP/native/migration-metadata gate passes 394 tests with zero failures/skips in one minute fifteen seconds, terminal exit 0. Reproduce using Scripts/Test-PurchaseOrderOwnership.ps1, the canonical Data DLL and IncludeWorkflowInbox, with FieldRevenueSourceTests|CompanyProcessStart|CompanyStep|PersonaInbox|CommercialIntentMigration|ModuleSchemaVerification. Temporary logs: oilgas-255-data.log, oilgas-255-native.log, oilgas-255-migration.log and oilgas-255-final-source.log. Dependency/source-composition warnings remain; full solution, real provider and browser/OIDC acceptance are not claimed.

## Remaining persona delivery

Connect CommercialAmendmentWorkspaceState and its existing preparer screen to save before execution, recover the exact saved submission after a fresh circuit, preserve source drift as a blocked original proposal, and reconcile a committed historical amendment without replay. Verify the actual rendered preparer recovery/retry and reviewer/handoff journeys. Continue approved delivery/effective terms to invoice/recognition and ledger read-back, royalty/partner queues and the wider thirteen-persona landing/dashboard/workflows and multinational gates.

No CA/FG phase or overall goal closes. No live business write, migration application, role grant, commit/push or deployment was performed. Browser reconnect/keyboard/accessibility/RTL, provider concurrency/isolation and full multinational acceptance remain open.
