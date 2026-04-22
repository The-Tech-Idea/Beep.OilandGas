# Phase 9 Scan Evidence

> Purpose: representative file-level evidence used to ground the Phase 9 data, admin, workflow, compliance, and finance plan.

---

## Duplicate or Overlapping Web/Admin Surfaces

- `Beep.OilandGas.Web/Pages/Data/DatabaseSetup.razor`
- `Beep.OilandGas.Web/Pages/Data/Validation.razor`
- `Beep.OilandGas.Web/Pages/Data/Versioning.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Data/DatabaseSetup.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Data/DataQualityDashboard.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Data/AccessAudit.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Setup/DatabaseSetupWizard.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/CreateDatabaseWizard.razor`

---

## PPDM and Data Platform Evidence

- `Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39DataController.cs`
- `Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39DataAccessAuditController.cs`
- `Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39DataQualityController.cs`
- `Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39MetadataController.cs`
- `Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39SchemaController.cs`
- `Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39SetupController.cs`
- `Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39VersioningController.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Well/`

---

## Workflow and Support Infrastructure Evidence

- `Beep.OilandGas.LifeCycle/Services/WorkOrder/WorkOrderManagementService.cs`
- `Beep.OilandGas.Web/Pages/PPDM39/BusinessProcess/ProcessDashboard.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/BusinessProcess/ProcessInstances.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/BusinessProcess/GateReviews.razor`
- `Beep.OilandGas.Web/Pages/Admin/AccessControl/UserRoles.razor`
- `Beep.OilandGas.ApiService/Controllers/AccessControl/AccessControlController.cs`
- `Beep.OilandGas.ApiService/Controllers/AccessControl/AssetHierarchyController.cs`
- `Beep.OilandGas.UserManagement/Services/UserManagementService.cs`
- `Beep.OilandGas.Branchs/BusinessProcess/`
- `Beep.OilandGas.DataManager/Core/`

---

## Compliance and Permit Evidence

- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/AerDrillingRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/RrcDrillingRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/BoemDrillingRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/TceqEnvironmentalRule.cs`
- `Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf/`
- `Beep.OilandGas.ApiService/Controllers/Compliance/ComplianceController.cs`
- `Beep.OilandGas.ApiService/Controllers/HSE/HSEController.cs`

---

## Finance Evidence

- `Beep.OilandGas.Accounting/Services/AccountingServices.cs`
- `Beep.OilandGas.Accounting/Services/ARService.cs`
- `Beep.OilandGas.Accounting/Services/GLIntegrationService.cs`
- `Beep.OilandGas.Accounting/Services/PeriodClosingService.cs`
- `Beep.OilandGas.Web/Pages/PPDM39/Accounting/AccountingDashboard.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Accounting/CostAllocation.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Accounting/Royalties.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/Accounting/ProductionAccounting.razor`
- `Beep.OilandGas.ApiService/Controllers/Accounting/`

---

## Cross-Phase Gaps Observed During Phase 9 Scan

- permit logic is much deeper in validation rules than in surfaced workflows
- finance logic is much deeper in services than in current web coverage
- PPDM admin/setup surfaces need route-tree consolidation before retirement work can be accurate
