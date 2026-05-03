# Phase 1 — Service contracts and feature data alignment

## Goal

**Done (repository state).** Canonical cross-assembly surface: **`ICompressorAnalysisService`** in the feature assembly; **extension table** types in **`Beep.OilandGas.CompressorAnalysis.Data`**; **wire** request/result types in **`Beep.OilandGas.Models.Data.Calculations`**. ID / persistence uses **`IPPDM39DefaultsRepository.FormatIdForTable`** where applicable.

## Target files (as implemented)

- `Beep.OilandGas.CompressorAnalysis/Core/Interfaces/ICompressorAnalysisService.cs`
- `Beep.OilandGas.CompressorAnalysis/Data/Tables/*.cs` — **`COMPRESSOR_*`**, **`R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`**
- `Beep.OilandGas.CompressorAnalysis/Services/CompressorAnalysisService*.cs`
- `Beep.OilandGas.ApiService/Program.cs` — `AddScoped<Beep.OilandGas.CompressorAnalysis.Core.Interfaces.ICompressorAnalysisService>(...)`
- `Beep.OilandGas.Models/Data/Calculations/` — **`CompressorAnalysisRequest`**, **`CompressorAnalysisResult`**, **`CompressorAnalysisWellKnown`**
- `Beep.OilandGas.LifeCycle/.../PPDMCalculationService.Facilities.cs` — **`ICompressorAnalysisService`** for packaged runs

## TODO checklist

- [x] **`ICompressorAnalysisService`** — **`CalculateCentrifugalPowerAsync`**, **`CalculateReciprocatingPowerAsync`**, **`CalculateRequiredPressureAsync`** on **`CompressorAnalysis.Data`** types.
- [x] **`CompressorAnalysisService`** implements the interface; extended helpers remain on partials.
- [x] **`FormatIdForTable`** via DI **`IPPDM39DefaultsRepository`** where IDs are formatted for tables.
- [x] **Table vs projection**: **`COMPRESSOR_*`** table classes are scalar-only (**CLAUDE.md**); nested graphs belong in projection/wire types only.
- [x] ApiService DI registration **after** **`IDMEEditor`** / Beep services per **Program.cs** order.

## Verification criteria

- `dotnet build Beep.OilandGas.Models`
- `dotnet build Beep.OilandGas.CompressorAnalysis`
- `dotnet build Beep.OilandGas.ApiService`
- No analyzer warnings on public API surface for compressor paths

## Risks

| Risk | Mitigation |
|------|------------|
| Interface growth | Keep **`ICompressorAnalysisService`** minimal; advanced overloads on **`CompressorAnalysisService`** concrete partials |
| Breaking **`CompressorController`** | HTTP boundary stays **`Models.Data.Calculations`** wire types; controller maps to **`COMPRESSOR_*`** for service calls |
