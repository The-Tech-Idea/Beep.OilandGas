# Phase 11 — Identity, Persona, and Access Governance

> Status: Planned  
> Depends On: Phases 8 through 10  
> Goal: Deliver a production-ready user management system with persona-based views, least-privilege authorization, scoped access to field and asset data, and full auditability.
> Detailed subplans: `Phase11-IdentityPersonaAndAccessControl/README.md`
> Data model and migration baseline: `Phase11-IdentityPersonaAndAccessControl/DataModel-And-Migration.md`

---

## Objective

Introduce a complete user and profile system for the oil and gas application that supports:

- persona-specific experiences for Reservoir Engineer, Petroleum Engineer, Accountant, and Geologist
- role and permission enforcement across web, API, and data access boundaries
- field and asset scope restrictions
- secure profile lifecycle management and auditable authorization decisions

---

## Personas In Scope

Primary personas for first rollout:

| Persona | Primary Views | Core Access Scope |
|--------|----------------|-------------------|
| Reservoir Engineer | Reservoir dashboard, reserves, EOR screening, characterization | reservoir, simulation inputs, reserves workflows |
| Petroleum Engineer | development, drilling, production optimization, interventions | well lifecycle, production workflows, intervention planning |
| Accountant | production accounting, royalties, allocations, AFE closeout | accounting periods, financial workflows, approved production summaries |
| Geologist | exploration, prospect maturation, seismic tracking, subsurface context | exploration assets, prospect workflows, geoscience metadata |

Extended personas for oil and gas enterprise coverage:

- Production Engineer
- Drilling Engineer
- Completion Engineer
- Facilities/Process Engineer
- HSE Specialist
- Regulatory/Permitting Specialist
- Land and Lease Analyst
- Petrophysicist
- Geophysicist
- Operations Supervisor
- Maintenance Planner
- Field Operator
- Production Accountant Lead / Finance Controller
- Data Steward
- Asset Manager / Reservoir Manager
- Executive Viewer
- Access Administrator (least-privilege security admin role)

---

## Pass Plan

### Pass A — Identity and Policy Baseline

- define canonical persona, role, permission, and scope model
- define policy naming standards and separation-of-duties rules
- establish profile schema and audit event contracts
- produce data model and migration blueprint before implementation work starts

### Pass B — Persona Profiles and Experience Layer

- implement profile management endpoints and typed clients
- add role-aware navigation and persona default landing pages
- add persona-specific view composition and route gating

### Pass C — Access Validation and Operations Hardening

- validate policy enforcement, denied-access behavior, and exception handling
- add observability for authorization decisions and profile updates
- complete hardening checklist for credentials, tokens, sessions, and escalation risks

---

## Todo

| ID | Task | Status |
|----|------|--------|
| 11.1 | Define expanded persona catalog (primary and extended personas) with ownership matrix | Planned |
| 11.2 | Publish canonical identity/access data model and migration blueprint as implementation baseline | Planned |
| 11.3 | Migrate user management security models from `Beep.OilandGas.Models` to `Beep.OilandGas.UserManagement` and define compatibility strategy | Planned |
| 11.4 | Enhance migrated models for persona profile, scope context, lifecycle state, and audit metadata | Planned |
| 11.5 | Implement profile and access-control API seams with auditing | Planned |
| 11.6 | Add typed web clients for persona/profile/access operations | Planned |
| 11.7 | Implement role-aware navigation and persona landing routes | Planned |
| 11.8 | Implement persona-specific page gating and field/asset scope checks | Planned |
| 11.9 | Add automated authorization, denial, and escalation regression tests | Planned |
| 11.10 | Complete hardening and finalize exception register | Planned |

---

## Exit Criteria

- every supported persona has a documented default route set and allowed workflow matrix
- role-to-permission mappings are versioned and auditable
- API endpoints enforce both role permissions and field/asset scope where required
- denied-access behavior is consistent and user-friendly
- profile changes and authorization decisions are observable and traceable
- security hardening checks are complete with no known privilege escalation path
