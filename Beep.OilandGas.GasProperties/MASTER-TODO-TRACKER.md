# MASTER-TODO-TRACKER — Beep.OilandGas.GasProperties



Rollup for **gas properties** (Z-factor, viscosity, pseudo-pressure, averages, **`GasPropertiesService`**). Details: **[`.plans/README.md`](.plans/README.md)**.



## Phase rollup



| Phase | Document | Status |

|-------|----------|--------|

| 0 | [.plans/00_GasProperties_Overview_And_Baseline.md](.plans/00_GasProperties_Overview_And_Baseline.md) | Baseline drafted |

| 1 | [.plans/01_Service_Models_Validation_And_PPDM.md](.plans/01_Service_Models_Validation_And_PPDM.md) | Active |

| 2 | [.plans/02_Tests_And_Verification.md](.plans/02_Tests_And_Verification.md) | **Extended** — Lee–Gonzalez–Eakin, **`AveragePropertiesCalculator`**, **`GasPropertiesService`** DI smoke (Moq) |

| 3 | [.plans/03_Documentation_And_Packaging.md](.plans/03_Documentation_And_Packaging.md) | **Extended** — **`IMPLEMENTATION_SUMMARY`** aligned; **`PackageReadmeFile`** + packed **`README.md`** |

| 4 | [.plans/04_Industry_Scenarios_Wells_Facilities_Reservoirs.md](.plans/04_Industry_Scenarios_Wells_Facilities_Reservoirs.md) | **Started** — `GetApplicabilityWarnings`; scenario-tagged tests |

| Run | [.plans/08_Consolidated_Execution_Checklist.md](.plans/08_Consolidated_Execution_Checklist.md) | Active |



## Consolidated next actions



| Priority | Action |

|----------|--------|

| P1 | Phase **1** — correlation naming table; validator vs calculator alignment; service vs pure-math audit |

| P2 | Phase **2** — persistence / **`SaveGasCompositionAsync`** integration tests (test DB or harness); trapezoidal pseudo-pressure; more **04** vectors; **`CalculateRealGasPropertiesAsync`** covered (guards + sweep) |

| P3 | Phase **3** — optional **`PackageReleaseNotes`** / version bump policy |

| P4 | Phase **4** — extend **04** checklist (chart-edge refinements, richer facility/reservoir vectors) |



## Verification



```bash

dotnet build Beep.OilandGas.GasProperties/Beep.OilandGas.GasProperties.csproj

dotnet test Beep.OilandGas.GasProperties.Tests/Beep.OilandGas.GasProperties.Tests.csproj

```


