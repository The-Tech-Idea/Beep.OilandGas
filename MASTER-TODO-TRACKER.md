# MASTER-TODO-TRACKER — Oil & Gas framework and application

Canonical cross-project execution tracker. Updated 2026-09-21 after reconciling remote master `c6754410` with local enhancements (original review: `cd2e4511`). Module trackers retain their detailed checklists; this file owns framework/app priorities and verification status. Historical facility work is preserved below.

## Review package

- [Enhancement plan and reading order](Plans/FrameworkAppEnhancement/README.md)
- [Source review, findings and limitations](Plans/FrameworkAppEnhancement/01-Review-and-Evidence.md)
- [Architecture decisions and alignment with existing plans](Plans/FrameworkAppEnhancement/02-Architecture-and-Plan-Alignment.md)

The review and planning deliverables are complete. Implementation has started with FG-00 build repairs and FG-12 notification isolation. See [latest implementation evidence](Plans/FrameworkAppEnhancement/Implementation-23-Field-Bound-Production-API.md). No full phase or release is complete.

**Confirmed development policy:** No legacy compatibility requirement. Refactor to one canonical implementation, update consumers together, and remove obsolete contracts; no compatibility shims or invented financial defaults. Architectural choices are delegated; use research and record evidence.

**Confirmed authorization requirement:** RBAC is owned by the oil and gas app itself. IdentityServer is authentication-only. FG-11 reads roles from the app's own RBAC through its API, uses standard ASP.NET role checks in Web, and independently enforces API access. IdentityServer roles are never application authorization authority; this boundary is not pending a decision.

## Current verification evidence — 2026-09-21

| Check | Outcome | Boundary / next action |
|---|---|---|
| Baseline | Master includes remote `c6754410`; local enhancements reconciled | Original work retained in stash `4e2fddaaca03cc2dc5abf0a2d7641e59b1977404`; no unresolved conflicts. |
| API build | **Passed**, API test build after field-bound dashboard integration; existing warnings remain | Clean CI/full-solution and external dependency acquisition remain open. |
| Web build | **Passed**, production dashboard/client build: zero errors; 160 Web tests pass | Browser/OIDC not exercised; existing analyzer/dependency warnings remain. |
| API tests | **959 passed, 10 skipped, 0 failed** (969 total) | Skipped LocalDB cases remain open. Rerun after explicit-field production endpoint and contract migration; actual project run. |
| Repository persona reader | **6/6 focused tests passed**, including new SQLite regression | Five overlap the API suite; validates active local user/profile/catalog without granting roles. |
| Notification integration | **21/21 passed** | Real SignalR/loopback transport; resource checks mocked, in-process delivery only. |
| Website / authentication | **160/160 passed** | Includes 12 royalty, 9 persona-workspace 12 field-dashboard and 11 field-selection 4 lifecycle-overview 8 well-comparison and 3 production-workspace route checks and 10 production-dashboard/client regressions plus existing app-role/auth checks; no live browser or identity provider. |
| Cost allocation | **13/13 passed** | Upstream configured cost-center pipeline plus strict source validation and no writes; alternate direct calculator retired. |
| Royalty and reconciliation | **85 royalty/accounting-cycle/schema tests + 11 reconciliation tests passed within the API suite** | Stored accrual/payment inputs, partial settlement, access/retry checks and read-only journal/ledger review verified; SQLite bootstrap uniqueness tested. Live provider races, schema deployment and interrupted-post recovery remain open. |
| Financial operation claims | **4/4 passed**, including independent SQLite connections racing for ownership | SQL Server/PostgreSQL/Oracle migrations generated and snapshots match; not deployed or integrated with financial writes. |
| Seed regression | **3/3 passed within the API suite** | Repeat-run counts, failure and cancellation; upstream repository persona routing retained. |
| Module ownership guard | **Passed**, 109 files scanned | Existing pattern guard only. |

**Current user direction:** Prioritize the website and its supporting framework. Deliver visible, complete user workflows; infrastructure work must directly enable those interactions. Continue FG-50/52 before further standalone financial-recovery infrastructure.

**Production framework finding resolved by retirement:** Removed the faulty production KPI endpoint/service and both client-local models after migrating its final operations-page consumer. Both production workspaces now share the existing field-dashboard component. Future daily-production/uptime/efficiency KPIs require explicit data, units and aggregation periods; current dashboard accuracy and browser acceptance remain open.

