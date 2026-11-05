# 336 - Recorded maintenance work on persona landings

Continue 334 with discoverable original report links on the existing maintenance, production and operations workspaces. Recorded work is separate from pending assignments, accepted closure, native asset return and financial close. This feature does not replace the qualified asset work in 335.

## Source and design

- Extend WorkOrderExecutionPermitService, the existing native WORK_ORDER_EXECUTION_SUBMISSION owner. Query only the current company's authorized original work orders, FIELD and explicit recorded UTC period. Require actual schema, complete bounded native membership, exact predicates, stable source/authority and original row/body proof before projecting public report summaries. No business EF, SQL, new table or startup migration.
- Reuse current app roles, Maintenance.ReturnToService and direct FIELD/order READ grants. Revalidate the stored FIELD, current company/account, module binding and current native work-order membership. Work-report discovery grants no command or asset access; the linked detail independently checks its own role/rights and original report/closure/asset sources.
- Add a Data-owned recorded-work window and page. Counts describe the complete authorized report population in that window, not tasks or asset operating state. Page links preserve the exact original process, FIELD, order and submission. Rationale, intent UUID and private command bodies remain excluded.
- The present generic Beep repository loads matching rows before application pagination. Keep the native population and retained-body budget explicit and fail rather than truncate; expose an exact-order filter for broad scopes. Do not claim provider-side paging or unrestricted historical scale. Typed date filters use native UTC audit fields and actual AppFilter type information; providers that do not honor predicates are refused.
- Extend the existing workflow client and an isolated read state. Context, period, order, page and persona changes clear old values and reject late replies. Reuse PersonaTaskInbox on catalog-matched maintenance/production/operations landings. Choosing a persona does not grant permission. Preserve existing pending-task counters.

## Delivery order and gates

1. Canonical contracts and exact source/detail navigation; validate UTC/window/page/membership and privacy.
2. Actual native report discovery with company/order predicates, schema, stable paired reads, original proof, bounds and no mutation. Test missing schema, foreign/predicate-violating rows, duplicated source IDs, corrupt bodies, changed authority/source and complete page membership.
3. API READ boundary and standard app-role/domain capability checks; current native order scope and direct grants must agree before private report parsing. Test two companies/colliding IDs, absent/revoked read rights, period/order filters, exact page/source links and no writes.
4. Existing typed client, context-fenced state and MudBlazor paged queue. Display original reported work, executor, completion/report UTC, defects count, report source and exact detail link. The queue must never call a mutation or label an executor report as accepted closure/asset return.
5. Compose on the existing persona inbox only for an eligible catalog route/profile and current read capability. Test mismatched persona/route, company/account/FIELD/auth changes, failed/late replies, page/period changes, source-link escaping and unchanged task counts.
6. Run focused native, API, client/state/rendered component and parent checks. Keep real provider paging/date/time-zone behavior, browser/OIDC/keyboard/RTL and release verification separately labeled.

## Status

Implemented for published work-report discovery on the existing maintenance, operations and production-engineer persona landings. Focused native, client/state, rendered queue and actual parent gates pass as listed below. The expanded protected HTTP gate also passes. This checkpoint does not close a phase or the full original goal; commit/push follows full goal/plan completion audit.

## Implemented boundary

- Data owns CompanyMaintenanceRecordedWorkWindow, public report entries and complete page metadata. The window is explicit UTC, at most 31 days; source review is limited to 128 authorized orders, 256 reports, 1 Mi characters per retained command and 16 Mi total characters. FIELD header discovery is bounded at 1,024 rows. Exceeding a bound refuses the read with a request to narrow the period/order; it never truncates into a successful empty or partial result.
- The existing native report owner validates physical submission schema/key/capacity, current direct order/FIELD rights, exact company/source/date predicates and original proof. Two complete ordered reads must agree. Malformed retained proof yields a controlled source conflict. This is a read-only comparison, not a transactional snapshot across grants, preferences and native data.
- CompanyMaintenanceHandoffService and CompanyMaintenanceRecordedWorkController expose the no-store read endpoint through ApiService. Exact-order discovery checks the native detail identity before report reads. App roles and Maintenance.ReturnToService remain required; private closure/return commands retain their separate mutation checks.
- MaintenanceRecordedWorkQueue uses the existing workflow client and component-owned MaintenanceRecordedWorkState. PersonaTaskInbox composes it only on catalog-matched routes with a matching current profile/account. The visible UTC filter initially covers the current day and preceding 13 UTC days; it is a discovery filter, not a generated domain observation. Failed source reads clear the queue and do not display an empty success. Company/FIELD/auth/persona/input changes reject late results.
- Original report navigation retains escaped process/submission and source tenant/company/FIELD markers. WorkOrderDetail rejects a foreign original source before loading its work order and never switches the selected company or FIELD. The detail independently reads its original report/closure and current native return evidence.
- Counts describe authorized published reports, not accepted closures, pending tasks, returned assets or financial close. No business database write, new schema, SQL, EF domain model, CSS or JavaScript is introduced.

