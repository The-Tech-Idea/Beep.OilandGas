# Implementation 95: company period action authority

Updated 2026-10-06. Supporting accountant period review/action boundary. Source wiring with focused role proof; native complete close/lock/readback remains open.

## Active endpoint correction

Existing Field.AccountingController.ClosePeriodAsync previously relied on current FIELD admission and passed no accounting database connection to the production owner, leaving its legacy default in the call. The method now has a standard Accountant/Administrator Authorize guard and explicit FIELD WRITE admission in addition to the existing current-field boundary.

Before business mutation it requires the selected request company's account to match the authenticated acting account, reads fresh approved company access through the canonical IRepositoryAccessService, requires standard ASP.NET roles to agree with that company's current roles, and checks FIELD WRITE through the existing resource owner. It resolves PRODUCTION through the canonical ModuleConnectionResolver, repeats current company roles/FIELD write access, verifies unchanged FIELD, and passes the resolved connection to the existing production close owner. No caller-selected connection or new authorization store is introduced. Timestamp-shaped period ends are refused rather than silently truncated to a date.

Existing period-check reads now reuse the same standard-role/company-role agreement rule instead of accepting only the selected-company assignment without principal agreement. IdentityServer remains authentication-only. This change adds no assignments or privileges; a global Administrator claim alone cannot authorize another company's close.

## Focused evidence

Test-AccountingPeriodRoles.ps1 compiles the actual AccountingPeriodRoleChecks source against actual Data contracts: **8 passed**, zero failures/skips, terminal exit 0, 0.8688 seconds. Log `%TEMP%/oilgas-period-roles.log`. Six cases cover matching Accountant/Administrator roles, reader denial, global Admin without matching company role and mismatched claim/assignment. Additional cases cover colliding company scope, revoked/missing access and unauthenticated role claims. Whitespace diff check passed.

Existing native controller test source now expects an unselected-company close to refuse before any mutation instead of treating a no-company/default-database request as successful. Those controller tests, MVC role/FIELD filters, service resolution and actual close invocation are unexecuted evidence here. The focused role tests do not prove controller wiring or database isolation.

## Full journey still required

Original reviewed book/period/version, independent preparer/controller command context, provider-level atomicity/idempotency, durable original intent, complete close/lock/history and ledger receipt/read-back remain open. The existing close request does not yet carry those reviewed fields; this endpoint is not certified as a completed finance workflow. Its internal service callbacks may resolve bindings during later stages; native provider tests must verify stable physical target and refuse changed bindings before work crosses databases. Passing an explicit connection at the gateway is not proof of transaction-wide binding stability.

Verify native allowed and denied routes with current company roles, direct deep links/API calls, revoked permissions, colliding company/FIELD IDs and changed PRODUCTION configuration. Connect the reviewed command and recorded outcome to the period screen only after those conditions are satisfied. Other accounting read endpoints and posting paths still need their own full authority/binding/version/receipt audit. The accountant and remaining persona plans remain in scope.

No SQL, schema change, business EF lane, role grants, invented close success or phase/goal closure.
