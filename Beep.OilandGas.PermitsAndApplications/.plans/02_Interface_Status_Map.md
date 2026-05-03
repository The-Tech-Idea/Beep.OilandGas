# PermitsAndApplications Interface Status Map

## Canonical Interfaces
- `IPermitApplicationLifecycleService`: canonical permit lifecycle persistence path.
- `IPermitApplicationWorkflowService`: compliance, jurisdiction, and workflow-specific operations.
- `IPermitComplianceCheckService`: readiness and compliance checks.
- `IPermitStatusHistoryService`: immutable transition audit trail.

## Compatibility Boundaries
- `PermitManagementService` in `LifeCycle` is compatibility-oriented and should call canonical module interfaces for future operations.
- API endpoints must resolve to canonical permit services rather than ad-hoc repository operations.

## PPDM-First Mapping Matrix
- Direct PPDM canonical entities:
  - `APPLIC_BA`: permit/application business associates
  - `APPLIC_DESC`: structured descriptions
  - `APPLIC_REMARK`: permit remarks
  - `BA_PERMIT`: business-associate permit references
  - `FACILITY_LICENSE`: facility-level license records
  - `WELL_PERMIT_TYPE`: well permit type master data
- Module extension entities:
  - `PERMIT_APPLICATION`, `PERMIT_STATUS_HISTORY`, `DRILLING_PERMIT_APPLICATION`, `ENVIRONMENTAL_PERMIT_APPLICATION`, `INJECTION_PERMIT_APPLICATION`, `APPLICATION_ATTACHMENT`, `JURISDICTION_REQUIREMENTS`, `MIT_RESULT`, `REQUIRED_FORM`
- Compatibility adapters:
  - `LifeCycle/PermitManagementService` legacy `APPLICATION`-based writes are a compatibility path until fully cut over to canonical module interfaces.
