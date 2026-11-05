# Implementation 119: Resource level and app permission intersection

## Concrete permission gap

Reviewing the production/maintenance inspection boundary found that existing UserAssetAccessService.CheckAssetAccessAsync matched an active asset, then checked the requested application permission, but did not compare a READ/WRITE/DELETE request with the matched grant's access level. A READ-only resource grant could therefore satisfy a WRITE application permission check. The inspection and other persona mutation gates call this shared owner; UI role visibility cannot repair that API data-boundary gap.

## Shared owner correction

TheTechIdea.Data now owns AssetAccessLevelChecks, which compares the three existing resource operation levels. READ cannot authorize WRITE/DELETE, WRITE cannot authorize DELETE, and DELETE permits the existing lower operations. Missing/unrecognized resource levels refuse. Other app permission identifiers remain the responsibility of the existing application permission owner; they are not assigned guessed READ/WRITE/DELETE meanings.

The existing UserAssetAccessService now requires the actual resource level before application permission evaluation and checks the fresh asset level again after that awaited permission review. A level revoked from WRITE to READ during that review cannot retain WRITE authority from the first read. The refusal reason identifies insufficient current resource authority. App roles/permissions continue to come from the existing app-owned stores and standard identity bridge. No new authorization service, role catalog, fallback grant or permission scope is introduced.

## Work-order admission finding remains open

WORK_ORDER is currently absent from the existing asset-type allowlist, row-policy paths, grant target verifier and hierarchy read set. The current inspection callback consequently refuses that unsupported resource type; the new commands are not yet a certified usable end-to-end persona workflow. Canonical WORK_ORDER has FIELD relationships through WORK_ORDER_COMPONENT, and multiple components may reference multiple fields. Do not add WORK_ORDER merely to the string allowlist, infer a single FIELD from a well/equipment ID, or remove the order permission check to make the screen appear functional.

The next required integration must review actual active compound component identities and every applicable current company FIELD entitlement at grant admission and on fresh resource reads, preserve direct asset denial and actor/company/physical binding, and support a truthful multiple-parent relationship. Extend the current access/row-policy/grant/hierarchy owners; do not create a second work-order permission store. The role-specific maintenance controls and native WORK_ORDER grants need that complete admission path.

## Focused verification

Actual isolated TheTechIdea.Data build: zero warnings/errors, 1.58 seconds. Scripts/Test-WorkOrderListState.ps1: **277 passed**, zero failed/skipped, terminal exit 0, 1.0088 seconds. Twenty new current Data helper cases cover the full operation matrix, case handling, missing/unrecognized grants, separate app-permission identifiers and a WRITE-to-READ level change. The preceding 257 work-order/inspection cases remain green. Log: %TEMP%/oilgas-asset-level-119.log. Whitespace diff check passed.

The gate tests the actual level helper and compiles the current five work-order service owners. It does not compile or execute UserAssetAccessService, application-role stores, real hierarchy/row-policy/grant provider calls, controller/HTTP or browser behavior. The service integration remains source-only until an actual read/permission-review/revocation flow verifies it. Helper tests alone do not prove that roles/resources or a persona are delivered.

No handwritten SQL, business schema/migration, new business EF lane, grants or live writes. Inspection Beep migration/provider/transaction/readback, durable receipts/correction history, independent supervisor acceptance, return-to-service/accounting and the other persona journeys remain open. No phase/persona/goal closure.
