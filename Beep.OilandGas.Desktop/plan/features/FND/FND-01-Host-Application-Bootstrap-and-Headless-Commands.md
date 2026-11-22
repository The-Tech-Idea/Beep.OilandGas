# FND-01 — Host application, bootstrap and headless commands

| Field | Value |
|---|---|
| Stage | STG-00 — Foundation and host shell |
| Area | Foundation |
| Status | Gate met - AC-1 to AC-6, including the shell-close lifetime and the smoke root. Evidence: [HEADLESS](../../evidence/STG-00-headless-verbs/observed-evidence.json) |
| Owner projects | none (desktop host only) |
| Desktop project | `src/Beep.OilandGas.Desktop/` |
| External packages | none |
| Depends on | — |
| Blocks | every feature |
| Web reference | none |

## 1. Purpose

Start the application deterministically, in the same order and with the same guarantees as
`Beep.DoitAll.Erp.Wpf/App.xaml.cs`: environment variables resolved before any path is read,
theme and localization initialised before a window exists, headless verbs available for
verification, and a single failure path if startup throws.

## 2. Source components

| Source | Reuse |
|---|---|
| `Beep.DoitAll.Erp.Wpf/App.xaml.cs` | full startup sequence, `--smoke`, `--schema-apply`, `--schema-resume`, `--drivers-report`, `--entitlement-report`, `--licence-check`, `--restore`, seeds and benchmarks |
| `Beep.DoitAll.Erp.Wpf/Bootstrapping/ErpPaths.cs` | `OgPaths` — `%LOCALAPPDATA%\TheTechIdea\BeepOilGas`, `BEEP_OILGAS_DATA_ROOT`, `BEEP_LOG_DIR` |
| `Beep.DoitAll.Erp.Wpf/Bootstrapping/ErpBootstrapper.cs` | `OgBootstrapper` |
| `Beep.DoitAll.Erp.Wpf/Bootstrapping/ErpApplication.cs` | `OgApplication` — `IsSetupComplete`, `GetSetupBlocker` |
| `Beep.DoitAll.Erp.Wpf/Shell/ErpShellWindow.xaml.cs` | shutdown behaviour (`OnExplicitShutdown`) |

## 3. Screens

No screens. The host exposes:

- `SplashWindow` — shown only when startup takes longer than 1 500 ms; carries no logic.
- Unhandled-failure dialog — text plus failure reference, no stack trace.

## 4. Data and contracts

`OgStartupSequence` returns an `OgStartupOutcome`:

```csharp
enum OgStartupOutcome { Shutdown, HeadlessComplete, ShellReady }
```

It must be possible to run the sequence with no window at all (test harness).

## 5. States

| State | Behaviour |
|---|---|
| Setup incomplete | `FirstRunWindow`; shell never opens |
| Initialise failed, setup complete | repair through `FirstRunWindow`; shell never opens |
| Headless verb | console output, exit code, `Shutdown` |
| Startup exception | failure recorded, dialog with reference, exit code 1 |

## 6. Commands and permissions

| Verb | Exit | Writes |
|---|---|---|
| `--smoke` | 0 ok / 1 failed | writes `smoke-result.txt` |
| `--schema-apply` / `--schema-resume` | 0 / 1 | schema — **refused (exit 2) until DAT-02 is delivered** |
| `--drivers-report` | 0 ok / 1 the report could not be produced | no |
| `--entitlement-report` | 0 ok / 1 the report could not be produced | no |
| `--licence-check <file>` | 0 signature verified and current / 2 refused | no |
| `--report-benchmark` / `--benchmark` | 0 report written (`result: ok` or `result: over-budget`) | no |
| `--seed-benchmark` | 0 file written | no |
| `--soak <minutes>` | 0 measurement written (`result: ok` or `result: over-budget`); 2 for a bad duration | no |
| `--restore <file>` | 0 / 2 | database — **refused (exit 2) until REL-01 provides a backup service** |
| an unknown `--verb` | 2 | no; prints the usage |

A report that cannot be produced is **not** a success: an operator scripting `--drivers-report &&
install` must not proceed on a report that never ran. Hence 1, not 0. A stated refusal (no backup
service, a verb that needs DAT-02, a licence that is missing or expired) is 2 and is not reported as an
unexpected failure.

## 7. Acceptance criteria

