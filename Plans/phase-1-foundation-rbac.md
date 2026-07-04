# Phase 1 — Foundation: RBAC Hardening

> **Status:** Not Started | **Depends on:** None | **Est. Effort:** 2–3 weeks
> **Target:** Explicit persona-role mapping, field-scoped authorization, permission audit, role hierarchy, temporary elevation

---

## Objectives

1. Bridge the gap between the Persona system (UI/UX) and the Role system (API authorization) via an explicit `PERSONA_ROLE` join table
2. Enforce field-level access scoping — a user with "Production.View" should only see data for fields they're assigned to
3. Close every permission gap: every controller action has an authorization check
4. Implement role inheritance so Manager automatically includes Viewer permissions
5. Support temporary role elevation for acting-manager / leave-coverage scenarios

---

## Current State vs Target State

| Aspect | Current | Target |
|--------|---------|--------|
| Persona ↔ Role | Implicit, disconnected | Explicit PERSONA_ROLE table |
| Field Scoping | `VALID_FIELD_SCOPE` column unused | Enforced on every data query |
| Controller Auth | Partial coverage | 100% of actions authorized |
| Role Inheritance | Not implemented | Manager : Viewer, Admin : Manager |
| Temp Elevation | Not supported | Time-bound with auto-expiry |
| Role Assignment | Direct DB write | Approval workflow gated |

---

## Task Details

### P1-01: Create PERSONA_ROLE Entity & DB Script

**File:** `Beep.OilandGas.UserManagement\Models\Identity\PERSONA_ROLE.cs` (NEW)
**SQL:** `Beep.OilandGas.PPDM39\Scripts\Sqlserver\PersonaRoleTable.sql` (NEW)

```csharp
public class PERSONA_ROLE : ModelEntityBase
{
    public string PERSONA_ROLE_ID { get; set; }   // PK
    public string PERSONA_ID { get; set; }         // FK → PERSONA_DEFINITION
    public string ROLE_ID { get; set; }            // FK → ROLE (PPDM) or AppRole
    public string PERSONA_CODE { get; set; }       // Denormalized for fast lookup
    public string ROLE_NAME { get; set; }           // Denormalized for fast lookup
    public bool IS_PRIMARY { get; set; }            // Primary role for this persona
    public int PRIORITY { get; set; }               // Order of role application
    public string EFFECTIVE_SCOPE { get; set; }     // FIELD / ASSET / GLOBAL
}
```

**Why this matters:** Currently, selecting "Accountant" persona in the UI has no effect on what API permissions you have. The PERSONA_ROLE table makes this explicit: Accountant → [Accounting.View, Accounting.PostJournal, Economics.View, Data.View].

**Validation rules:**
- A persona must have at least one primary role
- PERSONA_CODE must match an existing PERSONA_DEFINITION.PERSONA_CODE
- ROLE_NAME must match an existing ROLE.ROLE_NAME

---

### P1-02: Seed PERSONA_ROLE Mappings

**File:** `DefaultSecuritySeedService.cs` (modify — add method `SeedPersonaRoleMappingsAsync`)

Mapping table (19 personas → roles):

| Persona | Primary Role(s) | Scope |
|---------|----------------|-------|
| FIELD_ENGINEER | PetroleumEngineer | FIELD |
| EXECUTIVE | Manager, Viewer (all domains) | GLOBAL |
| PRODUCTION_MANAGER | Manager | FIELD |
| RESERVOIR_ENGINEER | ReservoirEngineer | FIELD |
| DRILLING_ENGINEER | PetroleumEngineer | FIELD |
| HSE_OFFICER | SafetyOfficer | FIELD |
| FACILITIES_ENGINEER | PetroleumEngineer | FIELD |
| DATA_ANALYST | Viewer (all), DataManagement.* | GLOBAL |
| ACCOUNTANT | Accounting.*, Economics.View | GLOBAL |
| ADMINISTRATOR | Administrator | GLOBAL |
| EXPLORATION_GEOLOGIST | PetroleumEngineer, Exploration.* | FIELD |
| DEVELOPMENT_PLANNER | Manager, DevelopmentPlanning.* | FIELD |
| PRODUCTION_ENGINEER | PetroleumEngineer, Production.* | FIELD |
| DECOMMISSIONING_COORDINATOR | PetroleumEngineer, Decommissioning.* | FIELD |
| COMPLIANCE_OFFICER | Compliance, Auditor | GLOBAL |
| HSE_COORDINATOR | SafetyOfficer, HSE.* | FIELD |
| FACILITY_OPERATOR | PetroleumEngineer | FIELD |
| ASSET_MANAGER | Manager | FIELD |
| WORKFLOW_ADMINISTRATOR | Administrator, Workflow.* | GLOBAL |

