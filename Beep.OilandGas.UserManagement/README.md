# Beep.OilandGas.UserManagement

The **Beep.OilandGas.UserManagement** project is a comprehensive identity, authentication, authorization, and security module for the Beep Oil & Gas platform. It provides a full RBAC (Role-Based Access Control) system with row-level security, multi-factor authentication (TOTP), persona-based user profiles, and extensive audit logging -- all backed by the single PPDM39 database schema.

---

## Table of Contents

- [Features](#features)
- [Project Structure](#project-structure)
- [Dependencies](#dependencies)
- [Services](#services)
- [Contracts (Interfaces)](#contracts-interfaces)
- [Models](#models)
- [Security Components](#security-components)
- [Module Setup & Seeding](#module-setup--seeding)
- [Dependency Injection](#dependency-injection)
- [API Endpoints](#api-endpoints)
- [Authentication & Authorization](#authentication--authorization)
- [Row-Level Security](#row-level-security)
- [Multi-Factor Authentication](#multi-factor-authentication)
- [Persona Profiles](#persona-profiles)
- [Audit Trail](#audit-trail)
- [Configuration](#configuration)
- [Usage Patterns](#usage-patterns)
- [Seeded Roles & Permissions](#seeded-roles--permissions)
- [Development Guidelines](#development-guidelines)

---

## Features

- **JWT Authentication** -- access tokens + refresh tokens with configurable expiry
- **RBAC Authorization** -- 13 pre-seeded Oil & Gas roles with 150+ permissions across 30+ domains
- **Row-Level Security** -- field/organization/asset-scoped data access enforcement
- **Multi-Factor Authentication** -- TOTP (RFC 6238) with QR code setup and backup codes
- **Persona Profiles** -- role-specific UI preferences, landing pages, and workflow configurations
- **Account Lockout** -- configurable failed attempt limits and lockout durations
- **Password Security** -- PBKDF2-SHA256 (120k iterations), complexity enforcement, expiry tracking
- **Audit Logging** -- access events, profile changes, role assignments, authorization traces
- **Security Seeding** -- automatic bootstrap of roles, permissions, personas, and scope assignments on first run

---

## Project Structure

```
Beep.OilandGas.UserManagement/
├── Beep.OilandGas.UserManagement.csproj
├── Contracts/
│   └── Services/
│       ├── IAuthService.cs                  # Login, tokens, password management
│       ├── IDefaultSecuritySeedService.cs   # Security bootstrap seeder
│       ├── IMfaService.cs                   # TOTP MFA + backup codes
│       ├── IPasswordHistoryService.cs       # Password history (interface + model)
│       ├── IPersonaProfileService.cs        # Persona profiles + view preferences
│       ├── IRoleAssignmentService.cs        # Role/permission assignment with audit
│       ├── IRowLevelSecurityService.cs      # Row-level access control
│       └── ISessionManagementService.cs     # Session management (interface + model)
├── DependencyInjection/
│   └── UserManagementServiceCollectionExtensions.cs  # DI registration
├── Models/
│   ├── Audit/
│   │   ├── AuthorizationDecisionTrace.cs    # Policy evaluation traces
│   │   ├── SetupWizardLog.cs               # Setup wizard step logs
│   │   ├── UserAccessAuditEvent.cs         # Login/logout/access events
│   │   └── UserProfileAuditEvent.cs        # Profile change snapshots
│   ├── Identity/
│   │   ├── AppPermission.cs                # Permission entity (resource.action.scope)
│   │   ├── AppRole.cs                      # Role entity with field-scope constraint
│   │   ├── AppRolePermission.cs            # Role-permission join with approval tracking
│   │   ├── AppUser.cs                      # Extended user with lockout/lifecycle
│   │   └── AppUserRole.cs                  # User-role assignment with effective dates
│   ├── Profile/
│   │   ├── PersonaDefinition.cs            # Persona catalog entry
│   │   ├── PersonaViewPreference.cs        # Per-user, per-persona view settings
│   │   └── UserPersonaProfile.cs           # User persona preferences + versioning
│   ├── Requests/
│   │   └── UserManagement/
│   │       ├── ApplyRowFiltersRequest.cs
│   │       ├── CheckDatabaseAccessRequest.cs
│   │       ├── CheckDataSourceAccessRequest.cs
│   │       ├── CheckPermissionAllRequest.cs
│   │       ├── CheckPermissionAnyRequest.cs
│   │       ├── CheckPermissionRequest.cs
│   │       ├── CheckRoleRequest.cs
│   │       ├── CheckRowAccessRequest.cs
│   │       └── CheckSourceAccessRequest.cs
│   └── Scope/
│       ├── OrganizationScope.cs            # Organization hierarchy
│       ├── UserAssetAccess.cs              # Asset-level access rules
│       └── UserScopeAssignment.cs          # User-to-scope mappings
├── Modules/
│   └── SecurityModule.cs                  # ModuleSetupBase for schema + seeding
├── Security/
│   ├── PermissionHandler.cs               # ASP.NET Core authorization handler
│   └── PermissionRequirement.cs           # IAuthorizationRequirement wrapper
└── Services/
    ├── AuthService.cs                     # JWT auth, login, password management
    ├── DefaultSecuritySeedService.cs      # Comprehensive security bootstrap
    ├── MfaService.cs                      # TOTP MFA implementation
    ├── PersonaProfileService.cs           # Persona profile CRUD
    ├── RoleAssignmentService.cs           # Role/permission assignment
    ├── RowLevelSecurityService.cs         # Row-level access control
    └── UserManagementService.cs           # User CRUD + permission checking
```

---

## Dependencies

### Project References
| Project | Purpose |
|---|---|
| `Beep.OilandGas.Models` | Shared model types, `ModelEntityBase`, `PermissionConstants`, `RoleDefinitions`, security DTOs |
| `Beep.OilandGas.PPDM39.DataManagement` | `PPDMGenericRepository`, `IDMEEditor`, `ICommonColumnHandler`, `IPPDM39DefaultsRepository`, `IPPDMMetadataRepository`, `ModuleSetupBase` |

### NuGet Packages
| Package | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.Authorization` | 10.0.7 | ASP.NET Core authorization policies |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.7 | JWT bearer authentication |
| `System.IdentityModel.Tokens.Jwt` | 8.12.1 | JWT token creation and validation |

### Target Framework
- .NET 10.0 (`net10.0`)

---

## Services

### AuthService

**File:** `Services/AuthService.cs`

JWT-based authentication service.

| Method | Description |
|---|---|
| `LoginAsync(request, ipAddress)` | Authenticate user, validate password (PBKDF2-SHA256), check lockout, return JWT + refresh token |
| `RefreshTokenAsync(refreshToken)` | Validate refresh token, issue new token pair |
| `RevokeTokenAsync(refreshToken)` | Single device logout |
| `RevokeAllTokensAsync(userId)` | Logout all devices |
| `ChangePasswordAsync(userId, oldPassword, newPassword)` | Change password with old password verification |
| `RequestPasswordResetAsync(email)` | Generate reset token, return reset URL |
| `ResetPasswordAsync(token, newPassword)` | Complete password reset |
| `UnlockAccountAsync(userId)` | Reset failed login counter and clear lockout |
| `RecordFailedLoginAsync(userId)` | Increment failed login counter, lock if threshold exceeded |

**JWT claims:** `NameIdentifier` (USER_ID), `Name` (USERNAME), `Email`, `tenant_id`, `ba_id`, `Role` (multiple), `permissions` (comma-separated)

**Password hash format:** `pbkdf2$iterations$salt$hash` (PBKDF2-SHA256, 120k iterations, 16-byte salt, 32-byte hash)

### UserManagementService

**File:** `Services/UserManagementService.cs`

Core user CRUD and permission checking.

| Method | Description |
|---|---|
| `GetByIdAsync(id)` | Get user by USER_ID |
| `GetByUsernameAsync(username)` | Get user by USERNAME |
| `GetAllAsync()` | Get all active users |
| `CreateAsync(user, password, userId)` | Create user with password validation |
| `UpdateAsync(user, callerUserId)` | Update user profile |
| `DeleteAsync(id, callerUserId)` | Soft-delete (set IS_ACTIVE = false) |
| `AddToRoleAsync(userId, roleName, callerUserId)` | Assign role to user |
| `RemoveFromRoleAsync(userId, roleName, callerUserId)` | Remove role from user |
| `GetRolesAsync(userId)` | Get user's assigned roles |
| `CheckPermissionAsync(userId, permissionCode)` | Check if user has specific permission |
| `SeedDefaultPermissionsAsync(userId)` | Seed all permissions from PermissionConstants |

### RoleAssignmentService

**File:** `Services/RoleAssignmentService.cs`

Advanced role and permission assignment with audit trail.

| Method | Description |
|---|---|
| `AssignRoleAsync(userId, roleName, reason, callerUserId)` | Assign role with reason tracking |
| `RevokeRoleAsync(userId, roleName, reason, callerUserId)` | Revoke role with reason tracking |
| `GrantPermissionToRoleAsync(roleName, permissionCode, callerUserId)` | Grant permission to role |
| `RevokePermissionFromRoleAsync(roleName, permissionCode, callerUserId)` | Revoke permission from role |
| `GetRoleCatalogAsync()` | Get all roles with permission counts |
| `GetPermissionCatalogAsync()` | Get all permissions with role counts |
| `GetUserRoleAssignmentsAsync(userId)` | Get user's role assignments with dates |
| `GetRolePermissionsAsync(roleName)` | Get permissions assigned to a role |

### PersonaProfileService

**File:** `Services/PersonaProfileService.cs`

User persona and view preference management.

| Method | Description |
|---|---|
| `GetProfileAsync(userId)` | Get or create user persona profile |
| `UpsertProfileAsync(profile, callerUserId)` | Create or update profile with versioning |
| `SwitchPersonaAsync(userId, personaCode, callerUserId)` | Switch user's active persona |
| `GetPersonaCatalogAsync()` | Get all persona definitions |
| `GetViewPreferencesAsync(userId, personaCode)` | Get user's view preferences for a persona |
| `SetViewPreferenceAsync(userId, personaCode, viewKey, viewValue)` | Set a view preference |

### MfaService

**File:** `Services/MfaService.cs`

TOTP-based multi-factor authentication (RFC 6238).

| Method | Description |
|---|---|
| `EnableMfaAsync(userId)` | Generate secret key + QR code URI |
| `VerifyTotpAsync(userId, totpCode)` | Verify TOTP code (+/- 1 time-step window) |
| `VerifyMfaSetupAsync(userId, totpCode)` | Confirm setup, activate MFA, generate backup codes |
| `DisableMfaAsync(userId, password)` | Disable MFA with password verification |
| `GenerateBackupCodesAsync(userId)` | Generate 10 one-time backup codes |
| `UseBackupCodeAsync(userId, code)` | Consume a backup code |
| `GetMfaStatusAsync(userId)` | Get MFA enabled/verified status |

**Technical details:**
- TOTP: 6 digits, 30-second period, HMAC-SHA1
- Secret key: 20 bytes, Base32 encoded
- Backup codes: 10 codes, 8 characters each
- QR code URI: `otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}&digits=6&period=30`

### RowLevelSecurityService

**File:** `Services/RowLevelSecurityService.cs`

Row-level and scope-based data access control.

| Method | Description |
|---|---|
| `CheckRowAccessAsync(request)` | Check if user can access specific row |
| `ApplyRowFiltersAsync(request)` | Generate SQL filter expressions for queries |
| `CheckSourceAccessAsync(request)` | Check data source access |
| `CheckDatabaseAccessAsync(request)` | Check database access |
| `CheckDataSourceAccessAsync(request)` | Check data source connection access |
| `GetUserAccessibleFieldsAsync(userId)` | Get user's accessible field IDs |
| `GetUserAccessibleAssetsAsync(userId)` | Get user's accessible asset IDs |
| `GetUserAccessibleOrganizationsAsync(userId)` | Get user's accessible organization IDs |

**Scope types:** `FIELD`, `ORGANIZATION`, `ASSET`, `GLOBAL`

### DefaultSecuritySeedService

**File:** `Services/DefaultSecuritySeedService.cs`

Comprehensive security bootstrap seeder. Seeds on first run:

1. **Business Associates** -- `BEEP_ORG_SYSTEM` (organization), `BEEP_USER_SYSTEM` (person)
2. **BA Organization** -- `BEEP_OILGAS` (TENANT type)
3. **System User** -- `SYSTEM` user with admin role
4. **Roles** (13 total) -- Viewer, Manager, PetroleumEngineer, ReservoirEngineer, Administrator, Approver, Auditor, Admin, Supervisor, ReservesEngineer, SafetyOfficer, GateApprover, Compliance
5. **Permissions** -- all `PermissionConstants` codes discovered via reflection (~150)
6. **Role-Permission mappings** -- comprehensive per-role permission sets
7. **Personas** (8) -- FIELD_ENGINEER, PRODUCTION_MANAGER, RESERVOIR_ENGINEER, DRILLING_ENGINEER, HSE_OFFICER, FACILITIES_ENGINEER, DATA_ANALYST, ADMINISTRATOR
8. **Organization Scope** -- `BEEP_OILGAS` tenant scope
9. **User Scope Assignments** -- SYSTEM user gets GLOBAL ORGANIZATION scope
10. **User Asset Access** -- SYSTEM user gets admin access to ORGANIZATION

All seeding is idempotent -- skips existing rows.

---

## Contracts (Interfaces)

| Interface | Description |
|---|---|
| `IAuthService` | Login, token refresh, token revocation, password change/reset, account unlock |
| `IRoleAssignmentService` | Role assignment/revocation, permission grants, catalog queries |
| `IPersonaProfileService` | Persona profile CRUD, persona switching, view preferences |
| `IMfaService` | TOTP MFA setup, verification, backup codes, status |
| `IRowLevelSecurityService` | Row access checks, filter generation, scope queries |
| `IDefaultSecuritySeedService` | Security bootstrap seeding |
| `IUserService` *(in Models)* | User CRUD, role management, permission checking |
| `IPasswordHistoryService` *(interface + model)* | Password history, rotation checks, cleanup |
| `ISessionManagementService` *(interface + model)* | Session creation, validation, cleanup |

---

## Models

### Identity Models

| Model | Table | Description |
|---|---|---|
| `AppUser` | `APP_USER` | Extended user with lockout, lifecycle state, PPDM linkage (BA_ID, EMPLOYEE_ID) |
| `AppRole` | `APP_ROLE` | Role with field-scope constraint (VALID_FIELD_SCOPE), SOD flag, sensitivity indicators |
| `AppPermission` | `APP_PERMISSION` | Permission in `resource.action.scope` format with risk level |
| `AppUserRole` | `APP_USER_ROLE` | User-role assignment with effective dates, approval status, reason tracking |
| `AppRolePermission` | `APP_ROLE_PERMISSION` | Role-permission grant with approval tracking and source system |

### Scope Models

| Model | Table | Description |
|---|---|---|
| `UserScopeAssignment` | `USER_SCOPE_ASSIGNMENT` | User-to-scope mapping (FIELD/ORGANIZATION/ASSET/GLOBAL) with inheritance and effective dates |
| `OrganizationScope` | `ORGANIZATION_SCOPE` | Organization hierarchy with parent references and full path |
| `UserAssetAccess` | `USER_ASSET_ACCESS` | Asset-level access with access level, deny override, expiration |

### Profile Models

| Model | Table | Description |
|---|---|---|
| `UserPersonaProfile` | `USER_PERSONA_PROFILE` | User persona preferences with versioning, default field/asset, locale, timezone, unit system |
| `PersonaDefinition` | `PERSONA_DEFINITION` | Persona catalog entry with category, landing route, allowed workflows |
| `PersonaViewPreference` | `PERSONA_VIEW_PREFERENCE` | Per-user, per-persona view key-value preferences |

### Audit Models

| Model | Table | Description |
|---|---|---|
| `UserAccessAuditEvent` | `USER_ACCESS_AUDIT_EVENT` | Login/logout/access events with IP, session, scope context |
| `UserProfileAuditEvent` | `USER_PROFILE_AUDIT_EVENT` | Profile change events with before/after JSON snapshots |
| `SetupWizardLog` | `SETUP_WIZARD_LOG` | Setup wizard step execution log |
| `AuthorizationDecisionTrace` | `AUTHORIZATION_DECISION_TRACE` | Authorization policy evaluation traces with denial reasons |

### Request Models

All in `Models/Requests/UserManagement/`:

| Request | Purpose |
|---|---|
| `CheckRowAccessRequest` | USER_ID + TABLE_NAME |
| `ApplyRowFiltersRequest` | USER_ID + TABLE_NAME + optional existing filters |
| `CheckSourceAccessRequest` | USER_ID + TARGET_SOURCE |
| `CheckDatabaseAccessRequest` | USER_ID + DATABASE_NAME |
| `CheckDataSourceAccessRequest` | USER_ID + DATASOURCE_NAME |
| `CheckPermissionRequest` | USER_ID + PERMISSION_CODE |
| `CheckPermissionAnyRequest` | USER_ID + PERMISSIONS[] (any match) |
| `CheckPermissionAllRequest` | USER_ID + PERMISSIONS[] (all must match) |
| `CheckRoleRequest` | USER_ID + ROLE_NAME |

---

## Security Components

### PermissionRequirement

**File:** `Security/PermissionRequirement.cs`

An `IAuthorizationRequirement` that wraps a single permission code string. Used with ASP.NET Core authorization policies.

### PermissionHandler

**File:** `Security/PermissionHandler.cs`

An `AuthorizationHandler<PermissionRequirement>` that checks the user's claims for the required permission. Supports both single `permission` claim and comma-separated `permissions` claim.

**Usage:**
```csharp
[Authorize(Policy = "Admin.ManageUsers")]
public class UserController : ControllerBase { ... }
```

### Permission Policy Registration

```csharp
services.AddPermissionPolicy("Admin.ManageUsers", PermissionConstants.Admin.ManageUsers);
services.AddPermissionPolicy("Admin.AssignRoles", PermissionConstants.Admin.AssignRoles);
```

---

## Module Setup & Seeding

### SecurityModule

**File:** `Modules/SecurityModule.cs`

A `ModuleSetupBase` implementation auto-discovered by the Beep framework.

| Property | Value |
|---|---|
| **ModuleId** | `SECURITY` |
| **ModuleName** | `Security Bootstrap (BA, Users, Roles)` |
| **Order** | `40` (runs after PPDM_CORE, before domain modules) |
| **Required Foundation** | Yes -- runs on every fresh database creation |

**Registered Entity Types (19 total):**

| Category | Types |
|---|---|
| Identity (PPDM) | `USER`, `ROLE`, `USER_ROLE`, `PERMISSION`, `ROLE_PERMISSION` |
| Scope | `UserScopeAssignment`, `OrganizationScope`, `UserAssetAccess` |
| Persona | `PersonaDefinition`, `PersonaViewPreference`, `UserPersonaProfile` |
| Audit | `UserProfileAuditEvent`, `UserAccessAuditEvent`, `SetupWizardLog`, `AuthorizationDecisionTrace` |
| Security | `UserMfaConfig`, `PasswordHistory`, `UserSession` |

**SeedAsync** delegates to `IDefaultSecuritySeedService.SeedDefaultsAsync()` and aggregates insertion counts across all 11 tables that receive seed data.

---

## Dependency Injection

### AddUserManagement Extension

**File:** `DependencyInjection/UserManagementServiceCollectionExtensions.cs`

```csharp
// In Program.cs -- MUST be called after AddBeepServices
builder.Services.AddUserManagement(configuration);
```

**Registered services (all scoped, factory pattern):**

| Service Interface | Implementation |
|---|---|
| `IUserService` | `UserManagementService` |
| `IRoleAssignmentService` | `RoleAssignmentService` |
| `IPersonaProfileService` | `PersonaProfileService` |
| `IDefaultSecuritySeedService` | `DefaultSecuritySeedService` |
| `IAuthService` | `AuthService` |
| `IRowLevelSecurityService` | `RowLevelSecurityService` |

**Connection name** resolved from config: `BeepOg:DatabaseConnectionName` (default: `"PPDM39"`).

---

## API Endpoints

### Authentication (`/api/auth`)

| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| POST | `/api/auth/login` | No | Login with username/password, returns JWT tokens |
| POST | `/api/auth/refresh` | No | Refresh access token |
| POST | `/api/auth/logout` | Yes | Revoke refresh token |
| POST | `/api/auth/logout-all` | Yes | Revoke all tokens for user |
| POST | `/api/auth/change-password` | Yes | Change current user's password |
| POST | `/api/auth/forgot-password` | No | Request password reset |
| POST | `/api/auth/reset-password` | No | Complete password reset with token |
| POST | `/api/auth/unlock/{userId}` | Admin.ManageUsers | Unlock a locked account |
| POST | `/api/auth/failed-login` | No | Record failed login attempt |

### User Management (`/api/identity/users`)

| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| GET | `/api/identity/users` | Admin.ManageUsers | Get all users |
| GET | `/api/identity/users/{id}` | Yes | Get user by ID (includes roles) |
| GET | `/api/identity/users/username/{username}` | Yes | Get user by username |
| POST | `/api/identity/users` | Admin.ManageUsers | Create new user (with optional roles) |
| PUT | `/api/identity/users/{id}` | Yes (admin or own) | Update user |
| DELETE | `/api/identity/users/{id}` | Admin.ManageUsers | Deactivate user |
| GET | `/api/identity/users/{id}/roles` | Yes | Get user's roles |
| POST | `/api/identity/users/{id}/roles` | Admin.AssignRoles | Assign role to user |
| DELETE | `/api/identity/users/{id}/roles/{roleName}` | Admin.AssignRoles | Remove role from user |

### Row-Level Security (`/api/identity/security`)

| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| POST | `/api/identity/security/check-row-access` | Yes | Check row-level access |
| POST | `/api/identity/security/apply-row-filters` | Yes | Get row filter expressions |
| POST | `/api/identity/security/check-source-access` | Yes | Check data source access |
| POST | `/api/identity/security/check-database-access` | Yes | Check database access |
| POST | `/api/identity/security/check-datasource-access` | Yes | Check data source connection access |
| GET | `/api/identity/security/accessible-fields` | Yes | Get user's accessible field IDs |
| GET | `/api/identity/security/accessible-assets` | Yes | Get user's accessible asset IDs |
| GET | `/api/identity/security/accessible-organizations` | Yes | Get user's accessible organization IDs |

---

## Authentication & Authorization

### JWT Tokens

- **Signing:** HMAC-SHA256
- **Access token expiry:** configurable (default 60 minutes)
- **Refresh token expiry:** configurable (default 7 days)
- **Refresh tokens:** cryptographically random (32 bytes, Base64)

### Password Security

- **Hashing:** PBKDF2-SHA256 with 120,000 iterations
- **Complexity:** minimum 12 characters, uppercase, lowercase, digit, special character
- **Account lockout:** configurable max failed attempts (default 5), lockout duration (default 30 minutes)
- **Password expiry:** configurable max age (default 90 days)
- **Reset tokens:** cryptographically random, 1-hour expiry
- **Legacy SHA256** fallback for existing hashes

### Authorization Flow

```
User -> USER -> USER_ROLE -> ROLE -> ROLE_PERMISSION -> PERMISSION
```

Permission check traverses the chain: user's roles -> roles' permissions -> permission code match.

---

## Row-Level Security

Row-level security enforces data access based on user scope assignments.

### Scope Types

| Scope | Description |
|---|---|
| `FIELD` | Access limited to specific field IDs |
| `ORGANIZATION` | Access limited to organization hierarchy |
| `ASSET` | Access limited to specific asset IDs |
| `GLOBAL` | Unrestricted access |

### Filter Generation

`ApplyRowFiltersAsync` produces SQL filter expressions:

```csharp
// Input: ApplyRowFiltersRequest(userId, "WELL")
// Output: FilterExpressions = ["FIELD_ID IN ('FIELD001','FIELD002')"]
```

These filters can be applied to `AppFilter` lists for `PPDMGenericRepository` queries.

---

## Multi-Factor Authentication

### Setup Flow

1. Call `EnableMfaAsync(userId)` -- returns secret key and QR code URI
2. User scans QR code in authenticator app (Google Authenticator, Authy, etc.)
3. Call `VerifyMfaSetupAsync(userId, totpCode)` -- confirms setup, activates MFA, returns backup codes
4. Store backup codes securely for account recovery

### Login Flow with MFA

1. User calls `LoginAsync` -- returns `MfaRequired = true` if MFA is enabled
2. User provides TOTP code from authenticator app
3. Call `VerifyTotpAsync(userId, totpCode)` -- validates code
4. If valid, proceed with authenticated session
5. If TOTP unavailable, use backup code via `UseBackupCodeAsync(userId, code)`

### Backup Codes

- 10 one-time codes generated per MFA setup
- 8 characters each
- Single use -- consumed on first successful verification
- Can be regenerated at any time (invalidates previous codes)

---

## Persona Profiles

Personas define role-specific UI configurations and default views.

### Seeded Personas

| Persona Code | Name | Category | Default Route |
|---|---|---|---|
| `FIELD_ENGINEER` | Field Engineer | Engineering | `/field/dashboard` |
| `PRODUCTION_MANAGER` | Production Manager | Management | `/production/dashboard` |
| `RESERVOIR_ENGINEER` | Reservoir Engineer | Engineering | `/reservoir/dashboard` |
| `DRILLING_ENGINEER` | Drilling Engineer | Engineering | `/drilling/dashboard` |
| `HSE_OFFICER` | HSE Officer | Safety | `/hse/dashboard` |
| `FACILITIES_ENGINEER` | Facilities Engineer | Engineering | `/facilities/dashboard` |
| `DATA_ANALYST` | Data Analyst | Analytics | `/analytics/dashboard` |
| `ADMINISTRATOR` | System Administrator | Administration | `/admin/dashboard` |

### View Preferences

Per-user, per-persona key-value settings:

```csharp
await _personaService.SetViewPreferenceAsync(
    userId, "FIELD_ENGINEER", "default_well_grid_columns",
    "[\"UWI\",\"WELL_NAME\",\"STATUS\",\"FIELD_ID\"]");
```

---

## Audit Trail

### Event Types

| Model | Events Tracked |
|---|---|
| `UserAccessAuditEvent` | LOGIN_SUCCESS, LOGIN_FAILED, TOKEN_REFRESH, LOGOUT, LOGOUT_ALL |
| `UserProfileAuditEvent` | Profile create, update, persona switch (with before/after JSON snapshots) |
| `AuthorizationDecisionTrace` | Policy evaluation results, denial reasons, claim inspection |
| `SetupWizardLog` | Setup wizard step execution, success/failure status |

### Audit Fields

All audit models include:
- `USER_ID` -- who performed the action
- `EVENT_TIMESTAMP` -- when it occurred
- `IP_ADDRESS` -- source IP (for access events)
- `SESSION_ID` -- session context
- `BEFORE_STATE` / `AFTER_STATE` -- JSON snapshots (for profile changes)

---

## Configuration

### appsettings.json

```json
{
  "BeepOg": {
    "DatabaseConnectionName": "PPDM39"
  },
  "Authentication": {
    "Jwt": {
      "SecretKey": "your-256-bit-secret-key-here",
      "Issuer": "Beep.OilandGas",
      "Audience": "Beep.OilandGas.Client",
      "AccessTokenExpiryMinutes": 60,
      "RefreshTokenExpiryDays": 7
    },
    "MaxFailedLoginAttempts": 5,
    "LockoutDurationMinutes": 30,
    "MaxPasswordAgeDays": 90
  }
}
```

### Configuration Keys

| Key | Default | Description |
|---|---|---|
| `BeepOg:DatabaseConnectionName` | `PPDM39` | PPDM database connection name |
| `Authentication:Jwt:SecretKey` | *(required)* | HMAC-SHA256 signing key (min 32 chars) |
| `Authentication:Jwt:Issuer` | `Beep.OilandGas` | JWT issuer claim |
| `Authentication:Jwt:Audience` | `Beep.OilandGas.Client` | JWT audience claim |
| `Authentication:Jwt:AccessTokenExpiryMinutes` | `60` | Access token lifetime |
| `Authentication:Jwt:RefreshTokenExpiryDays` | `7` | Refresh token lifetime |
| `Authentication:MaxFailedLoginAttempts` | `5` | Lockout threshold |
| `Authentication:LockoutDurationMinutes` | `30` | Lockout duration |
| `Authentication:MaxPasswordAgeDays` | `90` | Password expiry |

---

## Usage Patterns

### Registering in Program.cs

```csharp
// After AddBeepServices
builder.Services.AddUserManagement(configuration);

// Register permission policies
builder.Services.AddPermissionPolicy("Admin.ManageUsers", PermissionConstants.Admin.ManageUsers);
builder.Services.AddPermissionPolicy("Admin.AssignRoles", PermissionConstants.Admin.AssignRoles);

// SecurityModule is auto-discovered via AddDiscoveredModuleSetups()
```

### Login

```csharp
var result = await _authService.LoginAsync(
    new LoginRequest("admin", "P@ssw0rd!"),
    ipAddress: "192.168.1.1");

if (result.Success)
{
    Console.WriteLine($"Token: {result.AccessToken}");
    Console.WriteLine($"Roles: {string.Join(", ", result.Roles)}");
    Console.WriteLine($"Permissions: {string.Join(", ", result.Permissions)}");
}
```

### Row-Level Security Filters

```csharp
var filters = await _securityService.ApplyRowFiltersAsync(
    new ApplyRowFiltersRequest(userId, "WELL"));

// Result: FilterExpressions = ["FIELD_ID IN ('FIELD001','FIELD002')"]
```

### Persona Profile

```csharp
var profile = await _personaService.GetProfileAsync(userId);
if (profile == null)
{
    profile = new UserPersonaProfile
    {
        USER_ID = userId,
        PRIMARY_PERSONA = "FIELD_ENGINEER",
        DEFAULT_UNIT_SYSTEM = "Metric"
    };
    await _personaService.UpsertProfileAsync(profile, userId);
}

// Switch persona
await _personaService.SwitchPersonaAsync(userId, "PRODUCTION_MANAGER", userId);
```

### MFA Setup

```csharp
// 1. Enable MFA
var setup = await _mfaService.EnableMfaAsync(userId);
// Present setup.QrCodeUri to user for scanning

// 2. Verify setup
var backupCodes = await _mfaService.VerifyMfaSetupAsync(userId, "123456");
// Store backupCodes for the user

// 3. Verify during login
bool isValid = await _mfaService.VerifyTotpAsync(userId, "654321");
```

### Seeding Security Data

```csharp
var seeder = serviceProvider.GetRequiredService<IDefaultSecuritySeedService>();
var result = await seeder.SeedDefaultsAsync("SYSTEM");

Console.WriteLine($"Roles: {result.RolesInserted}");
Console.WriteLine($"Permissions: {result.PermissionsInserted}");
Console.WriteLine($"Personas: {result.PersonasInserted}");
Console.WriteLine($"Scopes: {result.OrganizationScopesInserted}");
```

### Permission Checking

```csharp
// Direct check
bool hasPermission = await _userService.CheckPermissionAsync(userId, "Production.Create");

// Via authorization attribute
[Authorize(Policy = "Production.Create")]
public async Task<IActionResult> CreateProduction(...) { ... }
```

---

## Seeded Roles & Permissions

### Roles

| Role | Permission Scope |
|---|---|
| **Administrator / Admin** | ALL permissions (~150) |
| **Manager** | View + Create + Edit + Approve across all domains |
| **Supervisor** | View + Create + Edit + Approve (operations focus) |
| **GateApprover** | Approve-only across all domains |
| **PetroleumEngineer** | Well, Production, Nodal/WellTest/Choke analysis, Gas Lift, Pump Performance, Properties, Drawing, Reporting |
| **ReservoirEngineer** | Reservoir, Production Forecasting, Enhanced Recovery, Nodal/WellTest analysis, Properties, Drawing, Reporting |
| **ReservesEngineer** | Reservoir + Production Forecasting + Nodal/WellTest + Properties + Reporting |
| **Auditor** | View-only across all domains + Admin.ViewAuditLogs |
| **SafetyOfficer** | HSE full + Environmental + Regulatory + limited view of other domains |
| **Compliance** | HSE view + Environmental + Regulatory + Admin.ViewAuditLogs |
| **Viewer** | View-only across all domains |
| **Approver** | View + Approve across all domains |

### Permission Domains

Permissions follow the `Domain.Action` format. Available domains:

| Domain | Actions |
|---|---|
| `Admin` | ManageUsers, AssignRoles, ViewAuditLogs, ConfigureSystem, ManageTenants, ViewSystemHealth |
| `Security` | ManagePermissions, ManageRoles, ViewAccessLogs, ManagePolicies, EmergencyAccess |
| `DataManagement` | ImportData, ExportData, ValidateData, ApproveData, DeleteData |
| `Accounting` | View, PostJournal, ApproveJournal, EditSettings, ViewReports, ManagePeriods |
| `Tax` | ViewProvision, Calculate, Adjust, SubmitReturns |
| `EconomicAnalysis` | View, Create, Edit, Approve, RunNPV, RunIRR |
| `WellManagement` | View, Create, Edit, Delete, Approve, ViewWellStatus, UpdateWellStatus, ViewWellStructures, ManageWellbore, ManageCompletion |
| `Production` | View, Create, Edit, Approve, Delete, ViewDaily, ViewMonthly, SubmitProduction, AllocateProduction, ViewDeclineCurves |
| `ProductionForecasting` | View, Create, Edit, RunDCA, Approve |
| `Reservoir` | View, Create, Edit, Approve, ViewMaterialBalance, RunSimulation, ViewReserves, UpdateReserves |
| `Drilling` | View, Create, Edit, Approve, ViewDailyReports, ManageWellProgram, ViewCostTracking |
| `Completions` | View, Create, Edit, Approve, DesignCompletion |
| `Facilities` | View, Create, Edit, Approve, ViewEquipment, ManageEquipment, ViewMaintenance |
| `Pipeline` | View, Create, Edit, Approve, RunHydraulics, ViewCapacity |
| `NodalAnalysis` | View, Create, Edit, RunIPR, RunVLP, Optimize |
| `WellTestAnalysis` | View, Create, Edit, RunPTA, Approve |
| `ChokeAnalysis` | View, Create, Edit, RunCorrelations |
| `CompressorAnalysis` | View, Create, Edit, RunCalculations |
| `GasLift` | View, Create, Edit, DesignValves, Optimize |
| `PumpPerformance` | View, Create, Edit, RunAnalysis |
| `SuckerRodPumping` | View, Create, Edit, DesignRodString, RunDynamometer |
| `PlungerLift` | View, Create, Edit, Analyze |
| `HydraulicPumps` | View, Create, Edit, RunAnalysis |
| `EnhancedRecovery` | View, Create, Edit, Approve, ManageInjection |
| `HSE` | View, ReportIncident, ManageIncidents, ApproveIncident, ViewObservations, CreateObservation, ViewAudits, ConductAudit, ViewPermits, IssuePermit, ApprovePermit, ViewRiskAssessments, CreateRiskAssessment, ViewCertifications, ManageCertifications |
| `Environmental` | View, Report, Approve, ViewEmissions, ReportEmissions, ViewCompliance |
| `Regulatory` | View, Submit, Approve, ViewCompliance, ManageFilings |
| `Exploration` | View, Create, Edit, Approve, ViewSeismic, ManageSeismic |
| `ProspectIdentification` | View, Create, Edit, Approve, RunScreening |
| `LeaseAcquisition` | View, Create, Edit, Approve |
| `DevelopmentPlanning` | View, Create, Edit, Approve, RunFDP |
| `Decommissioning` | View, Create, Edit, Approve, PlanAbandonment |
| `OilProperties` | View, Create, Edit, RunCorrelations |
| `GasProperties` | View, Create, Edit, RunCalculations |
| `FlashCalculations` | View, Create, Edit, RunFlash, RunEOS |
| `Drawing` | View, Create, Edit, Approve, Publish |
| `Reporting` | View, Create, Edit, Export, Schedule |
| `Dashboard` | View, Customize, Share |

---

## Development Guidelines

### Adding a New Permission

1. Add the constant to `PermissionConstants.cs` in `Beep.OilandGas.Models/Data/Security/`:
```csharp
public static class MyDomain
{
    public const string MyAction = "MyDomain.MyAction";
}
```
2. The permission is automatically discovered and seeded by `DefaultSecuritySeedService` (reflection-based).
3. Assign it to roles in `GetRolePermissionMap()` in `DefaultSecuritySeedService.cs`.

### Adding a New Role

1. Add the role name to `RoleDefinitions.cs` in `Beep.OilandGas.Models/Data/Security/`:
```csharp
public const string MyNewRole = "MyNewRole";
```
2. Add the role to `roleNames` array in `DefaultSecuritySeedService.SeedDefaultsAsync()`.
3. Add permission mappings in `GetRolePermissionMap()`.

### Adding a New Service

1. Define the interface in `Contracts/Services/`.
2. Implement in `Services/`.
3. Register in `UserManagementServiceCollectionExtensions.cs` using factory pattern:
```csharp
services.AddScoped<IMyService>(sp => new MyService(
    sp.GetRequiredService<IDMEEditor>(),
    sp.GetRequiredService<ICommonColumnHandler>(),
    sp.GetRequiredService<IPPDM39DefaultsRepository>(),
    sp.GetRequiredService<IPPDMMetadataRepository>(),
    connectionName,
    sp.GetRequiredService<ILoggerFactory>().CreateLogger<MyService>()));
```

### Adding a New Model (Table Class)

1. Create the class extending `ModelEntityBase` in the appropriate `Models/` subfolder.
2. Add the type to `SecurityModule._entityTypes` list.
3. The schema will be created automatically by the Beep framework's entity-driven tooling.

### Testing

```bash
# Build the project
dotnet build Beep.OilandGas.UserManagement/Beep.OilandGas.UserManagement.csproj

# Build the full solution
dotnet build Beep.OilandGas.sln

# Run the API
dotnet run --project Beep.OilandGas.ApiService
```
