# Remote master reconciliation — 2026-09-21

## Baseline and recovery

Integrated upstream commits `3666b17f` and `c6754410` by fast-forwarding master from `cd2e4511` to `c6754410`, then reconciled the local enhancement work. No source commit or push was made for the reconciled changes.

The complete original tracked and untracked work remains recoverable in stash `4e2fddaaca03cc2dc5abf0a2d7641e59b1977404` (`codex pre-reconcile local enhancements 2026-09-21`). Keep this checkpoint until the reconciled changes are committed and reviewed. Do not blindly reapply it over the reconciled tree.

Upstream changed 490 files; 44 locally modified paths overlapped and two new interface paths collided. Reapplication produced 23 conflicted paths. Resolution considered functional differences as well as conflict markers.

## Implementation decisions

| Area | Canonical reconciled implementation |
|---|---|
| Authentication and authorization | Keep upstream Repository and TheTechIdea.Data identity extensions, standard ASP.NET role bridges, and independent endpoint checks. IdentityServer authenticates only. Use the local NameIdentifier for API authorization and notification recipients; keep external sub for Web token retrieval. |
| Personas and task routing | Keep upstream repository persona catalog/profile, typed PersonaClient, and RepositoryRolePersonaReader. Remove our superseded PPDM persona-role resolver. Selecting a persona grants no role. |
| Notifications | Keep our scoped circuit state, token handling, reconnect cleanup, bounded/deduplicated history, visible failures, authenticated subscriptions and delivery rechecks. Resolve active persona profiles from the repository. Persona payloads must declare a required app role; check each recipient's current role independently of persona selection. Reject unscoped persona payloads. Keep token-expiration disconnects. |
| Cost allocation | Keep upstream COST_TRANSACTION/COST_CENTER pipeline and its four configured allocation methods. Add our rejection of empty input, duplicate/missing transaction IDs, negative costs and invalid dates. Remove the alternate ACCOUNTING_COST direct calculator. Preview is read-only: retain removal of the synthetic COST_ALLOCATION write and reject caller-supplied total overrides. UI exposes the configured calculation methods. |
| Royalty | Keep our single recorded-input calculation path with explicit read-only preview; no invented rate, price or deduction defaults. Keep upstream bound-module repository resolution and journal integration. Retire the old calculate facade/DTO and update the typed client/page. Posting transactionality/idempotency and reporting field mappings remain separate open work. |
| Volume reconciliation | Keep comparison of independently measured run tickets with their allocation results, inclusive date filters, explicit missing/ambiguous-data errors and nullable zero-denominator percentages. Do not restore the upstream self-comparison that always matched. |
| Seeding | Keep upstream bound LIFECYCLE connection and repository permission reader, accurate per-category counters, and our repeat-run counts, cancellation, real table counts and propagated seed failures. One count-returning seed method replaces parallel wrappers. |
| Module interfaces/build/setup | Keep upstream restored contracts and service registrations, shared audit contract, BeepSync fixes and new repository setup. Remove local duplicate interfaces and do not resurrect the deleted setup wizard. Preserve complementary workflow version/SLA mapping fixes. |
| Documentation | Earlier implementation notes describe the old baseline. This document and the master tracker supersede their current-state/build and persona/direct-cost architecture claims. |

## Verification

- API project build: passed, zero errors (77 warnings in the recorded build).
- Web build: passed as part of the real Web.Tests and Web.Auth.Tests project runs.
- API tests: **889 passed, 10 skipped, 0 failed**, 899 total. `Beep.OilandGas.ApiService.Tests/TestResults/reconciled-api.trx`.
- Additional repository-backed persona reader regression: focused PersonasAuthorizationTests **6 passed**, including a new SQLite test for local identity, current persona and disabled users/catalog entries. `reconciled-personas.trx`. Five tests overlap the full API run; do not add the totals together.
- Web notification tests: **21 passed**, real loopback host and SignalR with mocked resource checks. Includes same-persona/different-role exclusion, unscoped-payload rejection, changed persona, external/local ID separation, revocation and circuit isolation. `Beep.OilandGas.Web.Tests/TestResults/reconciled-notifications.trx`.
- Web authentication tests: **91 passed**. `Beep.OilandGas.Web.Auth.Tests/TestResults/reconciled-auth.trx`.
- Lifecycle cost allocation tests: **13 passed**, all four methods plus incomplete/invalid input and no writes. `Beep.OilandGas.LifeCycle.Tests/TestResults/reconciled-lifecycle.trx`.
- Module ownership guard: passed, 109 module files scanned.

The first API test attempt encountered a corrupt generated testhost and adapter. Replaced generated test-runtime binaries with the same-version working Web.Tests copies; no product behavior or tests were bypassed. The successful API run then executed the full suite. Three old mocks were updated to return explicit successful datasource operation results, as required by upstream repository hardening.

## Remaining limits

The 10 skipped API cases require LocalDB and were not executed. Real OIDC/browser, full live-database workflows, distributed SignalR delivery and release acceptance remain open. Existing build warnings include SkiaSharp version conflicts, Razor analyzer warnings and an unresolved external ThemeBranding project path; passing incremental project builds do not prove a clean external-dependency checkout. No full-solution or clean-CI certification is claimed.
