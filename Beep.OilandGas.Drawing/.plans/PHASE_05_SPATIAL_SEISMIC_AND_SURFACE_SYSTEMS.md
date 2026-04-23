# Phase 05 - Spatial, Seismic And Surface Systems

## Objective

Extend the library beyond subsurface-only views into field maps, spatial overlays, seismic context, facility schematics, production systems, and HSE or regulatory overlays.

## Starting Point In Code

- The project already has enough loader breadth to justify a real map scene.
- PPDM subject areas provided in the wider solution cover spatial descriptions, seismic components, facilities, equipment, land rights, notifications, and HSE incidents.
- No dedicated map or network renderer exists yet.

## Scope

- Field map layer stack for wells, fields, pools, leases, and regulatory areas.
- Seismic survey footprints and interpretation overlays.
- Facility and surface-system visualization.
- Production network and flow-path views.
- HSE, land, and compliance overlays.

## Standards And Domain Drivers

- GeoJSON RFC 7946
- OGC WMS
- OGC GeoPackage
- OGC GeoTIFF
- SEG-Y family for seismic adjacency
- PPDM `SP_*`, `AREA*`, `SEIS_*`, `FACILITY*`, `EQUIPMENT*`, `LAND_*`, `HSE_INCIDENT*`, `NOTIFICATION*`

## Work Packages

### 1. Map layer stack

- Add wells, leases, field boundaries, pools, and interpreted polygons to a common map scene.
- Support raster backdrops and vector overlays.
- Support thematic symbology and filterable visibility.

### 2. Seismic context

- Add seismic survey footprints and line or volume extents.
- Support overlay of interpreted faults, horizons, and sections.
- Keep first delivery focused on contextual overlays rather than full trace visualization.

### 3. Surface systems and facility context

- Add facility symbols, equipment overlays, and simplified surface network diagrams.
- Support production strings and flow-path context between well and facility assets.
- Add HSE incident and compliance overlays for field operations views.

## Deliverables

- Map layer engine.
- Seismic footprint overlay capability.
- Facility and surface-system renderer.
- HSE and land overlay layer set.

## Exit Criteria

- A field-level map can show wells, pools, facilities, land polygons, and interpreted overlays in one scene.
- Seismic extents can be visualized in context with wells and reservoir features.
- Facility and production-system views can share the same styling and metadata foundation as map scenes.

## Risks

- If map rendering and network rendering diverge too much, shared styling and legend logic will fracture.
- Seismic work can over-expand; keep early seismic scope to overlays, extents, and interpreted context first.