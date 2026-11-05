# Company hub scopes and notification delivery

Date: 2026-10-05. CA-03/05/06 and FG-12 remain partial. Extends [client selection and request scope](Implementation-28-Company-Selection-and-Client-Scope.md). No schema change, handwritten SQL or live database migration was introduced.

## Implemented boundary

CompanyRequestMiddleware now attaches its validated immutable RepositoryCompanyContext as a typed HTTP feature. CompanyHubFilter reads that feature at connection establishment and every invocation, resolves the authoritative account through the existing identity owner, rechecks current application membership and restores the selected company into the invocation's DI scope. Raw request headers do not substitute for the feature. Missing scope, another account or lost membership aborts the connection before hub work runs. The filter does not rebind an existing connection to another company; switching requires a new handshake.

CompanyHubGroups defines one canonical tenant/company channel grammar for user, persona/field, process, operation and workflow keys. Identical user/resource IDs in different tenants or legal entities produce different group names. Identifiers are encoded, parsed and checked for exact canonical form. Global `user:...`, `process:...`, legacy persona keys and malformed/ambiguous aliases are refused; no compatibility group is joined or published.

WorkflowNotificationHub takes its account and scope from the freshly restored repository context. Its existing user, persona and process subscriptions use company-bound groups. Resource authorization remains in the existing WorkflowNotificationAuthorization path. Persona/role decisions now use that company's current roles, rather than repository/global role listings. Process access requires the selected account and the existing field/entity access checks.

WorkflowNotificationLifetimeManager continues to extend the existing SignalR in-process transport. It records the immutable validated handshake, validates group ownership, and rechecks membership in a new service scope before every delivery, including the user channel. Persona/process delivery runs the existing resource checks after restoring the exact scope. Lost membership or failed authorization removes the subscription; failed lookups are reported and do not reuse prior grants. Required roles come from the fresh company snapshot.

Publishers must explicitly supply CompanyScope. User notifications additionally declare a nullable required-role argument: a role restricts the message to a current grant; explicit null represents a generic membership-only message. Persona notifications require a role and field; process notifications retain current resource checks. The existing role envelope is unwrapped only after authorization. Unscoped broadcasts, direct connection/user sends, unsupported group kinds and missing delivery envelopes are refused, preventing alternate transport entry points from bypassing the channel decision. Business publishers still must resolve payload/resource ownership and declare the appropriate requirement; a generic membership-only message is not a substitute for authorizing confidential business data.

The API registers the company filter with SignalR. Existing notification test-host publishers and clients were updated to supply scope and use current membership fixtures. Those complete host/lifetime-manager paths are source-reviewed, not executed by the focused target.

## Historical progress integration gap

Superseded by [Implementation 30](Implementation-30-Company-Progress-and-Queued-Execution.md): the canonical store, publisher, owner checks and queue scope are now updated, with focused transport/worker evidence. The following records the boundary at Implementation 29.

ProgressHub now requires authorization and uses tenant/company operation and workflow names. It no longer joins an arbitrary global operation ID. **ProgressTrackingService still stores and publishes global keys.** Consequently its old broadcasts no longer reach the newly scoped subscriptions. This is a temporary incomplete integration, not a working scoped-progress claim. The next required change must extend the existing store, publisher, polling and cancellation paths together with operation/workflow owner checks and queued company execution context. Do not restore global group aliases to bridge this gap.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true -p:GeneratePackageOnBuild=false -p:UseBeepDMSource=false --logger "trx;LogFileName=company.trx" --results-directory TestResults/company
```

**128 passed, zero failed/skipped**: previous 104 plus 24 hub-group, membership/role-delivery and filter cases. The focused target compiles actual CompanyHubGroups, CompanyHubScope, CompanyHubFilter and the updated CompanyRequestMiddleware alongside the previous production access/routing/client sources. It uses real framework hub contexts and a mock of the existing authoritative access gateway; no missing SDK contracts were recreated.

New tests cover every channel kind, identical identifiers across tenant/company scopes, strict encoding/canonical parsing, rejected global/malformed keys, delivery membership rechecks, role revocation for persona and user messages, generic membership messages, resource denial, missing handshake, another account/user channel, lookup failure, invocation abort after revocation, header-only refusal, and a validated HTTP middleware feature rebinding a separate hub scope with current roles. Existing persistence/access cases continue to verify the gateway's actual EF behavior, including expiry/disabled accounts and scoped grant lifecycle.

The complete WorkflowNotificationHub/Authorization/LifetimeManager, Program wiring, ProgressHub and network fixture were not compiled/executed by this focused target because they depend on the wider host sources. The passing gate/filter cases do not prove socket delivery, multi-node behavior or provider concurrency. The previously missing analyzer warning remains; published BeepDM packages are used. `git diff --check` passes.

## Remaining work

- Finish progress ownership, scoped stores/publication, polling/cancellation, queue scope restoration and revocation. Verify live progress recovery as one canonical path.
- Execute full notification transport tests: simultaneous tenants/legal entities, current resource ownership, role/membership/field revocation, reconnect, failure handling and direct-send refusal. Review distributed transports before enabling scale-out.
- Ensure every business publisher derives scope and delivery requirements from authoritative records, including field/process ownership within a tenant. No production caller of these notification helper methods was found in this slice; completing workflow integrations remains part of the broader plan.
- Complete physical database alias ownership, provisioning and the remaining direct-connection/filter/cache/worker audit.
- Run real browser/OIDC and provider gates before promoting multinational isolation or company release readiness.

This is verified gate/filter behavior with source integration changes. No complete isolation phase or release is claimed.
