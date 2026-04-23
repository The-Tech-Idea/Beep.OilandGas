# Phase 01 - Core Platform And CRS

## Objective

Create the technical foundation required for map scenes, section views, contouring, and standards-aware rendering. This phase should harden the scene model, coordinate handling, metadata normalization, and shared rendering services.

## Starting Point In Code

- `DrawingEngine`, `Viewport`, `CoordinateSystem`, and `DepthCoordinateSystem` already exist.
- `ReservoirData`, `WellSchematicData`, `LogData`, and `DeviationSurvey` already provide useful normalized model seeds.
- Current coordinate handling is not strong enough for mixed depth-domain and map-domain rendering.

## Scope

- Formal scene model for depth-domain, section-domain, and map-domain rendering.
- Unit-aware coordinate and measurement services.
- CRS abstraction that can represent local projected CRS, WGS84 / CRS84, and depth references.
- Shared legend, scale-bar, annotation, and symbology services.
- Common normalized model contracts for wells, logs, surfaces, maps, and facilities.

## Standards And Domain Drivers

- GeoJSON RFC 7946
- OGC GeoTIFF
- OGC WMS
- OGC Symbology Encoding
- PPDM `CS_*`, `SP_*`, `AREA*`, and legal location subject areas

## Work Packages

### 1. Scene and model contracts

- Define map-scene and section-scene abstractions.
- Separate source adapters from render-ready scene objects.
- Introduce explicit metadata bags for provenance, units, and CRS identity.

### 2. Coordinate and measurement system

- Replace ad hoc string-based coordinate metadata with typed CRS descriptors.
- Support projected XY, geographic XY, TVD, TVDSS, MD, and section-distance axes.
- Add shared unit conversion and label formatting services.

### 3. Shared drawing primitives

- Add reusable axis, legend, color-bar, north-arrow, and scale-bar primitives.
- Add a common label placement policy and collision rules.
- Add styling primitives for categorical, sequential, and diverging themes.

### 4. Validation and diagnostics

- Add loader-to-scene validation rules for missing units, missing CRS, invalid depth ranges, and empty geometry.
- Emit diagnostics that can be surfaced in sample tools or logs.

## Deliverables

- Typed scene contracts for map and section rendering.
- CRS and unit service layer.
- Shared legend and annotation framework.
- Validation and diagnostics package for data normalization.

## Exit Criteria

- Map scenes and depth scenes can both exist without special-case renderer logic.
- A reservoir grid, well trajectory, and GeoJSON fault polyline can be expressed in the same scene framework.
- Units and CRS labels are explicit in the rendered output.

## Risks

- If CRS design is deferred, contour maps and field maps will become expensive to correct later.
- If normalized scene contracts are skipped, every loader will drag its own rendering assumptions into the engine.