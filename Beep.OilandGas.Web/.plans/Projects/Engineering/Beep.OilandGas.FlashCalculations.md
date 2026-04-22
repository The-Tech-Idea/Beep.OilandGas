# Beep.OilandGas.FlashCalculations

## Snapshot

- Category: Engineering
- Scan depth: Standardized
- Current role: flash-calculation module
- Maturity signal: standardized module with API surfacing but no current dedicated web page found in this scan

## Observed Structure

- Top-level folders: `Calculations`, `Constants`, `Exceptions`, `Services`, `Validation`
- The module follows the common engineering-project pattern

## Representative Evidence

- Module structure: `Calculations/`, `Services/`, `Validation/`
- API surfacing: `Beep.OilandGas.ApiService/Controllers/Calculations/FlashCalculationController.cs`
- Current web surfacing status: no dedicated flash-calculation Razor page found in this scan

## Planning Notes

- This module is partially surfaced and is a good candidate for later UI addition if a real workflow needs it.
- Until then, it should remain a backend capability instead of forcing a thin standalone page.
