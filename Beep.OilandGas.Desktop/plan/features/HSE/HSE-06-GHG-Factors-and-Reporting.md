# HSE-06 — GHG factors and reporting

| Field | Value |
|---|---|
| Stage | STG-11 — HSE, compliance and permits |
| Area | HSE |
| Status | Gate met - see [evidence](../../evidence/STG-11-HSE-06/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement`, `Beep.OilandGas.HSE` |
| Desktop project | `Modules/Hse/Ghg/` |
| External packages | none |
| Depends on | HSE-05 |
| Blocks | — |
| Web reference | `Pages/GhgFactorSetup.razor`, `Pages/PPDM39/Compliance/GHGReport.razor`, `Components/Shared/CompanyEnvironmentIndicator.razor` |

## 1. Purpose

Produce a defensible emissions report from verified activity data and an explicitly reviewed factor
catalog — and refuse when either is not verified.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39.DataManagement/Services/Compliance/GHGReportingService.cs` | reporting |
| `Beep.OilandGas.HSE/Models/GhgFactorCatalog.cs`, `GhgArithmeticCheck.cs` | catalog and arithmetic |
| Tables `COMPANY_GHG_CATALOG_SELECTION`, factor catalog records | persistence |
| `Pages/GhgFactorSetup.razor` | administrator review flow |
| `Components/Shared/CompanyEnvironmentIndicator.razor` | revision mismatch indicator |

## 3. Screens

`HSE → GHG`

- Catalog panel: current catalog, revision, jurisdiction, effective dates, review state.
- Review panel: compare catalog revision with the company environment; require re-review when
  shared or country settings changed.
- Activity data panel: recorded production/consumption entities with source and units.
- Report: emissions by gas and scope, factor used per line, total; export.
- Refusal state: explains exactly what is missing (verified activity, applicable factors).

## 4. Data and contracts

`OgGhgLine { Activity, Unit, Quantity, Factor, FactorUnit, Revision, Emissions, GasType }`
`OgGhgReport { Period, Lines[], TotalCo2e, CatalogRevision, ReviewState }`.

## 5. States

| State | Behaviour |
|---|---|
| No verified factors | report is unavailable with the reason; **no zero total** |
| No verified activity data | unavailable with the reason |
| Catalog revision mismatch | re-review required before reporting |
| Arithmetic overflow / unrepresentable | refused by `GhgArithmeticCheck` |
| Environment changed after review | indicator requests re-review |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Review catalog | `hse.ghg.review` | yes |
| Select catalog | `hse.ghg.select` | yes |
| Produce report | `hse.ghg.read` | no |
| Save report | `hse.ghg.write` | yes |

## 7. Acceptance criteria

- AC-1 A report without verified factors is unavailable, never zero.
- AC-2 Every report line names its factor, unit, source and revision.
- AC-3 A revision mismatch forces re-review.
- AC-4 Arithmetic refuses unrepresentable values.
- AC-5 The API/screen distinguishes unavailable from zero.
- AC-6 Report export matches the on-screen figures exactly.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~HSE-06"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-11-HSE-06/observed-evidence.json). Nothing new was created: the catalog,
the company's reviewed revision, the environment revision and the activity are all recorded in the owner's tables, and the two
rules that decide whether the report is defensible are the owner's own.

**The owner's reporting service is a refusal stub.** `GHGReportingService` implements every method by throwing *"GHG reporting
requires verified company-scoped activity data and emission factors"*. That message names exactly the two things this feature
must supply, so the report is built here from the recorded rows - but through the owner's `GhgFactorCatalogValidation.Parse`
for the catalog contract and the owner's `GhgArithmetic.Check` for the multiplication, so neither rule is reimplemented.

**The catalog is not a table of factors.** It is a JSON document in `SYSTEM_SETUP_RECORD` with `CATEGORY = 'GHG_FACTORS'` and
the jurisdiction as its key; the company's choice is `COMPANY_GHG_CATALOG_SELECTION`, which records the catalog revision and
the company-environment revision it was reviewed against, and `COMPANY_OILGAS_ENVIRONMENT` carries the environment revision
that holds now. `SystemSetupModule` already owns those three tables, so they were **not** re-registered in `HseModule`; only
`PDEN` and `PDEN_VOL_DISPOSITION` were, because nothing declared them.

| Report element | Recorded in |
|---|---|
| Factor value, input unit, output unit, effective dates, source | the `GHG_FACTORS` catalog JSON |
| Catalog revision | `SYSTEM_SETUP_RECORD.REVISION` |
| Reviewed catalog and environment revisions | `COMPANY_GHG_CATALOG_SELECTION` |
| Environment revision that holds now | `COMPANY_OILGAS_ENVIRONMENT.REVISION` |
| Activity type, product, period type, volume, unit, dates | `PDEN_VOL_DISPOSITION` |
| The field an activity belongs to | `PDEN.FIELD_ID` |

**A report is produced, or it is unavailable - never zero.** The report is built only when the catalog is usable, the review is
current, at least one activity row exists, **and every row has a factor whose category, unit and effective period all match**
and whose product is representable. Otherwise the report is null, the status reads *"report unavailable - not zero"*, the total
reads *"unavailable"*, and the reasons name precisely what is missing: no catalog for the jurisdiction, the review sentence
with both revisions, no recorded activity, the activity with no applicable factor, or the activity whose product cannot be
represented.

**A revision mismatch is its own state.** The review compares what the company recorded with what holds now, so a catalog bump
(`cat-rev-1` -> `cat-rev-2`) and an environment bump (`env-rev-1` -> `env-rev-2`) are told apart and both values are named.
The select command records the pair it reviewed and the report returns.

**The export is structural, not a second calculation.** One formatting helper feeds the grid text, the total text and the CSV,
so the export cannot disagree with the screen; totals are grouped by output unit and are never summed across units.

**Not built, and stated as gaps:** saving the report (no report archive exists, and the setup store is not one); an environment
review re-derived from `TOOLS`/`DEFAULT`, the country preset and the company overrides as the API does (the desktop compares the
recorded environment revision only); a separate review record (review and select are one command); a gas split (a factor carries
an output unit, not a gas, so the report groups by unit and invents no CO2/CH4/N2O breakdown); and unit conversion (a catalog
that mixes `tCO2e` and `kgCO2e` yields two totals and no single figure).

## 9. Open questions and risks

- The repository removed a legacy calculation that defaulted to USA, invented factors and converted
  errors to zero. Any recurrence is a release blocker.
