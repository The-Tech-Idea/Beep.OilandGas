# Implementation 225 — Reviewed inventory-link workflow handoffs

## Procurement to configured role inbox

Verified inventory-link preparation, independent decision and reopened cycles can now enter the existing configured purchase-order workflow. The workflow preserves the original order maker while retaining the complete current source revision. Canonical Data derives original maker lines only from validated original preparation evidence and complete retained cycle history. It reconstructs each current line using only permitted reference, proof, history-cursor and change-audit fields; commercial, creation, membership or unreviewed audit changes refuse handoff. The header retains the existing unchanged-maker requirement.

ApiService source, catalog, start and step paths use this canonical provenance through existing native purchase-order/history owners. Retained preparation, decision or history cursor requires Inventory.View as well as procurement/workflow authority. Current account/company/binding, standard roles and complete permission sets are rechecked. Start's lifecycle transaction rechecks authority without bringing production database reads into that transaction.

The existing procurement landing/detail screen hides the configured handoff panel while receiving, link preparation, decision or cycle work is busy or uncertain. Reloading uncertain cycle work does not make the panel actionable. Confirmed original-intent read-back permits the panel to return. Existing app-owned roles, typed API clients and MudBlazor UI are reused.

## Focused verification

The actual canonical Data build passes with zero warnings/errors. The expanded affected gate passes 650 checks in 1.0512 minutes; the overlapping source gate passes 566 checks in 14.4920 seconds. The final strengthened rendered cycle-screen gate passes nine checks in 9.2776 seconds. All final runs reach terminal exit 0 without failed/skipped tests. Composition warnings remain; whitespace checks pass. These overlapping counts are not summed.

Seven new handoff journeys use the actual typed client, Kestrel API, native purchase and lifecycle services, configured role routing and the existing SQLite app-RBAC inbox fixture. Prepared, resolved and reopened orders retain the original maker and current complete source revision through handoff, inbox and step review. Missing inventory authority, changed archived evidence, altered cleared-line audit and corrupted active decision proof refuse without additional domain writes. Removing Inventory.View refuses subsequent step review. Screen checks confirm that handoff remains hidden after an uncertain reload and returns after confirmation. Existing receiver/accountant, start, step-command and inbox checks also pass.

## Acceptance boundary and next delivery

This is focused fixture and rendered-handler verification. Live provider locking, schema/grant provisioning through Beep MigrationManager, browser/OIDC, accessibility/RTL/responsive behavior and durable circuit recovery remain open. No handwritten SQL, new business EF path, live migration, grant provisioning or physical stock/financial effect is introduced. Generic workflow completion does not apply inventory references or move stock. No phase closes.

Next deliver the warehouse stock/return workflow through existing canonical inventory owners, ApiService, typed clients and persona screens. Require explicit owned item, receipt acceptance, quantity/unit/location, current authority, original intent and source provenance; unknown availability remains explicit. Reference approval or cycle reopening cannot imply receipt stock, returns or ledger posting. Broader company personas and multinational acceptance remain active.
