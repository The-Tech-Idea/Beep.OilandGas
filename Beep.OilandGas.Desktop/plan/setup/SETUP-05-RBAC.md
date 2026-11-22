# SETUP-05 — RBAC: roles, permissions and users

**Document ID:** SETUP-05
**Status:** Built
**Actor:** SystemAdmin
**Level:** Company

---

## 1. Purpose

Give the company an account directory and an authority model in its own database: who may sign in, what role each
account holds, and which pages and writes that role allows. Prepared once by the SystemAdmin, then maintained by the
SystemAdmin as people join and leave.

## 2. Actor

SystemAdmin. A User reads their own grants; nobody else's.

## 3. Preconditions

- SETUP-03 applied the SECURITY module, which creates the RBAC tables and seeds the default roles and permissions.
- At least one account exists (the SystemAdmin from SETUP-03).

## 4. Steps

1. **Schema** — SECURITY creates `USER`, `ROLE`, `PERMISSION`, `USER_ROLE`, `ROLE_PERMISSION`, `PERSONA_ROLE`,
   `USER_SCOPE_ASSIGNMENT`, `USER_ASSET_ACCESS`, the persona/profile tables and the audit tables, in that module's
   order.
2. **Seed** — `IDefaultSecuritySeedService` inserts the default roles (`Administrator`, `Admin`, `Manager`,
   `PetroleumEngineer`, `ReservoirEngineer`, `Approver`, `Auditor`, `Viewer`, …), their permission grants, the
   system principal (which **cannot** sign in) and the persona→role map.
3. **First administrator** — SETUP-03 creates the `USER` row and the `USER_ROLE` grant to the administrator role.
4. **Sign-in** — SETUP-09 authenticates against `USER` and reads the roles from `USER_ROLE` → `ROLE`.
5. **Authorisation** — the shell and pages check the resulting role set (`OgPermissions.IsSystemAdmin`,
   `HasRole`, `HasPermission`).
6. **Later maintenance** - create users, grant/revoke roles, reset passwords and clear a lock: the `users` page
   (`BeepDmUserAdministration`, `Views/UsersView`).

## 5. Records written

| Table | Row |
|---|---|
| `ROLE` | seeded roles, plus any the SystemAdmin adds |
| `PERMISSION`, `ROLE_PERMISSION` | the seeded permission catalogue and its grants |
| `USER` | the administrator; later, every user (PBKDF2 `PASSWORD_HASH`, `IS_ACTIVE`, tenant and business associate) |
| `USER_ROLE` | user ↔ role, with `EFFECTIVE_DATE`/`EXPIRY_DATE` |
| `USER_SCOPE_ASSIGNMENT`, `USER_ASSET_ACCESS` | the data scope of a user (seeded for the system principal) |
| `SETUP_WIZARD_LOG` | the module run and its outcome |

## 6. Failure states

| Code | Meaning | Sentence / next action |
|---|---|---|
| `sign-in-security-tables-missing` | the database has no `USER`/`ROLE` | run SETUP-03 with the security module |
| `administrator-role-missing` | no administrator role to grant | re-run the security seed |
| `sign-in-no-accounts` | the directory has no login account | the SystemAdmin must create one |
| `sign-in-refused` | wrong password, unknown user, inactive or seeded principal | one generic sentence |
| `password-policy` | the password is under 15 characters, over 64, digits-only or on the common list | the rule that was broken |

## 7. Security requirements

