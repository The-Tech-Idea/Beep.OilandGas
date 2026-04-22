# Project Knowledge Base

> Purpose: maintain one scan-backed planning document per repo-local Beep.OilandGas project.  
> Source set: repo-local `.csproj` inventory under `c:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas`.  
> Usage: update these project docs first, then update phase plans from them.

---

## Scan Strategy

- `Heavy`: requires individual deeper scan beyond root structure because it shapes architecture, routing, API boundaries, orchestration, or large service surfaces.
- `Medium`: root structure plus representative file evidence is usually enough for planning, but may need deeper follow-up when execution starts.
- `Standardized`: follows a repeated engineering-module pattern; root structure plus representative files is enough for planning unless execution uncovers divergence.

---

## Presentation

| Project | Scan Depth | Current Role | Document |
|---------|------------|--------------|----------|
| Beep.OilandGas.Web | Heavy | Primary web surface for phases 6-10 | `Presentation/Beep.OilandGas.Web.md` |
| Beep.OilandGas.ApiService | Heavy | Primary API boundary for phases 6-10 | `Presentation/Beep.OilandGas.ApiService.md` |

## Core

| Project | Scan Depth | Current Role | Document |
|---------|------------|--------------|----------|
| Beep.OilandGas.Models | Heavy | Shared contracts and domain DTO/model surface | `Core/Beep.OilandGas.Models.md` |
| Beep.OilandGas.PPDM.Models | Medium | Shared PPDM entity model project | `Core/Beep.OilandGas.PPDM.Models.md` |
| Beep.OilandGas.PPDM39 | Heavy | PPDM repository and metadata contracts | `Core/Beep.OilandGas.PPDM39.md` |
| Beep.OilandGas.PPDM39.DataManagement | Heavy | Main PPDM data-service implementation layer | `Core/Beep.OilandGas.PPDM39.DataManagement.md` |
| Beep.OilandGas.Client | Medium | Shared client abstractions and integration helpers | `Core/Beep.OilandGas.Client.md` |

## Operations

| Project | Scan Depth | Current Role | Document |
|---------|------------|--------------|----------|
| Beep.OilandGas.ProspectIdentification | Heavy | Exploration domain slice | `Operations/Beep.OilandGas.ProspectIdentification.md` |
| Beep.OilandGas.DevelopmentPlanning | Heavy | Development planning domain slice | `Operations/Beep.OilandGas.DevelopmentPlanning.md` |
| Beep.OilandGas.ProductionOperations | Medium | Production operations domain slice, currently thin | `Operations/Beep.OilandGas.ProductionOperations.md` |
| Beep.OilandGas.LifeCycle | Heavy | Cross-domain workflow and mapping hub | `Operations/Beep.OilandGas.LifeCycle.md` |
| Beep.OilandGas.LeaseAcquisition | Heavy | Lease and rights acquisition domain slice | `Operations/Beep.OilandGas.LeaseAcquisition.md` |
| Beep.OilandGas.EnhancedRecovery | Medium | EOR domain slice, currently thin | `Operations/Beep.OilandGas.EnhancedRecovery.md` |
| Beep.OilandGas.Decommissioning | Heavy | Decommissioning and abandonment domain slice | `Operations/Beep.OilandGas.Decommissioning.md` |
| Beep.OilandGas.DrillingAndConstruction | Medium | Drilling/construction support slice | `Operations/Beep.OilandGas.DrillingAndConstruction.md` |

## Finance And Support

| Project | Scan Depth | Current Role | Document |
|---------|------------|--------------|----------|
| Beep.OilandGas.Accounting | Heavy | General accounting and finance service layer | `FinanceAndSupport/Beep.OilandGas.Accounting.md` |
| Beep.OilandGas.ProductionAccounting | Heavy | Oil-and-gas-specific accounting service layer | `FinanceAndSupport/Beep.OilandGas.ProductionAccounting.md` |
| Beep.OilandGas.PermitsAndApplications | Heavy | Permit validation and compliance support layer | `FinanceAndSupport/Beep.OilandGas.PermitsAndApplications.md` |
| Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf | Medium | Permit document rendering helper | `FinanceAndSupport/Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf.md` |
| Beep.OilandGas.UserManagement | Medium | Authorization and permission support | `FinanceAndSupport/Beep.OilandGas.UserManagement.md` |
| Beep.OilandGas.DataManager | Medium | Data/admin utility and execution state support | `FinanceAndSupport/Beep.OilandGas.DataManager.md` |
| Beep.OilandGas.Branchs | Medium | Navigation/tree/category mapping support | `FinanceAndSupport/Beep.OilandGas.Branchs.md` |
| Beep.OilandGas.Drawing | Medium | Visualization and diagram support | `FinanceAndSupport/Beep.OilandGas.Drawing.md` |

## Engineering

| Project | Scan Depth | Current Role | Document |
|---------|------------|--------------|----------|
| Beep.OilandGas.DCA | Medium | Decline curve analysis module | `Engineering/Beep.OilandGas.DCA.md` |
| Beep.OilandGas.EconomicAnalysis | Medium | Economic analysis module | `Engineering/Beep.OilandGas.EconomicAnalysis.md` |
| Beep.OilandGas.ProductionForecasting | Standardized | Production forecasting module | `Engineering/Beep.OilandGas.ProductionForecasting.md` |
| Beep.OilandGas.ChokeAnalysis | Standardized | Choke analysis module | `Engineering/Beep.OilandGas.ChokeAnalysis.md` |
| Beep.OilandGas.GasLift | Standardized | Gas lift module | `Engineering/Beep.OilandGas.GasLift.md` |
| Beep.OilandGas.SuckerRodPumping | Standardized | SRP module | `Engineering/Beep.OilandGas.SuckerRodPumping.md` |
| Beep.OilandGas.PlungerLift | Standardized | Plunger lift module | `Engineering/Beep.OilandGas.PlungerLift.md` |
| Beep.OilandGas.HydraulicPumps | Standardized | Hydraulic pump module | `Engineering/Beep.OilandGas.HydraulicPumps.md` |
| Beep.OilandGas.CompressorAnalysis | Standardized | Compressor analysis module | `Engineering/Beep.OilandGas.CompressorAnalysis.md` |
| Beep.OilandGas.PipelineAnalysis | Standardized | Pipeline analysis module | `Engineering/Beep.OilandGas.PipelineAnalysis.md` |
| Beep.OilandGas.FlashCalculations | Standardized | Flash calculation module | `Engineering/Beep.OilandGas.FlashCalculations.md` |
| Beep.OilandGas.OilProperties | Standardized | Oil properties module | `Engineering/Beep.OilandGas.OilProperties.md` |
| Beep.OilandGas.GasProperties | Standardized | Gas properties module | `Engineering/Beep.OilandGas.GasProperties.md` |
| Beep.OilandGas.HeatMap | Medium | Heat-map and visualization module | `Engineering/Beep.OilandGas.HeatMap.md` |
| Beep.OilandGas.NodalAnalysis | Medium | Nodal analysis module | `Engineering/Beep.OilandGas.NodalAnalysis.md` |
| Beep.OilandGas.WellTestAnalysis | Medium | Well-test analysis module | `Engineering/Beep.OilandGas.WellTestAnalysis.md` |
| Beep.OilandGas.PumpPerformance | Medium | Pump performance and system analysis module | `Engineering/Beep.OilandGas.PumpPerformance.md` |