**Field-bound production API:** Main production dashboard now reads a single explicit-field response authorized by the existing app access service. Its shared contracts are owned by TheTechIdea.Data. Repository reads are sequential, not a transactional snapshot; current-field consumers elsewhere remain to be migrated.

## Execution order and status rules

Start FG-00/01/02; prioritize FG-12 notification isolation immediately alongside baseline work. Complete API/role boundaries before expanding client workflows. Data/setup gates precede calculation/workflow persistence; app journeys consume verified services; release gates cover the resulting whole system.

Statuses: **Planned**, **In progress**, **Implemented, unverified**, **Blocked**, **Verified**, **Historical**. Verified implementation needs source/commit, dated command or scenario and a successful result. Owner columns name project responsibilities; assign people at scheduling. Dependencies are task IDs, not implied completion claims.

## Phase 0 — baseline and ownership

Details: [Phase 0](Plans/FrameworkAppEnhancement/Phase-0-Baseline.md).

| ID | Priority | Task | Owner area | Depends on | Status |
|---|---|---|---|---|---|
| FG-00 | P0 | Restore reproducible builds; resolve current LifeCycle failures and capture clean CI baseline | Build / LifeCycle | — | In progress: API/Web builds pass after reconciliation; clean CI/external-dependency reproducibility remains open |
| FG-01 | P0 | Map local API/contracts/admin integration to AGENTS.md canonical platform ownership | Framework / platform integration | — | Planned |
| FG-02 | P1 | Reconcile historical completion claims, source paths and module evidence | Framework / module owners | FG-00 | Planned |

## Phase 1 — boundaries, roles and notifications

Details: [Phase 1](Plans/FrameworkAppEnhancement/Phase-1-Boundaries-and-Identity.md).

| ID | Priority | Task | Owner area | Depends on | Status |
|---|---|---|---|---|---|
| FG-10 | P0 | Make Web use authenticated HTTP only; retire local/auto selection from its execution path | Web / Client | FG-01 | Planned |
| FG-11 | P0 | Implement app-RBAC API role bridge and standard ASP.NET roles; retain independent API/resource checks | Web / API / UserManagement | FG-01 | Implemented upstream and reconciled; 91 Web auth tests pass; live OIDC/LocalDB acceptance remains open |
| FG-12 | P0 | Isolate notification state and authorize hub subscriptions | Web / API | Existing access services; verify with FG-00 | In progress: 21 project integration tests pass; repository persona plus per-message app-role checks; live-provider/browser and publisher rollout remain open |
| FG-13 | P1 | Consolidate inbox/hub transport and contract ownership; preserve request failure states | Client / API / Web | FG-01, FG-10, FG-11 | Planned |

## Phase 2 — data and setup

Details: [Phase 2](Plans/FrameworkAppEnhancement/Phase-2-Data-and-Setup.md).

| ID | Priority | Task | Owner area | Depends on | Status |
|---|---|---|---|---|---|
| FG-20 | P1 | Verify module/table ownership, dependency order and seed idempotency | PPDM39.DataManagement | FG-00, FG-01 | In progress: SoD repeat-run counts and failure/cancellation tests pass with mocked data source; full seed/provider gates remain open |
| FG-21 | P1 | Harden existing setup preflight, provider validation and interruption recovery | DataManager / API / admin integration | FG-10, FG-11, FG-20 | Planned |
| FG-22 | P1 | Verify authenticated facility/production persistence round trips | ProductionOperations / API | FG-11, FG-20 | Planned; carries forward facility phase 2 |

## Phase 3 — engineering quality

Details: [Phase 3](Plans/FrameworkAppEnhancement/Phase-3-Engineering-Quality.md).

| ID | Priority | Task | Owner area | Depends on | Status |
|---|---|---|---|---|---|
| FG-30 | P1 | Make units, data provenance, assumptions and saved calculation inputs explicit | LifeCycle / engineering / contracts | FG-01, FG-20 | Planned |
| FG-31 | P1 | Expand benchmark, applicability and numerical boundary coverage in existing modules | Engineering modules | FG-00, FG-30 | Planned |
| FG-32 | P2 | Deliver reproducible scenario comparison and uncertainty reporting | Engineering / API / Web | FG-10, FG-11, FG-30, FG-31 | Planned |

