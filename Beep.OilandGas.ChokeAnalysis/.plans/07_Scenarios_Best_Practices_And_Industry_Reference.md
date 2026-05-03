# Choke analysis — scenarios, parameters, and industry best practices

This document expands module planning with **field scenarios**, **choke types**, **fluid / reservoir contexts**, and **model-selection guidance** aligned with common petroleum engineering references. It informs requirements for `Beep.OilandGas.ChokeAnalysis` (what to implement, validate, and document). **It is not a substitute for facility-specific engineering judgment or regulatory requirements.**

**External references (verify independently):**

- [Petroleum Office — Choke models overview](https://www.petroleumoffice.com/doc/sf-choke-models) — critical/subcritical, Gilbert-type families, Sachdeva/Ashford-Pierce, single-phase gas/liquid forms.
- [Petroleum Office — Surface facilities / choke workflow](https://www.petroleumoffice.com/doc/sf-overview) — choke purposes, model-selection flowchart, tie-in to pipeline/separator balance.
- [PetroSkills TOTM — Gas lift, choke flow, Thornhill–Craver](https://www.petroskills.com/en/blog/entry/feb-2022-totm) — critical pressure ratio not a single universal constant; subcritical vs critical behavior.
- SPE **PetroWiki** — “Flow through chokes” (curated reference; search OnePetro/PetroWiki for current URL).
- Literature on **Sachdeva** multiphase choke tuning (e.g. field-data discharge-coefficient studies — use as **calibration** guidance, not universal constants).

---

## 1. Why chokes are used (operational scenarios)

| Scenario | Engineering intent | Choke / analysis implication |
|----------|---------------------|--------------------------------|
| **Rate control / allocation** | Match pipeline, plant, or lease targets; stabilize header pressure | Often designed for **critical flow** so rate is insensitive to downstream swings |
| **Well testing** | Multi-rate tests, deliverability, IPR points | Repeatable **bean sizes** and stabilized pressures; document **P_wh**, **T**, **choke ID**, duration |
| **Drawdown / sand control** | Limit velocity / drawdown to reduce sanding | Couple choke sizing to **reservoir depletion** and **perforation stress** — analysis may need IPR + VFP context |
| **Coning / water or gas breakthrough** | Limit rate to manage **GOR / WC** | Multiphase choke + **water cut** sensitivity; empirical correlations may need calibration |
| **Equipment protection** | Separator, compressor, flare limits | Ensure **downstream pressure** and rate constraints — subcritical regime possible |
| **Gas lift operations** | Coordinate injection and production | Surface choke interacts with **gas lift stability**; subcritical choke regime and **system slugging** may matter — analysis often requires **network / transient** context beyond steady choke equations |

**Best practice:** Treat choke calculations as one node in a chain: **reservoir → wellbore (VFP) → choke → flowline / separator**. Plans for API integration (phase 4) should allow optional **field / well context** when interpreting results.

---

## 2. Critical vs subcritical (sonic vs subsonic)

| Concept | Typical guidance |
|---------|------------------|
| **Pressure ratio** | \( r = P_2 / P_1 \) (downstream / upstream), using **absolute** pressures |
| **Critical flow** | Velocity reaches **sonic** conditions at the throat; **rate depends primarily on upstream pressure and choke geometry**, not on further reduction of downstream pressure |
| **Subcritical flow** | **Both** \( P_1 \) and \( P_2 \) affect rate; correlations must use **two-pressure** models |
| **Critical pressure ratio** | For ideal gas behavior, \( r_c \) depends on **heat capacity ratio \( k \)**. Literature often cites **~0.55** for natural gas when \( k \approx 1.26\text{–}1.32 \); **PetroSkills** notes measured critical ratios for real choke flow roughly **0.48–0.59** — **do not hard-code a single universal constant** without documenting assumptions |

**Implementation rule:** The library should **compute regime** from \( k \), \( P_1 \), \( P_2 \), and document which equation branch applies. When data is missing, **surface assumptions** in code must be visible in logs or validation messages.

---

## 3. Choke types (equipment) vs “model type”

### 3.1 Common production hardware

| Type | Description | Typical \( C_D \) band (illustrative — calibrate) |
|------|-------------|-----------------------------------------------------|
| **Fixed bean / orifice plate** | Discrete bean sizes (often expressed in **64ths of an inch**) | Sharp-edged orifice-like: often **~0.60–0.65** |
| **Adjustable choke (needle / plug)** | Variable opening; “percent open” maps to effective area | Often **~0.75–0.85** when modeled as equivalent orifice |
| **Nozzle-style** | More favorable recovery | **~0.70–0.80** |
| **Venturi-style** | Lower permanent pressure loss | **~0.95–0.99** |

(Source summary consistent with [Petroleum Office choke models](https://www.petroleumoffice.com/doc/sf-choke-models).)

### 3.2 Location

| Location | Notes |
|----------|------|
| **Surface choke** | Standard allocation / testing context; **wellhead pressure and temperature** usually defined |
| **Downhole choke** | Used for drawdown management; requires **bottomhole / tubular context** and correct **pressure reference** — same **isentropic / orifice** principles often apply but **inputs come from different gauges** |

### 3.3 Non-production “chokes” (scope boundary)

**Drilling BOP / choke & kill** equipment follows **API 16C** class specifications (pressure containment, qualification). That is **equipment standards**, not the same as **production choke performance correlations**. Document if future work integrates drilling hydraulics separately.

---

## 4. Fluid categories and model selection

### 4.1 Decision tree (engineering)

Per industry summaries ([Petroleum Office](https://www.petroleumoffice.com/doc/sf-overview)):

1. **Single-phase gas** → isentropic **sonic / subsonic** gas orifice equations (use \( Z \), \( T \), \( k \), \( C_D \)).
2. **Single-phase liquid** → **incompressible orifice** / Bernoulli-based \( \Delta P \) vs rate (validate compressibility if \( \Delta P \) is huge).
3. **Oil + gas (multiphase)** →  
   - **Critical:** empirical **Gilbert-type** family (**Gilbert, Ros, Baxendell, Achong, Pilehvari**, etc.) — **field-tuned** exponents.  
   - **Subcritical:** **Sachdeva** (mechanistic, widely used in software), **Ashford–Pierce** (semi-empirical subcritical), etc.

### 4.2 GLR bands (multiphase empirical correlations)

Gilbert-type correlations are commonly quoted for **critical** multiphase flow with **GLR roughly 300–50,000 scf/STB** and **bean sizes ~8/64–64/64 in.** ([Petroleum Office summary](https://www.petroleumoffice.com/doc/sf-choke-models)). Outside those bands:

- **Very high GLR** → behave closer to **gas**; **low GLR** → **Pilehvari** or other correlations may be preferred per vendor tables.
- **Water-cut / emulsion** effects are **not fully explicit** in classic Gilbert forms — **calibrate** or use **mechanistic** models.

### 4.3 Reservoir / fluid system archetypes

| Archetype | Dominant phases at choke | Typical considerations |
|-----------|-------------------------|-------------------------|
| **Dry gas** | Single-phase gas | \( Z \), \( k \), critical ratio; hydrates / liquids dropout **downstream** of choke (not same as throat physics) |
| **Gas condensate** | Gas-rich multiphase; liquids can condense with \( \Delta P \) | **Retrograde** behavior near dew point; empirical \( C_D \) tuning reported for **Sachdeva** in literature for gas-condensate vs oil wells |
| **Volatile / black oil** | Oil + free gas at wellhead (often below bubble point at surface) | Multiphase correlations; **WC** and **GOR** trend with choke drawdown |
| **High water cut** | Water–oil–gas | Empirical correlations weaker — prefer **mechanistic** + **field calibration** |
| **Heavy oil / foamy oil** | Enhanced slip / non-Newtonian effects | Choke models may be **qualitative** without lab PVT / field tests |

---

## 5. Key parameters (inputs / QC checklist)

Capture and validate these on every analysis (names align with PPDM / Models where possible):

| Parameter | Role |
|-----------|------|
| **P₁, P₂** | Upstream / downstream **absolute** pressures at choke location |
| **T** | Upstream **temperature** (°R or consistent SI) |
| **Z** | Gas **compressibility** at upstream conditions (or equation of state path) |
| **k (γ)** | Heat capacity ratio / isentropic exponent for gas |
| **γ_g** | Gas specific gravity (air = 1) |
| **Choke diameter** or **bean size** | Often **64ths inch** in US field units |
| **C_D** | Discharge coefficient — **geometry and Reynolds** dependent; **tune** with field data when possible |
| **GLR, WC, GOR** | Multiphase empirical correlations |
| **Oil / water SG** | Liquid-phase density for slip / mixture density in mechanistic models |

**Best practice:** When **inferring** \( P_2 \) or \( P_1 \) from sparse gauges, document uncertainty; prefer **measured wellhead pair** for subcritical checks.

---

## 6. Validation and calibration scenarios (test matrix ideas)

Use as **non-regression** and **acceptance** guides for phases 2 and 5:

### 6.1 Single-phase gas

- **Critical:** ratio \( P_2/P_1 < r_c \) — rate **insensitive** to small \( P_2 \) changes.
- **Subcritical:** \( P_2/P_1 > r_c \) — rate **sensitive** to both pressures.
- **Numerical edge:** near \( r \approx r_c \), small areas, very high \( P_1 \).

### 6.2 Multiphase (correlation coverage)

- **High GLR** vs **low GLR** branches if implementing Gilbert family variants.
- **Subcritical** multiphase: Sachdeva / Ashford–Pierce paths vs **critical** empirical path — **must not** mix equations across regimes.

### 6.3 Integration scenarios

- **PPDM path:** map **WELL_TEST_FLOW_MEAS**, **WELL_PRESSURE**, tubular IDs consistently (phase 3).
- **Gas lift:** acknowledge limits of steady choke-only models when **instability** dominates — flag “needs network model” in backlog/docs.

---

## 7. Known limitations (honest product stance)

| Limitation | Mitigation in Beep stack |
|------------|---------------------------|
| Empirical correlations are **region / fluid specific** | Allow **calibrated** \( C_D \) and correlation constants; store **provenance** |
| **Transient** slugging / hunting | Not captured by steady choke equations — separate **simulation** domain |
| **Wax / hydrate / scale** | Operational constraints; not intrinsic choke hydraulics |
| **Erosion / sand** | Separate calculators (project already has erosion helpers — align inputs with **sand rate**, fluid density, velocity) |

---

## 8. Traceability to repository phases

| Topic | Primary phase doc |
|-------|-------------------|
| Interface vs physics coverage | `01_Phase_Service_Contracts.md`, `09_Interface_Surface_Canonical_vs_Extended.md` |
| Equations, regimes, validation | `02_Phase_Calculation_And_Validation_Surface.md` |
| PPDM mapping, WellServices boundary | `03_Phase_PPDM_And_Data_Paths.md` |
| HTTP / orchestration | `04_Phase_API_And_Orchestration_Integration.md` |
| Regression matrix | `05_Phase_Tests_And_Verification.md` |
| Docs / backlog for multiphase expansions | `06_Phase_Packaging_Docs_And_Backlog.md` |
| End-to-end runbook (all phase checks) | `08_Consolidated_Execution_Checklist.md` |

---

## 9. Maintenance

When implementing new correlations:

- [ ] Add **units** and **assumption** blocks to README.
- [ ] Add **tests** per §6.
- [ ] Cite **primary literature** or **vendor reference** in code comments or `.plans` notes.

---

## 10. Units and engineering conventions

Common sources of field error (catch in validation and docs):

| Topic | Best practice |
|-------|----------------|
| **Gauge vs absolute** | Choke correlations use **absolute** pressure at the choke plane. If gauges read **psig**, convert with **+ atmospheric** (field-standard elevation correction if required). |
| **Temperature** | Use **absolute** temperature (**°R** or K) consistently with \(Z\) and \(k\). |
| **Bean size** | US field units often use **64ths of an inch**; confirm mapping to throat **diameter** and **area** (\(A = \pi D^2/4\)). |
| **Gas rate** | Confirm **Mscf/d**, **MMscf/d**, or SI; document conversion factors in README when exposing API fields. |
| **Liquid rate** | STB/d vs m³/d — multiphase empirical correlations are often tuned to **specific unit systems**. |

---

## 11. Governance and enterprise use

- **Design authority:** Steady choke calculators support **engineering workflows**; they do not replace facility **HAZOP**, **MOC**, or **regulatory** filings.
- **Data residency / audit:** When storing choke runs in PPDM, follow existing **audit columns** and **user id** patterns (`InsertAsync`/`UpdateAsync` with user id from API claims).
- **Calibration:** Field-tuned \(C_D\) or correlation constants should be **traceable** (who, when, which well test).

---

## 12. Further reading (classic references — verify editions)

Engineering texts and papers commonly cited for choke performance include **Gilbert (1954)** and successors (**Ros**, **Baxendell**, **Achong**, **Pilehvari**); **Sachdeva et al.** for critical/subcritical multiphase; **Ashford–Pierce** for subcritical two-phase. Use your corporate library / OnePetro for authoritative equations and **unit systems** used by each author.

---

*Last expanded from web-accessible engineering summaries and peer-reviewed abstracts as of 2026; verify equations against your corporate standards and applicable regulations.*
