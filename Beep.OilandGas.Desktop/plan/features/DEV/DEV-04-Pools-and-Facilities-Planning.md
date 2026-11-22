# DEV-04 — Pools and facilities planning

| Field | Value |
|---|---|
| Stage | STG-04 — Development and drilling |
| Area | Development |
| Status | Gate met - see [evidence](../../evidence/STG-04-DEV-04/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.DevelopmentPlanning`, `Beep.OilandGas.ProductionOperations` |
| Desktop project | `Modules/Development/PoolsFacilities/` |
| External packages | none |
| Depends on | DEV-02 |
| Blocks | DEV-08, PRD-02 |
| Web reference | `Pages/PPDM39/Development/Pools.razor`, `Facilities.razor`; `Pages/PPDM39/Production/Pools.razor` |

## 1. Purpose

Decide how the field is drained — pools, well counts, facility capacity — and record the facility
investment that the plan depends on.

## 2. Source components

| Source | Reuse |
|---|---|
| `DevelopmentPlanning` `FACILITY_INVESTMENT`, `DEVELOPMENT_WELL_SCHEDULE` | investment and schedule |
| `ProductionOperations/Services/IFacilityManagementService.cs` | facility identity and capacity |
| `PPDM.Models/39/POOL`, `RESERVOIR`, `FACILITY` | entities |

## 3. Screens

`Development → Pools and facilities`

- Pool grid: pool, reservoir, drive mechanism, OOIP, recovery factor, well count planned.
- Facility grid: facility, type, capacity (+unit), throughput assumption, investment, in-service date.
- Capacity check panel: planned rates vs capacity with a shortfall/headroom figure.
- Allocation preview: how the pool volumes will map onto facilities.

## 4. Data and contracts

`OgPoolPlan { PoolId, Name, Reservoir, Ooip, OoipUnit, RecoveryFactor, PlannedWells }`
`OgFacilityPlan { FacilityId, Name, Kind, Capacity, CapacityUnit, Investment, Currency,
InServiceDate }`.

## 5. States

| State | Behaviour |
|---|---|
| Capacity exceeded | explicit shortfall, not a clamp |
| Missing capacity unit | refused |
| Facility not yet existing | marked "planned" and excluded from operational pages |
| Failed read | failed state |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Edit pool plan | `development.pools.write` | yes |
| Edit facility plan | `development.facilities.write` | yes |

## 7. Acceptance criteria

- AC-1 Capacity and rate comparisons refuse mixed units.
- AC-2 A shortfall is shown as a number, never silently capped.
- AC-3 A planned facility is distinguishable from an operating one.
- AC-4 Pool and facility records link to the plan version they belong to.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~PoolsFacilities"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~PoolsFacilitiesGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-04-DEV-04/observed-evidence.json). Pools are read from the
DevelopmentPlanning-owned `DEVELOPMENT_POOL_PLAN`, facilities from the reused `FACILITY_INVESTMENT`, and the states from
the PPDM `FACILITY` row, all through `UnitofWork<T>`; the feature contains no statement text.

Decision on the open question in section 9: the development pool plan is **not** a second copy of the production pools.
`DEVELOPMENT_POOL_PLAN` carries only the plan's numbers and facility routing and refers to the PPDM `POOL` by
`POOL_ID`; PRD-02, when built, reads the same PPDM pool records and this planning row alongside them, so the two pages
are two views of one pool rather than two records.


## 9. Open questions and risks

- Web has both development and production pool pages; decide whether production pools (PRD-02) are
  the same records viewed differently. Record the decision here.
