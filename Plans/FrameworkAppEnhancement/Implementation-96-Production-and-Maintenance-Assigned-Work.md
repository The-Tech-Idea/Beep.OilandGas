# Implementation 96: production and maintenance assigned work

Updated 2026-10-06. Advances production engineer, operator/shift supervisor and maintenance landing screens using the same company-bound assigned-work journey as accounting. Partial source integration; full persona handoffs remain open.

## Existing screens connected

ProductionEngineerWorkspace (/production/engineer-workspace), ProductionManagerDashboard (/production/operations) and WorkOrderDashboard (/ppdm39/workorder) now include the existing PersonaTaskInbox. Each presents current authorized pending work, approval/review/data-entry and overdue/critical summaries, filter drill-downs, actual due dates and exact recorded source-process links through the same existing API/client/state owners. WorkOrderDashboard now has a standard authenticated page guard.

No second queue, task repository, process source page or persona store is created. Each source link reaches the existing recorded process/detail/action review journey. This connects landing -> assigned source; it does not certify an operational request, permit, material reservation, execution completion or next-role handoff.

## Workspace distinction

PersonaTaskInbox accepts the landing's known route set. PersonaTaskInboxState checks the current eligible catalog persona's DefaultRoute before requesting or publishing that landing's assigned work. A different workspace does not fetch a queue or manufacture zero counts; the screen directs the user to choose an eligible company workspace. A changed route while a request is pending discards its response. The general task inbox stays available for the current eligible workspace without a page-route restriction.

AccountingDashboard also supplies its two actual route aliases. Catalog route matching is navigation/display eligibility, not role authority. It neither changes persona preference nor grants a role. The existing API independently requires selected company/workspace roles and FIELD/entity resource rights. No hardcoded production/maintenance persona codes or guessed role assignments are added.

## Focused evidence

Test-PersonaTaskInboxState.ps1 compiled actual state/context/test sources against actual Data: **28 passed**, zero failures/skips, terminal exit 0, 0.8959 seconds. Log `%TEMP%/oilgas-persona-landing-queues.log`. Six new cases prove different landing route makes zero requests, each of the three landing routes can expose its catalog-matched source queue, route change suppresses late publication, and general inbox operation remains available. Previous twenty-two scope/count/drill-down cases pass. Route fixtures test navigation matching, not actual seeded production role/catalog mappings. Whitespace diff check passed.

Parent pages/shared component Razor/DI/native API/current catalog and rendered keyboard/RTL/mobile flows remain source-only; these state tests do not certify them. Existing production/work-order operational reads and commands are unchanged by this slice and require their own company/provider/authority/data audit.

## Full persona continuation

For production, verify measurement source/units -> diagnosis/recommendation -> independent review -> actual intervention/work-order handoff. For operator/supervisor, verify recorded observations/tests, exception ownership and shift handover. For maintenance, verify actual order -> planning/materials -> approved permit -> execution evidence -> independent return-to-service -> recorded outcome/cost handoff. Drill into contributing records using identical filters, with explicit missing/stale/restricted data and durable uncertain command recovery.

Native current-company role/catalog mappings, same IDs in two companies, role revocation, source API denial, reassignment/rejection and rendered source/confirmation/reload remain open. Accountant approval/post/ledger/period gates remain open too; advancing these persona landing screens does not shrink or close that scope.

No SQL, schema change, business EF lane, grants, fabricated operational data or phase/goal closure.
