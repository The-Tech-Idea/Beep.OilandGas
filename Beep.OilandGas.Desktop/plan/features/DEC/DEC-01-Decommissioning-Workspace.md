# DEC-01 — Decommissioning workspace

| Field | Value |
|---|---|
| Stage | STG-12 — Decommissioning |
| Area | Decommissioning |
| Status | Gate met - see [evidence](../../evidence/STG-12-DEC-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.Decommissioning` |
| Desktop project | `Modules/Decommissioning/Workspace/` |
| External packages | `TheTechIdea.OilGas.WellSchematic` (`Abandonment`) |
| Depends on | WEL-04, HSE-01 |
| Blocks | DEC-02, DEC-03 |
| Web reference | `Pages/PPDM39/Decommissioning/DecommissioningWorkspace.razor` (`/lifecycle/decommissioning/dashboard`) |

## 1. Purpose

The decommissioning coordinator's landing: what is in scope, what is planned, what it will cost,
and what is waiting for a decision.

## 2. Source components

| Source | Reuse |
|---|---|
| `Decommissioning/Services/FieldDecommissioningService.cs`, `.Analysis` | portfolio and analysis |
| `LifeCycle/Services/Decommissioning/*` | orchestration and processes |
| Tables `DECOMMISSIONING_STATUS`, `ABANDONMENT_STATUS`, `ENVIRONMENTAL_RESTORATION`, `R_DECOMMISSIONING_REFERENCE_CODE` | persistence |
| `DefaultSecuritySeedService` | `DECOMMISSIONING_COORDINATOR` landing |

## 3. Screens

- KPI cards: wells to abandon, facilities to decommission, estimated cost, spent to date, overdue items.
- Scope table: asset, type, current state, planned year, estimated cost.
- Persona inbox panel.
- Source links: well P&A, facility decommissioning, cost estimation, restoration.
- Cost strip: estimate vs spend by year.

## 4. Data and contracts

`OgDecomScope { AssetId, Kind, State, PlannedYear, EstimatedCost, Currency }`.

## 5. States

| State | Behaviour |
|---|---|
| Failed read | failure panel; no zero counts |
| Cost unestimated | "not estimated", excluded from totals with a count |
| Overdue | computed from planned year |
| Asset without state | "unclassified" and excluded |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| View | `decommissioning.read` |
| Open a source page | that page's gate |

## 7. Acceptance Criteria

- AC-1 Counts come from records.
- AC-2 Unestimated assets are excluded from totals and counted.
- AC-3 Persona landing matches RBAC.
- AC-4 Cost figures are `decimal` with currency.
- AC-5 Overdue is computed, not stored.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~DEC-01"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-12-DEC-01/observed-evidence.json). Nothing was created: the workspace
reads `ABANDONMENT_STATUS`, `DECOMMISSIONING_STATUS`, `ENVIRONMENTAL_RESTORATION` and `R_DECOMMISSIONING_REFERENCE_CODE`, all
already owned by `DecommissioningModule` or the LifeCycle schema, and composes WFL-05's task inbox.

**The scope is the latest recorded status.** One row per asset: a well with `SUSPENDED` then `PLANNED` appears once, in
`PLANNED`, ordered by `STATUS_DATE` then `ROW_CREATED_DATE`. The label comes from the owner's own reference set, keyed by table
- the same code `PLANNED` reads *"Plug and abandonment planned"* for a well and *"Planned for decommissioning"* for a facility,
and the gateway test proves both from one seeded reference table.

**An unestimated asset is counted, not zeroed.** Costs are `ESTIMATED_COST`/`ACTUAL_COST` with `COST_CURRENCY` from the
facility's status row; the abandonment table records no cost, so a well is *not estimated*. The first version summed the wells
into a `(no currency)` line of zeros, which read like a cost position - a currency group with no recorded cost now produces no
line at all, and the assets are counted in the workspace instead.

**The honest gap is the timetable.** Neither status table carries a planned date, so *"planned year"* - and therefore
*"overdue"* - has no recorded source. The rule is implemented and unit-tested (the same record is overdue in 2026 and not in
2027), but the workspace reports *"no planned year is recorded"* rather than inventing a schedule. For the same reason the KPI
*"wells to abandon"* is reported as a count by recorded state: the reference set states no terminal rule, so no asset is
labelled abandoned or not.

**Restoration is separate.** `ENVIRONMENTAL_RESTORATION` is counted and listed apart from the asset states, so a facility's
decommissioning state says nothing about whether its site is restored (SG-12-4). Note that the type exists twice - a LifeCycle
model with `SITE_ID` and a PPDM39 model without it - and the workspace reads the LifeCycle one explicitly.

**AC-3 completed a persona.** `DecommissioningCoordinator` is added to the role names, lands on the RBAC seed's
`/lifecycle/decommissioning/dashboard` mapped to the `decommissioning` page, and the smoke report records it
(`landing-map-count` 9 -> 10). Each source link is decided by the shell's own page gate; the well P&A, facility and cost pages
do not exist yet and their reasons name DEC-02 and DEC-03.

**Not built, and stated as gaps:** a planned date and a terminal-state rule (DEC-02's programme should record the first);
commands of any kind (this stage's are DEC-02's and DEC-03's); a well's cost (the abandonment table records none); a field
filter for restoration (it carries a site, not a field); paging; and escalation, contingency and cost-revision history, which
DEC-03 owns.

## 9. Open questions and risks

- `FieldDecommissioningService.Analysis` may return a prebuilt analysis object; inspect before
  designing the KPI cards.
