# Phase 06 - Export, Performance And Adoption

## Objective

Finish the platform so it is reliable for product use: interactive behavior, richer export formats, performance work, regression validation, and sample-driven adoption.

## Starting Point In Code

- Image export already exists for PNG, JPEG, and WebP.
- Shared PDF and SVG export now exist through `PdfExporter`, `SvgExporter`, and the engine-level `RenderToCanvas` path.
- GeoJSON export now exists through `GeoJsonExporter` for typed field maps and reservoir-map vector overlays.
- Georeferenced PNG export now exists through `GeoReferencedImageExporter`, using scene world bounds plus CRS sidecars.
- Scene-aware measurement APIs now exist through `SceneMeasurementService` and `DrawingEngine` screen-to-world helpers.
- Hit-testing now exists through `DrawingEngine.HitTest` plus optional interactive-layer contracts for field maps, reservoir overlays, and reservoir contours.
- Hit-testing now also exists for enhanced well schematics, where equipment and perforation intervals resolve as typed downhole features.
- The schematic callout lane is now part of the same interaction surface, so right-side equipment and perforation labels resolve the same typed features as the rendered downhole geometry.
- Scene-owned interaction persistence now exists through `DrawingScene.InteractionState` plus engine helpers for recording selections and measurements.
- An interactive sample host now exists in `Beep.OilandGas.Web` at `/ppdm39/data-management/drawing-samples`, backed by the canonical sample gallery and scene interaction APIs.
- The interactive sample host now also exposes workflow-level PNG, SVG, and PDF export actions through the shared web download service, using the current engine state rather than a host-only export model.
- The interactive sample host now also supports drag-based viewport panning, so live scene navigation is no longer limited to zoom and reset buttons.
- The interactive sample host now also supports targeted removal of the current persisted selection or an individual persisted measurement, so annotation cleanup no longer forces an all-or-nothing reset.
- The interactive sample host now also exposes a georeferenced PNG bundle for eligible map scenes and can surface explicit scene-declared exports such as Field Map GeoJSON without guessing from scene names.
- The interactive sample host now also surfaces a Reservoir Contour GeoJSON action through the same explicit supplemental export contract used for Field Map GeoJSON, so contour map exchange output follows the same host workflow as scene inspection and raster export.
- The Field Map georeferenced PNG bundle now also comes through the same explicit supplemental export contract, so scene-specific zip bundles and single-file exports use one host path instead of splitting capability logic between the sample gallery and the web host.
- The integrated sample host now renders scene-declared export actions with content-aware styling and shows each action's description, so the workflow communicates the difference between exchange files and multi-file bundles instead of presenting all supplemental exports as identical generic buttons.
- A golden-image regression harness now exists in `Beep.OilandGas.Drawing.Tests` with five deterministic scenes and pinned hashes for field maps, logs, contours, cross-sections, and schematics.
- Public sample-gallery APIs now exist through `DrawingSampleGallery` and `WellLogGallery`, so the same canonical scenes can drive docs, demos, exports, and regression coverage.
- An in-process benchmark harness now exists in `Beep.OilandGas.Drawing.Benchmarks`, using the same canonical sample scenes for build and PNG render baselines.
- The engine already uses a layered architecture that can support optimization.
- An interaction framework is still missing.

## Scope

- Vector and document exports.
- Regression and benchmark harnesses.
- Interaction support for hover, selection, zoom, pan, measurement, and annotation.
- Large-dataset performance work.
- Samples and documentation that prove the library across real engineering scenarios.

## Standards And Domain Drivers

- OGC symbology and delivery formats where applicable
- GeoJSON export for vector deliverables
- GeoTIFF and map-image export for georeferenced products
- PPDM-backed validation and metadata traceability

## Work Packages

### 1. Export expansion

- Add SVG and PDF export. Completed.
- Add GeoJSON export for contours and vector overlays. Completed.
- Add georeferenced raster export for map views where feasible. Completed for PNG + world-file + CRS sidecars.

### 2. Interactivity and measurement

- Add zoom, pan, selection, and tooltip behavior. Partially completed through the engine-level hit-test contract for interactive map layers, contour isolines, enhanced well schematic bodies, and schematic callout labels.
- Add measurement tools for map distance, section distance, depth, and area. Completed at the engine and service layer.
- Add annotation persistence hooks. Completed at the scene and engine helper layer.
- Add an interactive sample host for exploration and manual validation. Completed in the integrated web application.

### 3. Reliability and adoption

- Add regression scenes and golden-image comparisons. Completed.
- Add performance baselines for logs, maps, sections, and facility scenes. Completed through the shared benchmark harness.
- Add sample applications and phase-based examples. Completed through the shared gallery API and README quick starts.

## Deliverables

