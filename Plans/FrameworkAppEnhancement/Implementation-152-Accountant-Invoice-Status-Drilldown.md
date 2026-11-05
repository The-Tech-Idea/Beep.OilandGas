# Implementation 152: Accountant invoice status drill-down

## Daily accountant landing

AccountingDashboard now embeds the existing company AP invoice source queue alongside its configured financial definitions and assigned-work inbox. It uses the existing typed accounting client and APInvoiceWorkspaceState, with separate source loading/failure/reload behavior. No second invoice store, dashboard API or business model is introduced.

The queue is explicitly company-wide across suppliers. Selected FIELD does not establish or filter AP header ownership. Current company roles and Accounting.View are required before the parent reads it; the existing API independently enforces authoritative company access. Invoice source failure does not become a zero count or a successful financial readiness result. Configured financial definition loading remains separate.

## Exact recorded-status contributors

The same APInvoiceSourcePanel on the dashboard and payables workbench now shows buttons for All returned statuses and each actual recorded status count. Clicking a status selects its exact contributing current rows in the same frozen source snapshot, retaining encoded invoice links. Missing/blank source statuses have an explicit Unavailable recorded status bucket rather than being silently excluded. A literal recorded status named Unavailable remains a separate exact value and count.

The state retains the original raw status in each row, derives readonly status/count projections and allows only current recorded filters. The missing-status selection uses a separate boolean, so it cannot collide with any recorded string. Clearing/reloading or switching company clears counts, rows, selection and provenance; failed reads leave no previous summary. Counts are of the provider-returned active-company query, not certified enterprise totals.

These are source counts. They do not establish unmatched/unposted exceptions, aging, approval, payment, accounting period/book applicability or liability. Recorded amounts/currencies remain row-specific with no mixed-currency total. Complete currency-bound aging and authoritative workflow/KPI queues remain separate requirements.

## Focused evidence

The final affected actual-source gate passes **355 tests**, zero failed/skipped, terminal exit 0, in 3.2724 seconds. Log: `%TEMP%/oilgas-ap-status-dashboard-152.log`. It includes the previous 347 checks plus eight new cases; overlapping totals are not summed. The existing 175 dependency/source-precedence warnings remain. Data sources are unchanged from the prior clean complete-source build.

Four state cases verify exact recorded/missing-status contributors and counts, including a literal Unavailable status, and company-clearing behavior. One case verifies transport mutation cannot change counts and a failed reload clears the summary/selection. Three actual Razor render cases verify summary counts, selected visible count, exact contributing invoice links, omitted unrelated links and explicit no-liability-total feedback. Existing source/late-response/API/native cases remain in the gate, including 33 complete-controller loopback Kestrel HTTP checks.

The actual AP state/shared renderer and current source readers/controllers compile and execute in their focused scope. The complete AccountingDashboard parent page and complete typed Web client remain source-only, as do production role bridge/context adapter/full host. Browser events/keyboard/RTL, proxy and real-provider completeness/paging/performance/schema behavior are not certified. No SQL, business EF path, migration execution, role grant, business write or deployment is introduced.

## Remaining handoffs

Verify the actual dashboard-to-queue-to-invoice client/browser path and production role bridge/provider evidence. Implement authoritative exception/aging/currency/book/period drill-downs and independently configured invoice/allocation preparer/reviewer actions, global receipt consumption, matching tolerance/acceptance/unit rules, approval/posting/payment and durable outcomes/recovery. Apply/reconcile the AP-line/allocation Beep schemas through reviewed MigrationManager. Every other persona/company/multinational requirement remains open; no phase closes.
