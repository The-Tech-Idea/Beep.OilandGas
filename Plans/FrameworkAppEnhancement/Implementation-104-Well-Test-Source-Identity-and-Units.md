# Implementation 104: Recorded well-test identity and units

## Canonical review contract

Moved the existing WellTestResponse from Models to the canonical Data owner, preserving its consumer namespace and removing the old source definition. No duplicate production response exists. The existing response adds recorded SOURCE, exact string TEST_NUM/RUN_NUM and oil/gas/water UOM and original OUOM. Alphanumeric run identifiers are preserved rather than lost by the old decimal RunNum field; consumers still need reconciliation of that legacy numeric property.

## Existing mapping/read path

The current PPDMProductionService already reads WELL_TEST through the existing repository/binding and PPDMMappingService. The actual mapper now enriches that same response with the complete recorded primary key (UWI,SOURCE,TEST_TYPE,RUN_NUM,TEST_NUM) as an unambiguous JSON tuple identity, exact source/test/run strings and actual creator/updater metadata. Missing key parts refuse mapping instead of inventing an identity. No new observation repository/reader/model or SQL was added.

Current and original flow units are copied explicitly from actual PPDM fields. A focused check found the generic name converter dropped original OUOM, and the existing mapping owner was corrected. Values are not converted or averaged. The current review grid displays amounts with their actual current UOM or Units unavailable, exact run/source/test identity and recorded original units. It retains explicit source timezone/approval limitations.

## Focused evidence

Actual isolated Data build succeeded, zero warnings/errors, 1.77 seconds; `%TEMP%/oilgas-well-test-contract-data-build.log`.

Actual PPDMMappingService/current Response source with actual PPDM/Data dependencies: **13 passed**, zero failures/skips, terminal exit 0; `%TEMP%/oilgas-well-test-mapping-final.log`. Covers amounts/current-original units/preparer/updater, alphanumeric run/source/composite identity, each changed identity part, each missing key part and missing unit preservation. The earlier 12-pass/1-fail run identified lost original oil units; final corrected run passes all cases.

Actual review state/current Data response source: **11 passed**, zero failures/skips, terminal exit 0, 1.8243 seconds; `%TEMP%/oilgas-well-test-review-data-owner.log`. Covers the prior scope/nullability/identity/late-response cases against the moved owner. Focused checks compile actual source definitions over old cached model copies with CS0436 suppressed; this does not assert a whole native Models/LifeCycle/Web build. Source search confirms a single WellTestResponse definition. Whitespace diff check passed.

## Existing writes and remaining journey

Inspection confirmed WellManagementService.CreateWellTestAsync creates a testing work order, not a recorded WELL_TEST observation. ProductionProcessService.PerformProductionTestAsync stores serialized testing payload in the existing process step owner. Neither is presented here as durable observed measurement capture; native capture/correction/versioned supervisor validation/shift handover and committed read-back still need completion through the correct owner.

The native provider reader/mapper call, HTTP/controller/resource filters, parent page/rendered browser/OIDC/keyboard/RTL and original company/FIELD/version envelope remain unexecuted. PPDM generated flow fields are nonnullable decimals, so provider null-to-zero ambiguity requires real-provider proof; the nullable DTO does not recover already-lost source nulls. Unit semantic interpretation, source timezone, approved observation state and separate BSW/water-cut provenance remain explicit gates. Coverage-card drill-down and assigned supervisor/workflow handoff are still open.

No SQL, schema/migration, business EF lane, role grants or phase/goal closure. All other persona and accountant workflows remain active.
