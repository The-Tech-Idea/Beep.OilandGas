# Implementation 230 — Procurement and stock workspace coordination

## One scoped pair of existing persona owners

Production registration now explicitly connects the existing PurchaseOrderWorkspaceState and InventoryWorkspaceState under the same scoped CompanyClientContext. Inventory does not constructor-inject procurement, so there is no dependency cycle. Attachment rejects another company-context instance or a second live procurement owner; disposal detaches the partner's events. Isolated projection fixtures may still instantiate the purchase owner without coordination; production registration requires the existing inventory owner.

Busy or unconfirmed physical stock work now fences receiving review/save, inventory-link target/source review, preparation, independent decision, cycle history/reopening and their original-intent reconciliation paths. Closing generation/context checks also refuse stale publication. Stock activity clears previously displayed procurement/target evidence so it must be refreshed after confirmation. The procurement screen disables affected controls, hides generic order handoff and displays the original warehouse intent recovery route while stock work is busy or uncertain.

Conversely, busy or uncertain receiving, inventory-link, preparation, decision or cycle work prevents another stock review or stock command before an intent is created or sent. The stock screen disables affected actions and shows procurement recovery. Separate event channels distinguish stock activity from procurement activity, allowing a procurement cycle to finish/recover without invalidating itself or creating a mutual busy-state deadlock. Read-only landing/source reloads remain available; native/API source and authority checks remain independent enforcement.

## Focused verification

The final affected inventory/procurement/receiver/handoff/screen gate passes 735 tests in 1.3372 minutes. The overlapping source gate passes 566 tests in 11.9397 seconds. The preceding focused coordination/rendered screen gate passes 17 tests in 7.8433 seconds. All final runs reach terminal exit 0 without failed/skipped tests; composition warnings remain and whitespace checks pass. Canonical Data is unchanged from implementation 229's actual clean build. Counts overlap and are not summed.

Five new workspace checks use actual native stock/cycle fixtures. They verify stock busy/unknown outcomes fence procurement reads/actions before delegates run, original stock retry adds no write and restores procurement reload, procurement busy/unknown cycles refuse stock before creating intent, original cycle reconciliation remains available, one shared company-context/owner boundary and disposal/replacement. The rendered receipt journeys now resolve both owners with the production-style factory. An added journey proves in-flight procurement inventory review disables stock, refuses save without intent/write, then permits exact physical stock confirmation after procurement evidence settles. Existing receiving/link/cycle/stock and warehouse landing journeys pass with these guards.

## Remaining delivery

This coordinates one app scope; it does not provide a distributed cross-user lock or durable circuit ledger. Next deliver durable original stock intent/outcome recovery that distinguishes recorded physical movement from current item/purchase state after intervening source changes, using canonical Data and Beep-owned persistence. Then verify the complete procurement/warehouse/configured-inbox journey across receipts, partial returns and later link cycles. Preserve stock/financial boundaries and original audit rather than replaying or repairing altered source.

No live stock write, migration, grant assignment, deployment, handwritten SQL or new business EF lane is executed. Real provider concurrency/locking, Beep provisioning, full host/browser/OIDC, accessibility/RTL/responsive behavior and durable circuit recovery remain open. Wider company personas and multinational acceptance remain active; no phase closes.
