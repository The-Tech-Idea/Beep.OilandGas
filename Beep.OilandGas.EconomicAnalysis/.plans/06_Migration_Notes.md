# Phase 4 Migration Notes

## Objective
Capture migration impact, compatibility boundaries, and rollout safeguards for EconomicAnalysis canonicalization.

## Migration Scope
- Planning baseline introduction for EconomicAnalysis module.
- Potential contract split (canonical vs advanced) if implementation proceeds.
- Potential seed catalog introduction for module-owned economic reference families.
- Potential PPDM persistence expansion beyond `ECONOMIC_ANALYSIS_RESULT`.

## Compatibility Boundaries
- Existing `api/EconomicAnalysis/*` core routes remain stable unless explicitly versioned.
- Advanced methods remain non-canonical until promoted through execution + tests.
- Existing projection response models stay valid unless migration note marks a breaking shape change.

## Risk Register
| Risk | Impact | Mitigation |
|---|---|---|
| Contract drift between service and API | Build/runtime break | Interface status map + boundary tests |
| Seed duplication on rerun | Data integrity issues | Idempotent natural-key upsert pattern |
| Over-promotion of advanced methods | Unstable API behavior | Promotion gate rule in Phase 1 docs |
| Table/projection confusion | Bad writes and schema drift | Explicit class contract matrix in Phase 2 |

## Rollout Checklist
- [x] Apply execution tasks from `04_Execution_Plan.md` in order. (current pass subset completed)
- [x] Run verification gates from `05_Verification_Notes.md`. (module/api builds + focused tests)
- [x] Confirm no unreviewed API surface expansion.
- [x] Record final compatibility notes in PR/merge summary.

## Current Blockers
- None.

## Post-Migration Validation
- [x] Validate core route behavior with smoke tests.
- [x] Confirm persisted economic results remain queryable.
- [x] Confirm seed reruns remain deterministic. (idempotent seed-by-key implementation + catalog tests)
- [x] Confirm no regression in cross-module consumers.

## Exit Criteria
- Migration risks addressed and signed off.
- Compatibility boundaries documented and accepted.
- Rollout checklist fully complete.
