# Implementation 01: build repairs and notification isolation

> Historical slice: subsequent build repairs and delivery-time revocation are recorded in [implementation 02](Implementation-02-Revocation-and-Build.md). The four-error baseline and revocation limitation below describe this earlier slice.

Date: 2026-09-21. Working-tree changes on baseline `cd2e4511`. Execution status remains in the [master tracker](../../MASTER-TODO-TRACKER.md).

## Roles and personas

Both remain app-owned. Roles/permissions authorize actions and resources; personas organize work and navigation. IdentityServer provides authentication only. Selecting a persona never grants its mapped roles.

The existing `PERSONA_ROLE` primary mappings now supply required **role IDs**, matching the current `IAccessControlService.GetUserRolesAsync` implementation, which returns IDs. This slice does not claim to implement FG-11's standard role-name claim bridge. No new role store, persona model, workflow engine or notification hub was added.

## Implemented source

- Web registers NotificationService per circuit rather than globally. It resolves the authenticated subject and its token using the existing shared authentication helpers; the hub URL derives from ApiService configuration.
- Connections automatically join their authenticated user channel. The caller-selectable user subscription method was removed.
- Persona subscriptions require all primary app role IDs and access to an explicit field. Group keys contain both persona and field, and old persona subscriptions are removed when switching, including failed switches. Process subscriptions require access to both the process field and its underlying entity.
- Reconnect repeats server user-channel assignment and revalidates the client's persona subscription. Hub connections close on authentication expiration. User changes/logout clear notifications and dispose the old connection.
- The notification buffer is bounded at 100, notification IDs are deduplicated, and read counts derive from retained unread entries. The main layout mounts the notification UI for authenticated users, with visible connection failures and retry. Persona/field changes update the subscription through existing services.
- The existing persona profile service reads active primary mappings using PPDMGenericRepository. Generated PPDM models are unchanged.

## Build repairs

Removed the invalid class-level return from WellComparisonService. Added missing imports for existing approval/module-setup interfaces. Corrected accounting interface namespaces. Distinguished lifecycle initializer method names from cross-role initializer names; their process IDs, definitions and callers remain intact.

The API build progressed from CS1519 to 19 declaration errors, then to 4. Remaining errors reference `IAuditChainService` in ReportTemplateService and `IChokeAnalysisService` in PPDMCalculationService. UserManagement already depends on LifeCycle, so adding the reverse project reference would create a cycle; the audit contract needs a deliberate ownership correction.

## Verification

Added [Web.Tests](../../Beep.OilandGas.Web.Tests/Beep.OilandGas.Web.Tests.csproj) to the solution, with [notification integration tests](../../Beep.OilandGas.Web.Tests/WorkflowNotificationIsolationTests.cs). Thirteen tests pass for user isolation, caller-selected user rejection, persona role/field checks, cross-field isolation, process entity checks, anonymous rejection, reconnect, logout, account switch, missing user token, unknown persona, ignored external role claims, and bounded/deduplicated state (some scenarios share tests).

Normal command: `dotnet test Beep.OilandGas.Web.Tests/Beep.OilandGas.Web.Tests.csproj --logger trx -v quiet`. Restore ran, but LifeCycle compilation prevents the normal project test execution.

To get bounded evidence, a temporary project linked the **same production source and same test file**, rather than copied implementations: hub, NotificationService, inbox contracts/service, persona contract/model/service sources and shared TokenProvider/subject helper. It references the existing Models and PPDM39.DataManagement projects plus the same SignalR/xUnit/Moq package versions. The real loopback HTTP/SignalR host uses a test-only authentication handler and mocked access/process/persona services. The latest run passed 13, failed 0, skipped 0. This proves the exercised hub/client behavior, not the complete application host.

Local evidence: `%TEMP%/beep-notification-harness-path.txt` identifies that temporary project. Its `TestResults/notifications.trx` contains the result. `%TEMP%/beep-notification-tests.log`, `%TEMP%/beep-notification-full-tests.log` and `%TEMP%/beep-oilgas-implementation-build.log` hold focused/full/build logs respectively. Temporary evidence may be removed by normal OS cleanup.

The module ownership guard passed on 92 files. Whitespace checks passed.

## Remaining acceptance boundaries

1. Resolve FG-00 interface blockers, then run the normal Web.Tests/API gates and compile the Razor components.
2. Verify persona mapping queries against app RBAC data, including hierarchy/elevation semantics and real provider results. The current slice uses the existing direct assignment service as authority and grants nothing from external role claims.
3. Exercise two authenticated browser circuits, persona switching and disconnected/reconnected UI through real OIDC.
4. Implement immediate invalidation of existing persona/process group membership when app assignments or field permissions change. Currently access is rechecked on subscribe/reconnect and connections expire with authentication; this does not guarantee immediate revocation on an already connected idle session. Keep FG-12 open until that is resolved and tested.
5. Wire audited business publishers and verify recipient selection. No production publisher calls to these helper methods were found in the reviewed source, so passing transport tests is not a claim that all workflow events are now delivered.

Next source work: canonical audit/choke interface ownership under FG-00, followed by FG-11's API-backed standard ASP.NET role bridge and FG-12 revocation handling. Do not use IdentityServer roles as an interim authorization path.
