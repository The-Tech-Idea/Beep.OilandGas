# Phase 11 Folder Plan — Identity, Persona, and Access Governance

> Detailed execution plan for Phase 11  
> Companion overview: `../Phase11-IdentityPersonaAndAccessControl.md`  
> Project knowledge base: `../Projects/INDEX.md`
> Data model and migration baseline (must be produced first): `DataModel-And-Migration.md`
> W11-03 code migration artifact: `ClassByClass-Migration-Map.md`
> W11-04 target namespace/file artifact: `Namespace-File-Target-Matrix.md`
> W11 schema validation artifact: `Schema-Contract-UserManagement.md`
> W11 migration evidence artifact: `MigrationManager-Evidence-Checklist.md`

---

## Purpose

Phase 11 introduces a full user management capability with persona-driven user experiences and policy-driven authorization, aligned to existing Beep.OilandGas architecture constraints:

- Web -> ApiService -> domain/data services
- typed client usage from pages
- least privilege and auditable authorization
- no hidden direct domain calls from UI surfaces

This phase starts with a formal data model definition and migration blueprint before service and UI implementation.

---

## Whole-Solution Scope

| Layer | Scope In This Phase |
|------|----------------------|
| Web | role-aware navigation, persona home views, route gating, profile settings UX |
| ApiService | profile endpoints, role/permission assignment endpoints, policy enforcement seams, access decision telemetry |
| UserManagement | canonical security models, permission primitives, policy helpers, role mapping utilities, profile lifecycle service contracts |
| LifeCycle and support domains | field/asset scope-aware authorization checks in sensitive workflow endpoints |
| Models/contracts | migration source for legacy security models and transition compatibility contracts |

---

## Best-Practice Guardrails

1. Use RBAC plus scoped attributes (field, asset, organization) for effective access decisions.
2. Enforce separation of duties for finance, approvals, and user-administration operations.
3. Default-deny when permission or scope signals are missing.
4. Use auditable policy names: `resource.action.scope`.
5. Keep authorization checks at API boundaries even if UI hides routes.
6. Treat persona as UX composition hint, not sole authorization source.
7. Do not trust client-submitted scope claims without server-side validation.
8. Log profile mutations and permission/role changes as immutable audit events.
9. Migrate security models to `Beep.OilandGas.UserManagement` as the canonical home; keep compatibility adapters in `Beep.OilandGas.Models` only for controlled transition periods.

---

## Pass Set

| Pass | Focus | Document |
|------|-------|----------|
| A | Identity and policy baseline | `PassA-IdentityAndPolicyBaseline.md` |
| B | Persona profile and experience implementation | `PassB-PersonaProfileAndExperience.md` |
| C | Access validation, observability, and hardening | `PassC-AccessValidationAndOperations.md` |

---

## Deliverables

- canonical data model and migration document approved before pass B begins
- exact class-by-class migration map for `Beep.OilandGas.Models` security/access classes
- namespace and file target matrix under `Beep.OilandGas.UserManagement`
- schema contract listing table names, primary keys, and JSON payload columns for migration validation
- migration evidence checklist with mismatch register and W11-03 sign-off gate
- persona-to-workflow ownership matrix
- role-to-permission catalog and policy naming standard
- profile API and typed client implementation plan
- role-aware navigation and route gating blueprint
- authorization and escalation regression test matrix
- audit and telemetry checklist for identity/access events
