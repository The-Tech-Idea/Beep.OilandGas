# Phase 10 Project Coverage Matrix

> Scope: all Beep.OilandGas solution projects  
> Purpose: define how each project participates in validation, retirement, and closure of the update stream.

| Project | Group | Phase 10 Role | Pass A | Pass B | Pass C |
|---------|-------|---------------|--------|--------|--------|
| Beep.OilandGas.Web | Presentation | Primary | validate routes, pages, and typed-client usage | retire duplicate pages/components and reduce references | publish final route/client matrix |
| Beep.OilandGas.ApiService | API | Primary | validate endpoint coverage and ownership, including duplicate work-order families | retire overlapping endpoints and dead paths | publish final API ownership notes |
| Beep.OilandGas.Models | Shared contracts | Supporting | validate contract coverage | retire obsolete or duplicate contracts if any | publish final contract notes |
| Beep.OilandGas.PPDM.Models | Shared data models | Supporting | validate model continuity | retire duplicate model paths if any | publish final shared-model notes |
| Beep.OilandGas.PPDM39 | PPDM core | Supporting | validate entity/state persistence coverage | retire obsolete admin/state paths if any | publish final persistence notes |
| Beep.OilandGas.PPDM39.DataManagement | Data services | Primary | validate service ownership and persistence | retire bypass/duplicate data paths | publish final admin/data ownership notes |
| Beep.OilandGas.Client | Client abstractions | Supporting | validate shared client compatibility | retire obsolete client abstractions | publish final client boundary notes |
| Beep.OilandGas.DataManager | Data/admin support | Supporting | validate governed admin utility usage | retire obsolete bypass utilities if any | publish final admin support notes |
| Beep.OilandGas.UserManagement | Security | Supporting | validate role/access coverage | retire obsolete policy or route paths | publish final auth/access notes |
| Beep.OilandGas.Branchs | Navigation/tree | Supporting | validate navigation node coverage | retire obsolete nodes or redirects | publish final navigation notes |
| Beep.OilandGas.Drawing | Visualization | Supporting | validate shared visualization usage | retire duplicate visual components if any | publish final visualization notes |
| Beep.OilandGas.ProspectIdentification | Operational domain | Supporting | validate exploration ownership and calls | retire duplicate exploration paths | publish final exploration notes |
| Beep.OilandGas.DevelopmentPlanning | Operational domain | Supporting | validate development ownership and calls | retire duplicate development paths | publish final development notes |
| Beep.OilandGas.ProductionOperations | Operational domain | Supporting | validate production ownership and calls against current thin service implementation | retire duplicate production paths | publish final production notes and remaining maturity gaps |
| Beep.OilandGas.LifeCycle | Workflow orchestration | Primary | validate workflow state coverage | retire obsolete process/dashboard paths | publish final workflow notes |
| Beep.OilandGas.LeaseAcquisition | Operational domain | Supporting | validate lease flow coverage | retire duplicate lease paths | publish final lease notes |
| Beep.OilandGas.EnhancedRecovery | Operational domain | Supporting | validate EOR flow coverage against current minimal implementation | retire duplicate EOR paths | publish final EOR notes and remaining maturity gaps |
| Beep.OilandGas.Decommissioning | Operational domain | Supporting | validate decommissioning coverage | retire duplicate closeout paths | publish final decommissioning notes |
| Beep.OilandGas.DrillingAndConstruction | Operational domain | Supporting | validate construction handoff coverage | retire duplicate construction paths | publish final construction notes |
| Beep.OilandGas.ProductionAccounting | Finance support | Supporting | validate finance handoffs | retire duplicate finance paths | publish final production-accounting notes |
| Beep.OilandGas.Accounting | Finance support | Supporting | validate accounting handoffs | retire duplicate accounting paths | publish final accounting notes |
| Beep.OilandGas.PermitsAndApplications | Compliance support | Supporting | validate permit/compliance flows and current surfacing gaps | retire duplicate permit paths if new seams exist | publish final permit notes and remaining gaps |
| Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf | Compliance rendering | Validation | validate workflow document generation | retire obsolete rendering paths if any | publish final rendering notes |
| Beep.OilandGas.EconomicAnalysis | Engineering support | Supporting | validate economics integration coverage | retire duplicate economics paths | publish final economics notes |
| Beep.OilandGas.ProductionForecasting | Engineering support | Supporting | validate forecasting integration coverage | retire duplicate forecasting paths | publish final forecasting notes |
| Beep.OilandGas.DCA | Engineering support | Supporting | validate DCA integration coverage | retire duplicate DCA paths | publish final DCA notes |
| Beep.OilandGas.NodalAnalysis | Engineering support | Supporting | validate nodal integration coverage | retire duplicate nodal paths | publish final nodal notes |
| Beep.OilandGas.WellTestAnalysis | Engineering support | Supporting | validate well-test integration coverage | retire duplicate well-test paths | publish final well-test notes |
| Beep.OilandGas.PumpPerformance | Engineering support | Supporting | validate pump-performance integration coverage | retire duplicate pump-performance paths | publish final pump-performance notes |
| Beep.OilandGas.ChokeAnalysis | Engineering support | Supporting | validate choke-analysis integration coverage | retire duplicate choke-analysis paths | publish final choke-analysis notes |
| Beep.OilandGas.GasLift | Engineering support | Supporting | validate gas-lift integration coverage | retire duplicate gas-lift paths | publish final gas-lift notes |
| Beep.OilandGas.SuckerRodPumping | Engineering support | Supporting | validate SRP integration coverage | retire duplicate SRP paths | publish final SRP notes |
| Beep.OilandGas.PlungerLift | Engineering support | Supporting | validate plunger-lift integration coverage | retire duplicate plunger-lift paths | publish final plunger-lift notes |
| Beep.OilandGas.HydraulicPumps | Engineering support | Supporting | validate hydraulic-pump integration coverage | retire duplicate hydraulic-pump paths | publish final hydraulic-pump notes |
| Beep.OilandGas.CompressorAnalysis | Engineering support | Supporting | validate compressor-analysis integration coverage | retire duplicate compressor paths | publish final compressor-analysis notes |
| Beep.OilandGas.PipelineAnalysis | Engineering support | Supporting | validate pipeline-analysis integration coverage | retire duplicate pipeline-analysis paths | publish final pipeline-analysis notes |
| Beep.OilandGas.FlashCalculations | Engineering support | Supporting | validate flash-calculation integration coverage | retire duplicate flash paths | publish final flash notes |
| Beep.OilandGas.OilProperties | Engineering support | Supporting | validate oil-properties integration coverage | retire duplicate oil-property paths | publish final oil-properties notes |
| Beep.OilandGas.GasProperties | Engineering support | Supporting | validate gas-properties integration coverage | retire duplicate gas-property paths | publish final gas-properties notes |
| Beep.OilandGas.HeatMap | Engineering support | Supporting | validate heat-map integration coverage | retire duplicate heat-map paths | publish final heat-map notes |
