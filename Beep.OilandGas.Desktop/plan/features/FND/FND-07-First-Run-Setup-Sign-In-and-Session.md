# FND-07 — First-run setup, sign-in and session

| Field | Value |
|---|---|
| Stage | STG-00 — Foundation and host shell |
| Area | Foundation |
| Status | Gate met - AC-1..AC-7, with ADR-014 changing how local passwords are protected. See [evidence](../../evidence/STG-00-FND-07/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.Repository`, `Beep.OilandGas.UserManagement`, `Beep.OilandGas.Client` |
| Desktop project | `src/Beep.OilandGas.Desktop/Setup/`, `Views/` |
| External packages | `System.Security.Cryptography.ProtectedData` |
| Depends on | FND-02 |
| Blocks | every page |
| Web reference | `Beep.OilandGas.Web/Pages/InitialCompanySetup.razor`, `CompanyMembershipSetup.razor`, `CompanyEnvironmentSetup.razor`, `Components/Setup/FirstRunSetupGuide.razor`; ERP `FirstRunWindow`, `LoginWindow` |

## 1. Purpose

Get a new installation from "installed" to "signed in with a usable company, field and
database", and get an existing installation back to usable when the database or API is
unreachable.

## 2. Source components

| Source | Reuse |
|---|---|
| `Beep.DoitAll.Erp.Wpf/Setup/FirstRunWindow.xaml(.cs)` | wizard shell, step model, repair path |
| `Beep.DoitAll.Erp.Wpf/Setup/ErpSetupStore.cs`, `ErpSetupProfile.cs`, `ErpDeploymentSettings.cs` | persisted setup state |
| `Beep.DoitAll.Erp.Wpf/Setup/RegionalCatalog.cs` | country, currency, culture, time zone |
| `Beep.OilandGas.Repository/RepositoryBootstrapService.cs` | create the RBAC store |
| `Beep.OilandGas.UserManagement/Services/DefaultSecuritySeedService.cs` | 19 personas, roles, permissions, first-admin rule |
| `Beep.OilandGas.Client/Authentication/IAuthenticationProvider.cs` | remote token path |
| ERP `Security/ErpLocalIdentityService.cs`, `ErpLocalCredentialStore.cs`, `ErpPasswordPolicy.cs` | local identity path |
| `Beep.OilandGas.Web/Pages/CompanyEnvironmentSetup.razor` | country environment selection (with INT-05) |

## 3. Screens

`FirstRunWindow` steps:

1. Welcome and deployment mode (`Personal` / `CompanyDatabase` / `CompanyApi`).
2. Database (local file, or SQL Server/PostgreSQL/Oracle connection) — reuses DAT-01 controls.
3. Company identity (legal name, code, country, currency, culture, time zone, fiscal year).
4. Field and asset scope (at least one field).
5. Country environment (delegates to INT-05; optional).
6. Module setup — runs DAT-02 with progress and resume.
7. First administrator (name, email, username, password, policy validation).
8. Review and finish.

`SignInWindow`: username, password, domain notice (local vs identity provider), remember me,
language, and a "repair installation" link that reopens `FirstRunWindow`.

## 4. Data and contracts

`OgSetupState { Completed, Profile, Deployment, Mode, ConnectionName, CompletedSteps[] }`
persisted at `%LOCALAPPDATA%\TheTechIdea\BeepOilGas\setup.json`.

`OgSession { ConnectionName, CompanyId, CompanyName, FieldId, FieldName, UserId,
UserDisplayName, CultureName, TimeZoneId, BaseCurrencyCode, DeploymentMode, PersonaCode }`.

## 5. States

| State | Behaviour |
|---|---|
| Setup incomplete | shell cannot open; wizard opens |
| Setup complete but database missing | repair wizard; shell cannot open |
| Wrong password | failure, no session, no user enumeration |
| API unreachable at sign-in | clear message, retry, or switch to local |
| First administrator already exists | step 7 shows the existing admin, does not create a second |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| Complete setup | local operator (physical machine access) |
| Reset a password | Administrator, or self-service if IdentityServer provides it |
| Switch deployment mode | Administration + local confirmation |

## 7. Acceptance criteria

- AC-1 The shell cannot open while setup is incomplete.
- AC-2 A repair run fixes a missing local database without losing setup state.
- AC-3 The first administrator is created exactly once; a second attempt is refused.
- AC-4 Password policy is enforced before any credential is stored.
- AC-5 Local credentials are encrypted with DPAPI and never written in plain text.
- AC-6 Every sign-in attempt, success and failure, is audited.
- AC-7 The persona from the RBAC store sets the landing route.

## 8. Verification

```
dotnet run --project src/Beep.OilandGas.Desktop -- --smoke
```

The smoke run performs setup, sign-in, permission resolution and writes each step to
`smoke-result.txt`. Tests: `FND-07/T01`, `FND-07/T02`, `FND-07/T03`.

## 9. Open questions and risks

- Local mode and IdentityServer mode have different failure modes; both must be exercised.
- The 19 personas come from `DefaultSecuritySeedService`; confirm it runs in local mode or the
  desktop must seed them itself.
- Store the API token with DPAPI, never in `setup.json`.

### Delivered 2026-11-05

- `OgLocalCredentialStore` — PBKDF2-SHA256, 210 000 iterations, random 16-byte salt, constant-time
  comparison, at `%LOCALAPPDATA%\TheTechIdea\BeepOilGas\credentials.json` (ADR-014). The file has no
  password field, and a test asserts the password string never appears in it.
- `OgLocalIdentityService` — 12-character policy with four named rules, the first administrator created
  exactly once, and a wrong password and an unknown user produce the **same** sentence so the window
  cannot be used to enumerate accounts.
- `OgSignInService` — audits every attempt (one row, outcome `ok` or `refused`), builds the session the
  shell runs under from the recorded company, and updates `OgRuntimeContext` so later failure and audit
  rows carry who and where. Sign-out is audited and clears the actor.
- `OgSetupStore` + `OgStartupGate` — the setup state round-trips under the data root; the gate decides
  `RunSetup` / `SignIn` / `Ready`, and treats "setup flag set but no account" as `RunSetup`, so a wiped
  credential file cannot reach the shell.
- `FirstRunWindow` and `SignInWindow` — no exception text reaches the person; a failure shows a reference.
- `App.OnStartup` consults the gate before constructing `OgShellWindow`, and the guard
  `The_shell_is_created_only_after_the_startup_gate` fails if that order is reversed (proven by
  temporarily moving the shell creation above the gate).

### AC-7 persona landing - delivered 2026-11-06

- `OgPersonaLanding` maps the **RBAC role set** to a landing route, in priority order:
  Administrator/TenantAdmin -> `dashboard`, DrillingEngineer -> `wellpath`, ReservoirEngineer ->
  `correlation`, ProductionEngineer -> `logs`, HseOfficer -> `charts`, Accountant -> `export`,
  Viewer -> `dashboard`.
- The **more privileged persona wins** whatever order the store returns roles in, so the landing is
  deterministic rather than dependent on list order.
- The chosen route must pass the **same guard the navigation uses** (`OgNavigationService.CanAccess`). A page
  that cannot be opened falls back to `dashboard` with `IsFallback` true and a sentence naming the persona and
  the route that was unavailable - the shell never opens a route a moment before refusing it.
- The shell selects the landing after loading (`LandedRoute`) and writes **one audited `landing` row** with the
  persona, the roles and the reason, so "why did it open here?" is answerable later.
- `OgSession.PersonaCode` is no longer a placeholder: it is resolved from the roles at bootstrap
  (`OgPersonaLanding.PersonaFor`) and from the local administrator role at sign-in.

### Still open

- **Only the administrator persona was proven end to end.** The other personas were proven by resolving
  through the same guard the navigation uses, not by launching a shell with that role set.
- Local mode still grants the administrator role from the bootstrap; the store that will supply real role
  sets is the administration matrix, which is not built.
- **Deviation recorded.** AC-5 named DPAPI; ADR-014 chooses a one-way PBKDF2 derivation instead, because
  DPAPI ciphertext is reversible by the same Windows account.

### AC-2 repair run — delivered 2026-11-05

- `OgRepairService.Assess()` reports repair as needed only when **all** of these hold: `setup.json` says
  complete, an administrator exists, and the local database file is absent. Without an account it says
  "the first-run wizard owns this installation", so a repair can never become a second way to create an
  administrator.
- `RepairAsync(IOgConnectionCatalog)` re-provisions through the same `EnsureLocalAsync` path DAT-01 uses
  and writes one `repair` audit row. It touches nothing else: `setup.json` and `credentials.json` are
  byte-identical after a repair, which the test asserts rather than assumes.
- `OgStartupGate.Decide(..., repairNeeded)` returns `Repair` **after** the setup/account check and
  **before** sign-in; `App.OnStartup` re-evaluates the gate after the wizard, after the repair and after
  sign-in, because each step changes the facts.
- `RepairWindow` shows what is missing and what will happen ("the database file will be recreated empty;
  the company, the administrator account and every setting are kept"), and reports a failure by reference.
- The catalog is a **method parameter**, not a constructor dependency: resolving it starts BeepDM and loads
  every assembly (~8 s), and the assessment runs on every startup. Only an actual repair pays that.
- **Finding:** SQLite keeps the database file open while a provider is connected, so a repair can only
  happen when no runtime holds it — which is exactly the between-sessions case the feature is for. The
  test disposes the first runtime before deleting the file.
