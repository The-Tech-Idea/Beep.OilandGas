# Phase 8 Scan Evidence

> Purpose: representative file-level evidence used to ground the Phase 8 operational and lifecycle plan.

---

## Web Surfaces

- `Beep.OilandGas.Web/Pages/PPDM39/Exploration/Prospects.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Exploration/ProspectDetail.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Exploration/ProspectBoard.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Development/DevDashboard.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Development/FDPWizard.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Development/WellDesign.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Production/ProductionDashboard.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Production/WellPerformanceOptimizer.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Production/InterventionDecisions.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Operations/LeaseAcquisition.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Operations/EnhancedRecovery.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Operations/DrillingOperations.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Decommissioning/WellAbandonment.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Decommissioning/WellPAWorkflow.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/WorkOrder/WorkOrderDashboard.razor`

---

## API Surfaces

- `Beep.OilandGas.ApiService/Controllers/Operations/ProspectIdentificationController.cs`
- `Beep.OilandGas.ApiService/Controllers/Field/DevelopmentController.cs`
- `Beep.OilandGas.ApiService/Controllers/Production/ProductionOperationsController.cs`
- `Beep.OilandGas.ApiService/Controllers/Operations/LeaseAcquisitionController.cs`
- `Beep.OilandGas.ApiService/Controllers/Operations/EnhancedRecoveryController.cs`
- `Beep.OilandGas.ApiService/Controllers/Operations/DrillingOperationController.cs`
- `Beep.OilandGas.ApiService/Controllers/Field/DecommissioningController.cs`
- `Beep.OilandGas.ApiService/Controllers/WorkOrder/WorkOrderController.cs`
- `Beep.OilandGas.ApiService/Controllers/LifeCycle/WorkOrderController.cs`

---

## Domain Services and Models

### Mature or materially present slices

- `Beep.OilandGas.ProspectIdentification/Services/ProspectIdentificationService.cs`
- `Beep.OilandGas.LeaseAcquisition/Services/LeaseAcquisitionService.cs`
- `Beep.OilandGas.LeaseAcquisition/Services/LeaseAcquisitionService.DueDiligence.cs`
- `Beep.OilandGas.LeaseAcquisition/Services/LeaseAcquisitionService.Rights.cs`
- `Beep.OilandGas.Decommissioning/Services/WellPluggingService.cs`
- `Beep.OilandGas.Decommissioning/Services/FieldDecommissioningService.cs`
- `Beep.OilandGas.LifeCycle/Services/WorkOrder/WorkOrderManagementService.cs`

### Partial or thin slices

- `Beep.OilandGas.DevelopmentPlanning/Services/DevelopmentPlanService.cs`
- `Beep.OilandGas.DevelopmentPlanning/Services/DevelopmentPlanService.Advanced.cs`
- `Beep.OilandGas.ProductionOperations/Services/ProductionOperationsService.cs`
- `Beep.OilandGas.ProductionOperations/Services/ProductionManagementService.cs`
- `Beep.OilandGas.EnhancedRecovery/Services/EnhancedRecoveryService.cs`
- `Beep.OilandGas.EnhancedRecovery/Services/EnhancedRecoveryService.Advanced.cs`
- `Beep.OilandGas.DrillingAndConstruction/Services/DrillingOperationService.cs`
- `Beep.OilandGas.DrillingAndConstruction/Services/DrillingEngineeringService.cs`

---

## Cross-Phase Gaps Observed During Phase 8 Scan

- work-order ownership split between lifecycle and work-order controller families
- permit/compliance project lacks comparable first-class API and web surfacing for operational handoffs
- production, EOR, and drilling slices need more service maturity before workflow linking can be considered complete
