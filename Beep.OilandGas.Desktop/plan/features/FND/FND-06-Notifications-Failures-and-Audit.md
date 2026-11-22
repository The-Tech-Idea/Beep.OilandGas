# FND-06 — Notifications, failure reporting, audit and toast pipeline

| Field | Value |
|---|---|
| Stage | STG-00 — Foundation and host shell |
| Area | Foundation |
| Status | Gate met - AC-1 to AC-12, including the rolling log and both halves of AC-7. Evidence: [FND-06](../../evidence/STG-00-FND-06/observed-evidence.json) |
| Owner projects | `TheTechIdeaWeb.Diagnostics.Abstractions`, `TheTechIdeaWeb.Diagnostics`, `Beep.OilandGas.Diagnostics`, `Beep.OilandGas.Repository` |
| Desktop project | `src/Beep.OilandGas.Desktop/Services/Diagnostics/`, `src/Beep.OilandGas.Desktop.Controls/Notifications/` |
| External packages | `Microsoft.Extensions.Logging` |
| Depends on | FND-02 |
| Blocks | **every feature** |
| Web reference | `TheTechIdeaWeb.Diagnostics` usage; `Pages/Admin/Failures.razor`; ERP `ErpNotificationService`, `NotificationCenterWindow` |
| Authority | [`PLAN-07`](../../07-LOGGING-AUDIT-AND-NOTIFICATIONS.md) |

## 1. Purpose

Be the one place the application records and shows anything: every log, every exception, every audit
message arrives here, is made durable, and — when a person needs to know — appears as a toast that
carries a reference and nothing more.

This feature is not optional infrastructure. **No other feature may show an error, write a log
message as its only response to a failure, or raise a notification by any other route.**

## 2. Source components

| Source | Reuse |
|---|---|
| `TheTechIdeaWeb.Diagnostics.Abstractions` `IFailureReporter`, `FailureSeverity`, `FailureReferenceFormat`, `LastResortLog`, `ProcessFailureSubscription`, `ReportsFailureAttribute` | the contracts every `catch` uses |
| `TheTechIdeaWeb.Diagnostics` store, handlers, boundaries, notifier, retention | durable record and notifier |
| `TheTechIdeaWeb.Diagnostics.Device` `DeviceFailureReporter`, `FailureOutbox` | optional forwarding in `CompanyApi` mode |
| `Beep.OilandGas.Diagnostics` `AddOilGasDiagnostics`, `OilGasDiagnosticsDbContext` | local failure store at `%LOCALAPPDATA%\TheTechIdea\BeepOilGas\Diagnostics\failures.db` (ADR-013). Note: the project ships SQL Server / PostgreSQL / Oracle migrations only — a SQLite set must be added, or `EnsureCreated` chosen (ADR-013 sub-task). |
| `Beep.OilandGas.Repository` `RepositoryAccessService`, `AuditChainService` | business audit and actor resolution |
| `Beep.OilandGas.PPDM39.DataManagement/Services/PPDMAccessAuditService.cs` | governed-entity read/write audit |
| ERP `Services/ErpNotificationService.cs`, `Views/NotificationCenterWindow.xaml` | notification centre behaviour to reproduce |
| `Beep.IdentityServer.Beep.IdentityServer.Analyzers` | `BEEP0001..0004` build-time gates |

## 3. Screens

- **`ToastHost`** — the live surface. Bottom-right stack, max 5, worst on top, icon + localized
  message + copyable reference + optional action + dismiss. Sticky for `Critical`, paused on
  hover/focus, `Assertive` live setting for failures.
- **`NotificationCenterWindow`** — history, severity filter, mark read, open the related page.
- **`FailurePanel`** — inline in every page's failed state: consequence sentence, reference, copy,
  report, retry.
- **`OgFailureDetailsWindow`** — one failure: operation, consequence, severity, reference, timestamp,
  redacted context. **No stack trace shown.**
- **`Foundation → Failures`** — the durable list from the diagnostics store, mirroring
  `Pages/Admin/Failures.razor`: filters, resolve, retention state, forwarding state.
- **`Foundation → Audit log`** — business audit list, mirroring `Pages/Data/Audit.razor`.
- **`Foundation → Log tail`** — rolling log with level filter (`LogViewerWindow`).

## 4. Data and contracts

