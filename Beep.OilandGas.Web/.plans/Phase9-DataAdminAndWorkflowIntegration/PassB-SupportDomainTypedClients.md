# Phase 9 Pass B — Support-Domain Typed Clients

## Objective

Replace repeated raw HTTP usage and blurred service ownership with explicit typed clients for the support domains.

---

## Whole-Solution Workstreams

| Workstream | Project Groups | Output |
|------------|----------------|--------|
| Data admin clients | Web, ApiService, PPDM39, PPDM39.DataManagement, DataManager | typed admin clients for setup, schema, quality, audit, versioning |
| Workflow clients | Web, ApiService, LifeCycle, Branchs | typed workflow/process clients |
| Compliance and permits clients | Web, ApiService, PermitsAndApplications, PDF rendering | typed permit/compliance seam |
| Finance clients | Web, ApiService, ProductionAccounting, Accounting | typed finance and accounting seam |
| Security and navigation support | UserManagement, Branchs, Client, Models | role-aware service contracts and navigation stability |

---

## Required Outputs

1. Typed-client map for every support-domain page family.
2. Reduced raw `ApiClient` usage on complex admin/workflow pages.
3. Stable contracts for finance, permit, and admin actions.
4. Clear ownership of reusable admin/workflow components.

---

## Exit Gate

Complex admin, workflow, permit, and finance pages should no longer be assembled from ad hoc HTTP calls when a typed service seam exists.
