# Phase 4 — Industry scenarios and well-test / PTA best practices

## Purpose

Align library **capabilities**, **defaults**, and **documentation** with common **petroleum engineering** practice for **pressure transient** and **well performance** tests—without claiming full commercial simulator parity.

## Reference themes (documentation + test tags)

| Theme | Engineering practice | Library role |
|-------|----------------------|--------------|
| **Test design** | Stable rate before shut-in; gauge resolution vs expected Δp; minimum shut-in duration for radial flow | Document **input quality** expectations on **`WELL_TEST_DATA`** |
| **Flow regimes** | Wellbore storage → radial → boundaries; derivative plateau / dip / upturn | **`DerivativeAnalysis`**, **`IdentifyReservoirModel`** — state **assumptions** and **non-uniqueness** |
| **Horner vs MDH** | Horner after **constant** producing history idealization; MDH alternatives | **`AnalyzeBuildUp`** vs **`AnalyzeBuildUpMDH`** — document when each is used |
| **Oil vs gas** | Oil: often **p** vs **t** with μB; gas: **m(p)**, **pseudo-time** for rigorous PTA | **`GasWellAnalysis`** path; link to **`IGasPropertiesService`** for Z, μg |
| **Multi-phase / condensate** | Near-critical / rich gas — simplified single-phase PTA may mislead | **Applicability warnings** in results metadata or docs |
| **Non-Darcy / high rate** | Skin rate-dependent; turbulence in gas | Flag as **future** or **screening-only** in docs |
| **Horizontal / fractured** | Specialized models; derivative signatures differ from vertical radial | Position as **screening** unless explicit models exist |
| **DST vs surface PBU** | Fluid level, phase separation, wellbore phase redistribution | Operational context in phase **3** README |
| **Units** | Field (psi, hr, md, ft) vs SI — never silent mix | Phase **1** + public XML |
| **Data QC** | Outliers, missing sync between rate and pressure, afterflow | **`WellTestDataValidator`** extensions |
| **Regulatory / allocation** | Jurisdiction-specific; library supplies **technical** outputs only | Disclaimer in README |

## Scenario matrix (S-codes for tests / docs)

| ID | Scenario | Fluids / well | Focus modules | Partner modules |
|----|-----------|-----------------|---------------|-----------------|
| S1 | **Oil PBU** after constant oil rate | Black-oil μ, B from correlations | Horner / MDH, radial K & S | **OilProperties** |
| S2 | **Gas PBU** | m(p), T, γg, CO₂/N₂/H₂S fractions | **`GasWellAnalysis`** | **GasProperties** |
| S3 | **Constant-rate drawdown** | Single-phase screening | **`DrawdownAnalysis`** | **NodalAnalysis** (p̄, IPR context) |
| S4 | **Derivative diagnostic** | Model ID from shape | **`DerivativeAnalysis`** | — |
| S5 | **Noisy gauge** | Smoothing sensitivity | **`WellTestConstants.DefaultDerivativeSmoothing`** | Tune + document |
| S6 | **Type curve** screening | Library / lookup | **`TypeCurveLibrary`** | Product catalog |
| S7 | **Field context** | UWI, audit user | **`WellTestAnalysisService`** | **WellServices**, **FieldOrchestrator** |

## TODO checklist

| # | Task | Artifact |
|---|------|----------|
| 4.1 | Add condensed **scenario table** (S1–S7) to **`README.md`**. | Phase 3 |
| 4.2 | xUnit **traits** `Scenario=S2` for gas smoke tests when added. | Phase 2 |
| 4.3 | Optional **`AnalysisApplicabilityFlags`** on result DTO (product + **Models** change). | Backlog |

## Exit criteria

- [ ] At least **S1**, **S2**, and **S4** reflected in tests **or** explicitly deferred with rationale in **`MASTER-TODO-TRACKER.md`**.
