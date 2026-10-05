# PPDM Wizard Review - 2026-10-01

## Fixed

1. Creation routes had diverged: Target changed the global current connection,
   Foundation invoked direct schema creation, while Modules used a reviewed plan.
   All creation routes now resolve to the same Administrator-only module setup
   page. Connection registration remains available through Manage Connections;
   schema installation uses the selected module binding and approved BeepDM plan.
2. The old Summary read circuit state that module setup never updated. Removed
   those pages and their unused state service rather than showing stale success.
3. Connection management lacked an explicit Administrator route guard. Added it;
   API authorization remains independently required.
4. Plan review was a large JSON field. Added target/policy/count summaries, a paged
   operation/risk table, four workflow sections, and collapsible full diagnostics.
   PPDM_CORE is the initial module when available. No database is auto-selected.
5. Changing environment or backup/restore evidence could leave an old approval
   visible. Changes now clear review and approval and require a fresh plan.
6. The connection dialog blocked on asynchronous calls and loaded driver/script
   data too late. Next is asynchronous, guarded against overlap, and loads data
   before entering the corresponding step.

## Verification

`TestResults/ppdm-wizard-review.log`: 165 Web tests passed, zero skipped.
Tests cover unique Administrator-only routing for all seven setup entry routes,
connection management authorization, and invalidation of review/approval on each
evidence change. The Web project compiled with existing warnings elsewhere.
No schema creation, seeding, or database deletion was triggered by these tests.

## Remaining Findings

- Runtime validation is blocked by a missing `BackchannelLogoutRevocations` table
  in the configured diagnostics database. The Web startup check failed and its
  process exited. Apply the diagnostics project's appropriate pending migration
  through its normal installation path before browser testing. No authentication
  check was disabled. See `TestResults/ppdm-wizard-web.stdout.log`.
- The separate `PPDM39DatabaseWizard` connection dialog still has script-based
  schema/seed stages. It should be split into connection registration and navigation
  to approved module setup; that larger change and its backend endpoint retirement
  have not been performed here. Demo setup is not part of the consolidated wizard.
- Execution uses a request/response operation. A dropped connection produces an
  uncertain outcome, not automatic retry. Durable status/recovery presentation and
  authenticated desktop/mobile browser verification remain follow-up work.

The default LocalDB Identity/RBAC repository is independent of the selected PPDM
module target and was not modified by this wizard change.
