# MASTER TODO TRACKER - PPDM39 Setup, Consolidation, and MigrationManager

## Scope
- Consolidate duplicate PPDM39 contracts and ownership between `Beep.OilandGas.PPDM39` and `Beep.OilandGas.PPDM39.DataManagement`.
- Make MigrationManager the only schema create/update authority for setup and demo paths.
- Remove legacy setup routes; route schema changes through Beep Migration.

## Phase Status
- [x] Phase 01 - Setup Surface And Contracts (✅ COMPLETED 2026-04-25)
- [x] Phase 02 - Connection And Environment Foundation (✅ COMPLETED 2026-04-25)
- [x] Phase 03 - Migration Manager Schema Creation (✅ COMPLETED 2026-04-25)
- [x] Phase 04 - Wizard Orchestration And UX (✅ COMPLETED 2026-04-25)
- [x] Phase 05 - Demo, Seed, And LocalDB Lifecycle (✅ COMPLETED 2026-04-25)
- [x] Phase 06 - Validation, Governance, And Rollout (✅ COMPLETED 2026-04-25)

## Cross-Phase Critical Findings To Resolve
- [ ] Remove contract duplication drift (`IPPDMRepository`, metadata/defaults/tree contracts, DTO placement).
- [ ] Remove script-first schema assumptions from demo/setup orchestration.
- [ ] Eliminate sync-over-async hotspots (`GetAwaiter().GetResult()`) in runtime setup/data paths.
- [ ] Replace non-refreshable metadata cache behavior in `PPDMMetadataService`.
- [ ] Reduce repeated `Type.GetType` and repeated `new PPDMGenericRepository(...)` patterns.

## Deliverable Ledger
- [ ] Canonical setup contract set under `Beep.OilandGas.Models/Data/PPDM39Setup`.
- [ ] Canonical schema orchestration service with plan/policy/preflight/approve/execute/resume.
- [ ] Canonical wizard route and compatibility redirect map.
- [ ] Demo/seed/localdb lifecycle policy integrated with migration orchestration.
- [x] CI and operations artifact model with plan hash and approval provenance. (✅ COMPLETED 2026-04-25)

## Dependency Order
1. Phase 01 must complete before endpoint and wizard stabilization.
2. Phase 02 must complete before production-safe phase 03 orchestration.
3. Phase 03 must complete before phase 05 demo lifecycle migration off scripts.
4. Phase 04 depends on finalized contracts and orchestration endpoints.
5. Phase 06 validates all prior phases with governance and CI gates.

## Definition Of Done
- [ ] No setup path creates schema outside MigrationManager orchestration.
- [ ] All setup and wizard payloads use typed shared contracts.
- [ ] No uncontrolled contract drift between PPDM39 and DataManagement.
- [x] Setup operations produce auditable artifacts with masked sensitive values. (✅ COMPLETED 2026-04-25)

2026-10-29: Administrators can select an effective mapping into a company-only draft while retaining inherited source identity; an explicit module change preserves existing mapping overrides. Three focused OilandGasTools/editor and nine screen/validator cases passed. Live source semantics, operational activation, connector distribution and hosted setup remain open; see FrameworkAppEnhancement/Phase-2-Data-and-Setup.md and Feature-First-Run-Company-Environment-Setup.md.

2026-10-29: The focused company inbox HTTP test passed with different personas for one account in two companies. Corrected two investment routing persona lookups to use the selected company; investment workflow checks are running. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.

2026-10-29: The focused investment assigned-source test passed with a different persona for the same reviewer in another company; current authority revocation was also checked. Stage decision verification remains in progress with controlled provider boundaries.

2026-10-29: The existing focused two-stage investment HTTP decision test passed after the company-scoped lookup fix; its expanded two-company persona case is running. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.

2026-10-29: First-time identity connection setup now directs users to check the catalog after an unconfirmed create before retrying. One focused controlled screen test passed; hosted recovery remains open. See FrameworkAppEnhancement/Phase-2-Data-and-Setup.md.

2026-10-29: The expanded two-company persona investment stage decision test passed, including next-stage recipient lookup and final outcome with controlled provider boundaries. Hosted and real-provider verification remain open.

