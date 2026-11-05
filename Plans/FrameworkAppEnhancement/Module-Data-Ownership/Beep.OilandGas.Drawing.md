# Beep.OilandGas.Drawing — data ownership restoration plan

<!-- ownership-current:begin -->
## Current implementation

0 original files now belong to this project. All 502 original Data sources have been moved/consolidated. Actual paths/namespaces below supersede the initial proposal tables. Current verification: all 39 direct framework projects build with the canonical analyzer and default BeepDM source dependencies; 41 selected caller tests, 88 ownership tests, 6 native financial contract tests and 127 selected financial behavior cases pass (only the 201-write history case excluded). Compiled ownership/serialization/permit metadata checks pass; all 1,200 restored declarations are present with no cross-project duplicates. These are scoped checks using existing dependency outputs. Follow-up relationship tests pass 58/58 and the internal 73-project graph has no literal-reference cycles. Canonical AccountManagement and ThemeBranding outputs now build. The final native fixture boundary gate passes 71/71, including all ten prior expanded-run failures and the explicit missing-projection refusal; a full expanded rerun is not claimed. Default repository tests pass 69 with 3 skips, and 22/25 top-level test projects compile. The isolated 201-write history gate passes 1/1. Nodal and Pipeline callers now pass 13 and 9 focused cases; Diagnostics-dependent host verification remains open. The nested company-environment consumer passes 65/65 against actual owners after removing eight moved-definition source links. Expanded access tests passed 587/594 before final repairs; all seven failures pass in the final 27-case targeted recheck. A fresh full 594-case rerun is not claimed. See evidence/canonical-dependency-recovery/observed-evidence.json.

This project consumes changed contracts; no original Data definitions are assigned here. Caller checks remain in scope.

Shared contract/native partial groups use the existing Models project where lower-level consumers require them. Feature implementations remain in their existing projects. There is no replacement central data project.
<!-- ownership-current:end -->

Date: 2026-11-04. Status: Implementation applied; final owner/caller verification in progress.

Project: `Beep.OilandGas.Drawing/Beep.OilandGas.Drawing.csproj`. Accountable: repository owner. Responsible: implementing engineer; affected module maintainer reviews ownership and behavior.

## Scope and ownership

0 candidate incoming files; 123 caller files contain affected identifiers. Counts are lexical evidence; resolve same-name types semantically before edits.

Use this existing project for its own domain types. Domain rules/helpers travel with the owning feature. API transport types shared across layers need an existing dependency-safe contract owner; do not move a shared DTO into a concrete service assembly if that creates a cycle. Keep unresolved mappings open in inventory.json.

## Incoming files and proposed names

| Source | Current namespace / types | Historical same-name candidates | Naming and destination action |
| --- | --- | --- | --- |

## Calling code to update

The following files reference affected identifiers. For each, resolve its exact symbol against inventory.json, then update imports, fully-qualified names, generic arguments, constructor/DI wiring and project references together. A lexical match is not proof of a dependency.

