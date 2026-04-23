# Phase 02 - Well Schematics And Survey

## Objective

Upgrade the library from basic well sketching to engineer-grade wellbore visualization with deviation-aware geometry, completion context, production strings, and operational overlays.

## Starting Point In Code

- `WellSchematicLayer` already renders borehole, casing, tubing, equipment, and perforation primitives.
- `WellSchematicBuilder` already gives a usable entry point.
- `DeviationSurvey` exists, but the renderer still treats deviated wells as simplified rectangles.

## Scope

- True deviated path rendering using survey points.
- Multi-borehole and sidetrack support.
- Completion view overlays for casing, tubing, perforations, packers, safety equipment, and string components.
- Depth rulers, measured-depth and true-vertical-depth labels, and event markers.
- Optional status overlays for workover, drilling, and completion activity.

## Standards And Domain Drivers

- WITSML trajectories, logs, and completions context
- PPDM `WELL*`, `WELL_ACTIVITY_COMPONENT`, `PROD_STRING_COMPONENT`, `FACILITY*`, `EQUIPMENT*`

## Work Packages

### 1. Trajectory-aware geometry

- Build a path-calculation service using `DeviationSurvey`.
- Support MD, TVD, and lateral displacement views.
- Add interpolation and survey-quality checks.

### 2. Completion and equipment overlays

- Render string components, packers, valves, safety equipment, and completion intervals.
- Support multiple strings and selective depth windows.
- Standardize symbol usage across vertical and deviated views.

### 3. Annotation and engineer workflow support

- Add kickoff point, landing point, perforation interval, and equipment callout annotations.
- Add operational overlays for activity windows and completion status.
- Add simplified print-friendly and detailed engineering layouts.

## Deliverables

- Deviated well renderer.
- Completion overlay engine.
- Survey interpolation and QA helpers.
- Engineer-ready depth and event annotations.

## Exit Criteria

- A directional well can be rendered from survey points instead of a rectangular fallback.
- One well can show multiple completion strings and operational markers in a readable layout.
- The same scene can switch between compact, detailed, and print-oriented profiles.

## Risks

- Symbol sprawl will become unmanageable if equipment and completion glyphs are not normalized early.
- Survey interpolation errors will undermine trust if QA rules are not built into the rendering pipeline.