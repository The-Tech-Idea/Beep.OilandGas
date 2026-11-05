# Implementation 214 — Receiver-to-warehouse source review screen

## Delivered receiver and warehouse journey

The existing procurement receipt table offers Review receipt inventory linkage for active receipts with a canonical recorded receiver decision. The existing workbench handler requires the current authenticated account, standard assigned company roles, Procurement.View and Inventory.View, then calls the existing typed linked-source API. Both API grants remain independently enforced. A missing recorded link/location/unit or stale source produces an explicit error rather than a stock confirmation.

The new shared ReceiptInventoryReviewPanel shows the exact receipt and PO line, encoded item detail link, warehouse/location/unit, verified accepted/rejected quantities, receiver actor/time/reason, recorded on-hand quantity and the linked/PO/inventory revisions. The warehouse detail link opens the current existing inventory screen. Unknown on-hand quantity remains Unavailable. The panel explicitly distinguishes confirmed source linkage from physical movement, available stock, supplier return execution and financial posting.

The existing PurchaseOrderWorkspaceState now owns this read-only review in a partial implementation, without another workspace/store. It requires the linked API's PO revision to equal the displayed order revision. The private review is copied on acquisition and exposure. Concurrent linked reviews are refused; busy state settles on success, refusal or failure. Old company/account/source/disposal responses cannot publish. Clearing or starting receiver work invalidates the linked review; unsettled receiver intents prevent new linked reads. Authentication changes clear the entire displayed procurement source and linked review. Procurement reload now requires the authenticated account to match the selected account, preserving current standard-role/grant requirements.

## Implementation and fixture boundaries

Existing standard ASP.NET authentication/authorization remains the gate; no persona preference grants rights. The shared source panel dispatches receipt identity to the workbench's existing owner and typed API path. The full procurement page and receiver workflow are retained. No database access, schema change, SQL, EF lane, role provisioning or physical/financial write is added. The new optional line column still needs the Beep provisioning described in Implementation 213.

The existing procurement render fixtures now register ASP.NET authorization core and cascading authentication services, which their AuthorizeView navigation requires. Their authentication fixture includes the actual actor name-identifier claim. The missing fixture services caused five previous receiver-page checks to fail at rendering; these were fixed without weakening standard authorization or the stricter account guard. The earlier checkpoint was 62 passed and five failed, not a fully green run.

## Focused verification

The final affected gate passes **73 checks in 6.8388 seconds**, zero failed/skipped, with 398 composition warnings. Fourteen new cases comprise nine scoped-state cases and five complete procurement-page handler cases. They cover copied private evidence, source/company/account/context clear/disposal, concurrent requests, malformed/changed actor or source, failed/null responses, role/grant/account refusal, actual review handler rendering, warehouse route, unknown quantity and authentication invalidation. Prior actual linked-review HTTP/typed/native and receiver write/reconciliation/page checks pass in that same gate.

The source checkpoint passes **563 checks in 6.6125 seconds**, with 350 composition warnings. It preceded the final explicit null-response guard; the final affected gate compiles that guard and verifies its new null case. Data is unchanged from Implementation 213's clean build. All launched handles are terminal; whitespace checks pass. Page handler cases use the canonical native-generated review and a typed-client fixture; linked-review HTTP/native cases independently exercise the actual controller/client/repository. Real browser/OIDC/keyboard/screen-reader/RTL/responsive, live provider/migration/grant provisioning and circuit lifetime acceptance remain open. No phase closes.

## Next recorded effects

Implement reviewed PO-line inventory-reference creation/reassignment through the existing source owner, with current company/resource authority, revision-bound review, recorded reason/audit, independent review as required and original-intent reconciliation. Then record physical stock/return effects with exact receipt/item/unit/location provenance and owned transaction/read-back evidence. Keep ledger posting separate until complete book/period/account/currency and physical/financial consistency evidence exists. Extend the existing screens, landing queues and assigned-task drill-downs with confirmed effects. Wider persona and multinational acceptance remain active.
