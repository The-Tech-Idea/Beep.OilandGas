# Implementation 150: Accounting source HTTP routes

## Actual API-first source journey

The focused gate now compiles the complete current APController and PurchaseOrderController and runs their actual routes through ASP.NET Core TestServer. The accountant invoice detail -> owned PO source -> recorded receipt allocation path returns the existing shared Data reviews over HTTP using the actual current AP/PO services and generic repository. Company invoice/order queue roots retain their original owner predicates and source-detail routes.

The HTTP tests found that MVC converts a supplied whitespace vendorId into null. The optional supplier endpoint therefore treated an invalid explicit restriction as an unrestricted company query. Both root GET actions now check raw query presence/cardinality: omitted vendorId selects the company query, while empty, whitespace or repeated values return 400 before a business datasource read. International supplier IDs survive encoded query transport and shared JSON with exact company ownership.

## One current company/account/binding owner

ICompanyAccountingReadContext exposes a fresh current access snapshot, PRODUCTION binding and authoritative actor. Existing CompanyModuleConnection implements it using the existing app account reader, current company refresh, RequireActor, module resolver and binding fingerprint. Program resolves this interface to the same existing scoped CompanyModuleConnection instance; there is no second authorization/store implementation in production.

The two controllers use this boundary rather than duplicating company refresh/binding methods. Standard authenticated app-role and independent permission checks remain in the actual controllers/helpers. Detail/list closing captures compare original company, binding and actor. AP purchase/allocation callbacks pin all three across repeated reads, while their existing helpers independently require both permissions. IdentityServer remains authentication-only; no role grant or claim-source change is introduced.

The focused HTTP fixture substitutes only the declared current-account/company/binding boundary, test authentication and Beep datasource I/O. Production ActingUser depends on a PartyIdClaims API absent from the available older shared IdentityServer assembly; it is not reimplemented or replaced in production. The real adapter and Program wiring remain source-only pending the current shared dependency/full-host gate.

## Focused evidence

The final affected actual-source gate passes **323 tests**, zero failed/skipped, terminal exit 0, in 2.4764 seconds. Log: `%TEMP%/oilgas-accounting-http-150.log`. It includes the previous 301 cases plus 22 new HTTP cases; overlapping totals are not summed. The existing 175 dependency/source-precedence warnings remain. Data sources are unchanged from the prior clean complete-source build.

HTTP cases cover two anonymous 401 challenges; four missing standard-role/permission 403 refusals before business source reads; six empty/whitespace/repeated vendor 400 refusals; two creation 409 fences with no datasource interactions; two changed-binding 409 responses; two fixed provider-failure responses without private exception details; two original-company reads across colliding local IDs and encoded international suppliers; one complete invoice/owned-PO/allocation route read using typed shared JSON; and one procurement-permission denial before any PO read.

Complete controllers, real API helpers, current AP/PO services and generic repository/interface/transaction helpers compile and execute here. Authentication and current access are explicit fixtures, not the production OIDC/app-role bridge. The source-dependent CompanyModuleConnection adapter, Program/full DI host, complete typed Web client/parent pages, browser/keyboard/RTL, native provider schema/transactions/performance and physical migration application remain unverified. No SQL, business EF path, live migration, business write, role assignment or deployment is introduced.

## Remaining persona delivery

Verify production current-account/company access and PRODUCTION binding through the real role bridge and full host. Compile/run the complete typed Web client and parent landing/detail flow, including company changes and revocation in the browser. Apply/reconcile the AP-line/allocation Beep schemas through reviewed MigrationManager; establish allocation duty/acceptance/unit/currency/tolerance rules, global concurrent receipt consumption, independent approval/post/payment and durable outcomes/recovery. Complete buyer/receiver/stock and every other persona/multinational requirement. No phase closes.
