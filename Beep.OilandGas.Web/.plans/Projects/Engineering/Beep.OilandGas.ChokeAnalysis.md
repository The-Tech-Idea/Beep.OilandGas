# Beep.OilandGas.ChokeAnalysis

## Snapshot

- Category: Engineering
- Scan depth: Standardized
- Current role: choke analysis module
- Maturity signal: standardized module with live calculation API and web workbench surfacing

## Observed Structure

- Top-level folders: `Calculations`, `Constants`, `Exceptions`, `Models`, `Services`, `Validation`
- The module follows the common engineering-project pattern with an additional `Models` folder

## Representative Evidence

- Module structure: `Calculations/`, `Models/`, `Services/`, `Validation/`
- Current surfacing status: exposed through `Beep.OilandGas.ApiService/Controllers/CalculationsController.cs` and surfaced in `Beep.OilandGas.Web/Pages/PPDM39/Calculations/ChokeAnalysis.razor`

## Active Product Surface

- API entry point: `POST /api/calculations/choke`
- Web routes: `/ppdm39/calculations/choke-analysis` and `/analysis/choke`
- Navigation: active-shell production navigation plus `PPDMTreeView` calculation entries
- Context inputs: live field summary plus live development wells from field-scoped APIs

## Planning Notes

- The module is now productized as a typed calculation vertical slice rather than a page-level direct library call.
- The next integration step is not basic surfacing anymore; it is composition into broader production optimization and intervention workflows.
