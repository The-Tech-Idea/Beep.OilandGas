# Beep.OilandGas Compressor Analysis - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned compressor analysis platform for evaluating compressor performance, efficiency, and operational constraints.

**Key Principle**: **PPDM-style table entities** (`COMPRESSOR_*`, **`R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`**) live in **`Beep.OilandGas.CompressorAnalysis.Data`**; **cross-layer wire types** (`CompressorAnalysisRequest` / **`CompressorAnalysisResult`**) stay in **`Beep.OilandGas.Models.Data.Calculations`**. Services orchestrate runs and persist with full metadata.

**Scope**: Compressor performance and optimization for production facilities.

---

## Architecture Principles

### 1) Reproducible Analysis
- Preserve test inputs, operating conditions, and correlation choices.
- Results are versioned and auditable.

### 2) Operational Integrity
- Link compressor performance to facility throughput constraints.

### 3) PPDM39 Alignment
- Persist compressor analysis data in PPDM-aligned entities.

### 4) Cross-Project Integration
- **ProductionOperations**: facility constraints and downtime.
- **EconomicAnalysis**: cost/benefit of optimization.
- **PipelineAnalysis**: downstream constraints.

---

## Repository layout (shipped)

High-level layout under **`Beep.OilandGas.CompressorAnalysis`**:

```
Beep.OilandGas.CompressorAnalysis/
├── Core/Interfaces/          # ICompressorAnalysisService
├── Data/Tables/              # COMPRESSOR_* extension entities (scalar table shapes)
├── Data/Constants/         # Seed + numeric constants (namespace …CompressorAnalysis.Constants)
├── Modules/                  # CompressorAnalysisModule (EntityTypes, SeedAsync)
├── Services/                 # CompressorAnalysisService (+ partials)
├── Calculations/             # Centrifugal / reciprocating / pressure calculators
├── Validation/               # CompressorValidator
└── Exceptions/               # CompressorException and domain errors
```

Executable detail and phased backlog: **`Beep.OilandGas.CompressorAnalysis/.plans/README.md`**.

---

## Data model — shipped vs backlog

**Registered on `CompressorAnalysisModule.EntityTypes` today** (under **`Data/Tables`**):

- **`COMPRESSOR_OPERATING_CONDITIONS`**, **`CENTRIFUGAL_COMPRESSOR_PROPERTIES`**, **`RECIPROCATING_COMPRESSOR_PROPERTIES`**
- **`COMPRESSOR_POWER_RESULT`**, **`COMPRESSOR_PRESSURE_RESULT`**
- **`R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`** (LOV seed via **`CompressorAnalysisReferenceCodeSeed`**)

**Wire DTOs** (shared across API / LifeCycle / client): **`CompressorAnalysisRequest`**, **`CompressorAnalysisResult`**, **`CompressorAnalysisWellKnown`** in **`Beep.OilandGas.Models.Data.Calculations`**.

**Backlog / not modeled** until added as extension tables and wired into **`EntityTypes`**: scenario/run tables such as **`COMPRESSOR_TEST`**, **`COMPRESSOR_ANALYSIS_RUN`**, performance curve storage, surge/operating-window persistence, etc.

---

## Service interface

The DI surface is **`Beep.OilandGas.CompressorAnalysis.Core.Interfaces.ICompressorAnalysisService`**: **`CalculateCentrifugalPowerAsync`**, **`CalculateReciprocatingPowerAsync`**, **`CalculateRequiredPressureAsync`** using **`COMPRESSOR_*`** types in **`Beep.OilandGas.CompressorAnalysis.Data`**.

---

## HTTP surface (`CompressorController`)

Base route **`[Route("api/[controller]")]`** → **`/api/compressor`** (authorize). Representative actions:

| Method | Route | Purpose |
|--------|-------|---------|
| POST | **`/api/compressor/analyze`** | Default centrifugal path from operating conditions |
| POST | **`/api/compressor/power`** | Same physics as analyze (explicit power entry) |
| POST | **`/api/compressor/design/centrifugal`** | Full **`CENTRIFUGAL_COMPRESSOR_PROPERTIES`** body |
| POST | **`/api/compressor/design/reciprocating`** | Full **`RECIPROCATING_COMPRESSOR_PROPERTIES`** body |

Required discharge pressure / packaged facility flows use **`ICalculationService.PerformCompressorAnalysisAsync`** (LifeCycle) and **`POST /api/calculations/compressor`** — not necessarily duplicate routes on **`CompressorController`**.

---

## Success criteria (current product)

- Extension compressor tables are discoverable via **`CompressorAnalysisModule`** and consistent with PPDM metadata tooling.
- **`ICompressorAnalysisService`** is the single domain entry used by **`CompressorController`** and packaged **`PerformCompressorAnalysisAsync`** branches.
- **`CompressorAnalysis.Tests`** and ApiService tests guard orchestration and controller behavior.

---

**Document Version**: 1.1  
**Last Updated**: April 2026  
**Status**: Aligned with shipped **`Beep.OilandGas.CompressorAnalysis`**
