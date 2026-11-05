# Implementation 302 - Inventory persona FIELD reservation dashboard

Continuation of [301](Implementation-301-Current-Permit-Material-Comparison.md), within the approved company/persona data-workflow plan.

## Design and acceptance

The warehouse landing page must connect company inventory evidence with the selected FIELD's recorded work-order requirements. Use the existing InventoryWorkspaceState, typed accounting client and native InventoryService owner. The dashboard offers an explicit review button, record counts and status filters (active, upcoming, expired, released), original quantity/unit/location/UTC evidence, item/order/workflow links and empty, loading, disabled and error states. MudBlazor controls use the existing theme; no custom CSS or JavaScript is introduced. Counts describe records, never physical stock availability or totals across units. Original owner recovery links carry the exact retained intent only with the reservation permission; other actors receive ordinary original workflow links.

## Implementation

Canonical Data owns the bounded public queue and status definitions. The native owner reads complete company/FIELD reservation and release populations from the existing PRODUCTION tables. Current direct WORK_ORDER READ rights filter records before parsing retained private command bodies. Public summaries validate original reservation and full release membership, repeat source reads and close current authority/binding checks. Private command JSON and rationale stay absent. No stock, permit, migration or role write occurs.

The API independently checks the resolved current app account, standard roles, Inventory.View, stored selected FIELD and direct FIELD/work-order rights. The dedicated GET endpoint carries the explicit FIELD assertion. The client checks exact company/account/FIELD and full content revision and rejects late selection changes. The warehouse panel uses the scoped existing workspace, preserves original workflow addresses, and clears display on company, authentication, FIELD and disposal. Closing authentication/FIELD checks run before publishing the response.

## Focused evidence

Canonical Data build: zero warnings/errors, terminal exit 0 (3.24 seconds). Initial affected native/state/warehouse group: 54 passed, zero failures/skips, terminal exit 0 (4.7332 seconds). Dashboard canonical/state/client/rendered group: 13 passed, zero failures/skips, terminal exit 0 (3.2180 seconds). Final affected native/state/client/warehouse/dashboard group after closing display fences: 67 passed, zero failures/skips, terminal exit 0 (5.5715 seconds). Protected actual queue MVC/app-role/native journey passed one (5.6934 minutes), zero failures/skips, terminal exit 0. A fresh independent actor sees released public evidence, current direct work-order READ revocation hides the record, an asserted FIELD mismatch refuses, and revoked Inventory.View refuses before reservation reads. No queue read adds a write. Program registration is source-reviewed rather than full-host startup evidence; these overlapping groups are not a full solution/provider/browser/OIDC test.

A rendered fixture originally mounted a second panel and cleared shared state; it now refreshes the original panel. An HTTP fixture initially used a nonexistent access-table property; it now uses the canonical EF app-RBAC set. Neither repair weakens production checks. Existing app-RBAC fixture EF is separate from native business persistence.

## Remaining work

This is a read-only inventory persona landing slice. Durable material prerequisite acceptance, personnel/source documents, permit approval/corrections, physical issue/return/execution, logistics selection, the remaining persona/financial/multinational journeys and full completion/commit/push audit stay open. Source closure is sequential across native/core/RBAC owners; real-provider concurrency and cross-database atomicity are not claimed. Browser/keyboard/screen-reader/RTL/OIDC/full-host validation is separate. No live migration, SQL or new business EF is introduced.
