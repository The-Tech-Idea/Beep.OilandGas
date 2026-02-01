# Beep.OilandGas PPDM.Models - Architecture Plan

## Executive Summary

**Goal**: Provide the typed C# model layer for PPDM39 entities used across all domain projects.

**Key Principle**: Models are **thin, schema-aligned classes** implementing `IPPDMEntity` with consistent audit fields and naming.

**Scope**: Entity classes, shared base types, and common interfaces.

---

## Architecture Principles

### 1) Schema Fidelity
- C# models must mirror PPDM39 tables exactly.
- No business logic inside model classes.

### 2) Consistent Base Types
- All entities inherit from a shared base `Entity`.
- Standard audit fields are present on all entities.

### 3) Cross-Project Reuse
- Domain projects depend on PPDM models for persistence.

---

## Target Project Structure

```
Beep.OilandGas.PPDM.Models/
├── 39/
│   ├── EntityBase/
│   ├── Common/
│   ├── Reference/
│   └── Domain/
├── Interfaces/
│   └── IPPDMEntity.cs
└── Utilities/
    └── EntityHelpers.cs
```

---

## Data Model Requirements

### Core Rules
- Class names map to PPDM table names.
- Properties are ALL_CAPS to match schema.
- Use nullable types where PPDM allows nulls.

---

## Service Interface Standards

No services; this project is model-only.

---

## Implementation Phases

### Phase 1: Model Audit (Week 1)
- Verify model classes exist for all PPDM tables.

### Phase 2: Consistency Pass (Week 2)
- Ensure audit columns and key patterns are present in all models.

---

## Best Practices Embedded

- **Thin models**: no business logic inside entities.
- **Schema alignment**: model names and properties match tables.

---

## Success Criteria

- All PPDM tables have matching model classes.
- No schema mismatches between scripts and models.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