- AC-1 The static constructor sets the data root and log directory before any type reads them.
- AC-2 Every headless verb returns its documented exit code without opening a window.
- AC-3 `--smoke` writes a result file even when startup throws.
- AC-4 No startup exception reaches the user as a raw message.
- AC-5 The process exits when the shell closes and does not exit when the sign-in dialog closes.
- AC-6 A second instance of `--smoke` deletes and recreates its root.

## 8. Verification

```
dotnet run --project src/Beep.OilandGas.Desktop -- --smoke
type %LOCALAPPDATA%\TheTechIdea\BeepOilGas\smoke-result.txt
```

Expect `result: ok` and `views-constructed: True`.
Tests: `FND-01/T01`, `FND-01/T02`, `STG-00/T09`.

## 9. Open questions and risks

- `--seed-benchmark` and `--report-benchmark` may not be meaningful for this product; keep the
  verbs but make them no-ops until REL-04 defines benchmarks.

### Delivered 2026-11-05

- `OgCommandLine` maps the first argument to a verb; anything after it is that verb's argument, so a path
  containing `--restore` cannot hijack the run. An unknown `--flag` is `Unknown` (usage, exit 2), never a
  normal start.
- `OgHeadlessRunner` runs one verb against the composed application and returns the exit code. Every verb
  writes to an injected `TextWriter`, so the output and the code are testable without a process.
- `App.OnStartup` parses once; a headless verb never opens a window. If startup throws, a headless run
  prints the failure, writes `smoke-result.txt` for `--smoke`, and exits 1 **without a dialog** — a
  scheduled run must not hang on a `MessageBox` no console can dismiss (AC-2, AC-3).
- `--drivers-report` lists what BeepDM can actually use. It exposed a real gap: the packaged
  `ConnectionDrivers` folder does not exist on a normal build, so the local driver was only ever
  registered by the DAT-01 test. `OgBundledDrivers` now registers the data-source assemblies this build
  references, and the report shows 214 drivers with `local-driver: SqlLite`.
- `--entitlement-report` prints both gates per page (entitlement and role) and the resolved `open=` verdict.
- `--licence-check` **verifies the signature** (REL-02, delivered 2026-11-06): the file is an envelope whose
  payload is signed with RSA-2048/SHA-256 and checked against an embedded key. An unsigned or altered file is
  refused with `licence-unsigned` / `licence-signature-invalid`; a pass reads `verified-ok (key …)` and names
  the key. An expired licence is refused with its date and grace end.
- `--restore` says "not configured; REL-01 provides it" and exits 2 (FND-01 §9).
- `--schema-apply` / `--schema-resume` say DAT-02 owns them and exit 2.

### AC-3, AC-5 and AC-6 — delivered 2026-11-05

- **AC-3** `OgHeadlessFailureReport` composes and writes the failure report (command, `result: failed`, the
  reference). App calls it from the startup catch instead of showing a dialog, so a scheduled run cannot
  hang; a test proves the smoke verb writes the result file and a non-smoke verb does not.
- **AC-5** `OgShellWindow` takes an `onClosed` action whose production default is
  `Application.Shutdown(0)`; a STA test constructs the shell with a recorder and `Close()` calls it exactly
  once. `App.xaml` is `OnExplicitShutdown`, so the first-run, sign-in and repair dialogs cannot end the
  session, and a guard forbids `.Shutdown(` outside `App.xaml.cs` and the shell.
- **AC-6** `OgSmokeRoot.Prepare` deletes and recreates the root; a test seeds a stale store, `setup.json`
  and `smoke-result.txt` and asserts they are gone. Two live runs each report `audit-rows 1` and
  `failures-page-rows 1` — nothing accumulated, which is the behaviour an operator depends on.
- `--soak <minutes>` (REL-04 AC-5, 2026-11-06) runs scripted typical use and writes
  `Diagnostics/soak-report.json`; the default is 30 minutes.
- `--seed-benchmark` and `--report-benchmark` were recognised verbs that refused with exit 2 pending REL-04;
  **REL-04 delivered them (2026-11-06)**: `--report-benchmark` measures the budgets and writes
  `Diagnostics/benchmark-report.json` (exit 0, `result: ok` or `result: over-budget`), `--benchmark` is an
  alias, and `--seed-benchmark` writes the deterministic 100 000-sample log the benchmark uses.

### Still open (outside the acceptance criteria)

- The drivers report takes ~8 s because BeepDM loads every assembly; it is a diagnostic, not a startup path.
- Restore needs a backup service; that arrives with REL-01. Until then `--restore` reports
  "not configured" and exits 2.