## Phase 4 — lifecycle and accounting

Details: [Phase 4](Plans/FrameworkAppEnhancement/Phase-4-Lifecycle-and-Accounting.md).

| ID | Priority | Task | Owner area | Depends on | Status |
|---|---|---|---|---|---|
| FG-40 | P1 | Verify durable workflow transitions, retries, concurrency and delegation expiry | LifeCycle / UserManagement | FG-11, FG-20 | Planned |
| FG-41 | P1 | Verify allocation, posting, reconciliation and controlled close/reopen | ProductionAccounting / Accounting | FG-22, FG-40 | In progress: stored royalty inputs, preview/reporting, accrual authorization and retry reservations verified with mocked providers; partial/final payments, replay checks, accrual journal linkage, structured source references and posting review implemented; atomic repository claims tested but not integrated; live provider concurrency, posting recovery, approvals and close remain open |
| FG-42 | P2 | Complete domain handoffs across asset, HSE/permit and retirement journeys | Domain modules / LifeCycle | FG-40; FG-30 for calculations | Planned |

## Phase 5 — application experience

Details: [Phase 5](Plans/FrameworkAppEnhancement/Phase-5-App-Experience.md).

| ID | Priority | Task | Owner area | Depends on | Status |
|---|---|---|---|---|---|
| FG-50 | P1 | Distinguish empty, failed and unauthorized states; prevent stale field results | Web / Client | FG-10, FG-13 | In progress: field dashboard and royalty list/details now use explicit error/empty states, retries and stale-response guards; invented dashboard trends removed; shared field selection waits for API confirmation; lifecycle overview refreshes through shared dashboard state; main production dashboard consumes one authorized explicit-field response and clears stale/partial results; browser acceptance remains open |
| FG-51 | P1 | Consolidate CSS/JS and apply branding tokens, dark mode and RTL | Web / ThemeBranding | FG-01 | Planned |
| FG-52 | P2 | Complete engineer, accountant, HSE and manager journeys with accessible navigation/export | Web / API | FG-50, FG-51; relevant FG-32/41/42 | In progress: accountant royalty list-to-details/payment-history/posting-review workflow implemented; persona selector, reactive workspace navigation and dashboard badge implemented; production engineer and operations workspaces share one field-data component with reported alerts; well-comparison inputs/results/export now share a confirmed request snapshot; other journeys and browser acceptance remain open |

## Phase 6 — delivery and release

Details: [Phase 6](Plans/FrameworkAppEnhancement/Phase-6-Delivery-and-Verification.md).

| ID | Priority | Task | Owner area | Depends on | Status |
|---|---|---|---|---|---|
| FG-60 | P1 | Expand existing CI test matrix and independent architecture guards | Build / module owners | FG-00, FG-01 | Planned |
| FG-61 | P1 | Add real HTTP/database/browser verification with two identities and fields | API / Web / test owners | FG-11, FG-12, FG-22, promoted journeys | Planned |
| FG-62 | P1 | Verify operational health, correlation, restore and rollback | API operations / build | FG-60, FG-61 | Planned |

## Historical facility phase index

| Phase | Doc | Goal |
|-------|-----|------|
| 1 | [Plans/Phases/ProductionOperations-Facility-Phase1-Services.md](Plans/Phases/ProductionOperations-Facility-Phase1-Services.md) | PPDM-native facility service, delegation from production operations, production management queries + cancellation |
| 2 | [Plans/Phases/ProductionOperations-Facility-Phase2-API-Verification.md](Plans/Phases/ProductionOperations-Facility-Phase2-API-Verification.md) | Facility API controllers, full solution build, integration tests |

## Historical facility status, preserved with verification limits

- Phase 1: **Historical: mostly complete** — prior tracker reports services, DI order, docs and a successful ProductionOperations build. That build was not repeated in this review.
- Phase 2: **Implemented, unverified end-to-end** — facility controllers and mocked monitoring controller tests exist. The former Permits build blocker is historical; the current API probe fails in LifeCycle as recorded above. Persistence/HTTP integration remains open under FG-22.

## Related feature trackers

