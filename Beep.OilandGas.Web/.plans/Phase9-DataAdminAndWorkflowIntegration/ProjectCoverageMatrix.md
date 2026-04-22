# Phase 9 Project Coverage Matrix

> Scope: all Beep.OilandGas solution projects  
> Purpose: define how each project participates in the data, admin, workflow, compliance, and finance rationalization phase.

| Project | Group | Phase 9 Role | Pass A | Pass B | Pass C |
|---------|-------|--------------|--------|--------|--------|
| Beep.OilandGas.Web | Presentation | Primary | classify admin/workflow routes | refactor to typed support clients | align page access, dashboards, and support flows |
| Beep.OilandGas.ApiService | API | Primary | inventory admin/workflow controller overlap | normalize support-domain endpoints | align workflow, compliance, and finance orchestration |
| Beep.OilandGas.Models | Shared contracts | Supporting | inventory admin/workflow DTO gaps | add support-domain contracts | stabilize governance and finance payloads |
| Beep.OilandGas.PPDM.Models | Shared data models | Supporting | review admin-model overlap | align shared admin models | validate governed data flows |
| Beep.OilandGas.PPDM39 | PPDM core | Primary | freeze setup/schema ownership | support admin typed-client seams | validate governed entity/admin flows |
| Beep.OilandGas.PPDM39.DataManagement | Data services | Primary | freeze quality/audit/versioning ownership | expose admin service seams | align governance with workflow and admin pages |
| Beep.OilandGas.Client | Client abstractions | Supporting | review support-client overlap | align typed client contracts | validate compatibility |
| Beep.OilandGas.DataManager | Data/admin support | Primary | freeze data-admin utility ownership | align admin utility seams | validate no support bypass remains |
| Beep.OilandGas.UserManagement | Security | Primary | map role/admin route dependencies | align policies and admin guards | validate governance and access rules |
| Beep.OilandGas.Branchs | Navigation/tree | Supporting | map admin/workflow navigation nodes | align nodes to canonical routes | reflect workflow/governance state |
| Beep.OilandGas.Drawing | Visualization | Supporting | review admin/workflow visualization needs | align dashboards/components | support workflow/admin visual cues |
| Beep.OilandGas.ProspectIdentification | Operational domain | Validation | review admin/result surfaces | consume support clients only where needed | validate workflow/admin linkage |
| Beep.OilandGas.DevelopmentPlanning | Operational domain | Validation | review admin/result surfaces | consume permit/workflow clients where needed | validate workflow/admin linkage |
| Beep.OilandGas.ProductionOperations | Operational domain | Validation | review admin/result surfaces | consume workflow/admin clients where needed | validate workflow/admin linkage |
| Beep.OilandGas.LifeCycle | Workflow orchestration | Primary | freeze workflow dashboard ownership | expose typed workflow clients | align process dashboards with finance/compliance |
| Beep.OilandGas.LeaseAcquisition | Operational domain | Validation | review admin/result surfaces | consume support clients where needed | validate workflow/admin linkage |
| Beep.OilandGas.EnhancedRecovery | Operational domain | Validation | review admin/result surfaces | consume support clients where needed | validate workflow/admin linkage |
| Beep.OilandGas.Decommissioning | Operational domain | Validation | review admin/result surfaces | consume permit/accounting clients where needed | validate workflow/admin linkage |
| Beep.OilandGas.DrillingAndConstruction | Operational domain | Validation | review admin/result surfaces | consume permit/workflow clients where needed | validate workflow/admin linkage |
| Beep.OilandGas.ProductionAccounting | Finance support | Primary | freeze finance route ownership and document thin current UI footprint | expose typed finance clients and expand surfaced pages where needed | align production workflows to accounting actions |
| Beep.OilandGas.Accounting | Finance support | Primary | freeze accounting route ownership and document thin current UI footprint | expose typed accounting clients and expand surfaced pages where needed | align approvals, AFEs, and closeout flows |
| Beep.OilandGas.PermitsAndApplications | Compliance support | Primary | freeze permit/compliance route ownership and document current validation-heavy architecture | expose typed permit/compliance clients and add missing API/UI surfacing | align permits to operational workflows |
| Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf | Compliance rendering | Supporting | review rendering dependencies | support permit document generation seams | validate package output in workflows |
| Beep.OilandGas.EconomicAnalysis | Engineering support | Validation | review result-history/admin needs | consume support clients only where needed | validate linkage to finance/governance |
| Beep.OilandGas.ProductionForecasting | Engineering support | Validation | review result-history/admin needs | consume support clients only where needed | validate linkage to finance/governance |
| Beep.OilandGas.DCA | Engineering support | Validation | review result-history/admin needs | consume support clients only where needed | validate linkage to finance/governance |
| Beep.OilandGas.NodalAnalysis | Engineering support | Validation | review result-history/admin needs | consume support clients only where needed | validate linkage to workflow/governance |
| Beep.OilandGas.WellTestAnalysis | Engineering support | Validation | review result-history/admin needs | consume support clients only where needed | validate linkage to workflow/governance |
| Beep.OilandGas.PumpPerformance | Engineering support | Validation | review result-history/admin needs | consume support clients only where needed | validate linkage to workflow/governance |
| Beep.OilandGas.ChokeAnalysis | Engineering support | Validation | review result-history/admin needs | consume support clients only where needed | validate linkage to workflow/governance |
| Beep.OilandGas.GasLift | Engineering support | Validation | review result-history/admin needs | consume support clients only where needed | validate linkage to workflow/governance |
| Beep.OilandGas.SuckerRodPumping | Engineering support | Validation | review result-history/admin needs | consume support clients only where needed | validate linkage to workflow/governance |
| Beep.OilandGas.PlungerLift | Engineering support | Validation | review result-history/admin needs | consume support clients only where needed | validate linkage to workflow/governance |
| Beep.OilandGas.HydraulicPumps | Engineering support | Validation | review result-history/admin needs | consume support clients only where needed | validate linkage to workflow/governance |
| Beep.OilandGas.CompressorAnalysis | Engineering support | Validation | review result-history/admin needs | consume support clients only where needed | validate linkage to workflow/governance |
| Beep.OilandGas.PipelineAnalysis | Engineering support | Validation | review result-history/admin needs | consume support clients only where needed | validate linkage to workflow/governance |
| Beep.OilandGas.FlashCalculations | Engineering support | Validation | review result-history/admin needs | consume support clients only where needed | validate linkage to workflow/governance |
| Beep.OilandGas.OilProperties | Engineering support | Validation | review property-admin needs | consume support clients only where needed | validate governed data usage |
| Beep.OilandGas.GasProperties | Engineering support | Validation | review property-admin needs | consume support clients only where needed | validate governed data usage |
| Beep.OilandGas.HeatMap | Engineering support | Validation | review admin/dashboard needs | consume support clients only where needed | validate governed visualization usage |
