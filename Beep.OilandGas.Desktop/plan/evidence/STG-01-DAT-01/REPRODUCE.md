# STG-01 / DAT-01 — Reproduction

Run from `Beep.OilandGas.Desktop/`.

## 1. Build

```powershell
dotnet build src/Beep.OilandGas.Desktop.slnx -c Debug
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`.

> **Gate note.** The `Data` project now references BeepDM and `BeepDataSources` **source** (rewritten by
> the repository's `Directory.Build.targets`). On a truly clean rebuild those sibling projects emit their
> own warnings (CS8632/CS1573/CS8618/CS8604, ~10 000). They are not our code. The gate therefore means
> **zero warnings and errors attributable to `Beep.OilandGas.Desktop.*`**; incremental builds report 0/0.
> If a warning ever appears in a path under `Beep.OilandGas.Desktop`, the gate is red.

## 2. DAT-01 focused tests

```powershell
cd test/Beep.OilandGas.Desktop.Tests
dotnet test -c Debug --filter "FullyQualifiedName~ConnectionCatalogIntegrationTests"
```

Expected: `Passed! - Failed: 0, Passed: 2`.

| Test | Proves |
|---|---|
| `Creates_reopens_and_reads_a_local_database` | a driver is discovered; the catalog creates a local file database; the connection is persisted; a **fresh** runtime lists it and reopens it; the file is a real SQLite database holding the seeded row; no failure was reported |
| `Testing_an_unknown_connection_is_refused_with_a_reason` | an unknown connection is refused with `connection-not-found`, not an exception |

## 3. Full suite

```powershell
dotnet test -c Debug
```

Expected: `Passed! - Failed: 0, Passed: 30`.

## 4. Smoke

```powershell
src/Beep.OilandGas.Desktop/bin/Debug/net10.0-windows/TheTechIdea.Beep.OilandGas.Desktop.exe --smoke
```

Expected: `result: ok`, `views-constructed: True`, `shell-constructed: True`, exit `0`.
(`ConnectionsView` constructs without touching BeepDM, because the runtime is lazy.)

## 5. What is deliberately not covered

- Only the SQLite provider is exercised. SQL Server, PostgreSQL and Oracle are untested here.
- The discovered driver does **not** set `ConnectionDriversConfig.CreateLocal`; `FirstLocalDriver`
  falls back to the first non-in-memory driver. The AC wording "a driver advertising CreateLocal" is
  therefore only partially met — recorded as an open question in DAT-01.
- The integration test seeds its own table; the **full PPDM schema** is DAT-02 (module setup), not DAT-01.
- No UI click-through: `ConnectionsView` was constructed, not driven.
- `--drivers-report` and the rest of the headless verbs remain unimplemented.
