# Phase 4 — Industry scenarios: wells, facilities, and reservoirs

## Purpose

Align **`Beep.OilandGas.GasProperties`** behavior, **validation**, **documentation**, and **tests** with common **oil and gas practice**: consistent **basis** (where in the system P, T, and composition are measured), defensible **correlation choice**, and explicit **limits** when single-phase gas correlations are not appropriate (e.g. near **dew point**, **two-phase**, or **non-hydrocarbon**-dominated streams).

This phase is **requirements and test-design** guidance; implementation may span phases **1–3** and **`ENHANCEMENT_PLAN.md`**.

## Best practices (engineering)

| Practice | Implication for this library |
|----------|--------------------------------|
| Fix **pressure–temperature–composition** basis | Document whether inputs are **separator / pipeline / wellhead / sandface**; do not mix bases in one averaging run without documenting transforms. |
| Use **absolute pressure (psia)** and consistent **temperature (°R)** | Already the library contract; enforce in **`GasPropertiesValidator`** and **API** examples. |
| Prefer **composition-based** pseudo-criticals when non‑HC fractions matter | **`ZFactorCalculator.CalculatePseudoCriticalProperties`** path; **γg**-only shortcuts are acceptable only when documented (lean dry gas). |
| Choose **Z correlation** by **fluid type** and **P/T envelope** | See **Correlation selection** below; surface in **`README`** + service correlation table (phase 1). |
| Flag **Standing–Katz / DAK** chart edges | Warn or reject when **(Pr, Tr)** leave published chart; avoid silent extrapolation in reserves-grade work. |
| **Gas condensate / near-dew** systems | Single-phase **Z** correlations are **approximate**; document assumption; compositional / EOS belongs in **`FlashCalculations`** / PVT workflow — link from gas-properties docs. |
| **Sour / acid gas** | Elevated **CO₂ / H₂S** changes **Tc, Pc** and viscosity; require **full composition** (or extended fractions) and validate ranges for chosen correlation. |
| **Injection / storage** | **CO₂**, **N₂**, **fuel gas**, and **recycle** streams need composition or mixture rules — **`GasMixingRuleCalculator`** usage and documented limits. |
| **Traceability** | Log or return **which correlation** and **input summary** (not full PII) for audit in **`GasPropertiesService`** where persistence exists. |

## Correlation selection (high-level guide)

| Fluid / context | Typical approach | Caveat |
|-------------------|------------------|--------|
| Lean **dry gas** transmission | **DAK** on **γg** or light composition; **Brill–Beggs** common in legacy tools | Validate **Tr, Pr** inside chart |
| **Rich gas** / **gas condensate** | Composition-based **Tc, Pc** + **DAK** / **Hall–Yarborough** | Near **dew**: single-phase **Z** may be misleading |
| **Associated gas** (oil well) | Often **separator** gas at known **P, T**; may be **wet** | If knock-out / stabilization not modeled, composition may shift |
| **High CO₂ / H₂S** | Full **composition**; check correlation **published range** | Some charts weak at high **acid** fractions |
| **Low-pressure / surface** metering | **Brill–Beggs** or **DAK** with careful **T** (ambient + Joule–Thomson if needed) | Small **P** errors change **Z** strongly at low **P** |

## Scenario matrix — wells

| Scenario | Typical basis | Gas-properties focus | Test / doc expectation |
|----------|----------------|----------------------|-------------------------|
| **Dry gas producer** | Wellhead or bottomhole **P, T**; sales or separator **γg** / comp | **Z**, **μg**, **m(p)** for IPR / nodal | Golden vectors at mid **Tr, Pr** |
| **Gas condensate producer** | Separator gas or recombined **flash** gas | **Z** for tubing / pipeline segment; flag two-phase | Document **single-phase** assumption; optional link to flash |
| **Oil well with associated gas** | Casing / tubing gas at **P, T**; often high **GOR** context | Same as dry if single-phase gas leg | Validate **water** / **liquid** not double-counted in **composition** |
| **HP / HT gas** | Bottomhole **P, T** | **Hall–Yarborough** candidate | Edge-of-chart tests |
| **Low-perm / tight** | Long **buildup** — **P** changes along test | **Pseudo-pressure** integration path stable | Monotonic **m(p)** smoke tests |
| **Injector (gas)** | Manifold **P, T**; **CO₂** / **N₂** / **hydrocarbon** mix | **Mixing rules** + **Z** | Mixture regression tests |
| **Underbalanced / drilling gas** | Surface return line | Highly variable **comp** | Prefer **composition** over static **γg** if data exists |

