# Phase 03 - Logs, Lithology And Correlation

## Objective

Add production-ready log display and correlation capabilities so the library can support petrophysical review, lithology interpretation, zonation, and multi-well comparison workflows.

## Starting Point In Code

- `LogData` and `LogCurveMetadata` already provide a usable normalized model.
- `LasLogLoader`, `DlisLogLoader`, `WitsmlLogLoader`, and `PwlsMnemonicMapper` already exist.
- Lithology-related styling assets already exist.
- No full multi-track log renderer is implemented yet.

## Scope

- Multi-track log rendering.
- Curve scaling, units, shading, fills, and track templates.
- Lithology strips and interpreted interval panels.
- Multi-well correlation panels and section-style correlation fences.
- Crossplots and simple petrophysical overlays where useful.

## Standards And Domain Drivers

- LAS 2.0 / 3.0
- API RP66 / DLIS
- WITSML log objects
- PWLS mnemonic normalization
- PPDM `LITH_*`, `WELL*`, and well-log related subject areas

## Work Packages

### 1. Multi-track renderer

- Add depth track, curve tracks, headers, grid lines, and scale modes.
- Support linear and logarithmic scales where appropriate.
- Add null handling and curve clipping policies.

### 2. Lithology and zonation

- Use lithology patterns and colors for strip logs and zonation panels.
- Support tops, facies, core intervals, and pay indicators.
- Align lithology panels with curve tracks and depth rulers.

### 3. Correlation workflows

- Add synchronized depth scrolling across wells.
- Add fence-style correlation layouts and marker alignment.
- Add a lightweight interpretation overlay model for tops and picked intervals.

## Deliverables

- Log track renderer.
- Track template and styling system.
- Lithology strip renderer.
- Multi-well correlation view.

## Exit Criteria

- LAS, DLIS, and WITSML curves can be normalized into the same log scene.
- A standard petrophysical layout can be produced with repeatable track configuration.
- Multiple wells can be visually correlated with aligned markers and intervals.

## Risks

- If mnemonic normalization stays optional, curve templates will fragment by vendor naming.
- Correlation views will drift into one-off layouts unless the section model is formalized here.