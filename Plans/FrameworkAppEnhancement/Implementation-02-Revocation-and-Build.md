# Implementation 02: notification revocation and build recovery

Date: 2026-09-21. Follow-up to [implementation 01](Implementation-01-Notifications.md). Status is maintained in the [master tracker](../../MASTER-TODO-TRACKER.md).

## Delivered behavior

Persona and process notification delivery now rechecks current app-owned access for each recipient. The existing hub uses one shared authorization service for subscriptions and delivery. Its in-process SignalR lifetime manager removes memberships that no longer qualify and fails closed when access resolution fails. Unscoped broadcasts are rejected. Multiple authorized group matches produce one delivery per connection.

Roles remain authorization inputs from the application's existing access service. Personas remain workflow context; switching personas grants no roles. IdentityServer remains authentication-only. This work does not implement FG-11's standard role-claim bridge.

Revocation is enforced before the next group delivery, without waiting for reconnect. Idle memberships are not proactively removed by a background task. User-directed notifications still require publishers to select an entitled recipient. Distributed SignalR backplanes are outside this verified scope.

## Build recovery

- Relocated the single audit-chain implementation and its contract to LifeCycle, where process history and report-template consumers live. Removed its UserManagement registration and registered it in LifeCycle, avoiding a reverse project dependency.
- Restored the choke service contract in the existing Models interface location and aligned its implementation and test mock calls, including cancellation tokens.
- Restored process version and step description/SLA metadata already used by existing workflow definitions. Version maps through the existing PPDM mapper; step SLA flows into step instances.
- Fixed connection-name parameter shadowing in work-order accounting and used the generated `R_BA_STATUS.BA_STATUS` property in reference seeders. Generated PPDM entities were not modified.

Resolving the earlier four missing-interface errors exposed 484 deeper compilation errors. Repairing missing workflow metadata and the other items above reduces the latest API build to **10 errors / 43 warnings**. This is still a failed build, not an application validation result.

## Verification

The temporary source-linked project described in implementation 01 now links the production authorization service and lifetime manager as well. **18 tests passed, zero failed or skipped**, using actual SignalR connections and mocked application access services. Added cases exercise connected-client role revocation, field revocation, access lookup failure, process-entity revocation and rejected unscoped broadcasts. The test host uses the production authorization registration extension.

Evidence: `%TEMP%/beep-notification-harness-path.txt` locates the harness; its `TestResults/notifications.trx` contains the latest run. `%TEMP%/beep-notification-revocation-tests.log` records focused tests. `%TEMP%/beep-oilgas-build-current.log` records the API build (5.91 seconds). The module ownership guard passed on 92 files; `git diff --check` passed.

Normal project tests and Razor compilation remain blocked by LifeCycle compilation. These focused results do not establish real database authorization behavior, OIDC/browser behavior or complete host wiring.

## Next implementation gates

1. Repair five accounting compile errors in `PPDMAccountingService` by aligning actual allocation/royalty semantics and contracts, without inventing financial defaults to satisfy compilation.
2. Remove two reverse-dependency errors in `CrossPersonaTaskRouter` through an owner-provided persona resolver; retain a single persona mapping model.
3. Repair three writes to computed seed totals in `LifeCycleSeedService`; track actual inserted records rather than assumed counts.
4. Rerun the API build and normal Web.Tests/API test projects, addressing any newly exposed errors before marking FG-00 complete.
5. Complete FG-11's app-backed standard ASP.NET role bridge, then verify real-provider revocation, two browser circuits and audited business publishers for FG-12.
