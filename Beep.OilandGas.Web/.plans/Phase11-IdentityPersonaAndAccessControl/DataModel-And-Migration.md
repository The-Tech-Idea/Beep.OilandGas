# Phase 11 Data Model and Migration Baseline

> Phase: 11 Identity, Persona, and Access Governance  
> Prerequisite: This document is the first implementation artifact for W11-02 and W11-03.  
> Scope: Canonical identity/access data model, model enhancement targets, and migration strategy from `Beep.OilandGas.Models` to `Beep.OilandGas.UserManagement`.
> Schema contract artifact: `Schema-Contract-UserManagement.md`

---

## Why This Comes First

Before implementing APIs, UI gating, or role-aware navigation, the solution needs one canonical model ownership boundary and an explicit migration path.

Current state shows security entities and contracts spread across:

- `Beep.OilandGas.Models/Data/Security/*`
- `Beep.OilandGas.Models/Data/AccessControl/*`
- `Beep.OilandGas.Models/Core/Interfaces/Security/*`
- `Beep.OilandGas.UserManagement/*` service and security handlers

Target state:

- user management domain models live in `Beep.OilandGas.UserManagement` as canonical identity/access models
- shared contracts needed by other projects are exposed through controlled DTO contracts and compatibility adapters

---

## Persona Coverage Baseline

Primary rollout personas:

- Reservoir Engineer
- Petroleum Engineer
- Accountant
- Geologist

Extended enterprise personas:

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
- Access Administrator

---

## Canonical Entity Set

### Core identity entities

1. `AppUser`
2. `AppRole`
3. `AppPermission`
4. `AppUserRole`
5. `AppRolePermission`

### Persona and profile entities

6. `UserPersonaProfile`
7. `PersonaDefinition`
8. `PersonaViewPreference`

### Scope and access entities

9. `UserScopeAssignment`
10. `UserAssetAccess`
11. `OrganizationScope`

### Audit and governance entities

12. `UserAccessAuditEvent`
13. `UserProfileAuditEvent`
14. `AuthorizationDecisionTrace`

---

## Model Enhancements Required

For migrated models, add or normalize the following fields where missing:

- stable `Id` and optional external identity key
- lifecycle state: Active, Disabled, Locked, PendingActivation
- security timestamps: CreatedUtc, UpdatedUtc, LastLoginUtc, LastPasswordChangeUtc
- actor and source metadata for mutation tracking
- soft-delete and effective-date support where required
- concurrency token (for profile and role assignment updates)

For profile model specifically:

- primary persona
- secondary personas (optional)
- default landing route
- default field and asset context
- locale and time zone preferences
- notification preferences
- effective access context snapshot (derived, not blindly client-writable)

---

## Permission and Policy Model

Policy naming convention:

- `resource.action.scope`

Examples:

- `reservoir.view.field`
- `production.optimize.well`
- `accounting.close.period`
- `security.manage.users.global`

Authorization decision inputs:

1. role permissions
2. user lifecycle state
3. scope assignment (field/asset/org)
4. business-state constraints (optional, e.g. period status)

---

## Migration Source Inventory

Primary source files currently in `Beep.OilandGas.Models`:

- `Data/Security/USER.cs`
- `Data/Security/ROLE.cs`
- `Data/Security/PERMISSION.cs`
- `Data/Security/USER_ROLE.cs`
- `Data/Security/ROLE_PERMISSION.cs`
- `Data/Security/USER_PROFILE.cs`
- `Data/Security/USER_ASSET_ACCESS.cs`
- `Data/Security/PermissionConstants.cs`
- `Data/AccessControl/*` (profile/access DTOs and request contracts)
- `Core/Interfaces/Security/*`

---

## Migration Strategy

### Step 1: Introduce canonical models in UserManagement

- create `Beep.OilandGas.UserManagement/Models/` with canonical entity names and mappings
- preserve table mappings to existing persisted schema where possible

### Step 2: Add compatibility layer

- create adapters/mappers between legacy model types and new canonical models
- keep old public contracts temporarily where cross-project dependencies still exist

### Step 3: Update service ownership

- refactor `UserManagementService` to use canonical models and complete placeholders
- remove incomplete role-assignment placeholders and enforce real role resolution logic

### Step 4: Update API contracts and typed clients

- expose profile, role, permission, and scope endpoints based on canonical models
- ensure web typed clients consume only API contracts (not direct domain model coupling)

### Step 5: Deprecate legacy model paths

- mark legacy security models in `Beep.OilandGas.Models` as compatibility-only
- remove legacy paths after dependency matrix confirms no active references

---

## Risks and Controls

| Risk | Control |
|------|---------|
| Model duplication during migration | enforce one canonical namespace for new development |
| Breaking downstream references | staged compatibility adapters and deprecation periods |
| Inconsistent permission decisions | centralize policy evaluator and permission resolution logic |
| Privilege escalation through stale scopes | validate scope server-side on each sensitive operation |
| Audit gaps | immutable audit events for profile and role/permission changes |

---

## Definition of Done for Data Model Baseline

- canonical entity list approved
- migration source inventory completed
- namespace ownership decision recorded (`UserManagement` canonical)
- compatibility strategy documented with deprecation checkpoints
- enhancement fields and policy naming standard finalized
- W11 implementation tasks reference this document as baseline