- SVG and PDF export. Completed.
- GeoJSON export for field maps and reservoir map overlays. Completed.
- Georeferenced PNG export with sidecar metadata. Completed.
- Scene-aware measurement API for map, section, and depth scenes. Completed.
- Engine-level hit-testing contract for hover and selection on field maps and reservoir overlays. Completed.
- Reservoir contour participation in the engine-level hit-testing contract. Completed.
- Well schematic depth-scene attachment plus equipment and perforation hit testing. Completed.
- Shared well schematic callout-label layout plus hit testing. Completed.
- Well log depth-scene attachment plus first track-level hit-testing path. Completed.
- Exact well-log curve and interval hit testing from renderer-owned layouts. Completed.
- Scene-owned persistence hooks for selection and measurement annotations. Completed.
- Core-pipeline rendering of persisted selection and measurement annotations for live images and exports. Completed.
- Interactive sample host for the canonical gallery scenes. Completed.
- Workflow-level PNG, SVG, and PDF export from the integrated sample host. Completed.
- Drag-based viewport pan in the integrated sample host. Completed.
- Targeted delete actions for persisted selections and measurements in the integrated sample host. Completed.
- Georeferenced PNG bundle export for eligible map scenes in the integrated sample host. Completed.
- Explicit sample-scene supplemental export contract plus Field Map GeoJSON host action. Completed.
- Reservoir Contour GeoJSON supplemental export through the sample-scene contract. Completed.
- Georeferenced PNG bundle moved onto the sample-scene supplemental export contract. Completed.
- Supplemental export actions presented clearly in the integrated sample host. Completed.
- Reservoir contour georeferenced PNG bundle added through the same sample-scene contract. Completed.
- Reservoir cross-section JSON supplemental export added through the same sample-scene contract. Completed.
- Well schematic JSON supplemental export added through the same sample-scene contract. Completed.
- Well log JSON supplemental export added through the same sample-scene contract. Completed.
- Regression dataset suite. Completed.
- Performance benchmark suite. Completed.
- Sample gallery covering wells, logs, maps, sections, facilities, and overlays. Completed.

## Execution Note

