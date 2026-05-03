# Phase 4 — Industry scenarios (wells, ESP, pipeline, rotating equipment)

## Purpose

Map **where PumpPerformance applies** in oil and gas, **what inputs** matter, and **which other Beep.OilandGas modules** own adjacent physics so this library stays focused on **pump curves, hydraulics of the pump element, and screening system matches**.

## Scenario matrix (documentation + future test tags)

| ID | Scenario | Typical fluids / constraints | PumpPerformance focus | Partner / boundary module |
|----|-----------|--------------------------------|-------------------------|----------------------------|
| S1 | **Surface pipeline / booster** | Low GVF, water or stabilized oil | H–Q, BHP, efficiency, system curve | **PipelineAnalysis** (line hydraulics) |
| S2 | **Downhole ESP** | High TDH, stages, motor heat budget | ESP curve generation, stage stacking, viscosity correction | **Models `PumpPerformance`**, **NodalAnalysis** (intake / IPR) |
| S3 | **Gas lock / high free gas at pump** | Intake GVF, stage gas handling | De-rate factors, applicability warnings | **GasProperties**, **FlashCalculations** — do not fake multiphase inside pump curves without doc |
| S4 | **Heavy / viscous oil** | μ(T,P) effect on curve | **ViscosityCorrectionCalculator** usage, validity bands | **OilProperties** for μ screening |
| S5 | **Hydraulic jet / piston lift** | Power fluid vs produced | Not duplicated — screening in **`HydraulicPumps`** | Link scenarios only |
| S6 | **SRP / PCP** | Polished rod, slip, speed | PD pump models; torque/power elsewhere | **SuckerRodPumping** |
| S7 | **NPSH / cavitation risk** | Suction vessel level, subcooling | **NPSHCalculations**; margin reporting | Facility **P&ID** data via PPDM when wired |
| S8 | **VSD / affinity** | Speed sweep, energy management | **AffinityLaws**; verify BEP tracking | Production ops / setpoints |
| S9 | **Parallel / series stations** | Header balance | **MultiPumpConfiguration** | System design |
| S10 | **Sand / fouling / degradation** | Curve shift over time | Optional de-rating factors (product) | Condition monitoring (future) |

## Best-practice guardrails

| Topic | Guidance |
|-------|-----------|
| **Units** | Declare **GPM / ft / hp / SG** (or SI) in XML on public APIs; never mix silently. |
| **Operating point** | Intersection of **pump** and **system** curves — document uniqueness assumptions (single stable intersection). |
| **NPSH** | Distinguish **NPSHa** (system) vs **NPSHr** (vendor curve) — naming in results DTOs. |
| **Downhole vs surface** | TDH includes vertical lift + friction + choke — **nodal** context; this library supplies **pump head** capability vs **rate**. |

## TODO checklist

| # | Task | Artifact |
|---|------|----------|
| 4.1 | Add condensed **scenario table** to **`README.md`**. | Phase 3 |
| 4.2 | Define xUnit **traits** (`[Trait("Scenario","S2")]`) for ESP and NPSH tests. | Phase 2 |
| 4.3 | Optional **`GetApplicabilityWarnings`** style API for high GVF / out-of-range SG (mirror **GasProperties** pattern). | Product decision |

## Exit criteria

- [ ] Phase 4 doc reviewed with discipline lead (optional).
- [ ] At least **S1**, **S2**, and **S7** reflected in automated tests **or** explicitly deferred with rationale in **`MASTER-TODO-TRACKER.md`**.
