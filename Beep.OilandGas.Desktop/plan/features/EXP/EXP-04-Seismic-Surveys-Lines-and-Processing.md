# EXP-04 — Seismic surveys, lines and processing

| Field | Value |
|---|---|
| Stage | STG-03 — Exploration and seismic |
| Area | Exploration |
| Status | Gate met - AC-1..AC-5: survey register with acquisition state (from the recorded dates) and reference-coded processing state, lines scoped to the selected survey, the "no data loading" notice is a constant, a survey without coordinates disables the map with the reason, and processing states are reference codes plus labels (an unknown code is named as such). See [evidence](../../evidence/STG-03-EXP-04/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProspectIdentification` |
| Desktop project | `Modules/Exploration/Seismic/` |
| External packages | `TheTechIdea.OilGas.Mapping` (line geometry on a map) |
| Depends on | EXP-01 |
| Blocks | — |
| Web reference | `Pages/PPDM39/Exploration/SeismicTracker.razor`, `SeismicSurveys.razor`, `SeismicLines.razor`, `Pages/PPDM39/Seismic/Surveys.razor`, `Acquisition.razor`, `Processing.razor` |

## 1. Purpose

Track seismic surveys from planning through acquisition and processing, and see where the lines
are. **No seismic data is loaded or interpreted** — this is a register and a map, and the UI must
say so.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProspectIdentification/Services/SeismicAnalysisService.cs`, `ISeismicAnalysisService` | survey and line reads |
| Tables `SEIS_SURVEY`, `PROSPECT_SEIS_SURVEY`, seismic line records | persistence |
| `Mapping/CoordinateSystems/*`, `Rendering/ContourMapView` | line geometry display |

## 3. Screens

`Exploration → Seismic`

- Survey grid: name, type (2D/3D/4D), area, acquisition dates, status, contractor, processing state.
- Line list for the selected survey: line name, direction, length, shot count.
- Map panel: survey outline and line traces when coordinates exist.
- Processing panel: sequence, vendor, state, deliverables, notes.
- Explicit notice: "This screen records survey metadata. Seismic data is not loaded or interpreted."

## 4. Data and contracts

`OgSeismicSurvey { Id, Name, Kind, Area, StartDate, EndDate, Status, Contractor, ProcessingState }`
`OgSeismicLine { SurveyId, Name, Direction, Length, LengthUnit, ShotCount }`

## 5. States

| State | Behaviour |
|---|---|
| No coordinates | map panel disabled with the reason |
| Processing state unrecorded | shown as "not recorded", not "planned" |
| Failed read | failed state, no empty map |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Edit survey/line | `exploration.seismic.write` | yes |
| Edit processing state | `exploration.seismic.write` | yes |

## 7. Acceptance criteria

- AC-1 Surveys list with acquisition and processing state.
- AC-2 Lines are scoped to the selected survey.
- AC-3 The "no data loading" notice is always visible.
- AC-4 A survey without coordinates disables the map and states why.
- AC-5 Processing state uses reference codes, not free text.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~EXP-04"
```

## 9. Open questions and risks

- Three Web pages under `Seismic/` duplicate the exploration ones; this feature consolidates them.
- No SEG-Y reader exists in either repository. Do not imply one.
