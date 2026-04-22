# Beep.OilandGas.Decommissioning

## Snapshot

- Category: Operations
- Scan depth: Heavy
- Current role: abandonment and decommissioning domain slice
- Maturity signal: materially present with service, DTO, API, and web coverage

## Observed Structure

- Top-level folders: `.plans`, `Core`, `Data`, `Services`
- Services include `WellPluggingService` and `FieldDecommissioningService`, with advanced/analysis partials
- Data folder includes many decommissioning DTOs and analysis/request/response models

## Representative Evidence

- Services: `Services/WellPluggingService.cs`, `Services/WellPluggingService.Advanced.cs`, `Services/FieldDecommissioningService.cs`, `Services/FieldDecommissioningService.Analysis.cs`
- Data models: `Data/WellAbandonmentRequest.cs`, `DecommissioningCostRequest.cs`, `RegulatoryComplianceAnalysis.cs`, `WellPluggingPlan.cs`
- API surfacing: `Beep.OilandGas.ApiService/Controllers/Field/DecommissioningController.cs`
- Web surfacing: `Beep.OilandGas.Web/Pages/PPDM39/Decommissioning/WellAbandonment.razor`, `FacilityDecommissioning.razor`, `CostEstimation.razor`, `WellPAWorkflow.razor`

## Planning Notes

- This is one of the stronger end-of-life slices and should be used as a reference implementation for phase 8/9 closure flows.
- The production-to-decommissioning handoff is now live through `Controllers/Field/ProductionController.cs`, `InterventionDecisions.razor`, `WellPAWorkflow.razor`, and `WellAbandonment.razor`, which means late-life operational decisions can create a starter abandonment record and land directly in the P&A workflow.
- The remaining planning need is deeper linkage from active decommissioning into permits/compliance/accounting closeout, not basic slice creation.
