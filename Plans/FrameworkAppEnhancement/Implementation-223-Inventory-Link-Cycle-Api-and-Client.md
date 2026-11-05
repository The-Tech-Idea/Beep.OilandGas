# Implementation 223 — Inventory-link cycle API and typed client

## Company procurement boundary

The existing inventory controller and CompanyInventoryReads now expose complete owned cycle history, explicit reopening review and the native atomic cycle command. All routes decode original order/line IDs through the existing source route reader:

- GET `api/accounting/traditional/inventory/company/purchase-orders/{orderId}/lines/{lineId}/inventory-link-history`
- GET `api/accounting/traditional/inventory/company/purchase-orders/{orderId}/lines/{lineId}/inventory-link-cycle-review/{cycleAction}`
- POST `api/accounting/traditional/inventory/company/purchase-orders/{orderId}/lines/{lineId}/inventory-link-cycle`

History requires authentication, a current assigned standard app role, Procurement.View and Inventory.View. It returns the existing shared cycle-result contract containing complete current order and canonical history, so callers validate the active line cursor as well as the retained chain. No reopening, preparation or reviewer mutation grant is needed for this read. Review and reopening additionally require the new distinct Procurement.ReopenInventoryLink grant. Preparation/decision grants do not imply it. IdentityServer remains authentication-only; current app RBAC supplies authority. No roles or grants are provisioned by this slice.

## Current authority and exact original command

History repeats complete native order/history captures with current company/account/binding/role/grant callbacks and compares full source revisions before publishing. The explicit action must be defined and match a pending or resolved verified cycle; no action defaults to approval or reopening. Review preserves complete copied order/history evidence and the current binding.

Mutation freezes the submitted company/account/order/line/action/reason/intent/full review. Current binding must equal the retained binding. Native callbacks repeat the complete original authority tuple across transaction boundaries; changes to roles or grants, including additions, refuse confirmation. The existing native owner archives the original line and reopens it atomically. The returned proof must confirm the exact original submission and complete order/history before publication. Malformed action/routes return 400, missing authority 401/403, and unconfirmed evidence/provider/commit outcomes 409 with original-intent reconciliation guidance. Closing authority loss remains forbidden even if a mutation may have committed.

## Existing typed client

IAccountingServiceClient and its existing inventory partial now provide history, explicit cycle review and reopening methods. They freeze selected company/account context, use exact escaped resource IDs, validate shared canonical contracts and copy confirmed results. Reopening requires full original-submission proof; late company/account responses are discarded through the existing context cancellation/fence. The shared cycle result now owns its source validation, reused for history and mutation confirmation. No parallel DTO/store, frontend database access, SQL or new EF business path is added.

## Focused verification

Actual Data builds cleanly with zero warnings/errors in 1.47 seconds. The final affected gate passes 501 checks in 38.0745 seconds. The overlapping source gate passes 566 checks in 9.0596 seconds. Both final runs reach terminal exit 0 without failed/skipped tests; composition warnings remain. Whitespace checks pass.

Fifty new checks include 32 actual typed-client/Kestrel/native cases for resolved/pending/escaped-resource cycles, unknown commit and restored original-intent recovery, post-commit order drift, separate mutation/read grants, standard roles, account/binding/full authority drift, wrong company/route/action, changed intent/reason, archive/line failures and read-only history access. Eighteen response-boundary checks refuse altered returned references, cursors, archive/order evidence, account/company, action/revision and null results across all three typed methods.

The first HTTP gate found that the route placeholder `action` conflicts with MVC's action route value and caused 404 responses. Renaming it to `cycleAction` fixes endpoint matching; the corrected focused gate and final affected gate pass. Tests use actual canonical Data/native transaction/service/API/client code with fixture provider rows and metadata. Complete legacy host, live provider locking, migration/grant provisioning, browser/OIDC/accessibility/RTL and durable circuit recovery remain unverified. No schema or live migration is executed in this slice; prior canonical history/cursor Beep provisioning remains open. No phase closes.

## Next persona delivery

Extend the existing procurement workspace/screen for owned history, explicit pending supersession/resolved reopening review, reason, save and original-intent reconciliation. Use the current authenticated account, standard app roles and both source grants plus the separate cycle mutation grant; guard competing preparation/reviewer/receiver actions and invalidate late company/auth results. Display archived preparer/reviewer outcomes separately from current reference and physical stock effects. Then verify order/warehouse/inbox handoffs before stock/return or financial mutations. Wider persona and multinational acceptance remain active.
