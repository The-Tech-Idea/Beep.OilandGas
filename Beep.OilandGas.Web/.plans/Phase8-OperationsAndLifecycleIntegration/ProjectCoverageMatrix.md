# Phase 8 Project Coverage Matrix

> Scope: all Beep.OilandGas solution projects  
> Purpose: define how each project participates in the operations and lifecycle integration phase.

| Project | Group | Phase 8 Role | Pass A | Pass B | Pass C |
|---------|-------|--------------|--------|--------|--------|
| Beep.OilandGas.Web | Presentation | Primary | map operational pages to owning clients | refactor pages to domain-specific clients | add workflow launch and handoff actions |
| Beep.OilandGas.ApiService | API | Primary | inventory operational controllers and gaps | normalize field-scoped endpoints by domain | add handoff/orchestration endpoints |
| Beep.OilandGas.Models | Shared contracts | Supporting | inventory workflow DTO gaps | add/align request-response contracts | stabilize handoff payloads |
| Beep.OilandGas.PPDM.Models | Shared data models | Supporting | review shared model usage | align model ownership by slice | validate workflow model continuity |
| Beep.OilandGas.PPDM39 | PPDM core | Supporting | map field and entity ownership | support missing operational entities | validate status/state persistence |
| Beep.OilandGas.PPDM39.DataManagement | Data services | Primary | map service/repository owners | wire domain services to PPDM paths | persist workflow transitions |
| Beep.OilandGas.Client | Client abstractions | Supporting | review client contract overlap | align shared client contracts | validate compatibility |
| Beep.OilandGas.DataManager | Data/admin support | Validation | identify operational bypasses | keep admin utilities off main flows | validate no workflow bypass remains |
| Beep.OilandGas.UserManagement | Security | Supporting | map role dependencies | align permissions to domain slices | validate transition authorization |
| Beep.OilandGas.Branchs | Navigation/tree | Supporting | map workflow tree dependencies | align nodes to canonical route groups | reflect lifecycle transitions in navigation |
| Beep.OilandGas.Drawing | Visualization | Supporting | inventory visual dependencies | align drawings/components to domain pages | support workflow visual cues |
| Beep.OilandGas.ProspectIdentification | Operational domain | Primary | freeze exploration ownership | complete exploration vertical slice | hand off prospects into development/economics |
| Beep.OilandGas.DevelopmentPlanning | Operational domain | Primary | freeze development ownership | complete development-planning slice | hand off plans into permits/construction; WellDesign -> DrillingOperations seam is now live |
| Beep.OilandGas.ProductionOperations | Operational domain | Primary | freeze production ownership and document current service thinness | complete production-operations slice and add missing service orchestration | launch interventions, deferments, and late-life actions; decommissioning handoff is now live |
| Beep.OilandGas.LifeCycle | Operational orchestration | Primary | freeze lifecycle orchestration scope | align dashboards and work-order paths | own cross-module state transitions |
| Beep.OilandGas.LeaseAcquisition | Operational domain | Primary | freeze lease ownership | complete lease slice | connect lease outcomes to development/compliance |
| Beep.OilandGas.EnhancedRecovery | Operational domain | Primary | freeze EOR ownership and document current minimal implementation | complete EOR slice with missing models/contracts | connect pilots to economics and approvals; economics handoff is now live |
| Beep.OilandGas.Decommissioning | Operational domain | Primary | freeze decommissioning ownership | complete decommissioning slice | connect late-life triggers to closure workflows; production-triggered P&A intake plus compliance / closeout AFE follow-on are now live |
| Beep.OilandGas.DrillingAndConstruction | Operational domain | Supporting | map construction dependencies and current planning-surface gap | align with development-planning slice and add missing planning/progress surfacing | receive permit/development handoffs; development drilling intake is now live |
| Beep.OilandGas.ProductionAccounting | Finance support | Supporting | map production-accounting touchpoints | align operational data handoff needs | receive decommissioning and production workflow outputs; decommissioning cost/ARO follow-on remains open |
| Beep.OilandGas.Accounting | Finance support | Supporting | map accounting dependencies | align AFE/approval dependencies | receive EOR, intervention, and closeout handoffs; decommissioning closeout finance intake is now live through AFE handoff |
| Beep.OilandGas.PermitsAndApplications | Compliance support | Supporting | map permit/compliance dependencies and note current API/UI gap | align permit steps in domain slices through new surfacing seams | receive development and decommissioning handoffs; decommissioning closure compliance handoff is now live |
| Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf | Compliance rendering | Validation | inventory permit rendering dependencies | support permit package generation if needed | validate workflow document outputs |
| Beep.OilandGas.EconomicAnalysis | Engineering support | Supporting | map economics touchpoints | support exploration/development/EOR slices | receive prospect/intervention/EOR handoffs |
| Beep.OilandGas.ProductionForecasting | Engineering support | Supporting | map forecasting touchpoints | support production slice | feed production outcomes into workflow actions |
| Beep.OilandGas.DCA | Engineering support | Supporting | map DCA touchpoints | support production slice | feed decline insights into interventions |
| Beep.OilandGas.NodalAnalysis | Engineering support | Supporting | map nodal touchpoints | support production slice | feed nodal recommendations into actions |
| Beep.OilandGas.WellTestAnalysis | Engineering support | Supporting | map well-test touchpoints | support production slice | feed diagnostics into intervention workflows |
| Beep.OilandGas.PumpPerformance | Engineering support | Supporting | map pump-performance dependencies | support lift-related production slice | feed equipment decisions into actions |
| Beep.OilandGas.ChokeAnalysis | Engineering support | Supporting | map choke-analysis dependencies | support tuning workflows if surfaced | feed tuning decisions into interventions |
| Beep.OilandGas.GasLift | Engineering support | Supporting | map gas-lift dependencies | support artificial-lift workflows | feed gas-lift actions into work orders |
| Beep.OilandGas.SuckerRodPumping | Engineering support | Supporting | map SRP dependencies | support lift workflows | feed lift decisions into actions |
| Beep.OilandGas.PlungerLift | Engineering support | Supporting | map plunger-lift dependencies | support gas-well lift workflows | feed lift decisions into actions |
| Beep.OilandGas.HydraulicPumps | Engineering support | Supporting | map hydraulic-pump dependencies | support lift workflows | feed equipment decisions into actions |
| Beep.OilandGas.CompressorAnalysis | Engineering support | Supporting | map compressor-analysis dependencies | support facility/production workflows | feed facility actions into operations |
| Beep.OilandGas.PipelineAnalysis | Engineering support | Supporting | map pipeline-analysis dependencies | support operations/constraint workflows | feed constraint actions into workflows |
| Beep.OilandGas.FlashCalculations | Engineering support | Supporting | map flash/PVT dependencies | support facility/operations workflows | stabilize property handoffs |
| Beep.OilandGas.OilProperties | Engineering support | Supporting | map oil-property dependencies | support operations/economics inputs | validate property payload continuity |
| Beep.OilandGas.GasProperties | Engineering support | Supporting | map gas-property dependencies | support operations/economics inputs | validate property payload continuity |
| Beep.OilandGas.HeatMap | Engineering support | Supporting | map heat-map usage in workflows | support exploration/production visual slices | reflect workflow state spatially if needed |