| Feature | Tracker |
|---------|---------|
| Compressor analysis (extension tables, LOV seed, API/orchestration) | [Beep.OilandGas.CompressorAnalysis/MASTER-TODO-TRACKER.md](Beep.OilandGas.CompressorAnalysis/MASTER-TODO-TRACKER.md) |
| Enhanced oil recovery (PDEN, screening analytics, API/Web) | [Beep.OilandGas.EnhancedRecovery/MASTER-TODO-TRACKER.md](Beep.OilandGas.EnhancedRecovery/MASTER-TODO-TRACKER.md) |
| Flash / PVT (EOS LOVs, `R_FLASH_CALCULATION_REFERENCE_CODE`, module) | [Beep.OilandGas.FlashCalculations/MASTER-TODO-TRACKER.md](Beep.OilandGas.FlashCalculations/MASTER-TODO-TRACKER.md) |
| Gas lift (reference LOVs, `R_GAS_LIFT_REFERENCE_CODE`, module) | [Beep.OilandGas.GasLift/MASTER-TODO-TRACKER.md](Beep.OilandGas.GasLift/MASTER-TODO-TRACKER.md) |
| Gas properties (Z-factor, viscosity, pseudo-pressure, `IGasPropertiesService`) | [Beep.OilandGas.GasProperties/MASTER-TODO-TRACKER.md](Beep.OilandGas.GasProperties/MASTER-TODO-TRACKER.md) |
| Hydraulic pumps (jet/piston calculators, `IHydraulicPumpService`) | [Beep.OilandGas.HydraulicPumps/MASTER-TODO-TRACKER.md](Beep.OilandGas.HydraulicPumps/MASTER-TODO-TRACKER.md) |
| Oil properties (black-oil correlations, `IOilPropertiesService`) | [Beep.OilandGas.OilProperties/MASTER-TODO-TRACKER.md](Beep.OilandGas.OilProperties/MASTER-TODO-TRACKER.md) |
| Pump performance (H–Q, NPSH, ESP, `IPumpPerformanceService`) | [Beep.OilandGas.PumpPerformance/MASTER-TODO-TRACKER.md](Beep.OilandGas.PumpPerformance/MASTER-TODO-TRACKER.md) |
| Well test / PTA (Horner, MDH, derivative, `IWellTestAnalysisService`) | [Beep.OilandGas.WellTestAnalysis/MASTER-TODO-TRACKER.md](Beep.OilandGas.WellTestAnalysis/MASTER-TODO-TRACKER.md) |
| Choke analysis | [Tracker](Beep.OilandGas.ChokeAnalysis/MASTER-TODO-TRACKER.md) |
| Development planning | [Tracker](Beep.OilandGas.DevelopmentPlanning/MASTER-TODO-TRACKER.md) |
| Decommissioning | [Tracker](Beep.OilandGas.Decommissioning/MASTER-TODO-TRACKER.md) |
| Economic analysis | [Tracker](Beep.OilandGas.EconomicAnalysis/MASTER-TODO-TRACKER.md) |
| HSE | [Tracker](Beep.OilandGas.HSE/MASTER-TODO-TRACKER.md) |
| Lease acquisition | [Tracker](Beep.OilandGas.LeaseAcquisition/MASTER-TODO-TRACKER.md) |
| Nodal analysis | [Tracker](Beep.OilandGas.NodalAnalysis/MASTER-TODO-TRACKER.md) |
| Permits and applications | [Tracker](Beep.OilandGas.PermitsAndApplications/MASTER-TODO-TRACKER.md) |
| Production accounting | [Tracker](Beep.OilandGas.ProductionAccounting/MASTER-TODO-TRACKER.md) |
| Production forecasting including DCA | [Tracker](Beep.OilandGas.ProductionForecasting/MASTER-TODO-TRACKER.md) |
| Production operations | [Tracker](Beep.OilandGas.ProductionOperations/MASTER-TODO-TRACKER.md) |
| Prospect identification | [Tracker](Beep.OilandGas.ProspectIdentification/MASTER-TODO-TRACKER.md) |
| Database setup wizard | [Tracker](Plans/DataBaseWizardandCreationEnhancment/MASTER-TODO-TRACKER.md) |

Other projects, including Drawing, HeatMap, PipelineAnalysis, PlungerLift, SuckerRodPumping, Accounting, Client and Web, participate through the phase task IDs above and existing architecture/enhancement documents. The presence of a feature tracker is not a release-readiness claim.
