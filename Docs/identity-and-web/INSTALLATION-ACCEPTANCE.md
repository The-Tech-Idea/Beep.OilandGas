# OilGas Installation Acceptance

This audit follows the requested installation architecture, not every feature in
the oil-and-gas solution. It is not a declaration of production readiness.

## Required Architecture and Evidence

| Requirement | Current implementation and evidence | Remaining verification |
| --- | --- | --- |
| Default repository separate from module databases | `RepositoryServiceRegistration` selects one explicit repository provider/connection. `ModuleDatabaseBindings` stores module-to-BeepDM connection selection. Registration tests exercise all three providers without opening domain databases. | Real installation walkthrough below. |
| Standard ASP.NET tables | `RepositoryDbContext` uses Identity stores. Six migrations per provider create the seven standard `AspNet*` tables plus repository metadata and `APP_*` extensions. Provider tests check table presence and snapshot/model consistency. | SQL generation is not a live Oracle/PostgreSQL execution test. |
| Explicit basic installation | `Install-Repository.ps1` defaults to idempotent SQL preview; Apply executes pending EF migrations. It does not seed users or execute module migrations. OilGas startup checks readiness rather than implicitly creating its schema. | Repeat the documented operator workflow with the final OAuth configuration. |
| SQL Server LocalDB development | API development settings select `(localdb)\MSSQLLocalDB`, database `BeepOilGasRepository`, integrated authentication. Installer rejects non-LocalDB targets in LocalDevelopment mode. | No PostgreSQL or Oracle credentials are needed for local work. |
| First registered OilGas user is Administrator | `RepositoryBootstrapService` transaction creates the user, external login, canonical role/membership, extension audit, and singleton bootstrap marker. LocalDB tests cover competing registration and exactly one Administrator. Later users receive no automatic roles. | Real browser registration is pending. Existing users without a marker require recovery, not promotion of the next arrival. |
| Canonical roles and extension tables | Local ASP.NET roles and memberships are authoritative; extension metadata does not independently grant roles. API and Web transformations discard external authorization claims and resolve local roles. Role/user API tests enforce local Administrator checks. | Browser role assignment, revocation, and inactive-user denial remain to be exercised together. |
| Authentication-only IdentityServer | OilGas validates external identity and registers its own local user. JWT and introspection modes use explicit HTTPS authorities. Introspection tests verify that external Administrator claims do not grant local roles. | A real Web OAuth client and API introspection resource client are not yet verified. |
| User-selected module databases and BeepDM migrations | `ModuleConnectionResolver` requires named bindings and refuses SECURITY. Migration binding/target tests cover missing or changed bindings and scoped plans. Module setup uses the BeepDM migration workflow independently of repository EF installation. | Complete the UI walkthrough for selection, review, approval, execution, and subsequent module read. Existing tests are not proof that every domain feature is complete. |

## Current Verification

- `TestResults/installation-audit-repository.log`: 29 passed, zero skipped.
  `OILGAS_TEST_LOCALDB=1`; fresh installation and competing bootstrap use LocalDB.
  Provider SQL and idempotent scripts are generated offline for all three providers.
- `TestResults/installation-audit-web.log`: 81 passed, zero skipped.
  Tests cover the client registration and role bridge, not browser rendering.
- `TestResults/hierarchy-path-access-full.log`: latest full API run, 872 passed,
  zero skipped, with all three LocalDB integration flags enabled.
- No live Oracle/PostgreSQL test was performed. No existing database was reset.

Subsequent module client verification: `TestResults/module-response-web.log`
records 91 passing Web tests and zero skips. Successful approval must identify
the requested plan; successful execution must match its reviewed plan ID, plan
hash, and manifest hash. Mismatched/empty identities are not shown as success,
and an uncertain execution is not automatically retried. Failure responses retain
their diagnostic message. This is protocol-level evidence, not a browser walkthrough.

## Final Installation Walkthrough

1. Use the isolated LocalDB IdentityServer procedure in
   `LOCALDB-IDENTITY-VERIFICATION.md`. Synthetic registration consent is awaiting
   user approval; do not submit the terms/privacy checkbox without it.
2. Register the Web code-flow/PKCE client and a separate API introspection resource
   client. Keep secrets in secure process configuration. Verify real access-token
   acceptance; discovery availability alone does not prove client trust.
3. Create an isolated OilGas LocalDB repository, preview its EF script, apply all
   pending migrations, and verify readiness reports BootstrapRequired. Do not
   consume the real development repository's first Administrator with test data.
4. Register the first subject through Web, verify standard Administrator role and
   repository readiness, then register a second subject with no automatic role.
5. Exercise user administration: grant/revoke a role and disable the second user.
   Verify both UI role gates and protected API responses change accordingly.
6. Select a distinct named BeepDM module database; review and approve a migration
   plan, execute it, and verify the module reads that target. Confirm repository
   Identity tables remain in the default repository and no global fallback occurs.
7. Stop only the test processes; retain isolated databases and logs. Record actual
   browser/API evidence before declaring installation acceptance complete.

## Known Separate Defects

Hierarchy configuration still uses an invalid untyped repository. Its persistence,
audit actor, traversal, and recursive filtering need correction. These are genuine
feature defects, not evidence that repository EF installation itself failed.

Distinct external subjects that collide under a database's unique-key collation
are denied rather than merged. This prevents account alias escalation but leaves
registration availability for such subjects unresolved. It must remain visible
when assessing supported external identity providers.
