# Implementation 303 - Assigned maintenance inventory selection

Continuation of [302](Implementation-302-Inventory-Reservation-Dashboard.md), within the approved persona/company data-workflow plan.

## Design

The existing reservation panel should discover the current company inventory through its verified assigned maintenance task, then choose an exact recorded item. Show the recorded name, location, on-hand quantity and unit without inferring availability. Keep required quantity/unit, UTC window and rationale explicit. Item/input changes must invalidate the old source review. An original retained reservation/release remains immutable and blocks replacement until confirmed recovery/release. Reuse the current workflow client, InventoryService/CompanyInventoryReads owners and canonical Data contracts. No parallel inventory persistence or private command bodies.

Use an explicit load button and MudBlazor item chooser with initial, loading, disabled, empty and error states. Selected recorded evidence links to the existing warehouse detail. Clear catalog/inputs/review on company/account/FIELD/task/source changes; fence closing authority before publication. No CSS/JavaScript additions. Keyboard/screen-reader/browser/RTL validation remains separate from local component checks.

## Gates

- Canonical Data validates complete bounded company membership, active records, unique keys, assigned task/role, UTC and full content revision.
- API uses the current assigned task boundary, standard app roles, Inventory.View/reservation permission, existing native queue, PRODUCTION binding and repeated original source/authority closure.
- Typed workflow client rejects wrong company/account/task/role, damaged evidence and late selection changes.
- Actual reservation panel discovers/selects recorded items, resets stale reviews and refuses foreign/late catalog membership without another write.
- Existing reservation/release/reference/router/source screens remain intact under focused regression.
- Protected HTTP/native task-catalog journey verifies source and current permission/order-right refusal.

## Status

Canonical Data built with zero warnings/errors, terminal exit 0 (3.37 seconds). The full typed workflow/client/state group passed 161 (1.5306 seconds), the actual reservation/receiving/process-router/chooser group passed 117 (3.7173 seconds), and affected production/source/detail/chooser screens passed 168 (4.0995 seconds), zero failures/skips, terminal exit 0. These groups overlap. Protected actual assigned-task/catalog/source/reservation/release/original-reference MVC/app-role/native journey passed one (6.2415 minutes), zero failures/skips, terminal exit 0. It verifies exact current owned item/location/unit/quantity choices with no catalog write, missing reservation permission/direct work-order rights before inventory reads, full inventory drift refusal, revoked Inventory.View before source reads, and unchanged original reservation/release/reference recovery. No full provider/browser/OIDC/bootstrap result is claimed.

The initial canonical build lacked the existing inventory model namespace, the first Razor build needed an explicit string type for its unit callback, and three chooser fixture cases initially generated a different task for the catalog. These were corrected without weakening membership, current-role or source checks. Rendered tests now cover current selection, empty catalog, wrong actor/assigned role, late company/FIELD, foreign item refusal, damaged revision and stale source invalidation. Actual required quantity/unit/UTC/rationale remain explicit and no catalog discovery writes a reservation.

This selection slice is verified within focused local scope. Durable material prerequisite acceptance, personnel/source documents, permit approval/corrections, physical issue/return/execution and full persona/financial/multinational/completion/commit/push audit remain open. No live migration/provisioning, SQL or new business EF is introduced.
