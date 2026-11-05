# Implementation 142: Confirmed provider source results

## Accountant and procurement evidence correction

The active PPDMGenericRepository converter previously returned an empty sequence for a null provider payload and skipped null members of a returned collection. A failed or partial source could therefore appear as a confirmed empty invoice/order queue. The shared converter now refuses both cases with an explicit InvalidOperationException. It returns no partial mapped list after encountering a null row. Actual empty arrays/lists remain valid empty results.

The correction applies to the existing shared repository path; it introduces no AP-only adapter, alternate store, fallback or compatibility switch. Providers must return an actual empty collection for a confirmed no-row response. A provider that uses null for no results now produces an unavailable read until its contract is corrected. Such providers need native acceptance before promotion.

The existing AP service propagates these refusals and the existing AP UI state clears earlier supplier rows before a reload. Failed native reads cannot replace the previous invoice queue with an empty-success claim. Recorded amounts, company ownership, source revision and the accountant/procurement role boundaries remain the previously implemented contracts. No new financial action or permission is enabled.

## Current repository source gate

The existing purchase-to-pay gate now compiles the complete current PPDMGenericRepository, its actual interface, PPDMDataSourceTransaction and BeepOperationResult sources alongside the canonical AP/PO services and their previously documented actual dependencies. This replaces the cached repository implementation in the test compilation. The cached native PPDM dependency still supplies other existing metadata/wrapper types; this is not a complete PPDM project/host build.

The interface is compiled with the class because its delegate/nested result types refer to the repository itself. Existing actual nodal result/point sources are included to satisfy the repository's unrelated declared method signature. These are source links to existing files, not duplicate production definitions. Native Models remains aliased so Data-owned AP/PO types win; the existing PPDM.Models global import is supplied by the test harness, as in the production project.

## Focused verification

`Scripts/Test-PurchaseOrderOwnership.ps1`: **134 passed**, zero failed/skipped, terminal exit 0, 1.4339 seconds. Log: `%TEMP%/oilgas-confirmed-source-142.log`. The prior 129 cases are included rather than summed. Five new cases exercise null payload, null-only rows and partial valid/null rows through the actual AP service/current repository, plus two service-to-UI-state failed reload cases confirming prior rows/provenance are cleared. Existing genuinely empty array and successful owned-query cases still pass.

The test build has zero errors and **175 warnings**: 39 CS0436 notices from current repository/interface/helper source taking precedence over the cached native dependency; 93 obsolete account seed references; three accounting-basis enum switches; and 40 nullable warnings in current repository/dependency sources. These are not suppressed or claimed resolved. The source-precedence notices are specific to the focused harness; they do not add production classes or files.

Initial compilation needed the repository's actual interface/internal transaction-result helper and existing calculation source dependencies. Those source links and actual project global imports corrected the gate without substituting production implementations. Current Data assembly remains the previously clean built artifact; no Data change was made in this slice.

## Remaining journey acceptance

Verify complete production PPDM assembly integration and actual provider empty-result/null contracts across the supported stores. The focused fixtures are IDataSource contract tests, not real database reads. Current API controller, typed client, parent page/facade/host, HTTP/OIDC/browser/keyboard/RTL, Beep migration/reconciliation, atomic read/write consistency and durable financial commands remain open. No invoice-line/PO/receipt matching relationship or approval/payment outcome is inferred from these source checks. All persona and multinational requirements remain active; no phase closes. No SQL, business EF lane, migration, live write or role grant was added.
