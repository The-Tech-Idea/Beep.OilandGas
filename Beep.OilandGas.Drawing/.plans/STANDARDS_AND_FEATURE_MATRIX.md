# Standards And Feature Matrix

## Purpose

This document captures the external standards and interoperability targets that should drive the drawing library roadmap. The intent is not to implement every standard equally; it is to identify which standards matter for engineering-grade visualization and where the current library already has a start.

## Standards Matrix

| Capability Area | Standard Or Body | Why It Matters | Current State In Library | Planning Direction |
| --- | --- | --- | --- | --- |
| Well log exchange | CWLS LAS 2.0 / 3.0 | Baseline distribution format for wireline and interpreted logs; LAS 3.0 extends beyond simple curves into tops, core, deviation, and test data | `LasLogLoader` already exists | Keep as first-class import, add strict validation, track sections, curve mnemonics, units, null handling, and multi-object LAS 3 support |
| Digital log interchange | API RP66 / DLIS | Self-describing binary log format for complex well log datasets and rich metadata | `DlisLogLoader` already exists | Harden parsing, index channels and frames, preserve tool metadata, and support normalization into track-ready curve models |
| Drilling and well operations | Energistics WITSML | Standard for drilling, completions, logs, trajectories, and wellsite-to-office exchange | `WitsmlLogLoader` and `DataWorkOrderLoader` exist | Expand from loader-only coverage into trajectory, mud log, well test, and completion visualization workflows |
| Production and surveillance | Energistics PRODML | Production optimization, well test, flow network, time series, DTS, DAS, and fluid-property exchange | `ProdmlLoader` exists | Add production curves, surveillance overlays, network views, and production test visualizations |
| Reservoir model exchange | Energistics RESQML | Standard for subsurface structure, grid, interpretation, property, and model exchange | `ResqmlReservoirLoader` exists | Add gridded surfaces, contouring, structural maps, sections, and property visualization on top of normalized RESQML geometry |
| Energistics packaging and streaming | EPC / ETP ecosystem | Important for multi-object model packaging and near-real-time transfer patterns | Partial by association through current Energistics support | Plan loader normalization around package-aware metadata and future streaming, but do not block core rendering on this |
| Mnemonic normalization | PWLS | Reduces vendor mnemonic drift across well logs | `PwlsMnemonicMapper` exists | Make mnemonic normalization part of the ingestion pipeline before rendering and analytics |
| Spatial vector interchange | GeoJSON RFC 7946 | Lightweight vector interchange for wells, leases, contours, faults, boundaries, and interpretation exports | Not yet formalized in scene model | Add read and write support for contours, well locations, polygons, faults, and interpretation overlays |
| Spatial packaging | OGC GeoPackage | Portable container for vector, raster, and tabular spatial layers | Not currently implemented | Use for portable project bundles, map overlays, and derived contour deliverables |
| Geo-referenced imagery | OGC GeoTIFF | Exchange of georeferenced rasters for structural maps, backdrops, seismic slices, and interpreted rasters | Not currently implemented | Add raster backdrop support for map scenes and export of georeferenced render products |
| Web map access | OGC WMS | HTTP map image access for distributed geospatial layers | Not currently implemented | Use for basemaps, regulatory layers, and enterprise map backdrops |
| Styling interchange | OGC SLD / Symbology Encoding | Rule-based styling for geographic feature and coverage data | Internal themes exist; no external style model | Add importable/exportable map styling rules for contours, polygons, wells, and thematic layers |
| Geologic symbolization | FGDC Digital Cartographic Standard | Consistent patterns, lines, colors, and symbols for geologic maps and cross-sections | USGS-FGDC assets already exist in `LithologySymbols` | Convert the existing asset base into a formal pattern and symbol service used by reservoir and map views |
| CRS discipline | WGS84 / CRS84 / EPSG / OGC CRS conventions | Required for contour maps, facility maps, leases, wells, and any georeferenced overlay work | Current code only has limited generic and depth coordinate support | Introduce a real CRS abstraction, reprojection policy, and unit-aware world-to-screen mapping |
| Seismic interchange | SEG-Y family | Standard seismic data exchange for 2D and 3D interpretation workflows | No renderer yet | Support seismic footprint overlays and header-aware metadata first; full seismic trace rendering can follow later |
| Enterprise petroleum model | PPDM 3.9 | Canonical planning anchor for wells, logs, lithology, facilities, pool, field, land, spatial, seismic, and HSE subject areas | Current loaders reference PPDM38/39 partially | Use PPDM subject areas to prioritize visualization domains and table-backed adapters |

## Standards Already Reflected In Code

- LAS support is already present.
- DLIS support is already present.
- WITSML-related loading is already present.
- PRODML loading is already present.
- RESQML loading is already present.
- PPDM-related loaders already exist in partial form.
- FGDC-related lithology pattern assets already exist.
- PWLS mnemonic normalization already exists.

## Standards That Should Drive Phase Ordering

### Must shape early phases

- PPDM 3.9
- LAS
- RP66 / DLIS
- WITSML
- RESQML
- GeoJSON RFC 7946
- CRS84 / WGS84 / EPSG-backed CRS handling

### Should shape map and export phases

- GeoPackage
- GeoTIFF
- WMS
- OGC SLD / Symbology Encoding
- FGDC geologic symbolization

### Should shape later interoperability phases

- PRODML production network and time series coverage
- SEG-Y family support for seismic overlays
- ETP-aware streaming and package metadata

## Engineering Interpretation

The library should treat standards in three layers:

1. Ingestion standards: LAS, DLIS, WITSML, PRODML, RESQML, PPDM.
2. Scene standards: CRS handling, symbology, map vector and raster conventions.
3. Delivery standards: GeoJSON, GeoPackage, GeoTIFF, SVG, PDF, image export.

That split keeps the render engine stable while letting the data layer evolve.