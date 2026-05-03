# Scenarios, best practices, and reference (CompressorAnalysis)

## Purpose

Support **test design**, **documentation**, and **model selection** for centrifugal and reciprocating compression in oil and gas facilities (gathering, compression stations, reinjection).

## Typical scenarios

| Scenario | Compressor type | Notes |
|----------|-----------------|--------|
| Gathering booster | Centrifugal or reciprocating | Watch **surge** (centrifugal) at low flow; use **`CENTRIFUGAL_COMPRESSOR_PROPERTIES`** map helpers when tuning |
| Pipeline injection | Often centrifugal multi-stage | **`MultistageCompressor`** / design service stage split vs mechanical limits |
| Wellhead compression | Reciprocating common | **Clearance**, **volumetric efficiency**, speed limits drive **`ReciprocatingCompressorCalculator`** inputs |

## Units and conventions (must match code contracts)

Document the authoritative units in **`README.md`** after phase 2 audit. Baseline expectations:

- Pressures **psia** unless explicitly converted in calculator
- Thermodynamic temperature often **°R** for gas power equations — confirm per method XML on **`CentrifugalCompressorCalculator`/`ReciprocatingCompressorCalculator`**
- Gas flow **Mscf/day** where **`CompressorConstants`** conversions apply

## Best practices

- Keep **polytropic efficiency** source explicit (vendor curve vs default constant).
- For **centrifugal**, validate operating point vs **surge** / **stone wall** when using **`GeneratePerformanceMap`** / **`CheckOperatingRegion`**.
- For **reciprocating**, validate **clearance** and **volumetric efficiency** against OEM limits.

## External references (industry)

- GPSA Engineering Data Book — compression / horsepower chapters (edition per company standard)
- Vendor maps — Ariel / FMC / Dresser-Rand performance curves for reciprocating and centrifugal (use as validation benchmarks for phase 5 golden vectors)

## Traceability to phased work

| Topic | Phase |
|-------|--------|
| Units documented | 2 |
| Surge / map tests | 5 |
| Facility **`EQUIPMENT`** mapping | 3 |
