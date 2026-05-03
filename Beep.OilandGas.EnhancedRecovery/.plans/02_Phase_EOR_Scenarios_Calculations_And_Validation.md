# Phase 2 — EOR scenarios, calculations, and validation

## Goal

Cover **industry-representative EOR scenarios** with **traceable math**, **explicit assumptions**, and **input validation** — replacing implicit placeholders where decisions depend on outputs.

## Scenario catalog (must map to tests + docs)

### Secondary / pressure maintenance

| Scenario | Mechanism | Key surveillance | Notes |
|----------|-----------|-------------------|--------|
| **Pattern waterflood** | Displacement + volumetric sweep | Voidage replacement ratio (VRR), injectivity, water cut trend | Align “incremental RF” with **material balance** awareness — document that OOIP may come from **`POOL`** or external study. |
| **Profile / conformance** | Channeling, thief zones | Injectivity index trends, fractional flow | Advanced partial **`OptimizeInjectionWellPlacementAsync`** — treat as **optimization objectives**, not guaranteed optimum without grid model. |

### Gas-based EOR

| Scenario | Mechanism | Key inputs | Notes |
|----------|-----------|------------|--------|
| **Immiscible gas** | Viscosity reduction, swelling | **`InjectionPressure`**, **`MinimumMiscibilityPressure`** only scratch miscibility — extend with **fluid characterization** when available. |
| **Miscible (HC / CO₂)** | Low IFT displacement | MMP / MME screening | Current **`AnalyzeGasInjectionAsync`** miscibility flag — document **limitation**: Black-oil style screening unless compositional properties supplied. |
| **WAG** | Alternate slugs | Cycle timing, hysteresis | Future: explicit **`WAGCycle`** projection or operational PDEN children — backlog if not in PPDM core. |

### Chemical EOR

| Scenario | Mechanism | Focus | Notes |
|----------|-----------|-------|--------|
| **Polymer** | Viscosity sweep improvement | Retention, shear degradation | **`AnalyzeChemicalEORAsync`** — add ** retention / cost sensitivity** parameters when economics ties in. |
| **ASP / SP** | IFT / wettability | Phase behavior, adsorption | Document lab-scale vs field-scale scaling limits. |

### Thermal

| Scenario | Mechanism | Focus | Notes |
|----------|-----------|-------|--------|
| **Steam drive / stimulation** | Heat loss, viscosity drop | **`AnalyzeThermalRecoveryAsync`** — steam quality, losses | Heavy-oil relevance; light-oil fields may mark **N/A**. |

### Screening / ranking

| Scenario | Output | Notes |
|----------|--------|--------|
| **`CompareEORMethodsAsync`** | Ranked methods | Tie to **economics** (**`EOREconomicAnalysis`**) and **risk** flags (technical readiness). |

### Pressure / reservoir performance

| Scenario | Output | Notes |
|----------|--------|--------|
| **`AnalyzePressurePerformanceAsync`** | Pressure maintenance narrative | Pair with **production history** ingestion where available (LifeCycle / **`PDEN_VOL_SUMMARY`** paths — optional integration). |

## Target files

- **`Services/EnhancedRecoveryService.Advanced.cs`** — factor pure **calculator** helpers into **`Calculations/`** partial classes or static libraries for unit testing (keep **`EnhancedRecoveryService`** orchestration thin).
- **`Validation/`** — introduce **`EnhancedRecoveryValidator`** (ranges for pressures, rates, OOIP, discount rate, project life).
- **`Models/Data/Calculations/*`** — ensure result types carry **Assumptions** / **Warnings** collections where outputs are **screening-level**.

## TODO checklist

- [ ] Extract **pure functions** for NPV/IRR/payback from **`AnalyzeEOReconomicsAsync`** into testable helpers (avoid duplicated economic logic elsewhere).
- [ ] Replace **magic numbers** (e.g. fixed incremental RF bands) with **parameterized** screening defaults + **documented** correlation names (internal engineering references — cite in **`README`** only).
- [ ] Add **validation layer**: non-positive OOIP, mismatched history vector lengths, pressures below atmospheric where inappropriate.
- [ ] Standardize **async** patterns — remove redundant **`async`** without **`await`** where applicable (micro cleanup).
- [ ] Unit labels: **psia**, **°F** vs **°R**, **STB/d**, **Mscf/d** — single **`README`** table (phase 6).

## Verification criteria

- `dotnet build Beep.OilandGas.EnhancedRecovery`
- New **`Beep.OilandGas.EnhancedRecovery.Tests`** (phase 5): golden vectors for economics + at least one screening path per family (water / gas / chemical / thermal).

## Risks

| Risk | Mitigation |
|------|------------|
| Over-claiming reservoir simulation fidelity | Label outputs **screening** / **Level 1** in API docs and UI |
| CO₂ / regulatory reporting | Keep **storage** and **fiscal** reporting out of scope unless explicit phase |
