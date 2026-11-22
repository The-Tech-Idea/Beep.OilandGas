# SETUP-08 — Joining an existing company

**Document ID:** SETUP-08
**Status:** Built
**Actor:** Installer, then the person's own account
**Level:** Company (adoption only)

---

## 1. Purpose

Connect a second (third, hundredth) workstation to a company that already exists, without creating a second
company, a second administrator or a second set of settings.

This is the stage most likely to be done by someone who is not the SystemAdmin, so it must be impossible to
accidentally re-initialise the company.

## 2. Actor

The installer defines the connection; the user signs in with the account the SystemAdmin created for them. The
installer does **not** need administrator rights.

## 3. Preconditions

- The company database exists and has a `BEEPSETTINGS/COMPANY` row.
- The person has a `USER` row in that database (created by the SystemAdmin — SETUP-05).
- The workstation can reach the server (SETUP-02).

## 4. Steps

1. SETUP-01 chooses **A company database**.
2. SETUP-02 defines and proves the connection; the name is adopted.
3. The Company step offers **"Read what this database already contains"**. If a company record exists it is
   adopted: the wizard fills the fields and states that it will not write a second company. If it does not exist,
   the wizard says so and this is a first-run on a new company (SETUP-03).
4. The wizard **does not** create the administrator when the company already exists: it offers the existing account
   (a name that already exists is adopted, not recreated).
5. Nothing about the company is recorded on this machine: the connection is already there and the database answers the rest.
6. The person signs in with their own account and password (SETUP-09).

## 5. Records written

| Store | What |
|---|---|
| Machine | one connection definition, and nothing else |
| Company database | **nothing** — the whole point of this stage |

## 6. Failure states

| Code | Meaning | Sentence / next action |
|---|---|---|
| `settings-table-missing` | the server database has no settings table | it is not this product's company database, or the security/system module never ran |
| `connection-*` | the connection failed (SETUP-02 codes) | fix the connection |
| `sign-in-refused` | the person has no account or the wrong password | ask the SystemAdmin for an account |
| `sign-in-no-accounts` | the directory is empty | the database is not set up; this is SETUP-03, not a join |

## 7. Security requirements

- **No second company.** The adopt path reads the record and refuses to write when one exists; the safest possible
  failure on a join is "nothing happened".
- **No second administrator.** An existing user name is adopted; a new account is only created when the name is
  free, and only by an authorised run.
- **No cross-company leakage.** The workstation is bound to one connection; the settings are read through it after
  sign-in, and the company is derived from that connection, never from a typed identifier (OWASP Multi-Tenant §1).
- **The installer's rights are minimal:** the connection's service account may read the company database; it does
  not need administrator rights on the application.

## 8. UX requirements

- The Company step has one button that answers "is this database already set up?" before the person fills anything.
- When adopting, the wizard says that it will not write a second company, and shows the recorded company, when it
  was recorded and by whom.
- The sign-in screen names the company that was adopted.

## 9. Acceptance criteria

| # | Criterion | Status |
|---|---|---|
| AC-1 | An existing company record is adopted, not overwritten | **Met** — `FirstRunWindow.WriteSharedSettingsAsync` (adopt branches), `SharedSetupIntegrationTests` stale-write refusal |
| AC-2 | An existing user name is adopted, not recreated | **Met** — `CreateSystemAdministratorAsync` → `administrator-exists`, treated as adoption by the wizard |
| AC-3 | A joined workstation writes nothing to the company database | **Met** — `OgCompanyCreation` returns on the adoption branch before the module step, the settings writes and the account creation; `CompanyCreationTests.An_existing_company_is_adopted_and_nothing_is_rewritten` now also proves the module runner is not called a second time |
| AC-4 | The person signs in with their own account | **Met** — `DatabaseIdentityTests` |
| AC-5 | The person can choose their account from the directory instead of typing it | **Met** — `ListUserNamesAsync` (names only, sign-in accounts only) fills an editable picker on the sign-in window; `AccountPickerTests` (5) |
| AC-6 | A database holds one company, so a join adopts it and cannot create a second | **Met by design** — the adoption branch returns before any company write; a different company means a different database (SETUP-01 AC-5). There is no "another company" inside one database to detect |

## 10. What exists today and what is missing

Exists: the adopt button and its status text, `WriteSharedSettingsAsync` adopt branches, the administrator
adoption path, `DatabaseIdentity` and the account picker.

Missing: nothing this stage asks for. The module step can still be visited before the database is probed, but it
has no effect on a join: the creation service returns before applying anything (AC-3, tested).

**Trade-off recorded:** offering account names on the sign-in window reveals them to anyone at that machine. The
product accepts that because the machine is the company's own and the database is the company's own directory; a wrong
name is still refused with the same sentence as a wrong password, and no display name, address or hash is exposed.