## Focused verification

| Gate | Evidence |
| --- | --- |
| Canonical Data isolated build | Zero warnings/errors, 2.06 seconds; oilgas-336-data2.log |
| Actual native report owner and proof refusal | 13 passed, 8.5691 seconds; oilgas-336-native3.log |
| Client/state/rendered queue and return-state checks | 42 passed, 12.8103 seconds; oilgas-336-ui2.log |
| Expanded maintenance/return/completion UI regression | 300 passed, 46.9771 seconds; oilgas-336-ui-regression.log |
| Actual work-order detail parent with original-source links | 24 passed, 3.1362 seconds; oilgas-336-detail.log |
| Actual PersonaTaskInbox integration and existing inbox state | 85 passed, 2.3008 seconds; oilgas-336-landing2.log |
| Protected read-only API | 1 passed, 28.4744 seconds; oilgas-336-http4.log; company-collision, exact order/period, revoked order/capability, invalid page and no writes |

The focused PowerShell scripts link actual repository source against the isolated canonical Data assembly. Test source/date/transaction boundaries are explicit fakes; protected HTTP uses actual app-role/profile/company/grant/native owners and local hosting. Every listed gate is terminal with zero failures/skips. Counts overlap and are not a full-solution count. The inbox harness was updated to link current execution/return/closure partials used by its existing assigned-stage dispatcher. Cached/source-type conflicts, nullable warnings and existing MudBlazor analyzer warnings remain outside this checkpoint.

## Remaining scope

### Delivered inline asset evidence

Each published report row now offers an explicit read-only asset-evidence action. One selected original report appears under the existing queue, reusing MaintenanceAssetEvidencePanel and its current public API/client/state, including facility/equipment and mixed evidence. Selection must belong to the current authorized page. Switching page, period, order, company, account, FIELD or authentication closes the view; switching the selected report invalidates its old evidence. Closing/disposal clears the existing evidence state and fences late replies. Opening a row alone performs no asset read; the existing evidence buttons independently require the current standard role, FIELD and capability. The row button names its original report for assistive technology and exposes its expanded state. Pending assignment counts and report counts retain their existing meanings. No new endpoint, domain contract, write, SQL, domain EF, schema, CSS or JavaScript is introduced.

The final actual state/client/rendered queue/evidence/reference regression passes 164 in 46.9652 seconds; the earlier 161-case pass predates the extra absent-return, late-selection and account-switch assertions. This includes facility-only, equipment-only and mixed public results; explicit authoritative absence remains separate from failure. Closing, reloading, filter edits, another selected report, account/company/FIELD/auth changes and delayed replies cannot leave stale evidence visible. Foreign page rows are refused and read-only users perform no review/record command.

The actual PersonaTaskInbox parent and existing inbox state gate passes 88 in 3.1505 seconds, including original-report selection on maintenance, operations and production-engineer landings, role/profile/route refusal and unchanged pending work-order counts. Its first compile omitted the already-existing maintenance return-policy client partial from the isolated harness; linking that actual file repairs the harness without an application substitute. Earlier source-type/nullable/analyzer and unrelated PersonaSelector focused-harness warnings remain explicitly outside this slice. All runs are terminal; no shared native test output was rebuilt. Commands, hashes, outputs and seams are retained in [the inline evidence record](evidence/336-recorded-work/inline-asset-evidence-observed.json).

These are actual headless components and typed/state boundaries with controlled public projections/transport/auth/FIELD/JS seams. This adds no native/provider/browser/OIDC/keyboard/RTL acceptance claim and does not prove the original qualified native lifecycle or protected record POST. Authoritative return-intent absence/reset, later transition/rework, complete role dashboards and the broader finance/persona/multinational plan remain open. Full goal/plan completion precedes commit and push.

Verify date/predicate completeness, retained Unicode capacity and bounded-read behavior with a real Beep provider. Browser/OIDC, keyboard/RTL, timezone presentation, broader company/persona journeys and release checks remain unverified. Complete qualified facility/equipment and mixed-target evidence/return in [335](Implementation-335-Qualified-Facility-and-Equipment-Return.md), legitimate later-transition history, rejection/rework and finance requirements separately. This published-report queue does not claim historical closure/return inventory or unrestricted provider-side pagination.
