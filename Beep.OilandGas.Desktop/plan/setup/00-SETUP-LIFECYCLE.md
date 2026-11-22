# Setup lifecycle — the revised plan

**Document ID:** SETUP-00
**Version:** 2.0 (replaces the setup chapter implied by FND-07)
**Date:** 2026-11-09
**Status:** revision accepted for implementation; stage status is recorded per stage below and in the [master tracker](../00-MASTER-TRACKER.md)

---

## 1. Why this document exists

The desktop's first-run setup grew one feature at a time. FND-07 delivered "record the company and create the
administrator" for a local file; a later slice added a company database, `BEEPSETTINGS`, the module set and the
shared environment. Those pieces work, but the **lifecycle** was never written down, so the order of operations,
the actor for each step and the failure behaviour were decided by whoever touched the code last.

This document fixes that. It states the lifecycle, the actor for each stage, what each stage writes, how each
stage fails, and what is built versus what this environment cannot prove. Each stage has its own document in
this folder.

Two levels are deliberately separated, because conflating them caused the one real ordering defect found while
writing this (company settings were being read before anybody had signed in):

| Level | What it holds | Where it lives | Who writes it |
|---|---|---|---|
| **Machine** | one connection definition: the server, or the path of the local database file | the BeepDM connection store on this computer | whoever installs, once |
| **Database** | company record, settings, RBAC, module set, environment, **and which deployment this is** | the installation's database (`BEEPSETTINGS`, `USER`/`ROLE`/…, module tables) | the SystemAdmin |

**No setting is stored in a file.** There is no `setup.json`: a personal installation is one whose connection points
at a local database file, and a company installation is one whose connection points at a server — the database
answers the question. The connection is the single unavoidable local fact, because a program cannot open a database
it cannot name. Everything else is a row.

A setting is never read from a machine that has not authenticated. A company fact is never stored on a workstation.

---

## 2. Research: what corporate applications actually do

Five sources were read for this revision; the extracts and URLs are in
[`plan/evidence/SETUP-RESEARCH/observed-research.json`](../evidence/SETUP-RESEARCH/observed-research.json).

### 2.1 Onboarding is one orchestrated, all-or-nothing operation
The AWS Well-Architected SaaS Lens (tenant onboarding) puts every phase behind one mechanism: a registration entry
point, tenant identity and settings created together, the **tenant administrator created in the identity store**
during onboarding, provisioning, then everything observable with tenant-aware logs. It also insists that
offboarding is designed at the same time as onboarding.
→ *Adopted:* one wizard owns company creation; the company record and the SystemAdmin are created by the same run;
every step reports into the failure store with the company and connection named.

The OWASP Multi-Tenant Security Cheat Sheet §7 is stricter: onboarding is **all-or-nothing**; if a step fails,
partially created resources are removed and the failure is recorded; the audit trail includes failed and partial
operations; credentials are generated with ≥128 bits of entropy, returned once and stored hashed.
→ *Adopted (partly):* failures are recorded; rollback of a half-created company is **not** built and is a stage
acceptance criterion (SETUP-03 AC-5).

### 2.2 The bootstrap administrator is temporary, and there is a recovery path
Keycloak's bootstrap admin exists only while the realm does not, is marked **temporary** in the UI, and is expected
to be removed once permanent admin access exists; a dedicated recovery command exists for lost access. Secrets come
from the environment, not from a command line. GitLab treats the first user as the default administrator and
documents a reset path; Jenkins prints a one-time initial password.
→ *Adopted:* the first administrator is created by the setup run only, is the only account that can configure the
company, and recovery is an explicit product decision (SETUP-03 §5, SETUP-12).
→ *Adopted:* the account is marked provisional and the shell refuses to open until its owner replaces the
password (SETUP-03 AC-4).

### 2.3 Password rules are length-based, not composition-based
OWASP's Authentication Cheat Sheet (reading NIST SP 800-63B-4) says a password under **8** characters is weak *with*
MFA and under **15** *without* it, the maximum must be **at least 64**, composition rules should **not** be imposed,
passwords should be screened against common/breached lists, periodic forced rotation should be avoided, and failures
must be answered with one generic sentence.
→ *Adopted now:* `OgPasswordPolicy` was rewritten (minimum 15, maximum 64, no character-class rules, digits-only and
short common-password blocklist). The previous rule (12 + upper/lower/digit/symbol) was pushing people towards
`P@ssw0rd1!` shapes and is gone.
→ *Adopted:* the SecLists `10k-most-common` list (10,001 entries as retrieved, embedded) screens every new
password (SETUP-05 AC-6). *Not built:* MFA, after which the floor may drop to 8.

### 2.4 A wizard is for infrequent, ordered, resumable work
Nielsen Norman Group's wizard guidance: wizards suit setup done rarely; show the **list of steps with the current
one highlighted**; **enforce sequential order** (no jumping); label the next button with what happens next, not
"Next"; allow the person to **leave and resume**; make each step self-sufficient; and remember previous choices as
defaults.
→ *Adopted:* the six steps and their content (SETUP-01…07); the review step names what it will do.
→ *Gap found by this research:* the current wizard is a `TabControl`, so a person can jump to any step — the exact
pattern NN/g warns against. Recorded as SETUP-00 AC-3 and scheduled in SETUP-03 §7.

