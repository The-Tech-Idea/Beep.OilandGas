# Logging, Audit and Notification Pipeline

**Document ID:** PLAN-07
**Version:** 1.0
**Created:** 2026-11-05
**Status:** Mandatory for every stage and every feature

This document is the single authority for how the desktop records and surfaces logs, exceptions and
audit messages. No feature document may define its own logging, its own error dialog, or its own
toast. If a feature document conflicts with this one, this one wins.

Source of truth for the rules: `.claude/skills/beepauditlogging` (checked against
`TheTechIdeaWeb.Diagnostics*` on 2026-11-04) and `Beep.OilandGas/AGENTS.md`.

---

## 1. The standing rule

> Every catch reports. Every exception reaches one service. Every audit event is durable.
> Every message a person needs to see arrives through the same pipeline, and the pipeline
> shows it as a toast.

Three consequences, and they are not negotiable:

1. **No swallowed exceptions.** Not one. Not a `catch { }`, not a `catch { return null; }`, not a
   `catch (Exception ex) { _logger.LogWarning(ex, "..."); }` that only logs.
2. **One pipeline.** There is exactly one fan-in for logs, exceptions, audit and user messages.
   Feature code publishes into it; it never talks to a toast, a `MessageBox`, or a log file directly.
3. **The toast is the visible end of the pipeline**, driven by the service — not called by feature
   code.

---

## 2. Pipeline

```
┌──────────────────────── producer (any layer) ────────────────────────┐
│                                                                      │
│  catch (Exception ex)                                                │
│      → _failures.ReportHandled(ex, operation, consequence, severity) │
│                                                                      │
│  privileged / security-relevant change                               │
│      → _audit.Add(verb, subjectKind, subjectId, outcome, detail)     │
│                                                                      │
│  diagnostic / trace                                                  │
│      → _logger.LogInformation(...)        (framework ILogger only)   │
│                                                                      │
└───────────────────────────────┬──────────────────────────────────────┘
                                │
                  ┌─────────────▼──────────────┐
                  │ OgDiagnosticsPipeline      │   single fan-in
                  │  implements IFailureReporter│
                  │  implements IAuditService   │
                  └──┬──────────┬───────────┬──┘
                     │          │           │
      ┌──────────────▼──┐  ┌────▼────────┐  ┌▼─────────────────────┐
      │ durable store   │  │ rolling log │  │ OgNotificationHub    │
      │ OilGasDiagnostics│ │ BEEP_LOG_DIR│  │  (severity, dedupe,  │
      │ DbContext      │  │             │  │   reference, route)  │
      │ + retention    │  │             │  └──┬───────────────────┘
      └────────────────┘  └─────────────┘     │
                                              ▼
                                   ┌──────────────────────┐
                                   │ OgToastService       │
                                   └──┬────────────────┬──┘
                                      │                │
                            ┌─────────▼──────┐  ┌──────▼──────────────┐
                            │ ToastHost      │  │ NotificationCenter  │
                            │ (live toasts)  │  │ (history, mark read)│
                            └────────────────┘  └─────────────────────┘

  App-wide handlers ──────────────────────────────┘
   DispatcherUnhandledException
   AppDomain.CurrentDomain.UnhandledException
   TaskScheduler.UnobservedTaskException
   (all routed through ProcessFailureSubscription → same pipeline)
```

**One arrow in, three arrows out.** Nothing writes to the store, the log or a toast except this
pipeline.

---

## 3. Contracts

### 3.1 Failure reporting (every `catch`)

```csharp
// TheTechIdeaWeb.Diagnostics.Abstractions
public interface IFailureReporter
{
    string ReportHandled(Exception exception, string operation, string consequence,
                         FailureSeverity severity);
}

public enum FailureSeverity { Degraded, Error, Critical }
```

`ReportHandled` returns the **failure reference** — 12 lowercase hex characters
(`FailureReferenceFormat`). That reference is what a person sees, copies and quotes. The exception
text is never shown.

How to write a catch:

