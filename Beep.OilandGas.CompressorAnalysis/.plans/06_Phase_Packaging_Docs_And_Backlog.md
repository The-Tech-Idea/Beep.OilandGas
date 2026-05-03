# Phase 6 — Packaging, documentation, and backlog

## Goal

Keep **NuGet** packaging, **README**, and **summaries** aligned with code; track **SkiaSharp** and deep integration items without blocking core releases.

## Target files

- `Beep.OilandGas.CompressorAnalysis/Beep.OilandGas.CompressorAnalysis.csproj` — **`PackageReadmeFile`**, icon, version
- `README.md` — usage, units, examples calling **`CompressorController`** contracts or **`CompressorAnalysisService`**
- `IMPLEMENTATION_SUMMARY.md`
- `MASTER-TODO-TRACKER.md`

## TODO checklist

- [ ] **README**: Quickstart — minimal centrifugal + reciprocating **`CalculatePower`** example using **`Models`** entities; link **`.plans/07`** for units.
- [ ] **Versioning**: bump **`PackageVersion`** / assembly informational version when publishing (follow repo convention).
- [ ] **SkiaSharp**: **`IMPLEMENTATION_SUMMARY`** backlog — performance maps / curves remain optional; consider **`SkiaSharp`** only in Web or a separate visualization adapter to avoid bloating core NuGet consumers.
- [ ] **PipelineAnalysis** integration note — cross-link when compressor stations tie to network models.

## Verification criteria

- `dotnet pack Beep.OilandGas.CompressorAnalysis/Beep.OilandGas.CompressorAnalysis.csproj` (optional local)
- README renders in GitHub and NuGet gallery (no broken relative links for published package — use absolute repo URLs if needed)

## Backlog (prioritized)

| Item | Notes |
|------|--------|
| SkiaSharp compressor maps | UI / reporting layer |
| Economics — fuel gas cost | ProductionAccounting / Economics modules |
| Equipment catalog sync | Map **`COMPRESSOR_*`** runs to **EQUIPMENT** instances |
