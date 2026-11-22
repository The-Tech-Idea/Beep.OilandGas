# DEV-03 — Development well design

| Field | Value |
|---|---|
| Stage | STG-04 — Development and drilling |
| Area | Development |
| Status | Gate met - see [evidence](../../evidence/STG-04-DEV-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.DevelopmentPlanning` |
| Desktop project | `Modules/Development/WellDesign/` |
| External packages | `TheTechIdea.OilGas.WellSchematic` (schematic preview) |
| Depends on | DEV-02 |
| Blocks | DEV-05, ENG-02 |
| Web reference | `Pages/PPDM39/Development/WellDesign.razor` |

## 1. Purpose

Define each planned well: target, trajectory intent, casing scheme outline, completion concept and
cost — the input to the drilling program and the casing design check.

## 2. Source components

| Source | Reuse |
|---|---|
| `DevelopmentPlanning/Data/Contracts/CreateWellPlan.cs` | well plan contract |
| `DevelopmentPlanning/Services/DevelopmentPlanService.cs` | plan linkage |
| `DevelopmentPlanning` well schedule records | sequence and timing |
| `WellSchematic/WellModelConverter.cs` | preview from evidence |
| `DevelopmentPlanning/Services/CreateWellServiceJob` | service job linkage |

## 3. Screens

`Development → Well design`

- Well list for the plan with status (concept / defined / approved / drilled).
- Design tabs: Objective · Trajectory intent · Casing scheme · Completion concept · Cost · Risks.
- Schematic preview panel (read-only) once casing data exists.
- "Check casing design" action that hands data to ENG-02 and returns the result.

## 4. Data and contracts

`OgDevelopmentWell { Id, Name, Objective, TargetFormation, TargetDepth, DepthUnit, SpudWindow,
EstimatedCost, Currency, DesignState }`.

## 5. States

| State | Behaviour |
|---|---|
| No trajectory | preview disabled with the reason |
| Target depth without unit | refused |
| Design incomplete | program authoring blocked with the missing items |
| Cost revision | new version, old retained |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Edit design | `development.welldesign.write` | yes |
| Approve design | `development.welldesign.approve` | yes |
| Run casing check | `engineering.casing.read` | no |

## 7. Acceptance criteria

- AC-1 A development well is scoped to one plan and one field.
- AC-2 Target depth carries a unit.
- AC-3 The casing check passes its inputs and returns a linked result.
- AC-4 An incomplete design blocks drilling program authoring with named missing items.
- AC-5 Cost revisions are versioned.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WellDesign"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WellDesignGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-04-DEV-03/observed-evidence.json). The design reads and writes
`FIELD_DEVELOPMENT_PLAN`, `DEVELOPMENT_WELL_SCHEDULE` and the DevelopmentPlanning-owned `DEVELOPMENT_WELL_DESIGN`,
`DEVELOPMENT_CASING_STRING` and `DEVELOPMENT_CASING_CHECK` through `UnitofWork<T>`; it contains no statement text.
The casing check is an input/consistency check; the burst, collapse and tension arithmetic is ENG-02's.


## 9. Open questions and risks

- Trajectory design proper is ENG-06 (tools `WellPlan`); this screen records intent only and links.