- **No default credentials** (OWASP Authentication; Keycloak's bootstrap guidance): nothing that can sign in is
  seeded; the seeded principal's hash is `seeded-no-login` and is refused.
- **Length-based passwords** (OWASP/NIST): 15…64 characters, no composition rules, no periodic rotation; digits-only
  refused; a password on the embedded SecLists `10k-most-common` list (10,001 entries as retrieved) is refused, matched
  case-insensitively and exactly (a lengthened passphrase is a different secret, not a match).
- **One generic refusal** for wrong password, unknown user and disabled account (OWASP: no enumeration).
- **Lockout** per account, not per IP, with the attempt count recorded: five failures inside fifteen minutes lock
  the account for fifteen minutes, a correct password from the owner while locked is told the time, a wrong password
  on a locked account is refused generically and does not extend the lock, and a success clears the count. A failure
  older than the window starts the count again, so an occasional mistype never accumulates.
- **Server-verified identity only** (OWASP Multi-Tenant §1): the role set comes from the database at sign-in; a
  client cannot present a role.
- **Least privilege:** a write path is allowed by the administrator role **or** by the permission its role carries
  (`Admin.ManageUsers`, `Admin.ConfigureSystem`), read from `ROLE_PERMISSION` at sign-in. An expired role grant
  grants neither the role nor the permission.
- **Audit:** every sign-in attempt is audited (accepted or refused) with the roles granted.
- **Credential material:** only the PBKDF2 envelope is stored (`pbkdf2-sha256$210000$salt$hash`), compared with
  `CryptographicOperations.FixedTimeEquals`.

## 8. UX requirements

- The sign-in screen names the company and one failure sentence.
- The Settings page tells a non-administrator *why* it cannot change the settings rather than hiding them.
- A user sees only the pages their role allows; a refused page says so (`State.Denied`).

## 9. Acceptance criteria

| # | Criterion | Status |
|---|---|---|
| AC-1 | SECURITY runs in the desktop setup and creates the RBAC schema | **Met** — `OgSetupComposition` (guard test) |
| AC-2 | The first administrator is a real account with a real hash and the administrator role | **Met** — `DatabaseIdentityTests` |
| AC-3 | A seeded principal cannot sign in | **Met** — `DatabaseIdentityTests.A_seeded_principal_is_not_a_login_account` |
| AC-4 | Roles are read from the database at sign-in and applied to the session | **Met** — `OgSignInService.SignInAsync`, `OgApplication.SignInAsync` |
| AC-5 | Wrong password, unknown user and inactive account share one sentence | **Met** — `DatabaseIdentityTests.A_wrong_password_and_an_unknown_user_get_the_same_sentence_and_no_session` |
| AC-6 | Breached-password screening | **Met** — the SecLists `10k-most-common` list (10,001 entries as retrieved) (`10k-most-common.txt`, retrieved 2026-11-09) are embedded and screened case-insensitively; `PasswordPolicyTests` (12) |
| AC-7 | Account lockout/throttling recorded per account | **Met** — `OgSignInPolicy` (5 failures in 15 minutes, 15-minute lock) against `USER.FAILED_LOGIN_COUNT`/`LOCKED_IND`/`LOCKOUT_UNTIL_UTC`; `SignInLockoutTests` (5); an administrator can clear a lock from the `users` page |
| AC-8 | Permission-level authorisation, not only roles | **Met** — `PERMISSION` is read through `ROLE_PERMISSION` at sign-in into `OgPermissions.Permissions`; `CanManageUsers`/`CanConfigureSystem` gate the users and settings write paths; `PermissionGateTests` (5) |
| AC-9 | User management: create, disable, reset, assign roles | **Met** — `BeepDmUserAdministration`, `Views/UsersView` (SystemAdmin only, registered as `users`), `Views/OgAccountDialog`; `UserAdministrationTests` (6) |

## 10. What exists today and what is missing

Exists: `Data/Setup/OgSetupComposition.cs` (SECURITY + `IDefaultSecuritySeedService`), `Data/Security/OgDatabaseIdentity.cs`
(sign-in, roles, create administrator, PBKDF2), `Core/OgPasswordPolicy.cs`, `Security/OgSignInService.cs`
(`SignInAsync` with roles), `Core/OgPermissions.cs`, `Core/OgRoleNames.cs`, `OgApplication.MapRoles` (the database's
`Administrator`/`Admin` also grants the app's `SystemAdmin`).

Missing, in order: breached list (AC-6), MFA (which would also allow the 8-character floor).
