# Phase 10 Pass A — Validation and Coverage

## Objective

Prove that the new route, client, API, domain, and workflow structure works across the full solution before anything is retired.

---

## Whole-Solution Workstreams

| Workstream | Project Groups | Output |
|------------|----------------|--------|
| Build and compile validation | all solution projects | build matrix and blocker list |
| Route and page validation | Web, Branchs, UserManagement | route ownership and access validation |
| Client/API boundary validation | Web, Client, ApiService, Models | typed-client and endpoint coverage proof |
| Domain-service validation | operational and support-domain projects | owner map validated against actual code |
| Persistence and governance validation | PPDM39, PPDM39.DataManagement, DataManager | state, audit, validation, and persistence checks |

---

## Required Outputs

1. Validation checklist by project group.
2. Build/run blocker list.
3. Route and authorization coverage report.
4. Typed-client and endpoint coverage report.
5. Domain-ownership mismatch list, if any.

---

## Exit Gate

Only validated paths can move to retirement in Pass B.
