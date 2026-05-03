# Phase 6 — Documentation, packaging, and governance

## Goal

Make **EnhancedRecovery** **consumable** as a package and **defensible** for reservoir / facilities stakeholders: **units**, **assumptions**, **limitations**, and **versioning**.

## Target files

- **`Beep.OilandGas.EnhancedRecovery/README.md`** — **delivered**: capabilities, PPDM mapping, interfaces, HTTP + Web routes, units, limitations, DI, build (see repo root file).
- **`Beep.OilandGas.EnhancedRecovery/Beep.OilandGas.EnhancedRecovery.csproj`** — **`PackageReadmeFile`** + **`README.md`** packed (**done**); extend **description** / version when publishing.
- **`IMPLEMENTATION_SUMMARY.md`** (optional, module-local)

## Documentation checklist

- [x] **Units table**: covered in **`README.md`** (baseline; expand if new endpoints add units).
- [x] **Screening vs simulation**: **`README.md`** — limitations section.
- [x] **Data dependencies**: **`README.md`** — PPDM mapping + WellServices note.
- [x] **Security**: **`README.md`** — HTTP section notes **`[Authorize]`**; Web clients require authenticated **`ApiClient`** in hosted apps.

## Governance

- [ ] No **DTO** namespaces — **`Models.Data`** / **`Models.Data.Calculations`** only.
- [ ] Economics assumptions synchronized with **`EconomicAnalysis`** module if shared **pricing / fiscal** rules emerge.

## Verification criteria

- `dotnet pack Beep.OilandGas.EnhancedRecovery/Beep.OilandGas.EnhancedRecovery.csproj` (optional CI)
- Peer review: **petroleum engineer** sign-off on wording for **screening** claims

## Risks

| Risk | Mitigation |
|------|------------|
| Liability from misinterpreted screening results | **Disclaimer** block in README + optional **`Warnings`** on result DTOs |
