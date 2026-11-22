# SETUP-03 — Company creation and the first SystemAdmin

**Document ID:** SETUP-03
**Status:** Built
**Actor:** Installer, then the person who becomes SystemAdmin
**Level:** Company

---

## 1. Purpose

Create the company in the company database and the first account that can administer it, in one operation. After
this stage the installation has an identity: a company record, an account directory with one administrator, and the
schema the modules need.

This is the only stage that creates company-wide facts. It runs **once** per company database. A workstation that
joins an existing company does not run it (SETUP-08).

## 2. Actor

The installer starts it; the person named as administrator is the one who will sign in afterwards. On a company
database that person is the **SystemAdmin** — the only role that may change settings, the module set, the
environment and the RBAC.

## 3. Preconditions

- SETUP-01 recorded `CompanyDatabase` (or the personal path, which uses the same steps against the local file).
- SETUP-02 proved the connection opens and lists tables.
- The database is either empty or belongs to the same company; a database that already holds a company record is
  **adopted**, not overwritten (SETUP-08).

## 4. Steps

1. **Apply the module set** the person ticked (SETUP-06). SECURITY is always included: it creates `USER`, `ROLE`,
   `PERMISSION`, `USER_ROLE`, `ROLE_PERMISSION` and the scope/persona tables, and seeds the default roles and
   permissions. Without it no account can exist.
2. **Write the company record** into `BEEPSETTINGS` (`COMPANY`) — name, code, country, culture, time zone, currency,
   fiscal-year start, company type, unit system, default field, who recorded it and when (SETUP-04).
3. **Write the shared environment** into `BEEPSETTINGS` (`TOOLS_ENVIRONMENT`) — the country preset and unit system
   (SETUP-07).
4. **Record the module set** in `BEEPSETTINGS` (`MODULES`) so a later workstation knows what this database contains
   (SETUP-06).
5. **Create the SystemAdmin** in the company database: a `USER` row with a PBKDF2-SHA256 password envelope, and a
   `USER_ROLE` row granting the seeded administrator role. Never a default password, never a seeded principal that
   can log in.
6. **Read the installation back**: the probe re-opens the database and the gate now sees a company and an account.
7. **Finish** by loading the environment from `TOOLS_ENVIRONMENT` and reporting what was created.

Order matters: the settings row and the account are written **after** the schema exists, because both are rows in
tables the module set creates.

## 5. Records written

| Store | Table / file | Row |
|---|---|---|
| Company database | `BEEPSETTINGS` | `COMPANY`, `TOOLS_ENVIRONMENT`, `MODULES` |
| Company database | `USER` | the administrator, `PASSWORD_HASH = pbkdf2-sha256$210000$…` |
| Company database | `USER_ROLE` | that user ↔ the administrator role |
| Company database | `SETUP_WIZARD_LOG` (security module) | the module run, by the module's own seeder |
| Machine | the connection definition only | nothing about the company |

The password is never written in clear text anywhere, and there is no `setup.json` or `credentials.json`: the account,
the company record and the settings are rows in the database, for both deployments.

The sequence is owned by `Data/Setup/OgCompanyCreation.cs`, which records what it wrote and **undoes it** when a later
step fails (AC-5).

## 6. Failure states

| Code | When | What happens | Recovery |
|---|---|---|---|
| `sign-in-security-tables-missing` | SECURITY did not run | the administrator is refused | tick the security module and run Review again |
| `administrator-role-missing` | the ROLE table has no administrator role | refused | run the security module's seed again |
| `administrator-exists` | the user name is already in the database | the existing account is **adopted**, not recreated | that person signs in with their own password |
| `settings-record-changed` | another workstation wrote the company record between read and write | refused | reload and re-apply |
| `settings-table-not-created` | `BEEPSETTINGS` could not be created | refused, nothing written | check the service account's DDL rights |
| `settings-table-not-created` | `BEEPSETTINGS` could not be created | refused, nothing written | check the service account's DDL rights |
| module failure | a module's schema or seed refused | the outcome names the module and its reason | fix, then resume (the runner keeps per-module state) |

**Known gap (G-2):** a failure after the `COMPANY` row exists leaves a company with no administrator. The row is
recorded, and a failure part-way is rolled back: `OgCompanyCreation` records what it wrote and reverses it.

## 7. Security requirements

- **No default credentials.** Nothing is seeded that can sign in; the security module's principals carry
  `seeded-no-login` and `BeepDmDatabaseIdentity` refuses them explicitly.
