# Implementation 247 — Commercial terms configured workflow handoff

## Persona screen and workflow

The existing commercial terms workbench now offers the shared workflow handoff panel for an undecided recorded preparation when the selected account has Workflow.Start. The panel uses the original sales contract identity and a stable digest of the frozen preparation, rather than the digest that changes when an independent decision is added. Existing retained handoff state, catalog review and typed API commands are reused.

SALES_CONTRACT is supported by the existing company accounting process binding, source review, start catalog, step review, task route and inbox admission paths. Source ownership remains with CompanyFieldRevenueReads and ProductionAccounting: the exact active seller-owned native contract must contain valid recorded terms proof, which resolves its original FIELD, date window and revenue transaction. The normal current company role, Accounting.View, FIELD READ, module connection/binding and complete source checks still apply. No alternate commercial entity or source store was created.

Workflow definitions must be FINANCIAL with exactly one approval step, one configured reviewer role and StepConfiguration[CompanyAccountingStepEvidence.ConfigurationKey] = SALES_CONTRACT_TERMS_APPROVAL. The step requires approval and StepType APPROVAL. Both API review and the existing native lifecycle start owner validate this shape. Unsupported definitions refuse before creating process/task rows. No definition or grant is seeded or assigned by this change; recorded company configuration determines the actual role and persona destinations.

## Native decision and task completion

Preparation must be undecided when starting a new approval handoff. Process source identity remains the original preparation digest after a decision. Step eligibility separately carries the full recorded preparation/decision digest.

Completing this commercial approval task requires an actual native Approve decision by the current reviewer, current ProductionAccounting.Approve and Workflow.Approve, the configured standard role, existing maker/starter separation, and unchanged original source, definition, binding and process evidence. A rejection, missing decision, another reviewer, revoked grant or changed source cannot complete approval work. The task page now explains these commercial eligibility states and requires Accounting.View for sales contracts.

The existing owned LIFECYCLE transaction records process/step/history/approval/task changes. Native PRODUCTION evidence is checked through its canonical owner before and after the lifecycle operation; these are separate module transactions, not an atomic cross-module snapshot. Exact recorded completion retries add no lifecycle write. Native contract preparation and decision remain separate commands; a task does not fabricate either outcome.

## Focused verification

Canonical Data builds with zero warnings/errors. The focused actual-source/Razor composition passes 271 tests, zero failures/skips: FieldRevenueSourceTests, CompanyProcessStart tests and CompanyStep tests. Seven new cases include six real typed-client/local Kestrel/native workflow journeys and one rendered commercial handoff-control case. They cover approval and exact completion retry, rejection, different reviewer, revoked commercial approval grant, source drift and unsupported definition. Existing invoice and purchase-order start/step regression stays green within this gate. Cached source/dependency composition warnings remain; full solution and browser acceptance are not claimed.

Reproduce with Scripts/Test-PurchaseOrderOwnership.ps1 using the actual canonical Data DLL, IncludeWorkflowInbox and filter FullyQualifiedName~FieldRevenueSourceTests|FullyQualifiedName~CompanyProcessStart|FullyQualifiedName~CompanyStep. The script includes the exact commercial process partial. Native writes occur only in focused fixtures.

A broader CompanyAccountingHandoff rendering selection produced three purchase-order fixture failures: missing standard authorization services, and after a temporary fixture experiment its workbench did not expose the expected handoff state. That experiment was reverted. These purchase-order screen fixtures are outside this commercial gate and remain unresolved; the broader selection is not certified green.

## Remaining execution and acceptance

Commercial inbox admission and task routes are extended, but a real selected-persona inbox HTTP/rendered journey has not yet been certified. Next connect the assigned process screen to its exact recorded commercial source and preparation/decision workbench, then verify configured persona inbox to source review, decision and completion through the visible screens. Reviewer navigation must preserve recorded FIELD/window/transaction and require current authority rather than guess dates or choose another source.

Rejection/correction routing, amendment/version history, durable circuit-loss recovery and reconciliation of an uncertain start after process progression remain open. The current shape is one approval task sequence for the stored preparation. Invoice/recognition, royalty/partner queues and the wider thirteen-persona/multinational work remain in the master plan. Real provider schema/isolation/concurrency, OIDC, browser, RTL and accessibility gates remain open. No CA/FG phase or overall goal closes.

No handwritten SQL, new business EF lane, live schema/business write, deployment or commit/push was executed. Schema changes continue through the existing Beep Migration path.
