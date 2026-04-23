# Phase 11 Pass B — Persona Profile and Experience

## Objective

Implement profile-driven UX composition and access-control seams without breaking architecture boundaries.

---

## In Scope

- profile and access endpoints in ApiService
- typed web clients for profile and authorization use cases
- role-aware navigation and persona landing pages
- route-level and component-level gating
- profile settings pages (persona, preferences, default field, locale)

---

## API and Service Implementation Plan

| Area | Implementation Target |
|------|------------------------|
| Profile API | endpoints to get profile, update profile preferences, read effective access context |
| Access assignment API | endpoints for role assignment, permission assignment, scoped access assignment with audit |
| UserManagement service | complete role membership and permission resolution paths with no placeholders |
| AccessControl integration | unify policy checks with existing access-control endpoints and role retrieval seams |

---

## Web Experience Plan

| Area | Implementation Target |
|------|------------------------|
| NavMenu and layout | show only persona-allowed route groups; preserve active field context |
| Persona landing | route each persona to its workflow-start dashboard |
| Route guards | block direct navigation to unauthorized routes with clear denied message |
| Page-level gating | disable or hide high-risk actions where user lacks permission/scope |
| Profile settings | create profile UI under account/security routes with audit-aware update workflow |

---

## Best-Practice Controls

1. Keep all enforcement server-side; UI gating is only for user guidance.
2. Resolve effective permissions from roles plus scoped grants, not static persona labels.
3. Denied actions must produce consistent HTTP status and user-facing feedback.
4. Log role and profile changes with actor, target user, timestamp, and before/after values.
5. Do not persist plaintext passwords or weak hashes in any new code path.

---

## Exit Criteria

- profile and access APIs implemented and consumed via typed clients
- role-aware navigation active for all four target personas
- route and action gating active for core persona workflows
- user profile settings available and auditable
- no page-level direct domain authorization logic outside approved client/API seams
