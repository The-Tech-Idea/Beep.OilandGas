# Phase 10 Scan Evidence

> Purpose: representative file-level evidence used to ground the Phase 10 validation, retirement, and closure plan.

---

## Web Duplication and Retirement Candidates

- `Beep.OilandGas.Web/App.razor`
- `Beep.OilandGas.Web/Components/App.razor`
- `Beep.OilandGas.Web/Components/Routes.razor`
- `Beep.OilandGas.Web/Components/Shared/KpiCard.razor`
- `Beep.OilandGas.Web/Components/Dashboard/KpiCard.razor`
- `Beep.OilandGas.Web/Components/Pages/Account/Dashboard.razor`
- `Beep.OilandGas.Web/Pages/Data/DatabaseSetup.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Data/DatabaseSetup.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/CreateDatabaseWizard.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Setup/DatabaseSetupWizard.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Production/Forecasts.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Production/ProductionForecasting.razor`

---

## API Duplication and Ownership Evidence

- `Beep.OilandGas.ApiService/Controllers/WorkOrder/WorkOrderController.cs`
- `Beep.OilandGas.ApiService/Controllers/LifeCycle/WorkOrderController.cs`
- `Beep.OilandGas.LifeCycle/Services/WorkOrder/WorkOrderManagementService.cs`

---

## Partial/Thin Domain Evidence That Must Not Be Misclassified as Regression

- `Beep.OilandGas.ProductionOperations/Services/ProductionOperationsService.cs`
- `Beep.OilandGas.ProductionOperations/Services/ProductionManagementService.cs`
- `Beep.OilandGas.EnhancedRecovery/Services/EnhancedRecoveryService.cs`
- `Beep.OilandGas.DrillingAndConstruction/Services/DrillingOperationService.cs`

---

## Mature Service-Heavy Domains That Need Coverage Validation

- `Beep.OilandGas.Accounting/Services/`
- `Beep.OilandGas.ProductionAccounting/Services/`
- `Beep.OilandGas.PPDM39.DataManagement/Services/`
- `Beep.OilandGas.ApiService/Controllers/Accounting/`
- `Beep.OilandGas.ApiService/Controllers/PPDM39/`

---

## Deferred or Gap-Prone Surfaces to Track Explicitly

- permit surfacing gap relative to `Beep.OilandGas.PermitsAndApplications/Validation/Rules/`
- engineering modules without first-class page/API surfacing beyond the currently exposed set
- thin UI coverage relative to finance service breadth