2026-10-29: Source-built Beep SQLite projected reads retain explicit date markers. A focused Beep-migrated test passed UTC intent reconstruction, unmarked effective-date preservation and rollback. Full incident command, jurisdiction and workflow recovery remain open; see FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.

2026-10-29: Incident report schema preflight now checks the projected jurisdiction column. A focused Beep-migrated SQLite schema test passed, including a controlled missing-column refusal. Complete SQLite command, jurisdiction semantics and screen connection remain open.

2026-10-29: Nine focused incident command transaction, authority and recovery cases passed with a controlled provider after the physical-schema validator change. The focused build excluded an unrelated identity-controller test with missing source links.

2026-10-29: Actual Beep SQLite incident writer test passed creation, exact retry, changed-original refusal, rollback after authority revocation and retry. Company/binding authority and metadata were controlled; production authority regression is running. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.

2026-10-29: Nine focused production incident-command authority, failure and recovery cases passed after extracting the native writer, using a controlled provider. The separate real SQLite writer test used controlled authority and metadata.

2026-10-29: Added incident intent API, typed receipt validation and protected-session original retention. One direct controller, four controlled client and one protected-session case passed. Screen integration, hosted authorization, jurisdiction and workflow recovery remain open; see FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.

2026-10-29: Incident screen now submits the retained reviewed report and reconciles the same original after reopening. Thirty-six focused controlled screen cases passed. Hosted end-to-end, jurisdiction readback, workflow/history recovery and removal of older server report routes remain open; see FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.

