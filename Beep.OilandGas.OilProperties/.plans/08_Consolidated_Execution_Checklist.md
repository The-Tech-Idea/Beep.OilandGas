# Consolidated execution checklist — OilProperties

## Build

```bash
dotnet build c:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/Beep.OilandGas.OilProperties/Beep.OilandGas.OilProperties.csproj
```

## Tests (after phase 2 project exists)

```bash
dotnet test c:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/Beep.OilandGas.OilProperties.Tests/Beep.OilandGas.OilProperties.Tests.csproj
```

## API smoke (optional)

```bash
dotnet build c:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj
```

Ensure **`IOilPropertiesService`** registration in **`Program.cs`** still resolves after refactors.

## Definition of “green” for a PR touching this library

1. **`Beep.OilandGas.OilProperties`** builds with **0 errors**.
2. **OilProperties.Tests** (when present) — **all pass**.
3. **Unit basis** documented in **README** or **interface XML** if any correlation input contract changes.
