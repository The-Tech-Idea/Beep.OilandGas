# User Management Plan for Beep.OilandGas

## Overview
This plan defines a user management subsystem for the Beep.OilandGas framework to manage users, roles, permissions, and access to modules and data sources. The design targets flexibility: it should support web (internet/public), intranet (enterprise local auth), and desktop applications. It will integrate cleanly with ASP.NET Core dependency injection.

Goals
- Centralized user, role, and permission management
- Fine-grained authorization for functions, modules, and data sources
- Support for multiple authentication methods (OAuth2/OIDC, Windows Auth, local DB)
- Extensible storage (EF Core, Dapper, or custom repositories)
- Clear service interfaces for DI and testability
- Auditing and logging for critical user actions
- Support for multi-tenant scenarios

Security and Standards
- Use ASP.NET Core Identity or a lightweight custom implementation depending on requirements
- Follow least privilege principle
- Store passwords using a modern KDF (ASP.NET Identity's PBKDF2/Argon2) or integrate external IdP
- Use HTTPS/TLS for all web endpoints
- Implement account lockout, MFA hooks, and password policy

Folder Layout (proposed)
- `Plans/` - this plan and follow-up design docs
- `Beep.OilandGas.Models/Data/Security/` - entities for User, Role, Permission, Tenant, Audit
- `Beep.OilandGas.Core/Interfaces/Security/` - service interfaces
- `Beep.OilandGas.Data/Repositories/Security/` - data access implementations
- `Beep.OilandGas.Services/Security/` - business services
- `Beep.OilandGas.Web/` - ASP.NET Core integration (Auth, Policies)

Key Concepts
- User: an account (local or external) with identity attributes and many-to-many with Roles
- Role: named role grouping Permissions
- Permission: atomic capability (e.g., `Module:Read`, `Module:Write`, `DataSource:Access:Production`) that maps to policy checks
- Resource/Scope: module or data source identifier used in permission strings
- Tenant: optional multi-tenant scoping

Phase 1 - Design and Models
1. Define domain entities (User, Role, Permission, UserRole, RolePermission, Tenant, UserClaim) in `Beep.OilandGas.Models`.
2. Use `Entity` base type patterns consistent with the repo.
3. Define DTOs for API boundaries.

Phase 2 - Interfaces
1. `IUserService` - manage users (CRUD, password, claims)
2. `IRoleService` - manage roles and role permissions
3. `IPermissionService` - central lookup and semantics for permissions
4. `IAuthService` - login/out, token generation (JWT), external IdP integration
5. `IAuthorizationService` - policy evaluation against user permissions and resources

Phase 3 - Persistence
1. Provide EF Core and repository interfaces; implement a default EF Core provider project.
2. Provide migrations scripts for schema.
3. Ensure model maps do not break existing patterns.

Phase 4 - Services
1. Implement business logic with DI and async methods
2. Provide unit tests using xUnit

Phase 5 - ASP.NET Core Integration
1. Add authentication schemes (Cookie/JWT/OpenIdConnect/Windows)
2. Map permissions into policies using `IAuthorizationHandler` and `PermissionRequirement`
3. Register services in Startup/Program using `AddScoped`/`AddSingleton` appropriately
4. Provide middleware for auditing

Phase 6 - Desktop App Integration
1. Provide `IAuthService` wiring for desktop apps (token storage, refresh)
2. Provide examples for WPF/WinForms using MSAL or local login

Phase 7 - Testing and Documentation
1. Unit tests for services and handlers
2. Integration tests for auth flows
3. Developer docs and API docs

Deliverables
- `Plans/UserManagementPlan.md` (this file)
- Domain model files in `Beep.OilandGas.Models/Data/Security`
- Service interfaces in `Beep.OilandGas.Models/Core/Interfaces/Security`
- Implementations and DI wiring examples
- Unit tests

Next Steps
1. I'll create model files under `Beep.OilandGas.Models/Data/Security/` following the project's `Entity` pattern.
2. Create service interfaces in `Beep.OilandGas.Models/Core/Interfaces/Security`.
3. Wait for your approval to implement EF Core-based repository and service implementations.

Approved by: GitHub Copilot
Date: 2026-01-01
