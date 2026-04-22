# Phase 10 Pass B — Retirement and Dependency Reduction

## Objective

Remove the duplicate and compatibility paths that no longer need to exist after validation is complete.

---

## Whole-Solution Workstreams

| Workstream | Project Groups | Output |
|------------|----------------|--------|
| Route retirement | Web, Branchs, UserManagement | deprecated routes removed or redirected |
| Component retirement | Web, Drawing | duplicate shared components removed |
| Endpoint retirement | ApiService and affected domain modules | obsolete or overlapping API paths retired |
| Dependency reduction | Web, Client, Models, ApiService | direct project references reduced and justified |
| Compatibility note capture | all touched projects | explicit exceptions retained with reasons |

---

## Required Outputs

1. Duplicate page/component retirement list executed or deferred with reasons.
2. Obsolete endpoint list executed or deferred with reasons.
3. Web project reference reduction result.
4. Compatibility/redirect list for remaining transitional paths.

---

## Exit Gate

Legacy overlap should now be intentional and documented, not accidental.