**Seeding order:** Must run AFTER `SeedDefaultRolesAsync` and `SeedDefaultPersonasAsync`.

---

### P1-03: Register PERSONA_ROLE in SecurityModule

**File:** `SecurityModule.cs` (modify)

Add to `EntityTypes`:
```csharp
typeof(PERSONA_ROLE)
```

The `SecurityModule` (Order=40) in `Beep.OilandGas.UserManagement` already registers identity entities (AppRole, AppPermission, AppUserRole, PersonaDefinition, etc.). PERSONA_ROLE follows the same pattern.

---

### P1-04: Update DefaultSecuritySeedService

**File:** `DefaultSecuritySeedService.cs` (modify)

Add to the seeding orchestrator (after persona seeding):
```csharp
await SeedPersonaRoleMappingsAsync();
```

**Idempotency:** Check `PERSONA_ROLE_ID` existence before insert (same pattern as existing seed methods).

---

### P1-05: Create FieldScopeAuthorizationHandler

**File:** `Beep.OilandGas.UserManagement\Security\FieldScopeAuthorizationHandler.cs` (NEW)

This is the most impactful security task. Currently, a user with "Production.View" can see ALL fields' production data. After this, they only see their assigned fields.

```csharp
public class FieldScopeRequirement : IAuthorizationRequirement
{
    public string FieldId { get; }
    public string PermissionCode { get; }
}

public class FieldScopeAuthorizationHandler 
    : AuthorizationHandler<FieldScopeRequirement>
{
    // 1. Extract user's assigned fields from JWT claim "field_scope"
    // 2. If claim contains "*" or "GLOBAL" → allow all
    // 3. If claim contains the requested FieldId → allow
    // 4. Otherwise → deny
    // 5. Log decision to AuthorizationObservabilityService
}
```

**Integration points:**
- JWT token issuance must include `field_scope` claim (populated from UserPersonaProfile.DEFAULT_FIELD_ID and any explicit field assignments)
- All aggregation controllers check field scope before querying
- PPDMGenericRepository applies field filter when `FIELD_ID` column exists

---

### P1-06: Add Field-Scope Claim to JWT Issuance

**File:** IdentityServer profile service or token issuance pipeline (locate and modify)

```csharp
// In the profile service / claims transformation:
var fields = await _fieldAccessService.GetUserFieldsAsync(userId);
claims.Add(new Claim("field_scope", string.Join(",", fields))); // e.g., "FIELD_A,FIELD_B" or "*"
```

---

### P1-07: Permission Audit — All 177 Permissions vs Controllers

**Deliverable:** Audit spreadsheet mapping every controller action to required permission(s).

**Method:**
1. Use `[RequireRole]` and `[Authorize]` attributes as starting point
2. For each controller in `ApiService\Controllers\PPDM39\`:
   - List every [HttpGet], [HttpPost], [HttpPut], [HttpDelete] action
   - Map to the closest PermissionConstants entry
   - Flag unmapped actions (no auth attribute found)
3. For each Razor page in `Web\Pages\`:
   - Check if the page performs API calls that need auth
   - Verify the page itself is gated by NavigationPolicyService

**Known gaps (from Enhancement Roadmap):**
- RESERVES_REVISION controller → needs ReservesEngineer or ReservoirEngineer role
- Some DataManagement controller actions → need DataManagement.* permissions
- Webhook/stub endpoints → need audit trail even if no auth

**Output file:** `docs/permission-audit-2026-07.csv`

---

### P1-08: Add Missing Authorization Attributes

Based on P1-07 audit results, add `[RequireRole]` or `[Authorize(Policy = "...")]` to every unprotected action.

**Pattern to follow:**
```csharp
[HttpGet("fields/{fieldId}/production")]
[RequireRole("PetroleumEngineer", "Manager", "Administrator")]
public async Task<IActionResult> GetFieldProduction(string fieldId) { ... }
```

**For fine-grained permission checks (inside action body):**
```csharp
if (!await _authz.UserHasPermissionAsync(userId, PermissionConstants.Production.View))
    return Forbid();
