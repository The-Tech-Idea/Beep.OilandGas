# Implementation 120: Recorded work-order resource admission

## Persona workflow and concrete gap

The maintenance inspection screen and production-to-maintenance handoff require independent READ/WRITE authority over the recorded order. Implementation 119 found that WORK_ORDER was absent from the existing grant allowlist, target verifier, resource reader and row policy. This slice extends those existing owners so a direct order grant can be reviewed against the order's actual FIELD components. It is a supporting dependency for Persona 08 maintenance and Persona 12 company administration; it does not certify either complete journey.

## Existing owners and recorded data

RepositoryAssetAccessStore now accepts WORK_ORDER in its existing app-owned grant store. PPDMAssetGrantTargetVerifier resolves the actual canonical WORK_ORDER header and requires the requested level to fit every recorded component FIELD's current company entitlement. UserAssetAccessService reads canonical WORK_ORDER records only when the subject has a direct order grant. FIELD inheritance does not create an order grant, and no WELL/equipment identifier supplies a guessed FIELD.

The existing CompanyPPDMRowAccessPolicy now has a direct WORK_ORDER_ID path, active-header constraints and an effective resource-level review. Generic order mutations remain refused; writes belong to the existing reviewed command owners. CompanyWorkOrderFieldReview uses the existing bounded CompanyPPDMEvidenceReader and canonical metadata to read active WORK_ORDER_COMPONENT rows, validate their compound identities, preserve all distinct FIELD parents, and review each current company entitlement. It fingerprints the complete recorded component population before and after entitlement review and closes actor/company/physical connection authority. Missing, ambiguous, changed or unauthorized scope refuses.

TheTechIdea.Data owns WorkOrderParentAccess, which validates the component population and intersects the direct order level with every reviewed parent level. Several components may share one FIELD; several different FIELD parents are retained. Empty populations, missing FIELD references, mismatched order IDs and duplicate component identities cannot establish authority. An unknown or revoked parent level cannot be discarded. The closing row-policy decision requires the original direct grant's identity, scope, level, active status and concurrency stamp to remain unchanged; a later READ grant cannot preserve an earlier WRITE decision.

No parallel permission store, new role mechanism, automatic grant, single-parent hierarchy assumption or business schema change is introduced. The existing inspection schema continues through the PPDM_CORE Beep MigrationManager manifest. No handwritten SQL, new business EF lane, live grant or migration was executed.

## Focused verification and practical limits

Actual isolated TheTechIdea.Data build: zero warnings/errors, 1.67 seconds. Scripts/Test-WorkOrderListState.ps1: **292 passed**, zero failed/skipped, terminal exit 0, 1.0355 seconds. Log: `%TEMP%/oilgas-workorder-asset-120.log`. Fifteen new actual Data helper cases cover multiple parents, malformed/missing components, direct/parent level caps and later revocation. The preceding 277 cases remain green. The gate now additionally compiles the actual UserAssetAccessService and its actual application authorization/role-permission interfaces; it does not execute the service's native datasource or permission-review flow.

The API grant verifier, repository grant store, CompanyWorkOrderFieldReview and CompanyPPDMRowAccessPolicy changes are source-only in this gate: native metadata/provider calls, direct-grant mutation/revocation during awaited review, component effective/expiry policy, HTTP/controller/DI behavior and actual company-role enforcement require focused integration evidence. The final closing-grant correction is not represented as native API test proof. The prior five actual inspection-component rendering cases remain historical Implementation 118 evidence, not a new full-page/browser result.

## Remaining persona delivery

The current hierarchy/admin screens still need a truthful multiple-FIELD order relationship and reviewed direct-grant administration. Implement approved role-specific maintenance controls, then verify actual company-admin grant -> maintenance landing/task -> recorded order -> inspection result/readback -> independent acceptance -> production return-to-service handoff through the existing screens and API clients. Inspection Beep migration/provider transactions, durable outcomes and corrections, independent approval and accounting remain open. The other persona plans remain active; no phase, persona or overall goal is closed by this slice.