- `Beep.OilandGas.Drawing/Builders/FieldMapBuilder.cs`
- `Beep.OilandGas.Drawing/Builders/ReservoirContourBuilder.cs`
- `Beep.OilandGas.Drawing/Builders/ReservoirCrossSectionBuilder.cs`
- `Beep.OilandGas.Drawing/Builders/ReservoirMapBuilder.cs`
- `Beep.OilandGas.Drawing/Builders/ReservoirVisualizationResolver.cs`
- `Beep.OilandGas.Drawing/Builders/WellCorrelationBuilder.cs`
- `Beep.OilandGas.Drawing/Builders/WellLogBuilder.cs`
- `Beep.OilandGas.Drawing/Builders/WellSchematicBuilder.cs`
- `Beep.OilandGas.Drawing/CoordinateSystems/CoordinateAxisDefinition.cs`
- `Beep.OilandGas.Drawing/CoordinateSystems/CoordinateReferenceSystem.cs`
- `Beep.OilandGas.Drawing/CoordinateSystems/DepthTransform.cs`
- `Beep.OilandGas.Drawing/CoordinateSystems/MeasurementUnit.cs`
- `Beep.OilandGas.Drawing/Core/DrawingEngine.cs`
- `Beep.OilandGas.Drawing/Core/Viewport.cs`
- `Beep.OilandGas.Drawing/DataLoaders/DataLoaderFactory.cs`
- `Beep.OilandGas.Drawing/DataLoaders/IDataLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/ILayerLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/ILogLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/IReservoirLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/ISchematicLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Implementations/CsvLogLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Implementations/DatabaseLogLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Implementations/DlisLogLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Implementations/FileSchematicLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Implementations/LasLogLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Implementations/Ppdm38LayerLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Implementations/Ppdm38SchematicLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Implementations/ProdmlLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Implementations/ResqmlReservoirLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Implementations/SeaBedSchematicLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Implementations/WitsmlLogLoader.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Models/DataLoadResult.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Models/DataLoadStatistics.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Models/DeviationSurvey.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Models/FieldMapModels.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Models/LogCurveInfo.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Models/LogData.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Models/LogIntervalData.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Models/LogLoadConfiguration.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Models/ReservoirData.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Models/ReservoirGridSurfaceModels.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Models/ReservoirMapOverlayModels.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Models/ReservoirSectionModels.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Models/WellIdentifier.cs`
- `Beep.OilandGas.Drawing/DataLoaders/Models/WellSchematicData.cs`
- `Beep.OilandGas.Drawing/DataLoaders/PWLS/LogDataIngestionNormalizer.cs`
- `Beep.OilandGas.Drawing/DataLoaders/PWLS/PwlsMnemonicMapper.cs`
- `Beep.OilandGas.Drawing/DataLoaders/WITSML/DataWorkOrderLoader.cs`
- `Beep.OilandGas.Drawing/Exceptions/DrawingException.cs`
- `Beep.OilandGas.Drawing/Exceptions/RenderingException.cs`
- `Beep.OilandGas.Drawing/Export/GeoJsonExporter.cs`
- `Beep.OilandGas.Drawing/Export/GeoReferencedImageExporter.cs`
- `Beep.OilandGas.Drawing/Export/ImageExporter.cs`
- `Beep.OilandGas.Drawing/Export/PdfExporter.cs`
- `Beep.OilandGas.Drawing/Export/SvgExporter.cs`
- `Beep.OilandGas.Drawing/Interaction/HitTestGeometry.cs`
- `Beep.OilandGas.Drawing/Interaction/IInteractiveLayer.cs`
- `Beep.OilandGas.Drawing/Interaction/LayerHitResult.cs`
- `Beep.OilandGas.Drawing/Interaction/SceneInteractionRenderStyle.cs`
- `Beep.OilandGas.Drawing/Interaction/SceneInteractionRenderer.cs`
- `Beep.OilandGas.Drawing/Interaction/SceneInteractionState.cs`
- `Beep.OilandGas.Drawing/Interaction/SceneMeasurementAnnotation.cs`
- `Beep.OilandGas.Drawing/Interaction/SceneSelectionAnnotation.cs`
- `Beep.OilandGas.Drawing/Interaction/SceneWorldPoint.cs`
- `Beep.OilandGas.Drawing/Layers/ILayer.cs`
- `Beep.OilandGas.Drawing/Layers/LayerBase.cs`
- `Beep.OilandGas.Drawing/Layers/WellLogLayer.cs`
- `Beep.OilandGas.Drawing/Measurements/SceneMeasurementGeometryKind.cs`
- `Beep.OilandGas.Drawing/Measurements/SceneMeasurementKind.cs`
- `Beep.OilandGas.Drawing/Measurements/SceneMeasurementResult.cs`
- `Beep.OilandGas.Drawing/Measurements/SceneMeasurementService.cs`
- `Beep.OilandGas.Drawing/Primitives/LegendRenderer.cs`
- `Beep.OilandGas.Drawing/Primitives/NorthArrowRenderer.cs`
- `Beep.OilandGas.Drawing/Primitives/OverlayAnchor.cs`
- `Beep.OilandGas.Drawing/Primitives/ScaleBarRenderer.cs`
- `Beep.OilandGas.Drawing/Rendering/ContourGenerator.cs`
- `Beep.OilandGas.Drawing/Rendering/LayersRenderer.cs`
- `Beep.OilandGas.Drawing/Rendering/LayersRendererConfiguration.cs`
- `Beep.OilandGas.Drawing/Rendering/LogRenderer.cs`
- `Beep.OilandGas.Drawing/Rendering/LogRendererConfiguration.cs`
- `Beep.OilandGas.Drawing/Rendering/LogTrackDefinition.cs`
- `Beep.OilandGas.Drawing/Rendering/LogTrackTemplates.cs`
- `Beep.OilandGas.Drawing/Rendering/ReservoirCrossSectionExtractor.cs`
- `Beep.OilandGas.Drawing/Rendering/WellRenderer.cs`
- `Beep.OilandGas.Drawing/Rendering/WellRendererConfiguration.cs`
- `Beep.OilandGas.Drawing/Rendering/WellborePathCalculator.cs`
- `Beep.OilandGas.Drawing/Samples/DrawingSampleExportAction.cs`
- `Beep.OilandGas.Drawing/Samples/DrawingSampleGallery.cs`
- `Beep.OilandGas.Drawing/Samples/DrawingSampleScene.cs`
- `Beep.OilandGas.Drawing/Scenes/DrawingScene.cs`
- `Beep.OilandGas.Drawing/Scenes/DrawingSceneKind.cs`
- `Beep.OilandGas.Drawing/Scenes/SceneViewportState.cs`
- `Beep.OilandGas.Drawing/Styling/ColorPalette.cs`
- `Beep.OilandGas.Drawing/Styling/LithologyColorPalette.cs`
- `Beep.OilandGas.Drawing/Styling/LithologyPatternRenderer.cs`
- `Beep.OilandGas.Drawing/Styling/SvgColorProcessor.cs`
- `Beep.OilandGas.Drawing/Styling/SvgLithologyPatternLoader.cs`
- `Beep.OilandGas.Drawing/Styling/SvgPatternLoaderTest.cs`
- `Beep.OilandGas.Drawing/Styling/Theme.cs`
- `Beep.OilandGas.Drawing/Styling/UsgsFgdcPatternMapping.cs`
- `Beep.OilandGas.Drawing/Validation/DataNormalizationValidator.cs`
- `Beep.OilandGas.Drawing/Validation/DrawingDiagnostic.cs`
- `Beep.OilandGas.Drawing/Validation/DrawingDiagnosticSeverity.cs`
- `Beep.OilandGas.Drawing/Validation/DrawingSceneValidator.cs`
- `Beep.OilandGas.Drawing/Validation/DrawingValidationReport.cs`
- `Beep.OilandGas.Drawing/Validation/WellDataValidator.cs`
- `Beep.OilandGas.Drawing/Visualizations/FieldMap/FieldMapConfiguration.cs`
- `Beep.OilandGas.Drawing/Visualizations/FieldMap/FieldMapLayer.cs`
- `Beep.OilandGas.Drawing/Visualizations/Reservoir/ReservoirContourConfiguration.cs`
- `Beep.OilandGas.Drawing/Visualizations/Reservoir/ReservoirContourLayer.cs`
- `Beep.OilandGas.Drawing/Visualizations/Reservoir/ReservoirCrossSectionConfiguration.cs`
- `Beep.OilandGas.Drawing/Visualizations/Reservoir/ReservoirCrossSectionLayer.cs`
- `Beep.OilandGas.Drawing/Visualizations/Reservoir/ReservoirLayer.cs`
- `Beep.OilandGas.Drawing/Visualizations/Reservoir/ReservoirMapConfiguration.cs`
- `Beep.OilandGas.Drawing/Visualizations/Reservoir/ReservoirMapOverlayLayer.cs`
- `Beep.OilandGas.Drawing/Visualizations/WellSchematic/Configuration/WellSchematicConfiguration.cs`
- `Beep.OilandGas.Drawing/Visualizations/WellSchematic/EnhancedWellSchematicRenderer.cs`
- `Beep.OilandGas.Drawing/Visualizations/WellSchematic/Rendering/AnnotationRenderer.cs`
- `Beep.OilandGas.Drawing/Visualizations/WellSchematic/Rendering/EquipmentRenderer.cs`
- `Beep.OilandGas.Drawing/Visualizations/WellSchematic/Rendering/LegacyEquipmentSvgResolver.cs`
- `Beep.OilandGas.Drawing/Visualizations/WellSchematic/Rendering/PathHelper.cs`
- `Beep.OilandGas.Drawing/Visualizations/WellSchematic/Rendering/WellborePathCalculator.cs`
- `Beep.OilandGas.Drawing/Visualizations/WellSchematic/WellSchematicLayer.cs`

