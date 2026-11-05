# Implementation 157: Independent purchase-to-pay review definition

## Existing buyer, receiver and accountant workflow

The canonical ACCT_PROCURE_TO_PAY definition now describes invoice/receipt/PO matching as an assigned independent review rather than a SYSTEM action with assumed quantity/price tolerances. The existing THREE_WAY_MATCH identity, sequence and next invoice approval step are preserved. It is a required REVIEW with RequiresApproval and the existing Accountant role. The definition version is 1.1.0.

The requisition, PO, goods receipt, invoice preparation, invoice approval and payment chain remains in the existing process owner. No parallel accounting process, role catalog, workflow task model or financial store is added. The definition's descriptions explicitly require authoritative receipt acceptance, reviewed company matching policy and separation from the invoice preparer; recorded source comparisons alone do not approve a match. The fixed 5% quantity and 2% price defaults are removed from this definition.

OperationalAccountingWorkflowSeed exposes BuildProcureToPayDefinition as the single factory used by its existing initializer. Each call produces independent mutable step/role/configuration collections, so editing one definition cannot mutate another factory result. The definition remains consumable by existing process catalog/detail and assignment owners; no live instance or task is created in this slice.

## Standard role and independence boundary

The existing ProcessStepRoleChecks recognizes financial REVIEW steps and RequiresApproval as requiring an independent actor. Focused tests with the actual canonical definition verify Accountant and Administrator self-review refusal when the actor is the recorded process initiator, refusal without a confirmed initiator, Accountant review by another actor and refusal of an unrelated Manager role. The existing administrator role behavior is retained for an independent actor.

This is the recorded process-initiator gate. It does not yet prove separation from an invoice preparer who differs from that initiator. The controller's future company accounting source review must resolve the owned invoice's preparer and exact submitted intent, independently enforce current company roles/permissions and acceptance/policy, then admit the matching decision. Neither this seed metadata nor its descriptions implement financial commands or concurrent receipt reservation.

## Stored definition rollout

The production CreateProcessDefinitionIfNotExistsAsync still skips existing definitions. No stored definition or role assignment has been changed, and no live initialization has been executed. Existing definitions with the old SYSTEM/default description need a reviewed upgrade and active-instance reconciliation. Do not silently overwrite historical definitions or assume current stored metadata matches the new factory.

The existing task inbox remains FIELD/process-bound. The company-wide accounting process/source access and persisted preparer-to-reviewer routing remain required; assigning an AP invoice to a selected FIELD without authoritative association would fabricate scope. Connect these requirements in existing process/task owners before exposing submission or completion.

## Focused verification

The extended Test-ProcessStepRoles.ps1 gate passes **32 tests**, zero failed/skipped, terminal exit 0, in 0.8902 seconds, with zero compiler warnings/errors. Log: `%TEMP%/oilgas-matching-review-definition-157.log`. Nine new definition cases extend the previous 23 role/transition cases. They cover canonical step semantics without assumed tolerances, six actual standard-role/initiator combinations, sequence/approval/payment continuity, independent factory collections and the actual operational initializer's canonical definition/actor dispatch. Whitespace verification passes.

The complete actual operational seed file and actual role helper compile in this gate. A test-only partial boundary captures definition persistence calls; it is enabled only by ACCOUNTING_WORKFLOW_SEED_FOCUSED, so it does not shadow the production initializer in normal test builds. It does not replace or prove the production persistence service, skip-existing behavior, native provider or live initialization. The unchanged allocation/source screen retains the prior 437-case/49-HTTP focused evidence; the two scopes are not summed.

Full production initializer/process persistence, company accounting source/controller/assignment, active definition upgrade, browser/OIDC/RTL/accessibility and real provider acceptance remain unverified. No SQL, EF path, business record, live workflow, migration, grant or deployment is performed.

## Next handoff

Establish the company-scoped accounting process/source boundary, persisted draft/submission identity and actual invoice-preparer separation. Connect authoritative receipt acceptance and reviewed company policy, concurrent consumption, durable reviewer task routing and recorded/not-recorded/uncertain reconciliation. Independent match/approval/post/payment and buyer/receiver/stock outcomes remain open. This corrects the target workflow definition; no persona journey or roadmap phase closes.
