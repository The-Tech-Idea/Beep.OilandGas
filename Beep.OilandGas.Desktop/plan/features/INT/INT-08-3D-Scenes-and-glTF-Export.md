# INT-08 — 3D scenes and glTF export

| Field | Value |
|---|---|
| Stage | STG-17 — Data interop, mapping and 3D |
| Area | Interop |
| Status | Gate met - see [evidence](../../evidence/STG-17-INT-08/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Mapping` (`Scene3D`) |
| Desktop project | `Modules/Interop/Scene3D/` |
| External packages | `TheTechIdea.OilGas.Mapping` |
| Depends on | INT-07 |
| Blocks | — |
| Web reference | `OilandGasTools/docs/customer/mapping.html` (3D scenes) |

## 1. Purpose

View the field in 3D: a grid surface with well paths, orbit and pick, and export to glTF/GLB for
use elsewhere.

## 2. Source components

| Source | Reuse |
|---|---|
| `Mapping/Scene3D/Scene3D.cs` | scene graph, `Bounds3`, `SceneSurface`, `SceneWellPath`, `ScenePick` |
| `Mapping/Scene3D/Scene3DView.cs` | view (`FitToScene`, orbit/zoom/pan) |
| `Mapping/Scene3D/Camera3D.cs` | camera |
| `Mapping/Scene3D/SceneExport.cs` | `ToGlb`, `ToGltf` |
| `Mapping/Gridding/Grid.cs` | surface input |

## 3. Screens

`Data → 3D view`

- Surface source: a grid from INT-07 or a built-in sample.
- Well paths: wells to include.
- Vertical exaggeration slider (with the value shown).
- Lighting: ambient/directional intensity and direction.
- Main area: `Scene3DView` inside `SkiaViewSurfaceHost`.
- Picks: click to see the picked surface value and depth.
- Export: GLB/glTF with the current exaggeration.

## 4. Data and contracts

`OgSceneRequest { Surface, Wells[], VerticalExaggeration, Lighting }`.

## 5. States

| State | Behaviour |
|---|---|
| No surface | scene disabled with the reason |
| Very large grid | decimation applied and stated |
| Picking misses | no readout rather than a wrong one |
| Export with extreme exaggeration | warns about file size |
| Render exception | failure panel around the host |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `mapping.read` | no |
| Export | `data.export` | file |

## 7. Acceptance Criteria

- AC-1 A surface with three wells renders and orbits.
- AC-2 Picking returns the value at the picked cell.
- AC-3 Vertical exaggeration is applied to geometry and recorded in the export.
- AC-4 GLB export loads in an external viewer.
- AC-5 Grid decimation is stated when applied.
- AC-6 Large scenes remain interactive (REL-04 budget).

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~INT-08"
```

Port assertions from `OilandGasTools/tests/TheTechIdea.OilGas.Mapping.Tests/Scene3DTests.cs` and
`SceneExportTests.cs`.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-17-INT-08/observed-evidence.json).

**The scene is the tools'.** `Gridding.Run` makes the surface, `Scene3D.AddSurface` converts the depths to elevation in metres,
`AddWells` places the paths **through the same `WellMaps.Paths` the map uses** — so a well stands in the same place on both pages —
and `AddPicks` marks the top in tvdss. The view and the exporter are the engine's too, so **the picture on screen and the file written
are one scene**.

| What the page shows | Where it comes from |
|---|---|
| The scene, orbit, zoom | `Scene3DView` and `Camera3D`, hosted in the Skia surface host |
| The surface, the paths, the picks | `Gridding`, `Scene3D.AddWells`, `Scene3D.AddPicks` |
| The picked cell | `OgSceneRules.PickAt` over the view's own camera |
| The exaggeration | the view's `VerticalExaggeration`, printed beside the slider |
| The decimation | the view's own stride rule, re-derived and stated |
| The GLB or glTF | `SceneExport` |

**AC-5 found a silent behaviour.** The view thins a surface by `ceil(sqrt(cells / maxSurfaceCells))` **and says nothing** — a 300 × 300
grid is drawn from every second node with no sign of it, which is exactly what AC-5 exists to prevent. The page re-derives the same
formula from the same setting and prints the cells, the drawn cells and the stride.

**That exposed a unit error in a first version.** The view's limit counts **cells** — `(Nx-1)(Ny-1)` — while the page reported
**nodes** — `Nx×Ny` — so a grid drawn whole appeared to have lost **188 cells** (8888 against 8700). The gateway caught it, and both
figures are now cells: the unit the view draws in and the unit its limit is expressed in.

**AC-2 needed something the engine does not have.** There is a `Camera3D.Project` and **no inverse**, so nothing turns a click back into
a cell. The pick **projects every node the view draws through the same camera** and takes the nearest inside 25 points, skipping masked
nodes; a click on nothing is refused rather than answered with the closest node that happens to be far away.

**AC-3 and AC-4 are the exporter's own.** The exaggeration is the view's, passed to `SceneExport`, which writes it in the **root node's
extras and as the root scale `[1, ve, 1]`** with the vertices left true. The GLB is validated as a **glTF 2.0 container** — magic,
version, declared length, JSON and BIN chunks, and **every accessor resolving inside the BIN chunk** — which is what an external loader
checks first; the JSON form embeds its buffer as a data URI.

**A limit is stated rather than dressed up as a control.** The ambient and diffuse weights are the engine's **fixed 0.3 and 0.7**,
written into the drawing code, so the page prints them and offers no slider for them; only the light's azimuth, its elevation and the
surface opacity are configuration.

**The projection list is INT-07's**, because `mapping.coordinateSystems` ships empty and one answer to that question is enough.

**Not built, and stated as gaps:** a per-scene lighting control (the weights are constants); an exact ray/quad pick (the nearest drawn
node is what the feature asks for); net pay and arbitrary per-well values (the pay zones are PAY's data); a saved scene; and a REL-04
budget figure for this application on a named machine — the interactivity evidence is a measured frame in the test host.

## 9. Open questions and risks

- 3D is software-rendered on the 2D canvas. Expect slow interaction on large grids; measure and
  document the decimation policy.
