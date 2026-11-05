# Implementation 182 — Accepted receipt capacity and cross-account handoff

Date: 2026-10-07. Status: accountant allocation preparation, complete consumption, native recording and screen evidence now use verified receiver acceptance. Existing reviewed company handoff/inbox/step owners are verified across distinct receiver and accountant accounts. Matching policy, stock and financial decisions remain open.

## Shared capacity owner and native recording

APReceiptConsumptionReview derives verified receiver intent and accepted/rejected quantities from the existing canonical PO receipt proof. It requires current owned source provenance, an active order/line/receipt and exact invoice/PO/receipt units. Missing or altered proof and source/unit/activity uncertainty remain unavailable. Complete recorded active consumption still includes the existing owned contributor set across invoice lines; a received quantity or receipt status cannot substitute for accepted quantity.

The existing eight-row APReceiptAllocationComparisons frame now compares proposal plus complete recorded active receipt consumption with verified accepted quantity. Received capacity remains a separate comparison. Accepted capacity can be within limit, exceeded or unavailable, including full rejection. The evidence shows physical received, accepted, rejected, global recorded consumption, proposal and decision actor; it explicitly remains a read rather than an atomic reservation or available balance.

APReceiptAllocationCommand.Require now requires accepted capacity within limit in addition to exact units and invoice/received limits. The existing typed client, screen readiness and API/native writer use that same canonical gate. The native owner already refreshes whole purchase source and complete receipt contributors within its owned Serializable Beep transaction. Exact original retry excludes only its proven own allocation, preserving capacity without allocating twice. Unknown acceptance and rejected capacity cannot fall back to physical received quantity to authorize recording.

The existing APInvoiceWorkspaceState projection and Mud source panel expose accepted/rejected quantities and receiver actor alongside recorded contributors. Missing values remain explicitly unavailable. The existing financial diagnostics, tolerances, independent financial decision and stock/ledger boundaries remain intact.

## Fix for different accounts reviewing the same source

A new real receiver-to-accountant test exposed that PurchaseOrderSourceReview and APInvoiceSourceReview included the viewing account in their content digest. An unchanged order therefore had a different revision when an independent accountant opened the receiver's handoff, making step review conflict.

The existing source revisions now identify company-owned recorded content rather than the reader. Company ownership, all relevant header/line/receipt content and audit remain in the digest. ActorUserId remains explicit in each review and Require still rejects a response addressed to another account. Original commands, proof, current company/account/binding checks, standard ASP.NET roles, RBAC permissions and independent reviewer checks continue to bind the actor separately. No parallel content/view revision or compatibility path is introduced. Changed source content still changes the revision. Existing retained pre-change review hashes are not silently rewritten or accepted; no live commands were created or migrated during this work.

## Existing receiver-to-accountant route

The integration uses actual typed receiver submission and native PRODUCTION recording, then attaches the refreshed PO source to the existing company handoff state. The existing catalog/start API and native LIFECYCLE owner create a configured accountant persona task from that source revision. A separate accountant account opens the actual app-RBAC-backed inbox, reviews the same owned source revision under its configured standard role and independently completes the reviewed workflow step. The existing native owner closes the accountant task and creates the configured next controller task.

The receiver, workflow starter, original PO maker and accountant are explicitly distinguished. Changed receiver proof after handoff refuses completion without another production mutation or lifecycle command write. Completing a reviewed workflow step does not change invoice status, receiving quantities or stock and does not authorize financial posting/payment. The tests use explicit configured roles/persona mappings rather than deriving permissions from IdentityServer or hardcoded persona names.

These are existing generic company workflow transitions operating on a whole owned order revision. A dedicated per-receipt workflow policy, independent receiving correction/return command and automatic physical-decision-triggered outbox are still open; receipt confirmation alone does not complete or route a task.

## Focused verification

Actual Data source build: zero warnings/errors, 1.99 seconds. Log: %TEMP%/oilgas-accepted-182-data.log.

Scripts/Test-PurchaseOrderOwnership.ps1 with actual Data and -IncludeWorkflowInbox: **986 passed, zero failed/skipped, terminal exit 0, 34.0374 seconds**. Log: %TEMP%/oilgas-accepted-182-verified.log. Nineteen new cases extend the prior 967-case gate; counts overlap previous slices and are not added.

The smaller source composition passes **519 checks**, zero failed/skipped, terminal exit 0, 8.3990 seconds. Log: %TEMP%/oilgas-accepted-182-source.log. Its two new cross-reader revision cases run alongside existing source/account refusals. The larger composition retains 339 warnings, with no new changed-source compiler warning. The receiving fixture's explicitly absent prior change actor uses an intentional null annotation rather than adding another nullable warning.

New checks cover actual receiver partitions/global competing consumption/exceeded and rejected capacity; missing/tampered/changed receiver and activity/unit evidence; stale preparation; actual bound HTTP review, complete client, workspace, native allocation and rendered Mud accepted quantities; one content revision across readers with explicit account refusal; and distinct receiver/accountant handoff, real app-RBAC inbox, independent step completion/controller routing and changed-source refusal.

Allocation fixtures now explicitly seed verified prior receiving facts through the Data proof owner. Receiver fixtures remain undecided and execute the actual receiving writer. Existing diagnostic tests retain legacy allocations as explicit source evidence rather than recording an allocation before acceptance. The shared native lifecycle update fixture is reused for the cross-account transition. These fixture changes do not add a production fallback or imply live provider proof.

Physical datasource/isolation/durability, current receiving RBAC/binding and static renderer/auth/JS remain explicit seams. The accountant inbox uses the actual existing SQLite app RBAC path; this is not a business EF lane. Real OIDC/host/DI/browser/keyboard/RTL/accessibility, live provider concurrency and migration application remain unverified. No business schema, SQL, business EF migration, live grant/migration, commit, push or deployment is introduced. Whitespace checks pass.

## Next delivery and remaining roadmap

Establish reviewed company matching policy and the accountant/controller variance and decision workflow through existing company data, API, screen and task owners. Keep quantity/price/tax/rounding/currency policy explicit and independently authorized; do not treat allocation, receiver acceptance or generic workflow approval as financial matching/posting/payment.

Independent corrections/returns, warehouse/stock/valuation, book/period/journal linkage, financial decisions/post/payment, durable recovery/outbox, receipt-specific workflow policy, delegation, broader dashboards/personas, multinational requirements and live host/browser/provider verification remain active. Beep MigrationManager remains the business schema owner. No feature or phase closes.
