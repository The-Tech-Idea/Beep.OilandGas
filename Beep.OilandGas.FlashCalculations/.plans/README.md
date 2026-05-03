# Beep.OilandGas.FlashCalculations — phased plans

Phased enhancement for **phase equilibrium**, **isothermal / multi-stage flash**, **EOS** usage, **validation**, **`IFlashCalculationService`**, and **PPDM** persistence patterns.

## Non-negotiable solution standards

- **Three layers**; shared contract **`Beep.OilandGas.Models.Core.Interfaces.IFlashCalculationService`**; wire types in **`Beep.OilandGas.Models.Data.Calculations`** / **`Models.Data.FlashCalculations`**.
- **Table vs projection**: persist only **scalar** **`ModelEntityBase`** tables; **`FlashResult`**, **`FlashCalculationRequest`** remain projections for API/orchestration.
- **Extension schema**: **`FlashCalculationsModule`** + **`R_FLASH_CALCULATION_REFERENCE_CODE`** + entity-driven tooling — **not** hand-written **`Models/Scripts`** DDL per [CLAUDE.md](../../CLAUDE.md).
- **DI**: **`Beep.OilandGas.ApiService/Program.cs`** factory registration; **`AddBeepServices`** before **`IDMEEditor`** consumers.
- **`PPDM39.DataManagement`** does **not** reference feature projects for domain logic; **FlashCalculations** hosts **`ModuleSetupBase`**.

## Phase documents

| Phase | File | Focus |
|-------|------|--------|
| 0 | [00_FlashCalculations_Overview_And_Baseline.md](00_FlashCalculations_Overview_And_Baseline.md) | Scope, files, integration |
| 1 | [01_Phase_Contracts_And_Persistence.md](01_Phase_Contracts_And_Persistence.md) | **`IFlashCalculationService`**, save/history, IDs |
| 2 | [02_Phase_Numerics_Validation_And_Units.md](02_Phase_Numerics_Validation_And_Units.md) | EOS, tolerance, **`FlashConstants`**, tests |
| 3 | [03_Phase_ModuleSetup_And_Reference_LOV.md](03_Phase_ModuleSetup_And_Reference_LOV.md) | **`FlashCalculationsModule`**, seeds |
| 4 | [04_Phase_API_And_Orchestration.md](04_Phase_API_And_Orchestration.md) | LifeCycle, **`PerformFlashCalculationAsync`**, OCE |
| 5 | [05_Phase_Tests_And_Verification.md](05_Phase_Tests_And_Verification.md) | Unit tests, golden vectors |
| 6 | [06_Phase_Docs_And_Packaging.md](06_Phase_Docs_And_Packaging.md) | README accuracy, **PackageReadmeFile** |
| — | [07_PVT_Best_Practices_And_Reference.md](07_PVT_Best_Practices_And_Reference.md) | Industry PVT / flash notes |
| — | [08_Consolidated_Execution_Checklist.md](08_Consolidated_Execution_Checklist.md) | Merge checklist |

**Master tracker:** [../MASTER-TODO-TRACKER.md](../MASTER-TODO-TRACKER.md)

## Verification

```bash
dotnet build Beep.OilandGas.FlashCalculations/Beep.OilandGas.FlashCalculations.csproj
dotnet test Beep.OilandGas.FlashCalculations.Tests/Beep.OilandGas.FlashCalculations.Tests.csproj
```
