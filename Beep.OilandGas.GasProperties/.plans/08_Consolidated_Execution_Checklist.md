# Gas properties — execution checklist

## Architecture

- [x] **No `ModuleSetupBase`** — gas properties is a **shared calculation + service** library; reference **`R_*`** catalogs belong to consuming features (**`GasLift`**, **`FlashCalculations`**, …), not this project.
- [x] **Shared models** — **`Beep.OilandGas.Models.Data.GasProperties`** + **`IGasPropertiesService`** in **`Beep.OilandGas.Models.Core.Interfaces`**.
- [ ] **`PPDM39.DataManagement`** — reference only for **`GasPropertiesService`** infrastructure; do not add domain **`ModuleSetup`** here.

## Build

```bash
dotnet build Beep.OilandGas.GasProperties/Beep.OilandGas.GasProperties.csproj
```

## Tests

After **`Beep.OilandGas.GasProperties.Tests`** exists (phase 2):

```bash
dotnet test Beep.OilandGas.GasProperties.Tests/Beep.OilandGas.GasProperties.Tests.csproj
```

## Downstream smoke

Consumers (examples): **`Beep.OilandGas.GasLift`**, **`Beep.OilandGas.NodalAnalysis`**, **`Beep.OilandGas.PipelineAnalysis`**. After API changes, run their **`dotnet build`** slices as needed.

## Industry scenario review (phase 4)

- [ ] **`04_Industry_Scenarios_Wells_Facilities_Reservoirs.md`** reviewed with discipline leads (facilities vs reservoir vs wells).
- [ ] **`README`** scope / out-of-scope matches phase **4** (single-phase vs EOS / two-phase).
