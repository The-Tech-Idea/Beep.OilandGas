# Phase 1 — Service contract, models, validation, and PPDM usage

## Goal

Keep **`IGasPropertiesService`** / **`GasPropertiesService`** aligned with **`Models`** DTOs, enforce consistent **units** (psia, °R) and **correlation** naming, and use **`PPDMGenericRepository`** only where persistence is intentional (same patterns as **`CLAUDE.md`** / data-access commands).

**Industry alignment:** correlation defaults (if any), validation bands, and **basis** (separator vs wellhead vs sandface) must be traceable for **well**, **facility**, and **reservoir** use cases — see **[`04_Industry_Scenarios_Wells_Facilities_Reservoirs.md`](04_Industry_Scenarios_Wells_Facilities_Reservoirs.md)**.

## TODO checklist

- [ ] Catalogue every **`IGasPropertiesService`** surface method implemented on **`GasPropertiesService`** (and **`Advanced`**) — note which are pure math vs DB-backed.
- [ ] Document **correlation** parameter strings for **`CalculateZFactor`** (and any switch on correlation name) in **`README.md`** or a single internal doc table.
- [ ] Align **`GasPropertiesValidator`** with calculator preconditions (pressure, temperature, γg, composition sums) — no silent divergence between service and static calculators.
- [ ] Audit **`GasPropertiesService`** for raw repository creation on PPDM core tables — prefer metadata-driven **`PPDMGenericRepository`** + **`AppFilter`**; **do not** introduce **`WellServices`** leakage unless the operation is truly well-facet CRUD (gas properties is normally not well-facet setup).
- [ ] Confirm **`Beep.OilandGas.PPDM39.DataManagement`** reference remains justified (only shared infrastructure — no domain **`ModuleSetup`** here).
- [ ] Map **high-risk streams** (sour, **CO₂**-rich, near-critical / gas-condensate) to validator rules: either **hard reject** out-of-published-range **(Pr, Tr)** or emit structured **warnings** for UI/logs (see phase **4**).
- [ ] Ensure **composition** path vs **γg**-only path is explicit in the service API (no silent downgrade from full analysis to **γg** when **CO₂+H₂S** is non-trivial).

## Verification

```bash
dotnet build Beep.OilandGas.GasProperties/Beep.OilandGas.GasProperties.csproj
```

## Exit criteria

- [ ] Correlation names and units documented and consistent with **`GasPropertiesConstants`**.
- [ ] Validator + service + calculators agree on minimum valid ranges for public entry points.
