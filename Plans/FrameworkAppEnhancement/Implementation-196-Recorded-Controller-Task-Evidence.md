# Implementation 196 - Recorded controller evidence for configured tasks

Date: 2026-10-07. Status: configured controller disposition evidence now flows through the existing process review, completion command, native task owner and process detail screen. No persona phase is closed.

## Company workflow and screen behavior

A FINANCIAL AP_INVOICE approval step can explicitly configure `StepConfiguration.AccountingEvidence` as `AP_INVOICE_VARIANCE_DISPOSITION`. The role remains supplied by the company's existing process definition and persona mapping. No fixed Controller role, parallel workflow or assumed live configuration is introduced.

The existing company process reader requires the verified accountant proposal, unchanged original handoff source and verified controller decision. A missing decision returns RECORDED_DISPOSITION_REQUIRED; an actor other than the recorded controller returns RECORDED_CONTROLLER_REQUIRED. Current assigned standard roles, independent source maker/workflow starter checks and Workflow.Approve remain necessary. Administrator does not substitute for the recorded controller identity.

The process detail screen explains the missing disposition or recorded-controller requirement and offers completion only when eligible. This continues the existing API-first typed client and retained step intent path. Completing the task does not match, post or pay the invoice; the source stays DRAFT and its proposal/decision history remains separate.

## Evidence and native command boundary

The canonical Data-owned step review adds an optional AccountingEvidenceRevision. It hashes the current invoice source revision together with exact recorded decision JSON, including the decision command and reason. Both the decision row digest and the existing invoice source digest omit decision JSON; neither alone detects a structurally valid replacement reason. Null is omitted from JSON to preserve existing generic command serialization.

The API rechecks configured evidence before native completion and during historical confirmation/exact retry. A changed decision, source, proof, evidence hash or current action grant refuses confirmation. The existing native lifecycle owner admits only the single supported accounting configuration on an approval AP invoice step, requires the evidence revision on that current step, and preserves rejection of unsupported branching, validation and configuration. Generic unconfigured workflows retain their existing path.

The native command still owns its Serializable LIFECYCLE transaction and exact persisted task/process/history read-back. PRODUCTION invoice reads remain outside that transaction. This is not cross-database atomicity: drift after lifecycle commit can leave an unconfirmed outcome, for which the existing client retains the original intent. No SQL, business EF lane, live migration or role grant was introduced.

## Focused verification

Actual canonical Data builds with zero warnings/errors in 2.09 seconds. Final affected gate passes 1391 checks with zero failures/skips in 1.8628 minutes; the overlapping source gate passes 519 checks in 8.5366 seconds. The composition emits 366 existing warnings. Whitespace verification passes. All command handles reached terminal exit. New cases exercise actual API/typed client/native completion and retry, missing/wrong-controller/administrator refusal, forged eligibility, changed full decision evidence and grants, unsupported configuration, and complete static process screen rendering.

The initial affected run passed 1387 of 1388 cases. The successful configured completion exposed the native owner's blanket configuration rejection; the narrow supported-action check now resolves that production-path gap. A second affected run passed 1390 of 1391 cases: a valid replacement reason demonstrated that the existing source digest also omits decision JSON. The evidence revision now explicitly includes the decision bytes. An initial screen test compile also required System.Uri qualification because NavigationManager has a Uri property.

Fixtures supply configured company roles/personas, current grants and native provider/transaction seams. Static rendering is not browser verification. Live Beep schema/provider behavior, OIDC, browser, accessibility and cross-database consistency remain unverified.

## Next work and remaining roadmap

Expose pending/decided controller work through the existing persona inbox and landing/dashboard pages, using the configured task and authoritative decision evidence. Workflow definition seeding and live company configuration remain open. Continue correction/cancellation/history and lifetime recovery, policy tasks, tax/non-PO and ledger/period evidence, stock/returns, other company personas and multinational requirements. Financial matching/posting/payment needs its own verified evidence and action gates.
