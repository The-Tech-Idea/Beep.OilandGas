# Phase 6: delivery gates and release evidence

Dependencies: FG-00; final release gate depends on applicable earlier phases. Responsible areas: CI/build, API operations, test owners and app owners. Findings: R09, R11.

| Task | Deliverable | Acceptance |
|---|---|---|
| FG-60 | Extend existing CI to cover engineering test projects, run ownership/boundary guards independently, and provision external dependencies reproducibly. | A clean worker builds the intended solution and publishes test/build artifacts; no excluded project or skipped test is presented as verified. |
| FG-61 | Add an authenticated integration/browser suite using disposable provider fixtures and two identities/fields. | Critical journeys exercise middleware, persistence and UI; negative access, replay, recovery and unavailable-service cases pass. |
| FG-62 | Define operational readiness: health checks, structured correlation, latency/failure metrics, backup/restore and deployment rollback. | A staging rehearsal shows degraded dependency reporting, traceable failed operation, restore validation and compatible rollback using recorded artifacts. |

## Verification matrix

| Gate | Command or scenario | Required evidence |
|---|---|---|
| Solution | `dotnet restore Beep.OilandGas.sln`; `dotnet build Beep.OilandGas.sln --no-restore -c Release` | SDK, external dependency versions, commit, exit code and log. |
| API regression | `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj -c Release --logger trx` | Discovered/executed/pass/fail/skip counts and TRX. |
| Engineering | Run `dotnet test <existing-module.Tests.csproj> -c Release --logger trx` for each changed/released module. | Named fixtures, tolerances, tests executed and result artifacts. |
| Ownership | `pwsh -File Scripts/ci-guard-module-ownership.ps1 -RootPath .` | Actual exit code and output; broaden with FG-01 decisions where appropriate. |
| Boundaries | Proposed automated guards for Web direct-data dependencies, unapproved duplicate contracts and role/theme rules. | Rule scope and targeted exceptions documented; guards must inspect behavior-relevant registrations, not just names. |
| Data | Fresh install, repeat install, failure/recovery and persisted facility round trip. | Provider/schema version and isolated fixture results. |
| App/security | Role bridge + two-user hub test + two-field business journey. | HTTP/hub access decisions and browser assertions. |
| Operations | Staging backup/restore, failed dependency and rollback rehearsal. | Runbook, observed outcomes and unresolved limitations. |

## Release rules

`Planned` means no implementation claim. `In progress` means an active source change. `Implemented, unverified` requires a source reference but has not passed the relevant gate. `Blocked` identifies a concrete dependency and next action. `Verified` requires a command/scenario, date, commit and successful outcome. `Historical` preserves an older claim without implying current validity.

Every promoted feature needs API authorization, persistence where applicable, typed client consumption, observable failure handling and appropriate regression coverage. Run the smallest meaningful checks while implementing, then the release suite once integrated. Do not rerun broad suites without a change or failure that justifies it.

Structured logs should identify operation/correlation, field and failure category without recording tokens or unnecessary personal data. Long operations should expose progress and cancellation outcome from existing setup/workflow infrastructure. Define latency and throughput budgets against realistic field/well/time-series data and measure them before setting service-level claims.
