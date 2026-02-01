# Beep.OilandGas Oil Properties - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned oil properties platform for PVT modeling, correlations, and property generation used across flow assurance and production analysis.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.OilProperties` as the system of record; services orchestrate property generation and correlation management.

**Scope**: Oil PVT properties for nodal, pipeline, and production analysis.

---

## Architecture Principles

### 1) Correlation Transparency
- Preserve correlation choices and input assumptions.
- Results are versioned and auditable.

### 2) Consistent Property Generation
- Centralize property calculations for reuse by other projects.

### 3) PPDM39 Alignment
- Persist oil property models in PPDM-aligned entities.

### 4) Cross-Project Integration
- **PipelineAnalysis** and **NodalAnalysis** for flow modeling.
- **ProductionOperations** for operational constraints.

---

## Target Project Structure

```
Beep.OilandGas.OilProperties/
├── Services/
│   ├── OilPropertiesService.cs (orchestrator)
│   ├── CorrelationService.cs
│   └── PropertyGenerationService.cs
├── Calculations/
│   ├── PbCalculator.cs
│   ├── BoCalculator.cs
│   └── ViscosityCalculator.cs
├── Validation/
│   ├── InputValidator.cs
│   └── CorrelationValidator.cs
└── Exceptions/
    ├── OilPropertiesException.cs
    └── CorrelationException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.OilProperties`:

### Core Oil Properties
- OIL_PVT_MODEL
- OIL_PVT_INPUT
- OIL_PROPERTY_RESULT
- OIL_CORRELATION

### Outputs
- OIL_BUBBLE_POINT
- OIL_FORMATION_VOLUME_FACTOR
- OIL_VISCOSITY
- OIL_DENSITY

---

## Service Interface Standards

```csharp
public interface IOilPropertiesService
{
    Task<OIL_PVT_MODEL> CreateModelAsync(OIL_PVT_MODEL model, string userId);
    Task<OIL_PROPERTY_RESULT> GeneratePropertiesAsync(string modelId, string userId);
    Task<OIL_CORRELATION> RegisterCorrelationAsync(OIL_CORRELATION correlation, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement PVT model, input, and result entities.
- Create OilPropertiesService and validators.

### Phase 2: Calculations + Correlations (Weeks 2-3)
- Pb, Bo, and viscosity calculations.

### Phase 3: Integration (Week 4)
- Integrate with pipeline and nodal analysis.

---

## Best Practices Embedded

- **Correlation traceability**: correlation and inputs stored with results.
- **Reusability**: centralized property generation for all modules.
- **Auditability**: results preserved with run metadata.

---

## API Endpoint Sketch

```
/api/oil-properties/
├── /models
│   ├── POST
│   └── GET /{id}
├── /generate
│   └── POST /{modelId}
└── /correlations
    └── POST
```

---

## Success Criteria

- PPDM-aligned oil property entities persist all models and results.
- Properties are reproducible with audit trails.
- Integration with analysis modules is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
