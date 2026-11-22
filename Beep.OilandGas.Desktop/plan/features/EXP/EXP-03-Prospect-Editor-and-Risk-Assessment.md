# EXP-03 — Prospect editor and risk assessment

| Field | Value |
|---|---|
| Stage | STG-03 — Exploration and seismic |
| Area | Exploration |
| Status | Gate met - AC-1..AC-6: chance from the five recorded probabilities only, a missing element reads "incomplete" with no number, volume percentiles carry the recorded unit and are checked for order, a submitted prospect is read-only and revised into a new draft version, every risk save records assessor and date, and the economic summary is read-only with its ECO-01 link gated. See [evidence](../../evidence/STG-03-EXP-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProspectIdentification` |
| Desktop project | `Modules/Exploration/ProspectEditor/` |
| External packages | `TheTechIdea.OilGas.Mapping` (optional location map) |
| Depends on | EXP-02 |
| Blocks | EXP-05 |
| Web reference | `Pages/PPDM39/Exploration/ProspectDetail.razor`, `Prospects.razor`, `Components/ProspectEditDialog.razor` |

## 1. Purpose

Capture a prospect completely enough that its chance and volume can be defended: identity, trap,
reservoir, source, seal, volume estimate, and one risk row per element.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProspectIdentification/Services/ProspectIdentificationService.cs` and `.Risk` partial | prospect + risk |
| `ProspectIdentification/Services/IProspectRiskEconomicAnalysisService` | risk and economics |
| `ProspectIdentification/Data/Contracts/*` | request records |
| Tables `PROSPECT`, `PROSPECT_TRAP`, `PROSPECT_RESERVOIR`, `PROSPECT_SOURCE_ROCK`, `PROSPECT_RISK_ASSESSMENT`, `PROSPECT_VOLUME_ESTIMATE`, `PROSPECT_ANALOG`, `PROSPECT_ECONOMIC` | persistence |

## 3. Screens

`Exploration → Prospect` (tabs)

1. Identity — name, play, lead, basin, field, operator, status, owner.
2. Trap — type, closure area, spill point, seal type, confidence.
3. Reservoir — formation, lithology, porosity, permeability, net pay, fluid type.
4. Source — source rock, maturity, migration path.
5. Volume — area, gross rock volume, recovery factor, P90/P50/P10 (all with units).
6. Risk — one row per element: element, sub-element, probability, rationale, assessor.
7. Economics — links to ECO-01 scenario (read-only summary).
8. History — stage changes, assessments, documents.

## 4. Data and contracts

`OgProspectRiskElement { Element, SubElement, Probability, Rationale, Assessor, AssessedAt }`
Chance of success is **computed from the recorded probabilities only**, and only when every
required element is present.

## 5. States

| State | Behaviour |
|---|---|
| Missing risk element | chance is "incomplete"; save is allowed, submission is not |
| Volume without unit | refused |
| P90 > P50 or P50 > P10 | refused with the ordering rule |
| Concurrency conflict | side-by-side compare |
| Stage submitted | all fields read-only; a revision creates a new version |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Edit prospect | `exploration.write` | yes |
| Record risk | `exploration.risk.write` | yes |
| Submit for maturation | `exploration.mature` | yes + gate |

## 7. Acceptance criteria

- AC-1 A prospect with all required risk elements produces a chance value.
- AC-2 A prospect with a missing element shows incomplete — never a number.
- AC-3 Volume percentiles are ordered and unit-annotated.
- AC-4 A submitted prospect cannot be edited; a revision is a new version.
- AC-5 Every risk row records assessor and date.
- AC-6 Economic summary is read-only and links to ECO-01.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~EXP-03"
```

## 9. Open questions and risks

- `PROSPECT` is a 27-table cluster. Only tabs 1, 3, 5 and 6 are editable in 1.0; record the rest as
  deferred.
- Probability arithmetic must not exceed 1.0 or go below 0; refuse rather than clamp.
