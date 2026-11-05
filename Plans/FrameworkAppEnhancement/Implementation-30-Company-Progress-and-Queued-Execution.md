# Implementation 30 — Company progress and queued execution

Date: 2026-10-05. Tracker: CA-03/05/06 and FG-12. Status: focused progress/worker behavior verified; full application and company-isolation phases remain open.

## Canonical progress ownership

The existing IProgressTrackingService now resolves as a scoped service over one singleton CompanyProgressState. The hosted ProgressTrackingPublisher consumes that same store's snapshot channel. The former global singleton service/store/broadcaster is replaced; there is no second progress store or global group alias.

Operations, workflows and operation groups use a structured key containing company scope, account and resource ID. Creation requires the validated RepositoryCompanyContext. Polling and mutations require the same account/company and current membership grants sufficient for the captured creation context. Losing a captured role or permission denies access even while membership remains active. This conservative requirement preserves all grants present at creation; narrowing requirements to specific operation permissions is future policy work, not an implicit permission exemption.

Unknown update IDs cannot create progress entries. Operation groups accept only actual operations owned by the current account/company, reject duplicate identifiers within that ownership boundary, and keep reused group IDs in other companies/accounts separate. Returned models remain snapshots. Terminal operation/workflow/step state cannot be overwritten by late updates. Workflow tracking cancellation handles the owned workflow and pending/running steps rather than silently searching only the operation store.

The workflow cancel endpoint returns not-found for an unavailable workflow and describes tracking cancellation accurately. Tracking cancellation does not interrupt a repository command already executing. PPDM workflow execution still returns its existing explicit unavailable response; this slice does not implement missing workflow step handlers.

The unused generic `/api/ppdm39/setup/progress/{operationId}` route and its progress dependency were removed. Setup has no producer for these company operation records and has a separate schema/execution tracking path. Business operation polling remains in its business controller, where company middleware applies; no repository-admin progress alias bypasses that boundary.

## Publication and delivery

ProgressHub checks ownership before subscriptions. CompanyHubGroups defines the same tenant/company operation/workflow keys for subscription and publication. Published envelopes carry the immutable owner/access snapshot and a serialized payload; transport unwraps the payload only after authorization, preserving existing client payload shapes.

CompanyHubLifetimeManager is extracted from the existing workflow notification lifetime manager. Workflow and progress transports extend this one shared delivery path. It verifies the validated handshake, rechecks current company access in a new DI scope on group admission and delivery, rejects unscoped/direct broadcast alternatives, and removes denied subscriptions. ProgressHubLifetimeManager additionally checks resource ownership, captured grants and the envelope's account. A reused group ID in the same company cannot deliver another account's payload. Membership/role/permission loss and lookup failure stop delivery; restored access requires an explicit rejoin after subscription removal.

## Queued execution

ScopedBackgroundOperationQueue admits work only from the immutable company feature installed by validated request middleware. Headers alone and missing request/company context are refused. Work/status keys include tenant, company and account; identical caller keys do not collide across those boundaries.

The queue captures the immutable owner before request disposal, creates a fresh worker scope, and restores current membership and captured roles/permissions through RepositoryCompanyContextResolver before resolving/executing the business handler. Revoked authority fails the job before its handler executes. Error status contains the registered failure reference. Polling without the current validated owner cannot read job status.

This queue currently has one production admission caller: CSV import. Repository/schema setup uses its existing execution orchestration; it does not receive fabricated company ownership. Failure-report callbacks are explicit required constructor dependencies wired to the existing IFailureReporter in Program. They allow focused compilation of the actual workers without reconstructing absent shared diagnostics contracts.

Existing API worker/import tests were updated for the scoped tracker, hosted publisher and validated company fixtures. Those wider test-project changes are source integration work; they were not run through the missing full host dependencies.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true -p:GeneratePackageOnBuild=false -p:UseBeepDMSource=false --logger "trx;LogFileName=company.trx" --results-directory TestResults/company
```

**155 passed, zero failed/skipped:** previous 128 plus 27 company-progress/worker cases. The focused target compiles the actual progress store, scoped tracker, hub, hosted publisher, queue, shared lifetime manager, progress lifetime manager, restoration helper and existing model/interface sources. Beep Entity and framework components come from real packages/framework references; missing SDK contracts were not recreated.

New evidence covers tenant/company/account isolation for reads and every operation mutation, workflow steps/completion/cancellation, unknown-ID refusal, snapshots, reused group IDs, retention, tenant-only refusal, membership/role/permission revocation, lookup failure, fresh worker grants, structured queue status/admission keys, request disposal, missing-feature/header-only refusal, and preventing revoked handlers from executing. Four scenarios exercise the hosted publisher through the actual SignalR lifetime manager with JSON protocol and in-memory pipes, checking payload delivery, envelope removal, ownership, revocation and required rejoin. These are actual transport writes, not live network/browser/OIDC tests.

`git diff --check` passes. Published BeepDM packages are used; the pre-existing missing analyzer warning remains. No handwritten SQL, schema change or live migration is introduced by this slice.

## Open gates

- Full API/Web DI, HTTP controllers, socket reconnect and browser behavior, including independent repository administration and selected-company roles.
- Complete workflow notification integration following extraction of the shared lifetime manager; resource/field ownership and confidential payload requirements remain part of the wider isolation audit.
- CSV/domain module connection selection and physical database alias ownership, provisioning, filters, caches and other background consumers. Company-owned progress does not prove company-owned business database access.
- Durable jobs/state, restart recovery, multi-node delivery/backplanes and cancellation within provider commands. Progress state and work admission remain in-process.
- Phase A closure and subsequent finance/engineering/operations/regional release features. Continue with physical connection identity/ownership and the domain connection audit before claiming multinational isolation or completing dependent business slices.
