# Beep.OilandGas Models - Architecture Plan

## Executive Summary

**Goal**: Provide shared data and DTO models used across the solution, aligned to PPDM where applicable.

**Key Principle**: Domain persistence models live in PPDM and Data projects; shared models are **thin and reusable**.

**Scope**: Shared entity base classes, common value objects, and cross-domain models.

---

## Architecture Principles

### 1) Separation of Concerns
- PPDM entities remain authoritative for persistence.
- Models project provides shared utilities and non-persistent types.

### 2) Consistent Base Types
- Shared `Entity` base class and common audit fields.

### 3) Cross-Project Reuse
- Avoid duplicating common concepts across projects.

---

## Target Project Structure

```
Beep.OilandGas.Models/
├── Base/
│   ├── Entity.cs
│   └── AuditableEntity.cs
├── Common/
│   ├── AppFilter.cs
│   └── ResultModels.cs
└── Data/
    └── (Domain-specific PPDM-aligned data)
```

---

## Service Interface Standards

No services; shared models only.

---

## Implementation Phases

### Phase 1: Model Audit (Week 1)
- Identify shared models and remove duplicates.

### Phase 2: Consistency Pass (Week 2)
- Align base classes and shared utilities.

---

## Best Practices Embedded

- **Thin models**: avoid business logic.
- **Reusability**: shared concepts defined once.

---

## Success Criteria

- Shared models are consistent and reused across domains.
- No persistence logic in shared models.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
