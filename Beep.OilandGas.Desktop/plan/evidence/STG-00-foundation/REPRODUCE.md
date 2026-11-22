# STG-00 — Reproduction

Run from `Beep.OilandGas.Desktop/`.

## 1. Build

```powershell
dotnet build src/Beep.OilandGas.Desktop.slnx -c Debug
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`.

## 2. Focused tests

```powershell
cd test/Beep.OilandGas.Desktop.Tests
dotnet test -c Debug
```

Expected: `Passed! - Failed: 0, Passed: 24, Skipped: 0, Total: 24`.

Covers:

| File | Proves |
|---|---|
| `OgPathsTests` | the failure store and log are under the per-user root and never beside the executable (ADR-013) |
| `DiagnosticsPipelineTests` | well-formed reference, durable record, secret redaction, degraded store, one audit row per write and refusal |
| `ToastPipelineTests` | severity → toast mapping, sticky critical, audit-only events do not toast, a failure toast without a reference is refused |
| `SourceGuardTests` | the toast service is used only by the hub, `MessageBox.Show` only in startup, no catch without reporting, every catalog route has a factory case, unique routes, known module keys |

## 3. Headless smoke

```powershell
src/Beep.OilandGas.Desktop/bin/Debug/net10.0-windows/TheTechIdea.Beep.OilandGas.Desktop.exe --smoke
Get-Content "$env:TEMP\TheTechIdea\BeepOilGasSmoke\smoke-result.txt"
```

Expected: `result: ok`, `views-constructed: True`, `shell-constructed: True`, exit code `0`.

## 4. What is deliberately not covered

- No live database, provider, API or identity server was exercised.
- The WPF window was constructed but not shown; no screenshot, no pixel check, no RTL pass.
- `--schema-apply`, `--drivers-report`, `--entitlement-report`, `--licence-check` and `--restore`
  are not implemented yet, so SG-00-4 is not claimed.
- The audit row is written to the same local SQLite file as failures; business audit integration with
  `Beep.OilandGas.Repository` is STG-01 work.
