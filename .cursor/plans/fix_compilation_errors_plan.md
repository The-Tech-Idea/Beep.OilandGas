# Fix Compilation Errors Plan

## Overview
This plan addresses multiple compilation errors across the Beep.OilandGas solution.

## Error Categories

### 1. WellTestAnalysisRequest Issues
- **Problem**: Method signature uses `WellTestAnalysisRequest` but interface expects `WellTestAnalysisCalculationRequest`
- **Problem**: `WellTestAnalysisRequest` missing many properties (IsGasWell, WellId, PressureTimeData, etc.)
- **Fix**: Change method signature to use `WellTestAnalysisCalculationRequest`
- **Fix**: Use fully qualified type names to resolve ambiguity

### 2. Missing Repository Methods
- **Problem**: Missing methods: `GetAccountingAmortizationRepositoryAsync`, `GetAccountingCostRepositoryAsync`, `GetAccountingImpairmentRepositoryAsync`, `GetAccountingMethodRepositoryAsync`
- **Fix**: Add these methods following the pattern of existing repository methods

### 3. CreateAsync vs InsertAsync
- **Problem**: Code calls `repository.CreateAsync()` but method is `InsertAsync()`
- **Fix**: Replace all `CreateAsync` calls with `InsertAsync`

### 4. Missing Properties in Models
- **Problem**: `ProvedReserves` missing `OilReserves` and `GasReserves` properties
- **Problem**: `DevelopmentCosts` missing `WellId` and `DevelopmentCosts` properties
- **Fix**: Add convenience properties or fix code to use existing properties

### 5. Ambiguous References
- **Problem**: `NodalAnalyzer` exists in two namespaces
- **Problem**: `WellTestAnalysisResult` exists in two namespaces
- **Fix**: Use fully qualified type names

### 6. Type Conversion Issues
- **Problem**: Various type conversion errors (decimal? to decimal, etc.)
- **Fix**: Add proper null checks and conversions

### 7. Logger Type Mismatches
- **Problem**: Logger generic types don't match
- **Fix**: Use correct logger types or pass null

## Implementation Steps

1. Fix WellTestAnalysisRequest method signature
2. Add missing repository methods
3. Replace CreateAsync with InsertAsync
4. Fix ambiguous type references
5. Add missing model properties or fix code
6. Fix type conversions
7. Fix logger type mismatches