```

---

### P1-09: Create RoleHierarchy Entity & Seeding

**File:** `Beep.OilandGas.UserManagement\Models\Identity\RoleHierarchy.cs` (NEW)

```csharp
public class RoleHierarchy
{
    public string PARENT_ROLE_ID { get; set; }
    public string CHILD_ROLE_ID { get; set; }
    public string INHERITANCE_TYPE { get; set; }  // FULL / SELECTIVE / DENY
}
```

**Default hierarchy:**
```
Administrator
├── Manager (FULL)
│   ├── PetroleumEngineer (FULL)
│   ├── ReservoirEngineer (FULL)
│   ├── SafetyOfficer (SELECTIVE: HSE only)
│   └── Compliance (SELECTIVE: Regulatory only)
├── Auditor (FULL — read-only all)
├── Admin (FULL)
├── Supervisor (FULL — subset of Manager)
├── GateApprover (SELECTIVE: Approve.* only)
└── ReservesEngineer (FULL — subset of ReservoirEngineer)
```

**Seeding:** In `DefaultSecuritySeedService`, after role seeding.

---

### P1-10: Implement Role Inheritance in PermissionHandler

**File:** `PermissionHandler.cs` (modify)

Current: Checks only direct role-permission assignments.
Target: Walk the role hierarchy upward. If a parent role has the permission, the child inherits it.

```csharp
private async Task<bool> HasPermissionIncludingInherited(string userId, string permissionCode)
{
    var directRoles = await _roleService.GetUserRolesAsync(userId);
    var allRoles = new HashSet<string>(directRoles);
    
    foreach (var role in directRoles)
    {
        var parents = await _roleHierarchyService.GetParentRolesAsync(role);
        foreach (var parent in parents)
            allRoles.Add(parent);
    }
    
    return allRoles.Any(r => _rolePermissionMap[r].Contains(permissionCode));
}
```

**Performance:** Cache role hierarchy in memory (it changes rarely). Refresh cache on role/permission changes.

---

### P1-11: Create TemporaryRoleElevation Entity & Service

**File:** `Beep.OilandGas.UserManagement\Models\Identity\TempRoleElevation.cs` (NEW)

```csharp
public class TempRoleElevation
{
    public string ELEVATION_ID { get; set; }       // PK
    public string USER_ID { get; set; }             // Who gets elevated
    public string ELEVATED_ROLE_ID { get; set; }    // Temporary role
    public string BASE_ROLE_ID { get; set; }        // Their normal role
    public DateTime EFFECTIVE_FROM { get; set; }
    public DateTime EFFECTIVE_TO { get; set; }      // Auto-expiry
    public string REASON { get; set; }              // "Covering for Manager on leave"
    public string REQUESTED_BY { get; set; }        // Who approved it
    public string STATUS { get; set; }              // ACTIVE / EXPIRED / REVOKED
    public string SCOPE_LIMITATION { get; set; }    // Optional: limit to specific fields
}
```

**Service:** `TempRoleElevationService`
- `RequestElevationAsync()` — creates request, triggers approval workflow
- `ActivateElevationAsync()` — after approval, adds role to user's claims
- `RevokeElevationAsync()` — manual early revocation
- `CleanupExpiredAsync()` — background job: sets EXPIRED for past-due elevations

---

### P1-12: Implement Time-Bound Role Elevation

**Integration in JWT issuance:**
```csharp
// When building claims, check for active elevations:
var activeElevations = await _elevationService.GetActiveElevationsAsync(userId);
foreach (var elev in activeElevations)
{
    claims.Add(new Claim("elevated_role", elev.ELEVATED_ROLE_ID));
    claims.Add(new Claim("elevation_expiry", elev.EFFECTIVE_TO.ToString("O")));
}
```

**Authorization handler enhancement:**
`PermissionHandler` checks both base roles AND active elevated roles.

**Auto-expiry:** Background `IHostedService` runs every 5 minutes, expires past-due elevations. On expiry, user must re-authenticate to get a token without the elevated role.

---

### P1-13: Create RoleAssignmentApproval Workflow Definition

**Process Definition ID:** `RBAC_ROLE_ASSIGNMENT`

Steps:
1. **REQUEST** → Requester fills form (user, role, justification)
2. **MANAGER_APPROVAL** → User's line manager approves/denies
3. **SECURITY_REVIEW** → Administrator verifies SoD compliance (Phase 4)
4. **ACTIVATION** → System assigns role, logs to audit trail
5. **NOTIFICATION** → Email to user + manager

**Required roles per step:**
- REQUEST: Any authenticated user
- MANAGER_APPROVAL: User's manager (via org hierarchy)
- SECURITY_REVIEW: Administrator role
- ACTIVATION: System (automated)
- NOTIFICATION: System (automated)

---

### P1-14: Seed RoleAssignmentApproval Process Definition

Add to `ProcessDefinitionInitializer.cs`:
```csharp
private async Task InitializeRoleAssignmentProcessAsync(string userId)
{
    var definition = new ProcessDefinition
    {
        ProcessId = "RBAC_ROLE_ASSIGNMENT",
        ProcessName = "Role Assignment Approval",
        ProcessType = "ADMINISTRATIVE",
        EntityType = "USER",
        Steps = new List<ProcessStepDefinition>
        {
            new() { StepId = "REQUEST", StepName = "Request Role", SequenceNumber = 1, ... },
            new() { StepId = "MANAGER_APPROVAL", StepName = "Manager Approval", 
                    SequenceNumber = 2, RequiresApproval = true, RequiredRoles = new(){"Manager"} },
            new() { StepId = "SECURITY_REVIEW", StepName = "Security Review", 
                    SequenceNumber = 3, RequiresApproval = true, RequiredRoles = new(){"Administrator"} },
            // ...
        }
    };
    await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
}
```

---

## Phase 1 Completion Checklist

- [ ] PERSONA_ROLE table created and seeded for all 19 personas
- [ ] Field-scope claim present in all JWT tokens
- [ ] FieldScopeAuthorizationHandler active on all data queries
- [ ] 100% of controller actions have authorization attributes
- [ ] Role hierarchy seeded and enforced in PermissionHandler
- [ ] Temporary role elevation works end-to-end (request → approve → activate → expire)
- [ ] Role assignment requires approval workflow
- [ ] All new entities registered in LifeCycleModule
- [ ] All seed methods are idempotent

## Phase 1 Deliverables

| # | File | Action |
|---|------|--------|
| 1 | `UserManagement\Models\Identity\PERSONA_ROLE.cs` | CREATE |
| 2 | `PPDM39\Scripts\Sqlserver\PersonaRoleTable.sql` | CREATE |
| 3 | `UserManagement\Models\Identity\RoleHierarchy.cs` | CREATE |
| 4 | `UserManagement\Models\Identity\TempRoleElevation.cs` | CREATE |
| 5 | `UserManagement\Security\FieldScopeAuthorizationHandler.cs` | CREATE |
| 6 | `UserManagement\Services\TempRoleElevationService.cs` | CREATE |
| 7 | `UserManagement\Services\RoleHierarchyService.cs` | CREATE |
| 8 | `UserManagement\Services\FieldAccessService.cs` | CREATE |
| 9 | `UserManagement\Modules\SecurityModule.cs` | MODIFY (register PERSONA_ROLE) |
| 10 | `UserManagement\Services\DefaultSecuritySeedService.cs` | MODIFY |
| 11 | `UserManagement\Security\PermissionHandler.cs` | MODIFY |
| 12 | IdentityServer profile service (JWT claims) | MODIFY |
| 13 | `docs\permission-audit-2026-07.csv` | CREATE |

---

*Next: [Phase 2 — Workflow Engine Enhancement](phase-2-workflow-engine.md)*
