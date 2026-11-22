# Dependency Graph — build order and critical path

**Document ID:** PLAN-06
**Version:** 1.0
**Created:** 2026-11-05

---

## 1. Stage graph

```
                         ┌──────────────────────────────────────┐
                         │ STG-00  Foundation and host shell    │
                         └───────────────┬──────────────────────┘
                                         │
                         ┌───────────────▼──────────────────────┐
                         │ STG-01  Data platform and PPDM core  │
                         └───────┬──────────────────────┬───────┘
                                 │                      │
              ┌──────────────────▼───────┐   ┌──────────▼─────────────────┐
              │ STG-02 Wells, wellbores, │   │ STG-14 Process engine,     │
              │        stratigraphy      │   │        workflow, tasks     │
              └───┬──────────┬───────────┘   └──────────┬─────────────────┘
                  │          │                          │
   ┌──────────────▼──┐  ┌────▼───────────────┐   ┌──────▼─────────────┐
   │ STG-03          │  │ STG-06 Reservoir,  │   │ STG-11 HSE,        │
   │ Exploration &   │  │ PVT & forecasting  │   │ compliance, permits│
   │ seismic         │  └────┬───────────┬───┘   └──────┬─────────────┘
   │ (＋STG-03a Lease)│      │           │              │
   └───────┬─────────┘  ┌───▼──────┐ ┌──▼──────────┐  ┌▼────────────┐
           │            │ STG-07   │ │ STG-08      │  │ STG-12      │
   ┌───────▼─────────┐  │ Lift     │ │ Well test & │  │ Decomm.     │
   │ STG-04          │  └───┬──────┘ │ pipeline    │  └──────┬──────┘
   │ Development &   │      │        └──────┬──────┘         │
   │ drilling        │      │               │                │
   └───┬─────────┬───┘      │               │                │
       │         │          │               │                │
  ┌────▼─────┐ ┌─▼──────────▼───────────────▼───┐   ┌────────▼──────┐
  │ STG-05   │ │ STG-09 Economics and AFE      │   │ STG-13 Work   │
  │ Prod ops │ └──────────┬────────────────────┘   │ management    │
  │ & alloc  │            │                        └───────────────┘
  └────┬─────┘   ┌────────▼─────────────┐
       │         │ STG-10 Production    │
       └────────▶│ accounting & finance │
                 └────────┬─────────────┘
                          │
  ┌───────────────────────▼─────────────────────────────────────────┐
  │ STG-15 Wellbore visual suite                                    │
  └───────────────────────┬─────────────────────────────────────────┘
                          │
  ┌───────────────────────▼─────────────────────────────────────────┐
  │ STG-16 Well engineering toolboxes                               │
  └───────────────────────┬─────────────────────────────────────────┘
                          │
  ┌───────────────────────▼─────────────────────────────────────────┐
  │ STG-17 Data interop, mapping and 3D                             │
  └───────────────────────┬─────────────────────────────────────────┘
                          │
  ┌───────────────────────▼─────────────────────────────────────────┐
  │ STG-18 Training, reporting and release                          │
  └─────────────────────────────────────────────────────────────────┘
```

STG-15/16/17 depend only on STG-00 and STG-02, so they can run **in parallel** with
STG-03…STG-14 by a second workstream.

---

## 2. Parallel workstreams

Once STG-02 is gate-met, three independent streams are possible:

| Stream | Stages | Needs | Produces |
|---|---|---|---|
| A — Lifecycle | 03, 03a, 04, 05, 09, 10 | STG-02 | The business lifecycle end to end |
| B — Subsurface | 06, 07, 08 | STG-02 | Reservoir, lift, test, pipeline |
| C — Visual & engineering | 15, 16, 17 | STG-00, STG-02 | Wellbore views, toolboxes, interop |
| D — Governance | 11, 12, 13, 14 | STG-01, STG-02 | HSE, decommissioning, work, workflow |

Stream C is the fastest route to a demonstrable product and should start as soon as STG-00 is
closed, using the pipeline data from `STG-01`'s DAT-06 import rather than waiting for STG-02.

---

## 3. Feature dependency graph (within stages)