## Migration tasks

- [x] Record the confirmed existing owner, destination, namespace and dependency decisions in inventory.json and restoration-progress.json; current destination hashes are in observed evidence.
- [x] Keep the internal project graph acyclic and record shared-contract placements without another global data project. The current static superset audit covers 69 existing projects and 284 literal project-reference edges, with zero cycles; external symbolic references are listed separately in evidence/canonical-dependency-recovery/project-reference-audit.json.
- [ ] Move definitions once, update all listed callers and references, then remove forwarding stubs/obsolete definitions. Preserve source changes from the soft-reset commit.
- [ ] Fix CLR namespaces and ambiguous/duplicate DTO names; retain native table/key names and public JSON member names unless all consumers migrate together. Update explicit JsonPropertyName, EF/Beep mappings, entity registration, migration discovery, serializers and stored assembly-qualified type names where applicable.
- [ ] Update typed HTTP client signatures, controller request/response types, service interfaces, DI registration, Razor bindings, setup/module manifests, tests, source-linked fixtures and build scripts that name the moved source/assembly.
- [ ] Reuse existing log, audit and notification paths; check that changed exception/type names do not hide errors, lose correlation or change UI failure feedback. IdentityServer stays authentication-only and app RBAC remains authoritative.
- [ ] Update relevant docs and project/solution/package references. Remove stale centralized-ownership instructions as part of the authorized implementation.

## Verification and dependencies

Run only affected checks selected against the active master tracker. Compile the target owner and immediate consumers, then verify wire serialization, native entity/key registration, migration discovery and retained app-role/company/field boundaries. Test projects must replace source links and assembly HintPaths to moved definitions, not create test-only substitute models. Record full-host/provider/build failures separately from focused harness results. Current executed checks and unavailable integration gates are recorded in the Current implementation section and evidence/canonical-dependency-recovery/observed-evidence.json.

Depends on the common ownership/naming decision and inventory review; caller migration follows each owner group. Finish with one-definition and zero-obsolete-reference checks. Keep a move manifest so an individual slice can be reversed without resetting unrelated staged work.
