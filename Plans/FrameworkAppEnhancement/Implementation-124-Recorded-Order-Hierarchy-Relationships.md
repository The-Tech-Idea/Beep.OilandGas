# Implementation 124: Recorded order hierarchy relationships

## Visible company-admin and maintenance relationship review

The existing asset hierarchy now includes authorized WORK_ORDER records once under the selected company's presentation container. Orders retain every recorded FIELD component as references on the existing Data-owned AssetHierarchyNode, using the existing WorkOrderComponentReference contract. They are not duplicated under several FIELD parents, assigned a guessed single parent, or made visible through inherited FIELD access. An exact order-root selection returns the same relationship evidence.

AssetHierarchyService reads headers through the existing mandatory subject row policy and obtains components through a required API-owned callback. CompanyWorkOrderFieldReview now exposes the confirmed component population from its existing bounded metadata/entitlement/fingerprint review; its resource-level method reuses the same capture. API Program wires that callback into the current hierarchy owner. Final hierarchy delivery rechecks component content and current order row authority, company/physical binding and hierarchy configuration. Work-order path validation refuses a supplied component population differing from current recorded references. No separate order hierarchy store or grant mechanism is introduced.

The Web hierarchy state validates complete, unique recorded order components, refuses an inferred single parent, and preserves a snapshot of relationship contents for selection. A component altered after publication cannot remain selectable. Company switching clears the selection. The actual tree shows each FIELD component as an encoded relationship label and explains that references do not grant related-asset access. No extra CSS, JavaScript, routes, fictional entity or hierarchy permission is added.

## Actual rendering correction

Static rendering exposed an existing RenderTreeNode bug: MudButton.OnClick requires EventCallback<MouseEventArgs>, but the manual render builder supplied an untyped EventCallback. The tree now creates the correctly typed callback. This affected actual asset-button rendering and was not detected by Razor compilation alone. The corrected render gate passes.

## Focused evidence

Actual isolated TheTechIdea.Data build: zero warnings/errors, 1.52 seconds. The final focused company gate passed **272 cases**, zero failed/skipped, terminal exit 0, reported duration 48 seconds. Log: `%TEMP%/oilgas-order-hierarchy-124.log`. It selects CompanyAssetHierarchyTests, CompanyHierarchyUiTests, CompanyHierarchyConfigurationTests, CompositeFacilityGrantTests and CompanyAssetGrantTargetTests using the explicit actual current Data assembly introduced in Implementation 123.

Seven new cases prove one recorded order with two FIELD components at company/root scope, refusal of FIELD-inherited order visibility, immutable selection against component mutation/company change, and refusal of missing, duplicate or inferred-parent scope. The preceding 265 affected hierarchy/configuration/composite/admission cases remain green. Actual server and Web state sources execute with SQLite app RBAC and the existing controlled Beep PPDM fixture. Existing nullable/source-precedence/analyzer warnings remain; no full clean host build is claimed. Program DI wiring and the new work-order supplied-path comparison are source changes without dedicated hosted/path acceptance evidence in this gate.

Scripts/Test-HierarchyWebComponents.ps1 -RenderWorkOrders compiles the three actual access-control components against the current compiled state/Data and MudBlazor 9.7.0: zero warnings/errors, 2.47 seconds. Static HtmlRenderer checks pass for the recorded multi-FIELD order and unconfirmed order, plus five existing FIELD-interest page cases covering English/French numeric presentation and full/empty/error/loading states. Order labels remain encoded, the order appears once, and incomplete scope clears order evidence. Log: `%TEMP%/oilgas-order-hierarchy-razor-124.log`. Evidence: `%TEMP%/oilgas-hierarchy-razor-c384fbee75b7478f9654850a2923c970-render`. The initial render failure and its typed callback correction are retained in this document. No browser events, OIDC/route guards, hosted HTTP or native provider claim.

## Remaining persona delivery

Approved maintenance duty-to-role mapping, standard role controls, configured order level naming/visibility administration, native HTTP/provider/date-window/concurrency and hosted DI remain open. Complete administrator grant -> maintenance landing/task -> recorded order -> inspection requirement/result/readback -> independent acceptance -> return-to-service/accounting. Inspection Beep migration/transactions and durable outcomes/corrections remain open, as do all remaining company persona journeys. No SQL, business schema change/new EF lane, live grants, migration execution, deployment or persona/phase/goal closure.