- The regression harness compiles and all five golden hashes are pinned.
- Reservoir contour and reservoir map builders now attach plan-view scenes with CRS and world bounds, so GIS-oriented exports can reuse the same map metadata path as field maps.
- GeoJSON export is intentionally model-driven rather than screen-space-driven, so contour segments, fault traces, and typed field-map assets export directly from normalized engineering geometry.
- Measurement is intentionally scene-driven rather than layer-driven, so an interactive host can convert screen clicks to world coordinates once and then reuse the same measurement path across field maps, sections, and depth views.
- Hover and selection are intentionally opt-in at the layer level, so new renderers can join the interaction pipeline without breaking the existing render-only layer contract.
- Contour hit testing intentionally reuses the generated isoline segments already used during render, which avoids duplicating or approximating contour geometry in a separate interaction structure.
- Well schematic hit testing intentionally reuses the same path calculations used during render, so downhole feature selection stays aligned with the enhanced renderer's actual geometry and depth layout.
- Schematic callout-label hit testing intentionally reuses annotation layout calculations from `AnnotationRenderer`, so the rendered label lane and the interactive hit surface cannot drift apart.
- Well log depth scenes intentionally render in world-depth coordinates instead of pixel-height coordinates, so interaction and measurement can reuse the same depth axis without a host-side remapping step.
- Well log interaction now intentionally reuses `LogRenderer` layout outputs for both curve polylines and interval rectangles, so hit testing follows the exact rendered geometry instead of a second approximation path.
- Density-neutron crossover interaction now intentionally reuses the same renderer-owned segment builder as crossover shading, so the composite hit surface and the rendered shaded polygon stay aligned instead of creating a separate derived geometry path in `WellLogLayer`.
- Configured interval-only tracks now stay in the renderer's resolved track list, which keeps interval-focused log scenes renderable and interactive instead of silently dropping those tracks during layout preparation.
- Interaction persistence is intentionally scene-owned, so hosts can keep selection and measurement artifacts beside viewport and CRS state instead of inventing a separate synchronization model.
- Persisted interaction visuals now render through `DrawingEngine.RenderToCanvas`, so raster and vector exports inherit the same selection and measurement annotations without a separate export-only code path.
- Persisted interaction styling is now intentionally scene-owned as `SceneInteractionState.RenderStyle`, so a host or export workflow can switch visibility, colors, dash patterns, radii, and label sizing without editing `SceneInteractionRenderer` or forking the export path.
- The integrated sample host now exposes Auto, Map, Log, Schematic, Default, and Hidden annotation presets, but it still applies them by mutating the active scene's `RenderStyle` directly so the live image path stays identical to exports instead of introducing a web-only annotation theme model.
- The integrated sample host now keeps the SVG overlay only for in-progress draft measurements; once an annotation is persisted, the host refreshes the engine-rendered image instead of duplicating that visual in web-only markup.
- The canonical gallery scenes now live in the main drawing library and are reused by the regression harness instead of being duplicated only inside the test project.
- The benchmark harness compiles, runs in-process, writes JSON reports, and has completed a smoke run for the `WellLog_Petrophysical` build-scene workload.
- The benchmark harness now also measures `render-png-annotated` and `hit-test`, using benchmark-local interaction profiles to seed persisted annotations and stable feature probes for the canonical map and well-log scenes without adding benchmark-only data to `DrawingSampleGallery`.
- The benchmark harness now also measures `render-svg` and `render-pdf`, so vector export timing and payload size are tracked separately from PNG raster baselines instead of being inferred from `RenderToCanvas` alone.
- The benchmark harness now also measures `render-svg-annotated` and `render-pdf-annotated`, so persisted-annotation overhead is tracked for vector and document exports instead of only for raster image output.
- The benchmark suite intentionally avoids external child-process tooling because repeated process launches are less reliable than in-process execution on this workstation.
- The integrated sample host now exposes Export Scene actions for PNG, SVG, and PDF through the shared `IClientFileExportService`, so the current viewport plus persisted selections and measurements download from the same engine state that drives the live canvas image.
- The integrated sample host now routes drag gestures to `DrawingEngine.Pan`, which keeps viewport navigation on the same scene-owned state path as zoom and export instead of introducing host-only transform state, and its Reset View action now falls back to `ResetViewport()` when a sample renders without a typed scene.
- `SceneInteractionState` now owns targeted `RemoveSelection` and `RemoveMeasurement` operations, so host-level annotation editing can stay on the same scene-owned state contract instead of mutating the public lists directly or clearing the whole interaction history.
- `DrawingSampleScene` now declares explicit supplemental exports, so host workflows can surface scene-specific actions such as Field Map GeoJSON through a typed contract instead of guessing from sample names or renderer composition.
- The same supplemental export contract now also carries Field Map georeferenced PNG as a zip bundle, which confirms the contract can handle both single-file exchange output and multi-file artifact bundles without preserving a separate host-only export branch.
- The same supplemental export contract now also carries Reservoir Contour GeoJSON, which confirms the contract works for both typed field-map data and contour-surface exports without adding more host-specific conditional logic.
- The same supplemental export contract now also carries a Reservoir Contour georeferenced PNG zip bundle, which confirms the raster-bundle path works for map-driven contour scenes as well as field maps without widening the host again.
- The same supplemental export contract now also carries Reservoir Cross Section JSON, which confirms the host path is not limited to map-oriented exchange files and can surface typed engineering section data from the canonical gallery without adding scene-name heuristics.
- The same supplemental export contract now also carries Well Schematic JSON, which confirms the host path can surface typed schematic engineering payloads without adding a separate export model for depth-view samples.
- The same supplemental export contract now also carries Well Log JSON, which confirms the host path can surface typed petrophysical payloads for depth-view scenes and that all five canonical gallery scenes now expose explicit engineering-data artifacts through one contract.
- The integrated sample host now consumes scene-declared supplemental exports uniformly, while the Field Map sample uses `GeoReferencedImageExporter` behind that contract to produce a zip bundle with the PNG, world file, and CRS sidecar.
- The integrated sample host currently derives supplemental export button styling from content type and uses the export descriptions already declared in `DrawingSampleExportAction`, which keeps the UI readable without widening the core sample contract again.
- Repeated `dotnet test` execution is currently blocked in this workspace by Windows Application Control policy, so runnable verification is limited to build validation plus one-shot hash probes.
- Full `Beep.OilandGas.Web` build validation is currently blocked by pre-existing restore failures for `TheTechIdea.Beep.DataManagementEngine` 2.0.88 and `TheTechIdea.Beep.DataManagementModels` 2.0.142, plus an unrelated `Serilog` downgrade in the API dependency chain.

## Exit Criteria

- The library can generate publication-ready and exchange-ready outputs.
- Scene rendering has measurable benchmarks and regression protection.
- New consumers can learn the library from realistic examples instead of raw API exploration.

## Risks

- Export quality will lag behind rendering quality unless vector output is designed deliberately rather than bolted on.
- Performance tuning without benchmark scenes will create anecdotal optimizations instead of durable improvements.
- Interaction hosts still need a shared sample host and broader renderer participation; the current persistence hooks do not yet expose visual editing or cross-layer interaction UX.
- The current web host can only be fully CI-validated after the workspace package feed resolves the missing `TheTechIdea.Beep.*` package versions used by the web and branch projects.