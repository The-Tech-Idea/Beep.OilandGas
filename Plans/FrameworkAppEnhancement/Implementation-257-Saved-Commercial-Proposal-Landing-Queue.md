# Implementation 257 — Saved commercial proposal landing queue

## Company preparer discovery

The revenue landing page now includes My saved commercial proposals. The component reads the existing company accounting process API through its typed client; it needs no retained process link or old in-memory command. Each saved proposal links to its original closed rejection-review process, where the existing amendment workspace recovers the frozen submission and reviews current evidence before any retry.

The default queue shows unresolved proposals. All saved proposals also exposes entries already recorded in native version history. Discovery itself never executes, creates a handoff or enables a retry. Native recording is distinct from approval, billing readiness and ledger posting.

## Existing owners and canonical evidence

ProductionAccountingService discovers the original tenant/company/account journal using the existing Beep repository and metadata checks. It validates every returned command/audit/digest, rejects duplicate process or intent identities, returns detached copies and compares two complete ordered captures under fresh authority and the same datasource. Inactive or malformed owned entries are not silently excluded. Other company/account entries are excluded by explicit ownership filters.

The current complete-set safety envelope is 32 entries and eight million command characters. Exceeding either limit refuses the queue instead of presenting a partial total. This is a bounded first discovery implementation; durable journals grow across completed versions, so provider-backed pagination and continuation review remain required for larger company histories. No deployment-scale claim is made.

ApiService independently requires current standard app roles, Accounting.View and ProductionAccounting.Adjust, original account/company, exact lifecycle/production bindings and unchanged grants. Each discovered row passes the existing saved-intent read and closed original process checks, current FIELD authority and complete historical source review. The entire journal/process/source queue is captured twice; a changed entry, process, native history, role, grant or binding refuses the result. An unconfirmed row prevents a successful partial queue.

TheTechIdea.Data owns the queue and row contracts. A row validates its full saved submission, exact original process/predecessor, preserved native history and amendment origin. An exact recorded preparation is RECORDED, including when an independent later rejection moved it into older history. An unchanged native rejected predecessor with no matching proposal is SAVED_PENDING. A different complete native outcome is RECONCILIATION_REQUIRED. SAVED_PENDING does not certify current source equivalence or permit execution; the existing workspace must freshly review the frozen source before retry.

The SDK validates the canonical complete queue and original company/account snapshot after the HTTP read. Null, foreign, invalid revision/binding and late account responses refuse. No client opens a database. Business persistence and schema remain Beep-owned; this read-only feature adds no migration, SQL, EF business path, grant or live mutation.

## Screen states and context

The MudBlazor component displays loading, empty, denied/error and confirmed states, records retrieval UTC and provides standard buttons/table links. It checks the authenticated subject/account and authoritative selected app roles before reading and again before publishing. Company/account/authentication changes immediately clear evidence and trigger fresh discovery; late replies cannot republish the old queue. It never changes the global FIELD selector. Native outcome labels and proposal dates retain recorded facts instead of fabricating readiness or regional defaults.

No CSS, JavaScript or page-local theme is introduced. Existing native MudBlazor behavior supplies hover/focus/active/disabled presentation. Actual keyboard, screen-reader, responsive, theme/RTL and browser reconnect acceptance remain open; focused HTML rendering is scoped evidence only.

## Focused verification

Canonical Data builds with zero warnings/errors. The first focused checkpoint passed 219 cases and failed two: new authority-denial tests restored the fixture role header with a role name instead of its required yes marker. The queue/recovery checks had already passed; the fixture was corrected. This failed evidence is retained and is not acceptance.

The corrected source/Razor/local HTTP/native checkpoint passes 230 cases, zero failures/skips, in one minute three seconds, terminal exit 0. It includes complete saved discovery, pending and recorded queue rendering through actual HTTP/SDK, original process navigation, no extra writes and current role/grant/FIELD denial. New native cases refuse duplicate, oversized, tampered, drifting, revoked and missing-schema sets; foreign ownership does not enter the queue. Returned copies cannot alter stored entries.

A new native three-version journey saves and records two process-linked amendments, independently rejects the second version, preserves both predecessors and resolves both journals as recorded. A fresh workspace then recovers the older rejected amended version from its saved UUID and native history without executing, changing identities or inserting another row. These synthetic process identifiers exercise the native/history/state boundary; they do not claim a second actual lifecycle workflow or browser journey.

Additional final-source cases cover SDK empty/current, foreign company/account, invalid binding/revision, null and late responses; the rendered queue covers confirmed empty, denied account, sanitized provider failure and changed-account reply suppression. The final affected source/Razor/local HTTP/native/migration-metadata gate passes all 417 cases, zero failures/skips, in one minute twenty seconds, terminal exit 0. Its scope overlaps the earlier checkpoints; counts are not summed. No full host, physical provider or browser result is claimed.

Reproduce through Scripts/Test-PurchaseOrderOwnership.ps1 with canonical Data DLL, IncludeWorkflowInbox and FieldRevenueSourceTests|CompanyProcessStart|CompanyStep|PersonaInbox|CommercialIntentMigration|ModuleSchemaVerification. Temporary logs: oilgas-257-data.log, oilgas-257-source.log, oilgas-257-corrected-source.log and oilgas-257-final-source.log. Existing focused source/dependency warnings remain; no full solution or physical provider acceptance is claimed.

## Remaining delivery

Continue provider-backed queue pagination/large durable histories and actual landing-to-original-process recovery after further lifecycle advancement. Then continue approved delivery/effective terms -> invoice/recognition -> ledger read-back, royalty/partner queues, wider thirteen-persona workflows and multinational acceptance. Read-only recovery for actors without preparer command grants, reassignment/cancellation and retention remain explicit open decisions.

Actual provider concurrency/isolation/performance, complete host/OIDC/browser reconnect, accessibility/RTL, live Beep provisioning and the full company/multinational plan remain open. No CA/FG phase or overall goal closes. Commit and push remain authorized only after the complete goal and plan pass the final completion audit.
