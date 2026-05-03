# Consolidated execution checklist — PumpPerformance

## Build

```bash
dotnet build c:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/Beep.OilandGas.PumpPerformance/Beep.OilandGas.PumpPerformance.csproj
```

## Tests (after phase 2 project exists)

```bash
dotnet test c:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/Beep.OilandGas.PumpPerformance.Tests/Beep.OilandGas.PumpPerformance.Tests.csproj
```

## Definition of “green” for a PR touching this library

1. **PumpPerformance** project builds with **0 errors**.
2. **PumpPerformance.Tests** (when present) — all pass.
3. README / XML docs updated if **public API** or **units** change.
