# MASTER-TODO-TRACKER — Beep.OilandGas.PumpPerformance

Rollup for **pump curves**, **power / efficiency**, **NPSH**, **affinity**, **system curves**, **ESP design**, **pump types**, and **rendering**. Phased detail: **[`.plans/README.md`](.plans/README.md)**.

## Phase rollup

| Phase | Document | Status |
|-------|----------|--------|
| 0 | [.plans/00_PumpPerformance_Overview_And_Baseline.md](.plans/00_PumpPerformance_Overview_And_Baseline.md) | **Drafted** |
| 1 | [.plans/01_Calculators_Services_Validation_And_Contracts.md](.plans/01_Calculators_Services_Validation_And_Contracts.md) | **In progress** — plan **1.3** done (3960/1714 → constants in hot paths); service + interface docs |
| 2 | [.plans/02_Tests_And_Verification.md](.plans/02_Tests_And_Verification.md) | **Started** — `Beep.OilandGas.PumpPerformance.Tests` (**21** tests) |
| 3 | [.plans/03_Documentation_Packaging_API_Alignment.md](.plans/03_Documentation_Packaging_API_Alignment.md) | **In progress** — README + phase/ESP/render docs namespace pass |
| 4 | [.plans/04_Industry_Scenarios_Wells_ESP_Pipeline_Rotating.md](.plans/04_Industry_Scenarios_Wells_ESP_Pipeline_Rotating.md) | **Drafted** |
| Run | [.plans/08_Consolidated_Execution_Checklist.md](.plans/08_Consolidated_Execution_Checklist.md) | **Active** |

## Consolidated next actions

| Priority | Action |
|----------|--------|
| **P0** | Fix **README / doc namespace** — **done** (README + package readme path) |
| **P1** | Phase **1** — **partial**: strict validators, NPSH margin, **`HorsepowerFromGpmPsiFactor`** + code paths off magic **3960/1714** (see plan **1.3**); **GasProperties** kept (**`ESPDesignCalculator`**) |
| **P2** | **`Beep.OilandGas.PumpPerformance.Tests`** on solution — **21** tests (run **`dotnet test`**) |
| **P3** | Reconcile **`ENHANCEMENT_PLAN`** / phase ***.md** legacy titles (optional bulk replace) |
| **P4** | Scenario-tagged tests + applicability warnings (phase **4**) |

## Verification

```bash
dotnet build Beep.OilandGas.PumpPerformance/Beep.OilandGas.PumpPerformance.csproj
dotnet test Beep.OilandGas.PumpPerformance.Tests/Beep.OilandGas.PumpPerformance.Tests.csproj
```

