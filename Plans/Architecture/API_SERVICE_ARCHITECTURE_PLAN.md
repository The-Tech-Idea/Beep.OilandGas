# Beep.OilandGas API Service - Architecture Plan

## Executive Summary

**Goal**: Provide a unified API gateway for all Oil and Gas lifecycle services with consistent security, routing, and observability.

**Key Principle**: The API layer is **thin**, delegating business logic to domain services and enforcing common standards.

**Scope**: API endpoints, authentication, authorization, validation, and service orchestration.

---

## Architecture Principles

### 1) Thin API Layer
- Controllers delegate to domain services only.
- No business logic inside controllers.

### 2) Consistent Security
- Centralized authn/authz and role policies.
- Audit user actions for sensitive operations.

### 3) Standardized Contracts
- Common response envelope and error model.
- Input validation using shared validators.

---

## Target Project Structure

```
Beep.OilandGas.ApiService/
├── Controllers/
│   ├── LifecycleController.cs
│   ├── ProductionController.cs
│   └── EconomicsController.cs
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Filters/
│   ├── ValidationFilter.cs
│   └── AuthorizationFilter.cs
├── Configuration/
│   ├── ServiceRegistry.cs
│   └── SwaggerConfig.cs
└── Exceptions/
    ├── ApiException.cs
    └── ValidationException.cs
```

---

## Service Interface Standards

```csharp
public interface IApiResponse
{
    string CorrelationId { get; }
    bool Success { get; }
    string? Message { get; }
}
```

---

## Implementation Phases

### Phase 1: API Standards (Week 1)
- Standard response and error envelopes.

### Phase 2: Security + Observability (Week 2)
- Authentication, authorization, and request logging.

### Phase 3: Controller Integration (Week 3)
- Wire controllers to domain services and validate inputs.

---

## Best Practices Embedded

- **Thin controllers**: no business logic.
- **Consistency**: unified response and error patterns.
- **Observability**: correlation IDs on all requests.

---

## Success Criteria

- API endpoints delegate to domain services with no duplication.
- Security policies are enforced consistently.
- Logging and tracing are available for all requests.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
