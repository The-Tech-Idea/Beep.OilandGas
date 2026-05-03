# Decommissioning Migration Notes

## Canonical Path
- Runtime decommissioning service is `PPDMDecommissioningService` through `FieldOrchestrator`.
- WELL lifecycle state transitions use `WELL_STATUS` with reference validation.

## Replaced/Deprecated Patterns
- `PROJECT`-discriminator storage for restoration/cost decommissioning paths is removed from canonical logic.

## Rollout Notes
- Keep compatibility fallbacks only when metadata for optional PPDM tables is absent.
- Ensure each fallback path has explicit logs and deterministic response shape.
