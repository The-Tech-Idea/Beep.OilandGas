# Beep.OilandGas.PumpPerformance

## Snapshot

- Category: Engineering
- Scan depth: Medium
- Current role: pump performance and system analysis module
- Maturity signal: technically rich module with no direct API or web surfacing found in this scan

## Observed Structure

- Top-level folders: `Calculations`, `Constants`, `Exceptions`, `Interaction`, `PumpTypes`, `Rendering`, `Services`, `SystemAnalysis`, `Validation`
- Root files include `PumpPerformanceCalc.cs` and multiple phase summary documents
- The project is deeper than the standardized engineering modules because it includes pump-type and system-analysis structure

## Representative Evidence

- Core calculator: `PumpPerformanceCalc.cs`
- Specialized areas: `PumpTypes/`, `SystemAnalysis/`, `Rendering/`
- Current surfacing status: no dedicated pump-performance controller or Razor page found in this scan

## Planning Notes

- This is a strong candidate for future surfacing once the broader pump workflow story is rationalized.
- Phase 8 should avoid creating redundant UI if existing pump pages can consume this project internally.
