# PermitsAndApplications Context Map

## Module Scope
- Core module: `Beep.OilandGas.PermitsAndApplications`
- Integration surfaces:
  - `Beep.OilandGas.ApiService/Controllers/Permits/PermitsController.cs`
  - `Beep.OilandGas.LifeCycle/Services/Permits/PermitManagementService.cs`

## Canonical Data Domains
- PPDM canonical entities:
  - `APPLIC_BA`, `APPLIC_DESC`, `APPLIC_REMARK`, `BA_PERMIT`, `FACILITY_LICENSE`, `WELL_PERMIT_TYPE`
- Module extension entities:
  - `PERMIT_APPLICATION`, `PERMIT_STATUS_HISTORY`, `DRILLING_PERMIT_APPLICATION`, `ENVIRONMENTAL_PERMIT_APPLICATION`, `INJECTION_PERMIT_APPLICATION`, `JURISDICTION_REQUIREMENTS`, `MIT_RESULT`, `REQUIRED_FORM`, `APPLICATION_ATTACHMENT`

## Runtime Service Surfaces
- `PermitApplicationLifecycleService`
- `PermitApplicationWorkflowService`
- `PermitComplianceCheckService`
- `PermitStatusHistoryService`
