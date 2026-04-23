# Phase 04 - Reservoir Maps And Contours

## Objective

Make the library capable of the reservoir interpretation and mapping tasks petroleum engineers and geoscientists actually need: structure maps, isochore and isopach maps, property maps, contouring, cross-sections, fluid-contact views, and well overlays.

## Starting Point In Code

- `ReservoirLayer` already renders interval layers, lithology patterns, and fluid contacts in depth space.
- `ReservoirData` already includes layers, geometry, properties, contacts, coordinate system, and bounding box.
- `ResqmlReservoirLoader` already extracts geometry, metadata, and bounding extents.
- No contour engine, map projection pipeline, or section generator exists yet.

## Scope

- Grid and surface abstractions.
- Contour generation for structure, thickness, net-to-gross, porosity, permeability, saturation, and pressure maps.
- Well overlays, fault polylines, polygon boundaries, and fluid-contact markers.
- Cross-sections and flattened sections.
- Map legends, color bars, and uncertainty or confidence overlays.

## Standards And Domain Drivers

- RESQML
- GeoJSON RFC 7946
- OGC GeoTIFF
- OGC GeoPackage
- FGDC geologic symbolization
- PPDM `POOL*`, `FIELD*`, `PDEN*`, `LITH_*`, `STRAT_*`, `SP_*`, `AREA*`

## Work Packages

### 1. Surface and grid model

- Introduce a grid surface model separate from the render layer.
- Support raster-like property grids and vector contour outputs.
- Add interpolation strategy hooks for gridding and contouring.

### 2. Contour and map renderer

- Generate contour lines and contour labels.
- Render fault overlays, well symbols, lease or area polygons, and fluid-contact annotations.
- Support sequential and diverging palettes for property maps.

### 3. Cross-sections and interpretation views

- Build section extraction from reservoir surfaces plus well control.
- Support correlated horizons, layer fills, and section annotations.
- Add pay-zone and contact emphasis modes.

## Deliverables

- Contour engine.
- Map scene renderer.
- Reservoir property map renderer.
- Cross-section builder.

## Exit Criteria

- A RESQML-backed structure map can be contoured with well overlays and exported.
- A property grid can be visualized with legends and interpretable symbology.
- A cross-section can align wells, layers, and fluid contacts in a reproducible layout.

## Risks

- Contour credibility depends on explicit gridding, smoothing, and labeling rules; hidden heuristics will create distrust.
- Cross-sections will become visually inconsistent if horizon, layer, and well alignment rules are not standardized.