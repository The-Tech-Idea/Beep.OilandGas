# Consolidated execution checklist — WellTestAnalysis

## Build

```bash
dotnet build c:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/Beep.OilandGas.WellTestAnalysis/Beep.OilandGas.WellTestAnalysis.csproj
```

## Tests

```bash
dotnet test c:/Users/f_ald/source/repos/The-Tech-Idea/Beep.OilandGas/Beep.OilandGas.WellTestAnalysis.Tests/Beep.OilandGas.WellTestAnalysis.Tests.csproj
```

## Definition of “green” for a PR touching this library

1. **WellTestAnalysis** builds with **0 errors**.
2. **WellTestAnalysis.Tests** — all pass.
3. README / XML updated if **public API**, **units**, or **test design assumptions** change.
