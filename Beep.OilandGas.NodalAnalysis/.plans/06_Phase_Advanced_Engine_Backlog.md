# Phase 6: Advanced Engine Backlog

## Goal

Plan multi-release enhancements for nodal analysis depth, deterministic vectors, and optimization quality while keeping current API surface stable.

## Delivered Baseline To Preserve

- Existing IPR/VLP generation and operating-point detection.
- PPDM save/history flow for nodal results.
- Advanced service endpoints for diagnostics, lift recommendation, pressure strategy, and production forecast support.

## Extended Backlog (Multi-Release)

- [x] Deterministic nodal regression vectors (known IPR/VLP intersections, no-intersection scenarios, interpolation precision).
- [x] Numerical hardening for sparse/flat curves, noisy points, and extreme pressure boundaries.
- [x] Optimization engine uplift (replace placeholder recommendation strings with scored candidate ranking).
- [x] Artificial lift recommendation calibration with configurable policy constants and explainable scoring.
- [x] Economic sensitivity expansion with scenario bundles and reproducible parameter sweeps.
- [x] PPDM-backed run metadata and optional curve snapshot persistence (if storage model is approved).
- [x] API-facing diagnostics contracts aligned with canonical DTO versioning guidance.

## Newly Completed In This Pass

- `OptimizeSystemAsync` now builds a deterministic scored candidate list and selects the top-ranked operating point.
- Recommendations are emitted in ranked score order with explicit `(score, Q, Pwf)` values to support reproducible review.
- Added/updated nodal tests to lock deterministic ranking and service behavior.
- Hardened operating-point calculation for sparse/noisy/extreme inputs with curve sanitization, adaptive sampling, and crossing interpolation.
- Calibrated artificial lift scoring with policy constants and explainable score breakdown/candidate ranking in recommendation payloads.
- Expanded economic sensitivity output with deterministic scenario bundles (Pessimistic/Base/Optimistic) and reproducible sweep definitions.
- Added run metadata persistence (`NODAL_ANALYSIS_RUN_METADATA`) and optional IPR/VLP snapshot persistence controlled by `PersistCurveSnapshots`.
- Added explicit diagnostics API contract versioning (`NODAL_DIAGNOSTICS_V1`) across diagnostics DTOs and service outputs.

## Verification

- `dotnet build Beep.OilandGas.sln`
- `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~Nodal"`