## Scenario matrix — facilities

| Scenario | Typical basis | Gas-properties focus | Test / doc expectation |
|----------|----------------|----------------------|-------------------------|
| **Compressor suction / discharge** | Stage **P, T**; **k** value | **Z**, **μg** for power / head | Stage-wise vectors |
| **Pipeline line pack** | Average **P** segment; ground **T** model | **AveragePropertiesCalculator** | Document averaging policy (pressure-weighted vs arithmetic) |
| **Dehydration outlet (“dry”)** | Water removed — **composition** should reflect spec | **Z** sales gas | Document **composition** update vs **γg** only |
| **JT / choke station** | Upstream vs downstream **P, T** | **Z** on both sides if single-phase | Validator must catch **P** order if modeling adiabatic flash elsewhere |
| **Flare / relief** | Conservative **comp** / **Mw** | Usually lower accuracy need | Document “screening only” |
| **LNG / NGL plant feed** | Cryogenic-adjacent — often out of simple chart range | May require **EOS** outside this library | Explicit **out-of-scope** in README |

## Scenario matrix — reservoirs

| Scenario | Typical basis | Gas-properties focus | Test / doc expectation |
|----------|----------------|----------------------|-------------------------|
| **Dry gas in-place / recovery** | Initial **Pi, Ti**; **γg** or **comp** | **Z** for **Bg**, **Eg** material balance helpers | Document correlation used in **GIIP** documentation |
| **Gas condensate in-place** | **Rv**, **CGR**, or compositional PVT | **Z** at initial **single-phase** region only | Cross-link **FlashCalculations** / EOS |
| **Volatile / near-critical** | Near **critical locus** | **Z** highly sensitive | Widen tolerances or restrict product use |
| **Aquifer drive gas cap** | **P** decline path | **Z(P)** along depletion | Pseudo-pressure / average tests over **P** grid |
| **CO₂ / EOR gas** | Injection mixture + **reservoir** binary/triple | **Mixing** + **Z** | Dedicated mixture tests |
| **Geothermal / high-T** | Elevated **T** | Check correlation **T** range | Boundary tests |

## TODO checklist (phase 4)

- [ ] Add **“Applicability & basis”** section to **`README.md`** summarizing tables above (short form).
- [ ] Add **correlation selection** subsection: when **`CalculateZFactor`** string should be **DAK** vs **Hall–Yarborough** vs **Brill–Beggs** (well vs facility vs reservoir).
- [ ] Extend **`GasPropertiesValidator`** (or service wrapper) with **optional warnings** (not necessarily exceptions) for: chart edge **(Pr, Tr)**, **γg**-only with high **CO₂+H₂S** fraction in composition, **near-critical** **Tr** band.
- [ ] Extend phase **2** tests with **tagged** cases: `DryGas`, `AssociatedGas`, `SourLean`, `CO2Rich`, `PipelineSegment`, `SeparatorGas`, `NearDew_Condensate_SinglePhaseAssumed` (names illustrative).
- [ ] Document **integration boundary**: when callers must use **`FlashCalculations`** / EOS instead of this library’s single-phase **Z**.
- [ ] Review **`GasPropertiesService`** public methods for **scenario-specific** defaults (e.g. correlation default by well type) — only if product agrees; otherwise keep **explicit** correlation from caller.

## Verification

- Phase **4** is satisfied when **`README`** + **`.plans`** describe scenario coverage and tests include **at least one vector** from **three** pillars: **well**, **facility**, **reservoir** (can be minimal smoke).

## Exit criteria

- [ ] **`04`** linked from **`.plans/README.md`** and **`MASTER-TODO-TRACKER.md`**.
- [ ] **`README`** states **scope** (single-phase gas **Z**, **μg**, **m(p)**) vs **out of scope** (full compositional EOS, two-phase pipe).
- [ ] Test plan (phase **2**) references scenario **tags** in this doc.