```csharp
// Contracts come from TheTechIdeaWeb.Diagnostics.Abstractions — never re-declared.
public interface IFailureReporter
{
    string ReportHandled(Exception exception, string operation, string consequence,
                         FailureSeverity severity);
}

public enum FailureSeverity { Degraded, Error, Critical }

// Desktop adapter — implements both, publishes to the hub
public sealed class OgDiagnosticsPipeline : IFailureReporter, IOgAuditService
{
    public string ReportHandled(Exception exception, string operation, string consequence,
                                FailureSeverity severity);
    public void Add(string verb, string subjectKind, string subjectId, string outcome,
                    string? detail = null, string? fieldId = null);
    public event EventHandler<OgDiagnosticEvent>? Published;
}

// Only OgNotificationHub injects this interface.
public interface IOgToastService
{
    void Show(OgToast toast);
    event EventHandler<OgToast>? Shown;
}

public sealed record OgToast(
    OgToastKind Kind,          // Info | Success | Warning | Failure | Audit
    string MessageKey,         // Loc key — never a raw string
    object?[]? Args,
    string? Reference,         // required when Kind == Failure
    string? ActionRoute,
    TimeSpan Duration);
```

Startup order is mandatory and lives in `OgStartupSequence`:

1. `OgPaths` static constructor sets `BEEP_OILGAS_DATA_ROOT` and `BEEP_LOG_DIR`.
2. `AddOilGasDiagnostics(configuration)` — the store file is `OgPaths.FailureStorePath`, which is
   `%LOCALAPPDATA%\TheTechIdea\BeepOilGas\Diagnostics\failures.db` and **can never resolve beside the
   executable** (ADR-013, guard G-11).
3. Register `OgDiagnosticsPipeline`, `OgNotificationHub`, `OgToastService`, `OgAuditService`.
4. Subscribe `DispatcherUnhandledException`, `AppDomain.CurrentDomain.UnhandledException`,
   `TaskScheduler.UnobservedTaskException` → `ProcessFailureSubscription`.
5. Only then construct the shell.

## 5. States

| State | Behaviour |
|---|---|
| Caught exception | reported; reference returned; toast raised; failed state rendered by the caller |
| Uncaught exception | app-wide handler reports through the same pipeline; the app does not continue in an unknown state |
| Failure store unavailable | degrade to the rolling log plus `LastResortLog`; the reference is still returned and still shown |
| Log directory not writable | reported once; the app continues with the store only |
| Duplicate toast | collapses within 5 s with a ×n badge |
| Critical | sticky until dismissed; cannot be auto-dismissed by a timer |
| Startup failure (no window) | one approved `MessageBox` with the reference only |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Copy a reference | any signed-in user | no |
| Retry an operation | the originating page's command permission | as the origin |
| Resolve a failure record | `administration.failures.write` | store |
| View the failure list | `administration.failures.read` | no |
| View the audit log | `administration.audit.read` | no |
| Export failures or audit | `administration.audit.export` + audited itself | file |

## 7. Acceptance criteria

- AC-1 Every `catch` in desktop code reports through `ReportHandled` or rethrows (guard + analyzer).
- AC-2 No `catch` body consists only of a log call.
- AC-3 Every user-visible failure produces a toast carrying a 12-lowercase-hex reference.
- AC-4 No exception message, `ToString()` or stack trace reaches a toast, a `FailurePanel`, a result
  object or a field.
