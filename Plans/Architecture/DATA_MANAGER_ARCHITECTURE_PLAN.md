# Beep.OilandGas Data Manager - Architecture Plan

## Executive Summary

**Goal**: Provide a centralized data access and repository abstraction layer for PPDM and domain entities.

**Key Principle**: DataManager enforces **consistent CRUD, filtering, and metadata access** across all domains.

**Scope**: Repository implementations, metadata handlers, default value policies, and common data services.

---

## Architecture Principles

### 1) Consistent Data Access
- Use generic repository patterns for PPDM entities.
- Standardize filtering and paging behavior.

### 2) Metadata-Driven Operations
- Table metadata defines valid columns and default behaviors.

### 3) Cross-Project Reuse
- Domain services depend on DataManager for persistence.

---

## Target Project Structure

```
Beep.OilandGas.DataManager/
├── Repositories/
│   ├── PPDMGenericRepository.cs
│   └── RepositoryFactory.cs
├── Metadata/
│   ├── MetadataProvider.cs
│   └── TableMetadataCache.cs
├── Defaults/
│   ├── DefaultValueService.cs
│   └── CommonColumnHandler.cs
└── Exceptions/
    ├── DataManagerException.cs
    └── MetadataException.cs
```

---

## Service Interface Standards

```csharp
public interface IRepositoryFactory
{
    PPDMGenericRepository CreateRepository(Type entityType, string? connectionName = null);
}
```

---

## Implementation Phases

### Phase 1: Repository Standardization (Week 1)
- Audit and standardize repository methods and filters.

### Phase 2: Metadata Cache (Week 2)
- Add caching for table metadata.

### Phase 3: Defaults + Auditing (Week 3)
- Standardize default values and audit column handling.

---

## Best Practices Embedded

- **Consistency**: one repository path for CRUD operations.
- **Metadata-driven**: validation uses schema metadata.
- **Caching**: reduce metadata lookups.

---

## Success Criteria

- All domain services use shared repositories.
- Metadata lookups are centralized and cached.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
