# SETUP-13 — Personal installation

**Document ID:** SETUP-13
**Status:** Built
**Actor:** Installer (who is also the user)
**Level:** Machine (no company database)

---

## 1. Purpose

The easy path: one person, one computer, no server. It is **not** a different architecture — the personal
installation is one whose database is a local file, holding the same rows as a company installation (company record,
accounts, roles, module set, environment, schema version). "Easy" means the wizard creates the database for you and
there is no server to describe. This
document exists so that "personal is easy" is a written rule and not an accident — the enterprise steps must never
leak into it.

## 2. Actor

One person, who installs, is the administrator, and works. There is no second account.

## 3. Preconditions

- None beyond a writable data root.

## 4. Steps

1. SETUP-01: choose **This workstation only**.
2. SETUP-02: nothing to enter — the wizard creates `…\Data\Local\BeepOilGas.db` with whichever installed driver
   advertises local creation.
3. SETUP-03/04/05/06/07: one creation run against that local database — the SECURITY module creates
   `USER`/`ROLE` and seeds the roles, the company record and environment go into `BEEPSETTINGS`, and the
   administrator is a `USER` row with a PBKDF2 hash. The same `OgCompanyCreation` the company path uses.
4. SETUP-04: the company values are rows in that local database, in `BEEPSETTINGS/COMPANY`.
5. SETUP-07: the environment is this workstation's file (`…\Wellbore\environment.json`) or the built-in defaults.
6. SETUP-09: sign-in against the local credential file; no database round trip; the shell opens.

**What is deliberately skipped:** the server form, the choice of extra PPDM modules (SECURITY is always applied),
joining an existing company, and administering other users. The database, the settings table and the account
directory are **not** skipped: they are the installation.

## 5. Records written

| Store | What |
|---|---|
| Local database | `BEEPSETTINGS` (`COMPANY`, `TOOLS_ENVIRONMENT`, `MODULES`, `SYSTEM`) |
| Local database | `USER` (PBKDF2 hash), `ROLE`, `USER_ROLE` |
| Machine | `…\Data\Local\BeepOilGas.db` |
| Machine | `…\Wellbore\environment.json`, when a customer environment is used |
| Audit / failure stores | under the same data root |

## 6. Failure states

| Code | Meaning | Behaviour |
|---|---|---|
| `local-file-not-created` | the driver refused to create the file | refuse, name the path and the driver |
| `administrator-exists` | a second administrator is attempted | refused; there is exactly one |
| `repair-path-invalid` | the local path is unusable | refuse, state the path |
| `sign-in-refused` | wrong password / unknown user | one sentence |

## 7. Security requirements

- **Password policy is the same rule** (15…64, no composition, common passwords refused) — a personal installation
  is not allowed a weaker rule than a company one.
- **Hash only.** The credential file never contains the password.
- **One administrator.** A second `CreateFirstAdministrator` is refused, so a repair bug cannot mint a second one.
- **Nothing leaves the machine.** No server connection, so no network path for a failure to travel by accident.

## 8. UX requirements

- One window, no steps that do not apply: if the personal deployment is chosen, the connection and module steps say
  so and do not pretend to be needed.
- The data root is shown, and the promise "nothing is written beside the executable" holds.
- The password hint is a sentence, not a rule list.

## 9. Acceptance criteria

| # | Criterion | Status / evidence |
|---|---|---|
| AC-1 | A personal installation creates its file, its company record and its administrator, and signs in | **Met** — `ConnectionCatalogIntegrationTests`, `CompanyCreationTests`, `SetupAndSignInTests`, smoke first-run |
| AC-2 | The personal path reads and writes the same database rows as a company installation | **Met** — one `OgCompanyCreation`, one `LoadSettingsAsync` |
| AC-3 | The same password rule applies | **Met** — one `OgPasswordPolicy` |
| AC-4 | A second administrator is refused | **Met** — `DatabaseIdentityTests.Creating_a_second_account_with_the_same_name_is_refused` |
| AC-5 | A missing file is a setup run, and the installation is not silently replaced | **Met** — `OgStartupGate` sends it to setup; `LocalRepairTests` proves the repair path refuses (`repair-not-applicable`) rather than claiming to restore |

## 10. What exists today and what is missing

Exists: everything above (`FirstRunWindow`, `OgCompanyCreation`, `BeepDmDatabaseIdentity`,
`BeepDmConnectionCatalog.EnsureLocalAsync`, `OgRepairService`).

Missing: nothing required. A personal user who later wants a company database must re-install with the company
deployment; moving a personal database into a company is a separate feature and is not offered.
