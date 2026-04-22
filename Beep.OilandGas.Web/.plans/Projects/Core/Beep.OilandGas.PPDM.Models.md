# Beep.OilandGas.PPDM.Models

## Snapshot

- Category: Core/shared entities
- Scan depth: Medium
- Current role: PPDM entity model project used by PPDM39 and data-management layers
- Maturity signal: focused entity container rather than orchestration layer

## Observed Structure

- Top-level folders: `39`, `Utilities`
- Root files include `IPPDMEntity.cs`
- The project appears intentionally narrow compared with `Beep.OilandGas.Models`

## Representative Evidence

- Entity marker: `IPPDMEntity.cs`
- Entity folder: `39/`
- Support folder: `Utilities/`

## Planning Notes

- This project should remain a data/entity dependency, not a workflow or orchestration location.
- Phase 9 and 10 validation should watch this project for shared-entity continuity rather than new behavior.
