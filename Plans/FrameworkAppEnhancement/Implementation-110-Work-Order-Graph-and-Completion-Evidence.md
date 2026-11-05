# Implementation 110: Work-order graph and recorded completion evidence

## Changes to the existing workflow owner

WorkOrderService advertises and executes one WorkOrderWorkflow graph. The existing state/subtype constants and InspectionCondition contract now live once in TheTechIdea.Data, retaining their namespace. Unsupported subtypes/states have no offered transitions; terminal states cannot reopen implicitly; safety orders must enter their existing UNDER_REVIEW path before completion. The old execution guard only rejected terminal transitions and one safety case, allowing skipped states outside its own advertised choices.

InspectionService.AllConditionsPassedAsync now requires at least one recorded item for every requested category. Empty/null rows, absent categories, duplicate item sequences, missing identity/text, non-PASS status and absent/default inspection dates return false. A missing checklist no longer means passed. WorkOrderService requires recorded completion evidence across every category present, including a close-out item, before changing state. This establishes a necessary evidence gate; it does not prove an approved template is complete or replace permit/supervisor/resource controls.

Planning now requires the actual recorded jurisdiction before changing state. The previous missing-jurisdiction substitution to USA was removed. Existing seeded template selection is still subtype-based and must be replaced by reviewed jurisdiction/template authority; this change does not certify its regulatory contents for any country.

## Focused evidence

The actual canonical Data build passed with zero warnings/errors, 2.96 seconds. Scripts/Test-WorkOrderListState.ps1 passed **83 cases**, zero failures/skips, terminal exit 0, 1.0919 seconds. This includes 32 new graph/inspection cases and the preceding 51 list/create/detail/request checks. Log: %TEMP%/oilgas-workorder-workflow-110.log.

New cases cover offered destinations, skipped/unknown/terminal states, the safety review path, dated passes, empty/failed/NA/undated evidence, duplicate and malformed items, absent categories, all recorded categories and required close-out. The focused project compiles current canonical workflow/request and actual state/DTO sources, actual Data and cached dependency assemblies; old cached DTO collisions are suppressed with current source precedence. It does not execute the native service/controller/provider or a browser. Wiring from actual WorkOrderService/InspectionService to the shared graph/evidence gate was inspected in source. Whitespace diff check passed.

## Native persistence findings and remaining gates

The actual canonical PPDM PROJECT model does not contain FIELD_ID, WO_SUBTYPE, PROJECT_STATUS, JURISDICTION or EQUIPMENT_ID, while WorkOrderService reads/writes those assumed columns. The service also receives the default startup connection rather than a fresh company module binding. These are concrete integration gaps, not successful provider evidence. Reconcile the existing canonical WORK_ORDER/PROJECT relationships and current service ownership, using Beep MigrationManager for any required business schema extension; do not create a parallel workflow store or patch missing projects with stubs.

Independent supervisor acceptance, current company roles, source resource permission for every child operation, reviewed regional templates, atomic state/version commands, durable receipts, inspection audit identity/history, permit/material prerequisites and cost/return-to-service handoffs remain open. A recorded pass alone is not approval or legal compliance. The changes do not certify Persona 08 or close any phase.

No SQL, new business EF lane, schema/migration execution or grants.
