# Framework and application review

Reviewed 2026-09-21 at source baseline `cd2e4511`; initial working tree was clean. Inspection counted 62 project files, including 19 test projects, excluding bin/obj. All declared project-reference targets were present on this machine; that does not prove a clean CI checkout has them.

## Existing strengths

The solution already separates Web, API, Client, LifeCycle, PPDM data management, and engineering modules. Preserve `PPDMGenericRepository`, `ModuleSetupOrchestrator`, the field orchestrator, the existing calculation services, and the workflow engine. There is substantial controller, authorization, reference-seed, and numerical regression test source. API controller mapping requires authorization globally in `Beep.OilandGas.ApiService/Program.cs`; this is useful authentication coverage, not proof of resource-level authorization.

## Findings

Paths below are relative to the repository root. Severity represents implementation priority, not a claim that a runtime exploit or numerical incident was reproduced.

| ID | Priority | Observed evidence | Implication and planned response |
|---|---|---|---|
| R01 | P0 | `AGENTS.md` names TheTechIdeaWeb.ApiService, TheTechIdea.Data, BeepDiA and AppHost; these are not projects in this solution. `.github/copilot-instructions.md` describes the local Beep/PPDM architecture. | Resolve deployment and contract ownership explicitly before moving models. FG-01; user-supplied architecture rules remain governing. |
| R02 | P0 | `Beep.OilandGas.Web/Program.cs` requests `roles` and sets `RoleClaimType = "role"`. No `IClaimsTransformation` match was found in Web/API source. `ApiService/Attributes/RequireRoleAttribute.cs` implements a custom role filter backed by `IAccessControlService`. | Implement the mandated API-backed standard role bridge and migrate role gates without weakening independent API/resource checks. External shared auth dependencies were not exhaustively audited. FG-11. |
| R03 | P0 | Web calls `AddBeepOilandGasAppAuto`; `Client/DependencyInjection/ClientServiceCollectionExtensions.cs` can select local mode and constructs a service provider during registration. | The configured Web path can bypass the intended HTTP-only boundary. Actual database activity was not exercised. Make Web remote-only and audit dependencies. FG-10. |
| R04 | P0 | Web registers `INotificationService` as singleton. `Web/Services/NotificationService.cs` holds a mutable list, unread count and one hub connection; later starts return if a connection already exists. | User-specific state can be shared across circuits. Scope ownership and disposal per user/circuit; verify concurrent sessions. FG-12. |
| R05 | P0 | `ApiService/Hubs/WorkflowNotificationHub.cs` has `[Authorize]`, but its subscription methods add caller-provided user, persona and process identifiers directly to groups. | Authentication alone does not validate access to a requested group. Derive identity and check resource membership before joining. Runtime exposure not tested. FG-12. |
| R06 | P1 | NotificationService uses a relative hub URL without an access-token provider in the inspected builder; exceptions are swallowed. `Web/Services/UnifiedTaskInboxService.cs` converts failed requests into empty inbox/counts. | Connectivity failures can appear as no work. Use the existing authenticated API client infrastructure and visible failure states. FG-13/FG-50. |
| R07 | P1 | `LifeCycle/Services/Calculations/PPDMCalculationService.PropertyHelpers.Forecasting.cs` supplies defaults for pressure, permeability, thickness and other inputs, and fixes wellbore radius at 0.25. | A result can depend on assumptions that need explicit units, provenance and user acceptance. Do not infer all results are wrong; distinguish measured, derived and assumed inputs. FG-30. |
| R08 | P1 | `Web/Components/Dashboard/ProductionChart.razor`, `Data/EnhancedDataGrid.razor`, and other components contain style blocks; `Workflow/WorkflowDagVisualizer.razor` and `Reservoir/ReservesChart.razor` contain literal colors. | Consolidate CSS and adopt branding/Mud palette tokens; test dark mode, RTL and readable chart states. FG-51. |
| R09 | P1 | `.github/workflows/ci.yml` builds the solution, runs ApiService.Tests, and runs the ownership guard only after build succeeds. Web references ThemeBranding, IdentityServer.Shared and Beep.Razor.Components outside this repository. | Reproduce dependency acquisition on a clean worker, run fast guards independently, and cover the other engineering test projects. FG-00/FG-60. Local references currently exist. |
| R10 | P1 | Root tracker was facility-only. `Plans/workflow-rbac-master-plan.md` contains both Design Phase and 71/71 complete claims. `Plans/ENHANCEMENT_ROADMAP.md` proposes a separate DCA project while Arps source exists under `ProductionForecasting/DCA/AdvancedDeclineMethods/ArpsDeclineMethods.cs`. | Treat historical completion claims as historical until reverified; extend existing DCA rather than recreating it. FG-02. |
| R11 | P1 | `ApiService.Tests/FacilityMonitoringControllerTests.cs` directly constructs controllers with mocked facility services. Facility phase 2 still requests PDEN/volume integration coverage. | Preserve those unit tests and add real HTTP/auth/persistence round trips; mocked controller success does not establish middleware or database behavior. FG-22/FG-61. |
| R12 | P1 | `Web/Services/UnifiedTaskInboxService.cs` declares inbox/task contracts, while shared contracts also have established model/PPDM/module homes. | Inventory semantic ownership before relocating or adding contracts. This observation alone does not establish duplicate type definitions. FG-01/FG-13. |

## Review coverage and limits

Inspected startup/auth registration, client mode selection, notification and inbox implementations, calculation input mapping, representative UI components, CI, project references, existing plans, and representative facility tests. Inventory covered project and tracker files broadly; individual accounting standards, every correlation, database provider, route and Razor page were not audited method by method.

No authenticated app session, database migration, provider round trip, load test, or independent petroleum engineering benchmark was executed during this documentation task. Existing module checkmarks are not new verification evidence. Candidate capabilities in later phases are proposals until their acceptance gates pass.

Build probe and documentation-validation outcomes are recorded in the root tracker's review evidence section. A failed no-restore probe may reflect stale local assets; rerun with a controlled restore before attributing every failure to source.
