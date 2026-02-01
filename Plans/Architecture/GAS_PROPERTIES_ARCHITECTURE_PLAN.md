# Beep.OilandGas Gas Properties - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned gas properties platform for PVT modeling, correlations, and property generation used across flow assurance and production analysis.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.GasProperties` as the system of record; services orchestrate property generation and correlation management.

**Scope**: Gas PVT properties for nodal, pipeline, and compressor analysis.

---

## Architecture Principles

### 1) Correlation Transparency
- Preserve correlation choices and input assumptions.
- Results are versioned and auditable.

### 2) Consistent Property Generation
- Centralize property calculations for reuse by other projects.

### 3) PPDM39 Alignment
- Persist gas property models in PPDM-aligned entities.

### 4) Cross-Project Integration
- **PipelineAnalysis** and **CompressorAnalysis** for flow modeling.
- **NodalAnalysis** for inflow/outflow calculations.

---

## Target Project Structure

```
Beep.OilandGas.GasProperties/
├── Services/
│   ├── GasPropertiesService.cs (orchestrator)
│   ├── CorrelationService.cs
│   └── PropertyGenerationService.cs
├── Calculations/
│   ├── ZFactorCalculator.cs
│   ├── ViscosityCalculator.cs
│   └── FormationVolumeFactorCalculator.cs
├── Validation/
│   ├── InputValidator.cs
│   └── CorrelationValidator.cs
└── Exceptions/
    ├── GasPropertiesException.cs
    └── CorrelationException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.GasProperties`:

### Core Gas Properties
- GAS_PVT_MODEL
- GAS_PVT_INPUT
- GAS_PROPERTY_RESULT
- GAS_CORRELATION

### Outputs
- GAS_Z_FACTOR
- GAS_VISCOSITY
- GAS_FORMATION_VOLUME_FACTOR
- GAS_DENSITY

---

## Service Interface Standards

```csharp
public interface IGasPropertiesService
{
    Task<GAS_PVT_MODEL> CreateModelAsync(GAS_PVT_MODEL model, string userId);
    Task<GAS_PROPERTY_RESULT> GeneratePropertiesAsync(string modelId, string userId);
    Task<GAS_CORRELATION> RegisterCorrelationAsync(GAS_CORRELATION correlation, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement PVT model, input, and result entities.
- Create GasPropertiesService and validators.

### Phase 2: Calculations + Correlations (Weeks 2-3)
- Z-factor, viscosity, and FVF calculations.

### Phase 3: Integration (Week 4)
- Integrate with pipeline, compressor, and nodal analysis.

---

## Best Practices Embedded

- **Correlation traceability**: correlation and inputs stored with results.
- **Reusability**: centralized property generation for all modules.
- **Auditability**: results preserved with run metadata.

---

## API Endpoint Sketch

```
/api/gas-properties/
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

- PPDM-aligned gas property entities persist all models and results.
- Properties are reproducible with audit trails.
- Integration with analysis modules is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
