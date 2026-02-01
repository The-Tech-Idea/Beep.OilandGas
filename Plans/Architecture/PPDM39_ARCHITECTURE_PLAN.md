# Beep.OilandGas PPDM39 - Architecture Plan

## Executive Summary

**Goal**: Provide the authoritative PPDM39 schema, mappings, and data standards for all Oil and Gas domains in the solution.

**Key Principle**: PPDM39 is the **single source of truth** for enterprise data structures, naming conventions, and reference integrity.

**Scope**: Data models, schema scripts, reference data, and mapping utilities.

---

## Architecture Principles

### 1) Canonical Data Standard
- All domain entities map to PPDM39 tables and naming conventions.
- PPDM39 tables define common audit columns and key patterns.

### 2) Version-Controlled Schema
- Schema scripts live in `Beep.OilandGas.PPDM39/Scripts/Sqlserver/`.
- Schema changes are applied via explicit migration scripts.

### 3) Cross-Project Consistency
- Domain projects reference PPDM39 entities via `IPPDMEntity`.
- Mapping utilities centralize conversions and metadata access.

---

## Target Project Structure

```
Beep.OilandGas.PPDM39/
├── Scripts/
│   ├── Sqlserver/
│   └── ReferenceData/
├── Core/
│   ├── Metadata/
│   ├── DTOs/
│   └── Mappings/
├── Services/
│   ├── PPDMMetadataService.cs
│   └── PPDMMappingService.cs
└── Validation/
    └── SchemaValidator.cs
```

---

## Data Model Requirements

### Canonical Standards
- All entities are **ALL_CAPS** with underscores.
- Standard audit columns: ROW_CREATED_BY, ROW_CREATED_DATE, ROW_CHANGED_BY, ROW_CHANGED_DATE, ACTIVE_IND.
- Reference tables for code sets and enumerations.

---

## Service Interface Standards

```csharp
public interface IPPDMMetadataService
{
    Task<TableMetadata> GetTableMetadataAsync(string tableName);
    Task<List<TableMetadata>> GetAllTablesAsync();
}
```

---

## Implementation Phases

### Phase 1: Schema Audit (Week 1)
- Verify PPDM tables align with entity classes.

### Phase 2: Mapping Standardization (Week 2)
- Ensure mapping utilities are used consistently.

### Phase 3: Reference Data Governance (Week 3)
- Establish reference data population scripts and validation.

---

## Best Practices Embedded

- **Canonical naming**: no deviations from PPDM naming.
- **Schema control**: all schema changes are versioned.
- **Metadata-driven**: runtime metadata used for validation and UI.

---

## Success Criteria

- PPDM39 tables and models are consistent and complete.
- Domain projects rely on PPDM metadata for validation.
- Reference data is versioned and repeatable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
