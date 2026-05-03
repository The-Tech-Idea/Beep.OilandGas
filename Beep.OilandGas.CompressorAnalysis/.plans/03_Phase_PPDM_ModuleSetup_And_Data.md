# Phase 3 — PPDM mapping, ModuleSetup, and data paths

## Goal

Register **compressor extension** entities for setup/migration flows via **`IModuleSetup`**, keep **seed** idempotent, and clarify mapping to **facility/equipment** operational data without duplicating PPDM core tables.

## Target files (new / updated)

- `Beep.OilandGas.CompressorAnalysis/Modules/CompressorAnalysisModule.cs` (new)
- `Beep.OilandGas.CompressorAnalysis/Data/Tables/*` — entity list must match **`EntityTypes`**
- Reference seed: `Beep.OilandGas.CompressorAnalysis/Data/Constants/CompressorAnalysisReferenceCodeSeed.cs` (**`R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`**) — mirror **`ChokeAnalysisReferenceCodeSeed`** pattern

## EntityTypes candidate list (feature `Data/Tables`)

Register **extension** compressor tables only — typically:

- `COMPRESSOR_OPERATING_CONDITIONS`
- `CENTRIFUGAL_COMPRESSOR_PROPERTIES`
- `RECIPROCATING_COMPRESSOR_PROPERTIES`
- `COMPRESSOR_POWER_RESULT`
- `COMPRESSOR_PRESSURE_RESULT`
- `R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`

Do **not** register **`EQUIPMENT`**, **`FACILITY`**, or other standard PPDM core entities here (**CLAUDE.md**).

## TODO checklist

- [x] Add **`CompressorAnalysisModule : ModuleSetupBase`**:
  - **`ModuleId`** e.g. **`COMPRESSOR_ANALYSIS`**, **`Order`** in domain band (e.g. 78 — after choke **77** if unchanged).
  - **`EntityTypes`** — list above (exact **`typeof(...)`** from **`Beep.OilandGas.CompressorAnalysis.Data`**).
- [x] **`SeedAsync`**: **`R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`** idempotent insert via **`CompressorAnalysisReferenceCodeSeed`** (skip when rows exist).
- [ ] Metadata: ensure orchestrator / **`CreateSchemaFromEntitiesAsync`** picks up module types (same discovery pattern as other **`Beep.OilandGas.*`** assemblies).
- [ ] Integration narrative: document how **`FACILITY_ID`** / **`EQUIPMENT`** links flow from PPDM when persisting a **compressor run** (mapping layer in LifeCycle or dedicated service — not raw duplication in this library).

## Schema policy

Per **CLAUDE.md**: **no** new hand-written `Beep.OilandGas.Models/Scripts/**` DDL for these extensions; schema flows from **entities + tooling**.

## Verification criteria

- `dotnet build Beep.OilandGas.CompressorAnalysis`
- `dotnet build Beep.OilandGas.ApiService` (module discovered via **`AddDiscoveredModuleSetups`** — confirm no startup exception)
- Optional: seed smoke via **`PPDM39SetupController`** module run (environment permitting)
