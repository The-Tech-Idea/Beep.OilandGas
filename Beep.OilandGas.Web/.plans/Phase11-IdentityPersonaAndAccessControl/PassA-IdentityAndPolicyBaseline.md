# Phase 11 Pass A — Identity and Policy Baseline

## Objective

Create the canonical access model that drives both UX composition and enforcement for user actions.

---

## In Scope

- persona catalog and ownership matrix
- role and permission taxonomy
- access scope model (field, asset, organization)
- profile schema and lifecycle states
- policy naming and evaluation conventions

---

## Workstreams

| Workstream | Output |
|-----------|--------|
| Persona modeling | Persona registry with allowed route families and default landing views |
| Role and permission model | Role-to-permission matrix and separation-of-duties constraints |
| Scope model | Rules for field and asset restricted workflows |
| Profile contracts | DTO and API contract draft for profile read/update and effective access context |
| Policy catalog | Named policies for web and API endpoint enforcement |

---

## Persona Baseline Matrix

| Persona | Default Landing Area | Example Allowed Route Families | High-Risk Restricted Operations |
|--------|-----------------------|--------------------------------|---------------------------------|
| Reservoir Engineer | Reservoir summary | reservoir, reserves, EOR, characterization | user admin, permission changes, accounting approvals |
| Petroleum Engineer | Production/development workbench | development, drilling, production, interventions | user admin, financial close, role assignment |
| Accountant | Accounting dashboard | accounting, allocation, royalties, AFE financial closeout | drilling edits, geology edits, user admin |
| Geologist | Exploration workspace | exploration, prospects, seismic tracker | accounting postings, role assignment, privileged admin |

---

## Policy Baseline

Use policy names in this format:

- `reservoir.view.field`
- `production.optimize.well`
- `accounting.approve.period`
- `security.manage.users.global`

Policy decisions must evaluate:

1. Role permissions
2. User status (active/disabled)
3. Scope match (field, asset, organization)
4. Optional business-state constraints (period closed, approval stage)

---

## Exit Criteria

- approved persona catalog for the first four personas
- approved role-to-permission matrix
- approved scope model and policy naming standard
- draft profile and access-context contract set
- baseline risk register for separation-of-duties and escalation vectors