### 2.5 Tenant context is verified, never trusted from the client
OWASP Multi-Tenant §1 and §3: derive the tenant from the **authenticated** principal and current membership; treat a
client-supplied identifier as a selector only; every tenant-scoped read is constrained by it.
→ *Adopted:* the company settings are read only after sign-in, through the connection the signed-in workstation is
bound to; there is no "type a company id" path anywhere.

### 2.6 What was wrong in the previous implementation (and is now fixed)

| Defect | Why it is wrong | Now |
|---|---|---|
| Company settings were read **before** sign-in | reads a company's configuration for an unauthenticated caller | `LoadSettingsAsync` refuses in company mode unless `IsSignedIn`; the startup order is sign-in → settings → shell |
| The company deployment signed in against the **local** credential file | the account directory of a company is the company database | `BeepDmDatabaseIdentity` authenticates against `USER`, returns roles from `USER_ROLE`/`ROLE` |
| The security module was **excluded** from the desktop setup | a company database would never get its RBAC tables | SECURITY is discovered and its seed service registered |
| Password policy required four character classes at 12 characters | contradicts OWASP/NIST and weakens real passwords | length 15…64, no composition, common-password blocklist |
| The wizard's steps were tabs | lets a person skip to the review step | recorded as a change: ordered steps with a step rail (SETUP-03 §7) |

---

## 3. The lifecycle in one picture

```
MACHINE (once)                        COMPANY (once, by the SystemAdmin)
SETUP-01 deployment  ──► SETUP-02 connection
                                        │
                                        ▼
                                   SETUP-03 company + first SystemAdmin  (all-or-nothing)
                                        │
                     ┌──────────────────┼──────────────────┬─────────────────┐
                     ▼                  ▼                  ▼                 ▼
              SETUP-04 settings   SETUP-05 RBAC     SETUP-06 modules/LOV   SETUP-07 environment
                     └──────────────────┴──────────────────┴─────────────────┘
                                        │
                                        ▼
                            SETUP-08 join an existing company (other workstations)
                                        │
                                        ▼
                            SETUP-09 every start: authenticate → read settings → shell
                                        │
                                        ▼
                            SETUP-10 SystemAdmin changes settings / RBAC / configuration
                                        │
                     ┌──────────────────┴──────────────────┐
                     ▼                                     ▼
              SETUP-11 upgrade                      SETUP-12 backup / restore / offline
```

`SETUP-13` is the personal installation: the same stages 1–10 with a local file as "the database" and the local
credential file as the account directory. It is the easy path and is not allowed to grow enterprise steps.

---

## 4. Actors

| Actor | May do |
|---|---|
| **Installer** (whoever runs the product first on a machine) | deployment choice, connection definition, start the company setup |
| **SystemAdmin** (the first account, then any account holding the administrator role) | create/change the company settings, the module set, the environment, users and roles; run upgrades; restore |
| **User** (any account in the company database) | sign in, read the company settings their role allows, use the pages their role allows |
| **Owner** (outside the product) | decides commercial terms, provides a licence, attests release criteria |

No stage may be performed by a User. No stage may be performed by an unauthenticated caller except SETUP-01,
SETUP-02 and the sign-in step of SETUP-03/08/09.

---

## 5. Stages, status and documents

| ID | Stage | Actor | Status | Document |
|---|---|---|---|---|
| SETUP-01 | Deployment choice (machine) | Installer | **Built** | [SETUP-01](SETUP-01-DEPLOYMENT.md) |
| SETUP-02 | Database connection (machine) | Installer | **Built** (AC-7 blocked on an enterprise server) | [SETUP-02](SETUP-02-CONNECTION.md) |
| SETUP-03 | Company creation and the first SystemAdmin | Installer → SystemAdmin | **Built** | [SETUP-03](SETUP-03-COMPANY-CREATION.md) |
| SETUP-04 | Company settings record | SystemAdmin | **Built** | [SETUP-04](SETUP-04-COMPANY-SETTINGS.md) |
| SETUP-05 | RBAC: roles, permissions, users | SystemAdmin | **Built** | [SETUP-05](SETUP-05-RBAC.md) |
| SETUP-06 | Modules, reference data and LOV | SystemAdmin | **Built** | [SETUP-06](SETUP-06-MODULES-LOV.md) |
| SETUP-07 | Shared Oil & Gas environment | SystemAdmin | **Built** | [SETUP-07](SETUP-07-ENVIRONMENT.md) |
| SETUP-08 | Joining an existing company | Installer + User | **Built** | [SETUP-08](SETUP-08-JOIN-COMPANY.md) |
| SETUP-09 | Every start: sign in, then read settings | User | **Built** | [SETUP-09](SETUP-09-DAILY-LOGIN.md) |
| SETUP-10 | Admin change control | SystemAdmin | **Built** | [SETUP-10](SETUP-10-ADMIN-CHANGE.md) |
| SETUP-11 | Upgrade and migration | SystemAdmin | **Built** | [SETUP-11](SETUP-11-UPGRADE.md) |
| SETUP-12 | Backup, restore and an unreachable database | SystemAdmin | **Built** | [SETUP-12](SETUP-12-BACKUP-RESTORE-OFFLINE.md) |
| SETUP-13 | Personal installation | Installer | **Built** | [SETUP-13](SETUP-13-PERSONAL.md) |

