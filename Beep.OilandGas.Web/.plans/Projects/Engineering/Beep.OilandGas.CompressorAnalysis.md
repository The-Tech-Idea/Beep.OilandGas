# Beep.OilandGas.CompressorAnalysis

## Snapshot

- Category: Engineering
- Scan depth: Standardized
- Current role: compressor analysis module
- Maturity signal: standardized module with live calculation API and web workbench surfacing

## Observed Structure

- Top-level folders: `Calculations`, `Constants`, `Exceptions`, `Services`, `Validation`
- The module follows the common engineering-project pattern and includes implementation summary docs

## Representative Evidence

- Module structure: `Calculations/`, `Services/`, `Validation/`
- Current surfacing status: exposed through `Beep.OilandGas.ApiService/Controllers/CalculationsController.cs` and surfaced in `Beep.OilandGas.Web/Pages/PPDM39/Calculations/CompressorAnalysis.razor`

## Active Product Surface

- API entry point: `POST /api/calculations/compressor`
- Web routes: `/ppdm39/calculations/compressor-analysis` and `/analysis/compressor`
- Navigation: active-shell production navigation plus `PPDMTreeView` calculation entries
- Context inputs: live field summary plus field-scoped facilities from `GET /api/field/current/development/facilities`

## Planning Notes

- The module is now productized as a typed calculation vertical slice with a facility-aware workbench.
- The next integration step is composition into broader facility optimization and production workflow pages rather than another standalone surfacing pass.