```csharp
catch (Exception ex)
{
    var reference = _failures.ReportHandled(
        ex,
        operation: nameof(LoadWellsAsync),
        consequence: Loc.Get("Fail.Consequence.WellsNotLoaded"),
        severity: FailureSeverity.Error);
    return OgResult<OgPage<WellRow>>.Failed(reference);
}
```

Rules that follow from this:

| Rule | Meaning |
|---|---|
| Report, then decide | The catch reports, then returns an explicit outcome **or** rethrows. |
| Reporting ≠ handling | If the caller cannot tell "done" from "dropped", the return type must say so, or rethrow. |
| `Degraded` is earned | Only where the contract was still met and the degradation is written down. |
| Broad `catch (Exception)` needs a reason | A comment: `// Broad by decision: <why>`. |
| Never text to the user | No `ex.Message`, no `ex.ToString()`, no stack in a toast, result, field or log message a person reads. |
| Reporting never throws | If the store is down, degrade to the log line and `LastResortLog`, and still return the reference. |
| Do not catch to keep going | Where work cannot continue correctly, let it fail to the app-wide handler. |

### 3.2 Audit

```csharp
public interface IOgAuditService
{
    void Add(string verb, string subjectKind, string subjectId, string outcome,
             string? detail = null, string? fieldId = null);
}
```

Audit is written **inside the same transaction as the change** where the store allows it, so an
audited change and its audit row cannot diverge. Business audit rows live in the owning store
(`Beep.OilandGas.Repository`, `PPDM39.DataManagement/PPDMAccessAuditService`); security-relevant
events (sign-in, licence, mode switch, permission change) also land in the diagnostics store.

What must be audited: every sign-in success and failure; every permission or role change; every
write to a governed entity; every refusal (authorization, period lock, SoD, licence); every export
of personal or regulated data; every licence import and mode switch.

### 3.3 Logging

Framework `ILogger` only. No Serilog, NLog, log4net. Under the desktop the rolling file is written
to `%LOCALAPPDATA%\TheTechIdea\BeepOilGas\Logs` via `BEEP_LOG_DIR`, which `OgPaths` sets in the
static constructor before any logger is created.

A log line is for diagnosis. It is **not** a substitute for reporting: a catch that only logs is a
defect.

### 3.4 The notification hub and the toast

```csharp
public interface IOgToastService
{
    void Show(OgToast toast);                     // pipeline-facing
    event EventHandler<OgToast>? Shown;
}

public sealed record OgToast(
    OgToastKind Kind,            // Info | Success | Warning | Failure | Audit
    string MessageKey,           // localization key — never a raw string
    object?[]? Args,
    string? Reference,           // required when Kind == Failure
    string? ActionRoute,         // optional "open" target
    TimeSpan Duration);
```

**Feature code must not inject `IOgToastService`.** It is injected only into
`OgNotificationHub`. A feature reports a failure or raises an audit event; the hub decides whether
that becomes a toast and with what severity, duration and action. This is enforced by a guard test
(§6, G-3).

Severity mapping:

| Source | Toast kind | Duration | Sticky | Action |
|---|---|---|---|---|
| `FailureSeverity.Critical` | `Failure` | until dismissed | yes | Open failure detail |
| `FailureSeverity.Error` | `Failure` | 10 s | no | Copy reference |
| `FailureSeverity.Degraded` | `Warning` | 6 s | no | Open affected page |
| Refusal (authorization, period lock, SoD, licence) | `Warning` | 6 s | no | Open policy detail |
| Successful privileged write | `Success` | 3 s | no | Open the record |
| Audit-only event | `Audit` | 3 s, only if the user asked for audit visibility | no | Open audit entry |
| Informational | `Info` | 3 s | no | — |

---

## 4. Desktop adapters

