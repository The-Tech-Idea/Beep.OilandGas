# Phase 3 — ModuleSetup and optional extension tables

## Goal

**Delivered (baseline):** **`EnhancedRecoveryModule`** registers **`R_ENHANCED_RECOVERY_REFERENCE_CODE`** + **`SeedAsync`** for EOR method / screening LOVs. Continue here **only** when adding **more** extension entities beyond this reference list.

## When to stay on core PPDM only

- Operational **injection / EOR scheme** tracking uses **`PDEN`** attributes already modeled.
- **Rates** history fits **`PDEN_FLOW_MEASUREMENT`** product types and amendment sequences.
- Reporting uses existing **`FIELD`**, **`POOL`**, **`PDEN_VOL_SUMMARY`** joins.

## When to add extension schema

Add **`EnhancedRecoveryModule : ModuleSetupBase`** **only if**:

| Need | Example extension | Registration |
|------|-------------------|--------------|
| **Pilot lab / design parameters** not on **`PDEN`** | **`EOR_PILOT_DESIGN`** (scalar table class) | **`EntityTypes`** + **`PPDMGenericRepository`** |
| **Method-specific surveillance KPIs** | **`EOR_SURVEILLANCE_SNAPSHOT`** | Same |
| **Controlled EOR method LOV** | **`R_EOR_METHOD`** or reuse existing **`R_*`** if PPDM provides | **`SeedAsync`** idempotent |

## Target files (if phase proceeds)

- **`Modules/EnhancedRecoveryModule.cs`** (new)
- **`Data/Tables/*.cs`** — **only** **`ModelEntityBase`** scalar shapes
- **`Data/Constants/*Seed*.cs`** — reference rows mirroring **Choke/Compressor** seed patterns
- **`Beep.OilandGas.ApiService/Program.cs`** — module discovery already via **`AddDiscoveredModuleSetups`** — verify assembly loads

## TODO checklist

- [x] **`README.md`** — module + **`R_ENHANCED_RECOVERY_REFERENCE_CODE`** + seed catalog documented.
- [x] **`EnhancedRecoveryModule`** — **`EntityTypes`**, **`Order` 79**, **`SeedAsync`** (reference codes).
- [ ] **Optional**: add **`EOR_PILOT_DESIGN`**-style extension tables if pilot parameters are persisted — extend **`EntityTypes`** + migration pipeline.
- [ ] Metadata: ensure **`IPPDMMetadataRepository`** sees new tables after migration workflow (solution-specific pipeline).

## Verification criteria

- `dotnet build Beep.OilandGas.EnhancedRecovery`
- Database creation / migration smoke per **`CLAUDE.md`** verification steps for modules

## Risks

| Risk | Mitigation |
|------|------------|
| Duplicating PPDM-owned tables | Cross-check **PPDM 3.9** model before creating **`EOR_*`** |
