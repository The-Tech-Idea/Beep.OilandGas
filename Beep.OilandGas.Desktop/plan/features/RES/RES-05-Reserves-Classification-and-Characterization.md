# RES-05 — Reserves classification and characterization

| Field | Value |
|---|---|
| Stage | STG-06 — Reservoir, PVT and forecasting |
| Area | Reservoir |
| Status | Gate met - see [evidence](../../evidence/STG-06-RES-05/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle` |
| Desktop project | `Modules/Reservoir/Reserves/` |
| External packages | none |
| Depends on | RES-01 |
| Blocks | ECO-01, RPT-01 |
| Web reference | `Pages/PPDM39/Production/Reserves.razor`, `Pages/PPDM39/Reservoir/ReservesClassification.razor`, `CharacterizationSummary.razor`, `ReservoirOverview.razor`, `Pages/PPDM39/BusinessProcess/ReservesMaturationReport.razor` |

## 1. Purpose

Classify reserves by category and confidence, and summarise reservoir characterization — the two
questions an auditor and an investor both ask.

## 2. Source components

| Source | Reuse |
|---|---|
| `LifeCycle/Services/ReservoirLifecycle/ReservoirLifecycleService.cs` | reservoir state |
| `ProductionAccounting/Services/ReserveAccountingService.cs`, `ReserveDisclosureService.cs`, `UnprovedPropertyService.cs`, `FullCostService.cs`, `SuccessfulEffortsService.cs` | reserves accounting |
| Tables `RESERVOIR`, `RESERVOIR_STATUS`, `RESERVES`, `PROSPECT_VOLUME_ESTIMATE` | persistence |

## 3. Screens

`Reservoir → Reserves`

- Classification grid: pool, category (proved/probable/possible), confidence, gross, net, unit,
  revision, effective date, classifier.
- Characterization summary: pressure, temperature, drive mechanism, fluid, rock properties, with
  source for each.
- Maturation funnel: prospects and discoveries moving to reserves.
- Disclosure preview: the report shape used for external disclosure.

## 4. Data and contracts

`OgReserveEntry { PoolId, Category, Gross, Net, Unit, EffectiveDate, Revision, Classifier }`
Volumes carry a unit; conversions only through the catalog.

## 5. States

| State | Behaviour |
|---|---|
| Category unrecorded | shown as "unclassified", excluded from totals |
| Volumes without a classifier | flagged; disclosure blocked |
| Revision | new revision, prior retained |
| Failed read | failed state |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `reservoir.reserves.read` | no |
| Edit classification | `reservoir.reserves.write` | yes |
| Approve disclosure | `reservoir.reserves.approve` + SoD | yes |

## 7. Acceptance criteria

- AC-1 Totals exclude unclassified entries and state how many were excluded.
- AC-2 Every entry names a classifier and an effective date.
- AC-3 Revisions never overwrite history.
- AC-4 Characterization facts each show their source.
- AC-5 Disclosure is blocked while any entry lacks a classifier.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~Reserves"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ReservesGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-06-RES-05/observed-evidence.json). Entries and disclosures are read
and written through `UnitofWork<T>` over the LifeCycle-owned `RESERVE_ENTRY` and `RESERVE_DISCLOSURE`; the
characterization reads PPDM `FIELD`/`POOL`, DevelopmentPlanning `DEVELOPMENT_POOL_PLAN`, LifeCycle `RESERVOIR_STATUS`
and ProspectIdentification `PROSPECT_VOLUME_ESTIMATE`.

**The five Web pages, mapped (section 9):**

| Web page | Here |
|---|---|
| `Production/Reserves.razor` | the entries grid and the totals, route `reserves` |
| `Reservoir/ReservesClassification.razor` | the classification columns of the same grid: category, revision, classifier, effective date |
| `CharacterizationSummary.razor` | the characterization panel, one fact per row with its source |
| `Reservoir/ReservoirOverview.razor` | the field and pool facts inside the same panel |
| `BusinessProcess/ReservesMaturationReport.razor` | the maturation funnel panel, read from `PROSPECT_VOLUME_ESTIMATE` |

**Why the plan's own tables:** the repository had only field-level `PROVED_RESERVES` - proved only, no pool, no net, no
revision and no per-entry classifier - so it cannot hold `OgReserveEntry`. The plan names a `RESERVES` table that does not
exist. `RESERVE_ENTRY` implements the plan's grain and `RESERVE_DISCLOSURE` records the approval the command table says
writes something. `PROVED_RESERVES` stays the accounting record and is untouched.

**Revisions are appends (AC-3):** a new row carries `REVISION = previous + 1` and `SUPERSEDES_ENTRY_ID`; a source-guard
test asserts the service contains no `.Update(` or `.Delete(` on the entry, and the workspace shows history beside the
current revision so the supersession is visible.

**AC-2's reading, stated:** every entry carries a classifier and an effective date, and the effective date is required by
validation. A classifier is *not* required to save, because the state table says such an entry is "flagged; disclosure
blocked" rather than refused - so it is saved, flagged, listed and it blocks the disclosure (AC-5).

**AC-4's honesty:** reservoir pressure and temperature are not recorded anywhere in this repository, so they appear as
"not recorded" with the source "no table in this repository records reservoir pressure". Rock properties are absent for
the same reason. Omitting them would have made a gap look complete.

## 9. Open questions and risks

- Five Web pages cover reserves with different shapes. This feature consolidates them; record the
  route mapping.
