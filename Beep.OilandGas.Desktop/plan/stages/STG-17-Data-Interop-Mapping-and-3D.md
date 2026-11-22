# STG-17 — Data interop, mapping and 3D

| Field | Value |
|---|---|
| Stage | STG-17 |
| Features | 8 (INT-01 … INT-08) |
| Depends on | STG-15, STG-16 |
| Status | Gate met for what a build can prove - INT-01 to INT-08 all gate met. Two acceptance items are the owner's: INT-02 AC-2's Energistics XSDs and INT-03 AC-1's OSDU JSON schemas, both licensing/build-asset decisions rather than code. |
| Exit gate | LAS and DLIS load into a well, WITSML round-trips, a country environment is created by the wizard, and a structure map renders from well data |

---

## 1. Objective

Get data in and out, configure a customer, and map it. This stage turns the desktop from a
viewer of whatever is already in PPDM into a tool that ingests well data from the field.

---

## 2. Scope

**In**

- LAS 2.0 / 3.0 import and export, with LAS 3.0 compliance checking.
- DLIS import (RP66 v1).
- WITSML read, write, 2.1 export, and store client.
- OSDU export and ingest.
- EDM / CSV / XLSX mapping profiles and mapped data providers.
- Setup session wizard: country → data sources → coverage → code mapping → equipment and symbols
  → stratigraphy → lithology → logs → tubulars → surveys → verify → handover.
- Field QC and mapping suggestions.
- Coordinate systems, gridding, kriging, contouring, well maps, bubble maps.
- 3D scenes with glTF/GLB export.

**Out**

- SEG-Y.
- Real-time WITSML streaming (growing-log source is available; the live screen is deferred).
- A full GIS.

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `TheTechIdea.OilGas.Wellbore.Loaders` | `LasReader/Writer`, `Las3Checker`, `DlisReader`, `DlisFile`, `WitsmlReader/Writer`, `Witsml21Export`, `WitsmlStoreClient`, `WitsmlGrowingLogSource`, `OsduExport`, `DataSources`, `MappingProfile`, `MappedWellDataProvider`, `CsvTabularSource`, `ExcelTabularSource`, `EdmXmlTabularSource`, `TabularWriter`, `XlsxWriter`, `DataCoverage`, `FieldQc`, `ConfigurationResources` |
| `TheTechIdea.OilGas.Wellbore.Setup` | `SetupSession` (full wizard API), `DataSourcePlugins`, `MappingSuggestions`, `SimilarityMatcher` |
| `TheTechIdea.OilGas.DataAI` | `LegacyExtraction`, `WellReconciliation`, `OsduIngest` |
| `TheTechIdea.OilGas.Mapping` | `Gridding`, `Kriging`, `Contouring`, `WellMaps`, `CoordinateSystems`, `Geodesy`, `ContourMapView`, `MapExport`, `Scene3D`, `Scene3DView`, `SceneExport`, `MappingToolbox` |
| `TheTechIdea.OilGas.Wellbore.Data` | `CountryPresets` (62), `WellboreEnvironment`, `ConfigurationArchive`, `EnvironmentReport`, `ConfiguredFiles`, `Units` |
| `Beep.OilandGas.HeatMap` | `HeatMapRenderer` as a map layer (ADR-006) |
| `Beep.OilandGas.PPDM39` | Write targets for imported data |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| INT-01 | LAS and DLIS import/export | [INT-01](../features/INT/INT-01-LAS-and-DLIS-Import-Export.md) |
| INT-02 | WITSML read, write and store | [INT-02](../features/INT/INT-02-WITSML-Read-Write-and-Store.md) |
| INT-03 | OSDU export and ingest | [INT-03](../features/INT/INT-03-OSDU-Export-and-Ingest.md) |
| INT-04 | EDM, CSV and XLSX mapping profiles | [INT-04](../features/INT/INT-04-EDM-CSV-and-XLSX-Mapping-Profiles.md) |
| INT-05 | Setup session wizard and country environments | [INT-05](../features/INT/INT-05-Setup-Session-Wizard-and-Country-Environments.md) |
| INT-06 | Field QC and mapping suggestions | [INT-06](../features/INT/INT-06-Field-QC-and-Mapping-Suggestions.md) |
| INT-07 | Mapping, gridding and contour maps | [INT-07](../features/INT/INT-07-Mapping-Gridding-and-Contour-Maps.md) |
| INT-08 | 3D scenes and glTF export | [INT-08](../features/INT/INT-08-3D-Scenes-and-glTF-Export.md) |

---

## 5. New code introduced

`.Modules/Interop` — eight pairs. `.WellTools/Export` extended with LAS/DLIS/WITSML/OSDU writers.
`WizardHost` reused from STG-04 for INT-05. `SkiaViewSurfaceHost` reused for INT-07/08.

---

## 6. Architecture notes

- **Import is staged and previewable.** Load → map → preview → validate → commit. Nothing writes
  on load.
- **A rejected row is reported with its reason and line number.** Never silently dropped.
- **Every import records the source file hash, profile name, environment revision and actor.**
- **Environment is customer data, not app config.** It lives under the data root, is versioned,
  and `EnvironmentReport.Diff` shows what changed between saved revisions.
- **Mapping suggestions are never auto-applied.** This is an explicit rule in
  `TheTechIdea.OilGas.Wellbore.Setup` and must survive the port.
- **3D is software-rendered** on the same `ViewSurface`; no GPU dependency.

---

## 7. Dependencies and sequencing

INT-01, INT-02, INT-03, INT-04 depend on `VSB-02`. INT-05 → INT-06.
INT-07 → INT-08. INT-07 optional heat-map layer after REL-05.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-17-1 | A LAS 2.0 file loads, assigns curves and produces a log set | test `INT-01/T02` |
| SG-17-2 | A LAS 3.0 file passes `Las3Checker` and round-trips | test `INT-01/T04` |
| SG-17-3 | A DLIS file loads frames as log sets with warnings for array channels | test `INT-01/T06` |
| SG-17-4 | WITSML trajectory and markers round-trip through the writer | test `INT-02/T02` |
| SG-17-5 | OSDU export produces schema-valid JSON | test `INT-03/T02` |
| SG-17-6 | A CSV profile maps a file into the canonical `WellModel` | test `INT-04/T03` |
| SG-17-7 | The wizard creates a country environment that `SetupSession.Validate()` accepts | test `INT-05/T04` |
| SG-17-8 | Mapping suggestions are shown and only applied on explicit accept | test `INT-06/T03` |
| SG-17-9 | A contour map renders from ≥ 3 wells with a legend and scale bar | screenshot |
| SG-17-10 | A 3D scene exports to GLB that loads in a viewer | file check |

---

## 9. Test plan

`INT-01/T02`, `INT-01/T04`, `INT-01/T06`, `INT-02/T02`, `INT-03/T02`, `INT-04/T03`,
`INT-05/T04`, `INT-06/T03`, `INT-07/T02`, `INT-08/T02`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| `SetupSession` has a very large surface | Implement the 12 documented steps exactly as `docs/customer/onboarding.html` describes |
| Environment template is 1.2 MB and schema 1.56 MB | Ship as embedded resources; load lazily |
| WITSML store client is SOAP and may need credentials | Provide credential storage via `OgLocalCredentialStore`; never log the token |
| Mapping needs a coordinate system per well | Read from PPDM location records; if absent, require explicit user choice |
