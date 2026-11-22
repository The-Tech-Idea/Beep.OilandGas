# INT-04 — EDM, CSV and XLSX mapping profiles

| Field | Value |
|---|---|
| Stage | STG-17 — Data interop, mapping and 3D |
| Area | Interop |
| Status | Gate met - see [evidence](../../evidence/STG-17-INT-04/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Wellbore.Loaders` |
| Desktop project | `Modules/Interop/MappingProfiles/` |
| External packages | `TheTechIdea.OilGas.Wellbore.Loaders` |
| Depends on | VSB-02, DAT-06 |
| Blocks | INT-05, INT-06 |
| Web reference | `OilandGasTools/docs/customer/mapping.html`, `data.html` |

## 1. Purpose

Bring well data from vendor formats and spreadsheets by describing the mapping once and reusing it,
instead of hand-mapping every file.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore.Loaders/Tabular/MappingProfile.cs` | `Load`, `LoadBuiltIn`, `LoadFromFile`, `FromJson`, `BuiltInNames` (`csv`, `ppdm39`, `edm`, `template`) |
| `Wellbore.Loaders/Tabular/MappedWellDataProvider.cs` | `LoadWellAsync`, `FindWellsAsync`, `ObserveAsync` |
| `Wellbore.Loaders/Tabular/CsvTabularSource.cs`, `ExcelTabularSource.cs`, `EdmXmlTabularSource.cs` | sources |
| `Wellbore.Loaders/Tabular/TabularWriter.cs`, `XlsxWriter.cs` | write back |
| `Wellbore.Loaders/Tabular/Contracts.cs` | `ITabularSource`, `IWellDataProvider`, `IDataSourceFactory` |
| `Wellbore.Loaders/Profiles/*.profile.json` | built-in profiles |
| `Wellbore.Data/Configuration/DataSources` | configured sources |

## 3. Screens

`Data → Mapping profiles`

- Profile list: name, built-in/custom, entity coverage, last used.
- Profile editor: per entity (well, holeSection, string, component, interval, top, survey), source
  column → canonical field, unit, transform, required flag.
- Test panel: apply the profile to a file and show the resulting `WellModel` preview and warnings.
- Source tester: `DataSources.LoadWell` against a configured connection.
- Write-back: export a well to CSV folder or XLSX using a profile.

## 4. Data and contracts

`OgMappingProfile { Name, Entities[] { Entity, FieldMaps[] }, IsBuiltIn, LastUsed }`
Profile JSON is stored under the data root and versioned with the environment.

## 5. States

| State | Behaviour |
|---|---|
| Required field unmapped | test fails with the field named |
| Entity absent from the file | reported per entity, not as a total failure |
| Profile invalid JSON | refused, previous version retained |
| Built-in profile edited | must be saved as a custom copy |
| Write-back overwrite | confirmation required |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Edit profile | `interop.mapping.write` | file |
| Test | `interop.mapping.read` | no |
| Write back | `interop.mapping.export` | file |

## 7. Acceptance Criteria

- AC-1 A CSV file maps into the canonical `WellModel` via a profile.
- AC-2 An EDM XML file loads the documented entities.
- AC-3 An unmapped required field fails the test with the field named.
- AC-4 A built-in profile cannot be overwritten in place.
- AC-5 Write-back reloads identically through the same profile.
- AC-6 XLSX export produces one sheet per entity.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~INT-04"
```

Port fixtures from `OilandGasTools/tests/TheTechIdea.OilGas.Tests/EdmProfileTests.cs`,
`PpdmProfileTests.cs`, `DataSourceTests.cs`, `ExcelSourceTests.cs`.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-17-INT-04/observed-evidence.json).

**The mapping is not re-implemented.** The profile model, the CSV / EDM / Excel sources, `MappedWellDataProvider` and both writers
are the tools'. A CSV folder and a Landmark EDM export go through **the same code**, and the EDM fixture is **the tools' own test
export**, ported with its expected values.

| What the page shows | Where it comes from |
|---|---|
| Profile list: key, name, system, entities, field count, validity | the built-ins and every `<key>.profile.json` in the profiles folder |
| Entity report: table, rows, present, problems | each entity queried in the source |
| The model the profile produced | `MappedWellDataProvider.LoadWellAsync` |
| Write-back result and the round-trip differences | the model written, then re-read through the same profile |

**AC-3 needed something that did not exist.** A profile could map a field and default a unit but **could not say a field must
exist**, so a source missing a well's identity loaded a record with a blank name and nothing failed. `EntityMap.Required` is
additive and empty by default; the check runs **two ways** — the profile not mapping the field, and the file not carrying the column
it is mapped to — because a blank identity arrives from either direction.

**AC-6 needed a writer decision.** `XlsxWriter` produced exactly **one sheet per workbook**. A multi-sheet overload was added, and
the single-sheet call **now delegates to it**, so the pay-summary export is the same code path and there is still one writer. Sheet
names are cleaned and made unique, because a duplicate name makes a reader drop a sheet silently.

**A first version left the built-ins unreachable.** Their `Name` is a **sentence** — "CSV well exchange (TheTechIdea OilGas)" — while
the key that resolves them is **"csv"**. The summary now carries **both**, the list sorts by the key, and the page asks for the key.

**AC-5 is measured, not claimed.** Every export is **re-read through the same profile** and compared — the well name and UWI, six
entity counts, each hole section's bottom and each string's outside diameter — and a unit test **drops a hole section to prove the
comparison fails** when it should.

**AC-4 is enforced before any write.** A built-in name is refused with the instruction to save a copy; both an unparseable and a
structurally broken profile are refused, so the version on disk stays **byte-identical**, which the test asserts.

**Recorded rather than invented:** the screen's **"last used" column is left out**, because nothing records profile use and a file's
timestamp says when it was edited, not when it was used. And the **write-back exports the well that was tested from its file** — not
a database well, because exporting from PPDM is INT-02/INT-03's direction of travel.

**Not built, and stated as gaps:** a field-by-field profile editor (the profile is authored as JSON and validated on save); a file
browser; a last-used stamp; a comparison of every column rather than the listed ones; vendor datasource plugin loading, which the
feature's own open question leaves to REL-01; and paging.

## 9. Open questions and risks

- Vendor datasource plugins (`IDataSourceFactory` DLLs) load from a folder; decide whether the
  desktop ships a plugin folder and record it in REL-01.