```
FND-01 ─┬─ FND-02 ─┬─ FND-03 ─┬─ FND-04
        │          │          ├─ FND-05
        │          │          └─ FND-06
        │          └─ FND-07
        └─ (all)

DAT-01 ─▶ DAT-02 ─▶ DAT-03 ─▶ DAT-04 ─┬─ DAT-05
                                      ├─ DAT-06
                                      ├─ DAT-07
                                      └─ DAT-08

WEL-01 ─┬─ WEL-02 ─▶ WEL-03 ─▶ WEL-04
        ├─ WEL-05
        └─ WEL-06

EXP-01 ─▶ EXP-02 ─▶ EXP-03
EXP-04, EXP-05 independent after EXP-01
LSE-01 ─▶ LSE-02 ─▶ LSE-03

DEV-01 ─▶ DEV-02 ─▶ DEV-03 ─▶ DEV-04
DEV-05 ─▶ DEV-06 ─▶ DEV-07
DEV-08 needs DEV-04

PRD-01 ─▶ PRD-02 ─▶ PRD-03
PRD-04 ─▶ PRD-05
PRD-06 needs PRD-01 + WTA-01

RES-01, RES-02 ─▶ RES-03
RES-04, RES-05 independent
RES-06 ─▶ RES-07

LFT-01 needs RES-01 + RES-02
LFT-02 ─▶ LFT-01
LFT-03, LFT-04, LFT-05, LFT-06 independent
LFT-07, LFT-08 need RES-01

WTA-01 ─▶ WTA-02
WTA-03 independent

ECO-01 needs RES-06 + RES-07
ECO-02 needs ACC-05
ECO-03 needs WFL-04

ACC-02 ─▶ ACC-03 ─▶ ACC-04
ACC-05 ─▶ ACC-06
ACC-07 needs ACC-03
ACC-08 needs ACC-02

HSE-01 ─▶ HSE-02 ─▶ HSE-03
HSE-04, HSE-05, HSE-06, HSE-07 independent after HSE-01

DEC-01 ─▶ DEC-02
DEC-01 ─▶ DEC-03

WRK-01 ─▶ WRK-02
WRK-03, WRK-04 independent

WFL-01 ─┬─ WFL-02 ─▶ WFL-03 ─▶ WFL-04
        ├─ WFL-05
        └─ WFL-06

VSB-01 ─▶ VSB-02 ─┬─ VSB-03 ─┬─ VSB-04
                  │          ├─ VSB-05 ─▶ VSB-08
                  │          ├─ VSB-06
                  │          └─ VSB-07
                  └─ VSB-09 needs any one view

ENG-01, ENG-02, ENG-06, ENG-07, ENG-08, ENG-09, ENG-10 depend on VSB-02 only
ENG-03 ─▶ ENG-04 ─▶ ENG-05

INT-01, INT-02, INT-03, INT-04 depend on VSB-02
INT-05 ─▶ INT-06
INT-07 ─▶ INT-08

TRN-01 ─▶ TRN-02 ─▶ TRN-03
RPT-01 ─▶ RPT-02
RPT-03 independent
REL-0x needs everything it gates
```

---

## 4. Build order inside a stage

For any stage, the order is fixed:

1. **Core contracts** — result types, view-model base, any new `OgModuleKeys` entry.
2. **Service adapter** — if the stage needs a service the `Client` façade does not expose, add it
   to `.Data` or `LocalServiceComposer` first.
3. **View models** — one per feature, with unit tests against substituted services.
4. **Views** — XAML only, no logic beyond binding and input forwarding.
5. **Catalog entry** — add the page to `OgModuleCatalog` and its icon to `Assets/Icons`.
6. **Smoke construction** — add the view to the smoke test's `ConstructViews` list.
7. **Evidence** — run the stage's focused tests and record.

Skipping step 2 is the most common cause of a view model that only works offline.

---

## 5. External dependency readiness

| Dependency | Ready? | Gate |
|---|---|---|
| `Beep.OilandGas.Client` `IBeepOilandGasApp` covers all 15 subsystems | Yes | — |
| `AddBeepOilandGasAppLocal` composes every calculation service | **Unverified** | `FND-02` probe; fallback is `LocalServiceComposer` |
| `TheTechIdea.Beep.WPF.Controls` usable from `net10.0-windows` | Yes (ERP proves it) | — |
| `SkiaSharp` unified version | **Unverified** | `ADR-001` |
| `SkiaSharp.Views.WPF` at unified version | **Unverified** | `ADR-002` |
| `TheTechIdea.OilGas.*` packages published and restorable | **Unverified** | `ADR-008` |
| `Beep.OilandGas.ApiService` reachable in Remote mode | **Unverified** | `ADR-004` probe |
| A local SQLite PPDM database for integration tests | Partially — Web has `PPDM39DatabaseWizard` and `DemoDatabaseService` | `DAT-02` |

Every `Unverified` row is a Stage 0 or Stage 15 **spike** with a recorded outcome before the
dependent features start.
