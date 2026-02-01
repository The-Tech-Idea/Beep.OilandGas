# Beep.OilandGas User Management - Architecture Plan

## Executive Summary

**Goal**: Provide centralized user, role, and permission management for all Oil and Gas lifecycle modules.

**Key Principle**: User and role data are authoritative; access policies are consistent across API and UI.

**Scope**: Identity data, roles, permissions, and auditing.

---

## Architecture Principles

### 1) Single Identity Source
- Central user profile and role assignments.
- Consistent permission checks across services.

### 2) Audit and Compliance
- Track role changes and access events.

### 3) Cross-Project Integration
- API Service enforces permissions for all domain endpoints.

---

## Target Project Structure

```
Beep.OilandGas.UserManagement/
├── Services/
│   ├── UserService.cs
│   ├── RoleService.cs
│   └── PermissionService.cs
├── Validation/
│   ├── UserValidator.cs
│   └── RoleValidator.cs
└── Exceptions/
    ├── UserManagementException.cs
    └── PermissionException.cs
```

---

## Data Model Requirements

Create/verify these entities in `Beep.OilandGas.Models.Data.UserManagement`:

- USER_PROFILE
- USER_ROLE
- ROLE_PERMISSION
- USER_AUDIT_LOG

---

## Service Interface Standards

```csharp
public interface IUserService
{
    Task<USER_PROFILE> CreateUserAsync(USER_PROFILE user, string adminId);
    Task<bool> AssignRoleAsync(string userId, string roleId, string adminId);
    Task<List<ROLE_PERMISSION>> GetPermissionsAsync(string userId);
}
```

---

## Implementation Phases

### Phase 1: Core Identity (Week 1)
- Implement user and role entities and services.

### Phase 2: Permissions + Auditing (Week 2)
- Role-based access control and audit logging.

---

## Best Practices Embedded

- **Least privilege**: roles grant minimum required access.
- **Auditability**: role changes and access are logged.

---

## Success Criteria

- Role-based access enforced consistently across APIs.
- User changes are auditable end-to-end.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
