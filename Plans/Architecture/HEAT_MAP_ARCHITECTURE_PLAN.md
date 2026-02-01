# Beep.OilandGas Heat Map - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned heat map analytics platform for spatial performance visualization and hotspot detection across fields and assets.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.HeatMap` as the system of record; services orchestrate analytics and store results with full metadata.

**Scope**: Spatial analytics for production, reliability, and performance metrics.

---

## Architecture Principles

### 1) Spatial Traceability
- Preserve spatial inputs, interpolation methods, and assumptions.
- Results are versioned and auditable.

### 2) Reusable Analytics
- Standardize spatial aggregation for reuse by other modules.

### 3) PPDM39 Alignment
- Persist heat map models and results in PPDM-aligned entities.

### 4) Cross-Project Integration
- **ProductionOperations**: production and downtime metrics.
- **EconomicAnalysis**: economic performance overlays.
- **Drawing**: map rendering inputs.

---

## Target Project Structure

```
Beep.OilandGas.HeatMap/
├── Services/
│   ├── HeatMapService.cs (orchestrator)
│   ├── SpatialAggregationService.cs
│   └── VisualizationService.cs
├── Calculations/
│   ├── InterpolationCalculator.cs
│   ├── KernelDensityEstimator.cs
│   └── GridBuilder.cs
├── Validation/
│   ├── SpatialInputValidator.cs
│   └── MethodValidator.cs
└── Exceptions/
    ├── HeatMapException.cs
    └── SpatialComputationException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.HeatMap`:

### Core Heat Map
- HEAT_MAP_MODEL
- HEAT_MAP_INPUT
- HEAT_MAP_RUN
- HEAT_MAP_RESULT

### Spatial Metadata
- GRID_DEFINITION
- INTERPOLATION_METHOD
- SPATIAL_AGGREGATION_RULE

---

## Service Interface Standards

```csharp
public interface IHeatMapService
{
    Task<HEAT_MAP_MODEL> CreateModelAsync(HEAT_MAP_MODEL model, string userId);
    Task<HEAT_MAP_RUN> RunAsync(string modelId, string userId);
    Task<HEAT_MAP_RESULT> GetResultAsync(string runId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement heat map model, input, and result entities.
- Create HeatMapService and validators.

### Phase 2: Spatial Calculations (Weeks 2-3)
- Interpolation and grid generation.

### Phase 3: Integration (Week 4)
- Integrate with Drawing and ProductionOperations.

---

## Best Practices Embedded

- **Spatial transparency**: interpolation method stored with results.
- **Reusability**: standardized spatial analytics.
- **Auditability**: inputs and outputs preserved with run metadata.

---

## API Endpoint Sketch

```
/api/heat-map/
├── /models
│   ├── POST
│   └── GET /{id}
├── /runs
│   ├── POST /run/{modelId}
│   └── GET /{id}
└── /results
    └── GET /{runId}
```

---

## Success Criteria

- PPDM-aligned heat map entities persist all analytics data.
- Spatial results are reproducible with audit trails.
- Integration with visualization is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
