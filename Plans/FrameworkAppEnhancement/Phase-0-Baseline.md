# Phase 0: reproducible baseline and ownership

Dependencies: none. Responsible areas: solution/build maintainers and framework/API owners. Findings: R01, R09, R10, R12.

| Task | Deliverable | Acceptance |
|---|---|---|
| FG-00 | Record SDK, package feeds, external project acquisition, configuration prerequisites and solution membership; restore/build on a clean worker. | A second machine can restore/build using documented inputs, or every remaining failure is recorded with command, project and first actionable error. No feature project is excluded to manufacture a green build. |
| FG-01 | Ownership/deployment decision mapping local API and contracts to canonical ApiService/TheTechIdea.Data and external BeepDiA. Inventory generated PPDM entities versus business extensions. | Every shared contract family and database-writing entry point has one owner; migration ordering and consumers are listed; no duplicate models or alternative gateway are proposed. |
| FG-02 | Reconcile legacy plan checkmarks with source and test evidence; link module trackers to root execution rows. | Historical claims remain labeled as such; implemented source is distinguished from verified behavior; stale paths and already-built features are identified. |

## Implementation steps

1. Establish an explicit external-dependency acquisition mechanism supported by the organization. Local path existence is not enough for CI. Reuse the existing repositories/packages rather than copying their source into this repository.
2. Capture a restored Release build baseline and run test discovery for all test projects. Separate environment failures, compiler failures and assertion failures.
3. Review startup composition and contract ownership with the actual platform project locations. Keep AGENTS.md rules; do not rewrite them to match accidental implementation drift.
4. Add source-linked verification records to the existing trackers. Revisit the facility Permits build claim against the new baseline.

## Verification and exit

Run `dotnet restore Beep.OilandGas.sln`, then `dotnet build Beep.OilandGas.sln --no-restore -c Release`. Run `Scripts/ci-guard-module-ownership.ps1` independently. Capture commit, SDK, configuration, exit code and log artifact. Restore may need approved feed credentials supplied by the environment; never put credentials in plans or logs.

Exit when the baseline is reproducible and ownership is resolved. A documented blocker is useful evidence but does not complete the affected build or ownership gate. Dependencies outside this repository require a concrete integration decision before associated code work, while local notification isolation work can proceed.
