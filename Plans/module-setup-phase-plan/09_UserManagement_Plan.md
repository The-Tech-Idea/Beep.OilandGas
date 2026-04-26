# 09 — UserManagement: Best-Practice Audit & Revision Plan

## SP-A — Domain Audit Findings

### Standards Applied
| Standard | Scope |
|---|---|
| PPDM 3.9 `BUSINESS_ASSOCIATE` | User-to-BA linkage |
| NIST SP 800-63B | Identity lifecycle state, lockout, MFA flags |
| SOC 2 / OWASP ASVS | Role separation of duties (`SOD_FLAG`), sensitive role flag |
| O&G multi-field operations | Field-scoped user context, unit system preference |

### Audit Findings

1. **`AppPermission` missing from `EntityTypes`** — `AppPermission` is defined in `Models/Identity/` and referenced via FK from `AppRolePermission.PERMISSION_ID`, but was never registered in `SecurityModule._entityTypes`. This means the schema orchestrator would not create the `AppPermission` table. **Fixed: added `typeof(AppPermission)` before `typeof(AppRolePermission)`.**

2. **`AppUser` missing PPDM 3.9 linkage fields** — No `BA_ID` (BUSINESS_ASSOCIATE FK) or `EMPLOYEE_ID`. In O&G operations, a user account maps to a PPDM BA record for audit trail alignment. Added `BA_ID` and `EMPLOYEE_ID`.

3. **`AppRole` missing field-scope constraint** — Multi-field O&G deployments require roles that are optionally constrained to specific field assets. Added `VALID_FIELD_SCOPE` (nullable string; `null` = no restriction, `"*"` = all fields, explicit `FIELD_ID` = single field).

4. **`UserPersonaProfile.DEFAULT_UNIT_SYSTEM` missing** — The profile already had `DEFAULT_FIELD_ID` but no `DEFAULT_UNIT_SYSTEM` to capture the user's preferred unit system (metric/imperial) for engineering displays. Added.

5. **Auto-property pattern (acknowledged)** — All classes use `{ get; set; }` auto-properties rather than `SetProperty(ref ...)`. This is intentional for the security/identity domain (ASP.NET Identity alignment). `ModelEntityBase` INotifyPropertyChanged will not fire for these, but the Beep schema-creation path does not require change notifications. No change made.

---

## SP-B — File Changes

| File | Change |
|---|---|
| `Models/Identity/AppUser.cs` | Added `BA_ID` (PPDM BUSINESS_ASSOCIATE linkage) and `EMPLOYEE_ID` |
| `Models/Identity/AppRole.cs` | Added `VALID_FIELD_SCOPE` for field-scoped role constraint |
| `Models/Profile/UserPersonaProfile.cs` | Added `DEFAULT_UNIT_SYSTEM` |

---

## SP-C — EntityTypes Registry

`Modules/SecurityModule.cs` — **updated** — now registers 11 table types (was 10):

```
AppUser, AppRole, AppUserRole, AppPermission (added), AppRolePermission,
UserScopeAssignment, OrganizationScope, UserAssetAccess,
PersonaDefinition, PersonaViewPreference, UserPersonaProfile
```

Order 40 — before LeaseAcquisition (45) and Exploration (50).

---

## Build Result

```
dotnet build Beep.OilandGas.UserManagement/Beep.OilandGas.UserManagement.csproj -v q
→ 0 Error(s)   0 Warning(s)   ✓
```
