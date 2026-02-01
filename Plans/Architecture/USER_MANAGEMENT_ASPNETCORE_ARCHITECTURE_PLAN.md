# Beep.OilandGas UserManagement.AspNetCore - Architecture Plan

## Executive Summary

**Goal**: Provide the ASP.NET Core hosting layer for user management services, including authentication endpoints, token issuance, and policy enforcement.

**Key Principle**: The ASP.NET Core layer is **thin** and delegates identity logic to UserManagement services.

**Scope**: Authentication endpoints, token management, middleware, and policy enforcement.

---

## Architecture Principles

### 1) Thin Hosting Layer
- Controllers delegate to UserManagement services.
- No business logic in controllers.

### 2) Security First
- Token issuance, refresh, and revocation.
- Policy enforcement for role/permission checks.

### 3) Auditability
- Log access events and authentication attempts.

---

## Target Project Structure

```
Beep.OilandGas.UserManagement.AspNetCore/
├── Controllers/
│   ├── AuthController.cs
│   └── UsersController.cs
├── Middleware/
│   ├── AuthLoggingMiddleware.cs
│   └── TokenValidationMiddleware.cs
├── Configuration/
│   ├── IdentityConfig.cs
│   └── TokenConfig.cs
└── Exceptions/
    ├── AuthException.cs
    └── TokenException.cs
```

---

## Service Interface Standards

```csharp
public interface IAuthService
{
    Task<AuthToken> IssueTokenAsync(string username, string password);
    Task<bool> RevokeTokenAsync(string tokenId, string adminId);
}
```

---

## Implementation Phases

### Phase 1: Auth Endpoints (Week 1)
- Login, refresh, and revoke endpoints.

### Phase 2: Policy Enforcement (Week 2)
- Role and permission policies.

### Phase 3: Observability (Week 3)
- Access logging and security metrics.

---

## Best Practices Embedded

- **Token security**: refresh/revoke workflows enforced.
- **Auditability**: auth events logged consistently.

---

## Success Criteria

- Authentication and authorization are secure and consistent.
- All auth events are logged with correlation IDs.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
