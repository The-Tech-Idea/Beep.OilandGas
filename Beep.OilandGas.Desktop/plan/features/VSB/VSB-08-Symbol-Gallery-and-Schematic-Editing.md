# VSB-08 — Symbol gallery and schematic editing

| Field | Value |
|---|---|
| Stage | STG-15 — Wellbore visual suite |
| Area | Visuals |
| Status | Gate met - AC-1 to AC-6 (235 symbols, 15 categories; assignment visible and undoable; bad catalog refused). See [evidence](../../evidence/STG-15-VSB-08/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.WellSchematic` |
| Desktop project | `Modules/Wells/SymbolGallery/` |
| External packages | `TheTechIdea.OilGas.WellSchematic` |
| Depends on | VSB-05 |
| Blocks | DEC-02 (abandonment symbols) |
| Web reference | none |

## 1. Purpose

Choose and manage the 235 equipment symbols used on schematics, and edit a schematic's components
with undo.

## 2. Source components

| Source | Reuse |
|---|---|
| `WellSchematic/WellSchematicSymbolView.cs` | gallery (`Catalog`, `Filter`, `GalleryMode`, `SymbolId`, `SymbolAt`) |
| `WellSchematic/Catalog/SymbolCatalog.cs` | `Current`, `Default`, `Load`, `FromJson`, `LoadFromFile` |
| `WellSchematic/Resources/wellschematic.symbols.json` (235 symbols, 15 categories), `component-symbols.json` | data |
| `WellSchematic/WellModelConverter.cs` `ApplyEdits`, `SymbolFor` | editing |
| `WellSchematic/WellSchematicTemplates.cs` | templates |
| `WellSchematic/Tally.cs` | tally interaction |

## 3. Screens

`Wells → Symbols`

- Gallery: category rail, symbol grid with name and id, search, preview at a chosen size.
- Assignment: map a component type to a symbol; show overrides vs defaults.
- Schematic edit mode: select a component, change symbol, move, resize, delete; undo/redo.
- Template list: apply a schematic template to a well.
- Import/export: load a customer symbol catalog JSON.

## 4. Data and contracts

`OgSymbolAssignment { ComponentType, SymbolId, IsOverride }`
Editing produces a change record; undo/redo comes from the tools' edit history.

## 5. States

| State | Behaviour |
|---|---|
| Symbol not found for a type | gallery opens filtered to the type's category with a notice |
| Custom catalog fails validation | previous catalog retained, failure reported |
| Edit on an unverified barrier | allowed but marked |
| Nothing selected | edit commands disabled |
| Undo stack empty | commands disabled |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Browse symbols | `wells.read` | no |
| Edit schematic | `wells.schematic.write` | yes |
| Edit catalog | `settings.wellbore.write` | file |

## 7. Acceptance Criteria

- AC-1 The gallery reports the loaded symbol count and categories.
- AC-2 Filtering by category and by search both work.
- AC-3 An assignment change is visible on the schematic immediately.
- AC-4 Undo restores the previous symbol.
- AC-5 An invalid custom catalog is rejected without losing the active one.
- AC-6 The default catalog is restored by an explicit action.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~VSB-08"
```

## 9. Open questions and risks

- Symbol rendering uses SVG resources; confirm `Svg.Skia` compatibility under ADR-001.

### Delivered 2026-11-05

- **Route `symbols`** (en + ar): category rail, search, a symbol grid (Id / Name / Category / Group), a
  rendered preview, a component picker, **Assign**, **Undo** and **Restore default catalog**, with the
  schematic beside the gallery so an assignment is visible at once.
- `OgSymbolGallery` is the whole bridge: `Rows` (category + search), `Categories`, `Summarise`,
  `Preview` (BGRA frame via `SymbolRenderer.Draw`), `Assign` (records the previous id and the undo action),
  `EquipmentChoices`/`FindEquipment`, and `OgSymbolCatalogState` for the active catalog.
- **AC-1** 235 symbols in 15 categories, counted from the catalog and reported in the summary and the smoke
  run.
- **AC-2** category and search filters are separate and combine; search matches id, name and abbreviation.
- **AC-3 / AC-4** assignment changes the drawing and undo restores it byte-for-byte — after
  `CommitDocumentChanges`. **Finding:** the schematic layout is cached against the view document's revision,
  so an in-place edit is invisible until it is committed. The page now commits before every redraw, and the
  tools' own doc comment says the same.
- **AC-5** a parse failure propagates to the page, which reports it and leaves the active catalog in place;
  an empty catalog is refused with `catalog-empty`.
- **AC-6** `RestoreDefault` switches back to `SymbolCatalog.Default`; a one-symbol custom catalog followed by
  a restore returns more than 100 symbols.
- **ADR-001 check:** previews render through SkiaSharp alone — no SVG runtime is needed for the catalog's
  symbol definitions, so the `Svg.Skia` question raised in §9 does not arise for this screen.

### Still open

- Select-on-the-drawing, move, resize and delete are not implemented; assignment uses a component list.
- No file picker or export: `LoadCustom(path)` exists and is tested, but the toolbar button is still to come.
- The catalog editor (adding or overriding a symbol, `OgSymbolAssignment.IsOverride`) is not built; the
  gallery shows what the tools ship.
