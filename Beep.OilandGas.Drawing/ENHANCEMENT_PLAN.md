# Beep.OilandGas.Drawing - Enhancement Plan

## Purpose

This plan restructures the drawing library roadmap into a multi-document planning set so implementation can proceed in phases instead of through one broad backlog. The goal is to turn `Beep.OilandGas.Drawing` into a standards-aware visualization platform for petroleum engineering, subsurface interpretation, production surveillance, and field-level map workflows.

## Current Baseline In Code

The library already has a useful technical starting point and should be extended from that foundation rather than redesigned from scratch.

### Implemented surfaces

- `Core/DrawingEngine.cs` and `Core/Viewport.cs` provide the render loop and viewport abstraction.
- `Layers/ILayer.cs` and `Layers/LayerBase.cs` provide the existing layer contract.
- `Visualizations/WellSchematic/WellSchematicLayer.cs` renders basic well schematics.
- `Visualizations/Reservoir/ReservoirLayer.cs` already renders layered reservoir intervals with lithology colors, patterns, and fluid contacts.
- `CoordinateSystems/CoordinateSystem.cs` and `CoordinateSystems/DepthCoordinateSystem.cs` provide depth and generic coordinate handling.
- `DataLoaders` already includes LAS, DLIS, WITSML, PRODML, RESQML, PPDM38, file-based, and SeaBed loaders.
- `Styling` already contains lithology palettes, SVG lithology patterns, and FGDC-oriented assets.
- `Export/ImageExporter.cs` already covers PNG, JPEG, and WebP.

### Confirmed gaps

- No contouring or gridding engine exists yet.
- No production-ready multi-track log renderer exists yet.
- No map scene model exists for CRS-aware plan view or basemap overlays.
- No seismic overlay renderer exists yet.
- Facility, production network, and HSE overlay views are not yet formalized.
- Export coverage is still image-first and not yet presentation, vector, or GIS aware.

## Planning Set

Use this file as the index. The detailed plan is split into focused documents:

- [Standards And Feature Matrix](.plans/STANDARDS_AND_FEATURE_MATRIX.md)
- [PPDM Drawing Domain Map](.plans/PPDM_DRAWING_DOMAIN_MAP.md)
- [Phase 01 - Core Platform And CRS](.plans/PHASE_01_CORE_PLATFORM_AND_CRS.md)
- [Phase 02 - Well Schematics And Survey](.plans/PHASE_02_WELL_SCHEMATICS_AND_SURVEY.md)
- [Phase 03 - Logs, Lithology And Correlation](.plans/PHASE_03_LOGS_LITHOLOGY_AND_CORRELATION.md)
- [Phase 04 - Reservoir Maps And Contours](.plans/PHASE_04_RESERVOIR_MAPS_AND_CONTOURS.md)
- [Phase 05 - Spatial, Seismic And Surface Systems](.plans/PHASE_05_SPATIAL_SEISMIC_AND_SURFACE_SYSTEMS.md)
- [Phase 06 - Export, Performance And Adoption](.plans/PHASE_06_EXPORT_PERFORMANCE_AND_ADOPTION.md)
- [Todo Tracker](.plans/TODO_TRACKER.md)

## Phase Summary

| Phase | Theme | Main Outcome |
| --- | --- | --- |
| 01 | Platform, geometry, CRS, scene contracts | A stable technical base for map, section, and depth-domain rendering |
| 02 | Well schematics, deviation, completion views | Engineer-grade wellbore and completion visualization |
| 03 | Logs, lithology, zonation, correlation | Multi-track log display and interpretation workflows |
| 04 | Reservoir maps, contouring, cross-sections | Property maps, contours, sections, and reservoir interpretation views |
| 05 | Spatial layers, seismic, facilities, production systems | Integrated field map and surface-system visualization |
| 06 | Export, validation, performance, adoption | Production readiness, regression confidence, and integration support |

## Planning Principles

- Standards first: implement around accepted interchange standards before inventing custom formats.
- PPDM aligned: use PPDM subject areas and table families as planning anchors for data coverage.
- Rendering separation: normalize incoming data before rendering so the same scene can be fed by PPDM, Energistics, or flat files.
- Petroleum engineer focus: prioritize workflows such as contour maps, well correlations, trajectory review, completion views, reservoir sections, facility context, and production system visibility.
- Iterative delivery: every phase should leave behind shippable assets, not only architectural preparation.

## Delivery Guidance

- Finish Phase 01 before building broad map features or contouring.
- Build log and survey capability before pushing harder into reservoir interpretation.
- Treat contouring, CRS correctness, and uncertainty handling as engineering requirements, not polish.
- Use the tracker file as the live execution document and keep the phase docs stable as planning references.
- ✅ Extensible architecture

## Future Considerations

- Web-based rendering (WebAssembly)
- Mobile app support
- Cloud-based rendering service
- AI-assisted visualization
- Real-time collaboration
- Integration with drilling rig systems
- VR/AR visualization support

