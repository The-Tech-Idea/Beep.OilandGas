# Phase 3 — PPDM and data paths

## Goal

When calculations start from **PPDM entities** (`WELL`, `WELL_TEST_FLOW_MEAS`, `WELL_TUBULAR`, `WELL_PRESSURE`, …), mapping must be:

- **Explicit** (helper methods or dedicated mapper class).
- **Consistent** with single-schema PPDM39 rules (see [CLAUDE.md](../../CLAUDE.md)).
- **Safe** on missing data — validate before math, don’t invent physical values without documentation.

## WellServices rule

- This module **must not** implement well CRUD or parallel `WELL_STATUS` queries.
- Callers may pass **`WELL`** instances obtained elsewhere; if future work needs fresh well reads, consume **WellServices** from `Beep.OilandGas.PPDM39.DataManagement` in the **API or orchestration layer**, then pass entities into ChokeAnalysis.

## PPDMGenericRepository (when persisting choke-related results)

If choke results are persisted to PPDM tables:

1. Resolve metadata: `GetTableMetadataAsync("TABLE_NAME")`.
2. Resolve entity type from `Beep.OilandGas.PPDM39.Models`.
3. Instantiate `PPDMGenericRepository` with table name.
4. Use `AppFilter` for queries; `InsertAsync` / `UpdateAsync` for writes.

**Do not** use `ExecuteSql` for application SELECT paths.

Table classes passed to `InsertAsync`/`UpdateAsync` must remain **scalar** — no nested collections on persisted entities.

## Value retrieval

Centralize magic numbers and fallbacks (e.g. default choke diameter, 80% downstream estimate) in one place:

- Prefer named helpers (e.g. `ValueRetrievers` if present in service partials) with XML docs stating engineering assumption.

## TODO checklist

- [ ] Audit `ChokeAnalysisService` PPDM overloads for null handling and engineering assumptions.
- [ ] List each fallback default (Z-factor, temperature, pressures) — ticket any that must become measured inputs.
- [ ] Confirm IDs formatted with `_defaults.FormatIdForTable(...)` when introducing persisted choke result rows (if applicable).
- [ ] If adding new PPDM tables for choke: add Models table classes under shared library, metadata alignment, seed if reference data.

## Verification

```bash
dotnet build Beep.OilandGas.ChokeAnalysis/Beep.OilandGas.ChokeAnalysis.csproj
dotnet build Beep.OilandGas.PPDM39.DataManagement/Beep.OilandGas.PPDM39.DataManagement.csproj
```

## References

- [07_Scenarios_Best_Practices_And_Industry_Reference.md](07_Scenarios_Best_Practices_And_Industry_Reference.md) — surface vs downhole choke context, parameter checklist (\(P_1\), \(P_2\), GLR, WC), reservoir archetypes.
- `.cursor/commands/beep-dataaccess-generic-repository.md`
- `.cursor/commands/beep-dataaccess-overview.md`
- `.cursor/skills/ppdm39-db/SKILL.md` (repo-local skill if present)
