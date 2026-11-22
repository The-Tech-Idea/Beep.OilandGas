# ENG-01 — Survey uncertainty and anti-collision

| Field | Value |
|---|---|
| Stage | STG-16 — Well engineering toolboxes |
| Area | Engineering |
| Status | Gate met - see [evidence](../../evidence/STG-16-ENG-01/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Wellbore.Data` |
| Desktop project | `Modules/Engineering/AntiCollision/` |
| External packages | `TheTechIdea.OilGas.Wellbore.Data` (graphics-free) |
| Depends on | VSB-02, WEL-03 |
| Blocks | VSB-07, ENG-06 |
| Web reference | `OilandGasTools/docs/customer/anti-collision.html` |

## 1. Purpose

Prove the wells do not collide: compute ISCWSA survey uncertainty, separation factors and minimum
allowable separation distance against offset wells, and export the evidence.

## 2. Source components

| Source | Reuse |
|---|---|
| `Wellbore.Data/Model/SurveyUncertainty.cs` | `Compute(Trajectory, errorSources, sigma)`, `ForWell(...)` |
| `Wellbore.Data/Model/IscwsaErrorModel.cs`, `ErrorFormula.cs` | error model |
| `Wellbore.Data/Model/WorldMagneticModel.cs`, `MagneticReference.cs`, `NorthCorrection.cs` | north reference |
| `Wellbore.Data/Model/AntiCollision.cs` | `Scan(reference, offsets, warnings, settings)`, `SeparationFactor(...)`, `Masd(...)`, `ToCsv(...)` |
| `Wellbore.Data/Configuration/model-catalog.json`, `tool-models.json` | tool models |
| `Wellbore.Data/Resources` `WMM2025.COF` | geomagnetic model |
| `Mapping/CoordinateSystems/*` | plan geometry for VSB-07 |

## 3. Screens

`Engineering → Anti-collision`

- Reference well selector; offset well multi-selector with a distance filter.
- Settings: error model, sigma, tool model per interval, MASD rule, minimum separation factor.
- Results grid per station: MD, separation, sigma, SF, MASD, status (ok / watch / violation).
- Chart: separation factor vs depth with the threshold line.
- Warnings list from `AntiCollision` warnings.
- Export CSV and print pack.

## 4. Data and contracts

`OgSeparationRow { Md, Distance, SigmaS, SeparationFactor, Masd, Status }`
Settings come from the environment (`antiCollision`) and must be shown on screen.

## 5. States

| State | Behaviour |
|---|---|
| No error model | computation refused, naming the missing model |
| Offset well without survey | that offset is excluded and listed |
| Below minimum SF | violation rows highlighted, count in the header |
| Geomagnetic model unavailable | refused for magnetic azimuths, stated |
| No offsets | scan disabled with the reason |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Compute | `engineering.anticollision.read` | no |
| Save run | `engineering.anticollision.write` | yes |
| Export | `data.export` | no |

## 7. Acceptance Criteria

- AC-1 An ISCWSA reference case reproduces the published ellipse within tolerance.
- AC-2 Separation factor and MASD use the configured rule and the rule is displayed.
- AC-3 An offset without a survey is excluded and named.
- AC-4 Violations are counted and highlighted.
- AC-5 CSV export matches the grid exactly.
- AC-6 A run records the error model, sigma and environment revision.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ENG-01"
```

Port the reference assertions from `OilandGasTools/tests/TheTechIdea.OilGas.Tests/IscwsaConformanceTests.cs`.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-16-ENG-01/observed-evidence.json). The calculus is the tools' own; what
this feature adds is the survey read, the exclusion bookkeeping, the export and the run record.

**Nothing was re-implemented.** The scan calls `AntiCollision.Scan` with the environment's own `AntiCollisionSettings` - k = 3.5,
Sm = 0.3 m, sigmaPA = 0.5 m, a 10 m scan step and the four recorded levels (Stop below 1.0, Adjust plan below 1.25, Shut in /
adjust below 1.5, Report below 4.0). The page prints that rule from the same settings object, so a changed k changes what is
displayed.

| What the page shows | Recorded in |
|---|---|
| Reference well, identity and name | `WELL.UWI`, `WELL_NAME` |
| Wellhead position and depth datum | `WELLBORE.SURFACE_LATITUDE`, `SURFACE_LONGITUDE`, `DEPTH_DATUM` |
| Stations | `WELL_DIR_SRVY_STATION.STATION_MD`, `INCLINATION`, `AZIMUTH` |
| North reference | `WELL_DIR_SRVY.AZIMUTH_NORTH_TYPE` |
| The rule applied | the environment's `AntiCollisionSettings` |
| A saved run | the added `ANTICOLLISION_RUN` |

**AC-1 is proven against the published case, not against itself.** The tools' own ISCWSA MWD Rev 5 fixture
(`iscwsa-1-mwd-rev5.json`, 210 KB) was copied into the test project and every station's six covariance terms - NN, EE, VV, NE,
NV, EV - are compared to a relative 1e-9 through the same entry point the scan's model uses.

**AC-4 follows the owner's vocabulary.** A point is a violation when its level is the level the settings name as the lowest; the
desktop neither hard-codes 1.0 nor invents a severity of its own, and the count shown is computed from the same rows the grid
holds.

**AC-3 is bookkeeping the library leaves to the caller.** An offset with fewer than two stations, or with no usable position, is
excluded from the scan and **named** in the result and on the page, rather than quietly missing from the rows.

**A gap was closed in the owning module.** Nothing in either repository recorded that a scan had been performed, and therefore
nothing recorded the error model, the sigma or the settings it used. `ANTICOLLISION_RUN` now carries them - with the settings as
JSON, the station and violation counts as they were, and the actor and time - so a run can be reproduced and audited.

**Two corrections came out of the run.** The library labels each separation row with the offset's *well header name* rather than
the identity that was scanned, so the first assertion comparing `OFF-1` failed against `Offset 1`; and the first draft recorded
the reference well's *display name* in the run, so the saved row could not be found by the UWI that was scanned. The result now
carries the identity that was asked for.

**Not built, and stated as gaps:** a per-run override of the settings (they come from the environment), a choice of survey when a
well records more than one (the one with the most stations is used, with a warning), support for an offset with no recorded
wellhead position, re-quoting the uncertainty at 2 or 3 sigma, a count of skipped inactive stations, and the travelling-cylinder
and plan plots - those belong to VSB-07, which already renders them.

## 9. Open questions and risks

- `WMM2025.COF` is an embedded resource; verify it survives packaging (REL-01).
