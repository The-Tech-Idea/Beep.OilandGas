# Phase 1 — Module setup and reference LOV

## Goal

**`GasLiftModule`** registers **`R_GAS_LIFT_REFERENCE_CODE`** for DDL/metadata collection and idempotent **`SeedAsync`** (same pattern as **`FlashCalculationsModule`**).

## Reference sets

| `REFERENCE_SET` | Use |
|-----------------|-----|
| **`GAS_LIFT_PORT_SIZE_IN`** | Port diameters — codes match **`GasLiftConstants.StandardPortSizes`** (inches, invariant string) |
| **`GAS_LIFT_OPERATING_MODE`** | CONTINUOUS, INTERMITTENT, PLUNGER_COMPATIBLE |
| **`GAS_LIFT_DESIGN_METHOD`** | Spacing / staging policy labels |
| **`GAS_LIFT_VALVE_SERVICE`** | UNLOADING, INTERMEDIATE, OPERATING |
| **`GAS_LIFT_INJECTION_SOURCE`** | Facility / compressor / recycle labels |
| **`GAS_LIFT_DESIGN_LIMIT`** | Picklist keys aligned with **`GasLiftConstants`** guardrails (min/max injection, valves, depth, spacing) |

## TODO checklist

- [x] Entity **`R_GAS_LIFT_REFERENCE_CODE`** under **`Data/Tables/`**.
- [x] **`GasLiftReferenceSets`** + **`GasLiftReferenceCodeSeed`**.
- [x] **`GasLiftModule`** — **`ModuleId` GAS_LIFT**, **`Order` 74**, **`EntityTypes`**, **`SeedAsync`**.
- [x] Optional: map **`GAS_LIFT_DESIGN_LIMIT`** codes to UI validation messages — **`Constants/GasLiftDesignLimitMessages`** (+ **`GasLiftDesignLimitMessagesTests`**).

## Verification

```bash
dotnet build Beep.OilandGas.GasLift/Beep.OilandGas.GasLift.csproj
dotnet test Beep.OilandGas.GasLift.Tests/Beep.OilandGas.GasLift.Tests.csproj
```

Module setup orchestration discovers **`GasLiftModule`** automatically when **`Beep.OilandGas.GasLift.dll`** is deployed with **`Beep.OilandGas.ApiService`**.
