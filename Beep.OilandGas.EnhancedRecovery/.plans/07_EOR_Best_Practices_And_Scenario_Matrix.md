# EOR best practices and scenario matrix (reference)

This document is **guidance for prioritizing enhancement work** — not a promise of implemented fidelity. Align backlog items with phases **01–06**.

## Screening and pilot design (industry norms)

| Practice | Why it matters | Product tie-in |
|----------|----------------|----------------|
| **Technique applicability matrix** (lithology, depth, temperature, oil viscosity, salinity) | Avoids misfit methods | Feed **`CompareEORMethodsAsync`** weighting rules |
| **Minimum pilot duration** + **surveillance plan** (tracers, 4D optional) | De-risk scale-up | **`PlanEnhancedRecoveryAsync`** outputs — extend projections |
| **Incremental recovery definition** (decline vs counterfactual) | Economics integrity | **`EOREconomicAnalysis`** assumptions section |

## Surveillance KPIs (operations)

| KPI | Use | Data source direction |
|-----|-----|------------------------|
| **Voidage replacement ratio (VRR)** | Injectant vs fluid withdrawal balance | **`PDEN_FLOW_MEASUREMENT`** + production allocations |
| **Hall plot / injectivity index** | Formation damage vs fracture growth | Derived metrics — optional future **`EOR_SURVEILLANCE_SNAPSHOT`** |
| **Water cut / oil rate vs cumulative injection** | Sweep efficiency trends | **`AnalyzeWaterfloodPerformanceAsync`** inputs |

## Method-specific notes

### Waterflooding

- **Pattern** geometry affects sweep — communicate **1D/2D screening** limits.
- **Conformance** problems dominate failures — tie **“optimization”** outputs to **sensitivity**, not **single best** without static model.

### Gas injection / miscible

- **MMP** depends on **fluid composition** — document **default** MMP behavior vs **lab-measured** override.
- **WAG**: **relative permeability hysteresis** matters — flag as **future compositional coupling**.

### Chemical EOR

- **Adsorption / retention** drive chemical cost — expose **retention factor** input in screening.

### Thermal

- **Heat losses** to over/under burden — steam scenarios should capture **quality** and **subcool** where steam is modeled.

### CO₂ / CCUS context

- **EOR + storage**: regulatory **monitoring, reporting, verification (MRV)** is out of core **EnhancedRecovery** unless a dedicated phase — note **dependency** on **HSE / permits** modules.

## Scenario coverage checklist (backlog vs shipped)

| Scenario family | Shipped partial API | Advanced method exists | Planned HTTP exposure |
|-----------------|---------------------|--------------------------|------------------------|
| Core CRUD / injection | ✓ | ✓ | ✓ |
| Waterflood performance | — | ✓ | Phase 4 |
| Gas injection screening | — | ✓ | Phase 4 |
| Chemical / thermal | — | ✓ | Phase 4 |
| Method comparison | — | ✓ | Phase 4 |
| Pressure performance | — | ✓ | Phase 4 |
| Well placement “optimization” | — | ✓ | Phase 4 (label as sensitivity) |
