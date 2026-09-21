# Phase 1: API access, roles and notification isolation

**Confirmed requirement:** the oil and gas app owns RBAC. IdentityServer performs user authentication only. Roles, permissions and assignments remain in the app's own RBAC data; the API exposes and enforces that authority, and Web consumes it through the standard role bridge. No IdentityServer role provisioning, role synchronization or authorization policy is part of this plan.

Dependencies: FG-01 for shared contract/deployment changes; use FG-00 baseline. Responsible areas: Web, Client, ApiService, UserManagement and external platform integration. Findings: R02-R06, R12.

| Task | Work | Acceptance |
|---|---|---|
| FG-10 | Replace Web auto/local service selection with explicit authenticated HTTP registration. Audit facade consumers and remove Web database-capable execution paths. | Web startup cannot select local data access through configuration or service discovery. API outage produces an error, not a switch to local services. Architecture checks prevent reintroduction. |
| FG-11 | Add the read-only current-user roles API and Web `IClaimsTransformation`; source roles from app RBAC, strip untrusted external role authority, remove OIDC roles scope dependence. Migrate custom role-only filters to standard guards. | `[Authorize(Roles=...)]`, `AuthorizeView` and `IsInRole` agree; API independently resolves/enforces its authoritative roles. Missing roles, lookup failure and expired elevation deny access. A request marker avoids duplicate upstream lookup. |
| FG-12 | Scope notification state to the user/circuit; configure authenticated absolute API hub address, disposal and reconnect. Derive user group from principal; authorize persona/process groups before joining. | Two concurrent users cannot share notification state or join unauthorized groups. Disconnect/logout disposes resources; reconnect restores only currently authorized subscriptions. |
| FG-13 | Consolidate inbox and notification transport/contracts through the existing typed client path and FG-01 owner. Encode query parameters and preserve error details safely. | HTTP and hub calls reach the same configured API with user credentials; unauthorized/unavailable responses are distinguishable from empty data; no second DTO family is created. |

## Required behavior

Roles answer app-level role membership. Field and asset checks still constrain individual resources independently; do not remove them when standardizing roles. Client-selected field/persona identifiers are context, not proof of permission. The current-user roles endpoint returns only the authenticated user's roles and must not recursively depend on the role bridge it supplies. Define refresh/revocation behavior for long-lived Blazor circuits and expiring role elevations; do not persist role authority indefinitely in a cookie or singleton cache.

For hubs, user identity comes from authenticated claims. Persona and process subscriptions use the existing access services and include field/resource scope. Do not merely compare a user-supplied persona string. Review publisher targeting alongside subscription changes so restricted payloads are not broadcast to broad persona groups.

## Tests and rollout

Extend existing `RequireRoleAttributeTests`, `RequireCurrentFieldAccessAttributeTests` and `RequireAssetAccessAttributeTests` while migrating their relevant expectations to the standard role path. Add HTTP-host tests proving 401/403 outcomes and role lookup failure behavior. Add hub tests for forged user IDs, foreign processes, invalid personas, revoked permissions and reconnect. Add a two-circuit UI test for separate lists/unread counts.

Include two explicit role-authority regressions: a valid authenticated token with no role claims still receives the roles assigned in the app; a token carrying an external `Admin` role receives no Admin access when the app has not assigned it. Changing/revoking app assignments must affect authorization according to the documented refresh policy without changing IdentityServer configuration.

First migrate one protected page plus its API endpoint and verify both; then apply the same registration to remaining consumers and retire replaced role paths. Deploy API support before switching Web clients. Preserve additive public contract versioning only for identified external consumers. Exit requires negative authorization tests and the two-user isolation check; successful login alone is insufficient.