| Adapter | Location | Responsibility |
|---|---|---|
| `OgDiagnosticsPipeline` | `.Host/Services/Diagnostics/` | Implements `IFailureReporter` + `IOgAuditService`; writes the durable store, the log, and publishes to the hub |
| `OgNotificationHub` | `.Host/Services/Diagnostics/` | Maps severity to toast kind; deduplicates; assigns reference; publishes `OgToast` |
| `OgToastService` | `.Host/Services/Diagnostics/` | Thread-safe queue, marshal to the dispatcher |
| `ToastHost` | `.Controls/Notifications/` | The WPF toast surface |
| `NotificationCenterWindow` | `.Host/Shell/` | History, filter, mark read (mirrors the ERP window) |
| `FailurePanel` | `.Controls/` | Inline failed state: sentence, reference, copy, report, retry |
| `OgFailureDetailsWindow` | `.Host/Views/` | One failure: operation, consequence, severity, reference, timestamp, redacted context |
| `OgAdminFailuresView` | `.Modules/Foundation/` | Durable list from the diagnostics store (mirrors Web `Pages/Admin/Failures.razor`) |
| `OgAuditLogView` | `.Modules/Foundation/` | Business audit list (mirrors Web `Pages/Data/Audit.razor`) |
| `OgLogTailView` | `.Modules/Foundation/` | Rolling log tail with level filter |

Store: `Beep.OilandGas.Diagnostics` — `AddOilGasDiagnostics`, `OilGasDiagnosticsDbContext`, already
migrated for SQL Server, PostgreSQL and Oracle. **ADR-013 decides the desktop's store target.**

---

## 5. Toast presentation rules

| Rule | Value |
|---|---|
| Placement | Bottom-right, above the status bar, 24 px margin |
| Width | Min 320, max 480 px; text wraps, never truncates a reference |
| Stack | Max 5 visible; oldest collapses into the notification centre |
| Deduplication | Identical `MessageKey` + `Reference` within 5 s collapses to one toast with a ×n badge |
| Dismissal | Click × or swipe; sticky toasts require an explicit dismiss |
| Auto-dismiss pause | Hover pauses the timer; keyboard focus pauses it |
| Reference | Monospace, selectable, with a copy button |
| Accessibility | `AutomationProperties.LiveSetting="Assertive"` for `Failure`, `Polite` otherwise; the toast never steals focus |
| Keyboard | `Ctrl+Shift+T` focuses the newest toast; `Esc` dismisses the focused one |
| Colour | `BeepDangerBrush` / `BeepWarningBrush` / `BeepSuccessBrush` / `BeepAccentBrush` from the theme tokens only |
| Icon | Mandatory — colour is never the only signal |
| Number | Message text is a `Loc` key; the reference is data, not text |
| Redaction | A toast never contains a credential, token, connection string or full exception text |

---

## 6. Enforcement

These run in CI and fail the build. They are the reason the rule survives contact with 130 features.

| ID | Gate | Kind |
|---|---|---|
| G-1 | No `catch` without a `ReportHandled` call or a rethrow | Analyzer `BEEP0003` + `CatchReportingTests` |
| G-2 | No exception text leaves a catch | Analyzer `BEEP0004` + `ExceptionTextTests` |
| G-3 | `IOgToastService` is injected only into `OgNotificationHub` | `ToastInjectionGuardTests` (Roslyn source scan) |
| G-4 | No `_ =>` arm and no `default: throw` in an enum switch | Analyzers `BEEP0001`/`BEEP0002`, `CS8509` as error |
| G-5 | Every `OgToast` with `Kind == Failure` carries a non-empty `Reference` | `ToastContractTests` |
| G-6 | A toast never renders exception text (a thrown `"secret-should-not-appear"` never reaches a toast) | `ToastRedactionTests` |
| G-7 | Every raw `MessageBox.Show` in `Modules/` and `Controls/` is a build error | `MessageBoxGuardTests` |
| G-8 | Every reported failure is present in the durable store within one flush | `FailureStoreRoundTripTests` |
| G-9 | Every audited command writes exactly one audit row | `AuditCardinalityTests` |
| G-10 | No `ILogger`-only catch (a catch whose body logs and does not report or rethrow) | `CatchReportingTests` |
| G-11 | Every writable path resolves under `OgPaths.DataRoot`, never under `AppContext.BaseDirectory` | `DataRootGuardTests` |
| G-12 | The failure store and log directory are created under the per-user root, verified on a clean account | `StorageLocationTests` |

