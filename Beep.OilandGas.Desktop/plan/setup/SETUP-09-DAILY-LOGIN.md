# SETUP-09 — Every start: authenticate, then read the settings

**Document ID:** SETUP-09
**Status:** Built
**Actor:** User
**Level:** Company

---

## 1. Purpose

Describe what happens from the moment a person opens the application to the moment the shell appears, for both
deployments — including what is *not* read before sign-in.

This is the stage that had the one real ordering defect of the previous implementation: company settings were read
before anybody had authenticated. The order below is the corrected one and is enforced in code.

## 2. Actor

The user. The installer's machine facts are already recorded; the user contributes only an account and a password.

## 3. Preconditions

- SETUP-01/02 recorded and proved the connection (or the personal file exists).
- SETUP-03/08 created or adopted the company and the person's account.

## 4. Steps

1. **Gate.** `OgStartupGate.Decide` looks at: setup complete? accounts? signed in? repair needed?
   - Personal: "accounts" means the local credential file has a user.
   - Company: the account directory is the database, so the gate sends the person to sign-in and lets the sign-in
     say if the directory is empty.
2. **Sign-in.** The window shows the connection, not a company name (which cannot be known before authentication),
   and two fields. Both deployments authenticate against their database — a local file or a server:
   - Both deployments verify against the database's `USER` table (PBKDF2); a personal installation's database is a local file.
   - Company: `BeepDmDatabaseIdentity` verifies against the company database's `USER` table and reads the roles
     from `USER_ROLE` → `ROLE`.
3. **Roles.** The signed-in role set replaces the workstation default; `OgApplication.Permissions` is rebuilt from
   it, and the database's `Administrator`/`Admin` also grants the application's `SystemAdmin`.
4. **Settings, now and only now.** `OgApplication.LoadSettingsAsync` requires `IsSignedIn` and reads the
   installation's database; there is no local copy to fall back on:
   - Personal: the workstation's own record is authoritative; no database read.
   - Company: requires `IsSignedIn` — otherwise it returns `settings-not-authenticated`. It opens the recorded
     connection, reads `BEEPSETTINGS` (`COMPANY`, `TOOLS_ENVIRONMENT`, `MODULES`), adopts the company into the
     session in memory, applies the interface language, and loads the shared environment.
5. **Failure does not block.** If the company settings cannot be read (server down, record unreadable) the shell
   still opens, a notice carries the reference, and the settings page says what is unavailable. The person is not
   shown a company that the application invented.
6. **Provisional password.** If the account the setup created has not yet replaced its password, a change-password
   window sits between sign-in and the shell; cancelling signs out. The shell is not created until it is done.
7. **Shell.** Created only after the gate, the sign-in and (when required) the password change; the timeline marks
   `gate` and `shell-shown`.

## 5. Records written

| Store | What | When |
|---|---|---|
| Audit store | `sign-in` row, `ok`/`refused`, with the roles granted | every attempt |
| — | nothing on disk | the company values live only in the running session |
| Failure store | a `Degraded` record when the settings could not be read | on failure |

## 6. Failure states

| Code | Meaning | What the person sees |
|---|---|---|
| `sign-in-refused` | wrong password / unknown user / inactive / seeded principal | one sentence, password cleared |
| `sign-in-no-accounts` | the directory has no login account | "no account exists; ask the administrator" |
| `sign-in-security-tables-missing` | the database has no RBAC tables | the database is not set up |
| `sign-in-database-unreachable` | the server cannot be reached | the connection error, with the reference |
| `settings-not-authenticated` | settings asked for before sign-in (a programming error) | never shown; the guard returns it |
| `settings-table-missing` | joined a database with no settings table | "no company has been created in it" |
| `settings-read-failed` | provider failure reading the document | the reference; the shell still opens |

## 7. Security requirements

- **Authenticate before reading** (OWASP Multi-Tenant §1/§3): the company is derived from the authenticated
  session's connection, never from typed input.
- **One generic refusal** (OWASP Authentication): wrong password, unknown user and disabled account share a
  sentence, and a seeded principal is refused identically.
- **No secrets on the shell start path:** the settings documents hold no credential; the connection password never
  leaves the connection store.
- **Audited:** every attempt is recorded, accepted or refused.
- **Fail visible, not silent:** a settings failure is a recorded failure with a reference, and the shell says so.

## 8. UX requirements

- The sign-in window names the company (so a person on the wrong server notices).
- The password field is cleared and focused after a refusal.
- The wait during the settings read is visible (wait cursor) and bounded; a 30-second connection timeout is the
  ceiling.
- The settings page states the source and when it was read.

## 9. Acceptance criteria

| # | Criterion | Status / evidence |
|---|---|---|
| AC-1 | No company setting is read before a successful sign-in | **Met** — `LoadSettingsAsync` guard; `App.OnStartup` order; evidence in the tracker entry |
| AC-2 | Personal sign-in still works unchanged | **Met** — a personal installation signs in against its own local database through the same `BeepDmDatabaseIdentity`; `SetupAndSignInTests`, `AccountPickerTests` |
| AC-3 | Company sign-in authenticates against the database and yields its roles | **Met** — `DatabaseIdentityTests` |
| AC-4 | A failed settings read does not stop the shell | **Met** — `App.OnStartup` publishes a notice and continues |
| AC-5 | A refused attempt is audited exactly once | **Met** — `SetupAndSignInTests.A_refused_sign_in_writes_one_audit_row_and_creates_no_session` |
| AC-6 | The sign-in screen can list the accounts instead of requiring a typed name | **Met** — `ListUserNamesAsync` fills an editable picker; `AccountPickerTests` (5) |
| AC-7 | Account lockout after repeated failures | **Met** — `OgSignInPolicy` + `SignInLockoutTests` (5); an administrator can clear a lock |

## 10. What exists today and what is missing

Exists: `Setup/OgStartupGate.cs`, `Views/SignInWindow.cs`, `Security/OgSignInService.cs`,
`Data/Security/OgDatabaseIdentity.cs`, `Bootstrapping/OgApplication.cs`, `App.xaml.cs`.

Missing: nothing this stage asks for. A settings failure publishes a notice with its reference and the Settings
page states what is unavailable; there is no separate always-on banner, and none is claimed.
