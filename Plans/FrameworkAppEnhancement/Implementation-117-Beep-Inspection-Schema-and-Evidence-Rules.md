# Implementation 117: Beep inspection schema and recorded evidence rules

## Inspection ownership finding

Maintenance inspection cannot use the current InspectionService mapping. That implementation dynamically writes PROJECT_STEP_CONDITION fields such as COND_SEQ, COND_STATUS and INSPECT_DATE, which are not the canonical entity shape. Canonical WORK_ORDER_CONDITION contains commercial/payment conditions, not an inspection result store. Reusing its description/remark as an implicit PASS/FAIL ledger would invent an undocumented parallel interpretation. Automatic generic/US checklist seeding does not establish a selected company's reviewed jurisdiction or requirements.

This slice supplies the required business schema and evidence contracts before replacing the existing service/API/screen path. It does not deliver a usable inspection command yet and does not certify any checklist from existing rows.

## Canonical Data and Beep Migration registration

TheTechIdea.Data now owns WORK_ORDER_INSPECTION in the existing WorkOrder contract namespace. It extends the existing common PPDM entity/audit base and has an explicit INSPECTION_ID primary key with bounded required FIELD/order/condition identities, condition type/text, reported jurisdiction and requirement owner. Result status/notes/inspector and the recorded UTC inspection timestamp are separate fields. New requirements carry no result, inspector or completion time. Source/audit identity is explicit.

WorkOrderSchema exposes the immutable explicit extension-type manifest. The existing CorePpdmModule.EntityTypes now supplies that manifest, so the existing PPDM_CORE module plan path resolves these types alongside canonical PPDM entities, registers their actual Data assembly and builds the existing Beep MigrationManager plan for types. No new migration runner, business DbContext, handwritten SQL, separate repository or runtime auto-create path is introduced. Existing selected-company connection/manifest/policy/approval/preflight/execution/checkpoint handling remains the migration owner.

WorkOrderInspectionMetadata supplies exact table/type/primary-key and FIELD/WORK_ORDER parent metadata through the existing PPDMMetadataService loading path. It adds the canonical extension to whichever core metadata source the service loads and refuses a duplicate table declaration instead of choosing an alternative owner. This runtime relationship metadata is not proof that physical foreign keys or unique order/sequence constraints were generated or applied. Native Beep migration plan and provider constraints still need verification.

## Requirement and recorded-result rules

AddWorkOrderInspectionRequirementRequest and WorkOrderInspectionRules remain Data-owned. Preparing a requirement needs its original FIELD/order, positive sequence, known condition category, actual requirement text, reported jurisdiction and authenticated maker. UTC creation/audit identity is required. There is no default country, seeded regulatory template, result or inspector.

Recorded evidence review requires exact original FIELD/order, active unique inspection identities and condition sequences, known condition categories and complete requirement provenance. PASS, FAIL or NA needs recorded inspector, rationale and timestamp; an unrecorded requirement cannot carry a hidden result timestamp or inspector. An empty checklist is a valid empty read and is never evidence that all conditions passed. A full content/audit SHA revision changes when either the requirement or recorded result changes; it is prepared for the existing command owner's original-review checks, not a durable receipt or provider compare-and-swap.

## Focused verification

Actual isolated TheTechIdea.Data source build: zero warnings/errors, 1.78 seconds. Scripts/Test-WorkOrderListState.ps1: **229 passed**, zero failed/skipped, terminal exit 0, 1.0340 seconds. Twenty-seven additional cases exercise the actual Data schema/key ownership and module boundary, extension metadata and duplicate-owner refusal, absent inspector/pass defaults, required input/UTC rules, malformed/different/duplicate recorded evidence, content-revision changes and the existing empty-checklist completion refusal. The preceding 202 work-order cases remain green. Log: %TEMP%/oilgas-workorder-inspection-schema-117.log. Whitespace diff check passed.

The gate compiles current actual extension metadata, module-boundary and Data contracts/rules together with the existing focused work-order sources. It does not execute MigrationManager planning or a physical datasource, and it does not compile the parent CorePpdmModule/PPDMMetadataService/DI/HTTP/Razor host. Their wiring is source-only. No migration was applied and no inspection record was created.

## Next required implementation and remaining journey

Replace the existing unsupported InspectionService store path with this single canonical model after verifying generated schema/keys in a disposable provider database. Add explicit reviewed requirement creation through the existing API/client/detail surface; do not retain automatic US/generic template seeding as an alternate persistence lane. Recording results needs current company/FIELD/order WRITE authority, the original requirement revision, rationale, confirmed readback and retained uncertain intent. Corrections need recorded reason/history and durable command outcomes; no blind overwrite or automatic retry. Independent supervisor acceptance remains separate from requirement creation and technician result recording.

Connect inspection review/failure/confirmed absence independently to recorded header detail so a failed checklist request cannot erase the header or masquerade as a successful empty list. Native migration plan/provider constraints, resource and role revocation, transaction/concurrency/rollback, HTTP and rendered browser/keyboard/RTL remain open. Permit/material evidence, explicit process linkage, approved planning, return-to-service and accounting handoff remain open alongside other persona journeys. No persona, phase or goal closure.