---

## 7. What every feature inherits

Every feature document's §5 (States) and §6 (Commands) implicitly includes:

1. A **failed** state that renders `FailurePanel` with the reference — never an empty grid, never a
   zero value, never a stale page.
2. A toast for every user-visible failure, produced by the pipeline, not by the view model.
3. An **audit row** for every writing command, and for every refusal.
4. A reference on every failure the person can see, and no exception text anywhere.
5. No `catch` that does not report or rethrow.

A feature document does not need to restate this. It must not contradict it.

---

## 8. Bootstrap order (STG-00)

1. `OgPaths` static constructor sets `BEEP_OILGAS_DATA_ROOT` and `BEEP_LOG_DIR`.
2. `services.AddOilGasDiagnostics(configuration)` — durable store and `IFailureReporter`.
3. `services.AddSingleton<OgNotificationHub>()`, `OgToastService`, `OgAuditService`.
4. Register the app-wide handlers **before** any window is created:

```csharp
DispatcherUnhandledException += OnDispatcherUnhandledException;
AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
```

5. Route all three through `ProcessFailureSubscription`, which reports through the same
   `IFailureReporter`.
6. Only then create the shell, so a failure during shell construction is already reportable.

A failure during startup is the one case that cannot use the toast (no window yet): it shows a
`MessageBox` with the reference only — no exception text. That single `MessageBox` is registered as
an approved exception in G-7.

---

## 9. Store, retention and privacy

| Concern | Decision |
|---|---|
| Store | `OilGasDiagnosticsDbContext` (ADR-013) |
| Location | `%LOCALAPPDATA%\TheTechIdea\BeepOilGas\Diagnostics\failures.db` — resolved only by `OgPaths` |
| Never written to | `AppContext.BaseDirectory`, the install folder, `%ProgramData%`, `%APPDATA%` (roaming), the repository, or the current directory |
| Relocation | Only via `BEEP_OILGAS_DATA_ROOT`, and still under that root |
| Retention | 90 days for `Degraded`/`Error`, 365 days for `Critical`, configurable |
| Redaction | Credentials, tokens, connection strings and request bodies are stripped before insert |
| Length bounds | Message 4 000 chars, detail 8 000 chars, stack 16 000 chars |
| Access | `OgAdminFailuresView` requires `administration.failures.read` |
| Export | Audited; exports themselves are audited events |
| Privacy | Failure records are personal data where they contain a user id; they follow the retention in `Beep.OilandGas.Web/Pages/Legal/Privacy.razor` |
| Directory unavailable | Degrade to the rolling log plus `LastResortLog`; report once; never throw |

---

## 10. Anti-patterns that will be rejected in review

```csharp
// 1. Swallowed
catch { }

// 2. Logs only — still swallowed
catch (Exception ex) { _logger.LogWarning(ex, "load failed"); }

// 3. Exception text to the user
catch (Exception ex) { MessageBox.Show(ex.Message); }

// 4. Feature talks to the toast directly
private readonly IOgToastService _toast;       // forbidden outside the hub

// 5. Success reported when the outcome is unknown
catch (Exception ex) { _failures.ReportHandled(ex, "...", "...", FailureSeverity.Degraded);
                       return OgResult.Ok("Saved."); }

// 6. Enum switch with a discard arm
return kind switch { A => 1, B => 2, _ => throw new ArgumentOutOfRangeException() };
```

Each has a gate above that fails the build or the test.

---

## 11. Definition of Done (adds to PLAN-02 §5)

A feature is `Gate met` only when:

- Every `catch` reports or rethrows (G-1, G-10).
- No exception text reaches any surface (G-2, G-6).
- Every failure produces a toast with a reference (G-5).
- Every writing command produces exactly one audit row (G-9).
- The failed state renders `FailurePanel` (see §7.1).
- No `IOgToastService` injection outside the hub (G-3).
- The toast was observed once for `Failure`, `Warning` and `Success`.

---

**Document Version:** 1.0 · **Last Updated:** 2026-11-05