2026-10-29: Removed superseded HSE report POST routes. Seventy-eight focused screen, timestamp, incident-list and hosted test-server cases passed, including company/permission/FIELD gates and 405 on the old routes. The hosted report command was controlled. Jurisdiction readback and workflow recovery remain open.
2026-10-29: First-run driver selection now explains which connectors cannot host identity. One controlled screen test and one fresh-cache public SQLite download and Beep Migration test passed; identity-provider package availability and hosted setup remain open. See FrameworkAppEnhancement/Phase-2-Data-and-Setup.md.
2026-10-29: A focused localhost HTTP test installed the SQL Server connector from an isolated source-built feed and completed identity connection, restart, migration and bootstrap on disposable LocalDB. The setup form clears a submitted password after an unconfirmed response; one controlled component test passed. Public packages, the deployed host, browser and other providers remain open. See FrameworkAppEnhancement/Phase-2-Data-and-Setup.md.
2026-10-29: Removed a connection endpoint that returned success without creating anything; setup now reports Beep registry refusal for connection and SQLite creation. Six focused controlled tests passed. A client build with references disabled failed on duplicate lease types; hosted UI and provider acceptance remain open. See FrameworkAppEnhancement/Phase-2-Data-and-Setup.md.
2026-10-29: SQLite setup also refuses an existing connection name instead of replacing it; a focused controlled collision test passed. See FrameworkAppEnhancement/Phase-2-Data-and-Setup.md.
2026-10-29: Connection updates validate and probe before using the original Beep registry identity instead of removing the existing connection first. Five controlled cases and one ConfigEditor restart test passed; concurrency, persistence failure recovery and actual provider probing remain open. See FrameworkAppEnhancement/Phase-2-Data-and-Setup.md.
2026-10-29: Active PPDM connection details no longer return stored connection strings or passwords; server-side tests retain access to stored credentials. Two focused controlled cases passed; hosted UI and actual provider authentication remain open. See FrameworkAppEnhancement/Phase-2-Data-and-Setup.md.
2026-10-29: Removed script-discovery/execution and script-based create-database setup endpoints and their unused clients; replaced the edit-only script wizard with a tested connection form. Seven migration dispatch cases, one component case and one connection update case passed. API compile is blocked by missing diagnostics references; privilege-probe and drop/recreate SQL paths remain open. See FrameworkAppEnhancement/Phase-2-Data-and-Setup.md.
2026-10-29: Removed the active PPDM setup service direct SQL privilege probe and drop/recreate operations, their API/UI paths, and auto-approved schema shortcuts. Seven migration dispatch cases and one setup-service case passed. Full API, browser and remaining module SQL audits remain open. See FrameworkAppEnhancement/Phase-2-Data-and-Setup.md.
2026-10-29: Removed the unused handwritten PPDM schema-generation endpoint and its generator/contracts. The focused source boundary and seven migration dispatch cases passed, and Models built. DataManager remains in the repository without a current API execution caller; hosted API and other module paths remain open. See FrameworkAppEnhancement/Phase-2-Data-and-Setup.md.
2026-10-29: The safety incident list can search jurisdiction, location and incident ID; list/detail display and FIELD-change clearing passed one focused controlled screen case. A controlled incident command case verified retained-original jurisdiction readback and tamper refusal. Full live-provider authority and hosted browser remain open. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.
2026-10-29: The incident list prevents a reload during workflow submission and requires a fresh company/FIELD review after an unconfirmed response. Two focused controlled component cases passed. Durable intent, concurrent creation, process/state/history atomicity and hosted acceptance remain open. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.
2026-10-29: Added a Data-owned HSE workflow original with deterministic company/database/FIELD/incident identity and checked process-payload restoration. One focused contract case passed and Data built. It is not wired to the current endpoint; lifecycle transaction and recovery remain open. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.
2026-10-29: Added an HSE lifecycle start owner with retained original, separate incident/lifecycle bindings, owned graph/history/task writes and retry validation. Four controlled provider cases and one contract case passed. The current API is not wired; cross-database recovery, configured roles and live provider/hosted acceptance remain open. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.
2026-10-29: Added a canonical HSE incident workflow source review requiring current case/FIELD write authority, ManageIncidents, reported state and acknowledged status projection. Four controlled-provider cases passed; API/lifecycle wiring and live provider recovery remain open. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.
2026-10-29: Fresh-cache public SQLite connector installation, Beep Migration and organization read passed one focused test. Four configured relational connector versions were absent from NuGet.org and their installer cases failed; compatible package distribution and actual provider setup remain open. See FrameworkAppEnhancement/Feature-First-Run-Company-Environment-Setup.md.
2026-10-29: Driver setup now warns when a loaded SQL Server connector lacks the transaction contract required by the shared system repository. Two focused service cases and one bootstrap screen case passed. Connection-level transaction verification and compatible package distribution remain separate gates; see FrameworkAppEnhancement/Feature-First-Run-Company-Environment-Setup.md.
2026-10-29: The HSE workflow coordinator binds company, FIELD and lifecycle reviews to the native owner and validates its receipt. Four focused cases passed with controlled incident and lifecycle owners; API exposure, client recovery, provider and hosted verification remain open. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.
2026-10-29: Added protected session retention for the exact company/FIELD/incident workflow original; two focused storage cases passed. Screen and API integration, cross-process recovery and provider acceptance remain open. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.
2026-10-29: Registered reviewed HSE workflow endpoints and typed client. Five focused direct-controller cases and five client cases passed; incident screen adoption and removal of the old start route remain open. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.
2026-10-29: Incident list and detail now use reviewed HSE workflow start and retained-original recovery; removed the unreviewed start endpoint/client. Three focused control cases and two page cases passed with controlled clients. Hosted, concurrent and live-provider recovery remain open. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.
2026-10-29: Seven focused localhost HSE workflow cases passed through the actual controller, FIELD filter, company selection and incident-source review with controlled identity, access, provider and lifecycle boundaries. Real provider, deployed authorization and cross-database recovery remain open. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.
2026-10-29: Three focused localhost HSE cases connected the real incident reviewer and lifecycle owner through the API, verifying graph creation, simulated rollback and lost-commit retry with a controlled Beep datasource. Real provider, cross-database and hosted acceptance remain open. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.
2026-10-29: One focused real Beep SQLite HSE lifecycle test passed: Beep Migration created the schema, authority revocation rolled back the graph, and start/retry retained one process, history and task. Incident authority and persona callbacks were controlled; cross-database, concurrency and hosted checks remain open. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.
2026-10-29: HSE workflow reads and actions now use company/FIELD and database ownership checks. Eight focused ownership cases and 28 controlled controller/list and HTTP authorization cases passed; real provider, concurrency and deployed acceptance remain open. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.
2026-10-29: The in-progress RCA incident owner writes the reviewed cause and retained original in one owned transaction; six focused controlled-provider cases passed for write failures, changed authority and references, retry and unconfirmed commit. Screen integration, lifecycle coordination, cross-database recovery and live-provider acceptance remain open. See FrameworkAppEnhancement/Company-Persona-Data-Workflows.md.
