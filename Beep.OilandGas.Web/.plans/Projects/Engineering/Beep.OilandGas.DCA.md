# Beep.OilandGas.DCA

## Snapshot

- Category: Engineering
- Scan depth: Medium
- Current role: decline curve analysis module
- Maturity signal: rich library-style module with no current direct API or web surfacing found in this scan

## Observed Structure

- Top-level folders: `AdvancedDeclineMethods`, `Constants`, `DataImportExport`, `Exceptions`, `Interaction`, `MultiSegment`, `MultiWell`, `Performance`, `Rendering`, `Results`, `Services`, `Statistics`, `Validation`, `Visualization`
- Root files include `DCAGenerator.cs`, `DCAManager.cs`, and `NonlinearRegression.cs`
- The module is broader than the standardized engineering projects and includes import/export plus visualization support

## Representative Evidence

- Core analyzers: `DCAGenerator.cs`, `DCAManager.cs`, `NonlinearRegression.cs`
- Support areas: `AdvancedDeclineMethods/`, `MultiWell/`, `Validation/`, `Visualization/`
- Current surfacing status: no direct `DCA` API controller or Razor page found in this scan

## Planning Notes

- This project is technically richer than its current solution surfacing implies.
- Later planning can either surface it as a dedicated engineering workbench or reuse its internals behind broader production-forecast workflows.
