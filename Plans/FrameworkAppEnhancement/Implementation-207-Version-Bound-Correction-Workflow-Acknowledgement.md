# Implementation 207 — Version-bound correction workflow acknowledgement

## Accountant to independent reviewer

The existing company accounting step command now supports a configured source-correction work step followed immediately by an independent correction acknowledgement step. Shared APInvoiceCorrectionStepEvidence lives in TheTechIdea.Data and is carried in the existing CompanyAccountingStepReview/retained command. It binds the action, company, actor, invoice, correction version/intent, preparer/source maker, exact current source/history/financial revisions and PRODUCTION binding. Acknowledgement additionally binds the exact completed correction-work intent.

No invoice acknowledgement column or second store is added. The existing native LIFECYCLE workflow owner records the completion, task closure, next role task, process/step audit and approval record under its owned Serializable Beep transaction. Acknowledgement uses the distinct CORRECTION_ACKNOWLEDGEMENT / ACKNOWLEDGED / ACKNOWLEDGE_CORRECTION audit values. The retained acknowledgement command preserves the version and preceding work intent. Invoice matching, posting and payment status remain unchanged.

## Current evidence and configuration

The API resolves fresh authoritative PRODUCTION roles/grants and binding separately from the combined PRODUCTION/LIFECYCLE process binding. Complete current invoice, PO, allocations, global receipt contributors and matching policies are captured through the existing native read owners. Original controller correction request, verified correction history, DRAFT/Y source and original production binding are required.

Correction work requires the original correction preparer, current correction grant and Workflow.Execute. Independent acknowledgement requires current review grant and Workflow.Approve and excludes source maker and correction/proposal preparer. Standard app roles independently gate the process/action. The step digest explicitly includes the accounting evidence revision and correction evidence; it does not rely on tuple serialization.

Completing correction work requires the immediately following configured acknowledgement approval step. Missing configuration is shown as CORRECTION_ACKNOWLEDGEMENT_REQUIRED before eligibility, and the native owner independently refuses forged eligible submissions. Acknowledgement requires the preceding completed work intent and exact same correction version/intent/history/source. A newer correction after work completion is shown as CORRECTION_WORK_EVIDENCE_REQUIRED and cannot reuse earlier work evidence.

Changed source, current financial contributors, binding, grants or version refuse the submitted old review before lifecycle writes. Current PRODUCTION evidence remains outside the LIFECYCLE transaction. Post-commit contributor drift cannot publish confirmation; restoring exact original evidence permits reconciliation with the retained original intent and no additional writes. This is scoped proof of confirmation behavior, not a cross-database atomic transaction guarantee.

## Screens and landing pages

The existing process detail displays correction version, intent, preparer and current source/financial revisions with explicit acknowledgement/source-work wording. Its existing reviewed-step command handles the action and recovery. Shared assigned-work landing/inbox panels show correction versions and separate acknowledgement/work configuration blockers with filters. The invoice diagnostic panel directs users to the source process for recorded workflow state instead of claiming acknowledgement is globally pending.

Current role definitions/persona assignment remain authoritative. Live workflow definition or grant provisioning is not performed. Existing definitions without the acknowledgement step cannot close correction work until explicitly configured.

## Verification

Actual canonical Data builds with zero warnings/errors in 2.85 seconds. The affected gate passes **1608 checks in 4.4168 minutes**. After the final evidence-digest, screen-status and recovery refinements, **83 related workflow/UI checks pass in 1.2440 minutes**. The final new-feature gate then passes **all 17 acknowledgement/canonical checks in 1.0002 minutes**, including actual process-screen completion and retained receipt/intent confirmation. These gates overlap and their scopes differ; the final 17-case run verifies the added actual completion-handler assertion. The overlapping source gate passes **519 checks in 9.5970 seconds**. Composition reports 376 source/cache/package warnings. All launched command handles reached terminal exit and whitespace verification passes.

Seven canonical cases bind exact latest correction version/intent, actor, binding, action and digest. Ten actual native/Kestrel/typed-client workflow cases cover configured handoff/acknowledgement, exact retry, changed contributors/grants/binding/version, maker/preparer refusal and absent follow-up configuration. The complete static process screen invokes both review and completion handlers through the existing scoped step owner/client/API/native persistence and confirms its retained command/receipt. Post-commit drift remains unconfirmed, then exact original recovery adds no writes. Invoice status stays DRAFT and acknowledgement adds no AP writes.

Two earlier affected checkpoints had one UI wording assertion failure each: 1590 of 1591 passed in 4.5533 minutes and 1600 of 1601 passed in 4.7678 minutes. Controller-disposition wording was restored for that existing action, with distinct correction wording; the later affected and targeted gates pass.

Native provider/physical transactions and authoritative access capture remain fixture seams. Provider/OIDC/browser/keyboard/accessibility, live schema/grants/definition provisioning, durable circuit recovery and broad solution acceptance remain open. No SQL, new business EF lane or live migration is introduced; this feature reuses existing Beep workflow persistence.

## Next implementation target

Implement an explicit version-drift return to the original preparer and reopen configured correction work when a newer correction appears after its work step has closed. The current refusal intentionally cannot confirm that version with the preceding work intent. Then continue procurement/inventory stock and return workflows, remaining personas/dashboards and multinational acceptance. Tax/non-PO/book/period/journal evidence is still required before financial approval; circuit lifetime recovery and live provider/provisioning gates remain active. No phase closure is claimed.
