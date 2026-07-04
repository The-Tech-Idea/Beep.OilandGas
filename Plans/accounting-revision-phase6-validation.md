# Phase 6 — Validation & Error Handling

> **Status:** Not Started | **Depends on:** Phase 1-3 | **Est. Effort:** 1 week
> **Standards:** [standards.md](standards.md)

---

## Current Issues Found

1. **No input validation** — Most Accounting methods don't validate parameters
2. **No structured logging** — Many services have no logger at all
3. **Missing XML docs** — ~80% of Accounting methods lack `<summary>` comments
4. **Inconsistent error handling** — Some throw, some return default, some return null
5. **Fragile data parsing** — `InventoryLcmService` parses NRV from `REMARK` string field
6. **No audit logging** — Financial transactions not logged to audit trail
7. **Pessimistic error handling in closers** — `PeriodClosingService` returns `true` on error, preventing close

---

## Task Details

| ID | Task |
|----|------|
| A6-01 | Add null/empty guards to all public Accounting methods |
| A6-02 | Standardize error return: use `Result<T>` pattern with `Success` + `Errors` |
| A6-03 | Add `ILogger<T>` injection to all services missing it |
| A6-04 | Add structured logging (`LogInformation`/`LogWarning`/`LogError`) to all methods |
| A6-05 | Add XML doc `<summary>` comments to all public methods |
| A6-06 | Replace `REMARK` string parsing in `InventoryLcmService` with structured fields |
| A6-07 | Add financial audit trail logging (who changed what, when) |
| A6-08 | Fix pessimistic defaults in `PeriodClosingService` — return real status |
| A6-09 | Standardize exception propagation policy (Section 5 of standards.md) |
| A6-10 | Remove duplicate `using` directives found in Accounting files |