- **Length-based password** (OWASP/NIST): 15 characters minimum, 64 maximum, no character-class rules, common and
  digits-only passwords refused. One policy for the workstation store and the company database
  (`OgPasswordPolicy`).
- **The bootstrap account is provisional.** The setup writes a `BOOTSTRAP` marker row; the first sign-in reports
  it, and the shell does not open until the owner has replaced the password. Replacing it re-hashes the password and
  deletes the marker, so the password typed during setup stops working (Keycloak's bootstrap-admin rule). An account
  the workstation adopted is **not** marked: it already has its owner's password.
- **Enumeration.** A refusal says "the user name or the password was not accepted" for a wrong password, an unknown
  user and a principal alike.
- **All-or-nothing** (OWASP Multi-Tenant §7): AC-5, proved by `CompanyCreationTests`.
- **Audit.** The module run is logged by the security module's own seeder; the company record carries
  `RECORDED_BY` and `RECORDED_AT_UTC` inside its document; the creation is written to the application's audit trail.

## 8. UX requirements

- The step list shows where the person is and what remains (NN/g: communicate the mental model).
- The Review step states what will be created, in the order it will be created.
- The administrator step says **where** the account will exist ("in the company database" vs "on this computer").
- A failure says which step failed and what to do; it never leaves the wizard on a blank status.

## 9. Acceptance criteria

| # | Criterion | Status |
|---|---|---|
| AC-1 | One run creates the company record and the SystemAdmin, and the person can sign in with the password typed there | **Met** — `DatabaseIdentityTests.An_administrator_is_created_in_the_database_and_can_sign_in_with_the_roles_it_grants` |
| AC-2 | No account that can sign in is seeded; the security module's principals are refused | **Met** — `DatabaseIdentityTests.A_seeded_principal_is_not_a_login_account` |
| AC-3 | A database that already has a company is adopted, not overwritten, and an existing user is adopted | **Met** — `WriteSharedSettingsAsync` (adopt branches), `CreateSystemAdministratorAsync` refusal |
| AC-4 | The first administrator is provisional: marked, and required to change its password at first sign-in | **Met** — `OgBootstrapMarker` + `BEEPSETTINGS/BOOTSTRAP`, `MarkPasswordChangeRequiredAsync`, `ChangePasswordAsync`, `Views/ChangePasswordWindow`; `BootstrapPasswordTests` (5 tests: marked, adopted-not-marked, weak refusal, old password dead after the change, marker row deleted) |
| AC-5 | A stage that fails removes what it partly created and records the failure | **Met** — `OgCompanyCreation.RollBackAsync`; `CompanyCreationTests` prove both the settings rollback and that a module failure writes nothing |
| AC-6 | An interrupted setup resumes without re-entering accepted values | **Met** — each accepted step is written to `BEEPSETTINGS/SETUP_PROGRESS`; the wizard opens at the step after it and the rail marks the rest done; the record is cleared when the installation exists; `WizardResumeTests` |
| AC-7 | The wizard enforces the order of its steps | **Met** — `OgSetupSteps.MaySelect` and the rail's selection handler; `SetupStepOrderTests` (7) |
| AC-10 | The wizard shows completed, current and upcoming steps | **Met** — the tab strip became a `ListBox` rail labelled `n. Name - done/here/`, driven by `OgSetupSteps.StateOf`/`LabelFor`; `WizardResumeTests` (9) |
| AC-9 | No setting is written to a file by setup | **Met** — the store and its type were removed; the smoke run proves the gate from the database |
| AC-8 | The password rule is one rule for both account stores and follows OWASP/NIST | **Met** — `OgPasswordPolicy`, `PasswordPolicyTests`, `DatabaseIdentityTests.The_password_policy_is_one_rule_for_every_account_store` |

## 10. What exists today and what is missing

Exists:

- `Setup/FirstRunWindow.cs` — the six steps, the review sequence, module apply, shared-record writes, administrator
  creation, the all-or-nothing rollback.
- `Data/Security/OgDatabaseIdentity.cs` — `CreateSystemAdministratorAsync` (USER + USER_ROLE, PBKDF2).
- `Data/Settings/OgSharedSetup.cs` — `BEEPSETTINGS` creation and revision-checked writes.
- `Data/Setup/OgSetupComposition.cs` — SECURITY discovered and its seed service registered.

Missing: nothing this stage asks for. AC-4 (provisional administrator), AC-5 (rollback) and AC-6 (resume) were the
three open items and each is now built and tested.