- AC-5 An exception whose message contains a sentinel string never renders that sentinel anywhere.
- AC-6 A reported failure is present in the durable store and in the rolling log.
- AC-7 An unobserved task exception and an unhandled dispatcher exception both reach the reporter.
- AC-8 `IOgToastService` is injected only into `OgNotificationHub` (guard).
- AC-9 A writing command writes exactly one audit row; a refusal writes one too.
- AC-10 `Critical` toasts are sticky; `Error` auto-dismisses at 10 s.
- AC-11 When the store is down, the reference is still returned and still shown.
- AC-12 The startup `MessageBox` is the only `MessageBox` outside `Dialogs/` (guard).

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~FND-06"
```

| Test | Proves |
|---|---|
| `FND-06/T01` | A caught exception returns a 12-hex reference |
| `FND-06/T02` | The reference is present in the store |
| `FND-06/T03` | A failed read renders `FailurePanel` and raises one toast |
| `FND-06/T04` | The toast text contains the reference and not the exception message |
| `FND-06/T05` | A sentinel exception message never appears in any surface |
| `FND-06/T06` | The failure exists in both the store and the log |
| `FND-06/T07` | `TaskScheduler.UnobservedTaskException` reaches the reporter |
| `FND-06/T08` | Store failure degrades without throwing back into the catch |
| `FND-06/T09` | Exactly one audit row per write and per refusal |
| `FND-06/T10` | Duplicate toasts collapse within the window |
| `FND-06/T11` | A logger-only catch fails `CatchReportingTests` |
| `FND-06/T12` | `ToastInjectionGuardTests` fails when a feature injects the toast service |

Blind spot: the store-down test substitutes the store; it does not prove behaviour against a real
locked database file. Record that limit in the evidence.

## 9. Open questions and risks

- **ADR-011, ADR-012, ADR-013 must be accepted before this feature is coded.**
- `TheTechIdeaWeb.Diagnostics` is a sibling-repo package built for web hosting. Its store assumes an
  app database; ADR-013 routes the desktop to a dedicated local SQLite file at
  `%LOCALAPPDATA%\TheTechIdea\BeepOilGas\Diagnostics\failures.db` — never beside the executable.
- **Blocker:** `OilGasDiagnosticsDbContext` has no SQLite migration set. Choose option A/B/C from
  ADR-013 before coding, and record it in PLAN-07 §9.
- WPF has no Blazor `ErrorBoundary`. The equivalent is a `DispatcherUnhandledException` handler plus
  a per-page try/report boundary in the view-model base class. Both are required; neither alone is
  sufficient.
- `ProcessFailureSubscription` was written for a host with a request lifetime. Confirm it works with a
  long-lived WPF process, or subscribe directly with the same reporter.

### CompanyApi forwarding — delivered 2026-11-05

- `OgFailureRelay` subscribes to `OgDiagnosticsPipeline.Published`, the same seam the toast hub uses, so
  the local record is durable before any forwarding is attempted.
- `OgFailureForwarder` writes into the shared `FailureOutbox` under
  `%LOCALAPPDATA%\TheTechIdea\BeepOilGas\Outbox` and delivers through the library's `FailureDelivery`
  (`/api/client-failures`), preserving the reference and forwarding only the **redacted** stored fields —
  the exception's `ToString()` never leaves the machine.
- `FORWARDED` is set only after the intake accepts the batch, so the flag means "the API has it". A 400
  discards the report (no retry changes it) but does **not** mark it forwarded.
- A queued report survives a restart and is delivered by the next session; eight tests cover the
  disabled, queued, redacted, delivered, unreachable, refused, restarted and local-install cases.
- Registered only when an API base URL and a usable token are configured; otherwise
  `DisabledFailureForwarder`, and the smoke run asserts forwarding is off.

### Still open

- The failures and audit admin pages are built (see above) and their rows are unit-tested; they were not
  clicked through in a shown window.
- Forwarding is enabled at startup; switching to `CompanyApi` during a session needs a restart.

### Rolling log, dispatcher failures, AC-6 and AC-7 — delivered 2026-11-05

- `OgFileLoggerProvider` writes one log file per day under `%LOCALAPPDATA%\TheTechIdea\BeepOilGas\Logs`
  (ADR-013). The pipeline's failure line carries the reference, so the log and the store can be joined by
  it. A provider that failed into its caller would turn a recorded failure into an unrecorded one; the
  write failure goes to `LastResortLog`, which the analyzer accepts without a suppression.
- **AC-6** is now real: `A_reported_failure_is_in_the_store_and_in_the_file_log` asserts both, and the
  smoke run reports `log-file-written True` and `log-carries-reference True`.
- **AC-7 (dispatcher half):** `OgDispatcherFailureHandler` holds the rule — report, mark handled, show only
  the reference. It is a class rather than an inline `catch` so it can be tested without a window;
  `An_exception_escaping_a_user_action_is_reported_handled_and_shown_only_as_a_reference` proves the
  report, the reference shown, and that a secret in the exception never reaches either.
- **AC-7 (unobserved half):** already delivered — a task started and never awaited reaches the reporter.

### Admin pages and AC-7 — delivered 2026-11-05

- `AdminFailuresView` reads the local SQLite rows (newest first) and shows **Reference · When · Severity ·
  Operation · Consequence · Forwarding · User**, with a summary line (`N record(s) · X forwarded · Y pending
  delivery`, plus outbox depth) and a Refresh action.
- The forwarding column is derived, not stored: `OgFailureRow.From(record, forwardingEnabled)` yields
  `Forwarded`, `Pending delivery` or **`Local only`** — so an installation with no API never claims to be
  queueing anything.
- `AuditLogView` shows **When · Verb · Subject · Id · Outcome · Detail · User** with a refusal count.
- Both pages are projections over the stores, so the rules are unit-tested without a window
  (`FailureRowTests`), and the smoke run proves they read real rows: `failures-page-rows 1`,
  `audit-page-rows 1`, `failures-page-forwarding-label Local only`.
- **AC-7:** `An_unobserved_task_exception_reaches_the_reporter` starts a task that throws with nothing
  awaiting it, forces collections until the finalizer raises `TaskScheduler.UnobservedTaskException`, and
  asserts one report with operation *"running a task nothing awaited"*.
  `A_disposed_subscription_reports_nothing_more` proves the subscription unsubscribes, so a test run of
  many hosts cannot report into a disposed one.
