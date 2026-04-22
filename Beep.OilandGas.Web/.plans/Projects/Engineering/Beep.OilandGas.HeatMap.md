# Beep.OilandGas.HeatMap

## Snapshot

- Category: Engineering
- Scan depth: Medium
- Current role: heat-map and advanced visualization module
- Maturity signal: broad visualization/analysis library with direct API and web surfacing

## Observed Structure

- Top-level folders include `Aggregation`, `Analysis`, `Animation`, `Annotations`, `Clustering`, `ColorSchemes`, `Configuration`, `Contour`, `Density`, `Export`, `Filtering`, `Interaction`, `Interpolation`, `Layers`, `Performance`, `Realtime`, `Rendering`, `Services`, `Statistics`, `Tools`, `Visual`, `Visualization`
- Root files include `HeatMapGenerator.cs` and multiple enhancement/professional-feature notes
- This is one of the broadest engineering libraries in the repo

## Representative Evidence

- Core generator: `HeatMapGenerator.cs`
- Visualization support: `Visualization/`, `Contour/`, `Realtime/`, `Export/`
- API surfacing: `Beep.OilandGas.ApiService/Controllers/Properties/HeatMapController.cs`
- Web surfacing: `Beep.OilandGas.Web/Pages/PPDM39/Properties/HeatMap.razor`

## Planning Notes

- This module is mature enough for purposeful visualization-driven workflows.
- Phase plans should avoid turning it into a generic demo page and instead anchor it to operational or property analysis decisions.