"Built" means every acceptance criterion is met by code exercised by a test or the smoke run, or records plainly
why it cannot be met here. "Blocked" means the criterion needs something this environment does not have; it is named
in the stage document rather than left implied. There is no "planned" stage left: SETUP-01..13 are all built.

---

## 6. Principles every stage must satisfy

1. **Machine vs company.** Nothing about the company is stored on a workstation; nothing about the workstation is
   stored in the company database.
2. **Authenticate before reading.** A company setting is only read by a signed-in account, through the connection
   that account's workstation is bound to.
3. **One writer per fact.** The SystemAdmin writes company facts; a User reads them. No background path writes a
   company record.
4. **All-or-nothing.** A stage that fails removes what it partly created, or states exactly what remains — never
   leaves a company that half exists without saying so.
5. **Idempotent.** Running the same stage twice is the same as running it once: the revision check refuses a stale
   write, the intent id makes a retry the same change, and an existing company is adopted, not recreated.
6. **Refuse with a reason.** Every refusal is a sentence a person can act on plus a stable code; provider text and
   stack traces never reach the screen.
7. **No default credentials.** There is no shipped password, no `admin/admin`, no seed account that can sign in.
8. **Length, not composition.** Passwords follow OWASP/NIST: 15 characters without MFA, 64 maximum, no character
   classes, no forced rotation, common passwords refused.
9. **Recorded.** Every stage that changes the installation writes an audit row or a settings row with who and when.
10. **Observable.** A failed stage is a recorded failure with a reference, not a silent no-op.
11. **Resumable.** A setup that is interrupted can be continued without re-entering what was already accepted
    (SETUP-03 AC-6).
12. **Personal is the exception, not the template.** The personal installation skips the enterprise stages rather
    than pretending to run them.

---

## 7. Acceptance criteria for this revision

| # | Criterion | Evidence |
|---|---|---|
| AC-1 | Every stage 01–13 has a document with actor, preconditions, records, failure states and acceptance criteria | this folder |
| AC-2 | The research is recorded with URLs and the specific fact taken from each source | `evidence/SETUP-RESEARCH/observed-research.json` |
| AC-3 | The password rule is length-based and the change is tested | `OgPasswordPolicy`, `PasswordPolicyTests`, `DatabaseIdentityTests.The_password_policy_is_one_rule_for_every_account_store` |
| AC-4 | The order "authenticate → read settings → shell" is enforced in code, not just documented | `OgApplication.LoadSettingsAsync` guard, `App.OnStartup` |
| AC-5 | Each stage states honestly whether it is built, and names what it cannot prove here | §5 above and the stage documents |
| AC-6 | The master tracker names this revision and links every stage document | `plan/00-MASTER-TRACKER.md` §12a |

---

## 8. Gap register (what this revision does **not** deliver)

| # | Gap | Stage | Why it matters |
|---|---|---|---|
| G-1 | — **closed** — | SETUP-03 | the tabs became a labelled rail (done/here/ahead) and the wizard resumes where it stopped |
| G-2 | — **closed** — | SETUP-03 | `OgCompanyCreation` records what it wrote and undoes it; `CompanyCreationTests` |
| G-3 | — **closed** — | SETUP-03 | the setup writes a `BOOTSTRAP` marker; the shell waits for the password to be replaced |
| G-4 | — **closed** — | SETUP-05 | the `users` page creates, resets, enables/disables and grants roles |
| G-5 | — **closed** — | SETUP-05 | a write path is allowed by the role or by the permission its role carries |
| G-6 | — **closed** — | SETUP-05 | the embedded SecLists 10k list screens every new password |
| G-7 | — **closed** — | SETUP-12 | the settings page backs up and restores a file database; a server database stays the DBA's |
| G-8 | — **closed** — | SETUP-11 | an older installation is migrated at startup: the documents are re-saved through the service and the database version is stamped last |
| G-9 | **Blocked**, not built: enterprise server connections are reviewed, not exercised | SETUP-02 | no SQL Server/PostgreSQL/Oracle in this environment, so AC-7 cannot be proved here |

Each gap has an acceptance criterion in its stage document so it cannot be lost.

---

## 9. Document convention

Each stage document uses the same ten sections, so a reader can compare stages:

1. Purpose · 2. Actor · 3. Preconditions · 4. Steps · 5. Records written · 6. Failure states · 7. Security
   requirements (with the source that demands them) · 8. UX requirements · 9. Acceptance criteria · 10. What exists
   today and what is missing.
