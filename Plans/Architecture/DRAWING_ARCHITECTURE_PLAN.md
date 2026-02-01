# Beep.OilandGas Drawing - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned drawing/visualization platform for well schematics, facility layouts, and spatial overlays used across lifecycle workflows.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.Drawing` as the system of record; services orchestrate drawing generation and store artifacts with metadata.

**Scope**: Engineering drawings, schematics, and visualization assets.

---

## Architecture Principles

### 1) Asset Traceability
- Preserve drawing inputs, revisions, and approvals.
- Store drawing metadata with references to assets and models.

### 2) Reusable Visualization
- Provide standard drawing templates for wells, facilities, and maps.

### 3) PPDM39 Alignment
- Persist drawing metadata in PPDM-aligned entities.

### 4) Cross-Project Integration
- **HeatMap** for spatial overlays.
- **DevelopmentPlanning** and **DrillingAndConstruction** for engineering packages.
- **ProductionOperations** for facility schematics.

---

## Target Project Structure

```
Beep.OilandGas.Drawing/
├── Services/
│   ├── DrawingService.cs (orchestrator)
│   ├── TemplateService.cs
│   └── RevisionService.cs
├── Renderers/
│   ├── WellSchematicRenderer.cs
│   ├── FacilityLayoutRenderer.cs
│   └── MapOverlayRenderer.cs
├── Validation/
│   ├── DrawingValidator.cs
│   └── TemplateValidator.cs
└── Exceptions/
    ├── DrawingException.cs
    └── RenderingException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.Drawing`:

### Core Drawing
- DRAWING
- DRAWING_REVISION
- DRAWING_TEMPLATE
- DRAWING_ASSET_REFERENCE

### Metadata
- DRAWING_LAYER
- DRAWING_STYLE
- DRAWING_APPROVAL

---

## Service Interface Standards

```csharp
public interface IDrawingService
{
    Task<DRAWING> CreateDrawingAsync(DRAWING drawing, string userId);
    Task<DRAWING_REVISION> CreateRevisionAsync(string drawingId, string userId);
    Task<DRAWING> RenderDrawingAsync(string drawingId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement drawing, template, and revision entities.
- Create DrawingService and validators.

### Phase 2: Rendering + Templates (Weeks 2-3)
- Well and facility rendering templates.

### Phase 3: Integration (Week 4)
- Integrate with HeatMap and DevelopmentPlanning.

---

## Best Practices Embedded

- **Revision control**: drawing revisions with approvals.
- **Template consistency**: standard visual outputs.
- **Auditability**: inputs and outputs preserved with metadata.

---

## API Endpoint Sketch

```
/api/drawings/
├── /drawings
│   ├── POST
│   └── GET /{id}
├── /revisions
│   └── POST /{drawingId}
└── /render
    └── POST /{drawingId}
```

---

## Success Criteria

- PPDM-aligned drawing entities persist all metadata.
- Drawings are versioned, reproducible, and auditable.
- Integration with heat map and planning workflows is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
